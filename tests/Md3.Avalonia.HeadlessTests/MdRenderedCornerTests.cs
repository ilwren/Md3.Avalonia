using System;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Gallery.Pages;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Reads the pixels the renderer actually produced. Thirty-odd tests in this project capture a
/// frame and assert it is not null, which cannot fail for a reason anyone cares about. Corner
/// rounding, transparency and overdraw are only visible this way.
/// </summary>
public sealed class MdRenderedCornerTests
{
    private static uint[] ReadPixels(Window window, out PixelSize size)
    {
        var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        size = frame!.PixelSize;
        var pixels = new uint[size.Width * size.Height];
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            frame.CopyPixels(
                new PixelRect(0, 0, size.Width, size.Height),
                handle.AddrOfPinnedObject(),
                pixels.Length * 4,
                size.Width * 4);
        }
        finally
        {
            handle.Free();
        }

        return pixels;
    }

    [AvaloniaFact]
    public void The_Borderless_Chrome_Preview_Card_Has_Rounded_Corners()
    {
        var page = new BorderlessWindowGalleryPage();
        // A colour nothing in the theme uses, so "the card did not cover this pixel" is unambiguous.
        var window = new Window
        {
            Width = 900,
            Height = 700,
            Background = new SolidColorBrush(Color.FromRgb(255, 0, 255)),
            Content = page,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1d, window.RenderScaling);

            var card = page.GetVisualDescendants().OfType<Border>()
                .First(b => Math.Abs(b.CornerRadius.TopLeft - 16) < 0.01 && b.BorderThickness.Top > 0);
            var origin = card.TranslatePoint(default, window);
            Assert.NotNull(origin);

            var pixels = ReadPixels(window, out var size);
            uint At(double x, double y)
            {
                var px = (int)Math.Round(x);
                var py = (int)Math.Round(y);
                Assert.InRange(px, 0, size.Width - 1);
                Assert.InRange(py, 0, size.Height - 1);
                return pixels[py * size.Width + px];
            }

            var left = origin!.Value.X;
            var top = origin.Value.Y;
            var right = left + card.Bounds.Width;
            var bottom = top + card.Bounds.Height;

            // The window brush, read somewhere the card definitely is not.
            var background = At(left + 40, top - 6);

            // Two dip inside a 16 dip radius is comfortably outside the rounded path: the corner
            // centre sits at (16,16), and (2,2) is 19.8 dip away from it.
            var corners = new (string Name, double X, double Y)[]
            {
                ("top left", left + 2, top + 2),
                ("top right", right - 3, top + 2),
                ("bottom left", left + 2, bottom - 3),
                ("bottom right", right - 3, bottom - 3),
            };

            // Guard the guard: if the card never painted, every sample would read as background
            // and the test would pass by accident.
            var filled = At(left + 60, top + 20);
            Assert.True(filled != background,
                "the card did not paint at all, so the corner samples would be meaningless");

            var square = corners.Where(c => At(c.X, c.Y) != background).Select(c => c.Name).ToArray();
            Assert.True(square.Length == 0,
                $"square corners: {string.Join(", ", square)}. The card sets CornerRadius 16 with " +
                "ClipToBounds, but its children paint opaque square backgrounds into the corners.");
        }
        finally
        {
            window.Close();
        }
    }
}
