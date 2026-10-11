using Avalonia;
using Avalonia.Automation;
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

public sealed class MdMediaPlayerControlTests
{
    [AvaloniaFact]
    public void SeekBar_Clamps_State_And_Formats_Elapsed_And_Remaining_Time()
    {
        var seekBar = new MdSeekBar
        {
            Duration = TimeSpan.FromMinutes(2),
            Position = TimeSpan.FromMinutes(3),
            BufferedPosition = TimeSpan.FromMinutes(4),
            ShowRemainingTime = true
        };

        Assert.Equal(TimeSpan.FromMinutes(2), seekBar.Position);
        Assert.Equal(TimeSpan.FromMinutes(2), seekBar.BufferedPosition);
        Assert.Equal(1d, seekBar.ProgressFraction);
        Assert.Equal(1d, seekBar.BufferedFraction);
        Assert.Equal("2:00", seekBar.CurrentTimeText);
        Assert.Equal("-0:00", seekBar.EndTimeText);

        seekBar.Position = TimeSpan.FromSeconds(9.9);
        Assert.Equal("0:09", seekBar.CurrentTimeText);
        Assert.Equal("-1:50", seekBar.EndTimeText);
    }

    [AvaloniaFact]
    public void SeekBar_Keyboard_Change_Raises_One_Seek_Request_On_Key_Release()
    {
        var seekBar = new MdSeekBar
        {
            Width = 560,
            Duration = TimeSpan.FromMinutes(1),
            Position = TimeSpan.FromSeconds(10),
            SeekStep = TimeSpan.FromSeconds(5)
        };
        var window = new Window { Width = 640, Height = 150, Content = seekBar };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var slider = Assert.Single(seekBar.GetVisualDescendants().OfType<MdSlider>());
            Assert.True(slider.Focus());
            Dispatcher.UIThread.RunJobs();
            MdSeekRequestedEventArgs? request = null;
            var requestCount = 0;
            seekBar.SeekRequested += (_, args) =>
            {
                request = args;
                requestCount++;
            };

            window.KeyPress(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, null);
            Dispatcher.UIThread.RunJobs();
            Assert.True(seekBar.IsSeeking);
            Assert.Equal(TimeSpan.FromSeconds(15), seekBar.PreviewPosition);

            window.KeyRelease(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, null);
            Dispatcher.UIThread.RunJobs();

            Assert.False(seekBar.IsSeeking);
            Assert.Equal(TimeSpan.FromSeconds(15), seekBar.Position);
            Assert.Equal(1, requestCount);
            Assert.Equal(TimeSpan.FromSeconds(15), request!.Position);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void NowPlaying_Buttons_Emit_Playback_Requests_And_Update_Toggle_State()
    {
        var nowPlaying = new MdNowPlayingPanel
        {
            Title = "Night Drive",
            Artist = "Mira Sol",
            Duration = TimeSpan.FromMinutes(4),
            Position = TimeSpan.FromSeconds(30),
            IsPlaying = true
        };
        var window = new Window { Width = 720, Height = 720, Content = nowPlaying };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var requests = new List<MdPlaybackRequest>();
            nowPlaying.PlaybackRequested += (_, args) => requests.Add(args.Request);
            var buttons = nowPlaying.GetVisualDescendants().OfType<MdIconButton>().ToArray();

            var shuffle = Assert.Single(buttons, button => AutomationProperties.GetName(button) == "Shuffle");
            shuffle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.True(nowPlaying.IsShuffleEnabled);

            var repeat = Assert.Single(buttons, button => AutomationProperties.GetName(button) == "Repeat off");
            repeat.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(MdPlaybackRepeatMode.All, nowPlaying.RepeatMode);

            var pause = Assert.Single(buttons, button => AutomationProperties.GetName(button) == "Pause");
            pause.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.False(nowPlaying.IsPlaying);

            var queue = Assert.Single(buttons, button => AutomationProperties.GetName(button) == "Open queue");
            queue.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.Equal(
                new[] { MdPlaybackRequest.ToggleShuffle, MdPlaybackRequest.CycleRepeat, MdPlaybackRequest.Pause, MdPlaybackRequest.OpenQueue },
                requests);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Player_Controls_Fit_Mobile_Width_And_Wrap_NowPlaying_Buttons()
    {
        var video = new MdVideoPlayerControls
        {
            Width = 320,
            Height = 260,
            Duration = TimeSpan.FromMinutes(5),
            AutoHideControls = false
        };
        var nowPlaying = new MdNowPlayingPanel
        {
            Width = 320,
            Title = "Night Drive",
            Artist = "Mira Sol",
            Duration = TimeSpan.FromMinutes(4),
            Position = TimeSpan.FromMinutes(1)
        };
        var content = new StackPanel
        {
            Spacing = 12,
            Children = { video, nowPlaying }
        };
        var window = new Window { Width = 320, Height = 1100, Content = content };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            var wrapPanel = Assert.Single(nowPlaying.GetVisualDescendants().OfType<WrapPanel>());
            var nowPlayingButtons = wrapPanel.GetVisualDescendants().OfType<MdIconButton>().ToArray();
            var rows = nowPlayingButtons
                .Select(button => Math.Round(button.TranslatePoint(new Point(0, 0), wrapPanel)!.Value.Y))
                .Distinct()
                .Count();
            Assert.True(rows > 1, "Now-playing actions should wrap instead of overflowing at 320 DIPs.");

            foreach (var button in nowPlayingButtons)
            {
                var origin = button.TranslatePoint(new Point(0, 0), nowPlaying);
                Assert.NotNull(origin);
                Assert.True(origin!.Value.X >= -0.5);
                Assert.True(origin.Value.X + button.Bounds.Width <= nowPlaying.Bounds.Width + 0.5);
            }

            foreach (var button in video.GetVisualDescendants().OfType<MdIconButton>())
            {
                var origin = button.TranslatePoint(new Point(0, 0), video);
                Assert.NotNull(origin);
                Assert.True(origin!.Value.X >= -0.5);
                Assert.True(origin.Value.X + button.Bounds.Width <= video.Bounds.Width + 0.5);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Video_AutoHidden_Controls_Are_Not_Reachable_While_Focused_Controls_Stay_Visible()
    {
        var video = new MdVideoPlayerControls
        {
            Width = 400,
            Height = 260,
            IsPlaying = true,
            AutoHideControls = false
        };
        var focusSink = new Button { Content = "Focus sink" };
        var window = new Window
        {
            Width = 600,
            Height = 360,
            Content = new StackPanel { Spacing = 12, Children = { video, focusSink } }
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var pause = Assert.Single(video.GetVisualDescendants().OfType<MdIconButton>(),
                button => AutomationProperties.GetName(button) == "Pause");
            Assert.True(pause.IsEffectivelyVisible);
            Assert.True(pause.Focus());

            video.IsControlsVisible = false;
            Dispatcher.UIThread.RunJobs();
            Assert.True(video.IsControlsVisible, "Controls must remain visible while keyboard focus is within the player.");
            Assert.True(pause.IsEffectivelyVisible);

            Assert.True(focusSink.Focus());
            Dispatcher.UIThread.RunJobs();
            video.IsControlsVisible = false;
            Dispatcher.UIThread.RunJobs();
            Assert.False(pause.IsEffectivelyVisible, "Hidden transport buttons must leave the tab and automation tree.");

            video.IsControlsVisible = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(pause.IsEffectivelyVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Media_Control_Themes_Render_And_Expose_Seek_And_Transport_Parts()
    {
        var controls = new StackPanel
        {
            Spacing = 16,
            Children =
            {
                new MdVideoPlayerControls
                {
                    Height = 260,
                    Title = "Video sample",
                    Duration = TimeSpan.FromMinutes(5),
                    Position = TimeSpan.FromMinutes(1),
                    BufferedPosition = TimeSpan.FromMinutes(2),
                    AutoHideControls = false
                },
                new MdAudioMiniPlayer
                {
                    Title = "Audio sample",
                    Artist = "Test artist",
                    Duration = TimeSpan.FromMinutes(3),
                    Position = TimeSpan.FromSeconds(45),
                    BufferedPosition = TimeSpan.FromMinutes(1),
                    IsPlaying = true
                },
                new MdNowPlayingPanel
                {
                    Title = "Now playing",
                    Artist = "Test artist",
                    Album = "An album",
                    Duration = TimeSpan.FromMinutes(3),
                    Position = TimeSpan.FromSeconds(45),
                    BufferedPosition = TimeSpan.FromMinutes(1)
                }
            }
        };
        var window = new Window { Width = 900, Height = 1300, Content = controls };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Collection(controls.GetVisualDescendants().OfType<MdSeekBar>(), _ => { }, _ => { });
            Assert.Collection(controls.GetVisualDescendants().OfType<MdSlider>(), _ => { }, _ => { });
            Assert.Single(controls.GetVisualDescendants().OfType<MdVideoPlayerControls>());
            Assert.Single(controls.GetVisualDescendants().OfType<MdAudioMiniPlayer>());
            Assert.Single(controls.GetVisualDescendants().OfType<MdNowPlayingPanel>());
            Assert.NotNull(window.CaptureRenderedFrame());
        }
        finally
        {
            window.Close();
        }
    }
}
