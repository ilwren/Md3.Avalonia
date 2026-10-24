using System;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ShapePath = Avalonia.Controls.Shapes.Path;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Extra.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdTreeViewVisualRegressionTests
{
    [AvaloniaFact]
    public void TreeView_Chevron_Painted_Ink_Is_Centered_In_The_Transparent_State_Target()
    {
        var tree = new MdTreeView
        {
            Width = 180,
            Height = 120,
            Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
            Roots = [new MdTreeNode("root", "Root", [new MdTreeNode("child", "Child")])]
        };
        var window = new Window
        {
            Width = 240,
            Height = 160,
            Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
            Content = tree
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            // Keep the hover state off the toggle so the sampled pixels are only the chevron ink.
            window.MouseMove(new Point(window.Width - 2, window.Height - 2), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1d, window.RenderScaling);

            var toggle = Assert.Single(tree.GetVisualDescendants().OfType<ToggleButton>());
            var chevron = Assert.Single(toggle.GetVisualDescendants().OfType<ShapePath>());
            Assert.Equal(40d, toggle.Bounds.Width);
            Assert.Equal(40d, toggle.Bounds.Height);
            Assert.True(chevron.Bounds.Width > 0);
            Assert.True(chevron.Bounds.Height > 0);

            var origin = toggle.TranslatePoint(default, window);
            Assert.NotNull(origin);
            var left = (int)Math.Round(origin!.Value.X);
            var top = (int)Math.Round(origin.Value.Y);
            var width = (int)Math.Round(toggle.Bounds.Width);
            var height = (int)Math.Round(toggle.Bounds.Height);

            using var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            var pixels = ReadPixels(frame!, out var size);
            Assert.InRange(left, 0, size.Width - width);
            Assert.InRange(top, 0, size.Height - height);

            // The button surface is transparent over a uniform tree surface. Measure the weighted
            // centroid of the actual rasterized ink rather than trusting Path alignment properties;
            // this catches a centered Path control whose geometry is painted off-center within it.
            var background = pixels[(top + height / 2) * size.Width + left + 2];
            double weightedX = 0;
            double weightedY = 0;
            double totalWeight = 0;
            for (var y = top; y < top + height; y++)
            {
                for (var x = left; x < left + width; x++)
                {
                    var weight = PixelDistance(pixels[y * size.Width + x], background);
                    if (weight <= 8) continue;
                    weightedX += (x + 0.5) * weight;
                    weightedY += (y + 0.5) * weight;
                    totalWeight += weight;
                }
            }

            Assert.True(totalWeight > 0, "The TreeView chevron did not rasterize inside its toggle target.");
            var paintedCenterX = weightedX / totalWeight;
            var paintedCenterY = weightedY / totalWeight;
            var targetCenterX = left + width / 2d;
            var targetCenterY = top + height / 2d;
            Assert.InRange(Math.Abs(paintedCenterX - targetCenterX), 0d, 1d);
            Assert.InRange(Math.Abs(paintedCenterY - targetCenterY), 0d, 1d);
        }
        finally
        {
            window.Close();
        }
    }

    private static uint[] ReadPixels(Bitmap frame, out PixelSize size)
    {
        size = frame.PixelSize;
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

    private static int PixelDistance(uint left, uint right)
    {
        var distance = 0;
        for (var shift = 0; shift < 32; shift += 8)
        {
            distance += Math.Abs((int)((left >> shift) & 0xff) - (int)((right >> shift) & 0xff));
        }

        return distance;
    }
}
