using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Md3.Avalonia.Extra.Themes;

/// <summary>Opt-in themes for Md3.Avalonia.Extra extended controls. Add after the core MaterialTheme.</summary>
public sealed class ExtraTheme : Styles
{
    public ExtraTheme() => AvaloniaXamlLoader.Load(this);
}

namespace Md3.Avalonia.Extra.Themes
{
    /// <summary>Compatibility alias for <see cref="Md3.Avalonia.Extra.Themes.ExtraTheme"/>.</summary>
    public sealed class EcosystemTheme : Styles
    {
        public EcosystemTheme()
        {
            Children.Add(new Md3.Avalonia.Extra.Themes.ExtraTheme());
        }
    }
}
