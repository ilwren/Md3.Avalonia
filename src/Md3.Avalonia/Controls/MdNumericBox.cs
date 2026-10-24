using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Material numeric input that preserves Avalonia NumericUpDown parsing, validation, keyboard,
/// focused-wheel, increment and two-way Value semantics without styling NumericUpDown globally.
/// </summary>
[PseudoClasses(":filled", ":outlined", ":md-error", ":has-label", ":has-supporting-text", ":spinner-left")]
public sealed class MdNumericBox : NumericUpDown
{
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        WatermarkProperty.AddOwner<MdNumericBox>();

    public static readonly StyledProperty<IBrush?> PlaceholderForegroundProperty =
        AvaloniaProperty.Register<MdNumericBox, IBrush?>(nameof(PlaceholderForeground));

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

    public MdNumericBox()
    {
        UpdatePseudoClasses();
        // A text editor inside the spinner can mark wheel input handled before it reaches the
        // page scroller. Reconcile at the numeric control boundary: focused input spins once;
        // unfocused input is always allowed to bubble to the nearest ScrollViewer.
        AddHandler(PointerWheelChangedEvent, OnNumericPointerWheelChanged,
            RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private void OnNumericPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!IsEnabled || e.Delta.Y == 0) return;

        if (!IsKeyboardFocusWithin)
        {
            e.Handled = false;
            return;
        }

        // ButtonSpinner normally handles this path. Keep an explicit fallback for templates or
        // platform input routes that reach the parent without having been consumed by the part.
        if (!e.Handled && AllowSpin && !IsReadOnly)
        {
            OnSpin(new SpinEventArgs(
                e.Delta.Y < 0 ? SpinDirection.Decrease : SpinDirection.Increase,
                usingMouseWheel: true));
            e.Handled = true;
        }
    }

    public string? PlaceholderText { get => GetValue(PlaceholderTextProperty); set => SetValue(PlaceholderTextProperty, value); }
    public IBrush? PlaceholderForeground { get => GetValue(PlaceholderForegroundProperty); set => SetValue(PlaceholderForegroundProperty, value); }
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
