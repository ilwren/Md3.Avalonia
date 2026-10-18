using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Md3.Avalonia.Themes;

/// <summary>
/// Material Design 3 chrome for the <c>AvaloniaEdit</c> text editor. Add it after
/// <c>MaterialTheme</c>; it replaces the editor's own Fluent/Simple style sheets, so do not
/// include both.
/// AvaloniaEdit 文本编辑器的 Material Design 3 样式。
/// </summary>
/// <remarks>
/// <para>
/// This lives in its own package because it is the only part of the library that needs the
/// <c>Avalonia.AvaloniaEdit</c> dependency. Like <c>MaterialTheme</c> it is standalone: it does
/// not require Avalonia's <c>FluentTheme</c>. The styles are derived from the AvaloniaEdit theme
/// files, which are MIT licensed (see THIRD-PARTY-NOTICES.md in the package).
/// </para>
/// <para>
/// Every appearance the editor needs is declared here as
/// <c>Md.Comp.AvaloniaEdit.*</c> control themes that only the editor's own templates reference,
/// so registering this theme does not replace Avalonia's native <c>Button</c>, <c>ToggleButton</c>,
/// <c>TextBox</c> or <c>ListBox</c> themes for the rest of the application.
/// </para>
/// </remarks>
public sealed class MaterialAvaloniaEditTheme : Styles
{
    public MaterialAvaloniaEditTheme() => AvaloniaXamlLoader.Load(this);
}
