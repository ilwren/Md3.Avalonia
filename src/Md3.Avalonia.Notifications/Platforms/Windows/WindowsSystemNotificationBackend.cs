using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Md3.Avalonia.Notifications;

[SupportedOSPlatform("windows10.0.10240.0")]
internal sealed class WindowsSystemNotificationBackend : IMdSystemNotificationBackend
{
    private const int ErrorNotFoundHResult = unchecked((int)0x80070490);
    private readonly MdSystemNotificationOptions _options;

    public WindowsSystemNotificationBackend(MdSystemNotificationOptions options) => _options = options;

    public Task ShowAsync(MdSystemNotification notification, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var appUserModelId = _options.WindowsAppUserModelId;
        if (string.IsNullOrWhiteSpace(appUserModelId))
        {
            throw new MdSystemNotificationException(
                "Windows desktop toasts require a registered Windows AppUserModelID. " +
                "Set WindowsAppUserModelId to the ID on the host application's Start-menu shortcut.");
        }

        var title = SecurityElement.Escape(notification.Title) ?? string.Empty;
        var body = SecurityElement.Escape(notification.Body) ?? string.Empty;
        var xml = $"<toast><visual><binding template=\"ToastGeneric\"><text>{title}</text><text>{body}</text></binding></visual></toast>";

        var stage = "parsing toast XML";
        try
        {
            var document = new XmlDocument();
            document.LoadXml(xml);
            stage = "creating the toast notification";
            var toast = new ToastNotification(document);
            stage = "creating the Windows toast notifier";
            var notifier = ToastNotificationManager.CreateToastNotifier(appUserModelId);

            // On unpackaged apps Windows may not have created this AUMID's notification-settings
            // entry until its first toast is successfully posted. In that state Setting throws
            // HRESULT_FROM_WIN32(ERROR_NOT_FOUND) (0x80070490); that means "unknown yet", not
            // "blocked". Treat it as indeterminate and let Show establish the app's entry.
            stage = "checking the Windows notification setting";
            string? setting = null;
            try
            {
                setting = notifier.Setting.ToString();
            }
            catch (COMException exception) when (exception.HResult == ErrorNotFoundHResult)
            {
                // A first toast is needed before Windows materializes the per-app setting.
            }

            if (setting is not null &&
                (setting.StartsWith("Disabled", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(setting, "NotSupported", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(setting, "NotPresent", StringComparison.OrdinalIgnoreCase)))
            {
                throw new MdSystemNotificationException(
                    $"Windows notifications are disabled for this app ({setting}). Enable notifications for the Gallery in Settings > System > Notifications.");
            }

            stage = "submitting the toast to Windows";
            notifier.Show(toast);
            return Task.CompletedTask;
        }
        catch (Exception exception) when (exception is not MdSystemNotificationException)
        {
            throw new MdSystemNotificationException(
                $"Windows could not display the toast notification while {stage}.", exception);
        }
    }
}
