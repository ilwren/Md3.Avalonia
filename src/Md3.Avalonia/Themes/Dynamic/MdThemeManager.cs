using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Styling;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Themes.Dynamic;

public sealed record MdContrastDiagnostic(string ForegroundRole, string BackgroundRole, string Purpose, double Ratio, double RequiredRatio)
{
    public bool Passes => Ratio >= RequiredRatio;
}

/// <summary>Applies generated roles atomically at application scope and provides contrast diagnostics.</summary>
public static class MdThemeManager
{
    private static Application? _observedApplication;

    private static readonly (string Foreground, string Background, string Purpose, double Required)[] DiagnosticPairs =
    [
        ("OnPrimary", "Primary", "Filled primary actions", 4.5),
        ("OnPrimaryContainer", "PrimaryContainer", "Tonal actions and selected emphasis", 4.5),
        ("OnSecondary", "Secondary", "Secondary filled actions", 4.5),
        ("OnSecondaryContainer", "SecondaryContainer", "Selected controls and navigation indicators", 4.5),
        ("OnTertiary", "Tertiary", "Tertiary emphasis", 4.5),
        ("OnTertiaryContainer", "TertiaryContainer", "Complementary containers", 4.5),
        ("OnError", "Error", "Destructive actions", 4.5),
        ("OnErrorContainer", "ErrorContainer", "Error messages and recovery surfaces", 4.5),
        ("OnSurface", "Surface", "Primary body content", 4.5),
        ("OnSurfaceVariant", "SurfaceVariant", "Supporting content and metadata", 4.5),
        ("InverseOnSurface", "InverseSurface", "Snackbars and inverse surfaces", 4.5)
    ];

    public static MdThemeOptions Current { get; private set; } = new();

    public static IReadOnlyDictionary<string, Color> Apply(Application application, MdThemeOptions options, bool dark)
    {
        ObserveSystemTheme(application);
        var roles = MdThemeGenerator.Generate(options, dark);
        foreach (var (role, color) in roles)
        {
            application.Resources[$"Md.Sys.Color.{role}"] = color;
            application.Resources[$"Md.Sys.Color.{role}.Brush"] = new SolidColorBrush(color);
        }

        var primary = roles["Primary"];
        var onPrimary = roles["OnPrimary"];
        application.Resources["Md.Sys.Color.PrimaryAction"] = primary;
        application.Resources["Md.Sys.Color.OnPrimaryAction"] = onPrimary;
        application.Resources["Md.Sys.Color.PrimaryAction.Brush"] = new SolidColorBrush(primary);
        application.Resources["Md.Sys.Color.OnPrimaryAction.Brush"] = new SolidColorBrush(onPrimary);
        application.Resources["Md.Sys.Color.PrimarySelection.Brush"] =
            new SolidColorBrush(Color.FromArgb(0x52, primary.R, primary.G, primary.B));

        var onSurface = roles["OnSurface"];
        application.Resources["Md.Sys.Color.DisabledContainer.Brush"] = WithAlpha(onSurface, 0.10);
        application.Resources["Md.Sys.Color.DisabledContent.Brush"] = WithAlpha(onSurface, 0.38);
        application.Resources["Md.Sys.Color.DisabledFilledField.Brush"] = WithAlpha(onSurface, 0.04);
        application.Resources["Md.Sys.Color.DisabledIndicator.Brush"] = WithAlpha(onSurface, 0.38);
        application.Resources["Md.Sys.Color.DisabledOutline.Brush"] = WithAlpha(onSurface, 0.12);

        application.Resources["Md.Sys.Typeface.Brand"] = new FontFamily(
            options.FontProfile == MdThemeFontProfile.Brand ? "Arial" : "sans-serif");
        application.Resources["Md.Sys.Typeface.Plain"] = new FontFamily("Arial");
        application.Resources[MdMotion.SchemeResourceKey] = options.MotionScheme;
        ApplyMotionSchemeToVisualRoots(application, options.MotionScheme);

        var shapeFactor = options.ShapeScale switch
        {
            MdThemeShapeScale.Compact => 0.72,
            MdThemeShapeScale.Expressive => 1.28,
            _ => 1.0
        };
        application.Resources["Md.Sys.Shape.Scale"] = shapeFactor;
        application.Resources["Md.Sys.Shape.Corner.Small"] = new CornerRadius(8 * shapeFactor);
        application.Resources["Md.Sys.Shape.Corner.Medium"] = new CornerRadius(12 * shapeFactor);
        application.Resources["Md.Sys.Shape.Corner.Large"] = new CornerRadius(16 * shapeFactor);
        application.Resources["Md.Sys.Shape.Corner.ExtraLarge"] = new CornerRadius(28 * shapeFactor);
        application.Resources["Md.Comp.Button.XSmall.Shape.Round"] = new CornerRadius(16 * shapeFactor);
        application.Resources["Md.Comp.Button.Small.Shape.Round"] = new CornerRadius(20 * shapeFactor);
        application.Resources["Md.Comp.Button.Medium.Shape.Round"] = new CornerRadius(28 * shapeFactor);
        application.Resources["Md.Comp.Button.Large.Shape.Round"] = new CornerRadius(48 * shapeFactor);
        application.Resources["Md.Comp.Button.XLarge.Shape.Round"] = new CornerRadius(68 * shapeFactor);
        application.Resources["Md.Comp.Button.XSmall.Shape.Square"] = new CornerRadius(12 * shapeFactor);
        application.Resources["Md.Comp.Button.Small.Shape.Square"] = new CornerRadius(12 * shapeFactor);
        application.Resources["Md.Comp.Button.Medium.Shape.Square"] = new CornerRadius(16 * shapeFactor);
        application.Resources["Md.Comp.Button.Large.Shape.Square"] = new CornerRadius(28 * shapeFactor);
        application.Resources["Md.Comp.Button.XLarge.Shape.Square"] = new CornerRadius(28 * shapeFactor);

        Current = options;
        return roles;
    }

