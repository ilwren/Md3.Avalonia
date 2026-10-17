using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>
/// A Material waveform: one bar per peak, mirrored around the centre line by default, with the
/// played part in Primary and the rest in SurfaceContainerHighest - the same split a Material 3
/// slider makes between its active and inactive track.
/// </summary>
/// <remarks>
/// <para>
/// The control draws peaks it is handed; it does not decode audio. A waveform that owned a decoder
/// would have to own a file format, a threading model and a lifetime for the decoded buffer, and
/// none of that is presentation. Hosts produce peaks from whatever they already play with, and
/// <see cref="PeaksFromSamples"/> covers the common case of reducing a sample buffer down to bars.
/// </para>
/// <para>
/// There is nothing to animate here, so there is nothing for a reduced-motion scheme to reduce: the
/// playhead moves because the user moved it, and a transition on it would only add latency to a
/// scrub. Position is two-way bindable, so a player view model drives the playhead during playback
/// and receives seeks back.
/// </para>
/// </remarks>
[PseudoClasses(":pointerover", ":pressed", ":non-interactive")]
public sealed class MdWaveform : Control
{
    /// <summary>One normalized peak per bar, 0 to 1. Null or empty draws just the baseline.</summary>
    public static readonly StyledProperty<IReadOnlyList<float>?> PeaksProperty =
        AvaloniaProperty.Register<MdWaveform, IReadOnlyList<float>?>(nameof(Peaks));

    /// <summary>How far playback has got, 0 to 1. Two-way, so scrubbing reports back.</summary>
    // Coerced rather than clamped in the setter: a binding writes the property directly and would
    // otherwise park it outside 0..1, where the playhead draws past the end of the waveform.
    public static readonly StyledProperty<double> PlayheadPositionProperty =
        AvaloniaProperty.Register<MdWaveform, double>(nameof(PlayheadPosition), 0,
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay,
            coerce: static (_, value) => Math.Clamp(value, 0, 1));

    /// <summary>Whether pointer and keyboard input move the playhead.</summary>
    public static readonly StyledProperty<bool> IsInteractiveProperty =
        AvaloniaProperty.Register<MdWaveform, bool>(nameof(IsInteractive), true);

    /// <summary>Whether the playhead line is drawn at all.</summary>
    public static readonly StyledProperty<bool> ShowPlayheadProperty =
        AvaloniaProperty.Register<MdWaveform, bool>(nameof(ShowPlayhead), true);

    /// <summary>Mirror bars around the centre line, as an audio waveform does. False grows them up from the bottom.</summary>
    public static readonly StyledProperty<bool> IsMirroredProperty =
        AvaloniaProperty.Register<MdWaveform, bool>(nameof(IsMirrored), true);

    /// <summary>Space between bars, in DIP.</summary>
    public static readonly StyledProperty<double> BarGapProperty =
        AvaloniaProperty.Register<MdWaveform, double>(nameof(BarGap), 2);

    /// <summary>Corner rounding on each bar. The theme fills this from the shape scale.</summary>
    public static readonly StyledProperty<CornerRadius> BarCornerRadiusProperty =
        AvaloniaProperty.Register<MdWaveform, CornerRadius>(nameof(BarCornerRadius), new CornerRadius(1));

    /// <summary>Thickness of the playhead line, in DIP.</summary>
    public static readonly StyledProperty<double> PlayheadThicknessProperty =
        AvaloniaProperty.Register<MdWaveform, double>(nameof(PlayheadThickness), 2);

    /// <summary>Keyboard step for Left and Right, as a fraction of the whole waveform.</summary>
    public static readonly StyledProperty<double> KeyboardStepProperty =
        AvaloniaProperty.Register<MdWaveform, double>(nameof(KeyboardStep), .02);

    /// <summary>Bars the playhead has passed. Defaults to Primary.</summary>
    public static readonly StyledProperty<IBrush?> PlayedBrushProperty =
        AvaloniaProperty.Register<MdWaveform, IBrush?>(nameof(PlayedBrush));

    /// <summary>Bars still ahead of the playhead. Defaults to SurfaceContainerHighest.</summary>
    public static readonly StyledProperty<IBrush?> UnplayedBrushProperty =
        AvaloniaProperty.Register<MdWaveform, IBrush?>(nameof(UnplayedBrush));

    /// <summary>The playhead line and the handle that appears while it is held.</summary>
    public static readonly StyledProperty<IBrush?> PlayheadBrushProperty =
        AvaloniaProperty.Register<MdWaveform, IBrush?>(nameof(PlayheadBrush));

