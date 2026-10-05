using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
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
    public void Pointer_Input_Cannot_Be_Delivered_Into_A_Picker_Popup_Headless()
    {
        // Every "click" in this repository's picker tests raises Button.ClickEvent directly,
        // which skips hit testing: they prove a handler is wired and say nothing about whether a
        // pointer can reach the control. Driving the real mouse instead was supposed to close
        // that gap, and this records why it cannot be closed here.
        //
        // Translating a point inside the open popup into the window and clicking it lands on
        // whatever page content sits underneath - in this page, the AvaloniaEdit code sample -
        // because the popup content is not reachable by hit testing from the window root. So
        // pointer-level coverage of anything inside a popup is impossible in this harness, and a
        // green suite is not evidence that a popup is clickable. That has to come from a desktop
        // run. Asserting it keeps anyone (including a future me) from writing a pointer test for
        // popup content and trusting the result.
        //
        // What this run did establish, against the review-14 report: the surface animates up
        // from Opacity 0 and settles at 1, so the entrance animation is not leaving an invisible
        // and therefore unhittable surface behind.
        var page = new PickerGalleryPage();
        var window = new Window { Width = 1000, Height = 800, Content = page };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var picker = DockedPicker(page);
            picker.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            // Measured mid-transition at 0.101 and at 1 once the animation settles, so this
            // asserts the fact that matters - the surface is animating up from zero rather than
            // stuck invisible - without pinning a frame-dependent value.
            var surface = InPopup<Border>(picker, "PART_Surface");
            Assert.True(surface.Opacity > 0, "entrance animation left the surface fully transparent");

            var today = InPopup<Button>(picker, "PART_TodayButton");
            var centre = today.TranslatePoint(
                new Point(today.Bounds.Width / 2, today.Bounds.Height / 2), window);
            Assert.NotNull(centre);

            var reached = window.GetVisualsAt(centre!.Value)
                .Any(v => ReferenceEquals(v, today) || today.GetSelfAndVisualDescendants().Contains(v));
            Assert.False(reached,
                "Hit testing now reaches popup content from the window. If this ever starts " +
                "passing, replace every synthetic Button.ClickEvent in the picker tests with a " +
                "real pointer click - the synthetic ones cannot see anything that blocks input.");
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
