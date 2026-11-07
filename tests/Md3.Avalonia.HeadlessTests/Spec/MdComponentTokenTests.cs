using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Xunit;

namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>
/// Layer L1 — component tokens against the Material Web tables.
/// </summary>
/// <remarks>
/// <para>
/// <c>MdTokenConformanceTests</c> checks the tokens <c>spec-snapshot/tokens.json</c> already names, and
/// every one of those entries was transcribed by hand from the same pages. That is a real check of the
/// library against an external source, but it cannot catch a token that was transcribed <em>wrong</em>
/// into both the oracle and the theme: the two agree with each other and disagree with Material.
/// </para>
/// <para>
/// So this layer takes the other side of the comparison. <c>spec-snapshot/component-tokens.json</c> is a
/// frozen, machine-produced dump of the Material Web component token files - 307 literal values across
/// 55 components at the pinned commit - and <c>mapping.pairs</c> says which of this library's
/// <c>Md.Comp.*</c> keys corresponds to which Material Web token. Three of the 49 pairs were wrong when
/// this layer was first written, which is exactly the class of defect it exists to find.
/// </para>
/// </remarks>
public sealed class MdComponentTokenTests
{
    private const string Layer = "L1_tokens";

    [AvaloniaFact]
    public void Component_Tokens_Match_The_Material_Web_Tables()
    {
        var policy = MdSpecPolicy.Load();
        var report = new MdSpecReport("L1-component-tokens", Layer, "L1 — component tokens vs Material Web", policy);

        Assert.True(policy.IsLayerEnabled(Layer), "L1_tokens is disabled in spec-snapshot/conformance-policy.json.");

        var tables = ComponentTables.Load();
        Assert.NotEmpty(tables.Pairs);

        var application = Application.Current!;
        application.RequestedThemeVariant = ThemeVariant.Light;

        foreach (var pair in tables.Pairs)
        {
            Compare(application, tables, pair, report);
        }

        CompareFullShapes(application, tables, report);
        ReportCoverage(application, tables, report);

        report.Complete();
    }

    /// <summary>
    /// Compares one library key against the Material Web token the mapping pairs it with.
    /// </summary>
    private static void Compare(Application application, ComponentTables tables, ComponentPair pair, MdSpecReport report)
    {
        var expected = tables.Value(pair.Component, pair.Token);
        if (expected is null)
        {
            // The mapping names a token the frozen tables do not carry. That is a defect in the
            // mapping, not in the library, and it must be loud: a silently skipped pair is a check
            // that pretends to exist.
            report.Gap(
                pair.Library,
                $"the mapping pairs this with {pair.Component}.{pair.Token}, which the frozen Material Web " +
                "tables do not contain; the mapping or the snapshot is wrong");
            return;
        }

        if (!TryResolve(application, pair.Library, out var actual))
        {
            report.Gap(pair.Library, "the theme does not declare this key at all");
            return;
        }

        var wanted = expected.Value;
        var subject = $"{pair.Library} ({pair.Component}.{pair.Token})";

        if (wanted.Kind == ExpectedKind.Scalar)
        {
            if (actual.Kind == ResolvedKind.Scalar)
            {
                report.CheckScalar(subject, wanted.Number!.Value, actual.Number!.Value, tolerance: 0d);
                return;
            }

            // A single number in Material, a quad in the theme: corner radii and border widths are both
            // authored that way. Uniform is the equivalent value; anything else is a real difference.
            var quad = actual.Corners!;
            var uniform = quad.All(step => Math.Abs(step - quad[0]) < 0.0001);
            report.Check(
                subject,
                uniform && Math.Abs(quad[0] - wanted.Number!.Value) < 0.0001,
                $"Material states a single value ({wanted.Number:0.####}); the theme declares this key per " +
                $"edge as [{string.Join(", ", quad)}], which is " +
                (uniform ? "uniform but a different value" : "not even uniform"));
            return;
        }

        var expectedCorners = wanted.Corners!;
        var actualCorners = actual.Corners;
        if (actualCorners is null)
        {
            report.Gap(subject, $"Material states corner radius [{string.Join(", ", expectedCorners)}] but the " +
                                "theme declares this key as a single value");
            return;
        }

        report.Check(
            subject,
            expectedCorners.Length == actualCorners.Length &&
            expectedCorners.Zip(actualCorners).All(step => Math.Abs(step.First - step.Second) < 0.0001),
            $"expected corner radius [{string.Join(", ", expectedCorners)}], observed " +
            $"[{string.Join(", ", actualCorners)}] (worst delta " +
            $"{expectedCorners.Zip(actualCorners, (e, a) => Math.Abs(e - a)).DefaultIfEmpty(0).Max():0.####})");
    }

