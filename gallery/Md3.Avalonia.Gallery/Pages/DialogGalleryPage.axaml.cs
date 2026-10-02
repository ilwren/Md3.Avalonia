using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public sealed record DiscardDraftDialogModel(string Headline, string Message);

public sealed record ReviewChangesDialogModel(string Headline, string Message);

public partial class DialogGalleryPage : UserControl
{
    public DialogGalleryPage() => InitializeComponent();

    private void OpenBasicDialog(object? sender, RoutedEventArgs e)
    {
        _ = BasicDialogHost.ShowAsync(new DiscardDraftDialogModel(
            "Discard draft?",
            "This draft has unsaved changes. Discarding it cannot be undone."));
    }

    private void OpenFullScreenDialog(object? sender, RoutedEventArgs e)
    {
        _ = BasicDialogHost.ShowAsync(new ReviewChangesDialogModel(
            "Review changes",
            "Review the pending changes before returning to the editor."));
    }

    private void CloseDialog(object? sender, RoutedEventArgs e) => BasicDialogHost.Close();
}
