using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Md3.Avalonia.Localization;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>Determines how collapsed <see cref="MdExpandableText"/> limits its plain text.</summary>
public enum MdExpandableTextTrimMode
{
    /// <summary>Limit the number of wrapped lines.</summary>
    Lines,
    /// <summary>Limit Unicode text elements rather than UTF-16 code units.</summary>
    Characters
}

/// <summary>Displays plain text with a separate, keyboard-accessible expand/collapse button.</summary>
/// <remarks>
/// Line overflow is measured after arrangement using the text part's actual width and font;
/// character mode uses Unicode text elements. The button is hidden when nothing is truncated.
/// Rich inlines, annotations, inline links, selection and animated expansion are not supported.
/// </remarks>
public sealed class MdExpandableText : TemplatedControl
{
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<MdExpandableText, string?>(nameof(Text));
    public static readonly StyledProperty<MdExpandableTextTrimMode> TrimModeProperty = AvaloniaProperty.Register<MdExpandableText, MdExpandableTextTrimMode>(nameof(TrimMode));
    public static readonly StyledProperty<int> CollapsedLinesProperty = AvaloniaProperty.Register<MdExpandableText, int>(
        nameof(CollapsedLines), 2, coerce: static (_, value) => Math.Clamp(value, 1, 1000));
    public static readonly StyledProperty<int> CollapsedCharactersProperty = AvaloniaProperty.Register<MdExpandableText, int>(
        nameof(CollapsedCharacters), 100, coerce: static (_, value) => Math.Max(1, value));
    public static readonly StyledProperty<bool> IsExpandedProperty = AvaloniaProperty.Register<MdExpandableText, bool>(
        nameof(IsExpanded), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<string?> ExpandLabelProperty = AvaloniaProperty.Register<MdExpandableText, string?>(nameof(ExpandLabel));
    public static readonly StyledProperty<string?> CollapseLabelProperty = AvaloniaProperty.Register<MdExpandableText, string?>(nameof(CollapseLabel));
    public static readonly DirectProperty<MdExpandableText, string> DisplayTextProperty =
        AvaloniaProperty.RegisterDirect<MdExpandableText, string>(nameof(DisplayText), owner => owner.DisplayText);
    public static readonly DirectProperty<MdExpandableText, int> DisplayMaxLinesProperty =
        AvaloniaProperty.RegisterDirect<MdExpandableText, int>(nameof(DisplayMaxLines), owner => owner.DisplayMaxLines);
    public static readonly DirectProperty<MdExpandableText, bool> CanExpandProperty =
        AvaloniaProperty.RegisterDirect<MdExpandableText, bool>(nameof(CanExpand), owner => owner.CanExpand);
    public static readonly DirectProperty<MdExpandableText, string> ToggleLabelProperty =
        AvaloniaProperty.RegisterDirect<MdExpandableText, string>(nameof(ToggleLabel), owner => owner.ToggleLabel);

    private string _displayText = string.Empty;
    private int _displayMaxLines;
    private bool _canExpand;
    private string _toggleLabel = string.Empty;
    private TextBlock? _textPart;
    private Button? _togglePart;

    static MdExpandableText()
    {
        AffectsMeasure<MdExpandableText>(TextProperty, TrimModeProperty, CollapsedLinesProperty,
            CollapsedCharactersProperty, IsExpandedProperty, FontFamilyProperty, FontSizeProperty,
            FontStyleProperty, FontWeightProperty, FontStretchProperty, FlowDirectionProperty, PaddingProperty);
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdExpandableText>((control, _) => control.UpdatePresentation());
    }

    public MdExpandableText() => UpdatePresentation();

    /// <summary>Plain text to display.</summary>
    public string? Text { get => GetValue(TextProperty); set => SetValue(TextProperty, value); }
    /// <summary>Whether to limit wrapped lines or Unicode text elements.</summary>
    public MdExpandableTextTrimMode TrimMode { get => GetValue(TrimModeProperty); set => SetValue(TrimModeProperty, value); }
    /// <summary>Maximum collapsed lines; coerced to 1–1000. Default is 2.</summary>
    public int CollapsedLines { get => GetValue(CollapsedLinesProperty); set => SetValue(CollapsedLinesProperty, value); }
    /// <summary>Maximum collapsed Unicode text elements; at least one. Default is 100.</summary>
    public int CollapsedCharacters { get => GetValue(CollapsedCharactersProperty); set => SetValue(CollapsedCharactersProperty, value); }
    /// <summary>Whether the full text is visible; defaults to a two-way binding.</summary>
    public bool IsExpanded { get => GetValue(IsExpandedProperty); set => SetValue(IsExpandedProperty, value); }
    /// <summary>Optional button text when collapsed; null uses the localized built-in label.</summary>
    public string? ExpandLabel { get => GetValue(ExpandLabelProperty); set => SetValue(ExpandLabelProperty, value); }
    /// <summary>Optional button text when expanded; null uses the localized built-in label.</summary>
    public string? CollapseLabel { get => GetValue(CollapseLabelProperty); set => SetValue(CollapseLabelProperty, value); }
    /// <summary>Text currently passed to the template's text part.</summary>
    public string DisplayText => _displayText;
    /// <summary>Current line limit for the text part; zero is unlimited.</summary>
    public int DisplayMaxLines => _displayMaxLines;
    /// <summary>Whether collapsing would hide text, determined by the most recent layout in line mode.</summary>
    public bool CanExpand => _canExpand;
    /// <summary>Current localized or overridden button label.</summary>
    public string ToggleLabel => _toggleLabel;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TextProperty || change.Property == TrimModeProperty ||
            change.Property == CollapsedLinesProperty || change.Property == CollapsedCharactersProperty ||
            change.Property == IsExpandedProperty || change.Property == ExpandLabelProperty || change.Property == CollapseLabelProperty)
            UpdatePresentation();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_togglePart is not null) _togglePart.Click -= OnToggleClick;
        base.OnApplyTemplate(e);
        _textPart = e.NameScope.Find<TextBlock>("PART_Text");
        _togglePart = e.NameScope.Find<Button>("PART_Toggle");
        if (_togglePart is not null) _togglePart.Click += OnToggleClick;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        if (TrimMode == MdExpandableTextTrimMode.Lines)
            SetAndRaise(CanExpandProperty, ref _canExpand, HasLineOverflow());
        return result;
    }

    private void OnToggleClick(object? sender, RoutedEventArgs e)
    {
        if (CanExpand) SetCurrentValue(IsExpandedProperty, !IsExpanded);
    }

    private void UpdatePresentation()
    {
        var text = Text ?? string.Empty;
        var characterMode = TrimMode == MdExpandableTextTrimMode.Characters;
        if (characterMode)
        {
            var elements = StringInfo.ParseCombiningCharacters(text);
            var truncated = elements.Length > CollapsedCharacters;
            SetAndRaise(CanExpandProperty, ref _canExpand, truncated);
            SetAndRaise(DisplayTextProperty, ref _displayText,
                truncated && !IsExpanded ? text[..elements[CollapsedCharacters]] + "…" : text);
        }
        else
        {
            if (text.Length == 0) SetAndRaise(CanExpandProperty, ref _canExpand, false);
            SetAndRaise(DisplayTextProperty, ref _displayText, text);
        }
        SetAndRaise(DisplayMaxLinesProperty, ref _displayMaxLines, !characterMode && !IsExpanded ? CollapsedLines : 0);
        var label = IsExpanded ? CollapseLabel ?? MdLocalization.GetString("Collapse", this) : ExpandLabel ?? MdLocalization.GetString("Expand", this);
        SetAndRaise(ToggleLabelProperty, ref _toggleLabel, label);
    }

    private bool HasLineOverflow()
    {
        var text = Text;
        var part = _textPart;
        if (string.IsNullOrEmpty(text) || part is null) return false;
        var width = part.Bounds.Width;
        if (width <= 0) return true;
        using var layout = new TextLayout(text, new Typeface(part.FontFamily, part.FontStyle, part.FontWeight, part.FontStretch),
            part.FontSize, part.Foreground, part.TextAlignment, part.TextWrapping, TextTrimming.None,
            flowDirection: part.FlowDirection, maxWidth: width, lineHeight: part.LineHeight,
            letterSpacing: part.LetterSpacing, fontFeatures: part.FontFeatures);
        return layout.TextLines.Count > CollapsedLines || layout.WidthIncludingTrailingWhitespace > width + 0.01;
    }
}
