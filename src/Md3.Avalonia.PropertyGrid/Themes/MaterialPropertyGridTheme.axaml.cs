using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Md3.Avalonia.Themes;

/// <summary>
/// Material Design 3 chrome for <c>Avalonia.PropertyGrid</c>'s <c>PropertyGrid</c>. Add it after
/// <c>MaterialTheme</c>, in <c>Application.Styles</c> or in the <c>Styles</c> of whichever window
/// or page owns the grid.
/// </summary>
/// <remarks>
/// <para>
/// This lives in its own package because it is the only part of the library that needs the
/// <c>bodong.Avalonia.PropertyGrid</c> dependency. Like <c>MaterialTheme</c> it is standalone: it
/// does not require Avalonia's <c>FluentTheme</c> or <c>SimpleTheme</c>, which matters more here
/// than for the other theme packages - <c>PropertyGrid</c> sets no colours of its own and hosts
/// plain Avalonia controls (<c>TextBox</c>, <c>ComboBox</c>, <c>CheckBox</c>, <c>NumericUpDown</c>,
/// <c>Slider</c>, <c>DatePicker</c>, <c>TimePicker</c>, <c>Expander</c>, <c>ContextMenu</c> and the
/// rest), so in an application that has no native control theme the grid renders its chrome and
/// none of its cells.
/// </para>
/// <para>
/// The way it stays local is the reason the package is a <c>Styles</c> collection and not a
/// resource dictionary: every native control theme it carries is keyed
/// <c>Md.Comp.PropertyGrid.*</c> and is handed to a control by a style whose selector requires a
/// <c>PropertyGrid</c> ancestor, so registering this theme never replaces Avalonia's native
/// <c>TextBox</c>, <c>ComboBox</c> or <c>DatePicker</c> theme for the rest of the application.
/// The grid's own chrome - header labels, the filter field, the category expanders, the name/value
/// splitter and the scroller - is styled the same way.
/// </para>
/// <para>
/// Two things upstream paints itself stay upstream's: the 0.5 DIP <c>Gray</c> hairline around the
/// list, radio-button and toggle-group cell editors, and the same hairline around an expanded
/// sub-object. Both are local values in the grid's own templates (the second is set in C#), so no
/// style can reach them; this package deliberately does not fork the grid's eleven embedded control
/// styles to change them, because a copy would drift from the part names the grid looks up.
/// </para>
/// </remarks>
public sealed class MaterialPropertyGridTheme : Styles
{
    public MaterialPropertyGridTheme() => AvaloniaXamlLoader.Load(this);
}
