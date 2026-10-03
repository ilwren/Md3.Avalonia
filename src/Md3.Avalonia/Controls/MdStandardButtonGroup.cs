using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>An M3 Expressive standard button group with size-aware spacing.</summary>
[PseudoClasses(":xsmall", ":small", ":medium", ":large", ":xlarge")]
public class MdStandardButtonGroup : ItemsControl
{
    private readonly HashSet<Control> _sizeManagedContainers = [];

    public static readonly StyledProperty<MdButtonSize> SizeProperty = AvaloniaProperty.Register<MdStandardButtonGroup, MdButtonSize>(nameof(Size), MdButtonSize.Small);
    public static readonly StyledProperty<double> ItemSpacingProperty = AvaloniaProperty.Register<MdStandardButtonGroup, double>(nameof(ItemSpacing), 12);

    static MdStandardButtonGroup() => SizeProperty.Changed.AddClassHandler<MdStandardButtonGroup>((x, _) =>
    {
        x.UpdatePseudoClasses();
        x.UpdateManagedContainerSizes();
    });
    public MdStandardButtonGroup() => UpdatePseudoClasses();

    public MdButtonSize Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public double ItemSpacing { get => GetValue(ItemSpacingProperty); set => SetValue(ItemSpacingProperty, value); }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        ManageContainerSize(container);
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (_sizeManagedContainers.Remove(container))
        {
            if (container is MdButton button) button.ClearValue(MdButton.SizeProperty);
            else if (container is MdToggleButton toggle) toggle.ClearValue(MdToggleButton.SizeProperty);
        }
        base.ClearContainerForItemOverride(container);
    }

    private void ManageContainerSize(Control container)
    {
        if (container is MdButton button && (_sizeManagedContainers.Contains(button) || !button.IsSet(MdButton.SizeProperty)))
        {
            _sizeManagedContainers.Add(button);
            button.SetCurrentValue(MdButton.SizeProperty, Size);
        }
        else if (container is MdToggleButton toggle && (_sizeManagedContainers.Contains(toggle) || toggle.ReadLocalValue(MdToggleButton.SizeProperty) == AvaloniaProperty.UnsetValue))
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
}
