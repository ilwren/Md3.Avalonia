using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class DataTableGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public DataTableGalleryPage() => InitializeComponent();

    private void SortTable(object? sender, RoutedEventArgs e)
    {
        if ((sender as Control)?.Tag is string column) DemoTable.ToggleSort(column);
    }

    private void TableSortRequested(object? sender, MdDataTableSortEventArgs e) =>
        SortStatus.Text = L($"Sort {e.Column} · {e.Direction}. The host applies ordering to ItemsSource.", $"排序 {e.Column} · {e.Direction}。宿主负责对 ItemsSource 应用排序。");
}
