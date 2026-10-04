using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>One cell of the geometry matrix.</summary>
internal sealed record MdGeometryCase(
    string Name,
    Control Control,
    string[] ContainerParts)
{
    public string? HeightKey { get; init; }

    public string? WidthKey { get; init; }

    public string? CornerKey { get; init; }

    /// <summary>When true the measured height only has to be at least the oracle value.</summary>
    public bool HeightIsMinimum { get; init; }

    /// <summary>When true the control must offer a Material-minimum interactive target.</summary>
    public bool RequiresTouchTarget { get; init; } = true;
}

/// <summary>
/// Layer L2 — rendered geometry and interactive targets.
/// </summary>
/// <remarks>
/// <para>
/// L1 proves a token holds the right number. This layer proves the number reaches the screen:
/// it lays out a real control matrix and measures the arranged bounds and corner radii of the
/// template parts, comparing them against the same external oracle.
/// </para>
/// <para>
/// The two are genuinely different questions. <c>MdChip</c> hard-codes 32dp and 8dp in its theme
/// instead of exposing <c>Md.Comp.Chip.*</c> tokens, so it is a gap at L1 and a pass at L2; a
/// token that exists but is never bound in the template is the opposite. Neither layer alone is
/// sufficient.
/// </para>
/// <para>
/// Every case shares ONE window and ONE layout pass. Avalonia headless has a single process-wide
/// dispatcher, so per-case windows dominate the cost; 40-odd controls in one tree keep the whole
/// matrix inside a couple of hundred milliseconds.
/// </para>
/// </remarks>
public sealed class MdGeometryConformanceTests
{
    private const string Layer = "L2_geometry";

    [AvaloniaFact]
    public void Rendered_Geometry_And_Touch_Targets_Match_The_Specification_Oracle()
    {
        var policy = MdSpecPolicy.Load();
        var oracle = MdSpecOracle.Load();
        var report = new MdSpecReport("L2-geometry", Layer, "L2 — rendered geometry and interactive targets", policy);

        Assert.True(policy.IsLayerEnabled(Layer), "L2_geometry is disabled in spec-snapshot/conformance-policy.json.");

        var cases = BuildMatrix();
        Assert.NotEmpty(cases);

        var host = new WrapPanel { Orientation = Orientation.Horizontal };
        foreach (var item in cases)
        {
            host.Children.Add(item.Control);
        }

        using (MdVisualTreeQuery.Show(host, 1000, 900))
        {
            foreach (var item in cases)
            {
                Evaluate(item, oracle, report);
            }

            ReportIconButtonWidthOrdering(cases, report);
        }

        report.Complete();
    }

    /// <summary>
    /// The oracle only pins the default icon-button width, but the three width modes still have to
    /// be ordered. A derived invariant like this costs nothing and catches a swapped token that an
    /// absolute-value check cannot see, because no published number constrains narrow and wide.
    /// </summary>
    private static void ReportIconButtonWidthOrdering(List<MdGeometryCase> cases, MdSpecReport report)
    {
        double? WidthOf(string name) =>
            cases.FirstOrDefault(c => c.Name == name) is { } match &&
            MdVisualTreeQuery.FindPart<Control>(match.Control, match.ContainerParts) is { } part
                ? (double?)part.Bounds.Width
                : null;

        var narrow = WidthOf("MdIconButton/Medium/Narrow");
        var standard = WidthOf("MdIconButton/Medium");
        var wide = WidthOf("MdIconButton/Medium/Wide");

        if (narrow is null || standard is null || wide is null)
        {
            report.Gap(
                "MdIconButton/width-ordering",
                "could not measure all three width modes, so their ordering could not be verified",
                highConfidence: false);
            return;
        }

        report.Check(
            "MdIconButton/width-ordering",
            narrow < standard && standard < wide,
            $"width modes must widen monotonically, measured narrow={narrow:0.##}, default={standard:0.##}, " +
            $"wide={wide:0.##}");
    }

