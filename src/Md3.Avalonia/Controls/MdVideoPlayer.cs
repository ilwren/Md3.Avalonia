using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Md3.Avalonia.Localization;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A Material 3 video-player shell that presents a replaceable renderer and translates its
/// transport UI into backend-neutral playback requests.
/// </summary>
/// <remarks>
/// <see cref="MediaContent"/> can host a native or composited renderer. A backend synchronizes the
/// state properties and handles <see cref="PlaybackRequested"/>; the optional Md3.Avalonia.LibVLC
/// package supplies a desktop implementation. Keeping decoding out of the core control preserves
/// Android and NativeAOT compatibility for applications that use another media engine.
/// </remarks>
[TemplatePart("PART_PlayPauseButton", typeof(MdIconButton))]
[TemplatePart("PART_RewindButton", typeof(MdIconButton))]
[TemplatePart("PART_ForwardButton", typeof(MdIconButton))]
[TemplatePart("PART_Controls", typeof(Border))]
[TemplatePart("PART_MuteButton", typeof(MdIconButton))]
[TemplatePart("PART_FullScreenButton", typeof(MdIconButton))]
[TemplatePart("PART_SeekSlider", typeof(MdSlider))]
[TemplatePart("PART_VolumeSlider", typeof(MdSlider))]
[TemplatePart("PART_RateButton", typeof(MdButton))]
[PseudoClasses(":playing", ":paused", ":muted", ":buffering", ":live", ":full-screen", ":has-media", ":error", ":compact", ":controls-hidden", ":reduced-motion", ":no-motion")]
public class MdVideoPlayer : TemplatedControl
{
    public static readonly StyledProperty<object?> MediaContentProperty =
        AvaloniaProperty.Register<MdVideoPlayer, object?>(nameof(MediaContent));
    public static readonly StyledProperty<object?> PosterContentProperty =
        AvaloniaProperty.Register<MdVideoPlayer, object?>(nameof(PosterContent));
    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<MdVideoPlayer, object?>(nameof(Title));
    public static readonly StyledProperty<object?> SupportingTextProperty =
        AvaloniaProperty.Register<MdVideoPlayer, object?>(nameof(SupportingText));
    public static readonly StyledProperty<TimeSpan> PositionProperty =
        AvaloniaProperty.Register<MdVideoPlayer, TimeSpan>(nameof(Position));
    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<MdVideoPlayer, TimeSpan>(nameof(Duration));
    public static readonly StyledProperty<double> BufferProgressProperty =
        AvaloniaProperty.Register<MdVideoPlayer, double>(nameof(BufferProgress));
    public static readonly StyledProperty<double> VolumeProperty =
        AvaloniaProperty.Register<MdVideoPlayer, double>(nameof(Volume), 1d);
    public static readonly StyledProperty<double> PlaybackRateProperty =
        AvaloniaProperty.Register<MdVideoPlayer, double>(nameof(PlaybackRate), 1d);
    public static readonly StyledProperty<bool> IsPlayingProperty =
        AvaloniaProperty.Register<MdVideoPlayer, bool>(nameof(IsPlaying));
    public static readonly StyledProperty<bool> IsMutedProperty =
        AvaloniaProperty.Register<MdVideoPlayer, bool>(nameof(IsMuted));
    public static readonly StyledProperty<bool> IsBufferingProperty =
        AvaloniaProperty.Register<MdVideoPlayer, bool>(nameof(IsBuffering));
    public static readonly StyledProperty<bool> IsSeekableProperty =
        AvaloniaProperty.Register<MdVideoPlayer, bool>(nameof(IsSeekable), true);
    public static readonly StyledProperty<bool> IsLiveProperty =
        AvaloniaProperty.Register<MdVideoPlayer, bool>(nameof(IsLive));
    public static readonly StyledProperty<bool> IsFullScreenProperty =
        AvaloniaProperty.Register<MdVideoPlayer, bool>(nameof(IsFullScreen));
    public static readonly StyledProperty<bool> ShowControlsProperty =
        AvaloniaProperty.Register<MdVideoPlayer, bool>(nameof(ShowControls), true);
    public static readonly StyledProperty<bool> AutoHideControlsProperty =
        AvaloniaProperty.Register<MdVideoPlayer, bool>(nameof(AutoHideControls), true);
    public static readonly StyledProperty<TimeSpan> ControlsHideDelayProperty =
        AvaloniaProperty.Register<MdVideoPlayer, TimeSpan>(nameof(ControlsHideDelay), TimeSpan.FromSeconds(3));
    public static readonly StyledProperty<TimeSpan> SkipIntervalProperty =
        AvaloniaProperty.Register<MdVideoPlayer, TimeSpan>(nameof(SkipInterval), TimeSpan.FromSeconds(10));
    public static readonly StyledProperty<string?> ErrorMessageProperty =
        AvaloniaProperty.Register<MdVideoPlayer, string?>(nameof(ErrorMessage));

