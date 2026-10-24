using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Renders a commit list as a GitHub-style commit graph: one row per commit, vertical lane
/// lines, colored nodes and curved connectors at branch and merge points.
/// </summary>
/// <remarks>
/// <para>
/// Items must be <see cref="MdCommitNode"/> in display order, newest first, like
/// <c>git log</c>. The graph column is sized from <see cref="LaneWidth"/> and the assigned
/// lane count; lane colors come from the <c>Md.CommitGraph.Lane.Brush.N</c> theme tokens
/// (up to <see cref="MaxLanes"/>), with an optional per-commit <see cref="MdCommitNode.Color"/>
/// override. <see cref="HeadId"/> rings the node of the given commit.
/// </para>
/// <para>
/// Every commit's link to its first parent is drawn solid, so the mainline and each branch's
/// own spine stand out; the parts a merge absorbs - the merge curve and the short stub above a
/// merged branch's last commit - are dashed, the way IntelliJ's commit log fades absorbed
/// branch work, which is what makes dense multi-branch histories readable.
/// </para>
/// <para>
/// Clicking a row selects that commit (<see cref="SelectedId"/>) and raises
/// <see cref="CommitClicked"/>, so a host can inspect or act on a single commit.
/// </para>
/// <para>
/// Rows are not virtualized: the graph lays every row out so lanes stay continuous, which is
/// intended for histories of a few hundred commits or fewer.
/// </para>
/// </remarks>
[PseudoClasses(":has-head")]
public class MdCommitGraph : ItemsControl
{
    public static readonly StyledProperty<string?> HeadIdProperty =
        AvaloniaProperty.Register<MdCommitGraph, string?>(nameof(HeadId));
    public static readonly StyledProperty<string?> SelectedIdProperty =
        AvaloniaProperty.Register<MdCommitGraph, string?>(nameof(SelectedId));
    public static readonly StyledProperty<double> LaneWidthProperty =
        AvaloniaProperty.Register<MdCommitGraph, double>(nameof(LaneWidth), 20);
    public static readonly StyledProperty<int> MaxLanesProperty =
        AvaloniaProperty.Register<MdCommitGraph, int>(nameof(MaxLanes), 8, validate: v => v >= 1);
    public static readonly StyledProperty<double> NodeSizeProperty =
        AvaloniaProperty.Register<MdCommitGraph, double>(nameof(NodeSize), 10);
    public static readonly DirectProperty<MdCommitGraph, int> LaneCountProperty =
        AvaloniaProperty.RegisterDirect<MdCommitGraph, int>(nameof(LaneCount), graph => graph.LaneCount);

    private IReadOnlyList<MdCommitGraphRowGeometry> _rows = Array.Empty<MdCommitGraphRowGeometry>();
    private int _laneCount;
    private IBrush?[] _laneBrushCache = Array.Empty<IBrush>();
    private ThemeVariant? _laneBrushVariant;

    static MdCommitGraph()
    {
        HeadIdProperty.Changed.AddClassHandler<MdCommitGraph>((graph, _) => graph.Recompute());
        MaxLanesProperty.Changed.AddClassHandler<MdCommitGraph>((graph, _) => graph.Recompute());
        LaneWidthProperty.Changed.AddClassHandler<MdCommitGraph>((graph, _) => graph.ApplyLayoutToContainers());
        NodeSizeProperty.Changed.AddClassHandler<MdCommitGraph>((graph, _) => graph.InvalidateRows());
        ItemTemplateProperty.Changed.AddClassHandler<MdCommitGraph>((graph, _) => graph.ApplyLayoutToContainers());
        SelectedIdProperty.Changed.AddClassHandler<MdCommitGraph>((graph, _) => graph.ApplySelection());
    }

    /// <summary>Gets or sets the identifier of the head commit; its node gets a ring.</summary>
    public string? HeadId { get => GetValue(HeadIdProperty); set => SetValue(HeadIdProperty, value); }

    /// <summary>
    /// Gets or sets the identifier of the selected commit. Set by clicking a row or
    /// programmatically; the selected row is highlighted.
    /// </summary>
    public string? SelectedId { get => GetValue(SelectedIdProperty); set => SetValue(SelectedIdProperty, value); }

    /// <summary>Raised when a commit row is clicked; the argument is that commit.</summary>
    public event EventHandler<MdCommitNode?>? CommitClicked;

