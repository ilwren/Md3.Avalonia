using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.Themes;
using LiveChartsCore.VisualElements;
using Md3.Avalonia.Themes;
using SkiaSharp;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Covers the Material theme for LiveCharts2, which ships in the opt-in
/// Md3.Avalonia.LiveCharts2 package.
/// </summary>
/// <remarks>
/// The theme is shaped unlike every other theme in this repository: LiveCharts has no per-control
/// style seam at all. A chart resolves its look from the single theme held by
/// <c>LiveCharts.DefaultSettings</c>, so the package's surface is a configuration entry point
/// (<c>AddMaterialTheme</c> / <c>ApplyMaterialRules</c> / <c>SeriesPalette</c>) and there is nothing
/// to register in an <see cref="Application.Styles"/> collection. What the tests can hold to
/// account, then, is the mapping the rules produce: which token every paint and size is read from,
/// that both variants are read from the core theme rather than hardcoded, and that the package
/// cannot reach an Avalonia style tree at all.
/// </remarks>
public class MdLiveChartsThemeTests
{
    [AvaloniaFact]
    public void The_Palette_Is_Eight_Material_Roles_In_Order()
    {
        // The order is part of the contract: LiveCharts walks the array by series id, so the first
        // four entries are the ones a four-series chart reads.
        Assert.Equal(
            new[] { "Primary", "Tertiary", "Secondary", "Error", "PrimaryContainer", "TertiaryContainer", "InversePrimary", "Outline" },
            MaterialLiveChartsTheme.SeriesPaletteRoles);

        foreach (var dark in new[] { false, true })
        {
            var palette = MaterialLiveChartsTheme.SeriesPalette(dark);
            Assert.Equal(MaterialLiveChartsTheme.SeriesPaletteRoles.Length, palette.Length);

            // Every entry is the token's own value for that variant - which is what makes the
            // palette follow a seeded or customised MaterialTheme instead of a copied-in list.
            Assert.Equal(
                MaterialLiveChartsTheme.SeriesPaletteRoles.Select(role => Token(role, dark)),
                palette.Select(color => color.AsSKColor()));

            // Eight series have to be tellable apart before the palette starts repeating.
            Assert.Equal(palette.Length, palette.Distinct().Count());
        }

        // The two variants are different palettes; a rule that read light tokens under a dark
        // MaterialTheme would show up here.
        Assert.NotEqual(MaterialLiveChartsTheme.SeriesPalette(false), MaterialLiveChartsTheme.SeriesPalette(true));
    }

    [AvaloniaFact]
    public void Every_Palette_Role_Has_An_M3_Baseline_Value()
    {
        // Every colour a rule reads goes through the package's token reader, which falls back to an
        // M3 baseline table when the application never registered MaterialTheme at all. A role
        // missing from that table only throws on the fallback path, so the table is asserted here
        // rather than left to an application that does not theme itself.
        var fallbacks = (Dictionary<string, (string Light, string Dark)>)typeof(MaterialLiveChartsTheme)
            .Assembly.GetType("Md3.Avalonia.Themes.MdLiveChartsTokens")!
            .GetField("ColorFallbacks", BindingFlags.Static | BindingFlags.NonPublic)!
            .GetValue(null)!;

        // The palette plus the roles the rules read directly.
        var roles = MaterialLiveChartsTheme.SeriesPaletteRoles.Concat(
            ["OnPrimary", "Surface", "SurfaceContainerHighest", "InverseSurface", "InverseOnSurface", "OnSurfaceVariant", "OutlineVariant"]);

        foreach (var role in roles)
        {
            Assert.True(fallbacks.TryGetValue(role, out var baseline), $"there is no M3 baseline for {role}");

            // The baselines are the core theme's own values, so a chart looks the same whether or
            // not the application registered MaterialTheme.
            Assert.Equal(Token(role, dark: false), SKColor.Parse(baseline.Light));
            Assert.Equal(Token(role, dark: true), SKColor.Parse(baseline.Dark));
        }
    }

