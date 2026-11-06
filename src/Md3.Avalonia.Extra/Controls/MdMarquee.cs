using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>Physical scrolling direction; Auto follows the text flow direction.</summary>
public enum MdMarqueeDirection
{
    /// <summary>Scroll left for left-to-right text and right for right-to-left text.</summary>
    Auto,
    /// <summary>Scroll towards the left edge.</summary>
    Left,
    /// <summary>Scroll towards the right edge.</summary>
    Right
}

/// <summary>A single-line plain-text marquee that scrolls only when its content overflows.</summary>
/// <remarks>
/// Reduced and None motion, an inactive or hidden control, and detached visuals use a static
/// trimmed fallback. The full text is exposed once through the text automation peer. The render
/// loop is requested only while active; no dispatcher timer is retained after detachment.
/// </remarks>
public sealed class MdMarquee : TemplatedControl
{
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<MdMarquee, string?>(nameof(Text));
    public static readonly StyledProperty<bool> IsActiveProperty = AvaloniaProperty.Register<MdMarquee, bool>(nameof(IsActive), true);
    public static readonly StyledProperty<MdMarqueeDirection> DirectionProperty = AvaloniaProperty.Register<MdMarquee, MdMarqueeDirection>(nameof(Direction));
    public static readonly StyledProperty<double> SpeedProperty = AvaloniaProperty.Register<MdMarquee, double>(
        nameof(Speed), 40, coerce: static (_, value) => double.IsFinite(value) ? Math.Clamp(value, 0, 10000) : 40);
    public static readonly StyledProperty<double> GapProperty = AvaloniaProperty.Register<MdMarquee, double>(
        nameof(Gap), 24, coerce: static (_, value) => double.IsFinite(value) ? Math.Clamp(value, 0, 10000) : 24);
    public static readonly StyledProperty<TimeSpan> PauseAfterRoundProperty = AvaloniaProperty.Register<MdMarquee, TimeSpan>(
        nameof(PauseAfterRound), coerce: static (_, value) => value < TimeSpan.Zero ? TimeSpan.Zero : value > TimeSpan.FromHours(1) ? TimeSpan.FromHours(1) : value);
    public static readonly StyledProperty<TextTrimming> FallbackTrimmingProperty = AvaloniaProperty.Register<MdMarquee, TextTrimming>(
        nameof(FallbackTrimming), TextTrimming.CharacterEllipsis);
    public static readonly DirectProperty<MdMarquee, bool> IsAnimatingProperty =
        AvaloniaProperty.RegisterDirect<MdMarquee, bool>(nameof(IsAnimating), owner => owner.IsAnimating);

    private readonly List<(Visual Visual, EventHandler<AvaloniaPropertyChangedEventArgs> Handler)> _ancestors = [];
    private TextLayout? _layout;
    private string _line = string.Empty;
    private double _textWidth;
    private double _viewportWidth;
    private double _viewportHeight;
    private double _offset;
    private double _pauseRemaining;
    private bool _isAnimating;
    private bool _attached;
    private bool _frameRequested;
    private bool _lastFrameValid;
    private TimeSpan _lastFrame;
    private int _generation;

    static MdMarquee()
    {
        AffectsMeasure<MdMarquee>(TextProperty, FontFamilyProperty, FontSizeProperty, FontWeightProperty,
            FontStyleProperty, FontStretchProperty, FlowDirectionProperty, PaddingProperty);
        AffectsRender<MdMarquee>(TextProperty, ForegroundProperty, FontFamilyProperty, FontSizeProperty,
            FontWeightProperty, FontStyleProperty, FontStretchProperty, FlowDirectionProperty, PaddingProperty,
            FallbackTrimmingProperty, DirectionProperty);
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdMarquee>((control, _) => control.UpdateAnimation());
    }

    /// <summary>Full plain text. Line separators are displayed as spaces; the automation name retains the original.</summary>
    public string? Text { get => GetValue(TextProperty); set => SetValue(TextProperty, value); }
    /// <summary>Allows animation when all other conditions are met. False shows a static fallback.</summary>
    public bool IsActive { get => GetValue(IsActiveProperty); set => SetValue(IsActiveProperty, value); }
    /// <summary>Physical scroll direction, or Auto to follow flow direction.</summary>
    public MdMarqueeDirection Direction { get => GetValue(DirectionProperty); set => SetValue(DirectionProperty, value); }
    /// <summary>Constant speed in DIP per second, coerced to 0–10000. Zero stops the marquee.</summary>
    public double Speed { get => GetValue(SpeedProperty); set => SetValue(SpeedProperty, value); }
    /// <summary>Space in DIP between consecutive copies, coerced to 0–10000.</summary>
    public double Gap { get => GetValue(GapProperty); set => SetValue(GapProperty, value); }
    /// <summary>Time to hold the starting position after each round, coerced to zero through one hour.</summary>
    public TimeSpan PauseAfterRound { get => GetValue(PauseAfterRoundProperty); set => SetValue(PauseAfterRoundProperty, value); }
    /// <summary>Trimming used when motion is inactive or not permitted.</summary>
    public TextTrimming FallbackTrimming { get => GetValue(FallbackTrimmingProperty); set => SetValue(FallbackTrimmingProperty, value); }
    /// <summary>Whether the render loop is active, including pauses between rounds.</summary>
    public bool IsAnimating => _isAnimating;

