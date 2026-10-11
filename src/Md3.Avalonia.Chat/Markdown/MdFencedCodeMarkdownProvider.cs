using System.Text;
using Avalonia.Controls;
using Avalonia.Media;

namespace Md3.Avalonia.Chat;

/// <summary>
/// Reference provider that renders fenced code (```lang ... ```) as <see cref="MdCodeBlock"/> and the
/// remaining text as selectable wrapped text. It is not a full Markdown engine: applications that need
/// tables, emphasis or links should implement <see cref="IMdMarkdownProvider"/> with their own parser.
/// An unclosed fence, which is normal while a reply streams, runs to the end of the text.
/// </summary>
public sealed class MdFencedCodeMarkdownProvider : IMdMarkdownProvider
{
    private readonly IMdSyntaxHighlighter? _highlighter;

    /// <summary>Creates the provider. Pass a highlighter to colour the code blocks.</summary>
    /// <param name="highlighter">Optional highlighter given to every code block.</param>
    public MdFencedCodeMarkdownProvider(IMdSyntaxHighlighter? highlighter = null) => _highlighter = highlighter;

    /// <inheritdoc />
    public Control Render(string text, bool isStreaming)
    {
        var panel = new StackPanel { Spacing = 8 };
        var prose = new StringBuilder();
        StringBuilder? code = null;
        string? language = null;
        foreach (var line in text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                if (code is null)
                {
                    AddProse(panel, prose);
                    code = new StringBuilder();
                    language = trimmed[3..].Trim();
                }
                else
                {
                    panel.Children.Add(CreateCode(language, code));
                    code = null;
                    language = null;
                }
            }
            else if (code is not null)
            {
                code.Append(line).Append('\n');
            }
            else
            {
                prose.Append(line).Append('\n');
            }
        }
        if (code is not null) panel.Children.Add(CreateCode(language, code));
        AddProse(panel, prose);
        return panel;
    }

    private MdCodeBlock CreateCode(string? language, StringBuilder code) =>
        new() { Language = string.IsNullOrEmpty(language) ? null : language, Code = code.ToString().TrimEnd('\n'), Highlighter = _highlighter };

    private static void AddProse(StackPanel panel, StringBuilder prose)
    {
        var text = prose.ToString().Trim('\n');
        prose.Clear();
        if (text.Length == 0) return;
        panel.Children.Add(new SelectableTextBlock { Text = text, TextWrapping = TextWrapping.Wrap });
    }
}
