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
        Assert.Contains(":filled", box.Classes);
        Assert.DoesNotContain(":outlined", box.Classes);
        Assert.NotNull(box.Template);
    }

    [AvaloniaFact]
    public void Variant_Switches_The_PseudoClass()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var box = new MdMaskedTextBox { Mask = "00" };
        using var host = Show(box);

        box.Variant = MdMaskedTextBoxVariant.Outlined;

        Assert.Contains(":outlined", box.Classes);
        Assert.DoesNotContain(":filled", box.Classes);
    }

    [AvaloniaFact]
    public void Field_State_Properties_Drive_The_PseudoClasses()
    {
        var box = new MdMaskedTextBox { Mask = "00" };
        using var host = Show(box);

        Assert.DoesNotContain(":has-label", box.Classes);
        Assert.Contains(":no-label", box.Classes);
        Assert.DoesNotContain(":has-prefix", box.Classes);
        Assert.DoesNotContain(":has-supporting-text", box.Classes);
        Assert.DoesNotContain(":has-error-text", box.Classes);
        Assert.DoesNotContain(":md-error", box.Classes);

        box.Label = "Phone";
        box.PrefixText = "+1";
        box.SupportingText = "Helper";
        box.ErrorText = "Invalid";
        box.IsError = true;

        Assert.Contains(":has-label", box.Classes);
        Assert.Contains(":has-prefix", box.Classes);
        Assert.Contains(":has-supporting-text", box.Classes);
        Assert.Contains(":has-error-text", box.Classes);
        Assert.Contains(":md-error", box.Classes);
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
