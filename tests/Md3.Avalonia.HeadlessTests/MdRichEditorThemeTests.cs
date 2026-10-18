using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
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

            // RichEditor exposes no chrome - no Background, Border or CornerRadius - so the theme
            // styles the ink instead and the host supplies the container. Asserting against
            // upstream's own defaults is what makes this a real check: if the selector ever stops
            // matching, the styles silently do not apply and the control keeps the stock look.
            // Upstream defaults are SelectionBrush #500078D7 (Windows blue), CaretBrush black,
            // DefaultForeground black, and DefaultFontSize 10 - in points, not DIP.
            Assert.NotEqual("#500078d7", editor.SelectionBrush.ToString()!.ToLowerInvariant());
            Assert.NotEqual("#ff000000", editor.CaretBrush.ToString()!.ToLowerInvariant());
            Assert.NotEqual("#ff000000", editor.DefaultForeground.ToString()!.ToLowerInvariant());

            // M3 Body Large is 16 DIP, which is 12pt. Getting this wrong renders a third too large.
            Assert.Equal(12d, editor.DefaultFontSize);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Default_Foreground_Follows_OnSurface_When_Theme_Changes()
    {
        var application = Application.Current!;
        var previousVariant = application.RequestedThemeVariant;
        var editor = new RichEditor();
        var window = new Window { Width = 600, Height = 400, Content = editor };
        window.Show();
        try
        {
            foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                application.RequestedThemeVariant = variant;
                Dispatcher.UIThread.RunJobs();

                Assert.True(application.TryGetResource(
                    "Md.Sys.Color.OnSurface.Brush", variant, out var expected));
                Assert.Equal(expected, editor.DefaultForeground);
                if (variant == ThemeVariant.Dark)
                    Assert.NotEqual("#ff000000", editor.DefaultForeground.ToString()!.ToLowerInvariant());
            }
        }
        finally
        {
            window.Close();
            application.RequestedThemeVariant = previousVariant;
            Dispatcher.UIThread.RunJobs();
        }
    }
}
