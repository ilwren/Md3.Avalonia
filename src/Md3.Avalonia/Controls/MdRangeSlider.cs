using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A two-handle Material range slider with pointer, touch and keyboard input.</summary>
[PseudoClasses(":dragging-lower", ":dragging-upper", ":value-indicator", ":reduced-motion", ":no-motion")]
public sealed class MdRangeSlider : Control
{
    public static readonly StyledProperty<double> MinimumProperty = AvaloniaProperty.Register<MdRangeSlider, double>(nameof(Minimum));
    public static readonly StyledProperty<double> MaximumProperty = AvaloniaProperty.Register<MdRangeSlider, double>(nameof(Maximum), 100);
    public static readonly StyledProperty<double> LowerValueProperty = AvaloniaProperty.Register<MdRangeSlider, double>(nameof(LowerValue), 25, defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<double> UpperValueProperty = AvaloniaProperty.Register<MdRangeSlider, double>(nameof(UpperValue), 75, defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<double> StepProperty = AvaloniaProperty.Register<MdRangeSlider, double>(nameof(Step), 1);
    public static readonly StyledProperty<bool> ShowValueIndicatorsProperty = AvaloniaProperty.Register<MdRangeSlider, bool>(nameof(ShowValueIndicators));
    public static readonly StyledProperty<string> ValueFormatProperty = AvaloniaProperty.Register<MdRangeSlider, string>(nameof(ValueFormat), "0");
    public static readonly StyledProperty<IBrush?> ActiveTrackBrushProperty = AvaloniaProperty.Register<MdRangeSlider, IBrush?>(nameof(ActiveTrackBrush));
    public static readonly StyledProperty<IBrush?> InactiveTrackBrushProperty = AvaloniaProperty.Register<MdRangeSlider, IBrush?>(nameof(InactiveTrackBrush));
    public static readonly StyledProperty<IBrush?> HandleBrushProperty = AvaloniaProperty.Register<MdRangeSlider, IBrush?>(nameof(HandleBrush));
    public static readonly StyledProperty<IBrush?> ValueIndicatorBrushProperty = AvaloniaProperty.Register<MdRangeSlider, IBrush?>(nameof(ValueIndicatorBrush));
    public static readonly StyledProperty<IBrush?> ValueIndicatorForegroundProperty = AvaloniaProperty.Register<MdRangeSlider, IBrush?>(nameof(ValueIndicatorForeground));
    public static readonly StyledProperty<IBrush?> OverlayBrushProperty = AvaloniaProperty.Register<MdRangeSlider, IBrush?>(nameof(OverlayBrush));
    public static readonly StyledProperty<double> VisualLowerValueProperty = AvaloniaProperty.Register<MdRangeSlider, double>(nameof(VisualLowerValue), 25);
    public static readonly StyledProperty<double> VisualUpperValueProperty = AvaloniaProperty.Register<MdRangeSlider, double>(nameof(VisualUpperValue), 75);
    public static readonly StyledProperty<double> IndicatorOpacityProperty = AvaloniaProperty.Register<MdRangeSlider, double>(nameof(IndicatorOpacity));
    public static readonly StyledProperty<double> InteractionOpacityProperty = AvaloniaProperty.Register<MdRangeSlider, double>(nameof(InteractionOpacity));
    public static readonly StyledProperty<string> LowerThumbNameProperty = AvaloniaProperty.Register<MdRangeSlider, string>(nameof(LowerThumbName), "Lower value");
    public static readonly StyledProperty<string> UpperThumbNameProperty = AvaloniaProperty.Register<MdRangeSlider, string>(nameof(UpperThumbName), "Upper value");

    private bool _dragging;
    private bool _activeLower = true;

    static MdRangeSlider()
    {
        AffectsRender<MdRangeSlider>(MinimumProperty, MaximumProperty, LowerValueProperty, UpperValueProperty,
            ShowValueIndicatorsProperty, ValueFormatProperty, ActiveTrackBrushProperty, InactiveTrackBrushProperty,
            HandleBrushProperty, ValueIndicatorBrushProperty, ValueIndicatorForegroundProperty, OverlayBrushProperty,
            VisualLowerValueProperty, VisualUpperValueProperty, IndicatorOpacityProperty, InteractionOpacityProperty);
        MinimumProperty.Changed.AddClassHandler<MdRangeSlider>((slider, _) => slider.CoerceRange());
        MaximumProperty.Changed.AddClassHandler<MdRangeSlider>((slider, _) => slider.CoerceRange());
        LowerValueProperty.Changed.AddClassHandler<MdRangeSlider>((slider, _) => { slider.CoerceRange(); slider.UpdateVisualValues(); });
        UpperValueProperty.Changed.AddClassHandler<MdRangeSlider>((slider, _) => { slider.CoerceRange(); slider.UpdateVisualValues(); });
        ShowValueIndicatorsProperty.Changed.AddClassHandler<MdRangeSlider>((slider, _) => slider.UpdatePseudoClasses());
        FlowDirectionProperty.Changed.AddClassHandler<MdRangeSlider>((slider, _) => slider.InvalidateVisual());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdRangeSlider>((slider, _) => slider.UpdateMotion());
    }

    public MdRangeSlider()
    {
        Focusable = true;
        SetCurrentValue(VisualLowerValueProperty, LowerValue);
        SetCurrentValue(VisualUpperValueProperty, UpperValue);
        UpdatePseudoClasses();
        UpdateMotion();
    }

    public double Minimum { get => GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Maximum { get => GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    public double LowerValue { get => GetValue(LowerValueProperty); set => SetValue(LowerValueProperty, value); }
    public double UpperValue { get => GetValue(UpperValueProperty); set => SetValue(UpperValueProperty, value); }
    public double Step { get => GetValue(StepProperty); set => SetValue(StepProperty, value); }
    public bool ShowValueIndicators { get => GetValue(ShowValueIndicatorsProperty); set => SetValue(ShowValueIndicatorsProperty, value); }
    public string ValueFormat { get => GetValue(ValueFormatProperty); set => SetValue(ValueFormatProperty, value); }
    public IBrush? ActiveTrackBrush { get => GetValue(ActiveTrackBrushProperty); set => SetValue(ActiveTrackBrushProperty, value); }
    public IBrush? InactiveTrackBrush { get => GetValue(InactiveTrackBrushProperty); set => SetValue(InactiveTrackBrushProperty, value); }
    public IBrush? HandleBrush { get => GetValue(HandleBrushProperty); set => SetValue(HandleBrushProperty, value); }
    public IBrush? ValueIndicatorBrush { get => GetValue(ValueIndicatorBrushProperty); set => SetValue(ValueIndicatorBrushProperty, value); }
    public IBrush? ValueIndicatorForeground { get => GetValue(ValueIndicatorForegroundProperty); set => SetValue(ValueIndicatorForegroundProperty, value); }
    public IBrush? OverlayBrush { get => GetValue(OverlayBrushProperty); set => SetValue(OverlayBrushProperty, value); }
    public double VisualLowerValue => GetValue(VisualLowerValueProperty);
    public double VisualUpperValue => GetValue(VisualUpperValueProperty);
    public double IndicatorOpacity => GetValue(IndicatorOpacityProperty);
    public double InteractionOpacity => GetValue(InteractionOpacityProperty);
    public string LowerThumbName { get => GetValue(LowerThumbNameProperty); set => SetValue(LowerThumbNameProperty, value); }
    public string UpperThumbName { get => GetValue(UpperThumbNameProperty); set => SetValue(UpperThumbNameProperty, value); }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        const double edge = 24;
        var width = Math.Max(1, Bounds.Width - edge * 2);
        var y = ShowValueIndicators ? Bounds.Height - 24 : Bounds.Height / 2;
        var lowerX = edge + width * PositionForValue(VisualLowerValue);
        var upperX = edge + width * PositionForValue(VisualUpperValue);
        var activeStart = Math.Min(lowerX, upperX);
        var activeEnd = Math.Max(lowerX, upperX);

        DrawTrackSegment(context, InactiveTrackBrush, edge, activeStart - 2, y, leftRound: true, rightRound: false);
        DrawTrackSegment(context, ActiveTrackBrush, activeStart + 2, activeEnd - 2, y, leftRound: false, rightRound: false);
        DrawTrackSegment(context, InactiveTrackBrush, activeEnd + 2, edge + width, y, leftRound: false, rightRound: true);
        if (OverlayBrush is not null && InteractionOpacity > 0)
        {
            using (context.PushOpacity(InteractionOpacity))
                context.DrawEllipse(OverlayBrush, null, new Point(_activeLower ? lowerX : upperX, y), 20, 20);
        }
        context.DrawRectangle(HandleBrush, null, new RoundedRect(new Rect(lowerX - 2, y - 22, 4, 44), 2));
        context.DrawRectangle(HandleBrush, null, new RoundedRect(new Rect(upperX - 2, y - 22, 4, 44), 2));
        if (IsKeyboardFocusWithin && HandleBrush is not null)
            context.DrawEllipse(null, new Pen(HandleBrush, 2), new Point(_activeLower ? lowerX : upperX, y), 20, 20);

        if (IndicatorOpacity > 0)
        {
            using (context.PushOpacity(IndicatorOpacity))
            {
                if (_activeLower || !_dragging) DrawValueIndicator(context, lowerX, VisualLowerValue);
                if (!_activeLower || !_dragging) DrawValueIndicator(context, upperX, VisualUpperValue);
            }
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
        var value = ValueFromX(e.GetPosition(this).X);
        _activeLower = Math.Abs(value - LowerValue) <= Math.Abs(value - UpperValue);
        _dragging = true;
        Transitions = null;
        SetCurrentValue(InteractionOpacityProperty, 0.12);
        e.Pointer.Capture(this);
        SetNearestValue(value);
        UpdatePseudoClasses();
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_dragging) return;
        SetNearestValue(ValueFromX(e.GetPosition(this).X));
        e.Handled = true;
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (!_dragging) SetCurrentValue(InteractionOpacityProperty, 0.08);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (!_dragging) SetCurrentValue(InteractionOpacityProperty, 0);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_dragging) return;
        _dragging = false;
        e.Pointer.Capture(null);
        UpdateMotion();
        SetCurrentValue(InteractionOpacityProperty, IsPointerOver ? 0.08 : 0);
        UpdatePseudoClasses();
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (!_dragging) return;
        _dragging = false;
        UpdateMotion();
        SetCurrentValue(InteractionOpacityProperty, IsPointerOver ? 0.08 : 0);
        UpdatePseudoClasses();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift) && _activeLower)
            {
                ActivateThumb(lower: false);
                e.Handled = true;
                return;
            }
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift) && !_activeLower)
            {
                ActivateThumb(lower: true);
                e.Handled = true;
                return;
            }
        }

        var step = Math.Max(Step, double.Epsilon);
        var horizontalDirection = FlowDirection == FlowDirection.RightToLeft ? -1 : 1;
        var delta = e.Key switch
        {
            Key.Left => -step * horizontalDirection,
            Key.Right => step * horizontalDirection,
            Key.Down => -step,
            Key.Up => step,
            Key.PageDown => -step * 10,
            Key.PageUp => step * 10,
            _ => 0
        };
        if (delta != 0)
        {
            SetNearestValue((_activeLower ? LowerValue : UpperValue) + delta);
            e.Handled = true;
            return;
        }
        if (e.Key == Key.Home)
        {
            SetNearestValue(Minimum);
            e.Handled = true;
            return;
        }
        if (e.Key == Key.End)
        {
            SetNearestValue(Maximum);
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new MdRangeSliderAutomationPeer(this);

    internal void ActivateThumb(bool lower)
    {
        _activeLower = lower;
        Focus();
        UpdatePseudoClasses();
        InvalidateVisual();
    }

    internal void SetThumbValue(bool lower, double value)
    {
        _activeLower = lower;
        SetNearestValue(value);
        UpdatePseudoClasses();
    }

    internal double GetThumbCenterX(bool lower)
    {
        const double edge = 24;
        var width = Math.Max(1, Bounds.Width - edge * 2);
        return edge + width * PositionForValue(lower ? LowerValue : UpperValue);
    }

    internal double ThumbCenterY => ShowValueIndicators ? Bounds.Height - 24 : Bounds.Height / 2;

    internal bool IsThumbActive(bool lower) => _activeLower == lower;

    private void DrawValueIndicator(DrawingContext context, double centerX, double value)
    {
        if (ValueIndicatorBrush is null) return;
        var top = 0d;
        var geometry = new StreamGeometry();
        using (var stream = geometry.Open())
        {
            stream.BeginFigure(new Point(centerX, top), true);
            stream.CubicBezierTo(new Point(centerX - 13.255, top), new Point(centerX - 24, top + 10.745), new Point(centerX - 24, top + 24));
            stream.CubicBezierTo(new Point(centerX - 24, top + 35.5), new Point(centerX - 16.25, top + 44.9), new Point(centerX - 5.7, top + 47.4));
            stream.LineTo(new Point(centerX, top + 58));
            stream.LineTo(new Point(centerX + 5.7, top + 47.4));
            stream.CubicBezierTo(new Point(centerX + 16.25, top + 44.9), new Point(centerX + 24, top + 35.5), new Point(centerX + 24, top + 24));
            stream.CubicBezierTo(new Point(centerX + 24, top + 10.745), new Point(centerX + 13.255, top), new Point(centerX, top));
            stream.EndFigure(true);
        }
        context.DrawGeometry(ValueIndicatorBrush, null, geometry);
        if (ValueIndicatorForeground is null) return;
        var text = value.ToString(string.IsNullOrWhiteSpace(ValueFormat) ? "0" : ValueFormat,
            System.Globalization.CultureInfo.CurrentCulture);
        var layout = new TextLayout(text, new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Medium),
            13, ValueIndicatorForeground, TextAlignment.Center, maxWidth: 48);
        layout.Draw(context, new Point(centerX - 24, top + 13));
    }

    private static void DrawTrackSegment(DrawingContext context, IBrush? brush, double start, double end, double y, bool leftRound, bool rightRound)
    {
        if (brush is null || end <= start) return;
        const double radius = 8;
        var innerStart = leftRound ? Math.Min(end, start + radius) : start;
        var innerEnd = rightRound ? Math.Max(start, end - radius) : end;
        if (innerEnd > innerStart)
            context.DrawRectangle(brush, null, new Rect(innerStart, y - radius, innerEnd - innerStart, radius * 2));
        if (leftRound)
            context.DrawEllipse(brush, null, new Point(start + radius, y), radius, radius);
        if (rightRound)
            context.DrawEllipse(brush, null, new Point(end - radius, y), radius, radius);
    }

    private double Normalize(double value) => Maximum <= Minimum ? 0 : Math.Clamp((value - Minimum) / (Maximum - Minimum), 0, 1);

    private double PositionForValue(double value)
    {
        var position = Normalize(value);
        return FlowDirection == FlowDirection.RightToLeft ? 1 - position : position;
    }

    private double ValueFromX(double x)
    {
        var position = Math.Clamp((x - 24) / Math.Max(1, Bounds.Width - 48), 0, 1);
        if (FlowDirection == FlowDirection.RightToLeft) position = 1 - position;
        return Snap(Minimum + position * (Maximum - Minimum));
    }

    private double Snap(double value) => Step > 0 ? Math.Round(value / Step) * Step : value;

    private void SetNearestValue(double value)
    {
        if (_activeLower) SetCurrentValue(LowerValueProperty, Math.Clamp(value, Minimum, UpperValue));
        else SetCurrentValue(UpperValueProperty, Math.Clamp(value, LowerValue, Maximum));
    }

    private void UpdateVisualValues()
    {
        SetCurrentValue(VisualLowerValueProperty, LowerValue);
        SetCurrentValue(VisualUpperValueProperty, UpperValue);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        Transitions = _dragging
            ? null
            : MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, VisualLowerValueProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateDouble(this, VisualUpperValueProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateDouble(this, IndicatorOpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateDouble(this, InteractionOpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        if (scheme is MdMotionScheme.Reduced or MdMotionScheme.None) UpdateVisualValues();
    }

    private void CoerceRange()
    {
        if (Maximum < Minimum) SetCurrentValue(MaximumProperty, Minimum);
        var lower = Math.Clamp(LowerValue, Minimum, Maximum);
        var upper = Math.Clamp(UpperValue, lower, Maximum);
        if (!LowerValue.Equals(lower)) SetCurrentValue(LowerValueProperty, lower);
        if (!UpperValue.Equals(upper)) SetCurrentValue(UpperValueProperty, upper);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":dragging-lower", _dragging && _activeLower);
        PseudoClasses.Set(":dragging-upper", _dragging && !_activeLower);
        PseudoClasses.Set(":value-indicator", ShowValueIndicators);
        SetCurrentValue(IndicatorOpacityProperty, ShowValueIndicators ? 1 : 0);
        InvalidateVisual();
    }
}
