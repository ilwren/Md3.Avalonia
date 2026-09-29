using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Md3.Avalonia.Controls;

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
            1 => BuildThemePage(),
            2 => BuildAboutPage(),
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
        return BuildPage("Theme", new TextBlock
        {
            Text = "Theme resources are shared with desktop. Choose a mode; dynamic color and all control tokens update immediately.",
            TextWrapping = global::Avalonia.Media.TextWrapping.Wrap
        }, new WrapPanel { Children = { light, dark, system } }, new MdCard
        {
            Padding = new Thickness(20),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new TextBlock { Text = "Live surface", FontSize = 20 },
                    new MdTextBox { Label = "Theme-aware field", Text = "Material 3" },
                    new MdLinearProgressIndicator { Value = 68 }
                }
            }
        });
    }

    private static Control BuildAboutPage() => BuildPage("About", new MdCard
    {
        Padding = new Thickness(20),
        Content = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                new TextBlock { Text = "Md3.Avalonia", FontSize = 24 },
                new TextBlock { Text = "Avalonia 12 Material controls", TextWrapping = global::Avalonia.Media.TextWrapping.Wrap },
                new TextBlock { Text = "The Android shell uses the same independent controls, resource dictionaries, localization, and provider-optional icons as desktop.", TextWrapping = global::Avalonia.Media.TextWrapping.Wrap }
            }
        }
    });

    private static MdScrollViewer BuildPage(string title, params Control[] controls)
    {
        var panel = new StackPanel { Margin = new Thickness(20, 18, 20, 32), Spacing = 20 };
        panel.Children.Add(new TextBlock { Text = title, FontSize = 32 });
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
