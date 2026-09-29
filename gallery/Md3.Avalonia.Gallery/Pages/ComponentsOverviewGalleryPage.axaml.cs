using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class ComponentsOverviewGalleryPage : UserControl
{
    public ComponentsOverviewGalleryPage() => InitializeComponent();

    private void OpenComponent(object? sender, RoutedEventArgs e)
    {
        if (sender is MdButton { Tag: string title } && TopLevel.GetTopLevel(this) is MainWindow window)
            window.NavigateToIndexedPage(title);
    }
}
