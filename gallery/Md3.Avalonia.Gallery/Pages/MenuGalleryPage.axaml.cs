using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class MenuGalleryPage : UserControl
{
    public MenuGalleryPage()
    {
        InitializeComponent();
        AddHandler(Button.ClickEvent, OnMenuAction, RoutingStrategies.Bubble);
    }

    private void ToggleMenu(object? sender, RoutedEventArgs e) => DemoMenu.IsOpen = !DemoMenu.IsOpen;

    private void OpenContextMenu(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(ContextMenuTarget).Properties.PointerUpdateKind != PointerUpdateKind.RightButtonPressed) return;
        ContextDemoMenu.IsOpen = true;
        e.Handled = true;
    }

    private void OnMenuAction(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not MdMenuItem item) return;
        MenuResult.Text = $"{item.Content} selected.";
        DemoMenu.IsOpen = false;
        ContextDemoMenu.IsOpen = false;
    }
}
