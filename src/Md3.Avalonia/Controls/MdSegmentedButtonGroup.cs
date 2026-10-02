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

    static MdSegmentedButtonGroup()
    {
        AllowMultipleProperty.Changed.AddClassHandler<MdSegmentedButtonGroup>((group, _) => group.UpdateMode());
        FontSizeProperty.Changed.AddClassHandler<MdSegmentedButtonGroup>((group, _) => group.UpdateChildFontSizes());
        FontWeightProperty.Changed.AddClassHandler<MdSegmentedButtonGroup>((group, _) => group.UpdateChildFontSizes());
    }

    public MdSegmentedButtonGroup()
    {
        UpdateMode();
        LayoutUpdated += (_, _) =>
        {
            UpdateShapes();
            UpdateChildFontSizes();
        };
    }

    public bool AllowMultiple { get => GetValue(AllowMultipleProperty); set => SetValue(AllowMultipleProperty, value); }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is MdSegmentedButton segmentedItem)
        {
            if (!double.IsNaN(FontSize) && FontSize > 0)
                segmentedItem.SetCurrentValue(FontSizeProperty, FontSize);
            if (IsSet(FontWeightProperty))
                segmentedItem.SetCurrentValue(FontWeightProperty, FontWeight);
        }
    }

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

    private void UpdateChildFontSizes()
    {
        var fontSize = FontSize;
        if (double.IsNaN(fontSize) || fontSize <= 0) return;
        foreach (var item in this.GetVisualDescendants().OfType<MdSegmentedButton>())
        {
            item.SetCurrentValue(FontSizeProperty, fontSize);
            if (IsSet(FontWeightProperty))
                item.SetCurrentValue(FontWeightProperty, FontWeight);
        }
    }
}
