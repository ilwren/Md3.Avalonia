using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Chat;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>Phase 5 (list behaviour) of the chat plan: bottom following, unread counting and scrolling to messages.</summary>
public sealed class MdChatPhase5Tests
{
    private static ObservableCollection<MdChatMessage> Conversation(int count)
    {
        var messages = new ObservableCollection<MdChatMessage>();
        for (var i = 0; i < count; i++)
        {
            messages.Add(new MdChatMessage(
                $"m{i}",
                i % 2 == 0 ? MdChatMessageRole.User : MdChatMessageRole.Assistant,
                $"Message {i} with enough text to take a line or two of space in the list.",
                DateTimeOffset.Now.AddMinutes(i)));
        }
        return messages;
    }

    private static MdChatMessage NewReply(string id) =>
        new(id, MdChatMessageRole.Assistant, "A new reply at the end of the list", DateTimeOffset.Now);

    private static ScrollViewer MessagesScroll(MdChatView chat) =>
        chat.GetVisualDescendants().OfType<ListBox>().First(list => list.Name == "PART_MessagesHost")
            .GetVisualDescendants().OfType<ScrollViewer>().First();

    private static void Settle(Window window)
    {
        for (var i = 0; i < 3; i++)
        {
            // The same measure, arrange and run-jobs sequence the other headless layout tests use.
            window.Measure(new Size(window.Width, window.Height));
            window.Arrange(new Rect(0, 0, window.Width, window.Height));
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void At_Bottom_A_New_Message_Keeps_The_View_At_The_Bottom()
    {
        var messages = Conversation(40);
        var chat = new MdChatView { MessagesSource = messages };
        var window = new Window { Width = 480, Height = 320, Content = chat };
        window.Show();
        try
        {
            Settle(window);
            chat.ScrollToLatest();
            Settle(window);
            var scroll = MessagesScroll(chat);
            Assert.True(chat.IsAtBottom);

            messages.Add(NewReply("new-bottom"));
            Settle(window);

            Assert.True(chat.IsAtBottom);
            Assert.Equal(0, chat.UnreadCount);
            Assert.Equal(scroll.Extent.Height - scroll.Viewport.Height, scroll.Offset.Y, 1);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Scrolled_Up_A_New_Message_Keeps_The_Position_And_Counts_Unread()
    {
        var messages = Conversation(40);
        var chat = new MdChatView { MessagesSource = messages };
        var window = new Window { Width = 480, Height = 320, Content = chat };
        window.Show();
        try
        {
            Settle(window);
            var scroll = MessagesScroll(chat);
            scroll.Offset = new Vector(0, 0);
            Settle(window);
            Assert.False(chat.IsAtBottom);
            var before = scroll.Offset.Y;

            messages.Add(NewReply("new-middle"));
            Settle(window);

            Assert.Equal(before, scroll.Offset.Y, 1);
            Assert.False(chat.IsAtBottom);
            Assert.Equal(1, chat.UnreadCount);

            chat.ScrollToLatest();
            Settle(window);

            Assert.True(chat.IsAtBottom);
            Assert.Equal(0, chat.UnreadCount);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void ScrollToMessage_Brings_The_Message_Into_View()
    {
        var messages = Conversation(40);
        var chat = new MdChatView { MessagesSource = messages };
        var window = new Window { Width = 480, Height = 320, Content = chat };
        window.Show();
        try
        {
            Settle(window);
            var scroll = MessagesScroll(chat);
            chat.ScrollToLatest();
            Settle(window);
            Assert.True(scroll.Offset.Y > 0);

            chat.ScrollToMessage(messages[0]);
            Settle(window);

            Assert.Equal(0, scroll.Offset.Y, 1);
        }
        finally { window.Close(); }
    }

    /// <summary>The first realized message whose bottom edge is in view, with its screen position.</summary>
    private static (string Id, double Top) FirstVisible(MdChatView chat, ScrollViewer scroll)
    {
        foreach (var presenter in chat.GetVisualDescendants().OfType<MdChatMessagePresenter>())
        {
            if (presenter.Message is not { } message) continue;
            if (presenter.TranslatePoint(new Point(0, 0), scroll) is not { } top) continue;
            if (top.Y + presenter.Bounds.Height > 0) return (message.Id, top.Y);
        }
        throw new Xunit.Sdk.XunitException("No message is visible.");
    }

    [AvaloniaFact]
    public void Prepending_History_Keeps_The_First_Visible_Message_In_Place_And_Is_Not_Unread()
    {
        var messages = Conversation(40);
        var chat = new MdChatView { MessagesSource = messages };
        var window = new Window { Width = 480, Height = 320, Content = chat };
        window.Show();
        try
        {
            Settle(window);
            var scroll = MessagesScroll(chat);
            scroll.Offset = new Vector(0, 400);
            Settle(window);
            var before = FirstVisible(chat, scroll);

            for (var i = 0; i < 50; i++)
            {
                messages.Insert(i, new MdChatMessage($"older{i}", MdChatMessageRole.User,
                    $"Older message {i} with enough text to take a line or two of space in the list.",
                    DateTimeOffset.Now.AddHours(-1 - i)));
            }
            Settle(window);

            var after = FirstVisible(chat, scroll);
            Assert.Equal(before.Id, after.Id);
            Assert.Equal(before.Top, after.Top, 1);
            Assert.Equal(0, chat.UnreadCount);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void History_Is_Requested_Once_Near_The_Top_And_Re_Armed_By_A_Change()
    {
        var messages = Conversation(40);
        var chat = new MdChatView { MessagesSource = messages };
        var requests = 0;
        chat.HistoryRequested += (_, _) => requests++;
        var window = new Window { Width = 480, Height = 320, Content = chat };
        window.Show();
        try
        {
            Settle(window);
            Assert.Equal(1, requests);
            Settle(window);
            Assert.Equal(1, requests);

            for (var i = 0; i < 5; i++)
            {
                messages.Insert(i, new MdChatMessage($"older{i}", MdChatMessageRole.User, "Older message",
                    DateTimeOffset.Now.AddHours(-1 - i)));
            }
            Settle(window);
            var scroll = MessagesScroll(chat);
            scroll.Offset = new Vector(0, 0);
            Settle(window);

            Assert.Equal(2, requests);
        }
        finally { window.Close(); }
    }
}
