using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A Material-styled, time-based seek control with buffered progress, accessible time labels and
/// a live time bubble while scrubbing. It reports seek requests only; media transport stays in the host.
/// </summary>
[PseudoClasses(":seeking", ":reduced-motion", ":no-motion")]
public sealed class MdSeekBar : TemplatedControl
{
    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<MdSeekBar, TimeSpan>(nameof(Duration));
    public static readonly StyledProperty<TimeSpan> PositionProperty =
        AvaloniaProperty.Register<MdSeekBar, TimeSpan>(nameof(Position),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<TimeSpan> BufferedPositionProperty =
        AvaloniaProperty.Register<MdSeekBar, TimeSpan>(nameof(BufferedPosition));
    public static readonly StyledProperty<TimeSpan> SeekStepProperty =
        AvaloniaProperty.Register<MdSeekBar, TimeSpan>(nameof(SeekStep), TimeSpan.FromSeconds(5));
    public static readonly StyledProperty<bool> ShowTimeLabelsProperty =
        AvaloniaProperty.Register<MdSeekBar, bool>(nameof(ShowTimeLabels), true);
    public static readonly StyledProperty<bool> ShowRemainingTimeProperty =
        AvaloniaProperty.Register<MdSeekBar, bool>(nameof(ShowRemainingTime));
    public static readonly StyledProperty<bool> IsSeekEnabledProperty =
        AvaloniaProperty.Register<MdSeekBar, bool>(nameof(IsSeekEnabled), true);
    public static readonly StyledProperty<ICommand?> SeekCommandProperty =
        AvaloniaProperty.Register<MdSeekBar, ICommand?>(nameof(SeekCommand));
    public static readonly StyledProperty<string> AutomationNameProperty =
        AvaloniaProperty.Register<MdSeekBar, string>(nameof(AutomationName), "Seek position");

    public static readonly DirectProperty<MdSeekBar, TimeSpan> PreviewPositionProperty =
        AvaloniaProperty.RegisterDirect<MdSeekBar, TimeSpan>(nameof(PreviewPosition), control => control.PreviewPosition);
    public static readonly DirectProperty<MdSeekBar, bool> IsSeekingProperty =
        AvaloniaProperty.RegisterDirect<MdSeekBar, bool>(nameof(IsSeeking), control => control.IsSeeking);
    public static readonly DirectProperty<MdSeekBar, double> ProgressFractionProperty =
        AvaloniaProperty.RegisterDirect<MdSeekBar, double>(nameof(ProgressFraction), control => control.ProgressFraction);
    public static readonly DirectProperty<MdSeekBar, double> BufferedFractionProperty =
        AvaloniaProperty.RegisterDirect<MdSeekBar, double>(nameof(BufferedFraction), control => control.BufferedFraction);
    public static readonly DirectProperty<MdSeekBar, string> CurrentTimeTextProperty =
        AvaloniaProperty.RegisterDirect<MdSeekBar, string>(nameof(CurrentTimeText), control => control.CurrentTimeText);
    public static readonly DirectProperty<MdSeekBar, string> EndTimeTextProperty =
        AvaloniaProperty.RegisterDirect<MdSeekBar, string>(nameof(EndTimeText), control => control.EndTimeText);

    private MdSlider? _slider;
    private Border? _bufferedTrack;
    private Border? _previewIndicator;
    private TextBlock? _previewLabel;
    private readonly MdPresenceController _previewPresence;
    private TimeSpan _previewPosition;
    private bool _isSeeking;
    private double _progressFraction;
    private double _bufferedFraction;
    private string _currentTimeText = "0:00";
    private string _endTimeText = "0:00";
    private bool _syncingSlider;

    static MdSeekBar()
    {
        DurationProperty.Changed.AddClassHandler<MdSeekBar>((control, _) => control.RefreshState());
        PositionProperty.Changed.AddClassHandler<MdSeekBar>((control, _) => control.RefreshState());
        BufferedPositionProperty.Changed.AddClassHandler<MdSeekBar>((control, _) => control.RefreshState());
        SeekStepProperty.Changed.AddClassHandler<MdSeekBar>((control, _) => control.RefreshState());
        IsSeekEnabledProperty.Changed.AddClassHandler<MdSeekBar>((control, _) => control.RefreshState());
        ShowRemainingTimeProperty.Changed.AddClassHandler<MdSeekBar>((control, _) => control.RefreshState());
        AutomationNameProperty.Changed.AddClassHandler<MdSeekBar>((control, _) => control.UpdateAutomation());
        IsEnabledProperty.Changed.AddClassHandler<MdSeekBar>((control, _) => control.RefreshState());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdSeekBar>((control, _) => control.UpdateMotion());
    }

    public MdSeekBar()
    {
        _previewPresence = new MdPresenceController(present =>
        {
            if (_previewIndicator is not null) _previewIndicator.IsVisible = present;
        });
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(PointerCaptureLostEvent, OnPointerCaptureLost, RoutingStrategies.Bubble, handledEventsToo: true);
        RefreshState();
    }

    /// <summary>Total media duration. Negative values are coerced to zero.</summary>
    public TimeSpan Duration { get => GetValue(DurationProperty); set => SetValue(DurationProperty, value); }
    /// <summary>Current playback position. User commits update this value and raise <see cref="SeekRequested"/>.</summary>
    public TimeSpan Position { get => GetValue(PositionProperty); set => SetValue(PositionProperty, value); }
    /// <summary>Furthest position currently buffered by the host.</summary>
    public TimeSpan BufferedPosition { get => GetValue(BufferedPositionProperty); set => SetValue(BufferedPositionProperty, value); }
    /// <summary>Small keyboard increment. The default is five seconds.</summary>
    public TimeSpan SeekStep { get => GetValue(SeekStepProperty); set => SetValue(SeekStepProperty, value); }
    /// <summary>Whether elapsed and end-time labels are shown.</summary>
    public bool ShowTimeLabels { get => GetValue(ShowTimeLabelsProperty); set => SetValue(ShowTimeLabelsProperty, value); }
    /// <summary>Shows the remaining duration as a negative value instead of the total duration.</summary>
    public bool ShowRemainingTime { get => GetValue(ShowRemainingTimeProperty); set => SetValue(ShowRemainingTimeProperty, value); }
    /// <summary>Whether the user can interact with the seek thumb.</summary>
    public bool IsSeekEnabled { get => GetValue(IsSeekEnabledProperty); set => SetValue(IsSeekEnabledProperty, value); }
    /// <summary>Optional command invoked with the requested target <see cref="TimeSpan"/>.</summary>
    public ICommand? SeekCommand { get => GetValue(SeekCommandProperty); set => SetValue(SeekCommandProperty, value); }
    /// <summary>Accessible name applied to the underlying slider.</summary>
    public string AutomationName { get => GetValue(AutomationNameProperty); set => SetValue(AutomationNameProperty, value); }

    /// <summary>Current position preview while the user is dragging or using the keyboard.</summary>
    public TimeSpan PreviewPosition => _previewPosition;
    /// <summary>True while a pointer or keyboard seek is in progress.</summary>
    public bool IsSeeking => _isSeeking;
    /// <summary>Normalized playback position, clamped to the inclusive zero-to-one range.</summary>
    public double ProgressFraction => _progressFraction;
    /// <summary>Normalized buffered position, clamped to the inclusive zero-to-one range.</summary>
    public double BufferedFraction => _bufferedFraction;
    /// <summary>Formatted elapsed or preview position.</summary>
    public string CurrentTimeText => _currentTimeText;
    /// <summary>Formatted total or remaining duration.</summary>
    public string EndTimeText => _endTimeText;

    /// <summary>Raised once when a user completes a seek gesture.</summary>
    public event EventHandler<MdSeekRequestedEventArgs>? SeekRequested;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachSlider();
        base.OnApplyTemplate(e);
        _slider = e.NameScope.Find<MdSlider>("PART_Slider");
        _bufferedTrack = e.NameScope.Find<Border>("PART_BufferedTrack");
        _previewIndicator = e.NameScope.Find<Border>("PART_PreviewIndicator");
        _previewLabel = e.NameScope.Find<TextBlock>("PART_PreviewLabel");
        _previewPresence.Initialize(false);
        UpdateMotion();
        if (_slider is not null)
        {
            _slider.PropertyChanged += OnSliderPropertyChanged;
            _slider.SizeChanged += OnSliderSizeChanged;
            _slider.AddHandler(KeyDownEvent, OnSliderKeyDown, RoutingStrategies.Bubble, handledEventsToo: true);
            _slider.AddHandler(KeyUpEvent, OnSliderKeyUp, RoutingStrategies.Bubble, handledEventsToo: true);
        }
        RefreshState();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        RefreshState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _previewPresence.Stop();
        _previewPresence.Initialize(false);
        base.OnDetachedFromVisualTree(e);
    }

