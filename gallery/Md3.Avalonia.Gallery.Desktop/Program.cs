using System;
using Avalonia;
using Md3.Avalonia.Gallery;
#if MD3_GALLERY_LIBVLC
using Md3.Avalonia.LibVLC;
#endif

namespace Md3.Avalonia.Gallery.Desktop;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
#if MD3_GALLERY_LIBVLC
        // The shared Gallery never references LibVLC directly. Only the regular desktop host
        // supplies this factory; NativeAOT and Android retain the backend-neutral graph.
        GalleryVideoBackend.Factory = source => new MdLibVlcVideoPlayer
        {
            AutoPlay = false,
            Loop = true,
            Source = source
        };
#endif
#if WINDOWS
        try
        {
            GalleryNotificationIdentity.WindowsShortcutPath = WindowsToastRegistration.RegisterForCurrentUser();
            GalleryNotificationIdentity.WindowsRegistrationSucceeded = true;
        }
        catch (Exception exception)
        {
            // Keep the Gallery usable if Windows prevents Start-menu registration, but preserve
            // the reason so the Notifications page can show it if a send fails.
            GalleryNotificationIdentity.WindowsRegistrationError = exception.Message;
            System.Diagnostics.Trace.TraceWarning("Windows toast registration failed: {0}", exception);
        }
#endif
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<Md3.Avalonia.Gallery.App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
