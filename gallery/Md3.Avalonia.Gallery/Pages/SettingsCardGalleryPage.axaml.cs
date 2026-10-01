using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class SettingsCardGalleryPage : UserControl
{
    public SettingsCardGalleryPage()
    {
        InitializeComponent();
    }

    private void CardClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is MdSettingsCard card && CardStatus is not null)
        {
            CardStatus.Text = $"Selected setting: {card.Header} ({card.Description})";
        }
    }
}
