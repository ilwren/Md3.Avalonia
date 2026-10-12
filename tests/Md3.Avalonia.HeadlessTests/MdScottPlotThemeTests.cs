using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Md3.Avalonia.Themes;
using ScottPlot;
using ScottPlot.Avalonia;
using Xunit;
using AvaloniaColor = Avalonia.Media.Color;
using PlotColor = ScottPlot.Color;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Pins what <see cref="MaterialScottPlotTheme"/> does to a plot: the palette, the colours, the type
/// scale, and the fact that it touches only the plot it was handed.
/// </summary>
public sealed class MdScottPlotThemeTests
{
    [AvaloniaFact]
    public void The_Palette_Walks_The_Eight_Material_Roles_In_Order()
    {
        var roles = MaterialScottPlotTheme.SeriesPaletteRoles;
        var expected = new[]
        {
            "Primary", "Tertiary", "Secondary", "Error", "PrimaryContainer", "TertiaryContainer", "InversePrimary", "Outline",
        };
        Assert.Equal(expected, roles.ToArray());

        var light = MaterialScottPlotTheme.SeriesPalette(dark: false);
        Assert.Equal(roles.Select(role => Token(role, dark: false)), light.Colors);
        Assert.Equal("Material 3", light.Name);

        var dark = MaterialScottPlotTheme.SeriesPalette(dark: true);
        Assert.Equal(roles.Select(role => Token(role, dark: true)), dark.Colors);
        Assert.Equal("Material 3 (dark)", dark.Name);

        // The two variants are genuinely different palettes, not one palette reused.
        Assert.NotEqual(light.Colors, dark.Colors);
    }

    [AvaloniaFact]
    public void Applying_The_Theme_Styles_The_Surfaces_Ink_Grid_Legend_And_Type_Scale()
    {
        var plot = new Plot();
        new MaterialScottPlotTheme(dark: false).Apply(plot);

        // A Material chart is one surface: the figure and the data area are the same plane.
        Assert.Equal(Token("Surface", dark: false), plot.FigureBackground.Color);
        Assert.Equal(Token("Surface", dark: false), plot.DataBackground.Color);

        // The medium-emphasis ink carries the axes, their names, the title and the legend text.
        var ink = Token("OnSurfaceVariant", dark: false);
        Assert.Equal(ink, plot.Axes.Bottom.Label.ForeColor);
        Assert.Equal(ink, plot.Axes.Bottom.TickLabelStyle.ForeColor);
        Assert.Equal(ink, plot.Axes.Bottom.MajorTickStyle.Color);
        Assert.Equal(ink, plot.Axes.Title.Label.ForeColor);
        Assert.Equal(ink, plot.Legend.FontColor);

        // One OutlineVariant grid at major ticks, no minor lines.
        Assert.Equal(Token("OutlineVariant", dark: false), plot.Grid.MajorLineColor);
        Assert.Equal(0f, plot.Grid.MinorLineWidth);

        // The legend floats over the data, so it is a surface container with a soft outline.
        Assert.Equal(Token("SurfaceContainerHigh", dark: false), plot.Legend.BackgroundColor);
        Assert.Equal(Token("OutlineVariant", dark: false), plot.Legend.OutlineColor);

        // The type scale: TitleMedium title, LabelLarge axis names and legend, LabelMedium ticks.
        Assert.Equal(Size("TitleMedium"), plot.Axes.Title.Label.FontSize);
        Assert.Equal(Size("LabelLarge"), plot.Axes.Bottom.Label.FontSize);
        Assert.Equal(Size("LabelMedium"), plot.Axes.Bottom.TickLabelStyle.FontSize);
        Assert.Equal(Size("LabelLarge"), plot.Legend.FontSize);

        // All of it in the family the typeface token names.
        var family = Typeface();
        Assert.Equal(family, plot.Axes.Title.Label.FontName);
        Assert.Equal(family, plot.Axes.Bottom.Label.FontName);
        Assert.Equal(family, plot.Axes.Bottom.TickLabelStyle.FontName);
        Assert.Equal(family, plot.Legend.FontName);

        // The series palette is the Material one, through the plot's own plottable adder.
        Assert.Equal(MaterialScottPlotTheme.SeriesPalette(dark: false).Colors, plot.Add.Palette.Colors);
    }

