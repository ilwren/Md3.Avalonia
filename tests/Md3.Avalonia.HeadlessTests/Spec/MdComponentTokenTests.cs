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
            if (actual.Kind != ResolvedKind.Scalar)
            {
                report.Gap(subject, $"Material states a single value ({wanted.Number:0.####}) but the theme " +
                                    "declares this key as a CornerRadius");
                return;
            }

            report.CheckScalar(subject, wanted.Number!.Value, actual.Number!.Value, tolerance: 0d);
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
    /// The circular shapes: Material states <c>corner-full</c>, and so must the theme.
    /// </summary>
    /// <remarks>
    /// These are separated from the numeric pairs because "full" is not a measurement. Both vocabularies
    /// spell it as the full corner step, so the check is that the library uses that step rather than a
    /// hard-coded number that happens to look round at today's size. Three tokens in the library used to
    /// be bare numbers (3, 8, 20 - the dot badge, the labelled badge and the checkbox state layer) and
    /// two of those numbers were smaller than the control they rounded, so the control was not a circle.
    /// </remarks>
    private static void CompareFullShapes(Application application, ComponentTables tables, MdSpecReport report)
    {
        // Material spells this step 'corner-full' and states 9999px; this theme spells it
        // Md.Sys.Shape.Corner.Full and states 999. Both mean "as round as the box", so comparing the two
        // numbers would be a false failure. What is actually checkable is that each circular token is
        // wired to the library's own full step rather than to a literal that happens to look round at
        // today's size - which is how three of these were wrong (3, 8 and 20).
        Assert.True(
            application.TryGetResource("Md.Sys.Shape.Corner.Full", ThemeVariant.Light, out var fullValue) &&
            fullValue is CornerRadius fullCorner,
            "Md.Sys.Shape.Corner.Full is not declared as a CornerRadius; the full-shape checks have no reference.");

        foreach (var pair in tables.FullShapePairs)
        {
            if (!TryResolve(application, pair.Library, out var actual) || actual.Kind != ResolvedKind.Corner)
            {
                report.Gap(pair.Library, "expected a CornerRadius resource");
                continue;
            }

            var corners = actual.Corners!;
            var allFull = corners.All(step => Math.Abs(step - fullCorner.TopLeft) < 0.0001);
            report.Check(
                $"{pair.Library} (= {pair.Expect})",
                allFull,
                $"Material states {pair.Expect} for this shape, which is Md.Sys.Shape.Corner.Full " +
                $"({fullCorner.TopLeft:0.####}); the token resolves to " +
                $"[{string.Join(", ", corners)}] instead");
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
        Corner
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
            var expected = value.ValueKind == System.Text.Json.JsonValueKind.Array
                ? new Expected(ExpectedKind.Corner, null, value.EnumerateArray().Select(item => item.GetDouble()).ToArray())
                : new Expected(ExpectedKind.Scalar, value.GetDouble(), null);

            _expected[(component, token)] = expected;
            return expected;
        }
    }

    internal sealed record FullShapePair(string Library, string Expect);
}
