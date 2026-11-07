using BenchmarkDotNet.Attributes;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Benchmarks;

/// <summary>
/// Icon name resolution. Only application-independent paths are benchmarked here: a miss never
/// touches the font provider, and <see cref="MdSymbols.AllNames"/> only materializes the alias
/// catalog. Name hits additionally consult the registered font through
/// EnsureConfigured, so hit timing is covered by the headless budget tests instead, where an
/// application with a font manager exists.
/// </summary>
[MemoryDiagnoser]
public class IconLookupBenchmarks
{
    [Benchmark]
    public bool TryGetByName_Miss() => MdSymbols.TryGetByName("definitely_not_an_icon_name", out _);

    [Benchmark]
    public int AllNames_Materialize() => MdSymbols.AllNames.Count;

    [Benchmark]
    public int Catalog_Count() => MdSymbols.Count;
}
