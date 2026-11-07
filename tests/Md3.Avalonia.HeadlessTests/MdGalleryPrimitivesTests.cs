using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
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
