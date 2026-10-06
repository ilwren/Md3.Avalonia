using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class AnimationSequenceGalleryPage : UserControl
{
    public AnimationSequenceGalleryPage() => InitializeComponent();

    private async void ReplaySequence(object? sender, RoutedEventArgs e) => await Sequence.PlayAsync();
}