    private void DetachSlider()
    {
        if (_slider is not null)
        {
            _slider.PropertyChanged -= OnSliderPropertyChanged;
            _slider.SizeChanged -= OnSliderSizeChanged;
            _slider.RemoveHandler(KeyDownEvent, OnSliderKeyDown);
            _slider.RemoveHandler(KeyUpEvent, OnSliderKeyUp);
        }
        _slider = null;
        _bufferedTrack = null;
        _previewIndicator = null;
        _previewLabel = null;
    }

    private void OnSliderPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (!_syncingSlider && e.Property == RangeBase.ValueProperty && _slider is not null && _isSeeking)
        {
            SetPreviewPosition(FromSliderValue(_slider.Value));
            RefreshState();
        }
    }

    private void OnSliderSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateBufferedTrack();
        UpdatePreviewIndicator();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_slider is null || !IsSourceWithin(e.Source, _slider) ||
            !e.GetCurrentPoint(_slider).Properties.IsLeftButtonPressed || !IsSeekEnabled || Duration <= TimeSpan.Zero)
            return;
        BeginSeeking();
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isSeeking) CommitSeek();
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_isSeeking) CommitSeek();
    }

    private void OnSliderKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsSeekKey(e.Key) || !IsSeekEnabled || Duration <= TimeSpan.Zero || _slider is null) return;
        if (!_isSeeking)
        {
            var keyboardPosition = FromSliderValue(_slider.Value);
            BeginSeeking();
            SetPreviewPosition(keyboardPosition);
            RefreshState();
        }
    }

    private void OnSliderKeyUp(object? sender, KeyEventArgs e)
    {
        if (_isSeeking && IsSeekKey(e.Key)) CommitSeek();
    }

    private void BeginSeeking()
    {
        if (_isSeeking) return;
        SetIsSeeking(true);
        SetPreviewPosition(Clamp(Position, Duration));
        RefreshState();
    }

    private void CommitSeek()
    {
        if (!_isSeeking) return;
        var requested = _slider is null ? Clamp(PreviewPosition, Duration) : FromSliderValue(_slider.Value);
        requested = Clamp(requested, Duration);
        SetIsSeeking(false);
        SetCurrentValue(PositionProperty, requested);
        SetPreviewPosition(requested);
        RefreshState();
        MdMediaControlHelpers.RequestSeek(this, SeekCommand, SeekRequested, requested);
    }

    private void RefreshState()
    {
        if (Duration < TimeSpan.Zero)
        {
            SetCurrentValue(DurationProperty, TimeSpan.Zero);
            return;
        }

        var duration = Duration;
        var position = Clamp(Position, duration);
        if (position != Position)
        {
            SetCurrentValue(PositionProperty, position);
            return;
        }

        var buffered = Clamp(BufferedPosition, duration);
        if (buffered != BufferedPosition)
        {
            SetCurrentValue(BufferedPositionProperty, buffered);
            return;
        }

        if (!_isSeeking)
            SetPreviewPosition(position);
        else
            SetPreviewPosition(Clamp(PreviewPosition, duration));

        var displayedPosition = _isSeeking ? PreviewPosition : position;
        var progress = duration.Ticks > 0 ? Math.Clamp((double)displayedPosition.Ticks / duration.Ticks, 0, 1) : 0;
        var bufferedFraction = duration.Ticks > 0 ? Math.Clamp((double)buffered.Ticks / duration.Ticks, 0, 1) : 0;
        SetAndRaise(ProgressFractionProperty, ref _progressFraction, progress);
        SetAndRaise(BufferedFractionProperty, ref _bufferedFraction, bufferedFraction);
        SetAndRaise(CurrentTimeTextProperty, ref _currentTimeText, FormatTime(displayedPosition));
        var endText = ShowRemainingTime ? $"-{FormatTime(duration - displayedPosition)}" : FormatTime(duration);
        SetAndRaise(EndTimeTextProperty, ref _endTimeText, endText);

        UpdateSlider();
        UpdateBufferedTrack();
        UpdatePreviewIndicator();
        UpdateAutomation();
    }

    private void UpdateSlider()
    {
        if (_slider is null) return;
        var durationSeconds = Math.Max(1, Duration.TotalSeconds);
        var positionSeconds = (_isSeeking ? PreviewPosition : Position).TotalSeconds;
        var stepSeconds = Math.Max(0.1, SeekStep.TotalSeconds);
        _syncingSlider = true;
        try
        {
            _slider.SetCurrentValue(RangeBase.MinimumProperty, 0d);
            _slider.SetCurrentValue(RangeBase.MaximumProperty, durationSeconds);
            _slider.SetCurrentValue(RangeBase.SmallChangeProperty, stepSeconds);
            _slider.SetCurrentValue(RangeBase.LargeChangeProperty, Math.Max(stepSeconds * 6, stepSeconds));
            _slider.SetCurrentValue(RangeBase.ValueProperty, Math.Clamp(positionSeconds, 0, durationSeconds));
            _slider.SetCurrentValue(InputElement.IsEnabledProperty, IsEnabled && IsSeekEnabled && Duration > TimeSpan.Zero);
        }
        finally
        {
            _syncingSlider = false;
        }
    }

    private void UpdateBufferedTrack()
    {
        if (_slider is null || _bufferedTrack is null) return;
        var durationTicks = Math.Max(0, Duration.Ticks);
        var start = Clamp(_isSeeking ? PreviewPosition : Position, Duration);
        var end = Clamp(BufferedPosition, Duration);
        var availableWidth = Math.Max(0, _slider.Bounds.Width - 8);
        var visible = durationTicks > 0 && availableWidth > 0 && end > start;
        _bufferedTrack.SetCurrentValue(IsVisibleProperty, visible);
        _bufferedTrack.SetCurrentValue(WidthProperty,
            visible ? availableWidth * (double)(end.Ticks - start.Ticks) / durationTicks : 0);
        Canvas.SetLeft(_bufferedTrack,
            4 + (durationTicks > 0 ? availableWidth * (double)start.Ticks / durationTicks : 0));
        Canvas.SetTop(_bufferedTrack, Math.Max(0, _slider.Bounds.Height / 2 + 8));
    }

    private void UpdatePreviewIndicator()
    {
        PseudoClasses.Set(":seeking", _isSeeking);
        if (_previewIndicator is null || _previewLabel is null || _slider is null ||
            !_isSeeking || !IsEnabled || !IsSeekEnabled || Duration <= TimeSpan.Zero)
        {
            if (_previewIndicator is not null) _previewIndicator.Opacity = 0;
            _previewPresence.Update(false, GetEffectsDuration());
            return;
        }

        _previewLabel.Text = FormatTime(PreviewPosition);
        var sliderWidth = _slider.Bounds.Width;
        var indicatorWidth = _previewIndicator.Width;
        var availableWidth = Math.Max(0, sliderWidth - 8);
        var fraction = Duration.Ticks > 0
            ? Math.Clamp((double)PreviewPosition.Ticks / Duration.Ticks, 0, 1)
            : 0;
        var center = 4 + availableWidth * fraction;
        Canvas.SetLeft(_previewIndicator, Math.Clamp(center - indicatorWidth / 2, 0, Math.Max(0, sliderWidth - indicatorWidth)));
        Canvas.SetTop(_previewIndicator, 0);

        var needsFadeIn = !_previewPresence.IsPresent || _previewIndicator.Opacity < 1;
        _previewPresence.Update(true, TimeSpan.Zero);
        if (!needsFadeIn) return;
        if (_previewIndicator.Opacity >= 1) _previewIndicator.Opacity = 0;
        Dispatcher.UIThread.Post(() =>
        {
            if (_isSeeking && _previewIndicator is not null)
                _previewIndicator.Opacity = 1;
        }, DispatcherPriority.Render);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_previewIndicator is not null)
        {
            _previewIndicator.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, Visual.OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
            if (!_isSeeking)
                _previewPresence.Update(false, GetEffectsDuration());
        }
    }

    private TimeSpan GetEffectsDuration() =>
        MdMotion.Resolve(this, MdMotionKind.Effects, MdMotionSpeed.Fast).Duration;

    private void UpdateAutomation()
    {
        if (_slider is null) return;
        AutomationProperties.SetName(_slider, string.IsNullOrWhiteSpace(AutomationName) ? "Seek position" : AutomationName);
        AutomationProperties.SetHelpText(_slider, $"{CurrentTimeText} of {EndTimeText}");
    }

    private void SetPreviewPosition(TimeSpan value) =>
        SetAndRaise(PreviewPositionProperty, ref _previewPosition, value);

    private void SetIsSeeking(bool value) =>
        SetAndRaise(IsSeekingProperty, ref _isSeeking, value);

    private static bool IsSourceWithin(object? source, Control ancestor)
    {
        if (source is not Visual visual) return false;
        for (Visual? current = visual; current is not null; current = current.GetVisualParent())
            if (ReferenceEquals(current, ancestor)) return true;
        return false;
    }

    private TimeSpan FromSliderValue(double seconds)
    {
        if (!double.IsFinite(seconds) || seconds <= 0) return TimeSpan.Zero;
        if (seconds >= TimeSpan.MaxValue.TotalSeconds) return TimeSpan.MaxValue;
        return Clamp(TimeSpan.FromSeconds(seconds), Duration);
    }

    private static TimeSpan Clamp(TimeSpan value, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero || value <= TimeSpan.Zero) return TimeSpan.Zero;
        return value > duration ? duration : value;
    }

    private static string FormatTime(TimeSpan value)
    {
        var totalSeconds = Math.Max(0, (long)Math.Floor(value.TotalSeconds));
        var hours = totalSeconds / 3600;
        var minutes = totalSeconds / 60 % 60;
        var seconds = totalSeconds % 60;
        return hours > 0
            ? string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}:{2:00}", hours, minutes, seconds)
            : string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}", totalSeconds / 60, seconds);
    }

    private static bool IsSeekKey(Key key) => key is
        Key.Left or Key.Right or Key.Up or Key.Down or Key.PageUp or Key.PageDown or Key.Home or Key.End;
}
