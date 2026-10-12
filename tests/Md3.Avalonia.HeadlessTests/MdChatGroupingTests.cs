using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Chat;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>Phase 5 grouping, date separators and unread divider.</summary>
public sealed class MdChatGroupingTests
{
    private static readonly DateTimeOffset Noon = new(2026, 10, 12, 12, 0, 0, DateTimeOffset.Now.Offset);

    private static MdChatMessage Msg(string id, MdChatMessageRole role, DateTimeOffset at, string? sender = null) =>
        new(id, role, "text", at, sender);

    [Fact]
    public void Same_sender_within_the_window_is_grouped()
    {
        var policy = new MdChatGroupingPolicy();
        var first = Msg("a", MdChatMessageRole.User, Noon, "Ann");
        var second = Msg("b", MdChatMessageRole.User, Noon.AddMinutes(2), "Ann");

        var layout = policy.Layout(first, second, null);

        Assert.False(layout.ShowSender);
        Assert.False(layout.ShowAvatar);
        Assert.False(layout.ShowDateSeparator);
    }

    [Fact]
    public void A_gap_longer_than_the_window_starts_a_new_group()
    {
        var policy = new MdChatGroupingPolicy();
        var first = Msg("a", MdChatMessageRole.User, Noon, "Ann");
        var second = Msg("b", MdChatMessageRole.User, Noon.AddMinutes(6), "Ann");

        Assert.True(policy.Layout(first, second, null).ShowSender);
    }

    [Fact]
    public void A_different_sender_or_role_starts_a_new_group()
    {
        var policy = new MdChatGroupingPolicy();
        var first = Msg("a", MdChatMessageRole.User, Noon, "Ann");

        Assert.True(policy.Layout(first, Msg("b", MdChatMessageRole.User, Noon, "Bob"), null).ShowSender);
        Assert.True(policy.Layout(first, Msg("c", MdChatMessageRole.Assistant, Noon, "Ann"), null).ShowSender);
    }

    [Fact]
    public void System_messages_are_never_grouped()
    {
        var policy = new MdChatGroupingPolicy();
        var first = Msg("a", MdChatMessageRole.System, Noon);
        var second = Msg("b", MdChatMessageRole.System, Noon);

        Assert.True(policy.Layout(first, second, null).ShowSender);
    }

    [Fact]
    public void The_first_message_and_each_new_local_day_show_a_date_separator()
    {
        var policy = new MdChatGroupingPolicy();
        var first = Msg("a", MdChatMessageRole.User, Noon, "Ann");
        var nextDay = Msg("b", MdChatMessageRole.User, Noon.AddDays(1), "Ann");

        var initial = policy.Layout(null, first, null);
        Assert.True(initial.ShowDateSeparator);
        Assert.Equal(Noon.ToLocalTime().Date, initial.DateSeparator);

        var next = policy.Layout(first, nextDay, null);
        Assert.True(next.ShowDateSeparator);
        Assert.True(next.ShowSender, "a new day also breaks the group");
    }

    [Fact]
    public void The_message_after_the_last_read_message_has_the_unread_divider()
    {
        var policy = new MdChatGroupingPolicy();
        var read = Msg("read", MdChatMessageRole.Assistant, Noon);
        var unread = Msg("unread", MdChatMessageRole.User, Noon.AddMinutes(10));

        Assert.True(policy.Layout(read, unread, "read").ShowUnreadDivider);
        Assert.False(policy.Layout(read, unread, "other").ShowUnreadDivider);
        Assert.False(policy.Layout(read, unread, null).ShowUnreadDivider);
    }

    [AvaloniaFact]
    public void Chat_view_shows_the_unread_divider_under_the_last_read_message()
    {
        var messages = new ObservableCollection<MdChatMessage>();
        for (var i = 0; i < 8; i++)
            messages.Add(Msg($"m{i}", i % 2 == 0 ? MdChatMessageRole.User : MdChatMessageRole.Assistant, Noon.AddMinutes(i * 10), $"s{i}"));
        var chat = new MdChatView { MessagesSource = messages, LastReadMessageId = "m3" };
        var window = new Window { Width = 480, Height = 600, Content = chat };
        window.Show();
        try
        {
            Settle(window);
            var presenters = chat.GetVisualDescendants().OfType<MdChatMessagePresenter>().ToList();
            var after = presenters.First(p => p.Message?.Id == "m4");
            var before = presenters.First(p => p.Message?.Id == "m2");

            Assert.True(after.Layout?.ShowUnreadDivider);
            Assert.False(before.Layout?.ShowUnreadDivider);
            Assert.False(string.IsNullOrEmpty(after.UnreadDividerText));
            var divider = after.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_UnreadDivider");
            Assert.True(divider.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void A_streamed_update_recomputes_the_layout_of_the_following_message()
    {
        var messages = new ObservableCollection<MdChatMessage>
        {
            Msg("a", MdChatMessageRole.User, Noon, "Ann"),
            Msg("b", MdChatMessageRole.User, Noon.AddMinutes(1), "Ann"),
        };
        var chat = new MdChatView { MessagesSource = messages };
        var window = new Window { Width = 480, Height = 600, Content = chat };
        window.Show();
        try
        {
            Settle(window);
            var presenter = chat.GetVisualDescendants().OfType<MdChatMessagePresenter>().First(p => p.Message?.Id == "b");
            Assert.False(presenter.Layout?.ShowSender);

            // Moving the first message to another sender changes the layout of the second one.
            messages[0] = Msg("a", MdChatMessageRole.User, Noon, "Zed");
            Settle(window);

            Assert.True(presenter.Layout?.ShowSender);
        }
        finally
        {
            window.Close();
        }
    }

    private static void Settle(Window window)
    {
        for (var i = 0; i < 3; i++)
        {
            window.Measure(new Size(window.Width, window.Height));
            window.Arrange(new Rect(0, 0, window.Width, window.Height));
            Dispatcher.UIThread.RunJobs();
        }
    }
}
