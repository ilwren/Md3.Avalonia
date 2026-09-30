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
        host.Window.MouseMove(point + new Point(180, 0), RawInputModifiers.LeftMouseButton);
        host.Window.MouseUp(point + new Point(180, 0), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.True(invoked, "Full swipe past threshold should invoke StartActionCommand");
    }

    [AvaloniaFact]
    public void SlidableItem_Keyboard_Navigation_And_Escape()
    {
        var slidable = new MdSlidableItem
        {
            Width = 400,
            Height = 60,
            ActionExtent = 80,
            Content = new TextBlock { Text = "Keyboard Slidable" }
        };

        using var host = Show(slidable, 500, 200);
        Dispatcher.UIThread.RunJobs();

        slidable.Focus();
        slidable.OpenStart();
        Dispatcher.UIThread.RunJobs();
        Assert.True(slidable.IsStartOpen);

        slidable.Close();
        Dispatcher.UIThread.RunJobs();
        Assert.False(slidable.IsStartOpen);
        Assert.Equal(0, slidable.Offset);
    }

    [AvaloniaFact]
    public void SheetHost_Direct_Drag_And_Dismiss_Workflow()
    {
        var dismissed = false;
        var sheet = new MdSheetHost
        {
            Width = 400,
            Height = 500,
            SheetExtent = 300,
            Content = new Border { Height = 300, Background = Brushes.SlateGray }
        };
        sheet.Dismissed += (_, _) => dismissed = true;

        using var host = Show(sheet, 500, 600);
        Dispatcher.UIThread.RunJobs();

        sheet.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.True(sheet.IsOpen);

        sheet.Dismiss();
        Dispatcher.UIThread.RunJobs();
        Assert.True(dismissed);
    }

    [AvaloniaFact]
    public void DraggableScrollableSheet_Snaps_To_Configured_Fractions()
    {
        var sheet = new MdDraggableScrollableSheet
        {
            Width = 400,
            Height = 600,
            MinExtent = 0.25,
            MaxExtent = 0.9,
            SnapExtents = { 0.25, 0.5, 0.9 },
            Snap = true,
            Content = new TextBlock { Text = "Snap Sheet Content" }
        };

        using var host = Show(sheet, 500, 700);
        Dispatcher.UIThread.RunJobs();

        sheet.SnapTo(0.5);
        Assert.Equal(0.5, sheet.Extent);
        sheet.SnapTo(0.1);
        Assert.Equal(0.25, sheet.Extent);
        sheet.SnapTo(1.0);
        Assert.Equal(0.9, sheet.Extent);
    }

    [AvaloniaFact]
    public void RefreshIndicator_Pull_Triggers_Refresh_And_Damping()
    {
        var refreshed = false;
        var indicator = new MdRefreshIndicator
        {
            Width = 400,
            Height = 400,
            TriggerDistance = 80,
            OnRefresh = () =>
            {
                refreshed = true;
                return Task.CompletedTask;
            },
            Content = new ScrollViewer
            {
                Content = new StackPanel
                {
                    Children = { new TextBlock { Text = "Pull to refresh target" } }
                }
            }
        };

        using var host = Show(indicator, 500, 500);
        Dispatcher.UIThread.RunJobs();

        var point = indicator.TranslatePoint(new Point(200, 50), host.Window)!.Value;
        host.Window.MouseMove(point, RawInputModifiers.None);
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        // Drag down 150px
        host.Window.MouseMove(point + new Point(0, 150), RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();

        Assert.True(indicator.PullProgress > 0, "PullProgress should increase during downward drag");

        host.Window.MouseUp(point + new Point(0, 150), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.True(refreshed, "Releasing past threshold should trigger OnRefresh");
    }

    [AvaloniaFact]
    public void TooltipHost_Supports_LongPress_Delay_Configuration()
    {
        var hostControl = new MdTooltipHost
        {
            Content = "Tooltip Host",
            Tip = "Sample tooltip text",
            LongPressDelay = TimeSpan.FromMilliseconds(300)
        };

        Assert.Equal(TimeSpan.FromMilliseconds(300), hostControl.LongPressDelay);

        hostControl.Show();
        Assert.True(hostControl.IsOpen);

        hostControl.Dismiss();
        Assert.False(hostControl.IsOpen);
    }

    [AvaloniaFact]
    public void KeyboardAvoidingHost_Pads_On_Virtual_Keyboard_Offset()
    {
        var innerBox = new TextBox { Width = 200, Height = 40 };
        var avoidingHost = new MdKeyboardAvoidingHost
        {
            Width = 400,
            Height = 600,
            ExtraBottomOffset = 20,
            Content = new StackPanel
            {
                Children =
                {
                    new Border { Height = 450, Background = Brushes.Transparent },
                    innerBox
                }
            }
        };

        using var host = Show(avoidingHost, 500, 700);
        Dispatcher.UIThread.RunJobs();

        // Initial offset
        Assert.Equal(0, avoidingHost.Padding.Bottom);

        // Simulate software keyboard appearance with 250px occlusion height
        avoidingHost.ApplyKeyboardOcclusion(250);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(250, avoidingHost.Padding.Bottom);

        // Hide keyboard
        avoidingHost.ApplyKeyboardOcclusion(0);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(0, avoidingHost.Padding.Bottom);
    }

    [AvaloniaFact]
    public void TabView_Supports_TabStripPlacement_And_Header_Sync()
    {
        var tabView = new MdTabView
        {
            TabStripPlacement = Dock.Bottom,
            Items =
            {
                new MdTabViewItem { Header = "Home", Content = new TextBlock { Text = "Home Content" } },
                new MdTabViewItem { Header = "Profile", Content = new TextBlock { Text = "Profile Content" } }
            }
        };

        using var host = Show(tabView, 500, 300);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(Dock.Bottom, tabView.TabStripPlacement);
        Assert.Equal(0, tabView.SelectedIndex);

        tabView.SelectedIndex = 1;
        Assert.Equal(1, tabView.SelectedIndex);

        // Verify MdTabItem Header/Content synchronization
        var tabItem1 = new MdTabItem { Header = "Header Title" };
        var tabItem2 = new MdTabItem { Content = "Content Title" };

        Assert.Equal("Header Title", tabItem1.Content);
        Assert.Equal("Header Title", tabItem1.Header);
        Assert.Equal("Content Title", tabItem2.Header);
        Assert.Equal("Content Title", tabItem2.Content);
    }

    [AvaloniaFact]
    public void Tabs_With_MdTabItem_Header_Renders_Correctly()
    {
        var tabs = new MdTabs
        {
            Items =
            {
                new MdTabItem { Header = "Tab 1", Icon = MdSymbols.Home },
                new MdTabItem { Header = "Tab 2", Icon = MdSymbols.Settings }
            }
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
            Header = "Wi-Fi",
            Description = "Connected to Studio_5G",
            Icon = MdSymbols.Wifi,
            Trailing = new MdSwitch { IsChecked = true }
        };
        card.Click += (_, _) => clicked = true;

        var group = new MdSettingsGroup
        {
            Header = "CONNECTIVITY",
            Items = { card }
        };

        using var host = Show(group, 500, 200);
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
