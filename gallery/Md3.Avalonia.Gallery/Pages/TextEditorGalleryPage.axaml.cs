using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit;

namespace Md3.Avalonia.Gallery.Pages;

public partial class TextEditorGalleryPage : UserControl
{
    private const string Sample =
        """
        using AvaloniaEdit;
        using AvaloniaEdit.Search;

        // AvaloniaEdit, wearing the Md3.Avalonia.AvaloniaEdit theme.
        var editor = new TextEditor
        {
            ShowLineNumbers = true,
            Text = "Material Design 3 - Avalonia"
        };

        editor.SearchPanel.Open();
        editor.SearchPanel.SearchPattern = "Material";
        editor.SearchPanel.FindNext();

        Console.WriteLine($"{editor.Document.LineCount} lines");
        """;

    public TextEditorGalleryPage()
    {
        InitializeComponent();
        Editor.Text = Sample;
        Editor.CaretOffset = 0;
    }

    // The find panel is one of the surfaces the theme retemplates, so opening it from a Material
    // button is the quickest way to see the themed version.
    private void OpenFindPanel(object? sender, RoutedEventArgs e) => Find("Material");

    private void Find(string pattern)
    {
        Editor.SearchPanel.Open();
        Editor.SearchPanel.SearchPattern = pattern;
        Editor.SearchPanel.FindNext();
        Editor.Focus();
    }
}
