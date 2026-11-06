using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Pages;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdMarqueeTests
{
    private const string LongText = "Movement of these words is easily visible in the viewport.";

    [AvaloniaFact]
    public void Time_Step_Moves_At_The_Configured_Speed_And_Pauses_After_A_Round()
    {
        var marquee = Create();
        marquee.Gap = 20;
        marquee.Speed = 100;
        marquee.PauseAfterRound = TimeSpan.FromSeconds(1);
        var window = Show(marquee);
        try
        {
            Assert.True(marquee.IsRunning);
            // Reset after initial frame scheduling so these steps have a deterministic origin.
            marquee.Speed = 101;
            marquee.Speed = 100;
            Assert.Equal(0, marquee.ScrollOffset);
            marquee.Advance(0.25);
            Assert.Equal(25, marquee.ScrollOffset, 5);
            var remaining = (marquee.NaturalTextWidth + marquee.Gap - 25) / marquee.Speed;
            marquee.Advance(remaining + 0.25);
            Assert.Equal(0, marquee.ScrollOffset, 5);
            Assert.Equal(0.75, marquee.RemainingPause, 5);
            marquee.Advance(0.5);
            Assert.Equal(0, marquee.ScrollOffset, 5);
            Assert.Equal(0.25, marquee.RemainingPause, 5);
            marquee.Advance(0.5);
            Assert.Equal(25, marquee.ScrollOffset, 5);
            Assert.Equal(0, marquee.RemainingPause, 5);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Motion_Visibility_And_Detachment_Stop_And_Restart_The_Render_Loop()
    {
        var marquee = Create();
        var parent = new Border { Child = marquee, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        MdMotion.SetScheme(parent, MdMotionScheme.Standard);
        var window = Show(parent);
        try
        {
            Assert.True(marquee.IsRunning);
            marquee.Advance(0.5);
            Assert.True(marquee.ScrollOffset > 0);
            parent.IsVisible = false;
            Assert.False(marquee.IsRunning);
            Assert.Equal(0, marquee.ScrollOffset);
            parent.IsVisible = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(marquee.IsRunning);
            MdMotion.SetScheme(parent, MdMotionScheme.Reduced);
            Assert.False(marquee.IsRunning);
            MdMotion.SetScheme(parent, MdMotionScheme.None);
            Assert.False(marquee.IsRunning);
            MdMotion.SetScheme(parent, MdMotionScheme.Standard);
            Assert.True(marquee.IsRunning);
            marquee.IsActive = false;
            Assert.False(marquee.IsRunning);
            marquee.IsActive = true;
            Assert.True(marquee.IsRunning);
            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            Assert.False(marquee.IsRunning);
            window.Content = parent;
            Dispatcher.UIThread.RunJobs();
            Assert.True(marquee.IsRunning);
        }
        finally { window.Close(); }
        Assert.False(marquee.IsRunning);
    }

    [AvaloniaFact]
    public void Actual_Render_Frames_Advance_And_Stop_The_Marquee()
    {
        var marquee = Create();
        var window = Show(marquee);
        try
        {
            marquee.Speed = 41;
            marquee.Speed = 40;
            for (var i = 0; i < 20 && marquee.ScrollOffset == 0; i++)
            {
                Thread.Sleep(25);
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                Dispatcher.UIThread.RunJobs();
            }
            Assert.True(marquee.ScrollOffset > 0, "Real render-frame callbacks never advanced the text.");
            MdMotion.SetScheme(marquee, MdMotionScheme.None);
            Assert.False(marquee.IsRunning);
            Assert.Equal(0, marquee.ScrollOffset);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(0, marquee.ScrollOffset);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Overflow_And_Property_Changes_Control_Animation()
    {
        var marquee = Create();
        var window = Show(marquee);
        try
        {
            Assert.True(marquee.IsRunning);
            marquee.Text = "short";
            Dispatcher.UIThread.RunJobs();
            Assert.False(marquee.IsRunning);
            marquee.Text = LongText;
            Dispatcher.UIThread.RunJobs();
            Assert.True(marquee.IsRunning);
            marquee.Width = 700;
            Dispatcher.UIThread.RunJobs();
            Assert.False(marquee.IsRunning);
            marquee.Width = 120;
            Dispatcher.UIThread.RunJobs();
            Assert.True(marquee.IsRunning);
            marquee.Speed = 0;
            Assert.False(marquee.IsRunning);
            marquee.Speed = double.NaN;
            Assert.Equal(40, marquee.Speed);
            Assert.True(marquee.IsRunning);
            marquee.Gap = -1;
            Assert.Equal(0, marquee.Gap);
            marquee.PauseAfterRound = TimeSpan.FromSeconds(-1);
            Assert.Equal(TimeSpan.Zero, marquee.PauseAfterRound);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Rtl_Auto_And_Explicit_Directions_Use_Opposite_Physical_Origins()
    {
        var marquee = Create();
        marquee.FlowDirection = FlowDirection.RightToLeft;
        var window = Show(marquee);
        try
        {
            Assert.True(marquee.IsRunning);
            marquee.Speed = 41;
            marquee.Speed = 40;
            var start = marquee.GetOrigins();
            Assert.Equal(marquee.Bounds.Width - marquee.NaturalTextWidth, start.First, 3);
            Assert.True(start.Second < start.First);
            marquee.Advance(0.2);
            Assert.True(marquee.GetOrigins().First > start.First);
            marquee.Direction = MdMarqueeDirection.Left;
            start = marquee.GetOrigins();
            marquee.Advance(0.2);
            var next = marquee.GetOrigins();
            Assert.True(next.First < start.First);
            Assert.True(next.Second > next.First);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Rendered_Pixels_Move_Inside_The_Viewport_And_Static_Fallback_Is_Visible()
    {
        var marquee = Create();
        marquee.Foreground = Brushes.Red;
        marquee.Speed = 100;
        var window = Show(marquee);
        try
        {
            marquee.Speed = 101;
            marquee.Speed = 100;
            var before = PixelsWithin(window, marquee);
            marquee.Advance(0.2);
            Dispatcher.UIThread.RunJobs();
            var after = PixelsWithin(window, marquee);
            Assert.Equal(before.Length, after.Length);
            Assert.True(before.Zip(after, (a, b) => a != b).Count(different => different) > 20);
            MdMotion.SetScheme(marquee, MdMotionScheme.Reduced);
            Dispatcher.UIThread.RunJobs();
            var fallback = PixelsWithin(window, marquee);
            Assert.False(marquee.IsRunning);
            Assert.Contains(fallback, p => (p & 0x00ff00u) < 0x004000u && ((p & 0xffu) > 200 || ((p >> 16) & 0xffu) > 200));
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Automation_Exposes_The_Full_Text_Once_And_Honors_Explicit_Names()
    {
        var marquee = Create();
        marquee.Text = "First line\nSecond line";
        var window = Show(marquee);
        try
        {
            var peer = ControlAutomationPeer.CreatePeerForElement(marquee);
            Assert.Equal(AutomationControlType.Text, peer.GetAutomationControlType());
            Assert.Equal(marquee.Text, peer.GetName());
            var changes = 0;
            peer.PropertyChanged += (_, e) => { if (e.Property == AutomationElementIdentifiers.NameProperty) changes++; };
            marquee.Text = "Updated complete text";
            Assert.Equal(1, changes);
            Assert.Equal(marquee.Text, peer.GetName());
            AutomationProperties.SetName(marquee, "Custom announcement");
            var before = changes;
            marquee.Text = "Another update";
            Assert.Equal(before, changes);
            Assert.Equal("Custom announcement", peer.GetName());
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Gallery_Translates_And_Restores_The_Marquee_Content()
    {
        var page = new AnimatedTextGalleryPage();
        var marquee = page.GetLogicalDescendants().OfType<MdMarquee>().Single();
        var original = marquee.Text;
        GalleryLocalization.Apply(page, CultureInfo.GetCultureInfo("zh-CN"));
        Assert.StartsWith("较长的公告", marquee.Text);
        GalleryLocalization.Apply(page, CultureInfo.GetCultureInfo("en-US"));
        Assert.Equal(original, marquee.Text);
    }

    private static MdMarquee Create()
    {
        var marquee = new MdMarquee
        {
            Text = LongText, Width = 120, Height = 48, FontSize = 20,
            HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top
        };
        return marquee;
    }

    private static Window Show(Control content)
    {
        var window = new Window { Width = 800, Height = 150, Content = content, Background = Brushes.White };
        MdMotion.SetScheme(window, MdMotionScheme.Standard);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static uint[] PixelsWithin(Window window, Control control)
    {
        var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        var size = frame!.PixelSize;
        var pixels = new uint[size.Width * size.Height];
        var pinned = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try { frame.CopyPixels(new PixelRect(0, 0, size.Width, size.Height), pinned.AddrOfPinnedObject(), pixels.Length * 4, size.Width * 4); }
        finally { pinned.Free(); }
        var origin = control.TranslatePoint(new Point(), window)!.Value;
        var left = Math.Max(0, (int)origin.X);
        var top = Math.Max(0, (int)origin.Y);
        var right = Math.Min(size.Width, (int)Math.Ceiling(origin.X + control.Bounds.Width));
        var bottom = Math.Min(size.Height, (int)Math.Ceiling(origin.Y + control.Bounds.Height));
        var result = new List<uint>();
        for (var y = top; y < bottom; y++)
            for (var x = left; x < right; x++) result.Add(pixels[y * size.Width + x]);
        return result.ToArray();
    }
}
