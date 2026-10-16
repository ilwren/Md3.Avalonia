using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Material numeric input that preserves Avalonia NumericUpDown parsing, validation, keyboard,
/// wheel, increment and two-way Value semantics without styling NumericUpDown globally.
/// </summary>
[PseudoClasses(":filled", ":outlined", ":md-error", ":has-label", ":has-supporting-text", ":spinner-left")]
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

    private readonly Border _leadingSpinnerSpacer = CreateSpinnerSpacer();
    private readonly Border _trailingSpinnerSpacer = CreateSpinnerSpacer();
    private MdTextBox? _textBox;

    static MdNumericBox()
    {
        VariantProperty.Changed.AddClassHandler<MdNumericBox>((control, _) => control.UpdatePseudoClasses());
        LabelProperty.Changed.AddClassHandler<MdNumericBox>((control, _) => control.UpdatePseudoClasses());
        SupportingTextProperty.Changed.AddClassHandler<MdNumericBox>((control, _) => control.UpdatePseudoClasses());
        IsErrorProperty.Changed.AddClassHandler<MdNumericBox>((control, _) => control.UpdatePseudoClasses());
        ButtonSpinnerLocationProperty.Changed.AddClassHandler<MdNumericBox>((control, _) => control.UpdatePseudoClasses());
        ShowButtonSpinnerProperty.Changed.AddClassHandler<MdNumericBox>((control, _) => control.UpdatePseudoClasses());
    }

    public MdNumericBox() => UpdatePseudoClasses();

    public MdTextBoxVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public object? Label { get => GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public object? SupportingText { get => GetValue(SupportingTextProperty); set => SetValue(SupportingTextProperty, value); }
    public bool IsError { get => GetValue(IsErrorProperty); set => SetValue(IsErrorProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _textBox = e.NameScope.Find<MdTextBox>("PART_TextBox");
        UpdateSpinnerReservation();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        // Avalonia's ButtonSpinner only spins on a wheel event when it already owns keyboard
        // focus. A mouse wheel over an unfocused numeric field therefore used to bubble to the
        // page scroller without changing the value. Keep the native focused behavior above us,
        // then route an unhandled wheel through NumericUpDown.OnSpin so ValueChanged, bounds,
        // precision, and Spinned use the same code path as the +/- controls.
        base.OnPointerWheelChanged(e);
        if (e.Handled || !IsEnabled || !AllowSpin || IsReadOnly || e.Delta.Y == 0) return;

        var oldValue = Value;
        var direction = e.Delta.Y < 0 ? SpinDirection.Decrease : SpinDirection.Increase;
        OnSpin(new SpinEventArgs(direction, usingMouseWheel: true));
        if (Value != oldValue) e.Handled = true;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":filled", Variant == MdTextBoxVariant.Filled);
        PseudoClasses.Set(":outlined", Variant == MdTextBoxVariant.Outlined);
        PseudoClasses.Set(":md-error", IsError);
        PseudoClasses.Set(":has-label", Label is not null);
        PseudoClasses.Set(":has-supporting-text", SupportingText is not null);
        PseudoClasses.Set(":spinner-left", ShowButtonSpinner &&
            ButtonSpinnerLocation == global::Avalonia.Controls.Location.Left);
        UpdateSpinnerReservation();
    }

    private void UpdateSpinnerReservation()
    {
        if (_textBox is null) return;

        var showSpinner = ShowButtonSpinner;
        var spinnerOnLeft = showSpinner &&
            ButtonSpinnerLocation == global::Avalonia.Controls.Location.Left;
        _textBox.SetCurrentValue(MdTextBox.LeadingIconProperty, spinnerOnLeft ? _leadingSpinnerSpacer : null);
        _textBox.SetCurrentValue(MdTextBox.TrailingIconProperty,
            showSpinner && !spinnerOnLeft ? _trailingSpinnerSpacer : null);
    }

    private static Border CreateSpinnerSpacer() => new()
    {
        Width = 32,
        Height = 24,
        Background = Brushes.Transparent,
        IsHitTestVisible = false
    };
}
