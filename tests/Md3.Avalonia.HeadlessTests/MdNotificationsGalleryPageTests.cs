using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Pages;
using Md3.Avalonia.Notifications;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdNotificationsGalleryPageTests
{
    [AvaloniaFact]
    public void Notification_Page_Sends_The_Entered_Title_And_Message_Through_The_Service()
    {
        var service = new RecordingNotificationService();
        var page = new NotificationsGalleryPage(service);
        var window = new Window { Width = 640, Height = 720, Content = page };
        window.Show();
        try
        {
            page.FindControl<MdTextBox>("TitleInput")!.Text = "Build passed";
            page.FindControl<MdTextBox>("BodyInput")!.Text = "The gallery compiled successfully.";
            page.FindControl<MdButton>("SendButton")!
                .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.Equal("Build passed", service.LastNotification?.Title);
            Assert.Equal("The gallery compiled successfully.", service.LastNotification?.Body);
            Assert.Contains("request sent", page.FindControl<TextBlock>("StatusText")!.Text ?? string.Empty,
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Gallery_Index_Exposes_The_Notifications_Demo()
    {
        var window = new MainWindow { Width = 1280, Height = 860 };
        window.Show();
        try
        {
            Assert.Contains("Notifications", window.IndexedPageTitles);
            Assert.True(window.NavigateToIndexedPage("Notifications"));
        }
        finally
        {
            window.Close();
        }
    }

    private sealed class RecordingNotificationService : IMdSystemNotificationService
    {
        public MdSystemNotification? LastNotification { get; private set; }

        public Task ShowAsync(MdSystemNotification notification, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastNotification = notification;
            return Task.CompletedTask;
        }
    }
}
