using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using Md3.Avalonia.Media.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class MediaPlayerGalleryPage : UserControl
{
    // Licensed local sample clip. Attribution and source details are shown on the gallery page.
    private static readonly string PreviewVideoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Media", "BigBuckBunny-480p.mp4");
    private LibVLC? _libVlc;
    private MediaPlayer? _mediaPlayer;
    private LibVLCSharp.Shared.Media? _media;
    private LibVlcPlaybackSession? _session;

    public MediaPlayerGalleryPage()
    {
        InitializeComponent();
        Player.VideoContent = new Border
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.Parse("#273A55"), 0),
                    new GradientStop(Color.Parse("#10141D"), 1)
                }
            },
            Child = new TextBlock
            {
                Text = "Big Buck Bunny · CC BY 3.0",
                FontSize = 18,
                Foreground = Brushes.White,
                HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center
            }
        };
        Player.FullscreenRequested += (_, _) =>
        {
            if (TopLevel.GetTopLevel(this) is Window window)
                window.WindowState = window.WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
        };
        AttachedToVisualTree += (_, _) => StartPlayback();
        DetachedFromVisualTree += (_, _) => StopPlayback();
    }

    private void StartPlayback()
    {
        if (_mediaPlayer is not null) return;
        if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
        {
            PlaybackStatus.Text = "The real LibVLC preview currently requires the desktop gallery.";
            return;
        }

        try
        {
            if (!File.Exists(PreviewVideoPath))
                throw new FileNotFoundException("The bundled local sample video was not copied to the app output directory.", PreviewVideoPath);

            Core.Initialize();
            _libVlc = new LibVLC("--no-video-title-show");
            _mediaPlayer = new MediaPlayer(_libVlc);
            _mediaPlayer.EncounteredError += OnPlaybackError;
            _mediaPlayer.Playing += OnPlaybackStarted;
            _media = new LibVLCSharp.Shared.Media(_libVlc, new Uri(PreviewVideoPath));
            _mediaPlayer.Media = _media;
            Player.AttachLibVlc(_mediaPlayer);
            _session = (LibVlcPlaybackSession)Player.Session!;
            PlaybackStatus.Text = "Opening local video file…";
            if (!_mediaPlayer.Play())
                PlaybackStatus.Text = "LibVLC could not start the local preview. Check the bundled video file and native VLC runtime.";
        }
        catch (Exception ex)
        {
            PlaybackStatus.Text = $"Video preview unavailable: {ex.Message}";
            DisposePlayback();
        }
    }

    private void OnPlaybackStarted(object? sender, EventArgs e) =>
        Dispatcher.UIThread.Post(() => PlaybackStatus.Text = "Playing local Big Buck Bunny · Blender Foundation · CC BY 3.0");

    private void OnPlaybackError(object? sender, EventArgs e) =>
        Dispatcher.UIThread.Post(() => PlaybackStatus.Text = "Local playback failed. Check the sample file and installed LibVLC runtime.");

    private void StopPlayback()
    {
        if (_mediaPlayer is not null)
        {
            _mediaPlayer.EncounteredError -= OnPlaybackError;
            _mediaPlayer.Playing -= OnPlaybackStarted;
            _mediaPlayer.Stop();
        }
        DisposePlayback();
        Player.Session = null;
        Player.VideoContent = null;
    }

    private void DisposePlayback()
    {
        _session?.Dispose();
        _session = null;
        _media?.Dispose();
        _media = null;
        _mediaPlayer?.Dispose();
        _mediaPlayer = null;
        _libVlc?.Dispose();
        _libVlc = null;
    }
}