    /// <summary>Drawn across the middle when there are no peaks yet, so the space is not unexplained.</summary>
    public static readonly StyledProperty<IBrush?> BaselineBrushProperty =
        AvaloniaProperty.Register<MdWaveform, IBrush?>(nameof(BaselineBrush));

    /// <summary>Whether the playhead carries a knob on the centre line, the way a Material slider's
    /// handle sits on its track.</summary>
    public static readonly StyledProperty<bool> ShowPlayheadKnobProperty =
        AvaloniaProperty.Register<MdWaveform, bool>(nameof(ShowPlayheadKnob), true);

    /// <summary>Whether the bars pulse in place, which is what makes a static peak buffer read as a
    /// track that is playing rather than a picture of one.</summary>
    public static readonly StyledProperty<bool> IsPulsingProperty =
        AvaloniaProperty.Register<MdWaveform, bool>(nameof(IsPulsing));

    /// <summary>How far a bar dips at the trough of the pulse, as a fraction of its own height.</summary>
    public static readonly StyledProperty<double> AnimationAmplitudeProperty =
        AvaloniaProperty.Register<MdWaveform, double>(nameof(AnimationAmplitude), 0.4);

    /// <summary>How fast the pulse travels along the bars, in cycles per second.</summary>
    public static readonly StyledProperty<double> AnimationSpeedProperty =
        AvaloniaProperty.Register<MdWaveform, double>(nameof(AnimationSpeed), 1.2);

    // Fallbacks for hosts that draw the control without the theme, matching the M3 light scheme the
    // rest of the library falls back to.
    private static readonly IBrush FallbackPlayed = new SolidColorBrush(Color.FromRgb(103, 80, 164));
    private static readonly IBrush FallbackUnplayed = new SolidColorBrush(Color.FromRgb(202, 196, 208));

    private bool _pressed;
    private bool _frameRequested;

    /// <summary>
    /// Subscriptions to each ancestor's IsVisible. Avalonia exposes IsEffectivelyVisible only as a
    /// plain CLR property and keeps its change event internal, so a control is never told that an
    /// ancestor hid it. Watching the ancestors directly is the reachable equivalent, and it is what
    /// lets a pulse resume after a host - a collapsed floating window, a switched-away tab - shows
    /// the waveform again.
    /// </summary>
    private readonly List<IDisposable> _ancestorVisibility = new();
    private bool _lastFrameValid;
    private TimeSpan _lastFrame;
    private double _phase;

    static MdWaveform()
    {
        AffectsRender<MdWaveform>(PeaksProperty, PlayheadPositionProperty, ShowPlayheadProperty, IsMirroredProperty,
            BarGapProperty, BarCornerRadiusProperty, PlayheadThicknessProperty, PlayedBrushProperty,
            UnplayedBrushProperty, PlayheadBrushProperty, BaselineBrushProperty, ShowPlayheadKnobProperty,
            AnimationAmplitudeProperty, AnimationSpeedProperty);
        IsInteractiveProperty.Changed.AddClassHandler<MdWaveform>((waveform, _) => waveform.UpdatePseudoClasses());
        IsPulsingProperty.Changed.AddClassHandler<MdWaveform>((waveform, _) => waveform.UpdateAnimation());
        PeaksProperty.Changed.AddClassHandler<MdWaveform>((waveform, _) => waveform.UpdateAnimation());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdWaveform>((waveform, _) => waveform.UpdateAnimation());
    }

    /// <summary>One normalized peak per bar, 0 to 1.</summary>
    public IReadOnlyList<float>? Peaks
    {
        get => GetValue(PeaksProperty);
        set => SetValue(PeaksProperty, value);
    }

    /// <summary>How far playback has got, 0 to 1.</summary>
    public double PlayheadPosition
    {
        get => GetValue(PlayheadPositionProperty);
        set => SetValue(PlayheadPositionProperty, Math.Clamp(value, 0, 1));
    }

    /// <summary>Whether pointer and keyboard input move the playhead.</summary>
    public bool IsInteractive
    {
        get => GetValue(IsInteractiveProperty);
        set => SetValue(IsInteractiveProperty, value);
    }

    /// <summary>Whether the playhead line is drawn at all.</summary>
    public bool ShowPlayhead
    {
        get => GetValue(ShowPlayheadProperty);
        set => SetValue(ShowPlayheadProperty, value);
    }

