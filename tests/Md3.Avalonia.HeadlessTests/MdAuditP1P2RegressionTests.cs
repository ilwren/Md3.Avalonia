using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Localization;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdAuditP1P2RegressionTests
{
    [AvaloniaFact]
    public void AppBar_And_Toolbar_Defaults_Use_EdgeToEdge_And_Component_Tokens()
    {
        var appBar = new MdTopAppBar { Title = "Library" };
        var toolbar = new MdToolbar { Density = MdToolbarDensity.Compact };
        using var host = Show(new StackPanel { Children = { appBar, toolbar } }, 720, 240);

        Assert.Equal(new Thickness(0), appBar.BorderThickness);
        Assert.Equal(new CornerRadius(0), appBar.CornerRadius);
        Assert.Equal(56, toolbar.MinHeight);
        Assert.Equal(new Thickness(8, 4), toolbar.Padding);
    }

    [AvaloniaFact]
    public void ButtonGroups_Only_Manage_Direct_Containers_And_Preserve_Explicit_Size()
    {
        var inherited = new MdButton { Content = "Inherited" };
        var explicitSize = new MdButton { Content = "Explicit", Size = MdButtonSize.Large };
        var nested = new MdButton { Content = "Nested", Size = MdButtonSize.ExtraLarge };
        var standard = new MdStandardButtonGroup { Size = MdButtonSize.Medium, Items = { inherited, explicitSize } };
        var connected = new MdConnectedButtonGroup
        {
            Size = MdButtonSize.Small,
            Items = { new Border { Child = nested }, new MdButton { Content = "Direct" } }
        };
        using var host = Show(new StackPanel { Children = { standard, connected } }, 900, 300);

        Assert.Equal(MdButtonSize.Medium, inherited.Size);
        Assert.Equal(MdButtonSize.Large, explicitSize.Size);
        Assert.Equal(MdButtonSize.ExtraLarge, nested.Size);
    }

    [AvaloniaFact]
    public void SplitButton_Uses_Frozen_Trailing_Size_And_Names_Both_Actions()
    {
        var split = new MdSplitButton { Content = "Save", Size = MdButtonSize.Large };
        using var host = Show(split, 420, 160);
        var primary = split.GetVisualDescendants().OfType<MdButton>().Single(button => button.Name == "PART_PrimaryButton");
        var trailing = split.GetVisualDescendants().OfType<MdToggleIconButton>().Single(button => button.Name == "PART_TrailingButton");

        Assert.Equal(72, trailing.ContainerWidth);
        Assert.Equal("Show more actions", AutomationProperties.GetName(trailing));
        Assert.Equal("Save", primary.Content);
    }

    [AvaloniaFact]
    public void SpatialSpring_Retargeting_Preserves_InFlight_Velocity()
    {
        var owner = new Border();
        MdMotion.SetScheme(owner, MdMotionScheme.Expressive);
        var value = 0d;
        using var runner = new MdSpatialSpringRunner(owner, next => value = next);
        runner.Retarget(100);
        runner.Stop();
        runner.Advance(TimeSpan.FromMilliseconds(32));
        var velocity = runner.Velocity;

        Assert.True(value > 0);
        Assert.True(velocity > 0);
        runner.Retarget(0);
        runner.Stop();
        Assert.Equal(velocity, runner.Velocity);
    }

    [AvaloniaFact]
    public void Cascader_Uses_An_OutOfFlow_Popup_And_Transfer_Reflows_At_Compact_Width()
    {
        var cascader = new MdCascader
        {
            Width = 320,
            ItemsSource = [new MdCascaderItem("root", "Root", [new MdCascaderItem("leaf", "Leaf")])]
        };
        var transfer = new MdTransfer
        {
            Width = 420,
            ItemsSource = new[] { "One", "Two" },
            SelectedItems = new[] { "Two" }
        };
        using var host = Show(new StackPanel { Children = { cascader, transfer } }, 520, 760);
        var closedHeight = cascader.Bounds.Height;
        cascader.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();

        Assert.True(cascader.IsPopupOpen);
        Assert.Equal(closedHeight, cascader.Bounds.Height);
        Assert.True(transfer.IsCompact);
        var source = transfer.GetVisualDescendants().OfType<Border>().Single(border => border.Name == "PART_SourceSurface");
        var target = transfer.GetVisualDescendants().OfType<Border>().Single(border => border.Name == "PART_TargetSurface");
        Assert.True(target.Bounds.Top > source.Bounds.Bottom);
    }

    [AvaloniaFact]
    public void Breadcrumb_Overflow_Uses_An_Invokable_Ellipsis_And_Expands_The_Path()
    {
        var breadcrumb = new MdBreadcrumb
        {
            MaxDisplayedItems = 3,
            ItemsBeforeCollapse = 1,
            ItemsAfterCollapse = 1,
            ItemsSource = new[] { "Home", "Library", "Components", "Inputs", "Combo box" }
        };
        using var host = Show(breadcrumb, 640, 120);
        var containers = breadcrumb.GetVisualDescendants().OfType<ListBoxItem>().ToArray();

        Assert.Equal(3, containers.Count(item => item.IsVisible));
        Assert.Equal("Show full breadcrumb path", AutomationProperties.GetName(containers[1]));
        breadcrumb.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();
        Assert.True(breadcrumb.IsOverflowExpanded);
        Assert.All(containers, item => Assert.True(item.IsVisible));
    }

    [AvaloniaFact]
    public void Ecosystem_Status_And_Default_Names_Follow_Inherited_Culture()
    {
        var transfer = new MdTransfer();
        var chat = new MdChatView();
        var breadcrumb = new MdBreadcrumb { ItemsSource = new[] { "首页", "组件" } };
        var root = new StackPanel { Children = { transfer, chat, breadcrumb } };
        MdLocalization.SetCulture(root, System.Globalization.CultureInfo.GetCultureInfo("zh-CN"));
        using var host = Show(root, 720, 500);

        Assert.Equal("穿梭列表", AutomationProperties.GetName(transfer));
        Assert.Equal("全部", transfer.AllText);
        Assert.Equal("取消", chat.CancelText);
        Assert.Equal("面包屑导航", AutomationProperties.GetName(breadcrumb));
    }

    [Fact]
    public void Disabled_CommandPalette_Item_Exposes_Disabled_State()
    {
        var command = new DisabledCommand();
        var item = new MdCommandItem("Unavailable", command);
        Assert.False(item.IsEnabled);
        var palette = new MdCommandPalette { ItemsSource = new[] { item } };
        Assert.Single(palette.FilteredItems);
        Assert.False(palette.FilteredItems[0].IsEnabled);
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

    private sealed class DisabledCommand : System.Windows.Input.ICommand
    {
        public bool CanExecute(object? parameter) => false;
        public void Execute(object? parameter) { }
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }
}
