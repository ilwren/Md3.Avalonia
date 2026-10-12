using System;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using LiveChartsCore.Themes;
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
    public void Configuring_LiveCharts_Installs_One_Material_Theme()
    {
        // The documented setup, run exactly as an application runs it. Nothing else is needed: the
        // theme arrives with the tooltip and legend factories LiveCharts' own default theme brings,
        // which is what makes the layered CallAddMaterialTheme the shorter path.
        LiveCharts.Configure(settings => settings.AddMaterialTheme());

        // A chart view is what sets a theme up - GetTheme() runs the theme's initialization for the
        // variant the view draws in, so this is also the step that fills the palette.
        var theme = new CartesianChart().GetTheme();

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
    public void A_Line_Series_Is_A_Stroke_With_Surface_Filled_Markers()
    {
        var theme = MaterialRules(dark: false);
        var series = new LineSeries<double> { Values = [1, 2, 3] };

        theme.ApplyStyleToSeries(series);

        // The rule paints the series with the colour the theme holds for it, not with a literal.
        var stroke = Assert.IsType<SolidColorPaint>(series.Stroke);
        Assert.Equal(theme.GetSeriesColor(series).AsSKColor(), stroke.Color);
        Assert.Equal(3f, stroke.StrokeThickness);

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
        Assert.Equal(theme.GetSeriesColor(column).AsSKColor(), Assert.IsType<SolidColorPaint>(column.Fill).Color);
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
    public void An_Axis_Takes_Typescale_Sizes_Medium_Emphasis_Ink_And_A_OutlineVariant_Grid()
    {
        var theme = MaterialRules(dark: false);

        var yAxis = new Axis { Orientation = AxisOrientation.Y };
        theme.ApplyStyleToAxis(yAxis);

        Assert.Equal(Size("Md.Sys.TypeScale.LabelMedium.Size"), yAxis.TextSize);
        Assert.Equal(Size("Md.Sys.TypeScale.LabelLarge.Size"), yAxis.NameTextSize);
        Assert.Equal(Token("OnSurfaceVariant", dark: false), Assert.IsType<SolidColorPaint>(yAxis.LabelsPaint).Color);
        Assert.Equal(Token("OnSurfaceVariant", dark: false), Assert.IsType<SolidColorPaint>(yAxis.NamePaint).Color);

        // Horizontal grid lines are the rule colour; vertical ones stay off, as upstream draws it.
        Assert.Equal(Token("OutlineVariant", dark: false), Assert.IsType<SolidColorPaint>(yAxis.SeparatorsPaint).Color);
        Assert.True(yAxis.ShowSeparatorLines);
        var xAxis = new Axis { Orientation = AxisOrientation.X };
        theme.ApplyStyleToAxis(xAxis);
        Assert.Null(xAxis.SeparatorsPaint);

        Assert.NotNull(yAxis.Padding);
        Assert.Equal(12d, yAxis.Padding.Left);
        Assert.Equal(12d, yAxis.Padding.Top);

        // A polar plane has no "horizontal" line pair, so it gets the full OutlineVariant grid and a
        // surface-coloured label background.
        var polar = new PolarAxis();
        theme.ApplyStyleToAxis(polar);
        Assert.Equal(Token("OutlineVariant", dark: false), Assert.IsType<SolidColorPaint>(polar.SeparatorsPaint).Color);
        Assert.Equal(Token("Surface", dark: false), polar.LabelsBackground.AsSKColor());
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
        Assert.Equal(theme.GetSeriesColor(pie).AsSKColor(), Assert.IsType<SolidColorPaint>(pie.Fill).Color);

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
        Assert.Equal(theme.GetSeriesColor(scatter).AsSKColor(), Assert.IsType<SolidColorPaint>(scatter.Fill).Color);

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

        Assert.Equal(MaterialLiveChartsTheme.SeriesPalette(true), theme.Colors);
        Assert.Equal(Token("Surface", dark: true), theme.VirtualBackroundColor.AsSKColor());
        Assert.Equal(Token("InverseSurface", dark: true), Assert.IsType<SolidColorPaint>(theme.TooltipBackgroundPaint).Color);

        var series = new LineSeries<double> { Values = [1, 2, 3] };
        theme.ApplyStyleToSeries(series);
        var stroke = Assert.IsType<SolidColorPaint>(series.Stroke);
        Assert.Equal(theme.GetSeriesColor(series).AsSKColor(), stroke.Color);

        // The palette the rule read that colour out of is the dark one - the same slot is a
        // different colour under the light variant, so a hardcoded light value would show here.
        Assert.Equal(Token("Primary", dark: true), theme.Colors[0].AsSKColor());
        Assert.NotEqual(Token("Primary", dark: false), theme.Colors[0].AsSKColor());

        var axis = new Axis { Orientation = AxisOrientation.Y };
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
        Assert.Empty(assembly.GetTypes().Where(type =>
            typeof(Styles).IsAssignableFrom(type) ||
            typeof(Style).IsAssignableFrom(type) ||
            typeof(ControlTheme).IsAssignableFrom(type)));
    }

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
