using System.Globalization;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Localization;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdVideoPlayerTests
{
    [AvaloniaFact]
    public void Transport_Emits_Backend_Neutral_Requests_And_Formats_Timeline()
    {
        var player = new MdVideoPlayer
        {
            Duration = TimeSpan.FromMinutes(2),
            Position = TimeSpan.FromSeconds(35),
            Volume = .8
        };
        var window = new Window { Width = 820, Height = 560, Content = player };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("0:35", player.PositionText);
            Assert.Equal("2:00", player.DurationText);

            var requests = new List<MdPlaybackRequestEventArgs>();
            player.PlaybackRequested += (_, args) => requests.Add(args);
            var play = player.GetVisualDescendants().OfType<MdIconButton>()
                .Single(control => control.Name == "PART_PlayPauseButton");
            play.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(MdPlaybackRequest.Play, Assert.Single(requests).Request);

            requests.Clear();
            player.SeekBy(TimeSpan.FromSeconds(10));
            var forward = Assert.Single(requests);
            Assert.Equal(MdPlaybackRequest.Forward, forward.Request);
            Assert.Equal(TimeSpan.FromSeconds(45), forward.Position);

            requests.Clear();
            player.SetPlaybackVolume(.4);
            Assert.Equal(MdPlaybackRequest.SetVolume, Assert.Single(requests).Request);
            Assert.Equal(.4, player.Volume, 3);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Player_Localizes_Automation_And_Uses_Compact_Transport()
    {
        var player = new MdVideoPlayer { Width = 520, Height = 420 };
        MdLocalization.SetCulture(player, CultureInfo.GetCultureInfo("zh-CN"));
        var window = new Window { Width = 560, Height = 480, Content = player };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("播放", player.PlayPauseText);
            Assert.Equal("静音", player.MuteText);
            Assert.Equal("进入全屏", player.FullScreenText);

            var volume = player.GetVisualDescendants().OfType<MdSlider>()
                .Single(control => control.Name == "PART_VolumeSlider");
            Assert.False(volume.IsVisible);

            player.IsPlaying = true;
            player.IsMuted = true;
            player.IsFullScreen = true;
            Assert.Equal("暂停", player.PlayPauseText);
            Assert.Equal("取消静音", player.MuteText);
            Assert.Equal("退出全屏", player.FullScreenText);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Transport_Is_An_InPicture_Overlay_With_A_Central_Paused_Action_And_Motion()
    {
        var player = new MdVideoPlayer
        {
            Width = 800,
            Height = 450,
            Title = "Sample",
            MediaContent = new Border()
        };
        MdMotion.SetScheme(player, MdMotionScheme.Expressive);
        var window = new Window { Width = 840, Height = 500, Content = player };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var controls = player.GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_Controls");
            var centerPlay = player.GetVisualDescendants().OfType<MdIconButton>()
                .Single(control => control.Name == "PART_CenterPlayButton");

            Assert.True(controls.Bounds.Height > 400);
            Assert.True(centerPlay.IsVisible);
            Assert.NotNull(controls.Transitions);

            player.IsPlaying = true;
            Assert.False(centerPlay.IsVisible);

            MdMotion.SetScheme(player, MdMotionScheme.None);
            Assert.Null(controls.Transitions);
        }
        finally
        {
            window.Close();
        }
    }
}
