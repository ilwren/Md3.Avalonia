using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Md3.Avalonia.Themes;

/// <summary>
/// Material Design 3 theme for Avalonia's <c>DataGrid</c>. Add it after
/// <c>MaterialTheme</c>; it replaces the control's Fluent theme, so do not include both.
/// </summary>
/// <remarks>
/// This lives in its own package because it is the only part of the library that needs the
/// <c>Avalonia.Controls.DataGrid</c> dependency. Like <c>MaterialTheme</c> it is standalone: it
/// does not require Avalonia's <c>FluentTheme</c>.
/// </remarks>
public sealed class MaterialDataGridTheme : Styles
{
    public MaterialDataGridTheme() => AvaloniaXamlLoader.Load(this);
}
