using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>Coordinates a draggable bottom sheet that resizes before its nested scroller consumes input.</summary>
[PseudoClasses(":dragging", ":minimum", ":maximum", ":animating", ":reduced-motion", ":no-motion")]
public sealed class MdDraggableScrollableSheet : ContentControl
{
    public static readonly StyledProperty<double> MinimumExtentProperty = AvaloniaProperty.Register<MdDraggableScrollableSheet, double>(nameof(MinimumExtent), 0.25);
    public static readonly StyledProperty<double> InitialExtentProperty = AvaloniaProperty.Register<MdDraggableScrollableSheet, double>(nameof(InitialExtent), 0.5);
    public static readonly StyledProperty<double> MaximumExtentProperty = AvaloniaProperty.Register<MdDraggableScrollableSheet, double>(nameof(MaximumExtent), 1.0);
    public static readonly StyledProperty<double> ExtentProperty = AvaloniaProperty.Register<MdDraggableScrollableSheet, double>(nameof(Extent), 0.5, defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> SnapProperty = AvaloniaProperty.Register<MdDraggableScrollableSheet, bool>(nameof(Snap), true);
    public static readonly StyledProperty<IReadOnlyList<double>> SnapSizesProperty = AvaloniaProperty.Register<MdDraggableScrollableSheet, IReadOnlyList<double>>(nameof(SnapSizes), new[] { 0.25, 0.5, 1.0 });
    public static readonly StyledProperty<bool> ExpandProperty = AvaloniaProperty.Register<MdDraggableScrollableSheet, bool>(nameof(Expand), true);
    public static readonly StyledProperty<object?> HeaderProperty = AvaloniaProperty.Register<MdDraggableScrollableSheet, object?>(nameof(Header));
    public static readonly DirectProperty<MdDraggableScrollableSheet, double> SheetHeightProperty = AvaloniaProperty.RegisterDirect<MdDraggableScrollableSheet, double>(nameof(SheetHeight), sheet => sheet.SheetHeight);
    public static readonly DirectProperty<MdDraggableScrollableSheet, bool> IsAnimatingProperty = AvaloniaProperty.RegisterDirect<MdDraggableScrollableSheet, bool>(nameof(IsAnimating), sheet => sheet.IsAnimating);

    /// <summary>Backing styled property for <see cref="ShouldCloseOnMinExtent"/>.</summary>
    public static readonly StyledProperty<bool> ShouldCloseOnMinExtentProperty = AvaloniaProperty.Register<MdDraggableScrollableSheet, bool>(nameof(ShouldCloseOnMinExtent), true);

    /// <summary>Backing styled property for <see cref="SnapAnimationDuration"/>.</summary>
    public static readonly StyledProperty<TimeSpan?> SnapAnimationDurationProperty = AvaloniaProperty.Register<MdDraggableScrollableSheet, TimeSpan?>(nameof(SnapAnimationDuration));

    /// <summary>Backing styled property for <see cref="Controller"/>.</summary>
    public static readonly StyledProperty<MdDraggableScrollableController?> ControllerProperty = AvaloniaProperty.Register<MdDraggableScrollableSheet, MdDraggableScrollableController?>(nameof(Controller));

    private Control? _dragHandle;
    private Border? _surface;
    private bool _dragging;
    private Point _start;
    private double _startExtent;
    private double _lastPointerY;
    private ulong _lastPointerTimestamp;
    private double _verticalVelocity;
    private double _sheetHeight;
    private bool _isAnimating;
    private CancellationTokenSource? _animationCancellation;
    private int _jumpVersion;

    static MdDraggableScrollableSheet()
    {
        MinimumExtentProperty.Changed.AddClassHandler<MdDraggableScrollableSheet>((sheet, _) => sheet.CoerceExtents());
        InitialExtentProperty.Changed.AddClassHandler<MdDraggableScrollableSheet>((sheet, _) => sheet.CoerceExtents());
        MaximumExtentProperty.Changed.AddClassHandler<MdDraggableScrollableSheet>((sheet, _) => sheet.CoerceExtents());
        ExtentProperty.Changed.AddClassHandler<MdDraggableScrollableSheet>((sheet, _) => sheet.OnExtentChanged());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdDraggableScrollableSheet>((sheet, _) => sheet.OnMotionSchemeChanged());
        SnapAnimationDurationProperty.Changed.AddClassHandler<MdDraggableScrollableSheet>((sheet, _) => sheet.UpdateMotion());
        ControllerProperty.Changed.AddClassHandler<MdDraggableScrollableSheet>((sheet, change) =>
        {
            (change.OldValue as MdDraggableScrollableController)?.Detach(sheet);
            (change.NewValue as MdDraggableScrollableController)?.Attach(sheet);
        });
    }

    public MdDraggableScrollableSheet()
    {
        SetCurrentValue(ExtentProperty, InitialExtent);
        UpdateHeight();
    }

    public double MinimumExtent { get => GetValue(MinimumExtentProperty); set => SetValue(MinimumExtentProperty, value); }
    public double InitialExtent { get => GetValue(InitialExtentProperty); set => SetValue(InitialExtentProperty, value); }
    public double MaximumExtent { get => GetValue(MaximumExtentProperty); set => SetValue(MaximumExtentProperty, value); }
    public double Extent { get => GetValue(ExtentProperty); set => SetValue(ExtentProperty, value); }
    public bool Snap { get => GetValue(SnapProperty); set => SetValue(SnapProperty, value); }
    public IReadOnlyList<double> SnapSizes { get => GetValue(SnapSizesProperty); set => SetValue(SnapSizesProperty, value); }
    public bool Expand { get => GetValue(ExpandProperty); set => SetValue(ExpandProperty, value); }
    public object? Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }

    /// <summary>
    /// When <c>true</c> (the default), a drag that settles at <see cref="MinimumExtent"/> raises
    /// <see cref="MinExtentReached"/> so the host can close the sheet, mirroring Flutter's
    /// <c>DraggableScrollableSheet.shouldCloseOnMinExtent</c>.
    /// </summary>
    public bool ShouldCloseOnMinExtent { get => GetValue(ShouldCloseOnMinExtentProperty); set => SetValue(ShouldCloseOnMinExtentProperty, value); }

    /// <summary>
    /// Duration for snap/settle animations. <c>null</c> (the default) keeps the motion system's
    /// spatial duration, including reduced-motion handling; <see cref="TimeSpan.Zero"/> snaps
    /// without animating.
    /// </summary>
    public TimeSpan? SnapAnimationDuration { get => GetValue(SnapAnimationDurationProperty); set => SetValue(SnapAnimationDurationProperty, value); }

