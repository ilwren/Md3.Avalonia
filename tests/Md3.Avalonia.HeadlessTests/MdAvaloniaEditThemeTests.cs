using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Covers the Material theme for the AvaloniaEdit text editor, which ships in the opt-in
/// Md3.Avalonia.AvaloniaEdit package.
/// </summary>
public class MdAvaloniaEditThemeTests
{
    [AvaloniaFact]
    public void The_Material_Theme_Templates_Avalonia_Edits_Editor()
    {
        var editor = NewEditor();
        using var host = Show(editor);

        // The editor finds its scroller by name and casts it to ScrollViewer, so the Material
        // replacement has to keep both the name and the type.
        var scroller = Named(editor, "PART_ScrollViewer");
        Assert.IsType<MdScrollViewer>(scroller);

        // The editor has no Fluent/Simple theme anywhere in this test app, so the scrollbars
        // inside that scroller can only come from the core Material theme.
        Assert.NotEmpty(scroller!.GetVisualDescendants().OfType<MdScrollBar>());
        Assert.DoesNotContain(Application.Current!.Styles, style => style.GetType().Name == "FluentTheme");
    }

    [AvaloniaFact]
    public void The_Material_Theme_Applies_M3_Editor_Ink()
    {
        var editor = NewEditor();
        using var host = Show(editor);

        Assert.Equal(Brush(editor, "Md.Sys.Color.OnSurfaceVariant.Brush"), editor.LineNumbersForeground);
        Assert.Equal(Brush(editor, "Md.Sys.Color.TertiaryContainer.Brush"), editor.SearchResultsBrush);
        Assert.Equal(Brush(editor, "Md.Sys.Color.PrimarySelection.Brush"), editor.TextArea.SelectionBrush);
    }

    [AvaloniaFact]
    public void The_Editor_And_Its_Search_Panel_Work_Without_Fluent()
    {
        var editor = NewEditor();
        using var host = Show(editor);

        editor.SearchPanel.Open();
        Dispatcher.UIThread.RunJobs();

        var panel = editor.GetVisualDescendants().OfType<AvaloniaEdit.Search.SearchPanel>().Single();

        // The compact search fields name their own container PART_Border too - that is the name
        // Avalonia's stock TextBox chrome uses - so the panel's surface is the one whose templated
        // parent is the panel itself.
        var border = panel.GetVisualDescendants().OfType<Border>()
            .Single(candidate => candidate.Name == "PART_Border" && ReferenceEquals(candidate.TemplatedParent, panel));
        Assert.Equal(Brush(editor, "Md.Sys.Color.SurfaceContainerHigh.Brush"), border.Background);

        // The parts the panel resolves in code have to exist, and the surface has to be
        // reachable while the editor is read-only or not.
        Assert.NotNull(border.GetVisualDescendants().OfType<TextBox>().SingleOrDefault(box => box.Name == "PART_searchTextBox"));
        Assert.NotNull(border.GetVisualDescendants().OfType<TextBlock>().SingleOrDefault(text => text.Name == "PART_MessageContent"));

        editor.SearchPanel.SearchPattern = "hello";
        editor.SearchPanel.FindNext();
        Dispatcher.UIThread.RunJobs();

        Assert.False(editor.TextArea.Selection.IsEmpty);

        editor.SearchPanel.Close();
        Assert.True(editor.SearchPanel.IsClosed);
    }

    [AvaloniaFact]
    public void Registering_The_Editor_Theme_Does_Not_Replace_Native_Control_Themes()
    {
        // The point of the keyed Md.Comp.AvaloniaEdit.* themes: an application that adds this
        // package must not get Material-looking TextBoxes/ListBoxes everywhere else. Button is the
        // one native type this test application *does* theme, because Md3.Avalonia.Dock has to
        // supply the implicit Button theme Dock's chrome derives from - the exact contract is
        // pinned by MdDockThemeTests.Registering_The_Dock_Theme_Only_Adds_The_Base_Button_Theme_Docks_Chrome_Needs.
        var variant = Application.Current!.ActualThemeVariant;
        foreach (var native in new[] { typeof(ToggleButton), typeof(TextBox), typeof(ListBox), typeof(ListBoxItem) })
        {
            Assert.False(Application.Current.TryGetResource(native, variant, out _), native.Name);
        }

        var box = new TextBox { Text = "plain" };
        var window = new Window { Width = 200, Height = 100, Content = box };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.Null(box.Theme);
        window.Close();
    }

    [AvaloniaFact]
    public void The_Editor_Keeps_Avalonia_Edits_Editing_Behaviour()
    {
        var editor = NewEditor();
        using var host = Show(editor);

        editor.Text = "one\ntwo\nthree";
        editor.CaretOffset = 0;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("one\ntwo\nthree", editor.Text);
        Assert.Equal(3, editor.Document.LineCount);
        Assert.Equal(13, editor.Document.TextLength);

        editor.SelectAll();
        Assert.Equal("one\ntwo\nthree", editor.SelectedText);

        editor.TextArea.Selection = AvaloniaEdit.Editing.Selection.Create(editor.TextArea, 4, 7);
        Assert.Equal("two", editor.SelectedText);
    }

    private static TextEditor NewEditor() => new()
    {
        Width = 520,
        Height = 260,
        ShowLineNumbers = true,
        Text = "hello world"
    };

    private static Visual? Named(Visual root, string name) =>
        root.GetVisualDescendants().FirstOrDefault(visual => (visual as StyledElement)?.Name == name);

    // The tokens live in themed dictionaries, so the lookup has to name the variant the editor
    // is actually rendering in.
    private static IBrush Brush(StyledElement scope, string key)
    {
        Assert.True(Application.Current!.TryGetResource(key, scope.ActualThemeVariant, out var value), key);
        return Assert.IsAssignableFrom<IBrush>(value);
    }

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
