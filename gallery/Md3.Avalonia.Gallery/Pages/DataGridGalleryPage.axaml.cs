using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class DataGridGalleryPage : UserControl
{
    private bool _sortAscending = true;
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public DataGridGalleryPage()
    {
        InitializeComponent();
        EnterpriseGrid.Columns.Add(new MdDataGridColumn { Header = L("Name", "姓名"), PropertyName = nameof(GridRow.Name), Width = new GridLength(2, GridUnitType.Star) });
        EnterpriseGrid.Columns.Add(new MdDataGridColumn { Header = L("Team", "团队"), PropertyName = nameof(GridRow.Team), Width = new GridLength(1, GridUnitType.Star) });
        EnterpriseGrid.Columns.Add(new MdDataGridColumn { Header = L("Score", "分数"), PropertyName = nameof(GridRow.Score), Width = new GridLength(96), IsEditable = true });
        EnterpriseGrid.DataSource = Rows();

        // The stock Avalonia control, wearing the Material theme from Md3.Avalonia.DataGrid.
        ThemedDataGrid.Columns.Add(new DataGridTextColumn { Header = L("Name", "姓名"), Binding = new Binding(nameof(GridRow.Name)), Width = new DataGridLength(2, DataGridLengthUnitType.Star) });
        ThemedDataGrid.Columns.Add(new DataGridTextColumn { Header = L("Team", "团队"), Binding = new Binding(nameof(GridRow.Team)), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        ThemedDataGrid.Columns.Add(new DataGridTextColumn { Header = L("Score", "分数"), Binding = new Binding(nameof(GridRow.Score)), Width = new DataGridLength(96) });
        ThemedDataGrid.ItemsSource = Rows();

        GridRow[] Rows() =>
        [
            new("Ada", L("Design", "设计"), 92), new("Lin", L("Engineering", "工程"), 97),
            new("Maya", L("Research", "研究"), 89), new("Noah", L("Design", "设计"), 94),
            new("Zoe", L("Engineering", "工程"), 91)
        ];
    }

    private void GridFilterChanged(object? sender, TextChangedEventArgs e) => EnterpriseGrid.FilterText = (sender as TextBox)?.Text;

    private void SortGrid(object? sender, RoutedEventArgs e)
    {
        EnterpriseGrid.SortBy(nameof(GridRow.Name), _sortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending);
        _sortAscending = !_sortAscending;
    }

    private void PreviewGridClipboard(object? sender, RoutedEventArgs e) =>
        GridStatus.Text = EnterpriseGrid.BuildClipboardText().Replace(Environment.NewLine, " · ");

    private sealed class GridRow(string name, string team, int score)
    {
        public string Name { get; } = name;
        public string Team { get; } = team;
        public int Score { get; set; } = score;
    }

}
