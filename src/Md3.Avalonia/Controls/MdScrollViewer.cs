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
/// A scoped Material scrolling surface with smooth gesture drag scrolling, kinetic inertia physics,
/// touch/mouse tracking, and native extent/offset/chaining support.
/// </summary>
public sealed class MdScrollViewer : ScrollViewer
{
    public static readonly StyledProperty<bool> IsDragScrollingEnabledProperty =
        AvaloniaProperty.Register<MdScrollViewer, bool>(nameof(IsDragScrollingEnabled), defaultValue: true);

    public static readonly StyledProperty<bool> AllowMouseDragProperty =
        AvaloniaProperty.Register<MdScrollViewer, bool>(nameof(AllowMouseDrag), defaultValue: true);

    public static readonly StyledProperty<double> DragThresholdProperty =
        AvaloniaProperty.Register<MdScrollViewer, double>(nameof(DragThreshold), defaultValue: 8.0);

    public static readonly StyledProperty<double> FrictionProperty =
        AvaloniaProperty.Register<MdScrollViewer, double>(nameof(Friction), defaultValue: 0.94);

    public bool IsDragScrollingEnabled
    {
        get => GetValue(IsDragScrollingEnabledProperty);
        set => SetValue(IsDragScrollingEnabledProperty, value);
    }

    public bool AllowMouseDrag
    {
        get => GetValue(AllowMouseDragProperty);
        set => SetValue(AllowMouseDragProperty, value);
    }

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

        // Tunnel events to intercept touch drag before child controls swallow them
        AddHandler(PointerPressedEvent, OnPreviewPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, OnPreviewPointerMoved, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, OnPreviewPointerReleased, RoutingStrategies.Tunnel);
        AddHandler(PointerCaptureLostEvent, OnPreviewPointerCaptureLost, RoutingStrategies.Tunnel);
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

    private static bool IsInteractiveEditableControl(Visual? visual)
    {
        if (visual is null) return false;
        var cur = visual;
        while (cur is not null and not MdScrollViewer)
        {
            if (cur is TextBox or AutoCompleteBox or Slider or ScrollBar or Thumb or RepeatButton)
                return true;
            cur = cur.GetVisualParent();
        }
        return false;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        StopInertia();
        base.OnPointerWheelChanged(e);
    }

    private void OnPreviewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsDragScrollingEnabled) return;

        // Never intercept clicks on scrollbars, thumbs, or buttons - let native scrollbar dragging work
        if (IsInsideScrollBar(e.Source as Visual)) return;

        // For mouse pointers on desktop, don't drag if not allowed or if interacting with editable/slider controls
        if (e.Pointer.Type == PointerType.Mouse)
        {
            if (!AllowMouseDrag || IsInteractiveEditableControl(e.Source as Visual))
                return;
        }

        var currentPoint = e.GetCurrentPoint(this);
        var props = currentPoint.Properties;
        if (!props.IsLeftButtonPressed && props.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) return;

        // Stop any active inertia immediately on touch/click (smooth catch)
        StopInertia();

        _pointerStartPos = currentPoint.Position;
        _startOffset = Offset;
        _isPressed = true;
        _isDragging = false;
        _capturedPointer = e.Pointer;

        _stopwatch.Restart();
        _positionHistory.Clear();
        _positionHistory.Add((0, _pointerStartPos));
    }

    private void OnPreviewPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isPressed || !IsDragScrollingEnabled) return;

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
            var threshold = e.Pointer.Type == PointerType.Mouse ? Math.Max(DragThreshold, 10.0) : DragThreshold;
            if (dist >= threshold)
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

    private void OnPreviewPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isPressed) return;

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
    }

    private void OnPreviewPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (Equals(e.Pointer.Captured, this))
        {
            _isPressed = false;
            _isDragging = false;
            _capturedPointer = null;
        }
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
        StopInertia();
        base.OnDetachedFromVisualTree(e);
    }
}
