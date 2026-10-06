using System.Text.Json;

namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>A deliberate, documented divergence from the frozen specification oracle.</summary>
internal sealed record MdSpecWaiver(string Id, string? Layer, IReadOnlyList<string> Keys, string Reason);

/// <summary>
/// Reads <c>spec-snapshot/conformance-policy.json</c>.
/// </summary>
/// <remarks>
/// The policy implements a one-directional ratchet. In <c>bootstrap</c> mode every divergence is
/// recorded in <c>artifacts/spec/</c> but nothing fails, which is what lets this harness be merged
/// before the backlog it discovers has been burnt down. In <c>enforce</c> mode, high-confidence
/// divergences fail the build unless an explicit waiver covers them.
/// </remarks>
internal sealed class MdSpecPolicy
{
    private static readonly Lazy<MdSpecPolicy> Instance = new(LoadCore);

    private readonly IReadOnlyList<MdSpecWaiver> _waivers;
    private readonly IReadOnlyDictionary<string, bool> _layers;

    private MdSpecPolicy(
        string mode,
        IReadOnlyDictionary<string, bool> layers,
        IReadOnlyList<MdSpecWaiver> waivers,
        bool autoAcceptMissingBaseline,
        string visualBaselineOs,
        int visualPerChannelTolerance,
        double visualMaxChangedPixelRatio)
    {
        Mode = mode;
        _layers = layers;
        _waivers = waivers;
        AutoAcceptMissingBaseline = autoAcceptMissingBaseline;
        VisualBaselineOs = visualBaselineOs;
        VisualPerChannelTolerance = visualPerChannelTolerance;
        VisualMaxChangedPixelRatio = visualMaxChangedPixelRatio;
    }

    public static MdSpecPolicy Load() => Instance.Value;

    public string Mode { get; }

    /// <summary>When false, every finding is recorded but none of them fail the test run.</summary>
    public bool IsEnforcing => string.Equals(Mode, "enforce", StringComparison.OrdinalIgnoreCase);

    public bool AutoAcceptMissingBaseline { get; }

    public string VisualBaselineOs { get; }

    public int VisualPerChannelTolerance { get; }

    public double VisualMaxChangedPixelRatio { get; }

    public bool IsLayerEnabled(string layer) => !_layers.TryGetValue(layer, out var enabled) || enabled;

    /// <summary>Returns the waiver reason covering <paramref name="key"/>, or null.</summary>
    public string? FindWaiver(string layer, string key)
    {
        foreach (var waiver in _waivers)
        {
            if (waiver.Layer is not null && !string.Equals(waiver.Layer, layer, StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var candidate in waiver.Keys)
            {
                if (string.Equals(candidate, key, StringComparison.Ordinal))
                {
                    return waiver.Id + ": " + waiver.Reason;
                }
            }
        }

        return null;
    }

    private static MdSpecPolicy LoadCore()
    {
        var path = Path.Combine(MdSpecPaths.SpecSnapshotDirectory, "conformance-policy.json");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                "spec-snapshot/conformance-policy.json is required by the conformance harness.", path);
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;

        var mode = root.TryGetProperty("mode", out var modeElement)
            ? modeElement.GetString() ?? "bootstrap"
            : "bootstrap";

        var layers = new Dictionary<string, bool>(StringComparer.Ordinal);
        var autoAccept = true;
        if (root.TryGetProperty("layers", out var layersElement) && layersElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var layer in layersElement.EnumerateObject())
            {
                if (layer.Value.TryGetProperty("enabled", out var enabled))
                {
                    layers[layer.Name] = enabled.ValueKind != JsonValueKind.False;
                }

                if (layer.Name == "L3_visual" && layer.Value.TryGetProperty("autoAcceptMissingBaseline", out var auto))
                {
                    autoAccept = auto.ValueKind != JsonValueKind.False;
                }
            }
        }

        var baselineOs = "Linux";
        var perChannel = 12;
        var maxRatio = 0.004;
        if (root.TryGetProperty("visual", out var visual) && visual.ValueKind == JsonValueKind.Object)
        {
            if (visual.TryGetProperty("baselineOs", out var os))
            {
                baselineOs = os.GetString() ?? baselineOs;
            }

            if (visual.TryGetProperty("perChannelTolerance", out var tolerance))
            {
                perChannel = tolerance.GetInt32();
            }

            if (visual.TryGetProperty("maxChangedPixelRatio", out var ratio))
            {
                maxRatio = ratio.GetDouble();
            }
        }

        var waivers = new List<MdSpecWaiver>();
        if (root.TryGetProperty("waivers", out var waiverElement) && waiverElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in waiverElement.EnumerateArray())
            {
                var keys = new List<string>();
                if (item.TryGetProperty("keys", out var keysElement) && keysElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var key in keysElement.EnumerateArray())
                    {
                        var text = key.GetString();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            keys.Add(text);
                        }
                    }
                }

                waivers.Add(new MdSpecWaiver(
                    item.TryGetProperty("id", out var id) ? id.GetString() ?? "waiver" : "waiver",
                    item.TryGetProperty("layer", out var layer) ? layer.GetString() : null,
                    keys,
                    item.TryGetProperty("reason", out var reason) ? reason.GetString() ?? string.Empty : string.Empty));
            }
        }

        return new MdSpecPolicy(mode, layers, waivers, autoAccept, baselineOs, perChannel, maxRatio);
    }
}
