using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdButtonTests
{
    [AvaloniaFact]
    public void Defaults_Match_Material_Small_Filled_Button()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var button = new MdButton { Content = "Save" };
        using var window = Show(button);

        Assert.Equal(MdButtonVariant.Filled, button.Variant);
        Assert.Equal(MdButtonSize.Small, button.Size);
        Assert.Equal(MdButtonShape.Round, button.Shape);
        Assert.Equal(40, button.ContainerHeight);
        Assert.Equal(new CornerRadius(20), button.ContainerCornerRadius);
        Assert.Equal(20, button.IconSize);
        Assert.Equal(8, button.IconSpacing);
        Assert.Equal(48, button.MinHeight);
        Assert.NotNull(button.Template);
    }

    [AvaloniaFact]
    public void Medium_Square_Uses_Expressive_Size_And_Shape_Tokens()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var button = new MdButton
        {
            Content = "Continue",
            Size = MdButtonSize.Medium,
            Shape = MdButtonShape.Square
        };
        using var window = Show(button);

        Assert.Equal(56, button.ContainerHeight);
        Assert.Equal(new CornerRadius(16), button.ContainerCornerRadius);
        Assert.Equal(24, button.IconSize);
        Assert.Equal(new Thickness(24, 0), button.Padding);
    }

    [AvaloniaFact]
    public void Theme_Does_Not_Replace_Avalonia_Button()
    {
        var panel = new StackPanel();
        var materialButton = new MdButton { Content = "Material" };
        var avaloniaButton = new Button { Content = "Avalonia" };
        panel.Children.Add(materialButton);
        panel.Children.Add(avaloniaButton);
        using var window = Show(panel);

        var materialBackground = Assert.IsType<SolidColorBrush>(materialButton.Background);
        Assert.Equal(Color.Parse("#6750A4"), materialBackground.Color);
        Assert.Null(avaloniaButton.Background);
    }

    [AvaloniaFact]
    public void Tonal_Colors_Resolve_From_Light_Theme_Dictionary()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var button = new MdButton
        {
            Content = "Tonal",
            Variant = MdButtonVariant.Tonal
        };
        using var window = Show(button);

        var background = Assert.IsType<SolidColorBrush>(button.Background);
        var foreground = Assert.IsType<SolidColorBrush>(button.Foreground);
        Assert.Equal(Color.Parse("#E8DEF8"), background.Color);
        Assert.Equal(Color.Parse("#1D192B"), foreground.Color);
    }

    [AvaloniaFact]
    public void Icon_Slots_Accept_Arbitrary_Content()
    {
        var leading = new TextBlock { Text = "+" };
        var trailing = new TextBlock { Text = "→" };
        var button = new MdButton
        {
            Content = "Create",
            LeadingIcon = leading,
            TrailingIcon = trailing
        };
        using var window = Show(button);

        Assert.Same(leading, button.LeadingIcon);
        Assert.Same(trailing, button.TrailingIcon);
    }

    [AvaloniaFact]
    public void Button_Matrix_Can_Render_To_Bitmap()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;

        var variants = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        foreach (var variant in Enum.GetValues<MdButtonVariant>())
        {
            variants.Children.Add(new MdButton { Content = variant.ToString(), Variant = variant });
        }

        var sizes = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            VerticalAlignment = VerticalAlignment.Center
        };
        foreach (var size in Enum.GetValues<MdButtonSize>())
        {
            sizes.Children.Add(new MdButton { Content = size.ToString(), Size = size, Shape = MdButtonShape.Square });
        }

        var content = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#FFFBFE")),
            Padding = new Thickness(32),
            Child = new StackPanel
            {
                Spacing = 24,
                Children = { variants, sizes }
            }
        };

        var window = new Window { Width = 1120, Height = 320, Content = content };
        window.Show();
        try
        {
            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            MdPreviewAssets.Save(frame, "MdButtonPreview.png");
        }
        finally
        {
            window.Close();
        }
    }

    private static IDisposable Show(Control content)
    {
        var window = new Window
        {
            Width = 600,
            Height = 300,
            Content = content
        };
        window.Show();
        return new WindowScope(window);
    }

    private sealed class WindowScope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }
}
