using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// The desktop gallery keeps its own notification-area icon. A headless host has no tray host to
/// put one in, but the model, the menu and the commands are all testable there - and they are the
/// part that carries the behaviour: bringing the shell back, hiding it, switching the theme and
/// quitting.
/// </summary>
public sealed class MdGalleryTrayIconTests
{
    private static MdTrayMenuItem Entry(MdTrayIcon tray, string header) =>
        Assert.IsType<MdTrayMenuItem>(tray.Items.First(item => (item as MdTrayMenuItem)?.Header == header));

    [AvaloniaFact]
    public void The_Application_Tray_Icon_Exposes_The_Window_Actions()
    {
        var window = new MainWindow();
        window.Show();
        var previousTheme = Application.Current!.RequestedThemeVariant;
        try
        {
            using var trayIcon = GalleryTrayIcon.Attach(window);
            Assert.NotNull(trayIcon);
            var tray = trayIcon!.Tray;

            // Registered with the application, which is what removes it at shutdown.
            Assert.Contains(tray, Assert.IsType<MdTrayIcons>(MdTrayIcon.GetIcons(Application.Current!)));

            Assert.Equal("Material Gallery", tray.ToolTipText);
            Assert.NotNull(tray.Icon);
            Assert.Contains(tray.Items, item => item is MdTrayMenuItemSeparator);

            // Primary activation is the same action as the first menu entry.
            tray.Activate();
            Assert.True(window.IsVisible);

            Entry(tray, "Hide window").Command!.Execute(null);
            Assert.False(window.IsVisible);
            Entry(tray, "Open Material Gallery").Command!.Execute(null);
            Assert.True(window.IsVisible);

            // The theme entries drive the application and read their check marks back from it,
            // because the gallery's own selector can change the same value.
            var theme = Entry(tray, "Theme");
            var light = Assert.IsType<MdTrayMenuItem>(theme.Items.First(item => (item as MdTrayMenuItem)?.Header == "Light"));
            var dark = Assert.IsType<MdTrayMenuItem>(theme.Items.First(item => (item as MdTrayMenuItem)?.Header == "Dark"));
            var system = Assert.IsType<MdTrayMenuItem>(theme.Items.First(item => (item as MdTrayMenuItem)?.Header == "System"));

            Assert.True(system.IsChecked);
            dark.Command!.Execute(null);
            Assert.Equal(ThemeVariant.Dark, Application.Current!.RequestedThemeVariant);
            trayIcon.RefreshMenu();
            Assert.True(dark.IsChecked);
            Assert.False(system.IsChecked);

            light.Command!.Execute(null);
            Assert.Equal(ThemeVariant.Light, Application.Current!.RequestedThemeVariant);
            trayIcon.RefreshMenu();
            Assert.True(light.IsChecked);
            Assert.False(dark.IsChecked);

            Assert.NotNull(Entry(tray, "Quit").Command);

            // The menu is native, so nothing walks it for translations: the refresh is the only
            // place the gallery's language choice can reach it.
            var previousCulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
                trayIcon.RefreshMenu();
                Assert.Equal("退出", Entry(tray, "退出").Header);
                Assert.Equal("隐藏窗口", Entry(tray, "隐藏窗口").Header);
            }
            finally
            {
                CultureInfo.CurrentUICulture = previousCulture;
                trayIcon.RefreshMenu();
            }
            Assert.Equal("Quit", Entry(tray, "Quit").Header);
        }
        finally
        {
            Application.Current!.RequestedThemeVariant = previousTheme;
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Disposing_The_Application_Tray_Icon_Unregisters_It()
    {
        var window = new MainWindow();
        window.Show();
        var previous = MdTrayIcon.GetIcons(Application.Current!);
        try
        {
            var trayIcon = GalleryTrayIcon.Attach(window);
            Assert.NotNull(trayIcon);
            trayIcon!.Dispose();
            // A second dispose stays a no-op rather than re-clearing a set it no longer owns.
            trayIcon.Dispose();
            Assert.Null(MdTrayIcon.GetIcons(Application.Current!));
        }
        finally
        {
            MdTrayIcon.SetIcons(Application.Current!, previous);
            window.Close();
        }
    }

    [AvaloniaFact]
    public void The_Rasterised_Icon_Is_A_Usable_Bitmap()
    {
        var bitmap = TrayIconArt.Create(Application.Current!);
        try
        {
            Assert.NotNull(bitmap);
            Assert.Equal(32, bitmap!.PixelSize.Width);
            Assert.Equal(32, bitmap.PixelSize.Height);
            // The platform takes the pixels, so a blank tile would be a silent failure; this is
            // the same wrap the tray icon does with the result.
            var icon = new WindowIcon(bitmap);
            Assert.NotNull(icon);
        }
        finally
        {
            bitmap?.Dispose();
        }
    }
}
