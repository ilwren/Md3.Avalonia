using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>Hosts a tooltip trigger and coordinates hover, focus, long-press and direct open state.</summary>
[TemplatePart("PART_Popup", typeof(Popup))]
[PseudoClasses(":open", ":closed", ":present", ":reduced-motion", ":no-motion")]
public sealed class MdTooltipHost : ContentControl, IMdPopupPresenceOwner
{
    public static readonly StyledProperty<MdTooltip?> TooltipProperty =
        AvaloniaProperty.Register<MdTooltipHost, MdTooltip?>(nameof(Tooltip));
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MdTooltipHost, bool>(nameof(IsOpen),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly DirectProperty<MdTooltipHost, bool> IsPopupOpenProperty =
        AvaloniaProperty.RegisterDirect<MdTooltipHost, bool>(nameof(IsPopupOpen), host => host.IsPopupOpen);
    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        AvaloniaProperty.Register<MdTooltipHost, PlacementMode>(nameof(Placement), PlacementMode.Bottom);
    public static readonly StyledProperty<double> HorizontalOffsetProperty =
        AvaloniaProperty.Register<MdTooltipHost, double>(nameof(HorizontalOffset));
    public static readonly StyledProperty<double> VerticalOffsetProperty =
        AvaloniaProperty.Register<MdTooltipHost, double>(nameof(VerticalOffset), 8);
    public static readonly StyledProperty<TimeSpan> ShowDelayProperty =
        AvaloniaProperty.Register<MdTooltipHost, TimeSpan>(nameof(ShowDelay), TimeSpan.FromMilliseconds(500));
    public static readonly StyledProperty<TimeSpan> HideDelayProperty =
        AvaloniaProperty.Register<MdTooltipHost, TimeSpan>(nameof(HideDelay), TimeSpan.FromMilliseconds(100));
    public static readonly StyledProperty<TimeSpan> LongPressDelayProperty =
        AvaloniaProperty.Register<MdTooltipHost, TimeSpan>(nameof(LongPressDelay), TimeSpan.FromMilliseconds(500));
    public static readonly StyledProperty<bool> OpenOnClickProperty =
        AvaloniaProperty.Register<MdTooltipHost, bool>(nameof(OpenOnClick));
    public static readonly StyledProperty<TimeSpan> LongPressVisibleDurationProperty =
        AvaloniaProperty.Register<MdTooltipHost, TimeSpan>(nameof(LongPressVisibleDuration), TimeSpan.FromMilliseconds(1500));
    public static readonly StyledProperty<double> TouchSlopProperty =
        AvaloniaProperty.Register<MdTooltipHost, double>(nameof(TouchSlop), 12, validate: value => value >= 0);

    private readonly DispatcherTimer _showTimer;
    private readonly DispatcherTimer _hideTimer;
    private readonly DispatcherTimer _longPressTimer;
    private readonly MdPresenceController _presence;
    private Popup? _popup;
    private bool _isPopupOpen;
    private bool _longPressTriggered;
    private Point _pressOrigin;
    private IPointer? _pressedPointer;

    static MdTooltipHost()
    {
        IsOpenProperty.Changed.AddClassHandler<MdTooltipHost>((host, _) => host.UpdateOpenState());
        TooltipProperty.Changed.AddClassHandler<MdTooltipHost>((host, args) =>
            host.OnTooltipChanged(args.OldValue as MdTooltip, args.NewValue as MdTooltip));
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdTooltipHost>((host, _) => host.UpdateMotion());
    }

    public MdTooltipHost()
    {
        _presence = new MdPresenceController(SetPopupPresence);
        _presence.Initialize(IsOpen);
        _showTimer = CreateTimer(() => SetOpen(true));
        _hideTimer = CreateTimer(() => SetOpen(false));
        _longPressTimer = CreateTimer(() =>
        {
            _longPressTriggered = true;
            SetOpen(true);
        });

        PointerEntered += (_, e) =>
        {
            if (e.Pointer.Type != PointerType.Mouse) return;
            _hideTimer.Stop();
            Schedule(_showTimer, ShowDelay);
        };
        PointerExited += (_, e) =>
        {
            _showTimer.Stop();
            if (e.Pointer.Type is PointerType.Touch or PointerType.Pen) CancelLongPress();
            Schedule(_hideTimer, HideDelay);
        };
        GotFocus += (_, _) => Schedule(_showTimer, ShowDelay);
        LostFocus += (_, _) => Schedule(_hideTimer, HideDelay);
        AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, global::Avalonia.Interactivity.RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, global::Avalonia.Interactivity.RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, global::Avalonia.Interactivity.RoutingStrategies.Tunnel, handledEventsToo: true);
        PointerCaptureLost += (_, _) => CancelLongPress();
        UpdateOpenState();
    }

