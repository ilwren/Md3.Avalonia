using Avalonia.Controls;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class DismissibleGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public DismissibleGalleryPage() => InitializeComponent();

    private void SlideConfirmed(object? sender, EventArgs e) =>
        SlideStatus.Text = L("Confirmation received.", "已收到确认。");

    private void ResetSlide(object? sender, RoutedEventArgs e)
    {
        SlideDemo.Reset();
        SlideStatus.Text = L("Waiting for confirmation.", "等待确认。");
    }

    internal void RunSlideNativeAotSmoke()
    {
        var peer = ControlAutomationPeer.CreatePeerForElement(SlideDemo);
        var invoke = peer.GetProvider<IInvokeProvider>() ??
            throw new InvalidOperationException("Slide confirmation automation invoke is missing.");
        invoke.Invoke();
        if (!SlideDemo.IsConfirmed || string.IsNullOrWhiteSpace(peer.GetItemStatus()) ||
            SlideStatus.Text != L("Confirmation received.", "已收到确认。") || SlideDemo.Confirm())
            throw new InvalidOperationException("Slide confirmation or Gallery callback failed.");
        ResetSlide(null, new RoutedEventArgs());
        if (SlideDemo.IsConfirmed || SlideDemo.Progress != 0)
            throw new InvalidOperationException("Slide confirmation reset failed.");
    }

    // On the combined page this wrote to the dialog section's status line, several components
    // away, so dismissing the row appeared to do nothing.
    private void DismissibleDismissed(object? sender, EventArgs e) =>
        DismissibleStatus.Text = L("Dismissed. Restore puts the row back.", "已移除。点击“恢复”可将该行放回。");

    private void RestoreDismissible(object? sender, RoutedEventArgs e)
    {
        DismissibleDemo.Restore();
        DismissibleStatus.Text = L("Restored.", "已恢复。");
    }
}
