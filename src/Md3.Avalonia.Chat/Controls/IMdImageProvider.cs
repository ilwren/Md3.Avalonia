using Avalonia.Media;

namespace Md3.Avalonia.Chat;

/// <summary>
/// Loads the picture behind an image attachment. The implementation owns network, file and cache
/// I/O; the chat controls only await the result and show a loading, failed or retry state.
/// </summary>
public interface IMdImageProvider
{
    /// <summary>Returns the picture, or null when it cannot be loaded. Throwing is treated the same as null.</summary>
    /// <param name="uri">The <see cref="MdChatAttachment.Uri"/> of the attachment.</param>
    /// <param name="cancellationToken">Cancelled when the attachment is recycled or reloaded.</param>
    Task<IImage?> LoadImageAsync(string uri, CancellationToken cancellationToken);
}
