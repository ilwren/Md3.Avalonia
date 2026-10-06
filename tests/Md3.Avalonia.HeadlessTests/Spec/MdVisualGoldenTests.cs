using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>A reproducible scene rendered to a golden image.</summary>
/// <param name="Name">File stem of the baseline under <c>Spec/baselines/</c>.</param>
/// <param name="Width">Window width in device-independent pixels.</param>
/// <param name="Height">Window height in device-independent pixels.</param>
/// <param name="TextFree">
/// Text-free scenes rasterise identically on every platform and are therefore compared everywhere.
/// Scenes containing glyphs are only compared on the baseline OS, because font hinting differs
/// between Skia's platform back ends and would otherwise produce false failures.
/// </param>
/// <param name="Build">Scene factory.</param>
internal sealed record MdGoldenScene(string Name, int Width, int Height, bool TextFree, Func<Control> Build);

/// <summary>
/// Layer L3 — visual golden images.
/// </summary>
/// <remarks>
/// <para>
/// L1 and L2 can only check values someone thought to assert. A golden image catches the rest: a
/// state layer that stopped painting, an elevation shadow that inverted, a template that silently
/// lost a part. It is the only layer that fails on changes nobody predicted.
/// </para>
/// <para>
/// Both sides of the comparison are decoded by <see cref="MdPng"/>, so the live frame and the
/// committed baseline are compared in one canonical pixel order with no assumptions about
/// framebuffer layout.
/// </para>
/// <para>
/// When a baseline is missing the run does not fail: the candidate is written to
/// <c>artifacts/spec/baselines-new/</c> for a human to inspect and commit. A harness that adopts
/// its own output as truth would be an unconditional pass.
/// </para>
/// </remarks>
public sealed class MdVisualGoldenTests
{
    private const string Layer = "L3_visual";

    [AvaloniaFact]
    public void Rendered_Scenes_Match_The_Committed_Golden_Images()
    {
        var policy = MdSpecPolicy.Load();
        var report = new MdSpecReport("L3-visual", Layer, "L3 — visual golden images", policy);

        Assert.True(policy.IsLayerEnabled(Layer), "L3_visual is disabled in spec-snapshot/conformance-policy.json.");

        var onBaselineOs = string.Equals(
            policy.VisualBaselineOs,
            OperatingSystem.IsLinux() ? "Linux" : OperatingSystem.IsMacOS() ? "macOS" : "Windows",
            StringComparison.OrdinalIgnoreCase);

        var baselineDirectory = MdSpecPaths.BaselineDirectory;
        var candidateDirectory = Path.Combine(MdSpecPaths.EnsureArtifactDirectory(), "baselines-new");

        foreach (var scene in BuildScenes())
        {
            var rendered = Render(scene);
            if (rendered is null)
            {
                report.Gap(scene.Name, "the headless platform returned no rendered frame for this scene");
                continue;
            }

            var candidate = MdPng.Decode(rendered);
            var baselinePath = Path.Combine(baselineDirectory, scene.Name + ".png");

            if (!File.Exists(baselinePath))
            {
                Directory.CreateDirectory(candidateDirectory);
                File.WriteAllBytes(Path.Combine(candidateDirectory, scene.Name + ".png"), rendered);
                var message =
                    $"no committed baseline. A {candidate.Width}x{candidate.Height} candidate was written to " +
                    $"artifacts/spec/baselines-new/{scene.Name}.png; review it and copy it into " +
                    "tests/Md3.Avalonia.HeadlessTests/Spec/baselines/ to arm this golden.";

                if (policy.AutoAcceptMissingBaseline)
                {
                    report.Note(scene.Name, message);
                }
                else
                {
                    report.Gap(scene.Name, message);
                }

                continue;
            }

            MdRgbaImage baseline;
            try
            {
                baseline = MdPng.Decode(File.ReadAllBytes(baselinePath));
            }
            catch (Exception ex) when (ex is InvalidDataException or NotSupportedException)
            {
                report.Gap(scene.Name, $"the committed baseline could not be decoded: {ex.Message}");
                continue;
            }

            var diff = MdImageDiff.Compare(baseline, candidate, policy.VisualPerChannelTolerance);
            var comparable = scene.TextFree || onBaselineOs;

            if (!diff.SizeMatches)
            {
                report.Gap(
                    scene.Name,
                    $"baseline is {baseline.Width}x{baseline.Height} but the scene rendered at " +
                    $"{candidate.Width}x{candidate.Height}; a layout change this large needs a new baseline",
                    highConfidence: comparable);
                continue;
            }

            if (diff.ChangedRatio <= policy.VisualMaxChangedPixelRatio)
            {
                report.Pass(scene.Name);
                continue;
            }

            Directory.CreateDirectory(candidateDirectory);
            File.WriteAllBytes(Path.Combine(candidateDirectory, scene.Name + ".png"), rendered);
            File.WriteAllBytes(
                Path.Combine(candidateDirectory, scene.Name + ".diff.png"),
                MdPng.Encode(MdImageDiff.Visualise(baseline, candidate, policy.VisualPerChannelTolerance)));

            var detail =
                $"{diff.ChangedPixels} of {diff.TotalPixels} pixels ({diff.ChangedRatio:P3}) differ by more than " +
                $"{policy.VisualPerChannelTolerance}/255, worst channel delta {diff.MaxChannelDelta}. " +
                $"Candidate and a magenta diff mask are in artifacts/spec/baselines-new/.";

            if (comparable)
            {
                report.Gap(scene.Name, detail);
            }
            else
            {
                report.Note(
                    scene.Name,
                    detail + " Not gated: this scene contains text and the baseline OS is " +
                    policy.VisualBaselineOs + ".");
            }
        }

        report.Complete();
    }

