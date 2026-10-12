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
            Dispatcher.UIThread.RunJobs();
            window.LayoutManager.ExecuteLayoutPass();
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
}
