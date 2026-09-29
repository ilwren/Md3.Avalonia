using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A cross-platform, lookless Material Design 3 text field. It preserves Avalonia's native
/// TextBox editing, selection, IME and automation behavior while providing an independent theme.
/// </summary>
[PseudoClasses(
    ":filled", ":outlined", ":md-error", ":has-label", ":no-label", ":has-leading-icon",
    ":has-trailing-icon", ":has-prefix", ":has-suffix", ":has-supporting-text",
    ":has-error-text", ":show-counter", ":show-clear-button", ":password", ":has-end-action", ":multiline",
    ":reduced-motion", ":no-motion")]
public class MdTextBox : TextBox
{
    public static readonly StyledProperty<MdTextBoxVariant> VariantProperty =
        AvaloniaProperty.Register<MdTextBox, MdTextBoxVariant>(
            nameof(Variant),
            MdTextBoxVariant.Filled);

    public static readonly StyledProperty<object?> LabelProperty =
        AvaloniaProperty.Register<MdTextBox, object?>(nameof(Label));

    public static readonly StyledProperty<object?> SupportingTextProperty =
        AvaloniaProperty.Register<MdTextBox, object?>(nameof(SupportingText));

    public static readonly StyledProperty<object?> ErrorTextProperty =
        AvaloniaProperty.Register<MdTextBox, object?>(nameof(ErrorText));

    public static readonly StyledProperty<object?> LeadingIconProperty =
        AvaloniaProperty.Register<MdTextBox, object?>(nameof(LeadingIcon));

    public static readonly StyledProperty<object?> TrailingIconProperty =
        AvaloniaProperty.Register<MdTextBox, object?>(nameof(TrailingIcon));

    public static readonly StyledProperty<string?> PrefixTextProperty =
        AvaloniaProperty.Register<MdTextBox, string?>(nameof(PrefixText));

    public static readonly StyledProperty<string?> SuffixTextProperty =
        AvaloniaProperty.Register<MdTextBox, string?>(nameof(SuffixText));

    public static readonly StyledProperty<bool> IsErrorProperty =
        AvaloniaProperty.Register<MdTextBox, bool>(nameof(IsError));

    public static readonly StyledProperty<bool> ShowCharacterCounterProperty =
        AvaloniaProperty.Register<MdTextBox, bool>(nameof(ShowCharacterCounter));

    public static readonly StyledProperty<bool> ShowClearButtonProperty =
        AvaloniaProperty.Register<MdTextBox, bool>(nameof(ShowClearButton));

    public static readonly StyledProperty<bool> IsPasswordProperty =
        AvaloniaProperty.Register<MdTextBox, bool>(nameof(IsPassword));

    public static readonly StyledProperty<IBrush?> LabelBrushProperty =
        AvaloniaProperty.Register<MdTextBox, IBrush?>(nameof(LabelBrush));

    public static readonly StyledProperty<IBrush?> SupportingTextBrushProperty =
        AvaloniaProperty.Register<MdTextBox, IBrush?>(nameof(SupportingTextBrush));

    public static readonly StyledProperty<IBrush?> LeadingIconBrushProperty =
        AvaloniaProperty.Register<MdTextBox, IBrush?>(nameof(LeadingIconBrush));

    public static readonly StyledProperty<IBrush?> TrailingIconBrushProperty =
        AvaloniaProperty.Register<MdTextBox, IBrush?>(nameof(TrailingIconBrush));

    public static readonly StyledProperty<IBrush?> ActiveIndicatorBrushProperty =
        AvaloniaProperty.Register<MdTextBox, IBrush?>(nameof(ActiveIndicatorBrush));

    public static readonly StyledProperty<IBrush?> OutlineLabelBackgroundProperty =
        AvaloniaProperty.Register<MdTextBox, IBrush?>(nameof(OutlineLabelBackground));

    public static readonly DirectProperty<MdTextBox, int> CharacterCountProperty =
        AvaloniaProperty.RegisterDirect<MdTextBox, int>(
            nameof(CharacterCount),
            control => control.CharacterCount);

    public static readonly DirectProperty<MdTextBox, string> CharacterCounterTextProperty =
        AvaloniaProperty.RegisterDirect<MdTextBox, string>(
            nameof(CharacterCounterText),
            control => control.CharacterCounterText);

