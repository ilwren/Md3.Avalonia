using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Md3.Avalonia.Extra.Infrastructure;

namespace Md3.Avalonia.Extra.Controls;

public enum MdTimelineItemState { Neutral, Active, Completed, Error }
public sealed record MdTimelineItem(object? Title, object? Description = null, DateTimeOffset? Time = null, MdTimelineItemState State = MdTimelineItemState.Neutral, object? Icon = null);

/// <summary>Accessible vertical/horizontal timeline; semantics remain ordered as supplied.</summary>
[PseudoClasses(":vertical", ":horizontal")]
public sealed class MdTimeline : ItemsControl
{
    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<MdTimeline, Orientation>(nameof(Orientation), Orientation.Vertical);
    public static readonly StyledProperty<int> ActiveIndexProperty = AvaloniaProperty.Register<MdTimeline, int>(nameof(ActiveIndex), -1);
    static MdTimeline()
    {
        OrientationProperty.Changed.AddClassHandler<MdTimeline>((control, _) => control.UpdateState());
        ActiveIndexProperty.Changed.AddClassHandler<MdTimeline>((control, _) => control.UpdateState());
    }
    public MdTimeline() => UpdateState();
    public Orientation Orientation { get => GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }
    public int ActiveIndex { get => GetValue(ActiveIndexProperty); set => SetValue(ActiveIndexProperty, value); }
    private void UpdateState()
    {
        PseudoClasses.Set(":vertical", Orientation == Orientation.Vertical);
        PseudoClasses.Set(":horizontal", Orientation == Orientation.Horizontal);
    }
}

/// <summary>Theme-facing timeline item that projects state, position, and orientation into stable pseudo classes.</summary>
[PseudoClasses(":vertical", ":horizontal", ":first", ":last", ":neutral", ":active", ":completed", ":error")]
public sealed class MdTimelineItemPresenter : ContentControl
{
    public static readonly StyledProperty<MdTimelineItem?> ItemProperty =
        AvaloniaProperty.Register<MdTimelineItemPresenter, MdTimelineItem?>(nameof(Item));
    public static readonly DirectProperty<MdTimelineItemPresenter, MdTimelineItemState> EffectiveStateProperty =
        AvaloniaProperty.RegisterDirect<MdTimelineItemPresenter, MdTimelineItemState>(nameof(EffectiveState), control => control.EffectiveState);

    private MdTimeline? _owner;
    private MdTimelineItemState _effectiveState;

    static MdTimelineItemPresenter() =>
        ItemProperty.Changed.AddClassHandler<MdTimelineItemPresenter>((control, _) => control.UpdateState());

    public MdTimelineItem? Item { get => GetValue(ItemProperty); set => SetValue(ItemProperty, value); }
    public MdTimelineItemState EffectiveState
    {
        get => _effectiveState;
        private set => SetAndRaise(EffectiveStateProperty, ref _effectiveState, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _owner = this.GetVisualAncestors().OfType<MdTimeline>().FirstOrDefault();
        if (_owner is not null) _owner.PropertyChanged += OnOwnerPropertyChanged;
        UpdateState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_owner is not null) _owner.PropertyChanged -= OnOwnerPropertyChanged;
        _owner = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnOwnerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == MdTimeline.ActiveIndexProperty || e.Property == MdTimeline.OrientationProperty ||
            e.Property == ItemsControl.ItemsSourceProperty)
            UpdateState();
    }

    private void UpdateState()
    {
        var values = _owner?.Items.Cast<object?>().ToArray() ?? Array.Empty<object?>();
        var index = Array.FindIndex(values, value => ReferenceEquals(value, Item));
        if (index < 0) index = Array.FindIndex(values, value => Equals(value, Item));
        var state = Item?.State ?? MdTimelineItemState.Neutral;
        if (state == MdTimelineItemState.Neutral && index >= 0 && index == _owner?.ActiveIndex)
            state = MdTimelineItemState.Active;
        EffectiveState = state;

        var horizontal = _owner?.Orientation == Orientation.Horizontal;
        PseudoClasses.Set(":horizontal", horizontal);
        PseudoClasses.Set(":vertical", !horizontal);
        PseudoClasses.Set(":first", index <= 0);
        PseudoClasses.Set(":last", index >= 0 && index == values.Length - 1);
        foreach (var value in Enum.GetValues<MdTimelineItemState>())
            PseudoClasses.Set($":{value.ToString().ToLowerInvariant()}", value == state);
        AutomationProperties.SetName(this, Item?.Title?.ToString());
    }
}

public enum MdChartKind { Line, Area, Bar }
public sealed record MdChartPoint(double X, double Y, object? Label = null, object? Value = null);
public sealed record MdChartSeries(string Name, IReadOnlyList<MdChartPoint> Points, IBrush? Brush = null);
public interface IMdChartDataProvider { IReadOnlyList<MdChartSeries> GetSeries(); }

/// <summary>Provider-neutral lightweight chart. It owns presentation, axes, pointer hit-testing and selection—not a vendor data engine.</summary>
public sealed class MdChart : Control
{
    public static readonly StyledProperty<IEnumerable<MdChartSeries>?> SeriesProperty = AvaloniaProperty.Register<MdChart, IEnumerable<MdChartSeries>?>(nameof(Series));
    public static readonly StyledProperty<MdChartKind> KindProperty = AvaloniaProperty.Register<MdChart, MdChartKind>(nameof(Kind));
    public static readonly StyledProperty<bool> ShowGridProperty = AvaloniaProperty.Register<MdChart, bool>(nameof(ShowGrid), true);
    public static readonly StyledProperty<IBrush?> AxisBrushProperty = AvaloniaProperty.Register<MdChart, IBrush?>(nameof(AxisBrush));
    public static readonly StyledProperty<IBrush?> GridBrushProperty = AvaloniaProperty.Register<MdChart, IBrush?>(nameof(GridBrush));
    public static readonly StyledProperty<string?> XAxisTitleProperty = AvaloniaProperty.Register<MdChart, string?>(nameof(XAxisTitle));
    public static readonly StyledProperty<string?> YAxisTitleProperty = AvaloniaProperty.Register<MdChart, string?>(nameof(YAxisTitle));
    public static readonly StyledProperty<string> AccessibleTitleProperty = AvaloniaProperty.Register<MdChart, string>(nameof(AccessibleTitle), "Chart");
    public static readonly DirectProperty<MdChart, MdChartPoint?> HoveredPointProperty = AvaloniaProperty.RegisterDirect<MdChart, MdChartPoint?>(nameof(HoveredPoint), control => control.HoveredPoint);
    public static readonly DirectProperty<MdChart, string?> PointToolTipTextProperty = AvaloniaProperty.RegisterDirect<MdChart, string?>(nameof(PointToolTipText), control => control.PointToolTipText);
    private MdChartPoint? _hoveredPoint;
    private string? _pointToolTipText;
    private int _activePointIndex = -1;
    private MdChartAutomationPeer? _automationPeer;
    static MdChart()
    {
        AffectsRender<MdChart>(SeriesProperty, KindProperty, ShowGridProperty, AxisBrushProperty, GridBrushProperty, XAxisTitleProperty, YAxisTitleProperty);
        SeriesProperty.Changed.AddClassHandler<MdChart>((chart, _) => chart.OnSeriesChanged());
        AccessibleTitleProperty.Changed.AddClassHandler<MdChart>((chart, _) => AutomationProperties.SetName(chart, chart.AccessibleTitle));
    }
    public MdChart()
    {
        ClipToBounds = true;
        Focusable = true;
        AutomationProperties.SetName(this, AccessibleTitle);
        AutomationProperties.SetLiveSetting(this, AutomationLiveSetting.Polite);
    }
    public IEnumerable<MdChartSeries>? Series { get => GetValue(SeriesProperty); set => SetValue(SeriesProperty, value); }
    public MdChartKind Kind { get => GetValue(KindProperty); set => SetValue(KindProperty, value); }
    public bool ShowGrid { get => GetValue(ShowGridProperty); set => SetValue(ShowGridProperty, value); }
    public IBrush? AxisBrush { get => GetValue(AxisBrushProperty); set => SetValue(AxisBrushProperty, value); }
    public IBrush? GridBrush { get => GetValue(GridBrushProperty); set => SetValue(GridBrushProperty, value); }
    public string? XAxisTitle { get => GetValue(XAxisTitleProperty); set => SetValue(XAxisTitleProperty, value); }
    public string? YAxisTitle { get => GetValue(YAxisTitleProperty); set => SetValue(YAxisTitleProperty, value); }
    public string AccessibleTitle { get => GetValue(AccessibleTitleProperty); set => SetValue(AccessibleTitleProperty, value); }
    public MdChartPoint? HoveredPoint { get => _hoveredPoint; private set => SetAndRaise(HoveredPointProperty, ref _hoveredPoint, value); }
    public string? PointToolTipText { get => _pointToolTipText; private set => SetAndRaise(PointToolTipTextProperty, ref _pointToolTipText, value); }
    public IMdChartDataProvider? Provider { get; set; }
    public event EventHandler<MdChartPoint?>? HoveredPointChanged;
    public event EventHandler<MdChartPoint>? PointInvoked;
    public void RefreshProvider() { if (Provider is not null) Series = Provider.GetSeries(); }
    /// <summary>Returns a tab-separated text alternative for screen readers, export, or a host-provided table view.</summary>
    public string BuildAccessibleTable()
    {
        var rows = new List<string> { "Series\tX\tY\tLabel\tValue" };
        foreach (var series in Series ?? Array.Empty<MdChartSeries>())
            rows.AddRange(series.Points.Select(point => $"{series.Name}\t{point.X}\t{point.Y}\t{point.Label}\t{point.Value}"));
        return string.Join(Environment.NewLine, rows);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var data = (Series ?? Array.Empty<MdChartSeries>()).Where(series => series.Points.Count > 0).ToArray();
        if (data.Length == 0 || Bounds.Width <= 120 || Bounds.Height <= 100) return;

        // Reserve explicit label gutters so ticks are data, not decoration painted over the plot.
        var plot = new Rect(58, 20, Math.Max(1, Bounds.Width - 76), Math.Max(1, Bounds.Height - 66));
        var points = data.SelectMany(series => series.Points).ToArray();
        var minX = points.Min(point => point.X);
        var maxX = points.Max(point => point.X);
        if (maxX <= minX) maxX = minX + 1;
        var minY = Math.Min(0, points.Min(point => point.Y));
        var maxY = Math.Max(0, points.Max(point => point.Y));
        if (maxY <= minY) maxY = minY + 1;
        var axis = AxisBrush ?? Brushes.Gray;
        var grid = GridBrush ?? new SolidColorBrush(Color.FromArgb(40, 128, 128, 128));
        var typeface = new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Medium);

