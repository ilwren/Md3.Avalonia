using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LiveChartsCore;
using Md3.Avalonia.Themes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Md3.Avalonia.Gallery.Pages;

namespace Md3.Avalonia.Gallery;

public partial class App : Application
{
    private static bool _quitting;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        // LiveCharts resolves every chart's look from one theme held by its global settings - there
        // is no per-control style seam - so the gallery configures it once here, before the first
        // chart view exists. The chart page itself sets no paints at all.
        LiveCharts.Configure(settings => settings.AddMaterialTheme());

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow();
            desktop.MainWindow = window;
            ConfigureNativeAotSmoke(desktop, window);
            InstallTrayIcon(desktop);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new AndroidGalleryView();
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Runs a scenario inside the published native executable when requested by the AOT probe.
    /// Success is communicated out-of-process through a marker because merely keeping the Gallery
    /// window alive would not prove that the requested page, template and interaction path ran.
    /// </summary>
    private static void ConfigureNativeAotSmoke(
        IClassicDesktopStyleApplicationLifetime desktop,
        MainWindow window)
    {
        const string scenarioName = "file-system-picker";
        var scenario = Environment.GetEnvironmentVariable("MD3_GALLERY_AOT_SMOKE_SCENARIO");
        if (string.IsNullOrEmpty(scenario)) return;

        var markerPath = Environment.GetEnvironmentVariable("MD3_GALLERY_AOT_SMOKE_MARKER");
        if (!string.Equals(scenario, scenarioName, StringComparison.Ordinal) || string.IsNullOrWhiteSpace(markerPath))
            throw new InvalidOperationException("The native-AOT smoke scenario or marker path is invalid.");

        window.Opened += async (_, _) =>
        {
            try
            {
                if (!window.NavigateToIndexedPage("File-system picker") ||
                    window.CurrentPage is not FileSystemPickerGalleryPage page)
                {
                    throw new InvalidOperationException("The native-AOT Gallery could not open the file-system picker page.");
                }

                await page.RunNativeAotSmokeAsync();
                File.WriteAllText(markerPath, $"{scenarioName}: passed{Environment.NewLine}");
                Trace.TraceInformation("Native-AOT smoke scenario passed: {0}", scenarioName);
            }
            catch (Exception exception)
            {
                Trace.TraceError("Native-AOT smoke scenario failed: {0}", exception);
                try
                {
                    File.WriteAllText(markerPath + ".failed", exception.ToString());
                }
                catch
                {
                    // The process exit still makes the probe fail if the diagnostic cannot be written.
                }
                desktop.Shutdown();
            }
        };
    }

    /// <summary>
    /// Desktop-only shell feature: a tray icon whose menu shows the window, switches the theme
    /// and owns shutdown. Closing the window hides it instead of quitting while the tray is
    /// present; the Android single-view lifetime never reaches this code.
    /// </summary>
    private static void InstallTrayIcon(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var trayIcon = new TrayIcon
        {
            Icon = LoadGalleryIcon(),
            ToolTipText = "Md3.Avalonia Gallery",
            Menu = BuildTrayMenu(desktop),
            IsVisible = true,
        };
        TrayIcon.SetIcons(Current!, new TrayIcons { trayIcon });

        if (desktop.MainWindow is { } window)
        {
            window.Closing += (_, e) =>
            {
                if (_quitting || e.IsProgrammatic) return;
                e.Cancel = true;
                window.Hide();
            };
        }
    }

    internal static WindowIcon? LoadGalleryIcon()
    {
        // The visible Gallery mark, title-bar icon and tray all use the repository's canonical
        // logo. A missing or unreadable asset must not take the shell down; the menu still works.
        try
        {
            using var stream = AssetLoader.Open(new Uri("avares://Md3.Avalonia.Gallery/Assets/logo-128.png"));
            return new WindowIcon(new Bitmap(stream));
        }
        catch
        {
            return null;
        }
    }

    private static NativeMenu BuildTrayMenu(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var menu = new NativeMenu();

        var show = new NativeMenuItem(GalleryLocalization.Choose("Show Gallery", "显示 Gallery"));
        show.Click += (_, _) =>
        {
            if (desktop.MainWindow is { } window)
            {
                window.Show();
                window.WindowState = WindowState.Normal;
                window.Activate();
            }
        };
        menu.Add(show);

        var theme = new NativeMenuItem(GalleryLocalization.Choose("Theme", "主题")) { Menu = new NativeMenu() };
        foreach (var (english, chinese, variant) in new (string, string, ThemeVariant)[]
        {
            ("Light", "浅色", ThemeVariant.Light),
            ("Dark", "深色", ThemeVariant.Dark),
            ("System", "跟随系统", ThemeVariant.Default),
        })
        {
            var item = new NativeMenuItem(GalleryLocalization.Choose(english, chinese))
            {
                ToggleType = MenuItemToggleType.Radio,
                IsChecked = ReferenceEquals(Current?.RequestedThemeVariant, variant),
            };
            var chosen = variant;
            item.Click += (_, _) =>
            {
                if (Current is { } app)
                {
                    app.RequestedThemeVariant = chosen;
                }
            };
            theme.Menu!.Add(item);
        }
        menu.Add(theme);

        menu.Add(new NativeMenuItemSeparator());

        var exit = new NativeMenuItem(GalleryLocalization.Choose("Exit", "退出"));
        exit.Click += (_, _) =>
        {
            _quitting = true;
            desktop.Shutdown();
        };
        menu.Add(exit);

        return menu;
    }
}
