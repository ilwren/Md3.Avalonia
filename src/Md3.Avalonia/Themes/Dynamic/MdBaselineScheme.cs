using System;
using System.Collections.Generic;

namespace Md3.Avalonia.Themes.Dynamic;

/// <summary>
/// The published Material 3 baseline scheme, transcribed from material-web
/// (<c>tokens/versions/v0_192/_md-sys-color.scss</c> and <c>_md-ref-palette.scss</c>, Apache-2.0).
/// </summary>
/// <remarks>
/// <para>
/// This is the specification, not a generated approximation of it. Material publishes the baseline as
/// literal values with declared provenance; the HCT generator produces a scheme of the same shape from
/// the same seed, but the two are not the same numbers - tone mapping moves #6750A4 to #65558F - so
/// the baseline cannot be reproduced by calling the generator. When a caller asks for the baseline it
/// gets these values; every other seed, variant or contrast level still goes through HCT.
/// </para>
/// <para>
/// The palette is fixed by definition: the fixed roles (<c>PrimaryFixed</c> and friends) are identical
/// in light and dark, which is what makes them fixed. A test asserts that property rather than
/// trusting the transcription.
/// </para>
/// </remarks>
public static class MdBaselineScheme
{
    /// <summary>The M3 source colour. <see cref="MdThemeOptions.SeedColor"/> defaults to it.</summary>
    public const string SeedHex = "#6750A4";

    private static readonly Dictionary<string, uint> LightRoles = new(StringComparer.Ordinal)
    {
        ["Primary"] = 0xFF6750A4u,
        ["OnPrimary"] = 0xFFFFFFFFu,
        ["PrimaryContainer"] = 0xFFEADDFFu,
        ["OnPrimaryContainer"] = 0xFF21005Du,
        ["PrimaryFixed"] = 0xFFEADDFFu,
        ["PrimaryFixedDim"] = 0xFFD0BCFFu,
        ["OnPrimaryFixed"] = 0xFF21005Du,
        ["OnPrimaryFixedVariant"] = 0xFF4F378Bu,
        ["Secondary"] = 0xFF625B71u,
        ["OnSecondary"] = 0xFFFFFFFFu,
        ["SecondaryContainer"] = 0xFFE8DEF8u,
        ["OnSecondaryContainer"] = 0xFF1D192Bu,
        ["SecondaryFixed"] = 0xFFE8DEF8u,
        ["SecondaryFixedDim"] = 0xFFCCC2DCu,
        ["OnSecondaryFixed"] = 0xFF1D192Bu,
        ["OnSecondaryFixedVariant"] = 0xFF4A4458u,
        ["Tertiary"] = 0xFF7D5260u,
        ["OnTertiary"] = 0xFFFFFFFFu,
        ["TertiaryContainer"] = 0xFFFFD8E4u,
        ["OnTertiaryContainer"] = 0xFF31111Du,
        ["TertiaryFixed"] = 0xFFFFD8E4u,
        ["TertiaryFixedDim"] = 0xFFEFB8C8u,
        ["OnTertiaryFixed"] = 0xFF31111Du,
        ["OnTertiaryFixedVariant"] = 0xFF633B48u,
        ["Error"] = 0xFFB3261Eu,
        ["OnError"] = 0xFFFFFFFFu,
        ["ErrorContainer"] = 0xFFF9DEDCu,
        ["OnErrorContainer"] = 0xFF410E0Bu,
        ["Background"] = 0xFFFEF7FFu,
        ["OnBackground"] = 0xFF1D1B20u,
        ["Surface"] = 0xFFFEF7FFu,
        ["OnSurface"] = 0xFF1D1B20u,
        ["SurfaceVariant"] = 0xFFE7E0ECu,
        ["OnSurfaceVariant"] = 0xFF49454Fu,
        ["SurfaceDim"] = 0xFFDED8E1u,
        ["SurfaceBright"] = 0xFFFEF7FFu,
        ["SurfaceContainerLowest"] = 0xFFFFFFFFu,
        ["SurfaceContainerLow"] = 0xFFF7F2FAu,
        ["SurfaceContainer"] = 0xFFF3EDF7u,
        ["SurfaceContainerHigh"] = 0xFFECE6F0u,
        ["SurfaceContainerHighest"] = 0xFFE6E0E9u,
        ["Outline"] = 0xFF79747Eu,
        ["OutlineVariant"] = 0xFFCAC4D0u,
        ["InverseSurface"] = 0xFF322F35u,
        ["InverseOnSurface"] = 0xFFF5EFF7u,
        ["InversePrimary"] = 0xFFD0BCFFu,
        ["Scrim"] = 0xFF000000u,
        ["Shadow"] = 0xFF000000u,
        ["SurfaceTint"] = 0xFF6750A4u,
    };