    /// <summary>
    /// Optional imperative handle mirroring Flutter's <c>DraggableScrollableController</c>. The
    /// controller attaches to this sheet while it is set.
    /// </summary>
    public MdDraggableScrollableController? Controller { get => GetValue(ControllerProperty); set => SetValue(ControllerProperty, value); }

    public double SheetHeight => _sheetHeight;
    public new bool IsAnimating
    {
        get => _isAnimating;
        private set
        {
            if (_isAnimating == value) return;
            SetAndRaise(IsAnimatingProperty, ref _isAnimating, value);
            PseudoClasses.Set(":animating", value);
        }
    }

    public event EventHandler<double>? ExtentChanged;
    public event EventHandler? AnimationCompleted;

    /// <summary>
    /// Raised when a drag settles at <see cref="MinimumExtent"/> while
    /// <see cref="ShouldCloseOnMinExtent"/> is <c>true</c>.
    /// </summary>
    public event EventHandler? MinExtentReached;

    public void AnimateTo(double extent) => _ = AnimateToAsync(extent);

    /// <summary>Animates to <paramref name="extent"/>, overriding <see cref="SnapAnimationDuration"/> when given.</summary>
    public async ValueTask AnimateToAsync(double extent, TimeSpan? duration = null, CancellationToken cancellationToken = default)
    {
        CancelAnimation();
        var operation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _animationCancellation = operation;
        var spec = MdMotion.Resolve(this, MdMotionKind.Spatial, MdMotionSpeed.Default);
        UpdateMotion();
        var travel = ResolveSnapDuration(duration, spec);
        IsAnimating = spec.IsEnabled && travel > TimeSpan.Zero;
        SetExtent(extent);
        try
        {
            if (IsAnimating)
                await Task.Delay(travel, operation.Token);
            operation.Token.ThrowIfCancellationRequested();
            AnimationCompleted?.Invoke(this, EventArgs.Empty);
        }
        catch (OperationCanceledException) when (operation.IsCancellationRequested) { }
        finally
        {
            if (ReferenceEquals(_animationCancellation, operation))
            {
                _animationCancellation = null;
                IsAnimating = false;
            }
            operation.Dispose();
        }
    }

    public void JumpTo(double extent)
    {
        CancelAnimation();
        var version = ++_jumpVersion;
        if (_surface is not null) _surface.Transitions = null;
        SetExtent(extent);
        Dispatcher.UIThread.Post(() =>
        {
            if (version == _jumpVersion && !_dragging) UpdateMotion();
        }, DispatcherPriority.Render);
    }

    public void Reset() => AnimateTo(InitialExtent);

    public void SetExtent(double extent)
    {
        var value = Math.Clamp(extent, Math.Clamp(MinimumExtent, 0, 1), Math.Clamp(MaximumExtent, 0, 1));
        SetCurrentValue(ExtentProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_dragHandle is not null)
        {
            _dragHandle.PointerPressed -= OnHandlePressed;
            _dragHandle.PointerMoved -= OnHandleMoved;
            _dragHandle.PointerReleased -= OnHandleReleased;
            _dragHandle.PointerCaptureLost -= OnHandleCaptureLost;
        }
        base.OnApplyTemplate(e);
        _dragHandle = e.NameScope.Find<Control>("PART_DragHandle");
        _surface = e.NameScope.Find<Border>("PART_Surface");
        if (_dragHandle is not null)
        {
            _dragHandle.PointerPressed += OnHandlePressed;
            _dragHandle.PointerMoved += OnHandleMoved;
            _dragHandle.PointerReleased += OnHandleReleased;
            _dragHandle.PointerCaptureLost += OnHandleCaptureLost;
        }
        UpdateHeight();
        UpdateMotion();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelAnimation();
        base.OnDetachedFromVisualTree(e);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var value = base.ArrangeOverride(finalSize);
        UpdateHeight(finalSize.Height);
        return value;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        var scroller = (e.Source as Control)?.FindLogicalAncestorOfType<ScrollViewer>();
        var atTop = scroller is null || scroller.Offset.Y <= 0.5;
        var atBottom = scroller is null || scroller.Offset.Y >= Math.Max(0, scroller.Extent.Height - scroller.Viewport.Height) - 0.5;
        if (e.Delta.Y > 0 && atTop && Extent < MaximumExtent || e.Delta.Y < 0 && atBottom && Extent > MinimumExtent)
        {
            JumpTo(Extent + e.Delta.Y * 0.06);
            e.Handled = true;
            return;
        }
        base.OnPointerWheelChanged(e);
    }

