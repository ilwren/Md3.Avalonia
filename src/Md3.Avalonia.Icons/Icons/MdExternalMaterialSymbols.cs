using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Styling;
using Avalonia.Threading;

namespace Md3.Avalonia.Icons;

/// <summary>
/// Registers the Material Symbols Rounded font supplied offline when the Icons package is built.
/// An external file/directory override remains available for diagnostics and host customization.
/// </summary>
public static class MdExternalMaterialSymbols
{
    public const string DirectoryEnvironmentVariable = "MD3_MATERIAL_SYMBOLS_FONT_DIR";
    public static readonly Uri CollectionKey = new("fonts:Md3MaterialSymbols", UriKind.Absolute);
    public static readonly Uri EmbeddedFontSource = new("avares://Md3.Avalonia.Icons/Assets/Fonts", UriKind.Absolute);

    public static bool IsAvailable { get; private set; }
    public static string? LoadedFontPath { get; private set; }
    public static string? LoadFailureReason { get; private set; }
    public static string? LoadedFamilyName { get; private set; }
    public static int VerifiedControlGlyphCount { get; private set; }
    public static IReadOnlyList<string> MissingControlGlyphs { get; private set; } = Array.Empty<string>();
    public static FontFamily? FontFamily { get; private set; }
    private static WeakReference<FontManager>? _configuredManager;
    private static WeakReference<FontManager>? _automaticAttemptedManager;
    private static WeakReference<Application>? _appliedApplication;
    private static GlyphTypeface? _configuredGlyphTypeface;
    private static IReadOnlyDictionary<string, string> _resolvedControlGlyphs = new Dictionary<string, string>();

    // Resource contract consumed by the core controls. Hosts can replace any of these keys with a
    // different icon font/catalog without referencing this package.
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

