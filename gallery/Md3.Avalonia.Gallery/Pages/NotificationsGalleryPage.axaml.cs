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
            StatusText.Text = L(
                "Notification request sent. Check the operating system's notification center.",
                "系统通知请求已提交，请检查操作系统的通知中心。");
        }
        catch (Exception exception)
        {
            // Platform requirements (permissions, desktop services and app identity) vary by host.
            // Keep the gallery responsive and expose the backend's actionable diagnostic here.
            var registrationError = GalleryNotificationIdentity.WindowsRegistrationError;
            var diagnostic = string.IsNullOrWhiteSpace(registrationError)
                ? exception.Message
                : $"{exception.Message} Windows AppUserModelID registration failed: {registrationError}";
            StatusText.Text = $"{L("Could not send the notification:", "无法发送系统通知：")} {diagnostic}";
        }
        finally
        {
            SendButton.IsEnabled = true;
        }
    }

    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);
}
