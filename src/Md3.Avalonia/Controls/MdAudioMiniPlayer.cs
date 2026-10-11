using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Md3.Avalonia.Controls;

/// <summary>A compact artwork, track-info, progress and transport row for audio playback.</summary>
[PseudoClasses(":playing", ":busy")]
public sealed class MdAudioMiniPlayer : TemplatedControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<MdAudioMiniPlayer, string?>(nameof(Title));
    public static readonly StyledProperty<string?> ArtistProperty =
        AvaloniaProperty.Register<MdAudioMiniPlayer, string?>(nameof(Artist));
    public static readonly StyledProperty<IImage?> ArtworkProperty =
        AvaloniaProperty.Register<MdAudioMiniPlayer, IImage?>(nameof(Artwork));
    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<MdAudioMiniPlayer, TimeSpan>(nameof(Duration));
    public static readonly StyledProperty<TimeSpan> PositionProperty =
        AvaloniaProperty.Register<MdAudioMiniPlayer, TimeSpan>(nameof(Position),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<TimeSpan> BufferedPositionProperty =
        AvaloniaProperty.Register<MdAudioMiniPlayer, TimeSpan>(nameof(BufferedPosition));
    public static readonly StyledProperty<bool> IsPlayingProperty =
        AvaloniaProperty.Register<MdAudioMiniPlayer, bool>(nameof(IsPlaying));
    public static readonly StyledProperty<bool> IsBusyProperty =
        AvaloniaProperty.Register<MdAudioMiniPlayer, bool>(nameof(IsBusy));
    public static readonly StyledProperty<bool> CanPreviousProperty =
        AvaloniaProperty.Register<MdAudioMiniPlayer, bool>(nameof(CanPrevious), true);
    public static readonly StyledProperty<bool> CanNextProperty =
        AvaloniaProperty.Register<MdAudioMiniPlayer, bool>(nameof(CanNext), true);
    public static readonly StyledProperty<ICommand?> PlaybackCommandProperty =
        AvaloniaProperty.Register<MdAudioMiniPlayer, ICommand?>(nameof(PlaybackCommand));
    public static readonly StyledProperty<ICommand?> OpenCommandProperty =
        AvaloniaProperty.Register<MdAudioMiniPlayer, ICommand?>(nameof(OpenCommand));

    private MdIconButton? _previousButton;
    private MdIconButton? _playPauseButton;
    private MdIconButton? _nextButton;
    private MdLinearProgressIndicator? _bufferedProgress;
    private MdLinearProgressIndicator? _progress;
    private Image? _artworkImage;
    private MdIcon? _artworkPlaceholder;
    private MdCircularProgressIndicator? _busyIndicator;

    static MdAudioMiniPlayer()
    {
        IsPlayingProperty.Changed.AddClassHandler<MdAudioMiniPlayer>((control, _) => control.UpdateState());
        IsBusyProperty.Changed.AddClassHandler<MdAudioMiniPlayer>((control, _) => control.UpdateState());
        CanPreviousProperty.Changed.AddClassHandler<MdAudioMiniPlayer>((control, _) => control.UpdateState());
        CanNextProperty.Changed.AddClassHandler<MdAudioMiniPlayer>((control, _) => control.UpdateState());
        TitleProperty.Changed.AddClassHandler<MdAudioMiniPlayer>((control, _) => control.UpdateAutomation());
        ArtistProperty.Changed.AddClassHandler<MdAudioMiniPlayer>((control, _) => control.UpdateAutomation());
        ArtworkProperty.Changed.AddClassHandler<MdAudioMiniPlayer>((control, _) => control.UpdateArtwork());
        DurationProperty.Changed.AddClassHandler<MdAudioMiniPlayer>((control, _) => control.UpdateProgress());
        PositionProperty.Changed.AddClassHandler<MdAudioMiniPlayer>((control, _) => control.UpdateProgress());
        BufferedPositionProperty.Changed.AddClassHandler<MdAudioMiniPlayer>((control, _) => control.UpdateProgress());
        IsEnabledProperty.Changed.AddClassHandler<MdAudioMiniPlayer>((control, _) => control.UpdateState());
    }

    public MdAudioMiniPlayer()
    {
        Focusable = true;
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel, handledEventsToo: true);
        UpdateState();
    }

    public string? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string? Artist { get => GetValue(ArtistProperty); set => SetValue(ArtistProperty, value); }
    /// <summary>Optional artwork image. When null, the host-provided MusicNote symbol is shown.</summary>
    public IImage? Artwork { get => GetValue(ArtworkProperty); set => SetValue(ArtworkProperty, value); }
    public TimeSpan Duration { get => GetValue(DurationProperty); set => SetValue(DurationProperty, value); }
    public TimeSpan Position { get => GetValue(PositionProperty); set => SetValue(PositionProperty, value); }
    public TimeSpan BufferedPosition { get => GetValue(BufferedPositionProperty); set => SetValue(BufferedPositionProperty, value); }
    public bool IsPlaying { get => GetValue(IsPlayingProperty); set => SetValue(IsPlayingProperty, value); }
    public bool IsBusy { get => GetValue(IsBusyProperty); set => SetValue(IsBusyProperty, value); }
    public bool CanPrevious { get => GetValue(CanPreviousProperty); set => SetValue(CanPreviousProperty, value); }
    public bool CanNext { get => GetValue(CanNextProperty); set => SetValue(CanNextProperty, value); }
    public ICommand? PlaybackCommand { get => GetValue(PlaybackCommandProperty); set => SetValue(PlaybackCommandProperty, value); }
    public ICommand? OpenCommand { get => GetValue(OpenCommandProperty); set => SetValue(OpenCommandProperty, value); }

    /// <summary>Raised for play, pause, previous and next requests.</summary>
    public event EventHandler<MdPlaybackRequestEventArgs>? PlaybackRequested;
    /// <summary>Raised when the user activates the track-information surface.</summary>
    public event EventHandler? OpenRequested;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachParts();
        base.OnApplyTemplate(e);
        _previousButton = e.NameScope.Find<MdIconButton>("PART_PreviousButton");
        _playPauseButton = e.NameScope.Find<MdIconButton>("PART_PlayPauseButton");
        _nextButton = e.NameScope.Find<MdIconButton>("PART_NextButton");
        _progress = e.NameScope.Find<MdLinearProgressIndicator>("PART_Progress");
        _bufferedProgress = e.NameScope.Find<MdLinearProgressIndicator>("PART_BufferedProgress");
        _artworkImage = e.NameScope.Find<Image>("PART_Artwork");
        _artworkPlaceholder = e.NameScope.Find<MdIcon>("PART_ArtworkPlaceholder");
        _busyIndicator = e.NameScope.Find<MdCircularProgressIndicator>("PART_BusyIndicator");
        if (_previousButton is not null) _previousButton.Click += OnPreviousClick;
        if (_playPauseButton is not null) _playPauseButton.Click += OnPlayPauseClick;
        if (_nextButton is not null) _nextButton.Click += OnNextClick;
        UpdateState();
        UpdateArtwork();
        UpdateProgress();
        UpdateAutomation();
    }

    private void DetachParts()
    {
        if (_previousButton is not null) _previousButton.Click -= OnPreviousClick;
        if (_playPauseButton is not null) _playPauseButton.Click -= OnPlayPauseClick;
        if (_nextButton is not null) _nextButton.Click -= OnNextClick;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (MdMediaControlHelpers.IsInsideInteractiveControl(e.Source, this) ||
            !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        Open();
        e.Handled = true;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (MdMediaControlHelpers.IsInsideInteractiveControl(e.Source, this) || e.Key is not (Key.Enter or Key.Space)) return;
        Open();
        e.Handled = true;
    }

    private void OnPreviousClick(object? sender, RoutedEventArgs e) => Request(MdPlaybackRequest.Previous);
    private void OnPlayPauseClick(object? sender, RoutedEventArgs e)
    {
        var request = IsPlaying ? MdPlaybackRequest.Pause : MdPlaybackRequest.Play;
        SetCurrentValue(IsPlayingProperty, !IsPlaying);
        Request(request);
    }
    private void OnNextClick(object? sender, RoutedEventArgs e) => Request(MdPlaybackRequest.Next);

    private void Open() => MdMediaControlHelpers.RequestOpen(this, OpenCommand, OpenRequested);

    private void Request(MdPlaybackRequest request) =>
        MdMediaControlHelpers.RequestPlayback(this, PlaybackCommand, PlaybackRequested, request);

    private void UpdateState()
    {
        PseudoClasses.Set(":playing", IsPlaying);
        PseudoClasses.Set(":busy", IsBusy);
        MdMediaControlHelpers.SetIcon(this, _previousButton, "Md.Icon.SkipPrevious", "Previous track");
        MdMediaControlHelpers.SetIcon(this, _playPauseButton,
            IsPlaying ? "Md.Icon.Pause" : "Md.Icon.PlayArrow", IsPlaying ? "Pause" : "Play");
        MdMediaControlHelpers.SetIcon(this, _nextButton, "Md.Icon.SkipNext", "Next track");
        if (_previousButton is not null) _previousButton.SetCurrentValue(IsEnabledProperty, IsEnabled && CanPrevious);
        if (_nextButton is not null) _nextButton.SetCurrentValue(IsEnabledProperty, IsEnabled && CanNext);
        if (_playPauseButton is not null) _playPauseButton.SetCurrentValue(IsEnabledProperty, IsEnabled && !IsBusy);
        if (_busyIndicator is not null) _busyIndicator.SetCurrentValue(IsVisibleProperty, IsBusy);
        UpdateAutomation();
    }

    private void UpdateArtwork()
    {
        if (_artworkImage is not null) _artworkImage.SetCurrentValue(Image.SourceProperty, Artwork);
        if (_artworkPlaceholder is not null) _artworkPlaceholder.SetCurrentValue(IsVisibleProperty, Artwork is null);
    }

    private void UpdateProgress()
    {
        var durationTicks = Math.Max(0, Duration.Ticks);
        var positionTicks = Math.Clamp(Position.Ticks, 0, durationTicks);
        var bufferedTicks = Math.Clamp(BufferedPosition.Ticks, 0, durationTicks);
        var progress = durationTicks > 0 ? (double)positionTicks / durationTicks * 100 : 0;
        var buffered = durationTicks > 0 ? (double)bufferedTicks / durationTicks * 100 : 0;
        if (_bufferedProgress is not null)
            _bufferedProgress.SetCurrentValue(ProgressBar.ValueProperty, buffered);
        if (_progress is not null)
            _progress.SetCurrentValue(ProgressBar.ValueProperty, progress);
    }

    private void UpdateAutomation()
    {
        var title = string.IsNullOrWhiteSpace(Title) ? "Audio player" : Title;
        AutomationProperties.SetName(this, title);
        AutomationProperties.SetHelpText(this, string.IsNullOrWhiteSpace(Artist) ? null : Artist);
    }
}
