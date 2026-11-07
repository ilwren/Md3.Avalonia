using Md3.Avalonia.Notifications;

namespace Md3.Avalonia.Gallery;

/// <summary>Optional Android-host services, installed before the single-view Gallery is created.</summary>
public static class GalleryPlatformIntegration
{
    /// <summary>Creates the host's Android-specific notification service, including its small icon.</summary>
    public static Func<IMdSystemNotificationService>? CreateAndroidNotificationService { get; set; }

    /// <summary>Requests notification permission from the current foreground activity when required.</summary>
    public static Func<Task<bool>>? RequestAndroidNotificationPermission { get; set; }
}
