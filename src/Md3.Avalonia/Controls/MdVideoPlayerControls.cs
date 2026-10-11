using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

/// <summary>
/// An overlay for video surfaces with transport, mute, fullscreen and seek affordances.
/// This control raises requests only; the host supplies the video surface and playback engine.
/// </summary>
[PseudoClasses(":playing", ":paused", ":busy", ":controls-hidden", ":full-screen")]
public sealed class MdVideoPlayerControls : TemplatedControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<MdVideoPlayerControls, string?>(nameof(Title));
    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<MdVideoPlayerControls, TimeSpan>(nameof(Duration));
    public static readonly StyledProperty<TimeSpan> PositionProperty =
        AvaloniaProperty.Register<MdVideoPlayerControls, TimeSpan>(nameof(Position),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<TimeSpan> BufferedPositionProperty =
        AvaloniaProperty.Register<MdVideoPlayerControls, TimeSpan>(nameof(BufferedPosition));
    public static readonly StyledProperty<bool> IsPlayingProperty =
        AvaloniaProperty.Register<MdVideoPlayerControls, bool>(nameof(IsPlaying));
    public static readonly StyledProperty<bool> IsBufferingProperty =
        AvaloniaProperty.Register<MdVideoPlayerControls, bool>(nameof(IsBuffering));
    public static readonly StyledProperty<bool> IsMutedProperty =
        AvaloniaProperty.Register<MdVideoPlayerControls, bool>(nameof(IsMuted),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> IsFullScreenProperty =
        AvaloniaProperty.Register<MdVideoPlayerControls, bool>(nameof(IsFullScreen),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> CanPreviousProperty =
        AvaloniaProperty.Register<MdVideoPlayerControls, bool>(nameof(CanPrevious), true);
    public static readonly StyledProperty<bool> CanNextProperty =
        AvaloniaProperty.Register<MdVideoPlayerControls, bool>(nameof(CanNext), true);
    public static readonly StyledProperty<bool> AutoHideControlsProperty =
        AvaloniaProperty.Register<MdVideoPlayerControls, bool>(nameof(AutoHideControls), true);
    public static readonly StyledProperty<TimeSpan> AutoHideDelayProperty =
        AvaloniaProperty.Register<MdVideoPlayerControls, TimeSpan>(nameof(AutoHideDelay), TimeSpan.FromSeconds(3));
    public static readonly StyledProperty<bool> IsControlsVisibleProperty =
        AvaloniaProperty.Register<MdVideoPlayerControls, bool>(nameof(IsControlsVisible), true,
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<ICommand?> PlaybackCommandProperty =
        AvaloniaProperty.Register<MdVideoPlayerControls, ICommand?>(nameof(PlaybackCommand));
    public static readonly StyledProperty<ICommand?> SeekCommandProperty =
        AvaloniaProperty.Register<MdVideoPlayerControls, ICommand?>(nameof(SeekCommand));

    private readonly DispatcherTimer _autoHideTimer;
    private MdSeekBar? _seekBar;
    private MdIconButton? _previousButton;
    private MdIconButton? _rewindButton;
    private MdIconButton? _playPauseButton;
    private MdIconButton? _forwardButton;
    private MdIconButton? _nextButton;
    private MdIconButton? _muteButton;
    private MdIconButton? _fullScreenButton;
    private MdCircularProgressIndicator? _busyIndicator;

    static MdVideoPlayerControls()
    {
        TitleProperty.Changed.AddClassHandler<MdVideoPlayerControls>((control, _) => control.UpdateState());
        IsPlayingProperty.Changed.AddClassHandler<MdVideoPlayerControls>((control, _) => control.UpdateState());
        IsBufferingProperty.Changed.AddClassHandler<MdVideoPlayerControls>((control, _) => control.UpdateState());
        IsMutedProperty.Changed.AddClassHandler<MdVideoPlayerControls>((control, _) => control.UpdateState());
        IsFullScreenProperty.Changed.AddClassHandler<MdVideoPlayerControls>((control, _) => control.UpdateState());
        CanPreviousProperty.Changed.AddClassHandler<MdVideoPlayerControls>((control, _) => control.UpdateState());
        CanNextProperty.Changed.AddClassHandler<MdVideoPlayerControls>((control, _) => control.UpdateState());
        AutoHideControlsProperty.Changed.AddClassHandler<MdVideoPlayerControls>((control, _) => control.UpdateAutoHideTimer());
        AutoHideDelayProperty.Changed.AddClassHandler<MdVideoPlayerControls>((control, _) => control.UpdateAutoHideTimer());
        IsControlsVisibleProperty.Changed.AddClassHandler<MdVideoPlayerControls>((control, _) =>
        {
            if (!control.IsControlsVisible && control.IsKeyboardFocusWithin)
            {
                control.ShowControls();
                return;
            }
            control.UpdatePseudoClasses();
            control.UpdateAutoHideTimer();
        });
        IsEnabledProperty.Changed.AddClassHandler<MdVideoPlayerControls>((control, _) => control.UpdateState());
        IsKeyboardFocusWithinProperty.Changed.AddClassHandler<MdVideoPlayerControls>((control, _) =>
        {
            if (control.IsKeyboardFocusWithin) control.ShowControls();
            control.UpdateAutoHideTimer();
        });
    }

    public MdVideoPlayerControls()
    {
        _autoHideTimer = new DispatcherTimer();
        _autoHideTimer.Tick += OnAutoHideTimerTick;
        AddHandler(PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel, handledEventsToo: true);
        UpdateState();
    }

    /// <summary>Optional title displayed above the video controls.</summary>
    public string? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public TimeSpan Duration { get => GetValue(DurationProperty); set => SetValue(DurationProperty, value); }
    public TimeSpan Position { get => GetValue(PositionProperty); set => SetValue(PositionProperty, value); }
    public TimeSpan BufferedPosition { get => GetValue(BufferedPositionProperty); set => SetValue(BufferedPositionProperty, value); }
    public bool IsPlaying { get => GetValue(IsPlayingProperty); set => SetValue(IsPlayingProperty, value); }
    public bool IsBuffering { get => GetValue(IsBufferingProperty); set => SetValue(IsBufferingProperty, value); }
    public bool IsMuted { get => GetValue(IsMutedProperty); set => SetValue(IsMutedProperty, value); }
    public bool IsFullScreen { get => GetValue(IsFullScreenProperty); set => SetValue(IsFullScreenProperty, value); }
    public bool CanPrevious { get => GetValue(CanPreviousProperty); set => SetValue(CanPreviousProperty, value); }
    public bool CanNext { get => GetValue(CanNextProperty); set => SetValue(CanNextProperty, value); }
    /// <summary>Automatically hides the overlay after inactivity while media is playing.</summary>
    public bool AutoHideControls { get => GetValue(AutoHideControlsProperty); set => SetValue(AutoHideControlsProperty, value); }
    /// <summary>Inactivity delay before controls are hidden. The default is three seconds.</summary>
    public TimeSpan AutoHideDelay { get => GetValue(AutoHideDelayProperty); set => SetValue(AutoHideDelayProperty, value); }
    public bool IsControlsVisible { get => GetValue(IsControlsVisibleProperty); set => SetValue(IsControlsVisibleProperty, value); }
    /// <summary>Optional command receiving <see cref="MdPlaybackRequest"/> values.</summary>
    public ICommand? PlaybackCommand { get => GetValue(PlaybackCommandProperty); set => SetValue(PlaybackCommandProperty, value); }
    /// <summary>Optional command receiving a target <see cref="TimeSpan"/> when seeking.</summary>
    public ICommand? SeekCommand { get => GetValue(SeekCommandProperty); set => SetValue(SeekCommandProperty, value); }

    /// <summary>Raised for play, pause, skip, mute and fullscreen requests.</summary>
    public event EventHandler<MdPlaybackRequestEventArgs>? PlaybackRequested;
    /// <summary>Raised when a seek is committed.</summary>
    public event EventHandler<MdSeekRequestedEventArgs>? SeekRequested;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachParts();
        base.OnApplyTemplate(e);
        _seekBar = e.NameScope.Find<MdSeekBar>("PART_SeekBar");
        _previousButton = e.NameScope.Find<MdIconButton>("PART_PreviousButton");
        _rewindButton = e.NameScope.Find<MdIconButton>("PART_RewindButton");
        _playPauseButton = e.NameScope.Find<MdIconButton>("PART_PlayPauseButton");
        _forwardButton = e.NameScope.Find<MdIconButton>("PART_ForwardButton");
        _nextButton = e.NameScope.Find<MdIconButton>("PART_NextButton");
        _muteButton = e.NameScope.Find<MdIconButton>("PART_MuteButton");
        _fullScreenButton = e.NameScope.Find<MdIconButton>("PART_FullScreenButton");
        _busyIndicator = e.NameScope.Find<MdCircularProgressIndicator>("PART_BusyIndicator");

        if (_seekBar is not null) _seekBar.SeekRequested += OnSeekRequested;
        if (_previousButton is not null) _previousButton.Click += OnPreviousClick;
        if (_rewindButton is not null) _rewindButton.Click += OnRewindClick;
        if (_playPauseButton is not null) _playPauseButton.Click += OnPlayPauseClick;
        if (_forwardButton is not null) _forwardButton.Click += OnForwardClick;
        if (_nextButton is not null) _nextButton.Click += OnNextClick;
        if (_muteButton is not null) _muteButton.Click += OnMuteClick;
        if (_fullScreenButton is not null) _fullScreenButton.Click += OnFullScreenClick;
        UpdateState();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateAutoHideTimer();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _autoHideTimer.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private void DetachParts()
    {
        if (_seekBar is not null) _seekBar.SeekRequested -= OnSeekRequested;
        if (_previousButton is not null) _previousButton.Click -= OnPreviousClick;
        if (_rewindButton is not null) _rewindButton.Click -= OnRewindClick;
        if (_playPauseButton is not null) _playPauseButton.Click -= OnPlayPauseClick;
        if (_forwardButton is not null) _forwardButton.Click -= OnForwardClick;
        if (_nextButton is not null) _nextButton.Click -= OnNextClick;
        if (_muteButton is not null) _muteButton.Click -= OnMuteClick;
        if (_fullScreenButton is not null) _fullScreenButton.Click -= OnFullScreenClick;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        ShowControls();
        UpdateAutoHideTimer();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsControlsVisible)
        {
            ShowControls();
            e.Handled = true;
            return;
        }
        if (MdMediaControlHelpers.IsInsideInteractiveControl(e.Source, this))
        {
            ShowControls();
            UpdateAutoHideTimer();
            return;
        }
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        if (IsPlaying && IsControlsVisible)
            SetCurrentValue(IsControlsVisibleProperty, false);
        else
            ShowControls();
        e.Handled = true;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (MdMediaControlHelpers.IsInsideInteractiveControl(e.Source, this)) return;
        ShowControls();
        var request = e.Key switch
        {
            Key.Space or Key.Enter => IsPlaying ? MdPlaybackRequest.Pause : MdPlaybackRequest.Play,
            Key.Left => MdPlaybackRequest.Rewind,
            Key.Right => MdPlaybackRequest.Forward,
            Key.M => IsMuted ? MdPlaybackRequest.Unmute : MdPlaybackRequest.Mute,
            Key.F => IsFullScreen ? MdPlaybackRequest.ExitFullScreen : MdPlaybackRequest.EnterFullScreen,
            Key.Escape when IsFullScreen => MdPlaybackRequest.ExitFullScreen,
            _ => (MdPlaybackRequest?)null
        };
        if (request is null) return;
        if (request is MdPlaybackRequest.Play or MdPlaybackRequest.Pause)
            SetCurrentValue(IsPlayingProperty, request == MdPlaybackRequest.Play);
        if (request is MdPlaybackRequest.Mute or MdPlaybackRequest.Unmute)
            SetCurrentValue(IsMutedProperty, request == MdPlaybackRequest.Mute);
        if (request is MdPlaybackRequest.EnterFullScreen or MdPlaybackRequest.ExitFullScreen)
            SetCurrentValue(IsFullScreenProperty, request == MdPlaybackRequest.EnterFullScreen);
        MdMediaControlHelpers.RequestPlayback(this, PlaybackCommand, PlaybackRequested, request.Value);
        e.Handled = true;
    }

    private void OnAutoHideTimerTick(object? sender, EventArgs e)
    {
        _autoHideTimer.Stop();
        if (IsPlaying && AutoHideControls && !IsKeyboardFocusWithin)
            SetCurrentValue(IsControlsVisibleProperty, false);
        else
            UpdateAutoHideTimer();
    }

    private void UpdateAutoHideTimer()
    {
        _autoHideTimer.Stop();
        if (!this.IsAttachedToVisualTree() || !AutoHideControls || !IsPlaying || !IsControlsVisible || IsKeyboardFocusWithin)
            return;
        _autoHideTimer.Interval = AutoHideDelay <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(1) : AutoHideDelay;
        _autoHideTimer.Start();
    }

    private void ShowControls()
    {
        if (!IsControlsVisible)
            SetCurrentValue(IsControlsVisibleProperty, true);
    }

    private void UpdateState()
    {
        if (!IsPlaying && !IsControlsVisible)
            SetCurrentValue(IsControlsVisibleProperty, true);
        PseudoClasses.Set(":playing", IsPlaying);
        PseudoClasses.Set(":paused", !IsPlaying);
        PseudoClasses.Set(":busy", IsBuffering);
        PseudoClasses.Set(":full-screen", IsFullScreen);
        UpdatePseudoClasses();
        MdMediaControlHelpers.SetIcon(this, _previousButton, "Md.Icon.SkipPrevious", "Previous");
        MdMediaControlHelpers.SetIcon(this, _rewindButton, "Md.Icon.Replay10", "Rewind 10 seconds");
        MdMediaControlHelpers.SetIcon(this, _playPauseButton, IsPlaying ? "Md.Icon.Pause" : "Md.Icon.PlayArrow", IsPlaying ? "Pause" : "Play");
        MdMediaControlHelpers.SetIcon(this, _forwardButton, "Md.Icon.Forward10", "Forward 10 seconds");
        MdMediaControlHelpers.SetIcon(this, _nextButton, "Md.Icon.SkipNext", "Next");
        MdMediaControlHelpers.SetIcon(this, _muteButton, IsMuted ? "Md.Icon.VolumeOff" : "Md.Icon.VolumeUp", IsMuted ? "Unmute" : "Mute");
        MdMediaControlHelpers.SetIcon(this, _fullScreenButton,
            IsFullScreen ? "Md.Icon.FullscreenExit" : "Md.Icon.Fullscreen",
            IsFullScreen ? "Exit full screen" : "Enter full screen");
        if (_previousButton is not null) _previousButton.SetCurrentValue(IsEnabledProperty, IsEnabled && CanPrevious);
        if (_nextButton is not null) _nextButton.SetCurrentValue(IsEnabledProperty, IsEnabled && CanNext);
        if (_playPauseButton is not null) _playPauseButton.SetCurrentValue(IsEnabledProperty, IsEnabled && !IsBuffering);
        if (_rewindButton is not null) _rewindButton.SetCurrentValue(IsEnabledProperty, IsEnabled && !IsBuffering);
        if (_forwardButton is not null) _forwardButton.SetCurrentValue(IsEnabledProperty, IsEnabled && !IsBuffering);
        if (_muteButton is not null) _muteButton.SetCurrentValue(IsEnabledProperty, IsEnabled);
        if (_fullScreenButton is not null) _fullScreenButton.SetCurrentValue(IsEnabledProperty, IsEnabled);
        if (_busyIndicator is not null) _busyIndicator.SetCurrentValue(IsVisibleProperty, IsBuffering);
        AutomationProperties.SetName(this, string.IsNullOrWhiteSpace(Title) ? "Video player controls" : $"Video player controls for {Title}");
        UpdateAutoHideTimer();
    }

    private void UpdatePseudoClasses() => PseudoClasses.Set(":controls-hidden", !IsControlsVisible);

    private void OnPreviousClick(object? sender, RoutedEventArgs e) => Request(MdPlaybackRequest.Previous);
    private void OnRewindClick(object? sender, RoutedEventArgs e) => Request(MdPlaybackRequest.Rewind);
    private void OnPlayPauseClick(object? sender, RoutedEventArgs e) => TogglePlayback();
    private void OnForwardClick(object? sender, RoutedEventArgs e) => Request(MdPlaybackRequest.Forward);
    private void OnNextClick(object? sender, RoutedEventArgs e) => Request(MdPlaybackRequest.Next);
    private void OnMuteClick(object? sender, RoutedEventArgs e) => ToggleMute();
    private void OnFullScreenClick(object? sender, RoutedEventArgs e) => ToggleFullScreen();

    private void OnSeekRequested(object? sender, MdSeekRequestedEventArgs e) =>
        MdMediaControlHelpers.RequestSeek(this, SeekCommand, SeekRequested, e.Position);

    private void TogglePlayback()
    {
        var request = IsPlaying ? MdPlaybackRequest.Pause : MdPlaybackRequest.Play;
        SetCurrentValue(IsPlayingProperty, !IsPlaying);
        Request(request);
    }

    private void ToggleMute()
    {
        var request = IsMuted ? MdPlaybackRequest.Unmute : MdPlaybackRequest.Mute;
        SetCurrentValue(IsMutedProperty, !IsMuted);
        Request(request);
    }

    private void ToggleFullScreen()
    {
        var request = IsFullScreen ? MdPlaybackRequest.ExitFullScreen : MdPlaybackRequest.EnterFullScreen;
        SetCurrentValue(IsFullScreenProperty, !IsFullScreen);
        Request(request);
    }

    private void Request(MdPlaybackRequest request) =>
        MdMediaControlHelpers.RequestPlayback(this, PlaybackCommand, PlaybackRequested, request);
}
