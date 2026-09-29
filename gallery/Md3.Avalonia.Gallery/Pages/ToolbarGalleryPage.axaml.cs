using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class ToolbarGalleryPage : UserControl
{
    public ToolbarGalleryPage() => InitializeComponent();

    private void ToggleContextToolbar(object? sender, RoutedEventArgs e) =>
        ContextToolbarPopup.IsOpen = !ContextToolbarPopup.IsOpen;

    private void RunContextToolbarAction(object? sender, RoutedEventArgs e)
    {
        var action = (sender as Control)?.Tag?.ToString() ?? "Toolbar action";
        ContextToolbarStatus.Text = $"{action} selected.";
        ContextToolbarPopup.IsOpen = false;
    }
}
