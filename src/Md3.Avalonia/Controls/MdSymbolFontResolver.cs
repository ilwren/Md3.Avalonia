using System.Diagnostics.CodeAnalysis;
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
    private static void TryConfigureOptionalProvider()
    {
        try
        {
            if (!_providerLookupAttempted)
            {
                _providerLookupAttempted = true;
                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                                   .FirstOrDefault(candidate => candidate.GetName().Name == "Md3.Avalonia.Icons")
                               ?? Assembly.Load(new AssemblyName("Md3.Avalonia.Icons"));
                _ensureOptionalProvider = assembly
                    .GetType("Md3.Avalonia.Icons.MdExternalMaterialSymbols")?
                    .GetMethod("EnsureConfigured", BindingFlags.Public | BindingFlags.Static);
            }
            _ensureOptionalProvider?.Invoke(null, null);
        }
        catch
        {
            // The icon package is optional. Core controls remain usable and their zero-width
            // fallback glyphs stay hidden when the provider assembly or font is unavailable.
        }
    }
}
