using Avalonia.Controls;

namespace Md3.Avalonia.Chat;

/// <summary>
/// Turns the text of a chat message into presentation. The provider decides what Markdown means
/// (tables, code blocks, links, math); the chat controls only host the returned control.
/// </summary>
/// <remarks>Called on the UI thread. Implementations must not block on network or file I/O.</remarks>
public interface IMdMarkdownProvider
{
    /// <summary>Creates the content for a text message. The caller owns the returned control.</summary>
    /// <param name="text">The message text.</param>
    /// <param name="isStreaming">True while the message is still being generated.</param>
    Control Render(string text, bool isStreaming);
}
