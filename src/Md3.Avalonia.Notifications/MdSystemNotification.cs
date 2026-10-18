namespace Md3.Avalonia.Notifications;

/// <summary>A simple native system notification with a title and body. / 包含标题与正文的原生系统通知消息。</summary>
public sealed record MdSystemNotification
{
    /// <summary>Creates a system notification.</summary>
    /// <param name="title">The notification title. It cannot be blank.</param>
    /// <param name="body">The notification body; an empty body is allowed.</param>
    public MdSystemNotification(string title, string body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(body);
        Title = title;
        Body = body;
    }

    /// <summary>Gets the title displayed by the operating system.</summary>
    public string Title { get; }

    /// <summary>Gets the body displayed by the operating system.</summary>
    public string Body { get; }
}
