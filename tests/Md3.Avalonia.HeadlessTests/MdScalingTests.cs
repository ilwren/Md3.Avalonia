using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Md3.Avalonia.HeadlessTests.Spec;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Display-scaling guards. Avalonia lays out in device-independent pixels, so every size in a
/// template scales with the OS scale factor for free. These tests pin the places where the
/// library leaves DIP space — the only places a scale factor can actually be dropped.
/// </summary>
public sealed class MdScalingTests
{
    private static IEnumerable<string> SourceFiles(params string[] relative)
    {
        var root = MdSpecPaths.RepositoryRoot;
        if (root is null) yield break;
        foreach (var part in relative)
        {
            var directory = Path.Combine(root, part);
            if (!Directory.Exists(directory)) continue;
            foreach (var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
                yield return file;
        }
    }

    [Fact]
    public void Every_Render_Target_Bitmap_Is_Sized_In_Physical_Pixels()
    {
        // A RenderTargetBitmap is one of the few APIs in the library that takes real pixels. Sizing
        // one from Bounds alone renders a half-resolution, blurry bitmap at 200% and a cropped one
        // at 150%, so each construction has to go through RenderScaling.
        var offenders = new List<string>();
        foreach (var file in SourceFiles(Path.Combine("src")))
        {
            var text = File.ReadAllText(file);
            if (!text.Contains("new RenderTargetBitmap(", StringComparison.Ordinal)) continue;
            foreach (Match match in Regex.Matches(text, @"new RenderTargetBitmap\((?<args>[^;]*?)\);", RegexOptions.Singleline))
            {
                // The surrounding statement block has to mention RenderScaling somewhere above it.
                var upTo = text[..match.Index];
                var window = upTo.Length <= 600 ? upTo : upTo[^600..];
                if (!window.Contains("RenderScaling", StringComparison.Ordinal))
                    offenders.Add($"{Path.GetFileName(file)}: {match.Value.Replace('\n', ' ')}");
            }
        }

        Assert.True(offenders.Count == 0,
            "RenderTargetBitmap sized without RenderScaling:\n  " + string.Join("\n  ", offenders));
    }

    [Fact]
    public void The_Libraries_Ship_No_Raster_Assets()
    {
        // Raster artwork is the usual reason a control library goes soft at 150%. Everything the
        // packages draw is either a vector Path, a brush, or the Material Symbols font.
        var root = MdSpecPaths.RepositoryRoot;
        Assert.NotNull(root);
        var raster = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif", "*.ico" }
            .SelectMany(pattern => Directory.EnumerateFiles(Path.Combine(root!, "src"), pattern, SearchOption.AllDirectories))
            .Select(path => Path.GetRelativePath(root!, path))
            .ToArray();

        Assert.True(raster.Length == 0,
            "Raster assets inside src/ do not scale cleanly:\n  " + string.Join("\n  ", raster));
    }

    [AvaloniaFact]
    public void Layout_Is_Expressed_In_Device_Independent_Pixels()
    {
        // The contract behind every hard-coded size in the themes: a DIP is a DIP, and the
        // renderer applies RenderScaling on top. If this ever stopped being true, every template
        // in the library would need auditing, so it is worth one assertion.
        var button = new MdButton { Content = "Scale" };
        var window = new Window { Width = 400, Height = 200, Content = button };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var topLevel = TopLevel.GetTopLevel(button);
            Assert.NotNull(topLevel);

            // Bounds are DIPs, independent of the scale factor the compositor later applies.
            var dips = button.Bounds.Size;
            var physical = PixelSize.FromSize(dips, topLevel!.RenderScaling);
            Assert.Equal((int)Math.Ceiling(dips.Width * topLevel.RenderScaling), physical.Width);
            Assert.True(dips.Height >= 24, "the control's height is expressed in DIPs, not physical pixels");
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Layout_Rounding_Stays_On_So_Edges_Land_On_Whole_Pixels()
    {
        // Layout rounding is what keeps a 1 DIP outline from straddling two physical pixels at
        // 125% and turning into a 2px grey smear. Avalonia rounds using RenderScaling, so the
        // library only has to avoid switching it off.
        var root = MdSpecPaths.RepositoryRoot;
        Assert.NotNull(root);
        var offenders = Directory
            .EnumerateFiles(Path.Combine(root!, "src"), "*.axaml", SearchOption.AllDirectories)
            .Where(file => File.ReadAllText(file).Contains("UseLayoutRounding=\"False\"", StringComparison.OrdinalIgnoreCase))
            .Select(file => Path.GetRelativePath(root!, file))
            .ToArray();

        Assert.True(offenders.Length == 0,
            "UseLayoutRounding is disabled in:\n  " + string.Join("\n  ", offenders));

        var control = new MdButton();
        Assert.True(control.UseLayoutRounding);
    }
}
