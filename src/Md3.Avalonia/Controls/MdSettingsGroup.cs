using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A container that groups related <see cref="MdSettingsCard"/> items in an Android Material 3 styled card container.
/// </summary>
[PseudoClasses(":has-header", ":has-description")]
public class MdSettingsGroup : ItemsControl
{
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<MdSettingsGroup, object?>(nameof(Header));

    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<MdSettingsGroup, object?>(nameof(Description));

    static MdSettingsGroup()
    {
        HeaderProperty.Changed.AddClassHandler<MdSettingsGroup>((group, _) => group.UpdatePseudoClasses());
        DescriptionProperty.Changed.AddClassHandler<MdSettingsGroup>((group, _) => group.UpdatePseudoClasses());
    }

    public MdSettingsGroup()
    {
        UpdatePseudoClasses();
    }

    public object? Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
    public object? Description { get => GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":has-header", Header is not null);
        PseudoClasses.Set(":has-description", Description is not null);
    }
}
