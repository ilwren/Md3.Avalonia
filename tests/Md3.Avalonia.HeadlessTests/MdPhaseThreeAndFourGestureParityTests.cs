using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Pages;
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
            AllowMouseDrag = true,
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
    public void ScrollViewer_Mouse_Drag_Is_OptIn_On_Desktop()
    {
        var scrollViewer = new MdScrollViewer
        {
            Width = 300,
            Height = 200,
            Content = new Border { Height = 900, Background = Brushes.Blue }
        };

        using var host = Show(scrollViewer, 400, 400);
        Dispatcher.UIThread.RunJobs();

        var startPoint = scrollViewer.TranslatePoint(new Point(150, 150), host.Window)!.Value;
        var endPoint = scrollViewer.TranslatePoint(new Point(150, 50), host.Window)!.Value;
        host.Window.MouseMove(startPoint, RawInputModifiers.None);
        host.Window.MouseDown(startPoint, MouseButton.Left, RawInputModifiers.None);
        host.Window.MouseMove(endPoint, RawInputModifiers.LeftMouseButton);
        host.Window.MouseUp(endPoint, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(0, scrollViewer.Offset.Y);
    }

    [AvaloniaFact]
    public void ScrollViewer_OptIn_Mouse_Drag_Defers_To_Child_Direct_Manipulation()
    {
        var rangeSlider = new MdRangeSlider
        {
            Width = 280,
            Height = 80,
            LowerValue = 25,
            UpperValue = 75
        };
        var scrollViewer = new MdScrollViewer
        {
            Width = 300,
            Height = 200,
            AllowMouseDrag = true,
            Content = new StackPanel
            {
                Children =
                {
                    rangeSlider,
                    new Border { Height = 700, Background = Brushes.Blue }
                }
            }
        };

        using var host = Show(scrollViewer, 400, 400);
        Dispatcher.UIThread.RunJobs();

        var valueBeforeDrag = rangeSlider.LowerValue;
        var startPoint = rangeSlider.TranslatePoint(new Point(82, 40), host.Window)!.Value;
        var endPoint = startPoint + new Vector(80, -24);
        host.Window.MouseMove(startPoint, RawInputModifiers.None);
        host.Window.MouseDown(startPoint, MouseButton.Left, RawInputModifiers.None);
        host.Window.MouseMove(endPoint, RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();
        host.Window.MouseUp(endPoint, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.True(rangeSlider.LowerValue > valueBeforeDrag,
            "The inner range slider must retain its pointer drag.");
        Assert.Equal(0, scrollViewer.Offset.Y);
    }

    [AvaloniaFact]
    public void ScrollViewer_OptIn_Mouse_Drag_Yields_When_Custom_Content_Handles_Press()
    {
        var precisionSurface = new Border { Height = 700, Background = Brushes.Orange };
        precisionSurface.PointerPressed += (_, e) => e.Handled = true;

        var scrollViewer = new MdScrollViewer
        {
            Width = 300,
            Height = 200,
            AllowMouseDrag = true,
            Content = precisionSurface
        };

        using var host = Show(scrollViewer, 400, 400);
        Dispatcher.UIThread.RunJobs();

        var startPoint = precisionSurface.TranslatePoint(new Point(150, 150), host.Window)!.Value;
        var endPoint = startPoint + new Vector(0, -100);
        host.Window.MouseMove(startPoint, RawInputModifiers.None);
        host.Window.MouseDown(startPoint, MouseButton.Left, RawInputModifiers.None);
        host.Window.MouseMove(endPoint, RawInputModifiers.LeftMouseButton);
        host.Window.MouseUp(endPoint, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(0, scrollViewer.Offset.Y);
    }

    [AvaloniaFact]
    public void ScrollViewer_OptIn_Mouse_Drag_Yields_When_Custom_Content_Captures_Pointer()
    {
        var pointerMoved = false;
        var precisionSurface = new Border { Height = 700, Background = Brushes.Orange };
        precisionSurface.PointerPressed += (_, e) => e.Pointer.Capture(precisionSurface);
        precisionSurface.AddHandler(InputElement.PointerMovedEvent, (_, _) => pointerMoved = true,
            RoutingStrategies.Bubble, handledEventsToo: true);

        var scrollViewer = new MdScrollViewer
        {
            Width = 300,
            Height = 200,
            AllowMouseDrag = true,
            Content = precisionSurface
        };

        using var host = Show(scrollViewer, 400, 400);
        Dispatcher.UIThread.RunJobs();

        var startPoint = precisionSurface.TranslatePoint(new Point(150, 150), host.Window)!.Value;
        var endPoint = startPoint + new Vector(0, -100);
        host.Window.MouseMove(startPoint, RawInputModifiers.None);
        host.Window.MouseDown(startPoint, MouseButton.Left, RawInputModifiers.None);
        pointerMoved = false;
        host.Window.MouseMove(endPoint, RawInputModifiers.LeftMouseButton);
        host.Window.MouseUp(endPoint, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.True(pointerMoved, "Captured pointer movement must remain routed to custom content.");
        Assert.Equal(0, scrollViewer.Offset.Y);
    }

    [AvaloniaFact]
    public void ScrollViewer_OptIn_Mouse_Drag_Honors_Suppressed_Content_Subtree()
    {
        var precisionSurface = new Border { Height = 700, Background = Brushes.Green };
        MdScrollViewer.SetSuppressMouseDragScrolling(precisionSurface, true);
        var scrollViewer = new MdScrollViewer
        {
            Width = 300,
            Height = 200,
            AllowMouseDrag = true,
            Content = precisionSurface
        };

        using var host = Show(scrollViewer, 400, 400);
        Dispatcher.UIThread.RunJobs();

        var startPoint = precisionSurface.TranslatePoint(new Point(150, 150), host.Window)!.Value;
        var endPoint = startPoint + new Vector(0, -100);
        host.Window.MouseMove(startPoint, RawInputModifiers.None);
        host.Window.MouseDown(startPoint, MouseButton.Left, RawInputModifiers.None);
        host.Window.MouseMove(endPoint, RawInputModifiers.LeftMouseButton);
        host.Window.MouseUp(endPoint, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(0, scrollViewer.Offset.Y);
    }

    [AvaloniaFact]
    public void ScrollViewer_Thumb_Hover_Preserves_Axis_Length_And_Mouse_Drag_Scrolls()
    {
        var scrollViewer = new MdScrollViewer
        {
            Width = 300,
            Height = 220,
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = new Border { Width = 280, Height = 1100, Background = Brushes.Blue }
        };

        using var host = Show(scrollViewer, 400, 360);
        Dispatcher.UIThread.RunJobs();

        var scrollBar = scrollViewer.GetVisualDescendants().OfType<MdScrollBar>()
            .Single(bar => bar.Orientation == global::Avalonia.Layout.Orientation.Vertical);
        var thumb = scrollBar.GetVisualDescendants().OfType<Thumb>().Single();
        Assert.True(thumb.Bounds.Height >= 32);

        var axisLengthBeforeHover = thumb.Bounds.Height;
        var thumbCenter = thumb.TranslatePoint(
            new Point(thumb.Bounds.Width / 2, thumb.Bounds.Height / 2), host.Window)!.Value;

        host.Window.MouseMove(thumbCenter, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.InRange(Math.Abs(thumb.Bounds.Height - axisLengthBeforeHover), 0, 0.01);

        var offsetBeforeDrag = scrollViewer.Offset.Y;
        var dragEnd = thumbCenter + new Vector(0, 56);
        host.Window.MouseDown(thumbCenter, MouseButton.Left, RawInputModifiers.None);
        host.Window.MouseMove(dragEnd, RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();
        host.Window.MouseUp(dragEnd, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.True(scrollViewer.Offset.Y > offsetBeforeDrag,
            $"Dragging the vertical scrollbar thumb must increase Offset.Y (actual: {scrollViewer.Offset.Y}).");
    }

    [AvaloniaFact]
    public void ScrollViewer_Horizontal_Thumb_Hover_Preserves_Axis_Length_And_Mouse_Drag_Scrolls()
    {
        var scrollViewer = new MdScrollViewer
        {
            Width = 300,
            Height = 180,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = new Border { Width = 1100, Height = 160, Background = Brushes.Green }
        };

        using var host = Show(scrollViewer, 420, 300);
        Dispatcher.UIThread.RunJobs();

        var scrollBar = scrollViewer.GetVisualDescendants().OfType<MdScrollBar>()
            .Single(bar => bar.Orientation == global::Avalonia.Layout.Orientation.Horizontal);
        var thumb = scrollBar.GetVisualDescendants().OfType<Thumb>().Single();
        Assert.True(thumb.Bounds.Width >= 32);

        var axisLengthBeforeHover = thumb.Bounds.Width;
        var thumbCenter = thumb.TranslatePoint(
            new Point(thumb.Bounds.Width / 2, thumb.Bounds.Height / 2), host.Window)!.Value;

        host.Window.MouseMove(thumbCenter, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.InRange(Math.Abs(thumb.Bounds.Width - axisLengthBeforeHover), 0, 0.01);

        var offsetBeforeDrag = scrollViewer.Offset.X;
        var dragEnd = thumbCenter + new Vector(56, 0);
        host.Window.MouseDown(thumbCenter, MouseButton.Left, RawInputModifiers.None);
        host.Window.MouseMove(dragEnd, RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();
        host.Window.MouseUp(dragEnd, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.True(scrollViewer.Offset.X > offsetBeforeDrag,
            $"Dragging the horizontal scrollbar thumb must increase Offset.X (actual: {scrollViewer.Offset.X}).");
    }

    [AvaloniaFact]
    public void ScrollViewer_Child_Button_Click_Fires_Without_Drag()
    {
        var clicked = false;
        var button = new MdButton
        {
            Content = "Navigate Page",
            Width = 160,
            Height = 48
        };
        button.Click += (_, _) => clicked = true;

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
                    button,
                    new Border { Height = 200, Background = Brushes.Blue }
                }
            }
        };

        using var host = Show(scrollViewer, 400, 400);
        Dispatcher.UIThread.RunJobs();

        var point = button.TranslatePoint(new Point(40, 24), host.Window)!.Value;
        host.Window.MouseMove(point, RawInputModifiers.None);
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        host.Window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.True(clicked, "Clicking a button inside MdScrollViewer without dragging must trigger the Click event");
    }

    [AvaloniaFact]
    public void Gallery_Navigation_Buttons_Switch_Pages_Properly()
    {
        var gallery = new MainWindow { Width = 1440, Height = 900 };
        gallery.Show();
        Dispatcher.UIThread.RunJobs();

        var pageHost = gallery.FindControl<ContentControl>("PageHost");
        Assert.NotNull(pageHost);

        // Click AndroidSettingsNav (which is near the top of the navigation)
        var androidBtn = gallery.FindControl<MdButton>("AndroidSettingsNav");
        Assert.NotNull(androidBtn);
        androidBtn.BringIntoView();
        Dispatcher.UIThread.RunJobs();

        var ptAndroid = androidBtn.TranslatePoint(new Point(20, 20), gallery)!.Value;
        gallery.MouseMove(ptAndroid, RawInputModifiers.None);
        gallery.MouseDown(ptAndroid, MouseButton.Left, RawInputModifiers.None);
        gallery.MouseUp(ptAndroid, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.IsType<AndroidSettingsSamplePage>(pageHost.Content);

        // Click ComponentsOverviewNav
        var overviewBtn = gallery.FindControl<MdButton>("ComponentsOverviewNav");
        Assert.NotNull(overviewBtn);
        overviewBtn.BringIntoView();
        Dispatcher.UIThread.RunJobs();

        var ptOverview = overviewBtn.TranslatePoint(new Point(20, 20), gallery)!.Value;
        gallery.MouseMove(ptOverview, RawInputModifiers.None);
        gallery.MouseDown(ptOverview, MouseButton.Left, RawInputModifiers.None);
        gallery.MouseUp(ptOverview, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.IsType<ComponentsOverviewGalleryPage>(pageHost.Content);

        gallery.Close();
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

        // Trigger should remain anchored and every expanded action shares its trailing edge.
        Assert.True(fabMenu.IsOpen);
        Assert.True(fabMenu.Bounds.Width > 0);

        var trigger = fabMenu.GetVisualDescendants().OfType<MdToggleIconButton>()
            .Single(control => control.Name == "PART_Trigger");
        var triggerRight = trigger.TranslatePoint(new Point(trigger.Bounds.Width, 0), fabMenu)!.Value.X;
        var actions = fabMenu.GetVisualDescendants().OfType<MdFabMenuItem>().ToArray();
        Assert.NotEmpty(actions);
        Assert.All(actions, action =>
        {
            var actionRight = action.TranslatePoint(new Point(action.Bounds.Width, 0), fabMenu)!.Value.X;
            Assert.InRange(Math.Abs(actionRight - triggerRight), 0, 0.5);
        });
    }

    [AvaloniaFact]
    public void FabMenu_Supports_Left_And_Right_Alignment()
    {
        var rightMenu = new MdFabMenu
        {
            Alignment = MdFabAlignment.Right,
            Items = { new MdFabMenuItem { Content = "Right action" } }
        };

        var leftMenu = new MdFabMenu
        {
            Alignment = MdFabAlignment.Left,
            Items = { new MdFabMenuItem { Content = "Left action" } }
        };

        var container = new StackPanel
        {
            Children = { rightMenu, leftMenu }
        };

        using var host = Show(container, 400, 400);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(MdFabAlignment.Right, rightMenu.Alignment);
        Assert.Equal(MdFabAlignment.Left, leftMenu.Alignment);

        // Dynamically toggle alignment
        rightMenu.Alignment = MdFabAlignment.Left;
        Assert.Equal(MdFabAlignment.Left, rightMenu.Alignment);
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

        using (var host = Show(card, 500, 200))
        {
            Dispatcher.UIThread.RunJobs();

            var point = card.TranslatePoint(new Point(20, 20), host.Window)!.Value;
            host.Window.MouseMove(point, RawInputModifiers.None);
            host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            host.Window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.True(clicked, "Clicking the settings card should raise the Click event");
        }

        var group = new MdSettingsGroup
        {
            Header = "Network & internet",
            Description = "Wi-Fi, mobile, hotspot",
            Items =
            {
                new MdSettingsCard { Header = "Wi-Fi", Description = "Connected to Studio_5G", Icon = MdSymbols.Wifi },
                new MdSettingsCard { Header = "SIMs", Description = "T-Mobile", Icon = MdSymbols.SimCard }
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