    [AvaloniaFact]
    public void The_Dark_Variant_Reads_The_Dark_Tokens()
    {
        var light = new Plot();
        MaterialScottPlotTheme.Apply(light, dark: false);

        var dark = new Plot();
        MaterialScottPlotTheme.Apply(dark, dark: true);

        Assert.Equal(Token("Surface", dark: true), dark.FigureBackground.Color);
        Assert.Equal(Token("OnSurfaceVariant", dark: true), dark.Axes.Bottom.TickLabelStyle.ForeColor);
        Assert.Equal(Token("OutlineVariant", dark: true), dark.Grid.MajorLineColor);
        Assert.Equal(Token("SurfaceContainerHigh", dark: true), dark.Legend.BackgroundColor);
        Assert.Equal(MaterialScottPlotTheme.SeriesPalette(dark: true).Colors, dark.Add.Palette.Colors);

        // The two variants do not produce the same plot.
        Assert.NotEqual(light.FigureBackground.Color, dark.FigureBackground.Color);
        Assert.NotEqual(light.Add.Palette.Colors, dark.Add.Palette.Colors);
    }

    [AvaloniaFact]
    public void Following_A_Control_Keeps_It_In_Step_With_The_Applications_Variant()
    {
        var application = Application.Current!;
        var previous = application.RequestedThemeVariant;

        var plot = new AvaPlot();
        var window = new Window { Width = 640, Height = 400, Content = plot };
        window.Show();
        try
        {
            // ScottPlot bakes colours into a plot when they are set, so the theme tracks the
            // control's own variant instead of leaving the plot to re-resolve anything.
            application.RequestedThemeVariant = ThemeVariant.Light;
            MaterialScottPlotTheme.Follow(plot);
            Assert.Equal(Token("Surface", dark: false), plot.Plot.FigureBackground.Color);

            application.RequestedThemeVariant = ThemeVariant.Dark;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(Token("Surface", dark: true), plot.Plot.FigureBackground.Color);
            Assert.Equal(Token("OnSurfaceVariant", dark: true), plot.Plot.Legend.FontColor);
        }
        finally
        {
            window.Close();
            application.RequestedThemeVariant = previous;
        }

        // The palette follows the same switch.
        Assert.Equal(Token("Primary", dark: true), plot.Plot.Add.Palette.GetColor(0));
    }

    [AvaloniaFact]
    public void A_Plot_That_Was_Not_Asked_For_Keeps_ScottPlot_Defaults()
    {
        var styles = new Plot();
        var untouched = new Plot();

        var fontsDefault = Fonts.Default;
        var fontsDefaultStyle = Fonts.DefaultFontStyle;

        new MaterialScottPlotTheme(dark: false).Apply(styles);

        // The plot that was handed to the theme moved...
        Assert.Equal(Token("Surface", dark: false), styles.FigureBackground.Color);

        // ...and the one that was not kept every ScottPlot default.
        Assert.Equal(Colors.White, untouched.FigureBackground.Color);
        Assert.Equal(Colors.Transparent, untouched.DataBackground.Color);
        Assert.Equal(Colors.Black, untouched.Axes.Bottom.TickLabelStyle.ForeColor);
        Assert.Equal("Category 10", untouched.Add.Palette.Name);
        Assert.Null(untouched.Legend.FontSize);

        // Nothing process-wide was rewritten either: a theme that set Fonts.Default would change
        // every plot drawn after it, including plots this package was never asked to style.
        Assert.Equal(fontsDefault, Fonts.Default);
        Assert.Same(fontsDefaultStyle, Fonts.DefaultFontStyle);
    }