        for (var index = 0; index <= 4; index++)
        {
            var y = plot.Top + plot.Height * index / 4;
            if (ShowGrid)
                context.DrawLine(new Pen(grid, 1), new Point(plot.Left, y), new Point(plot.Right, y));
            var tickValue = maxY - (maxY - minY) * index / 4;
            DrawChartText(context, tickValue.ToString("0.##", CultureInfo.CurrentCulture),
                new Point(plot.Left - 8, y), axis, typeface, 11, true, true);
        }

        context.DrawLine(new Pen(axis, 1), plot.BottomLeft, plot.TopLeft);
        context.DrawLine(new Pen(axis, 1), plot.BottomLeft, plot.BottomRight);
        if (!string.IsNullOrWhiteSpace(YAxisTitle))
            DrawChartText(context, YAxisTitle!, new Point(plot.Left, 4), axis, typeface, 11, false, false);
        if (!string.IsNullOrWhiteSpace(XAxisTitle))
            DrawChartText(context, XAxisTitle!, new Point(plot.Right, Bounds.Height - 4), axis, typeface, 11, true, true);

        Point Map(MdChartPoint point) => new(
            plot.Left + (point.X - minX) / (maxX - minX) * plot.Width,
            plot.Bottom - (point.Y - minY) / (maxY - minY) * plot.Height);

        var xValues = points.Select(point => point.X).Distinct().OrderBy(value => value).ToArray();
        var representative = xValues.ToDictionary(value => value,
            value => points.First(point => Math.Abs(point.X - value) < double.Epsilon));
        var xTickStep = Math.Max(1, (int)Math.Ceiling(xValues.Length / 7d));
        for (var index = 0; index < xValues.Length; index += xTickStep)
        {
            var value = xValues[index];
            var x = Kind == MdChartKind.Bar
                ? plot.Left + (index + .5) * plot.Width / xValues.Length
                : plot.Left + (value - minX) / (maxX - minX) * plot.Width;
            var point = representative[value];
            var label = point.Label?.ToString() ?? value.ToString("0.##", CultureInfo.CurrentCulture);
            context.DrawLine(new Pen(axis, 1), new Point(x, plot.Bottom), new Point(x, plot.Bottom + 4));
            DrawChartText(context, label, new Point(x, plot.Bottom + 8), axis, typeface, 11, true, false);
        }

        for (var seriesIndex = 0; seriesIndex < data.Length; seriesIndex++)
        {
            var series = data[seriesIndex];
            var brush = series.Brush ?? Palette[seriesIndex % Palette.Length];
            var pen = new Pen(brush, 2);
            if (Kind == MdChartKind.Bar)
            {
                // Categorical band centres reserve half a band at both plot edges. Grouped series
                // share the band, so the first and last bars can never protrude beyond the axes.
                var bandWidth = plot.Width / Math.Max(1, xValues.Length);
                var groupWidth = bandWidth * .72;
                var barWidth = Math.Max(2, groupWidth / data.Length);
                foreach (var point in series.Points)
                {
                    var categoryIndex = Array.IndexOf(xValues, point.X);
                    var bandCenter = plot.Left + (categoryIndex + .5) * bandWidth;
                    var groupLeft = bandCenter - groupWidth / 2;
                    var x = groupLeft + (seriesIndex + .5) * barWidth;
                    var y = plot.Bottom - (point.Y - minY) / (maxY - minY) * plot.Height;
                    var zeroY = plot.Bottom - (0 - minY) / (maxY - minY) * plot.Height;
                    context.FillRectangle(brush,
                        new Rect(x - barWidth * .42, Math.Min(y, zeroY), barWidth * .84, Math.Max(1, Math.Abs(zeroY - y))));
                }
            }
            else
            {
                var ordered = series.Points.OrderBy(point => point.X).Select(Map).ToArray();
                if (Kind == MdChartKind.Area && ordered.Length > 0)
                {
                    var zeroY = plot.Bottom - (0 - minY) / (maxY - minY) * plot.Height;
                    var area = new StreamGeometry();
                    using (var stream = area.Open())
                    {
                        stream.BeginFigure(new Point(ordered[0].X, zeroY), true);
                        foreach (var point in ordered) stream.LineTo(point);
                        stream.LineTo(new Point(ordered[^1].X, zeroY));
                        stream.EndFigure(true);
                    }
                    using (context.PushOpacity(.2)) context.DrawGeometry(brush, null, area);
                }
                Point? previous = null;
                foreach (var point in ordered)
                {
                    if (previous is { } start) context.DrawLine(pen, start, point);
                    context.DrawEllipse(brush, null, point, 3, 3);
                    previous = point;
                }
            }
        }

