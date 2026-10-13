using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using MapsuiColor = Mapsui.Styles.Color;

namespace Md3.Avalonia.Themes;

/// <summary>
/// Reads the Material colour, type-scale, typeface and shape tokens a map needs, for one variant.
/// </summary>
/// <remarks>
/// <para>
/// Mapsui draws with its own <see cref="MapsuiColor"/>, pen and brush types and never asks an
/// Avalonia resource host for anything, so there is no style tree to walk up from: the tokens are
/// looked up on <see cref="Application"/> - which is where <c>MaterialTheme</c> puts them - for the
/// variant the map is drawing in. An application that does not register <c>MaterialTheme</c> at all
/// (or registers it on a window rather than on the application) gets the M3 baseline values below, so
/// a map is themed either way.
/// </para>
/// <para>
/// The fallbacks are the same baseline <c>Md3.Avalonia.ScottPlot</c> and
/// <c>Md3.Avalonia.LiveCharts2</c> carry, so the three providers agree on what a role means when no
/// token is registered.
/// </para>
/// </remarks>
internal static class MdMapsuiTokens
{
    /// <summary>The M3 baseline value for every role this theme reads, light first.</summary>
    private static readonly Dictionary<string, (string Light, string Dark)> ColorFallbacks = new()
    {
        ["Primary"] = ("#6750A4", "#D0BCFF"),
        ["PrimaryContainer"] = ("#EADDFF", "#4F378B"),
        ["Secondary"] = ("#625B71", "#CCC2DC"),
        ["Tertiary"] = ("#7D5260", "#EFB8C8"),
        ["TertiaryContainer"] = ("#FFD8E4", "#633B48"),
        ["InversePrimary"] = ("#D0BCFF", "#6750A4"),
        ["Error"] = ("#B3261E", "#F2B8B5"),
        ["Surface"] = ("#FFFBFE", "#141218"),
        ["SurfaceContainer"] = ("#F3EDF7", "#211F26"),
        ["OnSurface"] = ("#1D1B20", "#E6E0E9"),
        ["OnSurfaceVariant"] = ("#49454F", "#CAC4D0"),
        ["Outline"] = ("#79747E", "#938F99"),
        ["OutlineVariant"] = ("#CAC4D0", "#49454F"),
    };

    /// <summary>The M3 baseline family of the <c>Md.Sys.Typeface.Plain</c> token.</summary>
    private const string FontFamilyFallback = "Arial";

    /// <summary>Reads <c>Md.Sys.Color.&lt;role&gt;</c> as a Mapsui colour.</summary>
    /// <param name="role">The Material role, named without the <c>Md.Sys.Color.</c> prefix.</param>
    /// <param name="dark">Whether the dark variant's token should be read.</param>
    /// <returns>The role's colour, or its M3 baseline value when no token is registered.</returns>
    public static MapsuiColor Color(string role, bool dark)
    {
        if (Application.Current is { } application &&
            application.TryGetResource($"Md.Sys.Color.{role}", dark ? ThemeVariant.Dark : ThemeVariant.Light, out var value) &&
            value is Color color)
        {
            return new MapsuiColor(color.R, color.G, color.B, color.A);
        }

        return FromHex(dark ? ColorFallbacks[role].Dark : ColorFallbacks[role].Light);
    }

    /// <summary>
    /// Reads <c>Md.Sys.Color.&lt;role&gt;</c> and fades it to a fraction of its own opacity.
    /// </summary>
    /// <param name="role">The Material role, named without the <c>Md.Sys.Color.</c> prefix.</param>
    /// <param name="dark">Whether the dark variant's token should be read.</param>
    /// <param name="opacity">The fraction of the colour's alpha to keep, from 0 to 1.</param>
    /// <returns>The faded colour.</returns>
    /// <remarks>
    /// Mapsui's brush takes a colour rather than an opacity, so the fill of a region - which Material
    /// draws at 12 percent of its accent - has to be mixed here instead of set on the brush.
    /// </remarks>
    public static MapsuiColor Color(string role, bool dark, double opacity)
    {
        var color = Color(role, dark);
        return new MapsuiColor(color.R, color.G, color.B, (int)(color.A * opacity));
    }

    /// <summary>
    /// Reads <c>Md.Sys.TypeScale.&lt;role&gt;.Size</c> as the size a Mapsui font size takes.
    /// </summary>
    /// <param name="role">The type-scale role, named without the <c>Md.Sys.TypeScale.</c> prefix.</param>
    /// <param name="fallback">The size to use when no token is registered.</param>
    /// <returns>The role's size in pixels.</returns>
    /// <remarks>
    /// The type scale is not variant specific, so it is read against the application's current
    /// variant, exactly like the resource lookup the other themes go through.
    /// </remarks>
    public static double Size(string role, double fallback)
    {
        if (Application.Current is { } application &&
            application.TryGetResource($"Md.Sys.TypeScale.{role}.Size", null, out var value) &&
            value is double size)
        {
            return size;
        }

        return fallback;
    }

    /// <summary>Reads <c>Md.Sys.Shape.Corner.&lt;role&gt;</c> as a corner radius in pixels.</summary>
    /// <param name="role">The corner role, named without the <c>Md.Sys.Shape.Corner.</c> prefix.</param>
    /// <param name="fallback">The radius to use when no token is registered.</param>
    /// <returns>The role's radius.</returns>
    public static double Corner(string role, double fallback)
    {
        if (Application.Current is { } application &&
            application.TryGetResource($"Md.Sys.Shape.Corner.{role}", null, out var value) &&
            value is CornerRadius corner)
        {
            return corner.TopLeft;
        }

        return fallback;
    }

    /// <summary>Reads <c>Md.Sys.Typeface.Plain</c> as the family name Mapsui resolves a font from.</summary>
    /// <remarks>
    /// The name is only written onto a style; Mapsui resolves it when it draws, and falls back to a
    /// system font when the family is not installed, so a map whose app carries the token still draws
    /// on a machine that has no such font.
    /// </remarks>
    public static string FontFamily
    {
        get
        {
            if (Application.Current is { } application &&
                application.TryGetResource("Md.Sys.Typeface.Plain", null, out var value) &&
                value is FontFamily family)
            {
                return family.Name;
            }

            return FontFamilyFallback;
        }
    }

    /// <summary>Parses a <c>#RRGGBB</c> baseline value into a Mapsui colour.</summary>
    private static MapsuiColor FromHex(string hex) => new(
        int.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture),
        int.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture),
        int.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture));
}
