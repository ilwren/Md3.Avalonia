using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

public enum MdSettingsCardVariant
{
    Flat,
    Filled,
    Elevated,
    Outlined
}

/// <summary>
/// A Material 3 / Android styled preference settings card with icon, header, description, and trailing widget.
/// </summary>
[PseudoClasses(":has-icon", ":has-description", ":has-trailing", ":clickable", ":filled", ":elevated", ":outlined", ":flat", ":reduced-motion", ":no-motion")]
public class MdSettingsCard : ContentControl
{
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<MdSettingsCard, object?>(nameof(Header));

    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<MdSettingsCard, object?>(nameof(Description));

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<MdSettingsCard, object?>(nameof(Icon));

    public static readonly StyledProperty<IBrush?> IconBackgroundProperty =
        AvaloniaProperty.Register<MdSettingsCard, IBrush?>(nameof(IconBackground));

    public static readonly StyledProperty<IBrush?> IconForegroundProperty =
        AvaloniaProperty.Register<MdSettingsCard, IBrush?>(nameof(IconForeground));

    public static readonly StyledProperty<object?> TrailingProperty =
        AvaloniaProperty.Register<MdSettingsCard, object?>(nameof(Trailing));

    public static readonly StyledProperty<bool> IsClickableProperty =
        AvaloniaProperty.Register<MdSettingsCard, bool>(nameof(IsClickable), true);

    public static readonly StyledProperty<MdSettingsCardVariant> VariantProperty =
        AvaloniaProperty.Register<MdSettingsCard, MdSettingsCardVariant>(nameof(Variant), MdSettingsCardVariant.Flat);

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<MdSettingsCard, ICommand?>(nameof(Command));

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<MdSettingsCard, object?>(nameof(CommandParameter));

    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<MdSettingsCard, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);

    static MdSettingsCard()
    {
        HeaderProperty.Changed.AddClassHandler<MdSettingsCard>((card, _) => card.UpdatePseudoClasses());
        DescriptionProperty.Changed.AddClassHandler<MdSettingsCard>((card, _) => card.UpdatePseudoClasses());
        IconProperty.Changed.AddClassHandler<MdSettingsCard>((card, _) => card.UpdatePseudoClasses());
        TrailingProperty.Changed.AddClassHandler<MdSettingsCard>((card, _) => card.UpdatePseudoClasses());
        IsClickableProperty.Changed.AddClassHandler<MdSettingsCard>((card, _) => card.UpdatePseudoClasses());
        VariantProperty.Changed.AddClassHandler<MdSettingsCard>((card, _) => card.UpdatePseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdSettingsCard>((card, _) => card.UpdateMotion());
    }

    public MdSettingsCard()
    {
        UpdatePseudoClasses();
    }

    public object? Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
    public object? Description { get => GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }
    public object? Icon { get => GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public IBrush? IconBackground { get => GetValue(IconBackgroundProperty); set => SetValue(IconBackgroundProperty, value); }
    public IBrush? IconForeground { get => GetValue(IconForegroundProperty); set => SetValue(IconForegroundProperty, value); }
    public object? Trailing { get => GetValue(TrailingProperty); set => SetValue(TrailingProperty, value); }
    public bool IsClickable { get => GetValue(IsClickableProperty); set => SetValue(IsClickableProperty, value); }
    public MdSettingsCardVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public ICommand? Command { get => GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
    public object? CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }

    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (IsClickable && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            Focus();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (IsClickable)
        {
            RaiseClick();
            e.Handled = true;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (IsClickable && (e.Key == Key.Enter || e.Key == Key.Space))
        {
            RaiseClick();
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void RaiseClick()
    {
        var args = new RoutedEventArgs(ClickEvent, this);
        RaiseEvent(args);
        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":clickable", IsClickable);
        PseudoClasses.Set(":has-icon", Icon is not null);
        PseudoClasses.Set(":has-description", Description is not null);
        PseudoClasses.Set(":has-trailing", Trailing is not null);
        PseudoClasses.Set(":filled", Variant == MdSettingsCardVariant.Filled);
        PseudoClasses.Set(":elevated", Variant == MdSettingsCardVariant.Elevated);
        PseudoClasses.Set(":outlined", Variant == MdSettingsCardVariant.Outlined);
        PseudoClasses.Set(":flat", Variant == MdSettingsCardVariant.Flat);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
    }
}
