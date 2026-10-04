using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class AvatarGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public AvatarGalleryPage() => InitializeComponent();

    private void AssignReviewer(object? sender, RoutedEventArgs e) =>
        ReviewerStatus.Text = L($"Review assigned to {(sender as Control)?.Tag}.", $"评审已分配给 {(sender as Control)?.Tag}。");
}
