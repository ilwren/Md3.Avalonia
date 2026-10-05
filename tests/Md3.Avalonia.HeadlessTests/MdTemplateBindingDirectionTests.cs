using Avalonia.Controls;
using Avalonia.Controls.Presenters;
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
/// in 41 places across 15 theme files, so if it were OneWay then every one of those public
/// properties would be silently stale - the inner element shows the edit and the control never
/// hears about it. Avalonia's own stock TextBox theme uses the same construct on its
/// TextPresenter, so the cheat sheet is the accurate page; this pins that down so the 41
/// bindings are not rewritten on the strength of a stale doc, and catches it if it ever changes.
/// </summary>
public sealed class MdTemplateBindingDirectionTests
{
    [AvaloniaFact]
    public void Text_Entered_Through_The_Template_Reaches_The_Control_Property()
    {
        var box = new MdTextBox();
        var window = new Window { Width = 400, Height = 200, Content = box };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var presenter = Assert.Single(box.GetVisualDescendants().OfType<TextPresenter>());

            presenter.Text = "typed by the user";
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("typed by the user", box.Text);
        }
        finally { window.Close(); }
    }
}