    private void OnHandlePressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        CancelAnimation();
        _dragging = true;
        _start = e.GetPosition(this);
        _lastPointerY = _start.Y;
        _lastPointerTimestamp = e.Timestamp;
        _verticalVelocity = 0;
        _startExtent = Extent;
        e.Pointer.Capture(_dragHandle);
        PseudoClasses.Set(":dragging", true);
        UpdateMotion();
        e.Handled = true;
    }

    private void OnHandleMoved(object? sender, PointerEventArgs e)
    {
        if (!_dragging) return;
        var currentY = e.GetPosition(this).Y;
        var elapsed = e.Timestamp > _lastPointerTimestamp ? e.Timestamp - _lastPointerTimestamp : 1;
        _verticalVelocity = (currentY - _lastPointerY) / (double)elapsed;
        _lastPointerY = currentY;
        _lastPointerTimestamp = e.Timestamp;
        var available = Math.Max(1, Bounds.Height);
        SetExtent(_startExtent + (_start.Y - currentY) / available);
        e.Handled = true;
    }

    private void OnHandleReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_dragging) return;
        FinishDrag();
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void OnHandleCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_dragging) FinishDrag();
    }

    private void FinishDrag()
    {
        _dragging = false;
        PseudoClasses.Set(":dragging", false);
        UpdateMotion();
        var settled = Extent;
        if (Snap && SnapSizes.Count > 0)
        {
            var clamped = SnapSizes.Select(value => Math.Clamp(value, MinimumExtent, MaximumExtent)).Distinct().OrderBy(v => v).ToList();
            if (clamped.Count > 0)
            {
                // Upward flick (negative velocity): snap to next higher snap size
                if (_verticalVelocity < -0.35)
                {
                    settled = clamped.FirstOrDefault(v => v > Extent + 0.02, clamped.Last());
                }
                // Downward flick (positive velocity): snap to next lower snap size
                else if (_verticalVelocity > 0.35)
                {
                    settled = clamped.LastOrDefault(v => v < Extent - 0.02, clamped.First());
                }
                else
                {
                    settled = clamped.OrderBy(value => Math.Abs(value - Extent)).First();
                }
                SetExtent(settled);
            }
        }

        // A drag that settles at the minimum extent may be a dismissal in progress; hand the
        // decision to the host instead of closing on our own.
        if (ShouldCloseOnMinExtent && Math.Abs(settled - Math.Clamp(MinimumExtent, 0, 1)) < 0.001)
            MinExtentReached?.Invoke(this, EventArgs.Empty);
    }

    private void OnMotionSchemeChanged()
    {
        CancelAnimation();
        UpdateMotion();
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_surface is not null)
            _surface.Transitions = _dragging
                ? null
                : CreateHeightTransitions();
    }

    private Transitions? CreateHeightTransitions()
    {
        if (SnapAnimationDuration is { } custom)
        {
            // An explicit snap duration keeps the motion scheme's easing and enabled state but
            // replaces its timing, matching Flutter's snapAnimationDuration.
            var spec = MdMotion.Resolve(this, MdMotionKind.Spatial, MdMotionSpeed.Default);
            return MdMotionTransitions.Collect(new DoubleTransition
            {
                Property = HeightProperty,
                Duration = spec.IsEnabled ? custom : TimeSpan.Zero,
                Easing = spec.CreateEasing()
            });
        }
        return MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, HeightProperty, MdMotionKind.Spatial, MdMotionSpeed.Default));
    }

    private TimeSpan ResolveSnapDuration(TimeSpan? overrideDuration, MdMotionSpec spec)
    {
        if (overrideDuration is { } explicitDuration) return explicitDuration < TimeSpan.Zero ? TimeSpan.Zero : explicitDuration;
        if (SnapAnimationDuration is { } custom) return custom < TimeSpan.Zero ? TimeSpan.Zero : custom;
        return spec.IsEnabled ? spec.Duration : TimeSpan.Zero;
    }

    private void CancelAnimation()
    {
        ++_jumpVersion;
        var operation = _animationCancellation;
        _animationCancellation = null;
        operation?.Cancel();
        IsAnimating = false;
    }

    private void CoerceExtents()
    {
        var minimum = Math.Clamp(MinimumExtent, 0, 1);
        var maximum = Math.Clamp(MaximumExtent, minimum, 1);
        if (MinimumExtent != minimum) SetCurrentValue(MinimumExtentProperty, minimum);
        if (MaximumExtent != maximum) SetCurrentValue(MaximumExtentProperty, maximum);
        SetExtent(Extent);
    }

    private void OnExtentChanged()
    {
        var coerced = Math.Clamp(Extent, Math.Clamp(MinimumExtent, 0, 1), Math.Clamp(MaximumExtent, MinimumExtent, 1));
        if (!coerced.Equals(Extent)) { SetCurrentValue(ExtentProperty, coerced); return; }
        UpdateHeight();
        PseudoClasses.Set(":minimum", Math.Abs(Extent - MinimumExtent) < 0.001);
        PseudoClasses.Set(":maximum", Math.Abs(Extent - MaximumExtent) < 0.001);
        ExtentChanged?.Invoke(this, Extent);
    }

    private void UpdateHeight() => UpdateHeight(Bounds.Height > 0 ? Bounds.Height : 600);

    private void UpdateHeight(double available)
    {
        var height = Math.Max(0, available * Extent);
        SetAndRaise(SheetHeightProperty, ref _sheetHeight, height);
    }
}

/// <summary>
/// Imperative handle for a <see cref="MdDraggableScrollableSheet"/>, mirroring Flutter's
/// <c>DraggableScrollableController</c>. Set it as the sheet's <see cref="MdDraggableScrollableSheet.Controller"/>;
/// methods act only while a sheet is attached.
/// </summary>
public sealed class MdDraggableScrollableController
{
    private MdDraggableScrollableSheet? _sheet;

    public bool IsAttached => _sheet is not null;

    /// <summary>The attached sheet's current extent, or <c>0</c> when detached.</summary>
    public double CurrentExtent => _sheet?.Extent ?? 0d;

    /// <summary>Animates the attached sheet like <see cref="MdDraggableScrollableSheet.AnimateTo"/>.</summary>
    public void AnimateTo(double extent) => _sheet?.AnimateTo(extent);

    /// <summary>Animates the attached sheet with an explicit duration.</summary>
    public ValueTask AnimateToAsync(double extent, TimeSpan? duration = null, CancellationToken cancellationToken = default)
        => _sheet?.AnimateToAsync(extent, duration, cancellationToken) ?? ValueTask.CompletedTask;

    /// <summary>Moves the attached sheet without animation.</summary>
    public void JumpTo(double extent) => _sheet?.JumpTo(extent);

    /// <summary>Returns the attached sheet to its initial extent.</summary>
    public void Reset() => _sheet?.Reset();

    internal void Attach(MdDraggableScrollableSheet sheet) => _sheet = sheet;

    internal void Detach(MdDraggableScrollableSheet sheet)
    {
        if (ReferenceEquals(_sheet, sheet)) _sheet = null;
    }
}

public enum MdAdaptivePlatform { Automatic, Material, Cupertino }

/// <summary>A platform-adaptive switch wrapper. Material remains the fallback on unsupported platforms.</summary>
[PseudoClasses(":material", ":cupertino")]
public sealed class MdAdaptiveSwitch : MdSwitch
{
    public static readonly StyledProperty<MdAdaptivePlatform> PlatformProperty = AvaloniaProperty.Register<MdAdaptiveSwitch, MdAdaptivePlatform>(nameof(Platform));
    static MdAdaptiveSwitch() => PlatformProperty.Changed.AddClassHandler<MdAdaptiveSwitch>((control, _) => control.UpdatePlatform());
    public MdAdaptiveSwitch() => UpdatePlatform();
    public MdAdaptivePlatform Platform { get => GetValue(PlatformProperty); set => SetValue(PlatformProperty, value); }
    public bool UsesCupertinoStyle => Platform == MdAdaptivePlatform.Cupertino || Platform == MdAdaptivePlatform.Automatic && OperatingSystem.IsMacOS();
    private void UpdatePlatform() { PseudoClasses.Set(":cupertino", UsesCupertinoStyle); PseudoClasses.Set(":material", !UsesCupertinoStyle); }
}

