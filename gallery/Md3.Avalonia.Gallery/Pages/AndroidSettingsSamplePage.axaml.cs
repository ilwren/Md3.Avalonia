using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Themes.Dynamic;

namespace Md3.Avalonia.Gallery.Pages;

public partial class AndroidSettingsSamplePage : UserControl
{
    public AndroidSettingsSamplePage()
    {
        InitializeComponent();
        if (DarkThemeSwitch is not null && Application.Current is { } application)
        {
            DarkThemeSwitch.IsChecked = application.ActualThemeVariant == ThemeVariant.Dark;
        }
    }

    private void DarkThemeSwitchChanged(object? sender, RoutedEventArgs e)
    {
        if (this.GetVisualRoot() is MainWindow window && DarkThemeSwitch is not null)
        {
            window.ApplyThemeMode(DarkThemeSwitch.IsChecked == true ? MdThemeMode.Dark : MdThemeMode.Light);
        }
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
