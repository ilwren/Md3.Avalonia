using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Md3.Avalonia.Themes;

/// <summary>
/// Material Design 3 theme for the <c>AvaloniaHex</c> hex editor. Add it after
/// <c>MaterialTheme</c>; it replaces the AvaloniaHex Fluent/Simple style sheets, so do not include
/// both.
/// AvaloniaHex 十六进制编辑器的 Material Design 3 样式。
/// </summary>
/// <remarks>
/// <para>
/// This lives in its own package because it is the only part of the library that needs the
/// <c>AvaloniaHex</c> dependency. Like <c>MaterialTheme</c> it is standalone: it does not require
/// Avalonia's <c>FluentTheme</c>. The styles are derived from the AvaloniaHex theme files, which
/// are MIT licensed (see THIRD-PARTY-NOTICES.md in the package).
/// </para>
/// <para>
/// The theme fills the resource keys AvaloniaHex publishes (<c>HexForegroundBase</c>,
/// <c>OffsetForeground</c>, the header, current-line, caret and selection brushes and pens) from
/// <c>Md.Sys</c> tokens, and gives the editor its own <c>ControlTheme</c> so the surface, outline
/// and scrollbars match the rest of the library.
/// </para>
/// </remarks>
public sealed class MaterialAvaloniaHexTheme : Styles
{
    public MaterialAvaloniaHexTheme() => AvaloniaXamlLoader.Load(this);
}
