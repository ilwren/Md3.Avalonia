using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Controls;

/// <summary>
/// An expandable Android Material 3 settings card that reveals nested settings items when expanded.
/// </summary>
[PseudoClasses(":expanded")]
public class MdSettingsExpander : ItemsControl
{
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<MdSettingsExpander, object?>(nameof(Header));

    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<MdSettingsExpander, object?>(nameof(Description));

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<MdSettingsExpander, object?>(nameof(Icon));

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<MdSettingsExpander, bool>(nameof(IsExpanded), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    static MdSettingsExpander()
    {
        IsExpandedProperty.Changed.AddClassHandler<MdSettingsExpander>((expander, _) => expander.UpdatePseudoClasses());
    }

    public MdSettingsExpander()
    {
        UpdatePseudoClasses();
    }

    public object? Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
    public object? Description { get => GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }
    public object? Icon { get => GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public bool IsExpanded { get => GetValue(IsExpandedProperty); set => SetValue(IsExpandedProperty, value); }

    public void Toggle() => IsExpanded = !IsExpanded;

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":expanded", IsExpanded);
    }
}
