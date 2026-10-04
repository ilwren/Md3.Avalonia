using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Xunit;

namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>
/// Layer L1 — token conformance.
/// </summary>
/// <remarks>
/// <para>
/// Resolves every token named by <c>spec-snapshot/tokens.json</c> through the real theme merge
/// (<c>MaterialTheme</c> + <c>EcosystemTheme</c>, 58 merged dictionaries) and compares the resolved
/// value against the externally sourced oracle.
/// </para>
/// <para>
/// This complements, and does not duplicate, <c>scripts/lint-design-tokens.py</c>. The lint parses
/// the XAML text and therefore runs in a second without an SDK; this test exercises what the
/// resource system actually hands a control at runtime, which is the only thing that can catch a
/// token shadowed by merge order or stranded in the wrong theme dictionary.
/// </para>
/// </remarks>
public sealed class MdTokenConformanceTests
{
    private const string Layer = "L1_tokens";

    [AvaloniaFact]
    public void Resolved_Tokens_Match_The_Frozen_Specification_Oracle()
    {
        var policy = MdSpecPolicy.Load();
        var oracle = MdSpecOracle.Load();
        var report = new MdSpecReport("L1-tokens", Layer, "L1 — token conformance", policy);

        Assert.True(policy.IsLayerEnabled(Layer), "L1_tokens is disabled in spec-snapshot/conformance-policy.json.");
        Assert.NotEmpty(oracle.Tokens);

        var application = Application.Current!;
        application.RequestedThemeVariant = ThemeVariant.Light;

        foreach (var token in oracle.Tokens)
        {
            if (!application.TryGetResource(token.Key, ThemeVariant.Light, out var resolved) || resolved is null)
            {
                if (token.Required)
                {
                    report.Gap(
                        token.Key,
                        $"not defined by the merged theme; the oracle ({token.Source}) expects {token.Describe()}",
                        token.IsHighConfidence);
                }

                continue;
            }

            var actual = Flatten(resolved, token.Kind);
            if (actual is null)
            {
                report.Gap(
                    token.Key,
                    $"resolved to {resolved.GetType().Name}, which cannot be compared against a '{token.Kind}' oracle entry",
                    token.IsHighConfidence);
                continue;
            }

            if (actual.Length != token.Value.Length)
            {
                report.Gap(
                    token.Key,
                    $"resolved to a {actual.Length}-component value but the oracle entry has " +
                    $"{token.Value.Length}; the oracle 'type' and the declared resource type disagree",
                    token.IsHighConfidence);
                continue;
            }

            var tolerance = oracle.ToleranceFor(token);
            var worst = 0d;
            for (var i = 0; i < token.Value.Length; i++)
            {
                worst = Math.Max(worst, Math.Abs(token.Value[i] - actual[i]));
            }

            if (worst <= tolerance)
            {
                report.Pass(token.Key);
            }
            else
            {
                report.Gap(
                    token.Key,
                    $"resolved to {Describe(actual, token.Kind)}, the oracle ({token.Source}) expects " +
                    $"{token.Describe()} (worst delta {worst:0.####} > {tolerance:0.####})",
                    token.IsHighConfidence);
            }
        }

        ReportThemeVariantParity(application, report);
        ReportReferenceTypeface(application, report);

        report.Complete();
    }

    /// <summary>
    /// Every colour role the oracle cares about must exist in BOTH theme dictionaries. A role that
    /// resolves only in Light is a dark-mode defect that no screenshot of the light theme can reveal.
    /// </summary>
    private static void ReportThemeVariantParity(Application application, MdSpecReport report)
    {
        string[] roles =
        [
            "Primary", "OnPrimary", "PrimaryContainer", "OnPrimaryContainer",
            "Secondary", "OnSecondary", "SecondaryContainer", "OnSecondaryContainer",
            "Tertiary", "OnTertiary", "TertiaryContainer", "OnTertiaryContainer",
            "Error", "OnError", "ErrorContainer", "OnErrorContainer",
            "Surface", "OnSurface", "SurfaceVariant", "OnSurfaceVariant",
            "SurfaceContainerLowest", "SurfaceContainerLow", "SurfaceContainer",
            "SurfaceContainerHigh", "SurfaceContainerHighest",
            "Outline", "OutlineVariant", "InverseSurface", "InverseOnSurface", "InversePrimary",
            "Scrim", "Shadow"
        ];

        foreach (var role in roles)
        {
            var key = "Md.Sys.Color." + role;
            var hasLight = application.TryGetResource(key, ThemeVariant.Light, out var light) && light is not null;
            var hasDark = application.TryGetResource(key, ThemeVariant.Dark, out var dark) && dark is not null;

            if (!hasLight || !hasDark)
            {
                report.Gap(
                    key,
                    $"colour role missing from a theme variant (light: {hasLight}, dark: {hasDark}); " +
                    "M3 defines the full role set for both variants");
                continue;
            }

            if (light is Color lightColor && dark is Color darkColor && lightColor == darkColor &&
                role is not ("Shadow" or "Scrim"))
            {
                report.Gap(
                    key,
                    $"light and dark resolve to the identical colour {lightColor}; the variant is probably not wired up",
                    highConfidence: false);
                continue;
            }

            report.Pass(key);
        }
    }

    /// <summary>
    /// M3's reference typeface is Roboto / Roboto Flex. Shipping a different family is a legitimate
    /// engineering choice, but it is a divergence and must be visible rather than implied.
    /// </summary>
    private static void ReportReferenceTypeface(Application application, MdSpecReport report)
    {
        foreach (var key in new[] { "Md.Sys.Typeface.Brand", "Md.Sys.Typeface.Plain" })
        {
            if (!application.TryGetResource(key, ThemeVariant.Light, out var value) || value is not FontFamily family)
            {
                report.Gap(key, "reference typeface token is not defined as a FontFamily", highConfidence: false);
                continue;
            }

            var name = family.Name;
            if (name.Contains("Roboto", StringComparison.OrdinalIgnoreCase))
            {
                report.Pass(key);
            }
            else
            {
                report.Note(
                    key,
                    $"resolves to '{name}'. M3's reference typeface is Roboto/Roboto Flex; this library " +
                    "intentionally falls back to a platform font instead of embedding a 1-2 MB text face. " +
                    "Recorded as a divergence so the parity tables cannot imply typographic identity.");
            }
        }
    }

    private static double[]? Flatten(object resolved, MdOracleKind kind)
    {
        if (kind == MdOracleKind.Double)
        {
            return resolved switch
            {
                double value => new[] { value },
                int value => new double[] { value },
                _ => null
            };
        }

        if (kind == MdOracleKind.Corner && resolved is CornerRadius corner)
        {
            return new[] { corner.TopLeft, corner.TopRight, corner.BottomRight, corner.BottomLeft };
        }

        if (kind == MdOracleKind.Thickness && resolved is Thickness thickness)
        {
            return new[] { thickness.Left, thickness.Top, thickness.Right, thickness.Bottom };
        }

        return null;
    }

    private static string Describe(double[] values, MdOracleKind kind) => kind == MdOracleKind.Double
        ? values[0].ToString("0.####")
        : "[" + string.Join(", ", values.Select(v => v.ToString("0.####"))) + "]";
}
