using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Regression guards for the two reported picker defects: the anchor field must always paint the
/// committed value (desktop showed a stale string while the bound readout below was correct), and
/// a picker popup must not stay open while the surrounding page scrolls (drifted popups make every
/// dial/calendar drag land offset).
/// </summary>
public sealed class MdPickerFieldRefreshTests
{
    [AvaloniaFact]
    public void Date_Picker_Field_Paints_The_Committed_Selection()
    {
        var picker = new MdDatePicker();
        var window = new Window { Width = 520, Height = 520, Content = picker };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        picker.SelectedDate = new DateTimeOffset(2026, 9, 22, 0, 0, 0, TimeSpan.Zero);
        Dispatcher.UIThread.RunJobs();

        var field = picker.GetVisualDescendants().OfType<TextBlock>()
            .Single(text => text.Name == "PART_DisplayText");
        Assert.Equal(picker.DisplayText, field.Text);
        Assert.Contains("2026", field.Text);

        // A second commit must also land on the field, not just the first one.
        picker.SelectedDate = new DateTimeOffset(2026, 9, 5, 0, 0, 0, TimeSpan.Zero);
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(picker.DisplayText, field.Text);
        Assert.Contains("09/05", field.Text);
    }

    [AvaloniaFact]
    public void Time_Picker_Field_Paints_The_Committed_Selection()
    {
        var picker = new MdTimePicker();
        var window = new Window { Width = 520, Height = 520, Content = picker };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        picker.SelectedTime = new TimeSpan(14, 30, 0);
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        var field = picker.GetVisualDescendants().OfType<TextBlock>()
            .Single(text => text.Name == "PART_DisplayText");
        Assert.Equal(picker.DisplayText, field.Text);
        Assert.Contains("2:30", field.Text);

        picker.SelectedTime = new TimeSpan(17, 30, 0);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(picker.DisplayText, field.Text);
        Assert.Contains("5:30", field.Text);
    }

    [AvaloniaFact]
    public void Date_Picker_Popup_Closes_When_The_Ancestor_Page_Scrolls()
    {
        var picker = new MdDatePicker();
        var scroller = new MdScrollViewer
        {
            Height = 400,
            Content = new StackPanel
            {
                Children = { picker, new Border { Height = 1200 } },
            },
        };
        var window = new Window { Width = 520, Height = 440, Content = scroller };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        picker.IsOpen = true;
        Dispatcher.UIThread.RunJobs();
        Assert.True(picker.IsOpen, "the popup must open");

        scroller.Offset = new Vector(0, 240);
        Dispatcher.UIThread.RunJobs();
        Assert.False(picker.IsOpen, "background scroll must dismiss the popup before drags land offset");
    }

    [AvaloniaFact]
    public void Time_Picker_Popup_Closes_When_The_Ancestor_Page_Scrolls()
    {
        var picker = new MdTimePicker();
        var scroller = new MdScrollViewer
        {
            Height = 400,
            Content = new StackPanel
            {
                Children = { picker, new Border { Height = 1200 } },
            },
        };
        var window = new Window { Width = 520, Height = 440, Content = scroller };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        picker.IsOpen = true;
        Dispatcher.UIThread.RunJobs();
        Assert.True(picker.IsOpen, "the popup must open");

        scroller.Offset = new Vector(0, 240);
        Dispatcher.UIThread.RunJobs();
        Assert.False(picker.IsOpen, "background scroll must dismiss the popup before drags land offset");
    }
}
