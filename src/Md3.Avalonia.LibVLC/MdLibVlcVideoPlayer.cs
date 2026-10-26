using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using Md3.Avalonia.Controls;
using VlcEngine = LibVLCSharp.Shared.LibVLC;

namespace Md3.Avalonia.LibVLC;

/// <summary>A desktop <see cref="MdVideoPlayer"/> backed by LibVLCSharp and a composited Avalonia frame surface.</summary>
/// <remarks>
/// The host must deploy a compatible native LibVLC distribution, such as VideoLAN.LibVLC.Windows
/// or VideoLAN.LibVLC.Mac, or install LibVLC through the Linux package manager. Decoded frames are
/// copied into a writeable bitmap instead of a native child window, so the player clips and scrolls
/// with the Avalonia visual tree without native-airspace flashing. The adapter does not bundle
/// native codecs and is not part of the core Android/NativeAOT dependency graph.
/// </remarks>
public sealed class MdLibVlcVideoPlayer : MdVideoPlayer, IDisposable
{
    public static readonly StyledProperty<Uri?> SourceProperty =
        AvaloniaProperty.Register<MdLibVlcVideoPlayer, Uri?>(nameof(Source));
    public static readonly StyledProperty<bool> AutoPlayProperty =
        AvaloniaProperty.Register<MdLibVlcVideoPlayer, bool>(nameof(AutoPlay));
    public static readonly StyledProperty<bool> LoopProperty =
        AvaloniaProperty.Register<MdLibVlcVideoPlayer, bool>(nameof(Loop));

    private readonly Image _videoSurface = new()
    {
        Stretch = Stretch.Uniform,
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
    };
    private readonly object _videoSync = new();
    private VlcEngine? _libVlc;
    private MediaPlayer? _mediaPlayer;
    private Media? _media;
    private WriteableBitmap? _videoBitmap;
    private IntPtr _videoBuffer;
    private byte[]? _latestFrame;
    private int _videoWidth;
    private int _videoHeight;
    private int _videoPitch;
    private long _videoGeneration;
    private bool _renderPending;
    private bool _disposed;

    static MdLibVlcVideoPlayer()
    {
        SourceProperty.Changed.AddClassHandler<MdLibVlcVideoPlayer>((player, _) => player.OpenSource());
    }

    public MdLibVlcVideoPlayer()
    {
        MediaContent = _videoSurface;
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
        _mediaPlayer?.Stop();
        if (_mediaPlayer is not null) _mediaPlayer.Media = null;
        _media?.Dispose();
        _media = null;
        _mediaPlayer?.Dispose();
        _mediaPlayer = null;
        _libVlc?.Dispose();
        _libVlc = null;
        ReleaseVideoBuffer();
        ResetVideoSurface();
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
            ReleaseVideoBuffer();
            ResetVideoSurface();
            return;
        }
        try
        {
            EnsurePlayer();
            _mediaPlayer!.Stop();
            _mediaPlayer.Media = null;
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
        _mediaPlayer.SetVideoFormatCallbacks(OnVideoFormat, OnVideoCleanup);
        _mediaPlayer.SetVideoCallbacks(OnVideoLock, null, OnVideoDisplay);
        AttachPlayerEvents();
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
        catch (Exception exception) when (exception is VLCException or DllNotFoundException or TypeInitializationException or ObjectDisposedException)
        {
            ErrorMessage = exception.Message;
            IsPlaying = false;
        }
    }

    private uint OnVideoFormat(
        ref IntPtr opaque,
        IntPtr chroma,
        ref uint width,
        ref uint height,
        ref uint pitches,
        ref uint lines)
    {
        try
        {
            if (_disposed || chroma == IntPtr.Zero || width == 0 || height == 0) return 0;

            var pitch = ((ulong)width * 4 + 31) & ~31UL;
            var lineCount = ((ulong)height + 31) & ~31UL;
            var allocationSize = pitch * lineCount;
            var visibleSize = pitch * height;
            if (pitch > int.MaxValue || lineCount > uint.MaxValue ||
                allocationSize > int.MaxValue || visibleSize > int.MaxValue)
                return 0;

            // RV32 is BGRX on the little-endian desktop platforms supported by this adapter.
            Marshal.WriteByte(chroma, 0, (byte)'R');
            Marshal.WriteByte(chroma, 1, (byte)'V');
            Marshal.WriteByte(chroma, 2, (byte)'3');
            Marshal.WriteByte(chroma, 3, (byte)'2');

            lock (_videoSync)
            {
                ReleaseVideoBufferLocked();
                _latestFrame = null;
                _videoWidth = 0;
                _videoHeight = 0;
                _videoPitch = 0;
                _renderPending = false;
                _videoGeneration++;

                _videoBuffer = Marshal.AllocHGlobal((int)allocationSize);
                _latestFrame = new byte[(int)visibleSize];
                _videoWidth = checked((int)width);
                _videoHeight = checked((int)height);
                _videoPitch = (int)pitch;
            }

            opaque = IntPtr.Zero;
            pitches = (uint)pitch;
            lines = (uint)lineCount;
            return 1;
        }
        catch (Exception exception)
        {
            ReleaseVideoBuffer();
            ReportVideoRenderingError(exception);
            return 0;
        }
    }

