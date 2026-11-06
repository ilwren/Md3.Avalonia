using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>Placement of the elapsed and total media time labels.</summary>
public enum MdMediaTimeLabelPlacement
{
    /// <summary>Labels appear below the track.</summary>
    Below,
    /// <summary>Labels appear at either end of the track.</summary>
    Sides,
    /// <summary>Time labels are hidden.</summary>
    Hidden
}

/// <summary>A seekable media timeline with played and buffered segments. Values are measured in seconds.</summary>
/// <remarks>
/// The host owns the media player and supplies <see cref="RangeBase.Value"/>,
/// <see cref="RangeBase.Maximum"/> and <see cref="BufferedSeconds"/>. Pointer scrubbing updates
/// the two-way value while dragging and raises <see cref="SeekRequested"/> on release; keyboard
/// and automation requests raise it immediately. Programmatic value updates do not request a seek.
/// There is no timer or autonomous animation.
/// </remarks>
[PseudoClasses(":read-only")]
public sealed class MdMediaProgressBar : RangeBase
{
    public static readonly StyledProperty<double> BufferedSecondsProperty = AvaloniaProperty.Register<MdMediaProgressBar, double>(
        nameof(BufferedSeconds), coerce: static (_, value) => double.IsFinite(value) ? Math.Max(0, value) : 0);
    public static readonly StyledProperty<bool> IsReadOnlyProperty = AvaloniaProperty.Register<MdMediaProgressBar, bool>(nameof(IsReadOnly));
    public static readonly StyledProperty<MdMediaTimeLabelPlacement> LabelPlacementProperty =
        AvaloniaProperty.Register<MdMediaProgressBar, MdMediaTimeLabelPlacement>(nameof(LabelPlacement));
    public static readonly StyledProperty<bool> ShowRemainingTimeProperty = AvaloniaProperty.Register<MdMediaProgressBar, bool>(nameof(ShowRemainingTime));
    public static readonly StyledProperty<IBrush?> PlayedBrushProperty = AvaloniaProperty.Register<MdMediaProgressBar, IBrush?>(nameof(PlayedBrush));
    public static readonly StyledProperty<IBrush?> BufferedBrushProperty = AvaloniaProperty.Register<MdMediaProgressBar, IBrush?>(nameof(BufferedBrush));
    public static readonly StyledProperty<IBrush?> TrackBrushProperty = AvaloniaProperty.Register<MdMediaProgressBar, IBrush?>(nameof(TrackBrush));
    public static readonly StyledProperty<IBrush?> LabelBrushProperty = AvaloniaProperty.Register<MdMediaProgressBar, IBrush?>(nameof(LabelBrush));

    public static readonly DirectProperty<MdMediaProgressBar, bool> IsScrubbingProperty =
        AvaloniaProperty.RegisterDirect<MdMediaProgressBar, bool>(nameof(IsScrubbing), control => control.IsScrubbing);

    private bool _isScrubbing;

    static MdMediaProgressBar()
    {
        AffectsRender<MdMediaProgressBar>(ValueProperty, MinimumProperty, MaximumProperty, BufferedSecondsProperty,
            LabelPlacementProperty, ShowRemainingTimeProperty, PlayedBrushProperty, BufferedBrushProperty,
            TrackBrushProperty, LabelBrushProperty, FlowDirectionProperty, FontFamilyProperty, FontSizeProperty,
            FontStyleProperty, FontWeightProperty);
        AffectsMeasure<MdMediaProgressBar>(LabelPlacementProperty, FontSizeProperty);
        IsReadOnlyProperty.Changed.AddClassHandler<MdMediaProgressBar>((control, _) =>
            control.PseudoClasses.Set(":read-only", control.IsReadOnly));
    }

    public MdMediaProgressBar()
    {
        Maximum = 0;
        SmallChange = 5;
        LargeChange = 30;
        PseudoClasses.Set(":read-only", IsReadOnly);
        AutomationProperties.SetName(this, "Media progress");
        GotFocus += (_, _) => InvalidateVisual();
        LostFocus += (_, _) => InvalidateVisual();
    }

