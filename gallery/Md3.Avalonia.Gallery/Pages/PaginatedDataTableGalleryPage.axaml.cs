using Avalonia.Controls;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class PaginatedDataTableGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public PaginatedDataTableGalleryPage()
    {
        InitializeComponent();
        PagedTable.ItemsSource = new[]
        {
            new PackageRow("Md3.Avalonia", L("All", "全部"), 98),
            new PackageRow("Gallery.Desktop", L("Desktop", "桌面"), 95),
            new PackageRow("Gallery.Android", "Android", 92),
            new PackageRow("Theme.Tools", L("All", "全部"), 90),
            new PackageRow("Icons", L("Optional", "可选"), 88),
            new PackageRow("Ecosystem", L("All", "全部"), 86),
            new PackageRow("HeadlessTests", "CI", 99)
        };
    }

    // Moved here with the section: the row shape is this page's alone.
    private sealed record PackageRow(string Name, string Platform, int Score);

    private void PagedTableChanged(object? sender, MdPageChangedEventArgs e) =>
        PagedStatus.Text = L($"Page {e.PageIndex + 1}; first row index {e.FirstRowIndex}.", $"第 {e.PageIndex + 1} 页；首行索引 {e.FirstRowIndex}。");
}
