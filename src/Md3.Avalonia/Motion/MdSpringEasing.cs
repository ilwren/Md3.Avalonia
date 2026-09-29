using Avalonia.Animation.Easings;

namespace Md3.Avalonia.Motion;

/// <summary>Samples an <see cref="MdSpring"/> as an Avalonia transition easing.</summary>
public sealed class MdSpringEasing : Easing
{
    private readonly double _durationSeconds;
    private readonly bool _reduced;

    public MdSpringEasing(MdSpring spring, TimeSpan duration, bool reduced = false)
    {
        Spring = spring;
        Duration = duration;
        _durationSeconds = Math.Max(0, duration.TotalSeconds);
        _reduced = reduced;
    }

    public MdSpring Spring { get; }
    public TimeSpan Duration { get; }

    public override double Ease(double progress)
    {
        if (progress <= 0 || _durationSeconds <= 0) return 0;
        if (progress >= 1) return 1;

        // Reduced motion deliberately uses a short, monotonic effects fade rather than spring
        // travel. Smooth-step avoids a linear flash while never overshooting.
        if (_reduced)
        {
            return 1 - Math.Pow(1 - progress, 3);
        }

        return Spring.Sample(progress * _durationSeconds);
    }
}
