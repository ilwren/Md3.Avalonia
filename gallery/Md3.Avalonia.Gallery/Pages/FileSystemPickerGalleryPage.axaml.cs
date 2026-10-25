using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class FileSystemPickerGalleryPage : UserControl
{
    public FileSystemPickerGalleryPage()
    {
        InitializeComponent();
        PathPicker.InitialDirectory = ResolveAccessibleStartDirectory();
    }

    private void ShowFiles(object? sender, RoutedEventArgs e)
    {
        PathPicker.Mode = MdFileSystemPickerMode.File;
        PathPicker.Title = "Choose a file";
        PathPicker.ConfirmText = "Open";
        PathPicker.FilePatterns = null;
        PickerStatus.Text = "File mode shows folders for navigation and selects files.";
    }

    private void ShowFolders(object? sender, RoutedEventArgs e)
    {
        PathPicker.Mode = MdFileSystemPickerMode.Folder;
        PathPicker.Title = "Choose a folder";
        PathPicker.ConfirmText = "Choose folder";
        PathPicker.FilePatterns = null;
        PickerStatus.Text = "Folder mode returns selected folders, or the folder currently open.";
    }

    private void MultipleChanged(object? sender, RoutedEventArgs e) =>
        PathPicker.AllowMultiple = MultipleToggle.IsChecked == true;

    private void SelectionConfirmed(object? sender, MdFileSystemSelectionEventArgs e) =>
        PickerStatus.Text = $"Selected real path{(e.Paths.Count == 1 ? string.Empty : "s")}: {string.Join(" · ", e.Paths)}";

    private void SelectionCancelled(object? sender, EventArgs e) =>
        PickerStatus.Text = "Selection cancelled.";

    private void NavigationFailed(object? sender, MdFileSystemNavigationFailedEventArgs e) =>
        PickerStatus.Text = $"Cannot open {e.Path}: {e.Exception.Message}";

    internal static string ResolveAccessibleStartDirectory()
    {
        string[] candidates =
        [
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Directory.GetCurrentDirectory()
        ];
        return candidates.FirstOrDefault(path => !string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
               ?? Directory.GetCurrentDirectory();
    }
}
