using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
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
