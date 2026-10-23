using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdMaskedTextBoxTests
{
    [AvaloniaFact]
    public void Mask_Fills_Literals_As_The_Text_Is_Set()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var box = new MdMaskedTextBox { Mask = "(000) 000-0000" };
        using var host = Show(box);

        box.Text = "5551234567";

        Assert.Equal("(555) 123-4567", box.Text);
        Assert.True(box.MaskCompleted == true);
        Assert.True(box.MaskFull == true);
    }

    [AvaloniaFact]
    public void Partial_Input_Keeps_The_Mask_Incomplete()
    {
        var box = new MdMaskedTextBox { Mask = "0000" };
        using var host = Show(box);

        box.Text = "12";

        Assert.Equal("12__", box.Text);
        Assert.True(box.MaskCompleted == false);
        Assert.True(box.MaskFull == false);
    }

    [AvaloniaFact]
    public void Optional_Slots_Show_The_Prompt_Char()
    {
        var box = new MdMaskedTextBox { Mask = "##/##/####" };
        using var host = Show(box);

        box.Text = "1234";

        Assert.Equal("12/34/____", box.Text);
    }

    [AvaloniaFact]
    public void Defaults_Are_A_Filled_Field()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var box = new MdMaskedTextBox { Mask = "00" };
        using var host = Show(box);

        Assert.Equal(MdMaskedTextBoxVariant.Filled, box.Variant);
        Assert.True(box.PseudoClasses.Contains(":filled"));
        Assert.False(box.PseudoClasses.Contains(":outlined"));
        Assert.NotNull(box.Template);
    }

    [AvaloniaFact]
    public void Variant_Switches_The_PseudoClass()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var box = new MdMaskedTextBox { Mask = "00" };
        using var host = Show(box);

        box.Variant = MdMaskedTextBoxVariant.Outlined;

        Assert.True(box.PseudoClasses.Contains(":outlined"));
        Assert.False(box.PseudoClasses.Contains(":filled"));
    }

    [AvaloniaFact]
    public void Field_State_Properties_Drive_The_PseudoClasses()
    {
        var box = new MdMaskedTextBox { Mask = "00" };
        using var host = Show(box);

        Assert.False(box.PseudoClasses.Contains(":has-label"));
        Assert.True(box.PseudoClasses.Contains(":no-label"));
        Assert.False(box.PseudoClasses.Contains(":has-prefix"));
        Assert.False(box.PseudoClasses.Contains(":has-supporting-text"));
        Assert.False(box.PseudoClasses.Contains(":has-error-text"));
        Assert.False(box.PseudoClasses.Contains(":md-error"));

        box.Label = "Phone";
        box.PrefixText = "+1";
        box.SupportingText = "Helper";
        box.ErrorText = "Invalid";
        box.IsError = true;

        Assert.True(box.PseudoClasses.Contains(":has-label"));
        Assert.True(box.PseudoClasses.Contains(":has-prefix"));
        Assert.True(box.PseudoClasses.Contains(":has-supporting-text"));
        Assert.True(box.PseudoClasses.Contains(":has-error-text"));
        Assert.True(box.PseudoClasses.Contains(":md-error"));
    }

    private static IDisposable Show(Control content) => new WindowScope(ShowWindow(content));

    private static Window ShowWindow(Control content)
    {
        var window = new Window { Width = 600, Height = 320, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private sealed class WindowScope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }
}
