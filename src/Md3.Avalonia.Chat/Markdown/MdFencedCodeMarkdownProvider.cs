using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Md3.Avalonia.Chat;

/// <summary>
/// Reference provider that renders fenced code (```lang ... ```) as <see cref="MdCodeBlock"/> and the
/// remaining text as selectable wrapped text. Web addresses (http and https) in the text become links that
/// are passed to the <see cref="IMdLinkOpener"/>. It is not a full Markdown engine: applications that need
/// tables or emphasis should implement <see cref="IMdMarkdownProvider"/> with their own parser.
/// An unclosed fence, which is normal while a reply streams, runs to the end of the text.
/// </summary>
public sealed class MdFencedCodeMarkdownProvider : IMdMarkdownProvider
{
    private static readonly Regex LinkPattern = new(@"https?://[^\s<>\[\]()""']*[^\s<>\[\]()""'.,;:!?]", RegexOptions.CultureInvariant);

    private readonly IMdSyntaxHighlighter? _highlighter;
    private readonly IMdLinkOpener? _linkOpener;

    /// <summary>Creates the provider.</summary>
    /// <param name="highlighter">Optional highlighter given to every code block.</param>
    /// <param name="linkOpener">Optional opener for web links in the text. Without one, links do nothing when clicked.</param>
    public MdFencedCodeMarkdownProvider(IMdSyntaxHighlighter? highlighter = null, IMdLinkOpener? linkOpener = null)
    {
        _highlighter = highlighter;
        _linkOpener = linkOpener;
    }

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
        // While streaming, or when the author left the fence open, the code block runs to the end.
        if (code is not null) panel.Children.Add(CreateCode(language, code));
        AddProse(panel, prose);
        return panel;
    }

    private MdCodeBlock CreateCode(string? language, StringBuilder code) =>
        new() { Language = string.IsNullOrEmpty(language) ? null : language, Code = code.ToString().TrimEnd('\n'), Highlighter = _highlighter };

    private void AddProse(StackPanel panel, StringBuilder prose)
    {
        var text = prose.ToString().Trim('\n');
        prose.Clear();
        if (text.Length == 0) return;
        var block = new SelectableTextBlock { TextWrapping = TextWrapping.Wrap };
        var matches = LinkPattern.Matches(text);
        if (matches.Count == 0)
        {
            block.Text = text;
            panel.Children.Add(block);
            return;
        }
        var last = 0;
        foreach (Match match in matches)
        {
            if (match.Index > last) block.Inlines?.Add(new Run { Text = text[last..match.Index] });
            block.Inlines?.Add(new InlineUIContainer { Child = CreateLink(match.Value) });
            last = match.Index + match.Length;
        }
        if (last < text.Length) block.Inlines?.Add(new Run { Text = text[last..] });
        panel.Children.Add(block);
    }

    private HyperlinkButton CreateLink(string address)
    {
        var link = new HyperlinkButton { Content = address };
        link.Click += (_, _) =>
        {
            if (Uri.TryCreate(address, UriKind.Absolute, out var uri)) _linkOpener?.Open(uri);
        };
        return link;
    }
}
