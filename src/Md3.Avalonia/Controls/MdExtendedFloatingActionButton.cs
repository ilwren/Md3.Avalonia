using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Media;

namespace Md3.Avalonia.Controls;

/// <summary>A Material 3 Expressive extended FAB with icon and descriptive label.</summary>
[PseudoClasses(":small", ":medium", ":large", ":primary-container", ":secondary-container", ":tertiary-container", ":primary", ":secondary", ":tertiary", ":has-icon")]
public class MdExtendedFloatingActionButton : Button
{
    public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<MdExtendedFloatingActionButton, object?>(nameof(Icon));
    public static readonly StyledProperty<MdExtendedFabSize> SizeProperty = AvaloniaProperty.Register<MdExtendedFloatingActionButton, MdExtendedFabSize>(nameof(Size), MdExtendedFabSize.Small);
    public static readonly StyledProperty<MdFabColor> ColorStyleProperty = AvaloniaProperty.Register<MdExtendedFloatingActionButton, MdFabColor>(nameof(ColorStyle), MdFabColor.PrimaryContainer);
    public static readonly StyledProperty<double> ContainerHeightProperty = AvaloniaProperty.Register<MdExtendedFloatingActionButton, double>(nameof(ContainerHeight), 56);
    public static readonly StyledProperty<double> IconSizeProperty = AvaloniaProperty.Register<MdExtendedFloatingActionButton, double>(nameof(IconSize), 24);
    public static readonly StyledProperty<double> IconSpacingProperty = AvaloniaProperty.Register<MdExtendedFloatingActionButton, double>(nameof(IconSpacing), 8);
    public static readonly StyledProperty<CornerRadius> ContainerCornerRadiusProperty = AvaloniaProperty.Register<MdExtendedFloatingActionButton, CornerRadius>(nameof(ContainerCornerRadius), new CornerRadius(16));
    public static readonly StyledProperty<IBrush?> StateLayerBrushProperty = AvaloniaProperty.Register<MdExtendedFloatingActionButton, IBrush?>(nameof(StateLayerBrush));
    public static readonly StyledProperty<BoxShadows> ElevationProperty = AvaloniaProperty.Register<MdExtendedFloatingActionButton, BoxShadows>(nameof(Elevation));

    static MdExtendedFloatingActionButton()
    {
        SizeProperty.Changed.AddClassHandler<MdExtendedFloatingActionButton>((x, _) => x.UpdatePseudoClasses());
        ColorStyleProperty.Changed.AddClassHandler<MdExtendedFloatingActionButton>((x, _) => x.UpdatePseudoClasses());
        IconProperty.Changed.AddClassHandler<MdExtendedFloatingActionButton>((x, _) => x.UpdatePseudoClasses());
    }
    public MdExtendedFloatingActionButton() => UpdatePseudoClasses();

    public object? Icon { get => GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public MdExtendedFabSize Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public MdFabColor ColorStyle { get => GetValue(ColorStyleProperty); set => SetValue(ColorStyleProperty, value); }
    public double ContainerHeight { get => GetValue(ContainerHeightProperty); set => SetValue(ContainerHeightProperty, value); }
    public double IconSize { get => GetValue(IconSizeProperty); set => SetValue(IconSizeProperty, value); }
    public double IconSpacing { get => GetValue(IconSpacingProperty); set => SetValue(IconSpacingProperty, value); }
    public CornerRadius ContainerCornerRadius { get => GetValue(ContainerCornerRadiusProperty); set => SetValue(ContainerCornerRadiusProperty, value); }
    public IBrush? StateLayerBrush { get => GetValue(StateLayerBrushProperty); set => SetValue(StateLayerBrushProperty, value); }
    public BoxShadows Elevation { get => GetValue(ElevationProperty); set => SetValue(ElevationProperty, value); }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":small", Size == MdExtendedFabSize.Small);
        PseudoClasses.Set(":medium", Size == MdExtendedFabSize.Medium);
        PseudoClasses.Set(":large", Size == MdExtendedFabSize.Large);
        PseudoClasses.Set(":primary-container", ColorStyle == MdFabColor.PrimaryContainer);
        PseudoClasses.Set(":secondary-container", ColorStyle == MdFabColor.SecondaryContainer);
        PseudoClasses.Set(":tertiary-container", ColorStyle == MdFabColor.TertiaryContainer);
        PseudoClasses.Set(":primary", ColorStyle == MdFabColor.Primary);
        PseudoClasses.Set(":secondary", ColorStyle == MdFabColor.Secondary);
        PseudoClasses.Set(":tertiary", ColorStyle == MdFabColor.Tertiary);
        PseudoClasses.Set(":has-icon", Icon is not null);
    }
}
