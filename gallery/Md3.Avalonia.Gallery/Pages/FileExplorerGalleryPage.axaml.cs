using Avalonia.Controls;
using Avalonia.Platform;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class FileExplorerGalleryPage : UserControl
{
    public FileExplorerGalleryPage()
    {
        InitializeComponent();

        // Keep the sample self-contained: it opts into one generated demo directory rather than
        // probing user volumes or granting broad mobile-storage access.
        var root = Path.Combine(Path.GetTempPath(), "Md3.Avalonia.FileExplorerGallery");
        var documents = Path.Combine(root, "Documents");
        var photos = Path.Combine(root, "Photos");
        var downloads = Path.Combine(root, "Downloads");
        Directory.CreateDirectory(documents);
        Directory.CreateDirectory(photos);
        Directory.CreateDirectory(downloads);
        EnsureFile(Path.Combine(root, "Welcome.md"), "# Material file explorer\n\nThis is a local sample provider. Select this note to preview it, or create folders and try the Material 3 selection actions.\n");
        EnsureFile(Path.Combine(documents, "Project notes.md"), "# Project notes\n\n- Provider-neutral storage model\n- Adaptive Material 3 browser\n- Desktop and phone layouts\n");
        EnsureFile(Path.Combine(documents, "Budget.csv"), "Item,Estimate\nDesign,1200\nTesting,800\n");
        EnsureFile(Path.Combine(downloads, "Read me.txt"), "Files in this gallery sample live under the application's temporary directory.\n");
        EnsureSampleImage(Path.Combine(photos, "Sample landscape.png"));

        Explorer.Provider = new MdLocalFileSystemProvider(
        [
            new MdFileLocation("demo", "Gallery demo", root, MdSymbols.FolderOpen)
        ]);
        Explorer.GlyphSelector = entry => entry.IsDirectory
            ? MdSymbols.Folder
            : entry.ContentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true
                ? MdSymbols.Image
                : MdSymbols.Description;
        Explorer.EntryOpened += (_, entry) =>
            OpenStatus.Text = $"Selected for opening: {entry.RelativePath} (connect this event to your app's document workflow).";
    }

    private static void EnsureFile(string path, string contents)
    {
        if (!File.Exists(path)) File.WriteAllText(path, contents);
    }

    private static void EnsureSampleImage(string path)
    {
        if (File.Exists(path)) return;
        using var sample = AssetLoader.Open(new Uri("avares://Md3.Avalonia.Gallery/Assets/Media/file-explorer-sample.png"));
        using var output = File.Create(path);
        sample.CopyTo(output);
    }
}
