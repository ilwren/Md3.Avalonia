using Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class TransferGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public TransferGalleryPage()
    {
        InitializeComponent();
        Transfer.ItemsSource = new[] { L("Accessibility", "无障碍"), "Android", L("Desktop", "桌面"), L("Localization", "本地化"), L("Motion", "动效"), L("Testing", "测试") };
        Transfer.SelectedItems = new[] { L("Accessibility", "无障碍"), L("Testing", "测试") };
    }

    private void TransferSelectionChanged(object? sender, EventArgs e) =>
        TransferStatus.Text = L($"{Transfer.TargetItems.Count} selected · {Transfer.AvailableItems.Count} available.", $"已选择 {Transfer.TargetItems.Count} 项 · 可选 {Transfer.AvailableItems.Count} 项。");
}
