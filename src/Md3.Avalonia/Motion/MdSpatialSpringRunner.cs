using System.Diagnostics;
using Avalonia;
using Avalonia.Threading;

namespace Md3.Avalonia.Motion;

/// <summary>
/// Drives an interruptible scalar spatial spring. Retargeting retains the current position and
/// velocity, unlike a duration-based transition that restarts an easing curve from zero velocity.
/// </summary>
public sealed class MdSpatialSpringRunner : IDisposable
{
    private readonly AvaloniaObject _owner;
    private readonly Action<double> _apply;
    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _clock = new();
    private MdMotionSpeed _speed;
    private bool _disposed;

    public MdSpatialSpringRunner(AvaloniaObject owner, Action<double> apply, double initialValue = 0)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _apply = apply ?? throw new ArgumentNullException(nameof(apply));
        Position = Target = initialValue;
        _timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += OnTick;
    }

    public double Position { get; private set; }
    public double Velocity { get; private set; }
    public double Target { get; private set; }
    public bool IsRunning => _timer.IsEnabled;

    /// <summary>Changes the destination without discarding momentum from an in-flight spring.</summary>
    public void Retarget(double target, MdMotionSpeed speed = MdMotionSpeed.Default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Target = target;
        _speed = speed;
        var spec = MdMotion.Resolve(_owner, MdMotionKind.Spatial, speed);
        if (!spec.IsEnabled)
        {
            SnapTo(target);
            return;
        }

        if (Math.Abs(Target - Position) <= 0.001 && Math.Abs(Velocity) <= 0.001)
        {
            SnapTo(target);
            return;
        }

        _clock.Restart();
        _timer.Start();
    }

    /// <summary>Immediately establishes a value and clears velocity.</summary>
    public void SnapTo(double value)
    {
        Position = Target = value;
        Velocity = 0;
        _timer.Stop();
        _clock.Reset();
        _apply(value);
    }

    /// <summary>Stops frame delivery while preserving position, velocity, and target.</summary>
    public void Stop()
    {
        _timer.Stop();
        _clock.Reset();
    }

    /// <summary>Advances the runner deterministically; useful for hosts with their own frame clock.</summary>
    public void Advance(TimeSpan elapsed)
    {
        if (_disposed || elapsed <= TimeSpan.Zero) return;
        var spec = MdMotion.Resolve(_owner, MdMotionKind.Spatial, _speed);
        if (!spec.IsEnabled)
        {
            SnapTo(Target);
            return;
        }

        var remaining = Math.Min(elapsed.TotalSeconds, 0.05);
        const double maximumStep = 1d / 120d;
        var damping = 2 * spec.Spring.DampingRatio * Math.Sqrt(Math.Max(1, spec.Spring.Stiffness));
        while (remaining > 0)
        {
            var step = Math.Min(maximumStep, remaining);
            var acceleration = spec.Spring.Stiffness * (Target - Position) - damping * Velocity;
            Velocity += acceleration * step;
            Position += Velocity * step;
            remaining -= step;
        }

        var tolerance = Math.Max(0.01, Math.Abs(Target) * 0.0001);
        if (Math.Abs(Target - Position) <= tolerance && Math.Abs(Velocity) <= tolerance * 8)
        {
            Position = Target;
            Velocity = 0;
            _timer.Stop();
            _clock.Reset();
        }
        _apply(Position);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
        _timer.Tick -= OnTick;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var elapsed = _clock.Elapsed;
        _clock.Restart();
        Advance(elapsed);
    }
}
