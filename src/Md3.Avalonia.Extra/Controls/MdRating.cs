using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Md3.Avalonia.Localization;

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
        IsReadOnlyProperty.Changed.AddClassHandler<MdRating>((rating, _) =>
        {
            rating.PseudoClasses.Set(":read-only", rating.IsReadOnly);
            rating.UpdateAutomation();
        });
        ValueProperty.Changed.AddClassHandler<MdRating>((rating, _) => rating.UpdateAutomation());
        FlowDirectionProperty.Changed.AddClassHandler<MdRating>((rating, _) => rating.InvalidateVisual());
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdRating>((rating, _) => rating.UpdateAutomation());
    }

    public MdRating()
    {
        Minimum = 0;
        Maximum = 5;
        SmallChange = 0.5;
        Focusable = true;
        UpdateAutomation();
    }

    public double ItemSize { get => GetValue(ItemSizeProperty); set => SetValue(ItemSizeProperty, value); }
    public double Spacing { get => GetValue(SpacingProperty); set => SetValue(SpacingProperty, value); }
    public double Precision { get => GetValue(PrecisionProperty); set => SetValue(PrecisionProperty, value); }
    public bool IsReadOnly { get => GetValue(IsReadOnlyProperty); set => SetValue(IsReadOnlyProperty, value); }
    public IBrush? ActiveBrush { get => GetValue(ActiveBrushProperty); set => SetValue(ActiveBrushProperty, value); }
    public IBrush? InactiveBrush { get => GetValue(InactiveBrushProperty); set => SetValue(InactiveBrushProperty, value); }

    protected override Size MeasureOverride(Size availableSize)
    {
        var metrics = GetMetrics(0);
        return new Size(metrics.Width, metrics.Target);
    }

    /// <summary>
    /// Resolves the row geometry. Rendering and hit testing both read it, so the star under the
    /// pointer is always the star the pointer selects.
    /// </summary>
    /// <param name="arrangedWidth">The width the control was arranged at, or 0 to ask for the natural width.</param>
    private RowMetrics GetMetrics(double arrangedWidth)
    {
        var count = Math.Max(1, (int)Math.Ceiling(Maximum - Minimum));
        var size = Math.Max(16, ItemSize);
        var target = Math.Max(48, size);
        var spacing = Math.Max(0, Spacing);
        var width = (count * target) + ((count - 1) * spacing);

        // A host that arranges the row narrower than its natural width (an explicit Width, a tight
        // grid cell) used to leave the trailing stars drawn past the clip while hit testing kept
        // mapping the pointer across the full natural width. The stars still on screen therefore
        // answered with a lower value than they depicted, so the rating could be dragged down but
        // never back up. Fold the row into the width actually granted -- spacing first, then the
        // cell -- so every value stays both visible and reachable.
        if (arrangedWidth > 0 && arrangedWidth < width)
        {
            spacing = count > 1 ? Math.Clamp((arrangedWidth - (count * target)) / (count - 1), 0, spacing) : 0;
            var cell = (arrangedWidth - ((count - 1) * spacing)) / count;
            if (cell < target)
            {
                target = Math.Max(1, cell);
                size = Math.Min(size, target);
            }

            width = (count * target) + ((count - 1) * spacing);
        }

        return new RowMetrics(count, size, target, spacing, target + spacing, width);
    }

    private readonly record struct RowMetrics(int Count, double Size, double Target, double Spacing, double Pitch, double Width);

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        // A transparent bounds primitive makes every 48-DIP cell and the spacing between cells
        // participate in hit testing; the visible star remains compact and does not define the
        // pointer target by its irregular outline.
        context.DrawRectangle(Brushes.Transparent, null, new Rect(Bounds.Size));
        var metrics = GetMetrics(Bounds.Width);
        var count = metrics.Count;
        var size = metrics.Size;
        var target = metrics.Target;
        var spacing = metrics.Spacing;
        var active = ActiveBrush ?? Brushes.Goldenrod;
        var inactive = InactiveBrush ?? Brushes.Gray;
        var rtl = FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft;
        for (var index = 0; index < count; index++)
        {
            var visualIndex = rtl ? count - index - 1 : index;
            var cellX = visualIndex * (target + spacing);
            var x = cellX + (target - size) / 2;
            var y = Math.Max(0, (Bounds.Height - size) / 2);
            var geometry = CreateStar(x, y, size);
            context.DrawGeometry(null, new Pen(inactive, 1.5), geometry);
            var fill = Math.Clamp(Value - Minimum - index, 0, 1);
            if (fill <= 0) continue;
            var clipX = rtl ? x + size * (1 - fill) : x;
            using (context.PushClip(new Rect(clipX, y, size * fill, size)))
                context.DrawGeometry(active, null, geometry);
        }

        if (IsKeyboardFocusWithin)
            context.DrawRectangle(null, new Pen(active, 2), new Rect(1, 1, Math.Max(0, Bounds.Width - 2), Math.Max(0, Bounds.Height - 2)), 8);
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
        var rtl = FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft;
        if (e.Key == Key.Up || e.Key == Key.Right && !rtl || e.Key == Key.Left && rtl)
        {
            SetCurrentValue(ValueProperty, Math.Min(Maximum, Value + step));
            e.Handled = true;
        }
        else if (e.Key == Key.Down || e.Key == Key.Left && !rtl || e.Key == Key.Right && rtl)
        {
            SetCurrentValue(ValueProperty, Math.Max(Minimum, Value - step));
            e.Handled = true;
        }
        else if (e.Key == Key.Home) { SetCurrentValue(ValueProperty, Minimum); e.Handled = true; }
        else if (e.Key == Key.End) { SetCurrentValue(ValueProperty, Maximum); e.Handled = true; }
        else base.OnKeyDown(e);
    }

    private void SetFromPosition(double x)
    {
        var metrics = GetMetrics(Bounds.Width);
        var count = metrics.Count;
        var target = metrics.Target;
        var pitch = metrics.Pitch;
        var renderedWidth = metrics.Width;
        var position = Math.Clamp(x, 0, renderedWidth);
        if (FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft) position = renderedWidth - position;
        var itemIndex = Math.Min(count - 1, Math.Max(0, (int)Math.Floor(position / pitch)));
        var withinItem = Math.Clamp(position - itemIndex * pitch, 0, target);
        var precision = Math.Clamp(Precision, 0.01, 1);

        // Each value owns a full 48-DIP target while the visual star can remain compact.
        var fraction = Math.Min(1, Math.Max(precision,
            Math.Ceiling((withinItem / target) / precision) * precision));
        var value = Minimum + itemIndex + fraction;
        SetCurrentValue(ValueProperty, Math.Clamp(value, Minimum, Maximum));
    }

    internal void SetAutomationValue(double value)
    {
        if (!IsReadOnly) SetCurrentValue(ValueProperty, Math.Clamp(value, Minimum, Maximum));
    }

    private void UpdateAutomation()
    {
        var culture = MdLocalization.ResolveCulture(this);
        AutomationProperties.SetName(this, MdLocalization.GetString("Rating", this));
        var readOnly = IsReadOnly ? $", {MdLocalization.GetString("ReadOnly", this)}" : string.Empty;
        AutomationProperties.SetHelpText(this, $"{Value.ToString("0.##", culture)} / {Maximum.ToString("0.##", culture)}{readOnly}");
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new MdRatingAutomationPeer(this);

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

internal sealed class MdRatingAutomationPeer(MdRating owner) : ControlAutomationPeer(owner), IRangeValueProvider
{
    private MdRating RatingOwner => (MdRating)Owner;

    public bool IsReadOnly => RatingOwner.IsReadOnly;
    public double Minimum => RatingOwner.Minimum;
    public double Maximum => RatingOwner.Maximum;
    public double Value => RatingOwner.Value;
    public double LargeChange => Math.Max(1, RatingOwner.LargeChange);
    public double SmallChange => Math.Max(0.01, RatingOwner.Precision);

    public void SetValue(double value)
    {
        EnsureEnabled();
        RatingOwner.SetAutomationValue(value);
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Slider;
    protected override bool IsControlElementCore() => true;
    protected override bool IsContentElementCore() => true;
}