    private static readonly Dictionary<string, uint> DarkRoles = new(StringComparer.Ordinal)
    {
        ["Primary"] = 0xFFD0BCFFu,
        ["OnPrimary"] = 0xFF381E72u,
        ["PrimaryContainer"] = 0xFF4F378Bu,
        ["OnPrimaryContainer"] = 0xFFEADDFFu,
        ["PrimaryFixed"] = 0xFFEADDFFu,
        ["PrimaryFixedDim"] = 0xFFD0BCFFu,
        ["OnPrimaryFixed"] = 0xFF21005Du,
        ["OnPrimaryFixedVariant"] = 0xFF4F378Bu,
        ["Secondary"] = 0xFFCCC2DCu,
        ["OnSecondary"] = 0xFF332D41u,
        ["SecondaryContainer"] = 0xFF4A4458u,
        ["OnSecondaryContainer"] = 0xFFE8DEF8u,
        ["SecondaryFixed"] = 0xFFE8DEF8u,
        ["SecondaryFixedDim"] = 0xFFCCC2DCu,
        ["OnSecondaryFixed"] = 0xFF1D192Bu,
        ["OnSecondaryFixedVariant"] = 0xFF4A4458u,
        ["Tertiary"] = 0xFFEFB8C8u,
        ["OnTertiary"] = 0xFF492532u,
        ["TertiaryContainer"] = 0xFF633B48u,
        ["OnTertiaryContainer"] = 0xFFFFD8E4u,
        ["TertiaryFixed"] = 0xFFFFD8E4u,
        ["TertiaryFixedDim"] = 0xFFEFB8C8u,
        ["OnTertiaryFixed"] = 0xFF31111Du,
        ["OnTertiaryFixedVariant"] = 0xFF633B48u,
        ["Error"] = 0xFFF2B8B5u,
        ["OnError"] = 0xFF601410u,
        ["ErrorContainer"] = 0xFF8C1D18u,
        ["OnErrorContainer"] = 0xFFF9DEDCu,
        ["Background"] = 0xFF141218u,
        ["OnBackground"] = 0xFFE6E0E9u,
        ["Surface"] = 0xFF141218u,
        ["OnSurface"] = 0xFFE6E0E9u,
        ["SurfaceVariant"] = 0xFF49454Fu,
        ["OnSurfaceVariant"] = 0xFFCAC4D0u,
        ["SurfaceDim"] = 0xFF141218u,
        ["SurfaceBright"] = 0xFF3B383Eu,
        ["SurfaceContainerLowest"] = 0xFF0F0D13u,
        ["SurfaceContainerLow"] = 0xFF1D1B20u,
        ["SurfaceContainer"] = 0xFF211F26u,
        ["SurfaceContainerHigh"] = 0xFF2B2930u,
        ["SurfaceContainerHighest"] = 0xFF36343Bu,
        ["Outline"] = 0xFF938F99u,
        ["OutlineVariant"] = 0xFF49454Fu,
        ["InverseSurface"] = 0xFFE6E0E9u,
        ["InverseOnSurface"] = 0xFF322F35u,
        ["InversePrimary"] = 0xFF6750A4u,
        ["Scrim"] = 0xFF000000u,
        ["Shadow"] = 0xFF000000u,
        ["SurfaceTint"] = 0xFFD0BCFFu,
    };
    /// <summary>The baseline color roles for the requested variant, in ARGB.</summary>
    public static IReadOnlyDictionary<string, uint> Roles(bool dark) => dark ? DarkRoles : LightRoles;
}
