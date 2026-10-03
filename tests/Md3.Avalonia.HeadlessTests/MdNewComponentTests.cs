using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdNewComponentTests
{
    [AvaloniaFact]
    public void Text_Field_Clear_And_Password_Actions_Are_Functional()
    {
        var clearField = new MdTextBox { Text = "query", ShowClearButton = true };
        var passwordField = new MdTextBox { Text = "secret", IsPassword = true };
        var row = new StackPanel { Children = { clearField, passwordField } };
        using var host = Show(row);

        var clear = clearField.GetVisualDescendants().OfType<MdIconButton>()
            .Single(control => control.Name == "PART_ClearButton");
        Assert.True(clear.IsVisible);
        clear.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(string.Empty, clearField.Text);

        Assert.Equal('●', passwordField.PasswordChar);
        var reveal = passwordField.GetVisualDescendants().OfType<MdToggleIconButton>()
            .Single(control => control.Name == "PART_PasswordButton");
        Assert.True(reveal.IsVisible);
    }

    [AvaloniaFact]
    public void Card_And_Chip_Use_Independent_Scoped_Themes()
    {
        var card = new MdCard { Content = "Outlined", Variant = MdCardVariant.Outlined };
        var chip = new MdChip { Content = "Nearby", Variant = MdChipVariant.Filter, IsChecked = true };
        var row = new StackPanel { Children = { card, chip } };
        using var host = Show(row);

        Assert.Equal(new CornerRadius(12), card.CornerRadius);
        Assert.Equal(new Thickness(1), card.BorderThickness);
        Assert.NotNull(card.Template);
        Assert.NotNull(chip.Template);
        Assert.Equal(Color.Parse("#E8DEF8"), Assert.IsAssignableFrom<ISolidColorBrush>(chip.Background).Color);
        Assert.Contains(chip.GetVisualDescendants().OfType<MdIcon>(), icon => icon.Name == "PART_SelectedIcon" && icon.IsVisible);
    }

    [AvaloniaFact]
    public void Carousel_Uses_Horizontal_Native_Item_Containers()
    {
        var carousel = new MdCarousel { ItemWidth = 180, ItemHeight = 120 };
        carousel.Items.Add("One");
        carousel.Items.Add("Two");
        using var host = Show(carousel);

        var containers = carousel.GetVisualDescendants().OfType<ListBoxItem>().ToArray();
        Assert.Equal(2, containers.Length);
        Assert.Equal(180, containers[0].Width);
        Assert.Equal(118, containers[1].Width);
        Assert.All(containers, item => Assert.Equal(120, item.Height));
    }

    [AvaloniaFact]
    public void Date_Picker_Builds_Six_Week_Calendar_And_Tracks_Selected_Date()
    {
        var selected = new DateTimeOffset(2026, 9, 22, 0, 0, 0, TimeSpan.Zero);
        var picker = new MdDatePicker { SelectedDate = selected };
        using var host = Show(picker);

        Assert.Equal(42, picker.CalendarDays.Count);
        Assert.Single(picker.CalendarDays, day => day.IsSelected);
        Assert.Contains("2026", picker.MonthText);
        Assert.NotEqual("Choose date", picker.DisplayText);
        Assert.NotNull(picker.Template);
    }

    [AvaloniaFact]
    public void Time_Picker_Normalizes_Parts_And_Formats_24_Hour_Value()
    {
        var picker = new MdTimePicker { Is24Hour = true, Mode = MdTimePickerMode.Input };
        picker.Hour = 25;
        picker.Minute = 75;
        using var host = Show(picker);

        Assert.Equal(2, picker.Hour);
        Assert.Equal(15, picker.Minute);
        Assert.Equal(TimeSpan.FromHours(2) + TimeSpan.FromMinutes(15), picker.SelectedTime);
        Assert.Equal("02:15", picker.DisplayText);
        Assert.NotNull(picker.Template);
    }

    [AvaloniaFact]
    public void Flexible_App_Bar_Expands_For_Subtitle_And_Remains_An_Edge_To_Edge_Surface()
    {
        var bar = new MdTopAppBar
        {
            Title = "Library",
            Subtitle = "Updated moments ago",
            Variant = MdTopAppBarVariant.MediumFlexible
        };
        using var host = Show(bar);

        Assert.Equal(136, bar.Height);
        Assert.Equal(new CornerRadius(0), bar.CornerRadius);
        Assert.Equal(new Thickness(0), bar.BorderThickness);
        var subtitle = bar.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(control => control.Name == "PART_Subtitle");
        Assert.True(subtitle.Bounds.Height > 0);
    }

    [AvaloniaFact]
    public void New_Component_Matrix_Renders_In_Light_Theme()
    {
        var carousel = new MdCarousel { Height = 150, ItemHeight = 130, ItemWidth = 180 };
        carousel.Items.Add(new Border { Background = new SolidColorBrush(Color.Parse("#8C6FA9")), Child = new TextBlock { Text = "Carousel", Margin = new Thickness(16), Foreground = Brushes.White } });
        carousel.Items.Add(new Border { Background = new SolidColorBrush(Color.Parse("#527A70")), Child = new TextBlock { Text = "ItemsSource", Margin = new Thickness(16), Foreground = Brushes.White } });
        var content = new StackPanel
        {
            Margin = new Thickness(28),
            Spacing = 18,
            Children =
            {
                new StackPanel
                {
                    Orientation = global::Avalonia.Layout.Orientation.Horizontal,
                    Spacing = 12,
                    Children =
                    {
                        new MdCard { Width = 190, Height = 100, Content = "Elevated card" },
                        new MdCard { Width = 190, Height = 100, Content = "Filled card", Variant = MdCardVariant.Filled },
                        new MdCard { Width = 190, Height = 100, Content = "Outlined card", Variant = MdCardVariant.Outlined }
                    }
                },
                new StackPanel
                {
                    Orientation = global::Avalonia.Layout.Orientation.Horizontal,
                    Spacing = 10,
                    Children =
                    {
                        new MdChip { Content = "Assist" },
                        new MdChip { Content = "Filter", Variant = MdChipVariant.Filter, IsChecked = true },
                        new MdChip { Content = "Input", Variant = MdChipVariant.Input },
                        new MdChip { Content = "Suggestion", Variant = MdChipVariant.Suggestion }
                    }
                },
                carousel,
                new StackPanel
                {
                    Orientation = global::Avalonia.Layout.Orientation.Horizontal,
                    Spacing = 16,
                    Children =
                    {
                        new MdDatePicker { SelectedDate = new DateTimeOffset(2026, 9, 22, 0, 0, 0, TimeSpan.Zero) },
                        new MdTimePicker { SelectedTime = new TimeSpan(14, 30, 0) }
                    }
                }
            }
        };
        var window = new Window { Width = 820, Height = 470, Content = content };
        window.Show();
        try
        {
            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            var path = Path.Combine(AppContext.BaseDirectory, "MdNewComponentsPreview.png");
            using var stream = File.Create(path);
            frame.Save(stream, new PngBitmapEncoderOptions());
        }
        finally
        {
            window.Close();
        }
    }

    private static IDisposable Show(Control control)
    {
        var window = new Window { Width = 900, Height = 480, Content = control };
        window.Show();
        return new Scope(window);
    }

    private sealed class Scope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }
}