    private IntPtr OnVideoLock(IntPtr opaque, IntPtr planes)
    {
        lock (_videoSync)
        {
            if (_disposed || _videoBuffer == IntPtr.Zero || planes == IntPtr.Zero) return IntPtr.Zero;
            Marshal.WriteIntPtr(planes, _videoBuffer);
            return IntPtr.Zero;
        }
    }

    private void OnVideoDisplay(IntPtr opaque, IntPtr picture)
    {
        try
        {
            long generation;
            lock (_videoSync)
            {
                if (_disposed || _videoBuffer == IntPtr.Zero || _latestFrame is null) return;
                Marshal.Copy(_videoBuffer, _latestFrame, 0, _latestFrame.Length);
                if (_renderPending) return;
                _renderPending = true;
                generation = _videoGeneration;
            }

            Dispatcher.UIThread.Post(() => RenderLatestFrame(generation), DispatcherPriority.Render);
        }
        catch (Exception exception)
        {
            ReportVideoRenderingError(exception);
        }
    }

    private void OnVideoCleanup(ref IntPtr opaque)
    {
        ReleaseVideoBuffer();
        opaque = IntPtr.Zero;
    }

    private void RenderLatestFrame(long generation)
    {
        try
        {
            lock (_videoSync)
            {
                if (_disposed || generation != _videoGeneration || _latestFrame is null ||
                    _videoWidth <= 0 || _videoHeight <= 0 || _videoPitch <= 0)
                    return;

                if (_videoBitmap?.PixelSize != new PixelSize(_videoWidth, _videoHeight))
                {
                    _videoSurface.Source = null;
                    _videoBitmap?.Dispose();
                    _videoBitmap = new WriteableBitmap(
                        new PixelSize(_videoWidth, _videoHeight),
                        new Vector(96, 96),
                        PixelFormat.Bgra8888,
                        AlphaFormat.Opaque);
                    _videoSurface.Source = _videoBitmap;
                }

                using var framebuffer = _videoBitmap.Lock();
                var rowBytes = checked(_videoWidth * 4);
                for (var row = 0; row < _videoHeight; row++)
                {
                    Marshal.Copy(
                        _latestFrame,
                        row * _videoPitch,
                        IntPtr.Add(framebuffer.Address, row * framebuffer.RowBytes),
                        rowBytes);
                }
                _videoSurface.InvalidateVisual();
                _renderPending = false;
            }
        }
        catch (Exception exception)
        {
            lock (_videoSync)
            {
                if (generation == _videoGeneration) _renderPending = false;
            }
            ReportVideoRenderingError(exception);
        }
    }

    private void ReleaseVideoBuffer()
    {
        lock (_videoSync)
        {
            ReleaseVideoBufferLocked();
            _latestFrame = null;
            _videoWidth = 0;
            _videoHeight = 0;
            _videoPitch = 0;
            _renderPending = false;
            _videoGeneration++;
        }
    }

    private void ReleaseVideoBufferLocked()
    {
        if (_videoBuffer == IntPtr.Zero) return;
        Marshal.FreeHGlobal(_videoBuffer);
        _videoBuffer = IntPtr.Zero;
    }

    private void ResetVideoSurface()
    {
        void Reset()
        {
            _videoSurface.Source = null;
            _videoBitmap?.Dispose();
            _videoBitmap = null;
        }

        if (Dispatcher.UIThread.CheckAccess()) Reset();
        else Dispatcher.UIThread.Post(Reset, DispatcherPriority.Render);
    }

    private void ReportVideoRenderingError(Exception exception) => Dispatch(() =>
    {
        ErrorMessage = $"LibVLC could not render the decoded video frame: {exception.Message}";
        IsPlaying = false;
    });

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