    private static byte[]? Render(MdGoldenScene scene)
    {
        var application = Application.Current!;
        application.RequestedThemeVariant = ThemeVariant.Light;

        var window = new Window
        {
            Width = scene.Width,
            Height = scene.Height,
            WindowDecorations = WindowDecorations.None,
            Content = scene.Build()
        };

        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var frame = window.CaptureRenderedFrame();
            if (frame is null)
            {
                return null;
            }

            using var buffer = new MemoryStream();
            frame.Save(buffer, new PngBitmapEncoderOptions());
            return buffer.ToArray();
        }
        finally
        {
            window.Close();
        }
    }

    private static List<MdGoldenScene> BuildScenes() =>
    [
        // Pure geometry and colour: identical on every platform, so it is gated everywhere and acts
        // as the canary for shape-token and colour-role regressions.
        new MdGoldenScene("shape-scale", 420, 120, TextFree: true, BuildShapeScale),

        // State-layer opacities are the single most commonly broken piece of M3 interaction styling
        // and are invisible to value-level assertions once a template stops applying them.
        new MdGoldenScene("state-layers", 420, 120, TextFree: true, BuildStateLayers),

        // One text-bearing scene covering the controls whose templates change most often.
        new MdGoldenScene("component-sampler", 520, 220, TextFree: false, BuildComponentSampler)
    ];

    private static Control BuildShapeScale()
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        foreach (var step in new[] { "None", "ExtraSmall", "Small", "Medium", "Large", "ExtraLarge" })
        {
            row.Children.Add(new Border
            {
                Width = 56,
                Height = 56,
                CornerRadius = Resource("Md.Sys.Shape.Corner." + step) is CornerRadius corner ? corner : default,
                Background = Brush("Md.Sys.Color.PrimaryContainer"),
                BorderBrush = Brush("Md.Sys.Color.Outline"),
                BorderThickness = new Thickness(1)
            });
        }

        return Frame(row);
    }

    private static Control BuildStateLayers()
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        foreach (var state in new[]
                 {
                     "Md.Sys.State.Hover.Opacity", "Md.Sys.State.Focus.Opacity",
                     "Md.Sys.State.Pressed.Opacity", "Md.Sys.State.Dragged.Opacity",
                     "Md.Sys.State.Disabled.ContainerOpacity"
                 })
        {
            row.Children.Add(new Border
            {
                Width = 56,
                Height = 56,
                CornerRadius = new CornerRadius(16),
                Background = Brush("Md.Sys.Color.Surface"),
                Child = new Border
                {
                    Background = Brush("Md.Sys.Color.OnSurface"),
                    Opacity = Resource(state) is double opacity ? opacity : 0,
                    CornerRadius = new CornerRadius(16)
                }
            });
        }

        return Frame(row);
    }

    private static Control BuildComponentSampler()
    {
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                new MdButton { Content = "Filled", Variant = MdButtonVariant.Filled },
                new MdButton { Content = "Tonal", Variant = MdButtonVariant.Tonal },
                new MdButton { Content = "Outlined", Variant = MdButtonVariant.Outlined },
                new MdButton { Content = "Text", Variant = MdButtonVariant.Text }
            }
        };

        var selection = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                new MdCheckBox { Content = "On", IsChecked = true },
                new MdCheckBox { Content = "Off", IsChecked = false },
                new MdRadioButton { Content = "Picked", IsChecked = true },
                new MdSwitch { IsChecked = true },
                new MdChip { Content = "Chip", Variant = MdChipVariant.Assist }
            }
        };

        return Frame(new StackPanel
        {
            Spacing = 16,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Children = { buttons, selection }
        });
    }

    private static Control Frame(Control child) => new Border
    {
        Background = Brush("Md.Sys.Color.Surface"),
        Padding = new Thickness(16),
        Child = child
    };

    private static object? Resource(string key) =>
        Application.Current!.TryGetResource(key, ThemeVariant.Light, out var value) ? value : null;

    private static IBrush? Brush(string key) => Resource(key) switch
    {
        IBrush brush => brush,
        Color color => new SolidColorBrush(color),
        _ => null
    };
}
