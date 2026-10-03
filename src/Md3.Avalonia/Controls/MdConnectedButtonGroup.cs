using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>An M3 Expressive connected button group with two-DIP gaps and connected shapes.</summary>
[PseudoClasses(":xsmall", ":small", ":medium", ":large", ":xlarge")]
public class MdConnectedButtonGroup : ItemsControl
{
    private readonly HashSet<Control> _sizeManagedContainers = [];
    private readonly HashSet<Control> _shapeManagedContainers = [];
    private bool _shapesScheduled;

    public static readonly StyledProperty<MdButtonSize> SizeProperty = AvaloniaProperty.Register<MdConnectedButtonGroup, MdButtonSize>(nameof(Size), MdButtonSize.Small);
    public static readonly StyledProperty<double> ItemSpacingProperty = AvaloniaProperty.Register<MdConnectedButtonGroup, double>(nameof(ItemSpacing), 2);

    static MdConnectedButtonGroup()
    {
        SizeProperty.Changed.AddClassHandler<MdConnectedButtonGroup>((group, _) =>
        {
            group.UpdatePseudoClasses();
            group.UpdateManagedContainerSizes();
            group.UpdateConnectedShapes();
        });
        FlowDirectionProperty.Changed.AddClassHandler<MdConnectedButtonGroup>((group, _) => group.UpdateConnectedShapes());
    }

    public MdConnectedButtonGroup() => UpdatePseudoClasses();

    public MdButtonSize Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public double ItemSpacing { get => GetValue(ItemSpacingProperty); set => SetValue(ItemSpacingProperty, value); }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        ManageContainerSize(container);
        if (!_shapesScheduled)
        {
            _shapesScheduled = true;
            LayoutUpdated += UpdateShapesOnce;
        }
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (_sizeManagedContainers.Remove(container))
        {
            if (container is MdButton button) button.ClearValue(MdButton.SizeProperty);
            else if (container is MdToggleButton toggle) toggle.ClearValue(MdToggleButton.SizeProperty);
        }
        if (_shapeManagedContainers.Remove(container))
        {
            if (container is MdButton button) button.ClearValue(MdButton.ContainerCornerRadiusProperty);
            else if (container is MdToggleButton toggle)
            {
                toggle.ClearValue(MdToggleButton.ContainerCornerRadiusProperty);
                toggle.ClearValue(MdToggleButton.EnableSelectedShapeMorphProperty);
            }
        }
        base.ClearContainerForItemOverride(container);
    }

    private void UpdateShapesOnce(object? sender, EventArgs e)
    {
        LayoutUpdated -= UpdateShapesOnce;
        _shapesScheduled = false;
        UpdateConnectedShapes();
    }

    private void ManageContainerSize(Control container)
    {
        if (container is MdButton button && (_sizeManagedContainers.Contains(button) || !button.IsSet(MdButton.SizeProperty)))
        {
            _sizeManagedContainers.Add(button);
            button.SetCurrentValue(MdButton.SizeProperty, Size);
        }
        else if (container is MdToggleButton toggle && (_sizeManagedContainers.Contains(toggle) || !toggle.IsSet(MdToggleButton.SizeProperty)))
        {
            _sizeManagedContainers.Add(toggle);
            toggle.SetCurrentValue(MdToggleButton.SizeProperty, Size);
        }
    }

    private void UpdateManagedContainerSizes()
    {
        foreach (var container in _sizeManagedContainers.ToArray()) ManageContainerSize(container);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":xsmall", Size == MdButtonSize.ExtraSmall);
        PseudoClasses.Set(":small", Size == MdButtonSize.Small);
        PseudoClasses.Set(":medium", Size == MdButtonSize.Medium);
        PseudoClasses.Set(":large", Size == MdButtonSize.Large);
        PseudoClasses.Set(":xlarge", Size == MdButtonSize.ExtraLarge);
    }

    private void UpdateConnectedShapes()
    {
        // Only direct realized item containers participate. Nested buttons inside an item's
        // content belong to that item and must never be resized or reshaped by the group.
        var buttons = GetRealizedContainers()
            .Where(control => control is MdButton or MdToggleButton)
            .ToArray();
        if (buttons.Length == 0)
        {
            return;
        }

        var outer = Size switch
        {
            MdButtonSize.ExtraSmall => 16d,
            MdButtonSize.Small => 20d,
            MdButtonSize.Medium => 28d,
            MdButtonSize.Large => 48d,
            MdButtonSize.ExtraLarge => 68d,
            _ => 20d
        };
        const double inner = 0d;

        for (var index = 0; index < buttons.Length; index++)
        {
            var firstRadius = FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft
                ? new CornerRadius(inner, outer, outer, inner)
                : new CornerRadius(outer, inner, inner, outer);
            var lastRadius = FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft
                ? new CornerRadius(outer, inner, inner, outer)
                : new CornerRadius(inner, outer, outer, inner);
            var radius = buttons.Length == 1
                ? new CornerRadius(outer)
                : index == 0
                    ? firstRadius
                    : index == buttons.Length - 1
                        ? lastRadius
                        : new CornerRadius(inner);

            if (buttons[index] is MdButton button &&
                (_shapeManagedContainers.Contains(button) || !button.IsSet(MdButton.ContainerCornerRadiusProperty)))
            {
                _shapeManagedContainers.Add(button);
                button.SetCurrentValue(MdButton.ContainerCornerRadiusProperty, radius);
            }
            else if (buttons[index] is MdToggleButton toggle &&
                     (_shapeManagedContainers.Contains(toggle) || !toggle.IsSet(MdToggleButton.ContainerCornerRadiusProperty)))
            {
                _shapeManagedContainers.Add(toggle);
                if (!toggle.IsSet(MdToggleButton.EnableSelectedShapeMorphProperty))
                    toggle.SetCurrentValue(MdToggleButton.EnableSelectedShapeMorphProperty, false);
                toggle.SetCurrentValue(MdToggleButton.ContainerCornerRadiusProperty, radius);
            }
        }
    }
}
