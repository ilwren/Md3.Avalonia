using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Md3.Avalonia.Ecosystem.Infrastructure;

namespace Md3.Avalonia.Ecosystem.Controls;

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
    private MdChartPoint? _hoveredPoint;
    static MdChart()
    {
        AffectsRender<MdChart>(SeriesProperty, KindProperty, ShowGridProperty, AxisBrushProperty, GridBrushProperty, XAxisTitleProperty, YAxisTitleProperty);
        AccessibleTitleProperty.Changed.AddClassHandler<MdChart>((chart, _) => AutomationProperties.SetName(chart, chart.AccessibleTitle));
    }
    public MdChart() { ClipToBounds = true; Focusable = true; AutomationProperties.SetName(this, AccessibleTitle); }
    public IEnumerable<MdChartSeries>? Series { get => GetValue(SeriesProperty); set => SetValue(SeriesProperty, value); }
    public MdChartKind Kind { get => GetValue(KindProperty); set => SetValue(KindProperty, value); }
    public bool ShowGrid { get => GetValue(ShowGridProperty); set => SetValue(ShowGridProperty, value); }
    public IBrush? AxisBrush { get => GetValue(AxisBrushProperty); set => SetValue(AxisBrushProperty, value); }
    public IBrush? GridBrush { get => GetValue(GridBrushProperty); set => SetValue(GridBrushProperty, value); }
    public string? XAxisTitle { get => GetValue(XAxisTitleProperty); set => SetValue(XAxisTitleProperty, value); }
    public string? YAxisTitle { get => GetValue(YAxisTitleProperty); set => SetValue(YAxisTitleProperty, value); }
    public string AccessibleTitle { get => GetValue(AccessibleTitleProperty); set => SetValue(AccessibleTitleProperty, value); }
    public MdChartPoint? HoveredPoint { get => _hoveredPoint; private set => SetAndRaise(HoveredPointProperty, ref _hoveredPoint, value); }
    public IMdChartDataProvider? Provider { get; set; }
    public event EventHandler<MdChartPoint?>? HoveredPointChanged;
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
        base.OnPointerMoved(e); var position = e.GetPosition(this); var all = (Series ?? Array.Empty<MdChartSeries>()).SelectMany(series => series.Points).ToArray();
        if (all.Length == 0) return;
        // X-distance is deliberately used for stable touch/pointer exploration independent of Y scale.
        var normalized = Math.Clamp((position.X - 58) / Math.Max(1, Bounds.Width - 76), 0, 1); var min = all.Min(p => p.X); var max = all.Max(p => p.X);
        var next = all.MinBy(point => Math.Abs(point.X - (min + normalized * (max - min))));
        if (!Equals(next, HoveredPoint)) { HoveredPoint = next; HoveredPointChanged?.Invoke(this, next); }
    }
    protected override void OnPointerExited(PointerEventArgs e) { base.OnPointerExited(e); HoveredPoint = null; HoveredPointChanged?.Invoke(this, null); }
    private static readonly IBrush[] Palette = [Brushes.MediumPurple, Brushes.Teal, Brushes.OrangeRed, Brushes.RoyalBlue];
}

public enum MdRichEditorCommand { Bold, Italic, Underline, StrikeThrough, Heading, Quote, Code, Link, BulletedList, NumberedList, Undo, Redo }
public interface IMdRichEditorAdapter
{
    object? Document { get; set; }
    bool CanExecute(MdRichEditorCommand command, object? parameter = null);
    void Execute(MdRichEditorCommand command, object? parameter = null);
    event EventHandler? StateChanged;
}

