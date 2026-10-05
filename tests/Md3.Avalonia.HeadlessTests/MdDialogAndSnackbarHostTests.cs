using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdDialogAndSnackbarHostTests
{
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
