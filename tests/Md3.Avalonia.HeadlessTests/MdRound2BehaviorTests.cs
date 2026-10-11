using Avalonia;
using Avalonia.Controls;
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
        page.Filter = "Avalonia";
        Dispatcher.UIThread.RunJobs();
        Assert.NotNull(page.SelectedLicense);
        Assert.Equal("Avalonia", page.SelectedLicense!.Package);
    }
}