    protected override bool BypassFlowDirectionPolicies => true;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TextProperty)
            _line = (Text ?? string.Empty).Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ').Replace('\u0085', ' ').Replace('\u2028', ' ').Replace('\u2029', ' ');

        if (change.Property == TextProperty || change.Property == FontFamilyProperty || change.Property == FontSizeProperty ||
            change.Property == FontWeightProperty || change.Property == FontStyleProperty || change.Property == FontStretchProperty ||
            change.Property == FlowDirectionProperty || change.Property == ForegroundProperty)
        {
            _layout?.Dispose();
            _layout = null;
        }

        if (change.Property == TextProperty || change.Property == FontFamilyProperty || change.Property == FontSizeProperty ||
            change.Property == FontWeightProperty || change.Property == FontStyleProperty || change.Property == FontStretchProperty ||
            change.Property == FlowDirectionProperty || change.Property == PaddingProperty || change.Property == IsActiveProperty ||
            change.Property == DirectionProperty || change.Property == SpeedProperty || change.Property == GapProperty ||
            change.Property == PauseAfterRoundProperty)
        {
            // Never keep animating against stale metrics between a font/text change and arrange.
            if (change.Property == TextProperty || change.Property == FontFamilyProperty || change.Property == FontSizeProperty ||
                change.Property == FontWeightProperty || change.Property == FontStyleProperty || change.Property == FontStretchProperty ||
                change.Property == FlowDirectionProperty || change.Property == PaddingProperty)
                _textWidth = 0;
            ResetPosition();
            UpdateAnimation();
        }
        if (change.Property == IsVisibleProperty) UpdateAnimation();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var layout = GetLayout();
        return new Size(layout.WidthIncludingTrailingWhitespace, layout.Height).Inflate(Padding);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = finalSize.Deflate(Padding);
        var width = Math.Max(0, size.Width);
        var height = Math.Max(0, size.Height);
        var textWidth = GetLayout().WidthIncludingTrailingWhitespace;
        if (_viewportWidth != width || _viewportHeight != height || _textWidth != textWidth)
        {
            _viewportWidth = width;
            _viewportHeight = height;
            _textWidth = textWidth;
            ResetPosition();
        }
        UpdateAnimation();
        return finalSize;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_viewportWidth <= 0 || _viewportHeight <= 0 || _line.Length == 0) return;
        var clip = new Rect(Padding.Left, Padding.Top, _viewportWidth, _viewportHeight);
        using (context.PushClip(clip))
        {
            if (!IsAnimating)
            {
                using var fallback = new TextLayout(_line, Typeface(), FontSize, Foreground, TextAlignment.Start,
                    TextWrapping.NoWrap, FallbackTrimming, flowDirection: FlowDirection, maxWidth: _viewportWidth, maxLines: 1);
                fallback.Draw(context, new Point(Padding.Left, Padding.Top + Math.Max(0, (_viewportHeight - fallback.Height) / 2)));
                return;
            }

            var layout = GetLayout();
            var y = Padding.Top + Math.Max(0, (_viewportHeight - layout.Height) / 2);
            var (first, second) = GetOrigins();
            layout.Draw(context, new Point(Padding.Left + first, y));
            layout.Draw(context, new Point(Padding.Left + second, y));
        }
    }

    private Typeface Typeface() => new(FontFamily, FontStyle, FontWeight, FontStretch);
    private TextLayout GetLayout() => _layout ??= new TextLayout(_line, Typeface(), FontSize, Foreground,
        TextAlignment.Left, TextWrapping.NoWrap, TextTrimming.None, flowDirection: FlowDirection, maxLines: 1);

    internal (double First, double Second) GetOrigins()
    {
        var initial = FlowDirection == FlowDirection.RightToLeft ? _viewportWidth - _textWidth : 0;
        var right = Direction == MdMarqueeDirection.Right ||
                    (Direction == MdMarqueeDirection.Auto && FlowDirection == FlowDirection.RightToLeft);
        var first = initial + (right ? _offset : -_offset);
        return (first, first + (right ? -1 : 1) * (_textWidth + Gap));
    }

    private void ResetPosition()
    {
        _offset = 0;
        _pauseRemaining = 0;
        _lastFrameValid = false;
        InvalidateVisual();
    }

    private void UpdateAnimation()
    {
        var scheme = MdMotion.GetScheme(this);
        var shouldRun = IsActive && Speed > 0 && _line.Length > 0 && _viewportWidth > 0 && _viewportHeight > 0 &&
                        _textWidth > _viewportWidth + 0.01 && IsEffectivelyVisible && _attached &&
                        scheme != MdMotionScheme.None && scheme != MdMotionScheme.Reduced && TopLevel.GetTopLevel(this) is not null;
        if (SetAndRaise(IsAnimatingProperty, ref _isAnimating, shouldRun)) InvalidateVisual();
        if (shouldRun) RequestFrame();
        else
        {
            _generation++;
            _frameRequested = false;
            ResetPosition();
        }
    }

    private void RequestFrame()
    {
        if (_frameRequested || TopLevel.GetTopLevel(this) is not { } topLevel) return;
        _frameRequested = true;
        var generation = _generation;
        topLevel.RequestAnimationFrame(timestamp => OnFrame(timestamp, generation));
    }

    private void OnFrame(TimeSpan timestamp, int generation)
    {
        if (generation != _generation) return;
        _frameRequested = false;
        if (!IsAnimating) return;
        if (_lastFrameValid)
        {
            // A minimized or stalled top-level must not jump through whole rounds on its return.
            var delta = Math.Clamp((timestamp - _lastFrame).TotalSeconds, 0, 0.1);
            Advance(delta);
        }
        _lastFrame = timestamp;
        _lastFrameValid = true;
        RequestFrame();
    }

    // The same deterministic time-step routine is exercised by the headless tests.
    internal void Advance(double seconds)
    {
        if (!IsAnimating || !double.IsFinite(seconds) || seconds <= 0) return;
        var distance = _textWidth + Gap;
        if (distance <= 0 || Speed <= 0) return;
        if (_pauseRemaining > 0)
        {
            var consumed = Math.Min(seconds, _pauseRemaining);
            _pauseRemaining -= consumed;
            seconds -= consumed;
        }
        if (seconds <= 0) return;
        var remainingTravel = (distance - _offset) / Speed;
        if (seconds < remainingTravel) _offset += Speed * seconds;
        else
        {
            seconds -= remainingTravel;
            _offset = 0;
            var pause = PauseAfterRound.TotalSeconds;
            var period = distance / Speed + pause;
            if (period > 0) seconds %= period;
            if (seconds < pause) _pauseRemaining = pause - seconds;
            else _offset = Speed * (seconds - pause);
        }
        InvalidateVisual();
    }

    internal double ScrollOffset => _offset;
    internal double RemainingPause => _pauseRemaining;
    internal double NaturalTextWidth => _textWidth;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attached = true;
        UnsubscribeAncestors();
        foreach (var ancestor in this.GetVisualAncestors().OfType<Visual>())
        {
            EventHandler<AvaloniaPropertyChangedEventArgs> handler = (_, change) =>
            {
                if (change.Property == IsVisibleProperty) UpdateAnimation();
            };
            ancestor.PropertyChanged += handler;
            _ancestors.Add((ancestor, handler));
        }
        UpdateAnimation();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _attached = false;
        UnsubscribeAncestors();
        base.OnDetachedFromVisualTree(e);
        UpdateAnimation();
        _layout?.Dispose();
        _layout = null;
    }

    private void UnsubscribeAncestors()
    {
        foreach (var (visual, handler) in _ancestors) visual.PropertyChanged -= handler;
        _ancestors.Clear();
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new MdMarqueeAutomationPeer(this);
}

internal sealed class MdMarqueeAutomationPeer : ControlAutomationPeer
{
    public MdMarqueeAutomationPeer(MdMarquee owner) : base(owner)
    {
        owner.PropertyChanged += (_, e) =>
        {
            if (e.Property == MdMarquee.TextProperty && string.IsNullOrWhiteSpace(base.GetNameCore()))
                RaisePropertyChangedEvent(AutomationElementIdentifiers.NameProperty, e.OldValue, e.NewValue);
        };
    }

    protected override string? GetNameCore()
    {
        var name = base.GetNameCore();
        return string.IsNullOrWhiteSpace(name) ? ((MdMarquee)Owner).Text : name;
    }
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Text;
}
