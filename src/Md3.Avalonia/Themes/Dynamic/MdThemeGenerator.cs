using Avalonia.Media;
using MaterialColorUtilities.ColorAppearance;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Schemes;

namespace Md3.Avalonia.Themes.Dynamic;

/// <summary>
/// Generates Material dynamic color roles from a seed using the HCT-based Material Color
/// Utilities implementation. The output includes standard, fixed and surface-container roles.
/// </summary>
public static class MdThemeGenerator
{
    public static IReadOnlyDictionary<string, Color> Generate(MdThemeOptions options, bool dark)
    {
        var seed = ParseSeed(options.SeedColor);
        var palette = new CorePalette();
        palette.Fill(seed, ToStyle(options.SchemeVariant));
        if (options.SchemeVariant == MdThemeSchemeVariant.Monochrome)
        {
            var hue = Hct.FromInt(seed).Hue;
            palette.Primary = TonalPalette.FromHueAndChroma(hue, 0);
            palette.Secondary = TonalPalette.FromHueAndChroma(hue, 0);
            palette.Tertiary = TonalPalette.FromHueAndChroma(hue, 0);
            palette.Neutral = TonalPalette.FromHueAndChroma(hue, 0);
            palette.NeutralVariant = TonalPalette.FromHueAndChroma(hue, 0);
        }
        var scheme = dark
            ? new DarkSchemeMapper().Map(palette)
            : new LightSchemeMapper().Map(palette);

        var roles = new Dictionary<string, uint>(StringComparer.Ordinal)
        {
            ["Primary"] = scheme.Primary,
            ["OnPrimary"] = scheme.OnPrimary,
            ["PrimaryContainer"] = scheme.PrimaryContainer,
            ["OnPrimaryContainer"] = scheme.OnPrimaryContainer,
            ["Secondary"] = scheme.Secondary,
            ["OnSecondary"] = scheme.OnSecondary,
            ["SecondaryContainer"] = scheme.SecondaryContainer,
            ["OnSecondaryContainer"] = scheme.OnSecondaryContainer,
            ["Tertiary"] = scheme.Tertiary,
            ["OnTertiary"] = scheme.OnTertiary,
            ["TertiaryContainer"] = scheme.TertiaryContainer,
            ["OnTertiaryContainer"] = scheme.OnTertiaryContainer,
            ["Error"] = scheme.Error,
            ["OnError"] = scheme.OnError,
            ["ErrorContainer"] = scheme.ErrorContainer,
            ["OnErrorContainer"] = scheme.OnErrorContainer,
            ["Background"] = scheme.Background,
            ["OnBackground"] = scheme.OnBackground,
            ["Surface"] = scheme.Surface,
            ["OnSurface"] = scheme.OnSurface,
            ["SurfaceVariant"] = scheme.SurfaceVariant,
            ["OnSurfaceVariant"] = scheme.OnSurfaceVariant,
            ["SurfaceTint"] = scheme.Primary,
            ["Outline"] = scheme.Outline,
            ["OutlineVariant"] = scheme.OutlineVariant,
            ["Shadow"] = scheme.Shadow,
            ["Scrim"] = palette.Neutral[0],
            ["InverseSurface"] = scheme.InverseSurface,
            ["InverseOnSurface"] = scheme.InverseOnSurface,
            ["InversePrimary"] = scheme.InversePrimary,
            ["SurfaceDim"] = scheme.SurfaceDim,
            ["SurfaceBright"] = scheme.SurfaceBright,
            ["SurfaceContainerLowest"] = scheme.SurfaceContainerLowest,
            ["SurfaceContainerLow"] = scheme.SurfaceContainerLow,
            ["SurfaceContainer"] = scheme.SurfaceContainer,
            ["SurfaceContainerHigh"] = scheme.SurfaceContainerHigh,
            ["SurfaceContainerHighest"] = scheme.SurfaceContainerHighest,
            ["PrimaryFixed"] = palette.Primary[90],
            ["PrimaryFixedDim"] = palette.Primary[80],
            ["OnPrimaryFixed"] = palette.Primary[10],
            ["OnPrimaryFixedVariant"] = palette.Primary[30],
            ["SecondaryFixed"] = palette.Secondary[90],
            ["SecondaryFixedDim"] = palette.Secondary[80],
            ["OnSecondaryFixed"] = palette.Secondary[10],
            ["OnSecondaryFixedVariant"] = palette.Secondary[30],
            ["TertiaryFixed"] = palette.Tertiary[90],
            ["TertiaryFixedDim"] = palette.Tertiary[80],
            ["OnTertiaryFixed"] = palette.Tertiary[10],
            ["OnTertiaryFixedVariant"] = palette.Tertiary[30]
        };

        ApplyContrast(roles, palette, options.ContrastLevel, dark);
        return roles.ToDictionary(pair => pair.Key, pair => ToColor(pair.Value), StringComparer.Ordinal);
    }

    public static uint ParseSeed(string value)
    {
        var color = Color.Parse(value);
        return ((uint)color.A << 24) | ((uint)color.R << 16) | ((uint)color.G << 8) | color.B;
    }

    private static Style ToStyle(MdThemeSchemeVariant variant) => variant switch
    {
        MdThemeSchemeVariant.Neutral => Style.Spritz,
        MdThemeSchemeVariant.Vibrant => Style.Vibrant,
        MdThemeSchemeVariant.Expressive => Style.Expressive,
        MdThemeSchemeVariant.Monochrome => Style.Spritz,
        MdThemeSchemeVariant.Fidelity => Style.Content,
        _ => Style.TonalSpot
    };

    private static void ApplyContrast(Dictionary<string, uint> roles, CorePalette palette,
        MdThemeContrastLevel level, bool dark)
    {
        if (level == MdThemeContrastLevel.Standard) return;
        var foregroundTone = dark ? 0u : 100u;
        var variantTone = dark
            ? level == MdThemeContrastLevel.High ? 95u : 90u
            : level == MdThemeContrastLevel.High ? 5u : 10u;
        var outlineTone = dark
            ? level == MdThemeContrastLevel.High ? 90u : 80u
            : level == MdThemeContrastLevel.High ? 20u : 30u;

        roles["OnPrimary"] = palette.Primary[foregroundTone];
        roles["OnSecondary"] = palette.Secondary[foregroundTone];
        roles["OnTertiary"] = palette.Tertiary[foregroundTone];
        roles["OnError"] = palette.Error[foregroundTone];
        roles["OnSurface"] = palette.Neutral[variantTone];
        roles["OnSurfaceVariant"] = palette.NeutralVariant[variantTone];
        roles["Outline"] = palette.NeutralVariant[outlineTone];
    }

    private static Color ToColor(uint argb) => new(
        (byte)(argb >> 24),
        (byte)(argb >> 16),
        (byte)(argb >> 8),
        (byte)argb);
}
