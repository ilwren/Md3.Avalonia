using Avalonia.Controls;
using Avalonia.Media;

namespace Md3.Avalonia.Chat;

/// <summary>Reference provider that shows message text unchanged, with selectable wrapped text.</summary>
public sealed class MdPlainTextMarkdownProvider : IMdMarkdownProvider
{
    /// <inheritdoc />
    public Control Render(string text, bool isStreaming) => new SelectableTextBlock
    {
        Text = text,
        TextWrapping = TextWrapping.Wrap,
    };
}
