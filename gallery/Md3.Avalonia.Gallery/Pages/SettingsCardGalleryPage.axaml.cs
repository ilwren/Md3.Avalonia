using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;

using Md3.Avalonia.Controls;
using Md3.Avalonia.Themes.Dynamic;

namespace Md3.Avalonia.Gallery.Pages;

public partial class SettingsCardGalleryPage : UserControl
{
    public SettingsCardGalleryPage()
    {
        InitializeComponent();
        if (DarkThemeSwitch is not null && Application.Current is { } application)
        {
            DarkThemeSwitch.IsChecked = application.ActualThemeVariant == ThemeVariant.Dark;
        }
    }

    private void DarkThemeSwitchChanged(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is MainWindow window && DarkThemeSwitch is not null)
        {
            window.ApplyThemeMode(DarkThemeSwitch.IsChecked == true ? MdThemeMode.Dark : MdThemeMode.Light);
        }
    }

    private void CardClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is MdSettingsCard card && CardStatus is not null)
        {
            CardStatus.Text = $"Selected setting: {card.Header} ({card.Description})";
        }
    }
}
