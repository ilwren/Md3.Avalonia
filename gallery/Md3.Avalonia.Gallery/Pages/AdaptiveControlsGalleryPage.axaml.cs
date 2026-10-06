using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class AdaptiveControlsGalleryPage : UserControl
{
    private bool _adaptiveSyncRunning;
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public AdaptiveControlsGalleryPage() => InitializeComponent();

    private void AdaptiveControlChanged(object? sender, RoutedEventArgs e) =>
        AdaptiveStatus.Text = L($"Platform policy: {(AutomaticAdaptiveSwitch.IsChecked == true ? "automatic" : "manual")}; iOS preview {(CupertinoAdaptiveSwitch.IsChecked == true ? "enabled" : "disabled")}.", $"平台策略：{(AutomaticAdaptiveSwitch.IsChecked == true ? "自动" : "手动")}；iOS 预览已{(CupertinoAdaptiveSwitch.IsChecked == true ? "启用" : "禁用")}。");

    private async void RunAdaptiveSync(object? sender, RoutedEventArgs e)
    {
        if (_adaptiveSyncRunning) return;
        _adaptiveSyncRunning = true;
        MaterialAdaptiveProgress.Value = 0;
        CupertinoAdaptiveProgress.Value = 0;
        for (var value = 0; value <= 100; value += 5)
        {
            MaterialAdaptiveProgress.Value = value;
            CupertinoAdaptiveProgress.Value = value;
            AdaptiveStatus.Text = L($"Uploading design tokens · {value}%", $"正在上传设计令牌 · {value}%");
            await Task.Delay(35);
        }
        AdaptiveStatus.Text = L("Upload complete in both platform presentations.", "两种平台呈现均已完成上传。");
        _adaptiveSyncRunning = false;
    }
}
