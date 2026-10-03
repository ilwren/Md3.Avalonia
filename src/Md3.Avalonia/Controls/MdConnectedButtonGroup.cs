using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Data;

namespace Md3.Avalonia.Controls;

/// <summary>An M3 Expressive connected button group with two-DIP gaps and connected shapes.</summary>
[PseudoClasses(":xsmall", ":small", ":medium", ":large", ":xlarge")]
public class MdConnectedButtonGroup : ItemsControl
{
    private readonly Dictionary<Control, IDisposable> _sizeManagedValues = [];
    private readonly Dictionary<Control, IDisposable?> _shapeManagedValues = [];
    private readonly Dictionary<MdToggleButton, IDisposable?> _shapeMorphManagedValues = [];
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
        if (_sizeManagedValues.Remove(container, out var sizeValue)) sizeValue.Dispose();
        if (_shapeManagedValues.Remove(container, out var shapeValue)) shapeValue?.Dispose();
        if (container is MdToggleButton toggle && _shapeMorphManagedValues.Remove(toggle, out var morphValue)) morphValue?.Dispose();
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
        IDisposable? managedValue = container switch
        {
            MdButton button => button.SetValue(MdButton.SizeProperty, Size, BindingPriority.StyleTrigger),
            MdToggleButton toggle => toggle.SetValue(MdToggleButton.SizeProperty, Size, BindingPriority.StyleTrigger),
            _ => null
        };
        if (managedValue is null) return;
        if (_sizeManagedValues.Remove(container, out var previous)) previous.Dispose();
        _sizeManagedValues[container] = managedValue;
    }

    private void UpdateManagedContainerSizes()
    {
        foreach (var container in _sizeManagedValues.Keys.ToArray()) ManageContainerSize(container);
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
        var buttons = Enumerable.Range(0, ItemCount)
            .Select(index => ContainerFromIndex(index) as Control ?? Items[index] as Control)
            .Where(control => control is MdButton or MdToggleButton)
            .Cast<Control>()
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

            IDisposable? shapeValue;
            if (buttons[index] is MdButton button)
            {
                shapeValue = button.SetValue(MdButton.ContainerCornerRadiusProperty, radius, BindingPriority.StyleTrigger);
            }
            else if (buttons[index] is MdToggleButton toggle)
            {
                shapeValue = toggle.SetValue(MdToggleButton.ContainerCornerRadiusProperty, radius, BindingPriority.StyleTrigger);
                var morphValue = toggle.SetValue(MdToggleButton.EnableSelectedShapeMorphProperty, false, BindingPriority.StyleTrigger);
                if (_shapeMorphManagedValues.Remove(toggle, out var previousMorph)) previousMorph?.Dispose();
                _shapeMorphManagedValues[toggle] = morphValue;
            }
            else
            {
                continue;
            }

            if (_shapeManagedValues.Remove(buttons[index], out var previousShape)) previousShape?.Dispose();
            _shapeManagedValues[buttons[index]] = shapeValue;
        }
    }
}
