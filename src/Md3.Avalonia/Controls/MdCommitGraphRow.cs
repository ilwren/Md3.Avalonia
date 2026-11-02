using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace Md3.Avalonia.Controls;

/// <summary>
/// The item container of <see cref="MdCommitGraph"/>: a lane host column (drawn connectors and
/// node) beside the item content presenter. Internal because the control owns container
/// generation; the visuals themselves come from the graph's drawing code and theme tokens.
/// </summary>
internal sealed class CommitGraphRow : Grid
{
    private readonly CommitGraphSelectionBackground _selectionBackground;

    public CommitGraphLaneHost LaneHost { get; } = new();
    public ContentPresenter ContentPresenter { get; } = new();

    public CommitGraphRow()
    {
        // A null background makes the whole row invisible to hit testing except where its
        // text happens to be, so presses on the empty part of a row never reached the graph
        // and the commit could not be selected. Transparent keeps the row click-free of any
        // visible paint while registering its full bounds as a hit area.
        Background = Brushes.Transparent;
        ColumnDefinitions = new ColumnDefinitions("Auto,*");
        _selectionBackground = new CommitGraphSelectionBackground
        {
            Margin = new Thickness(-8, 0, -4, 0),
            IsHitTestVisible = false,
            Opacity = 0.55,
        };
        Grid.SetColumnSpan(_selectionBackground, 2);
        LaneHost.VerticalAlignment = VerticalAlignment.Stretch;
        LaneHost.MinHeight = 44;
        Grid.SetColumn(LaneHost, 0);
        ContentPresenter.HorizontalAlignment = HorizontalAlignment.Stretch;
        ContentPresenter.VerticalAlignment = VerticalAlignment.Center;
        // Keep the message block clear of the lane tree: the tree's rightmost node and the
        // selection highlight live in the lane column, so the text needs a visible gutter
        // before it starts instead of sitting against the lines.
        ContentPresenter.Margin = new Thickness(20, 0, 8, 0);
        Grid.SetColumn(ContentPresenter, 1);
        Children.Add(_selectionBackground);
        Children.Add(LaneHost);
        Children.Add(ContentPresenter);
    }

    public void Apply(MdCommitGraph.MdCommitGraphRowGeometry? geometry, object? item, MdCommitGraph graph, double laneColumnWidth)
    {
        LaneHost.Width = laneColumnWidth;
        LaneHost.Set(geometry, item as MdCommitNode, graph);
        _selectionBackground.SetOwner(graph);
        ContentPresenter.Content = item;
        ContentPresenter.ContentTemplate = graph.ItemTemplate;
    }

    public void SetSelected(bool selected)
    {
        _selectionBackground.SetSelected(selected);
        LaneHost.InvalidateVisual();
    }

    internal bool IsSelected => _selectionBackground.IsSelected;

    public void ClearGeometry()
    {
        LaneHost.Set(null, null, null);
    }

    public void InvalidateLaneHost() => LaneHost.InvalidateVisual();
}

/// <summary>
/// Draws the connectors and node of one commit row: vertical lane lines in the upper and lower
/// half-strips, cubic curves for branch/merge transitions and the commit node (plus a head
/// ring). Every link is continuous; first-parent links use full opacity while the segments a
/// merge absorbs are muted. The selected row is picked out by the soft background the row
/// paints, not by extra rings in the tree itself.
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

        void DrawSegment(Point from, Point to, IBrush? brush, bool active, bool curve)
        {
            if (brush is null)
                return;

            var pen = new Pen(brush, 2);
            void Draw()
            {
                if (curve)
                {
                    var curvePath = new StreamGeometry();
                    using (var figure = curvePath.Open())
                    {
                        figure.BeginFigure(from, isFilled: false);
                        figure.CubicBezierTo(
                            new Point(from.X, height * 0.75),
                            new Point(to.X, height * 0.75),
                            to);
                        figure.EndFigure(false);
                    }
                    context.DrawGeometry(null, pen, curvePath);
                }
                else
                {
                    context.DrawLine(pen, from, to);
                }
            }

            // A dashed pen restarts its dash phase for every row half and every curve. That can
            // leave both sides of a join blank and makes a valid branch look disconnected.
            // Keep the topology continuous and distinguish absorbed merge paths by opacity,
            // matching mature Avalonia Git clients and Flutter graph painters.
            if (active)
            {
                Draw();
            }
            else
            {
                using (context.PushOpacity(0.5))
                    Draw();
            }
        }

        foreach (var lane in geometry.TopLanes)
        {
            var x = LaneX(lane, laneWidth);
            DrawSegment(
                new Point(x, 0),
                new Point(x, mid),
                BrushFor(lane),
                geometry.ActiveTopLanes.Contains(lane),
                false);
        }

        foreach (var lane in geometry.BottomLanes)
        {
            var x = LaneX(lane, laneWidth);
            DrawSegment(
                new Point(x, mid),
                new Point(x, height),
                BrushFor(lane),
                geometry.ActiveBottomLanes.Contains(lane),
                false);
        }

        foreach (var (from, to) in geometry.Curves)
        {
            // StreamGeometry/Open is the drawing pattern the repo uses everywhere; PathGeometry
            // only rebuilds its native geometry when the Figures property is reassigned, so
            // figures added to the initial collection are never rendered.
            var start = new Point(LaneX(from, laneWidth), mid);
            var end = new Point(LaneX(to, laneWidth), height);
            DrawSegment(
                start,
                end,
                BrushFor(from),
                geometry.ActiveCurves.Contains((from, to)),
                true);
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

/// <summary>
/// The rounded highlight behind a selected commit row. Drawn rather than a bordered panel so
/// the brush is resolved from the theme tokens at paint time - the way the lane drawing does
/// it - which keeps it in step with runtime theme switches without any reference plumbing.
/// </summary>
internal sealed class CommitGraphSelectionBackground : Control
{
    private MdCommitGraph? _owner;
    private IBrush? _brush;
    private ThemeVariant? _variant;

    public bool IsSelected { get; private set; }

    public void SetOwner(MdCommitGraph owner)
    {
        _owner = owner;
        _variant = null;
        InvalidateVisual();
    }

    public void SetSelected(bool selected)
    {
        if (IsSelected == selected)
            return;
        IsSelected = selected;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        var owner = _owner;
        if (!IsSelected || owner is null || Bounds.Width <= 0)
            return;

        var variant = owner.ActualThemeVariant;
        if (_variant != variant)
        {
            _brush = MdThemeResources.FindBrush(owner, "Md.Sys.Color.SurfaceContainerHigh.Brush", variant);
            _variant = variant;
        }

        if (_brush is null)
            return;
        context.DrawRectangle(_brush, null, new Rect(0, 0, Bounds.Width, Bounds.Height), 8, 8);
    }
}