    /// <summary>Buffered position in seconds; the displayed segment is clamped to the current range.</summary>
    public double BufferedSeconds { get => GetValue(BufferedSecondsProperty); set => SetValue(BufferedSecondsProperty, value); }
    /// <summary>When true, pointer, keyboard and automation cannot seek.</summary>
    public bool IsReadOnly { get => GetValue(IsReadOnlyProperty); set => SetValue(IsReadOnlyProperty, value); }
    /// <summary>True while a pointer seek is in progress. Hosts can suspend playback updates until release.</summary>
    public bool IsScrubbing { get => _isScrubbing; private set => SetAndRaise(IsScrubbingProperty, ref _isScrubbing, value); }
    /// <summary>Location of the elapsed and total labels.</summary>
    public MdMediaTimeLabelPlacement LabelPlacement { get => GetValue(LabelPlacementProperty); set => SetValue(LabelPlacementProperty, value); }
    /// <summary>Shows time remaining rather than total time on the trailing label.</summary>
    public bool ShowRemainingTime { get => GetValue(ShowRemainingTimeProperty); set => SetValue(ShowRemainingTimeProperty, value); }
    public IBrush? PlayedBrush { get => GetValue(PlayedBrushProperty); set => SetValue(PlayedBrushProperty, value); }
    public IBrush? BufferedBrush { get => GetValue(BufferedBrushProperty); set => SetValue(BufferedBrushProperty, value); }
    public IBrush? TrackBrush { get => GetValue(TrackBrushProperty); set => SetValue(TrackBrushProperty, value); }
    public IBrush? LabelBrush { get => GetValue(LabelBrushProperty); set => SetValue(LabelBrushProperty, value); }

    /// <summary>Raised after a user or automation seek, in seconds. Host-driven updates do not raise it.</summary>
    public event EventHandler<double>? SeekRequested;

    /// <summary>Formats a non-negative duration as m:ss or h:mm:ss, using whole seconds.</summary>
    public static string FormatTimestamp(double seconds)
    {
        if (!double.IsFinite(seconds) || seconds < 0) seconds = 0;
        var whole = (long)Math.Min(Math.Floor(seconds), 315_537_897_599d);
        var hours = whole / 3600;
        var minutes = whole / 60 % 60;
        var remainder = whole % 60;
        return hours > 0
            ? string.Create(CultureInfo.InvariantCulture, $"{hours}:{minutes:00}:{remainder:00}")
            : string.Create(CultureInfo.InvariantCulture, $"{whole / 60}:{remainder:00}");
    }

    protected override Size MeasureOverride(Size availableSize) =>
        new(240, LabelPlacement == MdMediaTimeLabelPlacement.Below ? 64 : 48);

    private FormattedText MakeLabel(string text) => new(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
        new Typeface(FontFamily, FontStyle, FontWeight), Math.Max(1, FontSize), LabelBrush ?? Brushes.Black);

    private (double Left, double Right, double Y, FormattedText Elapsed, FormattedText End) Metrics()
    {
        var elapsed = MakeLabel(FormatTimestamp(Value));
        var end = MakeLabel(ShowRemainingTime ? "−" + FormatTimestamp(Math.Max(0, Maximum - Value)) : FormatTimestamp(Maximum));
        var left = 10d;
        var right = Math.Max(left, Bounds.Width - 10);
        if (LabelPlacement == MdMediaTimeLabelPlacement.Sides)
        {
            var rtl = FlowDirection == FlowDirection.RightToLeft;
            left += (rtl ? end.Width : elapsed.Width) + 12;
            right -= (rtl ? elapsed.Width : end.Width) + 12;
            if (right < left) left = right = Bounds.Width / 2;
        }
        return (left, right, LabelPlacement == MdMediaTimeLabelPlacement.Below ? 20 : Bounds.Height / 2, elapsed, end);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.DrawRectangle(Brushes.Transparent, null, new Rect(Bounds.Size));
        var (left, right, y, elapsed, end) = Metrics();
        var width = Math.Max(0, right - left);
        var range = Maximum - Minimum;
        var played = range > 0 ? Math.Clamp((Value - Minimum) / range, 0, 1) : 0;
        var buffered = range > 0 ? Math.Clamp((BufferedSeconds - Minimum) / range, 0, 1) : 0;
        var rtl = FlowDirection == FlowDirection.RightToLeft;
        var track = TrackBrush ?? Brushes.Gray;
        var active = PlayedBrush ?? Brushes.Black;
        var bufferedBrush = BufferedBrush ?? Brushes.LightGray;
        DrawSegment(context, track, left, y, width);
        DrawSegment(context, bufferedBrush, rtl ? right - width * buffered : left, y, width * buffered);
        DrawSegment(context, active, rtl ? right - width * played : left, y, width * played);
        context.DrawEllipse(active, null, new Point(rtl ? right - width * played : left + width * played, y), 6, 6);
        if (IsKeyboardFocusWithin && !IsReadOnly)
            context.DrawRectangle(null, new Pen(active, 2), new Rect(1, 1, Math.Max(0, Bounds.Width - 2), Math.Max(0, Bounds.Height - 2)), 8);

        if (LabelPlacement == MdMediaTimeLabelPlacement.Hidden) return;
        var labelY = LabelPlacement == MdMediaTimeLabelPlacement.Below ? y + 12 : y - Math.Max(elapsed.Height, end.Height) / 2;
        var elapsedX = rtl ? Math.Max(0, Bounds.Width - elapsed.Width) : 0;
        var endX = rtl ? 0 : Math.Max(0, Bounds.Width - end.Width);
        context.DrawText(elapsed, new Point(elapsedX, labelY));
        context.DrawText(end, new Point(endX, labelY));
    }

