using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>The Material keylines have to fit the viewport they were computed for.</summary>
public sealed class MdCarouselArrangementTests
{
    [AvaloniaFact]
    public void Multi_Browse_Keylines_Fit_The_Viewport_They_Were_Sized_Against()
    {
        // The whole point of the multi-browse arrangement is that a large, a medium and a small
        // item plus their spacing fit across the viewport, so the preview keyline stays visible.
        // The arrangement is recomputed from OnSizeChanged, which fires on the carousel's own
        // width - while the width it computes against prefers the scroll viewer's viewport. If
        // those two disagree on a pass, nothing fires again to correct it.
        foreach (var windowWidth in new[] { 360d, 480d, 720d, 1100d })
        {
            var carousel = new MdCarousel
            {
                Variant = MdCarouselVariant.MultiBrowse,
                ItemWidth = 320,
                SmallItemWidth = 56,
                ItemSpacing = 8,
                ItemHeight = 160,
                ItemsSource = Enumerable.Range(0, 8).Select(i => $"Item {i}").ToArray(),
            };
            var window = new Window { Width = windowWidth, Height = 400, Content = carousel };
            window.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();

                var viewport = carousel.Bounds.Width - carousel.Padding.Left - carousel.Padding.Right;
                Assert.True(viewport > 0, $"{windowWidth}: carousel never got a width");

                var widths = Enumerable.Range(0, 3)
                    .Select(i => (carousel.ContainerFromIndex(i) as Control)?.Bounds.Width ?? -1)
                    .ToArray();
                Assert.DoesNotContain(-1, widths);

                var needed = widths.Sum() + 2 * carousel.ItemSpacing;
                Assert.True(needed <= viewport + 1,
                    $"at a {windowWidth:0} dip window the three visible keylines need " +
                    $"{needed:0.#} dip but the viewport is {viewport:0.#}: widths " +
                    string.Join(" + ", widths.Select(w => w.ToString("0.#"))) +
                    $" plus 2 x {carousel.ItemSpacing:0.#} spacing");
            }
            finally { window.Close(); }
        }
    }
}
