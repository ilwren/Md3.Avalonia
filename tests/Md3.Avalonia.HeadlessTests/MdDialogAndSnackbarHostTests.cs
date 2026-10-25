using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdDialogAndSnackbarHostTests
{
    [AvaloniaFact]
    public void Gallery_Simple_Dialog_Scrim_Covers_The_Whole_Desktop_Window()
    {
        var window = new MainWindow { Width = 1280, Height = 800 };
        MdMotion.SetScheme(window, MdMotionScheme.None);
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var rootLayout = window.FindControl<Grid>("RootLayout");
            Assert.NotNull(rootLayout);
            window.FindControl<MdButton>("SimpleDialogNav")!
                .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            using var beforeFrame = window.CaptureRenderedFrame();
            Assert.NotNull(beforeFrame);
            var openButton = window.GetVisualDescendants().OfType<MdButton>()
                .Single(button => Equals(button.Content?.ToString(), "Choose account"));
            PointerInput.Click(openButton);
            Dispatcher.UIThread.RunJobs();

            var dialogHost = window.DialogHost;
            Assert.True(dialogHost.IsOpen);
            var simpleDialog = Assert.IsType<MdSimpleDialog>(dialogHost.Dialog);
            var dialogPresenter = dialogHost.GetVisualDescendants().OfType<ContentPresenter>()
                .Single(presenter => presenter.Name == "PART_DialogPresenter");
            var simpleDialogOverlay = simpleDialog.GetVisualDescendants().OfType<Grid>()
                .Single(grid => grid.Name == "PART_Overlay");
            var simpleDialogScrim = simpleDialog.GetVisualDescendants().OfType<Border>()
                .Single(border => border.Name == "PART_Scrim");
            Assert.Equal(dialogHost.Bounds.Width, dialogPresenter.Bounds.Width, 1);
            Assert.Equal(dialogHost.Bounds.Height, dialogPresenter.Bounds.Height, 1);
            Assert.Equal(dialogHost.Bounds.Width, simpleDialogOverlay.Bounds.Width, 1);
            Assert.Equal(dialogHost.Bounds.Height, simpleDialogOverlay.Bounds.Height, 1);
            Assert.True(simpleDialogOverlay.IsVisible);
            Assert.False(simpleDialogScrim.IsVisible,
                "MdDialogHost owns the full-window scrim; the nested SimpleDialog must not paint a partial second one.");

            var overlay = dialogHost.GetVisualDescendants().OfType<Grid>()
                .Single(grid => grid.Name == "PART_Overlay" &&
                                grid.Children.Any(child => child.Name == "PART_DialogPresenter"));
            var scrim = Assert.Single(overlay.Children.OfType<Border>(),
                border => border.Name == "PART_Scrim");

            // The host is a top-level sibling of ContentShell, so its scrim must match the
            // Window client viewport—not merely the dialog presenter or the scrolling page.
            Assert.Equal(window.ClientSize.Width, rootLayout.Bounds.Width, 1);
            Assert.Equal(window.ClientSize.Height, rootLayout.Bounds.Height, 1);
            Assert.Equal(window.ClientSize.Width, dialogHost.Bounds.Width, 1);
            Assert.Equal(window.ClientSize.Height, dialogHost.Bounds.Height, 1);
            var hostOrigin = dialogHost.TranslatePoint(default, window);
            Assert.NotNull(hostOrigin);
            Assert.Equal(0, hostOrigin!.Value.X, 1);
            Assert.Equal(0, hostOrigin.Value.Y, 1);
            Assert.Equal(dialogHost.Bounds.Width, scrim.Bounds.Width, 1);
            Assert.Equal(dialogHost.Bounds.Height, scrim.Bounds.Height, 1);
            var scrimOrigin = scrim.TranslatePoint(default, window);
            var scrimFarEdge = scrim.TranslatePoint(
                new Point(scrim.Bounds.Width, scrim.Bounds.Height), window);
            Assert.NotNull(scrimOrigin);
            Assert.NotNull(scrimFarEdge);
            Assert.Equal(0, scrimOrigin!.Value.X, 1);
            Assert.Equal(0, scrimOrigin.Value.Y, 1);
            Assert.Equal(window.ClientSize.Width, scrimFarEdge!.Value.X, 1);
            Assert.Equal(window.ClientSize.Height, scrimFarEdge.Value.Y, 1);
            Assert.True(overlay.IsVisible);

            using var afterFrame = window.CaptureRenderedFrame();
            Assert.NotNull(afterFrame);
            var uncoveredRegions = new[]
            {
                new Point(8, 8),
                new Point(window.ClientSize.Width / 2, 8),
                new Point(window.ClientSize.Width - 8, 8),
                new Point(8, window.ClientSize.Height / 2),
                new Point(window.ClientSize.Width - 8, window.ClientSize.Height / 2),
                new Point(8, window.ClientSize.Height - 8),
                new Point(window.ClientSize.Width / 2, window.ClientSize.Height - 8),
                new Point(window.ClientSize.Width - 8, window.ClientSize.Height - 8),
            };
            foreach (var point in uncoveredRegions)
            {
                var beforePixel = ReadPixel(beforeFrame!, point, window.RenderScaling);
                var afterPixel = ReadPixel(afterFrame!, point, window.RenderScaling);
                Assert.True(ReadRgbSum(afterPixel) < ReadRgbSum(beforePixel),
                    $"the full-window scrim must darken viewport point {point}; " +
                    $"before=0x{beforePixel:X8}, after=0x{afterPixel:X8}");
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Gallery_Simple_Dialog_Scrim_Covers_The_Whole_Android_Shell()
    {
        var shell = new AndroidGalleryView();
        var window = new Window { Width = 360, Height = 640, Content = shell };
        MdMotion.SetScheme(shell, MdMotionScheme.None);
        window.Show();
        try
        {
            Assert.True(shell.NavigateToPage("Simple dialog"));
            Dispatcher.UIThread.RunJobs();
            var open = shell.GetVisualDescendants().OfType<MdButton>()
                .Single(button => Equals(button.Content?.ToString(), "Choose account"));
            open.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            var host = shell.DialogHost;
            Assert.True(host.IsOpen);
            Assert.IsType<MdSimpleDialog>(host.Dialog);
            Assert.Equal(shell.Bounds.Width, host.Bounds.Width, 1);
            Assert.Equal(shell.Bounds.Height, host.Bounds.Height, 1);
            var origin = host.TranslatePoint(default, shell);
            Assert.NotNull(origin);
            Assert.Equal(0, origin!.Value.X, 1);
            Assert.Equal(0, origin.Value.Y, 1);

            var overlay = host.GetVisualDescendants().OfType<Grid>()
                .Single(grid => grid.Name == "PART_Overlay" &&
                                grid.Children.Any(child => child.Name == "PART_DialogPresenter"));
            var scrim = overlay.Children.OfType<Border>().Single(border => border.Name == "PART_Scrim");
            Assert.Equal(shell.Bounds.Width, scrim.Bounds.Width, 1);
            Assert.Equal(shell.Bounds.Height, scrim.Bounds.Height, 1);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Gallery_License_Dialog_Uses_A_Compact_Full_Screen_Android_Layout()
    {
        var shell = new AndroidGalleryView();
        var window = new Window { Width = 360, Height = 640, Content = shell };
        MdMotion.SetScheme(shell, MdMotionScheme.None);
        window.Show();
        try
        {
            Assert.True(shell.NavigateToPage("About and licenses"));
            Dispatcher.UIThread.RunJobs();
            shell.GetVisualDescendants().OfType<MdButton>()
                .Single(button => Equals(button.Content?.ToString(), "About this app"))
                .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            shell.DialogHost.GetVisualDescendants().OfType<MdButton>()
                .Single(button => Equals(button.Content?.ToString(), "View licenses"))
                .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            var dialog = Assert.IsType<MdDialog>(shell.DialogHost.Dialog);
            var page = Assert.IsType<MdLicensePage>(dialog.Content);
            Assert.Equal(MdDialogVariant.FullScreen, dialog.Variant);
            Assert.InRange(dialog.Bounds.Width, 1, shell.DialogHost.Bounds.Width);
            Assert.InRange(dialog.Bounds.Height, 1, shell.DialogHost.Bounds.Height);
            Assert.InRange(page.Bounds.Width, 1, dialog.Bounds.Width);
            Assert.InRange(page.Bounds.Height, 1, dialog.Bounds.Height);

            var compact = page.GetVisualDescendants().OfType<Grid>()
                .Single(grid => grid.Name == "PART_CompactLayout");
            Assert.True(compact.IsVisible);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void DialogHost_SimpleDialog_Uses_One_Full_Screen_Scrim_And_Synchronizes_Close()
    {
        var dialog = new MdSimpleDialog
        {
            Title = "Choose account",
            ItemsSource = new[] { "One", "Two" }
        };
        var host = new MdDialogHost
        {
            Content = new Border { Background = Brushes.White },
            Dialog = dialog,
            IsOpen = true
        };
        MdMotion.SetScheme(host, MdMotionScheme.None);
        var window = new Window { Width = 640, Height = 480, Content = host };
        object? selected = null;
        var dismissals = 0;
        dialog.ItemSelected += (_, item) =>
        {
            selected = item;
            host.Close(item);
        };
        dialog.Dismissed += (_, _) => dismissals++;
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.True(dialog.IsOpen);
            var presenter = host.GetVisualDescendants().OfType<ContentPresenter>()
                .Single(control => control.Name == "PART_DialogPresenter");
            var hostOverlay = host.GetVisualDescendants().OfType<Grid>()
                .Single(control => control.Name == "PART_Overlay" &&
                                   control.Children.Any(child => child.Name == "PART_DialogPresenter"));
            var overlay = dialog.GetVisualDescendants().OfType<Grid>()
                .Single(control => control.Name == "PART_Overlay");
            var scrim = dialog.GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_Scrim");
            Assert.True(hostOverlay.IsVisible,
                "a host opened before template application must still expose its full-window overlay.");
            Assert.Equal(1d, hostOverlay.Opacity, 3);
            Assert.Equal(host.Bounds.Width, presenter.Bounds.Width, 1);
            Assert.Equal(host.Bounds.Height, presenter.Bounds.Height, 1);
            Assert.Equal(host.Bounds.Width, overlay.Bounds.Width, 1);
            Assert.Equal(host.Bounds.Height, overlay.Bounds.Height, 1);
            Assert.False(scrim.IsVisible);

            var items = dialog.GetVisualDescendants().OfType<SelectingItemsControl>()
                .Single(control => control.Name == "PART_ItemsHost");
            items.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Two", selected);
            Assert.False(dialog.IsOpen);
            Assert.False(host.IsOpen);
            Assert.Equal(0, dismissals);

            host.IsOpen = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(dialog.IsOpen);
            host.Close();
            Dispatcher.UIThread.RunJobs();
            Assert.False(dialog.IsOpen);
            Assert.Equal(1, dismissals);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void DialogHost_Does_Not_Adopt_SimpleDialog_In_Main_Content()
    {
        var standaloneDialog = new MdSimpleDialog
        {
            Title = "Standalone",
            ItemsSource = new[] { "One" },
            IsOpen = true
        };
        var host = new MdDialogHost { Content = standaloneDialog };
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var scrim = standaloneDialog.GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_Scrim");
            Assert.True(standaloneDialog.IsOpen);
            Assert.True(scrim.IsVisible,
                "a SimpleDialog in the host's main content is standalone and keeps its own scrim.");
        }
        finally { window.Close(); }
    }

    private static int ReadRgbSum(uint pixel) =>
        (int)(pixel & 0xFF) + (int)((pixel >> 8) & 0xFF) + (int)((pixel >> 16) & 0xFF);

    private static uint ReadPixel(Bitmap frame, Point logicalPoint, double scaling)
    {
        var size = frame.PixelSize;
        var pixels = new uint[size.Width * size.Height];
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            frame.CopyPixels(new PixelRect(0, 0, size.Width, size.Height),
                handle.AddrOfPinnedObject(), pixels.Length * sizeof(uint), size.Width * sizeof(uint));
        }
        finally
        {
            handle.Free();
        }

        var x = Math.Clamp((int)Math.Floor(logicalPoint.X * scaling), 0, size.Width - 1);
        var y = Math.Clamp((int)Math.Floor(logicalPoint.Y * scaling), 0, size.Height - 1);
        return pixels[y * size.Width + x];
    }

    [AvaloniaFact]
    public async Task DialogHost_Selects_Multiple_Predeclared_Templates_By_Model_Type()
    {
        var host = new MdDialogHost { Content = new TextBlock { Text = "Page" } };
        host.DataTemplates.Add(new FuncDataTemplate<DeleteDialogModel>((model, _) =>
            new MdDialog { Headline = $"Delete {model!.Name}?", Content = "Delete body" }));
        host.DataTemplates.Add(new FuncDataTemplate<RenameDialogModel>((model, _) =>
            new MdDialog { Headline = $"Rename {model!.Name}", Content = "Rename body" }));

        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            var deleteModel = new DeleteDialogModel("Draft");
            var deleteResult = host.ShowAsync(deleteModel);
            Dispatcher.UIThread.RunJobs();

            Assert.Same(deleteModel, host.Dialog);
            Assert.Equal("Delete Draft?", host.GetVisualDescendants().OfType<MdDialog>().Single().Headline);
            host.Close(true);
            Assert.True((bool)(await deleteResult)!);

            var renameModel = new RenameDialogModel("Draft");
            var renameResult = host.ShowAsync(renameModel);
            Dispatcher.UIThread.RunJobs();

            Assert.Same(renameModel, host.Dialog);
            Assert.Equal("Rename Draft", host.GetVisualDescendants().OfType<MdDialog>().Single().Headline);
            host.Close("renamed");
            Assert.Equal("renamed", await renameResult);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task SnackbarService_Queues_ViewModel_Messages_Through_Attached_Host()
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
            var firstResult = service.ShowAsync(new MdSnackbarMessage("First")
            {
                Duration = TimeSpan.Zero,
                IsDismissible = true
            });
            var secondResult = service.ShowAsync(new MdSnackbarMessage("Second")
            {
                Duration = TimeSpan.Zero,
                ActionContent = "Undo"
            });
            Dispatcher.UIThread.RunJobs();

            var snackbar = host.GetVisualDescendants().OfType<MdSnackbar>().Single();
            Assert.True(snackbar.IsOpen);
            Assert.Equal("First", snackbar.Content);

            service.Dismiss();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(MdSnackbarResult.Dismissed, await firstResult);
            Assert.True(snackbar.IsOpen);
            Assert.Equal("Second", snackbar.Content);

            var action = snackbar.GetVisualDescendants().OfType<MdButton>()
                .Single(button => button.Name == "PART_ActionButton");
            PointerInput.Click(action);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(MdSnackbarResult.ActionInvoked, await secondResult);
            Assert.False(snackbar.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    private sealed record DeleteDialogModel(string Name);
    private sealed record RenameDialogModel(string Name);
}
