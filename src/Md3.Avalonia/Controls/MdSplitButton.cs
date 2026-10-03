using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>A two-part Material 3 button with primary action and dropdown action.</summary>
[PseudoClasses(":filled", ":tonal", ":outlined", ":elevated", ":xsmall", ":small", ":medium", ":large", ":xlarge", ":has-leading-icon", ":rtl")]
public class MdSplitButton : ContentControl
{
    public static readonly StyledProperty<MdToggleButtonVariant> VariantProperty = AvaloniaProperty.Register<MdSplitButton, MdToggleButtonVariant>(nameof(Variant), MdToggleButtonVariant.Filled);
    public static readonly StyledProperty<MdButtonSize> SizeProperty = AvaloniaProperty.Register<MdSplitButton, MdButtonSize>(nameof(Size), MdButtonSize.Small);
    public static readonly StyledProperty<object?> LeadingIconProperty = AvaloniaProperty.Register<MdSplitButton, object?>(nameof(LeadingIcon));
    public static readonly StyledProperty<ICommand?> CommandProperty = AvaloniaProperty.Register<MdSplitButton, ICommand?>(nameof(Command));
    public static readonly StyledProperty<object?> CommandParameterProperty = AvaloniaProperty.Register<MdSplitButton, object?>(nameof(CommandParameter));
    public static readonly StyledProperty<ICommand?> TrailingCommandProperty = AvaloniaProperty.Register<MdSplitButton, ICommand?>(nameof(TrailingCommand));
    public static readonly StyledProperty<object?> TrailingCommandParameterProperty = AvaloniaProperty.Register<MdSplitButton, object?>(nameof(TrailingCommandParameter));
    public static readonly StyledProperty<object?> DropDownContentProperty = AvaloniaProperty.Register<MdSplitButton, object?>(nameof(DropDownContent));
    public static readonly StyledProperty<bool> IsDropDownOpenProperty = AvaloniaProperty.Register<MdSplitButton, bool>(nameof(IsDropDownOpen), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<string?> PrimaryAutomationNameProperty = AvaloniaProperty.Register<MdSplitButton, string?>(nameof(PrimaryAutomationName));
    public static readonly StyledProperty<string?> TrailingAutomationNameProperty = AvaloniaProperty.Register<MdSplitButton, string?>(nameof(TrailingAutomationName), "Show more actions");

    static MdSplitButton()
    {
        VariantProperty.Changed.AddClassHandler<MdSplitButton>((x, _) => x.UpdatePseudoClasses());
        SizeProperty.Changed.AddClassHandler<MdSplitButton>((x, _) => x.UpdatePseudoClasses());
        LeadingIconProperty.Changed.AddClassHandler<MdSplitButton>((x, _) => x.UpdatePseudoClasses());
        FlowDirectionProperty.Changed.AddClassHandler<MdSplitButton>((x, _) => x.UpdatePseudoClasses());
    }
    public MdSplitButton() => UpdatePseudoClasses();

    public MdToggleButtonVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public MdButtonSize Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public object? LeadingIcon { get => GetValue(LeadingIconProperty); set => SetValue(LeadingIconProperty, value); }
    public ICommand? Command { get => GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
    public object? CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }
    public ICommand? TrailingCommand { get => GetValue(TrailingCommandProperty); set => SetValue(TrailingCommandProperty, value); }
    public object? TrailingCommandParameter { get => GetValue(TrailingCommandParameterProperty); set => SetValue(TrailingCommandParameterProperty, value); }
    public object? DropDownContent { get => GetValue(DropDownContentProperty); set => SetValue(DropDownContentProperty, value); }
    public bool IsDropDownOpen { get => GetValue(IsDropDownOpenProperty); set => SetValue(IsDropDownOpenProperty, value); }
    public string? PrimaryAutomationName { get => GetValue(PrimaryAutomationNameProperty); set => SetValue(PrimaryAutomationNameProperty, value); }
    public string? TrailingAutomationName { get => GetValue(TrailingAutomationNameProperty); set => SetValue(TrailingAutomationNameProperty, value); }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":filled", Variant == MdToggleButtonVariant.Filled);
        PseudoClasses.Set(":tonal", Variant == MdToggleButtonVariant.Tonal);
        PseudoClasses.Set(":outlined", Variant == MdToggleButtonVariant.Outlined);
        PseudoClasses.Set(":elevated", Variant == MdToggleButtonVariant.Elevated);
        PseudoClasses.Set(":xsmall", Size == MdButtonSize.ExtraSmall);
        PseudoClasses.Set(":small", Size == MdButtonSize.Small);
        PseudoClasses.Set(":medium", Size == MdButtonSize.Medium);
        PseudoClasses.Set(":large", Size == MdButtonSize.Large);
        PseudoClasses.Set(":xlarge", Size == MdButtonSize.ExtraLarge);
        PseudoClasses.Set(":has-leading-icon", LeadingIcon is not null);
        PseudoClasses.Set(":rtl", FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft);
    }
}
