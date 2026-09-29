using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Material numeric input that preserves Avalonia NumericUpDown parsing, validation, keyboard,
/// wheel, increment and two-way Value semantics without styling NumericUpDown globally.
/// </summary>
[PseudoClasses(":filled", ":outlined", ":md-error", ":has-label", ":has-supporting-text")]
public sealed class MdNumericBox : NumericUpDown
{
    public static readonly StyledProperty<MdTextBoxVariant> VariantProperty =
        AvaloniaProperty.Register<MdNumericBox, MdTextBoxVariant>(nameof(Variant), MdTextBoxVariant.Outlined);
    public static readonly StyledProperty<object?> LabelProperty =
        AvaloniaProperty.Register<MdNumericBox, object?>(nameof(Label));
    public static readonly StyledProperty<object?> SupportingTextProperty =
        AvaloniaProperty.Register<MdNumericBox, object?>(nameof(SupportingText));
    public static readonly StyledProperty<bool> IsErrorProperty =
        AvaloniaProperty.Register<MdNumericBox, bool>(nameof(IsError));

    static MdNumericBox()
    {
        VariantProperty.Changed.AddClassHandler<MdNumericBox>((control, _) => control.UpdatePseudoClasses());
        LabelProperty.Changed.AddClassHandler<MdNumericBox>((control, _) => control.UpdatePseudoClasses());
        SupportingTextProperty.Changed.AddClassHandler<MdNumericBox>((control, _) => control.UpdatePseudoClasses());
        IsErrorProperty.Changed.AddClassHandler<MdNumericBox>((control, _) => control.UpdatePseudoClasses());
    }

    public MdNumericBox() => UpdatePseudoClasses();

    public MdTextBoxVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public object? Label { get => GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public object? SupportingText { get => GetValue(SupportingTextProperty); set => SetValue(SupportingTextProperty, value); }
    public bool IsError { get => GetValue(IsErrorProperty); set => SetValue(IsErrorProperty, value); }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":filled", Variant == MdTextBoxVariant.Filled);
        PseudoClasses.Set(":outlined", Variant == MdTextBoxVariant.Outlined);
        PseudoClasses.Set(":md-error", IsError);
        PseudoClasses.Set(":has-label", Label is not null);
        PseudoClasses.Set(":has-supporting-text", SupportingText is not null);
    }
}
