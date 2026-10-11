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

public enum MdChatMessageRole { User, Assistant, System }
public sealed record MdChatMessage(
    string Id,
    MdChatMessageRole Role,
    object? Content,
    DateTimeOffset Timestamp,
    string? Sender = null,
    MdAsyncRequestState State = MdAsyncRequestState.Data,
    string? ReplyToId = null,
    object? ReplyPreview = null,
    string? ErrorText = null,
    string? AvatarGlyph = null,
    string? StatusGlyph = null,
    string? Initials = null,
    IReadOnlyList<MdChatAttachment>? Attachments = null,
    bool IsStreaming = false);

/// <summary>Kind of a chat attachment; it selects the bubble presentation.</summary>
public enum MdChatAttachmentKind { Image, File, Audio, Video }

/// <summary>
/// One attachment shown under a chat message. Pass the decoded picture as <see cref="Image"/>, or pass a
/// <see cref="Uri"/> and let <see cref="IMdImageProvider"/> load it. The controls perform no network or file I/O.
/// </summary>
public sealed record MdChatAttachment(
    string Id,
    MdChatAttachmentKind Kind,
    string Name,
    long? SizeBytes = null,
    IImage? Image = null,
    string? Uri = null)
{
    /// <summary>True when the attachment is shown as an inline picture, even before it has loaded.</summary>
    public bool IsImage => Kind == MdChatAttachmentKind.Image;

    /// <summary>True when the attachment is shown as a file card.</summary>
    public bool IsFile => !IsImage;

    /// <summary>Material Symbols glyph used by the file card.</summary>
    public string Glyph => Kind switch
    {
        MdChatAttachmentKind.Audio => "\ueb82",
        MdChatAttachmentKind.Video => "\ue404",
        _ => "\ue66d",
    };

    /// <summary>Human-readable size, or an empty string when the size is unknown.</summary>
    public string SizeText => SizeBytes is { } bytes ? FormatBytes(bytes) : string.Empty;

    private static string FormatBytes(long bytes) => bytes switch
    {
        < 1024 => bytes.ToString(CultureInfo.InvariantCulture) + " B",
        < 1024L * 1024 => (bytes / 1024.0).ToString("0.#", CultureInfo.CurrentCulture) + " KB",
        _ => (bytes / (1024.0 * 1024)).ToString("0.#", CultureInfo.CurrentCulture) + " MB",
    };
}
