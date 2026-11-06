using System.Linq;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Md3.Avalonia.Themes.Dynamic;
using Xunit;

namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>
/// Layer L1 — the Material 3 baseline colour scheme, as published.
/// </summary>
/// <remarks>
/// <para>
/// Material fixes the baseline scheme as 49 named roles with literal values
/// (material-web, <c>tokens/versions/v0_192/_md-sys-color.scss</c>, which references
/// <c>_md-ref-palette.scss</c>). <see cref="MdBaselineScheme"/> is that table, transcribed. This test
/// compares three things against it, because all three can drift independently:
/// </para>
/// <list type="number">
///   <item>the measurement — the theme the resource system actually hands a control;</item>
///   <item>the generator — what <see cref="MdThemeManager.Apply"/> writes at runtime for the default
///     options, which used to produce a different palette (#65558F) from the same seed (#6750A4);</item>
///   <item>the structure — every role has a <c>.Brush</c>, and the fixed roles are identical in light
///     and dark, which is the property that makes them fixed.</item>
/// </list>
/// <para>
/// The comparison is exact, with no tolerance. These are authored values, not computed ones: a
/// difference is a transcription error or a wrong choice of source, never rounding.
/// </para>
/// </remarks>
public sealed class MdBaselineColorTests
{
    private const string Layer = "L1_tokens";

    /// <summary>
    /// Roles Material gives the same value in both variants for a different reason than "fixed": a
    /// scrim and a shadow are black on a light surface and black on a dark one.
    /// </summary>
    private static readonly string[] VariantInvariantRoles = ["Scrim", "Shadow"];

    private static readonly string[] FixedRoles =
    [
        "PrimaryFixed", "PrimaryFixedDim", "OnPrimaryFixed", "OnPrimaryFixedVariant",
        "SecondaryFixed", "SecondaryFixedDim", "OnSecondaryFixed", "OnSecondaryFixedVariant",
        "TertiaryFixed", "TertiaryFixedDim", "OnTertiaryFixed", "OnTertiaryFixedVariant"
    ];

    [AvaloniaFact]
    public void The_Static_Theme_Is_The_Published_M3_Baseline_Scheme()
    {
        var policy = MdSpecPolicy.Load();
        var report = new MdSpecReport("L1-baseline-colors", Layer, "L1 — M3 baseline color scheme", policy);

        Assert.True(policy.IsLayerEnabled(Layer), "L1_tokens is disabled in spec-snapshot/conformance-policy.json.");

        var application = Application.Current!;
        foreach (var (variant, dark) in new[] { (ThemeVariant.Light, false), (ThemeVariant.Dark, true) })
        {
            application.RequestedThemeVariant = variant;

            foreach (var (role, expected) in MdBaselineScheme.Roles(dark))
            {
                var key = $"Md.Sys.Color.{role}";
                if (!application.TryGetResource(key, variant, out var value) || value is not Color actual)
                {
                    report.Gap(key, $"{variant} does not define this role as a Color", highConfidence: true);
                    continue;
                }

                report.Check(
                    $"{variant}/{role}",
                    actual == FromArgb(expected),
                    $"{variant} resolves to {Hex(actual)}, the published baseline is {Hex(FromArgb(expected))}");
            }
        }

        report.Metric("baseline_roles", MdBaselineScheme.Roles(dark: false).Count);
        report.Complete();
    }

    [AvaloniaFact]
    public void The_Runtime_Generator_Reproduces_The_Baseline_For_The_Default_Options()
    {
        var policy = MdSpecPolicy.Load();
        var report = new MdSpecReport("L1-baseline-generator", Layer, "L1 — generator agrees with the baseline", policy);
        Assert.True(policy.IsLayerEnabled(Layer), "L1_tokens is disabled in spec-snapshot/conformance-policy.json.");

        var application = Application.Current!;
        var mismatches = new List<string>();
        var rolesCompared = 0;

        foreach (var dark in new[] { false, true })
        {
            var generated = MdThemeGenerator.Generate(new MdThemeOptions(), dark);
            foreach (var (role, expected) in MdBaselineScheme.Roles(dark))
            {
                rolesCompared++;
                if (!generated.TryGetValue(role, out var actual))
                {
                    mismatches.Add($"{role}: the generator does not produce it");
                    report.Check($"{(dark ? "dark" : "light")}/{role}", false, "the generator does not produce it");
                    continue;
                }

                if (actual != FromArgb(expected))
                {
                    mismatches.Add($"{role}: generated {Hex(actual)}, baseline {Hex(FromArgb(expected))}");
                    report.Check($"{(dark ? "dark" : "light")}/{role}", false,
                        $"generated {Hex(actual)}, baseline {Hex(FromArgb(expected))}");
                }
                else
                {
                    report.Pass($"{(dark ? "dark" : "light")}/{role}");
                }
            }
        }

        foreach (var mismatch in mismatches)
        {
            report.Gap("baseline-generator", mismatch, highConfidence: true);
        }

        Assert.True(mismatches.Count == 0,
            $"{mismatches.Count} of {rolesCompared} generated role(s) disagree with the published baseline:" +
            $"{Environment.NewLine}  " + string.Join(Environment.NewLine + "  ", mismatches));

        // The point of the check: the default options describe the baseline, so the runtime write and the
        // static theme must land on the same colour. Before this, a default application repainted every
        // accent from #6750A4 to #65558F.
        MdThemeManager.Apply(application, new MdThemeOptions(), dark: false);
        Assert.True(application.TryGetResource("Md.Sys.Color.Primary", ThemeVariant.Light, out var applied));
        Assert.Equal(FromArgb(MdBaselineScheme.Roles(dark: false)["Primary"]), Assert.IsType<Color>(applied));

        report.Metric("generated_roles_compared", rolesCompared);
        report.Complete();
    }

