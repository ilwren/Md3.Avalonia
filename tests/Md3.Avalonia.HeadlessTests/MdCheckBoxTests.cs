using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using AvaloniaPath = Avalonia.Controls.Shapes.Path;
using Shape = Avalonia.Controls.Shapes.Shape;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdCheckBoxTests
{
    [AvaloniaFact]
    public void Defaults_Match_Material_Checkbox_Measurements()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var checkBox = new MdCheckBox { Content = "Option" };
        using var host = Show(checkBox);

        Assert.Equal(48, checkBox.MinHeight);
        Assert.Equal(new Thickness(2), checkBox.BorderThickness);
        Assert.Equal(new CornerRadius(2), checkBox.CornerRadius);
        Assert.False(checkBox.IsChecked);
        Assert.NotNull(checkBox.Template);
    }

    [AvaloniaFact]
    public void Space_Key_Uses_Native_CheckBox_Toggle_Behavior()
    {
        var checkBox = new MdCheckBox { Content = "Option" };
        var window = ShowWindow(checkBox);
        try
        {
            checkBox.Focus();
            window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
            window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
            Assert.True(checkBox.IsChecked);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Box_Hit_Target_Toggles_And_Receives_Pointer_State()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var checkBox = new MdCheckBox { Content = "Option" };
        var window = ShowWindow(checkBox);
        try
        {
            var interactiveArea = checkBox.GetVisualDescendants()
                .OfType<Grid>()
                .Single(control => control.Name == "PART_InteractiveArea");
            var point = interactiveArea.TranslatePoint(
                new Point(interactiveArea.Bounds.Width / 2, interactiveArea.Bounds.Height / 2),
                window)!.Value;
            window.MouseMove(point, RawInputModifiers.None);
            var hoverBorder = Assert.IsType<SolidColorBrush>(checkBox.BorderBrush);
            Assert.Equal(Color.Parse("#1D1B20"), hoverBorder.Color);

            window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            Assert.True(checkBox.IsChecked);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Check_Glyph_Uses_Path_Draw_Transition()
    {
        var checkBox = new MdCheckBox { IsChecked = true };
        using var host = Show(checkBox);
        var glyph = checkBox.GetVisualDescendants().OfType<AvaloniaPath>()
            .Single(path => path.Name == "PART_CheckGlyph");

        Assert.Equal(0, glyph.StrokeDashOffset);
        Assert.Contains(glyph.Transitions!, transition =>
            transition is DoubleTransition doubleTransition &&
            doubleTransition.Property == Shape.StrokeDashOffsetProperty);

        checkBox.IsChecked = false;
        Assert.Equal(18, glyph.GetBaseValue(Shape.StrokeDashOffsetProperty));
    }

    [AvaloniaFact]
    public void Selected_Indeterminate_And_Error_States_Resolve_Semantic_Colors()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var selected = new MdCheckBox { IsChecked = true };
        using var selectedHost = Show(selected);
        var selectedBackground = Assert.IsType<SolidColorBrush>(selected.Background);
        Assert.Equal(Color.Parse("#6750A4"), selectedBackground.Color);

        var indeterminate = new MdCheckBox { IsThreeState = true, IsChecked = null };
        using var indeterminateHost = Show(indeterminate);
        var indeterminateBackground = Assert.IsType<SolidColorBrush>(indeterminate.Background);
        Assert.Equal(Color.Parse("#6750A4"), indeterminateBackground.Color);

        var error = new MdCheckBox { IsError = true };
        using var errorHost = Show(error);
        var errorBorder = Assert.IsType<SolidColorBrush>(error.BorderBrush);
        Assert.Equal(Color.Parse("#B3261E"), errorBorder.Color);
    }

    [AvaloniaFact]
    public void Material_Theme_Does_Not_Style_Native_CheckBox()
    {
        var native = new CheckBox { Content = "Native" };
        using var host = Show(native);

        Assert.Null(native.Theme);
        Assert.Null(native.Background);
        Assert.Equal(0, native.MinHeight);
    }

    [AvaloniaFact]
    public void CheckBox_Matrix_Can_Render_To_Bitmap()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var matrix = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*"),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto"),
            ColumnSpacing = 28,
            RowSpacing = 8
        };

        Add(matrix, new MdCheckBox { Content = "Unselected" }, 0, 0);
        Add(matrix, new MdCheckBox { Content = "Selected", IsChecked = true }, 1, 0);
        Add(matrix, new MdCheckBox { Content = "Indeterminate", IsThreeState = true, IsChecked = null }, 2, 0);
        Add(matrix, new MdCheckBox { Content = "Error", IsError = true }, 0, 1);
        Add(matrix, new MdCheckBox { Content = "Selected error", IsError = true, IsChecked = true }, 1, 1);
        Add(matrix, new MdCheckBox { Content = "Disabled", IsEnabled = false, IsChecked = true }, 2, 1);

        var content = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#FFFBFE")),
            Padding = new Thickness(32),
            Child = matrix
        };

        var window = new Window { Width = 820, Height = 220, Content = content };
        window.Show();
        try
        {
            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            var path = Path.Combine(AppContext.BaseDirectory, "MdCheckBoxPreview.png");
            using var stream = File.Create(path);
            frame.Save(stream, new PngBitmapEncoderOptions());
        }
        finally
        {
            window.Close();
        }
    }

    private static void Add(Grid grid, Control child, int column, int row)
    {
        Grid.SetColumn(child, column);
        Grid.SetRow(child, row);
        grid.Children.Add(child);
    }

    private static IDisposable Show(Control content) => new WindowScope(ShowWindow(content));

    private static Window ShowWindow(Control content)
    {
        var window = new Window { Width = 600, Height = 240, Content = content };
        window.Show();
        return window;
    }

    private sealed class WindowScope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }
}
