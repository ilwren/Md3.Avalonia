using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Components;

public partial class SymbolTile : UserControl
{
    public static readonly StyledProperty<string> SymbolNameProperty =
        AvaloniaProperty.Register<SymbolTile, string>(nameof(SymbolName), string.Empty);
    public static readonly StyledProperty<string?> GlyphProperty =
        AvaloniaProperty.Register<SymbolTile, string?>(nameof(Glyph));
    public static readonly StyledProperty<string?> CopyTextProperty =
        AvaloniaProperty.Register<SymbolTile, string?>(nameof(CopyText));

    public SymbolTile() => InitializeComponent();

    public string SymbolName { get => GetValue(SymbolNameProperty); set => SetValue(SymbolNameProperty, value); }
    public string? Glyph { get => GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }
    public string? CopyText { get => GetValue(CopyTextProperty); set => SetValue(CopyTextProperty, value); }

    private async void CopySymbol(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is not null)
            await clipboard.SetTextAsync(CopyText ?? SymbolName);
    }
}
