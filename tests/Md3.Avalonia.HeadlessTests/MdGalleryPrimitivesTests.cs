using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Pages;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdGalleryPrimitivesTests
{
    [AvaloniaFact]
    public void Motion_Primitives_Respond_To_Their_Sliders()
    {
        var page = new MotionGalleryPage();
        var window = new Window { Width = 900, Height = 900, Content = page };
        window.Show();
        try
        {
            var reveal = page.FindControl<MdRevealHost>("RevealDemo")!;
            var morph = page.FindControl<MdMorphPanel>("MorphPanelDemo")!;
            page.FindControl<MdSlider>("RevealFractionSlider")!.Value = 0.25;
            page.FindControl<MdSlider>("MorphProgressSlider")!.Value = 0.75;
            Assert.Equal(0.25, reveal.Fraction);
            Assert.Equal(0.75, morph.Progress);
            Assert.Equal(2, morph.Children.Count);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Window_Example_Uses_Chrome_Parts_And_Routes_Pointer_Requests()
    {
        var page = new BorderlessWindowGalleryPage();
        var window = page.CreateDemoWindow();
        var adapter = new RecordingAdapter();
        window.PlatformAdapter = adapter;
        window.Show();
        try
        {
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            var drag = window.GetLogicalDescendants().OfType<MdWindowDragRegion>().Single();
            var grip = window.GetLogicalDescendants().OfType<MdWindowResizeGrip>().Single();
            Assert.Single(window.GetLogicalDescendants().OfType<MdWindowCaptionButton>());
            Assert.Equal(WindowEdge.SouthEast, grip.Edge);
            Assert.Equal(3, window.GetLogicalDescendants().OfType<MdCaptionButton>().Count());

            // Headless window decorations return the outer frame as the hit target here.
            // Targeted routed events verify the controls' adapter path, not native hit-testing.
            using var pointer = new Pointer(123, PointerType.Mouse, true);
            var properties = new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            var dragPress = new PointerPressedEventArgs(drag, pointer, window, new Point(40, 60), 1, properties, KeyModifiers.None);
            drag.RaiseEvent(dragPress);
            Assert.True(dragPress.Handled);
            Assert.Equal(1, adapter.Moves);
            var gripPress = new PointerPressedEventArgs(grip, pointer, window, new Point(730, 450), 2, properties, KeyModifiers.None);
            grip.RaiseEvent(gripPress);
            Assert.True(gripPress.Handled);
            Assert.Equal(WindowEdge.SouthEast, adapter.LastResize);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Scrollbar_Example_Exposes_Real_Policies_And_Value()
    {
        var page = new ScrollViewerGalleryPage();
        var window = new Window { Width = 900, Height = 900, Content = page };
        window.Show();
        try
        {
            var bar = page.FindControl<MdScrollBar>("StandaloneBar")!;
            Assert.Equal(MdScrollbarThumbVisibility.Always, bar.ThumbVisibility);
            Assert.True(bar.IsTrackVisible);
            bar.Value = 72;
            Assert.Contains("72", page.FindControl<TextBlock>("ScrollbarStatus")!.Text ?? "");
            page.FindControl<MdButton>("HiddenButton")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(MdScrollbarThumbVisibility.Hidden, bar.ThumbVisibility);
            page.FindControl<MdButton>("AutoButton")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(MdScrollbarThumbVisibility.Auto, bar.ThumbVisibility);
            page.FindControl<MdButton>("AlwaysButton")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            page.FindControl<MdButton>("InteractiveButton")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(MdScrollbarThumbVisibility.Always, bar.ThumbVisibility);
            Assert.False(bar.IsInteractive);
            Assert.False(bar.IsHitTestVisible);
            page.FindControl<MdButton>("InteractiveButton")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.True(bar.IsInteractive);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Time_Dial_Example_Switches_Part_And_Reports_Keyboard_Changes()
    {
        var page = new PickerGalleryPage();
        var window = new Window { Width = 900, Height = 900, Content = page };
        window.Show();
        try
        {
            var dial = page.FindControl<MdTimeDial>("StandaloneDial")!;
            Assert.True(dial.Is24Hour);
            page.FindControl<MdButton>("DialMinuteButton")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(MdTimeDialPart.Minute, dial.ActivePart);
            Assert.True(dial.Focus());
            window.KeyPress(Key.Up, RawInputModifiers.None, PhysicalKey.ArrowUp, null);
            Assert.Equal(31, dial.Minute);
            Assert.Contains("14:31", page.FindControl<TextBlock>("StandaloneDialStatus")!.Text ?? "");
            page.FindControl<MdButton>("DialHourButton")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(MdTimeDialPart.Hour, dial.ActivePart);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Paginated_Table_Example_Sorts_In_The_Host_And_Returns_To_First_Page()
    {
        var page = new PaginatedDataTableGalleryPage();
        var window = new Window { Width = 900, Height = 900, Content = page };
        window.Show();
        try
        {
            var table = page.FindControl<MdPaginatedDataTable>("PagedTable")!;
            Assert.Equal(3, table.Columns.Count);
            Assert.True(table.ShowFirstLastButtons);
            table.LastPage();
            Assert.Equal(2, table.PageIndex);
            var headers = table.GetVisualDescendants().OfType<Grid>().Single(x => x.Name == "PART_ColumnHeaders");
            var score = Assert.IsType<Button>(headers.Children[2]);
            score.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(0, table.PageIndex);
            Assert.True(table.SortAscending);
            Assert.Equal(86, Assert.IsType<PaginatedDataTableGalleryPage.PackageRow>(table.PageItems[0]).Score);
            score.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.False(table.SortAscending);
            Assert.Equal(99, Assert.IsType<PaginatedDataTableGalleryPage.PackageRow>(table.PageItems[0]).Score);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Window_Page_Creates_A_Regular_Material_Window()
    {
        var page = new BorderlessWindowGalleryPage();
        var window = page.CreateMaterialWindow();
        Assert.IsType<MdWindow>(window);
        window.Show();
        try { Assert.IsType<TextBlock>(window.Content); }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public async Task Dedicated_Aot_Scenario_Opens_And_Exercises_The_Pages()
    {
        var window = new MainWindow { Width = 1280, Height = 860 };
        window.Show();
        try { await App.RunDedicatedControlsNativeAotSmokeAsync(window); }
        finally { window.Close(); }
    }

    private sealed class RecordingAdapter : IMdWindowPlatformAdapter
    {
        public int Moves { get; private set; }
        public WindowEdge? LastResize { get; private set; }
        public MdWindowPlatform Platform => MdWindowPlatform.Unknown;
        public MdWindowCapabilities Capabilities => MdWindowCapabilities.All;
        public void Apply(MdBorderlessWindow window, MdBorderlessWindowOptions options) { }
        public bool TryBeginMove(MdBorderlessWindow window, PointerPressedEventArgs args) { Moves++; return true; }
        public bool TryBeginResize(MdBorderlessWindow window, WindowEdge edge, PointerPressedEventArgs args) { LastResize = edge; return true; }
        public bool TryShowSystemMenu(MdBorderlessWindow window, Point clientPoint) => false;
    }
}
