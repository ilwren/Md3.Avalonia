using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Which column a waterfall puts a child in, and whether a collapsed side sheet still accepts clicks
/// over the page, are both invisible in a property dump: a panel that packed children into the wrong
/// column, or a sheet that left its content hit-testable while closed, would report a perfectly
/// healthy property set.
/// </summary>
public sealed class MdLayoutExtrasTests
{
    [AvaloniaFact]
    public void Waterfall_Panel_Puts_Each_Child_In_The_Shortest_Column()
    {
        var panel = new MdWaterfallPanel { Width = 200, Columns = 2, ColumnSpacing = 0, ItemSpacing = 0 };
        var children = new double[] { 100, 40, 70, 30 }
            .Select(height => new Border { Height = height, Background = Brushes.Red })
            .ToArray();
        foreach (var child in children) panel.Children.Add(child);

        using (Show(panel))
        {
            Dispatcher.UIThread.RunJobs();

            // Two 100-wide columns. The first child fills the left, the second takes the right, the
            // third stacks onto the right because it is shorter, and the fourth goes back to the
            // left once the right has grown past it.
            Assert.Equal(new Rect(0, 0, 100, 100), children[0].Bounds);
            Assert.Equal(new Rect(100, 0, 100, 40), children[1].Bounds);
            Assert.Equal(new Rect(100, 40, 100, 70), children[2].Bounds);
            Assert.Equal(new Rect(0, 100, 100, 30), children[3].Bounds);
        }
    }

    [AvaloniaFact]
    public void Waterfall_Panel_Derives_Its_Column_Count_And_Never_Reserves_An_Empty_One()
    {
        // 500 wide with a 200 minimum fits two columns; a third would come in under the minimum.
        Assert.Equal(2, ColumnsUsedFor(width: 500, minWidth: 200, children: 5));
        // Too narrow for even one minimum-width column still gives one column, never zero.
        Assert.Equal(1, ColumnsUsedFor(width: 100, minWidth: 200, children: 5));
        // A wide panel with only two children does not reserve three empty columns for the room it has.
        Assert.Equal(2, ColumnsUsedFor(width: 1000, minWidth: 200, children: 2));
    }

    [AvaloniaFact]
    public void Waterfall_Panel_Measures_To_The_Tallest_Column_And_Fills_The_Width_It_Is_Given()
    {
        var panel = new MdWaterfallPanel { Columns = 2, ColumnSpacing = 0, ItemSpacing = 10 };
        panel.Children.Add(new Border { Height = 100 });
        panel.Children.Add(new Border { Height = 40 });
        panel.Children.Add(new Border { Height = 50 });

        panel.Measure(new Size(300, double.PositiveInfinity));

        // The left column carries 100; the right carries 40, a 10 gap and 50, so both end at 110 and
        // the trailing gap of the tallest column is not part of the height.
        Assert.Equal(new Size(300, 100), panel.DesiredSize);
    }

    [AvaloniaFact]
    public void Waterfall_Feeds_Its_Panel_From_Its_Own_Properties()
    {
        var feed = new MdWaterfall { Width = 600, Columns = 3, ColumnSpacing = 4, ItemSpacing = 6 };
        for (var i = 0; i < 5; i++) feed.Items.Add(new Border { Height = 40 });

        using (Show(feed))
        {
            Dispatcher.UIThread.RunJobs();

            // The theme hands the panel to the presenter through an ItemsPanelTemplate, and the panel
            // reads its four numbers back off the control, so a host never writes that template.
            var panel = Assert.IsType<MdWaterfallPanel>(feed.ItemsPanelRoot);
            Assert.Equal(3, panel.Columns);
            Assert.Equal(4, panel.ColumnSpacing);
            Assert.Equal(6, panel.ItemSpacing);
            Assert.Equal(3, panel.Children.Select(child => child.Bounds.X).Distinct().Count());
        }
    }