    /// <summary>Mirror bars around the centre line rather than growing them up from the bottom.</summary>
    public bool IsMirrored
    {
        get => GetValue(IsMirroredProperty);
        set => SetValue(IsMirroredProperty, value);
    }

    /// <summary>Space between bars, in DIP.</summary>
    public double BarGap
    {
        get => GetValue(BarGapProperty);
        set => SetValue(BarGapProperty, Math.Max(0, value));
    }

    /// <summary>Corner rounding on each bar. Typed as <see cref="CornerRadius"/> so the theme can
    /// hand it a shape token directly.</summary>
    public CornerRadius BarCornerRadius
    {
        get => GetValue(BarCornerRadiusProperty);
        set => SetValue(BarCornerRadiusProperty, value);
    }

    /// <summary>Thickness of the playhead line, in DIP.</summary>
    public double PlayheadThickness
    {
        get => GetValue(PlayheadThicknessProperty);
        set => SetValue(PlayheadThicknessProperty, Math.Max(1, value));
    }

    /// <summary>Keyboard step for Left and Right, as a fraction of the whole waveform.</summary>
    public double KeyboardStep
    {
        get => GetValue(KeyboardStepProperty);
        set => SetValue(KeyboardStepProperty, Math.Clamp(value, 0, 1));
    }

    /// <summary>Bars the playhead has passed.</summary>
    public IBrush? PlayedBrush
    {
        get => GetValue(PlayedBrushProperty);
        set => SetValue(PlayedBrushProperty, value);
    }

    /// <summary>Bars still ahead of the playhead.</summary>
    public IBrush? UnplayedBrush
    {
        get => GetValue(UnplayedBrushProperty);
        set => SetValue(UnplayedBrushProperty, value);
    }

    /// <summary>The playhead line and the handle that appears while it is held.</summary>
    public IBrush? PlayheadBrush
    {
        get => GetValue(PlayheadBrushProperty);
        set => SetValue(PlayheadBrushProperty, value);
    }

    /// <summary>Drawn across the middle when there are no peaks yet.</summary>
    public IBrush? BaselineBrush
    {
        get => GetValue(BaselineBrushProperty);
        set => SetValue(BaselineBrushProperty, value);
    }

    /// <summary>Whether the playhead carries a knob. Turn it off for a hairline-only seek line.</summary>
    public bool ShowPlayheadKnob
    {
        get => GetValue(ShowPlayheadKnobProperty);
        set => SetValue(ShowPlayheadKnobProperty, value);
    }

    /// <summary>Whether the bars pulse in place.</summary>
    public bool IsPulsing
    {
        get => GetValue(IsPulsingProperty);
        set => SetValue(IsPulsingProperty, value);
    }

    /// <summary>How far a bar dips at the trough of the pulse, as a fraction of its own height.</summary>
    public double AnimationAmplitude
    {
        get => GetValue(AnimationAmplitudeProperty);
        set => SetValue(AnimationAmplitudeProperty, Math.Clamp(value, 0, 1));
    }

    /// <summary>How fast the pulse travels along the bars, in cycles per second.</summary>
    public double AnimationSpeed
    {
        get => GetValue(AnimationSpeedProperty);
        set => SetValue(AnimationSpeedProperty, Math.Max(0, value));
    }

    /// <summary>
    /// Reduces a sample buffer to one peak per bar, which is what <see cref="Peaks"/> takes. Samples
    /// are read as magnitudes, so signed PCM and already-rectified envelopes both work, and the
    /// result is clamped to 1 so a hot recording cannot draw outside the control.
    /// </summary>
    public static float[] PeaksFromSamples(ReadOnlySpan<float> samples, int barCount)
    {
        if (barCount <= 0) throw new ArgumentOutOfRangeException(nameof(barCount), "a waveform needs at least one bar");
        var peaks = new float[barCount];
        if (samples.IsEmpty) return peaks;

        var perBar = (double)samples.Length / barCount;
        for (var bar = 0; bar < barCount; bar++)
        {
            var start = (int)(bar * perBar);
            var end = Math.Min((int)((bar + 1) * perBar), samples.Length);
            // More bars than samples: every bar takes one sample rather than none, so a short buffer
            // still draws something recognisable instead of a row of gaps.
            if (end <= start) end = Math.Min(start + 1, samples.Length);

            var peak = 0f;
            for (var i = start; i < end; i++)
            {
                var magnitude = Math.Abs(samples[i]);
                if (magnitude > peak) peak = magnitude;
            }
            peaks[bar] = Math.Min(peak, 1f);
        }
        return peaks;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width <= 0 || height <= 0) return;

