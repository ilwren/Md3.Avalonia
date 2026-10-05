using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Settles whether <c>{TemplateBinding X, Mode=TwoWay}</c> actually writes back in Avalonia.
/// The official docs disagree with themselves: the templated-controls guide and the WPF
/// migration page both say a TemplateBinding is OneWay only and tell you to use
/// <c>{Binding RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}</c> instead, while
/// the WPF cheat sheet says Avalonia supports Mode=TwoWay. The repository leans on the pattern
/// in 41 places across 15 theme files, so if it is OneWay then every one of those public
/// properties is silently stale - the inner element shows the edit and the control never hears
/// about it. One assertion decides it for all of them.
/// </summary>
public sealed class MdTemplateBindingDirectionTests
{
    [AvaloniaFact]
    public void Text_Typed_Into_The_Inner_TextBox_Reaches_The_Control_Property()
    {
        var box = new MdTextBox();
        var window = new Window { Width = 400, Height = 200, Content = box };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var inner = Assert.Single(box.GetVisualDescendants().OfType<TextBox>());

            inner.Text = "typed by the user";
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("typed by the user", box.Text);
        }
        finally { window.Close(); }
    }
}