    public static readonly DirectProperty<MdVideoPlayer, string> PositionTextProperty =
        AvaloniaProperty.RegisterDirect<MdVideoPlayer, string>(nameof(PositionText), player => player.PositionText);
    public static readonly DirectProperty<MdVideoPlayer, string> DurationTextProperty =
        AvaloniaProperty.RegisterDirect<MdVideoPlayer, string>(nameof(DurationText), player => player.DurationText);
    public static readonly DirectProperty<MdVideoPlayer, string> RateTextProperty =
        AvaloniaProperty.RegisterDirect<MdVideoPlayer, string>(nameof(RateText), player => player.RateText);
    public static readonly DirectProperty<MdVideoPlayer, string> PlayPauseTextProperty =
        AvaloniaProperty.RegisterDirect<MdVideoPlayer, string>(nameof(PlayPauseText), player => player.PlayPauseText);
    public static readonly DirectProperty<MdVideoPlayer, string> RewindTextProperty =
        AvaloniaProperty.RegisterDirect<MdVideoPlayer, string>(nameof(RewindText), player => player.RewindText);
    public static readonly DirectProperty<MdVideoPlayer, string> ForwardTextProperty =
        AvaloniaProperty.RegisterDirect<MdVideoPlayer, string>(nameof(ForwardText), player => player.ForwardText);
    public static readonly DirectProperty<MdVideoPlayer, string> MuteTextProperty =
        AvaloniaProperty.RegisterDirect<MdVideoPlayer, string>(nameof(MuteText), player => player.MuteText);
    public static readonly DirectProperty<MdVideoPlayer, string> FullScreenTextProperty =
        AvaloniaProperty.RegisterDirect<MdVideoPlayer, string>(nameof(FullScreenText), player => player.FullScreenText);
    public static readonly DirectProperty<MdVideoPlayer, string> PlaybackErrorTextProperty =
        AvaloniaProperty.RegisterDirect<MdVideoPlayer, string>(nameof(PlaybackErrorText), player => player.PlaybackErrorText);
    public static readonly DirectProperty<MdVideoPlayer, string> LiveTextProperty =
        AvaloniaProperty.RegisterDirect<MdVideoPlayer, string>(nameof(LiveText), player => player.LiveText);

    private static readonly double[] PlaybackRates = [.5, .75, 1, 1.25, 1.5, 2];
    private readonly DispatcherTimer _controlsTimer;
    private Border? _controlsSurface;
    private MdIconButton? _playPauseButton;
    private MdIconButton? _rewindButton;
    private MdIconButton? _forwardButton;
    private MdIconButton? _muteButton;
    private MdIconButton? _fullScreenButton;
    private MdSlider? _seekSlider;
    private MdSlider? _volumeSlider;
    private MdButton? _rateButton;
    private bool _syncingTemplate;
    private bool _controlsVisible = true;
    private string _positionText = "0:00";
    private string _durationText = "0:00";
    private string _rateText = "1×";
    private string _playPauseText = "Play";
    private string _rewindText = "Rewind 10 seconds";
    private string _forwardText = "Forward 10 seconds";
    private string _muteText = "Mute";
    private string _fullScreenText = "Enter full screen";
    private string _playbackErrorText = "Playback error";
    private string _liveText = "Live";