    private int _characterCount;
    private string _characterCounterText = "0";
    private Button? _clearButton;
    private ContentPresenter? _labelPresenter;
    private Grid? _inputRow;

    static MdTextBox()
    {
        VariantProperty.Changed.AddClassHandler<MdTextBox>((control, _) => control.UpdateVariantPseudoClasses());
        LabelProperty.Changed.AddClassHandler<MdTextBox>((control, _) => control.UpdateContentPseudoClasses());
        SupportingTextProperty.Changed.AddClassHandler<MdTextBox>((control, _) => control.UpdateContentPseudoClasses());
        ErrorTextProperty.Changed.AddClassHandler<MdTextBox>((control, _) => control.UpdateContentPseudoClasses());
        LeadingIconProperty.Changed.AddClassHandler<MdTextBox>((control, _) => control.UpdateContentPseudoClasses());
        TrailingIconProperty.Changed.AddClassHandler<MdTextBox>((control, _) => control.UpdateContentPseudoClasses());
        PrefixTextProperty.Changed.AddClassHandler<MdTextBox>((control, _) => control.UpdateContentPseudoClasses());
        SuffixTextProperty.Changed.AddClassHandler<MdTextBox>((control, _) => control.UpdateContentPseudoClasses());
        IsErrorProperty.Changed.AddClassHandler<MdTextBox>((control, _) => control.UpdateErrorPseudoClass());
        ShowCharacterCounterProperty.Changed.AddClassHandler<MdTextBox>((control, _) => control.UpdateCounterPseudoClass());
        ShowClearButtonProperty.Changed.AddClassHandler<MdTextBox>((control, _) => control.UpdateContentPseudoClasses());
        IsPasswordProperty.Changed.AddClassHandler<MdTextBox>((control, _) => control.UpdatePasswordState());
        AcceptsReturnProperty.Changed.AddClassHandler<MdTextBox>((control, _) => control.UpdateContentPseudoClasses());
        TextProperty.Changed.AddClassHandler<MdTextBox>((control, _) => control.UpdateCharacterCounter());
        MaxLengthProperty.Changed.AddClassHandler<MdTextBox>((control, _) => control.UpdateCharacterCounter());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdTextBox>((control, _) => control.UpdateMotion());
    }

    public MdTextBox()
    {
        UpdateVariantPseudoClasses();
        UpdateContentPseudoClasses();
        UpdateErrorPseudoClass();
        UpdateCounterPseudoClass();
        UpdatePasswordState();
        UpdateCharacterCounter();
        MdTextEditingContextMenu.Attach(this);
    }

    public MdTextBoxVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public object? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public object? SupportingText
    {
        get => GetValue(SupportingTextProperty);
        set => SetValue(SupportingTextProperty, value);
    }

    public object? ErrorText
    {
        get => GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    public object? LeadingIcon
    {
        get => GetValue(LeadingIconProperty);
        set => SetValue(LeadingIconProperty, value);
    }

    public object? TrailingIcon
    {
        get => GetValue(TrailingIconProperty);
        set => SetValue(TrailingIconProperty, value);
    }

    public string? PrefixText
    {
        get => GetValue(PrefixTextProperty);
        set => SetValue(PrefixTextProperty, value);
    }

    public string? SuffixText
    {
        get => GetValue(SuffixTextProperty);
        set => SetValue(SuffixTextProperty, value);
    }

    /// <summary>
    /// Gets or sets an explicit error state. Avalonia data-validation errors continue to use the
    /// inherited <c>:error</c> pseudo-class independently.
    /// </summary>
    public bool IsError
    {
        get => GetValue(IsErrorProperty);
        set => SetValue(IsErrorProperty, value);
    }

    public bool ShowCharacterCounter
    {
        get => GetValue(ShowCharacterCounterProperty);
        set => SetValue(ShowCharacterCounterProperty, value);
    }

    /// <summary>Shows an embedded clear action while the field contains text.</summary>
    public bool ShowClearButton
    {
        get => GetValue(ShowClearButtonProperty);
        set => SetValue(ShowClearButtonProperty, value);
    }

