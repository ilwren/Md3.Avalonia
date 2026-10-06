using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery.Pages;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdCarouselProbe
{
    [AvaloniaFact]
    public void Report()
    {
        var page = new CarouselGalleryPage { VerticalAlignment = VerticalAlignment.Top };
        var window = new Window { Width = 1200, Height = 1600, Content = page };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            var size = frame!.PixelSize;
            var px = new uint[size.Width * size.Height];
            var h = GCHandle.Alloc(px, GCHandleType.Pinned);
            try
            {
                frame.CopyPixels(new PixelRect(0, 0, size.Width, size.Height),
                    h.AddrOfPinnedObject(), px.Length * 4, size.Width * 4);
            }
            finally { h.Free(); }

            var sb = new StringBuilder();
            var carousels = page.GetVisualDescendants().OfType<MdCarousel>().ToList();
            sb.Append(CultureInfo.InvariantCulture, $"{carousels.Count} carousels. ");

            foreach (var (c, ci) in carousels.Select((c, i) => (c, i)))
            {
                var o = c.TranslatePoint(default, window) ?? default;
                sb.Append(CultureInfo.InvariantCulture,
                    $"[{ci}] {c.Variant} itemW={c.ItemWidth:0} smallW={c.SmallItemWidth:0} " +
                    $"box={c.Bounds.Width:0}x{c.Bounds.Height:0}@{o.X:0},{o.Y:0} items: ");

                for (var i = 0; i < Math.Min(5, c.ItemCount); i++)
                {
                    if (c.ContainerFromIndex(i) is not Control cont) continue;
                    var host = cont.GetVisualDescendants().OfType<ContentPresenter>()
                        .FirstOrDefault(p => p.Name == "PART_ContentHost");
                    var co = cont.TranslatePoint(default, window) ?? default;
                    sb.Append(CultureInfo.InvariantCulture,
                        $"{i}:cont={cont.Bounds.Width:0}@{co.X:0}");
                    if (host is not null)
                    {
                        var ho = host.TranslatePoint(default, window) ?? default;
                        sb.Append(CultureInfo.InvariantCulture,
                            $"/host={host.Bounds.Width:0}@{ho.X:0}(set={MdCarousel.GetContentExtent(cont):0.#}) ");
                    }
                    else sb.Append("/host=MISSING ");
                }

                // What the renderer actually put on a line through the middle of this carousel.
                var y = (int)Math.Round(o.Y + c.Bounds.Height / 2);
                if (y > 0 && y < size.Height)
                {
                    sb.Append("scan:");
                    var runs = 0;
                    var x0 = Math.Max(0, (int)o.X);
                    var x1 = Math.Min(size.Width - 1, (int)(o.X + c.Bounds.Width));
                    var prev = px[y * size.Width + x0];
                    var start = x0;
                    for (var x = x0 + 1; x <= x1 && runs < 14; x++)
                    {
                        var cur = px[y * size.Width + x];
                        if (cur == prev) continue;
                        sb.Append(CultureInfo.InvariantCulture, $" {prev:X8}x{x - start}");
                        runs++;
                        prev = cur;
                        start = x;
                    }
                    sb.Append(CultureInfo.InvariantCulture, $" {prev:X8}x{x1 - start + 1}; ");
                }
            }

            Assert.Fail(sb.ToString());
        }
        finally { window.Close(); }
    }
}
