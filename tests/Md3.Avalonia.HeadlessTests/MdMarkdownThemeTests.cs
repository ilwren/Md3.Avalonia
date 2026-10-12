using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ColorTextBlock.Avalonia;
using Markdown.Avalonia;
using Markdown.Avalonia.Controls;
using Md3.Avalonia.Themes;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Covers the Material style collection for Markdown.Avalonia, which ships in the opt-in
/// Md3.Avalonia.Markdown package.
/// </summary>
/// <remarks>
/// This package is shaped unlike the other six. Markdown.Avalonia does not read the application's
/// style tree for its look: MarkdownScrollViewer creates a style collection itself, inserts it into
/// its own <see cref="StyledElement.Styles"/> and keeps its five built-ins closed to third parties,
/// so a Material collection can only arrive through <see cref="MarkdownScrollViewer.MarkdownStyle"/>
/// and belongs to one viewer at a time. Nothing here is registered application-wide, which is what
/// makes the scoping test at the end meaningful.
/// </remarks>
public class MdMarkdownThemeTests
{
    [AvaloniaFact]
    public void The_Viewer_Takes_The_Collection_And_Keeps_A_Material_Selection_Tint()
    {
        // Upstream's own collection is in Styles[0] until MarkdownStyle is assigned; assigning ours
        // replaces it rather than stacking on top, and the built-in collections' internal INamedStyle
        // marker is what makes this property the only seam a third party has.
        var viewer = new MarkdownScrollViewer { Width = 720, Height = 400, Markdown = Document };
        Assert.Equal("MarkdownStyleStandard", Assert.Single(viewer.Styles).GetType().Name);

        var theme = new MaterialMarkdownTheme();
        viewer.MarkdownStyle = theme;
        Assert.Same(theme, Assert.Single(viewer.Styles));

        var window = new Window { Width = 760, Height = 460, Content = viewer };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            // The viewer also paints its own selection rectangles, so it reads the brush off whichever
            // IStyle it was handed (TrySetupSelectionBrush) and falls back to cyan when that yields
            // nothing. This collection sets the property from the token instead; a colour mismatch
            // here would mean the DynamicResource silently missed.
            var selection = Assert.IsAssignableFrom<IBrush>(viewer.SelectionBrush);
            Assert.Equal(ColorOf(Brush("Md.Sys.Color.PrimarySelection.Brush")), ColorOf(selection));
            Assert.NotEqual(Colors.Cyan, ColorOf(selection));

            // The document panel is what carries the class every selector in the collection is scoped
            // to, and it only gets that class once the Markdown has been rendered.
            Assert.Single(viewer.GetVisualDescendants()
                .Where(control => control.Classes.Contains("Markdown_Avalonia_MarkdownViewer")));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void The_Collection_Only_Reads_Tokens_The_Core_Theme_Defines()
    {
        // Every colour and every size in the collection is a DynamicResource lookup, and those lookups
        // are resolved from a style collection that is not part of Application.Styles - so a key that
        // does not exist in both variants of the core theme would silently leave a brush null or a
        // size NaN instead of failing the build.
        var colors = new[]
        {
            "Primary", "Tertiary", "OnSurface", "OnSurfaceVariant", "OutlineVariant", "Error",
            "SurfaceContainerLow", "SurfaceContainerHighest"
        };
        var roles = new[]
        {
            "HeadlineLarge", "HeadlineMedium", "HeadlineSmall", "TitleLarge", "TitleMedium", "TitleSmall",
            "BodyLarge", "BodyMedium", "LabelLarge"
        };

        foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        {
            foreach (var color in colors)
            {
                Assert.Equal(Assert.IsType<Color>(NonNull($"Md.Sys.Color.{color}", variant)),
                    ColorOf(Assert.IsAssignableFrom<ISolidColorBrush>(NonNull($"Md.Sys.Color.{color}.Brush", variant))));
            }

            foreach (var role in roles)
            {
                Assert.True(Size($"Md.Sys.TypeScale.{role}.Size", variant) > 0, role);
                Assert.True(Size($"Md.Sys.TypeScale.{role}.LineHeight", variant) > 0, role);
            }

            Assert.IsType<FontFamily>(NonNull("Md.Sys.Typeface.Plain", variant));
        }
    }

    [AvaloniaFact]
    public void The_Document_Is_Typeset_From_The_Material_Type_Scale()
    {
        using var scope = ShowDocument();
        var blocks = scope.Viewer.GetVisualDescendants().OfType<CTextBlock>().ToArray();

        // Prose is the body role on the medium-emphasis ink. Upstream's Fluent line leaves the ink to
        // inherit and sizes prose at the host's control font size instead.
        var prose = blocks.Where(block => block.Classes.Contains("Paragraph")).ToArray();
        Assert.NotEmpty(prose);
        Assert.All(prose, block =>
        {
            Assert.Equal(Size("Md.Sys.TypeScale.BodyLarge.Size"), block.FontSize);
            Assert.Equal(Size("Md.Sys.TypeScale.BodyLarge.LineHeight"), block.LineHeight);
            Assert.Equal(ColorOf(Brush("Md.Sys.Color.OnSurfaceVariant.Brush")), ColorOf(block.Foreground!));
            Assert.Equal(new Thickness(0, 5), block.Margin);
        });

        // Headings climb the type scale instead of Fluent's multipliers, and they are not all the same
        // role: the Material type scale is Regular down to TitleLarge and Medium from TitleMedium on,
        // which is the same split MdText's own ControlTheme uses. h5/h6 have classes and Heading
        // elements but no style in any built-in collection, so this is the only place they get one.
        AssertHeading(blocks, "Heading1", "HeadlineLarge", FontWeight.Normal, "Md.Sys.Color.OnSurface.Brush");
        AssertHeading(blocks, "Heading2", "HeadlineMedium", FontWeight.Normal, "Md.Sys.Color.OnSurface.Brush");
        AssertHeading(blocks, "Heading3", "HeadlineSmall", FontWeight.Normal, "Md.Sys.Color.OnSurfaceVariant.Brush");
        AssertHeading(blocks, "Heading4", "TitleLarge", FontWeight.Normal, "Md.Sys.Color.OnSurfaceVariant.Brush");
        AssertHeading(blocks, "Heading5", "TitleMedium", FontWeight.Medium, "Md.Sys.Color.OnSurfaceVariant.Brush");
        AssertHeading(blocks, "Heading6", "TitleSmall", FontWeight.Medium, "Md.Sys.Color.OnSurfaceVariant.Brush");

        // Every block is typeset in the core typeface token rather than inheriting the host's font, so
        // a Material document does not depend on the surrounding application's typography.
        var plain = Assert.IsType<FontFamily>(NonNull("Md.Sys.Typeface.Plain"));
        Assert.All(blocks, block => Assert.Equal(plain.Name, block.FontFamily.Name));

        // Spans are logical children of their block, not visuals, so they are found through the
        // logical tree. Links are the Primary role and underlined, exactly what upstream asks for.
        var link = scope.Viewer.GetLogicalDescendants().OfType<CHyperlink>().Single();
        Assert.True(link.IsUnderline);
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.Primary.Brush")), ColorOf(link.Foreground!));

        // The hover ink is the complementary accent upstream asks for. Markdown.Avalonia flips
        // :pointerover from its own hit-testing, which a headless layout cannot reach, but the class
        // collection is the same IPseudoClasses the control itself drives - StyledElement.PseudoClasses
        // is just that collection, protected.
        var pseudo = (IPseudoClasses)link.Classes;
        pseudo.Add(":pointerover");
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.Tertiary.Brush")), ColorOf(link.Foreground!));
        pseudo.Remove(":pointerover");
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.Primary.Brush")), ColorOf(link.Foreground!));

        // Inline code is the highest surface container with the code role's ink and the M3 extra-small
        // corner; upstream tints it with a divide of the accent instead.
        var inlineCode = scope.Viewer.GetLogicalDescendants().OfType<CCode>().Single();
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.SurfaceContainerHighest.Brush")), ColorOf(inlineCode.Background!));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.OnSurface.Brush")), ColorOf(inlineCode.Foreground!));
        Assert.Equal(new CornerRadius(4), inlineCode.CornerRadius);

        // Selecting inside the document uses the same wash the viewer paints with, which is the brush
        // a block would otherwise replace with cyan.
        var selection = ColorOf(Brush("Md.Sys.Color.PrimarySelection.Brush"));
        Assert.All(blocks, block => Assert.Equal(selection, ColorOf(block.SelectionBrush!)));
    }

    [AvaloniaFact]
    public void The_Block_Chrome_Uses_Material_Surfaces_And_Shapes()
    {
        using var scope = ShowDocument();
        var viewer = scope.Viewer;
        var borders = viewer.GetVisualDescendants().OfType<Border>().ToArray();

        // A blockquote is a Primary bar on the same 5 DIP the Fluent line uses, indented by upstream's
        // StackPanel rule.
        var quote = borders.Single(border => border.Classes.Contains("Blockquote"));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.Primary.Brush")), ColorOf(quote.BorderBrush!));
        Assert.Equal(new Thickness(5, 0, 0, 0), quote.BorderThickness);
        var quotePanel = viewer.GetVisualDescendants().OfType<StackPanel>()
            .Single(panel => panel.Classes.Contains("Blockquote"));
        Assert.Equal(new Thickness(10, 5), quotePanel.Margin);

        // Lists keep upstream's 40 DIP gutter and marker margin. The document has one bulleted and one
        // ordered list, so both grids are checked.
        var lists = viewer.GetVisualDescendants().OfType<Grid>()
            .Where(grid => grid.Classes.Contains("List")).ToArray();
        Assert.NotEmpty(lists);
        Assert.All(lists, list => Assert.Equal(new Thickness(40, 0, 0, 0), list.Margin));
        var marker = viewer.GetVisualDescendants().OfType<CTextBlock>().First(block => block.Classes.Contains("ListMarker"));
        Assert.Equal(new Thickness(0, 5, 5, 5), marker.Margin);

        // A fenced code block is a TextBlock inside a ScrollViewer inside the Border, all three tagged
        // CodeBlock. Material takes the two Fluent hairlines over into a card: the low surface
        // container, the extra-small radius and inner padding, with the package's own mono stack.
        var code = borders.Single(border => border.Classes.Contains("CodeBlock") && !border.Classes.Contains("NoContainer"));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.SurfaceContainerLow.Brush")), ColorOf(code.Background!));
        Assert.Equal(new CornerRadius(8), code.CornerRadius);
        Assert.Equal(new Thickness(0), code.BorderThickness);
        Assert.Equal(new Thickness(12, 10), code.Padding);
        Assert.Equal(new Thickness(5, 0, 5, 0), code.Margin);
        var codeText = code.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Classes.Contains("CodeBlock"));
        // The whole fallback stack is carried over: Material has no mono typeface token. FontFamily.Name
        // is only the first family, so the assert reads the collection.
        Assert.Equal("menlo,monaco,consolas,droid sans mono,inconsolata,courier new,monospace,dejavu sans mono",
            string.Join(",", codeText.FontFamily.FamilyNames));
        Assert.Equal(Size("Md.Sys.TypeScale.BodyMedium.Size"), codeText.FontSize);
        Assert.Equal(Size("Md.Sys.TypeScale.BodyMedium.LineHeight"), codeText.LineHeight);
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.OnSurface.Brush")), ColorOf(codeText.Foreground!));

        // A ::: block with no handler is drawn as a code block tagged NoContainer. Upstream only swaps
        // its brush, so the rule below is what keeps the marker - and the Error role - visible on a
        // card whose border thickness is now zero.
        var unhandled = borders.Single(border => border.Classes.Contains("NoContainer"));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.Error.Brush")), ColorOf(unhandled.BorderBrush!));
        Assert.Equal(new Thickness(3), unhandled.BorderThickness);

        // Notes are the other end of the container ladder.
        var note = borders.Single(border => border.Classes.Contains("Note"));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.SurfaceContainerHighest.Brush")), ColorOf(note.Background!));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.OutlineVariant.Brush")), ColorOf(note.BorderBrush!));
        Assert.Equal(new Thickness(5, 0, 5, 0), note.Margin);
        var noteText = viewer.GetVisualDescendants().OfType<CTextBlock>().Single(block => block.Classes.Contains("Note"));
        Assert.Equal(new Thickness(10, 5), noteText.Margin);

        // Tables: an OutlineVariant grid, header cells on the highest container with the label role
        // and its Medium weight, body cells at the denser body size on the zebra container.
        var table = borders.Single(border => border.Classes.Contains("Table"));
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.OutlineVariant.Brush")), ColorOf(table.BorderBrush!));
        Assert.Equal(new Thickness(0, 0, 1, 1), table.BorderThickness);
        Assert.Single(viewer.GetVisualDescendants().OfType<Grid>().Where(grid => grid.Classes.Contains("Table")));

        var cells = viewer.GetVisualDescendants().OfType<Grid>()
            .Single(grid => grid.Classes.Contains("Table")).Children.OfType<Border>().ToArray();
        // A cell is a Border carrying its row's classes and nothing else; the table has two columns,
        // so each row contributes two of them.
        var headerCells = cells.Where(border => border.Classes.Contains("TableHeader")).ToArray();
        Assert.Equal(2, headerCells.Length);
        Assert.All(headerCells, cell =>
        {
            Assert.Equal(ColorOf(Brush("Md.Sys.Color.SurfaceContainerHighest.Brush")), ColorOf(cell.Background!));
            Assert.Equal(new Thickness(1, 1, 0, 0), cell.BorderThickness);
            Assert.Equal(new Thickness(10, 6), cell.Padding);
        });
        var cell = headerCells[0];
        // A header cell is a Grid child, so the generic cell rule matches it too and only the fact that
        // the header rule is declared after it - same priority, no specificity in Avalonia - keeps the
        // LabelLarge role here.
        var headerText = cell.GetVisualDescendants().OfType<CTextBlock>().Single();
        Assert.Equal(Size("Md.Sys.TypeScale.LabelLarge.Size"), headerText.FontSize);
        Assert.Equal(Size("Md.Sys.TypeScale.LabelLarge.LineHeight"), headerText.LineHeight);
        Assert.Equal(FontWeight.Medium, headerText.FontWeight);
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.OnSurface.Brush")), ColorOf(headerText.Foreground!));

        var zebra = borders.Where(border => border.Classes.Contains("EvenTableRow")).ToArray();
        Assert.Equal(2, zebra.Length);
        Assert.All(zebra, cell => Assert.Equal(ColorOf(Brush("Md.Sys.Color.SurfaceContainerLow.Brush")), ColorOf(cell.Background!)));
        var cellText = zebra[0].GetVisualDescendants().OfType<CTextBlock>().Single();
        Assert.Equal(Size("Md.Sys.TypeScale.BodyMedium.Size"), cellText.FontSize);
        Assert.Equal(Size("Md.Sys.TypeScale.BodyMedium.LineHeight"), cellText.LineHeight);
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.OnSurfaceVariant.Brush")), ColorOf(cellText.Foreground!));

        // A rule draws itself with Foreground, which upstream leaves to the window's ink; without this
        // the divider is the same colour as prose.
        var rule = viewer.GetVisualDescendants().OfType<Rule>().Single();
        Assert.Equal(ColorOf(Brush("Md.Sys.Color.OutlineVariant.Brush")), ColorOf(rule.Foreground!));
        Assert.Equal(new Thickness(0, 3), rule.Margin);
    }

    [AvaloniaFact]
    public void Assigning_The_Collection_Leaves_The_Application_And_Native_Controls_Alone()
    {
        // The collection lives in one viewer's Styles and every selector in it is scoped to that
        // viewer's document class, so it cannot reach anything else - which is what makes it safe to
        // scope per page instead of registering it application-wide. (Button is deliberately absent
        // from the list below: the Dock package carries the implicit Button theme, and
        // MdDockThemeTests owns that contract.)
        var stylesBefore = Application.Current!.Styles.Count;
        using var scope = ShowDocument();

        Assert.Equal(stylesBefore, Application.Current.Styles.Count);
        Assert.DoesNotContain(Application.Current.Styles, style => style is MaterialMarkdownTheme);
        Assert.DoesNotContain(Application.Current.Styles,
            style => style.GetType().Namespace?.StartsWith("Markdown.Avalonia") == true);

        var variant = Application.Current.ActualThemeVariant;
        foreach (var native in new[] { typeof(TextBox), typeof(ScrollViewer), typeof(ListBox), typeof(ToggleButton) })
        {
            Assert.False(Application.Current.TryGetResource(native, variant, out _), native.Name);
        }

        // A viewer whose MarkdownStyle is left alone keeps the built-in collection Markdown.Avalonia
        // picked for itself, and gets its own instance of it.
        var untouched = new MarkdownScrollViewer();
        Assert.Equal("MarkdownStyleStandard", Assert.Single(untouched.Styles).GetType().Name);
        Assert.IsNotType<MaterialMarkdownTheme>(untouched.Styles[0]);
        Assert.NotSame(scope.Viewer.Styles[0], untouched.Styles[0]);
    }

    /// <summary>
    /// The document the rendered-role assertions are made on: all six heading levels, prose with inline
    /// code and a link, a quote, both list kinds, a fenced code block, a table, a note, a container
    /// block with no handler, and a rule. The table cells are deliberately plain text so that the
    /// inline-code and hyperlink roles have exactly one instance each to look at.
    /// </summary>
    private const string Document = """
        # Heading one

        A paragraph with `inline code` and a [link](https://example.com).

        ## Heading two

        ### Heading three

        #### Heading four

        ##### Heading five

        ###### Heading six

        > A quote with **emphasis**.

        - First bullet
        - Second bullet

        1. First ordered item

        ```csharp
        var theme = new MaterialMarkdownTheme();
        ```

        | Element | Token |
        | --- | --- |
        | Prose | OnSurfaceVariant |
        | Heading | OnSurface |

        < A note block.

        ::: warning
        A container block without a handler keeps upstream's red marker.
        :::

        ---
        """;

    private static void AssertHeading(CTextBlock[] blocks, string className, string role, FontWeight weight, string ink)
    {
        var heading = blocks.Single(block => block.Classes.Contains(className));
        Assert.Equal(Size($"Md.Sys.TypeScale.{role}.Size"), heading.FontSize);
        Assert.Equal(Size($"Md.Sys.TypeScale.{role}.LineHeight"), heading.LineHeight);
        Assert.Equal(weight, heading.FontWeight);
        Assert.Equal(ColorOf(Brush(ink)), ColorOf(heading.Foreground!));
    }

    private static Scope ShowDocument()
    {
        var viewer = new MarkdownScrollViewer
        {
            Width = 720,
            Height = 640,
            MarkdownStyle = new MaterialMarkdownTheme(),
            Markdown = Document
        };
        var window = new Window { Width = 760, Height = 700, Content = viewer };
        window.Show();
        viewer.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        viewer.UpdateLayout();
        return new Scope(window, viewer);
    }

    private sealed class Scope(Window window, MarkdownScrollViewer viewer) : IDisposable
    {
        public Window Window { get; } = window;
        public MarkdownScrollViewer Viewer { get; } = viewer;
        public void Dispose() => Window.Close();
    }

    // Md.Sys.* tokens live in the core theme's dictionaries, which TestApplication registers, and they
    // are read from the same resource host as the other theme tests use.
    private static IBrush Brush(string key) => Assert.IsAssignableFrom<IBrush>(NonNull(key));

    private static double Size(string key, ThemeVariant? variant = null) =>
        Assert.IsType<double>(NonNull(key, variant));

    private static object NonNull(string key, ThemeVariant? variant = null)
    {
        variant ??= Application.Current!.ActualThemeVariant;
        Assert.True(Application.Current!.TryGetResource(key, variant, out var value), key);
        return value!;
    }

    private static Color ColorOf(IBrush brush) => Assert.IsType<SolidColorBrush>(brush).Color;
}
