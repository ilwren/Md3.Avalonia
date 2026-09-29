using Avalonia;
using Avalonia.Controls;

namespace Md3.Avalonia.Controls;

/// <summary>Places a Material badge at the top-trailing corner of arbitrary content.</summary>
public sealed class MdBadgedBox : ContentControl
{
    public static readonly StyledProperty<object?> BadgeProperty =
        AvaloniaProperty.Register<MdBadgedBox, object?>(nameof(Badge));

    public static readonly StyledProperty<MdBadgeVariant> BadgeVariantProperty =
        AvaloniaProperty.Register<MdBadgedBox, MdBadgeVariant>(nameof(BadgeVariant));

    public object? Badge
    {
        get => GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }

    public MdBadgeVariant BadgeVariant
    {
        get => GetValue(BadgeVariantProperty);
        set => SetValue(BadgeVariantProperty, value);
    }
}
