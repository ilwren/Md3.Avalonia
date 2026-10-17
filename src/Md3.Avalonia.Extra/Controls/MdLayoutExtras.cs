using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
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
    public static readonly StyledProperty<int> ColumnsProperty =
        AvaloniaProperty.Register<MdWaterfallPanel, int>(nameof(Columns));
    public static readonly StyledProperty<double> MinColumnWidthProperty =
        AvaloniaProperty.Register<MdWaterfallPanel, double>(nameof(MinColumnWidth), 220);
    public static readonly StyledProperty<double> ColumnSpacingProperty =
        AvaloniaProperty.Register<MdWaterfallPanel, double>(nameof(ColumnSpacing), 12);
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<MdWaterfallPanel, double>(nameof(ItemSpacing), 12);

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

        foreach (var child in Children)
        {
            var column = Shortest(heights);
            var x = column * (columnWidth + ColumnSpacing);
            if (rtl) x = finalSize.Width - x - columnWidth;
            child.Arrange(new Rect(x, heights[column], columnWidth, child.DesiredSize.Height));
            heights[column] += child.DesiredSize.Height + ItemSpacing;
        }

        return finalSize;
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

    /// <summary>An explicit column count. Zero, the default, derives the count from <see cref="MinColumnWidth"/>.</summary>
    public int Columns { get => GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }

    /// <summary>The narrowest a column may be when the count is derived.</summary>
    public double MinColumnWidth { get => GetValue(MinColumnWidthProperty); set => SetValue(MinColumnWidthProperty, value); }

    /// <summary>Horizontal space between columns.</summary>
    public double ColumnSpacing { get => GetValue(ColumnSpacingProperty); set => SetValue(ColumnSpacingProperty, value); }

    /// <summary>Vertical space between items in the same column.</summary>
    public double ItemSpacing { get => GetValue(ItemSpacingProperty); set => SetValue(ItemSpacingProperty, value); }
}

/// <summary>The window edge a <see cref="MdSideSheet"/> is pinned to and slides out from.</summary>
public enum MdSideSheetAnchor
{
    Left,
    Right
}

/// <summary>
/// A sheet pinned to the left or right edge of a window, collapsed to a floating button and opened
/// from it: the button stays where it is, the sheet grows out from behind it, and the content it was
/// holding fades and slides in.
/// </summary>
/// <remarks>
/// <para>
/// Collapsing a side panel to a single floating button is what keeps a secondary surface - filters,
/// a layer list, a chat thread - available without spending width on it. The sheet is a sibling of
/// the content it overlays rather than a replacement for it, so opening one never re-lays-out the
/// page underneath.
/// </para>
/// <para>
/// Width, opacity and the button's rotation are animated through the <see cref="MdMotion"/> tokens,
/// so the sheet follows the same scheme and reduced-motion settings as everything else instead of
/// carrying its own timings.
/// </para>
/// </remarks>
[PseudoClasses(":open", ":closed", ":left", ":right", ":content-present", ":reduced-motion", ":no-motion")]
public class MdSideSheet : HeaderedContentControl
{
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MdSideSheet, bool>(nameof(IsOpen),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<MdSideSheetAnchor> AnchorProperty =
        AvaloniaProperty.Register<MdSideSheet, MdSideSheetAnchor>(nameof(Anchor));
    public static readonly StyledProperty<double> ExpandedWidthProperty =
        AvaloniaProperty.Register<MdSideSheet, double>(nameof(ExpandedWidth), 320);
    public static readonly StyledProperty<double> CollapsedWidthProperty =
        AvaloniaProperty.Register<MdSideSheet, double>(nameof(CollapsedWidth), 72);
    public static readonly StyledProperty<object?> ToggleIconProperty =
        AvaloniaProperty.Register<MdSideSheet, object?>(nameof(ToggleIcon));
    public static readonly StyledProperty<bool> IsToggleVisibleProperty =
        AvaloniaProperty.Register<MdSideSheet, bool>(nameof(IsToggleVisible), true);
    public static readonly StyledProperty<IBrush?> SheetBrushProperty =
        AvaloniaProperty.Register<MdSideSheet, IBrush?>(nameof(SheetBrush));
    public static readonly StyledProperty<BoxShadows> ElevationProperty =
        AvaloniaProperty.Register<MdSideSheet, BoxShadows>(nameof(Elevation));

    private Border? _sheet;
    private ContentPresenter? _sheetContent;
    private Button? _toggle;
    private readonly MdPresenceController _presence;

    static MdSideSheet()
    {
        IsOpenProperty.Changed.AddClassHandler<MdSideSheet>((sheet, _) => sheet.OnOpenChanged());
        AnchorProperty.Changed.AddClassHandler<MdSideSheet>((sheet, _) => sheet.UpdatePseudoClasses());
        IsToggleVisibleProperty.Changed.AddClassHandler<MdSideSheet>((sheet, e) =>
        {
            if (sheet._toggle is not null) sheet._toggle.IsVisible = e.GetNewValue<bool>();
        });
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdSideSheet>((sheet, _) => sheet.UpdateMotion());
    }

    public MdSideSheet()
    {
        _presence = new MdPresenceController(present =>
        {
            PseudoClasses.Set(":content-present", present);
            UpdateContentHitTesting();
        });
        _presence.Initialize(IsOpen);
        UpdatePseudoClasses();
    }

    /// <summary>Whether the sheet is open. Two-way, so a view model can drive it and be told about toggles.</summary>
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }

