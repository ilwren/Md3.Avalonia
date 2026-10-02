using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdAdditionalControlTests
{
    [AvaloniaFact]
    public void New_Component_Families_Render_Together()
    {
        var menu = new MdMenu();
        menu.Items.Add(new MdMenuItem { Content = "Settings" });
        var navigation = new MdNavigationBar { SelectedIndex = 0 };
        navigation.Items.Add(new MdNavigationBarItem { Label = "Home", Icon = new MdIcon { Glyph = MdSymbols.Home } });
        navigation.Items.Add(new MdNavigationBarItem { Label = "Search", Icon = new MdIcon { Glyph = MdSymbols.Search } });

        var drawer = new MdNavigationDrawer
        {
            Width = 640,
            Height = 220,
            IsOpen = true,
            DrawerWidth = 220,
            DrawerContent = new TextBlock { Text = "Drawer" },
            Content = new TextBlock { Text = "Page" }
        };
        var sheet = new MdSheetHost
        {
            Width = 640,
            Height = 260,
            IsOpen = true,
            SheetExtent = 140,
            SheetContent = new TextBlock { Text = "Sheet" },
            Content = new TextBlock { Text = "Page" }
        };
        var snackbar = new MdSnackbar { Content = "Saved", IsOpen = true, Duration = TimeSpan.Zero };
        var panel = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                menu,
                navigation,
                new MdSearchBar { Text = "Material", PlaceholderText = "Search" },
                new MdSlider { Width = 420, Value = 64, ShowValueIndicator = true },
                new MdSwitch { Content = "Wi-Fi", IsChecked = true, ShowIcons = true },
                snackbar,
                drawer,
                sheet
            }
        };
        var window = new Window { Width = 900, Height = 900, Content = new ScrollViewer { Content = panel } };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(window.CaptureRenderedFrame());
            Assert.Equal("64", ((MdSlider)panel.Children[3]).DisplayValue);
            Assert.True(((MdSwitch)panel.Children[4]).IsChecked);
            snackbar.Dismiss();
            Assert.False(snackbar.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Popup_Templates_Use_Overlay_Only_For_Platform_Safe_Material_Surfaces()
    {
        Control[] owners =
        {
            new MdComboBox { ItemsSource = new[] { "One", "Two" } },
            new MdDatePicker(),
            new MdTimePicker(),
            new MdDropdownMenu { Content = new MdMenu() },
            new MdTooltipHost { Tooltip = new MdTooltip { Content = "Help" }, Content = new Button { Content = "Trigger" } },
            new MdMenuAnchor { Menu = new MdMenu(), Content = new Button { Content = "Menu" } }
        };
        var panel = new StackPanel();
        foreach (var owner in owners) panel.Children.Add(owner);
        var window = new Window { Width = 640, Height = 500, Content = panel };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            foreach (var owner in owners)
            {
                var popup = Assert.IsAssignableFrom<TemplatedControl>(owner)
                    .GetTemplateDescendants().OfType<Popup>().Single();
                var expectsOverlay = owner is MdComboBox or MdDatePicker or MdTimePicker or MdMenuAnchor;
                Assert.Equal(expectsOverlay, popup.ShouldUseOverlayLayer);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Gallery_Uses_One_Finite_Shell_Scroller_And_Unwraps_Page_Local_Scrolling()
    {
        var gallery = new MainWindow { Width = 1280, Height = 800 };
        gallery.Show();
        try
        {
            var pageHost = gallery.GetVisualDescendants().OfType<ContentControl>()
                .Single(control => control.Name == "PageHost");
            var shellScroll = gallery.GetVisualDescendants().OfType<ScrollViewer>()
                .Single(control => control.Name == "PageScroll");
            Assert.Contains(shellScroll, pageHost.GetVisualAncestors().OfType<ScrollViewer>());
            var page = Assert.IsAssignableFrom<UserControl>(pageHost.Content);
            Assert.IsNotType<ScrollViewer>(page.Content);
        }
        finally
        {
            gallery.Close();
        }
    }

    [AvaloniaFact]
    public void Search_Snackbar_Sheet_And_Drawer_APIs_Are_Directly_Controllable()
    {
        var search = new MdSearchBar { Text = "query" };
        search.Clear();
        Assert.Equal(string.Empty, search.Text);

        var snackbar = new MdSnackbar { Duration = TimeSpan.Zero };
        snackbar.Show();
        Assert.True(snackbar.IsOpen);
        snackbar.Dismiss();
        Assert.False(snackbar.IsOpen);

        var sheet = new MdSheetHost();
        sheet.Show();
        Assert.True(sheet.IsOpen);
        sheet.Dismiss();
        Assert.False(sheet.IsOpen);

        var drawer = new MdNavigationDrawer { IsOpen = false };
        drawer.IsOpen = true;
        Assert.True(drawer.IsOpen);
    }

    [AvaloniaFact]
    public void Tabs_Toolbar_And_Tooltip_Render_Together()
    {
        var tabs = new MdTabs { Width = 640, SelectedIndex = 0 };
        tabs.Items.Add(new MdTabItem { Content = "Overview", Icon = new MdIcon { Glyph = MdSymbols.Home } });
        tabs.Items.Add(new MdTabItem { Content = "Activity", Icon = new MdIcon { Glyph = MdSymbols.Schedule } });
        tabs.SelectedIndex = 0;

        var toolbar = new MdToolbar { Mode = MdToolbarMode.Floating, Variant = MdToolbarVariant.Standard };
        toolbar.Items.Add(new MdIconButton { Icon = MdSymbols.Edit });
        toolbar.Items.Add(new MdIconButton { Icon = MdSymbols.Share });

        var tooltip = new MdTooltip
        {
            Variant = MdTooltipVariant.Rich,
            Title = "Keyboard shortcuts",
            Content = "Use Command K to search.",
            IsOpen = true
        };

        var window = new Window
        {
            Width = 760,
            Height = 420,
            Content = new StackPanel { Spacing = 24, Children = { tabs, toolbar, tooltip } }
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(window.CaptureRenderedFrame());
            Assert.Equal(0, tabs.SelectedIndex);
            Assert.Equal(MdToolbarMode.Floating, toolbar.Mode);
            Assert.True(tooltip.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Tabs_And_Tooltips_Expose_Mvvm_And_Direct_Control_State()
    {
        var tabs = new MdTabs { Variant = MdTabVariant.Secondary, IsScrollable = true };
        tabs.Items.Add(new MdTabItem { Content = "Overview" });
        tabs.Items.Add(new MdTabItem { Content = "Details" });
        tabs.SelectedIndex = 1;
        Assert.Equal("Details", ((MdTabItem)tabs.SelectedItem!).Content);

        var tooltip = new MdTooltip { Content = "Help" };
        var host = new MdTooltipHost { Tooltip = tooltip, Content = new Button { Content = "Trigger" } };
        host.Show();
        Assert.True(host.IsOpen);
        Assert.True(tooltip.IsOpen);
        host.Dismiss();
        Assert.False(host.IsOpen);
        Assert.False(tooltip.IsOpen);
    }
}