    /// <summary>Enables password masking and an embedded reveal/hide action.</summary>
    public bool IsPassword
    {
        get => GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    public IBrush? LabelBrush
    {
        get => GetValue(LabelBrushProperty);
        set => SetValue(LabelBrushProperty, value);
    }

    public IBrush? SupportingTextBrush
    {
        get => GetValue(SupportingTextBrushProperty);
        set => SetValue(SupportingTextBrushProperty, value);
    }

    public IBrush? LeadingIconBrush
    {
        get => GetValue(LeadingIconBrushProperty);
        set => SetValue(LeadingIconBrushProperty, value);
    }

    public IBrush? TrailingIconBrush
    {
        get => GetValue(TrailingIconBrushProperty);
        set => SetValue(TrailingIconBrushProperty, value);
    }

    public IBrush? ActiveIndicatorBrush
    {
        get => GetValue(ActiveIndicatorBrushProperty);
        set => SetValue(ActiveIndicatorBrushProperty, value);
    }

    public IBrush? OutlineLabelBackground
    {
        get => GetValue(OutlineLabelBackgroundProperty);
        set => SetValue(OutlineLabelBackgroundProperty, value);
    }

    public int CharacterCount
    {
        get => _characterCount;
        private set => SetAndRaise(CharacterCountProperty, ref _characterCount, value);
    }

    public string CharacterCounterText
    {
        get => _characterCounterText;
        private set => SetAndRaise(CharacterCounterTextProperty, ref _characterCounterText, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_clearButton is not null)
        {
            _clearButton.Click -= OnClearButtonClick;
        }

        base.OnApplyTemplate(e);
        _clearButton = e.NameScope.Find<Button>("PART_ClearButton");
        _labelPresenter = e.NameScope.Find<ContentPresenter>("PART_Label");
        _inputRow = e.NameScope.Find<Grid>("PART_InputRow");
        if (_clearButton is not null)
        {
            _clearButton.Click += OnClearButtonClick;
        }
        UpdateMotion();
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_labelPresenter is not null)
            _labelPresenter.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateBrush(this, ForegroundProperty, MdMotionSpeed.Fast));
        if (_inputRow is not null)
            _inputRow.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
    }

    private void OnClearButtonClick(object? sender, RoutedEventArgs e)
    {
        SetCurrentValue(TextProperty, string.Empty);
        Focus();
        e.Handled = true;
    }

    private void UpdateVariantPseudoClasses()
    {
        var isOutlined = Variant == MdTextBoxVariant.Outlined;
        PseudoClasses.Set(":filled", !isOutlined);
        PseudoClasses.Set(":outlined", isOutlined);
    }

    private void UpdateContentPseudoClasses()
    {
        var hasLabel = Label is not null;
        PseudoClasses.Set(":has-label", hasLabel);
        PseudoClasses.Set(":no-label", !hasLabel);
        PseudoClasses.Set(":has-leading-icon", LeadingIcon is not null);
        PseudoClasses.Set(":has-trailing-icon", TrailingIcon is not null);
        PseudoClasses.Set(":has-prefix", !string.IsNullOrEmpty(PrefixText));
        PseudoClasses.Set(":has-suffix", !string.IsNullOrEmpty(SuffixText));
        PseudoClasses.Set(":has-supporting-text", SupportingText is not null);
        PseudoClasses.Set(":has-error-text", ErrorText is not null);
        PseudoClasses.Set(":show-clear-button", ShowClearButton);
        PseudoClasses.Set(":has-end-action", TrailingIcon is not null || ShowClearButton || IsPassword);
        PseudoClasses.Set(":multiline", AcceptsReturn);
    }

    private void UpdatePasswordState()
    {
        PseudoClasses.Set(":password", IsPassword);
        SetCurrentValue(PasswordCharProperty, IsPassword ? '●' : '\0');
        if (!IsPassword)
        {
            SetCurrentValue(RevealPasswordProperty, false);
        }
        UpdateContentPseudoClasses();
    }

    private void UpdateErrorPseudoClass() => PseudoClasses.Set(":md-error", IsError);

    private void UpdateCounterPseudoClass() =>
        PseudoClasses.Set(":show-counter", ShowCharacterCounter);

    private void UpdateCharacterCounter()
    {
        CharacterCount = Text?.Length ?? 0;
        CharacterCounterText = MaxLength > 0
            ? $"{CharacterCount}/{MaxLength}"
            : CharacterCount.ToString(System.Globalization.CultureInfo.CurrentCulture);
    }
}
