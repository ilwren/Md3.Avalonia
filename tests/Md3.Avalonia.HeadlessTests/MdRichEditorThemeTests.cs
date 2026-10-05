using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaRichEditor.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Md3.Avalonia.RichEditor skins a third-party control by setting properties rather than by
/// replacing its template, so nothing about it fails at build time if a selector stops matching -
/// the styles would simply never apply and the editor would silently keep its stock look. These
/// assert that the Material tokens actually reach the control.
/// </summary>
public sealed class MdRichEditorThemeTests
{
    [AvaloniaFact]
    public void Material_Tokens_Reach_The_Third_Party_Editor()
    {
        var editor = new RichEditor();
        var window = new Window { Width = 600, Height = 400, Content = editor };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            // RichEditor exposes no chrome - no Background, Border or CornerRadius - so the
            // theme styles the ink instead, and these are the only properties any style in this
            // app assigns. The host supplies the container around it.
            Assert.NotNull(editor.SelectionBrush);
            Assert.NotNull(editor.CaretBrush);
            Assert.Equal(16d, editor.DefaultFontSize);

            Assert.True(window.TryFindResource("Md.Sys.Color.SecondaryContainer.Brush", out var selectionToken));
            Assert.True(window.TryFindResource("Md.Sys.Color.Primary.Brush", out var caretToken));
            var selection = Assert.IsAssignableFrom<IBrush>(selectionToken);
            var caret = Assert.IsAssignableFrom<IBrush>(caretToken);
            Assert.Equal(selection.ToString(), editor.SelectionBrush!.ToString());
            Assert.Equal(caret.ToString(), editor.CaretBrush!.ToString());
        }
        finally { window.Close(); }
    }
}