        if (HoveredPoint is { } activePoint)
        {
            var activePosition = Map(activePoint);
            context.DrawEllipse(null, new Pen(AxisBrush ?? Brushes.Black, IsKeyboardFocusWithin ? 3 : 2),
                activePosition, IsKeyboardFocusWithin ? 8 : 6, IsKeyboardFocusWithin ? 8 : 6);
        }
    }

    private static void DrawChartText(DrawingContext context, string text, Point anchor, IBrush brush,
        Typeface typeface, double fontSize, bool alignRightOrCenter, bool verticalCenter)
    {
        var formatted = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            typeface, fontSize, brush);
        var x = alignRightOrCenter ? anchor.X - formatted.Width : anchor.X;
        // X-axis labels pass verticalCenter=false and are centred around the tick separately below.
        if (!verticalCenter && anchor.Y > 20)
            x = anchor.X - formatted.Width / 2;
        var y = verticalCenter ? anchor.Y - formatted.Height / 2 : anchor.Y;
        context.DrawText(formatted, new Point(x, y));
    }
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var position = e.GetPosition(this);
        var all = GetPointEntries();
        if (all.Count == 0) return;
        // X-distance is deliberately used for stable touch/pointer exploration independent of Y scale.
        var normalized = Math.Clamp((position.X - 58) / Math.Max(1, Bounds.Width - 76), 0, 1);
        var min = all.Min(entry => entry.Point.X);
        var max = all.Max(entry => entry.Point.X);
        var next = all.MinBy(entry => Math.Abs(entry.Point.X - (min + normalized * (max - min))));
        SetActivePoint(next.Index, true);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (!IsKeyboardFocusWithin) SetActivePoint(-1, true);
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        if (_activePointIndex < 0 && GetPointEntries().Count > 0) SetActivePoint(0, true);
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        SetActivePoint(-1, true);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var points = GetPointEntries();
        if (points.Count == 0) { base.OnKeyDown(e); return; }
        var rtl = FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft;
        var delta = e.Key switch
        {
            Key.Right => rtl ? -1 : 1,
            Key.Left => rtl ? 1 : -1,
            Key.Down => 1,
            Key.Up => -1,
            _ => 0
        };
        if (delta != 0)
        {
            var current = _activePointIndex < 0 ? 0 : _activePointIndex;
            SetActivePoint((current + delta + points.Count) % points.Count, true);
            e.Handled = true;
        }
        else if (e.Key is Key.Home or Key.End)
        {
            SetActivePoint(e.Key == Key.Home ? 0 : points.Count - 1, true);
            e.Handled = true;
        }
        else if ((e.Key is Key.Enter or Key.Space) && _activePointIndex >= 0)
        {
            PointInvoked?.Invoke(this, points[_activePointIndex].Point);
            e.Handled = true;
        }
        else base.OnKeyDown(e);
    }

    private void OnSeriesChanged()
    {
        _activePointIndex = -1;
        SetActivePoint(-1, false);
        _automationPeer?.InvalidatePoints();
    }

    internal IReadOnlyList<MdChartPointEntry> GetPointEntries()
    {
        var result = new List<MdChartPointEntry>();
        var index = 0;
        foreach (var series in Series ?? Array.Empty<MdChartSeries>())
        foreach (var point in series.Points)
            result.Add(new MdChartPointEntry(index++, series.Name, point));
        return result;
    }

    internal void SetActivePoint(int index, bool raiseEvent)
    {
        var entries = GetPointEntries();
        _activePointIndex = entries.Count == 0 ? -1 : Math.Clamp(index, -1, entries.Count - 1);
        var entry = _activePointIndex >= 0 ? entries[_activePointIndex] : null;
        var point = entry?.Point;
        var changed = !Equals(point, HoveredPoint);
        HoveredPoint = point;
        PointToolTipText = entry is null ? null : DescribePoint(entry);
        AutomationProperties.SetHelpText(this, PointToolTipText ?? BuildAccessibleTable());
        InvalidateVisual();
        if (changed && raiseEvent) HoveredPointChanged?.Invoke(this, point);
    }

    internal void InvokePoint(int index)
    {
        var entries = GetPointEntries();
        if (index < 0 || index >= entries.Count) return;
        SetActivePoint(index, true);
        PointInvoked?.Invoke(this, entries[index].Point);
    }

    internal Rect GetPointBounds(int index)
    {
        var entries = GetPointEntries();
        if (index < 0 || index >= entries.Count || Bounds.Width <= 120 || Bounds.Height <= 100) return default;
        var points = entries.Select(entry => entry.Point).ToArray();
        var minX = points.Min(point => point.X);
        var maxX = Math.Max(minX + 1, points.Max(point => point.X));
        var minY = Math.Min(0, points.Min(point => point.Y));
        var maxY = Math.Max(minY + 1, Math.Max(0, points.Max(point => point.Y)));
        var plot = new Rect(58, 20, Math.Max(1, Bounds.Width - 76), Math.Max(1, Bounds.Height - 66));
        var point = entries[index].Point;
        var center = new Point(
            plot.Left + (point.X - minX) / (maxX - minX) * plot.Width,
            plot.Bottom - (point.Y - minY) / (maxY - minY) * plot.Height);
        return new Rect(center.X - 24, center.Y - 24, 48, 48);
    }

    internal string DescribePoint(MdChartPointEntry entry)
    {
        var label = entry.Point.Label?.ToString() ?? entry.Point.X.ToString("0.##", CultureInfo.CurrentCulture);
        var value = entry.Point.Value?.ToString() ?? entry.Point.Y.ToString("0.##", CultureInfo.CurrentCulture);
        return $"{entry.SeriesName}, {label}: {value}, point {entry.Index + 1} of {GetPointEntries().Count}";
    }

    protected override AutomationPeer OnCreateAutomationPeer() => _automationPeer = new MdChartAutomationPeer(this);

    private static readonly IBrush[] Palette = [Brushes.MediumPurple, Brushes.Teal, Brushes.OrangeRed, Brushes.RoyalBlue];
}

internal sealed record MdChartPointEntry(int Index, string SeriesName, MdChartPoint Point);

internal sealed class MdChartAutomationPeer(MdChart owner) : ControlAutomationPeer(owner)
{
    private IReadOnlyList<AutomationPeer>? _points;
    private MdChart ChartOwner => (MdChart)Owner;

