using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Media;

namespace Md3.Avalonia.Controls;

/// <summary>A Material 3 floating action button for the screen's primary action.</summary>
[PseudoClasses(":small", ":regular", ":medium", ":large", ":primary-container", ":secondary-container", ":tertiary-container", ":primary", ":secondary", ":tertiary")]
public class MdFloatingActionButton : Button
{
    public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<MdFloatingActionButton, object?>(nameof(Icon));
    public static readonly StyledProperty<MdFabSize> SizeProperty = AvaloniaProperty.Register<MdFloatingActionButton, MdFabSize>(nameof(Size), MdFabSize.Regular);
    public static readonly StyledProperty<MdFabColor> ColorStyleProperty = AvaloniaProperty.Register<MdFloatingActionButton, MdFabColor>(nameof(ColorStyle), MdFabColor.PrimaryContainer);
    public static readonly StyledProperty<double> ContainerSizeProperty = AvaloniaProperty.Register<MdFloatingActionButton, double>(nameof(ContainerSize), 56);
    public static readonly StyledProperty<double> IconSizeProperty = AvaloniaProperty.Register<MdFloatingActionButton, double>(nameof(IconSize), 24);
    public static readonly StyledProperty<CornerRadius> ContainerCornerRadiusProperty = AvaloniaProperty.Register<MdFloatingActionButton, CornerRadius>(nameof(ContainerCornerRadius), new CornerRadius(16));
    public static readonly StyledProperty<IBrush?> StateLayerBrushProperty = AvaloniaProperty.Register<MdFloatingActionButton, IBrush?>(nameof(StateLayerBrush));
    public static readonly StyledProperty<BoxShadows> ElevationProperty = AvaloniaProperty.Register<MdFloatingActionButton, BoxShadows>(nameof(Elevation));

    static MdFloatingActionButton()
    {
        SizeProperty.Changed.AddClassHandler<MdFloatingActionButton>((x, _) => x.UpdatePseudoClasses());
        ColorStyleProperty.Changed.AddClassHandler<MdFloatingActionButton>((x, _) => x.UpdatePseudoClasses());
    }
    public MdFloatingActionButton() => UpdatePseudoClasses();

    public object? Icon { get => GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public MdFabSize Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public MdFabColor ColorStyle { get => GetValue(ColorStyleProperty); set => SetValue(ColorStyleProperty, value); }
    public double ContainerSize { get => GetValue(ContainerSizeProperty); set => SetValue(ContainerSizeProperty, value); }
    public double IconSize { get => GetValue(IconSizeProperty); set => SetValue(IconSizeProperty, value); }
    public CornerRadius ContainerCornerRadius { get => GetValue(ContainerCornerRadiusProperty); set => SetValue(ContainerCornerRadiusProperty, value); }
    public IBrush? StateLayerBrush { get => GetValue(StateLayerBrushProperty); set => SetValue(StateLayerBrushProperty, value); }
    public BoxShadows Elevation { get => GetValue(ElevationProperty); set => SetValue(ElevationProperty, value); }

    protected void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":small", Size == MdFabSize.Small);
        PseudoClasses.Set(":regular", Size == MdFabSize.Regular);
        PseudoClasses.Set(":medium", Size == MdFabSize.Medium);
        PseudoClasses.Set(":large", Size == MdFabSize.Large);
        PseudoClasses.Set(":primary-container", ColorStyle == MdFabColor.PrimaryContainer);
        PseudoClasses.Set(":secondary-container", ColorStyle == MdFabColor.SecondaryContainer);
        PseudoClasses.Set(":tertiary-container", ColorStyle == MdFabColor.TertiaryContainer);
        PseudoClasses.Set(":primary", ColorStyle == MdFabColor.Primary);
        PseudoClasses.Set(":secondary", ColorStyle == MdFabColor.Secondary);
        PseudoClasses.Set(":tertiary", ColorStyle == MdFabColor.Tertiary);
    }
}