    [AvaloniaFact]
    public void Every_Colour_Role_Has_A_Brush_And_The_Fixed_Roles_Do_Not_Vary_By_Variant()
    {
        var policy = MdSpecPolicy.Load();
        var report = new MdSpecReport("L1-baseline-structure", Layer, "L1 — baseline role structure", policy);
        Assert.True(policy.IsLayerEnabled(Layer), "L1_tokens is disabled in spec-snapshot/conformance-policy.json.");

        var application = Application.Current!;
        var failures = new List<string>();

        foreach (var (variant, dark) in new[] { (ThemeVariant.Light, false), (ThemeVariant.Dark, true) })
        {
            application.RequestedThemeVariant = variant;

            foreach (var role in MdBaselineScheme.Roles(dark).Keys)
            {
                var brushKey = $"Md.Sys.Color.{role}.Brush";
                var present = application.TryGetResource(brushKey, variant, out var brush) && brush is IBrush;
                if (!present)
                {
                    failures.Add($"{brushKey} is missing; a control can only use the role if it ships a brush");
                }

                // One check per brush, not one per file: a layer that reports nothing to assert is an
                // unconditional pass, and the harness rejects that as evidence.
                report.Check($"{variant}/{brushKey}", present, $"{variant} has no brush for {role}");
            }
        }

        foreach (var role in FixedRoles)
        {
            var light = MdBaselineScheme.Roles(dark: false)[role];
            var darkValue = MdBaselineScheme.Roles(dark: true)[role];
            var stays = light == darkValue;
            if (!stays)
            {
                failures.Add($"{role} differs between light and dark ({light:x8} vs {darkValue:x8}); " +
                             "a fixed role is fixed across variants by definition");
            }

            report.Check($"fixed/{role}", stays, $"{role} changes between light and dark");
        }

        foreach (var failure in failures)
        {
            report.Gap("baseline-structure", failure, highConfidence: true);
        }

        Assert.True(failures.Count == 0,
            $"{failures.Count} structural problem(s) in the baseline scheme:" +
            $"{Environment.NewLine}  " + string.Join(Environment.NewLine + "  ", failures));

        report.Metric("baseline_brushes_checked", MdBaselineScheme.Roles(dark: false).Count * 2);
        report.Metric("fixed_roles", FixedRoles.Length);
        report.Complete();
    }

    /// <summary>
    /// The two variants must disagree exactly where Material says they do and nowhere else.
    /// </summary>
    /// <remarks>
    /// <c>spec-snapshot/tokens.json</c> keys a role by name, so it cannot hold two values for one name;
    /// that is why the baseline is not an oracle file and why this check lives here. The 49 roles split
    /// into 12 fixed, 2 that are black in both variants and 35 that Material gives two values to, so a
    /// role cannot drift from one group into another unnoticed.
    /// </remarks>
    [Fact]
    public void Only_The_Variant_Dependent_Roles_Differ_Between_Light_And_Dark()
    {
        var light = MdBaselineScheme.Roles(dark: false);
        var dark = MdBaselineScheme.Roles(dark: true);
        Assert.Equal(light.Keys.OrderBy(k => k, StringComparer.Ordinal), dark.Keys.OrderBy(k => k, StringComparer.Ordinal));

        var differing = light.Where(pair => pair.Value != dark[pair.Key]).Select(pair => pair.Key).ToList();

        // 49 roles split three ways: the 12 fixed ones never change, Scrim and Shadow are black in both
        // variants because a scrim over dark content and one over light content are the same black, and
        // the remaining 35 are the ones Material actually gives two values to.
        Assert.Equal(FixedRoles.Length + VariantInvariantRoles.Length - FixedRoles.Intersect(VariantInvariantRoles).Count(),
            FixedRoles.Concat(VariantInvariantRoles).Distinct(StringComparer.Ordinal).Count());
        foreach (var role in FixedRoles.Concat(VariantInvariantRoles))
        {
            Assert.DoesNotContain(role, differing);
        }

        Assert.Equal(light.Count - FixedRoles.Length - VariantInvariantRoles.Length, differing.Count);
        Assert.Equal(35, differing.Count);
        Assert.Equal(FixedRoles.Length, FixedRoles.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(12, FixedRoles.Length);
        Assert.Equal(49, light.Count);
    }

    private static Color FromArgb(uint argb) =>
        Color.FromArgb((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);

    private static string Hex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

}