    internal void InvalidatePoints()
    {
        _points = null;
        InvalidateChildren();
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.DataGrid;
    protected override IReadOnlyList<AutomationPeer>? GetChildrenCore() =>
        _points ??= ChartOwner.GetPointEntries()
            .Select(entry => (AutomationPeer)new MdChartPointAutomationPeer(ChartOwner, entry))
            .ToArray();
}

internal sealed class MdChartPointAutomationPeer(MdChart owner, MdChartPointEntry entry)
    : ControlAutomationPeer(owner), IInvokeProvider
{
    private MdChart ChartOwner => (MdChart)Owner;

    public void Invoke()
    {
        EnsureEnabled();
        ChartOwner.InvokePoint(entry.Index);
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.DataItem;
    protected override string? GetNameCore() => ChartOwner.DescribePoint(entry);
    protected override string? GetAutomationIdCore() => $"ChartPoint_{entry.Index}";
    protected override bool HasKeyboardFocusCore() => Equals(ChartOwner.HoveredPoint, entry.Point) && ChartOwner.IsKeyboardFocusWithin;
    protected override void SetFocusCore()
    {
        ChartOwner.Focus();
        ChartOwner.SetActivePoint(entry.Index, true);
    }
    protected override Rect GetBoundingRectangleCore()
    {
        if (TopLevel.GetTopLevel(ChartOwner) is not Visual root) return default;
        var transform = ChartOwner.TransformToVisual(root);
        return transform.HasValue ? ChartOwner.GetPointBounds(entry.Index).TransformToAABB(transform.Value) : default;
    }
}

public enum MdRichEditorCommand { Bold, Italic, Underline, StrikeThrough, Heading, Quote, Code, Link, BulletedList, NumberedList, Undo, Redo, HorizontalRule, ClearFormatting }

/// <summary>
/// Value converters mapping <see cref="MdRichEditorCommand"/> values to Material Symbols glyphs, labels, and tooltips.
/// </summary>
public static class MdRichEditorCommandConverters
{
    public static readonly IValueConverter GlyphConverter = new FuncValueConverter<MdRichEditorCommand, string>(GetGlyph);
    public static readonly IValueConverter TooltipConverter = new FuncValueConverter<MdRichEditorCommand, string>(GetTooltip);
    public static readonly IValueConverter LabelConverter = new FuncValueConverter<MdRichEditorCommand, string>(GetLabel);

    public static string GetGlyph(MdRichEditorCommand cmd) => cmd switch
    {
        MdRichEditorCommand.Undo => "\ue166",
        MdRichEditorCommand.Redo => "\ue15a",
        MdRichEditorCommand.Bold => "\ue238",
        MdRichEditorCommand.Italic => "\ue23f",
        MdRichEditorCommand.Underline => "\ue249",
        MdRichEditorCommand.StrikeThrough => "\ue246",
        MdRichEditorCommand.Heading => "\ue264",
        MdRichEditorCommand.Quote => "\ue244",
        MdRichEditorCommand.Code => "\ue86f",
        MdRichEditorCommand.Link => "\ue157",
        MdRichEditorCommand.BulletedList => "\ue241",
        MdRichEditorCommand.NumberedList => "\ue242",
        MdRichEditorCommand.HorizontalRule => "\uf108",
        MdRichEditorCommand.ClearFormatting => "\ue239",
        _ => "\ue8b8"
    };

    public static string GetTooltip(MdRichEditorCommand cmd) => cmd switch
    {
        MdRichEditorCommand.Undo => "Undo",
        MdRichEditorCommand.Redo => "Redo",
        MdRichEditorCommand.Bold => "Bold (**text**)",
        MdRichEditorCommand.Italic => "Italic (_text_)",
        MdRichEditorCommand.Underline => "Underline (<u>text</u>)",
        MdRichEditorCommand.StrikeThrough => "Strikethrough (~~text~~)",
        MdRichEditorCommand.Heading => "Heading (## text)",
        MdRichEditorCommand.Quote => "Quote (> text)",
        MdRichEditorCommand.Code => "Inline code (`code`)",
        MdRichEditorCommand.Link => "Insert link ([text](url))",
        MdRichEditorCommand.BulletedList => "Bulleted list (- item)",
        MdRichEditorCommand.NumberedList => "Numbered list (1. item)",
        MdRichEditorCommand.HorizontalRule => "Horizontal rule (---)",
        MdRichEditorCommand.ClearFormatting => "Clear formatting",
        _ => cmd.ToString()
    };

    public static string GetLabel(MdRichEditorCommand cmd) => cmd switch
    {
        MdRichEditorCommand.Undo => "Undo",
        MdRichEditorCommand.Redo => "Redo",
        MdRichEditorCommand.Bold => "B",
        MdRichEditorCommand.Italic => "I",
        MdRichEditorCommand.Underline => "U",
        MdRichEditorCommand.StrikeThrough => "S",
        MdRichEditorCommand.Heading => "H",
        MdRichEditorCommand.Quote => "\"",
        MdRichEditorCommand.Code => "</>",
        MdRichEditorCommand.Link => "Link",
        MdRichEditorCommand.BulletedList => "•",
        MdRichEditorCommand.NumberedList => "1.",
        MdRichEditorCommand.HorizontalRule => "---",
        MdRichEditorCommand.ClearFormatting => "Tx",
        _ => cmd.ToString()
    };
}

public interface IMdRichEditorAdapter
{
    object? Document { get; set; }
    bool CanExecute(MdRichEditorCommand command, object? parameter = null);
    void Execute(MdRichEditorCommand command, object? parameter = null);
    event EventHandler? StateChanged;
}

/// <summary>Optional formatting-state contract used by toggle-style rich-editor commands.</summary>
public interface IMdRichEditorStateAdapter
{
    bool IsCommandChecked(MdRichEditorCommand command);
}

public sealed record MdRichEditorCommandDescriptor(
    MdRichEditorCommand Command,
    string Name,
    string Glyph,
    string? Shortcut,
    bool IsToggle,
    bool IsChecked,
    bool IsEnabled)
{
    public string AccessibleDescription => string.IsNullOrEmpty(Shortcut) ? Name : $"{Name}, {Shortcut}";
}

/// <summary>
/// Built-in lightweight rich-text adapter for an Avalonia <see cref="TextBox"/>. Commands modify
/// the selected source using portable rich-text markers and immediately rebuild an optional
/// WYSIWYG preview panel. Applications can still replace it with a document-engine adapter.
/// </summary>
public sealed class MdTextBoxRichEditorAdapter : IMdRichEditorAdapter, IMdRichEditorStateAdapter
{
    private readonly TextBox _editor;
    private readonly Panel? _previewHost;
    public MdTextBoxRichEditorAdapter(TextBox editor, Panel? previewHost = null)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        _previewHost = previewHost;
        _editor.TextChanged += OnEditorTextChanged;
        RefreshPreview();
    }

    public MdRichEditorCommand? LastCommand { get; private set; }
    public object? Document
    {
        get => _editor.Text;
        set => _editor.Text = value?.ToString() ?? string.Empty;
    }
    public event EventHandler? StateChanged;

    public bool CanExecute(MdRichEditorCommand command, object? parameter = null) => command switch
    {
        MdRichEditorCommand.Undo => _editor.CanUndo,
        MdRichEditorCommand.Redo => _editor.CanRedo,
        _ => !_editor.IsReadOnly
    };

    public bool IsCommandChecked(MdRichEditorCommand command) => command switch
    {
        MdRichEditorCommand.Bold => HasSelectionWrapper("**", "**"),
        MdRichEditorCommand.Italic => HasSelectionWrapper("_", "_"),
        MdRichEditorCommand.Underline => HasSelectionWrapper("<u>", "</u>"),
        MdRichEditorCommand.StrikeThrough => HasSelectionWrapper("~~", "~~"),
        MdRichEditorCommand.Code => HasSelectionWrapper("`", "`"),
        MdRichEditorCommand.Heading => CurrentLineStartsWith("# ", "## ", "### "),
        MdRichEditorCommand.Quote => CurrentLineStartsWith("> "),
        MdRichEditorCommand.BulletedList => CurrentLineStartsWith("- ", "* "),
        MdRichEditorCommand.NumberedList => CurrentLineStartsWith("1. "),
        _ => false
    };

    public void Execute(MdRichEditorCommand command, object? parameter = null)
    {
        if (!CanExecute(command, parameter)) return;
        LastCommand = command;
        switch (command)
        {
            case MdRichEditorCommand.Undo: _editor.Undo(); break;
            case MdRichEditorCommand.Redo: _editor.Redo(); break;
            case MdRichEditorCommand.Bold: WrapSelection("**", "**", "bold text"); break;
            case MdRichEditorCommand.Italic: WrapSelection("_", "_", "italic text"); break;
            case MdRichEditorCommand.Underline: WrapSelection("<u>", "</u>", "underlined text"); break;
            case MdRichEditorCommand.StrikeThrough: WrapSelection("~~", "~~", "deleted text"); break;
            case MdRichEditorCommand.Code: WrapSelection("`", "`", "code"); break;
            case MdRichEditorCommand.Link: WrapSelection("[", $"]({parameter ?? "https://"})", "link text"); break;
            case MdRichEditorCommand.Heading: PrefixSelectedLines("## "); break;
            case MdRichEditorCommand.Quote: PrefixSelectedLines("> "); break;
            case MdRichEditorCommand.BulletedList: PrefixSelectedLines("- "); break;
            case MdRichEditorCommand.NumberedList: PrefixSelectedLines("1. "); break;
            case MdRichEditorCommand.HorizontalRule: InsertText("\n---\n"); break;
            case MdRichEditorCommand.ClearFormatting: StripFormattingFromSelection(); break;
        }
        RefreshPreview();
        StateChanged?.Invoke(this, EventArgs.Empty);
        _editor.Focus();
    }

    private void InsertText(string textToInsert)
    {
        var text = _editor.Text ?? string.Empty;
        var start = Math.Clamp(Math.Min(_editor.SelectionStart, _editor.SelectionEnd), 0, text.Length);
        var end = Math.Clamp(Math.Max(_editor.SelectionStart, _editor.SelectionEnd), start, text.Length);
        _editor.Text = text[..start] + textToInsert + text[end..];
        _editor.SelectionStart = start + textToInsert.Length;
        _editor.SelectionEnd = start + textToInsert.Length;
    }

    private void StripFormattingFromSelection()
    {
        var text = _editor.Text ?? string.Empty;
        var start = Math.Clamp(Math.Min(_editor.SelectionStart, _editor.SelectionEnd), 0, text.Length);
        var end = Math.Clamp(Math.Max(_editor.SelectionStart, _editor.SelectionEnd), start, text.Length);
        if (end <= start)
        {
            start = 0;
            end = text.Length;
        }
        var selected = text[start..end];
        var cleaned = selected
            .Replace("**", string.Empty)
            .Replace("<u>", string.Empty)
            .Replace("</u>", string.Empty)
            .Replace("~~", string.Empty)
            .Replace("`", string.Empty);
        var lines = cleaned.Replace("\r\n", "\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var l = lines[i];
            while (l.StartsWith('#')) l = l.TrimStart('#').TrimStart();
            if (l.StartsWith("> ", StringComparison.Ordinal)) l = l[2..];
            else if (l.StartsWith("- ", StringComparison.Ordinal) || l.StartsWith("* ", StringComparison.Ordinal)) l = l[2..];
            else if (char.IsDigit(l.FirstOrDefault()) && l.Length > 2 && l[1] == '.' && l[2] == ' ') l = l[3..];
            lines[i] = l;
        }
        cleaned = string.Join("\n", lines);
        _editor.Text = text[..start] + cleaned + text[end..];
        _editor.SelectionStart = start;
        _editor.SelectionEnd = start + cleaned.Length;
    }

    public void RefreshPreview()
    {
        if (_previewHost is null) return;
        _previewHost.Children.Clear();
        var lines = (_editor.Text ?? string.Empty).Replace("\r\n", "\n").Split('\n');
        var inCodeBlock = false;
        var codeBlockLines = new List<string>();

        foreach (var sourceLine in lines)
        {
            var line = sourceLine;
            if (line.StartsWith("```", StringComparison.Ordinal))
            {
                if (inCodeBlock)
                {
                    var codeText = string.Join("\n", codeBlockLines);
                    var codeBlock = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(32, 128, 128, 128)),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(12, 8),
                        Margin = new Thickness(0, 4, 0, 6),
                        Child = new TextBlock
                        {
                            Text = codeText,
                            FontFamily = new FontFamily("Cascadia Code,Consolas,Monospace"),
                            FontSize = 12,
                            TextWrapping = TextWrapping.Wrap
                        }
                    };
                    _previewHost.Children.Add(codeBlock);
                    codeBlockLines.Clear();
                    inCodeBlock = false;
                }
                else
                {
                    inCodeBlock = true;
                    codeBlockLines.Clear();
                }
                continue;
            }

            if (inCodeBlock)
            {
                codeBlockLines.Add(line);
                continue;
            }

            if (line.Trim() == "---" || line.Trim() == "***" || line.Trim() == "___")
            {
                var rule = new Border
                {
                    Height = 1,
                    Background = new SolidColorBrush(Color.FromArgb(64, 128, 128, 128)),
                    Margin = new Thickness(0, 8)
                };
                _previewHost.Children.Add(rule);
                continue;
            }

            if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                var block = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 26,
                    FontWeight = FontWeight.Bold,
                    Margin = new Thickness(0, 10, 0, 4)
                };
                ParseInlineRuns(block, line[2..]);
                _previewHost.Children.Add(block);
            }
            else if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                var block = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 20,
                    FontWeight = FontWeight.SemiBold,
                    Margin = new Thickness(0, 8, 0, 3)
                };
                ParseInlineRuns(block, line[3..]);
                _previewHost.Children.Add(block);
            }
            else if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                var block = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 16,
                    FontWeight = FontWeight.SemiBold,
                    Margin = new Thickness(0, 6, 0, 2)
                };
                ParseInlineRuns(block, line[4..]);
                _previewHost.Children.Add(block);
            }
            else if (line.StartsWith("> ", StringComparison.Ordinal))
            {
                var quoteContent = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    FontStyle = FontStyle.Italic,
                    Opacity = .88
                };
                ParseInlineRuns(quoteContent, line[2..]);
                var quoteBorder = new Border
                {
                    BorderThickness = new Thickness(3, 0, 0, 0),
                    BorderBrush = Brushes.DodgerBlue,
                    CornerRadius = new CornerRadius(0, 6, 6, 0),
                    Padding = new Thickness(10, 4),
                    Margin = new Thickness(0, 4, 0, 4),
                    Background = new SolidColorBrush(Color.FromArgb(24, 128, 128, 128)),
                    Child = quoteContent
                };
                _previewHost.Children.Add(quoteBorder);
            }
            else if (line.StartsWith("- ", StringComparison.Ordinal) || line.StartsWith("* ", StringComparison.Ordinal))
            {
                var block = new TextBlock { TextWrapping = TextWrapping.Wrap, MinHeight = 20, Margin = new Thickness(12, 2, 0, 2) };
                block.Inlines!.Add(new Run("•  ") { FontWeight = FontWeight.Bold });
                ParseInlineRuns(block, line[2..]);
                _previewHost.Children.Add(block);
            }
            else if (char.IsDigit(line.FirstOrDefault()) && line.Length > 2 && line[1] == '.' && line[2] == ' ')
            {
                var prefix = line[..3];
                var block = new TextBlock { TextWrapping = TextWrapping.Wrap, MinHeight = 20, Margin = new Thickness(12, 2, 0, 2) };
                block.Inlines!.Add(new Run(prefix));
                ParseInlineRuns(block, line[3..]);
                _previewHost.Children.Add(block);
            }
            else
            {
                var block = new TextBlock { TextWrapping = TextWrapping.Wrap, MinHeight = 20, Margin = new Thickness(0, 2) };
                ParseInlineRuns(block, line);
                _previewHost.Children.Add(block);
            }
        }
    }

    private void OnEditorTextChanged(object? sender, TextChangedEventArgs e)
    {
        RefreshPreview();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool HasSelectionWrapper(string prefix, string suffix)
    {
        var text = _editor.Text ?? string.Empty;
        var start = Math.Clamp(Math.Min(_editor.SelectionStart, _editor.SelectionEnd), 0, text.Length);
        var end = Math.Clamp(Math.Max(_editor.SelectionStart, _editor.SelectionEnd), start, text.Length);
        return start >= prefix.Length && end + suffix.Length <= text.Length &&
               text.AsSpan(start - prefix.Length, prefix.Length).SequenceEqual(prefix) &&
               text.AsSpan(end, suffix.Length).SequenceEqual(suffix);
    }

    private bool CurrentLineStartsWith(params string[] prefixes)
    {
        var text = _editor.Text ?? string.Empty;
        var caret = Math.Clamp(_editor.CaretIndex, 0, text.Length);
        var lineStart = caret == 0 ? 0 : text.LastIndexOf('\n', caret - 1) + 1;
        return prefixes.Any(prefix => text.AsSpan(lineStart).StartsWith(prefix, StringComparison.Ordinal));
    }

    private void WrapSelection(string prefix, string suffix, string fallback)
    {
        var text = _editor.Text ?? string.Empty;
        var start = Math.Clamp(Math.Min(_editor.SelectionStart, _editor.SelectionEnd), 0, text.Length);
        var end = Math.Clamp(Math.Max(_editor.SelectionStart, _editor.SelectionEnd), start, text.Length);
        var selected = end > start ? text[start..end] : fallback;
        _editor.Text = text[..start] + prefix + selected + suffix + text[end..];
        _editor.SelectionStart = start + prefix.Length;
        _editor.SelectionEnd = start + prefix.Length + selected.Length;
    }

    private void PrefixSelectedLines(string prefix)
    {
        var text = _editor.Text ?? string.Empty;
        var selectionStart = Math.Clamp(Math.Min(_editor.SelectionStart, _editor.SelectionEnd), 0, text.Length);
        var selectionEnd = Math.Clamp(Math.Max(_editor.SelectionStart, _editor.SelectionEnd), selectionStart, text.Length);
        var lineStart = text.LastIndexOf('\n', Math.Max(0, selectionStart - 1));
        lineStart = lineStart < 0 ? 0 : lineStart + 1;
        var lineEnd = text.IndexOf('\n', selectionEnd);
        if (lineEnd < 0) lineEnd = text.Length;
        var body = text[lineStart..lineEnd];
        var replacement = prefix + body.Replace("\n", "\n" + prefix, StringComparison.Ordinal);
        _editor.Text = text[..lineStart] + replacement + text[lineEnd..];
        _editor.SelectionStart = lineStart;
        _editor.SelectionEnd = lineStart + replacement.Length;
    }

    private static void ParseInlineRuns(TextBlock block, string text)
    {
        var index = 0;
        while (index < text.Length)
        {
            var marker = FindNextMarker(text, index);
            if (marker.Start < 0)
            {
                block.Inlines!.Add(new Run(text[index..]));
                break;
            }
            if (marker.Start > index) block.Inlines!.Add(new Run(text[index..marker.Start]));
            var contentStart = marker.Start + marker.Open.Length;
            var close = text.IndexOf(marker.Close, contentStart, StringComparison.Ordinal);
            if (close < 0)
            {
                block.Inlines!.Add(new Run(text[marker.Start..]));
                break;
            }
            var run = new Run(text[contentStart..close]);
            marker.Apply(run);
            block.Inlines!.Add(run);
            index = close + marker.Close.Length;
        }
        if (text.Length == 0) block.Inlines!.Add(new Run(" "));
    }

    private static (int Start, string Open, string Close, Action<Run> Apply) FindNextMarker(string text, int start)
    {
        var markers = new (string Open, string Close, Action<Run> Apply)[]
        {
            ("**", "**", run => run.FontWeight = FontWeight.Bold),
            ("<u>", "</u>", run => run.TextDecorations = TextDecorations.Underline),
            ("~~", "~~", run => run.TextDecorations = TextDecorations.Strikethrough),
            ("_", "_", run => run.FontStyle = FontStyle.Italic),
            ("`", "`", run => { run.FontFamily = new FontFamily("Cascadia Code,Consolas,Monospace"); run.Background = new SolidColorBrush(Color.FromArgb(40, 128, 128, 128)); })
        };
        var found = markers.Select(marker => (Index: text.IndexOf(marker.Open, start, StringComparison.Ordinal), marker.Open, marker.Close, marker.Apply))
            .Where(value => value.Index >= 0)
            .OrderBy(value => value.Index)
            .FirstOrDefault();
        return string.IsNullOrEmpty(found.Open) ? (-1, string.Empty, string.Empty, _ => { }) : (found.Index, found.Open, found.Close, found.Apply);
    }
}

