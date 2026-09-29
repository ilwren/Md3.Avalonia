using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Ecosystem.Controls;
using Md3.Avalonia.Gallery.Pages;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdParityHardeningTests
{
    [AvaloniaFact]
    public void ComboBox_Prepares_The_Real_Item_Container_With_The_Material_Theme()
    {
        var combo = new ComboBoxProbe { ItemsSource = new[] { "Alpha", "Beta" } };
        using var host = Show(combo, 420, 180);
        var expected = Assert.IsType<ControlTheme>(ResourceNodeExtensions.FindResource(combo, "MdComboBoxItemTheme"));
        var container = new ComboBoxItem { Content = "Alpha", Width = 280 };

        combo.Prepare(container, "Alpha", 0);

        Assert.Same(expected, container.Theme);
        using var itemHost = Show(container, 360, 160);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(72, container.MinHeight);
        Assert.True(container.Bounds.Height >= 72);
    }

    [AvaloniaFact]
    public void Expanded_Search_Commits_The_Result_Into_The_Query_And_Closes()
    {
        var bar = new MdSearchBar { Text = "tok" };
        var view = new MdSearchView
        {
            Header = bar,
            IsOpen = true,
            ResultDisplayMemberPath = nameof(SearchEntry.Name)
        };
        var entry = new SearchEntry("SearchTokens.axaml");
        MdSearchResultCommittedEventArgs? committed = null;
        view.ResultCommitted += (_, args) => committed = args;

        view.CommitResult(entry);

        Assert.Same(entry, view.SelectedResult);
        Assert.Equal(entry.Name, bar.Text);
        Assert.False(view.IsOpen);
        Assert.Same(entry, committed?.Result);
        Assert.Equal(entry.Name, committed?.DisplayText);
    }

    [AvaloniaFact]
    public void Search_Gallery_Uses_A_Real_Result_Click_To_Commit_The_Query()
    {
        var page = new SearchGalleryPage();
        using var host = Show(page, 900, 700);
        var view = page.GetVisualDescendants().OfType<MdSearchView>().Single();
        var query = page.GetVisualDescendants().OfType<MdSearchBar>().Single(control => control.Name == "ExpandedSearchBar");
        view.Show();
        query.Text = "tokens";
        Dispatcher.UIThread.RunJobs();
        var row = page.GetVisualDescendants().OfType<ListBoxItem>().Single(item =>
            Equals(item.DataContext?.GetType().GetProperty("Name")?.GetValue(item.DataContext), "SearchTokens.axaml"));
        var point = row.TranslatePoint(new Point(row.Bounds.Width / 2, row.Bounds.Height / 2), host.Window)!.Value;

        host.Window.MouseMove(point, RawInputModifiers.None);
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        host.Window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("SearchTokens.axaml", query.Text);
        Assert.False(view.IsOpen);
    }

    [AvaloniaFact]
    public void Flutter_And_Ecosystem_Row_State_Layers_Cover_Their_Root_Surfaces()
    {
        var reorder = new MdReorderableList { Width = 420, Height = 120, ItemsSource = new[] { "Row" } };
        var transfer = new MdTransfer
        {
            Width = 720,
            Height = 280,
            ItemsSource = new[] { "Available", "Selected" },
            SelectedItems = new[] { "Selected" }
        };
        var root = new StackPanel { Spacing = 16, Children = { reorder, transfer } };
        using var host = Show(root, 820, 520);
        Dispatcher.UIThread.RunJobs();

        var reorderRow = reorder.GetVisualDescendants().OfType<ListBoxItem>().Single();
        AssertStateCoversRoot(reorderRow, "PART_Root", "PART_State");
        var reorderState = reorderRow.GetVisualDescendants().OfType<Border>().Single(border => border.Name == "PART_State");
        var pointer = reorderState.TranslatePoint(new Point(reorderState.Bounds.Width / 2, reorderState.Bounds.Height / 2), host.Window)!.Value;
        host.Window.MouseMove(pointer, RawInputModifiers.None);
        Assert.Equal(0.08, reorderState.Opacity, precision: 3);
        host.Window.MouseDown(pointer, MouseButton.Left, RawInputModifiers.None);
        Assert.Equal(0.12, reorderState.Opacity, precision: 3);
        host.Window.MouseUp(pointer, MouseButton.Left, RawInputModifiers.None);

        foreach (var row in transfer.GetVisualDescendants().OfType<ListBoxItem>())
            AssertStateCoversRoot(row, "PART_Root", "PART_State");
    }

    [AvaloniaFact]
    public void Timeline_Projects_Item_State_Active_Index_And_Connectors()
    {
        var timeline = new MdTimeline
        {
            ActiveIndex = 1,
            ItemsSource = new[]
            {
                new MdTimelineItem("Done", State: MdTimelineItemState.Completed),
                new MdTimelineItem("Current"),
                new MdTimelineItem("Failed", State: MdTimelineItemState.Error)
            }
        };
        using var host = Show(timeline, 520, 320);
        Dispatcher.UIThread.RunJobs();
        var presenters = timeline.GetVisualDescendants().OfType<MdTimelineItemPresenter>().ToArray();

        Assert.Equal(3, presenters.Length);
        Assert.Equal(MdTimelineItemState.Completed, presenters[0].EffectiveState);
        Assert.Equal(MdTimelineItemState.Active, presenters[1].EffectiveState);
        Assert.Equal(MdTimelineItemState.Error, presenters[2].EffectiveState);
        Assert.False(presenters[0].GetVisualDescendants().OfType<Border>().Single(border => border.Name == "PART_VBefore").IsVisible);
        Assert.False(presenters[2].GetVisualDescendants().OfType<Border>().Single(border => border.Name == "PART_VAfter").IsVisible);
        Assert.NotNull(host.Window.CaptureRenderedFrame());
    }

    [AvaloniaFact]
    public void Breadcrumb_Direct_Invoke_Raises_Command_Path_Once()
    {
        var breadcrumb = new MdBreadcrumb { ItemsSource = new[] { "Home", "Components" } };
        var invocations = 0;
        breadcrumb.ItemInvoked += (_, _) => invocations++;

        breadcrumb.Invoke("Components");

        Assert.Equal(1, invocations);
        Assert.Equal("Components", breadcrumb.SelectedItem);
    }

    [AvaloniaFact]
    public void DataGrid_New_Sort_Column_Starts_Ascending_And_Header_Is_Interactive()
    {
        var grid = new MdDataGrid
        {
            Width = 560,
            Height = 260,
            DataSource = new[] { new GridRow("Zed", 1), new GridRow("Ada", 3) }
        };
        grid.Columns.Add(new MdDataGridColumn { Header = "Name", PropertyName = nameof(GridRow.Name) });
        grid.Columns.Add(new MdDataGridColumn { Header = "Score", PropertyName = nameof(GridRow.Score) });
        grid.SortBy(nameof(GridRow.Name), ListSortDirection.Ascending);
        grid.SortBy(nameof(GridRow.Score));
        Assert.Equal(ListSortDirection.Ascending, grid.SortDirection);

        using var host = Show(grid, 620, 320);
        Dispatcher.UIThread.RunJobs();
        var nameHeader = grid.GetVisualDescendants().OfType<MdButton>()
            .Single(button => button.Classes.Contains("sort-header") && Equals(button.Content, "Name"));
        var point = nameHeader.TranslatePoint(new Point(nameHeader.Bounds.Width / 2, nameHeader.Bounds.Height / 2), host.Window)!.Value;
        host.Window.MouseMove(point, RawInputModifiers.None);
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        host.Window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Assert.Equal(nameof(GridRow.Name), grid.SortColumn);
        Assert.Equal(ListSortDirection.Ascending, grid.SortDirection);
    }

    [AvaloniaFact]
    public void OnUserInteraction_Form_Does_Not_Expose_Errors_On_Untouched_Fields()
    {
        var touched = new MdFormField { IsRequired = true, IsTouched = true, Value = string.Empty };
        var untouched = new MdFormField { IsRequired = true, Value = string.Empty };
        var panel = new StackPanel { Children = { touched, untouched } };
        var form = new MdForm { AutovalidateMode = MdAutovalidateMode.OnUserInteraction, Content = panel };
        using var host = Show(form, 420, 240);

        touched.Value = "Valid";

        Assert.True(form.IsValid);
        Assert.Null(untouched.ErrorText);
    }

    [AvaloniaFact]
    public void Focus_Traversal_Group_Cycles_Real_Tab_Focus()
    {
        var first = new MdButton { Content = "First" };
        var second = new MdButton { Content = "Second" };
        var group = new MdFocusTraversalGroup
        {
            Cycle = true,
            Content = new StackPanel { Children = { first, second } }
        };
        using var host = Show(group, 420, 220);
        Assert.True(second.Focus());
        Assert.True(second.IsFocused);

        host.Window.KeyPress(Key.Tab, RawInputModifiers.None, PhysicalKey.Tab, "\t");
        Dispatcher.UIThread.RunJobs();

        Assert.True(first.IsFocused);
    }

    [AvaloniaFact]
    public void Text_Form_Field_Validates_From_Real_Keyboard_Input()
    {
        var editor = new MdTextBox { Label = "Name" };
        var field = new MdFormField
        {
            IsRequired = true,
            Validator = value => value?.ToString()?.Length >= 2 ? null : "Two characters required",
            Content = editor
        };
        editor.TextChanged += (_, _) => { field.MarkTouched(); field.Value = editor.Text; };
        var form = new MdForm { AutovalidateMode = MdAutovalidateMode.OnUserInteraction, Content = field };
        using var host = Show(form, 420, 220);
        Assert.True(editor.Focus());

        host.Window.KeyTextInput("A");
        Dispatcher.UIThread.RunJobs();
        Assert.True(field.IsTouched);
        Assert.False(field.IsValid);
        Assert.Equal("Two characters required", field.ErrorText);

        host.Window.KeyTextInput("B");
        Dispatcher.UIThread.RunJobs();
        Assert.True(field.IsValid);
        Assert.True(form.IsValid);
    }

    [AvaloniaFact]
    public void Chat_Only_Selects_When_The_Pointer_Is_Inside_The_Bubble()
    {
        var message = new MdChatMessage("1", MdChatMessageRole.User, "Bubble", DateTimeOffset.Now, "You");
        var chat = new MdChatView { Width = 680, Height = 300, MessagesSource = new[] { message } };
        using var host = Show(chat, 760, 380);
        Dispatcher.UIThread.RunJobs();
        var row = chat.GetVisualDescendants().OfType<ListBoxItem>().Single();
        var bubble = chat.GetVisualDescendants().OfType<Border>().Single(border => border.Name == "PART_Bubble");
        var blank = row.TranslatePoint(new Point(4, row.Bounds.Height / 2), host.Window)!.Value;

        host.Window.MouseMove(blank, RawInputModifiers.None);
        host.Window.MouseDown(blank, MouseButton.Left, RawInputModifiers.None);
        host.Window.MouseUp(blank, MouseButton.Left, RawInputModifiers.None);
        Assert.Equal(0, chat.SelectionCount);

        var bubblePoint = bubble.TranslatePoint(new Point(bubble.Bounds.Width / 2, bubble.Bounds.Height / 2), host.Window)!.Value;
        host.Window.MouseMove(bubblePoint, RawInputModifiers.None);
        host.Window.MouseDown(bubblePoint, MouseButton.Left, RawInputModifiers.None);
        host.Window.MouseUp(bubblePoint, MouseButton.Left, RawInputModifiers.None);
        Assert.Equal(1, chat.SelectionCount);
    }

    private static void AssertStateCoversRoot(Control owner, string rootName, string stateName)
    {
        var root = owner.GetVisualDescendants().OfType<Border>().Single(border => border.Name == rootName);
        var state = owner.GetVisualDescendants().OfType<Border>()
            .Where(border => border.Name == stateName)
            .MaxBy(border => border.Bounds.Width * border.Bounds.Height)!;
        Assert.Equal(root.Bounds.Width, state.Bounds.Width, precision: 3);
        Assert.Equal(root.Bounds.Height, state.Bounds.Height, precision: 3);
    }

    private static Scope Show(Control content, double width, double height)
    {
        var window = new Window { Width = width, Height = height, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new Scope(window);
    }

    private sealed class Scope(Window window) : IDisposable
    {
        public Window Window { get; } = window;
        public void Dispose() => Window.Close();
    }

    private sealed class ComboBoxProbe : MdComboBox
    {
        public void Prepare(Control container, object? item, int index) => PrepareContainerForItemOverride(container, item, index);
    }

    private sealed record SearchEntry(string Name);
    private sealed record GridRow(string Name, int Score);
}
