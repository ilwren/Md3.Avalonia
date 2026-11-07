using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
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

    [AvaloniaFact]
    public async Task Android_Permission_Denial_Does_Not_Send_And_Retry_Can_Succeed()
    {
        var service = new RecordingNotificationService();
        var pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var permissionRequests = 0;
        var created = 0;
        var page = new NotificationsGalleryPage(
            () => { created++; return service; },
            () => { permissionRequests++; return pending.Task; });
        var window = new Window { Width = 640, Height = 720, Content = page };
        window.Show();
        try
        {
            var button = page.FindControl<MdButton>("SendButton")!;
            var send = page.SendNotificationAsync();
            Assert.False(button.IsEnabled);
            Assert.Null(service.LastNotification);
            Assert.Equal(0, created);
            pending.SetResult(false);
            await send;
            Assert.True(button.IsEnabled);
            Assert.Null(service.LastNotification);
            Assert.Contains("not permitted", page.FindControl<TextBlock>("StatusText")!.Text ?? "", StringComparison.OrdinalIgnoreCase);

            pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            send = page.SendNotificationAsync();
            pending.SetResult(true);
            await send;
            Assert.True(button.IsEnabled);
            Assert.Equal(2, permissionRequests);
            Assert.Equal(1, created);
            Assert.Equal("Build complete", service.LastNotification?.Title);
            Assert.Contains("request sent", page.FindControl<TextBlock>("StatusText")!.Text ?? "", StringComparison.OrdinalIgnoreCase);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public async Task Permission_Request_Failure_Is_Reported_And_Button_Is_Reenabled()
    {
        var service = new RecordingNotificationService();
        var page = new NotificationsGalleryPage(() => service,
            () => Task.FromException<bool>(new InvalidOperationException("permission request failed")));
        var window = new Window { Content = page };
        window.Show();
        try
        {
            await page.SendNotificationAsync();
            Assert.True(page.FindControl<MdButton>("SendButton")!.IsEnabled);
            Assert.Null(service.LastNotification);
            Assert.Contains("permission request failed", page.FindControl<TextBlock>("StatusText")!.Text ?? "");
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public async Task Android_Index_Search_Opens_The_Injected_Notification_Demo()
    {
        var service = new RecordingNotificationService();
        var shell = new AndroidGalleryView(() => service, () => Task.FromResult(true));
        var window = new Window { Width = 390, Height = 780, Content = shell };
        window.Show();
        try
        {
            shell.FindControl<MdSearchBar>("HomeSearch")!.Text = "Notifications";
            Dispatcher.UIThread.RunJobs();
            var results = shell.FindControl<StackPanel>("HomeSearchResults")!;
            var button = Assert.Single(results.Children.OfType<MdButton>(), b => Equals(b.Content, "Notifications"));
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal("Notifications", shell.CurrentPageTitle);
            var viewport = Assert.IsAssignableFrom<Control>(shell.FindControl<ContentControl>("MobilePageHost")!.Content);
            var page = viewport as NotificationsGalleryPage ?? Assert.Single(viewport.GetLogicalDescendants().OfType<NotificationsGalleryPage>());
            await page.SendNotificationAsync();
            Assert.Equal("Build complete", service.LastNotification?.Title);
        }
        finally { window.Close(); }
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
