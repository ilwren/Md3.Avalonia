using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Md3.Avalonia.Chat;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Localization;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdChatViewRegressionTests
{
    [AvaloniaFact]
    public void Enter_In_Composer_Sends_The_Draft_And_Clears_It()
    {
        var chat = new MdChatView { ComposerText = "hello" };
        var submitted = new List<string>();
        chat.MessageSubmitted += (_, text) => submitted.Add(text);

        var window = new Window { Width = 480, Height = 320, Content = chat };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var composer = chat.GetVisualDescendants().OfType<TextBox>().Single(box => box.Name == "PART_Composer");
            composer.Focus();
            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            window.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(new[] { "hello" }, submitted);
            Assert.Equal(string.Empty, chat.ComposerText);
            Assert.DoesNotContain('\n', composer.Text ?? string.Empty);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Shift_Enter_In_Composer_Inserts_A_Line_Break_Without_Sending()
    {
        var chat = new MdChatView { ComposerText = "line one" };
        var submitted = 0;
        chat.MessageSubmitted += (_, _) => submitted++;

        var window = new Window { Width = 480, Height = 320, Content = chat };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var composer = chat.GetVisualDescendants().OfType<TextBox>().Single(box => box.Name == "PART_Composer");
            composer.Focus();
            composer.CaretIndex = composer.Text?.Length ?? 0;
            window.KeyPress(Key.Enter, RawInputModifiers.Shift, PhysicalKey.Enter, null);
            window.KeyRelease(Key.Enter, RawInputModifiers.Shift, PhysicalKey.Enter, null);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(0, submitted);
            Assert.Equal("line one", chat.ComposerText);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Chat_Strings_Follow_The_Inherited_Culture()
    {
        var chat = new MdChatView();
        MdLocalization.SetCulture(chat, CultureInfo.GetCultureInfo("en-US"));
        Assert.Equal("Quote", chat.QuoteText);
        Assert.Equal("Delete", chat.DeleteText);
        Assert.Equal("Type a message...", chat.TypeMessageText);

        MdLocalization.SetCulture(chat, CultureInfo.GetCultureInfo("zh-CN"));
        Assert.Equal("引用", chat.QuoteText);
        Assert.Equal("删除", chat.DeleteText);
        Assert.Equal("输入消息…", chat.TypeMessageText);
    }

    [AvaloniaFact]
    public void Quote_Header_Names_The_Quoted_Sender_And_Selection_Count_Is_Localized()
    {
        var chat = new MdChatView
        {
            MessagesSource = new[]
            {
                new MdChatMessage("1", MdChatMessageRole.Assistant, "First", DateTimeOffset.Now, "Ada"),
                new MdChatMessage("2", MdChatMessageRole.User, "Second", DateTimeOffset.Now, "You"),
            },
        };
        MdLocalization.SetCulture(chat, CultureInfo.GetCultureInfo("en-US"));
        chat.ToggleMessageSelection(chat.MessagesSource!.Cast<MdChatMessage>().First());
        Assert.Equal("1 message(s) selected", chat.SelectionCountText);

        Assert.False(chat.QuoteSelected());

        chat.ToggleMessageSelection(chat.MessagesSource!.Cast<MdChatMessage>().First());
        Assert.True(chat.QuoteSelected());
        Assert.Equal("Replying to Ada", chat.QuoteHeaderText);

        MdLocalization.SetCulture(chat, CultureInfo.GetCultureInfo("zh-CN"));
        Assert.Equal("回复 Ada", chat.QuoteHeaderText);

        chat.CancelQuote();
        Assert.Equal(string.Empty, chat.QuoteHeaderText);
    }

    [AvaloniaFact]
    public void Failed_Message_Without_Error_Text_Shows_Localized_Failure()
    {
        var failed = new MdChatMessage("3", MdChatMessageRole.User, "Oops", DateTimeOffset.Now, "You", MdAsyncRequestState.Error);
        var presenter = new MdChatMessagePresenter { Message = failed };
        MdLocalization.SetCulture(presenter, CultureInfo.GetCultureInfo("en-US"));
        Assert.Equal("Send failed", presenter.FailureText);
        Assert.Equal("Replying to message", presenter.ReplyHeaderText);

        MdLocalization.SetCulture(presenter, CultureInfo.GetCultureInfo("zh-CN"));
        Assert.Equal("发送失败", presenter.FailureText);
        Assert.Equal("回复消息", presenter.ReplyHeaderText);
    }
}
