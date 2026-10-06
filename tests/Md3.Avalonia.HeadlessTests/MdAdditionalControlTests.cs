using Avalonia.Controls;
using Avalonia.Controls.Presenters;
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
    public void Popup_Templates_Allow_Avalonia_To_Select_Native_Or_Overlay_Hosts()
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
                Assert.False(popup.ShouldUseOverlayLayer);
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
    public void Tabs_View_Shows_The_Page_For_The_Selected_Tab()
    {
        var tabs = new MdTabs { Width = 640, SelectedIndex = 0 };
        tabs.Items.Add(new MdTabItem { Content = "Overview" });
        tabs.Items.Add(new MdTabItem { Content = "Activity" });

        var overview = new Border { Child = new TextBlock { Text = "Overview page" } };
        var activity = new Border { Child = new TextBlock { Text = "Activity page" } };
        var view = new MdTabsView { Tabs = tabs, Height = 120 };
        view.Items.Add(overview);
        view.Items.Add(activity);

        var window = new Window { Width = 700, Height = 300, Content = new StackPanel { Children = { tabs, view } } };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var host = view.GetVisualDescendants().OfType<ContentPresenter>().Single(part => part.Name == "PART_SelectedContentHost");
        Assert.Equal(0, view.SelectedIndex);
        Assert.Same(overview, host.Content);

        // Choosing a tab pages the content.
        tabs.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, view.SelectedIndex);
        Assert.Same(activity, host.Content);

        // And paging the content moves the bar, so neither can be left behind the other.
        view.SelectedIndex = 0;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(0, tabs.SelectedIndex);

        window.Close();
    }

    [AvaloniaFact]
    public void Tabs_View_Keeps_Its_Page_When_The_Bar_Has_More_Tabs_Than_Pages()
    {
        var tabs = new MdTabs { Width = 640, SelectedIndex = 0 };
        tabs.Items.Add(new MdTabItem { Content = "One" });
        tabs.Items.Add(new MdTabItem { Content = "Two" });
        tabs.Items.Add(new MdTabItem { Content = "Three" });

        var view = new MdTabsView { Tabs = tabs, Height = 120 };
        view.Items.Add(new Border { Child = new TextBlock { Text = "One" } });
        view.Items.Add(new Border { Child = new TextBlock { Text = "Two" } });

        var window = new Window { Width = 700, Height = 300, Content = new StackPanel { Children = { tabs, view } } };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        tabs.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, view.SelectedIndex);

        tabs.SelectedIndex = 2;
        Dispatcher.UIThread.RunJobs();

        // There is no third page; blanking the view would be worse than leaving the last one up.
        Assert.Equal(1, view.SelectedIndex);
        Assert.NotNull(view.SelectedItem);

        window.Close();
    }

    [AvaloniaFact]
    public void Tabs_View_Draws_No_Second_Tab_Strip()
    {
        var tabs = new MdTabs { Width = 640, SelectedIndex = 0 };
        tabs.Items.Add(new MdTabItem { Content = "Overview" });

        var view = new MdTabsView { Tabs = tabs, Height = 120 };
        view.Items.Add(new Border());

        var window = new Window { Width = 700, Height = 300, Content = new StackPanel { Children = { tabs, view } } };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        // The bar is the strip; a TabControl template would otherwise bring its own.
        Assert.DoesNotContain(view.GetVisualDescendants().OfType<ItemsPresenter>(), _ => true);

        window.Close();
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
