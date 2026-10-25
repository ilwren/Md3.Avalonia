using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using Md3.Avalonia.Controls;
using Avalonia.Styling;
using Md3.Avalonia.Motion;
using Md3.Avalonia.Themes.Dynamic;

namespace Md3.Avalonia.Gallery.Pages;

public partial class ThemeResourcesGalleryPage : UserControl
{
    private static readonly string[] PreviewRoles =
    [
        "Primary", "OnPrimary", "PrimaryContainer", "Secondary", "SecondaryContainer",
        "Tertiary", "TertiaryContainer", "Error", "ErrorContainer", "Surface",
        "SurfaceContainerLow", "SurfaceContainer", "SurfaceContainerHigh", "SurfaceContainerHighest",
        "InverseSurface", "PrimaryFixed", "SecondaryFixed", "TertiaryFixed"
    ];

    public ThemeResourcesGalleryPage()
    {
        InitializeComponent();
        AutomationProperties.SetLiveSetting(StatusText, AutomationLiveSetting.Polite);
        VariantBox.ItemsSource = Enum.GetValues<MdThemeSchemeVariant>();
        ContrastBox.ItemsSource = Enum.GetValues<MdThemeContrastLevel>();
        ModeBox.ItemsSource = Enum.GetValues<MdThemeMode>();
        MotionBox.ItemsSource = Enum.GetValues<MdMotionScheme>();
        FontBox.ItemsSource = Enum.GetValues<MdThemeFontProfile>();
        ShapeBox.ItemsSource = Enum.GetValues<MdThemeShapeScale>();
        LoadOptions(MdThemeManager.Current);
        RefreshPreview(MdThemeGenerator.Generate(MdThemeManager.Current, IsDark(MdThemeManager.Current)));
    }

    private void ApplyTheme(object? sender, RoutedEventArgs e) => ApplyOptions(ReadOptions());

    private void ResetTheme(object? sender, RoutedEventArgs e)
    {
        var options = new MdThemeOptions();
        LoadOptions(options);
        ApplyOptions(options);
    }

    private void ApplyJson(object? sender, RoutedEventArgs e)
    {
        try
        {
            var options = MdThemeJson.Deserialize(JsonBox.Text ?? string.Empty);
            LoadOptions(options);
            ApplyOptions(options);
        }
        catch (Exception exception) when (exception is System.Text.Json.JsonException or ArgumentException or FormatException)
        {
            StatusText.Text = $"JSON was not applied: {exception.Message}";
        }
    }

