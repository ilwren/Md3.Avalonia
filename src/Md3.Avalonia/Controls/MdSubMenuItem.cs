using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;

namespace Md3.Avalonia.Controls;

/// <summary>A Material menu item that opens a cascading submenu.</summary>
[PseudoClasses(":submenu-open")]
public sealed class MdSubMenuItem : MdMenuItem
{
    public static readonly StyledProperty<object?> SubmenuProperty = AvaloniaProperty.Register<MdSubMenuItem, object?>(nameof(Submenu));
    public static readonly StyledProperty<bool> IsSubmenuOpenProperty = AvaloniaProperty.Register<MdSubMenuItem, bool>(nameof(IsSubmenuOpen), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    static MdSubMenuItem() => IsSubmenuOpenProperty.Changed.AddClassHandler<MdSubMenuItem>((item, _) => item.PseudoClasses.Set(":submenu-open", item.IsSubmenuOpen));

    public MdSubMenuItem()
    {
        Click += (_, _) => SetCurrentValue(IsSubmenuOpenProperty, !IsSubmenuOpen);
        PointerEntered += (_, _) => SetCurrentValue(IsSubmenuOpenProperty, true);
    }

    public object? Submenu { get => GetValue(SubmenuProperty); set => SetValue(SubmenuProperty, value); }
    public bool IsSubmenuOpen { get => GetValue(IsSubmenuOpenProperty); set => SetValue(IsSubmenuOpenProperty, value); }
}
