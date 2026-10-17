using System.Runtime.InteropServices;
using System.Security;

namespace Md3.Avalonia.Notifications;

internal sealed class DesktopSystemNotificationBackend : IMdSystemNotificationBackend
{
    private readonly MdSystemNotificationOptions _options;

    private DesktopSystemNotificationBackend(MdSystemNotificationOptions options) => _options = options;

    public static IMdSystemNotificationBackend Create(MdSystemNotificationOptions options)
    {
        if (OperatingSystem.IsMacOS()) return new DesktopSystemNotificationBackend(options);
        if (OperatingSystem.IsLinux()) return new LinuxSystemNotificationBackend(options);
        if (OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "The Windows native notification backend requires the host target framework " +
                "net10.0-windows10.0.19041.0.");
        }

        throw new PlatformNotSupportedException(
            "Native system notifications are supported on Windows, macOS, Linux and Android.");
    }

    public Task ShowAsync(MdSystemNotification notification, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            MacOSUserNotification.Show(notification.Title, notification.Body);
            return Task.CompletedTask;
        }
        catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException or InvalidOperationException or SecurityException)
        {
            throw new MdSystemNotificationException("macOS UserNotifications could not display the notification.", exception);
        }
    }
}

/// <summary>Small AppKit bridge so macOS notifications are attributed to the host process.</summary>
internal static class MacOSUserNotification
{
    private static readonly Lazy<IntPtr> AppKitHandle = new(() =>
        NativeLibrary.Load("/System/Library/Frameworks/AppKit.framework/AppKit"));

    public static void Show(string title, string body)
    {
        _ = AppKitHandle.Value;
        var pool = SendObject(GetClass("NSAutoreleasePool"), Selector("new"));
        if (pool == IntPtr.Zero) throw new InvalidOperationException("Could not create an Objective-C autorelease pool.");

        IntPtr notification = IntPtr.Zero;
        try
        {
            var titleValue = CreateString(title);
            var bodyValue = CreateString(body);
            var center = SendObject(GetClass("NSUserNotificationCenter"), Selector("defaultUserNotificationCenter"));
            notification = SendObject(GetClass("NSUserNotification"), Selector("new"));
            if (center == IntPtr.Zero || notification == IntPtr.Zero)
                throw new InvalidOperationException("NSUserNotificationCenter is unavailable.");

            SendVoid(notification, Selector("setTitle:"), titleValue);
            SendVoid(notification, Selector("setInformativeText:"), bodyValue);
            SendVoid(center, Selector("deliverNotification:"), notification);
        }
        finally
        {
            if (notification != IntPtr.Zero) SendVoid(notification, Selector("release"));
            SendVoid(pool, Selector("drain"));
        }
    }

    private static IntPtr CreateString(string value) =>
        SendObjectString(GetClass("NSString"), Selector("stringWithUTF8String:"), value);

    private static IntPtr GetClass(string name) => objc_getClass(name);
    private static IntPtr Selector(string name) => sel_registerName(name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr objc_getClass([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr sel_registerName([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SendObject(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SendObject(IntPtr receiver, IntPtr selector, IntPtr argument);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SendObjectString(
        IntPtr receiver,
        IntPtr selector,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string value);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
    private static extern void SendVoid(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
    private static extern void SendVoid(IntPtr receiver, IntPtr selector, IntPtr argument);
}
