using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

/// <summary>An M3 Expressive standard button group with size-aware spacing and font size propagation.</summary>
[PseudoClasses(":xsmall", ":small", ":medium", ":large", ":xlarge")]
public class MdStandardButtonGroup : ItemsControl
{
    public static readonly StyledProperty<MdButtonSize> SizeProperty = AvaloniaProperty.Register<MdStandardButtonGroup, MdButtonSize>(nameof(Size), MdButtonSize.Small);
    public static readonly StyledProperty<double> ItemSpacingProperty = AvaloniaProperty.Register<MdStandardButtonGroup, double>(nameof(ItemSpacing), 12);

    static MdStandardButtonGroup()
    {
        SizeProperty.Changed.AddClassHandler<MdStandardButtonGroup>((x, _) => x.UpdatePseudoClasses());
        FontSizeProperty.Changed.AddClassHandler<MdStandardButtonGroup>((group, _) => group.UpdateChildFontSizes());
        FontWeightProperty.Changed.AddClassHandler<MdStandardButtonGroup>((group, _) => group.UpdateChildFontSizes());
    }

    public MdStandardButtonGroup()
    {
        UpdatePseudoClasses();
        LayoutUpdated += (_, _) => UpdateChildFontSizes();
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

    private void UpdateChildFontSizes()
    {
        var fontSize = FontSize;
        if (double.IsNaN(fontSize) || fontSize <= 0) return;
        foreach (var btn in this.GetVisualDescendants().OfType<Control>())
        {
            if (btn is MdButton or MdToggleButton or Button)
            {
                btn.SetCurrentValue(FontSizeProperty, fontSize);
                if (IsSet(FontWeightProperty))
                    btn.SetCurrentValue(FontWeightProperty, FontWeight);
            }
        }
    }
}