    /// <summary>Gets or sets the width of one graph lane.</summary>
    public double LaneWidth { get => GetValue(LaneWidthProperty); set => SetValue(LaneWidthProperty, value); }

    /// <summary>Gets or sets how many lane colors the palette cycles through.</summary>
    public int MaxLanes { get => GetValue(MaxLanesProperty); set => SetValue(MaxLanesProperty, value); }

    /// <summary>Gets or sets the diameter of the commit node.</summary>
    public double NodeSize { get => GetValue(NodeSizeProperty); set => SetValue(NodeSizeProperty, value); }

    /// <summary>Gets the number of lanes the current commit list occupies.</summary>
    // Reads the field, not GetValue: a getter-only DirectProperty whose CLR getter calls GetValue
    // recurses through the property registry until the stack overflows.
    public int LaneCount => _laneCount;

    /// <summary>Gets the width of the graph column (lane count times <see cref="LaneWidth"/>).</summary>
    public double LaneColumnWidth => LaneCount * LaneWidth;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Recompute();
    }

    public MdCommitGraph()
    {
        // The base collection hook is private-protected and cannot be overridden from this
        // assembly, so the public items collection is watched directly.
        Items.CollectionChanged += OnItemsChanged;
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Recompute();

    /// <summary>Recomputes lanes and connectors from the current items; call after bulk data changes.</summary>
    public void Recompute()
    {
        var count = Items.Count;
        var nodes = new MdCommitNode[count];
        for (var i = 0; i < count; i++)
        {
            var item = Items[i];
            nodes[i] = item as MdCommitNode ?? new MdCommitNode
            {
                Id = $"__row_{i}__",
                Message = item?.ToString() ?? string.Empty,
            };
        }

        _rows = MdCommitGraphLayout.ComputeRows(nodes, HeadId);
        var laneCount = _rows.Count == 0 ? 0 : _rows.Max(row => row.Lane) + 1;
        SetAndRaise(LaneCountProperty, ref _laneCount, laneCount);
        PseudoClasses.Set(":has-head", HeadId is not null);
        ApplyLayoutToContainers();
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) =>
        new CommitGraphRow();

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is CommitGraphRow row)
        {
            row.Apply(index < _rows.Count ? _rows[index] : null, item, this, LaneColumnWidth);
            row.SetSelected(item is MdCommitNode { Id: var id } && id == SelectedId);
        }
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is CommitGraphRow row)
            row.ClearGeometry();
        base.ClearContainerForItemOverride(container);
    }

    /// <summary>Resolves the brush for a lane from the <c>Md.CommitGraph.Lane.Brush.N</c> tokens.</summary>
    /// <remarks>
    /// Hits are cached per (slot, theme variant): a render pass asks for one brush per lane per
    /// row, and a walk plus a lazy resource build per paint is wasted work. Misses are not
    /// cached, so a host whose theme attaches late still picks up the real tokens on the next
    /// paint instead of being frozen on the fallback.
    /// </remarks>
    public IBrush? GetLaneBrush(int lane)
    {
        var variant = ActualThemeVariant;
        var capacity = Math.Max(MaxLanes, 1);
        if (_laneBrushCache.Length != capacity || _laneBrushVariant != variant)
        {
            _laneBrushCache = new IBrush?[capacity];
            _laneBrushVariant = variant;
        }

        var slot = (lane % capacity) + 1;
        if (_laneBrushCache[slot - 1] is { } hit)
            return hit;

        var brush = MdThemeResources.FindBrush(this, $"Md.CommitGraph.Lane.Brush.{slot}", variant);
        if (brush is not null)
        {
            _laneBrushCache[slot - 1] = brush;
            return brush;
        }

        // Fallback for hosts without the Md theme loaded.
        return MdThemeResources.FindBrush(this, "Md.Sys.Color.Primary.Brush", variant)
            ?? new SolidColorBrush(Color.FromRgb(0x67, 0x50, 0xA4));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        if (e.Source is not Visual visual)
            return;
        if (visual.GetVisualAncestors().OfType<CommitGraphRow>().FirstOrDefault() is not { } row)
            return;
        var index = IndexFromContainer(row);
        if (index < 0)
            return;
        OnCommitRowClicked(index);
    }

    /// <summary>Selects the commit of the given row and raises <see cref="CommitClicked"/>.</summary>
    private void OnCommitRowClicked(int index)
    {
        var node = Items[index] as MdCommitNode;
        if (node is null)
            return;
        SelectedId = node.Id;
        CommitClicked?.Invoke(this, node);
    }

    private void ApplySelection()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            if (ContainerFromIndex(i) is CommitGraphRow row)
                row.SetSelected(Items[i] is MdCommitNode { Id: var id } && id == SelectedId);
        }
    }

    private void ApplyLayoutToContainers()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            if (ContainerFromIndex(i) is CommitGraphRow row)
                row.Apply(i < _rows.Count ? _rows[i] : null, Items[i], this, LaneColumnWidth);
        }
    }

    private void InvalidateRows()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            if (ContainerFromIndex(i) is CommitGraphRow row)
                row.InvalidateLaneHost();
        }
    }

    /// <summary>Per-row geometry for one commit row: node lane, connector lanes and curves.</summary>
    internal sealed record MdCommitGraphRowGeometry
    {
        public required int Lane { get; init; }
        public required IReadOnlyList<int> TopLanes { get; init; }
        public required IReadOnlyList<int> BottomLanes { get; init; }
        public required IReadOnlyList<(int From, int To)> Curves { get; init; }
        public bool IsHead { get; init; }

        /// <summary>The top half-strips that lie on the first-parent chain; the rest are dashed.</summary>
        public IReadOnlyCollection<int> ActiveTopLanes { get; init; } = Array.Empty<int>();

        /// <summary>The bottom half-strips that lie on the first-parent chain; the rest are dashed.</summary>
        public IReadOnlyCollection<int> ActiveBottomLanes { get; init; } = Array.Empty<int>();

        /// <summary>The first-parent curves of this row; the other curves are dashed.</summary>
        public IReadOnlyCollection<(int From, int To)> ActiveCurves { get; init; } = Array.Empty<(int, int)>();
    }

    /// <summary>
    /// Assigns lanes and connector geometry to a commit list in display order (newest first).
    /// A commit keeps its first parent's lane when it is a merge, when it is the parent's only
    /// child, or when it is the parent's newest child; every other commit takes the smallest
    /// free lane. Merged branches release their lane at the merge.
    /// </summary>
    internal static class MdCommitGraphLayout
    {
        public static IReadOnlyList<MdCommitGraphRowGeometry> ComputeRows(
            IReadOnlyList<MdCommitNode> commits,
            string? headId)
        {
            var count = commits.Count;
            var idToRow = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var i = 0; i < count; i++)
                idToRow[commits[i].Id] = i;

            // Children per parent in display order (first entry is the newest child).
            var childRows = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            for (var i = 0; i < count; i++)
            {
                foreach (var parentId in commits[i].ParentIds)
                {
                    if (!idToRow.ContainsKey(parentId))
                        continue;
                    if (!childRows.TryGetValue(parentId, out var children))
                        childRows[parentId] = children = new List<int>();
                    children.Add(i);
                }
            }

            var laneOf = new Dictionary<string, int>(StringComparer.Ordinal);
            var rowLane = new int[count];
            var live = new HashSet<int>();

            for (var r = count - 1; r >= 0; r--)
            {
                var commit = commits[r];

                string? firstParentId = null;
                var firstParentLane = -1;
                foreach (var parentId in commit.ParentIds)
                {
                    if (idToRow.TryGetValue(parentId, out _))
                    {
                        firstParentId = parentId;
                        laneOf.TryGetValue(parentId, out firstParentLane);
                        break;
                    }
                }

                int lane;
                if (firstParentId is not null && laneOf.ContainsKey(firstParentId))
                {
                    var continues = commit.ParentIds.Count >= 2;
                    if (!continues)
                    {
                        if (childRows.TryGetValue(firstParentId, out var children))
                            continues = children.Count == 1 || children[0] == r;
                        else
                            continues = true;
                    }

                    lane = continues && live.Contains(firstParentLane)
                        ? firstParentLane
                        : SmallestFree(live);
                }
                else
                {
                    lane = SmallestFree(live);
                }

                rowLane[r] = lane;
                laneOf[commit.Id] = lane;

                // A merge ends every other parent's branch at this commit.
                foreach (var parentId in commit.ParentIds)
                {
                    if (parentId == firstParentId)
                        continue;
                    if (laneOf.TryGetValue(parentId, out var parentLane) && parentLane != lane)
                        live.Remove(parentLane);
                }

                live.Add(lane);
            }

            // A lane's visual path is the union of its commit nodes (row midpoints) and its
            // curve ends: a curve leaves its source lane at the row midpoint and arrives at its
            // target lane at the row bottom. Points are kept in half-strip units (2x rows) so a
            // half-strip is drawn exactly when the path exists on both sides of it.
            var laneMin = new Dictionary<int, int>();
            var laneMax = new Dictionary<int, int>();
            void Touch(int lane, int point)
            {
                if (!laneMin.TryGetValue(lane, out var min))
                {
                    laneMin[lane] = point;
                    laneMax[lane] = point;
                    return;
                }

                if (point < min) laneMin[lane] = point;
                if (point > laneMax[lane]) laneMax[lane] = point;
            }

            var curves = new List<(int From, int To)>[count];
            for (var r = 0; r < count; r++)
            {
                curves[r] = new List<(int, int)>();
                var lane = rowLane[r];
                Touch(lane, 2 * r + 1);

                foreach (var parentId in commits[r].ParentIds)
                {
                    if (laneOf.TryGetValue(parentId, out var parentLane) && parentLane != lane)
                    {
                        curves[r].Add((lane, parentLane));
                        Touch(parentLane, 2 * r + 2);
                    }
                }
            }

            // First-parent skeleton, the IDEA look: each commit links to its first present
            // parent (the same parent the lane algorithm follows), and the segments of those
            // links are drawn solid; every other lane segment is branch continuation and stays
            // dashed, so dense histories read as one bold mainline with faded branch work.
            var activeTop = new HashSet<int>[count];
            var activeBottom = new HashSet<int>[count];
            var activeCurves = new HashSet<(int, int)>[count];
            for (var r = 0; r < count; r++)
            {
                activeTop[r] = new HashSet<int>();
                activeBottom[r] = new HashSet<int>();
                activeCurves[r] = new HashSet<(int, int)>();

                string? parentId = null;
                foreach (var pid in commits[r].ParentIds)
                {
                    if (idToRow.TryGetValue(pid, out _))
                    {
                        parentId = pid;
                        break;
                    }
                }

                if (parentId is null)
                    continue;

                var pr = idToRow[parentId];
                var lane = rowLane[r];
                var parentLane = rowLane[pr];
                if (lane == parentLane)
                {
                    // The link runs from this row's centre to the parent's centre: the bottom
                    // half-strips from this row down, the top half-strips from the row below up
                    // to the parent row's top strip (where the parent's node sits).
                    for (var row = r; row < pr; row++)
                        activeBottom[row].Add(lane);
                    for (var row = r + 1; row < pr; row++)
                        activeTop[row].Add(lane);
                    activeTop[pr].Add(lane);
                }
                else
                {
                    activeCurves[r].Add((lane, parentLane));
                    for (var row = r + 1; row < pr; row++)
                    {
                        activeTop[row].Add(parentLane);
                        activeBottom[row].Add(parentLane);
                    }
                    activeTop[pr].Add(parentLane);
                }
            }

            var result = new MdCommitGraphRowGeometry[count];
            for (var r = 0; r < count; r++)
            {
                var top = new List<int>();
                var bottom = new List<int>();
                foreach (var (lane, min) in laneMin)
                {
                    var max = laneMax[lane];
                    if (min <= 2 * r && max >= 2 * r + 1)
                        top.Add(lane);
                    if (min <= 2 * r + 1 && max >= 2 * r + 2)
                        bottom.Add(lane);
                }
                top.Sort();
                bottom.Sort();

                result[r] = new MdCommitGraphRowGeometry
                {
                    Lane = rowLane[r],
                    TopLanes = top,
                    BottomLanes = bottom,
                    Curves = curves[r],
                    IsHead = headId is not null && commits[r].Id == headId,
                    ActiveTopLanes = activeTop[r],
                    ActiveBottomLanes = activeBottom[r],
                    ActiveCurves = activeCurves[r],
                };
            }

            return result;
        }

        private static int SmallestFree(HashSet<int> taken)
        {
            var lane = 0;
            while (taken.Contains(lane))
                lane++;
            return lane;
        }
    }
}