    [AvaloniaFact]
    public void Configuring_LiveCharts_Installs_One_Material_Theme()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;

        // The documented setup, run exactly as an application runs it. Nothing else is needed: the
        // theme arrives with the tooltip and legend factories LiveCharts' own default theme brings,
        // which is what makes the layered AddMaterialTheme the shorter path.
        LiveCharts.Configure(settings => settings.AddMaterialTheme());

        // A chart view is what sets a theme up: its core chart's GetTheme() runs the theme's
        // initialization for the variant the view draws in, which is the step that fills the
        // palette and the card paints. (CoreChart exists as soon as the view is constructed.)
        var chart = new CartesianChart();
        var theme = chart.CoreChart.GetTheme();

        // The rules were appended to LiveCharts' own default theme rather than replacing it, which
        // is what keeps its tooltip and legend factories alive: the base Theme class throws from
        // both when nothing set them.
        Assert.IsAssignableFrom<IChartTooltip>(theme.GetDefaultTooltip());
        Assert.IsAssignableFrom<IChartLegend>(theme.GetDefaultLegend());
        Assert.NotEmpty(theme.Initialized);
        Assert.NotEmpty(theme.AxisBuilder);
        Assert.NotEmpty(theme.SeriesBuilder);

        // Visual elements are built differently: a theme has one slot per element type rather than a
        // list, and it throws when a type is registered twice - which is what LiveCharts' own default
        // theme does for the label, needle and angular-tick rules this theme has an opinion about. So
        // the Material rules take those slots over instead of appending to them, and this is what the
        // chart reads back out of them.
        var label = new LabelVisual();
        ElementRule<BaseLabelVisual>(theme)(label);
        Assert.Equal(Token("OnSurfaceVariant", dark: false), Assert.IsType<SolidColorPaint>(label.Paint).Color);

        var needle = new NeedleVisual();
        ElementRule<BaseNeedleVisual>(theme)(needle);
        Assert.Equal(20d, needle.Width);
        Assert.Equal(Token("OnSurfaceVariant", dark: false), Assert.IsType<SolidColorPaint>(needle.Fill).Color);

        var ticks = new AngularTicksVisual();
        ElementRule<BaseAngularTicksVisual>(theme)(ticks);
        Assert.Equal(Token("OnSurfaceVariant", dark: false), Assert.IsType<SolidColorPaint>(ticks.Stroke).Color);
        Assert.Equal(Token("OnSurfaceVariant", dark: false), Assert.IsType<SolidColorPaint>(ticks.LabelsPaint).Color);

        Assert.False(theme.IsDark);
        Assert.Equal(MaterialLiveChartsTheme.SeriesPalette(false), theme.Colors);
        Assert.Equal(Token("Surface", dark: false), theme.VirtualBackroundColor.AsSKColor());

        // Tooltips and legends are Material inverse-surface cards: the pairing M3 reserves for
        // surfaces that float over the content, with a shadow under the tooltip.
        var tooltip = Assert.IsType<SolidColorPaint>(theme.TooltipBackgroundPaint);
        Assert.Equal(Token("InverseSurface", dark: false), tooltip.Color);
        Assert.IsType<DropShadow>(tooltip.ImageFilter);
        Assert.Equal(Token("InverseOnSurface", dark: false), Assert.IsType<SolidColorPaint>(theme.TooltipTextPaint).Color);
        Assert.Null(theme.LegendBackgroundPaint);
        Assert.Equal(Token("OnSurfaceVariant", dark: false), Assert.IsType<SolidColorPaint>(theme.LegendTextPaint).Color);

