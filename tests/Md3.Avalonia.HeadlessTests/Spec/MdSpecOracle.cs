using System.Globalization;
using System.Text.Json;

namespace Md3.Avalonia.HeadlessTests.Spec;

internal enum MdOracleKind
{
    Double,
    Corner,
    Thickness
}

/// <summary>One expected value, traced back to a pinned external source.</summary>
/// <param name="Key">Avalonia resource key the library is expected to expose.</param>
/// <param name="Kind">How <paramref name="Value"/> is interpreted.</param>
/// <param name="Value">Length 1 for <see cref="MdOracleKind.Double"/>, otherwise length 4.</param>
/// <param name="Confidence"><c>high</c> entries can fail a build; <c>medium</c> entries never do.</param>
/// <param name="Required">When false, absence of the key is not reported.</param>
/// <param name="Source">Key into <c>spec-snapshot/manifest.json</c>.</param>
internal sealed record MdOracleToken(
    string Key,
    MdOracleKind Kind,
    double[] Value,
    string Confidence,
    bool Required,
    string Source)
{
    public bool IsHighConfidence => string.Equals(Confidence, "high", StringComparison.OrdinalIgnoreCase);

    public string Describe() => Kind == MdOracleKind.Double
        ? Value[0].ToString("0.####", CultureInfo.InvariantCulture)
        : "[" + string.Join(", ", Value.Select(v => v.ToString("0.####", CultureInfo.InvariantCulture))) + "]";
}

/// <summary>
/// Reads <c>spec-snapshot/tokens.json</c>, the externally sourced oracle for layers L1/L2/L5.
/// </summary>
/// <remarks>
/// This file is deliberately NOT generated from the library's own resource dictionaries. A test
/// that compares the implementation against itself always passes and proves nothing; that defect
/// is what invalidated the retracted "100% compliant" audit.
/// </remarks>
internal sealed class MdSpecOracle
{
    private static readonly Lazy<MdSpecOracle> Instance = new(LoadCore);

    private MdSpecOracle(
        IReadOnlyList<MdOracleToken> tokens,
        double toleranceDip,
        double toleranceCorner,
        double toleranceOpacity,
        double toleranceFontSize,
        double touchTargetMinDip,
        double motionSettlingTolerance,
        double motionIntegratorTolerance)
    {
        Tokens = tokens;
        ByKey = tokens.ToDictionary(t => t.Key, StringComparer.Ordinal);
        ToleranceDip = toleranceDip;
        ToleranceCorner = toleranceCorner;
        ToleranceOpacity = toleranceOpacity;
        ToleranceFontSize = toleranceFontSize;
        TouchTargetMinDip = touchTargetMinDip;
        MotionSettlingTolerance = motionSettlingTolerance;
        MotionIntegratorTolerance = motionIntegratorTolerance;
    }

    public static MdSpecOracle Load() => Instance.Value;

    public IReadOnlyList<MdOracleToken> Tokens { get; }

    public IReadOnlyDictionary<string, MdOracleToken> ByKey { get; }

    public double ToleranceDip { get; }

    public double ToleranceCorner { get; }

    public double ToleranceOpacity { get; }

    public double ToleranceFontSize { get; }

    public double TouchTargetMinDip { get; }

    public double MotionSettlingTolerance { get; }

    public double MotionIntegratorTolerance { get; }

    /// <summary>Looks up an expected scalar, or null when the oracle does not constrain it.</summary>
    public double? Scalar(string key) =>
        ByKey.TryGetValue(key, out var token) && token.Kind == MdOracleKind.Double
            ? (double?)token.Value[0]
            : null;

    /// <summary>Looks up an expected 4-component value, or null when the oracle does not constrain it.</summary>
    public double[]? Quad(string key) =>
        ByKey.TryGetValue(key, out var token) && token.Kind != MdOracleKind.Double ? token.Value : null;

    public double ToleranceFor(MdOracleToken token)
    {
        if (token.Kind == MdOracleKind.Corner)
        {
            return ToleranceCorner;
        }

        if (token.Kind == MdOracleKind.Double)
        {
            if (token.Key.Contains("Opacity", StringComparison.Ordinal))
            {
                return ToleranceOpacity;
            }

            if (token.Key.Contains("FontSize", StringComparison.Ordinal) ||
                token.Key.Contains("TypeScale", StringComparison.Ordinal))
            {
                return ToleranceFontSize;
            }
        }

        return ToleranceDip;
    }

    private static MdSpecOracle LoadCore()
    {
        var path = Path.Combine(MdSpecPaths.SpecSnapshotDirectory, "tokens.json");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                "spec-snapshot/tokens.json is required by the conformance harness.", path);
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;

        var tokens = new List<MdOracleToken>();
        if (root.TryGetProperty("tokens", out var tokensElement) && tokensElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in tokensElement.EnumerateArray())
            {
                var key = item.GetProperty("key").GetString();
                var typeName = item.GetProperty("type").GetString();
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(typeName))
                {
                    continue;
                }

                var kind = typeName switch
                {
                    "double" => MdOracleKind.Double,
                    "corner" => MdOracleKind.Corner,
                    "thickness" => MdOracleKind.Thickness,
                    _ => (MdOracleKind?)null
                };

                if (kind is null)
                {
                    continue;
                }

                var valueElement = item.GetProperty("value");
                double[] value;
                if (valueElement.ValueKind == JsonValueKind.Array)
                {
                    // A 'double' entry is a scalar by definition; a four-component literal there
                    // would silently desynchronise the comparison loop, so reject it at load time.
                    if (kind == MdOracleKind.Double)
                    {
                        continue;
                    }

                    value = valueElement.EnumerateArray().Select(v => v.GetDouble()).ToArray();
                    if (value.Length != 4)
                    {
                        continue;
                    }
                }
                else
                {
                    var scalar = valueElement.GetDouble();
                    value = kind == MdOracleKind.Double
                        ? new[] { scalar }
                        : new[] { scalar, scalar, scalar, scalar };
                }

                tokens.Add(new MdOracleToken(
                    key,
                    kind.Value,
                    value,
                    item.TryGetProperty("confidence", out var confidence) ? confidence.GetString() ?? "medium" : "medium",
                    !item.TryGetProperty("required", out var required) || required.ValueKind != JsonValueKind.False,
                    item.TryGetProperty("source", out var source) ? source.GetString() ?? "unknown" : "unknown"));
            }
        }

        var tolerances = root.TryGetProperty("tolerances", out var t) ? t : default;
        var geometry = root.TryGetProperty("geometry", out var g) ? g : default;
        var motion = root.TryGetProperty("motion", out var m) ? m : default;

        return new MdSpecOracle(
            tokens,
            ReadDouble(tolerances, "dip", 1.0),
            ReadDouble(tolerances, "corner", 0.5),
            ReadDouble(tolerances, "opacity", 0.005),
            ReadDouble(tolerances, "fontSize", 0.5),
            ReadDouble(geometry, "touchTargetMinDip", 48),
            ReadDouble(motion, "settlingTolerance", 0.0015),
            ReadDouble(motion, "integratorTolerance", 0.01));
    }

    private static double ReadDouble(JsonElement element, string name, double fallback) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value)
            ? value.GetDouble()
            : fallback;
}
