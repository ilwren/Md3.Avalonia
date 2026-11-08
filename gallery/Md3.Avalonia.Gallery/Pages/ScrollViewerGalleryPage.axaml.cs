using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class ScrollViewerGalleryPage : UserControl
{
    public ScrollViewerGalleryPage()
    {
        InitializeComponent();
        StandaloneBar.PropertyChanged += (_, e) =>
        {
            if (e.Property == RangeBase.ValueProperty || e.Property == MdScrollBar.ThumbVisibilityProperty ||
                e.Property == MdScrollBar.IsInteractiveProperty) UpdateScrollbarStatus();
        };
        UpdateScrollbarStatus();
    }

    private void ShowAlways(object? sender, RoutedEventArgs e) => StandaloneBar.ThumbVisibility = MdScrollbarThumbVisibility.Always;
    private void ShowAuto(object? sender, RoutedEventArgs e) => StandaloneBar.ThumbVisibility = MdScrollbarThumbVisibility.Auto;
    private void ShowHidden(object? sender, RoutedEventArgs e) => StandaloneBar.ThumbVisibility = MdScrollbarThumbVisibility.Hidden;
    private void ToggleInteraction(object? sender, RoutedEventArgs e) => StandaloneBar.IsInteractive = !StandaloneBar.IsInteractive;

    private void UpdateScrollbarStatus()
    {
        var interactive = StandaloneBar.IsInteractive;
        var policy = StandaloneBar.ThumbVisibility switch
        {
            MdScrollbarThumbVisibility.Always => GalleryLocalization.Choose("Always", "始终显示"),
            MdScrollbarThumbVisibility.Hidden => GalleryLocalization.Choose("Hidden", "隐藏"),
            _ => GalleryLocalization.Choose("Auto", "自动")
        };
        ScrollbarStatus.Text = GalleryLocalization.Choose(
            $"Value: {StandaloneBar.Value:0}; thumb: {policy}; interaction: {(interactive ? "on" : "off")}.",
            $"值：{StandaloneBar.Value:0}；滑块：{policy}；交互：{(interactive ? "开启" : "关闭")}。");
    }
}
