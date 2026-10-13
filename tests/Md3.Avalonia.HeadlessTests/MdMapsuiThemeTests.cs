using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.UI.Avalonia;
using Md3.Avalonia.Themes;
using Xunit;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaFontFamily = Avalonia.Media.FontFamily;
using MapsuiColor = Mapsui.Styles.Color;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Pins what <see cref="MaterialMapsuiTheme"/> does to a map: the palette, the canvas, the vector and
/// label styles, the type scale, and the fact that it touches only what it was handed.
/// </summary>
public sealed class MdMapsuiThemeTests
{
    [AvaloniaFact]
    public void The_Palette_Walks_The_Eight_Material_Roles_In_Order()
    {
        var roles = MaterialMapsuiTheme.SeriesPaletteRoles;
        var expected = new[]
        {
            "Primary", "Tertiary", "Secondary", "Error", "PrimaryContainer", "TertiaryContainer", "InversePrimary", "Outline",
        };
        Assert.Equal(expected, roles.ToArray());

        var light = MaterialMapsuiTheme.SeriesPalette(dark: false);
        Assert.Equal(roles.Count, light.Count);
        for (var i = 0; i < roles.Count; i++)
        {
            AssertColor(Token(roles[i], dark: false), light[i]);
        }

        var dark = MaterialMapsuiTheme.SeriesPalette(dark: true);
        Assert.Equal(roles.Count, dark.Count);
        for (var i = 0; i < roles.Count; i++)
        {
            AssertColor(Token(roles[i], dark: true), dark[i]);
        }

        // The two variants are genuinely different palettes, not one palette reused.
        Assert.True(
            Enumerable.Range(0, roles.Count).Any(i => !SameColor(light[i], dark[i])),
            "the light and dark palettes are the same colours");

        // And the order is the one the ScottPlot and LiveCharts2 providers walk, so a map and a
        // chart beside it agree on which hue is first.
        Assert.Equal(MaterialScottPlotTheme.SeriesPaletteRoles.ToArray(), roles.ToArray());
    }

    [AvaloniaFact]
    public void Applying_The_Theme_Makes_The_Canvas_A_Surface_And_Styles_Layers_Nobody_Has_Styled()
    {
        var map = new Map();

        // Every layer Mapsui creates already carries a style - BaseLayer's constructor gives it a grey
        // outline, a black line and a white fill - so "nobody has styled it" means the style is still
        // that default, not that it is null. This layer has had one number turned down and nothing
        // else, which is the case the theme still has to answer.
        var untouched = new MemoryLayer { Name = "untouched", Features = [] };
        Assert.IsType<VectorStyle>(untouched.Style);
        untouched.Style!.Opacity = 0.4f;

        // These two have been styled by somebody: one picks its own colours, one is labelled.
        var claimed = new VectorStyle { Outline = new Pen(MapsuiColor.Red, 3), Line = null, Fill = null };
        var claimedLayer = new MemoryLayer { Name = "claimed", Features = [], Style = claimed };
        var labelled = new LabelStyle { LabelColumn = "name" };
        var labelledLayer = new MemoryLayer { Name = "labelled", Features = [], Style = labelled };

        map.Layers.Add(untouched);
        map.Layers.Add(claimedLayer);
        map.Layers.Add(labelledLayer);

        // White is the colour a Map ships with, so this assertion proves the theme wrote the canvas
        // rather than reading a value it never set.
        AssertColor(MapsuiColor.White, map.BackColor);

        new MaterialMapsuiTheme(dark: false).Apply(map);

        AssertColor(Token("Surface", dark: false), map.BackColor);

        // The layer nobody styled is given the Material vector default - and keeps the one thing its
        // content had said about it, because only the colours are the theme's to replace.
        var styled = Assert.IsType<VectorStyle>(untouched.Style);
        AssertColor(Token("Outline", dark: false), styled.Outline!.Color);
        Assert.Equal(0.4f, styled.Opacity);

        // A style somebody chose is left exactly as it is: a layer's style is its content's business,
        // and a thematic map picks per feature.
        Assert.Same(claimed, claimedLayer.Style);
        Assert.Same(labelled, labelledLayer.Style);
    }

