using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class AnimatedTextGalleryPage : UserControl
{
    public AnimatedTextGalleryPage() => InitializeComponent();

    private void OnCelebrate(object? sender, RoutedEventArgs e) => ConfettiOverlay.Burst();
}
