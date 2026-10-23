using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Visuals.Platform;

namespace Md3.Avalonia.Controls;

/// <summary>
/// The item container of <see cref="MdCommitGraph"/>: a lane host column (drawn connectors and
/// node) beside the item content presenter. Internal because the control owns container
/// generation; the visuals themselves come from the graph's drawing code and theme tokens.
/// </summary>
internal sealed class CommitGraphRow : Grid
{
    public CommitGraphLaneHost LaneHost { get; } = new();
    public ContentPresenter ContentPresenter { get; } = new();

    public CommitGraphRow()
    {
        ColumnDefinitions = new ColumnDefinitions("Auto,*");
        LaneHost.VerticalAlignment = VerticalAlignment.Stretch;
        LaneHost.MinHeight = 44;
        Grid.SetColumn(LaneHost, 0);
        ContentPresenter.HorizontalAlignment = HorizontalAlignment.Stretch;
        ContentPresenter.VerticalAlignment = VerticalAlignment.Center;
        ContentPresenter.Margin = new Thickness(8, 0, 0, 0);
        Grid.SetColumn(ContentPresenter, 1);
        Children.Add(LaneHost);
        Children.Add(ContentPresenter);
    }

    public void Apply(MdCommitGraph.MdCommitGraphRowGeometry? geometry, object? item, MdCommitGraph graph, double laneColumnWidth)
    {
        LaneHost.Width = laneColumnWidth;
        LaneHost.Set(geometry, item as MdCommitNode, graph);
        ContentPresenter.Content = item;
        ContentPresenter.ContentTemplate = graph.ItemTemplate;
    }

    public void ClearGeometry()
    {
        LaneHost.Set(null, null, null);
    }

    public void InvalidateLaneHost() => LaneHost.InvalidateVisual();
}

/// <summary>
/// Draws the connectors and node of one commit row: vertical lane lines in the upper and lower
/// half-strips, cubic curves for branch/merge transitions and the commit node (plus a head ring).
/// </summary>
internal sealed class CommitGraphLaneHost : Control
{
    private MdCommitGraph.MdCommitGraphRowGeometry? _geometry;
    private MdCommitNode? _node;
    private MdCommitGraph? _owner;

    public void Set(MdCommitGraph.MdCommitGraphRowGeometry? geometry, MdCommitNode? node, MdCommitGraph? owner)
    {
        _geometry = geometry;
        _node = node;
        _owner = owner;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        if (_geometry is not { } geometry || _owner is not { } owner || Bounds.Height <= 0)
            return;

        var laneWidth = owner.LaneWidth;
        var nodeSize = owner.NodeSize;
        var height = Bounds.Height;
        var mid = height / 2;

        static double LaneX(int lane, double laneWidth) => lane * laneWidth + laneWidth / 2;

        IBrush? BrushFor(int lane) => owner.GetLaneBrush(lane);

        foreach (var lane in geometry.TopLanes)
        {
            var brush = BrushFor(lane);
            if (brush is null) continue;
            context.DrawLine(new Pen(brush, 2), new Point(LaneX(lane, laneWidth), 0), new Point(LaneX(lane, laneWidth), mid));
        }

        foreach (var lane in geometry.BottomLanes)
        {
            var brush = BrushFor(lane);
            if (brush is null) continue;
            context.DrawLine(new Pen(brush, 2), new Point(LaneX(lane, laneWidth), mid), new Point(LaneX(lane, laneWidth), height));
        }

        foreach (var (from, to) in geometry.Curves)
        {
            var brush = BrushFor(from);
            if (brush is null) continue;
            var start = new Point(LaneX(from, laneWidth), mid);
            var end = new Point(LaneX(to, laneWidth), height);
            var path = new PathGeometry();
            using (var builder = new PathGeometryContext(path))
            {
                builder.BeginFigure(start, isFilled: false);
                builder.CubicBezierTo(new Point(start.X, height * 0.75), new Point(end.X, height * 0.75), end);
                builder.EndFigure(false);
            }

            context.DrawGeometry(null, new Pen(brush, 2), path);
        }

        var nodeBrush = _node?.Color ?? BrushFor(geometry.Lane);
        if (nodeBrush is null)
            return;

        var center = new Point(LaneX(geometry.Lane, laneWidth), mid);
        if (geometry.IsHead)
            context.DrawEllipse(null, new Pen(nodeBrush, 2), center, nodeSize / 2 + 3, nodeSize / 2 + 3);
        context.DrawEllipse(nodeBrush, null, center, nodeSize / 2, nodeSize / 2);
    }
}
