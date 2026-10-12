using Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class MarkdownGalleryPage : UserControl
{
    public MarkdownGalleryPage()
    {
        InitializeComponent();
        DocumentViewer.Markdown = DemoDocument;
    }

    /// <summary>
    /// The document the page renders. It is sample content rather than page chrome - the headings,
    /// prose, quote, note, table and code block are what the theme is being shown on - so it is kept
    /// in code and out of the zh localization dictionary on purpose, like any other demo payload.
    /// </summary>
    private const string DemoDocument = """
        # Material 3 typography

        The opt-in `Md3.Avalonia.Markdown` package renders a document with the Material type scale and
        colour roles, so **bold**, *italic* and `inline code` all come from `Md.Sys.*` tokens.

        ## Headings

        ### Heading three

        #### Heading four

        ##### Heading five

        ###### Heading six

        ## Lists

        - Renderer: Markdown.Avalonia
        - Theme: `MaterialMarkdownTheme`
          - Nested item
        - One collection per viewer

        1. First ordered item
        2. Second ordered item

        ## Quotes and notes

        > A blockquote with a [link](https://github.com/ilwren/Md3.Avalonia) inside it.

        < A note block paints the highest surface container, framed by an OutlineVariant rule.

        ## Code

        ```csharp
        // The viewer owns its style collection, so the theme is assigned to it.
        var viewer = new MarkdownScrollViewer();
        viewer.MarkdownStyle = new MaterialMarkdownTheme();
        ```

        ## Tables

        | Element | Role | Token |
        | --- | --- | --- |
        | Prose | Body | `OnSurfaceVariant` |
        | Heading | Headline | `OnSurface` |
        | Code | Code block | `SurfaceContainerLow` |
        | Table header | Label | `SurfaceContainerHighest` |

        ---

        That rule is an `mdc|Rule`, drawn with the OutlineVariant foreground the theme gives it.
        """;
}
