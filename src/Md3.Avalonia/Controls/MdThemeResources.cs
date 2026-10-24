using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Resolves theme design-token brushes for the custom-drawn controls.
/// </summary>
/// <remarks>
/// The control's own resource chain is tried first so a scoped override still wins, then the
/// application: the chain from a freshly shown control can miss before its style attachment to
/// the app has settled, which is why the lookup does not stop at the control.
/// </remarks>
internal static class MdThemeResources
{
    public static IBrush? FindBrush(IResourceHost host, string key, ThemeVariant variant)
    {
        if (host.TryFindResource(key, variant, out var found) && found is IBrush brush)
            return brush;

        return Application.Current is { } app
            && app.TryFindResource(key, variant, out var appFound)
            && appFound is IBrush appBrush
                ? appBrush
                : null;
    }
}
