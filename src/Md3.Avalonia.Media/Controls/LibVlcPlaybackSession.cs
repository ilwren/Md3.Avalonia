using LibVLCSharp.Shared;

namespace Md3.Avalonia.Media.Controls;

/// <summary>Adapts a LibVLCSharp media player to the player control's small session contract.</summary>
/// <remarks>
/// The LibVLC instance, MediaPlayer, and Media remain owned by the caller. Dispose this adapter to
/// detach its event handlers; dispose the native objects separately when the host is finished with them.
/// </remarks>
public sealed class LibVlcPlaybackSession : IVideoPlaybackSession
{
    private readonly MediaPlayer _player;
    private bool _disposed;

    /// <summary>Creates a session around an existing LibVLCSharp media player.</summary>
    public LibVlcPlaybackSession(MediaPlayer player)
    {
        _player = player ?? throw new ArgumentNullException(nameof(player));
        _player.Playing += OnStateChanged;
        _player.Paused += OnStateChanged;
        _player.Stopped += OnStateChanged;
        _player.TimeChanged += OnTimeChanged;
        _player.LengthChanged += OnLengthChanged;
    }

    /// <inheritdoc />
    public bool IsPlaying => _player.IsPlaying;
    /// <inheritdoc />
    public bool IsBuffering => false;
    /// <inheritdoc />
    public TimeSpan Position
    {
        get => TimeSpan.FromMilliseconds(Math.Max(0, _player.Time));
        set => _player.Time = (long)Math.Max(0, value.TotalMilliseconds);
    }
    /// <inheritdoc />
    public TimeSpan Duration => TimeSpan.FromMilliseconds(Math.Max(0, _player.Length));
    /// <inheritdoc />
    public double Volume
    {
        get => Math.Clamp(_player.Volume / 100.0, 0, 1);
        set => _player.Volume = (int)Math.Round(Math.Clamp(value, 0, 1) * 100);
    }
    /// <inheritdoc />
    public bool IsMuted { get => _player.Mute; set => _player.Mute = value; }

    /// <inheritdoc />
    public event EventHandler? StateChanged;
    /// <inheritdoc />
    public event EventHandler? PositionChanged;

    /// <inheritdoc />
    public Task PlayAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _player.Play();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task PauseAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _player.Pause();
        return Task.CompletedTask;
    }

    private void OnStateChanged(object? sender, EventArgs e) => StateChanged?.Invoke(this, EventArgs.Empty);
    private void OnTimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e) => PositionChanged?.Invoke(this, EventArgs.Empty);
    private void OnLengthChanged(object? sender, MediaPlayerLengthChangedEventArgs e) => StateChanged?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _player.Playing -= OnStateChanged;
        _player.Paused -= OnStateChanged;
        _player.Stopped -= OnStateChanged;
        _player.TimeChanged -= OnTimeChanged;
        _player.LengthChanged -= OnLengthChanged;
        _disposed = true;
    }
}
