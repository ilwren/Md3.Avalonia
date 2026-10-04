using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class MasonryPanelGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public MasonryPanelGalleryPage() => InitializeComponent();

    private void MasonryCardInvoked(object? sender, RoutedEventArgs e) =>
        MasonryStatus.Text = L($"Opened project context: {(sender as Control)?.Tag}.", $"已打开项目上下文：{(sender as Control)?.Tag}。");
}
