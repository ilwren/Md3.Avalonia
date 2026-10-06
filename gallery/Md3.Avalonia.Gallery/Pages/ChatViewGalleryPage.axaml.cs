using System.Collections.ObjectModel;
using Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Extra.Infrastructure;

namespace Md3.Avalonia.Gallery.Pages;

public partial class ChatViewGalleryPage : UserControl
{
    private readonly ObservableCollection<MdChatMessage> _messages =
    [
        new("welcome", MdChatMessageRole.Assistant, L("Welcome. Select this bubble, quote it, or combine it with another selection.", "欢迎。选择此气泡后可引用它，也可与其他气泡多选。"), DateTimeOffset.Now.AddMinutes(-4), L("Material assistant", "Material 助手")),
        new("question", MdChatMessageRole.User, L("Can the chat preserve provider-neutral message actions?", "聊天能否保留与提供方无关的消息操作？"), DateTimeOffset.Now.AddMinutes(-3), L("You", "你")),
        new("answer", MdChatMessageRole.Assistant, L("Yes. Selection, quote, delete, and retry are surfaced as commands and events.", "可以。选择、引用、删除和重试均通过命令与事件公开。"), DateTimeOffset.Now.AddMinutes(-2), L("Material assistant", "Material 助手"), ReplyToId: "question", ReplyPreview: L("Can the chat preserve provider-neutral message actions?", "聊天能否保留与提供方无关的消息操作？")),
        new("failed", MdChatMessageRole.User, L("Send the release summary", "发送发布摘要"), DateTimeOffset.Now.AddMinutes(-1), L("You", "你"), MdAsyncRequestState.Error, ErrorText: L("Offline · not sent", "离线 · 未发送"))
    ];
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public ChatViewGalleryPage()
    {
        InitializeComponent();
        Chat.MessagesSource = _messages;
        Chat.SuggestionsSource = new[] { L("Show accessibility", "显示无障碍信息"), L("Explain paging", "解释分页"), L("Open docs", "打开文档") };
        Chat.AttachmentRequested += ChatAttachmentRequested;
    }

    private void ChatSubmitted(object? sender, string text)
    {
        var quote = Chat.QuotedMessage;
        _messages.Add(new MdChatMessage(Guid.NewGuid().ToString("N"), MdChatMessageRole.User, text, DateTimeOffset.Now, L("You", "你"),
            ReplyToId: quote?.Id, ReplyPreview: quote?.Content));
        _messages.Add(new MdChatMessage(Guid.NewGuid().ToString("N"), MdChatMessageRole.Assistant, L($"Provider adapter received: {text}", $"提供方适配器已收到：{text}"), DateTimeOffset.Now, L("Demo adapter", "演示适配器")));
        ChatStatus.Text = quote is null
            ? L($"Sent “{text}”; demo provider appended a response.", $"已发送“{text}”；演示提供方已追加回复。")
            : L($"Sent a reply to {quote.Sender}.", $"已向 {quote.Sender} 发送引用回复。");
    }

    private void ChatSelectionChanged(object? sender, IReadOnlyList<MdChatMessage> messages) =>
        ChatStatus.Text = messages.Count == 0
            ? L("Bubble selection cleared.", "已清除气泡选择。")
            : L($"{messages.Count} bubble{(messages.Count == 1 ? string.Empty : "s")} selected; use Quote, Delete, or Cancel.", $"已选择 {messages.Count} 个气泡；可引用、删除或取消。");

    private void ChatDeleteRequested(object? sender, IReadOnlyList<MdChatMessage> messages)
    {
        foreach (var message in messages) _messages.Remove(message);
        ChatStatus.Text = L($"Deleted {messages.Count} selected message{(messages.Count == 1 ? string.Empty : "s")}.", $"已删除 {messages.Count} 条所选消息。");
    }

    private void ChatQuoteRequested(object? sender, MdChatMessage message) =>
        ChatStatus.Text = L($"Replying to {message.Sender}; submit or cancel the quote preview.", $"正在回复 {message.Sender}；请发送或取消引用预览。");

    private void ChatRetryRequested(object? sender, MdChatMessage message)
    {
        var index = _messages.IndexOf(message);
        if (index >= 0) _messages[index] = message with { State = MdAsyncRequestState.Data, ErrorText = null, Timestamp = DateTimeOffset.Now };
        ChatStatus.Text = L($"Retried “{message.Content}”; provider marked it sent.", $"已重试“{message.Content}”；提供方已将其标记为已发送。");
    }

    private void ChatAttachmentRequested(object? sender, EventArgs e)
    {
        _messages.Add(new MdChatMessage(Guid.NewGuid().ToString("N"), MdChatMessageRole.User, L("📎 Release-notes.md (24 KB)", "📎 发布说明.md (24 KB)"), DateTimeOffset.Now, L("You", "你")));
        ChatStatus.Text = L("Attachment action invoked; sample attachment file added.", "已调用附件操作；已添加示例附件文件。");
    }
}
