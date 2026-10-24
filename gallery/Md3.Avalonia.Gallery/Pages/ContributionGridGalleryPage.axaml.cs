using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class ContributionGridGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    private readonly List<MdContributionDay> _days = [];
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

        for (var i = 364; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var weekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var weekdayFactor = weekend ? 0.4 : 1.0;
            // A couple of two-week push streaks keep the grid looking like a real year.
            var streak = i % 120 is < 20 or > 60 and < 80 ? 1.9 : 1.0;
            var value = (int)(random.Next(0, 12) * weekdayFactor * streak + random.Next(0, 3));
            _days.Add(new MdContributionDay { Date = date, Value = value });
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
            SelectionContent.Children.Add(Info(L("Click a day in the grid to see its value here.", "点击网格中的某一天，在这里查看它的数值。")));
            return;
        }

        SelectionContent.Children.Add(Info(day.Value == 0
            ? L($"{day.Date:yyyy-MM-dd} - no contributions", $"{day.Date:yyyy-MM-dd} - 没有贡献")
            : L($"{day.Date:yyyy-MM-dd} - {day.Value} contributions", $"{day.Date:yyyy-MM-dd} - {day.Value} 次贡献")));
        SelectionContent.Children.Add(Info(L($"Weekday: {day.Date.DayOfWeek}", $"星期：{day.Date.DayOfWeek}")));
        SelectionContent.Children.Add(Info(L($"Month: {day.Date:MMMM yyyy}", $"月份：{day.Date:MMMM yyyy}")));
    }

    private static TextBlock Line(string text, bool strong = false) => new()
    {
        Text = text,
        FontSize = strong ? 15 : 13,
        FontWeight = strong ? FontWeight.SemiBold : FontWeight.Normal,
        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
    };

    private static TextBlock Info(string text) => new()
    {
        Text = text,
        FontSize = 13,
        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
    };
}