    /// <summary>
    /// The shapes Material calls <c>corner-full</c>, checked as shape rather than as a number.
    /// </summary>
    /// <remarks>
    /// "Full" is not a measurement: Material states 9999px, this theme states 999 as
    /// <c>Md.Sys.Shape.Corner.Full</c>, and a fixed-size control states half its own box (a 6dp dot
    /// rounds at 3). Comparing numbers here produces false failures in both directions, so the check is
    /// the invariant that actually holds: a full-shape token must be at least as round as the box it
    /// rounds. A radius under half the box leaves square corners on a control Material draws as a circle.
    /// </remarks>
    private static void CompareFullShapes(Application application, ComponentTables tables, MdSpecReport report)
    {
        // Material spells this step 'corner-full' and states 9999px; this theme spells it
        // Md.Sys.Shape.Corner.Full and states 999. Both mean "as round as the box", so comparing the two
        // numbers would be a false failure - and so would demanding the 999.
        //
        // This check exists because the obvious reading ("a token Material calls full must hold 999")
        // produced three findings on the first run that were not defects: the dot badge is 6dp round
        // with a radius of 3, the labelled badge 16dp with 8, the checkbox state layer 40dp with 20.
        // Each is exactly half its box, which IS full. What the specification constrains is the shape,
        // not the number, so the check is that a full-shape token's radius covers half the box it rounds.
        // A radius smaller than that leaves square corners on a control Material draws as a circle.
        // Every token Material calls corner-full, paired with the box it rounds. The pairing is what
        // makes the check meaningful: "999" is not the invariant, "at least half the box" is.
        var roundTokens = new (string Key, string Size)[]
        {
            ("Md.Comp.Badge.Small.Shape", "Md.Comp.Badge.Small.Size"),
            ("Md.Comp.Badge.Large.Shape", "Md.Comp.Badge.Large.Height"),
            ("Md.Comp.CheckBox.StateLayer.Shape", "Md.Comp.CheckBox.StateLayer.Size"),
            ("Md.Comp.Button.Small.Shape.Round", "Md.Comp.Button.Small.Container.Height"),
            ("Md.Comp.IconButton.Small.RoundShape", "Md.Comp.IconButton.Small.Height")
        };

        foreach (var (key, sizeKey) in roundTokens)
        {
            if (!TryResolve(application, key, out var shape) || shape.Kind != ResolvedKind.Corner ||
                !TryResolve(application, sizeKey, out var size) || size.Kind != ResolvedKind.Scalar)
            {
                report.Gap(key, $"expected a CornerRadius shape and a scalar size ({sizeKey})");
                continue;
            }

            var corners = shape.Corners!;
            var box = size.Number!.Value;
            report.Check(
                $"{key} (= Full)",
                corners.All(step => step * 2 >= box - 0.0001),
                $"Material draws this as corner-full. The box is {box:0.####} and the radius is " +
                $"[{string.Join(", ", corners)}]; a radius under half the box leaves square corners.");
        }

        // The rest of the full-shape tokens have no single box to compare against (a round button's
        // radius tracks whichever size variant is active), so the stateable invariant is the weaker one:
        // the radius is symmetric, rather than a squashed quad pretending to be a circle. The pair list
        // comes from the snapshot so that adding a shape to the mapping is a one-line change.
        foreach (var pair in tables.FullShapePairs)
        {
            if (!TryResolve(application, pair.Library, out var resolved) || resolved.Kind != ResolvedKind.Corner)
            {
                report.Gap(pair.Library, "expected a CornerRadius resource");
                continue;
            }

            var corners = resolved.Corners!;
            report.Check(
                $"{pair.Library} (= {pair.Expect})",
                corners.Distinct().Count() == 1,
                $"Material states {pair.Expect}, so all four corners must match; the token resolves to " +
                $"[{string.Join(", ", corners)}]");
        }
    }

