using Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class HoverCardGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public HoverCardGalleryPage() => InitializeComponent();

    private void HoverCardOpened(object? sender, EventArgs e) =>
        OverlayStatus.Text = L("Hover card opened; move into the card and it should stay open while the pointer or focus is inside.", "悬停卡片已打开；将指针移入卡片后，只要指针或焦点仍在卡片内就应保持打开。");

    private void OverlayClosed(object? sender, EventArgs e) =>
        OverlayStatus.Text = L("Transient surface dismissed; later sections remain unobstructed.", "临时浮层已关闭；后续区域不会被遮挡。");
}
