using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>
/// A window that floats inside the application instead of above it: draggable by its title bar,
/// resizable from its lower-right corner, and able to host anything - a mini player, a chat thread,
/// an inspector - while the page underneath stays reachable and interactive.
/// </summary>
/// <remarks>
/// <para>
/// This is deliberately not an operating-system window. A real window sits above the application,
/// cannot be clipped by it and needs a compositor to move; an in-app surface is just a child that
/// draws last, so it inherits the theme, the motion scheme and the accessibility tree of the page it
/// floats over. The trade-off is that it is bounded by its host, and the control leans into that:
/// dragging is clamped to the visual parent, so the window can be moved but never lost off-screen.
/// </para>
/// <para>
/// Position is <see cref="X"/> and <see cref="Y"/>, and the control owns its <c>Margin</c> to
/// realise them - set the position through those two properties rather than through the margin.
/// Minimizing hides the content but leaves it mounted, which is what makes a hosted player keep
/// playing: nothing is torn down, so state, bindings and media all survive the collapse.
/// </para>
/// </remarks>
[PseudoClasses(":present", ":dragging", ":resizing", ":minimized")]
public sealed class MdFloatingWindow : ContentControl
{
    private readonly MdPresenceController _presence;
    private readonly MdPresenceController _contentPresence;
    private readonly DispatcherTimer _settleTimer;
    private Control? _titleBar;
    private Control? _contentHost;
    private Control? _resizeGrip;
    private Button? _minimizeButton;
    private Button? _closeButton;
    private Visual? _dragHost;
    private Point _pressPosition;
    private Point _dragOrigin;
    private Size _resizeOrigin;
    private double _expandedHeight = double.NaN;
    private double _pendingHeight = double.NaN;
    private double _minHeightOverride = double.NaN;
    private bool _wasAutoSized;
    private bool _dragging;
    private bool _resizing;

    /// <summary>Text shown in the title bar. Null hides the title.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<MdFloatingWindow, string?>(nameof(Title));

    /// <summary>An optional leading glyph, typically one of the Md.Icon.* resources.</summary>
    public static readonly StyledProperty<string?> HeaderIconProperty =
        AvaloniaProperty.Register<MdFloatingWindow, string?>(nameof(HeaderIcon));

    /// <summary>
    /// Distance from the left edge of the visual parent. Coerced to zero or more so a window cannot
    /// be positioned outside its host by a binding or a restore of stale state.
    /// </summary>
    public static readonly StyledProperty<double> XProperty =
        AvaloniaProperty.Register<MdFloatingWindow, double>(nameof(X), 16,
            coerce: static (_, value) => double.IsNaN(value) ? 0 : Math.Max(0, value));

    /// <summary>Distance from the top edge of the visual parent. Coerced like <see cref="X"/>.</summary>
    public static readonly StyledProperty<double> YProperty =
        AvaloniaProperty.Register<MdFloatingWindow, double>(nameof(Y), 16,
            coerce: static (_, value) => double.IsNaN(value) ? 0 : Math.Max(0, value));

    /// <summary>Whether the window is shown. Closing keeps the content mounted until the exit runs.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MdFloatingWindow, bool>(nameof(IsOpen), true);

    /// <summary>
    /// Collapses the window to its title bar. The content is hidden, not discarded, so a hosted
    /// player or editor resumes exactly where it left off.
    /// </summary>
    public static readonly StyledProperty<bool> IsMinimizedProperty =
        AvaloniaProperty.Register<MdFloatingWindow, bool>(nameof(IsMinimized));

    /// <summary>Whether the title bar drags the window.</summary>
    public static readonly StyledProperty<bool> CanDragProperty =
        AvaloniaProperty.Register<MdFloatingWindow, bool>(nameof(CanDrag), true);

    /// <summary>Whether the lower-right grip resizes the window.</summary>
    public static readonly StyledProperty<bool> CanResizeProperty =
        AvaloniaProperty.Register<MdFloatingWindow, bool>(nameof(CanResize), true);

    /// <summary>Whether the title bar offers a minimize button.</summary>
    public static readonly StyledProperty<bool> CanMinimizeProperty =
        AvaloniaProperty.Register<MdFloatingWindow, bool>(nameof(CanMinimize), true);

    /// <summary>Whether the title bar offers a close button.</summary>
    public static readonly StyledProperty<bool> CanCloseProperty =
        AvaloniaProperty.Register<MdFloatingWindow, bool>(nameof(CanClose), true);

