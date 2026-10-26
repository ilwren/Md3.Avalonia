using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
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

    /// <summary>
    /// Exercises the real picker page from the published native-AOT Gallery. This deliberately
    /// lives beside the demo instead of in the unit-test assembly so the trimmer and native linker
    /// must preserve exactly the code, compiled bindings, template and event path users execute.
    /// </summary>
    internal async Task RunNativeAotSmokeAsync()
    {
        var root = Path.Combine(Path.GetTempPath(), $"md3-picker-aot-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, "folder"));
        var jsonPath = Path.Combine(root, "theme.json");
        File.WriteAllText(jsonPath, "{}");
        File.WriteAllText(Path.Combine(root, "notes.txt"), "filtered");
        File.WriteAllText(Path.Combine(root, ".hidden.json"), "hidden");

        try
        {
            PathPicker.Mode = MdFileSystemPickerMode.File;
            PathPicker.AllowMultiple = false;
            PathPicker.ShowHiddenItems = false;
            PathPicker.FilePatterns = ["*.json"];
            PathPicker.ApplyTemplate();

            if (!await PathPicker.NavigateAsync(root))
                throw new InvalidOperationException(PathPicker.ErrorMessage ?? "The AOT picker could not open its fixture.");

            var list = PathPicker.GetVisualDescendants().OfType<MdList>()
                .SingleOrDefault(control => control.Name == "PART_ItemsHost")
                ?? throw new InvalidOperationException("The AOT picker template did not create PART_ItemsHost.");
            var file = PathPicker.Entries.SingleOrDefault(entry => !entry.IsDirectory)
                ?? throw new InvalidOperationException("The AOT picker did not enumerate the JSON fixture.");
            if (!string.Equals(file.FullPath, Path.GetFullPath(jsonPath), StringComparison.Ordinal) ||
                PathPicker.Entries.Any(entry => entry.Name is "notes.txt" or ".hidden.json") ||
                !PathPicker.Entries.Any(entry => entry.IsDirectory && entry.Name == "folder"))
            {
                throw new InvalidOperationException("The AOT picker returned an invalid filtered entry set.");
            }
            if (PathPicker.PathSegments.Count == 0 ||
                !string.Equals(PathPicker.PathSegments[^1].FullPath, Path.GetFullPath(root), StringComparison.Ordinal) ||
                !PathPicker.PathSegments[^1].IsCurrent)
            {
                throw new InvalidOperationException("The AOT picker did not build normalized breadcrumb segments.");
            }

            list.SelectedItem = file;
            if (PathPicker.SelectedPaths.Count != 1 ||
                !string.Equals(PathPicker.SelectedPaths[0], file.FullPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("The AOT picker did not project the list selection to a real path.");
            }

            IReadOnlyList<string>? confirmed = null;
            PathPicker.SelectionConfirmed += (_, args) => confirmed = args.Paths;
            PathPicker.ConfirmSelection();
            if (confirmed is null || confirmed.Count != 1 ||
                !string.Equals(confirmed[0], file.FullPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("The AOT picker did not confirm its selected path.");
            }
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

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
