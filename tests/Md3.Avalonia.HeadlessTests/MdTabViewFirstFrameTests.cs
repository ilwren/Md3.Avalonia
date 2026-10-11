using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Reproduces the reported desktop defect: with window Max constraints and
/// <c>TabStripPlacement="Bottom"</c>, the first frame painted the selected page over the whole
/// window and the tab strip only appeared after a window resize. Removing the Max constraints or
/// using Top placement rendered correctly. The strip must own its space from the very first
/// layout pass in every placement/constraint combination.
/// </summary>
public sealed class MdTabViewFirstFrameTests
{
    [AvaloniaFact]
    public void Bottom_Strip_With_Window_Max_Constraints_Is_Visible_On_The_First_Layout()
    {
        AssertStripOnFirstFrame(Dock.Bottom, constrainWindow: true);
    }

    [AvaloniaFact]
    public void Top_Strip_With_Window_Max_Constraints_Is_Visible_On_The_First_Layout()
    {
        AssertStripOnFirstFrame(Dock.Top, constrainWindow: true);
    }

    [AvaloniaFact]
    public void Bottom_Strip_Without_Window_Constraints_Is_Visible_On_The_First_Layout()
    {
        AssertStripOnFirstFrame(Dock.Bottom, constrainWindow: false);
    }

    private static void AssertStripOnFirstFrame(Dock placement, bool constrainWindow)
    {
        var viewer = new MdScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = new ItemsControl
            {
                ItemsSource = Enumerable.Range(0, 40)
                    .Select(i => new Border { Height = 300, Child = new TextBlock { Text = $"card {i}" } })
                    .ToArray(),
            },
        };

        var tabView = new MdTabView { TabStripPlacement = placement, SelectedIndex = 0 };
        tabView.Items.Add(new MdTabViewItem { Header = "home", Content = viewer });
        tabView.Items.Add(new MdTabViewItem { Header = "settings", Content = new Border { Height = 50 } });

        var window = new Window { Content = tabView };
        if (constrainWindow)
        {
            window.MinWidth = 400;
            window.MinHeight = 600;
            window.MaxWidth = 600;
            window.MaxHeight = 1000;
            window.CanMaximize = false;
        }

        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            var strip = tabView.GetVisualDescendants()
                .OfType<Border>()
                .First(b => b.Name == "PART_TabStripBorder");
            var host = tabView.GetVisualDescendants()
                .OfType<ContentPresenter>()
                .First(p => p.Name == "PART_SelectedContentHost");

            var diagnostics =
                $"[{placement}, constrained={constrainWindow}] window={window.Bounds.Size}, tabView={tabView.Bounds.Size}, strip={strip.Bounds}, host={host.Bounds}";

            Assert.True(strip.Bounds.Height >= 40,
                $"the tab strip must own its space on the first frame. {diagnostics}");
            Assert.True(host.Bounds.Height <= window.Bounds.Height - 40 + 1,
                $"the selected page must not cover the strip. {diagnostics}");
            Assert.True(strip.Bounds.Y + strip.Bounds.Height <= window.Bounds.Height + 1,
                $"the strip must stay inside the window. {diagnostics}");
        }
        finally
        {
            window.Close();
        }
    }
}
