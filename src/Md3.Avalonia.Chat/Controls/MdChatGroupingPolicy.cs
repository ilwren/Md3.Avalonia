namespace Md3.Avalonia.Chat;

/// <summary>How one message is presented next to the message before it.</summary>
/// <param name="ShowSender">False when the message continues the previous sender's group, so the sender line is hidden.</param>
/// <param name="ShowAvatar">False for the same reason as <paramref name="ShowSender"/>.</param>
/// <param name="ShowDateSeparator">True when the message is the first one on its local day.</param>
/// <param name="DateSeparator">The local day shown by the separator; null when there is no separator.</param>
/// <param name="ShowUnreadDivider">True when the message follows the last read message.</param>
public sealed record MdChatMessageLayout(
    bool ShowSender,
    bool ShowAvatar,
    bool ShowDateSeparator,
    DateTime? DateSeparator,
    bool ShowUnreadDivider);

/// <summary>
/// Decides how each message is grouped with the one before it. Consecutive messages from the same sender and role
/// within <see cref="GroupingWindow"/> share one avatar and sender line. A new local day starts with a date separator,
/// and the message after the last read message starts with an unread divider. System messages are never grouped.
/// </summary>
public sealed class MdChatGroupingPolicy
{
    /// <summary>The longest gap between two messages that are still grouped. Defaults to five minutes.</summary>
    public TimeSpan GroupingWindow { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>Computes the layout of <paramref name="current"/> given the message before it (null for the first one).</summary>
    public MdChatMessageLayout Layout(MdChatMessage? previous, MdChatMessage current, string? lastReadMessageId)
    {
        ArgumentNullException.ThrowIfNull(current);
        var localDay = current.Timestamp.ToLocalTime().Date;
        var newDay = previous is null || previous.Timestamp.ToLocalTime().Date != localDay;
        var grouped = !newDay
            && previous is not null
            && current.Role != MdChatMessageRole.System
            && previous.Role == current.Role
            && SenderKey(previous) == SenderKey(current)
            && (current.Timestamp - previous.Timestamp).Duration() <= GroupingWindow;
        var unread = lastReadMessageId is not null && previous?.Id == lastReadMessageId;
        return new MdChatMessageLayout(
            ShowSender: !grouped,
            ShowAvatar: !grouped,
            ShowDateSeparator: newDay,
            DateSeparator: newDay ? localDay : null,
            ShowUnreadDivider: unread);
    }

    private static string SenderKey(MdChatMessage message) => message.Sender ?? message.Role.ToString();
}
