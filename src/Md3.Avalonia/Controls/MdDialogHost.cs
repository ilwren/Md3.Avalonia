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
/// type-matched dialog views in the inherited <c>DataTemplates</c> collection, assign a view model to
/// <see cref="Dialog"/>, and use <see cref="ShowAsync(object, CancellationToken)"/> from direct code.
/// </summary>
[TemplatePart("PART_Scrim", typeof(Control))]
[TemplatePart("PART_Overlay", typeof(Control))]
[TemplatePart("PART_DialogPresenter", typeof(ContentPresenter))]
[PseudoClasses(":open", ":closed", ":present", ":full-screen-dialog", ":reduced-motion", ":no-motion")]
public sealed class MdDialogHost : ContentControl
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

    private readonly MdPresenceController _presence;
    private TaskCompletionSource<object?>? _completion;
    private CancellationTokenRegistration _cancellationRegistration;
    private Control? _scrim;
    private Control? _overlay;
    private ContentPresenter? _dialogPresenter;
    private MdDialog? _displayedDialog;
    private int _openStateVersion;
    private bool _isAttached;

    static MdDialogHost()
    {
        IsOpenProperty.Changed.AddClassHandler<MdDialogHost>((host, _) => host.UpdateOpenState());
        DialogProperty.Changed.AddClassHandler<MdDialogHost>((host, _) => host.ScheduleDialogStateUpdate());
        DialogTemplateProperty.Changed.AddClassHandler<MdDialogHost>((host, _) => host.ScheduleDialogStateUpdate());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdDialogHost>((host, _) => host.UpdateMotion());
    }

    public MdDialogHost()
    {
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

    public Task<object?> ShowAsync(object dialog, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dialog);
        if (_completion is not null) Close();

        SetCurrentValue(DialogProperty, dialog);
        _completion = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        _cancellationRegistration.Dispose();
        if (cancellationToken.CanBeCanceled)
            _cancellationRegistration = cancellationToken.Register(() => Close());
        SetCurrentValue(IsOpenProperty, true);
        return _completion.Task;
    }

    public void Close(object? result = null)
    {
        // Complete first: the IsOpen=false handler treats an external/MVVM closure as a null result.
        _completion?.TrySetResult(result);
        _completion = null;
        _cancellationRegistration.Dispose();
        _cancellationRegistration = default;
        SetCurrentValue(IsOpenProperty, false);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_scrim is not null) _scrim.PointerPressed -= OnScrimPressed;
        base.OnApplyTemplate(e);
        _scrim = e.NameScope.Find<Control>("PART_Scrim");
        _overlay = e.NameScope.Find<Control>("PART_Overlay");
        _dialogPresenter = e.NameScope.Find<ContentPresenter>("PART_DialogPresenter");
        if (_scrim is not null) _scrim.PointerPressed += OnScrimPressed;
        UpdateMotion();
        ScheduleDialogStateUpdate();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        ++_openStateVersion;
        _presence.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
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
        if (!IsOpen && _completion is not null)
        {
            _completion.TrySetResult(null);
            _completion = null;
            _cancellationRegistration.Dispose();
            _cancellationRegistration = default;
        }
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
}
