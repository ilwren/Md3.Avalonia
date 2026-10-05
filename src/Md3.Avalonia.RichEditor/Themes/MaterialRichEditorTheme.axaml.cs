using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Md3.Avalonia.Themes;

/// <summary>
/// Material Design 3 theme for the AvaloniaRichEditor control library. Add it after
/// <c>MaterialTheme</c>.
/// </summary>
/// <remarks>
/// <para>
/// This lives in its own package because it is the only part of the library that needs the
/// <c>AvaloniaRichEditor</c> dependency. Like <c>MaterialTheme</c> it is standalone: it does not
/// require Avalonia's <c>FluentTheme</c>.
/// </para>
/// <para>
/// It styles properties instead of replacing control templates, so it layers on top of the
/// editor's own theme and keeps working when the upstream template changes. Anything an
/// application sets locally still wins.
/// </para>
/// </remarks>
public sealed class MaterialRichEditorTheme : Styles
{
    public MaterialRichEditorTheme() => AvaloniaXamlLoader.Load(this);
}
