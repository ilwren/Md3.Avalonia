using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Regression guards for the round-2 review pass: the cascader must never present an empty
/// column (every rendered column carries the items it stands for, and picking a branch appends
/// exactly its children), and the license page must keep its detail pane populated by always
/// having a selection - a null selection rendered as a large featureless surface in reviews.
/// </summary>
public sealed class MdRound2BehaviorTests
{
    [AvaloniaFact]
    public void Cascader_Root_Column_Is_Populated_And_Branch_Selection_Appends_Children()
    {
        var items = new[]
        {
            new MdCascaderItem("asia", "Asia", new[]
            {
                new MdCascaderItem("cn", "China"),
                new MdCascaderItem("jp", "Japan"),
            }),
            new MdCascaderItem("eu", "Europe"),
        };
        var cascader = new MdCascader { ItemsSource = items };
        var window = new Window { Width = 520, Height = 520, Content = cascader };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        cascader.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();

        // Exactly one populated root column while nothing is selected.
        Assert.Single(cascader.Columns);
        Assert.Equal(2, cascader.Columns[0].Count);

        cascader.Select(0, items[0]);
        Dispatcher.UIThread.RunJobs();

        // The branch's children become the second column; no column is ever empty.
        Assert.Equal(2, cascader.Columns.Count);
        Assert.All(cascader.Columns, column => Assert.NotEmpty(column));
        Assert.Equal(new[] { "China", "Japan" }, cascader.Columns[1].Select(item => item.Label).ToArray());
        Assert.True(cascader.IsDropDownOpen); // a branch stays open for the next level
    }

    [AvaloniaFact]
    public void Cascader_Leaf_Selection_Closes_The_Popup_Without_Trailing_Columns()
    {
        var leaf = new MdCascaderItem("eu", "Europe");
        var cascader = new MdCascader { ItemsSource = new[] { leaf } };
        var window = new Window { Width = 520, Height = 520, Content = cascader };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        cascader.IsDropDownOpen = true;
        cascader.Select(0, leaf);
        Dispatcher.UIThread.RunJobs();

        Assert.False(cascader.IsDropDownOpen);
        Assert.Single(cascader.Columns);
        Assert.Equal("Europe", cascader.DisplayText);
    }

    [AvaloniaFact]
    public void Tabs_Selection_Indicator_Spans_The_Full_Tab_Width()
    {
        var tabs = new MdTabs { Width = 640 };
        tabs.Items.Add(new MdTabItem { Content = "Overview" });
        tabs.Items.Add(new MdTabItem { Content = "Activity" });
        tabs.SelectedIndex = 1; // after Items.Add: an index set on an empty list is coerced away
        var window = new Window { Width = 700, Height = 300, Content = tabs };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var item = Assert.IsType<MdTabItem>(tabs.Items[1]);
        Assert.True(item.IsSelected);
        Assert.True(item.Bounds.Width > 0);

        // MdTabs switches its items to the shared strip indicator; M3 wants that bar to span
        // the whole tab, not the old centered 24 px stub.
        var indicator = tabs.GetVisualDescendants().OfType<global::Avalonia.Controls.Border>()
            .Single(border => border.Name == "PART_SelectionIndicator");
        Assert.True(indicator.IsVisible);
        Assert.Equal(item.Bounds.Width, indicator.Bounds.Width, 1);
    }

    [AvaloniaFact]
    public void Navigation_Bar_Active_Indicator_Uses_The_M3_Pill_Size()
    {
        var bar = new MdNavigationBar { Width = 400 };
        bar.Items.Add(new MdNavigationBarItem { Label = "Home" });
        bar.Items.Add(new MdNavigationBarItem { Label = "Search" });
        var window = new Window { Width = 480, Height = 240, Content = bar };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        // M3: the navigation bar active indicator is a 64x32 dp pill.
        var indicator = bar.GetVisualDescendants().OfType<global::Avalonia.Controls.Border>()
            .First(border => border.Name == "PART_Indicator");
        Assert.Equal(64, indicator.Bounds.Width, 1);
        Assert.Equal(32, indicator.Bounds.Height, 1);
    }

