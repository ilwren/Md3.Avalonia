using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Md3.Avalonia.Extra.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdEcosystemWaveFTests
{
    [AvaloniaFact]
    public void PinInput_Sanitizes_Paste_Projects_States_And_Completes_Once()
    {
        var executed = string.Empty;
        var completions = 0;
        var pin = new MdPinInput { Length = 6, CompletedCommand = new TestCommand(value => executed = (string)value!) };
        pin.Completed += (_, code) => { completions++; Assert.Equal("123456", code); };

        pin.SetCode("12a345678");

        Assert.Equal("123456", pin.Code);
        Assert.Equal("123456", executed);
        Assert.Equal(1, completions);
        Assert.Equal(6, pin.Cells.Count);
        Assert.All(pin.Cells, cell => Assert.True(cell.IsFilled));

        pin.IsObscured = true;
        Assert.All(pin.Cells, cell => Assert.Equal("•", cell.DisplayText));
        pin.SetCode("123456");
        Assert.Equal(1, completions);
        pin.Clear();
        Assert.All(pin.Cells, cell => Assert.False(cell.IsFilled));
    }

    [AvaloniaFact]
    public void TreeView_Expands_Selects_Reveals_And_Mirrors_Indentation()
    {
        var leaf = new MdTreeNode("leaf", "Leaf");
        var branch = new MdTreeNode("branch", "Branch", [leaf]);
        var tree = new MdTreeView { Roots = [branch] };

        Assert.Single(tree.VisibleRows);
        Assert.True(tree.Expand(branch));
        Assert.Equal(2, tree.VisibleRows.Count);
        Assert.True(tree.SelectById("leaf"));
        Assert.Same(leaf, tree.SelectedNode);
        Assert.True(tree.VisibleRows[1].Indentation.Left > 0);

        tree.FlowDirection = global::Avalonia.Media.FlowDirection.RightToLeft;
        Assert.True(tree.VisibleRows[1].Indentation.Right > 0);
        Assert.Equal(0, tree.VisibleRows[1].Indentation.Left);
        tree.CollapseAll();
        Assert.Single(tree.VisibleRows);
        tree.ExpandAll();
        Assert.Equal(2, tree.VisibleRows.Count);
    }

    [AvaloniaFact]
    public void TagInput_Uses_Mutable_Mvvm_Source_Validation_Suggestions_And_Direct_APIs()
    {
        var source = new ObservableCollection<string> { "Android" };
        var tags = new MdTagInput
        {
            TagsSource = source,
            SuggestionsSource = ["Accessibility", "Android", "Localization", "Motion"],
            MaximumTags = 3,
            TagValidator = value => value.Length < 3 ? "Too short" : null
        };

        tags.Text = "Loc";
        Assert.Equal(["Localization"], tags.FilteredSuggestions);
        Assert.True(tags.AddTag("Localization"));
        Assert.Equal(["Android", "Localization"], source);
        Assert.False(tags.AddTag("Android"));
        Assert.Equal("Tag already added", tags.ValidationMessage);
        Assert.False(tags.AddTag("x"));
        Assert.Equal("Too short", tags.ValidationMessage);
        Assert.True(tags.RemoveTag("Android"));
        Assert.Equal(["Localization"], source);

        tags.Text = "Motion,Accessibility,";
        Assert.Equal(["Localization", "Motion", "Accessibility"], source);
        Assert.Equal(3, tags.Tags.Count);
        tags.ClearTags();
        Assert.Empty(source);
    }

    [AvaloniaFact]
    public void Breadcrumb_Supports_Rich_Item_Model_Commands_And_Separators()
    {
        var invokedCommandItem = string.Empty;
        var homeItem = new MdBreadcrumbItem
        {
            Label = "Home",
            Command = new TestCommand(param => invokedCommandItem = "Home")
        };
        var componentsItem = new MdBreadcrumbItem
        {
            Label = "Components",
            Command = new TestCommand(param => invokedCommandItem = "Components")
        };
        var ecosystemItem = new MdBreadcrumbItem
        {
            Label = "Ecosystem",
            IsCurrent = true
        };

        var breadcrumb = new MdBreadcrumb
        {
            ItemsSource = new ObservableCollection<MdBreadcrumbItem> { homeItem, componentsItem, ecosystemItem },
            Separator = "/"
        };

        var invoked = string.Empty;
        breadcrumb.ItemInvoked += (_, item) => invoked = (item as MdBreadcrumbItem)?.Label?.ToString() ?? string.Empty;

        breadcrumb.Invoke(homeItem);
        Assert.Equal("Home", invoked);
        Assert.Equal("Home", invokedCommandItem);
        Assert.Equal("/", breadcrumb.Separator);
    }

    [AvaloniaFact]
    public void WaveF_Controls_Render_Together_With_Real_Themes()
    {
        var branch = new MdTreeNode("root", "Root", [new MdTreeNode("child", "Child")]) { IsExpanded = true };
        var root = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                new MdPinInput { Code = "426", Length = 6 },
                new MdTreeView { Roots = new[] { branch }, Height = 180 },
                new MdTagInput { TagsSource = new ObservableCollection<string> { "Material", "Avalonia" }, SuggestionsSource = new[] { "Android" } }
            }
        };
        var window = new Window { Width = 720, Height = 520, Content = root };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(window.CaptureRenderedFrame());
            Assert.True(root.Bounds.Width > 0);
            Assert.True(root.Children.All(control => control.Bounds.Height > 0));
        }
        finally { window.Close(); }
    }

    private sealed class TestCommand(Action<object?> execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute(parameter);
    }
}
