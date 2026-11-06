using BenchmarkDotNet.Running;

namespace Md3.Avalonia.Benchmarks;

/// <summary>BenchmarkDotNet entry point. Pass --filter to select benchmarks, for example --filter *Theme*.</summary>
public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
