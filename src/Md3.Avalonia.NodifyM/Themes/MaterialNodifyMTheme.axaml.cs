using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Md3.Avalonia.Themes;

/// <summary>
/// Material Design 3 theme for NodifyM.Avalonia. Add it after <c>MaterialTheme</c>; it replaces the
/// <c>ControlStyles</c> that ship in the NodifyM.Avalonia package, so do not include those as well.
/// NodifyM 节点编辑器的 Material Design 3 主题。
/// </summary>
/// <remarks>
/// This lives in its own package because it is the only part of the library that needs the
/// <c>NodifyM.Avalonia</c> dependency. Like <c>MaterialTheme</c> it is standalone: it does not require
/// Avalonia's <c>FluentTheme</c>. It restyles nothing outside NodifyM's own controls - a node editor
/// is built from NodifyM's <c>Node</c>, <c>Connector</c> and <c>Connection</c> types, not from native
/// Avalonia controls - so registering it application-wide changes no other control in the app.
/// </remarks>
public sealed class MaterialNodifyMTheme : Styles
{
    public MaterialNodifyMTheme() => AvaloniaXamlLoader.Load(this);
}
