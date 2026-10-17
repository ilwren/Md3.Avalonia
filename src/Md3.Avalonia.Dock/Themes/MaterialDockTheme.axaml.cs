using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Md3.Avalonia.Themes;

/// <summary>
/// Material Design 3 theme for Dock. Add it after <c>MaterialTheme</c>; it replaces
/// <c>DockFluentTheme</c>, so do not include both.
/// </summary>
/// <remarks>
/// This lives in its own package because it is the only part of the library that needs the
/// <c>Dock.Avalonia</c> dependency. Unlike <c>DockFluentTheme</c> it carries no density or
/// preset API: the Material scheme, light/dark switching and density all come from the core
/// <c>MaterialTheme</c>, and the dock chrome only reads the tokens
/// <c>Themes/Accents/Material.axaml</c> maps onto <c>Md.Sys.Color.*</c>.
/// </remarks>
public sealed class MaterialDockTheme : Styles
{
    public MaterialDockTheme() => AvaloniaXamlLoader.Load(this);
}
