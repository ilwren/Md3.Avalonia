using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery.Pages;

namespace Md3.Avalonia.Gallery;

/// <summary>Compact Android-safe Gallery shell whose bottom destinations change real content.</summary>
public partial class AndroidGalleryView : UserControl
{
    private object? _componentsPage;

    public AndroidGalleryView()
    {
        InitializeComponent();
        _componentsPage = MobilePageHost.Content;
    }

    private void MobileNavigationChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (MobilePageHost is null || MobileNavigation is null) return;
        MobilePageHost.Content = MobileNavigation.SelectedIndex switch
        {
            1 => new ColorPickerGalleryPage(),
            2 => new MotionGalleryPage(),
            3 => BuildThemePage(),
            _ => _componentsPage
        };
    }

    private Control BuildThemePage()
    {
        var light = new MdButton { Content = "Light", Variant = MdButtonVariant.Tonal };
        var dark = new MdButton { Content = "Dark", Variant = MdButtonVariant.Tonal };
        var system = new MdButton { Content = "System", Variant = MdButtonVariant.Outlined };
        light.Click += (_, _) => ApplyTheme(ThemeVariant.Light);
        dark.Click += (_, _) => ApplyTheme(ThemeVariant.Dark);
        system.Click += (_, _) => ApplyTheme(ThemeVariant.Default);
        return BuildPage("Theme & Info", new TextBlock
        {
            Text = "Material 3 design tokens and theme resources are shared between desktop and mobile. Switch theme mode below:",
            TextWrapping = global::Avalonia.Media.TextWrapping.Wrap
        }, new WrapPanel { Children = { light, dark, system } }, new MdCard
        {
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new TextBlock { Text = "Md3.Avalonia v0.2.0", FontSize = 18, FontWeight = global::Avalonia.Media.FontWeight.SemiBold },
                    new TextBlock { Text = "Comprehensive Material Design 3 and Flutter ecosystem components for Avalonia UI.", TextWrapping = global::Avalonia.Media.TextWrapping.Wrap },
                    new MdLinearProgressIndicator { Value = 100 }
                }
            }
        });
    }

    private static MdScrollViewer BuildPage(string title, params Control[] controls)
    {
        var panel = new StackPanel { Margin = new Thickness(16, 14, 16, 24), Spacing = 16 };
        panel.Children.Add(new TextBlock { Text = title, FontSize = 26, FontWeight = global::Avalonia.Media.FontWeight.SemiBold });
        foreach (var control in controls) panel.Children.Add(control);
        return new MdScrollViewer
        {
            VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = panel
        };
    }

    private static void ApplyTheme(ThemeVariant variant)
    {
        if (Application.Current is { } application) application.RequestedThemeVariant = variant;
    }
}
