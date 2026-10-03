using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdTextBoxTests
{
    [Fact]
    public void Core_Control_Assembly_Has_No_Desktop_Platform_Dependency()
    {
        var references = typeof(MdTextBox).Assembly
            .GetReferencedAssemblies()
            .Select(name => name.Name)
            .Where(name => name is not null)
            .ToArray();

        Assert.DoesNotContain("Avalonia.Desktop", references);
        Assert.DoesNotContain("Avalonia.Win32", references);
        Assert.DoesNotContain("Avalonia.X11", references);
        Assert.DoesNotContain("Avalonia.Native", references);
    }

    [AvaloniaFact]
    public void Material_Theme_Does_Not_Style_Native_TextBox()
    {
        var native = new TextBox { Text = "Native" };
        using var host = Show(native);

        Assert.Null(native.Template);
        Assert.Null(native.Background);
    }

    [AvaloniaFact]
    public void Defaults_Match_Filled_Text_Field_Tokens()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var textBox = new MdTextBox { Label = "Name" };
        using var host = Show(textBox);

        Assert.Equal(MdTextBoxVariant.Filled, textBox.Variant);
        Assert.Equal(56, textBox.MinHeight);
        Assert.Equal(new CornerRadius(4, 4, 0, 0), textBox.CornerRadius);
        Assert.Equal(new Thickness(16, 0), textBox.Padding);
        Assert.Equal(16, textBox.FontSize);

        var background = Assert.IsType<SolidColorBrush>(textBox.Background);
        Assert.Equal(Color.Parse("#E6E0E9"), background.Color);
        Assert.NotNull(textBox.Template);
    }

    [AvaloniaFact]
    public void Outlined_Variant_Uses_Outline_And_Transparent_Container()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var textBox = new MdTextBox
        {
            Label = "Email",
            Variant = MdTextBoxVariant.Outlined
        };
        using var host = Show(textBox);

        Assert.Equal(new CornerRadius(4), textBox.CornerRadius);
        Assert.Equal(new Thickness(1), textBox.BorderThickness);
        var border = Assert.IsType<SolidColorBrush>(textBox.BorderBrush);
        Assert.Equal(Color.Parse("#79747E"), border.Color);
    }

    [AvaloniaFact]
    public void Character_Counter_Tracks_Text_And_MaxLength()
    {
        var textBox = new MdTextBox
        {
            MaxLength = 20,
            ShowCharacterCounter = true,
            Text = "Avalonia"
        };
        using var host = Show(textBox);

        Assert.Equal(8, textBox.CharacterCount);
        Assert.Equal("8/20", textBox.CharacterCounterText);

        textBox.Text = "Material";
        Assert.Equal(8, textBox.CharacterCount);
        Assert.Equal("8/20", textBox.CharacterCounterText);
    }

    [AvaloniaFact]
    public void Manual_Error_Uses_Error_Color_Without_Losing_TextBox_Behavior()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var textBox = new MdTextBox
        {
            Label = "Email",
            Text = "invalid",
            ErrorText = "Enter a valid email address",
            IsError = true,
            Variant = MdTextBoxVariant.Outlined
        };
        using var host = Show(textBox);

        var label = Assert.IsType<SolidColorBrush>(textBox.LabelBrush);
        var border = Assert.IsType<SolidColorBrush>(textBox.BorderBrush);
        Assert.Equal(Color.Parse("#B3261E"), label.Color);
        Assert.Equal(Color.Parse("#B3261E"), border.Color);
        Assert.Equal("invalid", textBox.Text);
    }

    [AvaloniaFact]
    public void Native_Text_Input_Path_Remains_Functional()
    {
        var textBox = new MdTextBox
        {
            Label = "Message",
            Variant = MdTextBoxVariant.Outlined
        };
        var window = ShowWindow(textBox);
        try
        {
            textBox.Focus();
            window.KeyTextInput("Hello");
            Assert.Equal("Hello", textBox.Text);
            // Focus uses a non-measuring overlay, so the base outline remains one DIP.
            Assert.Equal(new Thickness(1), textBox.BorderThickness);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Floating_Label_Animates_Without_Changing_Control_Height()
    {
        var textBox = new MdTextBox
        {
            Width = 320,
            Label = "Stable label",
            SupportingText = "Supporting text",
            Variant = MdTextBoxVariant.Outlined
        };
        var window = ShowWindow(textBox);
        try
        {
            var label = textBox.GetVisualDescendants()
                .OfType<ContentPresenter>()
                .Single(control => control.Name == "PART_Label");
            Assert.Contains(label.Transitions!, transition => transition is TransformOperationsTransition);

            var heightBeforeFocus = textBox.Bounds.Height;
            textBox.Focus();
            Dispatcher.UIThread.RunJobs();
            var heightAfterFocus = textBox.Bounds.Height;

            Assert.Equal(heightBeforeFocus, heightAfterFocus);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Outlined_Label_Uses_A_Transparent_Stroke_Notch_On_Arbitrary_Host_Color()
    {
        var textBox = new MdTextBox
        {
            Width = 320,
            Label = "HEX value",
            Text = "#8BC34A",
            Variant = MdTextBoxVariant.Outlined
        };
        var hostSurface = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#615D67")),
            Padding = new Thickness(24),
            Child = textBox
        };
        using var host = Show(hostSurface);

        var label = textBox.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(control => control.Name == "PART_Label");
        var outline = textBox.GetVisualDescendants().OfType<MdOutlinedFieldBorder>()
            .Single(control => control.Name == "PART_Outline");

        Assert.True(outline.IsVisible);
        Assert.True(outline.IsNotched);
        Assert.Same(label, outline.NotchTarget);
        Assert.Equal(Colors.Transparent, Assert.IsAssignableFrom<ISolidColorBrush>(label.Background).Color);
        Assert.NotEqual(
            Assert.IsType<SolidColorBrush>(hostSurface.Background).Color,
            Assert.IsAssignableFrom<ISolidColorBrush>(label.Background).Color);
    }

    [AvaloniaFact]
    public void TextBox_Matrix_Can_Render_To_Bitmap()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;

        var fields = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            ColumnSpacing = 24,
            RowSpacing = 24
        };

        fields.Children.Add(new MdTextBox
        {
            Label = "Filled label",
            PlaceholderText = "Type a value",
            SupportingText = "Supporting text"
        });

        var outlined = new MdTextBox
        {
            Label = "Outlined label",
            Variant = MdTextBoxVariant.Outlined,
            Text = "Populated value"
        };
        Grid.SetColumn(outlined, 1);
        fields.Children.Add(outlined);

        var counter = new MdTextBox
        {
            Label = "Amount",
            PrefixText = "$",
            SuffixText = "USD",
            Text = "120",
            MaxLength = 10,
            ShowCharacterCounter = true,
            LeadingIcon = new TextBlock { Text = "#" }
        };
        Grid.SetRow(counter, 1);
        fields.Children.Add(counter);

        var error = new MdTextBox
        {
            Label = "Email",
            Variant = MdTextBoxVariant.Outlined,
            Text = "name@",
            ErrorText = "Enter a valid email address",
            IsError = true,
            TrailingIcon = new TextBlock { Text = "!" }
        };
        Grid.SetRow(error, 1);
        Grid.SetColumn(error, 1);
        fields.Children.Add(error);

        var content = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#FFFBFE")),
            Padding = new Thickness(32),
            Child = fields
        };

        var window = new Window { Width = 760, Height = 260, Content = content };
        window.Show();
        try
        {
            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            var path = Path.Combine(AppContext.BaseDirectory, "MdTextBoxPreview.png");
            using var stream = File.Create(path);
            frame.Save(stream, new PngBitmapEncoderOptions());
        }
        finally
        {
            window.Close();
        }
    }

    private static IDisposable Show(Control content) => new WindowScope(ShowWindow(content));

    private static Window ShowWindow(Control content)
    {
        var window = new Window
        {
            Width = 600,
            Height = 260,
            Content = content
        };
        window.Show();
        return window;
    }

    private sealed class WindowScope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }
}
