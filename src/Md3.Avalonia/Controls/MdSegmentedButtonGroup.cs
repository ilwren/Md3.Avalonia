using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

/// <summary>Material segmented buttons with native ListBox single or multiple selection.</summary>
[PseudoClasses(":single", ":multiple")]
public sealed class MdSegmentedButtonGroup : ListBox
{
    public static readonly StyledProperty<bool> AllowMultipleProperty =
        AvaloniaProperty.Register<MdSegmentedButtonGroup, bool>(nameof(AllowMultiple));

    static MdSegmentedButtonGroup() =>
        AllowMultipleProperty.Changed.AddClassHandler<MdSegmentedButtonGroup>((group, _) => group.UpdateMode());

    public MdSegmentedButtonGroup()
    {
        UpdateMode();
        LayoutUpdated += (_, _) => UpdateShapes();
    }

    public bool AllowMultiple { get => GetValue(AllowMultipleProperty); set => SetValue(AllowMultipleProperty, value); }

    private void UpdateMode()
    {
        SetCurrentValue(SelectionModeProperty, AllowMultiple ? SelectionMode.Multiple : SelectionMode.Single);
        PseudoClasses.Set(":multiple", AllowMultiple);
        PseudoClasses.Set(":single", !AllowMultiple);
    }

    private void UpdateShapes()
    {
        var items = this.GetVisualDescendants().OfType<MdSegmentedButton>().ToArray();
        for (var i = 0; i < items.Length; i++)
        {
            var radius = items.Length == 1 ? new CornerRadius(20) :
                i == 0 ? new CornerRadius(20, 0, 0, 20) :
                i == items.Length - 1 ? new CornerRadius(0, 20, 20, 0) : new CornerRadius(0);
            items[i].SetCurrentValue(MdSegmentedButton.ContainerCornerRadiusProperty, radius);
        }
    }
}
