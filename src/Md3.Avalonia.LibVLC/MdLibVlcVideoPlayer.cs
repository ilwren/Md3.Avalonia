using Avalonia;
using Avalonia.Threading;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using Md3.Avalonia.Controls;
using VlcEngine = LibVLCSharp.Shared.LibVLC;

namespace Md3.Avalonia.LibVLC;

/// <summary>A desktop <see cref="MdVideoPlayer"/> backed by LibVLCSharp's Avalonia VideoView.</summary>
/// <remarks>
/// The host must deploy a compatible native LibVLC distribution, such as VideoLAN.LibVLC.Windows
/// or VideoLAN.LibVLC.Mac, or install LibVLC through the Linux package manager. The adapter does
/// not bundle native codecs and is not part of the core Android/NativeAOT dependency graph.
/// </remarks>
public sealed class MdLibVlcVideoPlayer : MdVideoPlayer, IDisposable
{
    public static readonly StyledProperty<Uri?> SourceProperty =
        AvaloniaProperty.Register<MdLibVlcVideoPlayer, Uri?>(nameof(Source));
    public static readonly StyledProperty<bool> AutoPlayProperty =
        AvaloniaProperty.Register<MdLibVlcVideoPlayer, bool>(nameof(AutoPlay));
    public static readonly StyledProperty<bool> LoopProperty =
        AvaloniaProperty.Register<MdLibVlcVideoPlayer, bool>(nameof(Loop));

    private readonly VideoView _videoView = new();
    private VlcEngine? _libVlc;
    private MediaPlayer? _mediaPlayer;
    private Media? _media;
    private bool _disposed;

    static MdLibVlcVideoPlayer()
    {
        SourceProperty.Changed.AddClassHandler<MdLibVlcVideoPlayer>((player, _) => player.OpenSource());
    }

    public MdLibVlcVideoPlayer()
    {
        MediaContent = _videoView;
        PlaybackRequested += OnPlaybackRequested;
    }

    protected override Type StyleKeyOverride => typeof(MdVideoPlayer);

    /// <summary>The file or network URI opened by LibVLC.</summary>
    public Uri? Source { get => GetValue(SourceProperty); set => SetValue(SourceProperty, value); }

    /// <summary>Whether a newly assigned source starts automatically.</summary>
    public bool AutoPlay { get => GetValue(AutoPlayProperty); set => SetValue(AutoPlayProperty, value); }

    /// <summary>Whether playback restarts after the media reaches its end.</summary>
    public bool Loop { get => GetValue(LoopProperty); set => SetValue(LoopProperty, value); }

    /// <summary>Arguments passed to LibVLC when the native engine is first created.</summary>
    public IList<string> LibVlcOptions { get; } = new List<string>();

    /// <summary>The lazily-created LibVLCSharp player, or <see langword="null"/> before loading.</summary>
    public MediaPlayer? MediaPlayer => _mediaPlayer;

    /// <summary>Reopens <see cref="Source"/> using the existing engine.</summary>
    /// <remarks>Create a new control when startup-only <see cref="LibVlcOptions"/> must change.</remarks>
    public void Reload()
    {
        ThrowIfDisposed();
        OpenSource();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        PlaybackRequested -= OnPlaybackRequested;
        DetachPlayerEvents();
        _videoView.MediaPlayer = null;
        _media?.Dispose();
        _media = null;
        _mediaPlayer?.Dispose();
        _mediaPlayer = null;
        _libVlc?.Dispose();
        _libVlc = null;
        MediaContent = null;
        GC.SuppressFinalize(this);
    }

    private void OpenSource()
    {
        if (_disposed) return;
        if (Source is null)
        {
            _mediaPlayer?.Stop();
            if (_mediaPlayer is not null) _mediaPlayer.Media = null;
            _media?.Dispose();
            _media = null;
            IsPlaying = false;
            IsBuffering = false;
            Position = TimeSpan.Zero;
            Duration = TimeSpan.Zero;
            BufferProgress = 0;
            IsSeekable = false;
            ErrorMessage = null;
            return;
        }
        try
        {
            EnsurePlayer();
            _media?.Dispose();
            _media = new Media(_libVlc!, Source);
            _mediaPlayer!.Media = _media;
            ErrorMessage = null;
            Position = TimeSpan.Zero;
            Duration = TimeSpan.Zero;
            BufferProgress = 0;
            IsSeekable = false;
            if (AutoPlay) _mediaPlayer.Play();
        }
        catch (Exception exception) when (exception is VLCException or DllNotFoundException or TypeInitializationException)
        {
            ErrorMessage = exception.Message;
            IsPlaying = false;
        }
    }

    private void EnsurePlayer()
    {
        ThrowIfDisposed();
        if (_mediaPlayer is not null) return;
        Core.Initialize();
        _libVlc = LibVlcOptions.Count == 0 ? new VlcEngine() : new VlcEngine(LibVlcOptions.ToArray());
        _mediaPlayer = new MediaPlayer(_libVlc)
        {
            Volume = (int)Math.Round(Math.Clamp(Volume, 0, 1) * 100),
            Mute = IsMuted
        };
        _mediaPlayer.SetRate((float)Math.Clamp(PlaybackRate, .25, 4));
        AttachPlayerEvents();
        _videoView.MediaPlayer = _mediaPlayer;
    }

