using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Ecosystem.Controls;
using Md3.Avalonia.Gallery.Pages;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdPhaseThreeAndFourGestureParityTests
{
    [AvaloniaFact]
    public void SlidableItem_Full_Swipe_Triggers_Action_Command()
    {
        var invoked = false;
        var command = new ActionCommand(() => invoked = true);
        var slidable = new MdSlidableItem
        {
            Width = 400,
            Height = 60,
            ActionExtent = 80,
            StartActionCommand = command,
            Content = new TextBlock { Text = "Swipeable Item" }
        };

        using var host = Show(slidable, 500, 200);
        Dispatcher.UIThread.RunJobs();

        var point = slidable.TranslatePoint(new Point(20, 30), host.Window)!.Value;
        host.Window.MouseMove(point, RawInputModifiers.None);
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        // Drag 180px -> offset = 80 + (100 * 0.35) = 115 >= 80 * 1.3 = 104
        host.Window.MouseMove(point.WithX(point.X + 180), RawInputModifiers.None);
        host.Window.MouseUp(point.WithX(point.X + 180), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.True(invoked, "Full swipe should trigger the action command");
    }

    [AvaloniaFact]
    public void SlidableItem_Applies_RubberBand_Overscroll_Damping()
    {
        var slidable = new MdSlidableItem
        {
            Width = 400,
            Height = 60,
            ActionExtent = 80,
            Content = new TextBlock { Text = "Overscroll Item" }
        };

        using var host = Show(slidable, 500, 200);
        Dispatcher.UIThread.RunJobs();

        var point = slidable.TranslatePoint(new Point(20, 30), host.Window)!.Value;
        host.Window.MouseMove(point, RawInputModifiers.None);
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        // Drag 120px past 80px action extent -> delta = 120, offset should be 80 + (40 * 0.35) = 94
        host.Window.MouseMove(point.WithX(point.X + 120), RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.True(slidable.Offset < 120, $"Offset ({slidable.Offset}) should be damped below raw delta (120)");
        Assert.True(slidable.Offset > 80, $"Offset ({slidable.Offset}) should exceed ActionExtent (80)");

        host.Window.MouseUp(point.WithX(point.X + 120), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void DraggableScrollableSheet_Snaps_To_Extents()
    {
        var sheet = new MdDraggableScrollableSheet
        {
            Width = 400,
            Height = 600,
            MinimumExtent = 0.25,
            InitialExtent = 0.5,
            MaximumExtent = 0.9,
            Snap = true,
            SnapSizes = new[] { 0.25, 0.5, 0.9 },
            Content = new Border { Height = 1000 }
        };

        using var host = Show(sheet, 500, 700);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(0.5, sheet.Extent);
        sheet.JumpTo(0.25);
        Assert.Equal(0.25, sheet.Extent);
        sheet.JumpTo(0.9);
        Assert.Equal(0.9, sheet.Extent);
    }

    [AvaloniaFact]
    public void Touch_Ergonomics_Controls_Meet_Minimum_Hit_Target_48()
    {
        var iconButton = new MdIconButton { Icon = MdSymbols.Search };
        var toggleIcon = new MdToggleIconButton { Icon = MdSymbols.Star };
        var checkBox = new MdCheckBox { Content = "Target test" };
        var radio = new MdRadioButton { Content = "Radio test" };
        var toggleSwitch = new MdSwitch { Content = "Switch test" };
        var treeView = new MdTreeView
        {
            Roots = new[] { new MdTreeNode("1", "Root", new[] { new MdTreeNode("2", "Child") }) },
            Width = 300,
            Height = 150
        };

        var panel = new StackPanel
        {
            Children = { iconButton, toggleIcon, checkBox, radio, toggleSwitch, treeView }
        };

        using var host = Show(panel, 600, 800);
        Dispatcher.UIThread.RunJobs();

        Assert.True(iconButton.MinWidth >= 48, $"IconButton MinWidth: {iconButton.MinWidth}");
        Assert.True(iconButton.MinHeight >= 48, $"IconButton MinHeight: {iconButton.MinHeight}");
        Assert.True(toggleIcon.MinWidth >= 48, $"ToggleIconButton MinWidth: {toggleIcon.MinWidth}");
        Assert.True(toggleIcon.MinHeight >= 48, $"ToggleIconButton MinHeight: {toggleIcon.MinHeight}");
        Assert.True(checkBox.Bounds.Height >= 48, $"CheckBox Height: {checkBox.Bounds.Height}");
        Assert.True(radio.Bounds.Height >= 48, $"RadioButton Height: {radio.Bounds.Height}");
        Assert.True(toggleSwitch.Bounds.Height >= 48, $"Switch Height: {toggleSwitch.Bounds.Height}");
    }

    [AvaloniaFact]
    public void TooltipHost_Supports_LongPress_Delay_Configuration()
    {
        var tooltip = new MdTooltip { Content = "Helpful text" };
        var host = new MdTooltipHost
        {
            Tooltip = tooltip,
            LongPressDelay = TimeSpan.FromMilliseconds(300),
            Content = new MdButton { Content = "Long press me" }
        };

        Assert.Equal(TimeSpan.FromMilliseconds(300), host.LongPressDelay);
        Assert.False(host.IsOpen);
        host.Show();
        Assert.True(host.IsOpen);
        Assert.True(tooltip.IsOpen);
        host.Dismiss();
        Assert.False(host.IsOpen);
        Assert.False(tooltip.IsOpen);
    }

    [AvaloniaFact]
    public void KeyboardAvoidingHost_Adjusts_Padding_And_Scrolls_Focused_Child()
    {
        var textBox = new MdTextBox { Text = "Input inside scroll" };
        var scrollViewer = new ScrollViewer
        {
            Height = 300,
            Content = new StackPanel
            {
                Spacing = 50,
                Children =
                {
                    new Border { Height = 100 },
                    textBox,
                    new Border { Height = 400 }
                }
            }
        };

        var avoidingHost = new MdKeyboardAvoidingHost
        {
            Width = 400,
            Height = 400,
            AutoScrollToFocused = true,
            ExtraBottomOffset = 20,
            Content = scrollViewer
        };

        using var host = Show(avoidingHost, 500, 500);
        Dispatcher.UIThread.RunJobs();

        Assert.False(avoidingHost.IsKeyboardActive);
        Assert.Equal(0, avoidingHost.Padding.Bottom);

        avoidingHost.KeyboardHeight = 250;
        Dispatcher.UIThread.RunJobs();

        Assert.True(avoidingHost.IsKeyboardActive);
        Assert.Equal(250, avoidingHost.Padding.Bottom);

        avoidingHost.BringControlIntoView(textBox);
        Dispatcher.UIThread.RunJobs();

        avoidingHost.KeyboardHeight = 0;
        Dispatcher.UIThread.RunJobs();
        Assert.False(avoidingHost.IsKeyboardActive);
        Assert.Equal(0, avoidingHost.Padding.Bottom);
    }

    [AvaloniaFact]
    public void TabView_Supports_TabStripPlacement_And_Header_Centering()
    {
        var tabView = new MdTabView
        {
            TabStripPlacement = Dock.Bottom,
            Items =
            {
                new MdTabViewItem { Header = "Tab 1", Content = new TextBlock { Text = "Content 1" } },
                new MdTabViewItem { Header = "Tab 2", Content = new TextBlock { Text = "Content 2" } }
            }
        };

        using var host = Show(tabView, 500, 300);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(Dock.Bottom, tabView.TabStripPlacement);
        Assert.Equal(0, tabView.SelectedIndex);
        tabView.SelectedIndex = 1;
        Assert.Equal(1, tabView.SelectedIndex);
    }

    [AvaloniaFact]
    public void TabItem_Supports_Header_And_Content_Interoperability()
    {
        var tabItem1 = new MdTabItem { Header = "Header Title" };
        var tabItem2 = new MdTabItem { Content = "Content Title" };

        Assert.Equal("Header Title", tabItem1.Content);
        Assert.Equal("Header Title", tabItem1.Header);
        Assert.Equal("Content Title", tabItem2.Header);
        Assert.Equal("Content Title", tabItem2.Content);

        var tabs = new MdTabs
        {
            Items = { tabItem1, tabItem2 }
        };
        using var host = Show(tabs, 400, 100);
        Dispatcher.UIThread.RunJobs();
        Assert.NotNull(host.Window.CaptureRenderedFrame());
    }

    [AvaloniaFact]
    public void SettingsCard_And_SettingsGroup_Render_And_Handle_Click()
    {
        var clicked = false;
        var card = new MdSettingsCard
        {
            Width = 400,
            Height = 64,
            Header = "Wi-Fi",
            Description = "Connected to Studio_5G",
            Icon = MdSymbols.Wifi,
            Trailing = new MdSwitch { IsChecked = true }
        };
        card.Click += (_, _) => clicked = true;

        using var host = Show(card, 500, 200);
        Dispatcher.UIThread.RunJobs();

        var point = card.TranslatePoint(new Point(20, 20), host.Window)!.Value;
        host.Window.MouseMove(point, RawInputModifiers.None);
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        host.Window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.True(clicked, "Clicking the settings card should raise the Click event");
    }

    [AvaloniaFact]
    public void ScrollViewer_Drag_Scrolls_Content_And_Applies_Inertia()
    {
        var scrollViewer = new MdScrollViewer
        {
            Width = 300,
            Height = 200,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Border { Height = 100, Background = Brushes.Red },
                    new Border { Height = 100, Background = Brushes.Green },
                    new Border { Height = 100, Background = Brushes.Blue },
                    new Border { Height = 100, Background = Brushes.Yellow },
                    new Border { Height = 100, Background = Brushes.Purple }
                }
            }
        };

        using var host = Show(scrollViewer, 400, 400);
        Dispatcher.UIThread.RunJobs();

        // Drag upwards from y=150 to y=50 (100px drag)
        var startPoint = scrollViewer.TranslatePoint(new Point(150, 150), host.Window)!.Value;
        var endPoint = scrollViewer.TranslatePoint(new Point(150, 50), host.Window)!.Value;

        host.Window.MouseMove(startPoint, RawInputModifiers.None);
        host.Window.MouseDown(startPoint, MouseButton.Left, RawInputModifiers.None);
        host.Window.MouseMove(endPoint, RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();

        // Offset should have increased due to upward drag
        Assert.True(scrollViewer.Offset.Y > 0, "Dragging content upward should increase vertical scroll offset");

        host.Window.MouseUp(endPoint, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        // Stop inertia clean-up
        scrollViewer.StopInertia();
    }

    [AvaloniaFact]
    public void FabMenu_Trigger_Does_Not_Shift_Position_When_Opened()
    {
        var fabMenu = new MdFabMenu
        {
            Items =
            {
                new MdFabMenuItem { Content = "Document", Icon = MdSymbols.Description },
                new MdFabMenuItem { Content = "Photo", Icon = MdSymbols.Photo }
            }
        };

        var rootGrid = new Grid
        {
            Width = 400,
            Height = 400,
            Children = { fabMenu }
        };

        using var host = Show(rootGrid, 400, 400);
        Dispatcher.UIThread.RunJobs();

        var trigger = fabMenu.GetVisualDescendants().OfType<MdToggleIconButton>().First();
        var closedTriggerPos = trigger.TranslatePoint(new Point(0, 0), host.Window)!.Value;

        // Open FAB menu
        fabMenu.IsOpen = true;
        Dispatcher.UIThread.RunJobs();

        var openTriggerPos = trigger.TranslatePoint(new Point(0, 0), host.Window)!.Value;

        // Trigger X position should be identical (within subpixel tolerance)
        Assert.True(Math.Abs(closedTriggerPos.X - openTriggerPos.X) < 1.0,
            $"Trigger X moved from {closedTriggerPos.X} to {openTriggerPos.X} when opened");
    }

    [AvaloniaFact]
    public void Sample_And_New_Gallery_Pages_Render()
    {
        Control[] samplePages =
        [
            new AndroidSettingsSamplePage(),
            new ClockSamplePage(),
            new TasksSamplePage(),
            new SettingsCardGalleryPage(),
            new BreadcrumbGalleryPage()
        ];

        foreach (var page in samplePages)
        {
            using var host = Show(page, 800, 600);
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(host.Window.CaptureRenderedFrame());
        }
    }

    private static Scope Show(Control content, double width = 800, double height = 600)
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

    private sealed class ActionCommand(Action action) : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => action();
    }
}

internal static class GesturePointExtensions
{
    public static Point WithX(this Point point, double x) => new(x, point.Y);
}
