using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
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
    public void Window_Example_Uses_Real_Chrome_Parts_And_Pointer_Requests()
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

            // Headless hit-testing needs a rendered/settled frame, just like the other Gallery
            // pointer tests. These send real mouse input, not routed-event shortcuts.
            var diagnostic = string.Join(" | ", drag.GetSelfAndVisualAncestors().OfType<Control>()
                .Select(c => $"{c.GetType().Name}#{c.Name}:bounds={c.Bounds},visible={c.IsVisible},effective={c.IsEffectivelyVisible},hit={c.IsHitTestVisible},clip={c.ClipToBounds}"));
            var point = drag.TranslatePoint(new Point(drag.Bounds.Width / 2, drag.Bounds.Height / 2), window)!.Value;
            var hits = window.GetVisualsAt(point).ToArray();
            Assert.True(hits.Any(v => ReferenceEquals(v, drag) || drag.GetSelfAndVisualDescendants().Contains(v)), $"point={point} hits={string.Join(",", hits.Select(x => x.GetType().Name))}; ancestors={diagnostic}");
            PointerInput.Click(drag);
            Assert.Equal(1, adapter.Moves);
            PointerInput.Click(grip);
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