    /// <summary>Raised after the close button sets <see cref="IsOpen"/> to false.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<MdFloatingWindow, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    /// <inheritdoc cref="ClosedEvent"/>
    public event EventHandler<RoutedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    static MdFloatingWindow()
    {
        IsOpenProperty.Changed.AddClassHandler<MdFloatingWindow>((window, _) => window.UpdateOpenState());
        IsMinimizedProperty.Changed.AddClassHandler<MdFloatingWindow>((window, _) => window.UpdateMinimizedState());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdFloatingWindow>((window, _) => window.UpdateMotion());
        XProperty.Changed.AddClassHandler<MdFloatingWindow>((window, _) => window.ApplyOffset());
        YProperty.Changed.AddClassHandler<MdFloatingWindow>((window, _) => window.ApplyOffset());
    }

    public MdFloatingWindow()
    {
        // The window is positioned by its own margin, so it has to be aligned to the corner the
        // margin is measured from. Both are local values on purpose: a theme setter for alignment
        // would let a page centre the window and silently break every position it reports.
        HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left;
        VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top;
        UpdateMotion();
        _presence = new MdPresenceController(value => PseudoClasses.Set(":present", value));
        _presence.Initialize(IsOpen);
        // The content host keeps its own presence for the same reason the window does: on the way
        // down it has to stay mounted while it fades out, and on the way back up a second click has
        // to cancel the pending hide instead of fighting it.
        _contentPresence = new MdPresenceController(value =>
        {
            if (_contentHost is { } host) host.IsVisible = value;
        });
        _contentPresence.Initialize(!IsMinimized);
        _settleTimer = new DispatcherTimer();
        _settleTimer.Tick += OnSettleTimerTick;
        ApplyOffset();
        ApplyOpenVisuals();
    }

    /// <inheritdoc cref="TitleProperty"/>
    public string? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    /// <inheritdoc cref="HeaderIconProperty"/>
    public string? HeaderIcon { get => GetValue(HeaderIconProperty); set => SetValue(HeaderIconProperty, value); }

    /// <inheritdoc cref="XProperty"/>
    public double X { get => GetValue(XProperty); set => SetValue(XProperty, value); }

    /// <inheritdoc cref="YProperty"/>
    public double Y { get => GetValue(YProperty); set => SetValue(YProperty, value); }

