using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaHex.Document;

namespace Md3.Avalonia.Gallery.Pages;

public partial class HexEditorGalleryPage : UserControl
{
    // A PNG signature followed by readable ASCII, so all three columns show something and the
    // ASCII column is not just dots.
    private static readonly byte[] Sample =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
        0x4D, 0x61, 0x74, 0x65, 0x72, 0x69, 0x61, 0x6C,
        0x20, 0x44, 0x65, 0x73, 0x69, 0x67, 0x6E, 0x20,
        0x33, 0x20, 0xC2, 0xB7, 0x20, 0x41, 0x76, 0x61,
        0x6C, 0x6F, 0x6E, 0x69, 0x61, 0x00, 0x00, 0x00
    ];

    public HexEditorGalleryPage()
    {
        InitializeComponent();
        Editor.Document = new MemoryBinaryDocument(Sample);

        // Put the caret on the "Design" run so the current-line and caret layers are visible
        // without the reader having to click first.
        Editor.Caret.Location = new BitLocation(16);
    }

    private void ToggleHeaders(object? sender, RoutedEventArgs e) =>
        Editor.IsHeaderVisible = !Editor.IsHeaderVisible;
}
