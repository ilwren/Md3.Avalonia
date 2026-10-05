using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery.Pages;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Review 14 reports that the gallery's docked date picker never leaves its seeded 2026-09-22:
/// not when a day is picked, not when Today is pressed, not after a light dismiss. Every earlier
/// test built a picker by hand, so none of them exercised the page the report is actually about.
/// This drives the real gallery page so no assumption about how it differs is left standing.
/// </summary>
public sealed class MdGalleryPickerPageTests
{
    private static MdDatePicker DockedPicker(Visual page) =>
        page.GetVisualDescendants().OfType<MdDatePicker>()
            .First(p => p.Mode == MdDatePickerMode.Docked);

    private static T InPopup<T>(MdDatePicker picker, string name) where T : Control =>
        picker.GetVisualDescendants().OfType<Popup>().First(p => p.Name == "PART_Popup")
            .Child!.GetSelfAndVisualDescendants().OfType<T>().First(c => c.Name == name);

    [AvaloniaFact]
    public void Gallery_Docked_Picker_Commits_Today_Into_Its_Own_Field()
    {
        var page = new PickerGalleryPage();
        var window = new Window { Width = 1000, Height = 800, Content = page };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var picker = DockedPicker(page);
            var seededDate = picker.SelectedDate;
            var seededText = picker.DisplayText;
            Assert.Equal(new DateTime(2026, 9, 22), seededDate!.Value.Date);

            picker.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var today = InPopup<Button>(picker, "PART_TodayButton");
            today.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(DateTimeOffset.Now.Date, picker.SelectedDate!.Value.Date);
            Assert.NotEqual(seededText, picker.DisplayText);

            // The field, not just the property behind it: the report is about what is on screen.
            var shown = picker.GetVisualDescendants().OfType<TextBlock>()
                .Select(t => t.Text).FirstOrDefault(t => t == picker.DisplayText);
            Assert.True(shown is not null,
                $"the anchor field still reads '{seededText}' while DisplayText is '{picker.DisplayText}'");
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Gallery_Docked_Picker_Keeps_Its_Value_When_The_Popup_Is_Dismissed_Unused()
    {
        // Opening and clicking away must not disturb a value that is already there.
        var page = new PickerGalleryPage();
        var window = new Window { Width = 1000, Height = 800, Content = page };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var picker = DockedPicker(page);

            picker.IsOpen = true;
            Dispatcher.UIThread.RunJobs();
            picker.IsOpen = false;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(new DateTime(2026, 9, 22), picker.SelectedDate!.Value.Date);
        }
        finally { window.Close(); }
    }
}
