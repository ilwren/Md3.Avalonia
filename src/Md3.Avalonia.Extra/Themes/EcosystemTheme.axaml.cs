using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Md3.Avalonia.Extra.Themes;

/// <summary>Opt-in themes for clean-room ecosystem controls. Add after the core MaterialTheme.</summary>
public sealed class EcosystemTheme : Styles
{
    public EcosystemTheme() => AvaloniaXamlLoader.Load(this);
}
