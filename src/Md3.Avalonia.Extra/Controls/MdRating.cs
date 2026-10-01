using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>Accessible fractional rating input with pointer, keyboard, read-only, and indicator modes.</summary>
[PseudoClasses(":read-only")]
public sealed class MdRating : RangeBase
{
    public static readonly StyledProperty<double> ItemSizeProperty = AvaloniaProperty.Register<MdRating, double>(nameof(ItemSize), 32);
    public static readonly StyledProperty<double> SpacingProperty = AvaloniaProperty.Register<MdRating, double>(nameof(Spacing), 4);
    public static readonly StyledProperty<double> PrecisionProperty = AvaloniaProperty.Register<MdRating, double>(nameof(Precision), 0.5);
    public static readonly StyledProperty<bool> IsReadOnlyProperty = AvaloniaProperty.Register<MdRating, bool>(nameof(IsReadOnly));
    public static readonly StyledProperty<IBrush?> ActiveBrushProperty = AvaloniaProperty.Register<MdRating, IBrush?>(nameof(ActiveBrush));
    public static readonly StyledProperty<IBrush?> InactiveBrushProperty = AvaloniaProperty.Register<MdRating, IBrush?>(nameof(InactiveBrush));

    static MdRating()
    {
        AffectsRender<MdRating>(ValueProperty, MinimumProperty, MaximumProperty, ItemSizeProperty, SpacingProperty, ActiveBrushProperty, InactiveBrushProperty);
        AffectsMeasure<MdRating>(MinimumProperty, MaximumProperty, ItemSizeProperty, SpacingProperty);
        IsReadOnlyProperty.Changed.AddClassHandler<MdRating>((rating, _) => rating.PseudoClasses.Set(":read-only", rating.IsReadOnly));
    }

    public MdRating()
    {
        Minimum = 0;
        Maximum = 5;
        SmallChange = 0.5;
        Focusable = true;
    }

    public double ItemSize { get => GetValue(ItemSizeProperty); set => SetValue(ItemSizeProperty, value); }
    public double Spacing { get => GetValue(SpacingProperty); set => SetValue(SpacingProperty, value); }
    public double Precision { get => GetValue(PrecisionProperty); set => SetValue(PrecisionProperty, value); }
    public bool IsReadOnly { get => GetValue(IsReadOnlyProperty); set => SetValue(IsReadOnlyProperty, value); }
    public IBrush? ActiveBrush { get => GetValue(ActiveBrushProperty); set => SetValue(ActiveBrushProperty, value); }
    public IBrush? InactiveBrush { get => GetValue(InactiveBrushProperty); set => SetValue(InactiveBrushProperty, value); }

    protected override Size MeasureOverride(Size availableSize)
    {
        var count = Math.Max(1, (int)Math.Ceiling(Maximum - Minimum));
        return new Size(count * Math.Max(16, ItemSize) + (count - 1) * Math.Max(0, Spacing), Math.Max(16, ItemSize));
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var count = Math.Max(1, (int)Math.Ceiling(Maximum - Minimum));
        var size = Math.Max(16, ItemSize);
        var active = ActiveBrush ?? Brushes.Goldenrod;
        var inactive = InactiveBrush ?? Brushes.Gray;
        for (var index = 0; index < count; index++)
        {
            var x = index * (size + Math.Max(0, Spacing));
            var geometry = CreateStar(x, 0, size);
            context.DrawGeometry(null, new Pen(inactive, 1.5), geometry);
            var fill = Math.Clamp(Value - Minimum - index, 0, 1);
            if (fill <= 0) continue;
            using (context.PushClip(new Rect(x, 0, size * fill, size)))
                context.DrawGeometry(active, null, geometry);
        }
    }

    /// <summary>Sets the rating from a horizontal coordinate in control space.</summary>
    public void SetValueFromPosition(double x)
    {
        if (!IsReadOnly) SetFromPosition(x);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (IsReadOnly || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        SetFromPosition(e.GetPosition(this).X);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (IsReadOnly || !ReferenceEquals(e.Pointer.Captured, this)) return;
        SetFromPosition(e.GetPosition(this).X);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (ReferenceEquals(e.Pointer.Captured, this)) e.Pointer.Capture(null);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (IsReadOnly) { base.OnKeyDown(e); return; }
        var step = Math.Max(0.01, Precision);
        if (e.Key is Key.Right or Key.Up) { SetCurrentValue(ValueProperty, Math.Min(Maximum, Value + step)); e.Handled = true; }
        else if (e.Key is Key.Left or Key.Down) { SetCurrentValue(ValueProperty, Math.Max(Minimum, Value - step)); e.Handled = true; }
        else base.OnKeyDown(e);
    }

    private void SetFromPosition(double x)
    {
        var count = Math.Max(1, (int)Math.Ceiling(Maximum - Minimum));
        var size = Math.Max(16, ItemSize);
        var spacing = Math.Max(0, Spacing);
        var pitch = size + spacing;
        var renderedWidth = count * size + (count - 1) * spacing;
        var position = Math.Clamp(x, 0, renderedWidth);
        var itemIndex = Math.Min(count - 1, Math.Max(0, (int)Math.Floor(position / pitch)));
        var withinItem = Math.Clamp(position - itemIndex * pitch, 0, size);
        var precision = Math.Clamp(Precision, 0.01, 1);

        // Quantize inside the star, not across the control's total width. Spacing therefore never
        // shifts a star's hit range: with Precision=.5 its left/right halves map to half/full, and
        // with Precision=1 every point on star N maps exactly to N + 1.
        var fraction = Math.Min(1, Math.Max(precision,
            Math.Ceiling((withinItem / size) / precision) * precision));
        var value = Minimum + itemIndex + fraction;
        SetCurrentValue(ValueProperty, Math.Clamp(value, Minimum, Maximum));
    }

    private static StreamGeometry CreateStar(double x, double y, double size)
    {
        var geometry = new StreamGeometry();
        using var writer = geometry.Open();
        for (var point = 0; point < 10; point++)
        {
            var angle = -Math.PI / 2 + point * Math.PI / 5;
            var radius = point % 2 == 0 ? size * 0.46 : size * 0.21;
            var value = new Point(x + size / 2 + Math.Cos(angle) * radius, y + size / 2 + Math.Sin(angle) * radius);
            if (point == 0) writer.BeginFigure(value, true); else writer.LineTo(value);
        }
        writer.EndFigure(true);
        return geometry;
    }
}
