using Md3.Avalonia.Notifications;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdSystemNotificationTests
{
    [Fact]
    public void Notification_Requires_A_Title_And_Allows_An_Empty_Body()
    {
        Assert.Throws<ArgumentException>(() => new MdSystemNotification(" ", "Body"));
        Assert.Throws<ArgumentNullException>(() => new MdSystemNotification("Title", null!));
        Assert.Equal(string.Empty, new MdSystemNotification("Title", string.Empty).Body);
    }

    [Fact]
    public void Default_Service_Respects_The_Current_Target_Framework()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Throws<PlatformNotSupportedException>(() => MdSystemNotificationService.CreateDefault());
            return;
        }

        var service = MdSystemNotificationService.CreateDefault();

        Assert.NotNull(service);
        Assert.False(string.IsNullOrWhiteSpace(new MdSystemNotificationOptions().ApplicationName));
    }

    [Fact]
    public void Android_Small_Icon_Resource_Must_Be_Positive_When_Provided()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MdSystemNotificationService.CreateDefault(
            new MdSystemNotificationOptions { AndroidSmallIconResourceId = 0 }));
    }

    [Fact]
    public async Task A_Cancelled_Request_Does_Not_Start_A_Platform_Notification()
    {
        if (OperatingSystem.IsWindows())
            Assert.Skip("This headless test uses the net10.0 asset; Windows notifications require the Windows-specific target.");

        var service = MdSystemNotificationService.CreateDefault();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            service.ShowAsync(new MdSystemNotification("Title", "Body"), cancellation.Token));
    }
}