    private void OnPlaybackRequested(object? sender, MdPlaybackRequestEventArgs e)
    {
        if (_disposed) return;
        try
        {
            if (e.Request == MdPlaybackRequest.Play)
            {
                EnsurePlayer();
                if (_mediaPlayer!.Media is null && Source is not null) OpenSource();
                _mediaPlayer.Play();
                return;
            }
            if (_mediaPlayer is null) return;
            switch (e.Request)
            {
                case MdPlaybackRequest.Pause:
                    _mediaPlayer.Pause();
                    break;
                case MdPlaybackRequest.Stop:
                    _mediaPlayer.Stop();
                    break;
                case MdPlaybackRequest.Seek:
                case MdPlaybackRequest.Rewind:
                case MdPlaybackRequest.Forward:
                    if (e.Position is { } position) _mediaPlayer.Time = Math.Max(0, (long)position.TotalMilliseconds);
                    break;
                case MdPlaybackRequest.Mute:
                    _mediaPlayer.Mute = true;
                    break;
                case MdPlaybackRequest.Unmute:
                    _mediaPlayer.Mute = false;
                    break;
                case MdPlaybackRequest.SetVolume:
                    if (e.Value is { } volume) _mediaPlayer.Volume = (int)Math.Round(Math.Clamp(volume, 0, 1) * 100);
                    break;
                case MdPlaybackRequest.SetPlaybackRate:
                    if (e.Value is { } rate) _mediaPlayer.SetRate((float)rate);
                    break;
            }
        }
        catch (Exception exception) when (exception is VLCException or ObjectDisposedException)
        {
            ErrorMessage = exception.Message;
        }
    }

    private void AttachPlayerEvents()
    {
        if (_mediaPlayer is null) return;
        _mediaPlayer.Playing += OnPlaying;
        _mediaPlayer.Paused += OnPaused;
        _mediaPlayer.Stopped += OnStopped;
        _mediaPlayer.EndReached += OnEndReached;
        _mediaPlayer.EncounteredError += OnEncounteredError;
        _mediaPlayer.TimeChanged += OnTimeChanged;
        _mediaPlayer.LengthChanged += OnLengthChanged;
        _mediaPlayer.Buffering += OnBuffering;
        _mediaPlayer.Muted += OnMuted;
        _mediaPlayer.Unmuted += OnUnmuted;
        _mediaPlayer.VolumeChanged += OnVolumeChanged;
        _mediaPlayer.SeekableChanged += OnSeekableChanged;
    }

    private void DetachPlayerEvents()
    {
        if (_mediaPlayer is null) return;
        _mediaPlayer.Playing -= OnPlaying;
        _mediaPlayer.Paused -= OnPaused;
        _mediaPlayer.Stopped -= OnStopped;
        _mediaPlayer.EndReached -= OnEndReached;
        _mediaPlayer.EncounteredError -= OnEncounteredError;
        _mediaPlayer.TimeChanged -= OnTimeChanged;
        _mediaPlayer.LengthChanged -= OnLengthChanged;
        _mediaPlayer.Buffering -= OnBuffering;
        _mediaPlayer.Muted -= OnMuted;
        _mediaPlayer.Unmuted -= OnUnmuted;
        _mediaPlayer.VolumeChanged -= OnVolumeChanged;
        _mediaPlayer.SeekableChanged -= OnSeekableChanged;
    }

    private void OnPlaying(object? sender, EventArgs e) => Dispatch(() =>
    {
        IsPlaying = true;
        IsBuffering = false;
        ErrorMessage = null;
    });

    private void OnPaused(object? sender, EventArgs e) => Dispatch(() => IsPlaying = false);

    private void OnStopped(object? sender, EventArgs e) => Dispatch(() =>
    {
        IsPlaying = false;
        IsBuffering = false;
    });

    private void OnEndReached(object? sender, EventArgs e) => Dispatch(() =>
    {
        IsPlaying = false;
        Position = Duration;
        if (Loop && _mediaPlayer is not null)
        {
            _mediaPlayer.Time = 0;
            _mediaPlayer.Play();
        }
    });

    private void OnEncounteredError(object? sender, EventArgs e) => Dispatch(() =>
    {
        IsPlaying = false;
        IsBuffering = false;
        ErrorMessage = "LibVLC could not play this media source.";
    });

    private void OnTimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e) =>
        Dispatch(() => Position = TimeSpan.FromMilliseconds(Math.Max(0, e.Time)));

    private void OnLengthChanged(object? sender, MediaPlayerLengthChangedEventArgs e) => Dispatch(() =>
    {
        Duration = TimeSpan.FromMilliseconds(Math.Max(0, e.Length));
        IsLive = e.Length <= 0;
    });

    private void OnBuffering(object? sender, MediaPlayerBufferingEventArgs e) => Dispatch(() =>
    {
        BufferProgress = Math.Clamp(e.Cache / 100d, 0, 1);
        IsBuffering = e.Cache < 100;
    });

    private void OnMuted(object? sender, EventArgs e) => Dispatch(() => IsMuted = true);
    private void OnUnmuted(object? sender, EventArgs e) => Dispatch(() => IsMuted = false);
    private void OnVolumeChanged(object? sender, MediaPlayerVolumeChangedEventArgs e) =>
        Dispatch(() => Volume = Math.Clamp(e.Volume / 100d, 0, 1));
    private void OnSeekableChanged(object? sender, MediaPlayerSeekableChangedEventArgs e) =>
        Dispatch(() => IsSeekable = e.Seekable != 0);

    private static void Dispatch(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess()) action();
        else Dispatcher.UIThread.Post(action);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