    /// <inheritdoc cref="IsOpenProperty"/>
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }

    /// <inheritdoc cref="IsMinimizedProperty"/>
    public bool IsMinimized { get => GetValue(IsMinimizedProperty); set => SetValue(IsMinimizedProperty, value); }

    /// <inheritdoc cref="CanDragProperty"/>
    public bool CanDrag { get => GetValue(CanDragProperty); set => SetValue(CanDragProperty, value); }

    /// <inheritdoc cref="CanResizeProperty"/>
    public bool CanResize { get => GetValue(CanResizeProperty); set => SetValue(CanResizeProperty, value); }

    /// <inheritdoc cref="CanMinimizeProperty"/>
    public bool CanMinimize { get => GetValue(CanMinimizeProperty); set => SetValue(CanMinimizeProperty, value); }

    /// <inheritdoc cref="CanCloseProperty"/>
    public bool CanClose { get => GetValue(CanCloseProperty); set => SetValue(CanCloseProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachParts();
        base.OnApplyTemplate(e);

        _titleBar = e.NameScope.Find<Control>("PART_TitleBar");
        _contentHost = e.NameScope.Find<Control>("PART_ContentHost");
        _resizeGrip = e.NameScope.Find<Control>("PART_ResizeGrip");
        _minimizeButton = e.NameScope.Find<Button>("PART_MinimizeButton");
        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");

        if (_titleBar is { } titleBar)
        {
            titleBar.PointerPressed += OnTitleBarPressed;
            titleBar.PointerMoved += OnTitleBarMoved;
            titleBar.PointerReleased += OnTitleBarReleased;
            titleBar.PointerCaptureLost += OnTitleBarCaptureLost;
        }

        if (_resizeGrip is { } grip)
        {
            grip.PointerPressed += OnGripPressed;
            grip.PointerMoved += OnGripMoved;
            grip.PointerReleased += OnGripReleased;
            grip.PointerCaptureLost += OnGripCaptureLost;
        }

        if (_minimizeButton is { } minimize) minimize.Click += OnMinimizeClicked;
        if (_closeButton is { } close) close.Click += OnCloseClicked;

        UpdateMotion();
        ApplyMinimizedState();
        ApplyOpenVisuals();
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _presence.Stop();
        _contentPresence.Stop();
        _settleTimer.Stop();
        DetachParts();
        base.OnDetachedFromVisualTree(e);
    }

    private void DetachParts()
    {
        if (_titleBar is { } titleBar)
        {
            titleBar.PointerPressed -= OnTitleBarPressed;
            titleBar.PointerMoved -= OnTitleBarMoved;
            titleBar.PointerReleased -= OnTitleBarReleased;
            titleBar.PointerCaptureLost -= OnTitleBarCaptureLost;
        }

        if (_resizeGrip is { } grip)
        {
            grip.PointerPressed -= OnGripPressed;
            grip.PointerMoved -= OnGripMoved;
            grip.PointerReleased -= OnGripReleased;
            grip.PointerCaptureLost -= OnGripCaptureLost;
        }

        if (_minimizeButton is { } minimize) minimize.Click -= OnMinimizeClicked;
        if (_closeButton is { } close) close.Click -= OnCloseClicked;

        _titleBar = null;
        _contentHost = null;
        _resizeGrip = null;
        _minimizeButton = null;
        _closeButton = null;
        _dragging = false;
        _resizing = false;
        PseudoClasses.Set(":dragging", false);
        PseudoClasses.Set(":resizing", false);
    }

    /// <summary>Realises <see cref="X"/> and <see cref="Y"/> as the margin they are measured from.</summary>
    private void ApplyOffset() => Margin = new Thickness(X, Y, 0, 0);

    private void UpdateOpenState()
    {
        // Presence is held for the whole exit so the window is still mounted while it animates away;
        // dropping it on the first frame would collapse the surface and show nothing at all.
        if (IsOpen) _presence.Update(true, TimeSpan.Zero);
        else _presence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Default, MdMotionSpeed.Fast));
        ApplyOpenVisuals();
    }

    /// <summary>
    /// Builds the transitions the window and its content host travel with. Re-run whenever the
    /// motion scheme changes, because a scheme is inherited and can be changed by any ancestor:
    /// without this the window would keep animating after an application switched to reduced
    /// motion, which is the one thing that setting must not do.
    /// </summary>
    private void UpdateMotion()
    {
        Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Default),
            // The collapse travels the height, so the height is a motion property like any other.
            MdMotionTransitions.CreateDouble(this, HeightProperty, MdMotionKind.Spatial, MdMotionSpeed.Default),
            MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Default));

        // The fade belongs to the content host rather than to the window: the surface itself stays
        // opaque while what is inside it goes, which is what reads as collapsing rather than as the
        // whole window blinking.
        if (_contentHost is { } host)
        {
            host.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(host, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        }
    }

    private void ApplyOpenVisuals()
    {
        Opacity = IsOpen ? 1d : 0d;
        RenderTransform = MdContentPhaser.Scale(IsOpen ? 1d : 0.92d);
        IsHitTestVisible = IsOpen;
    }

    private void UpdateMinimizedState()
    {
        PseudoClasses.Set(":minimized", IsMinimized);

        // Before the first measure there is no size to travel from, so the state is applied as it
        // stands; animating from zero would drive the window past its own title bar.
        if (_contentHost is not { } host || Bounds.Height <= 0)
        {
            ApplyMinimizedState();
            return;
        }

        if (IsMinimized) Collapse(host);
        else Expand(host);
    }

    /// <summary>
    /// Sets the minimized state with no travel: what the template gets when it is applied, and what
    /// a window gets if it is minimized before it has ever been measured.
    /// </summary>
    private void ApplyMinimizedState()
    {
        _settleTimer.Stop();
        PseudoClasses.Set(":minimized", IsMinimized);
        _contentPresence.Initialize(!IsMinimized);
        if (_contentHost is { } host) host.Opacity = IsMinimized ? 0d : 1d;

        // An explicit height would leave a tall empty box behind, so the title bar decides the
        // height while collapsed.
        if (IsMinimized)
        {
            RelaxMinHeightForCollapse();
            SetHeightWithoutMotion(double.NaN);
        }
        else
        {
            RestoreMinHeight();
        }

        _expandedHeight = double.NaN;
        _wasAutoSized = false;
    }

    private void Collapse(Control host)
    {
        var current = Bounds.Height;
        var contentHeight = host.Bounds.Height;

        // Restoring has to know where to go back to, and an auto-sized window has no Height to
        // remember, so the measured height is the answer for both.
        _wasAutoSized = double.IsNaN(Height);
        _expandedHeight = _wasAutoSized ? current : Height;

        // Freezing the height at the size it already has is what gives the collapse a number to
        // start from. The value equals the measured height, so nothing moves on this frame.
        SetHeightWithoutMotion(current);

        // Collapsed means the content row is gone, so what is left is the title bar plus the root
        // border and nothing else. Reading that off the title bar rather than subtracting the
        // content row keeps the answer right however the rows are laid out; the subtraction stays
        // as the fallback for a window collapsed before its parts have been measured.
        var collapsed = CollapsedHeight();
        if (double.IsNaN(collapsed))
            collapsed = contentHeight > 0 ? Math.Max(MinHeight, current - contentHeight) : MinHeight;

        // The floor has to come down before the height does. Otherwise the layout holds the strip
        // at the expanded minimum and the difference shows as empty space under the title bar.
        RelaxMinHeightForCollapse();

        host.Opacity = 0d;
        Height = collapsed;

        // Both the hide and the return to auto sizing wait for the travel to land. Hiding the
        // content first would blank the surface mid-collapse; going auto-sized first would cancel
        // the animation and snap.
        _contentPresence.Update(false, ExitDuration());
        ScheduleHeightSettle(double.NaN);
    }

    private void Expand(Control host)
    {
        // The collapse may still be waiting to hand sizing back to the layout, and letting that
        // land after a restore would drop the window straight back down.
        _settleTimer.Stop();

        var expanded = _expandedHeight;
        var wasAutoSized = _wasAutoSized;
        _expandedHeight = double.NaN;
        _wasAutoSized = false;

        _contentPresence.Update(true, TimeSpan.Zero);
        host.Opacity = 1d;

        // The collapse ends by handing sizing back to the layout, which leaves Height at NaN - and
        // a travel cannot leave from NaN, it just snaps. Freezing the measured height first gives
        // the return journey a real number to start from, exactly as Collapse does before its own.
        var current = Bounds.Height;

        RestoreMinHeight();

        // Nothing to travel back to means the window is measuring itself, which is already the
        // right answer, so the height is left alone rather than overwritten with a guess.
        if (expanded > 0)
        {
            SetHeightWithoutMotion(current);
            Height = expanded;
        }

        // A window that was measuring itself has to go back to it, but only once the travel lands.
        if (wasAutoSized && expanded > 0) ScheduleHeightSettle(double.NaN);
    }

    /// <summary>
    /// What the collapsed window is: the title bar plus the root border. Taking it off the parts
    /// rather than naming a number keeps the strip exact if the title bar's own minimum changes.
    /// NaN means the parts have not been measured yet, and the caller has to fall back.
    /// </summary>
    private double CollapsedHeight()
    {
        var bar = _titleBar?.Bounds.Height ?? 0d;
        if (bar <= 0) return double.NaN;

        var border = BorderThickness;
        return bar + border.Top + border.Bottom;
    }

    /// <summary>
    /// Lets the collapsed strip be as short as the title bar even when <c>MinHeight</c> asks for
    /// more. That floor belongs to the expanded surface; holding it while minimized is exactly what
    /// leaves a band of empty space under the title bar. Written through <c>SetCurrentValue</c> so a
    /// binding on <c>MinHeight</c> survives the round trip.
    /// </summary>
    private void RelaxMinHeightForCollapse()
    {
        var collapsed = CollapsedHeight();
        if (double.IsNaN(collapsed) || MinHeight <= collapsed) return;

        if (double.IsNaN(_minHeightOverride)) _minHeightOverride = MinHeight;
        SetCurrentValue(MinHeightProperty, collapsed);
    }

    /// <summary>Gives <c>MinHeight</c> back what the collapsed state took from it.</summary>
    private void RestoreMinHeight()
    {
        if (double.IsNaN(_minHeightOverride)) return;

        SetCurrentValue(MinHeightProperty, _minHeightOverride);
        _minHeightOverride = double.NaN;
    }

    /// <summary>How long the state change takes to land: the longer of the travel and the fade.</summary>
    private TimeSpan ExitDuration() => MdMotion.GetExitDuration(this, MdMotionSpeed.Default, MdMotionSpeed.Fast);

    private void ScheduleHeightSettle(double value)
    {
        _pendingHeight = value;
        _settleTimer.Stop();
        var duration = ExitDuration();
        if (duration <= TimeSpan.Zero)
        {
            SetHeightWithoutMotion(value);
            return;
        }

        _settleTimer.Interval = duration;
        _settleTimer.Start();
    }

    private void OnSettleTimerTick(object? sender, EventArgs e) => SetHeightWithoutMotion(_pendingHeight);

    /// <summary>
    /// Writes the control's <c>Height</c> with the transitions detached. Establishing the value an
    /// animation starts from, and returning to auto sizing after one has landed, both have to
    /// happen without travelling - only the travel itself should animate.
    /// </summary>
    private void SetHeightWithoutMotion(double value)
    {
        _settleTimer.Stop();
        var transitions = Transitions;
        Transitions = null;
        Height = value;
        Transitions = transitions;
    }

    private void OnMinimizeClicked(object? sender, RoutedEventArgs e)
    {
        IsMinimized = !IsMinimized;
        e.Handled = true;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        IsOpen = false;
        RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
        e.Handled = true;
    }

    private void OnTitleBarPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!CanDrag || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        BeginGesture(e, _titleBar, dragging: true);
        _dragOrigin = new Point(X, Y);
    }

    private void OnTitleBarMoved(object? sender, PointerEventArgs e)
    {
        if (!_dragging || _dragHost is null) return;
        var current = e.GetPosition(_dragHost);
        var target = ClampToHost(new Point(
            _dragOrigin.X + current.X - _pressPosition.X,
            _dragOrigin.Y + current.Y - _pressPosition.Y));
        X = target.X;
        Y = target.Y;
        e.Handled = true;
    }

    private void OnTitleBarReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_dragging) return;
        EndGesture(e.Pointer, dragging: true);
    }

    private void OnTitleBarCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (!_dragging) return;
        EndGesture(null, dragging: true);
    }

    private void OnGripPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!CanResize || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        BeginGesture(e, _resizeGrip, dragging: false);
        // An auto-sized window reports NaN for Width and Height, so fall back to what it measured.
        _resizeOrigin = new Size(
            double.IsNaN(Width) ? Bounds.Width : Width,
            double.IsNaN(Height) ? Bounds.Height : Height);
    }

    private void OnGripMoved(object? sender, PointerEventArgs e)
    {
        if (!_resizing || _dragHost is null) return;
        var current = e.GetPosition(_dragHost);
        Width = Math.Max(MinWidth, _resizeOrigin.Width + current.X - _pressPosition.X);
        Height = Math.Max(MinHeight, _resizeOrigin.Height + current.Y - _pressPosition.Y);
        e.Handled = true;
    }

    private void OnGripReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_resizing) return;
        EndGesture(e.Pointer, dragging: false);
    }

    private void OnGripCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (!_resizing) return;
        EndGesture(null, dragging: false);
    }

    /// <summary>
    /// Starts a drag or a resize. Positions are taken against the visual parent rather than against
    /// the window itself: a window that measured the pointer relative to its own moving origin would
    /// chase the cursor and drift further behind on every frame.
    /// </summary>
    private void BeginGesture(PointerPressedEventArgs e, Control? captureTarget, bool dragging)
    {
        _dragHost = this.GetVisualParent() as Visual ?? this;
        _pressPosition = e.GetPosition(_dragHost);
        if (dragging) _dragging = true;
        else _resizing = true;
        e.Pointer.Capture(captureTarget);
        PseudoClasses.Set(dragging ? ":dragging" : ":resizing", true);
        e.Handled = true;
    }

    private void EndGesture(IPointer? pointer, bool dragging)
    {
        if (dragging) _dragging = false;
        else _resizing = false;
        pointer?.Capture(null);
        PseudoClasses.Set(dragging ? ":dragging" : ":resizing", false);
        _dragHost = null;
    }

    /// <summary>
    /// Keeps a position inside the visual parent. A window the user can move but not lose is the
    /// whole point of bounding it in-app, so the clamp runs on every move rather than on release.
    /// </summary>
    private Point ClampToHost(Point position)
    {
        var host = this.GetVisualParent() as Visual;
        var maxX = Math.Max(0, (host?.Bounds.Width ?? double.PositiveInfinity) - Bounds.Width);
        var maxY = Math.Max(0, (host?.Bounds.Height ?? double.PositiveInfinity) - Bounds.Height);
        return new Point(Math.Clamp(position.X, 0, maxX), Math.Clamp(position.Y, 0, maxY));
    }
}