    [AvaloniaFact]
    public void The_Vector_Style_Is_A_Material_Boundary_Stroke_And_Tint()
    {
        var style = new MaterialMapsuiTheme(dark: false).VectorStyle();

        // A boundary is a hairline in the strong outline; a line feature is heavier, in the
        // medium-emphasis ink.
        AssertColor(Token("Outline", dark: false), style.Outline!.Color);
        Assert.Equal(1, style.Outline.Width);
        AssertColor(Token("OnSurfaceVariant", dark: false), style.Line!.Color);
        Assert.Equal(1.5, style.Line.Width);

        // A region is its accent at twelve percent. Mapsui's brush takes a colour rather than an
        // opacity, so the tint is mixed into the colour instead of set on the brush.
        var fill = style.Fill!.Color!.Value;
        var accent = Token("Primary", dark: false);
        Assert.Equal(accent.R, fill.R);
        Assert.Equal(accent.G, fill.G);
        Assert.Equal(accent.B, fill.B);
        Assert.Equal((int)(accent.A * 0.12), fill.A);
    }

    [AvaloniaFact]
    public void The_Label_Style_Is_A_Material_Chip_Typeset_From_The_Type_Scale()
    {
        var style = new MaterialMapsuiTheme(dark: false).LabelStyle();

        AssertColor(Token("OnSurface", dark: false), style.ForeColor);
        AssertColor(Token("SurfaceContainer", dark: false), style.BackColor!.Color!.Value);
        AssertColor(Token("OutlineVariant", dark: false), style.BorderColor);
        Assert.Equal(1, style.BorderThickness);

        // The corner is the ExtraSmall radius token, not a number the theme invented.
        Assert.Equal((int)Corner("ExtraSmall"), style.CornerRounding);

        // And the type is LabelMedium in the family the application's plain typeface token names.
        Assert.Equal(Size("LabelMedium"), style.Font.Size);
        Assert.Equal(Typeface(), style.Font.FontFamily);
    }

    [AvaloniaFact]
    public void The_Dark_Variant_Reads_The_Dark_Tokens()
    {
        var light = new Map();
        var dark = new Map();

        new MaterialMapsuiTheme(dark: false).Apply(light);
        new MaterialMapsuiTheme(dark: true).Apply(dark);

        AssertColor(Token("Surface", dark: false), light.BackColor);
        AssertColor(Token("Surface", dark: true), dark.BackColor);
        Assert.False(SameColor(light.BackColor, dark.BackColor), "both variants read the same canvas");

        // A theme keeps the variant it was built with, so an application can hold one per variant.
        Assert.False(new MaterialMapsuiTheme(dark: false).IsDark);
        Assert.True(new MaterialMapsuiTheme(dark: true).IsDark);
    }

    [AvaloniaFact]
    public void Following_A_Control_Themes_The_Map_It_Draws()
    {
        var control = new MapControl();

        MaterialMapsuiTheme.Follow(control);

        // Follow applies immediately, for the variant the control has - which, outside a window, is
        // the application's. Re-applying on a variant change is wired to the control's own event, so
        // there is nothing to subscribe to from here and nothing to unsubscribe.
        AssertColor(
            Token("Surface", dark: control.ActualThemeVariant == ThemeVariant.Dark),
            control.Map.BackColor);
    }

