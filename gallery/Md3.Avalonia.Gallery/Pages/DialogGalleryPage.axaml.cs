using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class DialogGalleryPage : UserControl
{
    public DialogGalleryPage() => InitializeComponent();

    private void OpenBasicDialog(object? sender, RoutedEventArgs e)
    {
        DemoDialog.Variant = MdDialogVariant.Basic;
        DemoDialog.Headline = "Discard draft?";
        BasicDialogHost.Dialog = null;
        BasicDialogHost.Dialog = DemoDialog;
        BasicDialogHost.IsOpen = true;
    }

    private void OpenFullScreenDialog(object? sender, RoutedEventArgs e)
    {
        DemoDialog.Variant = MdDialogVariant.FullScreen;
        DemoDialog.Headline = "Review changes";
        BasicDialogHost.Dialog = null;
        BasicDialogHost.Dialog = DemoDialog;
        BasicDialogHost.IsOpen = true;
    }

    private void CloseDialog(object? sender, RoutedEventArgs e) => BasicDialogHost.Close();
}
