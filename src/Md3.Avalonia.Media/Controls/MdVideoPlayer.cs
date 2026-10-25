using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Animation;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using LibVLCSharp.Shared;

namespace Md3.Avalonia.Media.Controls;

/// <summary>Playback state and operations consumed by <see cref="MdVideoPlayer"/>.</summary>
public interface IVideoPlaybackSession : IDisposable
{
    bool IsPlaying { get; }
    bool IsBuffering { get; }
    TimeSpan Position { get; set; }
    TimeSpan Duration { get; }
    double Volume { get; set; }
    bool IsMuted { get; set; }
    event EventHandler? StateChanged;
    event EventHandler? PositionChanged;
    Task PlayAsync(CancellationToken cancellationToken = default);
    Task PauseAsync(CancellationToken cancellationToken = default);
}

/// <summary>A skinnable video player shell with transport, seeking, volume, and fullscreen controls.</summary>
/// <remarks>
/// The control offers a backend-neutral <see cref="IVideoPlaybackSession"/> contract and a built-in
/// LibVLCSharp adapter. Use <see cref="AttachLibVlc(MediaPlayer)"/> for an Avalonia-rendered software
/// surface that allows this control's chrome to stay above the video, or assign another backend through
/// <see cref="Session"/> and <see cref="VideoContent"/>.
/// </remarks>
public sealed class MdVideoPlayer : UserControl
{
    private readonly ContentControl _videoHost;
    private readonly Button _playButton;
    private readonly Slider _seek;
    private readonly Slider _volume;
    private readonly TextBlock _time;
    private readonly Border _controls;
    private DispatcherTimer? _timer;
    private bool _updatingSeek;

