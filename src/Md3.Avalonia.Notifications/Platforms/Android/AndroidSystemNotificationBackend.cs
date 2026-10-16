using Android.App;
using Android.Content;
using Android.Content.PM;

namespace Md3.Avalonia.Notifications;

internal sealed class AndroidSystemNotificationBackend : IMdSystemNotificationBackend
{
    private static int _nextNotificationId;
    private readonly Context _context;
    private readonly MdSystemNotificationOptions _options;

    public AndroidSystemNotificationBackend(MdSystemNotificationOptions options)
    {
        _context = Application.Context;
        _options = options;
    }

    public Task ShowAsync(MdSystemNotification notification, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var manager = _context.GetSystemService(Context.NotificationService) as NotificationManager
            ?? throw new MdSystemNotificationException("Android NotificationManager is unavailable.");

        if (OperatingSystem.IsAndroidVersionAtLeast(33) &&
            _context.CheckSelfPermission(Android.Manifest.Permission.PostNotifications) != Permission.Granted)
        {
            throw new MdSystemNotificationPermissionException(
                "Android 13 and later require POST_NOTIFICATIONS permission. Request it from the foreground Activity before showing a notification.");
        }

        try
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(26) &&
                manager.GetNotificationChannel(_options.AndroidChannelId) is null)
            {
                var channel = new NotificationChannel(
                    _options.AndroidChannelId,
                    _options.AndroidChannelName,
                    NotificationImportance.Default)
                {
                    Description = _options.AndroidChannelName,
                };
                manager.CreateNotificationChannel(channel);
            }

            var icon = _options.AndroidSmallIconResourceId ?? _context.ApplicationInfo?.Icon ?? 0;
            if (icon == 0) icon = Android.Resource.Drawable.IcDialogInfo;
            Notification.Builder builder;
            if (OperatingSystem.IsAndroidVersionAtLeast(26))
                builder = new Notification.Builder(_context, _options.AndroidChannelId);
            else
                builder = new Notification.Builder(_context);
            var nativeNotification = builder
                .SetContentTitle(notification.Title)
                .SetContentText(notification.Body)
                .SetSmallIcon(icon)
                .SetAutoCancel(true)
                .Build();

            manager.Notify(Interlocked.Increment(ref _nextNotificationId), nativeNotification);
            return Task.CompletedTask;
        }
        catch (Java.Lang.SecurityException exception)
        {
            throw new MdSystemNotificationPermissionException(
                "Android denied the notification request. Check POST_NOTIFICATIONS permission and app notification settings.", exception);
        }
        catch (Java.Lang.Exception exception)
        {
            throw new MdSystemNotificationException("Android could not display the notification.", exception);
        }
    }
}
