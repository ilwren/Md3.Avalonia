using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Hosts one active Material dialog without a platform-specific window. Declare multiple
/// type-matched dialog views in the inherited <c>DataTemplates</c> collection, then drive the host
/// whichever way suits the caller — all three reach the same surface:
/// <list type="bullet">
/// <item><description>view code-behind: <see cref="ShowAsync(object, CancellationToken)"/> and <see cref="Close"/>;</description></item>
/// <item><description>a view model holding no control reference: an <see cref="IMdDialogService"/>
/// assigned to <see cref="Service"/>;</description></item>
/// <item><description>pure bindings: two-way <see cref="IsOpen"/> with <see cref="Dialog"/>.</description></item>
/// </list>
/// </summary>
[TemplatePart("PART_Scrim", typeof(Control))]
[TemplatePart("PART_Overlay", typeof(Control))]
[TemplatePart("PART_DialogPresenter", typeof(ContentPresenter))]
[PseudoClasses(":open", ":closed", ":present", ":full-screen-dialog", ":reduced-motion", ":no-motion")]
public sealed class MdDialogHost : ContentControl, IMdDialogService
{
    public static readonly StyledProperty<object?> DialogProperty =
        AvaloniaProperty.Register<MdDialogHost, object?>(nameof(Dialog));
    public static readonly StyledProperty<IDataTemplate?> DialogTemplateProperty =
        AvaloniaProperty.Register<MdDialogHost, IDataTemplate?>(nameof(DialogTemplate));
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MdDialogHost, bool>(nameof(IsOpen),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> DismissOnScrimClickProperty =
        AvaloniaProperty.Register<MdDialogHost, bool>(nameof(DismissOnScrimClick), true);
    public static readonly StyledProperty<MdDialogService?> ServiceProperty =
        AvaloniaProperty.Register<MdDialogHost, MdDialogService?>(nameof(Service));

    private readonly MdPresenceController _presence;
    private readonly MdDialogService _localService = new();
    private MdDialogService? _connectedService;
    private MdDialogService.Request? _activeRequest;
    private readonly MdModalFocusController _modalFocus;
    private Control? _scrim;
    private Control? _overlay;
    private ContentPresenter? _mainContent;
    private ContentPresenter? _dialogPresenter;
    private MdDialog? _displayedDialog;
    private int _openStateVersion;
    private bool _isAttached;
    private object? _closeResult;

    static MdDialogHost()
    {
        IsOpenProperty.Changed.AddClassHandler<MdDialogHost>((host, _) => host.UpdateOpenState());
        DialogProperty.Changed.AddClassHandler<MdDialogHost>((host, _) => host.ScheduleDialogStateUpdate());
        DialogTemplateProperty.Changed.AddClassHandler<MdDialogHost>((host, _) => host.ScheduleDialogStateUpdate());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdDialogHost>((host, _) => host.UpdateMotion());
        ServiceProperty.Changed.AddClassHandler<MdDialogHost>((host, _) => host.ConnectService());
    }

    // Android has no Escape key. The system back gesture arrives as TopLevel.BackRequested and
    // has to dismiss this surface, or it is unreachable by the one gesture phone users rely on.
    private readonly MdBackScope _backScope;

    public MdDialogHost()
    {
        _backScope = new MdBackScope(this, OnBackRequested);
        _modalFocus = new MdModalFocusController(this);
        _presence = new MdPresenceController(value => PseudoClasses.Set(":present", value));
        _presence.Initialize(IsOpen);
        UpdateOpenState();
    }

    /// <summary>The active dialog view or view model. Non-controls are rendered through <see cref="DialogTemplate"/> or a matching inherited data template.</summary>
    public object? Dialog { get => GetValue(DialogProperty); set => SetValue(DialogProperty, value); }

    /// <summary>An optional explicit template for <see cref="Dialog"/>. Leave null to select from <c>DataTemplates</c> by model type.</summary>
    public IDataTemplate? DialogTemplate { get => GetValue(DialogTemplateProperty); set => SetValue(DialogTemplateProperty, value); }

    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public bool DismissOnScrimClick { get => GetValue(DismissOnScrimClickProperty); set => SetValue(DismissOnScrimClickProperty, value); }

    /// <summary>
    /// The shared service view models show dialogs through. Leave null and the host uses a private
    /// one, so code-behind and bindings work with no setup.
    /// </summary>
    public MdDialogService? Service
    {
        get => GetValue(ServiceProperty);
        set => SetValue(ServiceProperty, value);
    }

    /// <inheritdoc />
    public Task<object?> ShowAsync(object dialog, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dialog);
        return (Service ?? _localService).ShowAsync(dialog, cancellationToken);
    }

    /// <inheritdoc />
    public void Show(object dialog) => (Service ?? _localService).Show(dialog);

    /// <inheritdoc />
    public void Close(object? result = null)
    {
        // Carry the result through the IsOpen handler, which is also where a dismissal that did
        // not come through this method (scrim, Escape, back gesture, a binding) is completed.
        _closeResult = result;
        SetCurrentValue(IsOpenProperty, false);
        _closeResult = null;
    }

    /// <summary>Displays the next queued request. Called by the service, never directly.</summary>
    internal void TryShowNext(MdDialogService service)
    {
        if (!_isAttached || !ReferenceEquals(_connectedService, service) ||
            _activeRequest is not null || IsOpen)
            return;

        // Opening before the template exists would hand the modal focus scope an empty presenter
        // and trap the focus walk. OnApplyTemplate pumps the service once there is somewhere to go.
        if (_dialogPresenter is null) return;

        if (service.AcquireCurrent(this) is not { } request) return;
        _activeRequest = request;
        SetCurrentValue(DialogProperty, request.Dialog);
        SetCurrentValue(IsOpenProperty, true);
    }

