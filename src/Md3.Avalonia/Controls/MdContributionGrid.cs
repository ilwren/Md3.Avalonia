using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

/// <summary>One day in a <see cref="MdContributionGrid"/>: a date at day precision and its activity value.</summary>
public sealed class MdContributionDay
{
    /// <summary>Gets or sets the day the value belongs to.</summary>
    public DateTimeOffset Date { get; init; }

    /// <summary>Gets or sets the activity value for the day (zero or more).</summary>
    public int Value { get; init; }
}

/// <summary>A month label placed above the week column in which that month starts.</summary>
public sealed record MdContributionMonthLabel(string Text, int WeekIndex);

/// <summary>
/// Renders a GitHub-style contribution graph: one column per week, seven rows, each day colored
/// by its activity level, with month labels above, weekday labels on the left and a level legend.
/// </summary>
/// <remarks>
/// Levels 1-4 are relative to the data itself (GitHub-style): the non-zero values are split at
/// their 25/50/75 percentiles, so the same visual distribution comes out of 1-5 values as of
/// 1-500. Days absent from <see cref="Days"/> count as zero. Clicking a day raises
/// <see cref="DayClicked"/> and hovering shows a tooltip with the value and date.
/// </remarks>
public class MdContributionGrid : TemplatedControl
{
    public static readonly StyledProperty<IReadOnlyList<MdContributionDay>> DaysProperty =
        AvaloniaProperty.Register<MdContributionGrid, IReadOnlyList<MdContributionDay>>(
            nameof(Days), Array.Empty<MdContributionDay>());
    public static readonly StyledProperty<double> CellSizeProperty =
        AvaloniaProperty.Register<MdContributionGrid, double>(nameof(CellSize), 12);
    public static readonly StyledProperty<double> CellSpacingProperty =
        AvaloniaProperty.Register<MdContributionGrid, double>(nameof(CellSpacing), 3);
    public static readonly StyledProperty<string> LessLabelProperty =
        AvaloniaProperty.Register<MdContributionGrid, string>(nameof(LessLabel), "Less");
    public static readonly StyledProperty<string> MoreLabelProperty =
        AvaloniaProperty.Register<MdContributionGrid, string>(nameof(MoreLabel), "More");

    private readonly Dictionary<DateTime, int> _valueByDay = new();
    private ContributionCellsHost? _cells;
    private Canvas? _monthLabels;
    private Canvas? _dayLabels;
    private IBrush?[] _levelBrushCache = Array.Empty<IBrush?>();
    private ThemeVariant? _levelBrushVariant;

    static MdContributionGrid()
    {
        DaysProperty.Changed.AddClassHandler<MdContributionGrid>((grid, _) => grid.UpdateModel());
        CellSizeProperty.Changed.AddClassHandler<MdContributionGrid>((grid, _) => grid.OnLayoutMetricsChanged());
        CellSpacingProperty.Changed.AddClassHandler<MdContributionGrid>((grid, _) => grid.OnLayoutMetricsChanged());
    }

    /// <summary>Gets or sets the days to render; order does not matter.</summary>
    public IReadOnlyList<MdContributionDay> Days
    {
        get => GetValue(DaysProperty);
        set => SetValue(DaysProperty, value);
    }

    /// <summary>Gets or sets the edge length of one day cell.</summary>
    public double CellSize
    {
        get => GetValue(CellSizeProperty);
        set => SetValue(CellSizeProperty, value);
    }

    /// <summary>Gets or sets the gap between two day cells.</summary>
    public double CellSpacing
    {
        get => GetValue(CellSpacingProperty);
        set => SetValue(CellSpacingProperty, value);
    }

    /// <summary>Gets or sets the legend text at the low end of the levels.</summary>
    public string LessLabel
    {
        get => GetValue(LessLabelProperty);
        set => SetValue(LessLabelProperty, value);
    }

    /// <summary>Gets or sets the legend text at the high end of the levels.</summary>
    public string MoreLabel
    {
        get => GetValue(MoreLabelProperty);
        set => SetValue(MoreLabelProperty, value);
    }

    /// <summary>Gets the number of week columns the data spans.</summary>
    public int WeekCount { get; private set; }

    /// <summary>Gets the first day of the first (possibly partial) week column.</summary>
    internal DateTime WeekStart { get; private set; }

    /// <summary>Gets the month labels with the week column each one starts at.</summary>
    public IReadOnlyList<MdContributionMonthLabel> MonthLabels
    {
        get; private set;
    } = Array.Empty<MdContributionMonthLabel>();

