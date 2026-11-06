using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>The active marker geometry of <see cref="MdPageIndicator"/>.</summary>
public enum MdPageIndicatorEffect
{
    /// <summary>A circular marker slides between pages.</summary>
    Slide,
    /// <summary>A rounded marker stretches between adjacent pages and contracts again.</summary>
    Worm
}

/// <summary>A page indicator driven by an integer or fractional page position, with optional page requests.</summary>
/// <remarks>
/// The host owns paging and any animation of <see cref="PagePosition"/>. This control does not
/// attach to a carousel or start a timer. Pointer, keyboard and automation requests update the
/// two-way position and raise <see cref="PageRequested"/>; hosts may use the event to navigate.
/// </remarks>
[PseudoClasses(":non-interactive")]
public sealed class MdPageIndicator : Control
{
    public static readonly StyledProperty<int> PageCountProperty =
        AvaloniaProperty.Register<MdPageIndicator, int>(nameof(PageCount), coerce: static (_, value) => Math.Clamp(value, 0, 1000));
    public static readonly StyledProperty<double> PagePositionProperty =
        AvaloniaProperty.Register<MdPageIndicator, double>(nameof(PagePosition), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay,
            coerce: static (owner, value) => double.IsFinite(value) ? Math.Clamp(value, 0, Math.Max(0, ((MdPageIndicator)owner).PageCount - 1)) : 0);
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<MdPageIndicator, Orientation>(nameof(Orientation));
    public static readonly StyledProperty<MdPageIndicatorEffect> EffectProperty =
        AvaloniaProperty.Register<MdPageIndicator, MdPageIndicatorEffect>(nameof(Effect));
    public static readonly StyledProperty<bool> IsInteractiveProperty =
        AvaloniaProperty.Register<MdPageIndicator, bool>(nameof(IsInteractive), true);
    public static readonly StyledProperty<double> DotSizeProperty =
        AvaloniaProperty.Register<MdPageIndicator, double>(nameof(DotSize), 8, coerce: static (_, value) => double.IsFinite(value) ? Math.Clamp(value, 2, 40) : 8);
    public static readonly StyledProperty<double> DotSpacingProperty =
        AvaloniaProperty.Register<MdPageIndicator, double>(nameof(DotSpacing), 12, coerce: static (_, value) => double.IsFinite(value) ? Math.Clamp(value, 0, 100) : 12);
    public static readonly StyledProperty<IBrush?> ActiveBrushProperty =
        AvaloniaProperty.Register<MdPageIndicator, IBrush?>(nameof(ActiveBrush));
    public static readonly StyledProperty<IBrush?> InactiveBrushProperty =
        AvaloniaProperty.Register<MdPageIndicator, IBrush?>(nameof(InactiveBrush));

    static MdPageIndicator()
    {
        AffectsMeasure<MdPageIndicator>(PageCountProperty, OrientationProperty, DotSizeProperty, DotSpacingProperty);
        AffectsRender<MdPageIndicator>(PageCountProperty, PagePositionProperty, OrientationProperty, EffectProperty,
            DotSizeProperty, DotSpacingProperty, ActiveBrushProperty, InactiveBrushProperty, FlowDirectionProperty);
        PageCountProperty.Changed.AddClassHandler<MdPageIndicator>((control, _) => control.CoerceValue(PagePositionProperty));
        IsInteractiveProperty.Changed.AddClassHandler<MdPageIndicator>((control, _) => control.PseudoClasses.Set(":non-interactive", !control.IsInteractive));
    }

    public MdPageIndicator()
    {
        PseudoClasses.Set(":non-interactive", !IsInteractive);
        GotFocus += (_, _) => InvalidateVisual();
        LostFocus += (_, _) => InvalidateVisual();
        AutomationProperties.SetName(this, "Page indicator");
    }

    /// <summary>Number of pages, coerced to the range 0–1000.</summary>
    public int PageCount { get => GetValue(PageCountProperty); set => SetValue(PageCountProperty, value); }
    /// <summary>Zero-based page position, including fractions; clamped to the available pages.</summary>
    public double PagePosition { get => GetValue(PagePositionProperty); set => SetValue(PagePositionProperty, value); }
    /// <summary>Direction in which the markers are laid out.</summary>
    public Orientation Orientation { get => GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }
    /// <summary>Marker shape between pages.</summary>
    public MdPageIndicatorEffect Effect { get => GetValue(EffectProperty); set => SetValue(EffectProperty, value); }
    /// <summary>Enables pointer, keyboard and automation page requests.</summary>
    public bool IsInteractive { get => GetValue(IsInteractiveProperty); set => SetValue(IsInteractiveProperty, value); }
    /// <summary>Marker diameter in DIP, coerced to 2–40.</summary>
    public double DotSize { get => GetValue(DotSizeProperty); set => SetValue(DotSizeProperty, value); }
    /// <summary>Minimum space between markers in DIP, coerced to 0–100.</summary>
    public double DotSpacing { get => GetValue(DotSpacingProperty); set => SetValue(DotSpacingProperty, value); }
    /// <summary>Active marker brush, normally supplied by the Extra theme.</summary>
    public IBrush? ActiveBrush { get => GetValue(ActiveBrushProperty); set => SetValue(ActiveBrushProperty, value); }
    /// <summary>Inactive marker brush, normally supplied by the Extra theme.</summary>
    public IBrush? InactiveBrush { get => GetValue(InactiveBrushProperty); set => SetValue(InactiveBrushProperty, value); }

    /// <summary>Raised for user or automation navigation, not for external position changes.</summary>
    public event EventHandler<int>? PageRequested;

