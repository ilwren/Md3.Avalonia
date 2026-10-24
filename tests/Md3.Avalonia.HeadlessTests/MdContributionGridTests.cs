using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdContributionGridTests
{
    private static MdContributionDay Day(DateTime date, int value) => new()
    {
        Date = new DateTimeOffset(date),
        Value = value,
    };

    [AvaloniaFact]
    public void Weeks_Run_From_The_First_Partial_Week_To_The_Last_With_Month_Marks()
    {
        var grid = new MdContributionGrid
        {
            Days = Enumerable.Range(0, 43)
                .Select(i => Day(new DateTime(2025, 1, 6).AddDays(i), i % 5))
                .ToList(),
        };

        // 2025-01-06 is a Monday: the first column starts on Sunday 2025-01-05 and the data
        // runs to 2025-02-16, 42 days later, so seven week columns.
        Assert.Equal(7, grid.WeekCount);
        Assert.Equal(new DateTime(2025, 1, 5), grid.WeekStart);
        Assert.Equal(new[] { ("Jan", 0), ("Feb", 3) },
            grid.MonthLabels.Select(l => (l.Text, l.WeekIndex)).ToArray());
    }

    [AvaloniaFact]
    public void Levels_Split_The_Non_Zero_Values_At_The_Quantiles()
    {
        var grid = new MdContributionGrid
        {
            Days = Enumerable.Range(1, 100).Select(i => Day(new DateTime(2025, 3, 1).AddDays(i % 40), i)).ToList(),
        };

        Assert.Equal(new[] { 28, 50, 76, 100 }, grid.Thresholds);
        Assert.Equal(0, MdContributionGrid.LevelForValue(0, grid.Thresholds));
        Assert.Equal(1, MdContributionGrid.LevelForValue(28, grid.Thresholds));
        Assert.Equal(2, MdContributionGrid.LevelForValue(29, grid.Thresholds));
        Assert.Equal(2, MdContributionGrid.LevelForValue(50, grid.Thresholds));
        Assert.Equal(3, MdContributionGrid.LevelForValue(51, grid.Thresholds));
        Assert.Equal(3, MdContributionGrid.LevelForValue(76, grid.Thresholds));
        Assert.Equal(4, MdContributionGrid.LevelForValue(77, grid.Thresholds));
        Assert.Equal(4, MdContributionGrid.LevelForValue(100, grid.Thresholds));
    }

    [AvaloniaFact]
    public void Days_Outside_The_Present_Data_Read_As_Zero()
    {
        var grid = new MdContributionGrid
        {
            Days = new[] { Day(new DateTime(2025, 6, 2), 5) }, // a Monday
        };

        Assert.True(grid.TryGetDayValue(new DateTime(2025, 6, 2), out var present));
        Assert.Equal(5, present);
        Assert.False(grid.TryGetDayValue(new DateTime(2025, 6, 3), out var missing));
    }

    [AvaloniaFact]
    public void Day_At_Resolves_The_Cell_Under_The_Point()
    {
        var grid = new MdContributionGrid
        {
            Days = new[] { Day(new DateTime(2025, 6, 2), 5) }, // Monday of week 0
        };
        var host = new ContributionCellsHost();
        host.Attach(grid);

        // Step is 12 + 3 = 15; Monday sits on row 1, so its cell spans y in [15, 27).
        var monday = host.DayAt(new Point(2, 15 + 2));
        Assert.NotNull(monday);
        Assert.Equal(new DateTime(2025, 6, 2), monday!.Date.Date);
        Assert.Equal(5, monday.Value);

        // A day the data does not mention still resolves, with value zero.
        var tuesday = host.DayAt(new Point(2, 30 + 2));
        Assert.NotNull(tuesday);
        Assert.Equal(new DateTime(2025, 6, 3), tuesday!.Date.Date);
        Assert.Equal(0, tuesday.Value);

        // The spacing gap between cells is empty.
        Assert.Null(host.DayAt(new Point(2, 14.5)));
        Assert.Null(host.DayAt(new Point(14.5, 16)));
        Assert.Null(host.DayAt(new Point(30, 16)));   // beyond the single week column
        Assert.Null(host.DayAt(new Point(2, 100)));   // beyond the seven rows
        Assert.Null(host.DayAt(new Point(-1, 16)));
    }

    [AvaloniaFact]
    public void Clicking_A_Cell_Raises_Day_Clicked_With_That_Day()
    {
        var grid = new MdContributionGrid
        {
            Width = 500,
            Height = 220,
            Days = new[] { Day(new DateTime(2025, 6, 2), 5) },
        };
        MdContributionDay? clicked = null;
        grid.DayClicked += (_, day) => clicked = day;

        using var host = ShowGrid(grid);
        var cells = host.Window.FindNamedChild<ContributionCellsHost>("PART_Cells");
        Assert.NotNull(cells);

        var windowPoint = new Point(2, 17).Transform(cells!.TransformToVisual(host.Window));
        host.Window.MouseDown(windowPoint, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        host.Window.MouseUp(windowPoint, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.NotNull(clicked);
        Assert.Equal(new DateTime(2025, 6, 2), clicked!.Date.Date);
        Assert.Equal(5, clicked.Value);
    }

    [AvaloniaFact]
    public void Level_Brushes_Come_From_The_Theme_Tokens()
    {
        var grid = new MdContributionGrid
        {
            Days = new[] { Day(new DateTime(2025, 6, 2), 5) },
        };
        using var host = ShowGrid(grid);

        var brushes = grid.GetLevelBrushes();
        Assert.Equal(5, brushes.Length);
        Assert.All(brushes, b => Assert.NotNull(b));
        Assert.NotSame(brushes[0], brushes[4]);
    }

    private static GridHost ShowGrid(MdContributionGrid grid)
    {
        var window = new Window
        {
            Width = 640,
            Height = 480,
            Content = new Border { Padding = new Thickness(24), Child = grid },
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new GridHost(window);
    }

    private sealed class GridHost(Window window) : IDisposable
    {
        public Window Window { get; } = window;

        public void Dispose() => window.Close();
    }
}
