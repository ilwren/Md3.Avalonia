using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Android;

public partial class MainView : UserControl
{
    private object? _componentsPage;

    public MainView()
    {
        InitializeComponent();
        _componentsPage = MobilePageHost.Content;
    }

    private void MobileNavigationChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (MobilePageHost is null || MobileNavigation is null) return;
        MobilePageHost.Content = MobileNavigation.SelectedIndex switch
        {
            1 => BuildColorPickerPage(),
            2 => BuildMotionPage(),
            3 => BuildThemePage(),
            _ => _componentsPage
        };
    }

    private static Control BuildColorPickerPage()
    {
        var picker = new MdColorPicker { IsAlphaEnabled = true, Margin = new Thickness(0, 8, 0, 0) };
        return BuildPage("Color Picker",
            new TextBlock { Text = "Flutter-Style Material 3 Color Picker with swatches, HSV sliders, and HEX input.", TextWrapping = global::Avalonia.Media.TextWrapping.Wrap },
            picker);
    }

    private static Control BuildMotionPage()
    {
        var morph = new MdContainerTransform { ClosedCornerRadius = new CornerRadius(16), OpenCornerRadius = new CornerRadius(24) };
        var closedBorder = new Border { Padding = new Thickness(16), Background = (global::Avalonia.Media.IBrush)Application.Current!.FindResource("Md.Sys.Color.SurfaceContainerHigh.Brush")!, CornerRadius = new CornerRadius(16), Content = new TextBlock { Text = "Tap to Morph into full article", FontWeight = global::Avalonia.Media.FontWeight.Medium } };
        var openBorder = new Border { Padding = new Thickness(20), Background = (global::Avalonia.Media.IBrush)Application.Current!.FindResource("Md.Sys.Color.SurfaceContainerLow.Brush")!, CornerRadius = new CornerRadius(24), Content = new TextBlock { Text = "Expanded Container View", FontSize = 20, FontWeight = global::Avalonia.Media.FontWeight.Bold } };
        closedBorder.PointerPressed += (_, _) => morph.IsExpanded = true;
        openBorder.PointerPressed += (_, _) => morph.IsExpanded = false;
        morph.ClosedContent = closedBorder;
        morph.OpenContent = openBorder;

        return BuildPage("Motion",
            new TextBlock { Text = "Material Design 3 Container Transform morphing transition:", TextWrapping = global::Avalonia.Media.TextWrapping.Wrap },
            morph);
    }

    private static Control BuildThemePage()
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
