using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Anchors a FAB-menu item stack outside the trigger's layout slot without changing the trigger's
/// resting position. The expansion direction is explicit and does not depend on the host's
/// <see cref="Layoutable.VerticalAlignment"/>.
/// </summary>
public sealed class MdFabMenuPanel : Panel
{
    public static readonly StyledProperty<MdFabMenuExpansionDirection> ExpansionDirectionProperty =
        AvaloniaProperty.Register<MdFabMenuPanel, MdFabMenuExpansionDirection>(
            nameof(ExpansionDirection), MdFabMenuExpansionDirection.Up);

    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<MdFabMenuPanel, double>(nameof(ItemSpacing), 8);

    static MdFabMenuPanel()
    {
        AffectsMeasure<MdFabMenuPanel>(ExpansionDirectionProperty, ItemSpacingProperty);
        AffectsArrange<MdFabMenuPanel>(ExpansionDirectionProperty, ItemSpacingProperty);
    }

    public MdFabMenuPanel()
    {
        ClipToBounds = false;
        AutomationProperties.SetAccessibilityView(this, AccessibilityView.Raw);
    }

    public MdFabMenuExpansionDirection ExpansionDirection
    {
        get => GetValue(ExpansionDirectionProperty);
        set => SetValue(ExpansionDirectionProperty, value);
    }

    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var constraint = new Size(availableSize.Width, double.PositiveInfinity);
        Control? trigger = null;
        Control? items = null;

        foreach (var child in Children)
        {
            child.Measure(constraint);
            if (child.Name == "PART_Trigger") trigger = child;
            else if (child.Name == "PART_MenuItems") items = child;
        }

        trigger ??= Children.OfType<Control>().LastOrDefault();
        items ??= Children.OfType<Control>().FirstOrDefault(child => !ReferenceEquals(child, trigger));

        var triggerSize = trigger?.DesiredSize ?? default;
        var itemSize = items?.DesiredSize ?? default;
        var width = Math.Max(triggerSize.Width, itemSize.Width);
        if (!double.IsInfinity(availableSize.Width)) width = Math.Min(width, availableSize.Width);

        // The panel occupies only the trigger slot. Menu actions are deliberately arranged outside
        // that slot so opening never moves an edge-anchored FAB.
        return new Size(Math.Max(0, width), Math.Max(0, triggerSize.Height));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Control? trigger = null;
        Control? items = null;
        foreach (var child in Children)
        {
            if (child.Name == "PART_Trigger") trigger = child;
            else if (child.Name == "PART_MenuItems") items = child;
        }

        trigger ??= Children.OfType<Control>().LastOrDefault();
        items ??= Children.OfType<Control>().FirstOrDefault(child => !ReferenceEquals(child, trigger));

        if (trigger is not null)
        {
            var triggerHeight = Math.Max(0, trigger.DesiredSize.Height);
            trigger.Arrange(new Rect(0, 0, finalSize.Width, triggerHeight));
        }

        if (items is not null)
        {
            var triggerHeight = Math.Max(0, trigger?.DesiredSize.Height ?? finalSize.Height);
            var itemHeight = Math.Max(0, items.DesiredSize.Height);
            var spacing = Math.Max(0, ItemSpacing);
            var y = ExpansionDirection == MdFabMenuExpansionDirection.Up
                ? -spacing - itemHeight
                : triggerHeight + spacing;
            items.Arrange(new Rect(0, y, finalSize.Width, itemHeight));
        }

        foreach (var child in Children.OfType<Control>())
        {
            if (!ReferenceEquals(child, trigger) && !ReferenceEquals(child, items))
                child.Arrange(new Rect(finalSize));
        }

        return finalSize;
    }
}
