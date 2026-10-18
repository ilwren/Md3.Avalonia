using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Reproduces the reported defect: an <see cref="MdScrollViewer"/> hosting card content with
/// <c>VerticalContentAlignment="Stretch"</c> showed a full-track scrollbar thumb on the very first
/// rendered frame, correcting itself only after a window resize. The scrollbar has to be
/// calibrated (Maximum/ViewportSize/Visibility in sync with the owner) by the first layout pass,
/// with no resize in between. The template keeps the bar in sync through explicit template
/// bindings, so the first frame can no longer depend on attach-ordering luck.
/// </summary>
public sealed class MdScrollBarFirstFrameTests
{
    [AvaloniaFact]
    public void Vertical_ScrollBar_Is_Calibrated_On_The_First_Layout_Without_A_Resize()
    {
        var viewer = new MdScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            Content = new ItemsControl
            {
                ItemsSource = Enumerable.Range(0, 3).Select(i => new Border
                {
                    Height = 350,
                    Child = new TextBlock { Text = $"card {i}" },
                }).ToArray(),
            },
        };

        var window = new Window { Width = 800, Height = 600, Content = viewer };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            // The composition overflows vertically: three 350 dip cards in a 600 dip window.
            Assert.True(viewer.Extent.Height > viewer.Viewport.Height,
                $"content must overflow on the first layout (extent {viewer.Extent.Height}, viewport {viewer.Viewport.Height})");

            var bar = viewer.GetVisualDescendants()
                .OfType<MdScrollBar>()
                .First(b => b.Orientation == Orientation.Vertical);

            // A bar left at RangeBase defaults (Maximum 100 / ViewportSize NaN) paints the
            // full-track thumb from the report. It must mirror the owner after the first pass.
            Assert.False(double.IsNaN(bar.ViewportSize), "ViewportSize was never initialised");
            Assert.Equal(viewer.Viewport.Height, bar.ViewportSize, 1);
            Assert.Equal(viewer.Extent.Height - viewer.Viewport.Height, bar.Maximum, 1);
            Assert.True(bar.Maximum > 0, "Maximum must reflect the overflow on the first frame");
            Assert.True(bar.IsVisible, "an Auto bar with overflow must be visible on the first frame");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Virtualized_List_Calibrates_Its_ScrollBar_On_The_First_Layout()
    {
        // MdList hosts the same MdScrollViewer template internally, so its bar must also be
        // calibrated on the first pass - with virtualization kept intact.
        var list = new MdList { ItemsSource = Enumerable.Range(0, 1000).Select(i => $"row {i}").ToArray() };

        var window = new Window { Width = 800, Height = 600, Content = list };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            var viewer = list.GetVisualDescendants().OfType<MdScrollViewer>().First();
            var bar = viewer.GetVisualDescendants()
                .OfType<MdScrollBar>()
                .First(b => b.Orientation == Orientation.Vertical);

            Assert.False(double.IsNaN(bar.ViewportSize), "ViewportSize was never initialised");
            Assert.True(viewer.Extent.Height > viewer.Viewport.Height, "a 1000-row list must overflow on the first frame");
            Assert.Equal(viewer.Extent.Height - viewer.Viewport.Height, bar.Maximum, 1);
            Assert.Equal(viewer.Viewport.Height, bar.ViewportSize, 1);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Vertical_ScrollBar_Stays_Hidden_When_Content_Fits_On_The_First_Layout()
    {
        var viewer = new MdScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = new ItemsControl
            {
                ItemsSource = Enumerable.Range(0, 2).Select(i => $"item {i}").ToArray(),
            },
        };

        var window = new Window { Width = 800, Height = 600, Content = viewer };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            var bar = viewer.GetVisualDescendants()
                .OfType<MdScrollBar>()
                .First(b => b.Orientation == Orientation.Vertical);

            Assert.Equal(0, bar.Maximum, 1);
            Assert.False(bar.IsVisible, "an Auto bar without overflow must not paint a thumb");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Vertical_ScrollBar_Resyncs_When_Content_Grows_After_The_First_Layout()
    {
        // A first frame where everything fits is legitimately calibrated to Maximum 0. When the
        // content later overflows, the bar has to follow without a window resize - the desktop
        // ordering defect the direct sync in MdScrollViewer closes.
        var items = new ObservableCollection<Border> { new() { Height = 100 } };

        var viewer = new MdScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = new ItemsControl { ItemsSource = items },
        };

        var window = new Window { Width = 800, Height = 600, Content = viewer };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            var bar = viewer.GetVisualDescendants()
                .OfType<MdScrollBar>()
                .First(b => b.Orientation == Orientation.Vertical);
            Assert.Equal(0, bar.Maximum, 1);

            for (var i = 0; i < 20; i++)
            {
                items.Add(new Border { Height = 100 });
            }

            Dispatcher.UIThread.RunJobs();

            Assert.True(viewer.Extent.Height > viewer.Viewport.Height, "the grown content must overflow");
            Assert.Equal(viewer.Extent.Height - viewer.Viewport.Height, bar.Maximum, 1);
            Assert.True(bar.Maximum > 0, "the bar must resync without a window resize");
            Assert.Equal(viewer.Viewport.Height, bar.ViewportSize, 1);
            Assert.True(bar.IsVisible, "an Auto bar that now overflows must become visible");
        }
        finally
        {
            window.Close();
        }
    }
}
