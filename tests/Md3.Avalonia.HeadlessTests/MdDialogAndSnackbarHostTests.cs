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

            // The host is a sibling of ContentShell in RootLayout, so its scrim includes the app
            // bar, navigation rail, document pane, and side pane—not just the dialog presenter.
            Assert.Equal(rootLayout.Bounds.Width, dialogHost.Bounds.Width, 1);
            Assert.Equal(rootLayout.Bounds.Height, dialogHost.Bounds.Height, 1);
            Assert.Equal(dialogHost.Bounds.Width, scrim.Bounds.Width, 1);
            Assert.Equal(dialogHost.Bounds.Height, scrim.Bounds.Height, 1);
            var scrimOrigin = scrim.TranslatePoint(default, rootLayout);
            var scrimFarEdge = scrim.TranslatePoint(
                new Point(scrim.Bounds.Width, scrim.Bounds.Height), rootLayout);
            Assert.NotNull(scrimOrigin);
            Assert.NotNull(scrimFarEdge);
            Assert.Equal(0, scrimOrigin!.Value.X, 1);
            Assert.Equal(0, scrimOrigin.Value.Y, 1);
            Assert.Equal(rootLayout.Bounds.Width, scrimFarEdge!.Value.X, 1);
            Assert.Equal(rootLayout.Bounds.Height, scrimFarEdge.Value.Y, 1);
            Assert.True(overlay.IsVisible);

            using var afterFrame = window.CaptureRenderedFrame();
            Assert.NotNull(afterFrame);
            var uncoveredRegions = new[]
            {
                new Point(8, 8),
                new Point(window.ClientSize.Width - 8, 8),
                new Point(8, window.ClientSize.Height - 8),
                new Point(window.ClientSize.Width - 8, window.ClientSize.Height - 8),
            };
            foreach (var point in uncoveredRegions)
            {
                Assert.NotEqual(ReadPixel(beforeFrame!, point, window.RenderScaling),
                    ReadPixel(afterFrame!, point, window.RenderScaling));
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DialogHost_SimpleDialog_Uses_One_Full_Screen_Scrim_And_Synchronizes_Close()
    {
        var dialog = new MdSimpleDialog
        {
            Title = "Choose account",
            ItemsSource = new[] { "One", "Two" },
            IsOpen = true
        };
        var host = new MdDialogHost
        {
            Content = new Border { Background = Brushes.White },
            Dialog = dialog,
            IsOpen = true
        };
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
            var overlay = dialog.GetVisualDescendants().OfType<Grid>()
                .Single(control => control.Name == "PART_Overlay");
            var scrim = dialog.GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_Scrim");
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
            Assert.True(scrim.IsVisible);

            host.IsOpen = true;
            Dispatcher.UIThread.RunJobs();
            host.Close();
            Dispatcher.UIThread.RunJobs();
            Assert.True(standaloneDialog.IsOpen);
            Assert.True(scrim.IsVisible);
        }
        finally { window.Close(); }
    }

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
