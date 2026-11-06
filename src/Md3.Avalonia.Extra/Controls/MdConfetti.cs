using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>A non-interactive, finite decorative particle burst within the control's bounds.</summary>
/// <remarks>
/// Call <see cref="Burst"/> after the control is attached, visible and arranged. Reduced/None
/// motion suppresses the effect. Hiding or detaching cancels it rather than restarting it later.
/// Particle geometry and physics are captured at the start of each burst; brushes remain live.
/// </remarks>
public sealed class MdConfetti : Control
{
    public static readonly StyledProperty<int> ParticleCountProperty = AvaloniaProperty.Register<MdConfetti, int>(
        nameof(ParticleCount), 48, coerce: static (_, value) => Math.Clamp(value, 0, 500));
    public static readonly StyledProperty<double> ParticleSizeProperty = AvaloniaProperty.Register<MdConfetti, double>(
        nameof(ParticleSize), 8, coerce: static (_, value) => double.IsFinite(value) ? Math.Clamp(value, 1, 40) : 8);
    public static readonly StyledProperty<double> SpeedProperty = AvaloniaProperty.Register<MdConfetti, double>(
        nameof(Speed), 160, coerce: static (_, value) => double.IsFinite(value) ? Math.Clamp(value, 0, 2000) : 160);
    public static readonly StyledProperty<double> DirectionDegreesProperty = AvaloniaProperty.Register<MdConfetti, double>(
        nameof(DirectionDegrees), -90, coerce: static (_, value) => double.IsFinite(value) ? Math.Clamp(value, -3600, 3600) : -90);
    public static readonly StyledProperty<double> SpreadDegreesProperty = AvaloniaProperty.Register<MdConfetti, double>(
        nameof(SpreadDegrees), 100, coerce: static (_, value) => double.IsFinite(value) ? Math.Clamp(value, 0, 360) : 100);
    public static readonly StyledProperty<double> GravityProperty = AvaloniaProperty.Register<MdConfetti, double>(
        nameof(Gravity), 150, coerce: static (_, value) => double.IsFinite(value) ? Math.Clamp(value, -2000, 2000) : 150);
    public static readonly StyledProperty<double> DragProperty = AvaloniaProperty.Register<MdConfetti, double>(
        nameof(Drag), 0.5, coerce: static (_, value) => double.IsFinite(value) ? Math.Clamp(value, 0, 20) : 0.5);
    public static readonly StyledProperty<TimeSpan> DurationProperty = AvaloniaProperty.Register<MdConfetti, TimeSpan>(
        nameof(Duration), TimeSpan.FromSeconds(2.5), coerce: static (_, value) =>
            value < TimeSpan.FromMilliseconds(100) ? TimeSpan.FromMilliseconds(100) : value > TimeSpan.FromSeconds(30) ? TimeSpan.FromSeconds(30) : value);
    public static readonly StyledProperty<Point> OriginProperty = AvaloniaProperty.Register<MdConfetti, Point>(
        nameof(Origin), new Point(0.5, 0.8), coerce: static (_, value) =>
            double.IsFinite(value.X) && double.IsFinite(value.Y) ? new Point(Math.Clamp(value.X, 0, 1), Math.Clamp(value.Y, 0, 1)) : new Point(0.5, 0.8));
    public static readonly StyledProperty<int?> SeedProperty = AvaloniaProperty.Register<MdConfetti, int?>(nameof(Seed));
    public static readonly StyledProperty<IBrush?> PrimaryBrushProperty = AvaloniaProperty.Register<MdConfetti, IBrush?>(nameof(PrimaryBrush));
    public static readonly StyledProperty<IBrush?> SecondaryBrushProperty = AvaloniaProperty.Register<MdConfetti, IBrush?>(nameof(SecondaryBrush));
    public static readonly StyledProperty<IBrush?> TertiaryBrushProperty = AvaloniaProperty.Register<MdConfetti, IBrush?>(nameof(TertiaryBrush));
    public static readonly DirectProperty<MdConfetti, bool> IsRunningProperty =
        AvaloniaProperty.RegisterDirect<MdConfetti, bool>(nameof(IsRunning), owner => owner.IsRunning);

    private readonly List<(Visual Visual, EventHandler<AvaloniaPropertyChangedEventArgs> Handler)> _ancestors = [];
    private Particle[] _particles = [];
    private Point _burstOrigin;
    private double _burstGravity;
    private double _burstDrag;
    private double _durationSeconds;
    private double _elapsed;
    private bool _attached;
    private bool _isRunning;
    private bool _frameRequested;
    private bool _lastFrameValid;
    private TimeSpan _lastFrame;
    private int _generation;