    public MdTooltip? Tooltip { get => GetValue(TooltipProperty); set => SetValue(TooltipProperty, value); }
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public bool IsPopupOpen
    {
        get => _isPopupOpen;
        private set => SetAndRaise(IsPopupOpenProperty, ref _isPopupOpen, value);
    }
    public PlacementMode Placement { get => GetValue(PlacementProperty); set => SetValue(PlacementProperty, value); }
    public double HorizontalOffset { get => GetValue(HorizontalOffsetProperty); set => SetValue(HorizontalOffsetProperty, value); }
    public double VerticalOffset { get => GetValue(VerticalOffsetProperty); set => SetValue(VerticalOffsetProperty, value); }
    public TimeSpan ShowDelay { get => GetValue(ShowDelayProperty); set => SetValue(ShowDelayProperty, value); }
    public TimeSpan HideDelay { get => GetValue(HideDelayProperty); set => SetValue(HideDelayProperty, value); }
    public TimeSpan LongPressDelay { get => GetValue(LongPressDelayProperty); set => SetValue(LongPressDelayProperty, value); }
    public bool OpenOnClick { get => GetValue(OpenOnClickProperty); set => SetValue(OpenOnClickProperty, value); }
    public TimeSpan LongPressVisibleDuration { get => GetValue(LongPressVisibleDurationProperty); set => SetValue(LongPressVisibleDurationProperty, value); }
    public double TouchSlop { get => GetValue(TouchSlopProperty); set => SetValue(TouchSlopProperty, value); }

    public void Show() => SetOpen(true);
    public void Dismiss() => SetOpen(false);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_popup is not null) _popup.Closed -= OnPopupClosed;
        base.OnApplyTemplate(e);
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        if (_popup is not null) _popup.Closed += OnPopupClosed;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateOpenState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _showTimer.Stop();
        _hideTimer.Stop();
        _longPressTimer.Stop();
        _presence.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsOpen)
        {
            Dismiss();
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private static DispatcherTimer CreateTimer(Action tick)
    {
        var timer = new DispatcherTimer();
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            tick();
        };
        return timer;
    }

    private static void Schedule(DispatcherTimer timer, TimeSpan delay)
    {
        timer.Stop();
        timer.Interval = delay <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(1) : delay;
        timer.Start();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _longPressTriggered = false;
        _hideTimer.Stop();
        _pressOrigin = e.GetPosition(this);
        _pressedPointer = e.Pointer;
        if (e.Pointer.Type is PointerType.Touch or PointerType.Pen)
            Schedule(_longPressTimer, LongPressDelay);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!ReferenceEquals(e.Pointer, _pressedPointer)) return;
        var point = e.GetPosition(this);
        var delta = point - _pressOrigin;
        if (Math.Abs(delta.X) > TouchSlop || Math.Abs(delta.Y) > TouchSlop)
            CancelLongPress();
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!ReferenceEquals(e.Pointer, _pressedPointer)) return;
        _longPressTimer.Stop();
        _pressedPointer = null;
        if (_longPressTriggered)
        {
            // Consume the release so a long-press tooltip never also invokes the hosted button.
            e.Handled = true;
            if (Tooltip?.Variant != MdTooltipVariant.Rich)
                Schedule(_hideTimer, LongPressVisibleDuration);
        }
        else if (OpenOnClick)
        {
            SetOpen(!IsOpen);
        }
    }

    private void CancelLongPress()
    {
        _longPressTimer.Stop();
        _pressedPointer = null;
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        if (IsOpen) SetCurrentValue(IsOpenProperty, false);
        _presence.Initialize(false);
    }

    private void OnTooltipChanged(MdTooltip? oldTooltip, MdTooltip? newTooltip)
    {
        if (oldTooltip is not null)
        {
            oldTooltip.PointerEntered -= TooltipPointerEntered;
            oldTooltip.PointerExited -= TooltipPointerExited;
        }
        if (newTooltip is not null)
        {
            newTooltip.PointerEntered += TooltipPointerEntered;
            newTooltip.PointerExited += TooltipPointerExited;
            newTooltip.SetCurrentValue(MdTooltip.IsOpenProperty, IsOpen);
        }
    }

    private void TooltipPointerEntered(object? sender, PointerEventArgs e) => _hideTimer.Stop();
    private void TooltipPointerExited(object? sender, PointerEventArgs e) => Schedule(_hideTimer, HideDelay);

    private void SetOpen(bool value) => SetCurrentValue(IsOpenProperty, value);

    void IMdPopupPresenceOwner.ClosePopupImmediately() => _presence.Initialize(false);

    private void SetPopupPresence(bool value)
    {
        IsPopupOpen = value;
        PseudoClasses.Set(":present", value);
    }

    private void UpdateOpenState()
    {
        if (IsOpen)
        {
            _presence.Update(true, TimeSpan.Zero);
            Tooltip?.SetCurrentValue(MdTooltip.IsOpenProperty, true);
            PseudoClasses.Set(":open", true);
            PseudoClasses.Set(":closed", false);
        }
        else
        {
            Tooltip?.SetCurrentValue(MdTooltip.IsOpenProperty, false);
            PseudoClasses.Set(":open", false);
            PseudoClasses.Set(":closed", true);
            _presence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
            _longPressTriggered = false;
        }
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (!IsOpen)
            _presence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
    }
}
