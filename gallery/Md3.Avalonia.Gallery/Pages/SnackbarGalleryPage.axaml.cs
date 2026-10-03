using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class SnackbarGalleryPage : UserControl
{
    private readonly MdSnackbarService _snackbarService = new();

    public SnackbarGalleryPage()
    {
        InitializeComponent();
        DemoSnackbarHost.Service = _snackbarService;
    }

    private async void ShowSnackbar(object? sender, RoutedEventArgs e)
    {
        SnackbarResult.Text = "Draft moved to archive";
        var result = await _snackbarService.ShowAsync(new MdSnackbarMessage("Draft archived")
        {
            ActionContent = "Undo",
            IsDismissible = true,
            Duration = TimeSpan.Zero
        });
        if (result == MdSnackbarResult.ActionInvoked) SnackbarResult.Text = "Draft restored";
    }

    private void UndoArchive(object? sender, EventArgs e) => SnackbarResult.Text = "Draft restored";
}