    [AvaloniaFact]
    public void Waterfall_Takes_Its_Surface_From_A_Material_Role()
    {
        var feed = new MdWaterfall();
        using (Show(feed))
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(
                TokenColor(feed, "Md.Sys.Color.SurfaceContainerLowest"),
                Assert.IsType<SolidColorBrush>(feed.Background).Color);
        }
    }

    [AvaloniaFact]
    public void Side_Sheet_Opens_From_Its_Floating_Button_And_Hides_Its_Content_Again()
    {
        var sheet = new MdSideSheet
        {
            Width = 400,
            Height = 300,
            CollapsedWidth = 72,
            ExpandedWidth = 320,
            Header = "Filters",
            Content = new TextBlock { Text = "Only unread" }
        };
        // Motion off, so the width lands on its target in the same pass instead of animating towards
        // it and leaving the assertion reading a value that is still on its way.
        MdMotion.SetScheme(sheet, MdMotionScheme.None);

        using (Show(sheet))
        {
            Dispatcher.UIThread.RunJobs();
            var rail = Rail(sheet);
            var content = SheetContent(sheet);

            Assert.False(sheet.IsOpen);
            Assert.Equal(72, rail.Width);
            Assert.False(content.IsVisible);
            Assert.False(content.IsHitTestVisible);

            Toggle(sheet).RaiseEvent(new RoutedEventArgs(Button.ClickEvent, Toggle(sheet)));
            Dispatcher.UIThread.RunJobs();

            // Width and visibility are what the :open and :content-present classes drive, so
            // asserting them asserts the state took effect in the template, not just on the control.
            Assert.True(sheet.IsOpen);
            Assert.Equal(320, rail.Width);
            Assert.True(content.IsVisible);
            Assert.True(content.IsHitTestVisible);

            sheet.Close();
            Dispatcher.UIThread.RunJobs();

            Assert.False(sheet.IsOpen);
            Assert.Equal(72, rail.Width);
            Assert.False(content.IsHitTestVisible);
        }
    }

    [AvaloniaFact]
    public void Side_Sheet_Anchor_Picks_The_Edge_It_Slides_From()
    {
        var left = new MdSideSheet { Width = 400, Height = 300 };
        using (Show(left))
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(global::Avalonia.Layout.HorizontalAlignment.Left, Rail(left).HorizontalAlignment);
            Assert.Equal(Icon(left, "Md.Icon.ChevronRight"), left.ToggleIcon);
        }

        var right = new MdSideSheet { Width = 400, Height = 300, Anchor = MdSideSheetAnchor.Right };
        using (Show(right))
        {
            Dispatcher.UIThread.RunJobs();
            // The rail moves to the other edge and the chevron turns to point out from it, which is
            // the only way to tell the two anchors apart once both sheets are closed.
            Assert.Equal(global::Avalonia.Layout.HorizontalAlignment.Right, Rail(right).HorizontalAlignment);
            Assert.Equal(Icon(right, "Md.Icon.ChevronLeft"), right.ToggleIcon);
        }
    }

    [AvaloniaFact]
    public void Side_Sheet_Takes_Its_Surface_And_Shadow_From_Material_Tokens()
    {
        var sheet = new MdSideSheet();
        using (Show(sheet))
        {
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(
                TokenColor(sheet, "Md.Sys.Color.SurfaceContainerLow"),
                Assert.IsType<SolidColorBrush>(sheet.SheetBrush).Color);

            Assert.True(sheet.TryFindResource("Md.Sys.Elevation.Level3", sheet.ActualThemeVariant, out var token),
                "the elevation token the theme binds Elevation to is missing");
            Assert.Equal((BoxShadows)token!, sheet.Elevation);
        }
    }

    private static Border Rail(MdSideSheet sheet) =>
        sheet.GetVisualDescendants().OfType<Border>().Single(control => control.Name == "PART_Sheet");

    private static ContentPresenter SheetContent(MdSideSheet sheet) =>
        sheet.GetVisualDescendants().OfType<ContentPresenter>().Single(control => control.Name == "PART_SheetContent");

    private static Button Toggle(MdSideSheet sheet) =>
        sheet.GetVisualDescendants().OfType<Button>().Single(control => control.Name == "PART_Toggle");

    private static int ColumnsUsedFor(double width, double minWidth, int children)
    {
        var panel = new MdWaterfallPanel { Width = width, MinColumnWidth = minWidth, ColumnSpacing = 0, ItemSpacing = 0 };
        for (var i = 0; i < children; i++) panel.Children.Add(new Border { Height = 20 });

        using (Show(panel))
        {
            Dispatcher.UIThread.RunJobs();
            // With equal heights the greedy fill takes one child per column before it doubles back,
            // so the distinct left edges are the column count.
            return panel.Children.Select(child => child.Bounds.X).Distinct().Count();
        }
    }

    private static IDisposable Show(Control content)
    {
        var window = new Window { Width = 900, Height = 500, Content = content };
        window.Show();
        return new WindowScope(window);
    }

    private static object? Icon(Control scope, string key)
    {
        Assert.True(scope.TryFindResource(key, scope.ActualThemeVariant, out var value), key);
        return value;
    }

    private static Color TokenColor(Control scope, string key)
    {
        Assert.True(scope.TryFindResource(key, scope.ActualThemeVariant, out var value), key);
        return Assert.IsType<SolidColorBrush>(value).Color;
    }

    private sealed class WindowScope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }
}
