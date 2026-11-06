using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Motion;
using Md3.Avalonia.Themes.Dynamic;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Regression tripwires for UI responsiveness: each test warms up once, takes the median of a
/// few runs, and asserts it stays under a deliberately generous ceiling. Shared CI runners are
/// noisy, so these are not SLAs — they exist to catch 10x regressions (a synchronous wait on
/// the UI thread, an accidental O(n^2) in theme application, broken list virtualization).
/// Absolute timings, headless-vs-device comparisons and per-operation allocations are measured
/// with the BenchmarkDotNet project in tests/Md3.Avalonia.Benchmarks and the profilers listed
/// in docs/PERFORMANCE.md.
/// </summary>
public sealed class MdPerformanceBudgetTests
{
    private const int Samples = 5;

    private sealed record ConfirmDialogModel(string Name);

    [Fact]
    public void Theme_Generation_Stays_Within_Budget()
    {
        var options = new MdThemeOptions
        {
            SeedColor = "#006A6A",
            SchemeVariant = MdThemeSchemeVariant.Expressive,
            ContrastLevel = MdThemeContrastLevel.High
        };

        // Warms MaterialColorUtilities statics and the HCT tables before measuring.
        Assert.Equal(49, MdThemeGenerator.Generate(options, dark: false).Count);

        var median = MeasureMedianMs(() => { MdThemeGenerator.Generate(options, dark: false); });
        Assert.True(median < 2_000, $"median HCT scheme generation took {median:0}ms, budget 2000ms.");
    }

    [Fact]
    public void Full_Scheme_Matrix_Generation_Stays_Within_Budget()
    {
        // The Theme Lab and gallery seed picker can sweep every scheme x contrast x mode.
        var variants = Enum.GetValues<MdThemeSchemeVariant>();
        var contrasts = Enum.GetValues<MdThemeContrastLevel>();

        var median = MeasureMedianMs(() =>
        {
            foreach (var dark in new[] { false, true })
            foreach (var variant in variants)
            foreach (var contrast in contrasts)
                MdThemeGenerator.Generate(new MdThemeOptions
                {
                    SeedColor = "#006A6A",
                    SchemeVariant = variant,
                    ContrastLevel = contrast
                }, dark);
        }, samples: 3);

        Assert.True(median < 10_000,
            $"median full-matrix generation (6 variants x 3 contrasts x light/dark) took {median:0}ms, budget 10000ms.");
    }

    [AvaloniaFact]
    public void Icon_Lookup_Stays_Within_Budget()
    {
        // The alias dictionary builds lazily and the first lookup also drives font registration
        // through EnsureConfigured, so warm up before measuring the steady state. Only the
        // catalog size is asserted: whether a name resolves to a glyph depends on the host font
        // provider, not on lookup speed.
        for (var i = 0; i < 100; i++) MdSymbols.TryGetByName("home", out _);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(4284, MdSymbols.Count);

        var median = MeasureMedianMs(() =>
        {
            for (var i = 0; i < 1_000; i++) MdSymbols.TryGetByName("home", out _);
        });
        Assert.True(median < 2_000, $"median 1000 icon lookups took {median:0}ms, budget 2000ms.");
    }

    [AvaloniaFact]
    public void First_Window_Render_Stays_Within_Budget()
    {
        ShowAndCloseGalleryRow(); // Warms template expansion, styling and the Skia pipelines.

        var median = MeasureMedianMs(() => ShowAndCloseGalleryRow(), samples: 3);
        Assert.True(median < 5_000, $"median first render took {median:0}ms, budget 5000ms.");
    }

