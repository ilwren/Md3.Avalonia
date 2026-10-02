using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Md3.Avalonia.Themes.Dynamic;

namespace Md3.Avalonia.Gallery.Pages;

public partial class ColorPickerGalleryPage : UserControl
{
    private Color _currentColor = Color.Parse("#6750A4");

    public ColorPickerGalleryPage()
    {
        InitializeComponent();
        UpdatePreview(_currentColor);
    }

    private void OnColorChanged(object? sender, Color color)
    {
        _currentColor = color;
        UpdatePreview(color);
    }

    private void UpdatePreview(Color color)
    {
        if (HexCodeText is null || ColorBanner is null) return;

        var hex = color.A < byte.MaxValue
            ? $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}"
            : $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        var seedHex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        var isDark = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
        var roles = MdThemeGenerator.Generate(
            MdThemeManager.Current with { SeedColor = seedHex }, isDark);

        HexCodeText.Text = hex;
        ColorBanner.Background = new SolidColorBrush(color);
        var bannerContent = new SolidColorBrush(ContrastingColor(color));
        HexCodeText.Foreground = bannerContent;
        ColorBannerLabel.Foreground = bannerContent;
        CopyHexButton.Foreground = bannerContent;

        var primary = new SolidColorBrush(roles["Primary"]);
        var onPrimary = new SolidColorBrush(roles["OnPrimary"]);
        SampleFilledBtnBorder.Background = primary;
        SampleFilledBtnText.Foreground = onPrimary;
        SampleFilledLabel.Foreground = onPrimary;

        var primaryContainer = new SolidColorBrush(roles["PrimaryContainer"]);
        var onPrimaryContainer = new SolidColorBrush(roles["OnPrimaryContainer"]);
        SampleContainerBorder.Background = primaryContainer;
        SampleContainerText.Foreground = onPrimaryContainer;
        SampleContainerLabel.Foreground = onPrimaryContainer;

        SampleOutlinedBorder.BorderBrush = primary;
        SampleOutlinedText.Foreground = primary;
        SampleOutlinedLabel.Foreground = primary;

        if (StatusToast is not null)
        {
            StatusToast.Text = $"Source {hex} generated Material primary and container roles for the {(isDark ? "dark" : "light")} appearance.";
        }
    }

    private static Color ContrastingColor(Color color)
    {
        static double Linearize(byte channel)
        {
            var value = channel / 255d;
            return value <= 0.04045
                ? value / 12.92
                : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        var luminance = 0.2126 * Linearize(color.R) +
                        0.7152 * Linearize(color.G) +
                        0.0722 * Linearize(color.B);
        var blackContrast = (luminance + 0.05) / 0.05;
        var whiteContrast = 1.05 / (luminance + 0.05);
        return blackContrast >= whiteContrast ? Colors.Black : Colors.White;
    }

    private async void OnCopyHexClicked(object? sender, RoutedEventArgs e)
    {
        var hex = HexCodeText?.Text;
        if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard && !string.IsNullOrEmpty(hex))
        {
            await clipboard.SetTextAsync(hex);
            if (StatusToast is not null)
            {
                StatusToast.Text = $"✓ Copied {hex} to clipboard!";
            }
        }
    }

    private void OnApplyThemeClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is { } app)
        {
            var hex = HexCodeText?.Text ??
                      $"#{_currentColor.A:X2}{_currentColor.R:X2}{_currentColor.G:X2}{_currentColor.B:X2}";
            var options = MdThemeManager.Current with
            {
                SeedColor = hex
            };
            var isDark = app.ActualThemeVariant == ThemeVariant.Dark;
            MdThemeManager.Apply(app, options, isDark);

            if (StatusToast is not null)
            {
                StatusToast.Text = $"✓ Global Material 3 theme updated with seed: {hex}!";
            }
        }
    }
}