    private static void DrawSegment(DrawingContext context, IBrush brush, double x, double y, double width)
    {
        if (width <= 0) return;
        context.DrawRectangle(brush, null, new RoundedRect(new Rect(x, y - 2, width, 4), 2));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (IsReadOnly || Maximum <= Minimum ||
            (e.Pointer.Type == PointerType.Mouse && !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)) return;
        IsScrubbing = true;
        Focus();
        e.Pointer.Capture(this);
        UpdateFromPointer(e.GetPosition(this).X);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!IsScrubbing || !ReferenceEquals(e.Pointer.Captured, this)) return;
        UpdateFromPointer(e.GetPosition(this).X);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!IsScrubbing) return;
        UpdateFromPointer(e.GetPosition(this).X);
        IsScrubbing = false;
        if (ReferenceEquals(e.Pointer.Captured, this)) e.Pointer.Capture(null);
        if (!IsReadOnly && Maximum > Minimum) SeekRequested?.Invoke(this, Value);
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        IsScrubbing = false;
        base.OnPointerCaptureLost(e);
    }

    private void UpdateFromPointer(double x)
    {
        if (IsReadOnly || Maximum <= Minimum) return;
        var (left, right, _, _, _) = Metrics();
        if (right <= left) return;
        var fraction = Math.Clamp((x - left) / (right - left), 0, 1);
        if (FlowDirection == FlowDirection.RightToLeft) fraction = 1 - fraction;
        SetCurrentValue(ValueProperty, Minimum + fraction * (Maximum - Minimum));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (IsReadOnly || Maximum <= Minimum) { base.OnKeyDown(e); return; }
        var rtl = FlowDirection == FlowDirection.RightToLeft;
        double target;
        if (e.Key == Key.Right) target = Value + (rtl ? -SmallChange : SmallChange);
        else if (e.Key == Key.Left) target = Value + (rtl ? SmallChange : -SmallChange);
        else if (e.Key == Key.Up) target = Value + SmallChange;
        else if (e.Key == Key.Down) target = Value - SmallChange;
        else if (e.Key == Key.PageUp) target = Value + LargeChange;
        else if (e.Key == Key.PageDown) target = Value - LargeChange;
        else if (e.Key == Key.Home) target = Minimum;
        else if (e.Key == Key.End) target = Maximum;
        else { base.OnKeyDown(e); return; }
        RequestSeek(target);
        e.Handled = true;
    }

    internal void RequestSeek(double seconds)
    {
        if (IsReadOnly || Maximum <= Minimum || !double.IsFinite(seconds)) return;
        SetCurrentValue(ValueProperty, Math.Clamp(seconds, Minimum, Maximum));
        SeekRequested?.Invoke(this, Value);
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new MdMediaProgressBarAutomationPeer(this);
}

internal sealed class MdMediaProgressBarAutomationPeer(MdMediaProgressBar owner) : ControlAutomationPeer(owner), IRangeValueProvider
{
    private MdMediaProgressBar Media => (MdMediaProgressBar)Owner;
    public bool IsReadOnly => Media.IsReadOnly || Media.Maximum <= Media.Minimum;
    public double Minimum => Media.Minimum;
    public double Maximum => Media.Maximum;
    public double Value => Media.Value;
    public double LargeChange => Media.LargeChange;
    public double SmallChange => Media.SmallChange;
    public void SetValue(double value) { EnsureEnabled(); Media.RequestSeek(value); }
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Slider;
    protected override bool IsControlElementCore() => true;
    protected override bool IsContentElementCore() => true;
}