    /// <summary>
    /// How much of the library's component-token surface this layer can speak about at all.
    /// </summary>
    /// <remarks>
    /// Reported as a measurement, never as a pass: the unmapped keys are a queue, and the number is what
    /// stops the layer from looking like it covers the whole component surface when it covers 49 keys of
    /// 269.
    /// </remarks>
    private static void ReportCoverage(Application application, ComponentTables tables, MdSpecReport report)
    {
        var declared = DeclaredComponentKeys();
        var covered = tables.Pairs.Select(pair => pair.Library)
            .Concat(tables.FullShapePairs.Select(pair => pair.Library))
            .ToHashSet(StringComparer.Ordinal);

        // The three circular tokens are compared as shape-against-size by CompareFullShapes rather than
        // by a pair entry, so they are covered even though no pair names them.
        covered.Add("Md.Comp.Badge.Small.Shape");
        covered.Add("Md.Comp.Badge.Large.Shape");
        covered.Add("Md.Comp.CheckBox.StateLayer.Shape");

        var unmapped = declared.Where(key => !covered.Contains(key))
            .Concat(tables.ExcludedByDesign.Keys.Where(key => !covered.Contains(key)))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToList();

        report.Inventory(
            "component_tokens_without_a_specification_counterpart",
            unmapped,
            "declared Md.Comp.* keys this layer cannot check, because the specification states no value " +
            "for them or because no mapping has been written yet. " +
            $"{covered.Count} key(s) are checked, and the excluded_by_design list in " +
            "spec-snapshot/component-tokens.json records the ones that are deliberately out of reach.");
    }

