using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Md3.Avalonia.Themes;

/// <summary>
/// Material Design 3 theme for Tabalonia. Add it after <c>MaterialTheme</c>; it replaces Tabalonia's
/// <c>CustomTheme</c> and <c>FluentTheme</c>, so do not include either of those.
/// Tabalonia 可拖动标签页的 Material Design 3 主题。
/// </summary>
/// <remarks>
/// This lives in its own package because it is the only part of the library that needs the
/// <c>Tabalonia</c> dependency. Like <c>MaterialTheme</c> it is standalone: it does not require
/// Avalonia's <c>FluentTheme</c>, and it does not restyle any native control - Tabalonia's own
/// templates already style their add and close buttons from scratch.
/// </remarks>
public sealed class MaterialTabaloniaTheme : Styles
{
    public MaterialTabaloniaTheme() => AvaloniaXamlLoader.Load(this);
}
