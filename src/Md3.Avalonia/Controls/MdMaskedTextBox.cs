using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>The look of an <see cref="MdMaskedTextBox"/>.</summary>
public enum MdMaskedTextBoxVariant
{
    /// <summary>A filled field with a bottom indicator line.</summary>
    Filled,

    /// <summary>An outlined field with a stroke that highlights on focus.</summary>
    Outlined,
}

/// <summary>A Material 3 text field that constrains input to a mask.</summary>
/// <remarks>
/// <para>
/// Wraps the built-in <see cref="MaskedTextBox"/> in Material styling: filled and outlined
/// variants, floating label, prefix text, supporting and error text. The mask syntax follows
/// <see cref="System.ComponentModel.MaskedTextProvider"/>: <c>0</c> is a required digit, <c>#</c>
/// an optional digit, <c>?</c> an optional character and any other character a literal, for
/// example <c>(000) 000-0000</c> or <c>##/##/####</c>.
/// </para>
/// <para>
/// <see cref="MaskedTextBox.MaskCompleted"/> and <see cref="MaskedTextBox.MaskFull"/> report
/// whether required input (or all input) is present; setting <see cref="TextBox.Text"/>
/// programmatically pushes the value through the mask.
/// </para>
/// </remarks>
[PseudoClasses(
    ":filled", ":outlined", ":md-error", ":has-label", ":no-label", ":has-prefix",
    ":has-supporting-text", ":has-error-text", ":reduced-motion", ":no-motion")]
public class MdMaskedTextBox : MaskedTextBox
{
    /// <summary>
    /// The base control styles itself as a plain <see cref="TextBox"/>, which would shadow this
    /// type's theme; keep the style key on the Material type.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(MdMaskedTextBox);

    public static readonly StyledProperty<MdMaskedTextBoxVariant> VariantProperty =
        AvaloniaProperty.Register<MdMaskedTextBox, MdMaskedTextBoxVariant>(
            nameof(Variant),
            MdMaskedTextBoxVariant.Filled);
    public static readonly StyledProperty<object?> LabelProperty =
        AvaloniaProperty.Register<MdMaskedTextBox, object?>(nameof(Label));
    public static readonly StyledProperty<string?> PrefixTextProperty =
        AvaloniaProperty.Register<MdMaskedTextBox, string?>(nameof(PrefixText));
    public static readonly StyledProperty<object?> SupportingTextProperty =
        AvaloniaProperty.Register<MdMaskedTextBox, object?>(nameof(SupportingText));
    public static readonly StyledProperty<object?> ErrorTextProperty =
        AvaloniaProperty.Register<MdMaskedTextBox, object?>(nameof(ErrorText));
    public static readonly StyledProperty<bool> IsErrorProperty =
        AvaloniaProperty.Register<MdMaskedTextBox, bool>(nameof(IsError));

    static MdMaskedTextBox()
    {
        VariantProperty.Changed.AddClassHandler<MdMaskedTextBox>((control, _) => control.UpdateVariantPseudoClasses());
        LabelProperty.Changed.AddClassHandler<MdMaskedTextBox>((control, _) => control.UpdateContentPseudoClasses());
        PrefixTextProperty.Changed.AddClassHandler<MdMaskedTextBox>((control, _) => control.UpdateContentPseudoClasses());
        SupportingTextProperty.Changed.AddClassHandler<MdMaskedTextBox>((control, _) => control.UpdateContentPseudoClasses());
        ErrorTextProperty.Changed.AddClassHandler<MdMaskedTextBox>((control, _) => control.UpdateContentPseudoClasses());
        IsErrorProperty.Changed.AddClassHandler<MdMaskedTextBox>((control, _) => control.UpdateErrorPseudoClass());
    }

    public MdMaskedTextBox()
    {
        UpdateVariantPseudoClasses();
        UpdateContentPseudoClasses();
        UpdateErrorPseudoClass();
        MdTextEditingContextMenu.Attach(this);
    }

    /// <summary>Gets or sets the Material field variant (filled or outlined).</summary>
    public MdMaskedTextBoxVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }

    /// <summary>Gets or sets the label shown above (outlined) or floating in (filled) the field.</summary>
    public object? Label { get => GetValue(LabelProperty); set => SetValue(LabelProperty, value); }

    /// <summary>Gets or sets text drawn before the input, for example a country code.</summary>
    public string? PrefixText { get => GetValue(PrefixTextProperty); set => SetValue(PrefixTextProperty, value); }

    /// <summary>Gets or sets helper text shown below the field.</summary>
    public object? SupportingText { get => GetValue(SupportingTextProperty); set => SetValue(SupportingTextProperty, value); }

    /// <summary>Gets or sets error text shown below the field.</summary>
    public object? ErrorText { get => GetValue(ErrorTextProperty); set => SetValue(ErrorTextProperty, value); }

    /// <summary>Gets or sets whether the field is in an error state.</summary>
    public bool IsError { get => GetValue(IsErrorProperty); set => SetValue(IsErrorProperty, value); }

    private void UpdateVariantPseudoClasses()
    {
        PseudoClasses.Set(":filled", Variant == MdMaskedTextBoxVariant.Filled);
        PseudoClasses.Set(":outlined", Variant == MdMaskedTextBoxVariant.Outlined);
    }

    private void UpdateContentPseudoClasses()
    {
        var hasLabel = Label is not null;
        PseudoClasses.Set(":has-label", hasLabel);
        PseudoClasses.Set(":no-label", !hasLabel);
        PseudoClasses.Set(":has-prefix", !string.IsNullOrEmpty(PrefixText));
        PseudoClasses.Set(":has-supporting-text", SupportingText is not null);
        PseudoClasses.Set(":has-error-text", ErrorText is not null);
    }

    private void UpdateErrorPseudoClass() => PseudoClasses.Set(":md-error", IsError);
}
