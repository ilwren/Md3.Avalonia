using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Covers the Material theme for Avalonia's own DataGrid, which ships in the opt-in
/// Md3.Avalonia.DataGrid package.
/// </summary>
public class MdDataGridThemeTests
{
    private sealed record Invoice(string Customer, decimal Total);

    private static readonly Invoice[] Rows =
    [
        new("Aiko Tanaka", 412.50m),
        new("Beatriz Lima", 98.00m),
        new("Caleb Osei", 1_240.75m)
    ];

    [AvaloniaFact]
    public void The_Material_Theme_Templates_Avalonias_Data_Grid()
    {
        var grid = NewGrid();
        using var host = Show(grid);

        // Parts are found by name, so a template that lost one breaks resizing and scrolling
        // in ways nothing else would catch.
        Assert.NotNull(Named(grid, "PART_ColumnHeadersPresenter"));
        Assert.NotNull(Named(grid, "PART_RowsPresenter"));
        Assert.NotNull(Named(grid, "PART_VerticalScrollbar"));
        Assert.NotEmpty(grid.GetVisualDescendants().OfType<DataGridRow>());
    }

    [AvaloniaFact]
    public void The_Material_Theme_Applies_M3_Data_Table_Metrics()
    {
        var grid = NewGrid();
        using var host = Show(grid);

        Assert.Equal(56, grid.ColumnHeaderHeight);
        Assert.Equal(52, grid.RowHeight);

        // A Material data table separates rows with a divider; Fluent ships none at all.
        Assert.Equal(DataGridGridLinesVisibility.Horizontal, grid.GridLinesVisibility);
        Assert.Equal(Brush(grid, "Md.Sys.Color.OutlineVariant.Brush"), grid.HorizontalGridLinesBrush);
    }

    [AvaloniaFact]
    public void The_Material_Theme_Needs_No_Fluent_Theme()
    {
        var grid = NewGrid();
        using var host = Show(grid);

        // MaterialTheme is standalone by design, and this theme is derived from the Fluent one,
        // so the test app deliberately never loads FluentTheme.
        Assert.DoesNotContain(Application.Current!.Styles, style => style.GetType().Name == "FluentTheme");

        // Every brush the templates reference therefore has to resolve from M3 tokens: an
        // unresolved DynamicResource would leave these null and the grid invisible.
        var header = grid.GetVisualDescendants().OfType<DataGridColumnHeader>().First();
        Assert.NotNull(header.Background);
        Assert.NotNull(header.Foreground);
        Assert.Equal(Brush(grid, "Md.Sys.Color.SurfaceContainerLow.Brush"), header.Background);
    }

    [AvaloniaFact]
    public void A_Selected_Row_Sits_On_Secondary_Container()
    {
        var grid = NewGrid();
        using var host = Show(grid);

        grid.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();

        var row = grid.GetVisualDescendants().OfType<DataGridRow>().Single(candidate => candidate.IsSelected);
        var rectangle = row.GetVisualDescendants().OfType<global::Avalonia.Controls.Shapes.Rectangle>()
            .Single(shape => shape.Name == "BackgroundRectangle");

        // M3 fills the selected row rather than washing it with a translucent accent.
        Assert.Equal(Brush(grid, "Md.Sys.Color.SecondaryContainer.Brush"), rectangle.Fill);
        Assert.Equal(1, rectangle.Opacity);
    }

    private static DataGrid NewGrid()
    {
        var grid = new DataGrid
        {
            Width = 560,
            Height = 280,
            AutoGenerateColumns = false,
            ItemsSource = Rows
        };
        grid.Columns.Add(new DataGridTextColumn { Header = "Customer", Binding = new Binding(nameof(Invoice.Customer)) });
        grid.Columns.Add(new DataGridTextColumn { Header = "Total", Binding = new Binding(nameof(Invoice.Total)) });
        return grid;
    }

    private static Visual? Named(Visual root, string name) =>
        root.GetVisualDescendants().FirstOrDefault(visual => (visual as StyledElement)?.Name == name);

    // The tokens live in themed dictionaries, so the lookup has to name the variant the grid
    // is actually rendering in.
    private static IBrush Brush(StyledElement scope, string key)
    {
        Assert.True(Application.Current!.TryGetResource(key, scope.ActualThemeVariant, out var value), key);
        return Assert.IsAssignableFrom<IBrush>(value);
    }

    private static IDisposable Show(Control content)
    {
        var window = new Window { Width = 700, Height = 400, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new Scope(window);
    }

    private sealed class Scope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }
}
