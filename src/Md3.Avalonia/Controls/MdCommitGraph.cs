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
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

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
/// Rows are not virtualized: the graph lays every row out so lanes stay continuous, which is
/// intended for histories of a few hundred commits or fewer.
/// </para>
/// </remarks>
[PseudoClasses(":has-head")]
public class MdCommitGraph : ItemsControl
{
    public static readonly StyledProperty<string?> HeadIdProperty =
        AvaloniaProperty.Register<MdCommitGraph, string?>(nameof(HeadId));
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

    static MdCommitGraph()
    {
        HeadIdProperty.Changed.AddClassHandler<MdCommitGraph>((graph, _) => graph.Recompute());
        MaxLanesProperty.Changed.AddClassHandler<MdCommitGraph>((graph, _) => graph.Recompute());
        LaneWidthProperty.Changed.AddClassHandler<MdCommitGraph>((graph, _) => graph.ApplyLayoutToContainers());
        NodeSizeProperty.Changed.AddClassHandler<MdCommitGraph>((graph, _) => graph.InvalidateRows());
        ItemTemplateProperty.Changed.AddClassHandler<MdCommitGraph>((graph, _) => graph.ApplyLayoutToContainers());
    }

    /// <summary>Gets or sets the identifier of the head commit; its node gets a ring.</summary>
    public string? HeadId { get => GetValue(HeadIdProperty); set => SetValue(HeadIdProperty, value); }

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
            row.Apply(index < _rows.Count ? _rows[index] : null, item, this, LaneColumnWidth);
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is CommitGraphRow row)
            row.ClearGeometry();
        base.ClearContainerForItemOverride(container);
    }

    /// <summary>Resolves the brush for a lane from the <c>Md.CommitGraph.Lane.Brush.N</c> tokens.</summary>
    public IBrush? GetLaneBrush(int lane)
    {
        var slot = MaxLanes <= 0 ? 1 : (lane % MaxLanes) + 1;
        var key = $"Md.CommitGraph.Lane.Brush.{slot}";
        if (this.FindResource(key) is IBrush brush)
            return brush;
        // Fallback for hosts without the Md theme loaded.
        return this.FindResource("Md.Sys.Color.Primary.Brush") as IBrush
            ?? new SolidColorBrush(Color.FromRgb(0x67, 0x50, 0xA4));
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