    // Google has shipped both legacy Material Icons and current Material Symbols codepoints for a
    // few common names. Resolve against the actual cmap rather than assuming one release vintage.
    private static readonly IReadOnlyDictionary<string, string[]> CompatibleGlyphCandidates =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Md.Icon.Search"] = ["\ue8b6", "\uef7a"],
            ["Md.Icon.Schedule"] = ["\ue8b5", "\uefd6"],
            ["Md.Icon.Check"] = ["\ue5ca", "\ue668"],
            ["Md.Icon.RotateRight"] = ["\ue41a", "\ue419"]
        };
    private static readonly IReadOnlyDictionary<int, int[]> CompatibleCodePoints =
        new Dictionary<int, int[]>
        {
            [0xef7a] = [0xe8b6], [0xe8b6] = [0xef7a],
            [0xefd6] = [0xe8b5], [0xe8b5] = [0xefd6],
            [0xe668] = [0xe5ca], [0xe5ca] = [0xe668],
            [0xe41a] = [0xe419], [0xe419] = [0xe41a]
        };

    /// <summary>
    /// Registers the build-supplied embedded Material Symbols font, then tries a host override when
    /// the embedded asset is invalid. Automatic integration calls this on first symbol use; hosts
    /// may also call it explicitly from AppBuilder.ConfigureFonts.
    /// </summary>
    public static void Configure(FontManager fontManager)
    {
        if (IsAvailable && _configuredManager?.TryGetTarget(out var configured) == true &&
            ReferenceEquals(configured, fontManager)) return;

        // A test host or designer can create more than one Avalonia application in a process. Font
        // collections belong to a FontManager instance, so register again for each new manager.
        IsAvailable = false;
        LoadedFontPath = null;
        FontFamily = null;
        LoadedFamilyName = null;
        VerifiedControlGlyphCount = 0;
        MissingControlGlyphs = Array.Empty<string>();
        _resolvedControlGlyphs = new Dictionary<string, string>();
        _configuredGlyphTypeface = null;
        LoadFailureReason = null;
        var sources = FindFontSources().ToArray();
        foreach (var source in sources)
        {
            try
            {
                var collection = new EmbeddedFontCollection(CollectionKey, source.Source);
                var familyNames = collection.Select(family => family.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                var familyName = familyNames.FirstOrDefault(name => name.Contains("Material Symbols Rounded", StringComparison.OrdinalIgnoreCase))
                    ?? familyNames.FirstOrDefault(name => name.Contains("Material Symbols", StringComparison.OrdinalIgnoreCase) && name.Contains("Round", StringComparison.OrdinalIgnoreCase))
                    ?? familyNames.FirstOrDefault(name => name.Contains("Material Icons", StringComparison.OrdinalIgnoreCase) && name.Contains("Round", StringComparison.OrdinalIgnoreCase))
                    ?? familyNames.FirstOrDefault(name => name.Equals("Material Icons", StringComparison.OrdinalIgnoreCase));
                if (familyName is null)
                {
                    LoadFailureReason = $"'{source.Description}' exposes [{string.Join(", ", familyNames)}], not a rounded Material Symbols/Icons family.";
                    continue;
                }

                fontManager.AddFontCollection(collection);
                var candidate = new FontFamily($"{CollectionKey}#{familyName}");
                if (!fontManager.TryGetGlyphTypeface(new Typeface(candidate), out var glyphTypeface))
                {
                    fontManager.RemoveFontCollection(CollectionKey);
                    LoadFailureReason = $"'{source.Description}' registered as '{familyName}', but Avalonia could not create its glyph typeface.";
                    continue;
                }

                ResolveControlGlyphs(glyphTypeface, out var resolvedGlyphs, out var missingGlyphs);
                if (!resolvedGlyphs.ContainsKey("Md.Icon.Search"))
                {
                    fontManager.RemoveFontCollection(CollectionKey);
                    LoadFailureReason = $"'{source.Description}' was opened as '{familyName}' but provides neither search U+E8B6 nor compatibility U+EF7A.";
                    continue;
                }

                FontFamily = candidate;
                LoadedFontPath = source.Description;
                LoadedFamilyName = familyName;
                _resolvedControlGlyphs = resolvedGlyphs;
                _configuredGlyphTypeface = glyphTypeface;
                VerifiedControlGlyphCount = resolvedGlyphs.Count;
                MissingControlGlyphs = missingGlyphs;
                IsAvailable = true;
                _configuredManager = new WeakReference<FontManager>(fontManager);
                LoadFailureReason = null;
                // Either explicit AppBuilder configuration or first symbol use may reach this path.
                // Marshal resource updates when catalog access originates on a worker thread.
                ApplyToCurrentApplication();
                return;
            }
            catch (Exception exception)
            {
                fontManager.RemoveFontCollection(CollectionKey);
                LoadFailureReason = $"Could not load '{source.Description}': {exception.Message}";
            }
        }
    }

    /// <summary>
    /// Ensures the embedded font is registered and its application resources are installed. This
    /// is invoked automatically by the catalog and by core symbol presenters, so referencing the
    /// Icons package is sufficient; hosts may still call ConfigureFonts explicitly.
    /// </summary>
    public static bool EnsureConfigured()
    {
        var manager = FontManager.Current;
        if (!IsAvailable || _configuredManager?.TryGetTarget(out var configured) != true || !ReferenceEquals(configured, manager))
        {
            if (_automaticAttemptedManager?.TryGetTarget(out var attempted) != true || !ReferenceEquals(attempted, manager))
            {
                _automaticAttemptedManager = new WeakReference<FontManager>(manager);
                Configure(manager);
            }
        }
        if (IsAvailable) ApplyToCurrentApplication();
        return IsAvailable;
    }

    private static void ApplyToCurrentApplication()
    {
        if (Application.Current is not { } application ||
            _appliedApplication?.TryGetTarget(out var applied) == true && ReferenceEquals(applied, application)) return;
        if (Dispatcher.UIThread.CheckAccess()) ApplyTo(application);
        else Dispatcher.UIThread.Post(() =>
        {
            if (ReferenceEquals(Application.Current, application)) ApplyTo(application);
        });
    }

    /// <summary>Returns true only when the configured font contains every codepoint in the glyph.</summary>
    public static bool HasGlyph(string? glyph)
    {
        if (_configuredGlyphTypeface is null || string.IsNullOrEmpty(glyph)) return false;
        for (var index = 0; index < glyph.Length; index++)
        {
            var codepoint = char.ConvertToUtf32(glyph, index);
            if (codepoint > 0xffff) index++;
            if (!_configuredGlyphTypeface.CharacterToGlyphMap.TryGetGlyph(codepoint, out var value) || value == 0) return false;
        }
        return true;
    }

    /// <summary>Maps known old/new catalog codepoints to the first glyph supported by the supplied font.</summary>
    public static string? ResolveGlyph(string? glyph)
    {
        if (string.IsNullOrEmpty(glyph) || _configuredGlyphTypeface is null) return null;
        if (HasGlyph(glyph)) return glyph;
        var codepoint = char.ConvertToUtf32(glyph, 0);
        if (!CompatibleCodePoints.TryGetValue(codepoint, out var alternatives)) return null;
        return alternatives.Select(char.ConvertFromUtf32).FirstOrDefault(HasGlyph);
    }

    private static void ResolveControlGlyphs(GlyphTypeface glyphTypeface,
        out IReadOnlyDictionary<string, string> resolved,
        out IReadOnlyList<string> missing)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var missingKeys = new List<string>();
        foreach (var resource in ControlGlyphResources)
        {
            var candidates = CompatibleGlyphCandidates.TryGetValue(resource.Key, out var compatible)
                ? compatible
                : [resource.Value];
            var selected = candidates.FirstOrDefault(value => value.Length > 0 &&
                glyphTypeface.CharacterToGlyphMap.TryGetGlyph(value[0], out var glyph) && glyph != 0);
            if (selected is null) missingKeys.Add(resource.Key);
            else values[resource.Key] = selected;
        }
        resolved = values;
        missing = missingKeys;
    }

    /// <summary>Replaces the Symbols token only when external registration succeeded.</summary>
    public static void ApplyTo(Application application)
    {
        if (!IsAvailable || FontFamily is null) return;

        const string resourceKey = "Md.Sys.Typeface.Symbols.Rounded";
        application.Resources[resourceKey] = FontFamily;
        foreach (var glyph in _resolvedControlGlyphs)
            application.Resources[glyph.Key] = glyph.Value;

        // MaterialTheme owns fallback tokens in its Styles.Resources dictionary. A resource
        // defined there is nearer to themed controls than Application.Resources, so update each
        // styles scope as well; this keeps icon slots correct without a core -> icons dependency.
        foreach (var style in application.Styles)
        {
            ApplyToStyle(style, resourceKey, FontFamily);
            foreach (var glyph in _resolvedControlGlyphs)
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
        // ControlTheme dynamic resources resolve in the ResourceDictionary that owns that theme,
        // before they consult Application.Resources. Stamp the token into every merged scope so
        // text-based icon slots (not only MdIcon) use the external family as well.
        dictionary[resourceKey] = value;
        foreach (var merged in dictionary.MergedDictionaries)
            if (merged is IResourceDictionary mergedDictionary)
                ApplyToDictionary(mergedDictionary, resourceKey, value);
        foreach (var themed in dictionary.ThemeDictionaries.Values)
            if (themed is IResourceDictionary themedDictionary)
                ApplyToDictionary(themedDictionary, resourceKey, value);
    }

    private static IEnumerable<MaterialFontSource> FindFontSources()
    {
        yield return new MaterialFontSource("embedded MaterialSymbolsRounded.ttf", EmbeddedFontSource);
        foreach (var file in FindFontFiles())
            yield return new MaterialFontSource(file.FullName, new Uri(file.FullName, UriKind.Absolute));
    }

    private static IEnumerable<FileInfo> FindFontFiles()
    {
        var emitted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var candidate in CandidatePaths())
        {
            if (string.IsNullOrWhiteSpace(candidate)) continue;
            if (File.Exists(candidate) && IsSupportedFont(candidate))
            {
                var fullPath = Path.GetFullPath(candidate);
                if (emitted.Add(fullPath)) yield return new FileInfo(fullPath);
                continue;
            }

            if (!Directory.Exists(candidate)) continue;
            foreach (var file in Directory.EnumerateFiles(candidate, "*", SearchOption.TopDirectoryOnly)
                         .Where(IsSupportedFont)
                         .Where(path =>
                         {
                             var name = Path.GetFileNameWithoutExtension(path);
                             return name.StartsWith("MaterialSymbolsRounded", StringComparison.OrdinalIgnoreCase) ||
                                    name.StartsWith("MaterialIcons", StringComparison.OrdinalIgnoreCase);
                         })
                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                var fullPath = Path.GetFullPath(file);
                if (emitted.Add(fullPath)) yield return new FileInfo(fullPath);
            }
        }
    }

    private static IEnumerable<string?> CandidatePaths()
    {
        // The environment variable is the supported deployment contract for desktop and Android
        // hosts. The application-local and LocalApplicationData folders are conveniences for
        // unpackaged hosts that cannot set process environment variables.
        yield return Environment.GetEnvironmentVariable(DirectoryEnvironmentVariable);
        yield return Path.Combine(AppContext.BaseDirectory, "MaterialSymbols");
        yield return Path.Combine(AppContext.BaseDirectory, "Fonts", "MaterialSymbols");
        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Md3.Avalonia", "Fonts", "MaterialSymbols");
    }

    private static bool IsSupportedFont(string path) =>
        Path.GetExtension(path).Equals(".ttf", StringComparison.OrdinalIgnoreCase) ||
        Path.GetExtension(path).Equals(".otf", StringComparison.OrdinalIgnoreCase);

    private sealed record MaterialFontSource(string Description, Uri Source);
}
