using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Md3.Avalonia.Extra.Infrastructure;
using Md3.Avalonia.Localization;

namespace Md3.Avalonia.Chat;

/// <summary>Role-aware message bubble used by <see cref="MdChatView"/>.</summary>
[PseudoClasses(":user", ":assistant", ":system", ":selected", ":failed", ":reply", ":text-content", ":attachments", ":streaming", ":grouped", ":dated", ":unread")]
public sealed class MdChatMessagePresenter : ContentControl
{
    public static readonly StyledProperty<MdChatMessage?> MessageProperty =
        AvaloniaProperty.Register<MdChatMessagePresenter, MdChatMessage?>(nameof(Message));
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<MdChatMessagePresenter, bool>(nameof(IsSelected));

    public static readonly DirectProperty<MdChatMessagePresenter, string> FormattedTimeProperty =
        AvaloniaProperty.RegisterDirect<MdChatMessagePresenter, string>(nameof(FormattedTime), control => control.FormattedTime);
    public static readonly StyledProperty<MdChatMessageLayout?> LayoutProperty =
        AvaloniaProperty.Register<MdChatMessagePresenter, MdChatMessageLayout?>(nameof(Layout));
    public static readonly DirectProperty<MdChatMessagePresenter, string> DateSeparatorTextProperty =
        AvaloniaProperty.RegisterDirect<MdChatMessagePresenter, string>(nameof(DateSeparatorText), control => control.DateSeparatorText);
    public static readonly DirectProperty<MdChatMessagePresenter, string> AvatarGlyphProperty =
        AvaloniaProperty.RegisterDirect<MdChatMessagePresenter, string>(nameof(AvatarGlyph), control => control.AvatarGlyph);
    public static readonly DirectProperty<MdChatMessagePresenter, string> SenderInitialsProperty =
        AvaloniaProperty.RegisterDirect<MdChatMessagePresenter, string>(nameof(SenderInitials), control => control.SenderInitials);
    public static readonly DirectProperty<MdChatMessagePresenter, string?> TextContentProperty =
        AvaloniaProperty.RegisterDirect<MdChatMessagePresenter, string?>(nameof(TextContent), control => control.TextContent);
    public static readonly StyledProperty<IMdMarkdownProvider?> MarkdownProviderProperty =
        AvaloniaProperty.Register<MdChatMessagePresenter, IMdMarkdownProvider?>(nameof(MarkdownProvider));
    public static readonly StyledProperty<IMdImageProvider?> ImageProviderProperty =
        AvaloniaProperty.Register<MdChatMessagePresenter, IMdImageProvider?>(nameof(ImageProvider));
    public static readonly DirectProperty<MdChatMessagePresenter, object?> RichContentProperty =
        AvaloniaProperty.RegisterDirect<MdChatMessagePresenter, object?>(nameof(RichContent), control => control.RichContent);
    public static readonly DirectProperty<MdChatMessagePresenter, string> ReplyHeaderTextProperty =
        AvaloniaProperty.RegisterDirect<MdChatMessagePresenter, string>(nameof(ReplyHeaderText), control => control.ReplyHeaderText);
    public static readonly DirectProperty<MdChatMessagePresenter, string> FailureTextProperty =
        AvaloniaProperty.RegisterDirect<MdChatMessagePresenter, string>(nameof(FailureText), control => control.FailureText);

    static MdChatMessagePresenter()
    {
        LayoutProperty.Changed.AddClassHandler<MdChatMessagePresenter>((presenter, _) => presenter.UpdateLayoutState());
        MessageProperty.Changed.AddClassHandler<MdChatMessagePresenter>((presenter, _) => presenter.UpdateRole());
        MarkdownProviderProperty.Changed.AddClassHandler<MdChatMessagePresenter>((presenter, _) => presenter.UpdateRichContent());
        IsSelectedProperty.Changed.AddClassHandler<MdChatMessagePresenter>((presenter, _) => presenter.UpdateRole());
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdChatMessagePresenter>((presenter, _) => presenter.UpdateRole());
    }

    public MdChatMessagePresenter() => UpdateRole();
    public MdChatMessage? Message { get => GetValue(MessageProperty); set => SetValue(MessageProperty, value); }
    public bool IsSelected { get => GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }

    /// <summary>Converts text content into a presentation. When null, text is shown as plain selectable text.</summary>
    public IMdMarkdownProvider? MarkdownProvider { get => GetValue(MarkdownProviderProperty); set => SetValue(MarkdownProviderProperty, value); }

    /// <summary>Loads picture attachments that carry a <see cref="MdChatAttachment.Uri"/>.</summary>
    public IMdImageProvider? ImageProvider { get => GetValue(ImageProviderProperty); set => SetValue(ImageProviderProperty, value); }

    /// <summary>The control or object the bubble displays for the message content. Cached per message.</summary>
    public object? RichContent { get => _richContent; private set => SetAndRaise(RichContentProperty, ref _richContent, value); }

