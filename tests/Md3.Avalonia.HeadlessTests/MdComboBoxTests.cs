using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdComboBoxTests
{
    [AvaloniaFact]
    public void Defaults_Match_Filled_Exposed_Dropdown_Field()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var comboBox = CreateComboBox();
        using var host = Show(comboBox);

        Assert.Equal(MdTextBoxVariant.Filled, comboBox.Variant);
        Assert.Equal(56, comboBox.MinHeight);
        Assert.Equal(new CornerRadius(4, 4, 0, 0), comboBox.CornerRadius);
        Assert.Equal(new Thickness(16, 0, 12, 0), comboBox.Padding);
        Assert.NotNull(comboBox.Template);
    }

    [AvaloniaFact]
    public void Selection_Uses_Native_ComboBox_State()
    {
        var comboBox = CreateComboBox();
        using var host = Show(comboBox);

        comboBox.SelectedIndex = 1;
        Assert.Equal(1, comboBox.SelectedIndex);
        Assert.Equal("Beta", comboBox.SelectedItem);
    }

    [AvaloniaFact]
    public void Outlined_Error_Uses_Error_Role_Without_Changing_Outline_Width()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var comboBox = CreateComboBox();
        comboBox.Variant = MdTextBoxVariant.Outlined;
        comboBox.IsError = true;
        comboBox.ErrorText = "Choose an option";
        using var host = Show(comboBox);

        Assert.Equal(new Thickness(1), comboBox.BorderThickness);
        var border = Assert.IsType<SolidColorBrush>(comboBox.BorderBrush);
        Assert.Equal(Color.Parse("#B3261E"), border.Color);
    }

    [AvaloniaFact]
    public void Popup_Uses_Scoped_Material_Option_Theme()
    {
        var comboBox = CreateComboBox();
        using var host = Show(comboBox);

        // Headless has no platform popup host; resolve and exercise the exact scoped theme directly.
        var itemTheme = Assert.IsType<ControlTheme>(comboBox.FindResource("MdComboBoxItemTheme"));
        var item = new ComboBoxItem { Content = "Option", Theme = itemTheme };
        using var itemHost = Show(item);

        Assert.Equal(72, item.MinHeight);
        Assert.Equal(new CornerRadius(4), item.CornerRadius);
        Assert.NotNull(item.Template);
    }

    [AvaloniaFact]
    public void Attached_Menu_Uses_Square_Top_Corners_And_Compact_Arrow()
    {
        var comboBox = CreateComboBox();
        using var host = Show(comboBox);
        Assert.True(Application.Current!.TryGetResource(
            "Md.Comp.ComboBox.Menu.Shape", ThemeVariant.Light, out var shape));
        Assert.Equal(new CornerRadius(0, 0, 4, 4), Assert.IsType<CornerRadius>(shape));

        var icon = comboBox.GetVisualDescendants().OfType<Grid>()
            .Single(control => control.Name == "PART_DropDownIcon");
        Assert.Equal(20, icon.Bounds.Width);
        Assert.Equal(20, icon.Bounds.Height);
    }

    [AvaloniaFact]
    public void Opening_Another_Material_ComboBox_Closes_Previous_Popup()
    {
        var first = CreateComboBox();
        var second = CreateComboBox();

        first.IsDropDownOpen = true;
        second.IsDropDownOpen = true;

        Assert.False(first.IsDropDownOpen);
        Assert.True(second.IsDropDownOpen);
        second.IsDropDownOpen = false;
    }

    [AvaloniaFact]
    public void Editable_Mode_Preserves_TextPresenter_Input_Path()
    {
        var comboBox = CreateComboBox();
        comboBox.IsEditable = true;
        var window = ShowWindow(comboBox);
        try
        {
            var editor = comboBox.GetVisualDescendants()
                .OfType<TextBox>()
                .Single(control => control.Name == "PART_EditableTextBox");
            editor.Focus();
            window.KeyTextInput("Custom");

            Assert.Equal("Custom", comboBox.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Material_Theme_Does_Not_Style_Native_ComboBox()
    {
        var native = new ComboBox { ItemsSource = new[] { "Native" } };
        using var host = Show(native);

        Assert.Null(native.Template);
    }

    [AvaloniaFact]
    public void Dark_Outlined_Labels_Are_Centered_On_Transparent_Outline()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
        var textBox = new MdTextBox
        {
            Width = 320,
            Label = "Text field",
            Text = "Value",
            Variant = MdTextBoxVariant.Outlined
        };
        var comboBox = CreateComboBox();
        comboBox.Label = "Combo box";
        comboBox.SelectedIndex = 0;
        comboBox.Variant = MdTextBoxVariant.Outlined;

        var row = new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal, Spacing = 24 };
        row.Children.Add(textBox);
        row.Children.Add(comboBox);
        var content = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#141218")),
            Padding = new Thickness(32),
            Child = row
        };
        var window = new Window { Width = 760, Height = 150, Content = content };
        window.Show();
        try
        {
            Assert.Equal(Colors.Transparent, Assert.IsAssignableFrom<ISolidColorBrush>(textBox.Background).Color);
            Assert.Equal(Colors.Transparent, Assert.IsAssignableFrom<ISolidColorBrush>(comboBox.Background).Color);

            var textLabel = textBox.GetVisualDescendants().OfType<ContentPresenter>()
                .Single(control => control.Name == "PART_Label");
            var comboLabel = comboBox.GetVisualDescendants().OfType<ContentPresenter>()
                .Single(control => control.Name == "PART_Label");
            Assert.Equal(16, textLabel.FontSize);
            Assert.Equal(16, comboLabel.FontSize);

            var textContainer = textBox.GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_Container");
            var comboContainer = comboBox.GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_Container");
            var textCenter = textLabel.TranslatePoint(new Point(0, textLabel.Bounds.Height / 2), textContainer)!.Value;
            var comboCenter = comboLabel.TranslatePoint(new Point(0, comboLabel.Bounds.Height / 2), comboContainer)!.Value;
            Assert.InRange(textCenter.Y, -1, 1);
            Assert.InRange(comboCenter.Y, -1, 1);

            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            var path = Path.Combine(AppContext.BaseDirectory, "MdOutlinedDarkPreview.png");
            using var stream = File.Create(path);
            frame.Save(stream, new PngBitmapEncoderOptions());
        }
        finally
        {
            window.Close();
            Application.Current.RequestedThemeVariant = ThemeVariant.Light;
        }
    }

    [AvaloniaFact]
    public void ComboBox_Matrix_Can_Render_To_Bitmap()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var matrix = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            ColumnSpacing = 24,
            RowSpacing = 24
        };

        var filled = CreateComboBox();
        filled.Label = "Framework";
        filled.PlaceholderText = "Choose one";
        filled.SupportingText = "Exposed dropdown";
        Add(matrix, filled, 0, 0);

        var outlined = CreateComboBox();
        outlined.Label = "Platform";
        outlined.Variant = MdTextBoxVariant.Outlined;
        outlined.SelectedIndex = 0;
        Add(matrix, outlined, 1, 0);

        var error = CreateComboBox();
        error.Label = "Required option";
        error.Variant = MdTextBoxVariant.Outlined;
        error.IsError = true;
        error.ErrorText = "Choose an option";
        Add(matrix, error, 0, 1);

        var disabled = CreateComboBox();
        disabled.Label = "Disabled";
        disabled.SelectedIndex = 1;
        disabled.IsEnabled = false;
        Add(matrix, disabled, 1, 1);

        var content = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#FFFBFE")),
            Padding = new Thickness(32),
            Child = matrix
        };

        var window = new Window { Width = 780, Height = 280, Content = content };
        window.Show();
        try
        {
            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            var path = Path.Combine(AppContext.BaseDirectory, "MdComboBoxPreview.png");
            using var stream = File.Create(path);
            frame.Save(stream, new PngBitmapEncoderOptions());
        }
        finally
        {
            window.Close();
        }
    }

    private static MdComboBox CreateComboBox() => new()
    {
        Width = 320,
        ItemsSource = new[] { "Alpha", "Beta", "Gamma" }
    };

    private static void Add(Grid grid, Control child, int column, int row)
    {
        Grid.SetColumn(child, column);
        Grid.SetRow(child, row);
        grid.Children.Add(child);
    }

    private static IDisposable Show(Control content) => new WindowScope(ShowWindow(content));

    private static Window ShowWindow(Control content)
    {
        var window = new Window { Width = 600, Height = 300, Content = content };
        window.Show();
        return window;
    }

    private sealed class WindowScope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }
}
