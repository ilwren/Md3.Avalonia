using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Collectability gates, following Avalonia's own LeakTests recipe: attach the control to a
/// live window, exercise it, detach it with a layout pass, then collect and assert. The window
/// itself is intentionally never asserted — even Avalonia's suite only ever asserts content
/// controls, because the shared headless platform keeps closed windows rooted. What these gates
/// catch is controls pinned by anything other than the visual tree: a service holding its host
/// strongly, a static event subscription, a binding that outlives its target.
/// </summary>
public sealed class MdMemoryLeakTests
{
    private sealed record LeakDialogModel(string Name);

    [AvaloniaFact]
    public void Detached_Material_Content_Is_Collected()
    {
        if (!ReleaseOnlyGate()) return;

        var window = new Window { Width = 640, Height = 480 };
        window.Show();
        try
        {
            var weak = AttachAndDetachContent(window);
            Assert.True(weak.TryGetTarget(out _), "the content must be attached before collection.");
            CollectGarbage();
            Assert.False(weak.TryGetTarget(out _), "detached content must be collectable.");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Dialog_Open_Close_Cycle_Does_Not_Leak_Host()
    {
        if (!ReleaseOnlyGate()) return;

        // The service deliberately stays alive for the assertion: it must never pin its host.
        var service = new MdDialogService();
        var window = new Window { Width = 640, Height = 480 };
        window.Show();
        try
        {
            var weak = AttachExerciseAndDetachDialog(window, service);
            Assert.True(weak.TryGetTarget(out _), "the host must be attached before collection.");
            CollectGarbage();
            Assert.False(weak.TryGetTarget(out _), "the detached dialog host must be collectable.");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Snackbar_Show_Dismiss_Cycle_Does_Not_Leak_Host()
    {
        if (!ReleaseOnlyGate()) return;

        var service = new MdSnackbarService();
        var window = new Window { Width = 640, Height = 480 };
        window.Show();
        try
        {
            var weak = AttachExerciseAndDetachSnackbar(window, service);
            Assert.True(weak.TryGetTarget(out _), "the host must be attached before collection.");
            CollectGarbage();
            Assert.False(weak.TryGetTarget(out _), "the detached snackbar host must be collectable.");
        }
        finally
        {
            window.Close();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<MdButton> AttachAndDetachContent(Window window)
    {
        var button = new MdButton { Content = "Hi" };
        window.Content = button;
        Dispatcher.UIThread.RunJobs();
        window.Content = null;
        Dispatcher.UIThread.RunJobs();
        return new WeakReference<MdButton>(button);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<MdDialogHost> AttachExerciseAndDetachDialog(Window window, MdDialogService service)
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
        window.Content = null;
        Dispatcher.UIThread.RunJobs();
        return new WeakReference<MdDialogHost>(host);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<MdSnackbarHost> AttachExerciseAndDetachSnackbar(Window window, MdSnackbarService service)
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
        return new WeakReference<MdSnackbarHost>(host);
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
