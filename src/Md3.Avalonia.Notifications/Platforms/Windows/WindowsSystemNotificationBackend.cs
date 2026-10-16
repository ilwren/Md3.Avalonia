using System.Runtime.Versioning;
using System.Security;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Md3.Avalonia.Notifications;

[SupportedOSPlatform("windows10.0.10240.0")]
internal sealed class WindowsSystemNotificationBackend : IMdSystemNotificationBackend
{
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

        try
        {
            var document = new XmlDocument();
            document.LoadXml(xml);
            var toast = new ToastNotification(document);
            ToastNotificationManager.CreateToastNotifier(appUserModelId).Show(toast);
            return Task.CompletedTask;
        }
        catch (Exception exception) when (exception is not MdSystemNotificationException)
        {
            throw new MdSystemNotificationException("Windows could not display the toast notification.", exception);
        }
    }
}