    /// <summary>Raised when a day cell is clicked; the argument is that day (missing days report zero).</summary>
    public event EventHandler<MdContributionDay?>? DayClicked;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _cells = e.NameScope.Find<ContributionCellsHost>("PART_Cells");
        _monthLabels = e.NameScope.Find<Canvas>("PART_MonthLabels");
        _dayLabels = e.NameScope.Find<Canvas>("PART_DayLabels");
        _cells?.Attach(this);
        UpdateModel();
    }

    internal void OnDayClicked(MdContributionDay? day) => DayClicked?.Invoke(this, day);

    internal bool TryGetDayValue(DateTime date, out int value) => _valueByDay.TryGetValue(date, out value);

    internal int[] Thresholds { get; private set; } = Array.Empty<int>();

    internal IBrush?[] GetLevelBrushes()
    {
        var variant = ActualThemeVariant;
        if (_levelBrushCache.Length != 5 || _levelBrushVariant != variant)
        {
            _levelBrushCache = new IBrush?[5];
            for (var i = 0; i < 5; i++)
                _levelBrushCache[i] = MdThemeResources.FindBrush(this, $"Md.ContributionGrid.Level.Brush.{i}", variant);
            _levelBrushVariant = variant;
        }

        return _levelBrushCache;
    }

    private void OnLayoutMetricsChanged()
    {
        _cells?.InvalidateMeasure();
        _cells?.InvalidateVisual();
        RebuildLabels();
    }

    private void UpdateModel()
    {
        var days = Days ?? Array.Empty<MdContributionDay>();
        _valueByDay.Clear();
        foreach (var day in days)
            _valueByDay[day.Date.Date] = day.Value;

        if (days.Count == 0)
        {
            WeekCount = 0;
            WeekStart = default;
            MonthLabels = Array.Empty<MdContributionMonthLabel>();
            Thresholds = Array.Empty<int>();
        }
        else
        {
            var first = days.Min(day => day.Date.Date);
            var last = days.Max(day => day.Date.Date);
            WeekStart = first.AddDays(-first.DayOfWeek); // week columns start on Sunday, like GitHub
            WeekCount = (int)((last - WeekStart).TotalDays / 7) + 1;

            var labels = new List<MdContributionMonthLabel>();
            var previousMonth = -1;
            for (var week = 0; week < WeekCount; week++)
            {
                var weekStart = WeekStart.AddDays(7L * week);
                if (weekStart.Month != previousMonth)
                {
                    labels.Add(new MdContributionMonthLabel(
                        CultureInfo.CurrentCulture.TextInfo.GetAbbreviatedMonthName(weekStart.Month),
                        week));
                    previousMonth = weekStart.Month;
                }
            }

            MonthLabels = labels;

            var nonZero = days
                .Where(day => day.Value > 0)
                .Select(day => (double)day.Value)
                .OrderBy(v => v)
                .ToList();
            Thresholds = nonZero.Count == 0
                ? new[] { 0, 0, 0, 0 }
                : new[]
                {
                    (int)Math.Round(Quantile(nonZero, 0.25)),
                    (int)Math.Round(Quantile(nonZero, 0.5)),
                    (int)Math.Round(Quantile(nonZero, 0.75)),
                    (int)nonZero[^1],
                };
        }

        _cells?.InvalidateMeasure();
        _cells?.InvalidateVisual();
        RebuildLabels();
    }

    private void RebuildLabels()
    {
        if (_monthLabels is not null)
        {
            _monthLabels.Children.Clear();
            var step = CellSize + CellSpacing;
            foreach (var label in MonthLabels)
            {
                // No explicit foreground: the labels inherit the control's Foreground, which
                // the theme points at the on-surface-variant token, so they follow runtime
                // theme switches for free.
                var text = new TextBlock
                {
                    Text = label.Text,
                    FontSize = 11,
                };
                Canvas.SetLeft(text, label.WeekIndex * step);
                _monthLabels.Children.Add(text);
            }
        }

        if (_dayLabels is not null)
        {
            _dayLabels.Children.Clear();
            _dayLabels.Width = 34;
            var step = CellSize + CellSpacing;
            var names = CultureInfo.CurrentCulture.TextInfo;
            var rows = new (string Name, int Row)[]
            {
                (names.GetAbbreviatedDayName(DayOfWeek.Monday), 1),
                (names.GetAbbreviatedDayName(DayOfWeek.Wednesday), 3),
                (names.GetAbbreviatedDayName(DayOfWeek.Friday), 5),
            };
            foreach (var (name, row) in rows)
            {
                var text = new TextBlock
                {
                    Text = name,
                    FontSize = 11,
                    Margin = new Thickness(0, 0, 6, 0),
                };
                // Center the label on its row: the row's top edge plus half a cell.
                Canvas.SetTop(text, row * step + CellSize / 2 - 7);
                _dayLabels.Children.Add(text);
            }
        }
    }

    /// <summary>
    /// Maps a value to its display level (0-4) using the GitHub-style percentile thresholds of
    /// the current dataset.
    /// </summary>
    internal static int LevelForValue(int value, int[] thresholds)
    {
        if (value <= 0 || thresholds.Length != 4)
            return 0;
        if (value <= thresholds[0]) return 1;
        if (value <= thresholds[1]) return 2;
        if (value <= thresholds[2]) return 3;
        return 4;
    }

    private static double Quantile(IList<double> sorted, double fraction)
    {
        if (sorted.Count == 1)
            return sorted[0];
        var index = (sorted.Count - 1) * fraction;
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);
        var weight = index - lower;
        return sorted[lower] + (sorted[upper] - sorted[lower]) * weight;
    }
}

