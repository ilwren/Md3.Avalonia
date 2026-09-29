using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Media;

namespace Md3.Avalonia.Controls;

/// <summary>A scoped semantic Material surface with color-role, shape and elevation support.</summary>
[PseudoClasses(":surface", ":lowest", ":low", ":container", ":high", ":highest", ":inverse")]
public sealed class MdSurface : ContentControl
{
    public static readonly StyledProperty<MdSurfaceVariant> VariantProperty =
        AvaloniaProperty.Register<MdSurface, MdSurfaceVariant>(nameof(Variant));
    public static readonly StyledProperty<BoxShadows> ElevationProperty =
        AvaloniaProperty.Register<MdSurface, BoxShadows>(nameof(Elevation));

    static MdSurface()
    {
        VariantProperty.Changed.AddClassHandler<MdSurface>((surface, _) => surface.UpdatePseudoClasses());
    }

    public MdSurface() => UpdatePseudoClasses();

    public MdSurfaceVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public BoxShadows Elevation { get => GetValue(ElevationProperty); set => SetValue(ElevationProperty, value); }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":surface", Variant == MdSurfaceVariant.Surface);
        PseudoClasses.Set(":lowest", Variant == MdSurfaceVariant.ContainerLowest);
        PseudoClasses.Set(":low", Variant == MdSurfaceVariant.ContainerLow);
        PseudoClasses.Set(":container", Variant == MdSurfaceVariant.Container);
        PseudoClasses.Set(":high", Variant == MdSurfaceVariant.ContainerHigh);
        PseudoClasses.Set(":highest", Variant == MdSurfaceVariant.ContainerHighest);
        PseudoClasses.Set(":inverse", Variant == MdSurfaceVariant.Inverse);
    }
}
