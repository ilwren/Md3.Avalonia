using LibVLCSharp.Shared;

namespace Md3.Avalonia.Gallery.Media;

/// <summary>
/// A Gallery-owned LibVLC session with independent audio and video players. It is deliberately
/// outside Md3.Avalonia so the Material controls remain playback-engine agnostic.
/// </summary>
public sealed class GalleryMediaPlaybackSession : IDisposable
{
    private readonly LibVLC _libVlc;
    private bool _disposed;

    public GalleryMediaPlaybackSession(VlcVideoSurface videoSurface)
    {
        ArgumentNullException.ThrowIfNull(videoSurface);
        Core.Initialize();
        var libVlc = new LibVLC(false, "--no-video-title-show");
        MediaPlayer? audioPlayer = null;
        MediaPlayer? videoPlayer = null;
        try
        {
            audioPlayer = new MediaPlayer(libVlc);
            videoPlayer = new MediaPlayer(libVlc);
            videoSurface.Attach(videoPlayer);
        }
        catch
        {
            if (videoPlayer is not null)
                TryDispose(videoPlayer.Dispose);
            if (audioPlayer is not null)
                TryDispose(audioPlayer.Dispose);
            TryDispose(libVlc.Dispose);
            throw;
        }

        _libVlc = libVlc;
        AudioPlayer = audioPlayer!;
        VideoPlayer = videoPlayer!;
    }

    public MediaPlayer AudioPlayer { get; }
    public MediaPlayer VideoPlayer { get; }

    public bool OpenAudio(string filePath) => Open(AudioPlayer, filePath);
    public bool OpenVideo(string filePath) => Open(VideoPlayer, filePath);

    private bool Open(MediaPlayer player, string filePath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        player.Stop();
        using var media = new LibVLCSharp.Shared.Media(_libVlc, filePath, FromType.FromPath, ":no-video-title-show");
        return player.Play(media);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        TryDispose(AudioPlayer.Stop);
        TryDispose(VideoPlayer.Stop);
        TryDispose(AudioPlayer.Dispose);
        TryDispose(VideoPlayer.Dispose);
        TryDispose(_libVlc.Dispose);
    }

    private static void TryDispose(Action action)
    {
        try
        {
            action();
        }
        catch
        {
            // Continue releasing the remaining native resources if one player already stopped.
        }
    }
}
