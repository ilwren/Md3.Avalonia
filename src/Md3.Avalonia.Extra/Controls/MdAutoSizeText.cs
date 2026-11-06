using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>Displays plain text at the largest stepped font size that fits its layout constraints.</summary>
/// <remarks>
/// <see cref="TemplatedControl.FontSize"/> is the requested upper size; the control never writes
/// back to it. The read-only <see cref="EffectiveFontSize"/> is recomputed for measure and arrange.
/// When no size fits, the minimum is used and normal trimming or clipping applies. At least one
/// finite layout dimension is required for resizing. Rich inlines and shared size groups are not
/// implemented.
/// </remarks>
public sealed class MdAutoSizeText : TemplatedControl
{
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<MdAutoSizeText, string?>(nameof(Text));
    public static readonly StyledProperty<double> MinFontSizeProperty = AvaloniaProperty.Register<MdAutoSizeText, double>(
        nameof(MinFontSize), 12, coerce: static (_, value) => double.IsFinite(value) ? Math.Clamp(value, 1, 512) : 12);
    public static readonly StyledProperty<double> StepGranularityProperty = AvaloniaProperty.Register<MdAutoSizeText, double>(
        nameof(StepGranularity), 1, coerce: static (_, value) => double.IsFinite(value) ? Math.Clamp(value, 0.1, 512) : 1);
    public static readonly StyledProperty<int> MaxLinesProperty = AvaloniaProperty.Register<MdAutoSizeText, int>(
        nameof(MaxLines), coerce: static (_, value) => Math.Clamp(value, 0, 1000));
    public static readonly StyledProperty<TextWrapping> TextWrappingProperty = AvaloniaProperty.Register<MdAutoSizeText, TextWrapping>(nameof(TextWrapping));
    public static readonly StyledProperty<TextTrimming> TextTrimmingProperty = AvaloniaProperty.Register<MdAutoSizeText, TextTrimming>(nameof(TextTrimming), TextTrimming.None);
    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty = AvaloniaProperty.Register<MdAutoSizeText, TextAlignment>(nameof(TextAlignment));
    public static readonly DirectProperty<MdAutoSizeText, double> EffectiveFontSizeProperty =
        AvaloniaProperty.RegisterDirect<MdAutoSizeText, double>(nameof(EffectiveFontSize), control => control.EffectiveFontSize);

    private double _effectiveFontSize;

    static MdAutoSizeText()
    {
        AffectsMeasure<MdAutoSizeText>(TextProperty, MinFontSizeProperty, StepGranularityProperty, MaxLinesProperty,
            TextWrappingProperty, TextTrimmingProperty, TextAlignmentProperty, FontFamilyProperty, FontSizeProperty, FontWeightProperty, FontStyleProperty,
            FontStretchProperty, FlowDirectionProperty, PaddingProperty);
        AffectsRender<MdAutoSizeText>(TextProperty, TextWrappingProperty, TextTrimmingProperty, TextAlignmentProperty,
            MaxLinesProperty, ForegroundProperty, FontFamilyProperty, FontSizeProperty, FontWeightProperty,
            FontStyleProperty, FontStretchProperty, FlowDirectionProperty, PaddingProperty);
    }

    /// <summary>Plain text to display.</summary>
    public string? Text { get => GetValue(TextProperty); set => SetValue(TextProperty, value); }
    /// <summary>Smallest permitted size in DIP (coerced to 1–512 and never larger than the requested FontSize).</summary>
    public double MinFontSize { get => GetValue(MinFontSizeProperty); set => SetValue(MinFontSizeProperty, value); }
    /// <summary>Size decrement in DIP (coerced to 0.1–512).</summary>
    public double StepGranularity { get => GetValue(StepGranularityProperty); set => SetValue(StepGranularityProperty, value); }
    /// <summary>Maximum lines; zero means unlimited.</summary>
    public int MaxLines { get => GetValue(MaxLinesProperty); set => SetValue(MaxLinesProperty, value); }
    /// <summary>How lines wrap at the available width.</summary>
    public TextWrapping TextWrapping { get => GetValue(TextWrappingProperty); set => SetValue(TextWrappingProperty, value); }
    /// <summary>How the final layout trims overflow when even the minimum size cannot fit.</summary>
    public TextTrimming TextTrimming { get => GetValue(TextTrimmingProperty); set => SetValue(TextTrimmingProperty, value); }
    /// <summary>Text alignment within the available width.</summary>
    public TextAlignment TextAlignment { get => GetValue(TextAlignmentProperty); set => SetValue(TextAlignmentProperty, value); }
    /// <summary>The font size chosen by the most recent layout pass.</summary>
    public double EffectiveFontSize => _effectiveFontSize;

