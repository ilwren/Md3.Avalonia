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

public sealed class MdMediaProgressBarTests
{
    [Fact]
    public void Time_Formatting_Handles_Long_Durations_And_Invalid_Values()
    {
        Assert.Equal("0:00", MdMediaProgressBar.FormatTimestamp(double.NaN));
        Assert.Equal("0:00", MdMediaProgressBar.FormatTimestamp(-1));
        Assert.Equal("1:02", MdMediaProgressBar.FormatTimestamp(62.9));
        Assert.Equal("1:01:01", MdMediaProgressBar.FormatTimestamp(3661));
    }

    [AvaloniaFact]
    public void Pointer_Scrub_Reports_Once_On_Release_Not_On_External_Changes()
    {
        var bar = CreateBar();
        var window = Show(bar);
        try
        {
            var count = 0;
            double requested = -1;
            bar.SeekRequested += (_, seconds) => { count++; requested = seconds; };
            bar.Value = 20;
            Assert.Equal(0, count);
            var start = bar.TranslatePoint(new Point(10, 20), window)!.Value;
            var middle = bar.TranslatePoint(new Point(120, 20), window)!.Value;
            window.MouseMove(start, RawInputModifiers.None);
            window.MouseDown(start, MouseButton.Left, RawInputModifiers.None);
            Assert.True(bar.IsScrubbing);
            window.MouseMove(middle, RawInputModifiers.LeftMouseButton);
            Assert.Equal(50, bar.Value, 3);
            Assert.Equal(0, count);
            window.MouseUp(middle, MouseButton.Left, RawInputModifiers.None);
            Assert.False(bar.IsScrubbing);
            Assert.Equal(1, count);
            Assert.Equal(50, requested, 3);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Pointer_Respects_Rtl_And_Empty_Range()
    {
        var bar = CreateBar();
        bar.FlowDirection = FlowDirection.RightToLeft;
        var window = Show(bar);
        try
        {
            var count = 0;
            bar.SeekRequested += (_, _) => count++;
            var left = bar.TranslatePoint(new Point(10, 20), window)!.Value;
            window.MouseMove(left, RawInputModifiers.None);
            window.MouseDown(left, MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(left, MouseButton.Left, RawInputModifiers.None);
            Assert.Equal(100, bar.Value, 3);
            Assert.Equal(1, count);
            bar.Maximum = 0;
            window.MouseDown(left, MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(left, MouseButton.Left, RawInputModifiers.None);
            Assert.False(bar.IsScrubbing);
            Assert.Equal(1, count);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Capture_Loss_Cancels_A_Pending_Seek()
    {
        var bar = CreateBar();
        var window = Show(bar);
        try
        {
            var count = 0;
            bar.SeekRequested += (_, _) => count++;
            var middle = bar.TranslatePoint(new Point(120, 20), window)!.Value;
            window.MouseMove(middle, RawInputModifiers.None);
            window.MouseDown(middle, MouseButton.Left, RawInputModifiers.None);
            Assert.True(bar.IsScrubbing);
            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            Assert.False(bar.IsScrubbing);
            Assert.Equal(0, count);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Keyboard_Automation_ReadOnly_And_Rtl_Use_Seconds()
    {
        var bar = CreateBar();
        var window = Show(bar);
        try
        {
            var count = 0;
            bar.SeekRequested += (_, _) => count++;
            bar.Value = 20;
            Assert.True(bar.Focus());
            window.KeyPress(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, null);
            Assert.Equal(25, bar.Value);
            bar.FlowDirection = FlowDirection.RightToLeft;
            window.KeyPress(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, null);
            Assert.Equal(20, bar.Value);
            var range = Assert.IsAssignableFrom<IRangeValueProvider>(ControlAutomationPeer.CreatePeerForElement(bar).GetProvider<IRangeValueProvider>());
            Assert.Equal(100, range.Maximum);
            range.SetValue(70);
            Assert.Equal(70, bar.Value);
            Assert.Equal(3, count);
            bar.IsReadOnly = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(range.IsReadOnly);
            Assert.False(bar.Focusable);
            range.SetValue(10);
            Assert.Equal(70, bar.Value);
            Assert.Equal(3, count);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Render_Shows_Played_Buffered_And_Unbuffered_Sections()
    {
        var bar = CreateBar();
        bar.Value = 25;
        bar.BufferedSeconds = 75;
        bar.PlayedBrush = Brushes.Red;
        bar.BufferedBrush = Brushes.Green;
        bar.TrackBrush = Brushes.Blue;
        var window = Show(bar);
        try
        {
            var played = Pixel(window, bar, new Point(30, 20));
            var buffered = Pixel(window, bar, new Point(120, 20));
            var rest = Pixel(window, bar, new Point(210, 20));
            Assert.NotEqual(played, buffered);
            Assert.NotEqual(buffered, rest);
            Assert.NotEqual(played, rest);
            bar.BufferedSeconds = double.NaN;
            Assert.Equal(0, bar.BufferedSeconds);
            bar.LabelPlacement = MdMediaTimeLabelPlacement.Sides;
            bar.ShowRemainingTime = true;
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(window.CaptureRenderedFrame());
        }
        finally { window.Close(); }
    }

    private static MdMediaProgressBar CreateBar() => new()
    {
        Maximum = 100, Width = 240, Height = 64,
        HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top
    };

    private static Window Show(Control content)
    {
        var window = new Window { Width = 300, Height = 150, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static uint Pixel(Window window, Control control, Point local)
    {
        var point = control.TranslatePoint(local, window)!.Value;
        var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        var size = frame!.PixelSize;
        var pixels = new uint[size.Width * size.Height];
        var pinned = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try { frame.CopyPixels(new PixelRect(0, 0, size.Width, size.Height), pinned.AddrOfPinnedObject(), pixels.Length * 4, size.Width * 4); }
        finally { pinned.Free(); }
        return pixels[(int)point.Y * size.Width + (int)point.X];
    }
}
