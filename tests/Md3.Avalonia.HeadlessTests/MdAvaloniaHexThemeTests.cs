using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaHex;
using AvaloniaHex.Document;
using AvaloniaHex.Editing;
using AvaloniaHex.Rendering;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Covers the Material theme for the AvaloniaHex hex editor, which ships in the opt-in
/// Md3.Avalonia.AvaloniaHex package.
/// </summary>
public class MdAvaloniaHexThemeTests
{
    [AvaloniaFact]
    public void The_Material_Theme_Templates_The_Hex_Editor()
    {
        var editor = NewEditor();
        using var host = Show(editor);

        // The editor looks its scroller up by name, hands it the HexView and casts it to
        // ScrollViewer, so the Material replacement has to keep the name, the type and the
        // content contract.
        var scroller = Assert.IsAssignableFrom<ScrollViewer>(Named(editor, "PART_ScrollViewer"));
        Assert.IsType<MdScrollViewer>(scroller);
        Assert.Same(editor.HexView, scroller.Content);

        // This test app carries no Fluent/Simple theme, so the scrollbars around the hex view can
        // only come from the core Material theme.
        Assert.NotEmpty(scroller.GetVisualDescendants().OfType<MdScrollBar>());
        Assert.DoesNotContain(Application.Current!.Styles, style => style.GetType().Name == "FluentTheme");
    }

    [AvaloniaFact]
    public void The_Material_Theme_Applies_M3_Hex_Ink()
    {
        var editor = NewEditor();
        using var host = Show(editor);

        // Bytes are onSurface; the address column and the column headers are onSurfaceVariant.
        Assert.Equal(Brush(editor, "HexForegroundBase"), editor.Foreground);
        Assert.Equal(ColorOf(Brush(editor, "Md.Sys.Color.OnSurface.Brush")), ColorOf(editor.Foreground!));

        var offset = editor.Columns.OfType<OffsetColumn>().Single();
        Assert.Equal(Brush(editor, "OffsetForeground"), offset.Foreground);
        Assert.Equal(ColorOf(Brush(editor, "Md.Sys.Color.OnSurfaceVariant.Brush")), ColorOf(offset.Foreground!));

        // The upstream metrics survive the retoken: column padding and header padding are the
        // values upstream's base theme sets.
        Assert.Equal(30d, editor.ColumnPadding);
        Assert.Equal(new Thickness(0, 3, 0, 5), editor.HexView.HeaderPadding);
        Assert.Equal(Brush(editor, "HeaderBackground"),
            editor.HexView.Layers.OfType<HeaderLayer>().Single().HeaderBackground);

        // Selection and caret use the same primary wash as the rest of the library.
        var selection = editor.HexView.Layers.OfType<SelectionLayer>().Single();
        Assert.Equal(ColorOf(Brush(editor, "Md.Sys.Color.PrimarySelection.Brush")),
            ColorOf(selection.PrimarySelectionBackground!));

        var caret = editor.HexView.Layers.OfType<CaretLayer>().Single();
        var caretPen = Assert.IsAssignableFrom<IPen>(caret.PrimaryColumnBorder);
        Assert.Equal(ColorOf(Brush(editor, "Md.Sys.Color.Primary.Brush")), ColorOf(caretPen.Brush!));
        Assert.Same(Brush(editor, "PrimaryCaretBackground"), caret.PrimaryColumnBackground);
    }

    [AvaloniaFact]
    public void Registering_The_Hex_Theme_Does_Not_Replace_Native_Control_Themes()
    {
        // An application that adds this package must not get Material-looking Buttons, TextBoxes
        // or ScrollViewers everywhere else: every selector in the theme targets an AvaloniaHex
        // type.
        var variant = Application.Current!.ActualThemeVariant;
        foreach (var native in new[] { typeof(Button), typeof(TextBox), typeof(ScrollViewer), typeof(ListBox) })
        {
            Assert.False(Application.Current.TryGetResource(native, variant, out _), native.Name);
        }

        var button = new Button { Content = "plain" };
        var window = new Window { Width = 200, Height = 100, Content = button };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.Null(button.Theme);
        window.Close();
    }

    [AvaloniaFact]
    public void The_Hex_Editor_Keeps_AvaloniaHexs_Editing_Behaviour()
    {
        var editor = NewEditor();
        using var host = Show(editor);

        Assert.Equal(32ul, editor.Document!.Length);

        editor.Selection.SelectAll();
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(0ul, editor.Selection.Range.Start.ByteIndex);
        Assert.Equal(editor.Document.Length, editor.Selection.Range.End.ByteIndex);

        editor.Caret.Location = new BitLocation(8);
        editor.ResetSelection();
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(8ul, editor.Caret.Location.ByteIndex);
        Assert.Equal(editor.Caret.Location.ByteIndex, editor.Selection.Range.Start.ByteIndex);

        var changes = 0;
        editor.DocumentChanged += (_, _) => changes++;
        editor.Document = new MemoryBinaryDocument(new byte[] { 1, 2, 3 });
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, changes);
        Assert.Equal(3ul, editor.Document!.Length);
    }

    private static HexEditor NewEditor()
    {
        var editor = new HexEditor
        {
            Width = 520,
            Height = 260,
            Document = new MemoryBinaryDocument(new byte[32])
        };
        editor.Columns.Add(new OffsetColumn());
        editor.Columns.Add(new HexColumn());
        editor.Columns.Add(new AsciiColumn());
        return editor;
    }

    private static Visual? Named(Visual root, string name) =>
        root.GetVisualDescendants().FirstOrDefault(visual => (visual as StyledElement)?.Name == name);

    // The tokens live in themed dictionaries, so the lookup has to name the variant the editor is
    // actually rendering in.
    private static IBrush Brush(StyledElement scope, string key)
    {
        Assert.True(Application.Current!.TryGetResource(key, scope.ActualThemeVariant, out var value), key);
        return Assert.IsAssignableFrom<IBrush>(value);
    }

    private static Color ColorOf(IBrush brush) =>
        Assert.IsAssignableFrom<ISolidColorBrush>(brush).Color;

    private static IDisposable Show(Control content)
    {
        var window = new Window { Width = 700, Height = 400, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new Scope(window);
    }

    private sealed class Scope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }
}