    private static void ApplyMotionSchemeToVisualRoots(Application application, MdMotionScheme scheme)
    {
        switch (application.ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                foreach (var window in desktop.Windows)
                    window.SetCurrentValue(MdMotion.SchemeProperty, scheme);
                break;
            case ISingleViewApplicationLifetime singleView when singleView.MainView is AvaloniaObject root:
                root.SetCurrentValue(MdMotion.SchemeProperty, scheme);
                break;
        }
    }

    private static void ObserveSystemTheme(Application application)
    {
        if (ReferenceEquals(_observedApplication, application)) return;
        if (_observedApplication is not null)
            _observedApplication.ActualThemeVariantChanged -= OnActualThemeVariantChanged;
        _observedApplication = application;
        application.ActualThemeVariantChanged += OnActualThemeVariantChanged;
    }

    private static void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        if (sender is not Application application || Current.ThemeMode != MdThemeMode.System) return;
        Apply(application, Current, application.ActualThemeVariant == ThemeVariant.Dark);
    }

    public static IReadOnlyList<MdContrastDiagnostic> Diagnose(IReadOnlyDictionary<string, Color> roles) =>
        DiagnosticPairs.Select(pair => new MdContrastDiagnostic(
            pair.Foreground,
            pair.Background,
            pair.Purpose,
            ContrastRatio(roles[pair.Foreground], roles[pair.Background]),
            pair.Required)).ToArray();

    public static double ContrastRatio(Color first, Color second)
    {
        var lighter = Math.Max(RelativeLuminance(first), RelativeLuminance(second));
        var darker = Math.Min(RelativeLuminance(first), RelativeLuminance(second));
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double RelativeLuminance(Color color)
    {
        static double Channel(byte value)
        {
            var normalized = value / 255d;
            return normalized <= 0.04045
                ? normalized / 12.92
                : Math.Pow((normalized + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Channel(color.R) + 0.7152 * Channel(color.G) + 0.0722 * Channel(color.B);
    }

    private static SolidColorBrush WithAlpha(Color color, double opacity) => new(
        Color.FromArgb((byte)Math.Round(Math.Clamp(opacity, 0, 1) * 255), color.R, color.G, color.B));
}