    [AvaloniaFact]
    public void Snackbar_Container_Uses_The_M3_Four_DP_Corner()
    {
        var host = new MdSnackbarHost();
        var window = new Window { Width = 480, Height = 320, Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        // The snackbar template materializes when a message is presented.
        host.Show(new MdSnackbarMessage("Saved"));
        Dispatcher.UIThread.RunJobs();

        // M3: md.comp.snackbar.container.shape = 4dp, not a 24dp pill.
        var container = host.GetVisualDescendants().OfType<global::Avalonia.Controls.Border>()
            .First(border => border.Name == "PART_Container");
        Assert.Equal(new CornerRadius(4), container.CornerRadius);
    }

    [AvaloniaFact]
    public void Linear_Indicator_Renders_Determinate_And_Indeterminate_Without_Faulting()
    {
        // Smoke guard for the rewritten Render paths: the disjoint sweep (timer + two-segment
        // geometry) and the determinate/wavy branch must both survive real render passes.
        var indicator = new MdLinearProgressIndicator { Width = 240, Height = 12, IsIndeterminate = true };
        var window = new Window { Width = 400, Height = 120, Content = indicator };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.True(indicator.Bounds.Width > 0);

        indicator.IsIndeterminate = false;
        indicator.Value = 40;
        Dispatcher.UIThread.RunJobs();

        indicator.Shape = MdProgressShape.Wavy;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(40, indicator.Value);
    }

    [Fact]
    public void Linear_Indeterminate_Sweep_Follows_The_M3_Two_Segment_Timeline()
    {
        // Both segments start collapsed at the leading edge of the 1800 ms loop.
        Assert.Equal(0, MdLinearProgressIndicator.SampleIndeterminateEdge(0, 0), 6);
        Assert.Equal(0, MdLinearProgressIndicator.SampleIndeterminateEdge(0, 3), 6);

        // Segment 2's end edge is already crossing while segment 1's end has not started (delay 0 vs 1000).
        Assert.True(MdLinearProgressIndicator.SampleIndeterminateEdge(400, 3) > 0.4);
        Assert.Equal(0, MdLinearProgressIndicator.SampleIndeterminateEdge(400, 1), 6);

        // By the end of the loop every edge has crossed the track, so the restart is seamless.
        for (var i = 0; i < 4; i++)
        {
            Assert.Equal(1, MdLinearProgressIndicator.SampleIndeterminateEdge(1800, i), 6);
        }

        // Segment 1 always trails segment 2 (start edge index 0 vs 2, end edge index 1 vs 3).
        for (var t = 0; t <= 1800; t += 50)
        {
            Assert.True(MdLinearProgressIndicator.SampleIndeterminateEdge(t, 1) <= MdLinearProgressIndicator.SampleIndeterminateEdge(t, 3) + 1e-9);
            Assert.True(MdLinearProgressIndicator.SampleIndeterminateEdge(t, 0) <= MdLinearProgressIndicator.SampleIndeterminateEdge(t, 2) + 1e-9);
        }
    }

    [AvaloniaFact]
    public void License_Page_Keeps_A_Selection_So_The_Detail_Pane_Is_Never_Blank()
    {
        var page = new MdLicensePage
        {
            Licenses = new[]
            {
                new MdLicenseEntry("Md3.Avalonia", "Apache-2.0", "Copyright contributors."),
                new MdLicenseEntry("Avalonia", "MIT", "MIT licensed."),
            },
        };
        var window = new Window { Width = 900, Height = 600, Content = page };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        Assert.NotNull(page.SelectedLicense);
        Assert.Equal("Md3.Avalonia", page.SelectedLicense!.Package);

        // Filtering away the selection re-selects from the filtered list instead of going null.
        page.Filter = "MIT";
        Dispatcher.UIThread.RunJobs();
        Assert.NotNull(page.SelectedLicense);
        Assert.Equal("Avalonia", page.SelectedLicense!.Package);
    }
}
