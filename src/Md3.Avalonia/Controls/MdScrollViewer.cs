using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A Material scrolling surface that delegates wheel, touch, pen, inertia, snap-point, and
/// scroll-chaining behavior to Avalonia's native <see cref="ScrollContentPresenter"/>. An optional
/// mouse-only drag path is layered on top for desktop canvas-style interaction.
/// </summary>
public sealed class MdScrollViewer : ScrollViewer
{
    public static readonly StyledProperty<bool> IsDragScrollingEnabledProperty =
        AvaloniaProperty.Register<MdScrollViewer, bool>(nameof(IsDragScrollingEnabled), defaultValue: true);

    public static readonly StyledProperty<bool> AllowMouseDragProperty =
        AvaloniaProperty.Register<MdScrollViewer, bool>(nameof(AllowMouseDrag));

    public static readonly AttachedProperty<bool> SuppressMouseDragScrollingProperty =
        AvaloniaProperty.RegisterAttached<MdScrollViewer, InputElement, bool>(
            "SuppressMouseDragScrolling", defaultValue: false, inherits: true);

    public static readonly StyledProperty<double> DragThresholdProperty =
        AvaloniaProperty.Register<MdScrollViewer, double>(nameof(DragThreshold), defaultValue: 4.0);

    public static readonly StyledProperty<double> FrictionProperty =
        AvaloniaProperty.Register<MdScrollViewer, double>(nameof(Friction), defaultValue: 0.94);

    public bool IsDragScrollingEnabled
    {
        get => GetValue(IsDragScrollingEnabledProperty);
        set => SetValue(IsDragScrollingEnabledProperty, value);
    }

    /// <summary>
    /// Enables canvas-style scrolling with the primary mouse button. Disabled by default on
    /// desktop so mouse selection and direct manipulation inside the content take precedence.
    /// Touch, pen, wheel, trackpad, and scrollbar input are unaffected.
    /// </summary>
    public bool AllowMouseDrag
    {
        get => GetValue(AllowMouseDragProperty);
        set => SetValue(AllowMouseDragProperty, value);
    }

    /// <summary>Gets whether an input subtree is excluded from optional mouse drag scrolling.</summary>
    public static bool GetSuppressMouseDragScrolling(InputElement element) =>
        element.GetValue(SuppressMouseDragScrollingProperty);

    /// <summary>Excludes an input subtree from optional mouse drag scrolling.</summary>
    public static void SetSuppressMouseDragScrolling(InputElement element, bool value) =>
        element.SetValue(SuppressMouseDragScrollingProperty, value);

    public double DragThreshold
    {
        get => GetValue(DragThresholdProperty);
        set => SetValue(DragThresholdProperty, value);
    }

    public double Friction
    {
        get => GetValue(FrictionProperty);
        set => SetValue(FrictionProperty, value);
    }

    private Point _pointerStartPos;
    private Vector _startOffset;
    private bool _isPressed;
    private bool _isDragging;
    private IPointer? _capturedPointer;
    private InputElement? _contentInputRoot;
    private bool _contentPressObserved;
    private bool _contentHandledAtTunnel;
    private object? _contentCaptureAtTunnel;
    private ScrollBar? _verticalScrollBar;
    private ScrollBar? _horizontalScrollBar;

    // Velocity tracker
    private readonly Stopwatch _stopwatch = new();
    private readonly List<(long timeMs, Point pos)> _positionHistory = new(16);

    // Inertia physics loop
    private readonly DispatcherTimer _inertiaTimer;
    private Vector _velocity; // pixels per second
    private long _lastInertiaTickMs;

    public MdScrollViewer()
    {
        Background = Brushes.Transparent;
        _inertiaTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _inertiaTimer.Tick += OnInertiaTick;

        // Avalonia's ScrollGestureRecognizer in the template owns touch and pen input. Its
        // presenter consumes bubbling mouse events even when mouse panning is explicitly enabled,
        // so the compatibility path observes the tunnel route. It rejects focusable direct-
        // manipulation content and explicitly suppressed precision-interaction subtrees, then
        // yields if custom content claims pointer capture.
        AddHandler(PointerPressedEvent, OnMousePointerPressed, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, OnMousePointerMoved, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, OnMousePointerReleased,
            RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(PointerCaptureLostEvent, OnMousePointerCaptureLost,
            RoutingStrategies.Bubble, handledEventsToo: true);

        // Wheel scrolling remains native; only cancel residual mouse-drag inertia before the
        // presenter consumes a new wheel gesture.
        AddHandler(PointerWheelChangedEvent, OnPreviewPointerWheelChanged,
            RoutingStrategies.Tunnel, handledEventsToo: true);

        LayoutUpdated += OnFirstLayoutUpdated;
    }

    private void OnFirstLayoutUpdated(object? sender, EventArgs e)
    {
        LayoutUpdated -= OnFirstLayoutUpdated;

        // Some virtualizing compositions realize their containers one layout pass late on
        // desktop platforms; the accidental cure users applied was resizing the window. One
        // extra measure pass after the first layout reproduces that cure before the first
        // frame paints, at the cost of a single startup pass.
        InvalidateMeasure();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty)
        {
            AttachContentPressObserver(Content as InputElement);
        }
        else if (change.Property == ExtentProperty ||
                 change.Property == ViewportProperty ||
                 change.Property == OffsetProperty ||
                 change.Property == VerticalScrollBarVisibilityProperty ||
                 change.Property == HorizontalScrollBarVisibilityProperty)
        {
            SyncScrollBars();
        }
    }

