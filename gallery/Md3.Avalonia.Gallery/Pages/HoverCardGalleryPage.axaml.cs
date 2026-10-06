using Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class HoverCardGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public HoverCardGalleryPage() => InitializeComponent();

    private void HoverCardOpened(object? sender, EventArgs e) =>
        OverlayStatus.Text = L("Hover card opened; moving away or scrolling dismisses it.", "悬停卡片已打开；移开指针或滚动可关闭。");

    private void OverlayClosed(object? sender, EventArgs e) =>
        OverlayStatus.Text = L("Transient surface dismissed; later sections remain unobstructed.", "临时浮层已关闭；后续区域不会被遮挡。");
}