/// <summary>
/// Built-in lightweight rich-text adapter for an Avalonia <see cref="TextBox"/>. Commands modify
/// the selected source using portable rich-text markers and immediately rebuild an optional
/// WYSIWYG preview panel. Applications can still replace it with a document-engine adapter.
/// </summary>
public sealed class MdTextBoxRichEditorAdapter : IMdRichEditorAdapter
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
        }
        RefreshPreview();
        StateChanged?.Invoke(this, EventArgs.Empty);
        _editor.Focus();
    }

    public void RefreshPreview()
    {
        if (_previewHost is null) return;
        _previewHost.Children.Clear();
        var lines = (_editor.Text ?? string.Empty).Replace("\r\n", "\n").Split('\n');
        foreach (var sourceLine in lines)
        {
            var line = sourceLine;
            var block = new TextBlock { TextWrapping = TextWrapping.Wrap, MinHeight = 22 };
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                line = line[3..];
                block.FontSize = 22;
                block.FontWeight = FontWeight.SemiBold;
                block.Margin = new Thickness(0, 8, 0, 2);
            }
            else if (line.StartsWith("> ", StringComparison.Ordinal))
            {
                line = "│  " + line[2..];
                block.FontStyle = FontStyle.Italic;
                block.Opacity = .82;
                block.Margin = new Thickness(8, 2);
            }
            else if (line.StartsWith("- ", StringComparison.Ordinal)) line = "•  " + line[2..];
            ParseInlineRuns(block, line);
            _previewHost.Children.Add(block);
        }
    }

    private void OnEditorTextChanged(object? sender, TextChangedEventArgs e)
    {
        RefreshPreview();
        StateChanged?.Invoke(this, EventArgs.Empty);
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
            ("`", "`", run => { run.FontFamily = new FontFamily("monospace"); run.Background = Brushes.Black; run.Foreground = Brushes.White; })
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
    static MdRichEditor() => AdapterProperty.Changed.AddClassHandler<MdRichEditor>((control, _) => control.OnAdapterChanged(control.Adapter));
    public MdRichEditor() => ToolbarCommands = Enum.GetValues<MdRichEditorCommand>();
    public IMdRichEditorAdapter? Adapter { get => GetValue(AdapterProperty); set => SetValue(AdapterProperty, value); }
    public IEnumerable<MdRichEditorCommand>? ToolbarCommands { get => GetValue(ToolbarCommandsProperty); set => SetValue(ToolbarCommandsProperty, value); }
    public event EventHandler? EditorStateChanged;
    private ItemsControl? _toolbar;
    private IMdRichEditorAdapter? _subscribedAdapter;
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
        if (e.Source is Button { DataContext: MdRichEditorCommand command }) { Execute(command); e.Handled = true; }
    }
    private void OnAdapterChanged(IMdRichEditorAdapter? newAdapter)
    {
        if (_subscribedAdapter is not null) _subscribedAdapter.StateChanged -= OnAdapterStateChanged;
        _subscribedAdapter = newAdapter;
        if (_subscribedAdapter is not null) _subscribedAdapter.StateChanged += OnAdapterStateChanged;
    }
    private void OnAdapterStateChanged(object? sender, EventArgs e) => EditorStateChanged?.Invoke(this, EventArgs.Empty);
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
    string? ErrorText = null);

/// <summary>Role-aware message bubble used by <see cref="MdChatView"/>.</summary>
[PseudoClasses(":user", ":assistant", ":system", ":selected", ":failed", ":reply")]
public sealed class MdChatMessagePresenter : ContentControl
{
    public static readonly StyledProperty<MdChatMessage?> MessageProperty =
        AvaloniaProperty.Register<MdChatMessagePresenter, MdChatMessage?>(nameof(Message));
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<MdChatMessagePresenter, bool>(nameof(IsSelected));

    static MdChatMessagePresenter()
    {
        MessageProperty.Changed.AddClassHandler<MdChatMessagePresenter>((presenter, _) => presenter.UpdateRole());
        IsSelectedProperty.Changed.AddClassHandler<MdChatMessagePresenter>((presenter, _) => presenter.UpdateRole());
    }

    public MdChatMessagePresenter() => UpdateRole();
    public MdChatMessage? Message { get => GetValue(MessageProperty); set => SetValue(MessageProperty, value); }
    public bool IsSelected { get => GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }

    private void UpdateRole()
    {
        PseudoClasses.Set(":user", Message?.Role == MdChatMessageRole.User);
        PseudoClasses.Set(":assistant", Message?.Role == MdChatMessageRole.Assistant);
        PseudoClasses.Set(":system", Message?.Role == MdChatMessageRole.System);
        PseudoClasses.Set(":selected", IsSelected);
        PseudoClasses.Set(":failed", Message?.State == MdAsyncRequestState.Error);
        PseudoClasses.Set(":reply", Message?.ReplyPreview is not null);
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
    public static readonly StyledProperty<ICommand?> DeleteMessagesCommandProperty = AvaloniaProperty.Register<MdChatView, ICommand?>(nameof(DeleteMessagesCommand));
    public static readonly StyledProperty<ICommand?> QuoteMessageCommandProperty = AvaloniaProperty.Register<MdChatView, ICommand?>(nameof(QuoteMessageCommand));
    public static readonly StyledProperty<ICommand?> RetryMessageCommandProperty = AvaloniaProperty.Register<MdChatView, ICommand?>(nameof(RetryMessageCommand));
    public static readonly StyledProperty<bool> IsBusyProperty = AvaloniaProperty.Register<MdChatView, bool>(nameof(IsBusy));
    public static readonly StyledProperty<bool> AllowMultipleSelectionProperty = AvaloniaProperty.Register<MdChatView, bool>(nameof(AllowMultipleSelection), true);
    public static readonly DirectProperty<MdChatView, int> SelectionCountProperty = AvaloniaProperty.RegisterDirect<MdChatView, int>(nameof(SelectionCount), control => control.SelectionCount);
    public static readonly DirectProperty<MdChatView, MdChatMessage?> QuotedMessageProperty = AvaloniaProperty.RegisterDirect<MdChatView, MdChatMessage?>(nameof(QuotedMessage), control => control.QuotedMessage);

    private Button? _sendButton;
    private Button? _cancelSelectionButton;
    private Button? _deleteButton;
    private Button? _quoteButton;
    private Button? _cancelQuoteButton;
    private ItemsControl? _suggestionsHost;
    private ListBox? _messagesHost;
    private TextBox? _composer;
    private int _selectionCount;
    private MdChatMessage? _quotedMessage;
    private bool _synchronizingSelection;

    static MdChatView() => MessagesSourceProperty.Changed.AddClassHandler<MdChatView>((control, _) => control.ReconcileSelection());

    public IEnumerable? MessagesSource { get => GetValue(MessagesSourceProperty); set => SetValue(MessagesSourceProperty, value); }
    public string? ComposerText { get => GetValue(ComposerTextProperty); set => SetValue(ComposerTextProperty, value); }
    public IEnumerable? SuggestionsSource { get => GetValue(SuggestionsSourceProperty); set => SetValue(SuggestionsSourceProperty, value); }
    public ICommand? SendCommand { get => GetValue(SendCommandProperty); set => SetValue(SendCommandProperty, value); }
    public ICommand? DeleteMessagesCommand { get => GetValue(DeleteMessagesCommandProperty); set => SetValue(DeleteMessagesCommandProperty, value); }
    public ICommand? QuoteMessageCommand { get => GetValue(QuoteMessageCommandProperty); set => SetValue(QuoteMessageCommandProperty, value); }
    public ICommand? RetryMessageCommand { get => GetValue(RetryMessageCommandProperty); set => SetValue(RetryMessageCommandProperty, value); }
    public bool IsBusy { get => GetValue(IsBusyProperty); set => SetValue(IsBusyProperty, value); }
    public bool AllowMultipleSelection { get => GetValue(AllowMultipleSelectionProperty); set => SetValue(AllowMultipleSelectionProperty, value); }
    public int SelectionCount => _selectionCount;
    public MdChatMessage? QuotedMessage => _quotedMessage;
    public ObservableCollection<MdChatMessage> SelectedMessages { get; } = [];
    public Func<CancellationToken, ValueTask<IReadOnlyList<MdChatMessage>>>? HistoryProvider { get; set; }

    public event EventHandler<string>? MessageSubmitted;
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
        return true;
    }

    public async ValueTask LoadHistoryAsync(CancellationToken cancellationToken = default)
    {
        if (HistoryProvider is null) return;
        IsBusy = true;
        try { HistoryLoaded?.Invoke(this, await HistoryProvider(cancellationToken)); }
        finally { IsBusy = false; }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateHandlers();
        base.OnApplyTemplate(e);
        _sendButton = e.NameScope.Find<Button>("PART_SendButton");
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
        MessageSelectionChanged?.Invoke(this, SelectedMessages.ToArray());
    }

    private void OnSend(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => Submit();
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