    static MdConfetti()
    {
        AffectsRender<MdConfetti>(PrimaryBrushProperty, SecondaryBrushProperty, TertiaryBrushProperty);
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdConfetti>((control, _) =>
        {
            if (!control.MotionAllowed) control.Stop();
        });
    }

    public MdConfetti() => IsHitTestVisible = false;

    /// <summary>Particles per burst, coerced to 0–500. Zero disables emission.</summary>
    public int ParticleCount { get => GetValue(ParticleCountProperty); set => SetValue(ParticleCountProperty, value); }
    /// <summary>Nominal particle size in DIP, coerced to 1–40.</summary>
    public double ParticleSize { get => GetValue(ParticleSizeProperty); set => SetValue(ParticleSizeProperty, value); }
    /// <summary>Maximum initial speed in DIP per second, coerced to 0–2000.</summary>
    public double Speed { get => GetValue(SpeedProperty); set => SetValue(SpeedProperty, value); }
    /// <summary>Emission angle in degrees: zero points right, 90 down, and -90 up.</summary>
    public double DirectionDegrees { get => GetValue(DirectionDegreesProperty); set => SetValue(DirectionDegreesProperty, value); }
    /// <summary>Angular spread in degrees, coerced to 0–360.</summary>
    public double SpreadDegrees { get => GetValue(SpreadDegreesProperty); set => SetValue(SpreadDegreesProperty, value); }
    /// <summary>Vertical acceleration in DIP per second squared, positive downwards.</summary>
    public double Gravity { get => GetValue(GravityProperty); set => SetValue(GravityProperty, value); }
    /// <summary>Exponential damping of the initial velocity per second; gravity is not damped.</summary>
    public double Drag { get => GetValue(DragProperty); set => SetValue(DragProperty, value); }
    /// <summary>Lifetime of one burst, coerced to 0.1–30 seconds.</summary>
    public TimeSpan Duration { get => GetValue(DurationProperty); set => SetValue(DurationProperty, value); }
    /// <summary>Emitter location normalized to the bounds, coerced to 0–1 on each axis.</summary>
    public Point Origin { get => GetValue(OriginProperty); set => SetValue(OriginProperty, value); }
    /// <summary>Optional deterministic seed; null chooses a fresh seed for each burst.</summary>
    public int? Seed { get => GetValue(SeedProperty); set => SetValue(SeedProperty, value); }
    /// <summary>First particle brush, supplied by the Extra theme by default.</summary>
    public IBrush? PrimaryBrush { get => GetValue(PrimaryBrushProperty); set => SetValue(PrimaryBrushProperty, value); }
    /// <summary>Second particle brush, supplied by the Extra theme by default.</summary>
    public IBrush? SecondaryBrush { get => GetValue(SecondaryBrushProperty); set => SetValue(SecondaryBrushProperty, value); }
    /// <summary>Third particle brush, supplied by the Extra theme by default.</summary>
    public IBrush? TertiaryBrush { get => GetValue(TertiaryBrushProperty); set => SetValue(TertiaryBrushProperty, value); }
    /// <summary>Whether a burst is currently active.</summary>
    public bool IsRunning => _isRunning;

    /// <summary>Raised only when a burst reaches its natural duration, not when it is canceled.</summary>
    public event EventHandler? Completed;

    /// <summary>Starts a new burst, replacing any existing one. Returns false when emission is suppressed.</summary>
    public bool Burst()
    {
        Stop();
        if (!_attached || !IsEffectivelyVisible || !MotionAllowed || Bounds.Width <= 0 || Bounds.Height <= 0 ||
            ParticleCount == 0 || TopLevel.GetTopLevel(this) is null) return false;

        _burstOrigin = new Point(Bounds.Width * Origin.X, Bounds.Height * Origin.Y);
        _burstGravity = Gravity;
        _burstDrag = Drag;
        _durationSeconds = Duration.TotalSeconds;
        var random = new Random(Seed ?? Random.Shared.Next());
        var particles = new Particle[ParticleCount];
        for (var i = 0; i < particles.Length; i++)
        {
            var angle = (DirectionDegrees + (random.NextDouble() - 0.5) * SpreadDegrees) * Math.PI / 180;
            var speed = Speed * (0.55 + random.NextDouble() * 0.45);
            particles[i] = new Particle(new Vector(Math.Cos(angle) * speed, Math.Sin(angle) * speed),
                ParticleSize * (0.65 + random.NextDouble() * 0.35), random.NextDouble() * Math.Tau,
                (random.NextDouble() * 2 - 1) * 6, random.Next(3), random.Next(2) == 0);
        }
        _particles = particles;
        _elapsed = 0;
        _lastFrameValid = false;
        SetAndRaise(IsRunningProperty, ref _isRunning, true);
        InvalidateVisual();
        RequestFrame();
        return true;
    }

