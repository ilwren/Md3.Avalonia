using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class SheetGalleryPage : UserControl
{
    public SheetGalleryPage() => InitializeComponent();
    private void ShowBottom(object? sender, RoutedEventArgs e) { DemoSheet.Placement = MdSheetPlacement.Bottom; DemoSheet.SheetExtent = 260; DemoSheet.Show(); }
    private void ShowLeft(object? sender, RoutedEventArgs e) { DemoSheet.Placement = MdSheetPlacement.Left; DemoSheet.SheetExtent = 320; DemoSheet.Show(); }
    private void ShowRight(object? sender, RoutedEventArgs e) { DemoSheet.Placement = MdSheetPlacement.Right; DemoSheet.SheetExtent = 320; DemoSheet.Show(); }
    private void CloseSheet(object? sender, RoutedEventArgs e) => DemoSheet.Dismiss();
}