/// <summary>A determinate/indeterminate adaptive circular progress wrapper.</summary>
[PseudoClasses(":material", ":cupertino", ":indeterminate")]
public sealed class MdAdaptiveProgressIndicator : TemplatedControl
{
    public static readonly StyledProperty<MdAdaptivePlatform> PlatformProperty = AvaloniaProperty.Register<MdAdaptiveProgressIndicator, MdAdaptivePlatform>(nameof(Platform));
    public static readonly StyledProperty<double> MinimumProperty = AvaloniaProperty.Register<MdAdaptiveProgressIndicator, double>(nameof(Minimum));
    public static readonly StyledProperty<double> MaximumProperty = AvaloniaProperty.Register<MdAdaptiveProgressIndicator, double>(nameof(Maximum), 100);
    public static readonly StyledProperty<double> ValueProperty = AvaloniaProperty.Register<MdAdaptiveProgressIndicator, double>(nameof(Value));
    public static readonly StyledProperty<bool> IsIndeterminateProperty = AvaloniaProperty.Register<MdAdaptiveProgressIndicator, bool>(nameof(IsIndeterminate), true);
    public static readonly StyledProperty<double> SizeProperty = AvaloniaProperty.Register<MdAdaptiveProgressIndicator, double>(nameof(Size), 36);
    static MdAdaptiveProgressIndicator()
    {
        PlatformProperty.Changed.AddClassHandler<MdAdaptiveProgressIndicator>((control, _) => control.UpdatePseudoClasses());
        IsIndeterminateProperty.Changed.AddClassHandler<MdAdaptiveProgressIndicator>((control, _) => control.UpdatePseudoClasses());
    }
    public MdAdaptiveProgressIndicator() => UpdatePseudoClasses();
    public MdAdaptivePlatform Platform { get => GetValue(PlatformProperty); set => SetValue(PlatformProperty, value); }
    public double Minimum { get => GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Maximum { get => GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    public double Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public bool IsIndeterminate { get => GetValue(IsIndeterminateProperty); set => SetValue(IsIndeterminateProperty, value); }
    public double Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public bool UsesCupertinoStyle => Platform == MdAdaptivePlatform.Cupertino || Platform == MdAdaptivePlatform.Automatic && OperatingSystem.IsMacOS();
    private void UpdatePseudoClasses() { PseudoClasses.Set(":cupertino", UsesCupertinoStyle); PseudoClasses.Set(":material", !UsesCupertinoStyle); PseudoClasses.Set(":indeterminate", IsIndeterminate); }
}

/// <summary>
/// Computes the bounds a hero flight occupies at a given <c>progress</c> in the 0..1 range.
/// </summary>
/// <remarks>
/// The Flutter equivalent is <c>Hero.createRectTween</c>; <see cref="MdRectTweens"/> holds the
/// built-in linear and Material-style arc factories.
/// </remarks>
public delegate Rect MdRectTween(Rect from, Rect to, double progress);

/// <summary>Built-in <see cref="MdRectTween"/> shapes for <see cref="MdHero.RectTween"/>.</summary>
public static class MdRectTweens
{
    /// <summary>Interpolates position and size linearly.</summary>
    public static MdRectTween Linear { get; } = (from, to, progress) =>
    {
        var t = Math.Clamp(progress, 0, 1);
        return new Rect(
            from.X + (to.X - from.X) * t,
            from.Y + (to.Y - from.Y) * t,
            from.Width + (to.Width - from.Width) * t,
            from.Height + (to.Height - from.Height) * t);
    };

    /// <summary>
    /// Material-style arc flight: the size interpolates linearly while the center rides a quadratic
    /// Bezier through the corner of the bounding box of the two centers, so diagonal flights lift
    /// off the straight line the way Material hero arcs do.
    /// </summary>
    public static MdRectTween Arc { get; } = (from, to, progress) =>
    {
        var t = Math.Clamp(progress, 0, 1);
        var start = new Point(from.X + from.Width / 2, from.Y + from.Height / 2);
        var end = new Point(to.X + to.Width / 2, to.Y + to.Height / 2);
        // The control point is the corner (end.X, start.Y) for horizontal-dominant flights and
        // (start.X, end.Y) otherwise; the curve bulges off the diagonal toward that corner.
        var control = Math.Abs(end.X - start.X) >= Math.Abs(end.Y - start.Y)
            ? new Point(end.X, start.Y)
            : new Point(start.X, end.Y);
        var inverse = 1 - t;
        var center = new Point(
            inverse * inverse * start.X + 2 * inverse * t * control.X + t * t * end.X,
            inverse * inverse * start.Y + 2 * inverse * t * control.Y + t * t * end.Y);
        var size = new Size(
            from.Width + (to.Width - from.Width) * t,
            from.Height + (to.Height - from.Height) * t);
        return new Rect(center.X - size.Width / 2, center.Y - size.Height / 2, size.Width, size.Height);
    };
}

/// <summary>
/// State handed to <see cref="MdHero.FlightShuttleBuilder"/> for a flight between two heroes.
/// </summary>
public sealed class MdHeroFlightShuttleContext : AvaloniaObject
{
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<MdHeroFlightShuttleContext, double>(nameof(Progress));

    private readonly MdRectTween _rectTween;

    internal MdHeroFlightShuttleContext(
        MdHero source, MdHero destination,
        Rect sourceBounds, Rect destinationBounds,
        MdRectTween rectTween)
    {
        Source = source;
        Destination = destination;
        SourceBounds = sourceBounds;
        DestinationBounds = destinationBounds;
        _rectTween = rectTween;
    }

    public MdHero Source { get; }
    public MdHero Destination { get; }

    /// <summary>Departure bounds in overlay coordinates.</summary>
    public Rect SourceBounds { get; }

    /// <summary>Arrival bounds in overlay coordinates.</summary>
    public Rect DestinationBounds { get; }

    /// <summary>Flight progress in the 0..1 range, driven by the hero during the flight.</summary>
    public double Progress { get => GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }

    /// <summary>The bounds the shuttle should occupy right now.</summary>
    public Rect CurrentBounds => _rectTween(SourceBounds, DestinationBounds, Progress);
}

/// <summary>
/// Shared-element transition that snapshots the source and destination into the top-level overlay,
/// interpolates their bounds with Material spatial motion, and falls back to a short cross-fade for
/// reduced motion. Flights sharing a tag cancel and replace one another deterministically.
/// </summary>
[PseudoClasses(":transitioning")]
public sealed class MdHero : ContentControl
{
    public static readonly StyledProperty<bool> IsTransitionEnabledProperty = AvaloniaProperty.Register<MdHero, bool>(nameof(IsTransitionEnabled), true);
    public static readonly StyledProperty<MdRectTween?> RectTweenProperty = AvaloniaProperty.Register<MdHero, MdRectTween?>(nameof(RectTween));
    public static readonly StyledProperty<Func<MdHeroFlightShuttleContext, Control?>?> FlightShuttleBuilderProperty = AvaloniaProperty.Register<MdHero, Func<MdHeroFlightShuttleContext, Control?>?>(nameof(FlightShuttleBuilder));
    public static readonly StyledProperty<Func<MdHero, Control?>?> PlaceholderBuilderProperty = AvaloniaProperty.Register<MdHero, Func<MdHero, Control?>?>(nameof(PlaceholderBuilder));
    public static readonly StyledProperty<bool> TransitionOnUserGesturesProperty = AvaloniaProperty.Register<MdHero, bool>(nameof(TransitionOnUserGestures));
    public static readonly DirectProperty<MdHero, bool> IsTransitioningProperty = AvaloniaProperty.RegisterDirect<MdHero, bool>(nameof(IsTransitioning), hero => hero.IsTransitioning);
    private static readonly Dictionary<object, MdHeroFlightRegistration> ActiveFlights = [];
    private MdHeroFlightRegistration? _flight;
    private bool _isTransitioning;

    static MdHero() =>
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdHero>((hero, _) => hero._flight?.Cancellation.Cancel());

    public bool IsTransitionEnabled { get => GetValue(IsTransitionEnabledProperty); set => SetValue(IsTransitionEnabledProperty, value); }

    /// <summary>
    /// Interpolator for the flight bounds; <c>null</c> selects <see cref="MdRectTweens.Linear"/>.
    /// </summary>
    public MdRectTween? RectTween { get => GetValue(RectTweenProperty); set => SetValue(RectTweenProperty, value); }

    /// <summary>
    /// Builds the visual shown during the flight instead of the default bitmap cross-fade. The
    /// builder is consulted on the source hero first; the returned control is reparented into the
    /// overlay for the flight and positioned at
    /// <see cref="MdHeroFlightShuttleContext.CurrentBounds"/> each frame.
    /// </summary>
    public Func<MdHeroFlightShuttleContext, Control?>? FlightShuttleBuilder
    {
        get => GetValue(FlightShuttleBuilderProperty);
        set => SetValue(FlightShuttleBuilderProperty, value);
    }

    /// <summary>
    /// Builds the stand-in shown at this hero's bounds while it flies. When unset (or returning
    /// <c>null</c>) the hero simply hides itself for the duration of the flight.
    /// </summary>
    public Func<MdHero, Control?>? PlaceholderBuilder
    {
        get => GetValue(PlaceholderBuilderProperty);
        set => SetValue(PlaceholderBuilderProperty, value);
    }

    /// <summary>
    /// Whether this hero may fly when a transition is gesture-driven (a back swipe, a drag
    /// dismissal). Both heroes must opt in; programmatic transitions are unaffected.
    /// </summary>
    public bool TransitionOnUserGestures
    {
        get => GetValue(TransitionOnUserGesturesProperty);
        set => SetValue(TransitionOnUserGesturesProperty, value);
    }

    public bool IsTransitioning
    {
        get => _isTransitioning;
        private set
        {
            if (_isTransitioning == value) return;
            SetAndRaise(IsTransitioningProperty, ref _isTransitioning, value);
            PseudoClasses.Set(":transitioning", value);
        }
    }

    public event EventHandler<MdHeroTransitionEventArgs>? TransitionRequested;
    public event EventHandler? TransitionCompleted;

    public void RequestTransitionTo(MdHero destination, bool isUserGesture = false) =>
        _ = TransitionToAsync(destination, isUserGesture);

    public ValueTask<bool> TransitionToAsync(MdHero destination, CancellationToken cancellationToken = default) =>
        TransitionToAsync(destination, isUserGesture: false, cancellationToken);

    public async ValueTask<bool> TransitionToAsync(
        MdHero destination, bool isUserGesture, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (!IsTransitionEnabled || !destination.IsTransitionEnabled || Tag is null || !Equals(Tag, destination.Tag)) return false;
        // Gesture-driven flights need both heroes to opt in; asking for one is not enough.
        if (isUserGesture && !(TransitionOnUserGestures && destination.TransitionOnUserGestures)) return false;
        var tag = Tag;
        if (!this.IsAttachedToVisualTree() || !destination.IsAttachedToVisualTree())
        {
            TransitionRequested?.Invoke(this, new MdHeroTransitionEventArgs(
                this, destination, new Rect(default(Point), Bounds.Size), new Rect(default(Point), destination.Bounds.Size), isUserGesture));
            return false;
        }

        // A hero can only participate in one flight. Cancel and fully clean up replaced flights
        // before taking snapshots so an older finally block cannot restore opacity over this flight.
        var replaced = new HashSet<MdHeroFlightRegistration>();
        if (_flight is not null) replaced.Add(_flight);
        if (destination._flight is not null) replaced.Add(destination._flight);
        if (ActiveFlights.TryGetValue(tag, out var taggedFlight)) replaced.Add(taggedFlight);
        foreach (var replacedFlight in replaced) replacedFlight.Cancellation.Cancel();
        if (replaced.Count > 0)
            await Task.WhenAll(replaced.Select(replacedFlight => replacedFlight.Completion.Task));
        if (cancellationToken.IsCancellationRequested) return false;

        if (!this.IsAttachedToVisualTree() || !destination.IsAttachedToVisualTree()) return false;
        var overlay = OverlayLayer.GetOverlayLayer(this);
        if (overlay is null || !ReferenceEquals(overlay, OverlayLayer.GetOverlayLayer(destination)))
        {
            TransitionRequested?.Invoke(this, new MdHeroTransitionEventArgs(
                this, destination, new Rect(default(Point), Bounds.Size), new Rect(default(Point), destination.Bounds.Size), isUserGesture));
            return false;
        }
        var sourceOrigin = this.TranslatePoint(default, overlay);
        var destinationOrigin = destination.TranslatePoint(default, overlay);
        if (sourceOrigin is null || destinationOrigin is null || Bounds.Width <= 0 || Bounds.Height <= 0 ||
            destination.Bounds.Width <= 0 || destination.Bounds.Height <= 0) return false;

        var sourceRect = new Rect(sourceOrigin.Value, Bounds.Size);
        var destinationRect = new Rect(destinationOrigin.Value, destination.Bounds.Size);
        TransitionRequested?.Invoke(this, new MdHeroTransitionEventArgs(this, destination, sourceRect, destinationRect, isUserGesture));
        var scheme = MdMotion.GetScheme(this);
        if (scheme == MdMotionScheme.None)
        {
            TransitionCompleted?.Invoke(this, EventArgs.Empty);
            return true;
        }

        // A shuttle builder replaces the default bitmap cross-fade entirely, so the captures are
        // only taken when the default presentation is actually needed.
        var shuttleBuilder = FlightShuttleBuilder ?? destination.FlightShuttleBuilder;
        var shuttleContext = shuttleBuilder is null
            ? null
            : new MdHeroFlightShuttleContext(this, destination, sourceRect, destinationRect,
                RectTween ?? destination.RectTween ?? MdRectTweens.Linear);
        var shuttle = shuttleBuilder?.Invoke(shuttleContext!);
        RenderTargetBitmap? sourceBitmap = null;
        RenderTargetBitmap? destinationBitmap = null;
        if (shuttle is null)
        {
            shuttleContext = null;
            sourceBitmap = MdVisualSnapshot.Capture(this);
            try
            {
                destinationBitmap = MdVisualSnapshot.Capture(destination);
            }
            catch
            {
                sourceBitmap.Dispose();
                throw;
            }
        }

        var operation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var registration = new MdHeroFlightRegistration(operation);
        _flight = registration;
        destination._flight = registration;
        ActiveFlights[tag] = registration;
        var spatial = MdMotion.Resolve(this, MdMotionKind.Spatial, MdMotionSpeed.Default);
        var effects = MdMotion.Resolve(this, MdMotionKind.Effects, MdMotionSpeed.Fast);
        var duration = spatial.IsEnabled ? spatial.Duration : effects.Duration;
        var flight = new MdHeroFlightPresenter(
            sourceBitmap, destinationBitmap, sourceRect, destinationRect,
            RectTween ?? destination.RectTween ?? MdRectTweens.Linear,
            spatial.IsEnabled, shuttle, shuttleContext)
        {
            Width = Math.Max(1, overlay.Bounds.Width),
            Height = Math.Max(1, overlay.Bounds.Height),
            IsHitTestVisible = false
        };
        Canvas.SetLeft(flight, 0);
        Canvas.SetTop(flight, 0);
        var transition = MdMotionTransitions.CreateDouble(this, MdHeroFlightPresenter.ProgressProperty,
            spatial.IsEnabled ? MdMotionKind.Spatial : MdMotionKind.Effects,
            spatial.IsEnabled ? MdMotionSpeed.Default : MdMotionSpeed.Fast);
        flight.Transitions = MdMotionTransitions.Collect(transition);
        var placeholders = new List<Control>();
        foreach (var hero in new[] { this, destination })
        {
            if (hero.PlaceholderBuilder?.Invoke(hero) is not { } placeholder) continue;
            var bounds = ReferenceEquals(hero, this) ? sourceRect : destinationRect;
            placeholder.Width = Math.Max(1, bounds.Width);
            placeholder.Height = Math.Max(1, bounds.Height);
            Canvas.SetLeft(placeholder, bounds.X);
            Canvas.SetTop(placeholder, bounds.Y);
            overlay.Children.Add(placeholder);
            placeholders.Add(placeholder);
        }
        var sourceOpacity = Opacity;
        var destinationOpacity = destination.Opacity;
        overlay.Children.Add(flight);
        if (shuttle is not null) overlay.Children.Add(shuttle);
        SetCurrentValue(OpacityProperty, 0);
        destination.SetCurrentValue(OpacityProperty, 0);
        IsTransitioning = true;
        destination.IsTransitioning = true;
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() => flight.Progress = 1, DispatcherPriority.Render);
            if (duration > TimeSpan.Zero) await Task.Delay(duration, operation.Token);
            operation.Token.ThrowIfCancellationRequested();
            TransitionCompleted?.Invoke(this, EventArgs.Empty);
            return true;
        }
        catch (OperationCanceledException) when (operation.IsCancellationRequested)
        {
            return false;
        }
        finally
        {
            overlay.Children.Remove(flight);
            if (shuttle is not null) overlay.Children.Remove(shuttle);
            foreach (var placeholder in placeholders) overlay.Children.Remove(placeholder);
            sourceBitmap?.Dispose();
            destinationBitmap?.Dispose();
            SetCurrentValue(OpacityProperty, sourceOpacity);
            destination.SetCurrentValue(OpacityProperty, destinationOpacity);
            IsTransitioning = false;
            destination.IsTransitioning = false;
            if (ActiveFlights.TryGetValue(tag, out var active) && ReferenceEquals(active, registration))
                ActiveFlights.Remove(tag);
            if (ReferenceEquals(_flight, registration)) _flight = null;
            if (ReferenceEquals(destination._flight, registration)) destination._flight = null;
            operation.Dispose();
            registration.Completion.TrySetResult(true);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _flight?.Cancellation.Cancel();
        base.OnDetachedFromVisualTree(e);
    }

    private sealed class MdHeroFlightRegistration(CancellationTokenSource cancellation)
    {
        public CancellationTokenSource Cancellation { get; } = cancellation;
        public TaskCompletionSource<bool> Completion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class MdHeroFlightPresenter : Control
    {
        public static readonly StyledProperty<double> ProgressProperty = AvaloniaProperty.Register<MdHeroFlightPresenter, double>(nameof(Progress));
        private readonly IImage? _source;
        private readonly IImage? _destination;
        private readonly Rect _sourceRect;
        private readonly Rect _destinationRect;
        private readonly bool _isSpatial;
        private readonly MdRectTween _rectTween;
        private readonly Control? _shuttle;
        private readonly MdHeroFlightShuttleContext? _shuttleContext;

        static MdHeroFlightPresenter() => AffectsRender<MdHeroFlightPresenter>(ProgressProperty);

        public MdHeroFlightPresenter(
            IImage? source,
            IImage? destination,
            Rect sourceRect,
            Rect destinationRect,
            MdRectTween rectTween,
            bool isSpatial,
            Control? shuttle = null,
            MdHeroFlightShuttleContext? shuttleContext = null)
        {
            _source = source;
            _destination = destination;
            _sourceRect = sourceRect;
            _destinationRect = destinationRect;
            _rectTween = rectTween;
            _isSpatial = isSpatial;
            _shuttle = shuttle;
            _shuttleContext = shuttleContext;
            UpdateShuttle();
        }

        public double Progress { get => GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ProgressProperty) UpdateShuttle();
        }

        private void UpdateShuttle()
        {
            var progress = Math.Clamp(Progress, 0, 1);
            if (_shuttleContext is not null) _shuttleContext.Progress = progress;
            if (_shuttle is null) return;
            var rect = CurrentBounds(progress);
            _shuttle.Width = Math.Max(1, rect.Width);
            _shuttle.Height = Math.Max(1, rect.Height);
            Canvas.SetLeft(_shuttle, rect.X);
            Canvas.SetTop(_shuttle, rect.Y);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            // A shuttle replaces the bitmap cross-fade entirely.
            if (_source is null || _destination is null) return;
            var progress = Math.Clamp(Progress, 0, 1);
            var rect = CurrentBounds(progress);
            using (context.PushOpacity(1 - progress))
                context.DrawImage(_source, new Rect(_source.Size), rect);
            using (context.PushOpacity(progress))
                context.DrawImage(_destination, new Rect(_destination.Size), rect);
        }

        private Rect CurrentBounds(double progress) => _isSpatial
            ? _rectTween(_sourceRect, _destinationRect, progress)
            : _destinationRect;
    }
}

public sealed class MdHeroTransitionEventArgs(
    MdHero source,
    MdHero destination,
    Rect sourceBounds,
    Rect destinationBounds,
    bool isUserGesture = false) : EventArgs
{
    public MdHero Source { get; } = source;
    public MdHero Destination { get; } = destination;
    public Rect SourceBounds { get; } = sourceBounds;
    public Rect DestinationBounds { get; } = destinationBounds;

    /// <summary>True when the surrounding transition is gesture-driven (back swipe, drag).</summary>
    public bool IsUserGesture { get; } = isUserGesture;
}

public enum MdFocusTraversalPolicy { VisualOrder, TabIndex, ReadingOrder }

/// <summary>An Avalonia-native focus traversal boundary with cycle, skip, tab-index, visual-tree, and geometric reading-order policies.</summary>
[PseudoClasses(":cycle", ":skip-traversal", ":visual-order", ":tab-index", ":reading-order")]
public sealed class MdFocusTraversalGroup : ContentControl
{
    public static readonly StyledProperty<MdFocusTraversalPolicy> PolicyProperty = AvaloniaProperty.Register<MdFocusTraversalGroup, MdFocusTraversalPolicy>(nameof(Policy), MdFocusTraversalPolicy.ReadingOrder);
    public static readonly StyledProperty<bool> CycleProperty = AvaloniaProperty.Register<MdFocusTraversalGroup, bool>(nameof(Cycle));
    public static readonly StyledProperty<bool> SkipTraversalProperty = AvaloniaProperty.Register<MdFocusTraversalGroup, bool>(nameof(SkipTraversal));
    static MdFocusTraversalGroup()
    {
        PolicyProperty.Changed.AddClassHandler<MdFocusTraversalGroup>((control, _) => control.UpdateTraversalState());
        CycleProperty.Changed.AddClassHandler<MdFocusTraversalGroup>((control, _) => control.UpdateTraversalState());
        SkipTraversalProperty.Changed.AddClassHandler<MdFocusTraversalGroup>((control, _) => control.UpdateTraversalState());
    }
    public MdFocusTraversalGroup()
    {
        AddHandler(InputElement.KeyDownEvent, OnTraversalKeyDown,
            RoutingStrategies.Tunnel, handledEventsToo: false);
        UpdateTraversalState();
    }
    public MdFocusTraversalPolicy Policy { get => GetValue(PolicyProperty); set => SetValue(PolicyProperty, value); }
    public bool Cycle { get => GetValue(CycleProperty); set => SetValue(CycleProperty, value); }
    public bool SkipTraversal { get => GetValue(SkipTraversalProperty); set => SetValue(SkipTraversalProperty, value); }
    private void UpdateTraversalState()
    {
        PseudoClasses.Set(":cycle", Cycle && !SkipTraversal);
        PseudoClasses.Set(":skip-traversal", SkipTraversal);
        PseudoClasses.Set(":visual-order", Policy == MdFocusTraversalPolicy.VisualOrder);
        PseudoClasses.Set(":tab-index", Policy == MdFocusTraversalPolicy.TabIndex);
        PseudoClasses.Set(":reading-order", Policy == MdFocusTraversalPolicy.ReadingOrder);
        KeyboardNavigation.SetTabNavigation(this, SkipTraversal
            ? KeyboardNavigationMode.None
            : Cycle ? KeyboardNavigationMode.Cycle : KeyboardNavigationMode.Continue);
    }

