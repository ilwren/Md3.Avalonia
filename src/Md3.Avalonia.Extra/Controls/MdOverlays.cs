using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Extra.Infrastructure;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>A provider-neutral transient popover with safe native-popup fallback and focus return.</summary>
[PseudoClasses(":open", ":closed", ":popup-present", ":reduced-motion", ":no-motion")]
public class MdPopover : TemplatedControl
{
    public static readonly StyledProperty<object?> AnchorProperty = AvaloniaProperty.Register<MdPopover, object?>(nameof(Anchor));
    public static readonly StyledProperty<object?> PopoverContentProperty = AvaloniaProperty.Register<MdPopover, object?>(nameof(PopoverContent));
    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<MdPopover, bool>(nameof(IsOpen), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<PlacementMode> PlacementProperty = AvaloniaProperty.Register<MdPopover, PlacementMode>(nameof(Placement), PlacementMode.BottomEdgeAlignedLeft);
    public static readonly StyledProperty<bool> DismissOnOutsideClickProperty = AvaloniaProperty.Register<MdPopover, bool>(nameof(DismissOnOutsideClick), true);
    public static readonly StyledProperty<bool> RestoreFocusOnCloseProperty = AvaloniaProperty.Register<MdPopover, bool>(nameof(RestoreFocusOnClose), true);
    public static readonly StyledProperty<bool> DismissOnScrollProperty = AvaloniaProperty.Register<MdPopover, bool>(nameof(DismissOnScroll), true);
    public static readonly DirectProperty<MdPopover, bool> IsPopupOpenProperty =
        AvaloniaProperty.RegisterDirect<MdPopover, bool>(nameof(IsPopupOpen), control => control.IsPopupOpen);

    private readonly MdFocusReturnScope _focusReturn = new();
    private readonly MdPresenceController _popupPresence;
    private Button? _anchorButton;
    private InputElement? _popoverInput;
    private TopLevel? _topLevel;
    private Popup? _popup;
    private Border? _surface;
    private bool _isPopupOpen;

    static MdPopover()
    {
        IsOpenProperty.Changed.AddClassHandler<MdPopover>((popover, _) => popover.OnOpenChanged());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdPopover>((popover, _) => popover.UpdateMotion());
    }

    public MdPopover()
    {
        _popupPresence = new MdPresenceController(SetPopupPresence);
        _popupPresence.Initialize(IsOpen);
        UpdateState();
    }

    public object? Anchor { get => GetValue(AnchorProperty); set => SetValue(AnchorProperty, value); }
    public object? PopoverContent { get => GetValue(PopoverContentProperty); set => SetValue(PopoverContentProperty, value); }
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public PlacementMode Placement { get => GetValue(PlacementProperty); set => SetValue(PlacementProperty, value); }
    public bool DismissOnOutsideClick { get => GetValue(DismissOnOutsideClickProperty); set => SetValue(DismissOnOutsideClickProperty, value); }
    public bool RestoreFocusOnClose { get => GetValue(RestoreFocusOnCloseProperty); set => SetValue(RestoreFocusOnCloseProperty, value); }
    public bool DismissOnScroll { get => GetValue(DismissOnScrollProperty); set => SetValue(DismissOnScrollProperty, value); }
    /// <summary>The actual popup-host lifetime, which remains true during an animated exit.</summary>
    public bool IsPopupOpen
    {
        get => _isPopupOpen;
        private set => SetAndRaise(IsPopupOpenProperty, ref _isPopupOpen, value);
    }

    public event EventHandler? Opened;
    public event EventHandler? Closed;
    public void Show() => SetCurrentValue(IsOpenProperty, true);
    public void Dismiss() => SetCurrentValue(IsOpenProperty, false);
    /// <summary>Closes the transient surface when its owning viewport changes position.</summary>
    public void NotifyOwnerScrolled()
    {
        if (IsOpen && DismissOnScroll) Dismiss();
    }
    public void Toggle() => SetCurrentValue(IsOpenProperty, !IsOpen);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_anchorButton is not null) _anchorButton.Click -= OnAnchorClick;
        if (_popup is not null) _popup.Closed -= OnPopupClosed;
        base.OnApplyTemplate(e);
        _anchorButton = e.NameScope.Find<Button>("PART_AnchorButton");
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _surface = e.NameScope.Find<Border>("PART_Surface");
        if (_anchorButton is not null) _anchorButton.Click += OnAnchorClick;
        if (_popup is not null) _popup.Closed += OnPopupClosed;
        _popupPresence.Initialize(IsOpen);
        UpdateMotion();
        UpdateSurfaceHitTesting();
    }

    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _topLevel = TopLevel.GetTopLevel(this);
        _topLevel?.AddHandler(InputElement.PointerWheelChangedEvent, OnTopLevelPointerWheel,
            global::Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        _topLevel?.AddHandler(InputElement.KeyDownEvent, OnPopupKeyDown,
            global::Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _topLevel?.RemoveHandler(InputElement.PointerWheelChangedEvent, OnTopLevelPointerWheel);
        _topLevel?.RemoveHandler(InputElement.KeyDownEvent, OnPopupKeyDown);
        DetachPopoverInput();
        _topLevel = null;
        if (IsOpen) SetCurrentValue(IsOpenProperty, false);
        _popupPresence.Initialize(false);
        _popupPresence.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (TryDismissFromEscape(e)) return;
        base.OnKeyDown(e);
    }

    private void OnPopupKeyDown(object? sender, KeyEventArgs e) => TryDismissFromEscape(e);
    private bool TryDismissFromEscape(KeyEventArgs e)
    {
        if (e.Key != Key.Escape || !IsOpen) return false;
        Dismiss();
        e.Handled = true;
        return true;
    }

    private void OnTopLevelPointerWheel(object? sender, PointerWheelEventArgs e) => NotifyOwnerScrolled();

    private void OnAnchorClick(object? sender, RoutedEventArgs e) => Toggle();
    private void OnOpenChanged()
    {
        ConfigureSurfaceTransitions(IsOpen ? MdMotionSpeed.Slow : MdMotionSpeed.Fast);
        if (IsOpen)
        {
            _popupPresence.Update(true, TimeSpan.Zero);
            UpdateState();
            _focusReturn.Capture(_anchorButton);
            EcosystemPopupCoordinator.Open(this);
            if (this.IsAttachedToVisualTree())
                Dispatcher.UIThread.Post(AttachAndFocusPopoverContent, DispatcherPriority.Loaded);
            Opened?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            UpdateState();
            _popupPresence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
            DetachPopoverInput();
            if (RestoreFocusOnClose) _focusReturn.Restore();
            Closed?.Invoke(this, EventArgs.Empty);
        }
        UpdateSurfaceHitTesting();
    }

    private void SetPopupPresence(bool value)
    {
        IsPopupOpen = value;
        PseudoClasses.Set(":popup-present", value);
    }

    private void ClosePopupImmediately() => _popupPresence.Initialize(false);

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        if (IsOpen) SetCurrentValue(IsOpenProperty, false);
        _popupPresence.Initialize(false);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        ConfigureSurfaceTransitions(IsOpen ? MdMotionSpeed.Slow : MdMotionSpeed.Fast);
        if (!IsOpen)
            _popupPresence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
    }

    private void ConfigureSurfaceTransitions(MdMotionSpeed speed)
    {
        if (_surface is null) return;
        _surface.Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, speed),
            MdMotionTransitions.CreateTransform(this, RenderTransformProperty, speed));
    }

    private void UpdateSurfaceHitTesting()
    {
        if (_surface is not null) _surface.IsHitTestVisible = IsOpen;
    }

    private void AttachAndFocusPopoverContent()
    {
        if (!IsOpen || PopoverContent is not Control root) return;
        DetachPopoverInput();
        _popoverInput = root;
        _popoverInput.AddHandler(InputElement.KeyDownEvent, OnPopupKeyDown,
            global::Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        var focusTarget = root.GetVisualDescendants().OfType<Control>()
            .FirstOrDefault(control => control.Focusable && control.IsEnabled && control.IsVisible);
        if (focusTarget is not null) focusTarget.Focus();
        else if (root.Focusable) root.Focus();
    }

    private void DetachPopoverInput()
    {
        _popoverInput?.RemoveHandler(InputElement.KeyDownEvent, OnPopupKeyDown);
        _popoverInput = null;
    }

    private void UpdateState() { PseudoClasses.Set(":open", IsOpen); PseudoClasses.Set(":closed", !IsOpen); }

    private static class EcosystemPopupCoordinator
    {
        private static WeakReference<MdPopover>? _open;
        public static void Open(MdPopover current)
        {
            if (_open?.TryGetTarget(out var previous) == true && !ReferenceEquals(previous, current))
            {
                previous.Dismiss();
                previous.ClosePopupImmediately();
            }
            _open = new WeakReference<MdPopover>(current);
        }
    }
}

