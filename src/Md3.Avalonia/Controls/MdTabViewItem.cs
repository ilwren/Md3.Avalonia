using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A Material tab header paired with lazily presented page content.</summary>
[PseudoClasses(":has-icon", ":has-badge", ":reduced-motion", ":no-motion")]
public sealed class MdTabViewItem : TabItem
{
    private Border? _indicator;
    public static readonly StyledProperty<object?> BadgeProperty = AvaloniaProperty.Register<MdTabViewItem, object?>(nameof(Badge));
    public static readonly StyledProperty<MdTabVariant> VariantProperty =
        AvaloniaProperty.Register<MdTabViewItem, MdTabVariant>(nameof(Variant), MdTabVariant.Secondary);

    static MdTabViewItem()
    {
        IconProperty.Changed.AddClassHandler<MdTabViewItem>((item, _) => item.UpdatePseudoClasses());
        BadgeProperty.Changed.AddClassHandler<MdTabViewItem>((item, _) => item.UpdatePseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdTabViewItem>((item, _) => item.UpdateMotion());
    }
    public MdTabViewItem() => UpdatePseudoClasses();
    public object? Badge { get => GetValue(BadgeProperty); set => SetValue(BadgeProperty, value); }

    /// <summary>Material tab style: primary (full-width indicator, text-first) or secondary.</summary>
    public MdTabVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _indicator = e.NameScope.Find<Border>("PART_Indicator");
        UpdateMotion();
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_indicator is not null)
            _indicator.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, WidthProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast));
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":has-icon", Icon is not null);
        PseudoClasses.Set(":has-badge", Badge is not null);
    }
}
