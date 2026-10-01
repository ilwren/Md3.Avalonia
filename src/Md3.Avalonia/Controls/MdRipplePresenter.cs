using System.Diagnostics;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Pointer-origin Material ripple with realistic press-and-release dynamics.
/// Observes the nearest interactive ancestor without participating in hit testing.
/// </summary>
public sealed class MdRipplePresenter : Control
{
    public static readonly StyledProperty<IBrush?> RippleBrushProperty =
        AvaloniaProperty.Register<MdRipplePresenter, IBrush?>(nameof(RippleBrush));

    public static readonly StyledProperty<double> MaxOpacityProperty =
        AvaloniaProperty.Register<MdRipplePresenter, double>(nameof(MaxOpacity), 0.16);

    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<MdRipplePresenter, TimeSpan>(nameof(Duration), TimeSpan.FromMilliseconds(450));

    public static readonly StyledProperty<bool> IsCenteredProperty =
        AvaloniaProperty.Register<MdRipplePresenter, bool>(nameof(IsCentered));

    private readonly DispatcherTimer _timer;
    private InputElement? _inputOwner;
    private long _startedAt;
    private long _releasedAt;
    private Point _origin;
    private double _maximumRadius;
    private bool _isAnimating;
    private bool _isPointerDown;

    static MdRipplePresenter() =>
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdRipplePresenter>((presenter, _) => presenter.OnMotionSchemeChanged());

    public MdRipplePresenter()
    {
        IsHitTestVisible = false;
        AutomationProperties.SetAccessibilityView(this, AccessibilityView.Raw);
        ClipToBounds = true;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += OnAnimationTick;
    }

    public IBrush? RippleBrush
    {
        get => GetValue(RippleBrushProperty);
        set => SetValue(RippleBrushProperty, value);
    }

