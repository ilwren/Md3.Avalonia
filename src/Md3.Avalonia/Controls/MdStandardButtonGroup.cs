using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Data;

namespace Md3.Avalonia.Controls;

/// <summary>An M3 Expressive standard button group with size-aware spacing.</summary>
[PseudoClasses(":xsmall", ":small", ":medium", ":large", ":xlarge")]
public class MdStandardButtonGroup : ItemsControl
{
    private readonly Dictionary<Control, IDisposable> _sizeManagedValues = [];

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
        if (_sizeManagedValues.Remove(container, out var managedValue)) managedValue.Dispose();
        base.ClearContainerForItemOverride(container);
    }

    private void ManageContainerSize(Control container)
    {
        IDisposable? managedValue = container switch
        {
            MdButton button => button.SetValue(MdButton.SizeProperty, Size, BindingPriority.Style),
            MdToggleButton toggle => toggle.SetValue(MdToggleButton.SizeProperty, Size, BindingPriority.Style),
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
}
