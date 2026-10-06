using Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class PopoverGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public PopoverGalleryPage() => InitializeComponent();

    private void OverlayOpened(object? sender, EventArgs e) =>
        OverlayStatus.Text = L("Popover opened; scroll, click outside, or press Escape to dismiss it.", "浮层已打开；滚动、点击外部或按 Escape 可关闭。");

    private void OverlayClosed(object? sender, EventArgs e) =>
        OverlayStatus.Text = L("Transient surface dismissed; later sections remain unobstructed.", "临时浮层已关闭；后续区域不会被遮挡。");
}
