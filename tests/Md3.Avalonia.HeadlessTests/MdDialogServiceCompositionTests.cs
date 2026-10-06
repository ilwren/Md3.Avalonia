using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Executable version of docs/DIALOG_SERVICE.md. Every claim that walkthrough makes about
/// wiring a dialog service to a view model is asserted here against a real window and real
/// pointer input, so the instructions cannot drift away from the control.
/// </summary>
public sealed class MdDialogServiceCompositionTests
{
    /// <summary>The dialog's own model. It answers through the service, never through a control.</summary>
    private sealed class ConfirmModel(IMdDialogService dialogs)
    {
        public void Confirm() => dialogs.Close(true);

        public void Cancel() => dialogs.Close(false);
    }

    /// <summary>
    /// The calling view model. Its only dependency is the interface: no window, no host, no
    /// control of any kind. <see cref="HoldsNoControls"/> is what keeps that honest.
    /// </summary>
    private sealed class DeleteViewModel(IMdDialogService dialogs)
    {
        public string Status { get; private set; } = "nothing yet";

        public async Task<bool> DeleteAsync()
        {
            var confirmed = await dialogs.ShowAsync<bool>(new ConfirmModel(dialogs));
            Status = confirmed ? "deleted" : "kept";
            return confirmed;
        }
    }

    private static bool HoldsNoControls(object viewModel) =>
        viewModel.GetType()
            .GetFields(System.Reflection.BindingFlags.Instance |
                       System.Reflection.BindingFlags.NonPublic |
                       System.Reflection.BindingFlags.Public)
            .All(f => !typeof(Visual).IsAssignableFrom(f.FieldType));

    /// <summary>
    /// The composition root an application runs once: one service object, handed to the host as
    /// a control property and to the view models as an interface.
    /// </summary>
    private static (MdDialogService Service, MdDialogHost Host, Window Window) Compose()
    {
        var service = new MdDialogService();
        var host = new MdDialogHost { Service = service };

        host.DataTemplates.Add(new FuncDataTemplate<ConfirmModel>((model, _) =>
        {
            var confirm = new MdButton { Content = "Delete" };
            confirm.Click += (_, _) => model.Confirm();
            var cancel = new MdButton { Content = "Keep" };
            cancel.Click += (_, _) => model.Cancel();
            return new MdDialog
            {
                Headline = "Delete item?",
                Content = new TextBlock { Text = "This cannot be undone." },
                Actions = new StackPanel
                {
                    Orientation = global::Avalonia.Layout.Orientation.Horizontal,
                    Spacing = 8,
                    Children = { cancel, confirm },
                },
            };
        }));

        // Whatever the dialog shrouds.
        host.Content = new Border { Child = new TextBlock { Text = "app content" } };

        var window = new Window { Width = 800, Height = 600, Content = host };
        return (service, host, window);
    }

    private static void Click(Window window, Control control)
    {
        var centre = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), window);
        Assert.NotNull(centre);
        window.MouseDown(centre!.Value, MouseButton.Left, RawInputModifiers.None);
        window.MouseUp(centre.Value, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
    }

    private static MdButton Button(Visual root, string content) =>
        root.GetVisualDescendants().OfType<MdButton>()
            .First(b => (b.Content as string) == content);

    [AvaloniaFact]
    public void A_View_Model_Holding_No_Control_Shows_A_Dialog_And_Gets_The_Answer_Back()
    {
        var (service, host, window) = Compose();
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            // The view model receives the interface, nothing else.
            var viewModel = new DeleteViewModel(service);
            Assert.True(HoldsNoControls(viewModel),
                "the view model captured a visual, which is the coupling the service exists to remove");

            var pending = viewModel.DeleteAsync();
            Dispatcher.UIThread.RunJobs();

            // The host resolved the view from its DataTemplates purely from the model's type.
            Assert.True(host.IsOpen);
            Assert.True(service.IsOpen);
            Assert.NotEmpty(host.GetVisualDescendants().OfType<MdDialog>());

            Click(window, Button(host, "Delete"));

            Assert.True(pending.IsCompleted, "clicking the dialog's action did not complete the call");
            Assert.True(pending.Result);
            Assert.Equal("deleted", viewModel.Status);
            Assert.False(host.IsOpen);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void A_Request_Made_Before_The_Host_Exists_Waits_Instead_Of_Throwing()
    {
        // Documented as forgiving on purpose: start-up order should not have to be choreographed.
        var service = new MdDialogService();
        var viewModel = new DeleteViewModel(service);
        var pending = viewModel.DeleteAsync();
        Assert.False(pending.IsCompleted);

        var host = new MdDialogHost { Service = service };
        host.DataTemplates.Add(new FuncDataTemplate<ConfirmModel>((model, _) =>
        {
            var confirm = new MdButton { Content = "Delete" };
            confirm.Click += (_, _) => model.Confirm();
            return new MdDialog { Headline = "Delete item?", Actions = confirm };
        }));
        var window = new Window { Width = 800, Height = 600, Content = host };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.True(host.IsOpen, "the queued request was dropped rather than shown on attach");

            Click(window, Button(host, "Delete"));
            Assert.True(pending.IsCompleted);
            Assert.True(pending.Result);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Escape_Completes_The_Pending_Call_With_The_Default_Rather_Than_Throwing()
    {
        var (service, host, window) = Compose();
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var viewModel = new DeleteViewModel(service);
            var pending = viewModel.DeleteAsync();
            Dispatcher.UIThread.RunJobs();
            Assert.True(host.IsOpen);

            window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            Dispatcher.UIThread.RunJobs();

            Assert.True(pending.IsCompleted);
            Assert.False(pending.Result);
            Assert.Equal("kept", viewModel.Status);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Replace_Jumps_The_Queue_That_Show_Waits_In()
    {
        // This is the distinction that decides which method a wizard step or an about-to-licence
        // transition should call: Show would hide the new dialog behind the old one.
        var (service, host, window) = Compose();
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            var first = new ConfirmModel(service);
            var queued = new ConfirmModel(service);
            var jumped = new ConfirmModel(service);

            service.Show(first);
            Dispatcher.UIThread.RunJobs();
            Assert.Same(first, host.Dialog);

            service.Show(queued);
            Dispatcher.UIThread.RunJobs();
            Assert.Same(first, host.Dialog);

            service.Replace(jumped);
            Dispatcher.UIThread.RunJobs();
            Assert.Same(jumped, host.Dialog);

            service.Close();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(queued, host.Dialog);
        }
        finally { window.Close(); }
    }
}
