using Avalonia;
using Avalonia.Animation;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Motion;
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
    public void SeekBar_Pointer_Drag_Commits_One_Seek_Request()
    {
        var seekBar = new MdSeekBar
        {
            Width = 560,
            Duration = TimeSpan.FromMinutes(1),
            Position = TimeSpan.FromSeconds(15)
        };
        MdMotion.SetScheme(seekBar, MdMotionScheme.Reduced);
        var window = new Window { Width = 640, Height = 150, Content = seekBar };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var slider = Assert.Single(seekBar.GetVisualDescendants().OfType<MdSlider>());
            var thumb = Assert.Single(slider.GetVisualDescendants().OfType<MdSliderThumb>());
            var start = thumb.TranslatePoint(new Point(2, 22), window)!.Value;
            var end = start.WithX(start.X + 80);
            MdSeekRequestedEventArgs? request = null;
            var requestCount = 0;
            seekBar.SeekRequested += (_, args) =>
            {
                request = args;
                requestCount++;
            };

            window.MouseMove(start, RawInputModifiers.None);
            window.MouseDown(start, MouseButton.Left, RawInputModifiers.None);
            window.MouseMove(end, RawInputModifiers.LeftMouseButton);
            Dispatcher.UIThread.RunJobs();
            Assert.True(seekBar.IsSeeking);
            var previewIndicator = Assert.Single(seekBar.GetVisualDescendants().OfType<Border>(),
                border => border.Name == "PART_PreviewIndicator");
            var previewLabel = Assert.Single(seekBar.GetVisualDescendants().OfType<TextBlock>(),
                textBlock => textBlock.Name == "PART_PreviewLabel");
            Assert.True(previewIndicator.IsVisible);
            Assert.NotEqual("0:00", previewLabel.Text);
            var previewFade = Assert.IsType<DoubleTransition>(Assert.Single(previewIndicator.Transitions!));
            Assert.Equal(TimeSpan.FromMilliseconds(100), previewFade.Duration);
            window.MouseUp(end, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.False(seekBar.IsSeeking);
            Assert.Equal(1, requestCount);
            var completedRequest = Assert.IsType<MdSeekRequestedEventArgs>(request);
            Assert.True(completedRequest.Position > TimeSpan.FromSeconds(15));
            Assert.Equal(completedRequest.Position, seekBar.Position);
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
        MdMotion.SetScheme(video, MdMotionScheme.None);
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
    public async Task Video_Controls_Fade_And_Honor_Reduced_And_No_Motion()
    {
        var video = new MdVideoPlayerControls
        {
            Width = 400,
            Height = 260,
            IsPlaying = true,
            AutoHideControls = false
        };
        MdMotion.SetScheme(video, MdMotionScheme.Reduced);
        var focusSink = new Button { Content = "Focus sink" };
        var window = new Window
        {
            Width = 600,
            Height = 360,
            Content = new StackPanel { Children = { video, focusSink } }
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.True(focusSink.Focus());
            var overlay = Assert.Single(video.GetVisualDescendants().OfType<Grid>(), grid => grid.Name == "PART_Overlay");
            var pause = Assert.Single(video.GetVisualDescendants().OfType<MdIconButton>(),
                button => AutomationProperties.GetName(button) == "Pause");
            var fade = Assert.IsType<DoubleTransition>(Assert.Single(overlay.Transitions!));
            Assert.Equal(TimeSpan.FromMilliseconds(100), fade.Duration);

            video.IsControlsVisible = false;
            Dispatcher.UIThread.RunJobs();
            Assert.True(overlay.IsVisible, "The overlay must remain mounted for its reduced-motion fade-out.");
            Assert.False(pause.IsEffectivelyEnabled, "Controls must stop accepting input as soon as they begin fading out.");
            await Task.Delay(130);
            Dispatcher.UIThread.RunJobs();
            Assert.False(overlay.IsVisible);
            Assert.False(pause.IsEffectivelyVisible, "Fully hidden controls must leave the automation and tab order.");

            MdMotion.SetScheme(video, MdMotionScheme.None);
            Dispatcher.UIThread.RunJobs();
            Assert.Null(overlay.Transitions);
            video.IsControlsVisible = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(overlay.IsVisible);
            video.IsControlsVisible = false;
            Dispatcher.UIThread.RunJobs();
            Assert.False(overlay.IsVisible, "The no-motion scheme removes the overlay without a delayed exit.");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Video_DoubleTap_Requests_Ten_Second_Jumps_And_Shows_Feedback()
    {
        var video = new MdVideoPlayerControls
        {
            Width = 600,
            Height = 260,
            IsPlaying = true,
            AutoHideControls = false
        };
        MdMotion.SetScheme(video, MdMotionScheme.None);
        var window = new Window { Width = 600, Height = 300, Content = video };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var requests = new List<MdPlaybackRequest>();
            video.PlaybackRequested += (_, args) => requests.Add(args.Request);

            Tap(window, new Point(40, 130));
            await Task.Delay(60);
            Dispatcher.UIThread.RunJobs();
            Tap(window, new Point(42, 130));
            Assert.Equal(new[] { MdPlaybackRequest.Rewind }, requests);
            var feedbackLabel = Assert.Single(video.GetVisualDescendants().OfType<TextBlock>(),
                textBlock => textBlock.Name == "PART_GestureFeedbackLabel");
            Assert.Equal("10 sec back", feedbackLabel.Text);
            var feedback = Assert.Single(video.GetVisualDescendants().OfType<Border>(),
                border => border.Name == "PART_GestureFeedback");
            Assert.True(feedback.IsVisible);

            Tap(window, new Point(560, 130));
            await Task.Delay(60);
            Dispatcher.UIThread.RunJobs();
            Tap(window, new Point(558, 130));
            Assert.Equal(new[] { MdPlaybackRequest.Rewind, MdPlaybackRequest.Forward }, requests);
            Assert.Equal("10 sec ahead", feedbackLabel.Text);

            Tap(window, new Point(300, 20));
            await Task.Delay(60);
            Dispatcher.UIThread.RunJobs();
            Tap(window, new Point(300, 20));
            Assert.Equal(new[] { MdPlaybackRequest.Rewind, MdPlaybackRequest.Forward, MdPlaybackRequest.Pause }, requests);
            Assert.False(video.IsPlaying, "A center double-tap should toggle playback without targeting a control.");
            Assert.Equal("Pause", feedbackLabel.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Video_DoubleTap_Does_Not_Replace_Transport_Button_Clicks()
    {
        var video = new MdVideoPlayerControls
        {
            Width = 600,
            Height = 260,
            IsPlaying = true,
            AutoHideControls = false
        };
        MdMotion.SetScheme(video, MdMotionScheme.None);
        var window = new Window { Width = 600, Height = 300, Content = video };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var rewind = Assert.Single(video.GetVisualDescendants().OfType<MdIconButton>(),
                button => AutomationProperties.GetName(button) == "Rewind 10 seconds");
            var point = rewind.TranslatePoint(new Point(rewind.Bounds.Width / 2, rewind.Bounds.Height / 2), window)!.Value;
            var requests = new List<MdPlaybackRequest>();
            video.PlaybackRequested += (_, args) => requests.Add(args.Request);

            Tap(window, point);
            Tap(window, point);

            Assert.Equal(new[] { MdPlaybackRequest.Rewind, MdPlaybackRequest.Rewind }, requests);

            requests.Clear();
            window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            await Task.Delay(700);
            Dispatcher.UIThread.RunJobs();
            Assert.Empty(requests);
            window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(new[] { MdPlaybackRequest.Rewind }, requests);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Video_LongPress_On_Either_Edge_Seeks_Continuously_With_Rate_Feedback()
    {
        var video = new MdVideoPlayerControls
        {
            Width = 600,
            Height = 260,
            Duration = TimeSpan.FromMinutes(3),
            Position = TimeSpan.FromSeconds(45),
            IsPlaying = true,
            AutoHideControls = false
        };
        MdMotion.SetScheme(video, MdMotionScheme.None);
        var window = new Window { Width = 600, Height = 300, Content = video };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var requestedPositions = new List<TimeSpan>();
            video.SeekRequested += (_, args) => requestedPositions.Add(args.Position);
            var feedbackLabel = Assert.Single(video.GetVisualDescendants().OfType<TextBlock>(),
                textBlock => textBlock.Name == "PART_GestureFeedbackLabel");

            var left = new Point(40, 130);
            window.MouseMove(left, RawInputModifiers.None);
            window.MouseDown(left, MouseButton.Left, RawInputModifiers.None);
            await Task.Delay(850);
            Dispatcher.UIThread.RunJobs();
            Assert.True(requestedPositions.Count >= 2, "Holding the left edge should issue repeated seek targets.");
            Assert.All(requestedPositions, position => Assert.True(position < TimeSpan.FromSeconds(45)));
            Assert.Equal("2x back", feedbackLabel.Text);
            window.MouseUp(left, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            var beforeForward = requestedPositions.Count;
            var right = new Point(560, 130);
            window.MouseMove(right, RawInputModifiers.None);
            window.MouseDown(right, MouseButton.Left, RawInputModifiers.None);
            await Task.Delay(2_100);
            Dispatcher.UIThread.RunJobs();
            Assert.True(requestedPositions.Count >= beforeForward + 2, "Holding the right edge should issue repeated seek targets.");
            Assert.All(requestedPositions.Skip(beforeForward), position => Assert.True(position > TimeSpan.FromSeconds(45)));
            Assert.Equal("4x ahead", feedbackLabel.Text);
            window.MouseUp(right, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.True(video.IsControlsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Video_PlayPause_Button_Emits_Animated_State_Feedback()
    {
        var video = new MdVideoPlayerControls
        {
            Width = 600,
            Height = 260,
            AutoHideControls = false
        };
        MdMotion.SetScheme(video, MdMotionScheme.Reduced);
        var window = new Window { Width = 600, Height = 300, Content = video };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var play = Assert.Single(video.GetVisualDescendants().OfType<MdIconButton>(),
                button => AutomationProperties.GetName(button) == "Play");
            var requests = new List<MdPlaybackRequest>();
            video.PlaybackRequested += (_, args) => requests.Add(args.Request);
            play.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            Assert.True(video.IsPlaying);
            Assert.Equal(new[] { MdPlaybackRequest.Play }, requests);
            var feedback = Assert.Single(video.GetVisualDescendants().OfType<Border>(),
                border => border.Name == "PART_GestureFeedback");
            var label = Assert.Single(video.GetVisualDescendants().OfType<TextBlock>(),
                textBlock => textBlock.Name == "PART_GestureFeedbackLabel");
            Assert.True(feedback.IsVisible);
            Assert.Equal("Play", label.Text);
            var feedbackFade = Assert.IsType<DoubleTransition>(Assert.Single(feedback.Transitions!));
            Assert.Equal(TimeSpan.FromMilliseconds(100), feedbackFade.Duration);
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

    private static void Tap(Window window, Point point)
    {
        window.MouseMove(point, RawInputModifiers.None);
        window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
    }
}
