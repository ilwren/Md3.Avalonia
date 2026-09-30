using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;

namespace Md3.Avalonia.Controls;

/// <summary>A selectable Material tab with optional icon, badge, and header slots.</summary>
[PseudoClasses(":primary", ":secondary", ":has-icon", ":has-badge", ":inline", ":shared-indicator")]
public sealed class MdTabItem : ListBoxItem
{
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<MdTabItem, object?>(nameof(Header));
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<MdTabItem, IDataTemplate?>(nameof(HeaderTemplate));
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
        HeaderProperty.Changed.AddClassHandler<MdTabItem>((item, e) =>
        {
            if (item.Content is null || Equals(item.Content, e.OldValue))
                item.SetCurrentValue(ContentProperty, e.NewValue);
        });
        ContentProperty.Changed.AddClassHandler<MdTabItem>((item, e) =>
        {
            if (item.Header is null || Equals(item.Header, e.OldValue))
                item.SetCurrentValue(HeaderProperty, e.NewValue);
        });
        IconProperty.Changed.AddClassHandler<MdTabItem>((item, _) => item.UpdatePseudoClasses());
        BadgeProperty.Changed.AddClassHandler<MdTabItem>((item, _) => item.UpdatePseudoClasses());
        IsIconInlineProperty.Changed.AddClassHandler<MdTabItem>((item, _) => item.UpdatePseudoClasses());
        UseSharedIndicatorProperty.Changed.AddClassHandler<MdTabItem>((item, _) => item.UpdatePseudoClasses());
        MdTabs.VariantProperty.Changed.AddClassHandler<MdTabItem>((item, _) => item.UpdatePseudoClasses());
    }

    public MdTabItem() => UpdatePseudoClasses();

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

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
