using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdPhaseTwoToFourCompletionTests
{
    [AvaloniaFact]
    public void Scoped_Material_ScrollViewer_Has_Extent_And_Handles_Wheel_Input()
    {
        var content = new Border { Width = 260, Height = 1000 };
        var scroll = new MdScrollViewer
        {
            Width = 300,
            Height = 220,
            Content = content,
            VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        var window = new Window { Width = 360, Height = 280, Content = scroll };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.True(scroll.Extent.Height > scroll.Viewport.Height);
            var before = scroll.Offset.Y;
            window.MouseWheel(new Point(120, 120), new Vector(0, -1), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.True(scroll.Offset.Y > before);
            Assert.Contains(scroll.GetVisualDescendants(), control => control is MdScrollBar);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Desktop_Input_Adapters_Preserve_Native_Value_And_Filter_APIs()
    {
        var autocomplete = new MdAutoCompleteBox
        {
            Label = "Framework",
            ItemsSource = new[] { "Avalonia", "Android", "Flutter" },
            MinimumPrefixLength = 1,
            IsTextCompletionEnabled = true
        };
        var numeric = new MdNumericBox
        {
            Label = "Quantity",
            Minimum = 0,
            Maximum = 20,
            Increment = 2,
            Value = 8
        };
        using var host = Show(new StackPanel { Children = { autocomplete, numeric } });

        Assert.Equal(8, numeric.Value);
        Assert.NotNull(autocomplete.Template);
        Assert.NotNull(numeric.Template);
        Assert.Contains(autocomplete.GetVisualDescendants(), control => control is MdTextBox);
        Assert.Contains(numeric.GetVisualDescendants(), control => control is MdTextBox);
    }

    [AvaloniaFact]
    public void Official_Chip_Types_Map_To_Shared_Selection_And_Remove_Model()
    {
        var assist = new MdAssistChip();
        var filter = new MdFilterChip { IsChecked = true };
        var input = new MdInputChip();
        var suggestion = new MdSuggestionChip();
        using var host = Show(new StackPanel { Children = { assist, filter, input, suggestion } });

        Assert.Equal(MdChipVariant.Assist, assist.Variant);
        Assert.Equal(MdChipVariant.Filter, filter.Variant);
        Assert.True(filter.IsChecked);
        Assert.Equal(MdChipVariant.Input, input.Variant);
        Assert.True(input.IsRemovable);
        Assert.Equal(MdChipVariant.Suggestion, suggestion.Variant);
        Assert.All(new MdChip[] { assist, filter, input, suggestion }, chip => Assert.NotNull(chip.Template));
    }

    [AvaloniaFact]
    public void Modal_And_Transient_Hosts_Expose_Consistent_Direct_APIs()
    {
        var drawer = new MdNavigationDrawer { IsModal = true, IsOpen = false };
        drawer.Show();
        Assert.True(drawer.IsOpen);
        drawer.Dismiss();
        Assert.False(drawer.IsOpen);

        var search = new MdSearchView();
        search.Show();
        Assert.True(search.IsOpen);
        search.Dismiss();
        Assert.False(search.IsOpen);

        var sheet = new MdSheetHost();
        sheet.Show();
        Assert.True(sheet.IsOpen);
        sheet.Dismiss();
        Assert.False(sheet.IsOpen);

        var fabMenu = new MdFabMenu();
        fabMenu.Show();
        Assert.True(fabMenu.IsOpen);
        fabMenu.Dismiss();
        Assert.False(fabMenu.IsOpen);
    }

    [AvaloniaFact]
    public void Flexible_Navigation_Bar_Propagates_Horizontal_Item_Layout()
    {
        var first = new MdNavigationBarItem { Label = "Home" };
        var second = new MdNavigationBarItem { Label = "Search" };
        var navigation = new MdNavigationBar
        {
            ItemLayout = MdNavigationBarLayout.Horizontal,
            Items = { first, second }
        };
        navigation.SelectedIndex = 0;
        using var host = Show(navigation);

        Assert.Equal(MdNavigationBarVariant.Flexible, navigation.Variant);
        Assert.Equal(64, navigation.Height);
        Assert.Equal(MdNavigationBarLayout.Horizontal, first.Layout);
        Assert.Equal(MdNavigationBarLayout.Horizontal, second.Layout);
        Assert.Same(first, navigation.SelectedItem);
    }

    [AvaloniaFact]
    public void Carousel_And_Fab_Menu_Offer_Explicit_Item_Types()
    {
        var carousel = new MdCarousel { Width = 420, Height = 160, ItemWidth = 180, ItemHeight = 140 };
        carousel.Items.Add(new MdCarouselItem { Content = "One" });
        carousel.Items.Add(new MdCarouselItem { Content = "Two" });
        carousel.SelectedIndex = 1;
        var menu = new MdFabMenu { Items = { new MdFabMenuItem { Content = "Photo" }, new MdFabMenuItem { Content = "Link" } } };
        using var host = Show(new StackPanel { Children = { carousel, menu } });

        Assert.IsType<MdCarouselItem>(carousel.SelectedItem);
        Assert.All(menu.Items.Cast<object>(), item => Assert.IsType<MdFabMenuItem>(item));
        Assert.All(carousel.GetVisualDescendants().OfType<MdCarouselItem>(), item => Assert.NotNull(item.Theme));
    }

    [AvaloniaFact]
    public void Adaptive_Layout_Tracks_Compact_Medium_And_Expanded_Breakpoints()
    {
        var adaptive = new MdAdaptiveLayout
        {
            Width = 400,
            Height = 120,
            MediumBreakpoint = 600,
            ExpandedBreakpoint = 840,
            CompactContent = new TextBlock { Text = "Compact" },
            MediumContent = new TextBlock { Text = "Medium" },
            ExpandedContent = new TextBlock { Text = "Expanded" }
        };
        using var host = Show(adaptive);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(MdAdaptiveLayoutMode.Compact, adaptive.Mode);

        adaptive.Width = 700;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(MdAdaptiveLayoutMode.Medium, adaptive.Mode);

        adaptive.Width = 900;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(MdAdaptiveLayoutMode.Expanded, adaptive.Mode);
    }

    [AvaloniaFact]
    public void Loading_Indicator_Supports_Contained_And_Uncontained_Forms()
    {
        var contained = new MdLoadingIndicator { IsContained = true, IsActive = false };
        var uncontained = new MdLoadingIndicator { IsContained = false, IsActive = false };
        using var host = Show(new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal, Children = { contained, uncontained } });

        Assert.True(contained.IsContained);
        Assert.NotNull(contained.ContainerBrush);
        Assert.NotNull(contained.IndicatorBrush);
        Assert.False(uncontained.IsContained);
    }

    [AvaloniaFact]
    public void Phase_Two_Three_And_Four_Control_Matrix_Renders_Together()
    {
        var navigation = new MdNavigationBar { Items = { new MdNavigationBarItem { Label = "Home" }, new MdNavigationBarItem { Label = "Search" } }, SelectedIndex = 0 };
        var content = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 12,
            Children =
            {
                new WrapPanel { Children = { new MdButton { Content = "Button" }, new MdIconButton { Icon = MdSymbols.Search }, new MdCheckBox { Content = "Check" }, new MdRadioButton { Content = "Radio" }, new MdSwitch { Content = "Switch" } } },
                new MdTextBox { Label = "Text field", Text = "Value" },
                new MdSlider { Value = 40 },
                new WrapPanel { Children = { new MdCard { Content = "Card" }, new MdBadge { Content = "3", Variant = MdBadgeVariant.Label }, new MdAssistChip { Content = "Assist" }, new MdTooltip { Content = "Tooltip", IsOpen = true } } },
                new MdList { Height = 90, Items = { new MdListItem { Headline = "List item" } } },
                navigation,
                new MdToolbar { Items = { new MdIconButton { Icon = MdSymbols.Edit } } },
                new MdDatePickerDialog(),
                new MdTimePickerDialog()
            }
        };
        var window = new Window { Width = 900, Height = 720, Content = content };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(window.CaptureRenderedFrame());
            Assert.All(content.Children.OfType<Control>(), control => Assert.True(control.Bounds.Width >= 0));
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Material_Window_Uses_Scoped_Surface_Theme()
    {
        var window = new MdWindow { Width = 420, Height = 240, Content = new MdText { Text = "Material window" } };
        window.Show();
        try
        {
            Assert.NotNull(window.Background);
            Assert.NotNull(window.Foreground);
            Assert.NotNull(window.CaptureRenderedFrame());
        }
        finally { window.Close(); }
    }

    private static IDisposable Show(Control control)
    {
        var window = new Window { Width = 1000, Height = 600, Content = control };
        window.Show();
        return new Scope(window);
    }

    private sealed class Scope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }
}
