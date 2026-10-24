using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
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
    public void First_Parent_Links_Are_Solid_Merge_Absorbed_Segments_Are_Dashed()
    {
        // m merges branch b into main:
        //   row 0: m  = a2 + b1   (merge)
        //   row 1: b1 = b0
        //   row 2: a2 = a1
        //   row 3: b0 = a1        (fork)
        //   row 4: a1 = a0
        //   row 5: a0             (root)
        var rows = MdCommitGraph.MdCommitGraphLayout.ComputeRows(new[]
        {
            Node("m", "a2", "b1"),
            Node("b1", "b0"),
            Node("a2", "a1"),
            Node("b0", "a1"),
            Node("a1", "a0"),
            Node("a0"),
        }, headId: "m");

        // The mainline (a0..m, lane 0) is a first-parent chain: solid everywhere it is drawn,
        // including through the fork row (row 3) where the other lane's node sits.
        Assert.Contains(0, rows[0].ActiveBottomLanes);
        Assert.Contains(0, rows[1].ActiveBottomLanes);
        Assert.Contains(0, rows[2].ActiveTopLanes);
        Assert.Contains(0, rows[3].ActiveTopLanes);
        Assert.Contains(0, rows[3].ActiveBottomLanes);
        Assert.Contains(0, rows[4].ActiveBottomLanes);
        Assert.Contains(0, rows[5].ActiveTopLanes);

        // Each branch commit links to its first parent solidly, including the fork curve
        // from b0 (lane 1) back to main (lane 0).
        Assert.Contains((1, 0), rows[3].ActiveCurves);
        Assert.Contains(1, rows[2].ActiveTopLanes);
        Assert.Contains(1, rows[2].ActiveBottomLanes);
        Assert.Contains(1, rows[3].ActiveTopLanes);

        // What the merge absorbs is dashed: the merge curve at row 0, and the short stub of
        // the merged branch's lane above its last commit (row 1).
        Assert.Contains((0, 1), rows[0].Curves);
        Assert.DoesNotContain((0, 1), rows[0].ActiveCurves);
        Assert.Contains(1, rows[1].TopLanes);
        Assert.DoesNotContain(1, rows[1].ActiveTopLanes);
    }

    [AvaloniaFact]
    public void Clicking_A_Row_Selects_The_Commit_And_Raises_The_Event()
    {
        var graph = new MdCommitGraph
        {
            ItemsSource = new[] { Node("a"), Node("b", "a"), Node("c", "b") },
        };
        MdCommitNode? clicked = null;
        graph.CommitClicked += (_, node) => clicked = node;

        using var host = Show(graph);

        var rows = new[]
        {
            (CommitGraphRow)graph.ContainerFromIndex(0)!,
            (CommitGraphRow)graph.ContainerFromIndex(1)!,
            (CommitGraphRow)graph.ContainerFromIndex(2)!,
        };

        static string Snap(MdCommitGraph g, CommitGraphRow[] rs)
        {
            var rects = rs.Select(r => $"{r.Bounds.X:F0},{r.Bounds.Y:F0},{r.Bounds.Width:F0}x{r.Bounds.Height:F0}");
            return $"laneW={g.LaneWidth} lanes={g.LaneCount} graph={g.Bounds.Width:F0}x{g.Bounds.Height:F0} rows=[{string.Join(" | ", rects)}]";
        }

        var before = Snap(graph, rows);
        var row = rows[1];
        var transform = row.TransformToVisual(host.Window) ?? Matrix.Identity;
        var bounds = row.Bounds.TransformToAABB(transform);
        var point = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        var afterDown = Snap(graph, rows);
        host.Window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        var afterUp = Snap(graph, rows);

        if (graph.SelectedId != "b")
            throw new Xunit.Sdk.XunitException(
                $"click at {point} selected '{graph.SelectedId ?? "<null>"}' instead of 'b'\n" +
                $"before:    {before}\nafterDown: {afterDown}\nafterUp:   {afterUp}");
        Assert.NotNull(clicked);
        Assert.Equal("b", clicked!.Id);
        Assert.True(row.IsSelected, "the clicked row must be highlighted");
        Assert.False(((CommitGraphRow)graph.ContainerFromIndex(0)!).IsSelected);
    }

    [AvaloniaFact]
    public void Selecting_The_Commit_Programmatically_Highlights_Its_Row()
    {
        var graph = new MdCommitGraph
        {
            ItemsSource = new[] { Node("a"), Node("b", "a") },
        };
        using var host = Show(graph);

        graph.SelectedId = "a";
        Dispatcher.UIThread.RunJobs();

        Assert.True(((CommitGraphRow)graph.ContainerFromIndex(0)!).IsSelected);
        Assert.False(((CommitGraphRow)graph.ContainerFromIndex(1)!).IsSelected);

        graph.SelectedId = null;
        Dispatcher.UIThread.RunJobs();
        Assert.False(((CommitGraphRow)graph.ContainerFromIndex(0)!).IsSelected);
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

    private static Host Show(MdCommitGraph graph) => new(ShowWindow(graph), graph);

    private static Window ShowWindow(Control content)
    {
        var window = new Window { Width = 640, Height = 480, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private sealed class Host : IDisposable
    {
        private readonly Window _window;

        public Host(Window window, MdCommitGraph graph)
        {
            _window = window;
            Graph = graph;
        }

        public Window Window => _window;
        public MdCommitGraph Graph { get; }

        public void Dispose() => _window.Close();
    }
}
