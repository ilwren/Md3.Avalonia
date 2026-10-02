using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery.Components;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdIssueAndFlutterParityTests
{
    [AvaloniaFact]
    public void ComboBox_Options_Use_Comfortable_SixtyFour_Dip_Rows()
    {
        var combo = new MdComboBox();
        using var comboHost = Show(combo);
        var theme = Assert.IsType<ControlTheme>(ResourceNodeExtensions.FindResource(combo, "MdComboBoxItemTheme"));
        var item = new ComboBoxItem { Theme = theme, Content = "Roomy option", Width = 280 };
        using var itemHost = Show(item, 360, 160);
        Dispatcher.UIThread.RunJobs();
        Assert.True(item.Bounds.Height >= 64, $"Actual option height: {item.Bounds.Height}");
    }

    [AvaloniaFact]
    public void DatePicker_Popup_Surface_Has_An_Explicit_Owner_And_Populated_Calendar()
    {
        var picker = new DatePickerProbe
        {
            DisplayDate = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero),
            SelectedDate = new DateTimeOffset(2026, 9, 23, 0, 0, 0, TimeSpan.Zero)
        };
        using var host = Show(picker);
        Dispatcher.UIThread.RunJobs();
        Assert.NotNull(picker.Surface);
        Assert.NotNull(picker.DaysHost);
        Assert.Same(picker, picker.Surface!.DataContext);
        Assert.Same(picker.CalendarDays, picker.DaysHost!.ItemsSource);
        Assert.Equal(42, picker.CalendarDays.Count);
        Assert.All(picker.CalendarDays, day => Assert.False(string.IsNullOrWhiteSpace(day.DayText)));
    }

    [AvaloniaFact]
    public void Slider_Droplet_Is_Reserved_Inside_The_Control_Bounds()
    {
        var slider = new MdSlider { Width = 680, Value = 68, ShowValueIndicator = true };
        var window = new Window { Width = 760, Height = 240, Content = slider };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var droplet = slider.GetVisualDescendants().OfType<Canvas>()
                .Single(canvas => canvas.Name == "PART_ThumbValue");
            var top = droplet.TranslatePoint(default, window)?.Y;
            Assert.NotNull(top);
            Assert.True(top >= 0, $"Droplet started at y={top}");
            Assert.True(slider.Bounds.Height >= 112);
            Assert.NotNull(window.CaptureRenderedFrame());
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Segmented_State_Layer_Covers_The_Whole_Padded_Segment()
    {
        var segment = new MdSegmentedButton { Content = "Selected", Padding = new Thickness(20, 8), IsSelected = true };
        using var host = Show(segment, 260, 120);
        Dispatcher.UIThread.RunJobs();
        var root = segment.GetVisualDescendants().OfType<Border>().Single(border => border.Name == "PART_Root");
        var state = segment.GetVisualDescendants().OfType<Border>().Single(border => border.Name == "PART_State");
        Assert.Equal(root.Bounds.Width - root.BorderThickness.Left - root.BorderThickness.Right, state.Bounds.Width);
        Assert.Equal(root.Bounds.Height - root.BorderThickness.Top - root.BorderThickness.Bottom, state.Bounds.Height);
        Assert.True(state.Bounds.Width > 200, "The state layer must include the former content padding.");
    }

    [AvaloniaFact]
    public void BottomSheet_Drag_Bypasses_Transitions_And_Tracks_The_Pointer_Directly()
    {
        var sheet = new MdSheetHost
        {
            Width = 700,
            Height = 500,
            IsOpen = true,
            SheetExtent = 260,
            SheetContent = new TextBlock { Text = "Sheet" },
            Content = new Border()
        };
        var window = new Window { Width = 760, Height = 560, Content = sheet };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var handle = sheet.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "PART_DragHandle");
            var surface = sheet.GetVisualDescendants().OfType<Border>().Single(border => border.Name == "PART_Surface");
            var point = handle.TranslatePoint(new Point(handle.Bounds.Width / 2, handle.Bounds.Height / 2), window)!.Value;
            window.MouseMove(point, RawInputModifiers.None);
            window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            window.MouseMove(point.WithY(point.Y + 64), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.Null(surface.Transitions);
            var transform = Assert.IsType<TranslateTransform>(surface.RenderTransform);
            Assert.True(transform.Y >= 60);
            window.MouseUp(point.WithY(point.Y + 64), MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(surface.Transitions);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void AvaloniaEdit_Uses_Dynamic_Readable_Foreground_In_Dark_Mode()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
        try
        {
            var example = new CodeExample { XamlCode = "<md:MdButton Content=\"Readable\" />", CSharpCode = "var readable = true;" };
            using var host = Show(example, 760, 360);
            Dispatcher.UIThread.RunJobs();
            var editors = example.GetVisualDescendants().OfType<TextEditor>().ToArray();
            Assert.Equal(2, editors.Length);
            Assert.All(editors, editor =>
            {
                Assert.NotNull(editor.SyntaxHighlighting);
                Assert.StartsWith("Material dark", editor.SyntaxHighlighting!.Name);
                Assert.NotNull(editor.Foreground);
            });
        }
        finally { Application.Current.RequestedThemeVariant = ThemeVariant.Light; }
    }

    [AvaloniaFact]
    public void Flutter_Parity_APIs_Are_Functional_And_Renderable()
    {
        var firstPanel = new MdExpansionPanel { Header = "One", Content = "First", IsExpanded = true };
        var secondPanel = new MdExpansionPanel { Header = "Two", Content = "Second" };
        var panels = new MdExpansionPanelList { AllowMultiple = false, Items = { firstPanel, secondPanel } };
        var table = new MdDataTable { Header = new TextBlock { Text = "Header" }, Items = { new MdDataTableRow { Content = "Row" } } };
        var stepper = new MdStepper { Items = { new MdStep { Header = "One" }, new MdStep { Header = "Two" } } };
        var refresh = new MdRefreshIndicator { Content = new TextBlock { Text = "Pull" } };
        var banner = new MdBanner { Content = "Banner", IsDismissible = true };
        var root = new StackPanel { Width = 720, Spacing = 12, Children = { banner, panels, table, stepper, refresh } };
        using var host = Show(root, 800, 1600);
        Dispatcher.UIThread.RunJobs();

        secondPanel.Expand();
        Assert.False(firstPanel.IsExpanded);
        Assert.True(secondPanel.IsExpanded);

        MdDataTableSortEventArgs? sort = null;
        table.SortRequested += (_, args) => sort = args;
        table.ToggleSort("Header");
        Assert.Equal(MdDataTableSortDirection.Ascending, sort?.Direction);

        stepper.Next();
        Assert.Equal(1, stepper.ActiveStep);

        var refreshed = false;
        refresh.RefreshRequested += (_, _) => refreshed = true;
        refresh.BeginRefresh();
        Assert.True(refreshed);
        Assert.True(refresh.IsRefreshing);
        refresh.CompleteRefresh();
        Assert.False(refresh.IsRefreshing);

        banner.Dismiss();
        Assert.False(banner.IsOpen);
        Assert.NotNull(((Window)((Scope)host).Window).CaptureRenderedFrame());
    }

    private sealed class DatePickerProbe : MdDatePicker
    {
        protected override Type StyleKeyOverride => typeof(MdDatePicker);
        public Border? Surface { get; private set; }
        public ItemsControl? DaysHost { get; private set; }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            Surface = e.NameScope.Find<Border>("PART_Surface");
            DaysHost = e.NameScope.Find<ItemsControl>("PART_DaysHost");
        }
    }

    private static IDisposable Show(Control content, double width = 1000, double height = 720)
    {
        var window = new Window { Width = width, Height = height, Content = content };
        window.Show();
        return new Scope(window);
    }

    private sealed class Scope(Window window) : IDisposable
    {
        public Window Window { get; } = window;
        public void Dispose() => Window.Close();
    }
}

internal static class PointTestExtensions
{
    public static Point WithY(this Point point, double y) => new(point.X, y);
}
