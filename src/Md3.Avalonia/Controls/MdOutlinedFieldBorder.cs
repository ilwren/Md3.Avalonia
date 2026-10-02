using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Media;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Draws an outlined-field border with a real transparent notch around a floating label. Unlike
/// painting a label background, the notch never assumes anything about the host surface color.
/// </summary>
public sealed class MdOutlinedFieldBorder : Control
{
    public static readonly StyledProperty<IBrush?> OutlineBrushProperty =
        AvaloniaProperty.Register<MdOutlinedFieldBorder, IBrush?>(nameof(OutlineBrush));

    public static readonly StyledProperty<Thickness> OutlineThicknessProperty =
        AvaloniaProperty.Register<MdOutlinedFieldBorder, Thickness>(nameof(OutlineThickness), new Thickness(1));

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<MdOutlinedFieldBorder, CornerRadius>(nameof(CornerRadius), new CornerRadius(4));

    public static readonly StyledProperty<bool> IsNotchedProperty =
        AvaloniaProperty.Register<MdOutlinedFieldBorder, bool>(nameof(IsNotched));

    public static readonly StyledProperty<Control?> NotchTargetProperty =
        AvaloniaProperty.Register<MdOutlinedFieldBorder, Control?>(nameof(NotchTarget));

    public static readonly StyledProperty<double> FallbackNotchStartProperty =
        AvaloniaProperty.Register<MdOutlinedFieldBorder, double>(nameof(FallbackNotchStart), 12);

    public static readonly StyledProperty<double> FallbackNotchWidthProperty =
        AvaloniaProperty.Register<MdOutlinedFieldBorder, double>(nameof(FallbackNotchWidth), 32);

    public static readonly StyledProperty<double> NotchPaddingProperty =
        AvaloniaProperty.Register<MdOutlinedFieldBorder, double>(nameof(NotchPadding), 4);

    private Control? _subscribedTarget;

    static MdOutlinedFieldBorder()
    {
        AffectsRender<MdOutlinedFieldBorder>(OutlineBrushProperty, OutlineThicknessProperty,
            CornerRadiusProperty, IsNotchedProperty, NotchTargetProperty,
            FallbackNotchStartProperty, FallbackNotchWidthProperty, NotchPaddingProperty);
        NotchTargetProperty.Changed.AddClassHandler<MdOutlinedFieldBorder>(
            (border, _) => border.SubscribeToNotchTarget());
    }

    public MdOutlinedFieldBorder()
    {
        IsHitTestVisible = false;
        AutomationProperties.SetAccessibilityView(this, AccessibilityView.Raw);
    }

    public IBrush? OutlineBrush
    {
        get => GetValue(OutlineBrushProperty);
        set => SetValue(OutlineBrushProperty, value);
    }

