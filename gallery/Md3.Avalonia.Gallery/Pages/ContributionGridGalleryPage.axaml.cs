using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Media;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class ContributionGridGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    // The same stack the XAML templates use; Avalonia 12 has no FontFamily.Monospace constant.
    private static readonly FontFamily Monospace =
        "Cascadia Mono,Consolas,Menlo,DejaVu Sans Mono,Liberation Mono,Noto Sans Mono,monospace";

    private static readonly string[] MessagePool =
    [
        "fix: race in theme cache invalidation",
        "feat: add ripple preset for icon buttons",
        "docs: clarify token inheritance rules",
        "chore: bump Avalonia to the latest patch",
        "refactor: split the layout probe helpers",
        "test: cover the pips pager overflow case",
        "perf: cache the lane brush lookup per theme",
        "fix: clip the commit graph selection halo",
        "feat: expose MaxVisiblePips on the pager",
        "chore: rework the CI annotation parser",
        "fix: keep dots paged on narrow windows",
        "docs: record the merge-lane algorithm",
        "refactor: hoist the shared geometry math",
        "test: pin the quantile edge cases",
        "fix: honour the reduced-motion scheme",
        "feat: add vertical peek to the dots demo",
        "chore: tidy the gallery page scaffolding",
        "perf: avoid rebuilding the legend brushes",
        "fix: stop the menu overlapping the anchor",
        "docs: refresh the flutter parity table",
        "feat: let rows resolve from their bounds",
        "refactor: move the presence timer out",
        "test: assert the click transform once",
        "fix: clamp the popup within the window",
        "chore: prune the deprecated style keys",
        "perf: batch the row invalidation calls",
        "fix: align the head ring with the node",
        "docs: note the shallow-history behaviour",
        "feat: report the full cascader path",
        "refactor: share the day-label layout pass",
    ];

    private static readonly string[] Authors = ["ilwren", "wren", "ada", "grace", "linus"];

    private readonly List<MdContributionDay> _days = [];
    private readonly Dictionary<DateTime, List<(string Hash, string Message, string Author, string Time)>> _commits = [];
    private int _seed = 20261024;

    public ContributionGridGalleryPage()
    {
        InitializeComponent();
        Generate();
        ShowSelection(null);
    }

    private void RegenerateButtonClicked(object? sender, RoutedEventArgs e)
    {
        _seed = Environment.TickCount;
        Generate();
        ShowSelection(null);
    }

    private void DemoGridDayClicked(object? sender, MdContributionDay? e)
    {
        ShowSelection(e);
    }

    private void Generate()
    {
        var random = new Random(_seed);
        var today = new DateTimeOffset(DateTime.Today);
        _days.Clear();
        _commits.Clear();

        for (var i = 364; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var weekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var weekdayFactor = weekend ? 0.4 : 1.0;
            // A couple of two-week push streaks keep the grid looking like a real year.
            var streak = i % 120 is < 20 or > 60 and < 80 ? 1.9 : 1.0;
            var value = (int)(random.Next(0, 12) * weekdayFactor * streak + random.Next(0, 3));

            // Every counted contribution is a real commit line, so a cell click can show what
            // actually happened on that day instead of just the total.
            var dayCommits = new List<(string, string, string, string)>(value);
            for (var c = 0; c < value; c++)
            {
                var hour = random.Next(8, 22);
                dayCommits.Add((
                    random.Next(1, 1 << 28).ToString("x8")[..7],
                    MessagePool[random.Next(MessagePool.Length)],
                    Authors[random.Next(Authors.Length)],
                    $"{hour:D2}:{random.Next(20, 60):D2}"));
            }

            _days.Add(new MdContributionDay { Date = date, Value = value });
            // Keyed by the local calendar date, exactly like the grid's own value map, so
            // the day a cell click reports is the same key the generation pass stored.
            _commits[date.Date] = dayCommits;
        }

        // A fresh list on each pass: the grids watch the Days property, not list mutations.
        var days = new List<MdContributionDay>(_days);
        DemoGrid.Days = days;
        CompactGrid.Days = days;

        var total = days.Sum(day => day.Value);
        var busiest = days.Aggregate((a, b) => b.Value >= a.Value ? b : a);
        StatsContent.Children.Clear();
        StatsContent.Children.Add(Line(L($"Contributions: {total:N0}", $"贡献总数：{total:N0}"), strong: true));
        StatsContent.Children.Add(Line(L($"Busiest day: {busiest.Date:yyyy-MM-dd} ({busiest.Value})", $"最活跃的一天：{busiest.Date:yyyy-MM-dd}（{busiest.Value} 次）")));
        StatsContent.Children.Add(Line(L($"Days with activity: {days.Count(d => d.Value > 0)} / 365", $"有活动天数：{days.Count(d => d.Value > 0)} / 365")));
        StatsContent.Children.Add(Line(L($"Week columns: {DemoGrid.WeekCount}", $"周列数：{DemoGrid.WeekCount}")));
    }

    private void ShowSelection(MdContributionDay? day)
    {
        SelectionContent.Children.Clear();
        if (day is null)
        {
            SelectionContent.Children.Add(Info(L("Click a day in the grid to see its commits here.", "点击网格中的某一天，在这里查看当天的提交。")));
            return;
        }

        SelectionContent.Children.Add(Line(
            L($"{day.Date:yyyy-MM-dd} - {day.Value} contributions", $"{day.Date:yyyy-MM-dd} - {day.Value} 次贡献"), strong: true));

        var commits = _commits.GetValueOrDefault(day.Date.Date, []);
        if (commits.Count == 0)
        {
            SelectionContent.Children.Add(Info(L("No commits on this day.", "这一天没有提交。")));
            return;
        }

        // Long push days can hold a dozen lines; show the first eight and count the rest so
        // the panel stays a quick glance instead of a scroll of its own.
        const int maxLines = 8;
        foreach (var (hash, message, author, time) in commits.Take(maxLines))
        {
            SelectionContent.Children.Add(CommitLine(hash, message, author, time));
        }

        if (commits.Count > maxLines)
            SelectionContent.Children.Add(Info(L($"…and {commits.Count - maxLines} more that day.", $"…当天还有 {commits.Count - maxLines} 条。")));
    }

    private TextBlock CommitLine(string hash, string message, string author, string time)
    {
        // Hashes and the author stamp use the on-surface-variant token, so the panel follows
        // runtime theme switches like the rest of the page.
        var muted = MdThemeResources.FindBrush(this, "Md.Sys.Color.OnSurfaceVariant.Brush", ActualThemeVariant);
        return new TextBlock
        {
            FontSize = 12.5,
            TextWrapping = TextWrapping.Wrap,
            // Inlines, not Runs: Avalonia's TextBlock has no Runs property, and the collection
            // must be constructed rather than initialized in place.
            Inlines = new InlineCollection
            {
                new Run(hash) { FontFamily = Monospace, Foreground = muted, FontSize = 11 },
                new Run("  " + message),
                new Run($"  ·  {author} {time}") { Foreground = muted, FontSize = 11 },
            },
        };
    }

    private static TextBlock Line(string text, bool strong = false) => new()
    {
        Text = text,
        FontSize = strong ? 15 : 13,
        FontWeight = strong ? FontWeight.SemiBold : FontWeight.Normal,
        TextWrapping = TextWrapping.Wrap,
    };

    private static TextBlock Info(string text) => new()
    {
        Text = text,
        FontSize = 13,
        TextWrapping = TextWrapping.Wrap,
    };
}
