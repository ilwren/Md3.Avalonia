using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Styling;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A three-to-five destination Material navigation bar retaining native ListBox selection.
/// Flexible is the current M3 Expressive default; Baseline remains available for compatibility.
/// </summary>
[PseudoClasses(":flexible", ":baseline", ":stacked-items", ":horizontal-items")]
public sealed class MdNavigationBar : ListBox
{
    public static readonly StyledProperty<MdNavigationBarVariant> VariantProperty =
        AvaloniaProperty.Register<MdNavigationBar, MdNavigationBarVariant>(nameof(Variant));
    public static readonly StyledProperty<MdNavigationBarLayout> ItemLayoutProperty =
        AvaloniaProperty.Register<MdNavigationBar, MdNavigationBarLayout>(nameof(ItemLayout));

    static MdNavigationBar()
    {
        VariantProperty.Changed.AddClassHandler<MdNavigationBar>((bar, _) => bar.UpdateState());
        ItemLayoutProperty.Changed.AddClassHandler<MdNavigationBar>((bar, _) =>
        {
            bar.UpdateState();
            bar.UpdateRealizedItems();
        });
    }

    public MdNavigationBar() => UpdateState();

    public MdNavigationBarVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public MdNavigationBarLayout ItemLayout { get => GetValue(ItemLayoutProperty); set => SetValue(ItemLayoutProperty, value); }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is MdNavigationBarItem materialItem)
        {
            materialItem.SetCurrentValue(MdNavigationBarItem.LayoutProperty, ItemLayout);
        }
        else if (container is ListBoxItem listItem &&
                 ResourceNodeExtensions.FindResource(this, "MdNavigationBarGeneratedItemTheme") is ControlTheme theme)
        {
            listItem.SetCurrentValue(ThemeProperty, theme);
        }
    }

    private void UpdateRealizedItems()
    {
        foreach (var container in GetRealizedContainers())
        {
            if (container is MdNavigationBarItem item)
            {
                item.SetCurrentValue(MdNavigationBarItem.LayoutProperty, ItemLayout);
            }
        }
    }

    private void UpdateState()
    {
        PseudoClasses.Set(":flexible", Variant == MdNavigationBarVariant.Flexible);
        PseudoClasses.Set(":baseline", Variant == MdNavigationBarVariant.Baseline);
        PseudoClasses.Set(":stacked-items", ItemLayout == MdNavigationBarLayout.Stacked);
        PseudoClasses.Set(":horizontal-items", ItemLayout == MdNavigationBarLayout.Horizontal);
    }
}
