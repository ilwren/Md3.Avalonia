using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Localization;
using Md3.Avalonia.Themes.Dynamic;

namespace Md3.Avalonia.Extra.Controls;

public enum MdColorPickerMode
{
    MaterialPalette,
    SpectrumSliders,
    Presets
}

/// <summary>
/// A custom Material-styled color picker composed from Material 3 surfaces, controls and color roles.
/// Material and Flutter do not provide a first-party ColorPicker component. This control supplies HCT
/// tonal swatches, HSV sliders, live HEX input/binding, and alpha-channel support.
/// </summary>
[PseudoClasses(":palette", ":spectrum", ":presets", ":palette-panel-visible", ":spectrum-panel-visible", ":presets-panel-visible", ":mode-selector-visible")]
public class MdColorPicker : TemplatedControl
{
    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<MdColorPicker, Color>(nameof(SelectedColor), Color.FromRgb(0x67, 0x50, 0xA4), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string> SelectedHexProperty =
        AvaloniaProperty.Register<MdColorPicker, string>(nameof(SelectedHex), "#6750A4", defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsAlphaEnabledProperty =
        AvaloniaProperty.Register<MdColorPicker, bool>(nameof(IsAlphaEnabled), true);

    public static readonly StyledProperty<bool> IsPreviewPanelVisibleProperty =
        AvaloniaProperty.Register<MdColorPicker, bool>(nameof(IsPreviewPanelVisible), true);

    public static readonly StyledProperty<bool> IsModeSelectorVisibleProperty =
        AvaloniaProperty.Register<MdColorPicker, bool>(nameof(IsModeSelectorVisible), true);

    public static readonly StyledProperty<bool> IsMaterialPalettePanelVisibleProperty =
        AvaloniaProperty.Register<MdColorPicker, bool>(nameof(IsMaterialPalettePanelVisible), true);

    public static readonly StyledProperty<bool> IsSpectrumPanelVisibleProperty =
        AvaloniaProperty.Register<MdColorPicker, bool>(nameof(IsSpectrumPanelVisible), true);

    public static readonly StyledProperty<bool> IsRecentColorsPanelVisibleProperty =
        AvaloniaProperty.Register<MdColorPicker, bool>(nameof(IsRecentColorsPanelVisible), true);

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

    public static readonly DirectProperty<MdColorPicker, string?> HexValidationMessageProperty =
        AvaloniaProperty.RegisterDirect<MdColorPicker, string?>(nameof(HexValidationMessage), picker => picker.HexValidationMessage);

    public static readonly DirectProperty<MdColorPicker, string?> CopyStatusMessageProperty =
        AvaloniaProperty.RegisterDirect<MdColorPicker, string?>(nameof(CopyStatusMessage), picker => picker.CopyStatusMessage);

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
    private string? _hexValidationMessage;
    private string? _copyStatusMessage;

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
        SelectedHexProperty.Changed.AddClassHandler<MdColorPicker>((picker, _) => picker.OnSelectedHexChanged());
        IsAlphaEnabledProperty.Changed.AddClassHandler<MdColorPicker>((picker, _) => picker.OnAlphaEnabledChanged());
        HueProperty.Changed.AddClassHandler<MdColorPicker>((picker, _) => picker.OnHsvChanged());
        SaturationProperty.Changed.AddClassHandler<MdColorPicker>((picker, _) => picker.OnHsvChanged());
        ColorValueProperty.Changed.AddClassHandler<MdColorPicker>((picker, _) => picker.OnHsvChanged());
        AlphaProperty.Changed.AddClassHandler<MdColorPicker>((picker, _) => picker.OnHsvChanged());
        PickerModeProperty.Changed.AddClassHandler<MdColorPicker>((picker, _) => picker.OnPanelConfigurationChanged());
        IsModeSelectorVisibleProperty.Changed.AddClassHandler<MdColorPicker>((picker, _) => picker.OnPanelConfigurationChanged());
        IsMaterialPalettePanelVisibleProperty.Changed.AddClassHandler<MdColorPicker>((picker, _) => picker.OnPanelConfigurationChanged());
        IsSpectrumPanelVisibleProperty.Changed.AddClassHandler<MdColorPicker>((picker, _) => picker.OnPanelConfigurationChanged());
        IsRecentColorsPanelVisibleProperty.Changed.AddClassHandler<MdColorPicker>((picker, _) => picker.OnPanelConfigurationChanged());
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

    /// <summary>Shows the selected-color preview, HEX editor, and copy action.</summary>
    public bool IsPreviewPanelVisible
    {
        get => GetValue(IsPreviewPanelVisibleProperty);
        set => SetValue(IsPreviewPanelVisibleProperty, value);
    }

    /// <summary>
    /// Shows the mode selector when at least two picker panels are available. Set this to false
    /// when the host exposes a single purpose-specific panel.
    /// </summary>
    public bool IsModeSelectorVisible
    {
        get => GetValue(IsModeSelectorVisibleProperty);
        set => SetValue(IsModeSelectorVisibleProperty, value);
    }

    /// <summary>Allows the Material source-color and generated tonal-palette panel to be displayed.</summary>
    public bool IsMaterialPalettePanelVisible
    {
        get => GetValue(IsMaterialPalettePanelVisibleProperty);
        set => SetValue(IsMaterialPalettePanelVisibleProperty, value);
    }

    /// <summary>Allows the HSV and optional alpha adjustment panel to be displayed.</summary>
    public bool IsSpectrumPanelVisible
    {
        get => GetValue(IsSpectrumPanelVisibleProperty);
        set => SetValue(IsSpectrumPanelVisibleProperty, value);
    }

    /// <summary>Allows the recent-colors panel to be displayed.</summary>
    public bool IsRecentColorsPanelVisible
    {
        get => GetValue(IsRecentColorsPanelVisibleProperty);
        set => SetValue(IsRecentColorsPanelVisibleProperty, value);
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

    /// <summary>HEX parsing feedback. This is intentionally separate from clipboard status.</summary>
    public string? HexValidationMessage
    {
        get => _hexValidationMessage;
        private set => SetAndRaise(HexValidationMessageProperty, ref _hexValidationMessage, value);
    }

    /// <summary>Non-error status for the asynchronous copy action.</summary>
    public string? CopyStatusMessage
    {
        get => _copyStatusMessage;
        private set => SetAndRaise(CopyStatusMessageProperty, ref _copyStatusMessage, value);
    }

    public event EventHandler<Color>? ColorChanged;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_copyHexButton is not null) _copyHexButton.Click -= OnCopyHexClicked;
        if (_hexTextBox is not null)
        {
            _hexTextBox.KeyDown -= OnHexKeyDown;
            _hexTextBox.LostFocus -= OnHexLostFocus;
        }

        base.OnApplyTemplate(e);

        _hexTextBox = e.NameScope.Find<TextBox>("PART_HexTextBox");
        _copyHexButton = e.NameScope.Find<Button>("PART_CopyHexButton");

        if (_copyHexButton is not null) _copyHexButton.Click += OnCopyHexClicked;
        if (_hexTextBox is not null)
        {
            _hexTextBox.KeyDown += OnHexKeyDown;
            _hexTextBox.LostFocus += OnHexLostFocus;
            AutomationProperties.SetLiveSetting(_hexTextBox, AutomationLiveSetting.Assertive);
        }
    }

    private async void OnCopyHexClicked(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.Clipboard is not { } clipboard) throw new InvalidOperationException("Clipboard unavailable.");
            await clipboard.SetTextAsync(SelectedHex);
            CopyStatusMessage = MdLocalization.GetString("CopiedHex", this);
        }
        catch
        {
            CopyStatusMessage = MdLocalization.GetString("CopyFailed", this);
        }
        AutomationProperties.SetHelpText(_copyHexButton is { } copyButton ? copyButton : this, CopyStatusMessage);
    }

    private void OnHexKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && _hexTextBox is not null)
        {
            TryApplyHex(_hexTextBox.Text);
            e.Handled = true;
        }
    }

    private void OnHexLostFocus(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_hexTextBox is not null) TryApplyHex(_hexTextBox.Text);
    }

    public bool TryApplyHex(string? hex)
    {
        if (!string.IsNullOrWhiteSpace(hex))
        {
            hex = hex.Trim();
            if (!hex.StartsWith("#")) hex = "#" + hex;
            if (Color.TryParse(hex, out var color))
            {
                HexValidationMessage = null;
                if (_hexTextBox is MdTextBox materialTextBox)
                {
                    materialTextBox.IsError = false;
                    materialTextBox.ErrorText = null;
                }
                SelectColor(color);
                return true;
            }
        }
        HexValidationMessage = MdLocalization.GetString("InvalidHex", this);
        if (_hexTextBox is MdTextBox invalidTextBox)
        {
            invalidTextBox.IsError = true;
            invalidTextBox.ErrorText = HexValidationMessage;
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

    private void ClearHexValidation()
    {
        HexValidationMessage = null;
        if (_hexTextBox is MdTextBox materialTextBox)
        {
            materialTextBox.IsError = false;
            materialTextBox.ErrorText = null;
        }
    }

    private void OnSelectedHexChanged()
    {
        if (_updatingInternals) return;
        TryApplyHex(SelectedHex);
    }

    private void OnAlphaEnabledChanged()
    {
        if (!IsAlphaEnabled && SelectedColor.A < byte.MaxValue)
            SelectColor(Color.FromArgb(byte.MaxValue, SelectedColor.R, SelectedColor.G, SelectedColor.B));
        else
        {
            _updatingInternals = true;
            try
            {
                var color = SelectedColor;
                SetCurrentValue(SelectedHexProperty, IsAlphaEnabled && color.A < byte.MaxValue
                    ? $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}"
                    : $"#{color.R:X2}{color.G:X2}{color.B:X2}");
                if (!IsAlphaEnabled) SetCurrentValue(AlphaProperty, 100d);
            }
            finally { _updatingInternals = false; }
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
            ClearHexValidation();
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

    private void OnPanelConfigurationChanged()
    {
        if (!IsPanelVisible(PickerMode))
        {
            MdColorPickerMode? fallback = IsMaterialPalettePanelVisible
                ? MdColorPickerMode.MaterialPalette
                : IsSpectrumPanelVisible
                    ? MdColorPickerMode.SpectrumSliders
                    : IsRecentColorsPanelVisible
                        ? MdColorPickerMode.Presets
                        : null;

            if (fallback is { } fallbackMode && fallbackMode != PickerMode)
            {
                SetCurrentValue(PickerModeProperty, fallbackMode);
                return;
            }
        }

        UpdateModePseudoClasses();
    }

    private bool IsPanelVisible(MdColorPickerMode mode) => mode switch
    {
        MdColorPickerMode.MaterialPalette => IsMaterialPalettePanelVisible,
        MdColorPickerMode.SpectrumSliders => IsSpectrumPanelVisible,
        MdColorPickerMode.Presets => IsRecentColorsPanelVisible,
        _ => false
    };

    private void UpdateModePseudoClasses()
    {
        var visiblePanelCount = (IsMaterialPalettePanelVisible ? 1 : 0) +
                                (IsSpectrumPanelVisible ? 1 : 0) +
                                (IsRecentColorsPanelVisible ? 1 : 0);

        PseudoClasses.Set(":palette-panel-visible", IsMaterialPalettePanelVisible);
        PseudoClasses.Set(":spectrum-panel-visible", IsSpectrumPanelVisible);
        PseudoClasses.Set(":presets-panel-visible", IsRecentColorsPanelVisible);
        PseudoClasses.Set(":mode-selector-visible", IsModeSelectorVisible && visiblePanelCount > 1);
        PseudoClasses.Set(":palette", PickerMode == MdColorPickerMode.MaterialPalette && IsMaterialPalettePanelVisible);
        PseudoClasses.Set(":spectrum", PickerMode == MdColorPickerMode.SpectrumSliders && IsSpectrumPanelVisible);
        PseudoClasses.Set(":presets", PickerMode == MdColorPickerMode.Presets && IsRecentColorsPanelVisible);
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

    public static readonly IValueConverter ColorToHex =
        new FuncValueConverter<Color, string>(color => color.A < byte.MaxValue
            ? $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}"
            : $"#{color.R:X2}{color.G:X2}{color.B:X2}");

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