/// <summary>
/// The drawing surface of <see cref="MdContributionGrid"/>: renders one rounded cell per day and
/// translates pointer positions into days for hovering (tooltip) and clicks.
/// </summary>
internal sealed class ContributionCellsHost : Control
{
    private MdContributionGrid? _owner;
    private MdContributionDay? _hoveredDay;

    public void Attach(MdContributionGrid owner) => _owner = owner;

    protected override Size MeasureOverride(Size availableSize)
    {
        if (_owner is null || _owner.WeekCount == 0)
            return Size.Zero;

        var step = _owner.CellSize + _owner.CellSpacing;
        var width = _owner.WeekCount * step - _owner.CellSpacing;
        var height = 7 * step - _owner.CellSpacing;
        return new Size(Math.Max(width, 0), Math.Max(height, 0));
    }

    public override void Render(DrawingContext context)
    {
        var owner = _owner;
        if (owner is null || owner.WeekCount == 0)
            return;

        var cell = owner.CellSize;
        var step = cell + owner.CellSpacing;
        var brushes = owner.GetLevelBrushes();
        var thresholds = owner.Thresholds;
        var corner = Math.Min(2, cell / 2);

        for (var week = 0; week < owner.WeekCount; week++)
        {
            for (var dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
            {
                var date = owner.WeekStart.AddDays(7L * week + dayOfWeek);
                var value = owner.TryGetDayValue(date, out var stored) ? stored : 0;
                var brush = brushes[MdContributionGrid.LevelForValue(value, thresholds)];
                if (brush is null)
                    continue;
                context.DrawRectangle(
                    brush,
                    null,
                    new Rect(week * step, dayOfWeek * step, cell, cell),
                    corner,
                    corner);
            }
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var day = DayAt(e.GetPosition(this));
        if (day == _hoveredDay)
            return;

        _hoveredDay = day;
        ToolTip.SetTip(this, day is null ? null : new TextBlock
        {
            Text = $"{day.Value} on {day.Date:yyyy-MM-dd}",
            FontSize = 12,
        });
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (_hoveredDay is not null)
        {
            _hoveredDay = null;
            ToolTip.SetTip(this, null);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        if (_owner is null)
            return;
        _owner.OnDayClicked(DayAt(e.GetPosition(this)));
    }

    /// <summary>Resolves the day under a point in this host's coordinates, or null in the gaps.</summary>
    public MdContributionDay? DayAt(Point position)
    {
        var owner = _owner;
        if (owner is null || owner.WeekCount == 0 || position.X < 0 || position.Y < 0)
            return null;

        var step = owner.CellSize + owner.CellSpacing;
        var week = (int)(position.X / step);
        var dayOfWeek = (int)(position.Y / step);
        if (week >= owner.WeekCount || dayOfWeek >= 7)
            return null;
        if (position.X - week * step > owner.CellSize || position.Y - dayOfWeek * step > owner.CellSize)
            return null;

        var date = owner.WeekStart.AddDays(7L * week + dayOfWeek);
        var value = owner.TryGetDayValue(date, out var stored) ? stored : 0;
        return new MdContributionDay { Date = date, Value = value };
    }
}