    [AvaloniaFact]
    public void Dialog_Open_Stays_Within_Budget()
    {
        var service = new MdDialogService();
        var host = new MdDialogHost
        {
            Content = new TextBlock { Text = "Page" },
            Service = service
        };
        host.DataTemplates.Add(new FuncDataTemplate<ConfirmDialogModel>((model, _) =>
            new MdDialog { Headline = $"Delete {model!.Name}?", Content = "Body" }));
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            OpenAndCloseDialog(service, host); // Warms template expansion for the dialog surface.

            var median = MeasureMedianMs(() => OpenAndCloseDialog(service, host), samples: 3);
            Assert.True(median < 3_000, $"median dialog open+close took {median:0}ms, budget 3000ms.");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Snackbar_Show_Stays_Within_Budget()
    {
        var service = new MdSnackbarService();
        var host = new MdSnackbarHost
        {
            Service = service,
            Content = new TextBlock { Text = "Page" }
        };
        MdMotion.SetScheme(host, MdMotionScheme.None);
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            ShowAndDismissSnackbar(service); // Warms the snackbar template and queue pump.

            var median = MeasureMedianMs(() => ShowAndDismissSnackbar(service), samples: 3);
            Assert.True(median < 3_000, $"median snackbar show+dismiss took {median:0}ms, budget 3000ms.");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Theme_Switch_Stays_Within_Budget()
    {
        var application = Assert.IsAssignableFrom<Application>(Application.Current);
        var previous = MdThemeManager.Current;
        var options = new MdThemeOptions
        {
            SeedColor = "#006A6A",
            SchemeVariant = MdThemeSchemeVariant.Expressive
        };
        try
        {
            MdThemeManager.Apply(application, options, dark: false);
            Dispatcher.UIThread.RunJobs();

            var median = MeasureMedianMs(() =>
            {
                var roles = MdThemeManager.Apply(application, options, dark: false);
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(49, roles.Count);
            }, samples: 3);
            Assert.True(median < 3_000, $"median theme switch took {median:0}ms, budget 3000ms.");
        }
        finally
        {
            MdThemeManager.Apply(application, previous, dark: false);
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void Large_Virtualized_List_First_Layout_Stays_Within_Budget()
    {
        var list = new MdList { ItemsSource = Enumerable.Range(0, 10_000).Select(i => $"row {i}").ToArray() };
        var window = new Window { Width = 800, Height = 600, Content = list };
        window.Show();
        try
        {
            var elapsed = MeasureMs(() => Dispatcher.UIThread.RunJobs());

            var realized = list.GetVisualDescendants().OfType<ListBoxItem>().Count();
            Assert.True(realized < 500, $"virtualization must hold: {realized} containers realized for 10000 rows.");
            Assert.True(elapsed < 10_000, $"10k-row list first layout took {elapsed:0}ms, budget 10000ms.");
        }
        finally
        {
            window.Close();
        }
    }

    private static void ShowAndCloseGalleryRow()
    {
        var button = new MdButton { Content = "Continue" };
        var panel = new StackPanel { Spacing = 16 };
        panel.Children.Add(button);
        panel.Children.Add(new MdTextBox { Label = "Project name" });
        panel.Children.Add(new MdCheckBox { Content = "Remember" });

        var window = new Window { Width = 800, Height = 600, Content = panel };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.True(button.IsMeasureValid, "the first layout pass must complete after Show + RunJobs.");
        }
        finally
        {
            window.Close();
        }
    }

    private static void OpenAndCloseDialog(MdDialogService service, MdDialogHost host)
    {
        var pending = ((IMdDialogService)service).ShowAsync(new ConfirmDialogModel("Draft"));
        Dispatcher.UIThread.RunJobs();
        Assert.True(host.IsOpen, "the dialog must open after ShowAsync + RunJobs.");
        service.Close(true);
        Dispatcher.UIThread.RunJobs();
        Assert.True(pending.IsCompleted, "closing must complete the dialog task without extra pumps.");
    }

    private static void ShowAndDismissSnackbar(MdSnackbarService service)
    {
        var pending = service.ShowAsync(new MdSnackbarMessage("Saved") { Duration = TimeSpan.Zero });
        Dispatcher.UIThread.RunJobs();
        service.Dismiss();
        Dispatcher.UIThread.RunJobs();
        Assert.True(pending.IsCompleted, "dismissing must complete the snackbar task without extra pumps.");
    }

    private static double MeasureMedianMs(Action action, int samples = Samples)
    {
        var timings = new double[samples];
        for (var i = 0; i < samples; i++)
            timings[i] = MeasureMs(action);
        Array.Sort(timings);
        return timings[samples / 2];
    }

    private static double MeasureMs(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
    }
}
