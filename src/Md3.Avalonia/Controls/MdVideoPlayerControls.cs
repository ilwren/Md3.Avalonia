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
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// An overlay for video surfaces with transport, mute, fullscreen and seek affordances.
/// Double-taps in the outer thirds request ten-second jumps, a center double-tap toggles playback,
/// and holding either edge seeks continuously. The host supplies the video surface and playback engine,
/// while this control raises requests only.
/// </summary>
[PseudoClasses(":playing", ":paused", ":busy", ":controls-hidden", ":full-screen", ":reduced-motion", ":no-motion")]
public sealed class MdVideoPlayerControls : TemplatedControl
{
    private const int DoubleTapWindowMilliseconds = 320;
    private const int LongPressDelayMilliseconds = 450;
    private const int LongPressSeekIntervalMilliseconds = 200;
    private const int LongPressFeedbackDismissMilliseconds = 450;
    private const double TapMovementThreshold = 24;

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
    private readonly DispatcherTimer _tapTimer;
    private readonly DispatcherTimer _longPressTimer;
    private readonly DispatcherTimer _feedbackTimer;
    private readonly MdPresenceController _controlsPresence;
    private readonly MdPresenceController _feedbackPresence;
    private Grid? _overlay;
    private Grid? _controlsSurface;
    private Border? _gestureFeedback;
    private MdIcon? _gestureFeedbackIcon;
    private TextBlock? _gestureFeedbackLabel;
    private MdSeekBar? _seekBar;
    private MdIconButton? _previousButton;
    private MdIconButton? _rewindButton;
    private MdIconButton? _playPauseButton;
    private MdIconButton? _forwardButton;
    private MdIconButton? _nextButton;
    private MdIconButton? _muteButton;
    private MdIconButton? _fullScreenButton;
    private MdCircularProgressIndicator? _busyIndicator;
    private bool _pointerPressed;
    private bool _longPressActive;
    private bool _longPressConsumed;
    private bool _tapPending;
    private IPointer? _pressedPointer;
    private Point _pointerPressPosition;
    private Point _pendingTapPosition;
    private long _pendingTapTimestamp;
    private long _longPressSeekStartedAt;
    private TimeSpan _longPressSeekOrigin;
    private TimeSpan? _lastLongPressSeekPosition;
    private TapZone _longPressSeekZone;

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
            control.UpdateControlVisibility();
            control.UpdateAutoHideTimer();
        });
        IsEnabledProperty.Changed.AddClassHandler<MdVideoPlayerControls>((control, _) => control.UpdateState());
        IsKeyboardFocusWithinProperty.Changed.AddClassHandler<MdVideoPlayerControls>((control, _) =>
        {
            if (control.IsKeyboardFocusWithin) control.ShowControls();
            control.UpdateAutoHideTimer();
        });
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdVideoPlayerControls>((control, _) => control.UpdateMotion());
    }

    public MdVideoPlayerControls()
    {
        _autoHideTimer = new DispatcherTimer();
        _autoHideTimer.Tick += OnAutoHideTimerTick;
        _tapTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(DoubleTapWindowMilliseconds) };
        _tapTimer.Tick += OnTapTimerTick;
        _longPressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(LongPressDelayMilliseconds) };
        _longPressTimer.Tick += OnLongPressTimerTick;
        _feedbackTimer = new DispatcherTimer();
        _feedbackTimer.Tick += OnFeedbackTimerTick;
        _controlsPresence = new MdPresenceController(present =>
        {
            if (_overlay is not null) _overlay.IsVisible = present;
        });
        _feedbackPresence = new MdPresenceController(present =>
        {
            if (_gestureFeedback is not null) _gestureFeedback.IsVisible = present;
        });

        AddHandler(PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerCaptureLostEvent, OnPointerCaptureLost, RoutingStrategies.Bubble, handledEventsToo: true);
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
        _overlay = e.NameScope.Find<Grid>("PART_Overlay");
        _controlsSurface = e.NameScope.Find<Grid>("PART_ControlsSurface");
        _gestureFeedback = e.NameScope.Find<Border>("PART_GestureFeedback");
        _gestureFeedbackIcon = e.NameScope.Find<MdIcon>("PART_GestureFeedbackIcon");
        _gestureFeedbackLabel = e.NameScope.Find<TextBlock>("PART_GestureFeedbackLabel");
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

        _controlsPresence.Initialize(IsControlsVisible);
        _feedbackPresence.Initialize(false);
        if (_gestureFeedback is not null)
            AutomationProperties.SetAccessibilityView(_gestureFeedback, AccessibilityView.Raw);
        UpdateMotion();
        UpdateControlVisibility(immediate: true);
        UpdateState();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateControlVisibility(immediate: true);
        UpdateAutoHideTimer();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _autoHideTimer.Stop();
        _tapTimer.Stop();
        _longPressTimer.Stop();
        _feedbackTimer.Stop();
        _controlsPresence.Stop();
        _feedbackPresence.Stop();
        _feedbackPresence.Initialize(false);
        var pressedPointer = _pressedPointer;
        _pointerPressed = false;
        _longPressActive = false;
        _longPressConsumed = false;
        _pressedPointer = null;
        _tapPending = false;
        if (pressedPointer is not null && ReferenceEquals(pressedPointer.Captured, this))
            pressedPointer.Capture(null);
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
        if (_pointerPressed && ReferenceEquals(e.Pointer, _pressedPointer) &&
            Distance(e.GetCurrentPoint(this).Position, _pointerPressPosition) > TapMovementThreshold)
        {
            _pointerPressed = false;
            _longPressTimer.Stop();
            if (_longPressActive)
            {
                StopLongPressSeek();
                _longPressConsumed = true;
            }
            else
            {
                ResumePendingTapTimer();
            }
        }
        ShowControls();
        UpdateAutoHideTimer();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsEnabled) return;
        if (MdMediaControlHelpers.IsInsideInteractiveControl(e.Source, this))
        {
            ShowControls();
            UpdateAutoHideTimer();
            return;
        }
        if (_pointerPressed || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        var point = e.GetCurrentPoint(this).Position;
        if (_tapPending && Environment.TickCount64 - _pendingTapTimestamp <= DoubleTapWindowMilliseconds &&
            GetTapZone(point) == GetTapZone(_pendingTapPosition))
            _tapTimer.Stop();

        _pointerPressed = true;
        _longPressActive = false;
        _longPressConsumed = false;
        _pressedPointer = e.Pointer;
        _pointerPressPosition = point;
        e.Pointer.Capture(this);
        _longPressTimer.Interval = TimeSpan.FromMilliseconds(LongPressDelayMilliseconds);
        _longPressTimer.Start();
        UpdateAutoHideTimer();
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!ReferenceEquals(e.Pointer, _pressedPointer)) return;
        var point = e.GetCurrentPoint(this).Position;
        _pressedPointer = null;
        if (ReferenceEquals(e.Pointer.Captured, this)) e.Pointer.Capture(null);

        if (_longPressActive)
        {
            _pointerPressed = false;
            StopLongPressSeek();
            _longPressConsumed = false;
            UpdateAutoHideTimer();
            e.Handled = true;
            return;
        }

        if (!_pointerPressed)
        {
            if (_longPressConsumed)
            {
                _longPressConsumed = false;
                e.Handled = true;
            }
            UpdateAutoHideTimer();
            return;
        }

        _pointerPressed = false;
        _longPressTimer.Stop();
        if (Distance(point, _pointerPressPosition) > TapMovementThreshold)
        {
            ResumePendingTapTimer();
            UpdateAutoHideTimer();
            return;
        }

        var now = Environment.TickCount64;
        if (_tapPending && now - _pendingTapTimestamp <= DoubleTapWindowMilliseconds &&
            GetTapZone(point) == GetTapZone(_pendingTapPosition))
        {
            _tapTimer.Stop();
            _tapPending = false;
            HandleDoubleTap(point);
            UpdateAutoHideTimer();
            e.Handled = true;
            return;
        }

        if (_tapPending) CompletePendingSingleTap();
        _pendingTapPosition = point;
        _pendingTapTimestamp = now;
        _tapPending = true;
        _tapTimer.Stop();
        _tapTimer.Start();
        UpdateAutoHideTimer();
        e.Handled = true;
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (!ReferenceEquals(e.Pointer, _pressedPointer)) return;
        _pressedPointer = null;
        _pointerPressed = false;
        _longPressTimer.Stop();
        if (_longPressActive)
            StopLongPressSeek();
        else
            ResumePendingTapTimer();
        _longPressConsumed = false;
        UpdateAutoHideTimer();
    }

    private void OnTapTimerTick(object? sender, EventArgs e)
    {
        _tapTimer.Stop();
        CompletePendingSingleTap();
    }

    private void CompletePendingSingleTap()
    {
        if (!_tapPending) return;
        _tapPending = false;
        if (!IsControlsVisible)
            ShowControls();
        else if (IsPlaying)
            SetCurrentValue(IsControlsVisibleProperty, false);
        else
            ShowControls();
    }

    private void ResumePendingTapTimer()
    {
        if (!_tapPending || _tapTimer.IsEnabled) return;
        var remainingMilliseconds = DoubleTapWindowMilliseconds - (Environment.TickCount64 - _pendingTapTimestamp);
        if (remainingMilliseconds <= 0)
        {
            CompletePendingSingleTap();
            return;
        }
        _tapTimer.Interval = TimeSpan.FromMilliseconds(remainingMilliseconds);
        _tapTimer.Start();
    }

    private void HandleDoubleTap(Point point)
    {
        ShowControls();
        switch (GetTapZone(point))
        {
            case TapZone.Back:
                Request(MdPlaybackRequest.Rewind);
                break;
            case TapZone.Center:
                TogglePlayback();
                break;
            case TapZone.Forward:
                Request(MdPlaybackRequest.Forward);
                break;
        }
    }

    private void OnLongPressTimerTick(object? sender, EventArgs e)
    {
        _longPressTimer.Stop();
        if (!_pointerPressed) return;

        if (!_longPressActive)
        {
            var zone = GetTapZone(_pointerPressPosition);
            if (zone == TapZone.Center || Duration <= TimeSpan.Zero) return;

            _longPressSeekZone = zone;
            _longPressSeekOrigin = Clamp(Position, Duration);
            _longPressSeekStartedAt = Environment.TickCount64;
            _lastLongPressSeekPosition = null;
            _longPressActive = true;
            _longPressConsumed = true;
            _tapTimer.Stop();
            _tapPending = false;
            ShowControls();
            IssueLongPressSeek();
        }
        else
        {
            IssueLongPressSeek();
        }

        _longPressTimer.Interval = TimeSpan.FromMilliseconds(LongPressSeekIntervalMilliseconds);
        _longPressTimer.Start();
    }

    private void IssueLongPressSeek()
    {
        if (!_longPressActive || Duration <= TimeSpan.Zero) return;

        var elapsedMilliseconds = Math.Max(0, Environment.TickCount64 - _longPressSeekStartedAt);
        var speed = elapsedMilliseconds >= 1_500 ? 4 : 2;
        var offsetSeconds = 2 + elapsedMilliseconds / 1000d * speed;
        var direction = _longPressSeekZone == TapZone.Back ? -1d : 1d;
        var targetTicks = _longPressSeekOrigin.Ticks + direction * offsetSeconds * TimeSpan.TicksPerSecond;
        var clampedTicks = (long)Math.Clamp(targetTicks, 0, Duration.Ticks);
        var requestedPosition = TimeSpan.FromTicks(clampedTicks);
        var request = _longPressSeekZone == TapZone.Back ? MdPlaybackRequest.Rewind : MdPlaybackRequest.Forward;
        var label = _longPressSeekZone == TapZone.Back ? $"{speed}x back" : $"{speed}x ahead";
        ShowGestureFeedback(request, label, keepVisible: true);
        if (_lastLongPressSeekPosition == requestedPosition) return;

        _lastLongPressSeekPosition = requestedPosition;
        MdMediaControlHelpers.RequestSeek(this, SeekCommand, SeekRequested, requestedPosition);
    }

    private void StopLongPressSeek()
    {
        _longPressTimer.Stop();
        if (!_longPressActive) return;
        IssueLongPressSeek();
        _longPressActive = false;
        _feedbackTimer.Stop();
        _feedbackTimer.Interval = TimeSpan.FromMilliseconds(LongPressFeedbackDismissMilliseconds);
        _feedbackTimer.Start();
        UpdateAutoHideTimer();
    }

    private static double Distance(Point first, Point second)
    {
        var deltaX = first.X - second.X;
        var deltaY = first.Y - second.Y;
        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    private static TimeSpan Clamp(TimeSpan value, TimeSpan duration)
    {
        if (value <= TimeSpan.Zero) return TimeSpan.Zero;
        return value > duration ? duration : value;
    }

    private TapZone GetTapZone(Point point)
    {
        if (Bounds.Width <= 0) return TapZone.Center;
        var fraction = point.X / Bounds.Width;
        if (fraction < 0.35) return TapZone.Back;
        return fraction > 0.65 ? TapZone.Forward : TapZone.Center;
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
        Request(request.Value);
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
        if (!this.IsAttachedToVisualTree() || !AutoHideControls || !IsPlaying || !IsControlsVisible ||
            IsKeyboardFocusWithin || _pointerPressed || _longPressActive)
            return;
        _autoHideTimer.Interval = AutoHideDelay <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(1) : AutoHideDelay;
        _autoHideTimer.Start();
    }

    private void ShowControls()
    {
        if (!IsControlsVisible)
            SetCurrentValue(IsControlsVisibleProperty, true);
        UpdateAutoHideTimer();
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
        if (_controlsSurface is not null)
            _controlsSurface.SetCurrentValue(IsEnabledProperty, IsEnabled && IsControlsVisible);
        AutomationProperties.SetName(this, string.IsNullOrWhiteSpace(Title) ? "Video player controls" : $"Video player controls for {Title}");
        UpdateAutoHideTimer();
    }

    private void UpdateControlVisibility(bool immediate = false)
    {
        if (_overlay is null || _controlsSurface is null)
        {
            _controlsPresence.Update(IsControlsVisible, GetEffectsDuration());
            return;
        }

        _controlsSurface.SetCurrentValue(IsEnabledProperty, IsEnabled && IsControlsVisible);
        _controlsSurface.IsHitTestVisible = IsControlsVisible;
        _overlay.IsHitTestVisible = true;

        if (immediate)
        {
            _controlsPresence.Initialize(IsControlsVisible);
            _overlay.IsVisible = IsControlsVisible;
            _overlay.Opacity = IsControlsVisible ? 1 : 0;
            return;
        }

        if (IsControlsVisible)
        {
            var wasPresent = _controlsPresence.IsPresent;
            _controlsPresence.Update(true, TimeSpan.Zero);
            if (!wasPresent)
            {
                _overlay.Opacity = 0;
                Dispatcher.UIThread.Post(() =>
                {
                    if (IsControlsVisible && _overlay is not null)
                        _overlay.Opacity = 1;
                }, DispatcherPriority.Render);
            }
            else
            {
                _overlay.Opacity = 1;
            }
        }
        else
        {
            _overlay.Opacity = 0;
            _controlsPresence.Update(false, GetEffectsDuration());
        }
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_overlay is not null)
        {
            _overlay.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, Visual.OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
            if (!IsControlsVisible)
                _controlsPresence.Update(false, GetEffectsDuration());
        }
        if (_gestureFeedback is not null)
        {
            _gestureFeedback.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, Visual.OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
            if (!_feedbackTimer.IsEnabled && _feedbackPresence.IsPresent)
                _feedbackPresence.Update(false, GetEffectsDuration());
        }
    }

    private TimeSpan GetEffectsDuration() =>
        MdMotion.Resolve(this, MdMotionKind.Effects, MdMotionSpeed.Fast).Duration;

    private void OnFeedbackTimerTick(object? sender, EventArgs e)
    {
        _feedbackTimer.Stop();
        if (_gestureFeedback is null) return;
        _gestureFeedback.Opacity = 0;
        _feedbackPresence.Update(false, GetEffectsDuration());
    }

    private void ShowGestureFeedback(MdPlaybackRequest request, string? labelOverride = null, bool keepVisible = false)
    {
        if (_gestureFeedback is null || _gestureFeedbackIcon is null || _gestureFeedbackLabel is null) return;

        var (resourceKey, defaultLabel) = request switch
        {
            MdPlaybackRequest.Play => ("Md.Icon.PlayArrow", "Play"),
            MdPlaybackRequest.Pause => ("Md.Icon.Pause", "Pause"),
            MdPlaybackRequest.Rewind => ("Md.Icon.Replay10", "10 sec back"),
            MdPlaybackRequest.Forward => ("Md.Icon.Forward10", "10 sec ahead"),
            _ => (string.Empty, string.Empty)
        };
        if (string.IsNullOrEmpty(resourceKey)) return;

        if (Application.Current?.TryGetResource(resourceKey, ActualThemeVariant, out var glyph) == true)
            _gestureFeedbackIcon.Glyph = glyph?.ToString();
        _gestureFeedbackLabel.Text = labelOverride ?? defaultLabel;

        var wasPresent = _feedbackPresence.IsPresent;
        _feedbackPresence.Update(true, TimeSpan.Zero);
        if (!wasPresent) _gestureFeedback.Opacity = 0;
        Dispatcher.UIThread.Post(() =>
        {
            if (_feedbackPresence.IsPresent && _gestureFeedback is not null)
                _gestureFeedback.Opacity = 1;
        }, DispatcherPriority.Render);

        _feedbackTimer.Stop();
        if (!keepVisible)
        {
            _feedbackTimer.Interval = TimeSpan.FromMilliseconds(620);
            _feedbackTimer.Start();
        }
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

    private void Request(MdPlaybackRequest request)
    {
        ShowGestureFeedback(request);
        MdMediaControlHelpers.RequestPlayback(this, PlaybackCommand, PlaybackRequested, request);
    }

    private enum TapZone
    {
        Back,
        Center,
        Forward
    }
}
