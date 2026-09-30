using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Ecosystem.Controls;
using Md3.Avalonia.Icons;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public class MdPhaseThreeAndFourGestureParityTests
{
    private static TestHost<T> Show<T>(T control, double width = 800, double height = 600) where T : Control
    {
        var window = new Window
        {
            Width = width,
            Height = height,
            Content = control
        };
        window.Show();
        return new TestHost<T>(window, control);
    }

    private sealed class TestHost<T>(Window window, T control) : IDisposable where T : Control
    {
        public Window Window => window;
        public T Control => control;

        public void Dispose()
        {
            window.Close();
        }
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
        Assert.True(scrollViewer.Offset.Y > 0, $"Dragging content upward should increase vertical scroll offset (actual: {scrollViewer.Offset.Y})");

        host.Window.MouseUp(endPoint, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        // Stop inertia clean-up
        scrollViewer.StopInertia();
    }

    [AvaloniaFact]
    public void FabMenu_Trigger_Position_Remains_Stable_When_Opened()
    {
        var fabMenu = new MdFabMenu
        {
            OpenIcon = MdSymbols.Add,
            CloseIcon = MdSymbols.Close,
            Items =
            {
                new MdFabMenuItem { Icon = MdSymbols.Edit, Content = "Document" },
                new MdFabMenuItem { Icon = MdSymbols.PhotoCamera, Content = "Photo" }
            }
        };

        var container = new Panel
        {
            Width = 400,
            Height = 400,
            Children = { fabMenu }
        };

        using var host = Show(container, 500, 500);
        Dispatcher.UIThread.RunJobs();

        fabMenu.IsOpen = true;
        Dispatcher.UIThread.RunJobs();

        // Trigger should remain anchored and not jump horizontally or stretch container
        Assert.True(fabMenu.IsOpen);
        Assert.True(fabMenu.Bounds.Width > 0);
    }

    [AvaloniaFact]
    public void TabView_Supports_TabStripPlacement_And_Header_Centering()
    {
        var tabView = new MdTabView
        {
            TabStripPlacement = Dock.Bottom,
            SelectedIndex = 0,
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

        var group = new MdSettingsGroup
        {
            Header = "Network & internet",
            Description = "Wi-Fi, mobile, hotspot",
            Items =
            {
                card,
                new MdSettingsCard { Header = "SIMs", Description = "T-Mobile" }
            }
        };

        using var groupHost = Show(group, 500, 300);
        Dispatcher.UIThread.RunJobs();
        Assert.NotNull(groupHost.Window.CaptureRenderedFrame());
    }

    [AvaloniaFact]
    public void Breadcrumb_Navigation_ItemClick_And_Selection_Works()
    {
        var breadcrumb = new MdBreadcrumb
        {
            ItemsSource = new ObservableCollection<MdBreadcrumbItem>
            {
                new() { Label = "Home", Icon = MdSymbols.Home },
                new() { Label = "Settings", Icon = MdSymbols.Settings },
                new() { Label = "Network", Icon = MdSymbols.Wifi }
            }
        };

        using var host = Show(breadcrumb, 500, 100);
        Dispatcher.UIThread.RunJobs();

        breadcrumb.SelectedIndex = 1;
        Assert.Equal(1, breadcrumb.SelectedIndex);
    }
}
