using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;

namespace Md3.Avalonia.Gallery;

public partial class App : Application
{
    private static bool _quitting;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            InstallTrayIcon(desktop);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new AndroidGalleryView();
        }

        base.OnFrameworkInitializationCompleted();
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
            Icon = LoadTrayIcon(),
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

    private static WindowIcon? LoadTrayIcon()
    {
        // A missing or unreadable icon must not take the shell down; the menu still works.
        try
        {
            using var stream = AssetLoader.Open(new Uri("avares://Md3.Avalonia.Gallery/Assets/tray-icon.png"));
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
