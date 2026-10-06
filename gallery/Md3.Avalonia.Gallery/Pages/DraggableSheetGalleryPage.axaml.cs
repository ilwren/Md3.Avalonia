using Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class DraggableSheetGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public DraggableSheetGalleryPage() => InitializeComponent();

    private void SheetExtentChanged(object? sender, double extent) =>
        SheetStatus.Text = L($"Extent {extent:P0}", $"展开比例 {extent:P0}");
}
