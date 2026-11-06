using BenchmarkDotNet.Attributes;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Benchmarks;

/// <summary>Motion token resolution and spring sampling: pure math behind every animated control.</summary>
[MemoryDiagnoser]
public class MotionBenchmarks
{
    [Benchmark]
    public MdMotionSpec Resolve_Expressive_Spatial() =>
        MdMotion.Resolve(MdMotionScheme.Expressive, MdMotionKind.Spatial, MdMotionSpeed.Default);

    [Benchmark]
    public double Easing_Evaluate_Midpoint()
    {
        var spec = MdMotion.Resolve(MdMotionScheme.Expressive, MdMotionKind.Spatial, MdMotionSpeed.Default);
        return spec.CreateEasing().Ease(0.5);
    }

    [Benchmark]
    public double Spring_Sample_MidFlight() => MdMotionTokens.ExpressiveDefaultSpatial.Sample(0.12);
}
