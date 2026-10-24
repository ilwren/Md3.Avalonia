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
        ColumnDefinitions = new ColumnDefinitions("Auto,*");
        _selectionBackground = new CommitGraphSelectionBackground
        {
            Grid.ColumnSpan = 2,
            Margin = new Thickness(-8, 0, -4, 0),
            IsHitTestVisible = false,
            Opacity = 0.55,
        };
        LaneHost.VerticalAlignment = VerticalAlignment.Stretch;
        LaneHost.MinHeight = 44;
        Grid.SetColumn(LaneHost, 0);
        ContentPresenter.HorizontalAlignment = HorizontalAlignment.Stretch;
        ContentPresenter.VerticalAlignment = VerticalAlignment.Center;
        ContentPresenter.Margin = new Thickness(8, 0, 0, 0);
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
/// ring). First-parent links are solid; the segments a merge absorbs are dashed (the IntelliJ
/// commit-log look), and the selected row gets a thicker neutral halo under its elements so a
/// single commit stands out in a dense history.
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
        var selected = _node is not null && owner.SelectedId == _node.Id;
        var haloBrush = selected
            ? MdThemeResources.FindBrush(this, "Md.Sys.Color.Outline.Brush", owner.ActualThemeVariant)
            : null;

        static double LaneX(int lane, double laneWidth) => lane * laneWidth + laneWidth / 2;

        static Pen MakePen(IBrush brush, double thickness, bool dashed, double height)
        {
            var pen = new Pen(brush, thickness);
            if (dashed)
                pen.DashStyle = new DashStyle(
                    new double[] { Math.Max(3, height * 0.25), Math.Max(2, height * 0.2) }, 0);
            return pen;
        }

        IBrush? BrushFor(int lane) => owner.GetLaneBrush(lane);

        void DrawSegment(Point from, Point to, IBrush? brush, bool active, bool curve)
        {
            if (brush is null)
                return;
            if (haloBrush is not null)
            {
                var haloPen = MakePen(haloBrush, 4, false, height);
                if (curve)
                {
                    var halo = new StreamGeometry();
                    using (var figure = halo.Open())
                    {
                        figure.BeginFigure(from, isFilled: false);
                        figure.CubicBezierTo(
                            new Point(from.X, height * 0.75),
                            new Point(to.X, height * 0.75),
                            to);
                        figure.EndFigure(false);
                    }
                    context.DrawGeometry(null, haloPen, halo);
                }
                else
                {
                    context.DrawLine(haloPen, from, to);
                }
            }

            var pen = MakePen(brush, 2, !active, height);
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
        if (haloBrush is not null)
            context.DrawEllipse(null, MakePen(haloBrush, 4, false, height), center, nodeSize / 2 + 3.5, nodeSize / 2 + 3.5);
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