        // Both are typeset at the end of the type scale M3 gives labels.
        Assert.Equal(Size("Md.Sys.TypeScale.LabelLarge.Size"), theme.TooltipTextSize);
        Assert.Equal(Size("Md.Sys.TypeScale.LabelLarge.Size"), theme.LegendTextSize);
    }

    [AvaloniaFact]
    public void The_Theme_Follows_The_Applications_Variant_Switch()
    {
        var application = Application.Current!;
        var previous = application.RequestedThemeVariant;

        try
        {
            application.RequestedThemeVariant = ThemeVariant.Light;
            LiveCharts.Configure(settings => settings.AddMaterialTheme());

            var chart = new CartesianChart();
            var theme = chart.CoreChart.GetTheme();
            Assert.False(theme.IsDark);
            Assert.Equal(Token("Surface", dark: false), theme.VirtualBackroundColor.AsSKColor());

            // The same theme object, the other variant: a chart asks LiveCharts to set the theme up
            // on every measure, and the setup re-runs the initialization when the variant flips -
            // so the palette and the card paints follow the application, as M3 requires.
            application.RequestedThemeVariant = ThemeVariant.Dark;

            Assert.True(theme.IsDark);
            Assert.Same(theme, chart.CoreChart.GetTheme());
            Assert.Equal(MaterialLiveChartsTheme.SeriesPalette(true), theme.Colors);
            Assert.Equal(Token("Surface", dark: true), theme.VirtualBackroundColor.AsSKColor());
            Assert.Equal(Token("InverseSurface", dark: true), Assert.IsType<SolidColorPaint>(theme.TooltipBackgroundPaint).Color);
            Assert.Equal(Token("InverseOnSurface", dark: true), Assert.IsType<SolidColorPaint>(theme.TooltipTextPaint).Color);
            Assert.Equal(Token("OnSurfaceVariant", dark: true), Assert.IsType<SolidColorPaint>(theme.LegendTextPaint).Color);

            // The dark palette is the one the rules paint from now: the same series under the
            // light variant would take the light Primary, and the two are different colours.
            var series = new LineSeries<double>();
            theme.ApplyStyleToSeries(series);
            Assert.Equal(
                MaterialLiveChartsTheme.SeriesPalette(true)[0].AsSKColor(),
                Assert.IsType<SolidColorPaint>(series.Stroke).Color);
        }
        finally
        {
            application.RequestedThemeVariant = previous;
        }
    }

    [AvaloniaFact]
    public void A_Line_Series_Is_A_Stroke_With_Surface_Filled_Markers()
    {
        var theme = MaterialRules(dark: false);
        var series = new LineSeries<double> { Values = [1, 2, 3] };

        theme.ApplyStyleToSeries(series);

        // The rule paints the series with the palette entry that belongs to it, not with a
        // literal. A series that is not in a chart yet has no slot - LiveCharts assigns it when a
        // chart adds the series - so it takes the first one.
        var palette = MaterialLiveChartsTheme.SeriesPalette(false);
        var stroke = Assert.IsType<SolidColorPaint>(series.Stroke);
        Assert.Equal(palette[0].AsSKColor(), stroke.Color);
        Assert.Equal(3f, stroke.StrokeThickness);

        // Once a chart has given a series a slot, that is the entry it reads: the palette rotates.
        foreach (var slot in new[] { 1, 2, 3 })
        {
            var slotted = InSlot(new LineSeries<double> { Values = [1, 2, 3] }, slot);
            theme.ApplyStyleToSeries(slotted);
            Assert.Equal(palette[slot].AsSKColor(), Assert.IsType<SolidColorPaint>(slotted.Stroke).Color);
        }

        // Upstream adds an area fill; a Material line chart is a line chart.
        Assert.Null(series.Fill);

        Assert.Equal(8d, series.GeometrySize);
        var ring = Assert.IsType<SolidColorPaint>(series.GeometryStroke);
        Assert.Equal(stroke.Color, ring.Color);
        Assert.Equal(2f, ring.StrokeThickness);
        Assert.Equal(Token("Surface", dark: false), Assert.IsType<SolidColorPaint>(series.GeometryFill).Color);
    }

    [AvaloniaFact]
    public void Columns_Are_Rounded_And_Inked_From_The_Core_Theme()
    {
        var theme = MaterialRules(dark: false);

        var column = new ColumnSeries<double> { Values = [1, 2, 3] };
        theme.ApplyStyleToSeries(column);

        Assert.Null(column.Stroke);
        Assert.Equal(MaterialLiveChartsTheme.SeriesPalette(false)[0].AsSKColor(), Assert.IsType<SolidColorPaint>(column.Fill).Color);
        Assert.Equal(4d, column.Rx);
        Assert.Equal(4d, column.Ry);
        Assert.Equal(6d, column.Padding);

        // A stacked bar is one column made of segments, so its corners stay square.
        var stacked = new StackedColumnSeries<double> { Values = [1, 2, 3] };
        theme.ApplyStyleToSeries(stacked);
        Assert.Equal(0d, stacked.Rx);
        Assert.Equal(0d, stacked.Ry);
        Assert.Null(stacked.Stroke);

        // Labels placed inside the fill take the ink that reads on the palette, not the axis ink.
        var labelled = new ColumnSeries<double> { Values = [1], ShowDataLabels = true, DataLabelsPosition = DataLabelsPosition.Middle };
        theme.ApplyStyleToSeries(labelled);
        Assert.Equal(Token("OnPrimary", dark: false), Assert.IsType<SolidColorPaint>(labelled.DataLabelsPaint).Color);
    }

    [AvaloniaFact]
    public void An_Axis_Takes_Typescale_Sizes_Medium_Emphasis_Ink_And_An_OutlineVariant_Grid()
    {
        var theme = MaterialRules(dark: false);

        // Which way an axis runs is the chart's to decide - the chart is what tells a plane's X
        // from its Y - so the rule reads the orientation rather than setting it. Sizes, inks and
        // padding do not depend on it, so they are read off a bare axis.
        var axis = new Axis();
        theme.ApplyStyleToAxis(axis);

        Assert.Equal(Size("Md.Sys.TypeScale.LabelMedium.Size"), axis.TextSize);
        Assert.Equal(Size("Md.Sys.TypeScale.LabelLarge.Size"), axis.NameTextSize);
        Assert.Equal(Token("OnSurfaceVariant", dark: false), Assert.IsType<SolidColorPaint>(axis.LabelsPaint).Color);
        Assert.Equal(Token("OnSurfaceVariant", dark: false), Assert.IsType<SolidColorPaint>(axis.NamePaint).Color);
        Assert.True(axis.ShowSeparatorLines);
        Assert.Equal(12f, axis.Padding.Left);
        Assert.Equal(12f, axis.Padding.Top);

        // A polar plane has no X/Y pair, so its rule draws the full grid and gives the labels the
        // surface to sit on.
        var polar = new PolarAxis();
        theme.ApplyStyleToAxis(polar);
        Assert.Equal(Token("OutlineVariant", dark: false), Assert.IsType<SolidColorPaint>(polar.SeparatorsPaint).Color);
        Assert.Equal(Token("Surface", dark: false), polar.LabelsBackground.AsSKColor());
    }

    [AvaloniaFact]
    public void A_Measured_Chart_Keeps_The_Horizontal_Grid_And_Paints_Its_Series_From_The_Palette()
    {
        // The orientation an axis gets, and therefore which of the pair draws separators, is only
        // written while a chart measures that axis, so this goes through a real chart. A chart
        // measures while its canvas draws frames, and a headless test never draws any; LiveCharts
        // ships an internal flag that lifts that check for unit tests, and it is the only way to
        // drive the measure without a render loop.
        EnableTestMeasures();

        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        LiveCharts.Configure(settings => settings.AddMaterialTheme());

        var xAxis = new Axis();
        var yAxis = new Axis();
        var series = new ColumnSeries<double> { Values = [1, 2, 3] };
        var chart = new CartesianChart
        {
            XAxes = [xAxis],
            YAxes = [yAxis],
            Series = [series],
        };

        var window = new Window { Width = 640, Height = 400, Content = chart };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            // Measure is the pass that styles planes and series; ApplyTheme runs it.
            chart.CoreChart.ApplyTheme();

            // The chart read the application's variant and gave each axis its orientation, and the
            // separator rule read that back: vertical grid lines are not drawn, horizontal ones are
            // the OutlineVariant rule.
            var theme = chart.CoreChart.GetTheme();
            Assert.False(theme.IsDark);
            Assert.Equal(AxisOrientation.X, xAxis.Orientation);
            Assert.Null(xAxis.SeparatorsPaint);
            Assert.Equal(AxisOrientation.Y, yAxis.Orientation);
            Assert.Equal(Token("OutlineVariant", dark: false), Assert.IsType<SolidColorPaint>(yAxis.SeparatorsPaint).Color);

            // The same rule styled the sizes the bare axis took above.
            Assert.Equal(Size("Md.Sys.TypeScale.LabelMedium.Size"), xAxis.TextSize);
            Assert.Equal(12f, yAxis.Padding.Left);

            // And the series the chart measured took its colour out of the theme's palette - the
            // chart gave it a slot before it styled it, which is what the rule read.
            Assert.Equal(MaterialLiveChartsTheme.SeriesPalette(false), theme.Colors);
            Assert.Equal(0, ((ISeries)series).SeriesId);
            Assert.Equal(theme.GetSeriesColor(series).AsSKColor(), Assert.IsType<SolidColorPaint>(series.Fill).Color);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Pie_Gauge_Scatter_And_Candlestick_Series_Keep_The_Material_Roles()
    {
        var theme = MaterialRules(dark: false);

        // Adjacent slices are separated by the surface showing through rather than by a border.
        var pie = new PieSeries<double> { Values = [1, 2] };
        theme.ApplyStyleToSeries(pie);
        var gap = Assert.IsType<SolidColorPaint>(pie.Stroke);
        Assert.Equal(Token("Surface", dark: false), gap.Color);
        Assert.Equal(2f, gap.StrokeThickness);
        Assert.Equal(MaterialLiveChartsTheme.SeriesPalette(false)[0].AsSKColor(), Assert.IsType<SolidColorPaint>(pie.Fill).Color);

        // A gauge is a neutral track with a primary indicator: the shape of Material's own ring.
        var gauge = new PieSeries<double>(isGauge: true);
        theme.ApplyStyleToSeries(gauge);
        Assert.Equal(Token("SurfaceContainerHighest", dark: false), Assert.IsType<SolidColorPaint>(gauge.Fill).Color);
        var gaugeFill = new PieSeries<double>(isGauge: true, isGaugeFill: true);
        theme.ApplyStyleToSeries(gaugeFill);
        Assert.Equal(Token("Primary", dark: false), Assert.IsType<SolidColorPaint>(gaugeFill.Fill).Color);

        var scatter = new ScatterSeries<double> { Values = [1, 2] };
        theme.ApplyStyleToSeries(scatter);
        Assert.Null(scatter.Stroke);
        Assert.Equal(MaterialLiveChartsTheme.SeriesPalette(false)[0].AsSKColor(), Assert.IsType<SolidColorPaint>(scatter.Fill).Color);

        // Candles leave upstream's green and red behind: the third accent rises, the error role falls.
        var candles = new CandlesticksSeries<double> { UpFill = null, DownFill = null };
        theme.ApplyStyleToSeries(candles);
        Assert.Equal(Token("Tertiary", dark: false), Assert.IsType<SolidColorPaint>(candles.UpFill).Color);
        Assert.Equal(Token("Error", dark: false), Assert.IsType<SolidColorPaint>(candles.DownFill).Color);
    }

    [AvaloniaFact]
    public void The_Dark_Variant_Reads_The_Dark_Tokens()
    {
        // Nothing here may be a light-mode literal: this theme is the same object in both variants,
        // and only theme.IsDark - which LiveCharts sets from the view, from the application's
        // variant - decides which token each rule reads.
        var theme = MaterialRules(dark: true);

        Assert.True(theme.IsDark);
        Assert.Equal(MaterialLiveChartsTheme.SeriesPalette(true), theme.Colors);

        var series = new LineSeries<double> { Values = [1, 2, 3] };
        theme.ApplyStyleToSeries(series);
        var stroke = Assert.IsType<SolidColorPaint>(series.Stroke);
        Assert.Equal(MaterialLiveChartsTheme.SeriesPalette(true)[0].AsSKColor(), stroke.Color);

        // The palette the rule read that colour out of is the dark one - the same slot is a
        // different colour under the light variant, so a hardcoded light value would show here.
        Assert.Equal(Token("Primary", dark: true), theme.Colors[0].AsSKColor());
        Assert.NotEqual(Token("Primary", dark: false), theme.Colors[0].AsSKColor());

        var axis = new Axis();
        theme.ApplyStyleToAxis(axis);
        Assert.Equal(Token("OnSurfaceVariant", dark: true), Assert.IsType<SolidColorPaint>(axis.LabelsPaint).Color);
        Assert.NotEqual(Token("OnSurfaceVariant", dark: false), Assert.IsType<SolidColorPaint>(axis.LabelsPaint).Color);
    }

    [AvaloniaFact]
    public void The_Package_Is_A_Configuration_Seam_And_Reaches_No_Style_Tree()
    {
        var assembly = typeof(MaterialLiveChartsTheme).Assembly;

        // The whole public surface is the configuration entry point. There is no Styles, Style or
        // ControlTheme anywhere in the package, which is what makes it impossible for the LiveCharts
        // look to leak into an application that did not configure it.
        Assert.Equal("MaterialLiveChartsTheme", Assert.Single(assembly.GetTypes(), type => type.IsPublic).Name);
        Assert.DoesNotContain(assembly.GetTypes(), type =>
            typeof(Styles).IsAssignableFrom(type) ||
            typeof(Style).IsAssignableFrom(type) ||
            typeof(ControlTheme).IsAssignableFrom(type));
    }

    /// <summary>
    /// Lifts the check that stops a chart from measuring while its canvas is not drawing frames.
    /// </summary>
    /// <remarks>
    /// <c>Chart.IsRendering()</c> compares the canvas' last frame timestamp with the one its last
    /// measure ran at, and a headless test never draws a frame. LiveCharts' own
    /// <c>CoreMotionCanvas.IsTesting</c> flag exists to remove that check in unit tests, and it is
    /// internal to the core package, so it is set here.
    /// </remarks>
    private static void EnableTestMeasures()
    {
        var isTesting = typeof(CoreMotionCanvas).GetProperty("IsTesting", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(isTesting);
        isTesting!.SetValue(null, true);
    }

    /// <summary>The one rule a theme holds for a visual element type.</summary>
    private static Action<TChartElement> ElementRule<TChartElement>(Theme theme) =>
        (Action<TChartElement>)theme.ChartElementElementBuilder[typeof(TChartElement)];

    /// <summary>Puts a series in the palette slot a chart would give it.</summary>
    private static T InSlot<T>(T series, int slot) where T : ISeries
    {
        series.SeriesId = slot;
        return series;
    }

    /// <summary>A theme with the Material rules on it, pinned to one variant.</summary>
    private static Theme MaterialRules(bool dark)
    {
        // A theme of the caller's own, which is the second documented entry point: the rules are
        // appended to whatever theme is already there. RequestedTheme pins the variant the way
        // LiveCharts pins it from the view, so both variants can be driven without a window.
        var theme = new Theme { RequestedTheme = dark ? LvcThemeKind.Dark : LvcThemeKind.Light };
        theme.ApplyMaterialRules();
        return theme;
    }

    // Md.Sys.* tokens live in the core theme's dictionaries, which TestApplication registers, and
    // they are read from the same resource host the other theme tests use.
    private static SKColor Token(string role, bool dark)
    {
        var variant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        Assert.True(Application.Current!.TryGetResource($"Md.Sys.Color.{role}", variant, out var value),
            $"Md.Sys.Color.{role} is missing from the {(dark ? "dark" : "light")} variant");
        var color = Assert.IsType<Color>(value);
        return new SKColor(color.R, color.G, color.B, color.A);
    }

    private static double Size(string key)
    {
        Assert.True(Application.Current!.TryGetResource(key, null, out var value), key);
        return Assert.IsType<double>(value);
    }
}
