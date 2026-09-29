using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery.Components;
using Md3.Avalonia.Gallery.Pages;
using AvaloniaEdit;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdNewControlTests
{
    [AvaloniaFact]
    public void Code_Editor_Has_Text_And_Layout()
    {
        var page = new ProgressGalleryPage();
        var window = new Window { Width = 1100, Height = 760, Content = page };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var example = page.GetVisualDescendants().OfType<CodeExample>().Single();
            var editor = example.GetVisualDescendants().OfType<TextEditor>().First();
            Assert.NotEmpty(example.XamlCode);
            Assert.Equal(example.XamlCode, editor.Text);
            Assert.True(editor.Bounds.Height > 0);
            Assert.True(editor.TextArea.Bounds.Height > 0);
            Assert.NotEmpty(editor.GetVisualDescendants());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void New_Components_Render_In_Light_And_Dark()
    {
        var list = new MdList { Variant = MdListVariant.Segmented, SelectionMode = SelectionMode.Multiple };
        list.Items.Add(new MdListItem { Headline = "Inbox", SupportingText = "Three unread", IsSelected = true });
        list.Items.Add(new MdListItem { Headline = "Archive" });

        var content = new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 16,
            Children =
            {
                new MdDivider(),
                list,
                new MdLoadingIndicator { Size = 48 },
                new MdLinearProgressIndicator { Width = 360, Value = 60, Shape = MdProgressShape.Wavy },
                new MdCircularProgressIndicator { Value = 72 }
            }
        };
        var window = new Window { Width = 520, Height = 600, Content = content };
        window.Show();
        try
        {
            Assert.NotNull(window.CaptureRenderedFrame());
            Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(window.CaptureRenderedFrame());
        }
        finally
        {
            Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Dialog_Host_Supports_Awaitable_Direct_API()
    {
        var host = new MdDialogHost { Content = new TextBlock { Text = "Page" } };
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            var dialog = new MdDialog { Variant = MdDialogVariant.FullScreen, Headline = "Confirm", Content = "Proceed?" };
            var resultTask = host.ShowAsync(dialog);
            Dispatcher.UIThread.RunJobs();
            Assert.True(host.IsOpen);
            Assert.Same(dialog, host.Dialog);
            Assert.True(dialog.Bounds.Width >= 600);
            Assert.True(dialog.Bounds.Height >= 440);
            Assert.NotNull(window.CaptureRenderedFrame());

            host.Close("accepted");
            Assert.Equal("accepted", await resultTask);
            Assert.False(host.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Popup_Owners_Are_Mutually_Exclusive_Across_Control_Types()
    {
        // Keep the owners detached: Avalonia Headless intentionally has no native IPopupImpl.
        var combo = new MdComboBox();
        var autocomplete = new MdAutoCompleteBox
        {
            ItemsSource = new[] { "Avalonia", "Android" },
            Text = "A",
            MinimumPrefixLength = 1
        };
        var date = new MdDatePicker();
        var time = new MdTimePicker();

        combo.IsDropDownOpen = true;
        autocomplete.IsDropDownOpen = true;
        Assert.False(combo.IsDropDownOpen);
        Assert.True(autocomplete.IsDropDownOpen);

        date.IsOpen = true;
        Assert.False(autocomplete.IsDropDownOpen);
        Assert.False(combo.IsDropDownOpen);
        Assert.True(date.IsOpen);

        time.IsOpen = true;
        Assert.False(date.IsOpen);
        Assert.True(time.IsOpen);
    }

    [AvaloniaFact]
    public void Date_Picker_Uses_Current_Culture_Weekday_Order_And_Full_Grid()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("zh-CN");
            var picker = new MdDatePicker { DisplayDate = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero) };
            Assert.Equal(7, picker.WeekdayLabels.Count);
            Assert.Equal("一", picker.WeekdayLabels[0]);
            Assert.Equal(42, picker.CalendarDays.Count);
            Assert.Contains(picker.CalendarDays, day => day.Date.Day == 22 && day.IsCurrentMonth);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [AvaloniaFact]
    public void Time_Picker_Defaults_To_Any_Minute_Precision()
    {
        var picker = new MdTimePicker { SelectedTime = new TimeSpan(10, 23, 0) };
        Assert.Equal(1, picker.MinuteStep);
        Assert.Equal(new TimeSpan(10, 23, 0), picker.SelectedTime);
    }

    [AvaloniaFact]
    public void Full_Surface_State_Layers_Match_Their_Containers()
    {
        Control[] controls =
        {
            new MdCard { Width = 260, Height = 112, Content = "Card" },
            new MdChip { Content = "Chip" },
            new MdToggleButton { Content = "Connected" }
        };
        var host = new StackPanel { Spacing = 12 };
        foreach (var control in controls) host.Children.Add(control);
        var window = new Window { Width = 500, Height = 400, Content = host };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            foreach (var control in controls)
            {
                var stateLayer = control.GetVisualDescendants().OfType<Border>()
                    .First(border => border.Name == "PART_StateLayer");
                var surfaceContent = Assert.IsAssignableFrom<Control>(stateLayer.GetVisualParent());
                Assert.Equal(surfaceContent.Bounds.Width, stateLayer.Bounds.Width, 1);
                Assert.Equal(surfaceContent.Bounds.Height, stateLayer.Bounds.Height, 1);
            }
        }
        finally
        {
            window.Close();
        }
    }
}