    private object? _richContent;

    /// <summary>How the message is grouped and separated; set by the chat row.</summary>
    public MdChatMessageLayout? Layout { get => GetValue(LayoutProperty); set => SetValue(LayoutProperty, value); }

    private string _dateSeparatorText = string.Empty;

    /// <summary>The local day shown above the message, or empty when there is no date separator.</summary>
    public string DateSeparatorText { get => _dateSeparatorText; private set => SetAndRaise(DateSeparatorTextProperty, ref _dateSeparatorText, value); }

    /// <summary>Localized text of the unread divider.</summary>
    public string UnreadDividerText => MdLocalization.GetString("ChatUnreadDivider", this);

    private void UpdateLayoutState()
    {
        PseudoClasses.Set(":grouped", Layout is { ShowSender: false });
        PseudoClasses.Set(":dated", Layout is { ShowDateSeparator: true });
        PseudoClasses.Set(":unread", Layout is { ShowUnreadDivider: true });
        DateSeparatorText = Layout?.DateSeparator is { } day ? day.ToString("D", CultureInfo.CurrentCulture) : string.Empty;
    }

    public string FormattedTime => Message?.Timestamp.LocalDateTime.ToString("t", CultureInfo.CurrentCulture) ?? string.Empty;
    public string? TextContent => Message?.Content as string;
    public string ReplyHeaderText => MdLocalization.GetString("ChatReplyingToMessage", this);
    public string FailureText => string.IsNullOrWhiteSpace(Message?.ErrorText)
        ? MdLocalization.GetString("ChatSendFailed", this)
        : Message!.ErrorText!;
    public string AvatarGlyph => Message?.AvatarGlyph ?? (Message?.Role == MdChatMessageRole.Assistant ? "\uf06c" : "\ue7fd");
    public string SenderInitials
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Message?.Initials)) return Message.Initials;
            var sender = Message?.Sender?.Trim();
            if (string.IsNullOrEmpty(sender)) return Message?.Role == MdChatMessageRole.Assistant ? "AI" : "U";
            var parts = sender.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[1][0])}";
            return sender.Length > 2 ? sender[..2].ToUpperInvariant() : sender.ToUpperInvariant();
        }
    }

    private void UpdateRichContent()
    {
        var content = Message?.Content;
        var provider = MarkdownProvider;
        var usesProvider = provider is not null && content is string;
        PseudoClasses.Set(":text-content", content is string && !usesProvider);
        object? next = content;
        if (usesProvider && content is string text && Message is not null)
            next = provider!.Render(text, Message.IsStreaming);
        SetAndRaise(RichContentProperty, ref _richContent, next);
    }

    private void UpdateRole()
    {
        PseudoClasses.Set(":user", Message?.Role == MdChatMessageRole.User);
        PseudoClasses.Set(":assistant", Message?.Role == MdChatMessageRole.Assistant);
        PseudoClasses.Set(":system", Message?.Role == MdChatMessageRole.System);
        PseudoClasses.Set(":selected", IsSelected);
        PseudoClasses.Set(":failed", Message?.State == MdAsyncRequestState.Error);
        PseudoClasses.Set(":reply", Message?.ReplyPreview is not null);
        PseudoClasses.Set(":attachments", Message?.Attachments is { Count: > 0 });
        PseudoClasses.Set(":streaming", Message?.IsStreaming == true);
        UpdateRichContent();
        RaisePropertyChanged(FormattedTimeProperty, string.Empty, FormattedTime);
        RaisePropertyChanged(AvatarGlyphProperty, string.Empty, AvatarGlyph);
        RaisePropertyChanged(SenderInitialsProperty, string.Empty, SenderInitials);
        RaisePropertyChanged(TextContentProperty, null, TextContent);
        RaisePropertyChanged(ReplyHeaderTextProperty, string.Empty, ReplyHeaderText);
        RaisePropertyChanged(FailureTextProperty, string.Empty, FailureText);

        var sender = Message?.Sender;
        if (string.IsNullOrWhiteSpace(sender)) sender = Message?.Role switch
        {
            MdChatMessageRole.Assistant => MdLocalization.GetString("Assistant", this),
            MdChatMessageRole.System => MdLocalization.GetString("System", this),
            _ => MdLocalization.GetString("You", this)
        };
        var state = Message?.State == MdAsyncRequestState.Error
            ? $"{MdLocalization.GetString("ChatSendFailed", this)}. {Message.ErrorText}".Trim()
            : Message?.State.ToString();
        var attachmentText = Message?.Attachments is { Count: > 0 } items
            ? MdLocalization.Format("ChatAttachments", this, items.Count)
            : string.Empty;
        AutomationProperties.SetName(this, $"{sender}, {FormattedTime}. {Message?.Content}. {attachmentText} {state}".Replace("  ", " ").Trim());
        AutomationProperties.SetHelpText(this, MdLocalization.GetString(IsSelected ? "SelectedMessage" : "Message", this));
    }
}
