using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Md3.Avalonia.Themes;

/// <summary>Registers Material 3 system tokens and themes for Md3.Avalonia controls.</summary>
public sealed class MaterialTheme : Styles
{
    public MaterialTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