    protected override Size MeasureOverride(Size availableSize)
    {
        var length = PageCount == 0 ? 0 : Math.Max(48, PageCount * CellSize);
        return Orientation == Orientation.Horizontal ? new Size(length, 48) : new Size(48, length);
    }

    private double CellSize => Math.Max(32, DotSize + DotSpacing);

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.DrawRectangle(Brushes.Transparent, null, new Rect(Bounds.Size));
        if (PageCount == 0 || Bounds.Width <= 0 || Bounds.Height <= 0) return;
        var active = ActiveBrush ?? Brushes.Black;
        var inactive = InactiveBrush ?? Brushes.Gray;
        if (IsKeyboardFocusWithin && IsInteractive)
            context.DrawRectangle(null, new Pen(active, 2), new Rect(1, 1, Math.Max(0, Bounds.Width - 2), Math.Max(0, Bounds.Height - 2)), 8);
        var cell = ActualCellSize;
        var radius = Math.Min(DotSize, cell) / 2;
        for (var index = 0; index < PageCount; index++)
            context.DrawEllipse(inactive, null, Centre(index, cell), radius, radius);

        var position = PagePosition;
        if (Effect == MdPageIndicatorEffect.Slide || position == Math.Floor(position))
        {
            context.DrawEllipse(active, null, Centre(position, cell), radius, radius);
            return;
        }

        var first = Math.Floor(position);
        var fraction = position - first;
        var tail = Centre(first + Math.Max(0, fraction * 2 - 1), cell);
        var head = Centre(first + Math.Min(1, fraction * 2), cell);
        var rect = Orientation == Orientation.Horizontal
            ? new Rect(Math.Min(tail.X, head.X) - radius, tail.Y - radius, Math.Abs(head.X - tail.X) + radius * 2, radius * 2)
            : new Rect(tail.X - radius, Math.Min(tail.Y, head.Y) - radius, radius * 2, Math.Abs(head.Y - tail.Y) + radius * 2);
        context.DrawRectangle(active, null, new RoundedRect(rect, radius));
    }

    // Compress the hit cells when a host constrains the available size. The same geometry is
    // used for painting and pointer selection, including RTL, so the displayed dot remains clickable.
    private double ActualCellSize => Math.Min(CellSize,
        Math.Max(0, (Orientation == Orientation.Horizontal ? Bounds.Width : Bounds.Height) / Math.Max(1, PageCount)));

    private Point Centre(double index, double cell)
    {
        if (Orientation == Orientation.Horizontal && FlowDirection == FlowDirection.RightToLeft)
            index = PageCount - 1 - index;
        var length = Orientation == Orientation.Horizontal ? Bounds.Width : Bounds.Height;
        var along = (length - PageCount * cell) / 2 + (index + 0.5) * cell;
        return Orientation == Orientation.Horizontal ? new Point(along, Bounds.Height / 2) : new Point(Bounds.Width / 2, along);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!IsInteractive || PageCount == 0 || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || ActualCellSize <= 0) return;
        Focus();
        var point = e.GetPosition(this);
        var length = Orientation == Orientation.Horizontal ? Bounds.Width : Bounds.Height;
        var along = Orientation == Orientation.Horizontal ? point.X : point.Y;
        var index = Math.Clamp((int)Math.Floor((along - (length - PageCount * ActualCellSize) / 2) / ActualCellSize), 0, PageCount - 1);
        if (Orientation == Orientation.Horizontal && FlowDirection == FlowDirection.RightToLeft) index = PageCount - 1 - index;
        RequestPage(index);
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!IsInteractive || PageCount == 0) { base.OnKeyDown(e); return; }
        var current = (int)Math.Round(PagePosition, MidpointRounding.AwayFromZero);
        var forward = Orientation == Orientation.Horizontal
            ? FlowDirection == FlowDirection.RightToLeft ? Key.Left : Key.Right
            : Key.Down;
        var backward = Orientation == Orientation.Horizontal
            ? FlowDirection == FlowDirection.RightToLeft ? Key.Right : Key.Left
            : Key.Up;
        if (e.Key == forward) RequestPage(Math.Min(current + 1, PageCount - 1));
        else if (e.Key == backward) RequestPage(Math.Max(current - 1, 0));
        else if (e.Key == Key.Home) RequestPage(0);
        else if (e.Key == Key.End) RequestPage(PageCount - 1);
        else { base.OnKeyDown(e); return; }
        e.Handled = true;
    }

    internal void RequestPage(int index)
    {
        if (!IsInteractive || PageCount == 0) return;
        index = Math.Clamp(index, 0, PageCount - 1);
        SetCurrentValue(PagePositionProperty, index);
        PageRequested?.Invoke(this, index);
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new MdPageIndicatorAutomationPeer(this);
}

internal sealed class MdPageIndicatorAutomationPeer(MdPageIndicator owner) : ControlAutomationPeer(owner), IRangeValueProvider
{
    private MdPageIndicator Indicator => (MdPageIndicator)Owner;
    public bool IsReadOnly => !Indicator.IsInteractive || Indicator.PageCount == 0;
    public double Minimum => 0;
    public double Maximum => Math.Max(0, Indicator.PageCount - 1);
    public double Value => Indicator.PagePosition;
    public double LargeChange => 1;
    public double SmallChange => 1;
    public void SetValue(double value)
    {
        EnsureEnabled();
        if (!IsReadOnly && double.IsFinite(value)) Indicator.RequestPage((int)Math.Round(Math.Clamp(value, Minimum, Maximum), MidpointRounding.AwayFromZero));
    }
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Slider;
    protected override bool IsControlElementCore() => true;
    protected override bool IsContentElementCore() => true;
}
