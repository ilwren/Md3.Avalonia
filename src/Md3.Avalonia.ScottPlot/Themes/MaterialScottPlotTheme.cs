using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Styling;
using ScottPlot;
using ScottPlot.Avalonia;

namespace Md3.Avalonia.Themes;

/// <summary>
/// Material 3 appearance for a ScottPlot plot: the surfaces, the ink, the grid, the legend and the
/// type scale a Material chart is drawn with, plus the Material series palette.
/// 用于 ScottPlot 图表的 Material 3 外观与配色主题。
/// </summary>
/// <remarks>
/// <para>
/// ScottPlot has no style collection to register in either: a plot carries its own colours and its own
/// type, so this theme is applied to a plot - or to the <see cref="AvaPlot"/> control that draws it -
/// instead of being added to <c>Application.Styles</c>. Nothing here reaches an Avalonia style tree,
/// so only the plots an application asks for are themed.
/// </para>
/// <para>
/// The rules are the same ones <c>Md3.Avalonia.LiveCharts2</c> writes for a LiveCharts chart, so the
/// two chart providers in this repository look alike: surface roles carry the plot, the
/// medium-emphasis ink carries axes and text, the grid is one <c>OutlineVariant</c> line at major
/// ticks, and the series palette is the same eight <c>Md.Sys.Color</c> roles in the same order.
/// Everything upstream decides stays as it ships - axis frames, tick generation, layout, interaction -
/// the theme only writes colours, sizes and the palette.
/// </para>
/// </remarks>
public sealed class MaterialScottPlotTheme
{
    /// <summary>The Material colour roles the series palette walks, in order.</summary>
    private static readonly string[] PaletteRoleNames =
    [
        "Primary",
        "Tertiary",
        "Secondary",
        "Error",
        "PrimaryContainer",
        "TertiaryContainer",
        "InversePrimary",
        "Outline",
    ];

    /// <summary>Initializes a theme that reads one variant's tokens.</summary>
    /// <param name="dark">Whether the dark variant's tokens should be read.</param>
    public MaterialScottPlotTheme(bool dark)
    {
        IsDark = dark;
    }

    /// <summary>Gets whether this theme reads the dark variant's tokens.</summary>
    public bool IsDark { get; }

    /// <summary>
    /// Gets the <c>Md.Sys.Color</c> roles the series palette walks, in the order it walks them.
    /// </summary>
    /// <remarks>
    /// The first four give a palette of four hues the way ScottPlot's own palettes do; the remaining
    /// four are further tones of the same accent hues, so a chart with more than four series still
    /// reads as Material instead of inventing colours.
    /// </remarks>
    public static IReadOnlyList<string> SeriesPaletteRoles => PaletteRoleNames;

    /// <summary>Builds the Material series palette for one variant.</summary>
    /// <param name="dark">Whether the dark variant's tokens should be read.</param>
    /// <returns>A palette over <see cref="SeriesPaletteRoles"/>, wrapping like ScottPlot's own.</returns>
    // Named in full: ScottPlot.Palettes is a nested namespace, and a using directive imports the types
    // of a namespace, not the namespaces inside it.
    public static IPalette SeriesPalette(bool dark) => new ScottPlot.Palettes.Custom(
        PaletteRoleNames.Select(role => MdScottPlotTokens.Color(role, dark)).ToArray(),
        dark ? "Material 3 (dark)" : "Material 3",
        "Md.Sys.Color roles, in the order the Material themes walk them");

    /// <summary>Applies the Material rules for one variant to a plot.</summary>
    /// <param name="plot">The plot to style.</param>
    /// <param name="dark">Whether the dark variant's tokens should be read.</param>
    /// <exception cref="ArgumentNullException"><paramref name="plot"/> is <c>null</c>.</exception>
    public static void Apply(Plot plot, bool dark)
    {
        ArgumentNullException.ThrowIfNull(plot);
        new MaterialScottPlotTheme(dark).Apply(plot);
    }

