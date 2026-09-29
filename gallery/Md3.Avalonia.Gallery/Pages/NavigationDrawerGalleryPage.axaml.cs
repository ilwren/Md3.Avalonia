using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class NavigationDrawerGalleryPage : UserControl
{
    public NavigationDrawerGalleryPage() => InitializeComponent();
    private void ShowStandard(object? sender, RoutedEventArgs e) { DemoDrawer.IsModal = false; DemoDrawer.IsOpen = true; }
    private void ShowModal(object? sender, RoutedEventArgs e) { DemoDrawer.IsModal = true; DemoDrawer.IsOpen = true; }
    private void CloseDrawer(object? sender, RoutedEventArgs e) => DemoDrawer.IsOpen = false;
}