    /// <summary>
    /// Every <c>Md.Comp.*</c> key the shipped theme packages declare.
    /// </summary>
    /// <remarks>
    /// Read from the source rather than from the merged application, because the question is what the
    /// library declares, and a key that fails to merge is a different (and separately measured) defect.
    /// </remarks>
    private static IReadOnlyList<string> DeclaredComponentKeys()
    {
        var root = MdSpecPaths.RequireRoot();
        var keys = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.axaml", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (System.Text.RegularExpressions.Match match in
                     System.Text.RegularExpressions.Regex.Matches(
                         File.ReadAllText(file), "x:Key=\"(Md\\.Comp\\.[^\"]+)\""))
            {
                keys.Add(match.Groups[1].Value);
            }
        }

        return keys.ToList();
    }

    private static bool TryResolve(Application application, string key, out Resolved resolved)
    {
        resolved = default;
        if (!application.TryGetResource(key, ThemeVariant.Light, out var value))
        {
            return false;
        }

        switch (value)
        {
            case double number:
                resolved = new Resolved(ResolvedKind.Scalar, number, null);
                return true;
            case int number:
                resolved = new Resolved(ResolvedKind.Scalar, number, null);
                return true;
            case Thickness thickness:
                // A border width is authored as a Thickness here (one value per edge) where Material
                // states a single number. A uniform quad compares equal to that number; a quad whose
                // edges differ does not, and the finding says so.
                resolved = new Resolved(
                    ResolvedKind.Edges,
                    null,
                    new[] { thickness.Left, thickness.Top, thickness.Right, thickness.Bottom });
                return true;
            case CornerRadius corner:
                resolved = new Resolved(
                    ResolvedKind.Corner,
                    null,
                    new[] { corner.TopLeft, corner.TopRight, corner.BottomRight, corner.BottomLeft });
                return true;
            default:
                return false;
        }
    }

    private enum ResolvedKind
    {
        Scalar,
        Corner,
        Edges
    }

    private readonly record struct Resolved(ResolvedKind Kind, double? Number, double[]? Corners);

    internal enum ExpectedKind
    {
        Scalar,
        Corner
    }

    /// <summary>One library key and the Material Web token it is checked against.</summary>
    internal sealed record ComponentPair(string Library, string Component, string Token);

    internal readonly record struct Expected(ExpectedKind Kind, double? Number, double[]? Corners);

    /// <summary>
    /// The frozen Material Web component tables, plus the mapping that makes them comparable.
    /// </summary>
    internal sealed class ComponentTables
    {
        private readonly System.Text.Json.JsonDocument _document;
        private readonly Dictionary<(string Component, string Token), Expected> _expected = new();

        private ComponentTables(
            System.Text.Json.JsonDocument document,
            IReadOnlyList<ComponentPair> pairs,
            IReadOnlyList<FullShapePair> fullShapePairs,
            IReadOnlyDictionary<string, string> excludedByDesign)
        {
            _document = document;
            Pairs = pairs;
            FullShapePairs = fullShapePairs;
            ExcludedByDesign = excludedByDesign;
        }

        public IReadOnlyList<ComponentPair> Pairs { get; }

        public IReadOnlyList<FullShapePair> FullShapePairs { get; }

        public IReadOnlyDictionary<string, string> ExcludedByDesign { get; }

        public static ComponentTables Load()
        {
            var path = Path.Combine(MdSpecPaths.SpecSnapshotDirectory, "component-tokens.json");
            Assert.True(File.Exists(path), $"{path} is missing; the component-token layer has no oracle.");

            var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(path));
            var root = document.RootElement;

            var pairs = root.GetProperty("mapping").GetProperty("pairs").EnumerateArray()
                .Select(entry => new ComponentPair(
                    entry.GetProperty("library").GetString()!,
                    entry.GetProperty("component").GetString()!,
                    entry.GetProperty("token").GetString()!))
                .ToList();

            var fullShapePairs = root.GetProperty("mapping").GetProperty("full_shape_pairs").EnumerateArray()
                .Select(entry => new FullShapePair(
                    entry.GetProperty("library").GetString()!,
                    entry.GetProperty("expect").GetString()!))
                .ToList();

            var excluded = root.GetProperty("mapping").GetProperty("excluded_by_design").EnumerateObject()
                .ToDictionary(entry => entry.Name, entry => entry.Value.GetString() ?? string.Empty, StringComparer.Ordinal);

            return new ComponentTables(document, pairs, fullShapePairs, excluded);
        }

        public Expected? Value(string component, string token)
        {
            if (_expected.TryGetValue((component, token), out var cached))
            {
                return cached;
            }

            var root = _document.RootElement.GetProperty("components");
            if (!root.TryGetProperty(component, out var componentElement) ||
                !componentElement.GetProperty("tokens").TryGetProperty(token, out var tokenElement))
            {
                return null;
            }

            var value = tokenElement.GetProperty("value");

            // A shape token is stored as the resolved number (12) plus the Material Web step it came from
            // ('corner-medium'), and the theme resolves it as a CornerRadius, not a double. The quad is
            // the value repeated four times, except for the composite steps ('corner-large-top' and
            // friends), whose value is already the four-corner array.
            var isShape = tokenElement.TryGetProperty("shape", out _);
            var expected = value.ValueKind == System.Text.Json.JsonValueKind.Array
                ? new Expected(ExpectedKind.Corner, null, value.EnumerateArray().Select(item => item.GetDouble()).ToArray())
                : isShape
                    ? new Expected(
                        ExpectedKind.Corner,
                        null,
                        Enumerable.Repeat(value.GetDouble(), 4).ToArray())
                    : new Expected(ExpectedKind.Scalar, value.GetDouble(), null);

            _expected[(component, token)] = expected;
            return expected;
        }
    }

    internal sealed record FullShapePair(string Library, string Expect);
}
