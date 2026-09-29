namespace Md3.Avalonia.Motion;

/// <summary>A normalized damped spring used to sample Material spatial motion.</summary>
public readonly record struct MdSpring(double DampingRatio, double Stiffness)
{
    public double Sample(double elapsedSeconds)
    {
        if (elapsedSeconds <= 0)
        {
            return 0;
        }

        var omega = Math.Sqrt(Math.Max(1, Stiffness));
        if (DampingRatio < 1)
        {
            var damped = omega * Math.Sqrt(1 - DampingRatio * DampingRatio);
            var envelope = Math.Exp(-DampingRatio * omega * elapsedSeconds);
            var coefficient = DampingRatio / Math.Sqrt(1 - DampingRatio * DampingRatio);
            return 1 - envelope * (Math.Cos(damped * elapsedSeconds) + coefficient * Math.Sin(damped * elapsedSeconds));
        }

        return 1 - Math.Exp(-omega * elapsedSeconds) * (1 + omega * elapsedSeconds);
    }
}

/// <summary>Frozen Material 3 motion-physics token values.</summary>
public static class MdMotionTokens
{
    public static MdSpring ExpressiveFastSpatial { get; } = new(0.6, 800);
    public static MdSpring ExpressiveDefaultSpatial { get; } = new(0.8, 380);
    public static MdSpring ExpressiveSlowSpatial { get; } = new(0.8, 200);
    public static MdSpring FastEffects { get; } = new(1.0, 3800);
    public static MdSpring DefaultEffects { get; } = new(1.0, 1600);
    public static MdSpring SlowEffects { get; } = new(1.0, 800);
    public static MdSpring StandardFastSpatial { get; } = new(0.9, 1400);
    public static MdSpring StandardDefaultSpatial { get; } = new(0.9, 700);
    public static MdSpring StandardSlowSpatial { get; } = new(0.9, 300);
}