    private void OnTraversalKeyDown(object? sender, KeyEventArgs e)
    {
        if (SkipTraversal || e.Key != Key.Tab) return;
        var controls = GetTraversalOrder();
        if (controls.Count == 0) return;
        var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Control;
        var index = focused is null ? -1 : controls.FindIndex(control =>
            ReferenceEquals(control, focused) || focused.GetVisualAncestors().Contains(control));
        var backwards = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var targetIndex = index < 0 ? (backwards ? controls.Count - 1 : 0) : index + (backwards ? -1 : 1);
        if (targetIndex < 0 || targetIndex >= controls.Count)
        {
            if (!Cycle) return;
            targetIndex = targetIndex < 0 ? controls.Count - 1 : 0;
        }
        e.Handled = controls[targetIndex].Focus(NavigationMethod.Tab);
    }

    private List<Control> GetTraversalOrder()
    {
        var controls = this.GetVisualDescendants().OfType<Control>()
            .Where(control => control.Focusable && control.IsVisible && control.IsEffectivelyEnabled &&
                              KeyboardNavigation.GetIsTabStop(control))
            .ToList();
        return Policy switch
        {
            MdFocusTraversalPolicy.TabIndex => controls
                .OrderBy(KeyboardNavigation.GetTabIndex)
                .ThenBy(control => controls.IndexOf(control))
                .ToList(),
            MdFocusTraversalPolicy.ReadingOrder => controls
                .OrderBy(control => Math.Round((control.TranslatePoint(default, this)?.Y ?? 0) / 4))
                .ThenBy(control => (FlowDirection == FlowDirection.RightToLeft ? -1 : 1) *
                                   (control.TranslatePoint(default, this)?.X ?? 0))
                .ToList(),
            _ => controls
        };
    }
}

/// <summary>A local shortcut router with visible-content composition and CommunityToolkit-compatible ICommand values.</summary>
public sealed class MdShortcutScope : ContentControl
{
    private readonly Dictionary<KeyGesture, (ICommand Command, object? Parameter)> _shortcuts = [];
    public IReadOnlyDictionary<KeyGesture, (ICommand Command, object? Parameter)> Shortcuts => _shortcuts;
    public event EventHandler<KeyGesture>? ShortcutInvoked;