    /// <summary>
    /// Captures the template scroll bars and calibrates them synchronously.
    /// </summary>
    /// <remarks>
    /// <c>ScrollBar.AttachToScrollViewer</c> creates its self-bindings only when the bar
    /// attaches to the visual tree, so depending on it alone can leave the first rendered frame
    /// - or any frame whose Extent/Viewport arrives through an unlucky ordering - at RangeBase
    /// defaults (a full-track thumb) until some later invalidation, classically a window resize;
    /// the defect was only reproducible on desktop platforms. <see cref="SyncScrollBars"/> writes
    /// the mirror values directly, synchronously, on every relevant owner change, so the bars
    /// track the owner from the very first frame on every platform, with no resize and no
    /// binding/attach ordering involved. XAML cannot express the mirror either:
    /// <c>TemplateBinding</c> rejects the <c>ScrollBarMaximum.Y</c>-style paths (AVLN2000).
    /// </remarks>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _verticalScrollBar = e.NameScope.Find<ScrollBar>("PART_VerticalScrollBar");
        _horizontalScrollBar = e.NameScope.Find<ScrollBar>("PART_HorizontalScrollBar");
        SyncScrollBars();
    }

    private void SyncScrollBars()
    {
        if (_verticalScrollBar is { } vertical)
        {
            vertical.SetCurrentValue(RangeBase.MaximumProperty, ScrollBarMaximum.Y);
            vertical.SetCurrentValue(RangeBase.ValueProperty, Offset.Y);
            vertical.SetCurrentValue(ScrollBar.ViewportSizeProperty, Viewport.Height);
            vertical.SetCurrentValue(ScrollBar.VisibilityProperty, VerticalScrollBarVisibility);
        }

        if (_horizontalScrollBar is { } horizontal)
        {
            horizontal.SetCurrentValue(RangeBase.MaximumProperty, ScrollBarMaximum.X);
            horizontal.SetCurrentValue(RangeBase.ValueProperty, Offset.X);
            horizontal.SetCurrentValue(ScrollBar.ViewportSizeProperty, Viewport.Width);
            horizontal.SetCurrentValue(ScrollBar.VisibilityProperty, HorizontalScrollBarVisibility);
        }
    }

    private void AttachContentPressObserver(InputElement? contentRoot)
    {
        if (ReferenceEquals(_contentInputRoot, contentRoot)) return;
        if (_contentInputRoot is not null)
        {
            _contentInputRoot.RemoveHandler(PointerPressedEvent, OnContentPointerPressedTunnel);
            _contentInputRoot.RemoveHandler(PointerPressedEvent, OnContentPointerPressedBubble);
        }

        _contentInputRoot = contentRoot;
        if (_contentInputRoot is not null)
        {
            // Observe both sides of the user-content route. Any handled/capture transition between
            // them belongs to content, while presenter gesture state already exists at tunnel entry.
            _contentInputRoot.AddHandler(PointerPressedEvent, OnContentPointerPressedTunnel,
                RoutingStrategies.Tunnel, handledEventsToo: true);
            _contentInputRoot.AddHandler(PointerPressedEvent, OnContentPointerPressedBubble,
                RoutingStrategies.Bubble, handledEventsToo: true);
        }
    }

    public void StopInertia()
    {
        _inertiaTimer.Stop();
        _velocity = default;
    }

    private bool IsInsideScrollBar(Visual? visual)
    {
        if (visual is null) return false;
        if (visual is ScrollBar or Thumb or RepeatButton) return true;
        return visual.GetVisualAncestors().OfType<ScrollBar>().Any();
    }

    private bool ShouldDeferMouseDrag(PointerPressedEventArgs e)
    {
        if (IsInsideScrollBar(e.Source as Visual)) return true;

        // Classify direct-manipulation source subtrees before the template presenter can consume
        // the bubbling event.
        if (e.Source is not Visual source) return false;
        foreach (var visual in source.GetVisualAncestors().Prepend(source))
        {
            // Template infrastructure can itself be focusable; only classify the user-content
            // side of the presenter as a direct-manipulation subtree.
            if (ReferenceEquals(visual, this) || ReferenceEquals(visual, Presenter)) break;
            if (visual is InputElement input &&
                (GetSuppressMouseDragScrolling(input) || input.Focusable))
                return true;
            if (ReferenceEquals(visual.GetVisualParent(), Presenter)) break;
        }

        return false;
    }

    private void OnContentPointerPressedTunnel(object? sender, PointerPressedEventArgs e)
    {
        if (!_isPressed || !ReferenceEquals(e.Pointer, _capturedPointer)) return;
        _contentPressObserved = true;
        _contentHandledAtTunnel = e.Handled;
        _contentCaptureAtTunnel = e.Pointer.Captured;
    }

    private void OnContentPointerPressedBubble(object? sender, PointerPressedEventArgs e)
    {
        if (!_isPressed || !_contentPressObserved || !ReferenceEquals(e.Pointer, _capturedPointer)) return;

        // A transition while routing through user content means a descendant accepted the press.
        // Cancel the outer candidate before its first move can capture the pointer.
        if ((e.Handled && !_contentHandledAtTunnel) ||
            !ReferenceEquals(e.Pointer.Captured, _contentCaptureAtTunnel))
            ResetMouseDragTracking(releaseOwnCapture: false);

        _contentPressObserved = false;
        _contentCaptureAtTunnel = null;
    }

    private void OnMousePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsDragScrollingEnabled || !AllowMouseDrag || e.Pointer.Type != PointerType.Mouse) return;

        // Focusable controls and explicitly suppressed precision content own primary-button drags.
        // Mouse panning remains available from background surfaces when explicitly enabled.
        if (ShouldDeferMouseDrag(e)) return;

        var currentPoint = e.GetCurrentPoint(this);
        var props = currentPoint.Properties;
        if (!props.IsLeftButtonPressed && props.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) return;

        // Stop any active mouse inertia immediately when a new mouse gesture begins.
        StopInertia();

        _pointerStartPos = currentPoint.Position;
        _startOffset = Offset;
        _isPressed = true;
        _isDragging = false;
        _capturedPointer = e.Pointer;
        _contentPressObserved = false;
        _contentCaptureAtTunnel = null;

        _stopwatch.Restart();
        _positionHistory.Clear();
        _positionHistory.Add((0, _pointerStartPos));
    }

    private void OnMousePointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isPressed || !ReferenceEquals(e.Pointer, _capturedPointer)) return;
        if (!IsDragScrollingEnabled || !AllowMouseDrag || e.Pointer.Type != PointerType.Mouse)
        {
            ResetMouseDragTracking(releaseOwnCapture: true);
            return;
        }

        var currentPoint = e.GetCurrentPoint(this);
        var currentPos = currentPoint.Position;
        var nowMs = _stopwatch.ElapsedMilliseconds;

        // Maintain rolling history of recent points within 120ms
        _positionHistory.Add((nowMs, currentPos));
        while (_positionHistory.Count > 1 && nowMs - _positionHistory[0].timeMs > 120)
        {
            _positionHistory.RemoveAt(0);
        }

        var delta = currentPos - _pointerStartPos;

        if (!_isDragging)
        {
            var dist = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
            if (dist >= DragThreshold)
            {
                _isDragging = true;
                _capturedPointer?.Capture(this);
            }
        }

        if (_isDragging)
        {
            var scrollPresenter = Presenter as ScrollContentPresenter;
            var extentW = Math.Max(Extent.Width, scrollPresenter?.Extent.Width ?? 0);
            var extentH = Math.Max(Extent.Height, scrollPresenter?.Extent.Height ?? 0);
            if (extentH == 0 && Content is Control contentControl)
            {
                extentH = Math.Max(contentControl.DesiredSize.Height, contentControl.Bounds.Height);
                extentW = Math.Max(contentControl.DesiredSize.Width, contentControl.Bounds.Width);
            }

            var viewportW = Math.Max(Viewport.Width, scrollPresenter?.Viewport.Width ?? Bounds.Width);
            var viewportH = Math.Max(Viewport.Height, scrollPresenter?.Viewport.Height ?? Bounds.Height);

            var maxOffsetX = Math.Max(0, extentW - viewportW);
            var maxOffsetY = Math.Max(0, extentH - viewportH);

            var newX = maxOffsetX > 0 ? Math.Clamp(_startOffset.X - delta.X, 0, maxOffsetX) : Math.Max(0, _startOffset.X - delta.X);
            var newY = maxOffsetY > 0 ? Math.Clamp(_startOffset.Y - delta.Y, 0, maxOffsetY) : Math.Max(0, _startOffset.Y - delta.Y);

            Offset = new Vector(newX, newY);
            e.Handled = true;
        }
    }

    private void OnMousePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isPressed || !ReferenceEquals(e.Pointer, _capturedPointer)) return;

        _isPressed = false;
        var wasDragging = _isDragging;
        _isDragging = false;

        if (wasDragging)
        {
            if (Equals(e.Pointer.Captured, this))
            {
                e.Pointer.Capture(null);
            }
            e.Handled = true;

            // Calculate release velocity from recent history
            if (_positionHistory.Count >= 2 && IsScrollInertiaEnabled)
            {
                var nowMs = _stopwatch.ElapsedMilliseconds;
                var (oldMs, oldPos) = _positionHistory[0];
                var dt = (nowMs - oldMs) / 1000.0;
                if (dt > 0.01)
                {
                    var currentPos = e.GetCurrentPoint(this).Position;
                    // Negative delta position = positive scroll offset delta
                    var vx = -(currentPos.X - oldPos.X) / dt;
                    var vy = -(currentPos.Y - oldPos.Y) / dt;

                    // Clamp initial fling velocity to avoid wild jumps
                    const double maxVelocity = 4000.0;
                    vx = Math.Clamp(vx, -maxVelocity, maxVelocity);
                    vy = Math.Clamp(vy, -maxVelocity, maxVelocity);

                    if (Math.Abs(vx) > 60 || Math.Abs(vy) > 60)
                    {
                        _velocity = new Vector(vx, vy);
                        _lastInertiaTickMs = _stopwatch.ElapsedMilliseconds;
                        _inertiaTimer.Start();
                    }
                }
            }
        }

        _capturedPointer = null;
        _contentPressObserved = false;
        _contentCaptureAtTunnel = null;
    }

    private void ResetMouseDragTracking(bool releaseOwnCapture)
    {
        var pointer = _capturedPointer;
        _isPressed = false;
        _isDragging = false;
        _capturedPointer = null;
        _contentPressObserved = false;
        _contentCaptureAtTunnel = null;
        _positionHistory.Clear();
        _stopwatch.Stop();
        if (releaseOwnCapture && ReferenceEquals(pointer?.Captured, this)) pointer.Capture(null);
    }

    private void OnMousePointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (ReferenceEquals(e.Pointer, _capturedPointer))
            ResetMouseDragTracking(releaseOwnCapture: false);
    }

    private void OnPreviewPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        StopInertia();
    }

    private void OnInertiaTick(object? sender, EventArgs e)
    {
        var nowMs = _stopwatch.ElapsedMilliseconds;
        var dt = Math.Min(0.05, Math.Max(0.001, (nowMs - _lastInertiaTickMs) / 1000.0));
        _lastInertiaTickMs = nowMs;

        var currentOffset = Offset;
        var scrollPresenter = Presenter as ScrollContentPresenter;
        var extentW = Math.Max(Extent.Width, scrollPresenter?.Extent.Width ?? 0);
        var extentH = Math.Max(Extent.Height, scrollPresenter?.Extent.Height ?? 0);
        var viewportW = Math.Max(Viewport.Width, scrollPresenter?.Viewport.Width ?? Bounds.Width);
        var viewportH = Math.Max(Viewport.Height, scrollPresenter?.Viewport.Height ?? Bounds.Height);

        var maxOffsetX = Math.Max(0, extentW - viewportW);
        var maxOffsetY = Math.Max(0, extentH - viewportH);

        var newX = maxOffsetX > 0 ? Math.Clamp(currentOffset.X + _velocity.X * dt, 0, maxOffsetX) : Math.Max(0, currentOffset.X + _velocity.X * dt);
        var newY = maxOffsetY > 0 ? Math.Clamp(currentOffset.Y + _velocity.Y * dt, 0, maxOffsetY) : Math.Max(0, currentOffset.Y + _velocity.Y * dt);

        Offset = new Vector(newX, newY);

        // Exponential decay friction
        var frameFriction = Math.Pow(Friction, dt / 0.016);
        _velocity = new Vector(_velocity.X * frameFriction, _velocity.Y * frameFriction);

        // Stop if velocity is low or reached boundary
        if ((Math.Abs(_velocity.X) < 15 && Math.Abs(_velocity.Y) < 15) ||
            (newX <= 0 && _velocity.X < 0) || (maxOffsetX > 0 && newX >= maxOffsetX && _velocity.X > 0) ||
            (newY <= 0 && _velocity.Y < 0) || (maxOffsetY > 0 && newY >= maxOffsetY && _velocity.Y > 0))
        {
            StopInertia();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ResetMouseDragTracking(releaseOwnCapture: true);
        StopInertia();
        base.OnDetachedFromVisualTree(e);
    }
}
