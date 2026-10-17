using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>
/// A masonry layout: children are placed into columns, each one going into whichever column is
/// currently shortest, so items of differing heights pack without leaving ragged gaps.
/// </summary>
/// <remarks>
/// <para>
/// This is the greedy shortest-column placement the web calls masonry and Android calls a staggered
/// grid. It is not an optimiser: it never reorders children to find the shortest possible result.
/// That is the right trade for a feed, where the order is the content and reordering it to save a
/// few pixels would misrepresent it.
/// </para>
/// <para>
/// The column count is either explicit through <see cref="Columns"/> or derived from
/// <see cref="MinColumnWidth"/>, so the same markup gives one column on a phone and five on a
/// desktop without a breakpoint. Derived counts also never exceed the number of children, which
/// keeps a two-item feed from reserving five empty columns.
/// </para>
/// </remarks>
public class MdWaterfallPanel : Panel
{
    private readonly Dictionary<Control, Rect> _placed = new();
    private int _columns;

    public static readonly StyledProperty<int> ColumnsProperty =
        AvaloniaProperty.Register<MdWaterfallPanel, int>(nameof(Columns));
    public static readonly StyledProperty<double> MinColumnWidthProperty =
        AvaloniaProperty.Register<MdWaterfallPanel, double>(nameof(MinColumnWidth), 220);
    public static readonly StyledProperty<double> ColumnSpacingProperty =
        AvaloniaProperty.Register<MdWaterfallPanel, double>(nameof(ColumnSpacing), 12);
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<MdWaterfallPanel, double>(nameof(ItemSpacing), 12);
    public static readonly StyledProperty<bool> AnimateLayoutProperty =
        AvaloniaProperty.Register<MdWaterfallPanel, bool>(nameof(AnimateLayout), true);

    static MdWaterfallPanel() => AffectsMeasure<MdWaterfallPanel>(
        ColumnsProperty, MinColumnWidthProperty, ColumnSpacingProperty, ItemSpacingProperty);

    /// <summary>An explicit column count. Zero, the default, derives the count from <see cref="MinColumnWidth"/>.</summary>
    public int Columns { get => GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }

    /// <summary>The narrowest a column may be when the count is derived. Ignored once <see cref="Columns"/> is set.</summary>
    public double MinColumnWidth { get => GetValue(MinColumnWidthProperty); set => SetValue(MinColumnWidthProperty, value); }

    /// <summary>Horizontal space between columns.</summary>
    public double ColumnSpacing { get => GetValue(ColumnSpacingProperty); set => SetValue(ColumnSpacingProperty, value); }

    /// <summary>Vertical space between items in the same column.</summary>
    public double ItemSpacing { get => GetValue(ItemSpacingProperty); set => SetValue(ItemSpacingProperty, value); }

