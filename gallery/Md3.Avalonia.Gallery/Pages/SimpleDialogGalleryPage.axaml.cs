using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class SimpleDialogGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    // Prefer either shell's viewport-level host so the scrim covers the desktop window or the
    // complete Android single view; fall back only when the page is previewed by itself.
    private MdDialogHost ActiveDialogHost => GalleryDialogHosts.Find(this) ?? LocalDialogHost;

    public SimpleDialogGalleryPage() => InitializeComponent();

    private void OpenSimpleDialog(object? sender, RoutedEventArgs e)
    {
        var dialog = new MdSimpleDialog
        {
            Title = L("Set backup account", "设置备份账户"),
            ItemsSource = new[] { "material@example.com", "avalonia@example.com", "android@example.com" },
            IsOpen = true
        };
        dialog.ItemSelected += SimpleDialogSelected;
        dialog.Dismissed += SimpleDialogDismissed;
        _ = ActiveDialogHost.ShowAsync(dialog);
        DialogStatus.Text = L("Choose an account in the modal dialog.", "请在模态对话框中选择账户。");
    }

    private void SimpleDialogSelected(object? sender, object? item)
    {
        DialogStatus.Text = L($"Backup account set to {item}.", $"备份账户已设置为 {item}。");
        ActiveDialogHost.Close(item);
    }

    private void SimpleDialogDismissed(object? sender, EventArgs e)
    {
        // Dismissed only fires when the dialog closed without a choice.
        if (ActiveDialogHost.IsOpen) ActiveDialogHost.Close();
        DialogStatus.Text = L("Account selection cancelled.", "已取消账户选择。");
    }
}
