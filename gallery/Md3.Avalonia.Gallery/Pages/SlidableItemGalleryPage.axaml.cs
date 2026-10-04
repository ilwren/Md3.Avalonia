using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class SlidableItemGalleryPage : UserControl
{
    public SlidableItemGalleryPage() => InitializeComponent();

    private void CloseSlidable(object? sender, RoutedEventArgs e) => Slidable.Close();
}
