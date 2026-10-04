using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// The dialog surface has to be reachable from a view model that holds no control reference, from
/// view code-behind, and from a plain two-way binding — without those routes fighting each other.
/// </summary>
public class MdDialogServiceTests
{
    private sealed record ConfirmDialogModel(string Name);

    private static MdDialogHost CreateHost(MdDialogService? service = null)
    {
        var host = new MdDialogHost { Content = new TextBlock { Text = "Page" }, Service = service };
        host.DataTemplates.Add(new FuncDataTemplate<ConfirmDialogModel>((model, _) =>
            new MdDialog { Headline = $"Delete {model!.Name}?", Content = "Body" }));
        return host;
    }

    [AvaloniaFact]
    public async Task Service_Shows_A_View_Model_Dialog_Without_A_Control_Reference()
    {
        // What a view model is given: the interface, nothing from the visual tree.
        var service = new MdDialogService();
        IMdDialogService viewModelFacing = service;

        var host = CreateHost(service);
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            var pending = viewModelFacing.ShowAsync(new ConfirmDialogModel("Draft"));
            Dispatcher.UIThread.RunJobs();

            Assert.True(host.IsOpen);
            Assert.True(viewModelFacing.IsOpen);
            Assert.Equal("Delete Draft?", host.GetVisualDescendants().OfType<MdDialog>().Single().Headline);

            viewModelFacing.Close(true);
            Dispatcher.UIThread.RunJobs();

            Assert.True((bool)(await pending)!);
            Assert.False(host.IsOpen);
            Assert.False(viewModelFacing.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Service_Returns_A_Typed_Result()
    {
        var service = new MdDialogService();
        var host = CreateHost(service);
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            var pending = ((IMdDialogService)service).ShowAsync<bool>(new ConfirmDialogModel("Draft"));
            Dispatcher.UIThread.RunJobs();
            service.Close(true);
            Dispatcher.UIThread.RunJobs();
            Assert.True(await pending);

            // A dismissal carries no result, so the typed overload falls back to default.
            var dismissed = ((IMdDialogService)service).ShowAsync<bool>(new ConfirmDialogModel("Draft"));
            Dispatcher.UIThread.RunJobs();
            service.Close();
            Dispatcher.UIThread.RunJobs();
            Assert.False(await dismissed);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Request_Made_Before_A_Host_Exists_Waits_For_One()
    {
        // A view model may show a dialog from a startup command, before any view is loaded.
        var service = new MdDialogService();
        var pending = ((IMdDialogService)service).ShowAsync(new ConfirmDialogModel("Early"));
        Dispatcher.UIThread.RunJobs();
        Assert.False(pending.IsCompleted);

        var host = CreateHost(service);
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.True(host.IsOpen);
            Assert.Equal("Delete Early?", host.GetVisualDescendants().OfType<MdDialog>().Single().Headline);

            service.Close("late");
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("late", await pending);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Second_Request_Waits_Its_Turn_Instead_Of_Replacing_The_First()
    {
        var service = new MdDialogService();
        var host = CreateHost(service);
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            var first = ((IMdDialogService)service).ShowAsync(new ConfirmDialogModel("First"));
            var second = ((IMdDialogService)service).ShowAsync(new ConfirmDialogModel("Second"));
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("Delete First?", host.GetVisualDescendants().OfType<MdDialog>().Single().Headline);
            Assert.False(second.IsCompleted);

            service.Close(1);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, await first);

            // The queued one takes the surface over on its own.
            Assert.True(host.IsOpen);
            Assert.Equal("Delete Second?", host.GetVisualDescendants().OfType<MdDialog>().Single().Headline);

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
    public async Task Dismissing_With_Escape_Completes_The_View_Model_Request_With_Null()
    {
        var service = new MdDialogService();
        var host = CreateHost(service);
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            var pending = ((IMdDialogService)service).ShowAsync(new ConfirmDialogModel("Draft"));
            Dispatcher.UIThread.RunJobs();
            Assert.True(host.IsOpen);

            // Whatever route closes the surface has to release the awaiting view model.
            host.SetCurrentValue(MdDialogHost.IsOpenProperty, false);
            Dispatcher.UIThread.RunJobs();

            Assert.Null(await pending);
            Assert.False(((IMdDialogService)service).IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Cancelling_The_Token_Closes_The_Dialog()
    {
        var service = new MdDialogService();
        var host = CreateHost(service);
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            using var cancellation = new CancellationTokenSource();
            var pending = ((IMdDialogService)service).ShowAsync(new ConfirmDialogModel("Draft"), cancellation.Token);
            Dispatcher.UIThread.RunJobs();
            Assert.True(host.IsOpen);

            cancellation.Cancel();
            Dispatcher.UIThread.RunJobs();

            Assert.Null(await pending);
            Assert.False(host.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Host_Without_A_Service_Still_Works_From_Code_Behind()
    {
        // The existing direct API has to keep working with no service assigned at all.
        var host = CreateHost();
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            var model = new ConfirmDialogModel("Draft");
            var pending = host.ShowAsync(model);
            Dispatcher.UIThread.RunJobs();

            Assert.True(host.IsOpen);
            Assert.Same(model, host.Dialog);

            host.Close("accepted");
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("accepted", await pending);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Binding_Driven_IsOpen_Still_Opens_The_Surface_With_No_Request_Behind_It()
    {
        var host = CreateHost();
        host.Dialog = new ConfirmDialogModel("Bound");
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            host.SetCurrentValue(MdDialogHost.IsOpenProperty, true);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Delete Bound?", host.GetVisualDescendants().OfType<MdDialog>().Single().Headline);

            host.SetCurrentValue(MdDialogHost.IsOpenProperty, false);
            Dispatcher.UIThread.RunJobs();
            Assert.False(host.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task A_Service_Request_Waits_While_A_Bound_Dialog_Holds_The_Surface()
    {
        var service = new MdDialogService();
        var host = CreateHost(service);
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            host.Dialog = new ConfirmDialogModel("Bound");
            host.SetCurrentValue(MdDialogHost.IsOpenProperty, true);
            Dispatcher.UIThread.RunJobs();

            var pending = ((IMdDialogService)service).ShowAsync(new ConfirmDialogModel("Queued"));
            Dispatcher.UIThread.RunJobs();

            // The binding owns the surface; the service must not steal it.
            Assert.Equal("Delete Bound?", host.GetVisualDescendants().OfType<MdDialog>().Single().Headline);
            Assert.False(pending.IsCompleted);

            host.SetCurrentValue(MdDialogHost.IsOpenProperty, false);
            Dispatcher.UIThread.RunJobs();

            Assert.True(host.IsOpen);
            Assert.Equal("Delete Queued?", host.GetVisualDescendants().OfType<MdDialog>().Single().Headline);

            service.Close("done");
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("done", await pending);
        }
        finally
        {
            window.Close();
        }
    }
}