    private static void Evaluate(MdGeometryCase item, MdSpecOracle oracle, MdSpecReport report)
    {
        var container = MdVisualTreeQuery.FindPart<Control>(item.Control, item.ContainerParts);
        if (container is null)
        {
            report.Gap(
                item.Name,
                $"template exposes none of the expected container parts ({string.Join(", ", item.ContainerParts)}); " +
                "geometry cannot be measured");
            return;
        }

        var size = container.Bounds.Size;

        var heightKey = item.HeightKey;
        if (heightKey is not null && oracle.Scalar(heightKey) is { } expectedHeight)
        {
            var highConfidence = oracle.ByKey[heightKey].IsHighConfidence;
            if (item.HeightIsMinimum)
            {
                report.Check(
                    item.Name + "/height",
                    size.Height >= expectedHeight - oracle.ToleranceDip,
                    $"container is {size.Height:0.##}dp tall, the oracle requires at least {expectedHeight:0.##}dp",
                    highConfidence);
            }
            else
            {
                report.CheckScalar(item.Name + "/height", expectedHeight, size.Height, oracle.ToleranceDip, highConfidence);
            }
        }

        var widthKey = item.WidthKey;
        if (widthKey is not null && oracle.Scalar(widthKey) is { } expectedWidth)
        {
            report.CheckScalar(
                item.Name + "/width",
                expectedWidth,
                size.Width,
                oracle.ToleranceDip,
                oracle.ByKey[widthKey].IsHighConfidence);
        }

        var cornerKey = item.CornerKey;
        if (cornerKey is not null && oracle.Quad(cornerKey) is { } expectedCorner)
        {
            var corner = MdVisualTreeQuery.CornerRadiusOf(container);
            if (corner is null)
            {
                report.Gap(
                    item.Name + "/corner",
                    $"part '{container.Name}' is a {container.GetType().Name}, which carries no CornerRadius to verify",
                    highConfidence: false);
            }
            else
            {
                var actual = new[]
                {
                    corner.Value.TopLeft, corner.Value.TopRight, corner.Value.BottomRight, corner.Value.BottomLeft
                };
                var worst = expectedCorner.Select((e, i) => Math.Abs(e - actual[i])).Max();
                report.Check(
                    item.Name + "/corner",
                    worst <= oracle.ToleranceCorner,
                    $"corner radius is [{string.Join(", ", actual.Select(v => v.ToString("0.##")))}], " +
                    $"the oracle expects [{string.Join(", ", expectedCorner.Select(v => v.ToString("0.##")))}]",
                    oracle.ByKey[cornerKey].IsHighConfidence);
            }
        }

        if (item.RequiresTouchTarget)
        {
            EvaluateTouchTarget(item, oracle, report);
        }
    }

    /// <summary>
    /// Material requires a 48x48dp interactive target even when the visual container is smaller.
    /// Two things have to hold: the control must be arranged at least that large, and the template
    /// root must actually paint a background over that area, otherwise the extra region is empty
    /// space that pointer input falls straight through.
    /// </summary>
    private static void EvaluateTouchTarget(MdGeometryCase item, MdSpecOracle oracle, MdSpecReport report)
    {
        var bounds = item.Control.Bounds;
        var minimum = oracle.TouchTargetMinDip;
        var subject = item.Name + "/touch-target";

        if (bounds.Width + oracle.ToleranceDip < minimum || bounds.Height + oracle.ToleranceDip < minimum)
        {
            report.Gap(
                subject,
                $"arranged at {bounds.Width:0.##}x{bounds.Height:0.##}dp; Material requires at least " +
                $"{minimum:0}x{minimum:0}dp of interactive target even when the visual container is smaller");
            return;
        }

        var templateRoot = item.Control.GetVisualDescendants().OfType<Panel>().FirstOrDefault();
        if (templateRoot is null)
        {
            report.Gap(subject, "template root is not a Panel, so the hit-test surface could not be inspected",
                highConfidence: false);
            return;
        }

        if (templateRoot.Background is null)
        {
            report.Gap(
                subject,
                $"arranged at {bounds.Width:0.##}x{bounds.Height:0.##}dp but the template root paints no " +
                "Background, so pointer input falls through the padding around the visual container");
            return;
        }

        report.Pass(subject);
    }

