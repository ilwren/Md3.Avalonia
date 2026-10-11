using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Md3.Avalonia.Controls;

/// <summary>A full audio now-playing surface with artwork, metadata, seek, queue and transport actions.</summary>
[PseudoClasses(":playing", ":busy", ":shuffle-enabled", ":repeat-enabled")]
public sealed class MdNowPlayingPanel : TemplatedControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, string?>(nameof(Title));
    public static readonly StyledProperty<string?> ArtistProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, string?>(nameof(Artist));
    public static readonly StyledProperty<string?> AlbumProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, string?>(nameof(Album));
    public static readonly StyledProperty<IImage?> ArtworkProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, IImage?>(nameof(Artwork));
    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, TimeSpan>(nameof(Duration));
    public static readonly StyledProperty<TimeSpan> PositionProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, TimeSpan>(nameof(Position),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<TimeSpan> BufferedPositionProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, TimeSpan>(nameof(BufferedPosition));
    public static readonly StyledProperty<bool> IsPlayingProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, bool>(nameof(IsPlaying));
    public static readonly StyledProperty<bool> IsBusyProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, bool>(nameof(IsBusy));
    public static readonly StyledProperty<bool> CanPreviousProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, bool>(nameof(CanPrevious), true);
    public static readonly StyledProperty<bool> CanNextProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, bool>(nameof(CanNext), true);
    public static readonly StyledProperty<bool> IsShuffleEnabledProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, bool>(nameof(IsShuffleEnabled),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<MdPlaybackRepeatMode> RepeatModeProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, MdPlaybackRepeatMode>(nameof(RepeatMode),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> CanToggleShuffleProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, bool>(nameof(CanToggleShuffle), true);
    public static readonly StyledProperty<bool> CanCycleRepeatProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, bool>(nameof(CanCycleRepeat), true);
    public static readonly StyledProperty<bool> CanOpenQueueProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, bool>(nameof(CanOpenQueue), true);
    public static readonly StyledProperty<ICommand?> PlaybackCommandProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, ICommand?>(nameof(PlaybackCommand));
    public static readonly StyledProperty<ICommand?> SeekCommandProperty =
        AvaloniaProperty.Register<MdNowPlayingPanel, ICommand?>(nameof(SeekCommand));

    private MdSeekBar? _seekBar;
    private Image? _artworkImage;
    private MdIcon? _artworkPlaceholder;
    private MdCircularProgressIndicator? _busyIndicator;
    private MdIconButton? _shuffleButton;
    private MdIconButton? _previousButton;
    private MdIconButton? _playPauseButton;
    private MdIconButton? _nextButton;
    private MdIconButton? _repeatButton;
    private MdIconButton? _queueButton;

    static MdNowPlayingPanel()
    {
        TitleProperty.Changed.AddClassHandler<MdNowPlayingPanel>((control, _) => control.UpdateAutomation());
        ArtistProperty.Changed.AddClassHandler<MdNowPlayingPanel>((control, _) => control.UpdateAutomation());
        AlbumProperty.Changed.AddClassHandler<MdNowPlayingPanel>((control, _) => control.UpdateAutomation());
        ArtworkProperty.Changed.AddClassHandler<MdNowPlayingPanel>((control, _) => control.UpdateArtwork());
        IsPlayingProperty.Changed.AddClassHandler<MdNowPlayingPanel>((control, _) => control.UpdateState());
        IsBusyProperty.Changed.AddClassHandler<MdNowPlayingPanel>((control, _) => control.UpdateState());
        IsShuffleEnabledProperty.Changed.AddClassHandler<MdNowPlayingPanel>((control, _) => control.UpdateState());
        RepeatModeProperty.Changed.AddClassHandler<MdNowPlayingPanel>((control, _) => control.UpdateState());
        CanPreviousProperty.Changed.AddClassHandler<MdNowPlayingPanel>((control, _) => control.UpdateState());
        CanNextProperty.Changed.AddClassHandler<MdNowPlayingPanel>((control, _) => control.UpdateState());
        CanToggleShuffleProperty.Changed.AddClassHandler<MdNowPlayingPanel>((control, _) => control.UpdateState());
        CanCycleRepeatProperty.Changed.AddClassHandler<MdNowPlayingPanel>((control, _) => control.UpdateState());
        CanOpenQueueProperty.Changed.AddClassHandler<MdNowPlayingPanel>((control, _) => control.UpdateState());
        IsEnabledProperty.Changed.AddClassHandler<MdNowPlayingPanel>((control, _) => control.UpdateState());
    }

    public string? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string? Artist { get => GetValue(ArtistProperty); set => SetValue(ArtistProperty, value); }
    public string? Album { get => GetValue(AlbumProperty); set => SetValue(AlbumProperty, value); }
    /// <summary>Optional cover art. The MusicNote symbol appears when no artwork is supplied.</summary>
    public IImage? Artwork { get => GetValue(ArtworkProperty); set => SetValue(ArtworkProperty, value); }
    public TimeSpan Duration { get => GetValue(DurationProperty); set => SetValue(DurationProperty, value); }
    public TimeSpan Position { get => GetValue(PositionProperty); set => SetValue(PositionProperty, value); }
    public TimeSpan BufferedPosition { get => GetValue(BufferedPositionProperty); set => SetValue(BufferedPositionProperty, value); }
    public bool IsPlaying { get => GetValue(IsPlayingProperty); set => SetValue(IsPlayingProperty, value); }
    public bool IsBusy { get => GetValue(IsBusyProperty); set => SetValue(IsBusyProperty, value); }
    public bool CanPrevious { get => GetValue(CanPreviousProperty); set => SetValue(CanPreviousProperty, value); }
    public bool CanNext { get => GetValue(CanNextProperty); set => SetValue(CanNextProperty, value); }
    public bool IsShuffleEnabled { get => GetValue(IsShuffleEnabledProperty); set => SetValue(IsShuffleEnabledProperty, value); }
    public MdPlaybackRepeatMode RepeatMode { get => GetValue(RepeatModeProperty); set => SetValue(RepeatModeProperty, value); }
    public bool CanToggleShuffle { get => GetValue(CanToggleShuffleProperty); set => SetValue(CanToggleShuffleProperty, value); }
    public bool CanCycleRepeat { get => GetValue(CanCycleRepeatProperty); set => SetValue(CanCycleRepeatProperty, value); }
    public bool CanOpenQueue { get => GetValue(CanOpenQueueProperty); set => SetValue(CanOpenQueueProperty, value); }
    public ICommand? PlaybackCommand { get => GetValue(PlaybackCommandProperty); set => SetValue(PlaybackCommandProperty, value); }
    public ICommand? SeekCommand { get => GetValue(SeekCommandProperty); set => SetValue(SeekCommandProperty, value); }

    /// <summary>Raised for transport, shuffle, repeat and queue requests.</summary>
    public event EventHandler<MdPlaybackRequestEventArgs>? PlaybackRequested;
    /// <summary>Raised when a seek is committed.</summary>
    public event EventHandler<MdSeekRequestedEventArgs>? SeekRequested;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachParts();
        base.OnApplyTemplate(e);
        _seekBar = e.NameScope.Find<MdSeekBar>("PART_SeekBar");
        _artworkImage = e.NameScope.Find<Image>("PART_Artwork");
        _artworkPlaceholder = e.NameScope.Find<MdIcon>("PART_ArtworkPlaceholder");
        _busyIndicator = e.NameScope.Find<MdCircularProgressIndicator>("PART_BusyIndicator");
        _shuffleButton = e.NameScope.Find<MdIconButton>("PART_ShuffleButton");
        _previousButton = e.NameScope.Find<MdIconButton>("PART_PreviousButton");
        _playPauseButton = e.NameScope.Find<MdIconButton>("PART_PlayPauseButton");
        _nextButton = e.NameScope.Find<MdIconButton>("PART_NextButton");
        _repeatButton = e.NameScope.Find<MdIconButton>("PART_RepeatButton");
        _queueButton = e.NameScope.Find<MdIconButton>("PART_QueueButton");
        if (_seekBar is not null) _seekBar.SeekRequested += OnSeekRequested;
        if (_shuffleButton is not null) _shuffleButton.Click += OnShuffleClick;
        if (_previousButton is not null) _previousButton.Click += OnPreviousClick;
        if (_playPauseButton is not null) _playPauseButton.Click += OnPlayPauseClick;
        if (_nextButton is not null) _nextButton.Click += OnNextClick;
        if (_repeatButton is not null) _repeatButton.Click += OnRepeatClick;
        if (_queueButton is not null) _queueButton.Click += OnQueueClick;
        UpdateState();
        UpdateArtwork();
        UpdateAutomation();
    }

    private void DetachParts()
    {
        if (_seekBar is not null) _seekBar.SeekRequested -= OnSeekRequested;
        if (_shuffleButton is not null) _shuffleButton.Click -= OnShuffleClick;
        if (_previousButton is not null) _previousButton.Click -= OnPreviousClick;
        if (_playPauseButton is not null) _playPauseButton.Click -= OnPlayPauseClick;
        if (_nextButton is not null) _nextButton.Click -= OnNextClick;
        if (_repeatButton is not null) _repeatButton.Click -= OnRepeatClick;
        if (_queueButton is not null) _queueButton.Click -= OnQueueClick;
    }

    private void OnShuffleClick(object? sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsShuffleEnabledProperty, !IsShuffleEnabled);
        Request(MdPlaybackRequest.ToggleShuffle);
    }

    private void OnPreviousClick(object? sender, RoutedEventArgs e) => Request(MdPlaybackRequest.Previous);

    private void OnPlayPauseClick(object? sender, RoutedEventArgs e)
    {
        var request = IsPlaying ? MdPlaybackRequest.Pause : MdPlaybackRequest.Play;
        SetCurrentValue(IsPlayingProperty, !IsPlaying);
        Request(request);
    }

    private void OnNextClick(object? sender, RoutedEventArgs e) => Request(MdPlaybackRequest.Next);

    private void OnRepeatClick(object? sender, RoutedEventArgs e)
    {
        var next = RepeatMode switch
        {
            MdPlaybackRepeatMode.None => MdPlaybackRepeatMode.All,
            MdPlaybackRepeatMode.All => MdPlaybackRepeatMode.One,
            _ => MdPlaybackRepeatMode.None
        };
        SetCurrentValue(RepeatModeProperty, next);
        Request(MdPlaybackRequest.CycleRepeat);
    }

    private void OnQueueClick(object? sender, RoutedEventArgs e) => Request(MdPlaybackRequest.OpenQueue);

    private void OnSeekRequested(object? sender, MdSeekRequestedEventArgs e) =>
        MdMediaControlHelpers.RequestSeek(this, SeekCommand, SeekRequested, e.Position);

    private void Request(MdPlaybackRequest request) =>
        MdMediaControlHelpers.RequestPlayback(this, PlaybackCommand, PlaybackRequested, request);

    private void UpdateState()
    {
        PseudoClasses.Set(":playing", IsPlaying);
        PseudoClasses.Set(":busy", IsBusy);
        PseudoClasses.Set(":shuffle-enabled", IsShuffleEnabled);
        PseudoClasses.Set(":repeat-enabled", RepeatMode != MdPlaybackRepeatMode.None);
        MdMediaControlHelpers.SetIcon(this, _shuffleButton, "Md.Icon.Shuffle", "Shuffle");
        MdMediaControlHelpers.SetIcon(this, _previousButton, "Md.Icon.SkipPrevious", "Previous track");
        MdMediaControlHelpers.SetIcon(this, _playPauseButton,
            IsPlaying ? "Md.Icon.Pause" : "Md.Icon.PlayArrow", IsPlaying ? "Pause" : "Play");
        MdMediaControlHelpers.SetIcon(this, _nextButton, "Md.Icon.SkipNext", "Next track");
        var repeatKey = RepeatMode == MdPlaybackRepeatMode.One ? "Md.Icon.RepeatOne" : "Md.Icon.Repeat";
        var repeatName = RepeatMode switch
        {
            MdPlaybackRepeatMode.All => "Repeat all",
            MdPlaybackRepeatMode.One => "Repeat one",
            _ => "Repeat off"
        };
        MdMediaControlHelpers.SetIcon(this, _repeatButton, repeatKey, repeatName);
        MdMediaControlHelpers.SetIcon(this, _queueButton, "Md.Icon.QueueMusic", "Open queue");
        if (_shuffleButton is not null)
        {
            _shuffleButton.SetCurrentValue(MdIconButton.VariantProperty, IsShuffleEnabled ? MdIconButtonVariant.Tonal : MdIconButtonVariant.Standard);
            _shuffleButton.SetCurrentValue(IsVisibleProperty, CanToggleShuffle);
            _shuffleButton.SetCurrentValue(IsEnabledProperty, IsEnabled && CanToggleShuffle);
        }
        if (_previousButton is not null) _previousButton.SetCurrentValue(IsEnabledProperty, IsEnabled && CanPrevious);
        if (_playPauseButton is not null) _playPauseButton.SetCurrentValue(IsEnabledProperty, IsEnabled && !IsBusy);
        if (_nextButton is not null) _nextButton.SetCurrentValue(IsEnabledProperty, IsEnabled && CanNext);
        if (_repeatButton is not null)
        {
            _repeatButton.SetCurrentValue(MdIconButton.VariantProperty, RepeatMode == MdPlaybackRepeatMode.None ? MdIconButtonVariant.Standard : MdIconButtonVariant.Tonal);
            _repeatButton.SetCurrentValue(IsVisibleProperty, CanCycleRepeat);
            _repeatButton.SetCurrentValue(IsEnabledProperty, IsEnabled && CanCycleRepeat);
        }
        if (_queueButton is not null)
        {
            _queueButton.SetCurrentValue(IsVisibleProperty, CanOpenQueue);
            _queueButton.SetCurrentValue(IsEnabledProperty, IsEnabled && CanOpenQueue);
        }
        if (_busyIndicator is not null) _busyIndicator.SetCurrentValue(IsVisibleProperty, IsBusy);
        UpdateAutomation();
    }

    private void UpdateArtwork()
    {
        if (_artworkImage is not null) _artworkImage.SetCurrentValue(Image.SourceProperty, Artwork);
        if (_artworkPlaceholder is not null) _artworkPlaceholder.SetCurrentValue(IsVisibleProperty, Artwork is null);
    }

    private void UpdateAutomation()
    {
        var title = string.IsNullOrWhiteSpace(Title) ? "Now playing" : Title;
        AutomationProperties.SetName(this, title);
        var metadata = string.Join(" · ", new[] { Artist, Album }.Where(value => !string.IsNullOrWhiteSpace(value)));
        AutomationProperties.SetHelpText(this, string.IsNullOrWhiteSpace(metadata) ? null : metadata);
    }
}
