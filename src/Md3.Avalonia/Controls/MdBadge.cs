using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>A Material 3 notification, count or status badge.</summary>
[PseudoClasses(":dot", ":label")]
public sealed class MdBadge : ContentControl
{
    public static readonly StyledProperty<MdBadgeVariant> VariantProperty =
        AvaloniaProperty.Register<MdBadge, MdBadgeVariant>(nameof(Variant));

    static MdBadge()
    {
        VariantProperty.Changed.AddClassHandler<MdBadge>((badge, _) => badge.UpdatePseudoClasses());
    }

    public MdBadge() => UpdatePseudoClasses();

    public MdBadgeVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":dot", Variant == MdBadgeVariant.Dot);
        PseudoClasses.Set(":label", Variant == MdBadgeVariant.Label);
    }
}