    public void Register(KeyGesture gesture, ICommand command, object? parameter = null) => _shortcuts[gesture] = (command, parameter);
    public bool Unregister(KeyGesture gesture) => _shortcuts.Remove(gesture);
    public void ClearShortcuts() => _shortcuts.Clear();

    protected override void OnKeyDown(KeyEventArgs e)
    {
        foreach (var pair in _shortcuts)
        {
            if (!pair.Key.Matches(e)) continue;
            if (pair.Value.Command.CanExecute(pair.Value.Parameter)) pair.Value.Command.Execute(pair.Value.Parameter);
            ShortcutInvoked?.Invoke(this, pair.Key);
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }
}

/// <summary>
/// A host that monitors soft keyboard insets and automatically adjusts its bottom viewport
/// padding and scrolls focused text controls into view.
/// </summary>
[PseudoClasses(":keyboard-active")]
public sealed class MdKeyboardAvoidingHost : ContentControl
{
    public static readonly StyledProperty<bool> AutoScrollToFocusedProperty =
        AvaloniaProperty.Register<MdKeyboardAvoidingHost, bool>(nameof(AutoScrollToFocused), true);

    public static readonly StyledProperty<double> KeyboardHeightProperty =
        AvaloniaProperty.Register<MdKeyboardAvoidingHost, double>(nameof(KeyboardHeight), 0.0);

