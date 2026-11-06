using Avalonia.Media;
using BenchmarkDotNet.Attributes;
using Md3.Avalonia.Themes.Dynamic;

namespace Md3.Avalonia.Benchmarks;

/// <summary>HCT scheme generation: the CPU cost behind seed changes and the Theme Lab matrix.</summary>
[MemoryDiagnoser]
public class ThemeGenerationBenchmarks
{
    private static readonly MdThemeOptions Options = new()
    {
        SeedColor = "#006A6A",
        SchemeVariant = MdThemeSchemeVariant.Expressive,
        ContrastLevel = MdThemeContrastLevel.High
    };

    private IReadOnlyDictionary<string, Color> _roles = new Dictionary<string, Color>();

    [GlobalSetup]
    public void Setup() => _roles = MdThemeGenerator.Generate(Options, dark: false);

    [Benchmark]
    public IReadOnlyDictionary<string, Color> Generate_SingleScheme() =>
        MdThemeGenerator.Generate(Options, dark: false);

    [Benchmark]
    public int Generate_FullMatrix()
    {
        var count = 0;
        foreach (var dark in new[] { false, true })
        foreach (var variant in Enum.GetValues<MdThemeSchemeVariant>())
        foreach (var contrast in Enum.GetValues<MdThemeContrastLevel>())
        {
            MdThemeGenerator.Generate(new MdThemeOptions
            {
                SeedColor = "#006A6A",
                SchemeVariant = variant,
                ContrastLevel = contrast
            }, dark);
            count++;
        }
        return count;
    }

    [Benchmark]
    public IReadOnlyList<MdContrastDiagnostic> Diagnose_GeneratedRoles() =>
        MdThemeManager.Diagnose(_roles);

    [Benchmark]
    public IReadOnlyList<Color> Generate_PrimaryTonalPalette() =>
        MdThemeGenerator.GeneratePrimaryTonalPalette(Color.Parse("#006A6A"));
}
