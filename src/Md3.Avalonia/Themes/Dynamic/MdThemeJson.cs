using System.Text.Json;
using System.Text.Json.Serialization;

namespace Md3.Avalonia.Themes.Dynamic;

/// <summary>
/// The serializer context for <see cref="MdThemeOptions"/>, generated at compile time.
/// </summary>
/// <remarks>
/// Reflection-based <c>JsonSerializer</c> cannot be trimmed: it has to discover the shape of the
/// type at runtime. The contract here is closed and owned by the library, so the source generator
/// can emit it, which keeps theme import and export working in a trimmed application and avoids
/// the startup cost of building the contract reflectively.
/// </remarks>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(MdThemeOptions))]
internal sealed partial class MdThemeJsonContext : JsonSerializerContext;

/// <summary>Stable JSON import/export contract used by Theme Lab and applications.</summary>
public static class MdThemeJson
{
    public static string Serialize(MdThemeOptions theme) =>
        JsonSerializer.Serialize(theme, MdThemeJsonContext.Default.MdThemeOptions);

    public static MdThemeOptions Deserialize(string json) =>
        JsonSerializer.Deserialize(json, MdThemeJsonContext.Default.MdThemeOptions)
        ?? throw new JsonException("Theme JSON did not contain a theme object.");
}
