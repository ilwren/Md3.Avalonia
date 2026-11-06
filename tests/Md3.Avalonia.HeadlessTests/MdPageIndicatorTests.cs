using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Md3.Avalonia.Extra.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdPageIndicatorTests
{
    [AvaloniaFact]
    public void Position_And_Dimensions_Are_Coerced_For_Bindings_Too()
    {
        var indicator = new MdPageIndicator { PageCount = 5, PagePosition = 2.5 };
        Assert.Equal(2.5, indicator.PagePosition);
        indicator.SetValue(MdPageIndicator.PagePositionProperty, 50);
        Assert.Equal(4, indicator.PagePosition);
        indicator.SetValue(MdPageIndicator.PagePositionProperty, double.NaN);
        Assert.Equal(0, indicator.PagePosition);
        indicator.PagePosition = 3;
        indicator.PageCount = 2;
        Assert.Equal(1, indicator.PagePosition);
        indicator.PageCount = 0;
        Assert.Equal(0, indicator.PagePosition);
        indicator.DotSize = double.PositiveInfinity;
        indicator.DotSpacing = -2;
        Assert.Equal(8, indicator.DotSize);
        Assert.Equal(0, indicator.DotSpacing);
    }

    [AvaloniaFact]
    public void Pointer_Requests_The_Painted_Page_In_Ltr_And_Rtl()
    {
        var indicator = new MdPageIndicator { PageCount = 4, Width = 128, Height = 48,
            HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        var window = new Window { Width = 300, Height = 150, Content = indicator };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var requested = -1;
            indicator.PageRequested += (_, index) => requested = index;
            Click(window, indicator, new Point(16, 24));
            Assert.Equal(0, requested);
            Click(window, indicator, new Point(112, 24));
            Assert.Equal(3, requested);
            Assert.Equal(3, indicator.PagePosition);
            indicator.FlowDirection = FlowDirection.RightToLeft;
            Click(window, indicator, new Point(16, 24));
            Assert.Equal(3, requested);
            Click(window, indicator, new Point(112, 24));
            Assert.Equal(0, requested);
            indicator.IsInteractive = false;
            Dispatcher.UIThread.RunJobs();
            Assert.False(indicator.Focusable);
            Click(window, indicator, new Point(16, 24));
            Assert.Equal(0, indicator.PagePosition);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Keyboard_And_Automation_Use_Page_Indices()
    {
        var indicator = new MdPageIndicator { PageCount = 4, Orientation = Orientation.Vertical };
        var window = new Window { Width = 200, Height = 250, Content = indicator };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var last = -1;
            indicator.PageRequested += (_, index) => last = index;
            Assert.True(indicator.Focus());
            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            Assert.Equal(1, last);
            Assert.Equal(1, indicator.PagePosition);
            window.KeyPress(Key.End, RawInputModifiers.None, PhysicalKey.End, null);
            Assert.Equal(3, indicator.PagePosition);
            var peer = ControlAutomationPeer.CreatePeerForElement(indicator);
            var range = Assert.IsAssignableFrom<IRangeValueProvider>(peer.GetProvider<IRangeValueProvider>());
            Assert.Equal(3, range.Maximum);
            range.SetValue(1.6);
            Assert.Equal(2, last);
            Assert.Equal(2, indicator.PagePosition);
            indicator.IsInteractive = false;
            Assert.True(range.IsReadOnly);
            range.SetValue(0);
            Assert.Equal(2, indicator.PagePosition);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Fractional_Worm_And_Slide_Paint_Different_Markers()
    {
        var indicator = new MdPageIndicator { PageCount = 3, PagePosition = 0.5, Effect = MdPageIndicatorEffect.Worm,
            Width = 128, Height = 48, ActiveBrush = Brushes.Red, InactiveBrush = Brushes.Blue };
        var window = new Window { Width = 200, Height = 200, Content = indicator };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var wormFirst = Pixel(window, indicator, new Point(32, 24));
            var wormMiddle = Pixel(window, indicator, new Point(48, 24));
            var inactive = Pixel(window, indicator, new Point(96, 24));
            Assert.NotEqual(inactive, wormFirst);
            Assert.Equal(wormFirst, wormMiddle);
            indicator.Effect = MdPageIndicatorEffect.Slide;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(inactive, Pixel(window, indicator, new Point(32, 24)));
            Assert.NotEqual(inactive, Pixel(window, indicator, new Point(48, 24)));
        }
        finally { window.Close(); }
    }

    private static uint Pixel(Window window, Control control, Point local)
    {
        var origin = control.TranslatePoint(local, window)!.Value;
        var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        var size = frame!.PixelSize;
        var pixels = new uint[size.Width * size.Height];
        var pinned = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try { frame.CopyPixels(new PixelRect(0, 0, size.Width, size.Height), pinned.AddrOfPinnedObject(), pixels.Length * 4, size.Width * 4); }
        finally { pinned.Free(); }
        return pixels[(int)origin.Y * size.Width + (int)origin.X];
    }

    private static void Click(Window window, Control target, Point local)
    {
        var point = target.TranslatePoint(local, window)!.Value;
        window.MouseMove(point, RawInputModifiers.None);
        window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
    }
}