    [AvaloniaFact]
    public void The_M3_Baselines_Match_The_Tokens_They_Stand_In_For()
    {
        // An application that never registers MaterialTheme still gets a Material map, from the
        // baselines the package carries. They are the token values, so the two can only drift if one
        // of them is edited - which is what this pins.
        var tokens = typeof(MaterialMapsuiTheme).Assembly.GetType("Md3.Avalonia.Themes.MdMapsuiTokens");
        Assert.NotNull(tokens);

        var field = tokens!.GetField("ColorFallbacks", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(field);
        var fallbacks = Assert.IsType<Dictionary<string, (string Light, string Dark)>>(field!.GetValue(null));

        foreach (var role in MaterialMapsuiTheme.SeriesPaletteRoles.Concat(
                     ["Surface", "SurfaceContainer", "OnSurface", "OnSurfaceVariant", "OutlineVariant"]))
        {
            Assert.True(fallbacks.TryGetValue(role, out var baseline), $"no M3 baseline for {role}");
            AssertColor(Token(role, dark: false), FromHex(baseline.Light));
            AssertColor(Token(role, dark: true), FromHex(baseline.Dark));
        }
    }

    [AvaloniaFact]
    public void The_Package_Is_A_Map_Entry_Point_Not_A_Style_Collection()
    {
        var assembly = typeof(MaterialMapsuiTheme).Assembly;

        // One public type, and it is the entry point.
        var publicTypes = assembly.GetTypes().Where(type => type.IsPublic).ToArray();
        Assert.Single(publicTypes, type => type == typeof(MaterialMapsuiTheme));

        // Mapsui has no style tree to reach, so this package must not carry one: no style, no control
        // theme, nothing that could leak into an application that never asked for it.
        Assert.DoesNotContain(assembly.GetTypes(), type => typeof(IStyle).IsAssignableFrom(type));
        Assert.DoesNotContain(assembly.GetTypes(), type => typeof(ControlTheme).IsAssignableFrom(type));

        // The one-shot calls an application uses really are public.
        var theme = typeof(MaterialMapsuiTheme);
        Assert.NotNull(theme.GetMethod("Apply", [typeof(Map)]));
        Assert.NotNull(theme.GetMethod("Apply", [typeof(Map), typeof(bool)]));
        Assert.NotNull(theme.GetMethod("Apply", [typeof(ILayer)]));
        Assert.NotNull(theme.GetMethod("Follow", [typeof(MapControl)]));
        Assert.NotNull(theme.GetMethod("SeriesPalette", [typeof(bool)]));
    }

    /// <summary>Reads <c>Md.Sys.Color.&lt;role&gt;</c> from the running application.</summary>
    private static MapsuiColor Token(string role, bool dark)
    {
        Assert.True(
            Application.Current!.TryGetResource($"Md.Sys.Color.{role}", dark ? ThemeVariant.Dark : ThemeVariant.Light, out var value),
            $"Md.Sys.Color.{role} is not registered for the {(dark ? "dark" : "light")} variant");
        var color = Assert.IsType<AvaloniaColor>(value);
        return new MapsuiColor(color.R, color.G, color.B, color.A);
    }

    /// <summary>Whether two Mapsui colours are the same colour.</summary>
    /// <remarks>
    /// <see cref="MapsuiColor"/> is a class that overrides no equality member - the only operators it
    /// declares are the casts to and from <c>System.Drawing.Color</c> - so comparing two of them
    /// directly compares references. The components are what a colour is.
    /// </remarks>
    private static bool SameColor(MapsuiColor expected, MapsuiColor actual) =>
        expected.R == actual.R && expected.G == actual.G && expected.B == actual.B && expected.A == actual.A;

    /// <summary>Asserts two Mapsui colours are the same colour.</summary>
    private static void AssertColor(MapsuiColor expected, MapsuiColor actual) =>
        Assert.Equal(
            (expected.R, expected.G, expected.B, expected.A),
            (actual.R, actual.G, actual.B, actual.A));

    /// <summary>Reads <c>Md.Sys.TypeScale.&lt;role&gt;.Size</c> from the running application.</summary>
    private static double Size(string role)
    {
        Assert.True(
            Application.Current!.TryGetResource($"Md.Sys.TypeScale.{role}.Size", null, out var value),
            $"Md.Sys.TypeScale.{role}.Size is not registered");
        return Assert.IsType<double>(value);
    }

    /// <summary>Reads <c>Md.Sys.Shape.Corner.&lt;role&gt;</c> from the running application.</summary>
    private static double Corner(string role)
    {
        Assert.True(
            Application.Current!.TryGetResource($"Md.Sys.Shape.Corner.{role}", null, out var value),
            $"Md.Sys.Shape.Corner.{role} is not registered");
        return Assert.IsType<CornerRadius>(value).TopLeft;
    }

    /// <summary>Reads <c>Md.Sys.Typeface.Plain</c> from the running application.</summary>
    private static string Typeface()
    {
        Assert.True(
            Application.Current!.TryGetResource("Md.Sys.Typeface.Plain", null, out var value),
            "Md.Sys.Typeface.Plain is not registered");
        return Assert.IsType<AvaloniaFontFamily>(value).Name;
    }

    /// <summary>Parses an M3 baseline the package carries, the way the theme itself does.</summary>
    private static MapsuiColor FromHex(string hex) => new(
        int.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture),
        int.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture),
        int.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture));
}
