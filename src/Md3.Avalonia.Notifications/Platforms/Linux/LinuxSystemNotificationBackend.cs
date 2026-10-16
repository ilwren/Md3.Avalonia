using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;

namespace Md3.Avalonia.Notifications;

[SupportedOSPlatform("linux")]
internal sealed class LinuxSystemNotificationBackend : IMdSystemNotificationBackend
{
    private static readonly object InitializationLock = new();
    private static bool _isInitialized;

    private readonly MdSystemNotificationOptions _options;

    public LinuxSystemNotificationBackend(MdSystemNotificationOptions options) => _options = options;

    public Task ShowAsync(MdSystemNotification notification, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            EnsureInitialized(_options.ApplicationName);
            var body = SecurityElement.Escape(notification.Body) ?? string.Empty;
            var nativeNotification = LibNotify.NotificationNew(notification.Title, body, IntPtr.Zero);
            if (nativeNotification == IntPtr.Zero)
                throw new MdSystemNotificationException("libnotify could not allocate a notification.");

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                IntPtr error = IntPtr.Zero;
                try
                {
                    if (LibNotify.NotificationShow(nativeNotification, out error) == 0)
                    {
                        var detail = ReadErrorMessage(error);
                        throw new MdSystemNotificationException(
                            string.IsNullOrWhiteSpace(detail)
                                ? "The Linux notification service rejected the notification."
                                : $"The Linux notification service rejected the notification: {detail}");
                    }
                }
                finally
                {
                    if (error != IntPtr.Zero) LibNotify.ErrorFree(error);
                }
            }
            finally
            {
                LibNotify.ObjectUnref(nativeNotification);
            }

            return Task.CompletedTask;
        }
        catch (MdSystemNotificationException)
        {
            throw;
        }
        catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException)
        {
            throw new MdSystemNotificationException(
                "The Linux native notification backend requires libnotify (usually provided by the libnotify package) and a running desktop notification service.",
                exception);
        }
    }

    private static void EnsureInitialized(string applicationName)
    {
        lock (InitializationLock)
        {
            if (_isInitialized) return;
            if (LibNotify.Initialize(applicationName) == 0)
            {
                throw new MdSystemNotificationException(
                    "libnotify could not connect to a desktop notification service. Check that a desktop session and D-Bus notification daemon are running.");
            }

            _isInitialized = true;
        }
    }

    private static string? ReadErrorMessage(IntPtr error)
    {
        if (error == IntPtr.Zero) return null;
        var nativeError = Marshal.PtrToStructure<GLibError>(error);
        return nativeError.Message == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(nativeError.Message);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GLibError
    {
        public uint Domain;
        public int Code;
        public IntPtr Message;
    }

    private static class LibNotify
    {
        private const string NotifyLibrary = "libnotify.so.4";
        private const string GObjectLibrary = "libgobject-2.0.so.0";
        private const string GLibLibrary = "libglib-2.0.so.0";

        [DllImport(NotifyLibrary, EntryPoint = "notify_init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Initialize([MarshalAs(UnmanagedType.LPUTF8Str)] string applicationName);

        [DllImport(NotifyLibrary, EntryPoint = "notify_notification_new", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr NotificationNew(
            [MarshalAs(UnmanagedType.LPUTF8Str)] string summary,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string body,
            IntPtr icon);

        [DllImport(NotifyLibrary, EntryPoint = "notify_notification_show", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int NotificationShow(IntPtr notification, out IntPtr error);

        [DllImport(GObjectLibrary, EntryPoint = "g_object_unref", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ObjectUnref(IntPtr instance);

        [DllImport(GLibLibrary, EntryPoint = "g_error_free", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ErrorFree(IntPtr error);
    }
}
