using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>A selectable Material tab with optional icon and badge slots.</summary>
[PseudoClasses(":primary", ":secondary", ":has-icon", ":has-badge", ":inline", ":shared-indicator")]
public sealed class MdTabItem : ListBoxItem
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<MdTabItem, object?>(nameof(Icon));
    public static readonly StyledProperty<object?> BadgeProperty =
        AvaloniaProperty.Register<MdTabItem, object?>(nameof(Badge));
    public static readonly StyledProperty<bool> IsIconInlineProperty =
        AvaloniaProperty.Register<MdTabItem, bool>(nameof(IsIconInline));
    public static readonly StyledProperty<bool> UseSharedIndicatorProperty =
        AvaloniaProperty.Register<MdTabItem, bool>(nameof(UseSharedIndicator));

    static MdTabItem()
    {
        IconProperty.Changed.AddClassHandler<MdTabItem>((item, _) => item.UpdatePseudoClasses());
        BadgeProperty.Changed.AddClassHandler<MdTabItem>((item, _) => item.UpdatePseudoClasses());
        IsIconInlineProperty.Changed.AddClassHandler<MdTabItem>((item, _) => item.UpdatePseudoClasses());
        UseSharedIndicatorProperty.Changed.AddClassHandler<MdTabItem>((item, _) => item.UpdatePseudoClasses());
        MdTabs.VariantProperty.Changed.AddClassHandler<MdTabItem>((item, _) => item.UpdatePseudoClasses());
    }

    public MdTabItem() => UpdatePseudoClasses();

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public object? Badge
    {
        get => GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }

    public bool IsIconInline
    {
        get => GetValue(IsIconInlineProperty);
        set => SetValue(IsIconInlineProperty, value);
    }

    public bool UseSharedIndicator
    {
        get => GetValue(UseSharedIndicatorProperty);
        set => SetValue(UseSharedIndicatorProperty, value);
    }

    private void UpdatePseudoClasses()
    {
        var variant = MdTabs.GetVariant(this);
        PseudoClasses.Set(":primary", variant == MdTabVariant.Primary);
        PseudoClasses.Set(":secondary", variant == MdTabVariant.Secondary);
        PseudoClasses.Set(":has-icon", Icon is not null);
        PseudoClasses.Set(":has-badge", Badge is not null);
        PseudoClasses.Set(":inline", IsIconInline);
        PseudoClasses.Set(":shared-indicator", UseSharedIndicator);
    }
}