/// <summary>Rich-editor chrome contract. Hosts plug in any document engine through IMdRichEditorAdapter.</summary>
public class MdRichEditor : ContentControl
{
    public static readonly StyledProperty<IMdRichEditorAdapter?> AdapterProperty = AvaloniaProperty.Register<MdRichEditor, IMdRichEditorAdapter?>(nameof(Adapter));
    public static readonly StyledProperty<IEnumerable<MdRichEditorCommand>?> ToolbarCommandsProperty = AvaloniaProperty.Register<MdRichEditor, IEnumerable<MdRichEditorCommand>?>(nameof(ToolbarCommands));
    public static readonly DirectProperty<MdRichEditor, IReadOnlyList<MdRichEditorCommandDescriptor>> ToolbarItemsProperty =
        AvaloniaProperty.RegisterDirect<MdRichEditor, IReadOnlyList<MdRichEditorCommandDescriptor>>(nameof(ToolbarItems), control => control.ToolbarItems);
    static MdRichEditor()
    {
        AdapterProperty.Changed.AddClassHandler<MdRichEditor>((control, _) => control.OnAdapterChanged(control.Adapter));
        ToolbarCommandsProperty.Changed.AddClassHandler<MdRichEditor>((control, _) => control.RefreshToolbarItems());
    }
    public MdRichEditor()
    {
        ToolbarCommands = Enum.GetValues<MdRichEditorCommand>();
        RefreshToolbarItems();
    }
    public IMdRichEditorAdapter? Adapter { get => GetValue(AdapterProperty); set => SetValue(AdapterProperty, value); }
    public IEnumerable<MdRichEditorCommand>? ToolbarCommands { get => GetValue(ToolbarCommandsProperty); set => SetValue(ToolbarCommandsProperty, value); }
    public IReadOnlyList<MdRichEditorCommandDescriptor> ToolbarItems => _toolbarItems;
    public event EventHandler? EditorStateChanged;
    private ItemsControl? _toolbar;
    private IMdRichEditorAdapter? _subscribedAdapter;
    private IReadOnlyList<MdRichEditorCommandDescriptor> _toolbarItems = Array.Empty<MdRichEditorCommandDescriptor>();
    public bool Execute(MdRichEditorCommand command, object? parameter = null)
    {
        if (Adapter?.CanExecute(command, parameter) != true) return false; Adapter.Execute(command, parameter); return true;
    }
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_toolbar is not null) _toolbar.RemoveHandler(Button.ClickEvent, OnToolbarClick);
        base.OnApplyTemplate(e);
        _toolbar = e.NameScope.Find<ItemsControl>("PART_Toolbar");
        _toolbar?.AddHandler(Button.ClickEvent, OnToolbarClick);
    }
    private void OnToolbarClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Source is Visual source)
        {
            var btn = source.GetVisualAncestors().Prepend(source).OfType<Button>().FirstOrDefault();
            if (btn?.DataContext is MdRichEditorCommandDescriptor descriptor)
            {
                Execute(descriptor.Command);
                RefreshToolbarItems();
                e.Handled = true;
            }
            else if (btn?.DataContext is MdRichEditorCommand command)
            {
                Execute(command);
                RefreshToolbarItems();
                e.Handled = true;
            }
            else if (btn?.CommandParameter is MdRichEditorCommand cmdParam)
            {
                Execute(cmdParam);
                RefreshToolbarItems();
                e.Handled = true;
            }
        }
    }
    private void OnAdapterChanged(IMdRichEditorAdapter? newAdapter)
    {
        if (_subscribedAdapter is not null) _subscribedAdapter.StateChanged -= OnAdapterStateChanged;
        _subscribedAdapter = newAdapter;
        if (_subscribedAdapter is not null) _subscribedAdapter.StateChanged += OnAdapterStateChanged;
        RefreshToolbarItems();
    }

    private void OnAdapterStateChanged(object? sender, EventArgs e)
    {
        RefreshToolbarItems();
        EditorStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RefreshToolbarItems()
    {
        var stateAdapter = Adapter as IMdRichEditorStateAdapter;
        var items = (ToolbarCommands ?? Array.Empty<MdRichEditorCommand>()).Select(command =>
            new MdRichEditorCommandDescriptor(
                command,
                MdRichEditorCommandConverters.GetTooltip(command),
                MdRichEditorCommandConverters.GetGlyph(command),
                GetShortcut(command),
                IsToggleCommand(command),
                stateAdapter?.IsCommandChecked(command) == true,
                Adapter?.CanExecute(command) == true)).ToArray();
        SetAndRaise(ToolbarItemsProperty, ref _toolbarItems, items);
    }

    private static bool IsToggleCommand(MdRichEditorCommand command) => command is
        MdRichEditorCommand.Bold or MdRichEditorCommand.Italic or MdRichEditorCommand.Underline or
        MdRichEditorCommand.StrikeThrough or MdRichEditorCommand.Heading or MdRichEditorCommand.Quote or
        MdRichEditorCommand.Code or MdRichEditorCommand.BulletedList or MdRichEditorCommand.NumberedList;

    private static string? GetShortcut(MdRichEditorCommand command) => command switch
    {
        MdRichEditorCommand.Bold => "Ctrl+B",
        MdRichEditorCommand.Italic => "Ctrl+I",
        MdRichEditorCommand.Underline => "Ctrl+U",
        MdRichEditorCommand.Link => "Ctrl+K",
        MdRichEditorCommand.Undo => "Ctrl+Z",
        MdRichEditorCommand.Redo => "Ctrl+Y",
        _ => null
    };
}

