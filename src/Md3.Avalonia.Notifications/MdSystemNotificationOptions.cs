using System.IO;

namespace Md3.Avalonia.Notifications;

/// <summary>Host and platform configuration for <see cref="MdSystemNotificationService"/>.</summary>
public sealed class MdSystemNotificationOptions
{
    /// <summary>Gets or sets the app name shown by the notification service when the platform supports it.</summary>
    public string ApplicationName { get; init; } =
        Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName) ??
        Path.GetFileNameWithoutExtension(Environment.ProcessPath) ?? "Application";

    /// <summary>
    /// Gets or sets the registered Windows AppUserModelID used to attribute desktop toast notifications.
    /// Classic desktop apps must register this ID with Windows before showing a toast.
    /// </summary>
    public string? WindowsAppUserModelId { get; init; }

    /// <summary>Gets or sets the Android notification channel ID. The default is <c>md3.notifications</c>.</summary>
    public string AndroidChannelId { get; init; } = "md3.notifications";

    /// <summary>Gets or sets the user-visible Android notification channel name.</summary>
    public string AndroidChannelName { get; init; } = "Notifications";

    /// <summary>Gets or sets the Android drawable resource ID used as the notification's small icon.</summary>
    /// <remarks>When unset, the host application's icon is used, then a platform information icon as fallback.</remarks>
    public int? AndroidSmallIconResourceId { get; init; }

    internal void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ApplicationName);
        ArgumentException.ThrowIfNullOrWhiteSpace(AndroidChannelId);
        ArgumentException.ThrowIfNullOrWhiteSpace(AndroidChannelName);
        if (AndroidSmallIconResourceId is <= 0)
            throw new ArgumentOutOfRangeException(nameof(AndroidSmallIconResourceId), "The Android small icon resource ID must be greater than zero.");
    }
}
