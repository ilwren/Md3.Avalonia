using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery.Pages;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdPaginationTests
{
    [AvaloniaFact]
    public void Defaults_Are_Numbered_On_The_First_Page()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var pager = new MdPagination { NumberOfPages = 10 };
        using var host = Show(pager);

        Assert.Equal(MdPaginationDisplayMode.Numbers, pager.DisplayMode);
        Assert.Contains(":numbers", pager.Classes);
        Assert.DoesNotContain(":dots", pager.Classes);
        Assert.NotNull(pager.Template);
        Assert.Contains(":first-page", pager.Classes);
        Assert.DoesNotContain(":last-page", pager.Classes);
    }

    [AvaloniaFact]
    public void DisplayMode_Switches_The_PseudoClasses()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var pager = new MdPagination { NumberOfPages = 5, DisplayMode = MdPaginationDisplayMode.Dots };
        using var host = Show(pager);

        Assert.Contains(":dots", pager.Classes);
        Assert.DoesNotContain(":numbers", pager.Classes);

        pager.DisplayMode = MdPaginationDisplayMode.Numbers;
        Assert.Contains(":numbers", pager.Classes);
        Assert.DoesNotContain(":dots", pager.Classes);
    }

    [AvaloniaFact]
    public void Selection_Updates_The_Pager_And_Raises_Change_Events()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var pager = new MdPagination { NumberOfPages = 12 };
        int last = -1;
        pager.SelectedIndexChanged += (_, e) => last = e.NewIndex;
        using var host = Show(pager);

        pager.SelectedPageIndex = 5;
        Assert.Equal(5, pager.SelectedPageIndex);
        Assert.Equal(5, last);
        Assert.DoesNotContain(":first-page", pager.Classes);
        Assert.DoesNotContain(":last-page", pager.Classes);

        pager.SelectedPageIndex = 11;
        Assert.Contains(":last-page", pager.Classes);
    }

    [AvaloniaFact]
    public void Template_Carries_The_Navigation_Buttons_And_Pip_List()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var pager = new MdPagination { NumberOfPages = 8 };
        using var host = Show(pager);
        Dispatcher.UIThread.RunJobs();

        var previous = pager.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "PART_PreviousButton");
        var next = pager.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "PART_NextButton");
        var list = pager.GetVisualDescendants().OfType<ListBox>().Single(control => control.Name == "PART_PipsPagerList");

        Assert.NotNull(previous);
        Assert.NotNull(next);
        Assert.NotNull(list);
        // The first page cannot be stepped backwards to.
        Assert.False(previous.IsEnabled);
        Assert.True(next.IsEnabled);
    }

    [AvaloniaFact]
    public void Vertical_Orientation_Is_Available_From_The_Base_Pager()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var pager = new MdPagination { NumberOfPages = 5, Orientation = global::Avalonia.Layout.Orientation.Vertical };
        using var host = Show(pager);

        Assert.Contains(":vertical", pager.Classes);
        Assert.DoesNotContain(":horizontal", pager.Classes);
    }

    [AvaloniaFact]
    public void Vertical_Dots_Next_Button_Scrolls_To_The_Next_Page()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var page = new PaginationGalleryPage();
        MdMotion.SetScheme(page, MdMotionScheme.None);
        var window = new Window { Width = 1000, Height = 900, Content = page };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            var pager = page.GetVisualDescendants().OfType<MdPagination>()
                .Single(control => control.Name == "VerticalDotsPager");
            var scroll = page.GetVisualDescendants().OfType<MdScrollViewer>()
                .Single(control => control.Name == "VerticalDotsScroll");
            var next = pager.GetVisualDescendants().OfType<Button>()
                .Single(button => button.Name == "PART_NextButton");

            // Hidden suppresses only the bar. Disabled suppresses the scroll axis itself and
            // coerces every Offset assignment back to zero.
            Assert.Equal(ScrollBarVisibility.Hidden, scroll.VerticalScrollBarVisibility);
            Assert.True(scroll.Extent.Height > scroll.Viewport.Height);
            Assert.Equal(0, pager.SelectedPageIndex);
            Assert.Equal(0, scroll.Offset.Y);

            next.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, pager.SelectedPageIndex);
            Assert.True(scroll.Offset.Y > 0,
                $"the next page was selected but the content offset stayed at {scroll.Offset.Y}");
        }
        finally { window.Close(); }
    }

    private static IDisposable Show(Control content) => new WindowScope(ShowWindow(content));

    private static Window ShowWindow(Control content)
    {
        var window = new Window { Width = 600, Height = 320, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private sealed class WindowScope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }
}
