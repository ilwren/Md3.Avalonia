using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Collectability gates, following Avalonia's own LeakTests recipe: attach the control to a
/// live window, exercise it, detach it with a layout pass, then collect and assert. Two
/// adaptations for this suite's real Skia rendering, which upstream's mock-platform tests never
/// exercise: each factory forces a fresh rendered frame after detach (the headless renderer
/// holds the last committed scene until a new one replaces it), and collection is retried with
/// wall-clock delays because the render timer and compositor batches release on their own
/// cadence, not the dispatcher's. The window itself is intentionally never asserted — even
/// Avalonia's suite only ever asserts content controls, because the shared headless platform
/// keeps closed windows rooted. What these gates catch is controls pinned by anything other
/// than the visual tree or the last frame: a service holding its host strongly, a static event
/// subscription, a binding that outlives its target.
/// </summary>
public sealed class MdMemoryLeakTests
{
    private sealed record LeakDialogModel(string Name);

    [AvaloniaFact]
    public async Task Detached_Material_Content_Is_Collected()
    {
        if (!ReleaseOnlyGate()) return;

        var window = new Window { Width = 640, Height = 480 };
        window.Show();
        try
        {
            var weak = AttachAndDetachContent(window);
            Assert.True(weak.IsAlive, "the content must be attached before collection.");
            await CollectUntilCollectedAsync(weak);
            Assert.False(weak.IsAlive, "detached content must be collectable once fresh frames replace it.");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Dialog_Open_Close_Cycle_Does_Not_Leak_Host()
    {
        if (!ReleaseOnlyGate()) return;

        // The service deliberately stays alive for the assertion: it must never pin its host.
        var service = new MdDialogService();
        var window = new Window { Width = 640, Height = 480 };
        window.Show();
        try
        {
            var weak = AttachExerciseAndDetachDialog(window, service);
            Assert.True(weak.IsAlive, "the host must be attached before collection.");
            await CollectUntilCollectedAsync(weak);
            Assert.False(weak.IsAlive, "the detached dialog host must be collectable.");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Snackbar_Show_Dismiss_Cycle_Does_Not_Leak_Host()
    {
        if (!ReleaseOnlyGate()) return;

        var service = new MdSnackbarService();
        var window = new Window { Width = 640, Height = 480 };
        window.Show();
        try
        {
            var weak = AttachExerciseAndDetachSnackbar(window, service);
            Assert.True(weak.IsAlive, "the host must be attached before collection.");
            await CollectUntilCollectedAsync(weak);
            Assert.False(weak.IsAlive, "the detached snackbar host must be collectable.");
        }
        finally
        {
            window.Close();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AttachAndDetachContent(Window window)
    {
        var button = new MdButton { Content = "Hi" };
        window.Content = button;
        Dispatcher.UIThread.RunJobs();
        window.Content = null;
        Dispatcher.UIThread.RunJobs();
        ReplaceLastFrame(window);
        return new WeakReference(button);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AttachExerciseAndDetachDialog(Window window, MdDialogService service)
    {
        var host = new MdDialogHost
        {
            Content = new TextBlock { Text = "Page" },
            Service = service
        };
        MdMotion.SetScheme(host, MdMotionScheme.None);
        host.DataTemplates.Add(new FuncDataTemplate<LeakDialogModel>((model, _) =>
            new MdDialog { Headline = $"Delete {model!.Name}?", Content = "Body" }));
        window.Content = host;
        Dispatcher.UIThread.RunJobs();
        var pending = ((IMdDialogService)service).ShowAsync(new LeakDialogModel("Draft"));
        Dispatcher.UIThread.RunJobs();
        service.Close(true);
        Dispatcher.UIThread.RunJobs();
        // The modal controller moved keyboard focus inside the dialog and nothing was focused
        // before it, so focus would otherwise stay on a detached element and pin the host
        // through the live window's focus manager. A live window always has focus somewhere.
        window.Focus();
        Dispatcher.UIThread.RunJobs();
        window.Content = null;
        Dispatcher.UIThread.RunJobs();
        ReplaceLastFrame(window);
        return new WeakReference(host);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AttachExerciseAndDetachSnackbar(Window window, MdSnackbarService service)
    {
        var host = new MdSnackbarHost
        {
            Service = service,
            Content = new TextBlock { Text = "Page" }
        };
        MdMotion.SetScheme(host, MdMotionScheme.None);
        window.Content = host;
        Dispatcher.UIThread.RunJobs();
        service.Show(new MdSnackbarMessage("Saved") { Duration = TimeSpan.Zero });
        Dispatcher.UIThread.RunJobs();
        service.Dismiss();
        Dispatcher.UIThread.RunJobs();
        window.Content = null;
        Dispatcher.UIThread.RunJobs();
        ReplaceLastFrame(window);
        return new WeakReference(host);
    }

    private static void ReplaceLastFrame(Window window)
    {
        using (window.CaptureRenderedFrame()) { }
        Dispatcher.UIThread.RunJobs();
    }

    private static async Task CollectUntilCollectedAsync(WeakReference target)
    {
        for (var i = 0; i < 10 && target.IsAlive; i++)
        {
            CollectGarbage();
            await Task.Delay(50);
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void CollectGarbage()
    {
        // Avalonia's own leak-test recipe: flush Loaded work (which can hold control
        // references) before each collection.
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);
        GC.Collect();
        Dispatcher.UIThread.RunJobs();
        GC.Collect();
    }

    /// <summary>
    /// Lifetime assertions only hold under Release JIT rules (Avalonia gates its own leak
    /// tests the same way with ReleaseFact); CI runs this suite in Release.
    /// </summary>
    private static bool ReleaseOnlyGate()
    {
#if DEBUG
        return false;
#else
        return true;
#endif
    }
}
