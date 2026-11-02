using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>Regression coverage for the DI, MVVM, template, queue, and direct banner routes.</summary>
public sealed class MdBannerServiceTests
{
    private sealed record SyncBannerModel(string Message);

    private static MdBannerHost CreateHost(MdBannerService? service = null)
    {
        var host = new MdBannerHost
        {
            Content = new TextBlock { Text = "Page" },
            Service = service
        };
        host.DataTemplates.Add(new FuncDataTemplate<SyncBannerModel>((model, _) =>
            new MdBanner
            {
                Content = new TextBlock { Text = model!.Message },
                IsDismissible = true
            }));
        MdMotion.SetScheme(host, MdMotionScheme.None);
        return host;
    }

    [AvaloniaFact]
    public async Task Injected_Service_Resolves_A_Model_Template_And_Returns_A_Result()
    {
        var service = new MdBannerService();
        IMdBannerService viewModelFacing = service;
        var host = CreateHost(service);
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            var pending = viewModelFacing.ShowAsync(new SyncBannerModel("Could not sync"));
            Dispatcher.UIThread.RunJobs();

            Assert.True(host.IsOpen);
            Assert.True(viewModelFacing.IsOpen);
            var banner = Assert.Single(host.GetVisualDescendants().OfType<MdBanner>());
            Assert.True(banner.IsOpen);
            Assert.Equal("Could not sync", Assert.IsType<TextBlock>(banner.Content).Text);

            viewModelFacing.Close("retry");
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("retry", await pending);
            Assert.False(host.IsOpen);
            Assert.False(banner.IsOpen);
            Assert.False(viewModelFacing.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Requests_Are_Queued_And_Shown_One_At_A_Time()
    {
        var service = new MdBannerService();
        var host = CreateHost(service);
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            var first = service.ShowAsync(new SyncBannerModel("First"));
            var second = service.ShowAsync(new SyncBannerModel("Second"));
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("First", Assert.IsType<SyncBannerModel>(host.Banner).Message);
            Assert.False(second.IsCompleted);

            service.Close(1);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, await first);
            Assert.True(host.IsOpen);
            Assert.Equal("Second", Assert.IsType<SyncBannerModel>(host.Banner).Message);

            service.Close(2);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(2, await second);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Request_Made_Before_Host_Attaches_Waits_And_Cancellation_Closes_It()
    {
        var service = new MdBannerService();
        using var cancellation = new CancellationTokenSource();
        var pending = service.ShowAsync(new SyncBannerModel("Early"), cancellation.Token);
        Dispatcher.UIThread.RunJobs();
        Assert.False(pending.IsCompleted);

        var host = CreateHost(service);
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.True(host.IsOpen);
            Assert.Equal("Early", Assert.IsType<SyncBannerModel>(host.Banner).Message);

            cancellation.Cancel();
            Dispatcher.UIThread.RunJobs();
            Assert.Null(await pending);
            Assert.False(host.IsOpen);
            Assert.False(service.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Template_Banner_Dismiss_Action_Completes_The_Service_Request()
    {
        var service = new MdBannerService();
        var host = CreateHost(service);
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            var pending = service.ShowAsync(new SyncBannerModel("Dismiss me"));
            Dispatcher.UIThread.RunJobs();
            var banner = Assert.Single(host.GetVisualDescendants().OfType<MdBanner>());

            banner.Dismiss();
            Dispatcher.UIThread.RunJobs();

            Assert.Null(await pending);
            Assert.False(host.IsOpen);
            Assert.False(service.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Host_Direct_API_And_Two_Way_State_Work_Without_A_Shared_Service()
    {
        var host = CreateHost();
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            var pending = host.ShowAsync(new SyncBannerModel("Direct"));
            Dispatcher.UIThread.RunJobs();
            Assert.True(host.IsOpen);

            host.Close(true);
            Dispatcher.UIThread.RunJobs();
            Assert.True((bool)(await pending)!);

            host.Banner = new SyncBannerModel("Bound");
            host.SetCurrentValue(MdBannerHost.IsOpenProperty, true);
            Dispatcher.UIThread.RunJobs();
            Assert.True(Assert.Single(host.GetVisualDescendants().OfType<MdBanner>()).IsOpen);

            host.SetCurrentValue(MdBannerHost.IsOpenProperty, false);
            Dispatcher.UIThread.RunJobs();
            Assert.False(host.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }
}
