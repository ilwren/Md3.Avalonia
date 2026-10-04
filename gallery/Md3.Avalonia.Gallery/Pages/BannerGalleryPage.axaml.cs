using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class BannerGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public BannerGalleryPage() => InitializeComponent();

    private void ShowBanner(object? sender, RoutedEventArgs e)
    {
        DemoBanner.Show();
        BannerStatus.Text = L("Banner shown.", "横幅已显示。");
    }

    private void BannerDismissed(object? sender, EventArgs e) => BannerStatus.Text = L("Banner dismissed.", "横幅已关闭。");

    private void BannerAction(object? sender, RoutedEventArgs e)
    {
        BannerStatus.Text = L($"{(sender as ContentControl)?.Content} selected.", $"已选择 {(sender as ContentControl)?.Content}。");
        DemoBanner.Dismiss();
    }
}
