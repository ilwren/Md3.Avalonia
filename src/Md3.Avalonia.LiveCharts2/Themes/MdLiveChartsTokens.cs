using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using SkiaSharp;

namespace Md3.Avalonia.Themes;

/// <summary>
/// Reads the Material colour and type-scale tokens LiveCharts needs, for one theme variant.
/// </summary>
/// <remarks>
/// LiveCharts hands its theme builders no control, so there is no resource host to walk up from:
/// the tokens are looked up on <see cref="Application"/> - which is where <c>MaterialTheme</c> puts
/// them - for the variant the chart is drawing in. An application that does not register
/// <c>MaterialTheme</c> at all (or registers it on a window rather than on the application) gets
/// the M3 baseline values below, so a chart is themed either way.
/// </remarks>
internal static class MdLiveChartsTokens
{
    /// <summary>The M3 baseline value for every role this theme reads, light first.</summary>
    private static readonly Dictionary<string, (string Light, string Dark)> ColorFallbacks = new()
    {
        ["Primary"] = ("#6750A4", "#D0BCFF"),
        ["PrimaryContainer"] = ("#EADDFF", "#4F378B"),
        ["OnPrimary"] = ("#FFFFFF", "#381E72"),
        ["Secondary"] = ("#625B71", "#CCC2DC"),
        ["Tertiary"] = ("#7D5260", "#EFB8C8"),
        ["TertiaryContainer"] = ("#FFD8E4", "#633B48"),
        ["InversePrimary"] = ("#D0BCFF", "#6750A4"),
        ["Error"] = ("#B3261E", "#F2B8B5"),
        ["Surface"] = ("#FFFBFE", "#141218"),
        ["SurfaceContainerHighest"] = ("#E6E0E9", "#36343B"),
        ["InverseSurface"] = ("#322F35", "#E6E0E9"),
        ["InverseOnSurface"] = ("#F5EFF7", "#322F35"),
        ["OnSurface"] = ("#1D1B20", "#E6E0E9"),
        ["OnSurfaceVariant"] = ("#49454F", "#CAC4D0"),
        ["OutlineVariant"] = ("#CAC4D0", "#49454F"),
    };

    /// <summary>Reads <c>Md.Sys.Color.&lt;role&gt;</c> as a Skia colour.</summary>
    /// <param name="role">The Material role, named without the <c>Md.Sys.Color.</c> prefix.</param>
    /// <param name="dark">Whether the dark variant's token should be read.</param>
    /// <returns>The role's colour, or its M3 baseline value when no token is registered.</returns>
    public static SKColor Color(string role, bool dark)
    {
        if (Application.Current is { } application &&
            application.TryGetResource($"Md.Sys.Color.{role}", dark ? ThemeVariant.Dark : ThemeVariant.Light, out var value) &&
            value is Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        var fallback = ColorFallbacks[role];
        return SKColor.Parse(dark ? fallback.Dark : fallback.Light);
    }

    /// <summary>
    /// Reads <c>Md.Sys.TypeScale.&lt;role&gt;.Size</c> as the DIP size a LiveCharts text size takes.
    /// </summary>
    /// <param name="role">The type-scale role, named without the <c>Md.Sys.TypeScale.</c> prefix.</param>
    /// <param name="fallback">The size to use when no token is registered.</param>
    /// <returns>The role's size in DIP.</returns>
    /// <remarks>
    /// The type scale is not variant specific, so it is read against the application's current
    /// variant, exactly like the resource lookup the other themes go through.
    /// </remarks>
    public static float Size(string role, float fallback)
    {
        if (Application.Current is { } application &&
            application.TryGetResource($"Md.Sys.TypeScale.{role}.Size", null, out var value) &&
            value is double size)
        {
            return (float)size;
        }

        return fallback;
    }
}
