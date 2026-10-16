namespace Md3.Avalonia.Notifications;

/// <summary>Displays a native operating-system notification.</summary>
public interface IMdSystemNotificationService
{
    /// <summary>Displays a notification using the current platform's native notification surface.</summary>
    /// <param name="notification">The notification title and body.</param>
    /// <param name="cancellationToken">Cancels the request before it is submitted to the operating system.</param>
    Task ShowAsync(MdSystemNotification notification, CancellationToken cancellationToken = default);
}

/// <summary>Creates native system notifications on Windows, macOS, Linux and Android.</summary>
public sealed class MdSystemNotificationService : IMdSystemNotificationService
{
    private readonly IMdSystemNotificationBackend _backend;

    private MdSystemNotificationService(IMdSystemNotificationBackend backend) => _backend = backend;

    /// <summary>Creates the default backend for the current operating system.</summary>
    /// <param name="options">Optional app identity and platform settings.</param>
    public static MdSystemNotificationService CreateDefault(MdSystemNotificationOptions? options = null)
    {
        var resolvedOptions = options ?? new MdSystemNotificationOptions();
        resolvedOptions.Validate();
#if ANDROID
        return new MdSystemNotificationService(new AndroidSystemNotificationBackend(resolvedOptions));
#elif WINDOWS
        return new MdSystemNotificationService(new WindowsSystemNotificationBackend(resolvedOptions));
#else
        return new MdSystemNotificationService(DesktopSystemNotificationBackend.Create(resolvedOptions));
#endif
    }

    /// <inheritdoc />
    public async Task ShowAsync(MdSystemNotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        cancellationToken.ThrowIfCancellationRequested();
        await _backend.ShowAsync(notification, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Creates and displays a notification from a title and body.</summary>
    /// <param name="title">The notification title.</param>
    /// <param name="body">The notification body.</param>
    /// <param name="cancellationToken">Cancels the request before it is submitted to the operating system.</param>
    public Task ShowAsync(string title, string body, CancellationToken cancellationToken = default) =>
        ShowAsync(new MdSystemNotification(title, body), cancellationToken);
}

/// <summary>Base exception for a native system notification failure.</summary>
public class MdSystemNotificationException : Exception
{
    /// <summary>Creates a notification exception.</summary>
    public MdSystemNotificationException(string message) : base(message) { }

    /// <summary>Creates a notification exception with an underlying platform error.</summary>
    public MdSystemNotificationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>Raised when Android notification permission has not been granted.</summary>
public sealed class MdSystemNotificationPermissionException : MdSystemNotificationException
{
    /// <summary>Creates a permission exception.</summary>
    public MdSystemNotificationPermissionException(string message) : base(message) { }

    /// <summary>Creates a permission exception with an underlying platform error.</summary>
    public MdSystemNotificationPermissionException(string message, Exception innerException) : base(message, innerException) { }
}

internal interface IMdSystemNotificationBackend
{
    Task ShowAsync(MdSystemNotification notification, CancellationToken cancellationToken);
}