/// <summary>Long-form compatibility name for MdRichEditor.</summary>
public sealed class MdRichTextEditor : MdRichEditor
{
    protected override Type StyleKeyOverride => typeof(MdRichEditor);
}

public enum MdChatMessageRole { User, Assistant, System }
public sealed record MdChatMessage(
    string Id,
    MdChatMessageRole Role,
    object? Content,
    DateTimeOffset Timestamp,
    string? Sender = null,
    MdAsyncRequestState State = MdAsyncRequestState.Data,
    string? ReplyToId = null,
    object? ReplyPreview = null,
    string? ErrorText = null,
    string? AvatarGlyph = null,
    string? StatusGlyph = null,
    string? Initials = null);

/// <summary>Role-aware message bubble used by <see cref="MdChatView"/>.</summary>
[PseudoClasses(":user", ":assistant", ":system", ":selected", ":failed", ":reply", ":text-content")]
public sealed class MdChatMessagePresenter : ContentControl
{
    public static readonly StyledProperty<MdChatMessage?> MessageProperty =
        AvaloniaProperty.Register<MdChatMessagePresenter, MdChatMessage?>(nameof(Message));
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<MdChatMessagePresenter, bool>(nameof(IsSelected));

    public static readonly DirectProperty<MdChatMessagePresenter, string> FormattedTimeProperty =
        AvaloniaProperty.RegisterDirect<MdChatMessagePresenter, string>(nameof(FormattedTime), control => control.FormattedTime);
    public static readonly DirectProperty<MdChatMessagePresenter, string> AvatarGlyphProperty =
        AvaloniaProperty.RegisterDirect<MdChatMessagePresenter, string>(nameof(AvatarGlyph), control => control.AvatarGlyph);
    public static readonly DirectProperty<MdChatMessagePresenter, string> SenderInitialsProperty =
        AvaloniaProperty.RegisterDirect<MdChatMessagePresenter, string>(nameof(SenderInitials), control => control.SenderInitials);
    public static readonly DirectProperty<MdChatMessagePresenter, string?> TextContentProperty =
        AvaloniaProperty.RegisterDirect<MdChatMessagePresenter, string?>(nameof(TextContent), control => control.TextContent);

    static MdChatMessagePresenter()
    {
        MessageProperty.Changed.AddClassHandler<MdChatMessagePresenter>((presenter, _) => presenter.UpdateRole());
        IsSelectedProperty.Changed.AddClassHandler<MdChatMessagePresenter>((presenter, _) => presenter.UpdateRole());
    }

    public MdChatMessagePresenter() => UpdateRole();
    public MdChatMessage? Message { get => GetValue(MessageProperty); set => SetValue(MessageProperty, value); }
    public bool IsSelected { get => GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }

    public string FormattedTime => Message?.Timestamp.LocalDateTime.ToString("t", CultureInfo.CurrentCulture) ?? string.Empty;
    public string? TextContent => Message?.Content as string;
    public string AvatarGlyph => Message?.AvatarGlyph ?? (Message?.Role == MdChatMessageRole.Assistant ? "\uf06c" : "\ue7fd");
    public string SenderInitials
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Message?.Initials)) return Message.Initials;
            var sender = Message?.Sender?.Trim();
            if (string.IsNullOrEmpty(sender)) return Message?.Role == MdChatMessageRole.Assistant ? "AI" : "U";
            var parts = sender.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[1][0])}";
            return sender.Length > 2 ? sender[..2].ToUpperInvariant() : sender.ToUpperInvariant();
        }
    }

    private void UpdateRole()
    {
        PseudoClasses.Set(":user", Message?.Role == MdChatMessageRole.User);
        PseudoClasses.Set(":assistant", Message?.Role == MdChatMessageRole.Assistant);
        PseudoClasses.Set(":system", Message?.Role == MdChatMessageRole.System);
        PseudoClasses.Set(":selected", IsSelected);
        PseudoClasses.Set(":failed", Message?.State == MdAsyncRequestState.Error);
        PseudoClasses.Set(":reply", Message?.ReplyPreview is not null);
        PseudoClasses.Set(":text-content", Message?.Content is string);
        RaisePropertyChanged(FormattedTimeProperty, string.Empty, FormattedTime);
        RaisePropertyChanged(AvatarGlyphProperty, string.Empty, AvatarGlyph);
        RaisePropertyChanged(SenderInitialsProperty, string.Empty, SenderInitials);
        RaisePropertyChanged(TextContentProperty, null, TextContent);

        var sender = Message?.Sender;
        if (string.IsNullOrWhiteSpace(sender)) sender = Message?.Role switch
        {
            MdChatMessageRole.Assistant => "Assistant",
            MdChatMessageRole.System => "System",
            _ => "You"
        };
        var state = Message?.State == MdAsyncRequestState.Error
            ? $"Send failed. {Message.ErrorText}".Trim()
            : Message?.State.ToString();
        AutomationProperties.SetName(this, $"{sender}, {FormattedTime}. {Message?.Content}. {state}".Trim());
        AutomationProperties.SetHelpText(this, IsSelected ? "Selected message" : "Message");
    }
}

