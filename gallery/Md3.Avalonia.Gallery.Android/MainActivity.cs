using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Notifications;
using GalleryApp = Md3.Avalonia.Gallery.App;

namespace Md3.Avalonia.Gallery.Android;

[Activity(
    Label = "Material 3 Gallery",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    Exported = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public sealed class MainActivity : AvaloniaMainActivity
{
    private const int NotificationPermissionRequestCode = 7314;
    private static WeakReference<MainActivity>? _current;
    private TaskCompletionSource<bool>? _pendingNotificationPermission;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        _current = new WeakReference<MainActivity>(this);
        base.OnCreate(savedInstanceState);
    }

    protected override void OnDestroy()
    {
        // A destroyed activity must not strand the page's async send. A new activity will be
        // resolved on the next attempt rather than retaining this instance across recreation.
        _pendingNotificationPermission?.TrySetResult(false);
        _pendingNotificationPermission = null;
        if (_current?.TryGetTarget(out var current) == true && ReferenceEquals(current, this))
            _current = null;
        base.OnDestroy();
    }

    internal static Task<bool> RequestNotificationPermissionAsync() =>
        _current?.TryGetTarget(out var activity) == true
            ? activity.RequestNotificationPermissionFromActivityAsync()
            : Task.FromResult(false);

    private Task<bool> RequestNotificationPermissionFromActivityAsync()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(33) ||
            CheckSelfPermission(global::Android.Manifest.Permission.PostNotifications) == Permission.Granted)
            return Task.FromResult(true);
        if (_pendingNotificationPermission is { } pending) return pending.Task;

        var request = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingNotificationPermission = request;
        try
        {
            RequestPermissions([global::Android.Manifest.Permission.PostNotifications], NotificationPermissionRequestCode);
        }
        catch
        {
            _pendingNotificationPermission = null;
            throw;
        }
        return request.Task;
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode != NotificationPermissionRequestCode || _pendingNotificationPermission is not { } request) return;
        _pendingNotificationPermission = null;
        var granted = permissions.Length == 1 && permissions[0] == global::Android.Manifest.Permission.PostNotifications &&
                      grantResults.Length == 1 && grantResults[0] == Permission.Granted;
        request.TrySetResult(granted);
    }
}

[Application]
public class MainApplication : AvaloniaAndroidApplication<GalleryApp>
{
    public MainApplication(IntPtr javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        GalleryPlatformIntegration.CreateAndroidNotificationService = static () =>
            MdSystemNotificationService.CreateDefault(new MdSystemNotificationOptions
            {
                ApplicationName = "Material 3 Gallery",
                AndroidChannelId = "gallery.samples",
                AndroidChannelName = "Gallery sample notifications",
                AndroidSmallIconResourceId = Resource.Drawable.ic_notification
            });
        GalleryPlatformIntegration.RequestAndroidNotificationPermission = MainActivity.RequestNotificationPermissionAsync;
        return base.CustomizeAppBuilder(builder).WithInterFont();
    }
}
