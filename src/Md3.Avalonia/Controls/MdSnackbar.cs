using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A transient Material status message with optional action and dismiss affordance.</summary>
[TemplatePart("PART_ActionButton", typeof(Button))]
[TemplatePart("PART_DismissButton", typeof(Button))]
[PseudoClasses(":open", ":closed", ":present", ":has-action", ":dismissible", ":reduced-motion", ":no-motion")]
public sealed class MdSnackbar : ContentControl
{
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MdSnackbar, bool>(nameof(IsOpen),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<MdSnackbar, object?>(nameof(ActionContent));
    public static readonly StyledProperty<ICommand?> ActionCommandProperty =
        AvaloniaProperty.Register<MdSnackbar, ICommand?>(nameof(ActionCommand));
    public static readonly StyledProperty<object?> ActionCommandParameterProperty =
        AvaloniaProperty.Register<MdSnackbar, object?>(nameof(ActionCommandParameter));
    public static readonly StyledProperty<bool> IsDismissibleProperty =
        AvaloniaProperty.Register<MdSnackbar, bool>(nameof(IsDismissible));
    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<MdSnackbar, TimeSpan>(nameof(Duration), TimeSpan.FromSeconds(4));

    private readonly DispatcherTimer _timer;
    private readonly MdPresenceController _presence;
    private Button? _actionButton;
    private Button? _dismissButton;
    private int _openStateVersion;
    private bool _isAttached;

    static MdSnackbar()
    {
        IsOpenProperty.Changed.AddClassHandler<MdSnackbar>((bar, _) => bar.UpdateOpenState());
        ActionContentProperty.Changed.AddClassHandler<MdSnackbar>((bar, _) => bar.UpdateAffordanceState());
        IsDismissibleProperty.Changed.AddClassHandler<MdSnackbar>((bar, _) => bar.UpdateAffordanceState());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdSnackbar>((bar, _) => bar.UpdateMotion());
    }

    public MdSnackbar()
    {
        AutomationProperties.SetLiveSetting(this, AutomationLiveSetting.Polite);
        _presence = new MdPresenceController(value => PseudoClasses.Set(":present", value));
        _presence.Initialize(IsOpen);
        _timer = new DispatcherTimer();
        _timer.Tick += (_, _) => Dismiss();
        PointerEntered += (_, _) => _timer.Stop();
        PointerExited += (_, _) => RestartTimer();
        UpdateAffordanceState();
        UpdateOpenState();
    }

    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public object? ActionContent { get => GetValue(ActionContentProperty); set => SetValue(ActionContentProperty, value); }
    public ICommand? ActionCommand { get => GetValue(ActionCommandProperty); set => SetValue(ActionCommandProperty, value); }
    public object? ActionCommandParameter { get => GetValue(ActionCommandParameterProperty); set => SetValue(ActionCommandParameterProperty, value); }
    public bool IsDismissible { get => GetValue(IsDismissibleProperty); set => SetValue(IsDismissibleProperty, value); }
    public TimeSpan Duration { get => GetValue(DurationProperty); set => SetValue(DurationProperty, value); }

    public event EventHandler? ActionInvoked;
    public event EventHandler? Dismissed;

    public void Show() => SetCurrentValue(IsOpenProperty, true);

    public void Dismiss()
    {
        _timer.Stop();
        if (!IsOpen) return;
        SetCurrentValue(IsOpenProperty, false);
        Dismissed?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_actionButton is not null) _actionButton.Click -= OnActionClicked;
        if (_dismissButton is not null) _dismissButton.Click -= OnDismissClicked;
        base.OnApplyTemplate(e);
        _actionButton = e.NameScope.Find<Button>("PART_ActionButton");
        _dismissButton = e.NameScope.Find<Button>("PART_DismissButton");
        if (_actionButton is not null) _actionButton.Click += OnActionClicked;
        if (_dismissButton is not null) _dismissButton.Click += OnDismissClicked;
        UpdateMotion();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        UpdateOpenState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        ++_openStateVersion;
        _timer.Stop();
        _presence.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private void OnActionClicked(object? sender, RoutedEventArgs e)
    {
        var parameter = ActionCommandParameter;
        if (ActionCommand?.CanExecute(parameter) == true) ActionCommand.Execute(parameter);
        ActionInvoked?.Invoke(this, EventArgs.Empty);
        Dismiss();
    }

    private void OnDismissClicked(object? sender, RoutedEventArgs e) => Dismiss();

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
            _presence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
        }
        if (IsOpen) RestartTimer(); else _timer.Stop();
    }

    private void UpdateAffordanceState()
    {
        PseudoClasses.Set(":has-action", ActionContent is not null);
        PseudoClasses.Set(":dismissible", IsDismissible);
    }

    private void RestartTimer()
    {
        _timer.Stop();
        // Material snackbars with an action remain until the user acts or dismisses them.
        if (!IsOpen || ActionContent is not null || Duration <= TimeSpan.Zero) return;
        _timer.Interval = Duration;
        _timer.Start();
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
            MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
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
}
