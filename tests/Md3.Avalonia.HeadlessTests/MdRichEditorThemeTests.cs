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

            // The stock control leaves these unset or Fluent-coloured; the Material theme is the
            // only thing in this app that assigns them.
            Assert.NotNull(editor.Background);
            Assert.NotNull(editor.CaretBrush);
            Assert.Equal(new Avalonia.CornerRadius(12), editor.CornerRadius);
            Assert.Equal(new Avalonia.Thickness(16), editor.Padding);

            var expected = (IBrush?)window.FindResource("Md.Sys.Color.SurfaceContainerLow.Brush");
            Assert.NotNull(expected);
            Assert.Equal(expected!.ToString(), editor.Background!.ToString());
        }
        finally { window.Close(); }
    }
}
