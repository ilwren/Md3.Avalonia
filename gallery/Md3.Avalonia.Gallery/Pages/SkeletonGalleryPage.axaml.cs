using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class SkeletonGalleryPage : UserControl
{
    public SkeletonGalleryPage() => InitializeComponent();

    private void ToggleSkeleton(object? sender, RoutedEventArgs e) => Skeleton.IsLoading = !Skeleton.IsLoading;
}
