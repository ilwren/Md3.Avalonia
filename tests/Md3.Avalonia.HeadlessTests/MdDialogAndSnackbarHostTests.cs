using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
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
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var rootLayout = window.FindControl<Grid>("RootLayout");
            Assert.NotNull(rootLayout);

            var dialogHost = window.DialogHost;
            dialogHost.Dialog = new MdSimpleDialog
            {
                Title = "Choose an account",
                ItemsSource = new[] { "one@example.com", "two@example.com" },
                IsOpen = true
            };
            dialogHost.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var overlay = dialogHost.GetVisualDescendants().OfType<Grid>()
                .Single(grid => grid.Name == "PART_Overlay" &&
                                grid.Children.Any(child => child.Name == "PART_DialogPresenter"));
            var scrim = Assert.Single(overlay.Children.OfType<Border>()
                .Where(border => border.Name == "PART_Scrim"));

            // The global host is a window-root overlay, not a child of ContentShell's body row.
            // Its scrim must therefore include the app bar and the primary navigation rail too.
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
        }
        finally
        {
            window.Close();
        }
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
