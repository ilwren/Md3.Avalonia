using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Themes.Dynamic;

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
        SelectColorCommand = new DelegateCommand(parameter =>
        {
            if (parameter is Color color) SelectColor(color);
        });
        SetPickerModeCommand = new DelegateCommand(parameter =>
        {
            if (parameter is MdColorPickerMode mode)
                SetCurrentValue(PickerModeProperty, mode);
            else if (parameter is string text && Enum.TryParse<MdColorPickerMode>(text, out var parsed))
                SetCurrentValue(PickerModeProperty, parsed);
        });

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

    /// <summary>Curated source-color presets used to generate Material 3 HCT tonal palettes.</summary>
    public IReadOnlyList<Color> MaterialPrimaryColors => DefaultMaterialPrimaryColors;

    public ICommand SelectColorCommand { get; }

    public ICommand SetPickerModeCommand { get; }

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
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.Clipboard is { } clipboard)
            {
                _ = clipboard.SetTextAsync(SelectedHex);
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
            SelectColor(color);
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
        // Material 3 tonal palettes are generated in HCT, not by scaling HSV value. Reusing the
        // same TonalSpot pipeline as dynamic ColorScheme generation keeps picker output aligned
        // with the color roles that will be produced when the selected color becomes a theme seed.
        MaterialShades = MdThemeGenerator.GeneratePrimaryTonalPalette(baseColor);
    }

    private void UpdateModePseudoClasses()
    {
        PseudoClasses.Set(":palette", PickerMode == MdColorPickerMode.MaterialPalette);
        PseudoClasses.Set(":spectrum", PickerMode == MdColorPickerMode.SpectrumSliders);
        PseudoClasses.Set(":presets", PickerMode == MdColorPickerMode.Presets);
    }

    private sealed class DelegateCommand(Action<object?> execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute(parameter);
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

    private Button? _dropDownButton;
    private MdColorPicker? _picker;

    static MdColorPickerButton()
    {
        SelectedColorProperty.Changed.AddClassHandler<MdColorPickerButton>((button, change) =>
        {
            var color = (Color)change.NewValue!;
            button.SetCurrentValue(SelectedHexProperty, color.A < byte.MaxValue
                ? $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}"
                : $"#{color.R:X2}{color.G:X2}{color.B:X2}");
        });
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

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_dropDownButton is not null) _dropDownButton.Click -= OnDropDownButtonClicked;
        if (_picker is not null) _picker.ColorChanged -= OnPickerColorChanged;

        base.OnApplyTemplate(e);

        _dropDownButton = e.NameScope.Find<Button>("PART_DropDownButton");
        _picker = e.NameScope.Find<MdColorPicker>("PART_Picker");

        if (_dropDownButton is not null) _dropDownButton.Click += OnDropDownButtonClicked;
        if (_picker is not null) _picker.ColorChanged += OnPickerColorChanged;
    }

    private void OnDropDownButtonClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
    }

    private void OnPickerColorChanged(object? sender, Color color)
    {
        SetCurrentValue(SelectedColorProperty, color);
    }
}

public static class MdColorConverters
{
    public static readonly IValueConverter ColorToBrush =
        new FuncValueConverter<Color, IBrush>(color => new SolidColorBrush(color));

    public static readonly IValueConverter ContrastBrush =
        new FuncValueConverter<Color, IBrush>(color =>
        {
            var luminance = RelativeLuminance(color);
            var blackContrast = (luminance + 0.05) / 0.05;
            var whiteContrast = 1.05 / (luminance + 0.05);
            return blackContrast >= whiteContrast ? Brushes.Black : Brushes.White;
        });

    public static readonly IMultiValueConverter ColorsEqual = new ColorEqualityConverter();

    private static double RelativeLuminance(Color color)
    {
        static double Linearize(byte channel)
        {
            var value = channel / 255.0;
            return value <= 0.04045
                ? value / 12.92
                : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Linearize(color.R) +
               0.7152 * Linearize(color.G) +
               0.0722 * Linearize(color.B);
    }

    private sealed class ColorEqualityConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            return values.Count >= 2 &&
                   values[0] is Color candidate &&
                   values[1] is Color selected &&
                   candidate == selected;
        }
    }
}
