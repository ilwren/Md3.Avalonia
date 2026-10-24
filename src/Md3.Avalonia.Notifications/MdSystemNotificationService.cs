namespace Md3.Avalonia.Notifications;

/// <summary>
/// Displays native notifications using the current platform's notification surface.
/// 使用当前平台的通知界面显示原生通知。
/// </summary>
/// <remarks>
/// The platform may require host-app setup or runtime permission; see the package documentation.
/// 部分平台需要宿主应用配置或运行时权限，详见包文档。
/// </remarks>
public interface IMdSystemNotificationService
{
    /// <summary>
    /// Displays a notification using the current platform's native notification surface.
    /// 使用当前平台的原生通知界面显示通知。
    /// </summary>
    /// <param name="notification">The notification title and body. / 通知标题和正文。</param>
    /// <param name="cancellationToken">Cancels the request before it is submitted to the operating system. / 在请求提交给操作系统之前取消该请求。</param>
    Task ShowAsync(MdSystemNotification notification, CancellationToken cancellationToken = default);
}

/// <summary>Creates native system notifications on Windows, macOS, Linux and Android. / 在 Windows、macOS、Linux 和 Android 上创建并显示原生系统通知。</summary>
public sealed class MdSystemNotificationService : IMdSystemNotificationService
{
    private readonly IMdSystemNotificationBackend _backend;

    private MdSystemNotificationService(IMdSystemNotificationBackend backend) => _backend = backend;

    /// <summary>Creates the default backend for the current operating system. / 为当前操作系统创建默认的通知后端。</summary>
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

/// <summary>Base exception for a native system notification failure. / 表示原生系统通知操作失败的基类异常。</summary>
public class MdSystemNotificationException : Exception
{
    /// <summary>Creates a notification exception.</summary>
    public MdSystemNotificationException(string message) : base(message) { }

    /// <summary>Creates a notification exception with an underlying platform error.</summary>
    public MdSystemNotificationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>Raised when Android notification permission has not been granted. / 表示 Android 系统通知权限尚未授予的异常。</summary>
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