/// <summary>
/// Provider-neutral chat shell with composer, history paging, suggestions, bubble-only multi-selection,
/// quote/delete actions, and failed-message retry. Providers remain responsible for mutating their source.
/// </summary>
[PseudoClasses(":selection", ":quoted")]
public sealed class MdChatView : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> MessagesSourceProperty = AvaloniaProperty.Register<MdChatView, IEnumerable?>(nameof(MessagesSource));
    public static readonly StyledProperty<string?> ComposerTextProperty = AvaloniaProperty.Register<MdChatView, string?>(nameof(ComposerText), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<IEnumerable?> SuggestionsSourceProperty = AvaloniaProperty.Register<MdChatView, IEnumerable?>(nameof(SuggestionsSource));
    public static readonly StyledProperty<ICommand?> SendCommandProperty = AvaloniaProperty.Register<MdChatView, ICommand?>(nameof(SendCommand));
    public static readonly StyledProperty<ICommand?> AttachmentCommandProperty = AvaloniaProperty.Register<MdChatView, ICommand?>(nameof(AttachmentCommand));
    public static readonly StyledProperty<ICommand?> DeleteMessagesCommandProperty = AvaloniaProperty.Register<MdChatView, ICommand?>(nameof(DeleteMessagesCommand));
    public static readonly StyledProperty<ICommand?> QuoteMessageCommandProperty = AvaloniaProperty.Register<MdChatView, ICommand?>(nameof(QuoteMessageCommand));
    public static readonly StyledProperty<ICommand?> RetryMessageCommandProperty = AvaloniaProperty.Register<MdChatView, ICommand?>(nameof(RetryMessageCommand));
    public static readonly StyledProperty<bool> IsBusyProperty = AvaloniaProperty.Register<MdChatView, bool>(nameof(IsBusy));
    public static readonly StyledProperty<bool> AllowMultipleSelectionProperty = AvaloniaProperty.Register<MdChatView, bool>(nameof(AllowMultipleSelection), true);
    public static readonly DirectProperty<MdChatView, int> SelectionCountProperty = AvaloniaProperty.RegisterDirect<MdChatView, int>(nameof(SelectionCount), control => control.SelectionCount);
    public static readonly DirectProperty<MdChatView, MdChatMessage?> QuotedMessageProperty = AvaloniaProperty.RegisterDirect<MdChatView, MdChatMessage?>(nameof(QuotedMessage), control => control.QuotedMessage);
    public static readonly DirectProperty<MdChatView, string> StatusTextProperty = AvaloniaProperty.RegisterDirect<MdChatView, string>(nameof(StatusText), control => control.StatusText);

    private Button? _sendButton;
    private Button? _attachmentButton;
    private Button? _cancelSelectionButton;
    private Button? _deleteButton;
    private Button? _quoteButton;
    private Button? _cancelQuoteButton;
    private ItemsControl? _suggestionsHost;
    private ListBox? _messagesHost;
    private TextBox? _composer;
    private int _selectionCount;
    private MdChatMessage? _quotedMessage;
    private string _statusText = string.Empty;
    private bool _synchronizingSelection;
    private INotifyCollectionChanged? _observedMessages;

    static MdChatView()
    {
        MessagesSourceProperty.Changed.AddClassHandler<MdChatView>((control, _) => control.OnMessagesSourceChanged());
        IsBusyProperty.Changed.AddClassHandler<MdChatView>((control, _) => control.StatusText = control.IsBusy ? "Loading messages" : "Messages ready");
    }

    public IEnumerable? MessagesSource { get => GetValue(MessagesSourceProperty); set => SetValue(MessagesSourceProperty, value); }
    public string? ComposerText { get => GetValue(ComposerTextProperty); set => SetValue(ComposerTextProperty, value); }
    public IEnumerable? SuggestionsSource { get => GetValue(SuggestionsSourceProperty); set => SetValue(SuggestionsSourceProperty, value); }
    public ICommand? SendCommand { get => GetValue(SendCommandProperty); set => SetValue(SendCommandProperty, value); }
    public ICommand? AttachmentCommand { get => GetValue(AttachmentCommandProperty); set => SetValue(AttachmentCommandProperty, value); }
    public ICommand? DeleteMessagesCommand { get => GetValue(DeleteMessagesCommandProperty); set => SetValue(DeleteMessagesCommandProperty, value); }
    public ICommand? QuoteMessageCommand { get => GetValue(QuoteMessageCommandProperty); set => SetValue(QuoteMessageCommandProperty, value); }
    public ICommand? RetryMessageCommand { get => GetValue(RetryMessageCommandProperty); set => SetValue(RetryMessageCommandProperty, value); }
    public bool IsBusy { get => GetValue(IsBusyProperty); set => SetValue(IsBusyProperty, value); }
    public bool AllowMultipleSelection { get => GetValue(AllowMultipleSelectionProperty); set => SetValue(AllowMultipleSelectionProperty, value); }
    public int SelectionCount => _selectionCount;
    public MdChatMessage? QuotedMessage => _quotedMessage;
    public string StatusText { get => _statusText; private set => SetAndRaise(StatusTextProperty, ref _statusText, value); }
    public ObservableCollection<MdChatMessage> SelectedMessages { get; } = [];
    public Func<CancellationToken, ValueTask<IReadOnlyList<MdChatMessage>>>? HistoryProvider { get; set; }

    public event EventHandler<string>? MessageSubmitted;
    public event EventHandler? AttachmentRequested;
    public event EventHandler<IReadOnlyList<MdChatMessage>>? HistoryLoaded;
    public event EventHandler<IReadOnlyList<MdChatMessage>>? MessageSelectionChanged;
    public event EventHandler<IReadOnlyList<MdChatMessage>>? DeleteRequested;
    public event EventHandler<MdChatMessage>? QuoteRequested;
    public event EventHandler<MdChatMessage>? RetryRequested;

    public bool Submit()
    {
        var text = ComposerText?.Trim();
        if (string.IsNullOrEmpty(text) || IsBusy) return false;
        if (SendCommand?.CanExecute(text) == true) SendCommand.Execute(text);
        MessageSubmitted?.Invoke(this, text);
        StatusText = "Message submitted";
        ComposerText = string.Empty;
        CancelQuote();
        return true;
    }

    public bool ToggleMessageSelection(MdChatMessage message)
    {
        if (!AllowMultipleSelection && !SelectedMessages.Contains(message)) SelectedMessages.Clear();
        if (!SelectedMessages.Remove(message)) SelectedMessages.Add(message);
        SynchronizeHostSelection();
        UpdateSelectionState();
        return SelectedMessages.Contains(message);
    }

    public void ClearSelection()
    {
        SelectedMessages.Clear();
        SynchronizeHostSelection();
        UpdateSelectionState();
    }

    public bool DeleteSelected()
    {
        var selected = SelectedMessages.ToArray();
        if (selected.Length == 0) return false;
        if (DeleteMessagesCommand?.CanExecute(selected) == true) DeleteMessagesCommand.Execute(selected);
        DeleteRequested?.Invoke(this, selected);
        ClearSelection();
        StatusText = $"Deleted {selected.Length} message{(selected.Length == 1 ? string.Empty : "s")}";
        return true;
    }

    public bool QuoteSelected()
    {
        var message = SelectedMessages.LastOrDefault();
        if (message is null) return false;
        SetAndRaise(QuotedMessageProperty, ref _quotedMessage, message);
        PseudoClasses.Set(":quoted", true);
        if (QuoteMessageCommand?.CanExecute(message) == true) QuoteMessageCommand.Execute(message);
        QuoteRequested?.Invoke(this, message);
        ClearSelection();
        _composer?.Focus();
        return true;
    }

    public void CancelQuote()
    {
        SetAndRaise(QuotedMessageProperty, ref _quotedMessage, null);
        PseudoClasses.Set(":quoted", false);
    }

    public bool Retry(MdChatMessage message)
    {
        if (message.State != MdAsyncRequestState.Error) return false;
        if (RetryMessageCommand?.CanExecute(message) == true) RetryMessageCommand.Execute(message);
        RetryRequested?.Invoke(this, message);
        StatusText = $"Retrying message from {message.Sender ?? message.Role.ToString()}";
        return true;
    }

    public async ValueTask LoadHistoryAsync(CancellationToken cancellationToken = default)
    {
        if (HistoryProvider is null) return;
        IsBusy = true;
        try { HistoryLoaded?.Invoke(this, await HistoryProvider(cancellationToken)); }
        finally { IsBusy = false; }
    }

    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ObserveMessagesSource();
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (_observedMessages is not null) _observedMessages.CollectionChanged -= OnMessagesCollectionChanged;
        _observedMessages = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateHandlers();
        base.OnApplyTemplate(e);
        _sendButton = e.NameScope.Find<Button>("PART_SendButton");
        _attachmentButton = e.NameScope.Find<Button>("PART_AttachmentButton");
        _cancelSelectionButton = e.NameScope.Find<Button>("PART_CancelSelectionButton");
        _deleteButton = e.NameScope.Find<Button>("PART_DeleteButton");
        _quoteButton = e.NameScope.Find<Button>("PART_QuoteButton");
        _cancelQuoteButton = e.NameScope.Find<Button>("PART_CancelQuoteButton");
        _suggestionsHost = e.NameScope.Find<ItemsControl>("PART_SuggestionsHost");
        _messagesHost = e.NameScope.Find<ListBox>("PART_MessagesHost");
        if (_messagesHost is not null) _messagesHost.SelectionMode = SelectionMode.Multiple | SelectionMode.Toggle;
        _composer = e.NameScope.Find<TextBox>("PART_Composer");
        AttachTemplateHandlers();
        SynchronizeHostSelection();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && SelectionCount > 0) { ClearSelection(); e.Handled = true; }
        else if (e.Key == Key.Escape && QuotedMessage is not null) { CancelQuote(); e.Handled = true; }
        else if (e.Key == Key.Delete && SelectionCount > 0 && DeleteSelected()) e.Handled = true;
        else if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift) && Submit()) e.Handled = true;
        else base.OnKeyDown(e);
    }

    private void AttachTemplateHandlers()
    {
        if (_sendButton is not null) _sendButton.Click += OnSend;
        if (_attachmentButton is not null) _attachmentButton.Click += OnAttachment;
        if (_cancelSelectionButton is not null) _cancelSelectionButton.Click += OnCancelSelection;
        if (_deleteButton is not null) _deleteButton.Click += OnDelete;
        if (_quoteButton is not null) _quoteButton.Click += OnQuote;
        if (_cancelQuoteButton is not null) _cancelQuoteButton.Click += OnCancelQuote;
        if (_messagesHost is not null)
        {
            _messagesHost.SelectionChanged += OnMessageSelectionChanged;
            _messagesHost.AddHandler(InputElement.PointerPressedEvent, OnMessagePointerPressed,
                global::Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
            _messagesHost.AddHandler(Button.ClickEvent, OnMessageActionClick);
        }
        _suggestionsHost?.AddHandler(Button.ClickEvent, OnSuggestionClick);
    }

    private void DetachTemplateHandlers()
    {
        if (_sendButton is not null) _sendButton.Click -= OnSend;
        if (_attachmentButton is not null) _attachmentButton.Click -= OnAttachment;
        if (_cancelSelectionButton is not null) _cancelSelectionButton.Click -= OnCancelSelection;
        if (_deleteButton is not null) _deleteButton.Click -= OnDelete;
        if (_quoteButton is not null) _quoteButton.Click -= OnQuote;
        if (_cancelQuoteButton is not null) _cancelQuoteButton.Click -= OnCancelQuote;
        if (_messagesHost is not null)
        {
            _messagesHost.SelectionChanged -= OnMessageSelectionChanged;
            _messagesHost.RemoveHandler(InputElement.PointerPressedEvent, OnMessagePointerPressed);
            _messagesHost.RemoveHandler(Button.ClickEvent, OnMessageActionClick);
        }
        _suggestionsHost?.RemoveHandler(Button.ClickEvent, OnSuggestionClick);
    }

    private void OnMessageSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_synchronizingSelection || _messagesHost is null) return;
        _synchronizingSelection = true;
        try
        {
            SelectedMessages.Clear();
            foreach (var message in (_messagesHost.SelectedItems ?? Array.Empty<object>()).OfType<MdChatMessage>())
            {
                if (!AllowMultipleSelection) SelectedMessages.Clear();
                SelectedMessages.Add(message);
            }
        }
        finally { _synchronizingSelection = false; }
        UpdateSelectionState();
    }

    private void SynchronizeHostSelection()
    {
        if (_messagesHost is null || _synchronizingSelection) return;
        _synchronizingSelection = true;
        try
        {
            var selectedItems = _messagesHost.SelectedItems;
            if (selectedItems is null) return;
            selectedItems.Clear();
            foreach (var message in SelectedMessages) selectedItems.Add(message);
        }
        finally { _synchronizingSelection = false; }
    }

    private void OnMessagesSourceChanged()
    {
        ObserveMessagesSource();
        ReconcileSelection();
    }

    private void ObserveMessagesSource()
    {
        var next = this.IsAttachedToVisualTree() ? MessagesSource as INotifyCollectionChanged : null;
        if (ReferenceEquals(next, _observedMessages)) return;
        if (_observedMessages is not null) _observedMessages.CollectionChanged -= OnMessagesCollectionChanged;
        _observedMessages = next;
        if (_observedMessages is not null) _observedMessages.CollectionChanged += OnMessagesCollectionChanged;
    }

    private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ReconcileSelection();
        var message = e.NewItems?.OfType<MdChatMessage>().LastOrDefault();
        if (message is null) return;
        var senderName = message.Sender ?? (message.Role switch
        {
            MdChatMessageRole.Assistant => "Assistant",
            MdChatMessageRole.System => "System",
            _ => "You"
        });
        StatusText = message.State == MdAsyncRequestState.Error
            ? $"Message from {senderName} failed. {message.ErrorText}".Trim()
            : $"New message from {senderName}: {message.Content}";
    }

    private void ReconcileSelection()
    {
        var available = (MessagesSource ?? Array.Empty<object>()).OfType<MdChatMessage>().ToHashSet();
        foreach (var selected in SelectedMessages.Where(message => !available.Contains(message)).ToArray()) SelectedMessages.Remove(selected);
        SynchronizeHostSelection();
        UpdateSelectionState();
    }

    private void UpdateSelectionState()
    {
        SetAndRaise(SelectionCountProperty, ref _selectionCount, SelectedMessages.Count);
        PseudoClasses.Set(":selection", SelectionCount > 0);
        StatusText = SelectionCount == 0 ? "Message selection cleared" : $"{SelectionCount} message{(SelectionCount == 1 ? string.Empty : "s")} selected";
        MessageSelectionChanged?.Invoke(this, SelectedMessages.ToArray());
    }

    private void OnSend(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => Submit();
    private void OnAttachment(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (AttachmentCommand?.CanExecute(null) == true) AttachmentCommand.Execute(null);
        AttachmentRequested?.Invoke(this, EventArgs.Empty);
    }
    private void OnCancelSelection(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => ClearSelection();
    private void OnDelete(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => DeleteSelected();
    private void OnQuote(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => QuoteSelected();
    private void OnCancelQuote(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => CancelQuote();

    private void OnMessagePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var source = e.Source as Visual;
        if (source is null) return;
        var path = source.GetVisualAncestors().Prepend(source).ToArray();
        if (path.OfType<Button>().Any()) return;
        // Selection belongs to the visual bubble. Transparent space in a full-width virtualized
        // row must remain available for scrolling and must never toggle message selection.
        if (!path.OfType<Border>().Any(border => border.Name == "PART_Bubble")) e.Handled = true;
    }

    private void OnMessageActionClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Source is Button { Classes: var classes, DataContext: MdChatMessage message } && classes.Contains("retry"))
        {
            Retry(message);
            e.Handled = true;
        }
    }

    private void OnSuggestionClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Source is Button { DataContext: { } suggestion })
        {
            SetCurrentValue(ComposerTextProperty, suggestion.ToString());
            _composer?.Focus();
            e.Handled = true;
        }
    }
}
