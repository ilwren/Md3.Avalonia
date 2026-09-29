using System.Text.Json;
using System.Text.Json.Serialization;

namespace Md3.Avalonia.Themes.Dynamic;

/// <summary>Stable JSON import/export contract used by Theme Lab and applications.</summary>
public static class MdThemeJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static string Serialize(MdThemeOptions theme) =>
        JsonSerializer.Serialize(theme, Options);

    public static MdThemeOptions Deserialize(string json) =>
        JsonSerializer.Deserialize<MdThemeOptions>(json, Options)
        ?? throw new JsonException("Theme JSON did not contain a theme object.");
}