    public static readonly StyledProperty<double> ExtraBottomOffsetProperty =
        AvaloniaProperty.Register<MdKeyboardAvoidingHost, double>(nameof(ExtraBottomOffset), 16.0);

    public static readonly StyledProperty<bool> AutomaticallyMonitorKeyboardProperty =
        AvaloniaProperty.Register<MdKeyboardAvoidingHost, bool>(nameof(AutomaticallyMonitorKeyboard), true);

    public static readonly DirectProperty<MdKeyboardAvoidingHost, bool> IsKeyboardActiveProperty =
        AvaloniaProperty.RegisterDirect<MdKeyboardAvoidingHost, bool>(nameof(IsKeyboardActive), host => host.IsKeyboardActive);

    private bool _isKeyboardActive;
    private bool _updatingPadding;
    private Thickness _restingPadding;
    private IInputPane? _inputPane;

    static MdKeyboardAvoidingHost()
    {
        KeyboardHeightProperty.Changed.AddClassHandler<MdKeyboardAvoidingHost>((host, _) => host.OnKeyboardHeightChanged());
        AutomaticallyMonitorKeyboardProperty.Changed.AddClassHandler<MdKeyboardAvoidingHost>((host, _) => host.SubscribeToInputPane());
        PaddingProperty.Changed.AddClassHandler<MdKeyboardAvoidingHost>((host, _) => host.OnPaddingChanged());
    }

