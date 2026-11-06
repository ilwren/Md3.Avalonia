using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Collectability gates: windows, dialog hosts and snackbar hosts must be garbage-collectable
/// after close. Each test builds its UI inside a non-inlined factory so no test-frame local can
/// root it, then pumps the dispatcher and forces full collections before asserting. The dialog
/// and snackbar services intentionally hold their hosts through <see cref="WeakReference{T}"/>,
/// so an open/close cycle must not pin the host either.
/// </summary>
public sealed class MdMemoryLeakTests
{
    private sealed record LeakDialogModel(string Name);

    [AvaloniaFact]
    public void Closed_Window_Is_Collected()
    {
        var weak = CreateAndCloseWindow();
        CollectGarbage();
        Assert.False(weak.TryGetTarget(out _), "a closed window must be collectable.");
    }

    [AvaloniaFact]
    public void Dialog_Open_Close_Cycle_Does_Not_Leak_Host()
    {
        var weak = OpenAndCloseDialog();
        CollectGarbage();
        Assert.False(weak.TryGetTarget(out _), "the dialog host must be collectable after the cycle.");
    }

    [AvaloniaFact]
    public void Snackbar_Show_Dismiss_Cycle_Does_Not_Leak_Host()
    {
        var weak = ShowAndDismissSnackbar();
        CollectGarbage();
        Assert.False(weak.TryGetTarget(out _), "the snackbar host must be collectable after the cycle.");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Window> CreateAndCloseWindow()
    {
        var window = new Window
        {
            Width = 640,
            Height = 480,
            Content = new MdButton { Content = "Hi" }
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.Close();
        Dispatcher.UIThread.RunJobs();
        return new WeakReference<Window>(window);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<MdDialogHost> OpenAndCloseDialog()
    {
        var service = new MdDialogService();
        var host = new MdDialogHost
        {
            Content = new TextBlock { Text = "Page" },
            Service = service
        };
        host.DataTemplates.Add(new FuncDataTemplate<LeakDialogModel>((model, _) =>
            new MdDialog { Headline = $"Delete {model!.Name}?", Content = "Body" }));
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        var pending = ((IMdDialogService)service).ShowAsync(new LeakDialogModel("Draft"));
        Dispatcher.UIThread.RunJobs();
        service.Close(true);
        Dispatcher.UIThread.RunJobs();
        window.Close();
        Dispatcher.UIThread.RunJobs();
        return new WeakReference<MdDialogHost>(host);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<MdSnackbarHost> ShowAndDismissSnackbar()
    {
        var service = new MdSnackbarService();
        var host = new MdSnackbarHost
        {
            Service = service,
            Content = new TextBlock { Text = "Page" }
        };
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        service.Show(new MdSnackbarMessage("Saved") { Duration = TimeSpan.Zero });
        Dispatcher.UIThread.RunJobs();
        service.Dismiss();
        Dispatcher.UIThread.RunJobs();
        window.Close();
        Dispatcher.UIThread.RunJobs();
        return new WeakReference<MdSnackbarHost>(host);
    }

    private static void CollectGarbage()
    {
        for (var i = 0; i < 3; i++)
        {
            Dispatcher.UIThread.RunJobs();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
        }
        Dispatcher.UIThread.RunJobs();
    }
}
