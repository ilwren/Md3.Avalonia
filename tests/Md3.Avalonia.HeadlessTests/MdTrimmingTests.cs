using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Every control that resolves a string property path also offers a delegate that reaches the
/// same data without reflection. These hold that second path open: it is what a trimmed
/// application uses, and it is the one that silently rots if nobody exercises it.
/// </summary>
public sealed class MdTrimmingTests
{
    private sealed class Row(string name, int quantity)
    {
        public string Name { get; set; } = name;
        public int Quantity { get; set; } = quantity;
    }

    [AvaloniaFact]
    public void A_Data_Grid_Column_Reads_Through_Its_Selector_Instead_Of_Reflecting()
    {
        var reflected = new MdDataGridColumn { PropertyName = "Name" };
        var selected = new MdDataGridColumn
        {
            // Deliberately a different source from PropertyName, so a reflective read would
            // produce a different answer and the test would catch the fallback being taken.
            PropertyName = "Name",
            ValueSelector = item => $"<{((Row)item!).Name}>"
        };
        var row = new Row("Bolt", 4);

        Assert.Equal("Bolt", reflected.Read(row));
        Assert.Equal("<Bolt>", selected.Read(row));
    }

    [AvaloniaFact]
    public void A_Data_Grid_Sorts_And_Filters_Through_The_Selector()
    {
        var grid = new MdDataGrid();
        grid.Columns.Add(new MdDataGridColumn
        {
            PropertyName = "Name",
            ValueSelector = item => ((Row)item!).Name
        });
        grid.Columns.Add(new MdDataGridColumn
        {
            PropertyName = "Quantity",
            ValueSelector = item => ((Row)item!).Quantity
        });
        grid.DataSource = new[] { new Row("Nut", 9), new Row("Bolt", 4), new Row("Washer", 7) };

        grid.SortBy("Name");
        var sorted = grid.ItemsSource!.Cast<Row>().Select(row => row.Name).ToArray();
        Assert.Equal(new[] { "Bolt", "Nut", "Washer" }, sorted);

        grid.FilterText = "was";
        var filtered = grid.ItemsSource!.Cast<Row>().Select(row => row.Name).ToArray();
        Assert.Equal(new[] { "Washer" }, filtered);
    }

    [AvaloniaFact]
    public void A_Column_With_A_Setter_Commits_An_Edit_Without_Reflection()
    {
        var grid = new MdDataGrid();
        var column = new MdDataGridColumn
        {
            PropertyName = "Quantity",
            IsEditable = true,
            ValueSelector = item => ((Row)item!).Quantity,
            ValueParser = text => int.TryParse(text, out var parsed) ? parsed : null,
            ValueSetter = (item, value) => ((Row)item).Quantity = (int)value!
        };
        grid.Columns.Add(column);
        var row = new Row("Bolt", 4);

        Assert.True(grid.CommitEdit(row, column, "11"));
        Assert.Equal(11, row.Quantity);

        // A value the parser rejects must leave the model alone.
        Assert.False(grid.CommitEdit(row, column, "eleven"));
        Assert.Equal(11, row.Quantity);
    }

    [AvaloniaFact]
    public void The_Reflective_Edit_Path_Still_Converts_And_Still_Refuses_Bad_Text()
    {
        var grid = new MdDataGrid();
        var column = new MdDataGridColumn { PropertyName = "Quantity", IsEditable = true };
        grid.Columns.Add(column);
        var row = new Row("Bolt", 4);

        Assert.True(grid.CommitEdit(row, column, "11"));
        Assert.Equal(11, row.Quantity);

        Assert.False(grid.CommitEdit(row, column, "eleven"));
        Assert.Equal(11, row.Quantity);

        // A property that does not exist is a no-op rather than a crash.
        Assert.False(grid.CommitEdit(row, new MdDataGridColumn { PropertyName = "Nope" }, "1"));
    }

    [AvaloniaFact]
    public void A_Cancelled_Edit_Leaves_The_Model_Untouched_On_Both_Paths()
    {
        var grid = new MdDataGrid();
        grid.CellEditCommitting += (_, args) => args.Cancel = true;
        var row = new Row("Bolt", 4);

        var reflective = new MdDataGridColumn { PropertyName = "Quantity", IsEditable = true };
        var direct = new MdDataGridColumn
        {
            PropertyName = "Quantity",
            IsEditable = true,
            ValueSelector = item => ((Row)item!).Quantity,
            ValueParser = text => int.TryParse(text, out var parsed) ? parsed : null,
            ValueSetter = (item, value) => ((Row)item).Quantity = (int)value!
        };
        grid.Columns.Add(reflective);

        Assert.False(grid.CommitEdit(row, reflective, "11"));
        Assert.False(grid.CommitEdit(row, direct, "11"));
        Assert.Equal(4, row.Quantity);
    }

    [AvaloniaFact]
    public void A_Search_View_Prefers_Its_Display_Selector_Over_The_Path()
    {
        var view = new MdSearchView { ResultDisplayMemberPath = "Name" };
        var row = new Row("Bolt", 4);

        Assert.Equal("Bolt", view.ResolveDisplayText(row));

        view.ResultDisplaySelector = result => $"<{((Row)result!).Name}>";
        Assert.Equal("<Bolt>", view.ResolveDisplayText(row));
    }

    [AvaloniaFact]
    public void An_Async_Select_Prefers_Its_Display_Selector_Over_The_Path()
    {
        var select = new MdAsyncSelect
        {
            DisplayMemberPath = "Name",
            Debounce = TimeSpan.Zero,
            ItemsSource = new[] { new Row("Bolt", 4), new Row("Nut", 9) }
        };

        select.Query = "bol";
        Assert.Single(select.Results!.Cast<object>());

        select.DisplaySelector = item => ((Row)item!).Quantity.ToString();
        select.Query = "9";
        Assert.Single(select.Results!.Cast<object>());
    }

    [AvaloniaFact]
    public void The_Shipped_Assemblies_Declare_Themselves_Trimmable()
    {
        // The flag is what lets an application's trimmer touch these assemblies at all. If it is
        // dropped from a csproj the library silently stops being trimmed rather than failing.
        foreach (var assembly in new[]
                 {
                     typeof(MdButton).Assembly,
                     typeof(MdDataGrid).Assembly
                 })
        {
            var trimmable = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(attribute =>
                    string.Equals(attribute.Key, "IsTrimmable", StringComparison.OrdinalIgnoreCase));

            Assert.True(trimmable is not null, $"{assembly.GetName().Name} is missing IsTrimmable");
            Assert.Equal("True", trimmable!.Value, ignoreCase: true);
        }
    }
}