    public MdKeyboardAvoidingHost()
    {
        _restingPadding = Padding;
        AddHandler(GotFocusEvent, OnChildGotFocus, RoutingStrategies.Bubble);
    }

    public bool AutoScrollToFocused
    {
        get => GetValue(AutoScrollToFocusedProperty);
        set => SetValue(AutoScrollToFocusedProperty, value);
    }

    public double KeyboardHeight
    {
        get => GetValue(KeyboardHeightProperty);
        set => SetValue(KeyboardHeightProperty, value);
    }

    public double ExtraBottomOffset
    {
        get => GetValue(ExtraBottomOffsetProperty);
        set => SetValue(ExtraBottomOffsetProperty, value);
    }

    /// <summary>Whether the host subscribes to the current top-level platform input pane.</summary>
    public bool AutomaticallyMonitorKeyboard
    {
        get => GetValue(AutomaticallyMonitorKeyboardProperty);
        set => SetValue(AutomaticallyMonitorKeyboardProperty, value);
    }

    public bool IsKeyboardActive
    {
        get => _isKeyboardActive;
        private set => SetAndRaise(IsKeyboardActiveProperty, ref _isKeyboardActive, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (KeyboardHeight <= 0) _restingPadding = Padding;
        SubscribeToInputPane();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeFromInputPane();
        base.OnDetachedFromVisualTree(e);
    }

    private void SubscribeToInputPane()
    {
        UnsubscribeFromInputPane();
        if (!AutomaticallyMonitorKeyboard || !this.IsAttachedToVisualTree()) return;
        _inputPane = TopLevel.GetTopLevel(this)?.InputPane;
        if (_inputPane is null) return;
        _inputPane.StateChanged += OnInputPaneStateChanged;
        SetCurrentValue(KeyboardHeightProperty,
            _inputPane.State == InputPaneState.Open ? Math.Max(0, _inputPane.OccludedRect.Height) : 0);
    }

    private void UnsubscribeFromInputPane()
    {
        if (_inputPane is not null) _inputPane.StateChanged -= OnInputPaneStateChanged;
        _inputPane = null;
    }

    private void OnInputPaneStateChanged(object? sender, InputPaneStateEventArgs e) =>
        SetCurrentValue(KeyboardHeightProperty,
            e.NewState == InputPaneState.Open ? Math.Max(0, e.EndRect.Height) : 0);

    private void OnPaddingChanged()
    {
        if (_updatingPadding) return;
        _restingPadding = Padding;
        if (KeyboardHeight > 0) ApplyKeyboardPadding();
    }

    private void OnKeyboardHeightChanged()
    {
        var active = KeyboardHeight > 0;
        IsKeyboardActive = active;
        PseudoClasses.Set(":keyboard-active", active);
        ApplyKeyboardPadding();
        if (active && AutoScrollToFocused)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.FocusManager?.GetFocusedElement() is Control focused)
            {
                BringControlIntoView(focused);
            }
        }
    }

    private void ApplyKeyboardPadding()
    {
        _updatingPadding = true;
        SetCurrentValue(PaddingProperty, new Thickness(
            _restingPadding.Left,
            _restingPadding.Top,
            _restingPadding.Right,
            _restingPadding.Bottom + Math.Max(0, KeyboardHeight)));
        _updatingPadding = false;
    }

    private void OnChildGotFocus(object? sender, FocusChangedEventArgs e)
    {
        if (!AutoScrollToFocused || e.Source is not Control focused) return;
        BringControlIntoView(focused);
    }

    public void BringControlIntoView(Control target)
    {
        if (!target.IsAttachedToVisualTree()) return;
        var scrollViewer = target.FindAncestorOfType<ScrollViewer>();
        if (scrollViewer is null)
        {
            target.BringIntoView();
            return;
        }

        var origin = target.TranslatePoint(default, scrollViewer);
        if (origin is null)
        {
            target.BringIntoView();
            return;
        }

        var viewportAlreadyInset = scrollViewer.GetVisualAncestors().Contains(this);
        var visibleHeight = scrollViewer.Viewport.Height -
            (viewportAlreadyInset ? 0 : Math.Max(0, KeyboardHeight));
        var targetBottom = origin.Value.Y + target.Bounds.Height + ExtraBottomOffset;
        if (targetBottom > visibleHeight)
        {
            var delta = targetBottom - visibleHeight;
            scrollViewer.Offset = new Vector(scrollViewer.Offset.X, scrollViewer.Offset.Y + delta);
        }
    }
}
