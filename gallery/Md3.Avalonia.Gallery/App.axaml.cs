using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using System.Linq;
using System.Threading.Tasks;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LiveChartsCore;
using Md3.Avalonia.Themes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Md3.Avalonia.Gallery.Pages;

namespace Md3.Avalonia.Gallery;

public partial class App : Application
{
    private static bool _quitting;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        // LiveCharts resolves every chart's look from one theme held by its global settings - there
        // is no per-control style seam - so the gallery configures it once here, before the first
        // chart view exists. The chart page itself sets no paints at all.
        LiveCharts.Configure(settings => settings.AddMaterialTheme());

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow();
            desktop.MainWindow = window;
            ConfigureNativeAotSmoke(desktop, window);
            InstallTrayIcon(desktop);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new AndroidGalleryView(
                GalleryPlatformIntegration.CreateAndroidNotificationService,
                GalleryPlatformIntegration.RequestAndroidNotificationPermission);
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Runs a scenario inside the published native executable when requested by the AOT probe.
    /// Success is communicated out-of-process through a marker because merely keeping the Gallery
    /// window alive would not prove that the requested page, template and interaction path ran.
    /// </summary>
    private static void ConfigureNativeAotSmoke(
        IClassicDesktopStyleApplicationLifetime desktop,
        MainWindow window)
    {
        var scenario = Environment.GetEnvironmentVariable("MD3_GALLERY_AOT_SMOKE_SCENARIO");
        if (string.IsNullOrEmpty(scenario)) return;

        var markerPath = Environment.GetEnvironmentVariable("MD3_GALLERY_AOT_SMOKE_MARKER");
        if (scenario is not ("file-system-picker" or "emoji-picker" or "slide-confirm" or "dedicated-controls") || string.IsNullOrWhiteSpace(markerPath))
            throw new InvalidOperationException("The native-AOT smoke scenario or marker path is invalid.");

        window.Opened += async (_, _) =>
        {
            try
            {
                if (scenario == "file-system-picker")
                {
                    if (!window.NavigateToIndexedPage("File-system picker") ||
                        window.CurrentPage is not FileSystemPickerGalleryPage pickerPage)
                        throw new InvalidOperationException("The native-AOT Gallery could not open the file-system picker page.");
                    await pickerPage.RunNativeAotSmokeAsync();
                }
                else if (scenario == "dedicated-controls")
                {
                    await RunDedicatedControlsNativeAotSmokeAsync(window);
                }
                else if (scenario == "slide-confirm")
                {
                    if (!window.NavigateToIndexedPage("Dismissible") || window.CurrentPage is not DismissibleGalleryPage slidePage)
                        throw new InvalidOperationException("The native-AOT Gallery could not open the slide confirmation page.");
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        window.UpdateLayout();
                        slidePage.RunSlideNativeAotSmoke();
                    }, DispatcherPriority.Loaded);
                }
                else
                {
                    if (!window.NavigateToIndexedPage("Color picker") || window.CurrentPage is not ColorPickerGalleryPage colorPage)
                        throw new InvalidOperationException("The native-AOT Gallery could not open the color picker page.");
                    // Let the shell attach its new page before exercising real template parts.
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        window.UpdateLayout();
                        colorPage.RunEmojiNativeAotSmoke();
                    }, DispatcherPriority.Loaded);
                }
                File.WriteAllText(markerPath, $"{scenario}: passed{Environment.NewLine}");
                Trace.TraceInformation("Native-AOT smoke scenario passed: {0}", scenario);
            }
            catch (Exception exception)
            {
                Trace.TraceError("Native-AOT smoke scenario failed: {0}", exception);
                try
                {
                    File.WriteAllText(markerPath + ".failed", exception.ToString());
                }
                catch
                {
                    // The process exit still makes the probe fail if the diagnostic cannot be written.
                }
                desktop.Shutdown();
            }
        };
    }

    // Exercise the actual dedicated index entries in a published native process. Keeping this
    // separate from the older color/dismissible smoke catches trimming and navigation regressions
    // specific to these pages, including the type-based language switch.
    internal static async Task RunDedicatedControlsNativeAotSmokeAsync(MainWindow window)
    {
        await CheckDedicatedPage<AutoSizeTextDemoPage, MdAutoSizeText>(window, "Auto-size text");
        await CheckDedicatedPage<ConfettiDemoPage, MdConfetti>(window, "Confetti");
        await CheckDedicatedPage<EmojiPickerDemoPage, MdEmojiPicker>(window, "Emoji picker");
        await CheckDedicatedPage<ExpandableTextDemoPage, MdExpandableText>(window, "Expandable text");
        await CheckDedicatedPage<MarqueeDemoPage, MdMarquee>(window, "Marquee");
        await CheckDedicatedPage<MediaProgressDemoPage, MdMediaProgressBar>(window, "Media progress bar");
        await CheckDedicatedPage<PageIndicatorDemoPage, MdPageIndicator>(window, "Page indicator");
        await CheckDedicatedPage<SlideToConfirmDemoPage, MdSlideToConfirm>(window, "Slide to confirm");
        await CheckDedicatedPage<ScrollViewerGalleryPage, MdScrollBar>(window, "Scrolling surface");
        await CheckDedicatedPage<PickerGalleryPage, MdTimeDial>(window, "Date and time pickers");
        await CheckDedicatedPage<PaginatedDataTableGalleryPage, MdPaginatedDataTable>(window, "Paginated table");
        await CheckDedicatedPage<WheelPickerDemoPage, MdWheelPicker>(window, "Wheel picker");

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var selector = window.FindControl<ComboBox>("LanguageSelector") ?? throw new InvalidOperationException("Language selector was not found.");
            selector.SelectedIndex = 1;
            if (window.CurrentPage is not WheelPickerDemoPage page ||
                !page.GetLogicalDescendants().OfType<MdWheelPicker>().Any())
                throw new InvalidOperationException("The dedicated page could not be recreated after switching language.");
            selector.SelectedIndex = 0;
        }, DispatcherPriority.Loaded);
    }

    private static async Task CheckDedicatedPage<TPage, TControl>(MainWindow window, string title)
        where TPage : Control where TControl : Control
    {
        if (!window.NavigateToIndexedPage(title) || window.CurrentPage is not TPage page)
            throw new InvalidOperationException($"Could not open the dedicated {title} page.");
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            window.UpdateLayout();
            var control = page.GetLogicalDescendants().OfType<TControl>().SingleOrDefault();
            if (control is null || !control.IsAttachedToVisualTree() || control.Bounds.Width <= 0 || control.Bounds.Height <= 0)
                throw new InvalidOperationException($"The {title} demo control was not laid out in the visual tree.");
            if (control is MdSlideToConfirm slide)
            {
                var peer = ControlAutomationPeer.CreatePeerForElement(slide);
                var invoke = peer.GetProvider<IInvokeProvider>() ?? throw new InvalidOperationException("Slide confirmation automation is missing.");
                invoke.Invoke();
                if (!slide.IsConfirmed) throw new InvalidOperationException("Slide confirmation did not complete.");
            }
            if (control is MdScrollBar bar)
            {
                bar.Value = 72;
                if (bar.Value != 72 || !bar.IsTrackVisible)
                    throw new InvalidOperationException("Standalone scrollbar did not update.");
            }
            if (control is MdTimeDial dial)
            {
                dial.ActivePart = MdTimeDialPart.Minute;
                dial.Minute = 42;
                if (dial.Minute != 42) throw new InvalidOperationException("Standalone time dial did not update.");
            }
            if (control is MdPaginatedDataTable table)
            {
                table.RequestSort(2);
                window.UpdateLayout();
                if (table.PageItems.FirstOrDefault() is not PaginatedGalleryRow { Score: 86 } ||
                    !table.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Text == "86"))
                    throw new InvalidOperationException("Paginated table sorting or compiled row binding failed.");
            }
            if (control is MdWheelPicker wheel)
            {
                var peer = ControlAutomationPeer.CreatePeerForElement(wheel);
                var range = peer.GetProvider<IRangeValueProvider>() ?? throw new InvalidOperationException("Wheel automation is missing.");
                range.SetValue(15);
                if (wheel.Value != 15) throw new InvalidOperationException("Wheel picker did not change value.");
            }
        }, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Desktop-only shell feature: a tray icon whose menu shows the window, switches the theme
    /// and owns shutdown. Closing the window hides it instead of quitting while the tray is
    /// present; the Android single-view lifetime never reaches this code.
    /// </summary>
    private static void InstallTrayIcon(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var trayIcon = new TrayIcon
        {
            Icon = LoadGalleryIcon(),
            ToolTipText = "Md3.Avalonia Gallery",
            Menu = BuildTrayMenu(desktop),
            IsVisible = true,
        };
        TrayIcon.SetIcons(Current!, new TrayIcons { trayIcon });

        if (desktop.MainWindow is { } window)
        {
            window.Closing += (_, e) =>
            {
                if (_quitting || e.IsProgrammatic) return;
                e.Cancel = true;
                window.Hide();
            };
        }
    }

    internal static WindowIcon? LoadGalleryIcon()
    {
        // The visible Gallery mark, title-bar icon and tray all use the repository's canonical
        // logo. A missing or unreadable asset must not take the shell down; the menu still works.
        try
        {
            using var stream = AssetLoader.Open(new Uri("avares://Md3.Avalonia.Gallery/Assets/logo-128.png"));
            return new WindowIcon(new Bitmap(stream));
        }
        catch
        {
            return null;
        }
    }

    private static NativeMenu BuildTrayMenu(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var menu = new NativeMenu();

        var show = new NativeMenuItem(GalleryLocalization.Choose("Show Gallery", "显示 Gallery"));
        show.Click += (_, _) =>
        {
            if (desktop.MainWindow is { } window)
            {
                window.Show();
                window.WindowState = WindowState.Normal;
                window.Activate();
            }
        };
        menu.Add(show);

        var theme = new NativeMenuItem(GalleryLocalization.Choose("Theme", "主题")) { Menu = new NativeMenu() };
        foreach (var (english, chinese, variant) in new (string, string, ThemeVariant)[]
        {
            ("Light", "浅色", ThemeVariant.Light),
            ("Dark", "深色", ThemeVariant.Dark),
            ("System", "跟随系统", ThemeVariant.Default),
        })
        {
            var item = new NativeMenuItem(GalleryLocalization.Choose(english, chinese))
            {
                ToggleType = MenuItemToggleType.Radio,
                IsChecked = ReferenceEquals(Current?.RequestedThemeVariant, variant),
            };
            var chosen = variant;
            item.Click += (_, _) =>
            {
                if (Current is { } app)
                {
                    app.RequestedThemeVariant = chosen;
                }
            };
            theme.Menu!.Add(item);
        }
        menu.Add(theme);

        menu.Add(new NativeMenuItemSeparator());

        var exit = new NativeMenuItem(GalleryLocalization.Choose("Exit", "退出"));
        exit.Click += (_, _) =>
        {
            _quitting = true;
            desktop.Shutdown();
        };
        menu.Add(exit);

        return menu;
    }
}
