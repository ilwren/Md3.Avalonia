using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Md3.Avalonia.Themes;

/// <summary>
/// The Material Design 3 style collection for Markdown.Avalonia.
/// Markdown.Avalonia 文档控件的 Material Design 3 样式集合。
/// </summary>
/// <remarks>
/// <para>
/// Unlike the other Material themes this one is not merged into <c>Application.Styles</c>:
/// <see cref="Markdown.Avalonia.MarkdownScrollViewer"/> picks the collection it renders with itself
/// and inserts it into its own <c>Styles</c>, so the closest collection wins and an application-level
/// one can never restyle a viewer. Assign this collection to the viewer instead:
/// </para>
/// <code>
/// &lt;md:MarkdownScrollViewer&gt;
///   &lt;md:MarkdownScrollViewer.MarkdownStyle&gt;
///     &lt;themes:MaterialMarkdownTheme /&gt;
///   &lt;/md:MarkdownScrollViewer.MarkdownStyle&gt;
/// &lt;/md:MarkdownScrollViewer&gt;
/// </code>
/// <para>
/// Each viewer needs its own instance, exactly like Markdown.Avalonia's built-in collections (their
/// static properties also hand out a fresh object per access). The colours are
/// <c>DynamicResource Md.Sys.*</c> lookups, so the light/dark variant of the core Material theme is
/// followed at runtime; the theme itself ships no colour values.
/// </para>
/// <para>
/// See <c>README.md</c> for the token mapping, the two gaps this collection fills
/// (<c>Heading5</c>/<c>Heading6</c> and the rule's foreground) and what an app that adds
/// Markdown.Avalonia's SyntaxHigh plugin on top needs to know.
/// </para>
/// </remarks>
public class MaterialMarkdownTheme : Styles
{
    /// <summary>Loads the style collection from the XAML of the same name.</summary>
    public MaterialMarkdownTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
