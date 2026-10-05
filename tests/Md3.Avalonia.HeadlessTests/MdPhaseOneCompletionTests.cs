using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Pages;
using Md3.Avalonia.Localization;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdPhaseOneCompletionTests
{
    [AvaloniaFact]
    public void Phase_One_Components_Render_Together()
    {
        var segments = new MdSegmentedButtonGroup();
        segments.Items.Add(new MdSegmentedButton { Content = "Day" });
        segments.Items.Add(new MdSegmentedButton { Content = "Week" });
        segments.SelectedIndex = 0;
        var tabs = new MdTabView();
        tabs.Items.Add(new MdTabViewItem { Header = "One", Content = new TextBlock { Text = "Page one" } });
        tabs.Items.Add(new MdTabViewItem { Header = "Two", Content = new TextBlock { Text = "Page two" } });
        tabs.SelectedIndex = 0;
        var rail = new MdNavigationRail { SelectedIndex = 0 };
        rail.Items.Add(new MdNavigationRailItem { Label = "Home", Icon = MdSymbols.Home });
        rail.Items.Add(new MdNavigationRailItem { Label = "Search", Icon = MdSymbols.Search });
        var stack = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                segments,
                new MdRangeSlider { Width = 500, LowerValue = 20, UpperValue = 80 },
                new MdDateRangePicker { Width = 600 },
                tabs,
                new MdMenuAnchor { Content = new MdButton { Content = "Menu" }, Menu = new MdMenu() },
                rail
            }
        };
        var window = new Window { Width = 900, Height = 720, Content = new ScrollViewer { Content = stack } };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(window.CaptureRenderedFrame());
            Assert.Equal(0, segments.SelectedIndex);
            Assert.Equal(20, ((MdRangeSlider)stack.Children[1]).LowerValue);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Range_DateRange_And_Adaptive_APIs_Normalize_State()
    {
        var range = new MdRangeSlider { Minimum = 0, Maximum = 100, LowerValue = 80, UpperValue = 20 };
        Assert.True(range.LowerValue <= range.UpperValue);

        var dates = new MdDateRangePicker
        {
            StartDate = new DateTimeOffset(2026, 10, 10, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero)
        };
        Assert.True(dates.StartDate <= dates.EndDate);
        dates.Clear();
        Assert.Null(dates.StartDate);
        Assert.Null(dates.EndDate);

        var suite = new MdNavigationSuite { Width = 800, Height = 300, Mode = MdNavigationSuiteMode.Auto };
        var window = new Window { Width = 800, Height = 300, Content = suite };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(MdNavigationSuiteMode.NavigationRail, suite.EffectiveMode);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Library_Localization_And_Text_Editing_Menu_Are_Available()
    {
        var panel = new StackPanel();
        MdLocalization.SetCulture(panel, CultureInfo.GetCultureInfo("zh-CN"));
        var date = new MdDatePicker();
        var time = new MdTimePicker();
        var field = new MdTextBox { Text = "Material" };
        var search = new MdSearchBar { Text = "Search" };
        panel.Children.Add(date);
        panel.Children.Add(time);
        panel.Children.Add(field);
        panel.Children.Add(search);
        var window = new Window { Width = 700, Height = 300, Content = panel };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("选择日期", date.DisplayText);
            Assert.Equal("选择时间", time.DisplayText);
            Assert.NotNull(field.ContextMenu);
            Assert.NotNull(search.ContextMenu);
            Assert.True(field.ContextMenu!.Items.Count >= 6);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Long_Gallery_Page_Has_A_Real_Scroll_Extent_And_Accepts_Offset()
    {
        var gallery = new MainWindow { Width = 1440, Height = 620 };
        gallery.Show();
        try
        {
            // Synthetic on purpose: this test is about the scroll extent of the page that
            // opens, not about reaching the rail button that opens it.
            var navigation = gallery.GetVisualDescendants().OfType<MdButton>()
                .Single(button => button.Name == "DataGridNav");
            navigation.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            var pageHost = gallery.GetVisualDescendants().OfType<ContentControl>()
                .Single(control => control.Name == "PageHost");
            // Segmented & range was split per #12; the data-grid page is now the tall one.
            Assert.IsType<DataGridGalleryPage>(pageHost.Content);
            var scroll = gallery.GetVisualDescendants().OfType<ScrollViewer>()
                .Single(viewer => viewer.Name == "PageScroll");
            Assert.True(scroll.Extent.Height > scroll.Viewport.Height,
                $"extent={scroll.Extent.Height}, viewport={scroll.Viewport.Height}, bounds={scroll.Bounds.Height}, page={((Control)pageHost.Content!).Bounds.Height}");
            var beforeWheel = scroll.Offset.Y;
            gallery.MouseWheel(new Point(600, 300), new Vector(0, -1), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.True(scroll.Offset.Y > beforeWheel,
                $"A wheel event over PageHost did not scroll: before={beforeWheel}, after={scroll.Offset.Y}");
        }
        finally { gallery.Close(); }
    }

    [AvaloniaFact]
    public void Gallery_Language_Selector_Updates_Visible_Chrome()
    {
        var gallery = new MainWindow { Width = 1440, Height = 800 };
        gallery.Show();
        try
        {
            var selector = gallery.GetVisualDescendants().OfType<MdComboBox>().Single(x => x.Name == "LanguageSelector");
            selector.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            Assert.Contains(gallery.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == "本页内容");
        }
        finally { gallery.Close(); }
    }
}
