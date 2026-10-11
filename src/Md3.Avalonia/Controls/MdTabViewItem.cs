using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled) return;

        // Upstream TabControl does not reliably move the selection with the arrow keys (the
        // conformance audit caught Right/Left doing nothing on a realized view). Material tabs
        // must be fully keyboard operable, so the item moves its owner's selection itself and
        // hands focus to the newly selected container.
        var owner = this.GetVisualAncestors().OfType<TabControl>().FirstOrDefault();
        if (owner is null || owner.ItemCount < 2) return;

        var horizontal = TabStripPlacement is Dock.Top or Dock.Bottom;
        var delta = e.Key switch
        {
            Key.Right when horizontal => 1,
            Key.Left when horizontal => -1,
            Key.Down when !horizontal => 1,
            Key.Up when !horizontal => -1,
            _ => 0,
        };
        if (delta == 0) return;

        var target = (owner.SelectedIndex + delta + owner.ItemCount) % owner.ItemCount;
        owner.SelectedIndex = target;
        if (owner.ContainerFromIndex(target) is { } container)
        {
            container.Focus();
        }

        e.Handled = true;
    }

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