    public double MaxOpacity
    {
        get => GetValue(MaxOpacityProperty);
        set => SetValue(MaxOpacityProperty, value);
    }

    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public bool IsCentered
    {
        get => GetValue(IsCenteredProperty);
        set => SetValue(IsCenteredProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _inputOwner = this.GetVisualAncestors()
            .OfType<InputElement>()
            .FirstOrDefault(IsInteractiveElement);
        if (_inputOwner is not null)
        {
            _inputOwner.AddHandler(PointerPressedEvent, OnOwnerPointerPressed,
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
            _inputOwner.AddHandler(PointerReleasedEvent, OnOwnerPointerReleased,
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
            _inputOwner.AddHandler(PointerCaptureLostEvent, OnOwnerPointerReleased,
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_inputOwner is not null)
        {
            _inputOwner.RemoveHandler(PointerPressedEvent, OnOwnerPointerPressed);
            _inputOwner.RemoveHandler(PointerReleasedEvent, OnOwnerPointerReleased);
            _inputOwner.RemoveHandler(PointerCaptureLostEvent, OnOwnerPointerReleased);
            _inputOwner = null;
        }

        _timer.Stop();
        _isAnimating = false;
        _isPointerDown = false;
        base.OnDetachedFromVisualTree(e);
    }

    private static bool IsInteractiveElement(InputElement element)
    {
        return element is Button
            or ListBoxItem
            or MenuItem
            or TreeViewItem
            or ToggleButton
            or RadioButton
            or CheckBox
            or MdCard
            or MdChip
            or MdListItem
            or MdTabItem
            or MdNavigationRailItem
            or MdNavigationBarItem
            or MdSegmentedButton
            or MdSwitch
            || element.Focusable;
    }

    private void OnOwnerPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_inputOwner is not null && !_inputOwner.IsEffectivelyEnabled) return;
        if (MdMotion.GetScheme(this) == MdMotionScheme.None) return;

        _isPointerDown = true;
        _origin = IsCentered
            ? new Point(Bounds.Width / 2, Bounds.Height / 2)
            : e.GetPosition(this);
        _maximumRadius = Math.Max(
            Distance(_origin, new Point(0, 0)),
            Math.Max(
                Distance(_origin, new Point(Bounds.Width, 0)),
                Math.Max(
                    Distance(_origin, new Point(0, Bounds.Height)),
                    Distance(_origin, new Point(Bounds.Width, Bounds.Height)))));
        _startedAt = Stopwatch.GetTimestamp();
        _releasedAt = 0;
        _isAnimating = true;
        _timer.Start();
        InvalidateVisual();
    }

    private void OnOwnerPointerReleased(object? sender, RoutedEventArgs e)
    {
        if (!_isPointerDown) return;
        _isPointerDown = false;
        _releasedAt = Stopwatch.GetTimestamp();
        InvalidateVisual();
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        if (!_isAnimating) return;

        var elapsed = Stopwatch.GetElapsedTime(_startedAt);
        var effectiveDuration = GetEffectiveDuration();

        if (!_isPointerDown)
        {
            var releaseElapsed = _releasedAt > 0 ? Stopwatch.GetElapsedTime(_releasedAt) : TimeSpan.Zero;
            if (elapsed >= effectiveDuration && releaseElapsed >= TimeSpan.FromMilliseconds(200))
            {
                _isAnimating = false;
                _timer.Stop();
            }
        }

        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (!_isAnimating || RippleBrush is null || Bounds.Width <= 0 || Bounds.Height <= 0) return;

        var scheme = MdMotion.GetScheme(this);
        if (scheme == MdMotionScheme.None) return;

        var duration = Math.Max(1, GetEffectiveDuration().TotalMilliseconds);
        var progress = Math.Clamp(Stopwatch.GetElapsedTime(_startedAt).TotalMilliseconds / duration, 0, 1);
        var spatial = scheme switch
        {
            MdMotionScheme.Standard => Math.Clamp(MdMotionTokens.StandardFastSpatial.Sample(progress * 0.36), 0, 1.02),
            MdMotionScheme.Reduced => 1,
            _ => Math.Clamp(MdMotionTokens.ExpressiveFastSpatial.Sample(progress * 0.42), 0, 1.08)
        };

        double fade;
        if (scheme == MdMotionScheme.Reduced)
        {
            fade = 1 - progress;
        }
        else if (_isPointerDown)
        {
            fade = 1.0;
        }
        else
        {
            var releaseMs = _releasedAt > 0 ? Stopwatch.GetElapsedTime(_releasedAt).TotalMilliseconds : 0;
            var releaseFade = Math.Clamp(1.0 - (releaseMs / 200.0), 0, 1.0);
            var naturalFade = progress < 0.5 ? 1.0 : 1.0 - (progress - 0.5) / 0.5;
            fade = Math.Min(releaseFade, naturalFade);
        }

        var opacity = Math.Clamp(MaxOpacity * fade, 0, 1);
        if (opacity <= 0.001) return;

        using (context.PushClip(new Rect(Bounds.Size)))
        using (context.PushOpacity(opacity))
        {
            var radius = _maximumRadius * spatial;
            context.DrawEllipse(RippleBrush, null, _origin, radius, radius);
        }
    }

    private TimeSpan GetEffectiveDuration()
    {
        if (MdMotion.GetScheme(this) == MdMotionScheme.Reduced)
            return MdMotion.Resolve(this, MdMotionKind.Effects, MdMotionSpeed.Fast).Duration;
        return Duration <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(1) : Duration;
    }

    private void OnMotionSchemeChanged()
    {
        if (!_isAnimating) return;
        if (MdMotion.GetScheme(this) == MdMotionScheme.None)
        {
            _isAnimating = false;
            _timer.Stop();
        }
        else if (MdMotion.GetScheme(this) == MdMotionScheme.Reduced)
        {
            _startedAt = Stopwatch.GetTimestamp();
        }
        InvalidateVisual();
    }

    private static double Distance(Point first, Point second)
    {
        var x = first.X - second.X;
        var y = first.Y - second.Y;
        return Math.Sqrt(x * x + y * y);
    }
}
