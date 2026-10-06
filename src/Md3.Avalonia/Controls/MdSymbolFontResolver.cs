using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Media;

namespace Md3.Avalonia.Controls;

/// <summary>Resolves the optional host-provided symbol family without a core-to-icons dependency.</summary>
internal static class MdSymbolFontResolver
{
    internal const string ResourceKey = "Md.Sys.Typeface.Symbols.Rounded";
    private static MethodInfo? _ensureOptionalProvider;
    private static bool _providerLookupAttempted;

    internal static bool TryResolve(StyledElement element, out FontFamily family)
    {
        TryConfigureOptionalProvider();
        if (Application.Current?.TryGetResource(ResourceKey, element.ActualThemeVariant, out var value) == true &&
            value is FontFamily resolved)
        {
            family = resolved;
            return true;
        }

        family = default!;
        return false;
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods,
        "Md3.Avalonia.Icons.MdExternalMaterialSymbols", "Md3.Avalonia.Icons")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods,
        "Md3.Avalonia.Icons.Lite.MdExternalMaterialSymbolsLite", "Md3.Avalonia.Icons.Lite")]
    [UnconditionalSuppressMessage("Trimming", "IL2075",
        Justification = "EnsureConfigured on either optional icon package is rooted by the two " +
                        "DynamicDependency attributes above, so the trimmer keeps it when the " +
                        "package is referenced. A package that is absent is a supported outcome: " +
                        "the catch below leaves the zero-width fallback glyphs in place.")]
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "Assembly.Load probes for an optional package. Core must not reference " +
                        "the icon packages, so this is the only way to find one when present.")]
    [UnconditionalSuppressMessage("Trimming", "IL2057",
        Justification = "Both type names are constants rooted by the DynamicDependency attributes.")]
    private static void TryConfigureOptionalProvider()
    {
        try
        {
            if (!_providerLookupAttempted)
            {
                _providerLookupAttempted = true;
                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                                   .FirstOrDefault(candidate => candidate.GetName().Name == "Md3.Avalonia.Icons" || candidate.GetName().Name == "Md3.Avalonia.Icons.Lite")
                               ?? (TryLoadAssembly("Md3.Avalonia.Icons") ?? TryLoadAssembly("Md3.Avalonia.Icons.Lite"));
                if (assembly != null)
                {
                    _ensureOptionalProvider = (assembly.GetType("Md3.Avalonia.Icons.MdExternalMaterialSymbols")
                        ?? assembly.GetType("Md3.Avalonia.Icons.Lite.MdExternalMaterialSymbolsLite"))?
                        .GetMethod("EnsureConfigured", BindingFlags.Public | BindingFlags.Static);
                }
            }
            _ensureOptionalProvider?.Invoke(null, null);
        }
        catch
        {
            // The icon package is optional. Core controls remain usable and their zero-width
            // fallback glyphs stay hidden when the provider assembly or font is unavailable.
        }
    }

    private static Assembly? TryLoadAssembly(string name)
    {
        try { return Assembly.Load(new AssemblyName(name)); } catch { return null; }
    }
}
