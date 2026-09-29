using Avalonia;
using Avalonia.Controls;

namespace Md3.Avalonia.Controls;

/// <summary>Material page structure for app bars, adaptive navigation, body, FAB and transient feedback.</summary>
public sealed class MdScaffold : ContentControl
{
    public static readonly StyledProperty<object?> TopBarProperty = AvaloniaProperty.Register<MdScaffold, object?>(nameof(TopBar));
    public static readonly StyledProperty<object?> NavigationProperty = AvaloniaProperty.Register<MdScaffold, object?>(nameof(Navigation));
    public static readonly StyledProperty<object?> BottomBarProperty = AvaloniaProperty.Register<MdScaffold, object?>(nameof(BottomBar));
    public static readonly StyledProperty<object?> FloatingActionButtonProperty = AvaloniaProperty.Register<MdScaffold, object?>(nameof(FloatingActionButton));
    public static readonly StyledProperty<object?> SnackbarProperty = AvaloniaProperty.Register<MdScaffold, object?>(nameof(Snackbar));
    public static readonly StyledProperty<object?> OverlayProperty = AvaloniaProperty.Register<MdScaffold, object?>(nameof(Overlay));

    public object? TopBar { get => GetValue(TopBarProperty); set => SetValue(TopBarProperty, value); }
    public object? Navigation { get => GetValue(NavigationProperty); set => SetValue(NavigationProperty, value); }
    public object? BottomBar { get => GetValue(BottomBarProperty); set => SetValue(BottomBarProperty, value); }
    public object? FloatingActionButton { get => GetValue(FloatingActionButtonProperty); set => SetValue(FloatingActionButtonProperty, value); }
    public object? Snackbar { get => GetValue(SnackbarProperty); set => SetValue(SnackbarProperty, value); }
    public object? Overlay { get => GetValue(OverlayProperty); set => SetValue(OverlayProperty, value); }
}
