using System.Runtime.InteropServices;
using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Pages;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdConfettiTests
{
    [AvaloniaFact]
    public void A_Seeded_Burst_Uses_Independent_Velocity_Gravity_And_Drag()
    {
        var first = Create();
        var second = Create();
        first.Seed = second.Seed = 123;
        first.DirectionDegrees = second.DirectionDegrees = 0;
        first.SpreadDegrees = second.SpreadDegrees = 0;
        first.Gravity = second.Gravity = 80;
        first.Drag = 0;
        second.Drag = 1;
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Children = { first, second } };
        var window = Show(panel);
        try
        {
            Assert.True(first.Burst());
            Assert.True(second.Burst());
            Assert.Equal(first.ParticleCount, first.ActiveParticleCount);
            Assert.Equal(first.ParticlePosition(0), second.ParticlePosition(0));
            var origin = first.ParticlePosition(0);
            first.Advance(1);
            second.Advance(1);
            var undamped = first.ParticlePosition(0);
            var damped = second.ParticlePosition(0);
            Assert.True(undamped.X > damped.X && damped.X > origin.X);
            Assert.Equal(origin.Y + 40, undamped.Y, 5);
            Assert.Equal(undamped.Y, damped.Y, 5);
            first.Gravity = -300; // changes affect the next burst, not the active snapshot
            first.Advance(0.1);
            Assert.True(first.ParticlePosition(0).Y > undamped.Y);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Restart_Stop_And_Natural_Completion_Have_Distinct_Contracts()
    {
        var confetti = Create();
        confetti.Duration = TimeSpan.FromSeconds(1);
        var window = Show(confetti);
        try
        {
            var completed = 0;
            confetti.Completed += (_, _) => completed++;
            Assert.True(confetti.Burst());
            confetti.Advance(0.5);
            Assert.True(confetti.IsRunning);
            Assert.True(confetti.Burst());
            Assert.Equal(0, confetti.ElapsedSeconds);
            Assert.Equal(0, completed);
            confetti.Stop();
            Assert.False(confetti.IsRunning);
            Assert.Equal(0, confetti.ActiveParticleCount);
            Assert.Equal(0, completed);
            Assert.True(confetti.Burst());
            confetti.Advance(1);
            Assert.False(confetti.IsRunning);
            Assert.Equal(0, confetti.ActiveParticleCount);
            Assert.Equal(1, completed);
            confetti.Advance(1);
            Assert.Equal(1, completed);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Reduced_Motion_Hiding_And_Detachment_Cancel_Without_Auto_Restart()
    {
        var confetti = Create();
        Assert.False(confetti.Burst());
        var parent = new Border { Child = confetti, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        MdMotion.SetScheme(parent, MdMotionScheme.Standard);
        var window = Show(parent);
        try
        {
            var completed = 0;
            confetti.Completed += (_, _) => completed++;
            Assert.True(confetti.Burst());
            parent.IsVisible = false;
            Assert.False(confetti.IsRunning);
            Assert.False(confetti.Burst());
            parent.IsVisible = true;
            Dispatcher.UIThread.RunJobs();
            Assert.False(confetti.IsRunning);
            Assert.True(confetti.Burst());
            MdMotion.SetScheme(parent, MdMotionScheme.Reduced);
            Assert.False(confetti.IsRunning);
            Assert.False(confetti.Burst());
            MdMotion.SetScheme(parent, MdMotionScheme.None);
            Assert.False(confetti.Burst());
            MdMotion.SetScheme(parent, MdMotionScheme.Standard);
            Assert.True(confetti.Burst());
            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            Assert.False(confetti.IsRunning);
            window.Content = parent;
            Dispatcher.UIThread.RunJobs();
            Assert.False(confetti.IsRunning);
            Assert.True(confetti.Burst());
            Assert.Equal(0, completed);
        }
        finally { window.Close(); }
        Assert.False(confetti.IsRunning);
    }

    [AvaloniaFact]
    public void Actual_Render_Frames_Advance_And_Can_Be_Canceled()
    {
        var confetti = Create();
        var window = Show(confetti);
        try
        {
            Assert.True(confetti.Burst());
            for (var i = 0; i < 20 && confetti.ElapsedSeconds == 0; i++)
            {
                Thread.Sleep(25);
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                Dispatcher.UIThread.RunJobs();
            }
            Assert.True(confetti.ElapsedSeconds > 0, "Real render-frame callbacks never advanced the particles.");
            confetti.Stop();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            Assert.False(confetti.IsRunning);
            Assert.Equal(0, confetti.ActiveParticleCount);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Drawn_Particles_Are_Clipped_To_The_Control()
    {
        var confetti = Create();
        confetti.Margin = new Thickness(30, 20, 0, 0);
        confetti.ParticleCount = 160;
        confetti.Seed = 42;
        confetti.Speed = 300;
        confetti.SpreadDegrees = 360;
        confetti.PrimaryBrush = confetti.SecondaryBrush = confetti.TertiaryBrush = Brushes.Red;
        var window = Show(confetti);
        try
        {
            Assert.True(confetti.Burst());
            confetti.Advance(0.3);
            Dispatcher.UIThread.RunJobs();
            var (pixels, size) = Pixels(window);
            var origin = confetti.TranslatePoint(new Point(), window)!.Value;
            var left = (int)origin.X;
            var top = (int)origin.Y;
            var right = (int)Math.Ceiling(origin.X + confetti.Bounds.Width);
            var bottom = (int)Math.Ceiling(origin.Y + confetti.Bounds.Height);
            var inside = 0;
            var outside = 0;
            for (var y = 1; y < size.Height - 1; y++)
                for (var x = 1; x < size.Width - 1; x++)
                {
                    var p = pixels[y * size.Width + x];
                    var isRed = (p & 0x00ff00u) < 0x004000u && ((p & 0xffu) > 200 || ((p >> 16) & 0xffu) > 200);
                    if (!isRed) continue;
                    if (x >= left && x < right && y >= top && y < bottom) inside++;
                    else outside++;
                }
            Assert.True(inside > 20, "Expected colored particle pixels inside the control.");
            Assert.Equal(0, outside);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Overlay_Does_Not_Intercept_Pointer_Or_Add_Automation_Content()
    {
        var button = new MdButton { Content = "Test", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        var confetti = new MdConfetti { Seed = 1 };
        var grid = new Grid { Width = 240, Height = 160, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        grid.Children.Add(button);
        grid.Children.Add(confetti);
        var window = Show(grid);
        try
        {
            Assert.False(confetti.IsHitTestVisible);
            var peer = ControlAutomationPeer.CreatePeerForElement(confetti);
            Assert.Equal(AutomationControlType.None, peer.GetAutomationControlType());
            Assert.False(peer.IsContentElement());
            Assert.False(peer.IsControlElement());
            var clicks = 0;
            button.Click += (_, _) => clicks++;
            Assert.True(confetti.Burst());
            var point = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;
            window.MouseMove(point, RawInputModifiers.None);
            window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            Assert.Equal(1, clicks);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Gallery_Button_Triggers_A_Burst_And_The_Label_Is_Bilingual()
    {
        var page = new AnimatedTextGalleryPage();
        var button = page.GetLogicalDescendants().OfType<MdButton>().Single(element => Equals(element.Content, "Celebrate"));
        GalleryLocalization.Apply(page, CultureInfo.GetCultureInfo("zh-CN"));
        Assert.Equal("庆祝", button.Content);
        GalleryLocalization.Apply(page, CultureInfo.GetCultureInfo("en-US"));
        Assert.Equal("Celebrate", button.Content);
        var window = new Window { Width = 900, Height = 1800, Content = page };
        MdMotion.SetScheme(window, MdMotionScheme.Standard);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            var confetti = page.GetVisualDescendants().OfType<MdConfetti>().Single();
            var point = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;
            window.MouseMove(point, RawInputModifiers.None);
            window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            Assert.True(confetti.IsRunning);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Invalid_Values_Are_Coerced_And_Zero_Count_Suppresses_A_Burst()
    {
        var confetti = Create();
        var window = Show(confetti);
        try
        {
            confetti.ParticleCount = -1;
            Assert.Equal(0, confetti.ParticleCount);
            Assert.False(confetti.Burst());
            confetti.ParticleCount = 1000;
            Assert.Equal(500, confetti.ParticleCount);
            confetti.Origin = new Point(double.NaN, 0);
            Assert.Equal(new Point(0.5, 0.8), confetti.Origin);
            confetti.Speed = double.PositiveInfinity;
            Assert.Equal(160, confetti.Speed);
            confetti.Drag = -2;
            Assert.Equal(0, confetti.Drag);
            confetti.Duration = TimeSpan.Zero;
            Assert.Equal(TimeSpan.FromMilliseconds(100), confetti.Duration);
        }
        finally { window.Close(); }
    }

    private static MdConfetti Create() => new()
    {
        Width = 240, Height = 160, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top
    };

    private static Window Show(Control content)
    {
        var window = new Window { Width = 320, Height = 220, Content = content, Background = Brushes.White };
        MdMotion.SetScheme(window, MdMotionScheme.Standard);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static (uint[] Pixels, PixelSize Size) Pixels(Window window)
    {
        var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        var size = frame!.PixelSize;
        var pixels = new uint[size.Width * size.Height];
        var pinned = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try { frame.CopyPixels(new PixelRect(0, 0, size.Width, size.Height), pinned.AddrOfPinnedObject(), pixels.Length * 4, size.Width * 4); }
        finally { pinned.Free(); }
        return (pixels, size);
    }
}
