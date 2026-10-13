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
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaRichEditor.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Gallery.Pages;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdRichEditorTreeVisualRegressionTests
{
    [AvaloniaFact]
    public void RichEditor_Gallery_Starts_With_Rasterized_Sample_Content()
    {
        var page = new RichEditorGalleryPage();
        var window = new Window
        {
            Width = 1100,
            Height = 760,
            Content = page
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1d, window.RenderScaling);

            var editor = Assert.Single(page.GetVisualDescendants().OfType<RichEditor>());
            Assert.True(editor.IsVisible);
            Assert.True(editor.Bounds.Width > 0);
            Assert.True(editor.Bounds.Height > 0);

            var initialHtml = editor.ToHtml();
            Assert.Contains("Welcome to the Material 3 rich-text editor.", initialHtml, StringComparison.Ordinal);

            using var populatedFrame = window.CaptureRenderedFrame();
            Assert.NotNull(populatedFrame);
            var populatedPixels = ReadPixels(populatedFrame!, out var pixelSize);

            // Compare the Gallery's initial frame with the same editor cleared. This proves the
            // sample is not just stored in the document model: it actually changes pixels inside
            // the visible editor surface.
            editor.LoadHtml("<p></p>");
            Dispatcher.UIThread.RunJobs();
            using var emptyFrame = window.CaptureRenderedFrame();
            Assert.NotNull(emptyFrame);
            var emptyPixels = ReadPixels(emptyFrame!, out var emptyPixelSize);
            Assert.Equal(pixelSize, emptyPixelSize);

            var origin = editor.TranslatePoint(default, window);
            Assert.NotNull(origin);
            var left = (int)Math.Floor(origin!.Value.X);
            var top = (int)Math.Floor(origin.Value.Y);
            var right = (int)Math.Ceiling(origin.Value.X + editor.Bounds.Width);
            var bottom = (int)Math.Ceiling(origin.Value.Y + editor.Bounds.Height);
            var changedPixels = 0;
            for (var y = Math.Max(0, top); y < Math.Min(pixelSize.Height, bottom); y++)
            {
                for (var x = Math.Max(0, left); x < Math.Min(pixelSize.Width, right); x++)
                {
                    var index = y * pixelSize.Width + x;
                    if (populatedPixels[index] != emptyPixels[index]) changedPixels++;
                }
            }

            Assert.True(changedPixels > 20,
                $"Expected the initial sample text to rasterize in the RichEditor; only {changedPixels} pixels changed when the document was cleared.");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void RichEditor_Gallery_Text_Is_Light_In_Dark_Theme()
    {
        var application = Application.Current!;
        var previousVariant = application.RequestedThemeVariant;
        application.RequestedThemeVariant = ThemeVariant.Dark;

        var page = new RichEditorGalleryPage();
        var window = new Window { Width = 1100, Height = 760, Content = page };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1d, window.RenderScaling);

            var editor = Assert.Single(page.GetVisualDescendants().OfType<RichEditor>());
            Assert.True(application.TryGetResource(
                "Md.Sys.Color.OnSurface.Brush", ThemeVariant.Dark, out var onSurface));
            Assert.Equal(onSurface, editor.DefaultForeground);

            using var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            var pixels = ReadPixels(frame!, out var pixelSize);
            var origin = editor.TranslatePoint(default, window);
            Assert.NotNull(origin);
            var left = (int)Math.Floor(origin!.Value.X);
            var top = (int)Math.Floor(origin.Value.Y);
            var right = (int)Math.Ceiling(origin.Value.X + editor.Bounds.Width);
            var bottom = (int)Math.Ceiling(origin.Value.Y + editor.Bounds.Height);
            var lightInkPixels = 0;

            // The editor has no chrome of its own; the surrounding Gallery Border supplies the
            // dark surface. Bright pixels inside the document viewport therefore come from the
            // actual default-styled glyphs, not a label or toolbar.
            for (var y = Math.Max(0, top); y < Math.Min(pixelSize.Height, bottom); y++)
            {
                for (var x = Math.Max(0, left); x < Math.Min(pixelSize.Width, right); x++)
                {
                    var rgb = pixels[y * pixelSize.Width + x] & 0x00ffffff;
                    var red = (rgb >> 16) & 0xff;
                    var green = (rgb >> 8) & 0xff;
                    var blue = rgb & 0xff;
                    if (red + green + blue > 450) lightInkPixels++;
                }
            }

            Assert.True(lightInkPixels > 20,
                $"Expected the default-styled RichEditor text to rasterize brightly on a dark surface; found {lightInkPixels} bright pixels.");
        }
        finally
        {
            window.Close();
            application.RequestedThemeVariant = previousVariant;
            Dispatcher.UIThread.RunJobs();
        }
    }

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
