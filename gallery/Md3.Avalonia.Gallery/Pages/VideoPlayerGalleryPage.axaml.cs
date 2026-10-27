using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class VideoPlayerGalleryPage : UserControl
{
    private readonly DispatcherTimer _timer;
    private readonly MdVideoPlayer _player;
    private readonly bool _usesNativeBackend;
    private readonly IDisposable? _nativeBackend;

    public VideoPlayerGalleryPage()
    {
        InitializeComponent();
        _player = SimulatedPlayer;

        if (GalleryVideoBackend.Factory is { } factory)
        {
            try
            {
                var player = factory(GalleryVideoBackend.ExtractBundledSample());
                player.Title = "Material motion";
                player.SupportingText = "Local H.264 · LibVLC";
                player.PlaybackRequested += OnPlaybackRequested;
                PlayerHost.Content = player;
                _player = player;
                _nativeBackend = player as IDisposable;
                _usesNativeBackend = true;
                BackendStatus.Text = "Using Md3.Avalonia.LibVLC with the bundled local video.";
            }
            catch (Exception exception)
            {
                var failure = GalleryLocalization.Choose(
                    "The desktop video backend could not be created",
                    "无法创建桌面视频后端");
                BackendStatus.Text = $"{failure}: {exception.Message}";
            }
        }

        _timer = new DispatcherTimer
        {
            Interval = _usesNativeBackend ? TimeSpan.FromMilliseconds(250) : TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnPlaybackTick;
        if (_usesNativeBackend) _timer.Start();
        DetachedFromVisualTree += OnDetachedFromVisualTree;
        UpdateStatus();
    }

    private void OnPlaybackRequested(object? sender, MdPlaybackRequestEventArgs e)
    {
        if (_usesNativeBackend)
        {
            if (e.Request is MdPlaybackRequest.EnterFullScreen or MdPlaybackRequest.ExitFullScreen)
                _player.IsFullScreen = e.Request == MdPlaybackRequest.EnterFullScreen;
            UpdateStatus();
            return;
        }

        switch (e.Request)
        {
            case MdPlaybackRequest.Play:
                if (_player.Duration > TimeSpan.Zero && _player.Position >= _player.Duration)
                    _player.Position = TimeSpan.Zero;
                _player.IsPlaying = true;
                _timer.Start();
                break;
            case MdPlaybackRequest.Pause:
                _player.IsPlaying = false;
                _timer.Stop();
                break;
            case MdPlaybackRequest.Stop:
                _player.IsPlaying = false;
                _player.Position = TimeSpan.Zero;
                _timer.Stop();
                break;
            case MdPlaybackRequest.Mute:
                _player.IsMuted = true;
                break;
            case MdPlaybackRequest.Unmute:
                _player.IsMuted = false;
                break;
            case MdPlaybackRequest.EnterFullScreen:
                _player.IsFullScreen = true;
                break;
            case MdPlaybackRequest.ExitFullScreen:
                _player.IsFullScreen = false;
                break;
        }
        UpdateStatus();
    }

    private void OnPlaybackTick(object? sender, EventArgs e)
    {
        if (_usesNativeBackend)
        {
            if (!string.IsNullOrWhiteSpace(_player.ErrorMessage))
            {
                BackendStatus.Text = string.Format(
                    GalleryLocalization.Choose(
                        "The local video could not start: {0}",
                        "本地视频无法启动：{0}"),
                    _player.ErrorMessage);
            }
            UpdateStatus();
            return;
        }

        if (!_player.IsPlaying) return;
        _player.Position += TimeSpan.FromSeconds(_player.PlaybackRate);
        if (_player.Position >= _player.Duration)
        {
            _player.Position = _player.Duration;
            _player.IsPlaying = false;
            _timer.Stop();
        }
        UpdateStatus();
    }

    private void RestartSample(object? sender, RoutedEventArgs e)
    {
        if (_player.IsSeekable) _player.Seek(TimeSpan.Zero);
        _player.Play();
    }

    private void UpdateStatus()
    {
        var state = _player.IsPlaying
            ? GalleryLocalization.Choose("Playing", "正在播放")
            : GalleryLocalization.Choose("Paused", "已暂停");
        var volume = GalleryLocalization.Choose("volume", "音量");
        PlayerStatus.Text = $"{state} {_player.PositionText} · {_player.PlaybackRate:0.##}× · {volume} {_player.Volume:P0}.";
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _timer.Stop();
        _player.PlaybackRequested -= OnPlaybackRequested;
        _nativeBackend?.Dispose();
    }
}
