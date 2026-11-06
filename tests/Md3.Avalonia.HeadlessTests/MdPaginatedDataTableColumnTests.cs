using System;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdPaginatedDataTableColumnTests
{
    [AvaloniaFact]
    public void Sort_Requests_Toggle_And_Leave_Items_In_Host_Order()
    {
        var table = new MdPaginatedDataTable { ItemsSource = new[] { 3, 1, 2 } };
        var name = new MdPaginatedDataColumn { Label = "Name", IsSortable = true };
        table.Columns.Add(name);
        table.Columns.Add(new MdPaginatedDataColumn { Label = "Notes" });
        MdPaginatedDataTableSortEventArgs? request = null;
        var count = 0;
        var command = new RecordingCommand();
        table.SortRequestedCommand = command;
        table.SortRequested += (_, e) => { request = e; count++; };

        table.RequestSort(-1);
        table.RequestSort(1);
        table.RequestSort(2);
        Assert.Equal(0, count);
        table.RequestSort(0);
        Assert.NotNull(request);
        Assert.Same(name, request!.Column);
        Assert.Equal(0, request.ColumnIndex);
        Assert.True(request.Ascending);
        Assert.Same(request, command.Parameter);
        Assert.Equal(1, command.Count);
        Assert.Equal(0, table.SortColumnIndex);
        Assert.True(table.SortAscending);
        Assert.Equal(new object?[] { 3, 1, 2 }, table.PageItems);
        table.RequestSort(0);
        Assert.False(table.SortAscending);
        Assert.False(request!.Ascending);
        Assert.Equal(2, count);
        Assert.Equal(2, command.Count);
        command.Enabled = false;
        table.RequestSort(0);
        Assert.Equal(3, count);
        Assert.Equal(2, command.Count);
    }

    [AvaloniaFact]
    public void Column_Headers_React_To_Changes_And_Request_Sort()
    {
        var column = new MdPaginatedDataColumn { Label = "Amount", IsNumeric = true, IsSortable = true, Tooltip = "Sort amount" };
        var table = new MdPaginatedDataTable();
        table.Columns.Add(column);
        var window = new Window { Width = 700, Height = 350, Content = table };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var headers = table.GetVisualDescendants().OfType<Grid>().Single(x => x.Name == "PART_ColumnHeaders");
            Assert.True(headers.IsVisible);
            var button = Assert.IsType<Button>(Assert.Single(headers.Children));
            Assert.Equal(HorizontalAlignment.Right, button.HorizontalAlignment);
            Assert.Equal("Sort amount", ToolTip.GetTip(button));
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(0, table.SortColumnIndex);
            Assert.Same(button, Assert.Single(headers.Children)); // A sort must not destroy keyboard focus.
            Assert.Equal("Amount ↑", button.Content);
            column.Label = "Total";
            Assert.Equal("Total ↑", Assert.IsType<Button>(Assert.Single(headers.Children)).Content);
            column.IsSortable = false;
            Assert.Equal("Total ↑", Assert.IsType<TextBlock>(Assert.Single(headers.Children)).Text);
            table.Columns.Clear();
            Assert.False(headers.IsVisible);
            Assert.Empty(headers.Children);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Column_Changes_While_Detached_Appear_After_Reattach()
    {
        var column = new MdPaginatedDataColumn { Label = "Before" };
        var table = new MdPaginatedDataTable();
        table.Columns.Add(column);
        var window = new Window { Width = 700, Height = 350, Content = table };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.Content = null;
            column.Label = "After";
            table.Columns.Add(new MdPaginatedDataColumn { Label = "Extra" });
            window.Content = table;
            Dispatcher.UIThread.RunJobs();
            var headers = table.GetVisualDescendants().OfType<Grid>().Single(x => x.Name == "PART_ColumnHeaders");
            Assert.Equal(2, headers.Children.Count);
            Assert.Equal("After", Assert.IsType<TextBlock>(headers.Children[0]).Text);
            column.Label = "Updated";
            Assert.Equal("Updated", Assert.IsType<TextBlock>(headers.Children[0]).Text);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void First_And_Last_Page_Buttons_Use_Existing_Page_Notifications()
    {
        var table = new MdPaginatedDataTable
        {
            ItemsSource = Enumerable.Range(1, 8).ToArray(), RowsPerPage = 3, ShowFirstLastButtons = true,
            Actions = "Actions"
        };
        var events = 0;
        table.PageChanged += (_, _) => events++;
        var window = new Window { Width = 700, Height = 350, Content = table };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var first = table.GetVisualDescendants().OfType<Button>().Single(x => x.Name == "PART_FirstPageButton");
            var last = table.GetVisualDescendants().OfType<Button>().Single(x => x.Name == "PART_LastPageButton");
            Assert.True(first.IsVisible);
            Assert.True(last.IsVisible);
            Assert.False(first.IsEnabled);
            Assert.True(last.IsEnabled);
            last.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(2, table.PageIndex);
            Assert.Equal(new object?[] { 7, 8 }, table.PageItems);
            Assert.False(last.IsEnabled);
            Assert.True(first.IsEnabled);
            first.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(0, table.PageIndex);
            Assert.Equal(2, events);
        }
        finally { window.Close(); }
    }

    private sealed class RecordingCommand : ICommand
    {
        public bool Enabled { get; set; } = true;
        public int Count { get; private set; }
        public object? Parameter { get; private set; }
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => Enabled;
        public void Execute(object? parameter) { Count++; Parameter = parameter; }
    }
}