    private static List<MdGeometryCase> BuildMatrix()
    {
        var cases = new List<MdGeometryCase>();

        // --- MdButton: size x shape drives every geometric token on the control. -----------------
        foreach (var size in new[]
                 {
                     MdButtonSize.ExtraSmall, MdButtonSize.Small, MdButtonSize.Medium,
                     MdButtonSize.Large, MdButtonSize.ExtraLarge
                 })
        {
            var token = ButtonSizeToken(size);
            foreach (var shape in new[] { MdButtonShape.Round, MdButtonShape.Square })
            {
                cases.Add(new MdGeometryCase(
                    $"MdButton/{size}/{shape}",
                    new MdButton { Content = "Label", Size = size, Shape = shape },
                    ["PART_Container"])
                {
                    HeightKey = $"Md.Comp.Button.{token}.Container.Height",
                    CornerKey = $"Md.Comp.Button.{token}.Shape.{shape}"
                });
            }
        }

        // Colour variants must not perturb geometry; one sweep at the default size proves it.
        foreach (var variant in new[]
                 {
                     MdButtonVariant.Filled, MdButtonVariant.Tonal, MdButtonVariant.Outlined,
                     MdButtonVariant.Text, MdButtonVariant.Elevated
                 })
        {
            cases.Add(new MdGeometryCase(
                $"MdButton/Variant/{variant}",
                new MdButton { Content = "Label", Variant = variant },
                ["PART_Container"])
            {
                HeightKey = "Md.Comp.Button.Small.Container.Height",
                CornerKey = "Md.Comp.Button.Small.Shape.Round"
            });
        }

        // --- MdIconButton: sweep the size scale at the default width, then the width modes once.
        // The oracle only constrains the default width, so sweeping all 5x3 combinations would add
        // ten cases that re-check the values the size sweep already covers.
        foreach (var size in new[]
                 {
                     MdButtonSize.ExtraSmall, MdButtonSize.Small, MdButtonSize.Medium,
                     MdButtonSize.Large, MdButtonSize.ExtraLarge
                 })
        {
            var token = ButtonSizeToken(size);
            cases.Add(new MdGeometryCase(
                $"MdIconButton/{size}",
                new MdIconButton { Size = size },
                ["PART_Container"])
            {
                HeightKey = $"Md.Comp.IconButton.{token}.Height",
                WidthKey = $"Md.Comp.IconButton.{token}.DefaultWidth",
                CornerKey = $"Md.Comp.IconButton.{token}.RoundShape"
            });
        }

        foreach (var width in new[] { MdIconButtonWidth.Narrow, MdIconButtonWidth.Wide })
        {
            cases.Add(new MdGeometryCase(
                $"MdIconButton/Medium/{width}",
                new MdIconButton { Size = MdButtonSize.Medium, WidthMode = width },
                ["PART_Container"])
            {
                HeightKey = "Md.Comp.IconButton.Medium.Height",
                CornerKey = "Md.Comp.IconButton.Medium.RoundShape"
            });
        }

        // --- Floating action button. -------------------------------------------------------------
        foreach (var size in new[] { MdFabSize.Small, MdFabSize.Regular, MdFabSize.Medium, MdFabSize.Large })
        {
            cases.Add(new MdGeometryCase(
                $"MdFloatingActionButton/{size}",
                new MdFloatingActionButton { Size = size },
                ["PART_Container"])
            {
                HeightKey = $"Md.Comp.Fab.{size}.Size",
                WidthKey = $"Md.Comp.Fab.{size}.Size",
                CornerKey = $"Md.Comp.Fab.{size}.Shape"
            });
        }

        // --- Selection controls: small visual container, full-size target. -----------------------
        cases.Add(new MdGeometryCase("MdCheckBox", new MdCheckBox { Content = "Label" }, ["PART_Container"])
        {
            HeightKey = "Md.Comp.CheckBox.Container.Size",
            WidthKey = "Md.Comp.CheckBox.Container.Size",
            CornerKey = "Md.Comp.CheckBox.Container.Shape"
        });

        cases.Add(new MdGeometryCase(
            "MdCheckBox/StateLayer",
            new MdCheckBox { Content = "Label" },
            ["PART_StateLayer"])
        {
            HeightKey = "Md.Comp.CheckBox.StateLayer.Size",
            WidthKey = "Md.Comp.CheckBox.StateLayer.Size",
            RequiresTouchTarget = false
        });

        cases.Add(new MdGeometryCase(
            "MdRadioButton",
            new MdRadioButton { Content = "Label" },
            ["PART_OuterCircle", "PART_Container"])
        {
            HeightKey = "Md.Comp.RadioButton.Icon.Size",
            WidthKey = "Md.Comp.RadioButton.Icon.Size"
        });

        cases.Add(new MdGeometryCase(
            "MdRadioButton/StateLayer",
            new MdRadioButton { Content = "Label" },
            ["PART_StateLayer"])
        {
            HeightKey = "Md.Comp.RadioButton.StateLayer.Size",
            WidthKey = "Md.Comp.RadioButton.StateLayer.Size",
            RequiresTouchTarget = false
        });

        cases.Add(new MdGeometryCase("MdSwitch", new MdSwitch(), ["PART_Track"])
        {
            HeightKey = "Md.Comp.Switch.Track.Height",
            WidthKey = "Md.Comp.Switch.Track.Width"
        });

        // --- Text fields: the container height is a minimum, not a fixed size. -------------------
        foreach (var variant in new[] { MdTextBoxVariant.Filled, MdTextBoxVariant.Outlined })
        {
            cases.Add(new MdGeometryCase(
                $"MdTextBox/{variant}",
                new MdTextBox { Variant = variant, Label = "Label", Width = 220 },
                ["PART_Container"])
            {
                HeightKey = "Md.Comp.TextField.Container.MinHeight",
                HeightIsMinimum = true,
                CornerKey = $"Md.Comp.TextField.{variant}.Container.Shape"
            });
        }

        // --- Chips: tokens are absent (an L1 gap) but the rendered geometry can still be checked.
        foreach (var variant in new[]
                 {
                     MdChipVariant.Assist, MdChipVariant.Filter, MdChipVariant.Input, MdChipVariant.Suggestion
                 })
        {
            cases.Add(new MdGeometryCase(
                $"MdChip/{variant}",
                new MdChip { Content = "Chip", Variant = variant },
                ["PART_Container"])
            {
                HeightKey = "Md.Comp.Chip.Container.Height",
                CornerKey = "Md.Comp.Chip.Container.Shape"
            });
        }

        return cases;
    }

    private static string ButtonSizeToken(MdButtonSize size) => size switch
    {
        MdButtonSize.ExtraSmall => "XSmall",
        MdButtonSize.Small => "Small",
        MdButtonSize.Medium => "Medium",
        MdButtonSize.Large => "Large",
        MdButtonSize.ExtraLarge => "XLarge",
        _ => size.ToString()
    };
}
