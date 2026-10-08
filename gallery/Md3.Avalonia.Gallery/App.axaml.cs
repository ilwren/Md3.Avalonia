using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Md3.Avalonia.Gallery;

public partial class App : Application
{
    private GalleryTrayIcon? _trayIcon;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow();
            desktop.MainWindow = window;
            // The desktop application keeps its own notification-area icon, so the shell can be
            // dismissed and brought back the way a desktop application is expected to behave. The
            // helper resolves to null on a platform whose family has no notification area, and the
            // no-op adapter covers a host that has the family but no implementation.
            _trayIcon = GalleryTrayIcon.Attach(window);
            desktop.ShutdownRequested += (_, _) => _trayIcon?.Dispose();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new AndroidGalleryView();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
