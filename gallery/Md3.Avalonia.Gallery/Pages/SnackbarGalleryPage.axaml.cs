using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class SnackbarGalleryPage : UserControl
{
    public SnackbarGalleryPage() => InitializeComponent();

    private void ShowSnackbar(object? sender, RoutedEventArgs e)
    {
        SnackbarResult.Text = "Draft moved to archive";
        DemoSnackbar.Show();
    }

    private void UndoArchive(object? sender, EventArgs e) => SnackbarResult.Text = "Draft restored";
}
