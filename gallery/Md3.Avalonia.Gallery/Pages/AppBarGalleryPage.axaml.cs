using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class AppBarGalleryPage : UserControl
{
    public AppBarGalleryPage() => InitializeComponent();

    private void ToggleSmallMenu(object? sender, RoutedEventArgs e)
    {
        LibraryMenu.IsOpen = false;
        SmallMenu.IsOpen = !SmallMenu.IsOpen;
    }

    private void ToggleLibraryMenu(object? sender, RoutedEventArgs e)
    {
        SmallMenu.IsOpen = false;
        LibraryMenu.IsOpen = !LibraryMenu.IsOpen;
    }

    private void CloseMenus(object? sender, RoutedEventArgs e)
    {
        SmallMenu.IsOpen = false;
        LibraryMenu.IsOpen = false;
    }
}
