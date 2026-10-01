using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Extra.Controls;

public enum MdColorPickerMode
{
    MaterialPalette,
    SpectrumSliders,
    Presets
}

/// <summary>
/// A Flutter / Material Design 3 inspired color picker featuring Material 3 tonal swatches,
/// HSV sliders, live HEX input/binding, and Alpha channel support.
/// </summary>
[PseudoClasses(":palette", ":spectrum", ":presets")]
public class MdColorPicker : TemplatedControl
{
    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<MdColorPicker, Color>(nameof(SelectedColor), Color.FromRgb(0x67, 0x50, 0xA4), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string> SelectedHexProperty =
        AvaloniaProperty.Register<MdColorPicker, string>(nameof(SelectedHex), "#6750A4", defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsAlphaEnabledProperty =
        AvaloniaProperty.Register<MdColorPicker, bool>(nameof(IsAlphaEnabled), true);

    public static readonly StyledProperty<MdColorPickerMode> PickerModeProperty =
        AvaloniaProperty.Register<MdColorPicker, MdColorPickerMode>(nameof(PickerMode), MdColorPickerMode.MaterialPalette);

    public static readonly StyledProperty<double> HueProperty =
        AvaloniaProperty.Register<MdColorPicker, double>(nameof(Hue), 260.0, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> SaturationProperty =
        AvaloniaProperty.Register<MdColorPicker, double>(nameof(Saturation), 50.0, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> ColorValueProperty =
        AvaloniaProperty.Register<MdColorPicker, double>(nameof(ColorValue), 65.0, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> AlphaProperty =
        AvaloniaProperty.Register<MdColorPicker, double>(nameof(Alpha), 100.0, defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<MdColorPicker, IReadOnlyList<Color>> MaterialPrimaryColorsProperty =
        AvaloniaProperty.RegisterDirect<MdColorPicker, IReadOnlyList<Color>>(nameof(MaterialPrimaryColors), picker => picker.MaterialPrimaryColors);

    public static readonly DirectProperty<MdColorPicker, IReadOnlyList<Color>> MaterialShadesProperty =
        AvaloniaProperty.RegisterDirect<MdColorPicker, IReadOnlyList<Color>>(nameof(MaterialShades), picker => picker.MaterialShades);

    private static readonly Color[] DefaultMaterialPrimaryColors =
    [
        Color.Parse("#F44336"), // Red
        Color.Parse("#E91E63"), // Pink
        Color.Parse("#9C27B0"), // Purple
        Color.Parse("#673AB7"), // Deep Purple
        Color.Parse("#3F51B5"), // Indigo
        Color.Parse("#2196F3"), // Blue
        Color.Parse("#03A9F4"), // Light Blue
        Color.Parse("#00BCD4"), // Cyan
        Color.Parse("#009688"), // Teal
        Color.Parse("#4CAF50"), // Green
        Color.Parse("#8BC34A"), // Light Green
        Color.Parse("#CDDC39"), // Lime
        Color.Parse("#FFEB3B"), // Yellow
        Color.Parse("#FFC107"), // Amber
        Color.Parse("#FF9800"), // Orange
        Color.Parse("#FF5722"), // Deep Orange
        Color.Parse("#795548"), // Brown
        Color.Parse("#9E9E9E"), // Grey
        Color.Parse("#607D8B"), // Blue Grey
        Color.Parse("#6750A4")  // M3 Seed Purple
    ];

    private bool _updatingInternals;
    private IReadOnlyList<Color> _materialShades = Array.Empty<Color>();
    private TextBox? _hexTextBox;
    private Button? _copyHexButton;

    public ObservableCollection<Color> RecentColors { get; } = new()
    {
        Color.Parse("#6750A4"),
        Color.Parse("#006A6A"),
        Color.Parse("#984061"),
        Color.Parse("#2196F3"),
        Color.Parse("#4CAF50"),
        Color.Parse("#FF9800"),
        Color.Parse("#E91E63"),
        Color.Parse("#1C1B1F")
    };

    static MdColorPicker()
    {
        SelectedColorProperty.Changed.AddClassHandler<MdColorPicker>((picker, change) => picker.OnSelectedColorChanged(change));
        HueProperty.Changed.AddClassHandler<MdColorPicker>((picker, _) => picker.OnHsvChanged());
        SaturationProperty.Changed.AddClassHandler<MdColorPicker>((picker, _) => picker.OnHsvChanged());
        ColorValueProperty.Changed.AddClassHandler<MdColorPicker>((picker, _) => picker.OnHsvChanged());
        AlphaProperty.Changed.AddClassHandler<MdColorPicker>((picker, _) => picker.OnHsvChanged());
        PickerModeProperty.Changed.AddClassHandler<MdColorPicker>((picker, _) => picker.UpdateModePseudoClasses());
    }

    public MdColorPicker()
    {
        UpdateModePseudoClasses();
        UpdateShades(SelectedColor);
        SyncHsvFromColor(SelectedColor);
    }

    public Color SelectedColor
    {
        get => GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public string SelectedHex
    {
        get => GetValue(SelectedHexProperty);
        set => SetValue(SelectedHexProperty, value);
    }

    public bool IsAlphaEnabled
    {
        get => GetValue(IsAlphaEnabledProperty);
        set => SetValue(IsAlphaEnabledProperty, value);
    }

    public MdColorPickerMode PickerMode
    {
        get => GetValue(PickerModeProperty);
        set => SetValue(PickerModeProperty, value);
    }

    public double Hue
    {
        get => GetValue(HueProperty);
        set => SetValue(HueProperty, value);
    }

    public double Saturation
    {
        get => GetValue(SaturationProperty);
        set => SetValue(SaturationProperty, value);
    }

    public double ColorValue
    {
        get => GetValue(ColorValueProperty);
        set => SetValue(ColorValueProperty, value);
    }

    public double Alpha
    {
        get => GetValue(AlphaProperty);
        set => SetValue(AlphaProperty, value);
    }

    public IReadOnlyList<Color> MaterialPrimaryColors => DefaultMaterialPrimaryColors;

    public IReadOnlyList<Color> MaterialShades
    {
        get => _materialShades;
        private set => SetAndRaise(MaterialShadesProperty, ref _materialShades, value);
    }

    public event EventHandler<Color>? ColorChanged;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_copyHexButton is not null) _copyHexButton.Click -= OnCopyHexClicked;
        if (_hexTextBox is not null) _hexTextBox.KeyDown -= OnHexKeyDown;

        base.OnApplyTemplate(e);

        _hexTextBox = e.NameScope.Find<TextBox>("PART_HexTextBox");
        _copyHexButton = e.NameScope.Find<Button>("PART_CopyHexButton");

        if (_copyHexButton is not null) _copyHexButton.Click += OnCopyHexClicked;
        if (_hexTextBox is not null) _hexTextBox.KeyDown += OnHexKeyDown;
    }

    private void OnCopyHexClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
            {
                var data = new DataObject();
                data.Set(DataFormats.Text, SelectedHex);
                _ = clipboard.SetDataObjectAsync(data);
            }
        }
        catch { }
    }

    private void OnHexKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && _hexTextBox is not null)
        {
            TryApplyHex(_hexTextBox.Text);
            e.Handled = true;
        }
    }

    public bool TryApplyHex(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return false;
        hex = hex.Trim();
        if (!hex.StartsWith("#")) hex = "#" + hex;
        if (Color.TryParse(hex, out var color))
        {
            SelectedColor = color;
            return true;
        }
        return false;
    }

    public void SelectColor(Color color)
    {
        SelectedColor = color;
        if (!RecentColors.Contains(color))
        {
            RecentColors.Insert(0, color);
            if (RecentColors.Count > 16) RecentColors.RemoveAt(RecentColors.Count - 1);
        }
    }

    private void OnSelectedColorChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (_updatingInternals) return;
        _updatingInternals = true;
        try
        {
            var color = (Color)change.NewValue!;
            var hex = IsAlphaEnabled && color.A < 255
                ? $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}"
                : $"#{color.R:X2}{color.G:X2}{color.B:X2}";

            SetCurrentValue(SelectedHexProperty, hex);
            UpdateShades(color);
            SyncHsvFromColor(color);
            ColorChanged?.Invoke(this, color);
        }
        finally
        {
            _updatingInternals = false;
        }
    }

    private void OnHsvChanged()
    {
        if (_updatingInternals) return;
        _updatingInternals = true;
        try
        {
            var h = Math.Clamp(Hue, 0, 360);
            var s = Math.Clamp(Saturation / 100.0, 0, 1.0);
            var v = Math.Clamp(ColorValue / 100.0, 0, 1.0);
            var a = IsAlphaEnabled ? Math.Clamp(Alpha / 100.0, 0, 1.0) : 1.0;

            var hsv = new HsvColor(a, h, s, v);
            var color = hsv.ToRgb();

            SetCurrentValue(SelectedColorProperty, color);
            var hex = IsAlphaEnabled && color.A < 255
                ? $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}"
                : $"#{color.R:X2}{color.G:X2}{color.B:X2}";

            SetCurrentValue(SelectedHexProperty, hex);
            UpdateShades(color);
            ColorChanged?.Invoke(this, color);
        }
        finally
        {
            _updatingInternals = false;
        }
    }

    private void SyncHsvFromColor(Color color)
    {
        var hsv = color.ToHsv();
        SetCurrentValue(HueProperty, hsv.H);
        SetCurrentValue(SaturationProperty, hsv.S * 100.0);
        SetCurrentValue(ColorValueProperty, hsv.V * 100.0);
        SetCurrentValue(AlphaProperty, (color.A / 255.0) * 100.0);
    }

    private void UpdateShades(Color baseColor)
    {
        var hsv = baseColor.ToHsv();
        var shades = new List<Color>(10);
        var lightnessFactors = new[] { 0.95, 0.85, 0.75, 0.65, 0.55, 0.45, 0.35, 0.25, 0.15, 0.08 };

        foreach (var l in lightnessFactors)
        {
            var shadeHsv = new HsvColor(1.0, hsv.H, Math.Clamp(hsv.S * 0.9, 0.1, 1.0), l);
            shades.Add(shadeHsv.ToRgb());
        }

        MaterialShades = shades;
    }

    private void UpdateModePseudoClasses()
    {
        PseudoClasses.Set(":palette", PickerMode == MdColorPickerMode.MaterialPalette);
        PseudoClasses.Set(":spectrum", PickerMode == MdColorPickerMode.SpectrumSliders);
        PseudoClasses.Set(":presets", PickerMode == MdColorPickerMode.Presets);
    }
}

/// <summary>
/// A compact Material 3 button that displays the current color swatch and opens a popover color picker.
/// </summary>
public sealed class MdColorPickerButton : TemplatedControl
{
    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<MdColorPickerButton, Color>(nameof(SelectedColor), Color.FromRgb(0x67, 0x50, 0xA4), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string> SelectedHexProperty =
        AvaloniaProperty.Register<MdColorPickerButton, string>(nameof(SelectedHex), "#6750A4", defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<MdColorPickerButton, bool>(nameof(IsDropDownOpen), false, defaultBindingMode: BindingMode.TwoWay);

    public Color SelectedColor
    {
        get => GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public string SelectedHex
    {
        get => GetValue(SelectedHexProperty);
        set => SetValue(SelectedHexProperty, value);
    }

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
}

public static class MdColorConverters
{
    public static readonly global::Avalonia.Data.Converters.IValueConverter ColorToBrush =
        new global::Avalonia.Data.Converters.FuncValueConverter<Color, IBrush>(c => new SolidColorBrush(c));
}
