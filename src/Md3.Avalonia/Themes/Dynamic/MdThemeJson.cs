using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Themes.Dynamic;

/// <summary>
/// The serializer metadata for <see cref="MdThemeOptions"/>, generated at compile time.
/// </summary>
/// <remarks>
/// Reflection-based <c>JsonSerializer</c> cannot be trimmed: it discovers the shape of the type
/// at runtime. This contract is closed and owned by the library, so the source generator can emit
/// it, which keeps theme import and export working in a trimmed application and removes the
/// startup cost of building the contract reflectively.
/// </remarks>
[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(MdThemeOptions))]
internal sealed partial class MdThemeJsonContext : JsonSerializerContext
{
}

/// <summary>Stable JSON import/export contract used by Theme Lab and applications.</summary>
public static class MdThemeJson
{
    private static readonly JsonTypeInfo<MdThemeOptions> TypeInfo = CreateTypeInfo();

    public static string Serialize(MdThemeOptions theme) =>
        JsonSerializer.Serialize(theme, TypeInfo);

    public static MdThemeOptions Deserialize(string json) =>
        JsonSerializer.Deserialize(json, TypeInfo)
        ?? throw new JsonException("Theme JSON did not contain a theme object.");

    private static JsonTypeInfo<MdThemeOptions> CreateTypeInfo()
    {
        // The generated context supplies the metadata; these options supply the wire format.
        // The enum converters are the generic ones on purpose: the non-generic
        // JsonStringEnumConverter builds its converter at runtime, which is exactly the kind of
        // thing this file exists to avoid. Naming is camelCase on both properties and enum
        // members because that is the contract already written to disk by Theme Lab.
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = MdThemeJsonContext.Default,
            Converters =
            {
                new JsonStringEnumConverter<MdThemeSchemeVariant>(JsonNamingPolicy.CamelCase),
                new JsonStringEnumConverter<MdThemeContrastLevel>(JsonNamingPolicy.CamelCase),
                new JsonStringEnumConverter<MdThemeMode>(JsonNamingPolicy.CamelCase),
                new JsonStringEnumConverter<MdMotionScheme>(JsonNamingPolicy.CamelCase),
                new JsonStringEnumConverter<MdThemeFontProfile>(JsonNamingPolicy.CamelCase),
                new JsonStringEnumConverter<MdThemeShapeScale>(JsonNamingPolicy.CamelCase)
            }
        };

        return (JsonTypeInfo<MdThemeOptions>)options.GetTypeInfo(typeof(MdThemeOptions));
    }
}
