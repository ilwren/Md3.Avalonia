using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Extra.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdVisualExtrasTests
{
    [AvaloniaFact]
    public void BeforeAfter_Clamps_Position_And_Exposes_Orientation()
    {
        var control = new MdBeforeAfter { Position = 2, Orientation = MdComparisonOrientation.Vertical };
        Assert.Equal(1, control.Position);
        Assert.Equal(MdComparisonOrientation.Vertical, control.Orientation);
        control.Position = -.5;
        Assert.Equal(0, control.Position);
    }

    [AvaloniaFact]
    public void BeforeAfter_Honours_Its_Divider_Brush()
    {
        // DividerBrush was a registered property the template ignored in favour of a literal, so
        // setting it did nothing - defect 6 all over again.
        var tertiary = new SolidColorBrush(Colors.Teal);
        var control = new MdBeforeAfter { DividerBrush = tertiary, DividerThickness = 6 };
        var window = new Window { Width = 400, Height = 300, Content = control };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var divider = control.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "PART_Divider");
            Assert.Same(tertiary, divider.Background);
            Assert.Equal(6, divider.Width);

            // The default is still the primary brush, so nothing that relied on it moved.
            var untouched = new MdBeforeAfter();
            var otherWindow = new Window { Width = 400, Height = 300, Content = untouched };
            otherWindow.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();
                Assert.NotNull(untouched.DividerBrush);
            }
            finally { otherWindow.Close(); }
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void BeforeAfter_Position_Changed_Follows_The_Property_Not_The_Gesture()
    {
        // The event fired only from the pointer handler, so the arrow keys and a two-way binding
        // moved the divider silently.
        var control = new MdBeforeAfter { Position = .5 };
        var raised = 0;
        control.PositionChanged += (_, _) => raised++;

        control.Position = .75;
        Assert.Equal(1, raised);
        control.SetCurrentValue(MdBeforeAfter.PositionProperty, .25);
        Assert.Equal(2, raised);
    }

    [AvaloniaFact]
    public void BeforeAfter_Is_Inert_When_It_Is_Not_Interactive()
    {
        // IsInteractive gated the pointer only, so a display-only comparison was still a tab stop
        // whose divider moved under the arrow keys.
        var control = new MdBeforeAfter { Position = .5, IsInteractive = false };
        var window = new Window { Width = 400, Height = 300, Content = control };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.False(control.Focusable);

            window.KeyPress(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, null);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(.5, control.Position);

            control.IsInteractive = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(control.Focusable);
            Assert.True(control.Focus());
            window.KeyPress(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, null);
            Dispatcher.UIThread.RunJobs();
            Assert.True(control.Position > .5);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void AnimatedText_And_SpinKit_Expose_Provider_Free_APIs()
    {
        var text = new MdAnimatedText { Text = "Avalonia", Effect = MdAnimatedTextEffect.Typewriter, AutoPlay = false };
        text.Start();
        Assert.True(text.IsPlaying);
        text.Stop();
        Assert.False(text.IsPlaying);
        var spinner = new MdSpinKit { Kind = MdSpinKitKind.ThreeBounce, Size = 48, IsActive = false };
        Assert.Equal(48, spinner.Size);
        Assert.False(spinner.IsActive);
    }

    [AvaloniaFact]
    public async Task StaggeredPanel_Can_Play_And_Restore_Children()
    {
        var panel = new MdStaggeredPanel { AutoPlay = false, Stagger = TimeSpan.Zero };
        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
        await panel.PlayAsync();
        Assert.All(panel.Children, child => Assert.Equal(1, child.Opacity));
    }

    [AvaloniaFact]
    public void SlidableItem_Provides_Bindable_Open_State()
    {
        var item = new MdSlidableItem { ActionExtent = 100 };
        item.OpenEnd();
        Assert.True(item.IsOpen);
        Assert.Equal(-100, item.Offset);
        item.Close();
        Assert.False(item.IsOpen);
    }
}
