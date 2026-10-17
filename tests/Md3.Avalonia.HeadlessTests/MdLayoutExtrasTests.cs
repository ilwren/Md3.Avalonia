using System;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Which column a waterfall puts a child in, and which of its children it decides to glide, are both
/// invisible in a property dump: a panel that packed children into the wrong column, or one that
/// restarted a transition on every frame of a resize, would report a perfectly healthy property set.
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
                TokenColor(feed, "Md.Sys.Color.SurfaceContainerLowest.Brush"),
                Assert.IsType<SolidColorBrush>(feed.Background).Color);
        }
    }

    [AvaloniaFact]
    public void Waterfall_Glides_Its_Children_Only_When_The_Column_Count_Changes()
    {
        var panel = new MdWaterfallPanel { Width = 400, MinColumnWidth = 180, ColumnSpacing = 0, ItemSpacing = 0 };
        for (var i = 0; i < 4; i++) panel.Children.Add(new Border { Height = 60 });

        using (Show(panel))
        {
            Dispatcher.UIThread.RunJobs();
            // Two columns fit in 400. Nothing has reflowed, so nothing should be mid-glide: a resize
            // that keeps the column count has the children tracking it already, and animating every
            // one of those frames would restart the transition before it finished.
            Assert.All(panel.Children, child => Assert.Null(child.Transitions));

            panel.Width = 600;
            Dispatcher.UIThread.RunJobs();
            // Three columns fit in 600, so the children have new places and get carried to them.
            Assert.Contains(panel.Children, child => child.Transitions is { Count: > 0 });
        }
    }

    [AvaloniaFact]
    public void Waterfall_Leaves_Children_Alone_When_Animation_Is_Off_Or_Motion_Is_Reduced()
    {
        var panel = new MdWaterfallPanel
        {
            Width = 400, MinColumnWidth = 180, ColumnSpacing = 0, ItemSpacing = 0, AnimateLayout = false
        };
        for (var i = 0; i < 4; i++) panel.Children.Add(new Border { Height = 60 });
        using (Show(panel))
        {
            Dispatcher.UIThread.RunJobs();
            panel.Width = 600;
            Dispatcher.UIThread.RunJobs();
            Assert.All(panel.Children, child => Assert.Null(child.Transitions));
        }

        var reduced = new MdWaterfallPanel { Width = 400, MinColumnWidth = 180, ColumnSpacing = 0, ItemSpacing = 0 };
        for (var i = 0; i < 4; i++) reduced.Children.Add(new Border { Height = 60 });
        MdMotion.SetScheme(reduced, MdMotionScheme.Reduced);
        using (Show(reduced))
        {
            Dispatcher.UIThread.RunJobs();
            reduced.Width = 600;
            Dispatcher.UIThread.RunJobs();
            Assert.All(reduced.Children, child => Assert.Null(child.Transitions));
        }
    }

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
