using Avalonia.Animation;

namespace Md3.Avalonia.Motion;

/// <summary>
/// A cubic-bezier easing sampled with the CSS <c>cubic-bezier(x1, y1, x2, y2)</c> definition:
/// the curve runs from (0,0) to (1,1) with the two given control points.
/// </summary>
/// <remarks>
/// Material's emphasized/expressive motion ships its non-spring curves as cubic beziers, so the
/// values can be frozen as tokens the same way <see cref="MdMotionTokens"/> freezes the springs.
/// </remarks>
public sealed class MdCubicBezierEasing : Easing
{
    private const int SampleIterations = 24;

    public MdCubicBezierEasing(double x1, double y1, double x2, double y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }

    public double X1 { get; }
    public double Y1 { get; }
    public double X2 { get; }
    public double Y2 { get; }

    public override double Ease(double progress)
    {
        if (progress <= 0) return 0;
        if (progress >= 1) return 1;

        // Solve x(t) = progress for t by bisection; x is monotonic on [0,1] for the Material
        // control points, and 24 halvings resolve t to better than 1e-7.
        var low = 0d;
        var high = 1d;
        for (var iteration = 0; iteration < SampleIterations; iteration++)
        {
            var t = (low + high) / 2;
            var x = SampleAxis(X1, X2, t);
            if (x < progress) low = t;
            else high = t;
        }

        var solved = (low + high) / 2;
        var value = SampleAxis(Y1, Y2, solved);
        return Math.Clamp(value, 0, 1);
    }

    private static double SampleAxis(double control1, double control2, double t)
    {
        var inverse = 1 - t;
        return 3 * inverse * inverse * t * control1 +
               3 * inverse * t * t * control2 +
               t * t * t;
    }
}

/// <summary>
/// Frozen Material motion easing curves for the platforms and cases that use duration + easing
/// instead of the spring tokens in <see cref="MdMotionTokens"/>.
/// </summary>
/// <remarks>
/// Values match the published <c>md-sys-motion-easing-*</c> tokens. The emphasized and expressive
/// families are the same curves under Material's two naming eras, so the expressive members are
/// aliases of the emphasized ones.
/// </remarks>
public static class MdEasing
{
    /// <summary>Utilitarian in-out easing: cubic-bezier(0.2, 0.0, 0.0, 1.0).</summary>
    public static Easing Standard { get; } = new MdCubicBezierEasing(0.2, 0.0, 0.0, 1.0);

    /// <summary>Accelerating utilitarian easing: cubic-bezier(0.3, 0.0, 1.0, 1.0).</summary>
    public static Easing StandardAccelerate { get; } = new MdCubicBezierEasing(0.3, 0.0, 1.0, 1.0);

    /// <summary>Decelerating utilitarian easing: cubic-bezier(0.0, 0.0, 0.0, 1.0).</summary>
    public static Easing StandardDecelerate { get; } = new MdCubicBezierEasing(0.0, 0.0, 0.0, 1.0);

    /// <summary>Emphasized in-out easing: cubic-bezier(0.05, 0.7, 0.1, 1.0).</summary>
    public static Easing Emphasized { get; } = new MdCubicBezierEasing(0.05, 0.7, 0.1, 1.0);

    /// <summary>Accelerating emphasized easing: cubic-bezier(0.3, 0.0, 0.8, 0.15).</summary>
    public static Easing EmphasizedAccelerate { get; } = new MdCubicBezierEasing(0.3, 0.0, 0.8, 0.15);

    /// <summary>Decelerating emphasized easing; the token value matches <see cref="Emphasized"/>.</summary>
    public static Easing EmphasizedDecelerate => Emphasized;

    /// <summary>M3 Expressive name for <see cref="Emphasized"/>.</summary>
    public static Easing Expressive => Emphasized;

    /// <summary>M3 Expressive name for <see cref="EmphasizedAccelerate"/>.</summary>
    public static Easing ExpressiveAccelerate => EmphasizedAccelerate;

    /// <summary>M3 Expressive name for <see cref="EmphasizedDecelerate"/>.</summary>
    public static Easing ExpressiveDecelerate => EmphasizedDecelerate;

    /// <summary>Legacy pre-M3 in-out easing: cubic-bezier(0.4, 0.0, 0.2, 1.0).</summary>
    public static Easing Legacy { get; } = new MdCubicBezierEasing(0.4, 0.0, 0.2, 1.0);
}

/// <summary>
/// Frozen Material motion duration tiers: <c>md-sys-motion-duration-short1..long4</c>, twelve
/// values grouped as short (50–200 ms), medium (250–400 ms) and long (450–600 ms).
/// </summary>
/// <remarks>
/// The spring-driven resolver in <see cref="MdMotion"/> keeps its own settling durations; these
/// tiers are for non-spring effects (plain fades, icon swaps, theme cross-fades) and for hosts
/// that reason in the token scale directly.
/// </remarks>
public static class MdMotionDurations
{
    public static TimeSpan Short1 { get; } = TimeSpan.FromMilliseconds(50);
    public static TimeSpan Short2 { get; } = TimeSpan.FromMilliseconds(100);
    public static TimeSpan Short3 { get; } = TimeSpan.FromMilliseconds(150);
    public static TimeSpan Short4 { get; } = TimeSpan.FromMilliseconds(200);
    public static TimeSpan Medium1 { get; } = TimeSpan.FromMilliseconds(250);
    public static TimeSpan Medium2 { get; } = TimeSpan.FromMilliseconds(300);
    public static TimeSpan Medium3 { get; } = TimeSpan.FromMilliseconds(350);
    public static TimeSpan Medium4 { get; } = TimeSpan.FromMilliseconds(400);
    public static TimeSpan Long1 { get; } = TimeSpan.FromMilliseconds(450);
    public static TimeSpan Long2 { get; } = TimeSpan.FromMilliseconds(500);
    public static TimeSpan Long3 { get; } = TimeSpan.FromMilliseconds(550);
    public static TimeSpan Long4 { get; } = TimeSpan.FromMilliseconds(600);
}