    [AvaloniaFact]
    public void The_Material_Baselines_Cover_Every_Role_And_Match_The_Core_Tokens()
    {
        // The theme falls back to the M3 baselines when an application does not register
        // MaterialTheme, so every role it can ask for needs one - a missing key would be a crash at
        // chart-configure time, and a stale value would paint a colour the core theme never uses.
        var tokens = typeof(MaterialScottPlotTheme).Assembly.GetType("Md3.Avalonia.Themes.MdScottPlotTokens");
        Assert.NotNull(tokens);
        var field = tokens!.GetField("ColorFallbacks", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(field);
        var fallbacks = Assert.IsType<Dictionary<string, (string Light, string Dark)>>(field!.GetValue(null));

        foreach (var role in MaterialScottPlotTheme.SeriesPaletteRoles)
        {
            Assert.True(fallbacks.TryGetValue(role, out var baseline), $"no M3 baseline for {role}");
            Assert.Equal(Token(role, dark: false), PlotColor.FromHex(baseline.Light));
            Assert.Equal(Token(role, dark: true), PlotColor.FromHex(baseline.Dark));
        }

        foreach (var role in new[] { "Surface", "SurfaceContainerHigh", "OnSurfaceVariant", "OutlineVariant" })
        {
            Assert.True(fallbacks.TryGetValue(role, out var baseline), $"no M3 baseline for {role}");
            Assert.Equal(Token(role, dark: false), PlotColor.FromHex(baseline.Light));
            Assert.Equal(Token(role, dark: true), PlotColor.FromHex(baseline.Dark));
        }
    }

    [AvaloniaFact]
    public void The_Package_Is_A_Plot_Entry_Point_Not_A_Style_Collection()
    {
        var assembly = typeof(MaterialScottPlotTheme).Assembly;

        // One public type, and it is the entry point.
        var publicTypes = assembly.GetTypes().Where(type => type.IsPublic).ToArray();
        Assert.Single(publicTypes, type => type == typeof(MaterialScottPlotTheme));

        // ScottPlot has no style tree to reach, so this package must not carry one: no style, no
        // control theme, nothing that could leak into an application that never asked for it.
        Assert.DoesNotContain(assembly.GetTypes(), type => typeof(IStyle).IsAssignableFrom(type));
        Assert.DoesNotContain(assembly.GetTypes(), type => typeof(ControlTheme).IsAssignableFrom(type));

        // The one-shot calls an application uses really are public.
        var theme = typeof(MaterialScottPlotTheme);
        Assert.NotNull(theme.GetMethod("Apply", [typeof(Plot)]));
        Assert.NotNull(theme.GetMethod("Apply", [typeof(Plot), typeof(bool)]));
        Assert.NotNull(theme.GetMethod("Apply", [typeof(AvaPlot)]));
        Assert.NotNull(theme.GetMethod("Follow", [typeof(AvaPlot)]));
        Assert.NotNull(theme.GetMethod("SeriesPalette", [typeof(bool)]));
    }

    /// <summary>Reads <c>Md.Sys.Color.&lt;role&gt;</c> from the running application.</summary>
    private static PlotColor Token(string role, bool dark)
    {
        Assert.True(
            Application.Current!.TryGetResource($"Md.Sys.Color.{role}", dark ? ThemeVariant.Dark : ThemeVariant.Light, out var value),
            $"Md.Sys.Color.{role} is not registered for the {(dark ? "dark" : "light")} variant");
        var color = Assert.IsType<AvaloniaColor>(value);
        return new PlotColor(color.R, color.G, color.B, color.A);
    }

    /// <summary>Reads <c>Md.Sys.TypeScale.&lt;role&gt;.Size</c> from the running application.</summary>
    private static float Size(string role)
    {
        Assert.True(
            Application.Current!.TryGetResource($"Md.Sys.TypeScale.{role}.Size", null, out var value),
            $"Md.Sys.TypeScale.{role}.Size is not registered");
        return (float)Assert.IsType<double>(value);
    }

    /// <summary>Reads <c>Md.Sys.Typeface.Plain</c> from the running application.</summary>
    private static string Typeface()
    {
        Assert.True(
            Application.Current!.TryGetResource("Md.Sys.Typeface.Plain", null, out var value),
            "Md.Sys.Typeface.Plain is not registered");
        return Assert.IsType<Avalonia.Media.FontFamily>(value).Name;
    }
}