        // Avalonia hit-tests the recorded drawing, not the bounds: CompositionDrawListVisual.HitTest
        // is DrawList?.HitTest(pt) ?? false. Without this transparent rect a click that lands in the
        // gap between two bars - or above a short one - hits nothing and the scrub silently does
        // nothing, which reads as a flaky drag rather than as a two-dip dead zone. MdRating draws
        // the same rect for the same reason.
        context.DrawRectangle(Brushes.Transparent, null, new Rect(Bounds.Size));

        var peaks = Peaks;
        var position = Math.Clamp(PlayheadPosition, 0, 1);
        var centre = height / 2;

        if (peaks is null || peaks.Count == 0)
        {
            var baseline = BaselineBrush ?? FallbackUnplayed;
            context.DrawLine(new Pen(baseline, 1), new Point(0, centre), new Point(width, centre));
            return;
        }

        var played = PlayedBrush ?? FallbackPlayed;
        var unplayed = UnplayedBrush ?? FallbackUnplayed;
        var count = peaks.Count;
        var gap = Math.Max(0, BarGap);
        var barWidth = (width - gap * (count - 1)) / count;
        if (barWidth < 1)
        {
            // More bars than pixels: drop the gaps rather than the bars, so the shape survives.
            barWidth = Math.Max(1, width / count);
            gap = 0;
        }

        var radius = BarCornerRadius;
        // A bar's corners cannot be rounder than half its width, so a full-radius shape token
        // yields the round cap Flutter's StrokeCap.round gives rather than an over-constrained
        // rectangle. Taking the smallest of the four keeps a per-corner radius meaningful.
        var barRadius = Math.Min(
            Math.Min(Math.Min(radius.TopLeft, radius.TopRight), Math.Min(radius.BottomRight, radius.BottomLeft)),
            barWidth / 2);
        var splitAt = position * width;
        var mirrored = IsMirrored;

        var animating = ShouldAnimate;
        var amplitude = Math.Clamp(AnimationAmplitude, 0, 1);

        for (var i = 0; i < count; i++)
        {
            var magnitude = Math.Clamp(peaks[i], 0, 1);
            if (magnitude <= 0) continue;

            if (animating)
            {
                // One pulse travelling along the bars, phase-shifted per bar, and scaled by the bar's
                // own peak - so a quiet passage stays quiet instead of thumping as loud as the drop.
                var trough = 0.5 + 0.5 * Math.Sin(_phase - i * 0.35);
                magnitude *= (float)(1 - amplitude * trough);
            }

            var span = magnitude * height;
            var x = i * (barWidth + gap);
            var rect = new Rect(x, mirrored ? centre - span / 2 : height - span, barWidth, span);
            // A bar is played once the playhead is past its middle, which is how a slider decides
            // too; splitting a straddling bar would leave a sliver of colour nobody can read.
            //
            // DrawRectangle has no CornerRadius overload - its fourth argument is a double radiusX,
            // and RoundedRect has no (Rect, CornerRadius) constructor either, so the corners are
            // mapped by hand. That is worth saying out loud: passing the CornerRadius straight
            // through compiles against the wrong overload in every direction but this one.
            context.DrawRectangle(x + barWidth / 2 <= splitAt ? played : unplayed, null,
                new RoundedRect(rect, barRadius));
        }

        if (!ShowPlayhead) return;

        var playhead = PlayheadBrush ?? played;
        var thickness = Math.Max(1, PlayheadThickness);
        var at = position * width;
        context.DrawRectangle(playhead, null,
            new RoundedRect(new Rect(at - thickness / 2, 0, thickness, height), thickness / 2));

        if (!ShowPlayheadKnob || !IsInteractive) return;

        // A bare full-height line reads as a divider rather than as a position. The knob is what a
        // Material slider's handle is: it says something is being held here, it grows while it
        // actually is, and the halo under it is the same state layer that slider shows on press.
        // Pushing opacity rather than building a lighter brush keeps this correct for a gradient
        // PlayheadBrush too, where "the same colour at 16%" is not a thing that can be constructed.
        if (_pressed)
        {
            using (context.PushOpacity(0.16))
            {
                context.DrawEllipse(playhead, null, new Point(at, centre), 22 / 2, 22 / 2);
            }
        }

