using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Md3.Avalonia.Chat.Themes;

/// <summary>Opt-in theme for Md3.Avalonia.Chat controls. Add after the core MaterialTheme.</summary>
public sealed class ChatTheme : Styles
{
    public ChatTheme() => AvaloniaXamlLoader.Load(this);
}
