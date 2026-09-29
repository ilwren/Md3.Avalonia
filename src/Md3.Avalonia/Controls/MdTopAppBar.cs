using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>A Material 3 top app bar with small and expressive flexible configurations.</summary>
[PseudoClasses(":small", ":medium-flexible", ":large-flexible", ":centered", ":scrolled", ":has-subtitle")]
public sealed class MdTopAppBar : ContentControl
{
    public static readonly StyledProperty<MdTopAppBarVariant> VariantProperty =
        AvaloniaProperty.Register<MdTopAppBar, MdTopAppBarVariant>(nameof(Variant));
    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<MdTopAppBar, object?>(nameof(Title));
    public static readonly StyledProperty<object?> SubtitleProperty =
        AvaloniaProperty.Register<MdTopAppBar, object?>(nameof(Subtitle));
    public static readonly StyledProperty<object?> LeadingContentProperty =
        AvaloniaProperty.Register<MdTopAppBar, object?>(nameof(LeadingContent));
    public static readonly StyledProperty<object?> TrailingContentProperty =
        AvaloniaProperty.Register<MdTopAppBar, object?>(nameof(TrailingContent));
    public static readonly StyledProperty<bool> IsTitleCenteredProperty =
        AvaloniaProperty.Register<MdTopAppBar, bool>(nameof(IsTitleCentered));
    public static readonly StyledProperty<bool> IsScrolledProperty =
        AvaloniaProperty.Register<MdTopAppBar, bool>(nameof(IsScrolled));

    static MdTopAppBar()
    {
        VariantProperty.Changed.AddClassHandler<MdTopAppBar>((bar, _) => bar.UpdatePseudoClasses());
        IsTitleCenteredProperty.Changed.AddClassHandler<MdTopAppBar>((bar, _) => bar.UpdatePseudoClasses());
        IsScrolledProperty.Changed.AddClassHandler<MdTopAppBar>((bar, _) => bar.UpdatePseudoClasses());
        SubtitleProperty.Changed.AddClassHandler<MdTopAppBar>((bar, _) => bar.UpdatePseudoClasses());
    }

    public MdTopAppBar() => UpdatePseudoClasses();

    public MdTopAppBarVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public object? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public object? Subtitle { get => GetValue(SubtitleProperty); set => SetValue(SubtitleProperty, value); }
    public object? LeadingContent { get => GetValue(LeadingContentProperty); set => SetValue(LeadingContentProperty, value); }
    public object? TrailingContent { get => GetValue(TrailingContentProperty); set => SetValue(TrailingContentProperty, value); }
    public bool IsTitleCentered { get => GetValue(IsTitleCenteredProperty); set => SetValue(IsTitleCenteredProperty, value); }
    public bool IsScrolled { get => GetValue(IsScrolledProperty); set => SetValue(IsScrolledProperty, value); }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":small", Variant == MdTopAppBarVariant.Small);
        PseudoClasses.Set(":medium-flexible", Variant == MdTopAppBarVariant.MediumFlexible);
        PseudoClasses.Set(":large-flexible", Variant == MdTopAppBarVariant.LargeFlexible);
        PseudoClasses.Set(":centered", IsTitleCentered);
        PseudoClasses.Set(":scrolled", IsScrolled);
        PseudoClasses.Set(":has-subtitle", Subtitle is not null);
    }
}