    public MdVideoPlayer()
    {
        _videoHost = new ContentControl { HorizontalContentAlignment = HorizontalAlignment.Stretch, VerticalContentAlignment = VerticalAlignment.Stretch };
        _playButton = Button("▶", "Play or pause");
        _seek = new Slider { Minimum = 0, Maximum = 1000, VerticalAlignment = VerticalAlignment.Center, MinWidth = 80 };
        _volume = new Slider { Minimum = 0, Maximum = 100, Value = 100, Width = 90, VerticalAlignment = VerticalAlignment.Center };
        _time = new TextBlock { Text = "00:00 / 00:00", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 };
        var muteButton = Button("◖", "Mute or unmute");
        muteButton.Click += (_, _) => { if (Session is { } session) session.IsMuted = !session.IsMuted; };
        var full = Button("⛶", "Toggle fullscreen");
        _controls = new Border
        {
            Padding = new Thickness(12, 8),
            Background = new SolidColorBrush(Color.Parse("#D9101010")),
            Child = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    _seek,
                    new DockPanel
                    {
                        LastChildFill = false,
                        Children =
                        {
                            AddDocked(_playButton, global::Avalonia.Controls.Dock.Left), AddDocked(_time, global::Avalonia.Controls.Dock.Left),
                            AddDocked(muteButton, global::Avalonia.Controls.Dock.Right), AddDocked(_volume, global::Avalonia.Controls.Dock.Right), AddDocked(full, global::Avalonia.Controls.Dock.Right)
                        }
                    }
                }
            }
        };
        var layout = new Grid();
        layout.Children.Add(_videoHost);
        var chrome = new Grid { VerticalAlignment = VerticalAlignment.Bottom };
        chrome.Children.Add(_controls);
        layout.Children.Add(chrome);
        Background = Brushes.Black;
        Content = layout;

        _playButton.Click += async (_, _) => await TogglePlaybackAsync();
        _seek.PointerPressed += (_, _) => _updatingSeek = true;
        _seek.PointerReleased += (_, _) => { SeekToSlider(); _updatingSeek = false; };
        _seek.PropertyChanged += (_, e) => { if (e.Property == RangeBase.ValueProperty && _updatingSeek) SeekToSlider(); };
        _volume.PropertyChanged += (_, e) => { if (e.Property == RangeBase.ValueProperty && Session is { } s) s.Volume = _volume.Value / 100.0; };
        full.Click += (_, _) => ToggleFullscreen();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += (_, _) => RefreshState();
        _timer.Start();
        AttachedToVisualTree += (_, _) => _timer?.Start();
        DetachedFromVisualTree += (_, _) => _timer?.Stop();
    }

    /// <summary>Gets or sets the playback session. The control does not dispose a session assigned by its owner.</summary>
    public IVideoPlaybackSession? Session { get; set; }

    /// <summary>Gets or sets the backend's video rendering control.</summary>
    public Control? VideoContent { get => _videoHost.Content as Control; set => _videoHost.Content = value; }

    /// <summary>Connects the control directly to LibVLCSharp and creates its Avalonia video surface.</summary>
    /// <param name="mediaPlayer">An initialized LibVLCSharp player with media assigned.</param>
    /// <returns>The created Avalonia-composited video surface, also available through <see cref="VideoContent"/>.</returns>
    public LibVlcVideoSurface AttachLibVlc(MediaPlayer mediaPlayer)
    {
        ArgumentNullException.ThrowIfNull(mediaPlayer);
        var view = new LibVlcVideoSurface(mediaPlayer);
        VideoContent = view;
        Session = new LibVlcPlaybackSession(mediaPlayer);
        return view;
    }

    /// <summary>Gets or sets whether the controls overlay is shown.</summary>
    public bool AreControlsVisible { get => _controls.IsVisible; set => _controls.IsVisible = value; }

    /// <summary>Raised when the fullscreen button is pressed; the host window decides how fullscreen is implemented.</summary>
    public event EventHandler? FullscreenRequested;

    private static Button Button(string text, string tooltip)
    {
        var button = new Button
        {
            Content = text, Padding = new Thickness(10, 4),
            Background = Brushes.Transparent, Foreground = Brushes.White, BorderThickness = new Thickness(0),
            Transitions = new Transitions
            {
                new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(140) }
            }
        };
        button.PointerEntered += (_, _) => button.Opacity = 0.82;
        button.PointerExited += (_, _) => button.Opacity = 1;
        button.PointerPressed += (_, _) => button.Opacity = 0.58;
        button.PointerReleased += (_, _) => button.Opacity = 0.82;
        button.PointerCaptureLost += (_, _) => button.Opacity = 1;
        ToolTip.SetTip(button, tooltip);
        return button;
    }

    private static Control AddDocked(Control child, global::Avalonia.Controls.Dock dock)
    {
        DockPanel.SetDock(child, dock);
        return child;
    }

    private async Task TogglePlaybackAsync()
    {
        if (Session is not { } session) return;
        if (session.IsPlaying) await session.PauseAsync(); else await session.PlayAsync();
        RefreshState();
    }

    private void SeekToSlider()
    {
        if (Session is { Duration.Ticks: > 0 } session)
            session.Position = TimeSpan.FromTicks((long)(session.Duration.Ticks * _seek.Value / 1000));
    }

    private void RefreshState()
    {
        var s = Session;
        if (s is null) { _time.Text = "00:00 / 00:00"; return; }
        _playButton.Content = s.IsPlaying ? "Ⅱ" : "▶";
        if (!_updatingSeek)
        {
            _seek.Value = s.Duration.Ticks == 0 ? 0 : Math.Clamp(s.Position.Ticks * 1000.0 / s.Duration.Ticks, 0, 1000);
            _time.Text = $"{Format(s.Position)} / {Format(s.Duration)}";
        }
        if (Math.Abs(_volume.Value - s.Volume * 100) > 1) _volume.Value = Math.Clamp(s.Volume * 100, 0, 100);
    }

    private static string Format(TimeSpan value) => value.TotalHours >= 1 ? value.ToString(@"h\:mm\:ss") : value.ToString(@"m\:ss");
    private void ToggleFullscreen() => FullscreenRequested?.Invoke(this, EventArgs.Empty);
}
