using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery;

/// <summary>
/// The desktop gallery's own notification-area icon. It is the same Material API the "Tray icon"
/// page demonstrates, wired to the real window: primary activation brings the window back, the
/// menu carries the window and theme actions, and the theme entries read their check marks from the
/// application instead of from a stored copy.
/// </summary>
/// <remarks>
/// Only desktop lifetimes create one. Android and the browser resolve the no-op tray adapter, and
/// the shared gallery project compiles for them too, so nothing here may assume a window exists.
/// </remarks>
internal sealed class GalleryTrayIcon : IDisposable
{
    private readonly MdTrayIcon _tray;
    private readonly MdTrayMenuItem _light;
    private readonly MdTrayMenuItem _dark;
    private readonly MdTrayMenuItem _system;
    private readonly RenderTargetBitmap? _iconBitmap;
    private MdTrayIcons? _registered;

    private GalleryTrayIcon(MdTrayIcon tray, RenderTargetBitmap? iconBitmap,
        MdTrayMenuItem light, MdTrayMenuItem dark, MdTrayMenuItem system)
    {
        _tray = tray;
        _iconBitmap = iconBitmap;
        _light = light;
        _dark = dark;
        _system = system;
    }

    /// <summary>
    /// Builds the icon and registers it with the application so it is removed on shutdown.
    /// Returns <see langword="null"/> on platforms whose family has no notification area, which is
    /// the same answer <see cref="MdTrayIcon.IsSupported"/> gives before an icon is attached.
    /// </summary>
    internal static GalleryTrayIcon? Attach(Window window)
    {
        if (Application.Current is not { } application) return null;
        if (!MdTrayIconPlatformAdapterResolver.IsSupported) return null;

        var iconBitmap = TrayIconArt.Create(application);
        var tray = new MdTrayIcon
        {
            Icon = iconBitmap is null ? null : new WindowIcon(iconBitmap),
            ToolTipText = "Material Gallery",
            Command = new GalleryCommand(_ => Show(window))
        };

        var open = new MdTrayMenuItem("Open Material Gallery") { Command = new GalleryCommand(_ => Show(window)) };
        var hide = new MdTrayMenuItem("Hide window") { Command = new GalleryCommand(_ => window.Hide()) };
        var light = new MdTrayMenuItem("Light") { ToggleType = MenuItemToggleType.Radio };
        var dark = new MdTrayMenuItem("Dark") { ToggleType = MenuItemToggleType.Radio };
        var system = new MdTrayMenuItem("System") { ToggleType = MenuItemToggleType.Radio };
        light.Command = new GalleryCommand(_ => ApplyTheme(ThemeVariant.Light));
        dark.Command = new GalleryCommand(_ => ApplyTheme(ThemeVariant.Dark));
        system.Command = new GalleryCommand(_ => ApplyTheme(ThemeVariant.Default));

        var theme = new MdTrayMenuItem("Theme");
        theme.Items.Add(light);
        theme.Items.Add(dark);
        theme.Items.Add(system);

        tray.Items.Add(open);
        tray.Items.Add(hide);
        tray.Items.Add(theme);
        tray.Items.Add(new MdTrayMenuItemSeparator());
        tray.Items.Add(new MdTrayMenuItem("Quit")
        {
            Command = new GalleryCommand(_ => Quit())
        });

        var instance = new GalleryTrayIcon(tray, iconBitmap, light, dark, system);
        // The theme a menu shows has to be read when the menu opens: the app's own theme selector
        // changes it too, and a check mark written once would go stale.
        tray.MenuRefreshRequested += (_, _) => instance.SyncThemeChecks();
        instance.SyncThemeChecks();

        var registered = new MdTrayIcons { tray };
        MdTrayIcon.SetIcons(application, registered);
        instance._registered = registered;
        return instance;
    }

    /// <summary>The icon itself, for a host or a test that wants to inspect the exported menu.</summary>
    internal MdTrayIcon Tray => _tray;

    internal void SyncThemeChecks()
    {
        var current = Application.Current?.RequestedThemeVariant;
        _light.IsChecked = current == ThemeVariant.Light;
        _dark.IsChecked = current == ThemeVariant.Dark;
        _system.IsChecked = current is null || current == ThemeVariant.Default;
    }

    private static void ApplyTheme(ThemeVariant variant)
    {
        if (Application.Current is { } application) application.RequestedThemeVariant = variant;
    }

    private static void Show(Window window)
    {
        // Show() on an already visible window still raises the opening path on some platforms, so
        // the state is set first and the activation is what the user actually sees.
        if (!window.IsVisible) window.Show();
        window.WindowState = WindowState.Normal;
        window.Activate();
    }

    private static void Quit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    public void Dispose()
    {
        _tray.Dispose();
        if (Application.Current is { } application && _registered is not null &&
            ReferenceEquals(MdTrayIcon.GetIcons(application), _registered))
        {
            MdTrayIcon.SetIcons(application, null);
        }
        _registered = null;
        _iconBitmap?.Dispose();
    }
}

/// <summary>
/// Rasterises the notification-area icon. The platform takes a bitmap rather than a vector or a
/// font glyph, so the Material symbol is drawn off-screen; without the optional icon package the
/// tile still renders as a valid solid surface instead of a look-alike character.
/// </summary>
internal static class TrayIconArt
{
    internal static RenderTargetBitmap? Create(Application application, string? glyph = null)
    {
        // Reading a glyph is what makes the optional icon package register its font and inject the
        // family resource, so it has to happen before the family is looked up: the other order
        // works only when some earlier presenter already configured it.
        var glyphValue = glyph ?? MdSymbols.Palette;

        var surface = new Border
        {
            Width = 32,
            Height = 32,
            CornerRadius = new CornerRadius(10),
            Background = Resolve<IBrush>(application, "Md.Sys.Color.Primary.Brush")
        };

        if (glyphValue is not null &&
            Resolve<FontFamily>(application, "Md.Sys.Typeface.Symbols.Rounded") is { } family)
        {
            surface.Child = new MdIcon
            {
                Glyph = glyphValue,
                Size = 22,
                FontFamily = family,
                Foreground = Resolve<IBrush>(application, "Md.Sys.Color.OnPrimary.Brush") ?? Brushes.White
            };
        }

        surface.Measure(new Size(32, 32));
        surface.Arrange(new Rect(0, 0, 32, 32));

        var bitmap = new RenderTargetBitmap(new PixelSize(32, 32), new Vector(96, 96));
        bitmap.Render(surface);
        return bitmap;
    }

    private static T? Resolve<T>(Application application, string key) where T : class =>
        application.TryGetResource(key, application.ActualThemeVariant, out var value) ? value as T : null;
}

/// <summary>An <see cref="ICommand"/> over a delegate, for menu entries.</summary>
internal sealed class GalleryCommand(Action<object?> execute) : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => execute(parameter);
}
