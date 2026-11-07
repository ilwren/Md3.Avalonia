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
/// (<c>MaterialTheme</c> + <c>ExtraTheme</c>, 58 merged dictionaries) and compares the resolved
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

        // A colour token that states both variants is compared twice, once per variant, and the two
        // checks are named apart: 'Primary' and 'Primary (dark)'. Everything else is compared once, in
        // the light variant, which is what the resource system resolves by default.
        foreach (var token in oracle.Tokens)
        {
            CompareToken(application, oracle, token, dark: false, report);

            if (token.HasVariantSplit)
            {
                CompareToken(application, oracle, token, dark: true, report);
            }
        }

        ReportThemeVariantParity(application, report);
        ReportReferenceTypeface(application, report);

        report.Complete();
    }

    /// <summary>
    /// Compares one token against the value the oracle expects in <paramref name="dark"/>.
    /// </summary>
    /// <remarks>
    /// The variant is part of the subject name when the oracle states one, because "Primary is wrong"
    /// and "Primary is wrong in dark mode only" are different bugs and the report has to be able to say
    /// which one happened.
    /// </remarks>
    private static void CompareToken(
        Application application, MdSpecOracle oracle, MdOracleToken token, bool dark, MdSpecReport report)
    {
        var subject = dark ? $"{token.Key} (dark)" : token.Key;
        var variant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        var expected = token.ValueFor(dark) ?? token.Value;
        var expectedDescription = Describe(expected, token.Kind);

        if (!application.TryGetResource(token.Key, variant, out var resolved) || resolved is null)
        {
            if (token.Required)
            {
                report.Gap(
                    subject,
                    $"{variant} does not define it; the oracle ({token.Source}) expects {expectedDescription}",
                    token.IsHighConfidence);
            }

            return;
        }

        var actual = Flatten(resolved, token.Kind);
        if (actual is null)
        {
            report.Gap(
                subject,
                $"resolved to {resolved.GetType().Name}, which cannot be compared against a '{token.Kind}' oracle entry",
                token.IsHighConfidence);
            return;
        }

        if (actual.Length != expected.Length)
        {
            report.Gap(
                subject,
                $"resolved to a {actual.Length}-component value but the oracle entry has " +
                $"{expected.Length}; the oracle 'type' and the declared resource type disagree",
                token.IsHighConfidence);
            return;
        }

        // Exact for colours: the oracle holds authored bytes, so a difference is a wrong value rather
        // than accumulated rounding.
        var tolerance = token.Kind == MdOracleKind.Color ? 0d : oracle.ToleranceFor(token);
        var worst = 0d;
        for (var i = 0; i < expected.Length; i++)
        {
            worst = Math.Max(worst, Math.Abs(expected[i] - actual[i]));
        }

        if (worst <= tolerance)
        {
            report.Pass(subject);
        }
        else
        {
            report.Gap(
                subject,
                $"resolved to {Describe(actual, token.Kind)}, the oracle ({token.Source}) expects " +
                $"{expectedDescription} (worst delta {worst:0.####} > {tolerance:0.####})",
                token.IsHighConfidence);
        }
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

        // Colours are compared as bytes, exactly, with no tolerance: the oracle holds authored values.
        if (kind == MdOracleKind.Color && resolved is Color color)
        {
            return new double[] { color.R, color.G, color.B, color.A };
        }

        if (kind == MdOracleKind.Color && resolved is ISolidColorBrush brush)
        {
            var brushColor = brush.Color;
            return new double[] { brushColor.R, brushColor.G, brushColor.B, brushColor.A };
        }

        return null;
    }

    private static string Describe(double[] values, MdOracleKind kind)
    {
        if (kind == MdOracleKind.Color)
        {
            return $"#{(int)values[0]:X2}{(int)values[1]:X2}{(int)values[2]:X2}";
        }

        return kind == MdOracleKind.Double
            ? values[0].ToString("0.####")
            : "[" + string.Join(", ", values.Select(v => v.ToString("0.####"))) + "]";
    }
}
