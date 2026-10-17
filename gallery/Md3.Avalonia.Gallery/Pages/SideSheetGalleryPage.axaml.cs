using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class SideSheetGalleryPage : UserControl
{
    public SideSheetGalleryPage()
    {
        InitializeComponent();

        UpdateToggleLabel();
        ExternalSheet.IsOpenChanged += (_, _) => UpdateToggleLabel();
    }

    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    private void ToggleExternalClick(object? sender, RoutedEventArgs e) => ExternalSheet.Toggle();

    private void UpdateToggleLabel() =>
        ToggleExternal.Content = ExternalSheet.IsOpen
            ? L("Close the sheet", "收起侧栏")
            : L("Open the sheet", "展开侧栏");
}