    static MdVideoPlayer()
    {
        MediaContentProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.UpdateVisualState());
        PositionProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.UpdateTimeline());
        DurationProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.UpdateTimeline());
        BufferProgressProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.CoerceState());
        VolumeProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.OnVolumeChanged());
        PlaybackRateProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.OnRateChanged());
        IsPlayingProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.OnPlayingChanged());
        IsMutedProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.UpdateVisualState());
        IsBufferingProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.UpdateVisualState());
        IsLiveProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.UpdateVisualState());
        IsFullScreenProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.UpdateVisualState());
        ShowControlsProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.ShowControlsTemporarily());
        SkipIntervalProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.UpdateLocalizedText());
        ErrorMessageProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.UpdateVisualState());
        BoundsProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.UpdateVisualState());
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.UpdateLocalizedText());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdVideoPlayer>((player, _) => player.UpdateMotion());
    }

    public MdVideoPlayer()
    {
        Focusable = true;
        _controlsTimer = new DispatcherTimer { Interval = ControlsHideDelay };
        _controlsTimer.Tick += (_, _) => HideControls();
        DetachedFromVisualTree += (_, _) => _controlsTimer.Stop();
        UpdateTimeline();
        UpdateLocalizedText();
        UpdateVisualState();
    }

    public object? MediaContent { get => GetValue(MediaContentProperty); set => SetValue(MediaContentProperty, value); }
    public object? PosterContent { get => GetValue(PosterContentProperty); set => SetValue(PosterContentProperty, value); }
    public object? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public object? SupportingText { get => GetValue(SupportingTextProperty); set => SetValue(SupportingTextProperty, value); }
    public TimeSpan Position { get => GetValue(PositionProperty); set => SetValue(PositionProperty, value); }
    public TimeSpan Duration { get => GetValue(DurationProperty); set => SetValue(DurationProperty, value); }
    public double BufferProgress { get => GetValue(BufferProgressProperty); set => SetValue(BufferProgressProperty, value); }
    public double Volume { get => GetValue(VolumeProperty); set => SetValue(VolumeProperty, value); }
    public double PlaybackRate { get => GetValue(PlaybackRateProperty); set => SetValue(PlaybackRateProperty, value); }
    public bool IsPlaying { get => GetValue(IsPlayingProperty); set => SetValue(IsPlayingProperty, value); }
    public bool IsMuted { get => GetValue(IsMutedProperty); set => SetValue(IsMutedProperty, value); }
    public bool IsBuffering { get => GetValue(IsBufferingProperty); set => SetValue(IsBufferingProperty, value); }
    public bool IsSeekable { get => GetValue(IsSeekableProperty); set => SetValue(IsSeekableProperty, value); }
    public bool IsLive { get => GetValue(IsLiveProperty); set => SetValue(IsLiveProperty, value); }
    public bool IsFullScreen { get => GetValue(IsFullScreenProperty); set => SetValue(IsFullScreenProperty, value); }
    public bool ShowControls { get => GetValue(ShowControlsProperty); set => SetValue(ShowControlsProperty, value); }
    public bool AutoHideControls { get => GetValue(AutoHideControlsProperty); set => SetValue(AutoHideControlsProperty, value); }
    public TimeSpan ControlsHideDelay { get => GetValue(ControlsHideDelayProperty); set => SetValue(ControlsHideDelayProperty, value); }
    public TimeSpan SkipInterval { get => GetValue(SkipIntervalProperty); set => SetValue(SkipIntervalProperty, value); }
    public string? ErrorMessage { get => GetValue(ErrorMessageProperty); set => SetValue(ErrorMessageProperty, value); }

    public string PositionText => _positionText;
    public string DurationText => _durationText;
    public string RateText => _rateText;
    public string PlayPauseText => _playPauseText;
    public string RewindText => _rewindText;
    public string ForwardText => _forwardText;
    public string MuteText => _muteText;
    public string FullScreenText => _fullScreenText;
    public string PlaybackErrorText => _playbackErrorText;
    public string LiveText => _liveText;

    /// <summary>Raised whenever the Material transport asks the attached media backend to act.</summary>
    public event EventHandler<MdPlaybackRequestEventArgs>? PlaybackRequested;

    public void Play() => Request(MdPlaybackRequest.Play);
    public void Pause() => Request(MdPlaybackRequest.Pause);
    public void Stop() => Request(MdPlaybackRequest.Stop);
    public void TogglePlayback() => Request(IsPlaying ? MdPlaybackRequest.Pause : MdPlaybackRequest.Play);
    public void ToggleMute() => Request(IsMuted ? MdPlaybackRequest.Unmute : MdPlaybackRequest.Mute);
    public void ToggleFullScreen() => Request(IsFullScreen ? MdPlaybackRequest.ExitFullScreen : MdPlaybackRequest.EnterFullScreen);

    public void Seek(TimeSpan position)
    {
        if (!IsSeekable || IsLive) return;
        var duration = NonNegative(Duration);
        var target = NonNegative(position);
        if (duration > TimeSpan.Zero && target > duration) target = duration;
        SetCurrentValue(PositionProperty, target);
        PlaybackRequested?.Invoke(this, new MdPlaybackRequestEventArgs(MdPlaybackRequest.Seek, target));
        ShowControlsTemporarily();
    }

    public void SeekBy(TimeSpan offset)
    {
        if (!IsSeekable || IsLive) return;
        var request = offset < TimeSpan.Zero ? MdPlaybackRequest.Rewind : MdPlaybackRequest.Forward;
        var target = NonNegative(Position + offset);
        if (Duration > TimeSpan.Zero && target > Duration) target = Duration;
        SetCurrentValue(PositionProperty, target);
        PlaybackRequested?.Invoke(this, new MdPlaybackRequestEventArgs(request, target));
        ShowControlsTemporarily();
    }

    public void SetPlaybackVolume(double volume)
    {
        var target = Math.Clamp(volume, 0, 1);
        SetCurrentValue(VolumeProperty, target);
        PlaybackRequested?.Invoke(this, new MdPlaybackRequestEventArgs(MdPlaybackRequest.SetVolume, value: target));
    }

    public void SetPlaybackRate(double rate)
    {
        var target = Math.Clamp(rate, .25, 4);
        SetCurrentValue(PlaybackRateProperty, target);
        PlaybackRequested?.Invoke(this, new MdPlaybackRequestEventArgs(MdPlaybackRequest.SetPlaybackRate, value: target));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateParts();
        base.OnApplyTemplate(e);
        _controlsSurface = e.NameScope.Find<Border>("PART_Controls");
        _playPauseButton = e.NameScope.Find<MdIconButton>("PART_PlayPauseButton");
        _rewindButton = e.NameScope.Find<MdIconButton>("PART_RewindButton");
        _forwardButton = e.NameScope.Find<MdIconButton>("PART_ForwardButton");
        _muteButton = e.NameScope.Find<MdIconButton>("PART_MuteButton");
        _fullScreenButton = e.NameScope.Find<MdIconButton>("PART_FullScreenButton");
        _seekSlider = e.NameScope.Find<MdSlider>("PART_SeekSlider");
        _volumeSlider = e.NameScope.Find<MdSlider>("PART_VolumeSlider");
        _rateButton = e.NameScope.Find<MdButton>("PART_RateButton");
        if (_playPauseButton is not null) _playPauseButton.Click += OnPlayPauseClick;
        if (_rewindButton is not null) _rewindButton.Click += OnRewindClick;
        if (_forwardButton is not null) _forwardButton.Click += OnForwardClick;
        if (_muteButton is not null) _muteButton.Click += OnMuteClick;
        if (_fullScreenButton is not null) _fullScreenButton.Click += OnFullScreenClick;
        if (_seekSlider is not null) _seekSlider.PropertyChanged += OnSeekSliderChanged;
        if (_volumeSlider is not null) _volumeSlider.PropertyChanged += OnVolumeSliderChanged;
        if (_rateButton is not null) _rateButton.Click += OnRateClick;
        SyncTemplate();
        UpdateLocalizedText();
        UpdateMotion();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        ShowControlsTemporarily();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        ShowControlsTemporarily();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Space:
            case Key.K:
                TogglePlayback();
                break;
            case Key.Left:
                SeekBy(-SkipInterval);
                break;
            case Key.Right:
                SeekBy(SkipInterval);
                break;
            case Key.Up:
                SetPlaybackVolume(Volume + .05);
                break;
            case Key.Down:
                SetPlaybackVolume(Volume - .05);
                break;
            case Key.M:
                ToggleMute();
                break;
            case Key.F:
                ToggleFullScreen();
                break;
            default:
                base.OnKeyDown(e);
                return;
        }
        e.Handled = true;
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new MdVideoPlayerAutomationPeer(this);

    private void OnPlayPauseClick(object? sender, RoutedEventArgs e) => TogglePlayback();
    private void OnRewindClick(object? sender, RoutedEventArgs e) => SeekBy(-SkipInterval);
    private void OnForwardClick(object? sender, RoutedEventArgs e) => SeekBy(SkipInterval);
    private void OnMuteClick(object? sender, RoutedEventArgs e) => ToggleMute();
    private void OnFullScreenClick(object? sender, RoutedEventArgs e) => ToggleFullScreen();

    private void OnRateClick(object? sender, RoutedEventArgs e)
    {
        var current = Array.FindIndex(PlaybackRates, value => Math.Abs(value - PlaybackRate) < .001);
        SetPlaybackRate(PlaybackRates[(current + 1 + PlaybackRates.Length) % PlaybackRates.Length]);
    }

    private void OnSeekSliderChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_syncingTemplate || e.Property != RangeBase.ValueProperty || _seekSlider is null || Duration <= TimeSpan.Zero) return;
        Seek(TimeSpan.FromTicks((long)(Duration.Ticks * Math.Clamp(_seekSlider.Value / 1000d, 0, 1))));
    }

    private void OnVolumeSliderChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_syncingTemplate || e.Property != RangeBase.ValueProperty || _volumeSlider is null) return;
        SetPlaybackVolume(_volumeSlider.Value / 100d);
    }

    private void Request(MdPlaybackRequest request)
    {
        PlaybackRequested?.Invoke(this, new MdPlaybackRequestEventArgs(request));
        ShowControlsTemporarily();
    }

    private void OnPlayingChanged()
    {
        if (IsPlaying && AutoHideControls)
            RestartControlsTimer();
        else
        {
            _controlsTimer.Stop();
            _controlsVisible = true;
        }
        UpdateVisualState();
    }

    private void OnVolumeChanged()
    {
        CoerceState();
        SyncTemplate();
    }

    private void OnRateChanged()
    {
        CoerceState();
        SetAndRaise(RateTextProperty, ref _rateText, $"{PlaybackRate:0.##}×");
    }

    private void CoerceState()
    {
        var volume = Math.Clamp(Volume, 0, 1);
        if (Math.Abs(volume - Volume) > .0001) SetCurrentValue(VolumeProperty, volume);
        var buffer = Math.Clamp(BufferProgress, 0, 1);
        if (Math.Abs(buffer - BufferProgress) > .0001) SetCurrentValue(BufferProgressProperty, buffer);
        var rate = Math.Clamp(PlaybackRate, .25, 4);
        if (Math.Abs(rate - PlaybackRate) > .0001) SetCurrentValue(PlaybackRateProperty, rate);
        UpdateVisualState();
    }

    private void UpdateTimeline()
    {
        var duration = NonNegative(Duration);
        var position = NonNegative(Position);
        if (duration > TimeSpan.Zero && position > duration) position = duration;
        if (position != Position) SetCurrentValue(PositionProperty, position);
        SetAndRaise(PositionTextProperty, ref _positionText, FormatTime(position));
        SetAndRaise(DurationTextProperty, ref _durationText, FormatTime(duration));
        SyncTemplate();
    }

    private void SyncTemplate()
    {
        _syncingTemplate = true;
        try
        {
            if (_seekSlider is not null)
                _seekSlider.Value = Duration > TimeSpan.Zero
                    ? Math.Clamp(Position.TotalMilliseconds / Duration.TotalMilliseconds * 1000, 0, 1000)
                    : 0;
            if (_volumeSlider is not null)
                _volumeSlider.Value = Math.Clamp(Volume * 100, 0, 100);
        }
        finally
        {
            _syncingTemplate = false;
        }
    }

    private void ShowControlsTemporarily()
    {
        _controlsVisible = ShowControls;
        UpdateVisualState();
        if (IsPlaying && AutoHideControls && ShowControls)
            RestartControlsTimer();
    }

    private void RestartControlsTimer()
    {
        _controlsTimer.Stop();
        _controlsTimer.Interval = ControlsHideDelay <= TimeSpan.Zero ? TimeSpan.FromSeconds(3) : ControlsHideDelay;
        _controlsTimer.Start();
    }

    private void HideControls()
    {
        _controlsTimer.Stop();
        if (!IsPlaying || !AutoHideControls) return;
        _controlsVisible = false;
        UpdateVisualState();
    }

    private void UpdateLocalizedText()
    {
        SetAndRaise(PlayPauseTextProperty, ref _playPauseText,
            MdLocalization.GetString(IsPlaying ? "VideoPause" : "VideoPlay", this));
        var skipSeconds = Math.Max(1, (int)Math.Round(Math.Abs(SkipInterval.TotalSeconds)));
        SetAndRaise(RewindTextProperty, ref _rewindText,
            MdLocalization.Format("VideoRewindSeconds", this, skipSeconds));
        SetAndRaise(ForwardTextProperty, ref _forwardText,
            MdLocalization.Format("VideoForwardSeconds", this, skipSeconds));
        SetAndRaise(MuteTextProperty, ref _muteText,
            MdLocalization.GetString(IsMuted ? "VideoUnmute" : "VideoMute", this));
        SetAndRaise(FullScreenTextProperty, ref _fullScreenText,
            MdLocalization.GetString(IsFullScreen ? "VideoExitFullScreen" : "VideoEnterFullScreen", this));
        SetAndRaise(PlaybackErrorTextProperty, ref _playbackErrorText,
            MdLocalization.GetString("VideoPlaybackError", this));
        SetAndRaise(LiveTextProperty, ref _liveText, MdLocalization.GetString("VideoLive", this));
        if (_playPauseButton is not null) AutomationProperties.SetName(_playPauseButton, PlayPauseText);
        if (_rewindButton is not null) AutomationProperties.SetName(_rewindButton, RewindText);
        if (_forwardButton is not null) AutomationProperties.SetName(_forwardButton, ForwardText);
        if (_muteButton is not null) AutomationProperties.SetName(_muteButton, MuteText);
        if (_fullScreenButton is not null) AutomationProperties.SetName(_fullScreenButton, FullScreenText);
        if (_seekSlider is not null) AutomationProperties.SetName(_seekSlider, MdLocalization.GetString("VideoTimeline", this));
        if (_volumeSlider is not null) AutomationProperties.SetName(_volumeSlider, MdLocalization.GetString("VideoVolume", this));
        if (_rateButton is not null) AutomationProperties.SetName(_rateButton, MdLocalization.GetString("VideoPlaybackSpeed", this));
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_controlsSurface is not null)
            _controlsSurface.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(
                    this,
                    OpacityProperty,
                    MdMotionKind.Effects,
                    MdMotionSpeed.Fast));
    }

    private void UpdateVisualState()
    {
        PseudoClasses.Set(":playing", IsPlaying);
        PseudoClasses.Set(":paused", !IsPlaying);
        PseudoClasses.Set(":muted", IsMuted || Volume <= 0);
        PseudoClasses.Set(":buffering", IsBuffering);
        PseudoClasses.Set(":live", IsLive);
        PseudoClasses.Set(":full-screen", IsFullScreen);
        PseudoClasses.Set(":has-media", MediaContent is not null);
        PseudoClasses.Set(":error", !string.IsNullOrWhiteSpace(ErrorMessage));
        PseudoClasses.Set(":compact", Bounds.Width > 0 && Bounds.Width < 600);
        PseudoClasses.Set(":controls-hidden", !ShowControls || !_controlsVisible);
        UpdateLocalizedText();
    }

    private void DetachTemplateParts()
    {
        if (_playPauseButton is not null) _playPauseButton.Click -= OnPlayPauseClick;
        if (_rewindButton is not null) _rewindButton.Click -= OnRewindClick;
        if (_forwardButton is not null) _forwardButton.Click -= OnForwardClick;
        if (_muteButton is not null) _muteButton.Click -= OnMuteClick;
        if (_fullScreenButton is not null) _fullScreenButton.Click -= OnFullScreenClick;
        if (_seekSlider is not null) _seekSlider.PropertyChanged -= OnSeekSliderChanged;
        if (_volumeSlider is not null) _volumeSlider.PropertyChanged -= OnVolumeSliderChanged;
        if (_rateButton is not null) _rateButton.Click -= OnRateClick;
        _controlsSurface = null;
        _playPauseButton = null;
        _rewindButton = null;
        _forwardButton = null;
        _muteButton = null;
        _fullScreenButton = null;
        _seekSlider = null;
        _volumeSlider = null;
        _rateButton = null;
    }

    private static TimeSpan NonNegative(TimeSpan value) => value < TimeSpan.Zero ? TimeSpan.Zero : value;

    private static string FormatTime(TimeSpan value) => value.TotalHours >= 1
        ? $"{(int)value.TotalHours}:{value.Minutes:00}:{value.Seconds:00}"
        : $"{(int)value.TotalMinutes}:{value.Seconds:00}";
}

internal sealed class MdVideoPlayerAutomationPeer(MdVideoPlayer owner) : ControlAutomationPeer(owner)
{
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Pane;
    protected override bool IsControlElementCore() => true;
    protected override bool IsContentElementCore() => true;
    protected override string? GetNameCore() =>
        AutomationProperties.GetName(Owner) ?? ((MdVideoPlayer)Owner).Title?.ToString() ?? "Video player";
}