    private async void CopyJson(object? sender, RoutedEventArgs e)
    {
        JsonBox.Text = MdThemeJson.Serialize(ReadOptions());
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(JsonBox.Text);
            StatusText.Text = "Theme JSON copied to the clipboard.";
        }
    }

    private void ExportJson(object? sender, RoutedEventArgs e)
    {
        ShowFileSystemPicker(
            title: "Choose an export folder",
            mode: MdFileSystemPickerMode.Folder,
            patterns: null,
            async path =>
            {
                var outputPath = Path.Combine(path, "material-theme.json");
                try
                {
                    await File.WriteAllTextAsync(outputPath, MdThemeJson.Serialize(ReadOptions()));
                    StatusText.Text = $"Exported to {outputPath}.";
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                {
                    StatusText.Text = $"Theme was not exported: {exception.Message}";
                }
            });
    }

    private void ImportJson(object? sender, RoutedEventArgs e)
    {
        ShowFileSystemPicker(
            title: "Import a Material theme",
            mode: MdFileSystemPickerMode.File,
            patterns: ["*.json"],
            async path =>
            {
                try
                {
                    JsonBox.Text = await File.ReadAllTextAsync(path);
                    ApplyJson(sender, e);
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                {
                    StatusText.Text = $"Theme was not imported: {exception.Message}";
                }
            });
    }

    private void ShowFileSystemPicker(
        string title,
        MdFileSystemPickerMode mode,
        IEnumerable<string>? patterns,
        Func<string, Task> confirmed)
    {
        var host = GalleryDialogHosts.Find(this);
        if (host is null)
        {
            StatusText.Text = "The file-system picker needs a Gallery dialog host.";
            return;
        }

        var viewportWidth = host.Bounds.Width > 0
            ? host.Bounds.Width
            : TopLevel.GetTopLevel(this)?.ClientSize.Width ?? Bounds.Width;
        var useFullScreen = viewportWidth <= 0 || viewportWidth < 760;
        var picker = new MdFileSystemPicker
        {
            Title = title,
            Mode = mode,
            ConfirmText = mode == MdFileSystemPickerMode.Folder ? "Export here" : "Import",
            InitialDirectory = FileSystemPickerGalleryPage.ResolveAccessibleStartDirectory(),
            FilePatterns = patterns
        };
        if (!useFullScreen)
        {
            picker.Width = 700;
            picker.Height = 520;
        }

        picker.CancelRequested += (_, _) => host.Close();
        picker.SelectionConfirmed += async (_, args) =>
        {
            var path = args.Paths.FirstOrDefault();
            if (path is null) return;
            host.Close(path);
            await confirmed(path);
        };

        var dialog = new MdDialog
        {
            Content = picker,
            Padding = new Thickness(0),
            Variant = useFullScreen ? MdDialogVariant.FullScreen : MdDialogVariant.Basic
        };
        if (!useFullScreen)
        {
            dialog.MinWidth = 700;
            dialog.MaxWidth = 760;
        }
        _ = host.ShowAsync(dialog);
    }

    private void ApplyOptions(MdThemeOptions options)
    {
        try
        {
            _ = MdThemeGenerator.ParseSeed(options.SeedColor);
            var application = Application.Current ?? throw new InvalidOperationException("Application is unavailable.");
            application.RequestedThemeVariant = options.ThemeMode switch
            {
                MdThemeMode.Light => ThemeVariant.Light,
                MdThemeMode.Dark => ThemeVariant.Dark,
                _ => ThemeVariant.Default
            };
            var roles = MdThemeManager.Apply(application, options, IsDark(options));
            if (TopLevel.GetTopLevel(this) is StyledElement root)
                MdMotion.SetScheme(root, options.MotionScheme);
            JsonBox.Text = MdThemeJson.Serialize(options);
            RefreshPreview(roles);
            StatusText.Text = $"Applied {options.SchemeVariant} · {options.ContrastLevel} · {(IsDark(options) ? "Dark" : "Light")}.";
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException)
        {
            StatusText.Text = $"Theme was not applied: {exception.Message}";
        }
    }

    private void RefreshPreview(IReadOnlyDictionary<string, Color> roles)
    {
        SwatchesPanel.Children.Clear();
        foreach (var role in PreviewRoles)
        {
            if (!roles.TryGetValue(role, out var color)) continue;
            var foreground = MdThemeManager.ContrastRatio(color, Colors.White) >= 4.5 ? Colors.White : Colors.Black;
            SwatchesPanel.Children.Add(new Border
            {
                Width = 184,
                Height = 84,
                Margin = new Thickness(0, 0, 10, 10),
                Padding = new Thickness(14),
                CornerRadius = new CornerRadius(16),
                Background = new SolidColorBrush(color),
                Child = new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        new TextBlock { Text = role, Foreground = new SolidColorBrush(foreground), FontWeight = FontWeight.Medium },
                        new TextBlock { Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}", Foreground = new SolidColorBrush(foreground), FontSize = 12 }
                    }
                }
            });
        }

        DiagnosticsPanel.Children.Clear();
        foreach (var item in MdThemeManager.Diagnose(roles))
        {
            var foreground = roles[item.ForegroundRole];
            var background = roles[item.BackgroundRole];
            var sampleText = new StackPanel
            {
                Spacing = 3,
                Children =
                {
                    new TextBlock
                    {
                        Text = $"{item.ForegroundRole} on {item.BackgroundRole}",
                        Foreground = new SolidColorBrush(foreground),
                        FontWeight = FontWeight.Medium
                    },
                    new TextBlock
                    {
                        Text = item.Purpose,
                        Foreground = new SolidColorBrush(foreground),
                        FontSize = 12
                    },
                    new TextBlock
                    {
                        Text = $"FG #{foreground.R:X2}{foreground.G:X2}{foreground.B:X2}  ·  BG #{background.R:X2}{background.G:X2}{background.B:X2}",
                        Foreground = new SolidColorBrush(foreground),
                        FontSize = 11,
                        Opacity = 0.82
                    }
                }
            };
            var status = new Border
            {
                [Grid.ColumnProperty] = 1,
                MinWidth = 124,
                Padding = new Thickness(12, 8),
                CornerRadius = new CornerRadius(18),
                Background = new SolidColorBrush(foreground),
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text = $"{item.Ratio:0.00}:1  {(item.Passes ? "PASS" : "REVIEW")}",
                    Foreground = new SolidColorBrush(background),
                    FontWeight = FontWeight.Medium,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };
            DiagnosticsPanel.Children.Add(new Border
            {
                Padding = new Thickness(16, 12),
                CornerRadius = new CornerRadius(16),
                Background = new SolidColorBrush(background),
                BorderBrush = new SolidColorBrush(roles["OutlineVariant"]),
                BorderThickness = new Thickness(1),
                Child = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                    ColumnSpacing = 16,
                    Children = { sampleText, status }
                }
            });
        }
    }

    private MdThemeOptions ReadOptions() => new()
    {
        SeedColor = string.IsNullOrWhiteSpace(SeedBox.Text) ? "#6750A4" : SeedBox.Text.Trim(),
        SchemeVariant = Selected(VariantBox, MdThemeSchemeVariant.TonalSpot),
        ContrastLevel = Selected(ContrastBox, MdThemeContrastLevel.Standard),
        ThemeMode = Selected(ModeBox, MdThemeMode.System),
        MotionScheme = Selected(MotionBox, MdMotionScheme.Expressive),
        FontProfile = Selected(FontBox, MdThemeFontProfile.Brand),
        ShapeScale = Selected(ShapeBox, MdThemeShapeScale.Standard)
    };

    private void LoadOptions(MdThemeOptions options)
    {
        SeedBox.Text = options.SeedColor;
        VariantBox.SelectedItem = options.SchemeVariant;
        ContrastBox.SelectedItem = options.ContrastLevel;
        ModeBox.SelectedItem = options.ThemeMode;
        MotionBox.SelectedItem = options.MotionScheme;
        FontBox.SelectedItem = options.FontProfile;
        ShapeBox.SelectedItem = options.ShapeScale;
        JsonBox.Text = MdThemeJson.Serialize(options);
    }

    private static T Selected<T>(MdComboBox control, T fallback) where T : struct =>
        control.SelectedItem is T value ? value : fallback;

    private static bool IsDark(MdThemeOptions options)
    {
        if (options.ThemeMode == MdThemeMode.Dark) return true;
        if (options.ThemeMode == MdThemeMode.Light) return false;
        return Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
    }
}
