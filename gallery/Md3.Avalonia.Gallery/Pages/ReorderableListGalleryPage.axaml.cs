using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class ReorderableListGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public ReorderableListGalleryPage() => InitializeComponent();

    private void MoveReorderUp(object? sender, RoutedEventArgs e) => ReorderList.MoveSelectedUp();
    private void MoveReorderDown(object? sender, RoutedEventArgs e) => ReorderList.MoveSelectedDown();

    private void ReorderCompleted(object? sender, MdReorderEventArgs e)
    {
        var item = ReorderList.SelectedItem is TextBlock text ? text.Text : ReorderList.SelectedItem?.ToString();
        ReorderStatus.Text = L($"Moved {item ?? "item"} from {e.OldIndex + 1} to {e.NewIndex + 1}.", $"已将 {item ?? "项目"} 从第 {e.OldIndex + 1} 位移动到第 {e.NewIndex + 1} 位。");
    }
}
