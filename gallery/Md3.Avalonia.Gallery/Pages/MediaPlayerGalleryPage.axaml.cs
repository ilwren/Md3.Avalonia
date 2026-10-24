using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Md3.Avalonia.Media.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class MediaPlayerGalleryPage : UserControl
{
    private readonly PreviewPlaybackSession _session = new();

    public MediaPlayerGalleryPage()
    {
        InitializeComponent();
        Player.Session = _session;
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
            Child = new StackPanel
            {
                HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = "▶", FontSize = 44, Foreground = Brushes.White, HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center },
                    new TextBlock { Text = "Material Player Preview", FontSize = 18, Foreground = Brushes.White },
                    new TextBlock { Text = "Interactive controls · simulated timeline", FontSize = 12, Foreground = Brushes.LightGray, HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center }
                }
            }
        };
        Player.FullscreenRequested += (_, _) =>
        {
            if (TopLevel.GetTopLevel(this) is Window window)
                window.WindowState = window.WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
        };
        DetachedFromVisualTree += (_, _) => _session.PauseAsync();
    }

    private sealed class PreviewPlaybackSession : IVideoPlaybackSession
    {
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(250) };
        private TimeSpan _position = TimeSpan.FromSeconds(38);
        private double _volume = 0.8;

        public PreviewPlaybackSession() => _timer.Tick += (_, _) =>
        {
            Position = Position + TimeSpan.FromMilliseconds(250);
            if (Position >= Duration) { Position = TimeSpan.Zero; _ = PauseAsync(); }
        };

        public bool IsPlaying { get; private set; }
        public bool IsBuffering => false;
        public TimeSpan Position
        {
            get => _position;
            set { _position = value < TimeSpan.Zero ? TimeSpan.Zero : value > Duration ? Duration : value; PositionChanged?.Invoke(this, EventArgs.Empty); }
        }
        public TimeSpan Duration { get; } = TimeSpan.FromSeconds(154);
        public double Volume { get => _volume; set => _volume = Math.Clamp(value, 0, 1); }
        public bool IsMuted { get; set; }
        public event EventHandler? StateChanged;
        public event EventHandler? PositionChanged;

        public Task PlayAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IsPlaying = true;
            _timer.Start();
            StateChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public Task PauseAsync(CancellationToken cancellationToken = default)
        {
            IsPlaying = false;
            _timer.Stop();
            StateChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public void Dispose() => _timer.Stop();
    }
}
