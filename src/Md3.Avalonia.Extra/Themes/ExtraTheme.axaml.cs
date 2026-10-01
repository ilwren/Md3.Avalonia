using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Md3.Avalonia.Extra.Themes;

/// <summary>Opt-in themes for Md3.Avalonia.Extra extended controls. Add after the core MaterialTheme.</summary>
public sealed class ExtraTheme : Styles
{
    public ExtraTheme() => AvaloniaXamlLoader.Load(this);
}
