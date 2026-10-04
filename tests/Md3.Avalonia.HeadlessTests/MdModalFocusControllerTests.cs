using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// A modal surface whose scope cannot take focus used to deadlock the UI thread: the focus
/// redirect re-armed itself on every attempt, so the dispatcher queue refilled as fast as it
/// drained and <c>Dispatcher.RunJobs()</c> never returned. The headless suite ran for 45 minutes
/// against a 1m48s baseline instead of failing. These tests pin the guard that replaced it.
/// </summary>
public sealed class MdModalFocusControllerTests
{
    [AvaloniaFact]
    public void Redirect_Gives_Up_When_The_Scope_Will_Not_Take_Focus()
    {
        // Two targets, because a failed redirect leaves focus exactly where it was: re-focusing
        // the same button would raise no second GotFocus and the guard would never be exercised.
        var first = new Button { Content = "Outside A" };
        var second = new Button { Content = "Outside B" };
        // Collapsed, so neither the scope nor anything in it can ever accept focus — the state a
        // surface is in when it is opened before being attached.
        var scope = new Border { IsVisible = false, Child = new Button { Content = "Unreachable" } };
        var owner = new Border { Child = new TextBlock { Text = "Owner" } };
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = new StackPanel { Children = { first, second, owner, scope } }
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var controller = new MdModalFocusController(owner);
            controller.Update(active: true, scope: scope);
            Dispatcher.UIThread.RunJobs();
            Assert.True(controller.IsActive);

            for (var attempt = 0; attempt < 6 && !controller.HasAbandonedFocusRedirect; attempt++)
            {
                (attempt % 2 == 0 ? first : second).Focus();
                Dispatcher.UIThread.RunJobs();
            }

            Assert.True(controller.HasAbandonedFocusRedirect);
            // Giving up on focus is not the same as giving up on modality.
            Assert.True(controller.IsActive);

            controller.Deactivate();
            Dispatcher.UIThread.RunJobs();
            Assert.False(controller.IsActive);
            Assert.False(controller.HasAbandonedFocusRedirect);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Redirect_Keeps_Working_When_The_Scope_Can_Take_Focus()
    {
        var outside = new Button { Content = "Outside" };
        var inside = new Button { Content = "Inside" };
        var scope = new Border { Child = inside };
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = new StackPanel { Children = { outside, scope } }
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var controller = new MdModalFocusController(scope);
            controller.Update(active: true, scope: scope);
            Dispatcher.UIThread.RunJobs();
            Assert.True(inside.IsFocused);

            for (var attempt = 0; attempt < 5; attempt++)
            {
                outside.Focus();
                Dispatcher.UIThread.RunJobs();
                Assert.True(inside.IsFocused);
            }

            Assert.False(controller.HasAbandonedFocusRedirect);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Dialog_Host_Opened_Before_Attachment_Settles()
    {
        // The original repro: IsOpen set in an object initializer, so the controller is asked to
        // capture focus for a surface that is not in the tree yet.
        var host = new MdDialogHost
        {
            IsOpen = true,
            Content = new TextBlock { Text = "Page behind the dialog" },
            Dialog = new MdDialog { Headline = "Opened early", Content = "No focusable content" }
        };
        var window = new Window { Width = 480, Height = 360, Content = host };
        window.Show();
        try
        {
            // Before the guard this call never returned.
            Dispatcher.UIThread.RunJobs();
            Assert.True(host.IsOpen);
            host.IsOpen = false;
            Dispatcher.UIThread.RunJobs();
            Assert.False(host.IsOpen);
        }
        finally { window.Close(); }
    }
}
