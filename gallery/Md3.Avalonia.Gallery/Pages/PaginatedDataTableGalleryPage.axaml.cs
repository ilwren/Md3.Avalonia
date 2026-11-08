using Avalonia.Controls;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class PaginatedDataTableGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public PaginatedDataTableGalleryPage()
    {
        InitializeComponent();
        PagedTable.Columns.Add(new MdPaginatedDataColumn { Label = L("Package", "包"), IsSortable = true });
        PagedTable.Columns.Add(new MdPaginatedDataColumn { Label = L("Platform", "平台"), IsSortable = true });
        PagedTable.Columns.Add(new MdPaginatedDataColumn { Label = L("Score", "分数"), IsNumeric = true, IsSortable = true });
        PagedTable.ItemsSource = _rows = new[]
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

    private PackageRow[] _rows = [];

    // The table emits a request; the host, not the control, owns the data order.
    private void PagedTableSortRequested(object? sender, MdPaginatedDataTableSortEventArgs e)
    {
        var ordered = e.ColumnIndex switch
        {
            0 => e.Ascending ? _rows.OrderBy(row => row.Name, StringComparer.CurrentCultureIgnoreCase)
                             : _rows.OrderByDescending(row => row.Name, StringComparer.CurrentCultureIgnoreCase),
            1 => e.Ascending ? _rows.OrderBy(row => row.Platform, StringComparer.CurrentCultureIgnoreCase)
                             : _rows.OrderByDescending(row => row.Platform, StringComparer.CurrentCultureIgnoreCase),
            _ => e.Ascending ? _rows.OrderBy(row => row.Score) : _rows.OrderByDescending(row => row.Score)
        };
        _rows = ordered.ToArray();
        PagedTable.ItemsSource = _rows;
        PagedTable.SetPage(0);
        SortStatus.Text = e.Ascending
            ? L($"Sorted by {e.Column.Label}, ascending.", $"按{e.Column.Label}升序排序。")
            : L($"Sorted by {e.Column.Label}, descending.", $"按{e.Column.Label}降序排序。");
    }

    internal sealed record PackageRow(string Name, string Platform, int Score);

    private void PagedTableChanged(object? sender, MdPageChangedEventArgs e) =>
        PagedStatus.Text = L($"Page {e.PageIndex + 1}; first row index {e.FirstRowIndex}.", $"第 {e.PageIndex + 1} 页；首行索引 {e.FirstRowIndex}。");
}
