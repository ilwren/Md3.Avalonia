using System.Reflection;
using Avalonia.Media;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Icons.Lite;

/// <summary>Auto-configures the lite embedded Material Symbols Rounded font for MdIcon.</summary>
public static class MdExternalMaterialSymbolsLite
{
    private static bool _configured;

    public static bool EnsureConfigured()
    {
        if (_configured) return true;
        try
        {
            var family = new FontFamily("avares://Md3.Avalonia.Icons.Lite/Assets/Fonts#Material Symbols Rounded");
            MdSymbolFontResolver.RegisterGlobalResolver(() => family);
            _configured = true;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
