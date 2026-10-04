using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class DismissibleGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public DismissibleGalleryPage() => InitializeComponent();

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
