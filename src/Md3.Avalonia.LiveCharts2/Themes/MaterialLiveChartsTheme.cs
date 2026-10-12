using System;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using LiveChartsCore.Themes;
using LiveChartsCore.VisualElements;
using SkiaSharp;

namespace Md3.Avalonia.Themes;

/// <summary>
/// The Material Design 3 look for <a href="https://livecharts.dev">LiveCharts2</a> charts.
/// </summary>
/// <remarks>
/// <para>
/// LiveCharts has no per-control style seam: every chart resolves its look from the one theme held
/// by <c>LiveCharts.DefaultSettings</c>, so this is a configuration entry point rather than a
/// <c>Styles</c> subclass. Call <see cref="AddMaterialTheme"/> once at startup, before the first
/// chart view is created - LiveCharts requires a theme to be configured at all:
/// </para>
/// <code>
/// using LiveChartsCore;
/// using Md3.Avalonia.Themes;
///
/// LiveCharts.Configure(settings =&gt; settings.AddMaterialTheme());
/// </code>
/// <para>
/// The rules are added <em>on top of</em> LiveCharts' own default theme, so its tooltip and legend
/// factories, animations and the geometry it already decides stay exactly as upstream ships them;
/// what this changes is the Material roles, the series palette, the type sizes and the parts of the
/// look Material has an opinion about (rounded bars, unfilled line series, donut separators). An
/// application that builds its own theme can call <see cref="ApplyMaterialRules"/> on it instead,
/// which appends the same rules to that theme.
/// </para>
/// <para>
/// Every colour is read from the core Material theme's <c>Md.Sys.Color.*</c> tokens for the variant
/// the chart is drawing in - <c>IChartView.IsDarkMode</c>, which the Avalonia view takes from the
/// application's theme variant - so charts follow the same light/dark switch as the rest of the
/// application. When the application does not register <c>MaterialTheme</c> the M3 baseline values
/// stand in; see the LiveCharts2 section of the root README.
/// </para>
/// <para>
/// <c>MdChart</c> in the core package stays provider neutral: it keeps drawing through whatever
/// chart provider an application chooses, and this package only styles LiveCharts2. The series
/// palette is available on its own through <see cref="SeriesPalette"/> for a provider that has to
/// be handed colours directly.
/// </para>
/// </remarks>
public static class MaterialLiveChartsTheme
{
    /// <summary>
    /// The <c>Md.Sys.Color</c> roles the series palette is built from, in the order LiveCharts walks
    /// them. The first four carry the scheme's hue variety and chroma (the two accents, the neutral
    /// accent and the error role); the remaining four are further tones of the same accent hues, so a
    /// chart with more than four series still reads as Material instead of inventing colours.
    /// </summary>
    public static string[] SeriesPaletteRoles { get; } =
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

    /// <summary>
    /// Adds the Material theme: LiveCharts' default theme with <see cref="ApplyMaterialRules"/>
    /// layered on top.
    /// </summary>
    /// <param name="settings">The LiveCharts settings to configure.</param>
    /// <returns>The same settings, so calls can be chained.</returns>
    /// <example>
    /// <code>LiveCharts.Configure(settings =&gt; settings.AddMaterialTheme());</code>
    /// </example>
    /// <remarks>
    /// Call it once. Like every other <c>AddDefaultTheme</c> overload this replaces the theme
    /// LiveCharts is currently using, so adding it after a theme of your own would drop that one.
    /// </remarks>
    public static LiveChartsSettings AddMaterialTheme(this LiveChartsSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return settings.AddDefaultTheme(theme => ApplyMaterialRules(theme));
    }

    /// <summary>
    /// Appends the Material rules to a LiveCharts theme.
    /// </summary>
    /// <param name="theme">The theme to extend.</param>
    /// <returns>The same theme, so calls can be chained.</returns>
    /// <remarks>
    /// <para>
    /// Use this when the application already builds a theme through <c>LiveChartsSettings.HasTheme</c>
    /// and wants to keep the rest of that theme: rules are appended, so they run after whatever the
    /// caller registered and win. A theme built from scratch also needs LiveCharts' tooltip and
    /// legend factories (<c>HasDefaultTooltip</c> / <c>HasDefaultLegend</c>); the default theme that
    /// <see cref="AddMaterialTheme"/> starts from brings them along.
    /// </para>
    /// <para>
    /// The palette has to be in place before anything is styled, because the rules read each series'
    /// colour back out of it. Appending it here is enough: LiveCharts sets the theme up - which is
    /// what runs the callback that fills <c>Theme.Colors</c> - before it styles the first axis or
    /// series, and again whenever the application flips between light and dark.
    /// </para>
    /// </remarks>
    public static Theme ApplyMaterialRules(this Theme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);

