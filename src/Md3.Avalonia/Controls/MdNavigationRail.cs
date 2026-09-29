using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>Material navigation rail for medium and expanded window widths.</summary>
[PseudoClasses(":collapsed", ":expanded", ":reduced-motion", ":no-motion")]
public sealed class MdNavigationRail : ListBox
{
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<MdNavigationRail, bool>(nameof(IsExpanded));
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<MdNavigationRail, object?>(nameof(Header));
    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<MdNavigationRail, object?>(nameof(Footer));

    static MdNavigationRail()
    {
        IsExpandedProperty.Changed.AddClassHandler<MdNavigationRail>((rail, _) =>
        {
            rail.UpdatePseudoClasses();
            rail.UpdateRealizedItems();
        });
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdNavigationRail>((rail, _) => rail.UpdateMotion());
    }

    public MdNavigationRail()
    {
        UpdatePseudoClasses();
        UpdateMotion();
    }

    public bool IsExpanded { get => GetValue(IsExpandedProperty); set => SetValue(IsExpandedProperty, value); }
    public object? Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
    public object? Footer { get => GetValue(FooterProperty); set => SetValue(FooterProperty, value); }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is MdNavigationRailItem materialItem)
            materialItem.SetCurrentValue(MdNavigationRailItem.IsExpandedProperty, IsExpanded);
    }

    private void UpdateRealizedItems()
    {
        foreach (var container in GetRealizedContainers())
        {
            if (container is MdNavigationRailItem item)
                item.SetCurrentValue(MdNavigationRailItem.IsExpandedProperty, IsExpanded);
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":expanded", IsExpanded);
        PseudoClasses.Set(":collapsed", !IsExpanded);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, WidthProperty, MdMotionKind.Spatial));
    }
}
