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
    public void Gallery_Docked_Picker_Commits_Today_From_A_Real_Pointer_Click()
    {
        // Every other "click" in this repository raises Button.ClickEvent directly, which skips
        // hit testing: those tests prove a handler is wired and say nothing about whether a
        // pointer can actually reach the control. This one drives the real mouse.
        //
        // Pumping the render timer first matters. Straight after IsOpen the surface is still
        // mid-entrance - measured at Opacity 0.101 - and a click at that moment misses the
        // button and lands on the page content underneath. Settling the animation is what makes
        // this deterministic, and it also rules the animation out as the cause of the reported
        // defect: once settled the surface is fully opaque and hit testable.
        var page = new PickerGalleryPage();
        var window = new Window { Width = 1000, Height = 800, Content = page };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var picker = DockedPicker(page);
            picker.IsOpen = true;
            Dispatcher.UIThread.RunJobs();
            // The animation clock runs on wall time, so ticking frames does not fast-forward it
            // (120 ticks only got the surface to Opacity 0.364). Wait it out instead.
            var surface = InPopup<Border>(picker, "PART_Surface");
            var settleDeadline = DateTime.UtcNow.AddSeconds(5);
            while (surface.Opacity < 1 && DateTime.UtcNow < settleDeadline)
            {
                Thread.Sleep(25);
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                Dispatcher.UIThread.RunJobs();
            }

            Assert.Equal(1d, surface.Opacity);

            var today = InPopup<Button>(picker, "PART_TodayButton");
            Assert.True(today.Bounds.Width > 0 && today.Bounds.Height > 0, "Today button has no layout box");

            var centre = today.TranslatePoint(
                new Point(today.Bounds.Width / 2, today.Bounds.Height / 2), window);
            Assert.NotNull(centre);

            var underCursor = window.GetVisualsAt(centre!.Value).ToList();
            Assert.True(
                underCursor.Any(v => ReferenceEquals(v, today) || today.GetSelfAndVisualDescendants().Contains(v)),
                $"pointer cannot reach the Today button; topmost at cursor: " +
                string.Join(" / ", underCursor.Take(4).Select(v => v.GetType().Name)));

            window.MouseDown(centre.Value, MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(centre.Value, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.True(picker.SelectedDate!.Value.Date == DateTimeOffset.Now.Date,
                $"a real pointer click on Today left the field on {picker.SelectedDate:yyyy-MM-dd}; " +
                $"popup still open: {picker.IsOpen}");
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
