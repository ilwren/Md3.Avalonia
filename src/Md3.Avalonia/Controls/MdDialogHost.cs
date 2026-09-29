using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Hosts Material dialogs without a platform-specific window. Use IsOpen/Dialog in AXAML or
/// await ShowAsync from direct C# code.
/// </summary>
[TemplatePart("PART_Scrim", typeof(Control))]
[TemplatePart("PART_Overlay", typeof(Control))]
[TemplatePart("PART_DialogPresenter", typeof(ContentPresenter))]
[PseudoClasses(":open", ":closed", ":present", ":full-screen-dialog", ":reduced-motion", ":no-motion")]
public sealed class MdDialogHost : ContentControl
{
    public static readonly StyledProperty<Control?> DialogProperty =
        AvaloniaProperty.Register<MdDialogHost, Control?>(nameof(Dialog));
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
    private int _openStateVersion;
    private bool _isAttached;

    static MdDialogHost()
    {
        IsOpenProperty.Changed.AddClassHandler<MdDialogHost>((host, _) => host.UpdateOpenState());
        DialogProperty.Changed.AddClassHandler<MdDialogHost>((host, _) => host.UpdateDialogState());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdDialogHost>((host, _) => host.UpdateMotion());
    }

    public MdDialogHost()
    {
        _presence = new MdPresenceController(value => PseudoClasses.Set(":present", value));
        _presence.Initialize(IsOpen);
        UpdateOpenState();
    }

    public Control? Dialog { get => GetValue(DialogProperty); set => SetValue(DialogProperty, value); }
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public bool DismissOnScrimClick { get => GetValue(DismissOnScrimClickProperty); set => SetValue(DismissOnScrimClickProperty, value); }

    public Task<object?> ShowAsync(Control dialog, CancellationToken cancellationToken = default)
    {
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

    private void UpdateDialogState() =>
        PseudoClasses.Set(":full-screen-dialog", Dialog is MdDialog { Variant: MdDialogVariant.FullScreen });

    private void UpdateOpenState()
    {
        var version = ++_openStateVersion;
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
            _presence.Update(false, MdMotion.GetExitDuration(this));
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

        if (_overlay is not null)
        {
            _overlay.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects));
        }
        if (_dialogPresenter is not null)
        {
            _dialogPresenter.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty));
        }

        if (IsOpen && scheme == MdMotionScheme.None)
        {
            ++_openStateVersion;
            PseudoClasses.Set(":closed", false);
            PseudoClasses.Set(":open", true);
        }
        else if (!IsOpen)
        {
            _presence.Update(false, MdMotion.GetExitDuration(this));
        }
    }
}