/// <summary>A hover/focus-triggered informational card with deterministic open/close delays.</summary>
public sealed class MdHoverCard : MdPopover
{
    public static readonly StyledProperty<TimeSpan> OpenDelayProperty = AvaloniaProperty.Register<MdHoverCard, TimeSpan>(nameof(OpenDelay), TimeSpan.FromMilliseconds(400));
    public static readonly StyledProperty<TimeSpan> CloseDelayProperty = AvaloniaProperty.Register<MdHoverCard, TimeSpan>(nameof(CloseDelay), TimeSpan.FromMilliseconds(120));
    private readonly DispatcherTimer _timer = new();
    private bool _pendingOpen;

    public MdHoverCard()
    {
        _timer.Tick += (_, _) => { _timer.Stop(); if (_pendingOpen) Show(); else Dismiss(); };
        PointerEntered += (_, _) => Schedule(true);
        PointerExited += (_, _) => Schedule(false);
        GotFocus += (_, _) => Schedule(true);
        LostFocus += (_, _) => Schedule(false);
    }

    public TimeSpan OpenDelay { get => GetValue(OpenDelayProperty); set => SetValue(OpenDelayProperty, value); }
    public TimeSpan CloseDelay { get => GetValue(CloseDelayProperty); set => SetValue(CloseDelayProperty, value); }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _timer.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private void Schedule(bool open)
    {
        _pendingOpen = open;
        _timer.Stop();
        _timer.Interval = open ? OpenDelay : CloseDelay;
        if (_timer.Interval <= TimeSpan.Zero) { if (open) Show(); else Dismiss(); }
        else _timer.Start();
    }
}
