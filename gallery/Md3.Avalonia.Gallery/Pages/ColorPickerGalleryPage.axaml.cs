using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
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

        var hex = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        HexCodeText.Text = hex;
        ColorBanner.Background = new SolidColorBrush(color);

        // Update Sample Filled Button
        if (SampleFilledBtnBorder is not null)
        {
            SampleFilledBtnBorder.Background = new SolidColorBrush(color);
            // Lightness check for contrasting text
            var lum = 0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B;
            SampleFilledBtnText.Foreground = lum > 140 ? Brushes.Black : Brushes.White;
        }

        // Update Sample Container
        if (SampleContainerBorder is not null)
        {
            var containerColor = Color.FromArgb(color.A, (byte)Math.Min(255, color.R + 80), (byte)Math.Min(255, color.G + 80), (byte)Math.Min(255, color.B + 80));
            SampleContainerBorder.Background = new SolidColorBrush(containerColor);
            SampleContainerText.Foreground = new SolidColorBrush(color);
        }

        // Update Sample Outlined
        if (SampleOutlinedBorder is not null)
        {
            SampleOutlinedBorder.BorderBrush = new SolidColorBrush(color);
            SampleOutlinedText.Foreground = new SolidColorBrush(color);
        }

        if (StatusToast is not null)
        {
            StatusToast.Text = $"Active color: {hex} (R:{color.R}, G:{color.G}, B:{color.B}, A:{color.A})";
        }
    }

    private async void OnCopyHexClicked(object? sender, RoutedEventArgs e)
    {
        var hex = HexCodeText.Text;
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
            var options = MdThemeManager.Current with
            {
                SeedColor = _currentColor,
                CustomColorOverride = _currentColor
            };
            var isDark = app.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;
            MdThemeManager.Apply(app, options, isDark);

            if (StatusToast is not null)
            {
                StatusToast.Text = $"✓ Global Material 3 theme updated with seed: {HexCodeText.Text}!";
            }
        }
    }
}