    /// <summary>
    /// Whether children glide to their new places when the column count changes.
    /// </summary>
    /// <remarks>
    /// Only a change of column count is animated. A resize that keeps the count has the children
    /// tracking it frame by frame already, and starting a transition on every one of those frames
    /// would restart it before it finished, leaving each child permanently lagging the pointer.
    /// </remarks>
    public bool AnimateLayout { get => GetValue(AnimateLayoutProperty); set => SetValue(AnimateLayoutProperty, value); }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count == 0) return default;

        var columns = ResolveColumns(availableSize.Width);
        var columnWidth = ColumnWidth(availableSize.Width, columns);
        var heights = new double[columns];

        foreach (var child in Children)
        {
            child.Measure(new Size(columnWidth, availableSize.Height));
            var column = Shortest(heights);
            heights[column] += child.DesiredSize.Height + ItemSpacing;
        }

        var height = 0d;
        foreach (var value in heights) height = Math.Max(height, value);

        var width = double.IsInfinity(availableSize.Width)
            ? columns * columnWidth + Math.Max(0, columns - 1) * ColumnSpacing
            : availableSize.Width;

        // Every item was given trailing space; the tallest column does not need the last one.
        return new Size(width, Math.Max(0, height - ItemSpacing));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count == 0) return finalSize;

        var columns = ResolveColumns(finalSize.Width);
        var columnWidth = ColumnWidth(finalSize.Width, columns);
        var heights = new double[columns];
        var rtl = FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft;

        var reflowed = AnimateLayout && _columns > 0 && columns != _columns && CanAnimate;
        var from = reflowed ? new Dictionary<Control, Rect>(_placed) : null;
        _columns = columns;

        var to = new Dictionary<Control, Rect>(Children.Count);
        foreach (var child in Children)
        {
            var column = Shortest(heights);
            var x = column * (columnWidth + ColumnSpacing);
            if (rtl) x = finalSize.Width - x - columnWidth;
            var rect = new Rect(x, heights[column], columnWidth, child.DesiredSize.Height);
            child.Arrange(rect);
            to[child] = rect;
            heights[column] += child.DesiredSize.Height + ItemSpacing;
        }

        _placed.Clear();
        foreach (var pair in to) _placed[pair.Key] = pair.Value;

        if (from is not null)
        {
            foreach (var pair in to)
            {
                if (!from.TryGetValue(pair.Key, out var was)) continue;
                var dx = pair.Value.X - was.X;
                var dy = pair.Value.Y - was.Y;
                // A child that did not move - and one that arrived after the previous pass, so has no
                // previous place to come from - is left alone rather than flown in from nowhere.
                if (Math.Abs(dx) < 0.5 && Math.Abs(dy) < 0.5) continue;
                Glide(pair.Key, dx, dy);
            }
        }

        return finalSize;
    }

    /// <summary>
    /// Carries a child from where it was to where the layout has just put it. This is the same
    /// first-last-invert-play shape an implicit animation uses: put the child back by the difference,
    /// attach the transition, then let go. Setting the transform before the transition is attached is
    /// what makes the second assignment animate instead of snapping.
    /// </summary>
    /// <remarks>
    /// This writes the child's <c>RenderTransform</c> and <c>Transitions</c>, which a host that
    /// animates its own children will want to know about; turning <see cref="AnimateLayout"/> off
    /// leaves both untouched.
    /// </remarks>
    private void Glide(Control child, double dx, double dy)
    {
        child.RenderTransform = new TranslateTransform(-dx, -dy);
        child.Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateTransform(this, RenderTransformProperty));
        child.RenderTransform = null;
    }

    private bool CanAnimate
    {
        get
        {
            var scheme = MdMotion.GetScheme(this);
            return scheme != MdMotionScheme.None && scheme != MdMotionScheme.Reduced;
        }
    }

    private int ResolveColumns(double width)
    {
        if (Columns > 0) return Columns;
        // An unbounded width gives nothing to divide, so a derived layout falls back to one column
        // rather than guessing; set Columns explicitly when the panel is scrolled horizontally.
        if (double.IsInfinity(width)) return 1;
        var thatFit = (int)Math.Floor((width + ColumnSpacing) / Math.Max(1, MinColumnWidth + ColumnSpacing));
        return Math.Max(1, Math.Min(thatFit, Children.Count));
    }

    private double ColumnWidth(double width, int columns)
    {
        if (double.IsInfinity(width)) return Math.Max(1, MinColumnWidth);
        return Math.Max(0, (width - Math.Max(0, columns - 1) * ColumnSpacing) / columns);
    }

    /// <summary>The first column at the minimum height, so ties fill left to right deterministically.</summary>
    private static int Shortest(double[] heights)
    {
        var index = 0;
        for (var i = 1; i < heights.Length; i++)
        {
            if (heights[i] < heights[index]) index = i;
        }
        return index;
    }
}

/// <summary>
/// An items control whose panel is a <see cref="MdWaterfallPanel"/>, for a masonry feed bound to a
/// collection rather than one written out by hand.
/// </summary>
/// <remarks>
/// The layout properties live here and the theme's panel reads them, so a feed configures itself
/// without reaching into an <c>ItemsPanelTemplate</c>. Replacing <c>ItemsPanel</c> with something
/// else is supported and simply leaves these four properties unused.
/// </remarks>
public class MdWaterfall : ItemsControl
{
    public static readonly StyledProperty<int> ColumnsProperty =
        AvaloniaProperty.Register<MdWaterfall, int>(nameof(Columns));
    public static readonly StyledProperty<double> MinColumnWidthProperty =
        AvaloniaProperty.Register<MdWaterfall, double>(nameof(MinColumnWidth), 220);
    public static readonly StyledProperty<double> ColumnSpacingProperty =
        AvaloniaProperty.Register<MdWaterfall, double>(nameof(ColumnSpacing), 12);
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<MdWaterfall, double>(nameof(ItemSpacing), 12);
    public static readonly StyledProperty<bool> AnimateLayoutProperty =
        AvaloniaProperty.Register<MdWaterfall, bool>(nameof(AnimateLayout), true);

    /// <summary>An explicit column count. Zero, the default, derives the count from <see cref="MinColumnWidth"/>.</summary>
    public int Columns { get => GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }

    /// <summary>The narrowest a column may be when the count is derived.</summary>
    public double MinColumnWidth { get => GetValue(MinColumnWidthProperty); set => SetValue(MinColumnWidthProperty, value); }

    /// <summary>Horizontal space between columns.</summary>
    public double ColumnSpacing { get => GetValue(ColumnSpacingProperty); set => SetValue(ColumnSpacingProperty, value); }

    /// <summary>Vertical space between items in the same column.</summary>
    public double ItemSpacing { get => GetValue(ItemSpacingProperty); set => SetValue(ItemSpacingProperty, value); }

    /// <summary>Whether children glide to their new places when the column count changes.</summary>
    public bool AnimateLayout { get => GetValue(AnimateLayoutProperty); set => SetValue(AnimateLayoutProperty, value); }
}
