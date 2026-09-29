using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Themes.Dynamic;

public enum MdThemeSchemeVariant
{
    TonalSpot,
    Neutral,
    Vibrant,
    Expressive,
    Monochrome,
    Fidelity
}

public enum MdThemeContrastLevel
{
    Standard,
    Medium,
    High
}

public enum MdThemeMode
{
    System,
    Light,
    Dark
}

public enum MdThemeFontProfile
{
    Brand,
    Plain
}

public enum MdThemeShapeScale
{
    Compact,
    Standard,
    Expressive
}

/// <summary>Serializable Theme Lab state and runtime dynamic-theme configuration.</summary>
public sealed record MdThemeOptions
{
    public string SeedColor { get; init; } = "#6750A4";
    public MdThemeSchemeVariant SchemeVariant { get; init; } = MdThemeSchemeVariant.TonalSpot;
    public MdThemeContrastLevel ContrastLevel { get; init; } = MdThemeContrastLevel.Standard;
    public MdThemeMode ThemeMode { get; init; } = MdThemeMode.System;
    public MdMotionScheme MotionScheme { get; init; } = MdMotionScheme.Expressive;
    public MdThemeFontProfile FontProfile { get; init; } = MdThemeFontProfile.Brand;
    public MdThemeShapeScale ShapeScale { get; init; } = MdThemeShapeScale.Standard;
}
