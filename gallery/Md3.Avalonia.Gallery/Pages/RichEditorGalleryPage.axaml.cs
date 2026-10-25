using Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Extra.Infrastructure;

namespace Md3.Avalonia.Gallery.Pages;

public partial class RichEditorGalleryPage : UserControl
{
    private readonly MdTextBoxRichEditorAdapter _editorAdapter;
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public RichEditorGalleryPage()
    {
        InitializeComponent();
        _editorAdapter = new MdTextBoxRichEditorAdapter(EditorText, RichPreview);
        RichEditor.Adapter = _editorAdapter;
    }

    private void EditorStateChanged(object? sender, EventArgs e) =>
        EditorStatus.Text = L($"Executed {_editorAdapter.LastCommand}; current text length {EditorText.Text?.Length ?? 0}.", $"已执行 {_editorAdapter.LastCommand}；当前文本长度为 {EditorText.Text?.Length ?? 0}。");
}
