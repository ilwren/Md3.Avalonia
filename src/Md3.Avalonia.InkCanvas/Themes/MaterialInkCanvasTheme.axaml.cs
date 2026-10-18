using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using DotNetCampus.Inking;
using SkiaSharp;

namespace Md3.Avalonia.Themes;

/// <summary>
/// Material Design 3 for handwriting. Registering the theme styles <see cref="Md3.Avalonia.Controls.MdSignaturePad"/>;
/// <see cref="Follow"/> and <see cref="Apply"/> are the entry points for an application that uses a
/// bare <see cref="InkCanvas"/> instead and only wants its ink to stop being red.
/// </summary>
/// <remarks>
/// <c>InkCanvas</c> is not a <c>TemplatedControl</c> and registers no styles: it draws strokes with
/// Skia from an <c>AvaloniaSkiaInkCanvasSettings</c> whose defaults are red ink ten units thick. So
/// the theme for a bare canvas is an entry point that writes those settings - the same shape the
/// Mapsui and ScottPlot themes take for controls that own their own rendering.
/// </remarks>
public sealed class MaterialInkCanvasTheme : Styles
{
    /// <summary>Stroke width the Material entry points use, in device-independent pixels.</summary>
    public const double DefaultInkThickness = 3;

    public MaterialInkCanvasTheme() => AvaloniaXamlLoader.Load(this);

    /// <summary>
    /// Styles a bare canvas for the variant it currently draws in, and keeps following the light/dark
    /// switch. Attach it once, after the canvas is in the tree, so the tokens resolve.
    /// </summary>
    public static void Follow(InkCanvas canvas)
    {
        Apply(canvas, canvas.ActualThemeVariant == ThemeVariant.Dark);
        canvas.ActualThemeVariantChanged += (_, _) => Apply(canvas, canvas.ActualThemeVariant == ThemeVariant.Dark);
    }

    /// <summary>Styles a bare canvas for one variant, for a host that manages the switch itself.</summary>
    public static void Apply(InkCanvas canvas, bool dark)
    {
        var variant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        var settings = canvas.AvaloniaSkiaInkCanvas.Settings;
        settings.InkColor = FromControl(canvas, variant) ?? FromApplication(variant) ?? SKColors.Black;
        settings.InkThickness = (float)DefaultInkThickness;
    }

    /// <summary>
    /// Reads the token from the canvas's own resource host first, which is the right answer for a
    /// canvas inside a window that carries the theme, then from the application. The second step is
    /// not a nicety: a control that is never attached resolves nothing at all, which is exactly the
    /// off-screen canvas this entry point exists for, and without the fallback it would silently
    /// draw black instead of the theme's ink.
    /// </summary>
    /// <remarks>
    /// The key is <c>Md.Sys.Color.OnSurface</c>, which is the <c>Color</c>; the matching brush is
    /// <c>Md.Sys.Color.OnSurface.Brush</c>. There is no <c>.Color</c> suffix, and asking for one
    /// resolves to nothing.
    /// </remarks>
    private static SKColor? FromControl(InkCanvas canvas, ThemeVariant variant) =>
        canvas.TryFindResource(OnSurfaceKey, variant, out var value) && value is Color color
            ? ToSk(color)
            : null;

    private static SKColor? FromApplication(ThemeVariant variant) =>
        Application.Current is { } app && app.TryGetResource(OnSurfaceKey, variant, out var value) && value is Color color
            ? ToSk(color)
            : null;

    private const string OnSurfaceKey = "Md.Sys.Color.OnSurface";

    private static SKColor ToSk(Color color) => new(color.R, color.G, color.B, color.A);
}
