using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Styling;

namespace Md3.Avalonia.Controls;

/// <summary>A selectable Material 3 standard or expressive segmented list.</summary>
[PseudoClasses(":standard", ":segmented")]
public sealed class MdList : ListBox
{
    public static readonly StyledProperty<MdListVariant> VariantProperty =
        AvaloniaProperty.Register<MdList, MdListVariant>(nameof(Variant));
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<MdList, double>(nameof(ItemSpacing));

    static MdList()
    {
        VariantProperty.Changed.AddClassHandler<MdList>((list, _) =>
        {
            list.UpdatePseudoClasses();
            list.UpdateRealizedContainers();
        });
        ItemSpacingProperty.Changed.AddClassHandler<MdList>((list, _) => list.UpdateRealizedContainers());
    }

    public MdList() => UpdatePseudoClasses();

    public MdListVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public double ItemSpacing { get => GetValue(ItemSpacingProperty); set => SetValue(ItemSpacingProperty, value); }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        PrepareContainer(container);
    }

    private void UpdateRealizedContainers()
    {
        foreach (var container in GetRealizedContainers())
        {
            PrepareContainer(container);
        }
    }

    private void PrepareContainer(Control container)
    {
        if (container is MdListItem materialItem)
        {
            materialItem.SetCurrentValue(MarginProperty, new Thickness(0, 0, 0, ItemSpacing));
            materialItem.SetCurrentValue(ListBoxItem.CornerRadiusProperty,
                Variant == MdListVariant.Segmented ? new CornerRadius(16) : new CornerRadius(0));
        }
        else if (container is ListBoxItem item &&
                 ResourceNodeExtensions.FindResource(this, "MdListGeneratedItemTheme") is ControlTheme theme)
        {
            item.SetCurrentValue(ThemeProperty, theme);
            item.SetCurrentValue(MarginProperty, new Thickness(0, 0, 0, ItemSpacing));
            item.SetCurrentValue(ListBoxItem.CornerRadiusProperty,
                Variant == MdListVariant.Segmented ? new CornerRadius(16) : new CornerRadius(0));
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":standard", Variant == MdListVariant.Standard);
        PseudoClasses.Set(":segmented", Variant == MdListVariant.Segmented);
    }
}