    /// <summary>The edge the sheet is pinned to and slides out from.</summary>
    public MdSideSheetAnchor Anchor { get => GetValue(AnchorProperty); set => SetValue(AnchorProperty, value); }

    /// <summary>The sheet's width once open.</summary>
    public double ExpandedWidth { get => GetValue(ExpandedWidthProperty); set => SetValue(ExpandedWidthProperty, value); }

    /// <summary>The width the sheet keeps while closed, which is the room the floating button needs.</summary>
    public double CollapsedWidth { get => GetValue(CollapsedWidthProperty); set => SetValue(CollapsedWidthProperty, value); }

    /// <summary>The icon shown on the floating button.</summary>
    public object? ToggleIcon { get => GetValue(ToggleIconProperty); set => SetValue(ToggleIconProperty, value); }

    /// <summary>Whether the floating button is shown. Turn it off when something else drives <see cref="IsOpen"/>.</summary>
    public bool IsToggleVisible { get => GetValue(IsToggleVisibleProperty); set => SetValue(IsToggleVisibleProperty, value); }

    /// <summary>The sheet's surface. Defaults to a Material surface container role.</summary>
    public IBrush? SheetBrush { get => GetValue(SheetBrushProperty); set => SetValue(SheetBrushProperty, value); }

    /// <summary>The sheet's shadow.</summary>
    public BoxShadows Elevation { get => GetValue(ElevationProperty); set => SetValue(ElevationProperty, value); }

    /// <summary>Raised after <see cref="IsOpen"/> changes, with the new value.</summary>
    public event EventHandler<bool>? IsOpenChanged;

    public void Open() => SetCurrentValue(IsOpenProperty, true);
    public void Close() => SetCurrentValue(IsOpenProperty, false);
    public void Toggle() => SetCurrentValue(IsOpenProperty, !IsOpen);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_toggle is not null) _toggle.Click -= OnToggleClick;
        base.OnApplyTemplate(e);
        _sheet = e.NameScope.Find<Border>("PART_Sheet");
        _sheetContent = e.NameScope.Find<ContentPresenter>("PART_SheetContent");
        _toggle = e.NameScope.Find<Button>("PART_Toggle");
        if (_toggle is not null)
        {
            _toggle.Click += OnToggleClick;
            _toggle.IsVisible = IsToggleVisible;
        }
        _presence.Initialize(IsOpen);
        UpdateMotion();
        UpdateContentHitTesting();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _presence.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private void OnToggleClick(object? sender, RoutedEventArgs e) => Toggle();

    private void OnOpenChanged()
    {
        UpdatePseudoClasses();
        _presence.Update(IsOpen, MdMotion.GetExitDuration(this));
        UpdateContentHitTesting();
        IsOpenChanged?.Invoke(this, IsOpen);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);

        if (_sheet is not null)
        {
            // Growing the sheet is spatial motion: the width change is the thing being animated,
            // and it should carry the scheme's spatial duration and easing.
            _sheet.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, WidthProperty, MdMotionKind.Spatial));
        }
        if (_sheetContent is not null)
        {
            _sheetContent.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty));
        }
        if (_toggle is not null)
        {
            _toggle.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
        }
        if (!IsOpen) _presence.Update(false, MdMotion.GetExitDuration(this));
    }

    private void UpdateContentHitTesting()
    {
        // A collapsed sheet is only as wide as its button, but its content is still in the tree for
        // the exit animation. Leaving it hit-testable would put an invisible target over the page.
        if (_sheetContent is not null) _sheetContent.IsHitTestVisible = IsOpen;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":open", IsOpen);
        PseudoClasses.Set(":closed", !IsOpen);
        PseudoClasses.Set(":left", Anchor == MdSideSheetAnchor.Left);
        PseudoClasses.Set(":right", Anchor == MdSideSheetAnchor.Right);
    }
}
