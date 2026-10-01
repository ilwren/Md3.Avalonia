using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Styling;
using Avalonia.Threading;

namespace Md3.Avalonia.Icons.Lite;

/// <summary>
/// Registers the lightweight embedded Material Symbols Rounded font directly from repository assets.
/// No network download is required.
/// </summary>
public static class MdExternalMaterialSymbolsLite
{
    public static readonly Uri EmbeddedFontSource = new("avares://Md3.Avalonia.Icons.Lite/Assets/Fonts", UriKind.Absolute);
    public static FontFamily FontFamily { get; } = new("avares://Md3.Avalonia.Icons.Lite/Assets/Fonts#Material Symbols Rounded");

    public static bool IsAvailable => true;
    private static WeakReference<Application>? _appliedApplication;

    private static readonly IReadOnlyDictionary<string, string> ControlGlyphResources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Md.Icon.Add"] = "\ue145",
            ["Md.Icon.Check"] = "\ue5ca",
            ["Md.Icon.Close"] = "\ue5cd",
            ["Md.Icon.Done"] = "\ue876",
            ["Md.Icon.ExpandLess"] = "\ue5ce",
            ["Md.Icon.ExpandMore"] = "\ue5cf",
            ["Md.Icon.CalendarMonth"] = "\uebcc",
            ["Md.Icon.ChevronLeft"] = "\ue5cb",
            ["Md.Icon.ChevronRight"] = "\ue5cc",
            ["Md.Icon.Image"] = "\ue3f4",
            ["Md.Icon.ZoomOut"] = "\ue900",
            ["Md.Icon.ZoomIn"] = "\ue8ff",
            ["Md.Icon.RotateRight"] = "\ue419",
            ["Md.Icon.Refresh"] = "\ue5d5",
            ["Md.Icon.Slideshow"] = "\ue41b",
            ["Md.Icon.PlayArrow"] = "\ue037",
            ["Md.Icon.Pause"] = "\ue034",
            ["Md.Icon.SkipPrevious"] = "\ue045",
            ["Md.Icon.SkipNext"] = "\ue044",
            ["Md.Icon.Replay10"] = "\ue059",
            ["Md.Icon.Forward10"] = "\ue056",
            ["Md.Icon.VolumeUp"] = "\ue050",
            ["Md.Icon.VolumeOff"] = "\ue04f",
            ["Md.Icon.Fullscreen"] = "\ue5d0",
            ["Md.Icon.FullscreenExit"] = "\ue5d1",
            ["Md.Icon.MusicNote"] = "\ue405",
            ["Md.Icon.Search"] = "\ue8b6",
            ["Md.Icon.Visibility"] = "\ue8f4",
            ["Md.Icon.VisibilityOff"] = "\ue8f5",
            ["Md.Icon.Schedule"] = "\ue8b5"
        };

    public static bool EnsureConfigured()
    {
        ApplyToCurrentApplication();
        return true;
    }

    private static void ApplyToCurrentApplication()
    {
        if (Application.Current is not { } application ||
            _appliedApplication?.TryGetTarget(out var applied) == true && ReferenceEquals(applied, application)) return;

        if (Dispatcher.UIThread.CheckAccess())
            ApplyTo(application);
        else
            Dispatcher.UIThread.Post(() =>
            {
                if (ReferenceEquals(Application.Current, application))
                    ApplyTo(application);
            });
    }

    public static void ApplyTo(Application application)
    {
        const string resourceKey = "Md.Sys.Typeface.Symbols.Rounded";
        application.Resources[resourceKey] = FontFamily;
        foreach (var glyph in ControlGlyphResources)
            application.Resources[glyph.Key] = glyph.Value;

        foreach (var style in application.Styles)
        {
            ApplyToStyle(style, resourceKey, FontFamily);
            foreach (var glyph in ControlGlyphResources)
                ApplyToStyle(style, glyph.Key, glyph.Value);
        }
        _appliedApplication = new WeakReference<Application>(application);
    }

    private static void ApplyToStyle(IStyle style, string resourceKey, object value)
    {
        if (style is not Styles styles) return;
        ApplyToDictionary(styles.Resources, resourceKey, value);
        foreach (var child in styles)
            ApplyToStyle(child, resourceKey, value);
    }

    private static void ApplyToDictionary(IResourceDictionary dictionary, string resourceKey, object value)
    {
        dictionary[resourceKey] = value;
        foreach (var merged in dictionary.MergedDictionaries)
            ApplyToDictionary(merged, resourceKey, value);
    }
}
