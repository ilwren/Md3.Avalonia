using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// The picker and carousel paths that no existing test reached. Every earlier picker test only
/// toggled <c>IsOpen</c>, so the popup content was never realized and the select/confirm route
/// through the real calendar buttons was never executed. The carousel tests only measured one
/// layout pass, so a stale arrangement surviving a resize would not have shown up.
/// </summary>
public sealed class MdDesktopPickerAndCarouselTests
{
    private static Popup PopupOf(Control control) =>
        control.GetVisualDescendants().OfType<Popup>().First(p => p.Name == "PART_Popup");

    private static IEnumerable<Control> PopupContent(Control control)
    {
        var child = PopupOf(control).Child;
        return child is null ? [] : child.GetSelfAndVisualDescendants().OfType<Control>();
    }

    // ---------------------------------------------------------------- date picker

    [AvaloniaFact]
    public void Date_Picker_Commits_A_Day_Picked_From_The_Real_Popup_Content()
    {
        var picker = new MdDatePicker
        {
            DisplayDate = new DateTime(2026, 10, 1),
            Mode = MdDatePickerMode.Modal,
        };
        var window = new Window { Width = 900, Height = 700, Content = picker };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            picker.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            // The popup content has to actually exist before anything inside it can be clicked.
            var buttons = PopupContent(picker).OfType<Button>().ToArray();
            Assert.True(buttons.Length > 0, "the popup realized no content at all");

            var day = buttons.FirstOrDefault(b => b.DataContext?.GetType().Name.Contains("Day", StringComparison.Ordinal) == true)
                      ?? buttons.FirstOrDefault(b => b.Content is string s && s == "15");
            Assert.True(day is not null, "no calendar day button in the realized popup content");

            // Modal keeps the pick provisional until Confirm, so the day click alone must not commit.
            day!.Command?.Execute(day.CommandParameter);
            RaiseClick(day);
            Dispatcher.UIThread.RunJobs();

            var confirm = PopupContent(picker).OfType<Button>()
                .FirstOrDefault(b => b.Name == "PART_ConfirmButton");
            Assert.True(confirm is not null, "no confirm button in the realized popup content");
            RaiseClick(confirm!);
            Dispatcher.UIThread.RunJobs();

            Assert.False(picker.IsOpen);
            Assert.NotNull(picker.SelectedDate);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Date_Picker_Action_Labels_Come_From_The_Control_Not_The_Page()
    {
        // These used {Binding}, which resolves against whatever DataContext the page happens to
        // have, while the time picker next to it used {TemplateBinding}.
        // ConfirmText/CancelText are read-only and come from localization, so the assertion is
        // that the buttons show the control's values and not the page's DataContext.
        var picker = new MdDatePicker();
        var window = new Window { Width = 900, Height = 700, Content = picker, DataContext = new UnrelatedPageModel() };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            picker.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var labels = PopupContent(picker).OfType<Button>()
                .Where(b => b.Name is "PART_ConfirmButton" or "PART_CancelButton")
                .Select(b => b.Content as string)
                .ToArray();
            Assert.Contains(picker.ConfirmText, labels);
            Assert.Contains(picker.CancelText, labels);
            Assert.All(labels, label => Assert.False(string.IsNullOrEmpty(label)));
        }
        finally { window.Close(); }
    }

    private sealed class UnrelatedPageModel
    {
        // Deliberately carries neither ConfirmText nor CancelText.
        public int Unrelated => 1;
    }

    private static void RaiseClick(Button button) =>
        button.RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));

    // ---------------------------------------------------------------- carousel

    [AvaloniaFact]
    public void Carousel_Rearranges_Its_Keylines_When_The_Window_Is_Resized()
    {
        // Phones do not resize; desktop windows do. The arrangement is fitted to the viewport, so
        // if a resize does not re-prepare the realized containers they keep the extents of the
        // old width - items half off the edge, labels cut mid-word.
        var carousel = new MdCarousel
        {
            Variant = MdCarouselVariant.MultiBrowse,
            ItemWidth = 280,
            ItemHeight = 160,
            ItemsSource = Enumerable.Range(0, 8).Select(i => $"Item {i}").ToArray(),
        };
        var window = new Window { Width = 1200, Height = 400, Content = carousel };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var wide = RealizedWidths(carousel);
            Assert.True(wide.Length > 0, "the carousel realized no containers");

            window.Width = 520;
            Dispatcher.UIThread.RunJobs();
            carousel.Measure(new Size(520, 400));
            carousel.Arrange(new Rect(0, 0, 520, 400));
            Dispatcher.UIThread.RunJobs();

            var narrow = RealizedWidths(carousel);
            Assert.True(narrow.Length > 0);

            // The large keyline has to fit the new viewport, not the old one.
            var widest = narrow.Max();
            Assert.True(widest <= 520 + .5,
                $"after narrowing to 520 the widest item is still {widest}; the arrangement did not follow the resize.");
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Carousel_Items_Never_Lay_Content_Out_Narrower_Than_Their_Mask()
    {
        // Material masks a shrinking carousel item, it does not reflow its content. If the
        // content host were ever narrower than the container the label would rewrap instead of
        // being clipped, which is what produced one-character-per-line side items.
        var carousel = new MdCarousel
        {
            Variant = MdCarouselVariant.MultiBrowse,
            ItemWidth = 280,
            ItemHeight = 160,
            ItemsSource = Enumerable.Range(0, 8).Select(i => $"Item {i}").ToArray(),
        };
        var window = new Window { Width = 1200, Height = 400, Content = carousel };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            foreach (var container in carousel.GetRealizedContainers().OfType<ListBoxItem>())
            {
                var host = container.GetVisualDescendants().OfType<ContentPresenter>()
                    .FirstOrDefault(p => p.Name == "PART_ContentHost");
                if (host is null) continue;
                Assert.True(host.Bounds.Width + .5 >= container.Bounds.Width,
                    $"content host is {host.Bounds.Width} inside a {container.Bounds.Width} container; " +
                    "the content would rewrap to the masked width instead of being clipped.");
            }
        }
        finally { window.Close(); }
    }

    private static double[] RealizedWidths(MdCarousel carousel) =>
        carousel.GetRealizedContainers().OfType<Control>()
            .Select(c => double.IsNaN(c.Width) ? c.Bounds.Width : c.Width)
            .Where(w => w > 0)
            .ToArray();
}
