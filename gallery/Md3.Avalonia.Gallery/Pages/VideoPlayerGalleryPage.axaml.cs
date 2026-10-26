using Avalonia.Controls;
using Avalonia.Threading;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class VideoPlayerGalleryPage : UserControl
{
    private readonly DispatcherTimer _timer;

    public VideoPlayerGalleryPage()
    {
        InitializeComponent();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnPlaybackTick;
        DetachedFromVisualTree += (_, _) => _timer.Stop();
    }

    private void OnPlaybackRequested(object? sender, MdPlaybackRequestEventArgs e)
    {
        switch (e.Request)
        {
            case MdPlaybackRequest.Play:
                Player.IsPlaying = true;
                _timer.Start();
                break;
            case MdPlaybackRequest.Pause:
                Player.IsPlaying = false;
                _timer.Stop();
                break;
            case MdPlaybackRequest.Stop:
                Player.IsPlaying = false;
                Player.Position = TimeSpan.Zero;
                _timer.Stop();
                break;
            case MdPlaybackRequest.Mute:
                Player.IsMuted = true;
                break;
            case MdPlaybackRequest.Unmute:
                Player.IsMuted = false;
                break;
            case MdPlaybackRequest.EnterFullScreen:
                Player.IsFullScreen = true;
                break;
            case MdPlaybackRequest.ExitFullScreen:
                Player.IsFullScreen = false;
                break;
        }
        UpdateStatus();
    }

    private void OnPlaybackTick(object? sender, EventArgs e)
    {
        if (!Player.IsPlaying) return;
        Player.Position += TimeSpan.FromSeconds(Player.PlaybackRate);
        if (Player.Position >= Player.Duration)
        {
            Player.Position = Player.Duration;
            Player.IsPlaying = false;
            _timer.Stop();
        }
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var state = Player.IsPlaying
            ? GalleryLocalization.Choose("Playing", "正在播放")
            : GalleryLocalization.Choose("Paused", "已暂停");
        var volume = GalleryLocalization.Choose("volume", "音量");
        PlayerStatus.Text = $"{state} {Player.PositionText} · {Player.PlaybackRate:0.##}× · {volume} {Player.Volume:P0}.";
    }
}
