using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>An M3 Expressive standard button group with size-aware spacing.</summary>
[PseudoClasses(":xsmall", ":small", ":medium", ":large", ":xlarge")]
public class MdStandardButtonGroup : ItemsControl
{
    public static readonly StyledProperty<MdButtonSize> SizeProperty = AvaloniaProperty.Register<MdStandardButtonGroup, MdButtonSize>(nameof(Size), MdButtonSize.Small);
    public static readonly StyledProperty<double> ItemSpacingProperty = AvaloniaProperty.Register<MdStandardButtonGroup, double>(nameof(ItemSpacing), 12);

    static MdStandardButtonGroup() => SizeProperty.Changed.AddClassHandler<MdStandardButtonGroup>((x, _) => x.UpdatePseudoClasses());
    public MdStandardButtonGroup() => UpdatePseudoClasses();

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
}