    /// <summary>Applies the Material rules to a plot.</summary>
    /// <param name="plot">The plot to style.</param>
    /// <exception cref="ArgumentNullException"><paramref name="plot"/> is <c>null</c>.</exception>
    public void Apply(Plot plot)
    {
        ArgumentNullException.ThrowIfNull(plot);

        var dark = IsDark;
        var surface = MdScottPlotTokens.Color("Surface", dark);
        var ink = MdScottPlotTokens.Color("OnSurfaceVariant", dark);
        var outlineVariant = MdScottPlotTokens.Color("OutlineVariant", dark);
        var fontFamily = MdScottPlotTokens.FontFamily;

        // A Material chart is a surface: the figure and the data area are the same plane, so a chart
        // inside a card blends with the card.
        plot.FigureBackground.Color = surface;
        plot.DataBackground.Color = surface;

        // Axes, their labels and the title carry the medium-emphasis ink, and the grid is one
        // OutlineVariant line at major ticks - minor lines are not part of Material's chart
        // specimens.
        plot.Axes.Color(ink);
        plot.Grid.MajorLineColor = outlineVariant;
        plot.Grid.MinorLineWidth = 0;

        // The legend floats over the data area, so it is a surface container with the soft outline a
        // Material card uses rather than upstream's white box.
        plot.Legend.BackgroundColor = MdScottPlotTokens.Color("SurfaceContainerHigh", dark);
        plot.Legend.OutlineColor = outlineVariant;
        plot.Legend.FontColor = ink;

        // Type scale: a TitleMedium chart title, LabelLarge axis names and legend entries, and
        // LabelMedium tick labels - the roles the LiveCharts2 theme typesets with.
        var titleSize = MdScottPlotTokens.Size("TitleMedium", 16f);
        var labelSize = MdScottPlotTokens.Size("LabelLarge", 14f);
        var tickSize = MdScottPlotTokens.Size("LabelMedium", 12f);

        plot.Axes.Title.Label.FontSize = titleSize;
        plot.Axes.Title.Label.FontName = fontFamily;
        plot.Legend.FontSize = labelSize;
        plot.Legend.FontName = fontFamily;

        foreach (var axis in plot.Axes.GetAxes())
        {
            axis.Label.FontSize = labelSize;
            axis.Label.FontName = fontFamily;
            axis.TickLabelStyle.FontSize = tickSize;
            axis.TickLabelStyle.FontName = fontFamily;
        }

        plot.Add.Palette = SeriesPalette(dark);
    }

    /// <summary>Applies the Material rules to the plot an <see cref="AvaPlot"/> control draws.</summary>
    /// <param name="plot">The control whose plot should be styled and redrawn.</param>
    /// <exception cref="ArgumentNullException"><paramref name="plot"/> is <c>null</c>.</exception>
    public void Apply(AvaPlot plot)
    {
        ArgumentNullException.ThrowIfNull(plot);
        Apply(plot.Plot);
        plot.Refresh();
    }

    /// <summary>
    /// Applies the Material rules to an <see cref="AvaPlot"/> control using the variant the control
    /// actually has, now and after every theme-variant change.
    /// </summary>
    /// <param name="plot">The control whose plot should follow the application's variant.</param>
    /// <exception cref="ArgumentNullException"><paramref name="plot"/> is <c>null</c>.</exception>
    /// <remarks>
    /// ScottPlot bakes colours into a plot when they are set, so - unlike an Avalonia style - a
    /// themed plot does not re-resolve them on its own. This is the one-liner that keeps a chart in
    /// step with the light/dark switch; it applies the rules immediately and re-applies them from
    /// the control's own <c>ActualThemeVariantChanged</c> event, so the subscription lives exactly as
    /// long as the control does. Applying the theme by hand instead is fine - call it again with the
    /// new variant and refresh.
    /// </remarks>
    public static void Follow(AvaPlot plot)
    {
        ArgumentNullException.ThrowIfNull(plot);
        ApplyForVariant(plot);
        plot.ActualThemeVariantChanged += (_, _) => ApplyForVariant(plot);
    }

    /// <summary>Applies the rules for the variant the given control has, and redraws it.</summary>
    private static void ApplyForVariant(AvaPlot plot)
    {
        Apply(plot.Plot, plot.ActualThemeVariant == ThemeVariant.Dark);
        plot.Refresh();
    }
}
