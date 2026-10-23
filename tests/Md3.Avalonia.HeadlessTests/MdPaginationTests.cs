using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
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
        Assert.True(pager.PseudoClasses.Contains(":numbers"));
        Assert.False(pager.PseudoClasses.Contains(":dots"));
        Assert.NotNull(pager.Template);
        Assert.True(pager.PseudoClasses.Contains(":first-page"));
        Assert.False(pager.PseudoClasses.Contains(":last-page"));
    }

    [AvaloniaFact]
    public void DisplayMode_Switches_The_PseudoClasses()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var pager = new MdPagination { NumberOfPages = 5, DisplayMode = MdPaginationDisplayMode.Dots };
        using var host = Show(pager);

        Assert.True(pager.PseudoClasses.Contains(":dots"));
        Assert.False(pager.PseudoClasses.Contains(":numbers"));

        pager.DisplayMode = MdPaginationDisplayMode.Numbers;
        Assert.True(pager.PseudoClasses.Contains(":numbers"));
        Assert.False(pager.PseudoClasses.Contains(":dots"));
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
        Assert.False(pager.PseudoClasses.Contains(":first-page"));
        Assert.False(pager.PseudoClasses.Contains(":last-page"));

        pager.SelectedPageIndex = 11;
        Assert.True(pager.PseudoClasses.Contains(":last-page"));
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
        var pager = new MdPagination { NumberOfPages = 5, Orientation = Avalonia.Layout.Orientation.Vertical };
        using var host = Show(pager);

        Assert.True(pager.PseudoClasses.Contains(":vertical"));
        Assert.False(pager.PseudoClasses.Contains(":horizontal"));
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
