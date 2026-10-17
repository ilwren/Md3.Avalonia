using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Notifications;

namespace Md3.Avalonia.Gallery.Pages;

public partial class NotificationsGalleryPage : UserControl
{
    private IMdSystemNotificationService? _notificationService;

    public NotificationsGalleryPage() : this(null) { }

    public NotificationsGalleryPage(IMdSystemNotificationService? notificationService)
    {
        _notificationService = notificationService;
        InitializeComponent();
        PlatformStatusText.Text = GetPlatformStatus();
        AutomationProperties.SetLiveSetting(StatusText, AutomationLiveSetting.Polite);
    }

    private async void SendNotification(object? sender, RoutedEventArgs e)
    {
        var title = TitleInput.Text?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            StatusText.Text = L("Enter a notification title.", "请输入通知标题。");
            TitleInput.Focus();
            return;
        }

        SendButton.IsEnabled = false;
        StatusText.Text = L("Sending notification...", "正在发送通知……");
        try
        {
            _notificationService ??= MdSystemNotificationService.CreateDefault(new MdSystemNotificationOptions
            {
                ApplicationName = "Material Design 3 Gallery",
                WindowsAppUserModelId = GalleryNotificationIdentity.WindowsAppUserModelId
            });
            await _notificationService.ShowAsync(new MdSystemNotification(title, BodyInput.Text ?? string.Empty));
            var registrationError = GalleryNotificationIdentity.WindowsRegistrationError;
            var status = L(
                "Notification request sent to the operating system. Native notifications appear outside the Gallery window; check the operating system's notification center.",
                "系统通知请求已提交。通知会显示在 Gallery 窗口之外；请检查操作系统的通知中心。");
            if (!string.IsNullOrWhiteSpace(registrationError))
                status += $" {L("Windows app registration failed:", "Windows 应用注册失败：")} {registrationError}";
            StatusText.Text = status;
        }
        catch (Exception exception)
        {
            // Platform requirements (permissions, desktop services and app identity) vary by host.
            // Keep the gallery responsive and expose the backend's actionable diagnostic here.
            var registrationError = GalleryNotificationIdentity.WindowsRegistrationError;
            var detail = exception is MdSystemNotificationException { InnerException: { } innerException }
                ? $"{exception.Message} [{innerException.GetType().Name}, HRESULT 0x{innerException.HResult:X8}: {innerException.Message}]"
                : exception.Message;
            var diagnostic = string.IsNullOrWhiteSpace(registrationError)
                ? detail
                : $"{detail} {L("Windows AppUserModelID registration failed:", "Windows AppUserModelID 注册失败：")} {registrationError}";
            StatusText.Text = $"{L("Could not send the notification:", "无法发送系统通知：")} {diagnostic}";
        }
        finally
        {
            SendButton.IsEnabled = true;
        }
    }

    private static string GetPlatformStatus()
    {
        if (OperatingSystem.IsWindows())
        {
            var registrationError = GalleryNotificationIdentity.WindowsRegistrationError;
            if (!string.IsNullOrWhiteSpace(registrationError))
            {
                return $"{L("Windows toast registration failed at launch:", "Windows Toast 启动注册失败：")} {registrationError}";
            }

            if (GalleryNotificationIdentity.WindowsRegistrationSucceeded)
            {
                return L(
                    $"Windows toast identity is ready ({GalleryNotificationIdentity.WindowsAppUserModelId}); Start-menu shortcut: {GalleryNotificationIdentity.WindowsShortcutPath}. Toasts appear outside this Gallery window; press Win+N to check Notification Center.",
                    $"Windows Toast 身份已就绪（{GalleryNotificationIdentity.WindowsAppUserModelId}）；开始菜单快捷方式：{GalleryNotificationIdentity.WindowsShortcutPath}。通知会显示在 Gallery 窗口之外；按 Win+N 打开通知中心。");
            }

            return L(
                "Windows toast registration did not run. Launch the Windows-specific Desktop Gallery build to test native notifications.",
                "Windows Toast 注册未运行。请启动 Windows 专用 Desktop Gallery 构建以测试系统通知。");
        }

        return L(
            "Native notifications are shown by the host operating system, outside this Gallery window.",
            "系统通知由宿主操作系统显示在 Gallery 窗口之外。");
    }

    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);
}
