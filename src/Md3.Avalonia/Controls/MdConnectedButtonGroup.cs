using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

/// <summary>An M3 Expressive connected button group with two-DIP gaps and connected shapes.</summary>
[PseudoClasses(":xsmall", ":small", ":medium", ":large", ":xlarge")]
public class MdConnectedButtonGroup : ItemsControl
{
    public static readonly StyledProperty<MdButtonSize> SizeProperty = AvaloniaProperty.Register<MdConnectedButtonGroup, MdButtonSize>(nameof(Size), MdButtonSize.Small);
    public static readonly StyledProperty<double> ItemSpacingProperty = AvaloniaProperty.Register<MdConnectedButtonGroup, double>(nameof(ItemSpacing), 2);

    static MdConnectedButtonGroup() => SizeProperty.Changed.AddClassHandler<MdConnectedButtonGroup>((x, _) => x.UpdatePseudoClasses());

    public MdConnectedButtonGroup()
    {
        UpdatePseudoClasses();
        LayoutUpdated += (_, _) => UpdateConnectedShapes();
    }

    public MdButtonSize Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public double ItemSpacing { get => GetValue(ItemSpacingProperty); set => SetValue(ItemSpacingProperty, value); }

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
        var buttons = this.GetVisualDescendants()
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
            var radius = buttons.Length == 1
                ? new CornerRadius(outer)
                : index == 0
                    ? new CornerRadius(outer, inner, inner, outer)
                    : index == buttons.Length - 1
                        ? new CornerRadius(inner, outer, outer, inner)
                        : new CornerRadius(inner);

            if (buttons[index] is MdButton button)
            {
                button.SetCurrentValue(MdButton.ContainerCornerRadiusProperty, radius);
            }
            else if (buttons[index] is MdToggleButton toggle)
            {
                toggle.SetCurrentValue(MdToggleButton.EnableSelectedShapeMorphProperty, false);
                toggle.SetCurrentValue(MdToggleButton.ContainerCornerRadiusProperty, radius);
            }
        }
    }
}