    internal void ReleaseService(MdDialogService service)
    {
        if (!ReferenceEquals(_connectedService, service)) return;
        CompleteActiveRequest(null);
        service.Detach(this);
        _connectedService = null;
    }

    private void ConnectService()
    {
        var service = Service ?? _localService;
        if (ReferenceEquals(_connectedService, service))
        {
            if (_isAttached) service.Attach(this);
            return;
        }

        DisconnectService();
        _connectedService = service;
        if (_isAttached) service.Attach(this);
    }

    private void DisconnectService()
    {
        if (_connectedService is null) return;
        CompleteActiveRequest(null);
        _connectedService.Detach(this);
        _connectedService = null;
    }

    /// <summary>Hands the result back to whoever awaited it, whatever closed the dialog.</summary>
    private void CompleteActiveRequest(object? result)
    {
        if (_activeRequest is not { } request) return;
        _activeRequest = null;
        _connectedService?.Complete(this, request, result);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_scrim is not null) _scrim.PointerPressed -= OnScrimPressed;
        base.OnApplyTemplate(e);
        _scrim = e.NameScope.Find<Control>("PART_Scrim");
        _overlay = e.NameScope.Find<Control>("PART_Overlay");
        _mainContent = e.NameScope.Find<ContentPresenter>("PART_MainContent");
        _dialogPresenter = e.NameScope.Find<ContentPresenter>("PART_DialogPresenter");
        if (_scrim is not null) _scrim.PointerPressed += OnScrimPressed;
        UpdateMotion();
        ScheduleDialogStateUpdate();
        UpdateModalFocus();
        _connectedService?.Pump();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        ++_openStateVersion;
        _presence.Stop();
        _modalFocus.Deactivate();
        // A detached host cannot display anything. Release the service so a host attached later
        // picks the queue up, and never leave an awaiting caller hanging on this one.
        if (_connectedService is { } service)
        {
            CompleteActiveRequest(null);
            service.Detach(this);
        }
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        ConnectService();
        UpdateOpenState();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsOpen)
        {
            Close();
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void OnScrimPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DismissOnScrimClick && ReferenceEquals(e.Source, _scrim))
        {
            Close();
            e.Handled = true;
        }
    }

    private void ScheduleDialogStateUpdate()
    {
        UpdateDialogState();
        if (_isAttached)
            Dispatcher.UIThread.Post(UpdateDialogState, DispatcherPriority.Render);
    }

    private void UpdateDialogState()
    {
        var displayedDialog = Dialog as MdDialog ??
            _dialogPresenter?.GetVisualDescendants().OfType<MdDialog>().FirstOrDefault();
        if (!ReferenceEquals(_displayedDialog, displayedDialog))
        {
            if (_displayedDialog is not null)
                _displayedDialog.PropertyChanged -= OnDisplayedDialogPropertyChanged;
            _displayedDialog = displayedDialog;
            if (_displayedDialog is not null)
                _displayedDialog.PropertyChanged += OnDisplayedDialogPropertyChanged;
        }

        PseudoClasses.Set(":full-screen-dialog", displayedDialog?.Variant == MdDialogVariant.FullScreen);
    }

    private void OnDisplayedDialogPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == MdDialog.VariantProperty) UpdateDialogState();
    }

    private void UpdateOpenState()
    {
        _backScope.Update(IsOpen);
        var version = ++_openStateVersion;
        ConfigureTransitions(IsOpen ? MdMotionSpeed.Default : MdMotionSpeed.Fast);
        if (IsOpen)
        {
            _presence.Update(true, TimeSpan.Zero);
            if (MdMotion.GetScheme(this) == MdMotionScheme.None)
            {
                PseudoClasses.Set(":closed", false);
                PseudoClasses.Set(":open", true);
            }
            else
            {
                PseudoClasses.Set(":open", false);
                PseudoClasses.Set(":closed", true);
                if (_isAttached)
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (version != _openStateVersion || !_isAttached || !IsOpen) return;
                        PseudoClasses.Set(":closed", false);
                        PseudoClasses.Set(":open", true);
                    }, DispatcherPriority.Render);
            }
        }
        else
        {
            PseudoClasses.Set(":open", false);
            PseudoClasses.Set(":closed", true);
            _presence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
        }

        UpdateDialogState();
        UpdateModalFocus();
        if (!IsOpen)
        {
            // _closeResult is set only by Close(result); every other route here — scrim, Escape,
            // the back gesture, a view model clearing a bound IsOpen — means a null result.
            CompleteActiveRequest(_closeResult);
            _connectedService?.Pump();
        }
    }

    private void UpdateModalFocus()
    {
        _modalFocus.Update(IsOpen && _isAttached, _dialogPresenter, _mainContent);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        ConfigureTransitions(IsOpen ? MdMotionSpeed.Default : MdMotionSpeed.Fast);

        if (IsOpen && scheme == MdMotionScheme.None)
        {
            ++_openStateVersion;
            PseudoClasses.Set(":closed", false);
            PseudoClasses.Set(":open", true);
        }
        else if (!IsOpen)
        {
            _presence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
        }
    }

    private void ConfigureTransitions(MdMotionSpeed speed)
    {
        if (_overlay is not null)
        {
            _overlay.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, speed));
        }
        if (_dialogPresenter is not null)
        {
            _dialogPresenter.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, speed),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, speed));
        }
    }

    private bool OnBackRequested()
    {
        if (!IsOpen) return false;
        Close();
        return true;
    }
}