    /// <summary>Cancels the active burst, clears the particles, and does not raise Completed.</summary>
    public void Stop()
    {
        _generation++;
        _frameRequested = false;
        _lastFrameValid = false;
        _particles = [];
        _elapsed = 0;
        if (SetAndRaise(IsRunningProperty, ref _isRunning, false)) InvalidateVisual();
    }

    private bool MotionAllowed => MdMotion.GetScheme(this) is not (MdMotionScheme.Reduced or MdMotionScheme.None);

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (!IsRunning || Bounds.Width <= 0 || Bounds.Height <= 0) return;
        IBrush?[] brushes = [PrimaryBrush, SecondaryBrush, TertiaryBrush];
        var fade = Math.Clamp((_durationSeconds - _elapsed) / Math.Min(0.35, _durationSeconds), 0, 1);
        using (context.PushClip(new Rect(Bounds.Size)))
        using (context.PushOpacity(fade))
        {
            foreach (var particle in _particles)
            {
                var brush = brushes[particle.BrushIndex];
                if (brush is null) continue;
                var point = Position(particle, _elapsed);
                var size = particle.Size;
                if (particle.Circle)
                    context.DrawEllipse(brush, null, point, size / 2, size / 2);
                else
                {
                    using (context.PushTransform(Matrix.CreateRotation(particle.Rotation + particle.Spin * _elapsed) * Matrix.CreateTranslation(point.X, point.Y)))
                        context.DrawRectangle(brush, null, new Rect(-size / 2, -size / 4, size, size / 2));
                }
            }
        }
    }

    private Point Position(Particle particle, double elapsed)
    {
        var travel = _burstDrag < 0.00001 ? elapsed : (1 - Math.Exp(-_burstDrag * elapsed)) / _burstDrag;
        return _burstOrigin + particle.Velocity * travel + new Vector(0, 0.5 * _burstGravity * elapsed * elapsed);
    }

    private void RequestFrame()
    {
        if (_frameRequested || !IsRunning || TopLevel.GetTopLevel(this) is not { } topLevel) return;
        _frameRequested = true;
        var generation = _generation;
        topLevel.RequestAnimationFrame(timestamp => OnFrame(timestamp, generation));
    }

    private void OnFrame(TimeSpan timestamp, int generation)
    {
        if (generation != _generation) return;
        _frameRequested = false;
        if (!IsRunning || !_attached || !IsEffectivelyVisible || !MotionAllowed) { Stop(); return; }
        if (_lastFrameValid) Advance(Math.Clamp((timestamp - _lastFrame).TotalSeconds, 0, 0.1));
        // Completion handlers can synchronously start a new burst. This frame belongs to the
        // old generation and must not overwrite the new burst's timing or frame request.
        if (generation != _generation) return;
        _lastFrame = timestamp;
        _lastFrameValid = true;
        if (IsRunning) RequestFrame();
    }

    // Shared by the actual frame loop and deterministic headless tests.
    internal void Advance(double seconds)
    {
        if (!IsRunning || !double.IsFinite(seconds) || seconds <= 0) return;
        _elapsed += seconds;
        if (_elapsed >= _durationSeconds)
        {
            Stop();
            Completed?.Invoke(this, EventArgs.Empty);
        }
        else InvalidateVisual();
    }

    internal int ActiveParticleCount => _particles.Length;
    internal double ElapsedSeconds => _elapsed;
    internal Point ParticlePosition(int index) => Position(_particles[index], _elapsed);
    private readonly record struct Particle(Vector Velocity, double Size, double Rotation, double Spin, int BrushIndex, bool Circle);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if ((change.Property == IsVisibleProperty && !IsEffectivelyVisible) ||
            (change.Property == BoundsProperty && (Bounds.Width <= 0 || Bounds.Height <= 0))) Stop();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attached = true;
        UnsubscribeAncestors();
        foreach (var ancestor in this.GetVisualAncestors().OfType<Visual>())
        {
            EventHandler<AvaloniaPropertyChangedEventArgs> handler = (_, change) =>
            {
                if (change.Property == IsVisibleProperty && !IsEffectivelyVisible) Stop();
            };
            ancestor.PropertyChanged += handler;
            _ancestors.Add((ancestor, handler));
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _attached = false;
        UnsubscribeAncestors();
        Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private void UnsubscribeAncestors()
    {
        foreach (var (visual, handler) in _ancestors) visual.PropertyChanged -= handler;
        _ancestors.Clear();
    }
}