    public Thickness OutlineThickness
    {
        get => GetValue(OutlineThicknessProperty);
        set => SetValue(OutlineThicknessProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public bool IsNotched
    {
        get => GetValue(IsNotchedProperty);
        set => SetValue(IsNotchedProperty, value);
    }

    public Control? NotchTarget
    {
        get => GetValue(NotchTargetProperty);
        set => SetValue(NotchTargetProperty, value);
    }

    public double FallbackNotchStart
    {
        get => GetValue(FallbackNotchStartProperty);
        set => SetValue(FallbackNotchStartProperty, value);
    }

    public double FallbackNotchWidth
    {
        get => GetValue(FallbackNotchWidthProperty);
        set => SetValue(FallbackNotchWidthProperty, value);
    }

    public double NotchPadding
    {
        get => GetValue(NotchPaddingProperty);
        set => SetValue(NotchPaddingProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeToNotchTarget();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeFromNotchTarget();
        base.OnDetachedFromVisualTree(e);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var thickness = Math.Max(0, Math.Max(OutlineThickness.Left,
            Math.Max(OutlineThickness.Top, Math.Max(OutlineThickness.Right, OutlineThickness.Bottom))));
        if (OutlineBrush is null || thickness <= 0 || Bounds.Width <= thickness || Bounds.Height <= thickness)
            return;

        var inset = thickness / 2;
        var left = inset;
        var top = inset;
        var right = Bounds.Width - inset;
        var bottom = Bounds.Height - inset;
        var maxRadius = Math.Max(0, Math.Min((right - left) / 2, (bottom - top) / 2));
        var topLeft = Math.Min(maxRadius, Math.Max(0, CornerRadius.TopLeft));
        var topRight = Math.Min(maxRadius, Math.Max(0, CornerRadius.TopRight));
        var bottomRight = Math.Min(maxRadius, Math.Max(0, CornerRadius.BottomRight));
        var bottomLeft = Math.Min(maxRadius, Math.Max(0, CornerRadius.BottomLeft));

        var notchStart = right;
        var notchEnd = right;
        if (IsNotched)
        {
            (notchStart, notchEnd) = ResolveNotch(left, right);
            // A malformed/zero-width target must not erase a random part of the outline.
            if (notchEnd - notchStart < 1) notchStart = notchEnd = right;
        }

        using var geometry = new StreamGeometry();
        using (var path = geometry.Open())
        {
            var start = IsNotched && notchStart < notchEnd
                ? new Point(notchStart, top)
                : new Point(right - topRight, top);
            path.BeginFigure(start, isFilled: false);

            // Trace counter-clockwise from the notch's leading edge (or the top-right corner)
            // around all four rounded corners, ending at the notch's trailing edge.
            path.LineTo(new Point(left + topLeft, top));
            if (topLeft > 0)
                path.QuadraticBezierTo(new Point(left, top), new Point(left, top + topLeft));
            else
                path.LineTo(new Point(left, top));

            path.LineTo(new Point(left, bottom - bottomLeft));
            if (bottomLeft > 0)
                path.QuadraticBezierTo(new Point(left, bottom), new Point(left + bottomLeft, bottom));
            else
                path.LineTo(new Point(left, bottom));

            path.LineTo(new Point(right - bottomRight, bottom));
            if (bottomRight > 0)
                path.QuadraticBezierTo(new Point(right, bottom), new Point(right, bottom - bottomRight));
            else
                path.LineTo(new Point(right, bottom));

            path.LineTo(new Point(right, top + topRight));
            if (topRight > 0)
                path.QuadraticBezierTo(new Point(right, top), new Point(right - topRight, top));
            else
                path.LineTo(new Point(right, top));

            if (IsNotched && notchStart < notchEnd)
                path.LineTo(new Point(notchEnd, top));
            else
                path.EndFigure(isClosed: true);

            if (IsNotched && notchStart < notchEnd)
                path.EndFigure(isClosed: false);
        }

        context.DrawGeometry(null, new Pen(OutlineBrush, thickness), geometry);
    }

    private (double Start, double End) ResolveNotch(double left, double right)
    {
        var start = FallbackNotchStart;
        var end = start + Math.Max(0, FallbackNotchWidth);
        if (NotchTarget is { } target)
        {
            var targetStart = target.TranslatePoint(default, this);
            var targetEnd = target.TranslatePoint(new Point(target.Bounds.Width, 0), this);
            if (targetStart is { } first && targetEnd is { } second)
            {
                start = Math.Min(first.X, second.X);
                end = Math.Max(first.X, second.X);
            }
        }

        var padding = Math.Max(0, NotchPadding);
        start -= padding;
        end += padding;
        var minimum = left + Math.Max(0, CornerRadius.TopLeft);
        var maximum = right - Math.Max(0, CornerRadius.TopRight);
        start = Math.Clamp(start, minimum, maximum);
        end = Math.Clamp(end, start, maximum);
        return (start, end);
    }

    private void SubscribeToNotchTarget()
    {
        if (ReferenceEquals(_subscribedTarget, NotchTarget)) return;
        UnsubscribeFromNotchTarget();
        _subscribedTarget = NotchTarget;
        if (_subscribedTarget is not null)
            _subscribedTarget.LayoutUpdated += OnNotchTargetLayoutUpdated;
        InvalidateVisual();
    }

    private void UnsubscribeFromNotchTarget()
    {
        if (_subscribedTarget is not null)
            _subscribedTarget.LayoutUpdated -= OnNotchTargetLayoutUpdated;
        _subscribedTarget = null;
    }

    private void OnNotchTargetLayoutUpdated(object? sender, EventArgs e) => InvalidateVisual();
}