    protected override bool BypassFlowDirectionPolicies => true;

    protected override Size MeasureOverride(Size availableSize)
    {
        var contentSize = availableSize.Deflate(Padding);
        UpdateFontSize(contentSize);
        using var layout = CreateLayout(EffectiveFontSize, contentSize, forFit: false);
        return new Size(layout.WidthIncludingTrailingWhitespace, layout.Height).Inflate(Padding);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        UpdateFontSize(finalSize.Deflate(Padding));
        return finalSize;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var size = Bounds.Size.Deflate(Padding);
        if (size.Width <= 0 || size.Height <= 0 || string.IsNullOrEmpty(Text)) return;
        using var layout = CreateLayout(EffectiveFontSize > 0 ? EffectiveFontSize : RequestedSize, size, forFit: false);
        using (context.PushClip(new Rect(Padding.Left, Padding.Top, size.Width, size.Height)))
            layout.Draw(context, new Point(Padding.Left, Padding.Top));
    }

    private double RequestedSize => double.IsFinite(FontSize) ? Math.Clamp(FontSize, 1, 512) : 12;

    private void UpdateFontSize(Size available)
    {
        var max = RequestedSize;
        var min = Math.Min(MinFontSize, max);
        if (string.IsNullOrEmpty(Text) || (!double.IsFinite(available.Width) && !double.IsFinite(available.Height)))
        { SetEffectiveSize(max); return; }
        if ((double.IsFinite(available.Width) && available.Width <= 0) ||
            (double.IsFinite(available.Height) && available.Height <= 0)) { SetEffectiveSize(min); return; }

        // Search the finite, descending set of stepped sizes. The final entry is always exactly
        // the minimum, even when the step does not divide the requested interval evenly.
        var steps = (int)Math.Ceiling((max - min) / StepGranularity);
        var lower = 0;
        var upper = steps;
        while (lower < upper)
        {
            var mid = lower + (upper - lower) / 2;
            var candidate = Math.Max(min, max - mid * StepGranularity);
            if (Fits(candidate, available)) upper = mid;
            else lower = mid + 1;
        }
        SetEffectiveSize(Math.Max(min, max - lower * StepGranularity));
    }

    private void SetEffectiveSize(double value)
    {
        if (SetAndRaise(EffectiveFontSizeProperty, ref _effectiveFontSize, value)) InvalidateVisual();
    }

    private bool Fits(double size, Size available)
    {
        using var layout = CreateLayout(size, available, forFit: true);
        return (!double.IsFinite(available.Width) || layout.WidthIncludingTrailingWhitespace <= available.Width + 0.01) &&
               (!double.IsFinite(available.Height) || layout.Height <= available.Height + 0.01) &&
               (MaxLines == 0 || layout.TextLines.Count <= MaxLines);
    }

    private TextLayout CreateLayout(double size, Size available, bool forFit)
    {
        var width = double.IsFinite(available.Width) ? Math.Max(0.01, available.Width) : double.PositiveInfinity;
        var height = double.IsFinite(available.Height) ? Math.Max(0.01, available.Height) : double.PositiveInfinity;
        // Do not clip or truncate during the fit test: otherwise an overflowing paragraph could
        // appear to fit simply because TextLayout discarded the extra lines.
        var layoutWidth = forFit && TextWrapping == TextWrapping.NoWrap ? double.PositiveInfinity : width;
        return new TextLayout(Text ?? string.Empty, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
            size, Foreground, TextAlignment, TextWrapping, forFit ? TextTrimming.None : TextTrimming,
            flowDirection: FlowDirection, maxWidth: layoutWidth,
            maxHeight: forFit ? double.PositiveInfinity : height, maxLines: forFit ? 0 : MaxLines);
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new MdAutoSizeTextAutomationPeer(this);
}

internal sealed class MdAutoSizeTextAutomationPeer : ControlAutomationPeer
{
    public MdAutoSizeTextAutomationPeer(MdAutoSizeText owner) : base(owner)
    {
        owner.PropertyChanged += (_, e) =>
        {
            if (e.Property == MdAutoSizeText.TextProperty && string.IsNullOrWhiteSpace(base.GetNameCore()))
                RaisePropertyChangedEvent(AutomationElementIdentifiers.NameProperty, e.OldValue, e.NewValue);
        };
    }

    private MdAutoSizeText TextOwner => (MdAutoSizeText)Owner;
    protected override string? GetNameCore()
    {
        var name = base.GetNameCore();
        return string.IsNullOrWhiteSpace(name) ? TextOwner.Text : name;
    }
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Text;
}