        var knob = _pressed ? 20 : 13;
        context.DrawEllipse(playhead, null, new Point(at, centre), knob / 2, knob / 2);
    }

    /// <summary>Whether the pulse should be running right now.</summary>
    private bool ShouldAnimate
    {
        get
        {
            if (!IsPulsing || AnimationAmplitude <= 0 || Peaks is not { Count: > 0 } || !IsEffectivelyVisible)
                return false;
            // A pulse is decorative motion, so the ambient scheme decides: Reduced and None both
            // mean the bars hold still, and the control still draws the same peaks either way.
            var scheme = MdMotion.GetScheme(this);
            return scheme != MdMotionScheme.None && scheme != MdMotionScheme.Reduced;
        }
    }

    /// <summary>Starts the pulse if it should be running, and lets it lapse if it should not.</summary>
    private void UpdateAnimation()
    {
        _lastFrameValid = false;
        if (ShouldAnimate) RequestFrame();
    }

    /// <summary>
    /// Advances on the render loop rather than a timer. RequestAnimationFrame only fires while frames
    /// are being produced, so a waveform that is hidden, minimized or switched off costs nothing, and
    /// there is no timer left running when the control is torn down.
    /// </summary>
    private void RequestFrame()
    {
        if (_frameRequested) return;
        if (TopLevel.GetTopLevel(this) is not { } topLevel) return;
        _frameRequested = true;
        topLevel.RequestAnimationFrame(OnFrame);
    }

    private void OnFrame(TimeSpan timestamp)
    {
        _frameRequested = false;
        if (!ShouldAnimate)
        {
            _lastFrameValid = false;
            return;
        }

        if (_lastFrameValid)
        {
            _phase += (timestamp - _lastFrame).TotalSeconds * AnimationSpeed * Math.Tau;
            while (_phase > Math.Tau) _phase -= Math.Tau;
        }
        _lastFrame = timestamp;
        _lastFrameValid = true;

        InvalidateVisual();
        RequestFrame();
    }

    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeToAncestorVisibility();
        UpdatePseudoClasses();
        UpdateAnimation();
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeFromAncestorVisibility();
        base.OnDetachedFromVisualTree(e);
    }

    private void SubscribeToAncestorVisibility()
    {
        UnsubscribeFromAncestorVisibility();
        foreach (var ancestor in this.GetVisualAncestors().OfType<Visual>())
            _ancestorVisibility.Add(ancestor.GetObservable(IsVisibleProperty).Subscribe(_ => UpdateAnimation()));
    }

    private void UnsubscribeFromAncestorVisibility()
    {
        foreach (var subscription in _ancestorVisibility) subscription.Dispose();
        _ancestorVisibility.Clear();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        PseudoClasses.Set(":pointerover", true);
        base.OnPointerEntered(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        PseudoClasses.Set(":pointerover", false);
        base.OnPointerExited(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!IsInteractive || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            base.OnPointerPressed(e);
            return;
        }

        e.Pointer.Capture(this);
        SetPressed(true);
        SeekTo(e.GetPosition(this).X);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (ReferenceEquals(e.Pointer.Captured, this)) SeekTo(e.GetPosition(this).X);
        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (ReferenceEquals(e.Pointer.Captured, this))
        {
            e.Pointer.Capture(null);
            SetPressed(false);
            e.Handled = true;
        }
        base.OnPointerReleased(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!IsInteractive)
        {
            base.OnKeyDown(e);
            return;
        }

        var position = Math.Clamp(PlayheadPosition, 0, 1);
        var step = Math.Clamp(KeyboardStep, 0, 1);
        switch (e.Key)
        {
            case Key.Left: SetValue(PlayheadPositionProperty, position - step); break;
            case Key.Right: SetValue(PlayheadPositionProperty, position + step); break;
            case Key.Home: SetValue(PlayheadPositionProperty, 0); break;
            case Key.End: SetValue(PlayheadPositionProperty, 1); break;
            default: base.OnKeyDown(e); return;
        }
        e.Handled = true;
    }

    private void SetPressed(bool pressed)
    {
        _pressed = pressed;
        PseudoClasses.Set(":pressed", pressed);
        InvalidateVisual();
    }

    private void SeekTo(double x)
    {
        var width = Bounds.Width;
        if (width <= 0) return;
        SetValue(PlayheadPositionProperty, Math.Clamp(x / width, 0, 1));
    }

    private void UpdatePseudoClasses()
    {
        var interactive = IsInteractive;
        PseudoClasses.Set(":non-interactive", !interactive);
        if (!interactive) SetPressed(false);
    }
}
