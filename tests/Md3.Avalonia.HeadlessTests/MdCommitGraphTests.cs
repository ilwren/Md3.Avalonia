using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdCommitGraphTests
{
    [AvaloniaFact]
    public void Linear_History_Uses_A_Single_Lane()
    {
        var rows = MdCommitGraph.MdCommitGraphLayout.ComputeRows(
        [
            Node("c", "b"),
            Node("b", "a"),
            Node("a"),
        ], headId: null);

        Assert.Equal(3, rows.Count);
        Assert.All(rows, row => Assert.Equal(0, row.Lane));
        Assert.All(rows, row => Assert.Empty(row.Curves));
        Assert.Empty(rows[0].TopLanes);
        Assert.Equal(new[] { 0 }, rows[1].TopLanes);
        Assert.Equal(new[] { 0 }, rows[1].BottomLanes);
        Assert.Empty(rows[2].BottomLanes);
    }

    [AvaloniaFact]
    public void Branch_Takes_A_New_Lane_And_Curves_Back_To_Its_Parent()
    {
        // Mainline a -> b -> c; side branch d off b. Display order is newest first.
        var rows = MdCommitGraph.MdCommitGraphLayout.ComputeRows(
        [
            Node("d", "b"),
            Node("c", "b"),
            Node("b", "a"),
            Node("a"),
        ], headId: null);

        Assert.Equal(0, rows[0].Lane); // d keeps b's lane as b's newest child
        Assert.Equal(1, rows[1].Lane); // c opens a new lane
        Assert.Equal(0, rows[2].Lane);
        Assert.Equal(0, rows[3].Lane);

        Assert.Empty(rows[0].Curves);
        Assert.Equal(new[] { (1, 0) }, rows[1].Curves); // c curves from its lane (1) back to b's lane (0)
        Assert.All(rows.Skip(2), row => Assert.Empty(row.Curves));
    }

    [AvaloniaFact]
    public void Merge_Stays_On_The_First_Parents_Lane_And_Releases_The_Branch()
    {
        var rows = MdCommitGraph.MdCommitGraphLayout.ComputeRows(
        [
            Node("m", "a2", "b1"),
            Node("b1", "a2"),
            Node("a2", "a1"),
            Node("a1", "a0"),
            Node("a0"),
        ], headId: null);

        Assert.Equal(0, rows[0].Lane);
        Assert.Equal(1, rows[1].Lane);
        Assert.Equal(0, rows[2].Lane);
        Assert.Equal(0, rows[3].Lane);
        Assert.Equal(0, rows[4].Lane);

        Assert.Equal(new[] { (0, 1) }, rows[0].Curves); // the branch curves into the merge
        Assert.Equal(new[] { (1, 0) }, rows[1].Curves); // the branch forks off the mainline
        Assert.All(rows.Skip(2), row => Assert.Empty(row.Curves));
        // Row 1 connects the merge curve to the branch node, so lane 1 lines its top half only.
        Assert.Contains(1, rows[1].TopLanes);
        Assert.DoesNotContain(1, rows[1].BottomLanes);
        // Below the branch commit the lane is released: nothing further draws on lane 1.
        Assert.All(rows.Skip(2), row => Assert.DoesNotContain(1, row.TopLanes));
        Assert.All(rows.Skip(2), row => Assert.DoesNotContain(1, row.BottomLanes));
    }

    [AvaloniaFact]
    public void Head_Only_Rings_The_Given_Commit()
    {
        var rows = MdCommitGraph.MdCommitGraphLayout.ComputeRows(
        [
            Node("m", "a2", "b1"),
            Node("b1", "a2"),
            Node("a2", "a1"),
            Node("a1"),
        ], headId: "m");

        Assert.True(rows[0].IsHead);
        Assert.All(rows.Skip(1), row => Assert.False(row.IsHead));
    }

    [AvaloniaFact]
    public void Control_Exposes_Lane_Count_Head_State_And_Containers()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var graph = new MdCommitGraph { HeadId = "m" };
        graph.ItemsSource = new MdCommitNode[]
        {
            Node("m", "a2", "b1"),
            Node("b1", "a2"),
            Node("a2", "a1"),
            Node("a1"),
        };
        using var host = Show(graph);

        Assert.Equal(2, graph.LaneCount);
        Assert.Equal(graph.LaneCount * graph.LaneWidth, graph.LaneColumnWidth);
        Assert.Contains(":has-head", graph.Classes);
        Assert.IsType<CommitGraphRow>(graph.ContainerFromIndex(0));
        Assert.NotNull(graph.GetLaneBrush(0));

        graph.HeadId = null;
        Assert.DoesNotContain(":has-head", graph.Classes);
    }

    [AvaloniaFact]
    public void Lane_Brushes_Cycle_Through_MaxLanes()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var graph = new MdCommitGraph { MaxLanes = 2, LaneWidth = 20, NodeSize = 10 };
        graph.ItemsSource = new MdCommitNode[]
        {
            Node("c", "b"),
            Node("b", "a"),
            Node("a"),
        };
        using var host = Show(graph);

        var laneZero = graph.GetLaneBrush(0);
        var laneOne = graph.GetLaneBrush(1);
        var laneTwo = graph.GetLaneBrush(2);
        Assert.NotNull(laneZero);
        Assert.NotNull(laneOne);
        Assert.NotNull(laneTwo);
        // Compare colors, not instances: FindResource is free to hand out a fresh brush object
        // each call, and the cache that used to keep them identical gets rebuilt when other
        // theme consumers (the gallery pages) load in the same process.
        Assert.NotEqual(((SolidColorBrush)laneOne!).Color, ((SolidColorBrush)laneZero!).Color);
        Assert.Equal(((SolidColorBrush)laneZero!).Color, ((SolidColorBrush)laneTwo!).Color); // 2 % 2 + 1 wraps back to the first slot
    }

    [AvaloniaFact]
    public void Non_Node_Items_Fall_Back_To_A_Synthetic_Id()
    {
        var graph = new MdCommitGraph();
        graph.ItemsSource = new object?[] { "just a string", Node("a") };
        using var host = Show(graph);

        Assert.Equal(2, graph.LaneCount);
    }

    [AvaloniaFact]
    public void Date_Converter_Uses_The_Short_Form_Within_The_Current_Year()
    {
        var converter = new MdCommitGraphDateTextConverter();
        var thisYear = new DateTimeOffset(DateTime.Today.Year, 3, 14, 12, 0, 0, TimeSpan.Zero);

        Assert.Equal("Mar 14", converter.Convert(thisYear, typeof(string), null, CultureInfo.InvariantCulture));
        Assert.Equal("Mar 14, 2019", converter.Convert(new DateTimeOffset(2019, 3, 14, 12, 0, 0, TimeSpan.Zero), typeof(string), null, CultureInfo.InvariantCulture));
        Assert.Null(converter.Convert("not a date", typeof(string), null, CultureInfo.InvariantCulture));
    }

    private static MdCommitNode Node(string id, params string[] parents) => new()
    {
        Id = id,
        Message = $"commit {id}",
        ParentIds = parents,
    };

    private static IDisposable Show(Control content) => new WindowScope(ShowWindow(content));

    private static Window ShowWindow(Control content)
    {
        var window = new Window { Width = 640, Height = 480, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private sealed class WindowScope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }
}
