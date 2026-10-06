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
    public void A_Wide_Carousel_Fills_Its_Viewport_Instead_Of_Tapering_Immediately()
    {
        // Fitting is not enough: the arrangement only ever produced one large item and one
        // medium one, so on a desktop-width carousel everything from the third item on was a
        // 56 dip sliver and more than half the track was empty.
        foreach (var variant in new[] { MdCarouselVariant.MultiBrowse, MdCarouselVariant.Hero })
        {
            var carousel = new MdCarousel
            {
                Variant = variant,
                ItemWidth = 280,
                SmallItemWidth = 56,
                ItemSpacing = 8,
                ItemHeight = 160,
                ItemsSource = Enumerable.Range(0, 8).Select(i => $"Item {i}").ToArray(),
            };
            var window = new Window { Width = 1200, Height = 400, Content = carousel };
            window.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();
                var viewport = carousel.Bounds.Width - carousel.Padding.Left - carousel.Padding.Right;
                Assert.True(viewport > 600, $"{variant}: carousel never got a width");

                var widths = Enumerable.Range(0, 8)
                    .Select(i => (carousel.ContainerFromIndex(i) as Control)?.Bounds.Width ?? 0)
                    .ToArray();
                var content = widths.Sum() + 7 * carousel.ItemSpacing;
                var covered = Math.Min(content, viewport) / viewport;
                Assert.True(covered >= 0.85,
                    $"{variant}: the items cover {covered:P0} of a {viewport:0} dip viewport " +
                    $"({content:0.#} dip of content). Widths: " +
                    string.Join(", ", widths.Select(w => w.ToString("0.#"))));
            }
            finally { window.Close(); }
        }
    }

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
