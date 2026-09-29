using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class TabGalleryPage : UserControl
{
    public TabGalleryPage() => InitializeComponent();

    private void StressTabSwitch(object? sender, RoutedEventArgs e)
    {
        for (var index = 0; index < 100; index++) DemoTabView.SelectedIndex = index % 2;
        DemoTabView.SelectedIndex = 1;
        TabStressStatus.Text = "Completed 100 changes · Activity is selected · UI remains responsive.";
    }

    private void PrimarySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (PrimaryContent is null || PrimaryTabs.SelectedItem is not ListBoxItem item) return;
        PrimaryContent.Text = $"{item.Content} content";
    }
}