        // LiveCharts sets a theme up - which is what runs the callback below - before it styles the
        // first axis or series, so the palette is in place by then. Seeding it here as well keeps the
        // rules usable on a theme that is styled without ever being set up (a caller driving
        // ApplyStyleToSeries itself, or a test): GetSeriesColor divides by the palette's length.
        theme.Colors = SeriesPalette(theme.IsDark);

        return theme
            .OnInitialized(() =>
            {
                var colors = new Colors(theme.IsDark, forPalette: true);

                // LiveCharts walks this array by series id, so its order is the palette's order.
                theme.Colors = [.. colors.Palette];

                // A chart's canvas is cleared to the Material surface, so a chart inside a card
                // blends with the card instead of punching a white or black rectangle through it.
                theme.VirtualBackroundColor = colors.Surface.AsLvcColor();

                // Tooltips and legends are Material inverse-surface cards - the high-contrast
                // pairing M3 reserves for surfaces that float above the content - and they are
                // typeset at the small end of the type scale.
                theme.TooltipTextSize = MdLiveChartsTokens.Size("LabelLarge", 14);
                theme.TooltipTextPaint = new SolidColorPaint(colors.InverseOnSurface);
                theme.TooltipBackgroundPaint = new SolidColorPaint(colors.InverseSurface)
                {
                    ImageFilter = new DropShadow(0, 1, 3, 3, colors.Shadow),
                };
                theme.LegendBackgroundPaint = null;
                theme.LegendTextSize = MdLiveChartsTokens.Size("LabelLarge", 14);
                theme.LegendTextPaint = new SolidColorPaint(colors.OnSurfaceVariant);
            })
            .HasRuleForAxes(axis =>
            {
                var colors = new Colors(theme.IsDark);

                // Axis text is the small end of the type scale in the medium-emphasis ink M3 uses
                // for supporting text; upstream leaves both sizes at 16 and the inks near black.
                axis.TextSize = MdLiveChartsTokens.Size("LabelMedium", 12);
                axis.NameTextSize = MdLiveChartsTokens.Size("LabelLarge", 14);
                axis.NamePaint = new SolidColorPaint(colors.OnSurfaceVariant);
                axis.LabelsPaint = new SolidColorPaint(colors.OnSurfaceVariant);
                axis.ShowSeparatorLines = true;

                // Grid lines are the same OutlineVariant rule the rest of the theme draws with.
                // Only a cartesian plane's horizontal lines are drawn, as upstream does; a polar
                // plane gets the full grid.
                if (axis is ICartesianAxis cartesian)
                {
                    axis.SeparatorsPaint = cartesian.Orientation == AxisOrientation.Y
                        ? new SolidColorPaint(colors.OutlineVariant)
                        : null;
                    cartesian.Padding = new Padding(12);
                }
                else
                {
                    if (axis is IPolarAxis polar)
                        polar.LabelsBackground = colors.Surface.AsLvcColor();

                    axis.SeparatorsPaint = new SolidColorPaint(colors.OutlineVariant);
                }
            })
            .HasRuleForAnySeries(series =>
            {
                // Data labels default to the medium-emphasis ink; the rules for the series that put
                // them inside a filled shape replace it with something that reads on that fill.
                if (series.ShowDataLabels)
                    series.DataLabelsPaint = new SolidColorPaint(new Colors(theme.IsDark).OnSurfaceVariant);
            })
            .HasRuleForLineSeries(lineSeries =>
            {
                var colors = new Colors(theme.IsDark);
                var color = theme.GetSeriesColor(lineSeries).AsSKColor();

                // A Material line chart is a stroke with data points, so upstream's area fill is
                // cleared - set Fill yourself for an area chart. The markers are the series colour
                // ringed over the surface, which keeps them legible where lines cross.
                lineSeries.Stroke = new SolidColorPaint(color, 3);
                lineSeries.Fill = null;
                lineSeries.GeometrySize = 8;
                lineSeries.GeometryStroke = new SolidColorPaint(color, 2);
                lineSeries.GeometryFill = new SolidColorPaint(colors.Surface);

                if (lineSeries.ShowError)
                    lineSeries.ErrorPaint = new SolidColorPaint(colors.OnSurfaceVariant);
            })
            .HasRuleForStepLineSeries(stepLineSeries =>
            {
                // A step line is the same statement as a line, drawn square.
                stepLineSeries.Stroke = new SolidColorPaint(theme.GetSeriesColor(stepLineSeries).AsSKColor(), 3);
                stepLineSeries.Fill = null;
            })
            .HasRuleForStackedLineSeries(stackedLineSeries =>
            {
                // A stacked area is read as one filled shape, so it keeps its fill; the markers go
                // away, because a stack of markers is noise rather than data.
                stackedLineSeries.Stroke = new SolidColorPaint(theme.GetSeriesColor(stackedLineSeries).AsSKColor(), 3);
                stackedLineSeries.GeometrySize = 0;
            })
            .HasRuleForBarSeries(barSeries =>
            {
                var colors = new Colors(theme.IsDark);

                barSeries.Stroke = null;
                barSeries.Fill = new SolidColorPaint(theme.GetSeriesColor(barSeries).AsSKColor());

                // Material bars grow with a 4 DIP rounded end and sit further apart than upstream's
                // default, which sets a bar's own width per unit rather than a gap between bars.
                barSeries.Rx = 4;
                barSeries.Ry = 4;
                barSeries.Padding = 6;

                if (barSeries.ShowDataLabels)
                {
                    barSeries.DataLabelsPaint = barSeries.DataLabelsPosition == DataLabelsPosition.Middle
                        ? new SolidColorPaint(colors.OnPrimary)
                        : new SolidColorPaint(colors.OnSurfaceVariant);
                }

                if (barSeries.ShowError)
                    barSeries.ErrorPaint = new SolidColorPaint(colors.OnSurfaceVariant);
            })
            .HasRuleForStackedBarSeries(stackedBarSeries =>
            {
                // A stacked bar is one column made of segments, so the segments keep square corners.
                stackedBarSeries.Stroke = null;
                stackedBarSeries.Fill = new SolidColorPaint(theme.GetSeriesColor(stackedBarSeries).AsSKColor());
                stackedBarSeries.Rx = 0;
                stackedBarSeries.Ry = 0;
            })
            .HasRuleForStackedColumnSeries(stackedColumnSeries =>
            {
                stackedColumnSeries.Stroke = null;
                stackedColumnSeries.Fill = new SolidColorPaint(theme.GetSeriesColor(stackedColumnSeries).AsSKColor());
                stackedColumnSeries.Rx = 0;
                stackedColumnSeries.Ry = 0;
            })
            .HasRuleForPieSeries(pieSeries =>
            {
                var colors = new Colors(theme.IsDark);

                // Slices are separated by the surface showing through, which is how M3 draws
                // adjacent segments - the ring is the surface, not a border.
                pieSeries.Stroke = new SolidColorPaint(colors.Surface, 2);
                pieSeries.Fill = new SolidColorPaint(theme.GetSeriesColor(pieSeries).AsSKColor());

                if (pieSeries.ShowDataLabels)
                {
                    pieSeries.DataLabelsPaint = pieSeries.DataLabelsPosition == PolarLabelsPosition.Outer
                        ? new SolidColorPaint(colors.OnSurfaceVariant)
                        : new SolidColorPaint(colors.OnPrimary);
                }
            })
            .HasRuleForGaugeSeries(gaugeSeries =>
            {
                // A gauge's own arc is the unfilled track: the highest surface container, the role
                // the rest of the theme uses for a shape that is filled but inactive.
                gaugeSeries.Stroke = null;
                gaugeSeries.Fill = new SolidColorPaint(new Colors(theme.IsDark).SurfaceContainerHighest);
            })
            .HasRuleForGaugeFillSeries(gaugeFillSeries =>
            {
                // The fill series is the value arc drawn over that track, so it takes the primary
                // accent: a track and an indicator, the shape Material's own progress ring uses.
                gaugeFillSeries.Fill = new SolidColorPaint(new Colors(theme.IsDark).Primary);
            })
            .HasRuleForScatterSeries(scatterSeries =>
            {
                var colors = new Colors(theme.IsDark);

                scatterSeries.Stroke = null;
                scatterSeries.Fill = new SolidColorPaint(theme.GetSeriesColor(scatterSeries).AsSKColor());

                if (scatterSeries.ShowError)
                    scatterSeries.ErrorPaint = new SolidColorPaint(colors.OnSurfaceVariant);
            })
            .HasRuleForFinancialSeries(financialSeries =>
            {
                var colors = new Colors(theme.IsDark);

                // Upstream hardcodes a green/red pair here. The rising series takes the third
                // accent - the role M3 reaches for when something has to read as positive without
                // being a semantic colour - and the falling one the error role.
                financialSeries.UpFill = new SolidColorPaint(colors.Tertiary);
                financialSeries.UpStroke = new SolidColorPaint(colors.Tertiary, 3);
                financialSeries.DownFill = new SolidColorPaint(colors.Error);
                financialSeries.DownStroke = new SolidColorPaint(colors.Error, 3);
            })
            .HasRuleFor<BaseLabelVisual>(label =>
            {
                label.Paint = new SolidColorPaint(new Colors(theme.IsDark).OnSurfaceVariant);
            })
            .HasRuleFor<BaseAngularTicksVisual>(ticks =>
            {
                var colors = new Colors(theme.IsDark);

                ticks.Stroke = new SolidColorPaint(colors.OnSurfaceVariant);
                ticks.LabelsPaint = new SolidColorPaint(colors.OnSurfaceVariant);
            });
    }

    /// <summary>
    /// The series palette for one variant, in the order <see cref="SeriesPaletteRoles"/> lists it.
    /// </summary>
    /// <param name="dark">Whether the dark variant's tokens should be read.</param>
    /// <returns>One colour per role, so index <c>n</c> is what LiveCharts gives series <c>n</c>.</returns>
    /// <remarks>
    /// The same colours the theme puts in <c>Theme.Colors</c>, exposed for a chart provider that
    /// takes a colour per series instead of a palette.
    /// </remarks>
    public static LvcColor[] SeriesPalette(bool dark) =>
        [.. SeriesPaletteRoles.Select(role => MdLiveChartsTokens.Color(role, dark).AsLvcColor())];

    /// <summary>The tokens the rules read, resolved once per rule.</summary>
    private readonly struct Colors
    {
        public Colors(bool dark, bool forPalette = false)
        {
            Primary = MdLiveChartsTokens.Color("Primary", dark);
            Tertiary = MdLiveChartsTokens.Color("Tertiary", dark);
            Error = MdLiveChartsTokens.Color("Error", dark);
            OnPrimary = MdLiveChartsTokens.Color("OnPrimary", dark);
            Surface = MdLiveChartsTokens.Color("Surface", dark);
            SurfaceContainerHighest = MdLiveChartsTokens.Color("SurfaceContainerHighest", dark);
            InverseSurface = MdLiveChartsTokens.Color("InverseSurface", dark);
            InverseOnSurface = MdLiveChartsTokens.Color("InverseOnSurface", dark);
            OnSurfaceVariant = MdLiveChartsTokens.Color("OnSurfaceVariant", dark);
            OutlineVariant = MdLiveChartsTokens.Color("OutlineVariant", dark);

            // M3's elevation shadows are black at a low opacity (the second elevation step is
            // about a quarter); the tooltip's card is the inverse surface, so one value reads in
            // both variants.
            Shadow = new SKColor(0, 0, 0, 61);

            Palette = forPalette ? SeriesPalette(dark) : [];
        }

        public SKColor Primary { get; }

        public SKColor Tertiary { get; }

        public SKColor Error { get; }

        public SKColor OnPrimary { get; }

        public SKColor Surface { get; }

        public SKColor SurfaceContainerHighest { get; }

        public SKColor InverseSurface { get; }

        public SKColor InverseOnSurface { get; }

        public SKColor OnSurfaceVariant { get; }

        public SKColor OutlineVariant { get; }

        public SKColor Shadow { get; }

        /// <summary>The palette, filled in only where it is needed.</summary>
        public LvcColor[] Palette { get; }
    }
}
