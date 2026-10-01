using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class AndroidSettingsSamplePage : UserControl
{
    public AndroidSettingsSamplePage()
    {
        InitializeComponent();
    }

    private void OnSettingClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is MdSettingsCard card && SettingNotification is not null)
        {
            SettingNotification.Text = $"Selected: {card.Header}";
        }
    }

    private void OnSettingToggle(object? sender, RoutedEventArgs e)
    {
        if (sender is MdSwitch sw && SettingNotification is not null)
        {
            SettingNotification.Text = $"Setting toggled: {(sw.IsChecked == true ? "ON" : "OFF")}";
        }
    }
}
