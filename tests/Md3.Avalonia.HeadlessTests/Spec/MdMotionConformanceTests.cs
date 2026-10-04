using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>
/// Layer L5 — motion trajectory conformance.
/// </summary>
/// <remarks>
/// <para>
/// Motion is the easiest layer to fake and the hardest to prove. Asserting that
/// <c>MdSpring.Sample</c> returns what <c>MdSpring.Sample</c> returns is self-referential; so is
/// asserting that a duration constant equals the duration constant. This layer therefore checks
/// only claims that an independent computation can falsify:
/// </para>
/// <list type="number">
/// <item>the closed-form spring solution against a fourth-order Runge-Kutta integration of the
/// underlying differential equation;</item>
/// <item>the documented claim that each published duration is the spring's ~0.1% settling point;</item>
/// <item>the runtime integrator inside <c>MdSpatialSpringRunner</c> against that same closed form —
/// two independent implementations of the same physics;</item>
/// <item>the C# motion tokens against the XAML motion tokens, which are separate declarations that
/// nothing else keeps in sync.</item>
/// </list>
/// <para>
/// The spring physics need no UI, so they run as a plain <c>[Fact]</c> outside the Avalonia
/// headless session. Only the parts that genuinely need a dispatcher or the resource system pay
/// for one.
/// </para>
/// </remarks>
public sealed class MdMotionConformanceTests
{
    private const string Layer = "L5_motion";

    private static readonly (string Name, MdSpring Spring)[] Tokens =
    [
        ("ExpressiveFastSpatial", MdMotionTokens.ExpressiveFastSpatial),
        ("ExpressiveDefaultSpatial", MdMotionTokens.ExpressiveDefaultSpatial),
        ("ExpressiveSlowSpatial", MdMotionTokens.ExpressiveSlowSpatial),
        ("FastEffects", MdMotionTokens.FastEffects),
        ("DefaultEffects", MdMotionTokens.DefaultEffects),
        ("SlowEffects", MdMotionTokens.SlowEffects),
        ("StandardFastSpatial", MdMotionTokens.StandardFastSpatial),
        ("StandardDefaultSpatial", MdMotionTokens.StandardDefaultSpatial),
        ("StandardSlowSpatial", MdMotionTokens.StandardSlowSpatial)
    ];

    [Fact]
    public void Spring_Physics_Match_An_Independent_Integration_And_The_Published_Settling_Points()
    {
        var policy = MdSpecPolicy.Load();
        var oracle = MdSpecOracle.Load();
        var report = new MdSpecReport("L5-motion-physics", Layer, "L5 — spring physics", policy);

        Assert.True(policy.IsLayerEnabled(Layer), "L5_motion is disabled in spec-snapshot/conformance-policy.json.");

        VerifyClosedFormAgainstIntegration(oracle, report);
        VerifySettlingDurations(oracle, report);
        VerifyReducedAndNoneContracts(report);
        VerifyEasingEndpoints(report);

        report.Complete();
    }

    [AvaloniaFact]
    public void Runtime_Motion_Matches_The_Closed_Form_And_The_Xaml_Tokens()
    {
        var policy = MdSpecPolicy.Load();
        var report = new MdSpecReport("L5-motion-runtime", Layer, "L5 — runtime motion", policy);

        VerifyXamlTokenParity(report);
        VerifySpringRunnerTrajectory(report);
        VerifySpringRunnerInterruption(report);

        report.Complete();
    }

    /// <summary>
    /// The analytic solution is compared against RK4 on x'' = k(1 - x) - 2*zeta*sqrt(k)*x'.
    /// A sign error, a swapped damping ratio, or a dropped term in the under-damped branch all
    /// produce divergence here; none of them can be caught by comparing the formula to itself.
    /// </summary>
    private static void VerifyClosedFormAgainstIntegration(MdSpecOracle oracle, MdSpecReport report)
    {
        foreach (var (name, spring) in Tokens)
        {
            var omega = Math.Sqrt(Math.Max(1, spring.Stiffness));
            var damping = 2 * spring.DampingRatio * omega;
            var horizon = Math.Min(2.0, 12.0 / (spring.DampingRatio * omega));

            var worst = 0d;
            var worstAt = 0d;
            var x = 0d;
            var v = 0d;
            const double step = 2e-5;
            var samples = 0;
            var nextProbe = horizon / 20.0;

            for (var t = 0d; t < horizon; t += step)
            {
                (x, v) = RungeKutta(x, v, spring.Stiffness, damping, step);
                if (t + step < nextProbe)
                {
                    continue;
                }

                var analytic = spring.Sample(t + step);
                var delta = Math.Abs(analytic - x);
                if (delta > worst)
                {
                    worst = delta;
                    worstAt = t + step;
                }

                samples++;
                nextProbe += horizon / 20.0;
            }

            if (samples == 0)
            {
                report.Gap($"spring/{name}/integration", "integration produced no probe points");
                continue;
            }

            report.Check(
                $"spring/{name}/integration",
                worst <= oracle.MotionIntegratorTolerance,
                $"closed form deviates from a fourth-order integration of x'' = {spring.Stiffness:0.##}(1-x) " +
                $"- {damping:0.###}x' by {worst:0.#####} at t={worstAt:0.####}s " +
                $"(tolerance {oracle.MotionIntegratorTolerance:0.####})");
        }
    }

    /// <summary>
    /// <c>MdMotion.Resolve</c> documents its durations as "the approximate 0.1% settling points of
    /// the frozen spring tokens". That is a falsifiable claim about the pair (spring, duration):
    /// sampling the spring at its own duration must land within a thousandth of the target.
    /// </summary>
    private static void VerifySettlingDurations(MdSpecOracle oracle, MdSpecReport report)
    {
        foreach (var scheme in new[] { MdMotionScheme.Expressive, MdMotionScheme.Standard })
        {
            foreach (var kind in new[] { MdMotionKind.Spatial, MdMotionKind.Effects })
            {
                foreach (var speed in new[] { MdMotionSpeed.Fast, MdMotionSpeed.Default, MdMotionSpeed.Slow })
                {
                    var spec = MdMotion.Resolve(scheme, kind, speed);
                    var subject = $"settling/{scheme}/{kind}/{speed}";

                    if (!spec.IsEnabled)
                    {
                        report.Gap(subject, $"{scheme}/{kind}/{speed} resolved to a disabled spec");
                        continue;
                    }

                    var residual = Math.Abs(1 - spec.Spring.Sample(spec.Duration.TotalSeconds));
                    report.Check(
                        subject,
                        residual <= oracle.MotionSettlingTolerance,
                        $"spring ({spec.Spring.DampingRatio:0.##}, {spec.Spring.Stiffness:0}) has settled to " +
                        $"{1 - residual:0.#####} after its published {spec.Duration.TotalMilliseconds:0}ms, " +
                        $"leaving a residual of {residual:0.#####} (tolerance {oracle.MotionSettlingTolerance:0.#####}). " +
                        "The duration and the spring token have drifted apart.");
                }
            }
        }
    }

    /// <summary>Reduced and None are accessibility-adjacent contracts the library states explicitly.</summary>
    private static void VerifyReducedAndNoneContracts(MdSpecReport report)
    {
        foreach (var speed in new[] { MdMotionSpeed.Fast, MdMotionSpeed.Default, MdMotionSpeed.Slow })
        {
            var reducedSpatial = MdMotion.Resolve(MdMotionScheme.Reduced, MdMotionKind.Spatial, speed);
            report.Check(
                $"reduced/Spatial/{speed}",
                !reducedSpatial.IsEnabled && reducedSpatial.Duration == TimeSpan.Zero,
                $"reduced motion must remove spatial travel entirely, got enabled={reducedSpatial.IsEnabled} " +
                $"duration={reducedSpatial.Duration.TotalMilliseconds}ms");

            var reducedEffects = MdMotion.Resolve(MdMotionScheme.Reduced, MdMotionKind.Effects, speed);
            report.Check(
                $"reduced/Effects/{speed}",
                reducedEffects.IsEnabled && reducedEffects.Duration > TimeSpan.Zero &&
                reducedEffects.Duration <= TimeSpan.FromMilliseconds(150),
                $"reduced motion must keep a short effects fade, got enabled={reducedEffects.IsEnabled} " +
                $"duration={reducedEffects.Duration.TotalMilliseconds}ms");

            foreach (var kind in new[] { MdMotionKind.Spatial, MdMotionKind.Effects })
            {
                var none = MdMotion.Resolve(MdMotionScheme.None, kind, speed);
                report.Check(
                    $"none/{kind}/{speed}",
                    !none.IsEnabled && none.Duration == TimeSpan.Zero,
                    $"the None scheme must disable all motion, got enabled={none.IsEnabled} " +
                    $"duration={none.Duration.TotalMilliseconds}ms");
            }
        }
    }

    /// <summary>
    /// A transition that does not end exactly on its target leaves a visible residual offset, so
    /// the easing endpoints are a hard contract regardless of the spring's overshoot in between.
    /// </summary>
    private static void VerifyEasingEndpoints(MdSpecReport report)
    {
        foreach (var (name, spring) in Tokens)
        {
            var easing = new MdSpringEasing(spring, TimeSpan.FromMilliseconds(400));
            var start = easing.Ease(0);
            var end = easing.Ease(1);
            var finite = true;

            for (var p = 0d; p <= 1d; p += 0.02)
            {
                var value = easing.Ease(p);
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    finite = false;
                    break;
                }
            }

            report.Check(
                $"easing/{name}/endpoints",
                start == 0 && end == 1 && finite,
                "easing must start at 0 and finish exactly at 1 with finite intermediate values; " +
                $"got start={start}, end={end}, finite={finite}");

            // An under-damped spring is supposed to overshoot and an over-damped one is not, which
            // is what makes the damping ratio meaningful. The horizon follows the spring's own
            // damped period rather than a fixed window, otherwise a slow spring's first peak falls
            // outside the sample range and the check reports a phantom failure.
            var omega = Math.Sqrt(Math.Max(1, spring.Stiffness));
            var horizon = spring.DampingRatio < 1
                ? 1.5 * (2 * Math.PI / (omega * Math.Sqrt(1 - spring.DampingRatio * spring.DampingRatio)))
                : 8.0 / omega;

            var peak = 0d;
            for (var t = 0d; t <= horizon; t += horizon / 400.0)
            {
                peak = Math.Max(peak, spring.Sample(t));
            }

            var overshoots = peak > 1.0001;
            var shouldOvershoot = spring.DampingRatio < 1;
            report.Check(
                $"spring/{name}/overshoot",
                overshoots == shouldOvershoot,
                $"damping ratio {spring.DampingRatio:0.##} implies overshoot={shouldOvershoot}, " +
                $"but the peak over {horizon * 1000:0}ms was {peak:0.#####}",
                highConfidence: false);
        }
    }

    /// <summary>
    /// The C# tokens and the XAML tokens are two independent declarations of the same frozen
    /// values. Nothing in the build keeps them in step, which is exactly why this is worth testing.
    /// </summary>
    /// <remarks>
    /// This is a catalogue-completeness and agreement check, deliberately not a claim that the
    /// XAML drives anything: <c>MdMotion</c> reads only <c>Md.Sys.Motion.Scheme</c> at runtime and
    /// takes every spring from the compiled constants. The resource keys document the motion
    /// system and let a consumer read the values; overriding one does not currently change an
    /// animation. That limitation is recorded as a note below so the token surface cannot be
    /// mistaken for a theming hook.
    /// </remarks>
    private static void VerifyXamlTokenParity(MdSpecReport report)
    {
        var application = Application.Current!;
        (string Token, string ResourcePrefix)[] mapping =
        [
            ("ExpressiveFastSpatial", "Md.Sys.Motion.Expressive.FastSpatial"),
            ("ExpressiveDefaultSpatial", "Md.Sys.Motion.Expressive.DefaultSpatial"),
            ("ExpressiveSlowSpatial", "Md.Sys.Motion.Expressive.SlowSpatial"),
            ("FastEffects", "Md.Sys.Motion.FastEffects"),
            ("DefaultEffects", "Md.Sys.Motion.DefaultEffects"),
            ("SlowEffects", "Md.Sys.Motion.SlowEffects"),
            ("StandardFastSpatial", "Md.Sys.Motion.Standard.FastSpatial"),
            ("StandardDefaultSpatial", "Md.Sys.Motion.Standard.DefaultSpatial"),
            ("StandardSlowSpatial", "Md.Sys.Motion.Standard.SlowSpatial")
        ];

        foreach (var (tokenName, prefix) in mapping)
        {
            var spring = Tokens.First(t => t.Name == tokenName).Spring;
            var hasDamping = application.TryGetResource(prefix + ".DampingRatio", ThemeVariant.Light, out var damping);
            var hasStiffness = application.TryGetResource(prefix + ".Stiffness", ThemeVariant.Light, out var stiffness);

            if (!hasDamping || !hasStiffness || damping is not double dampingValue || stiffness is not double stiffnessValue)
            {
                report.Gap(
                    prefix,
                    $"MdMotionTokens.{tokenName} = ({spring.DampingRatio:0.##}, {spring.Stiffness:0}) has no XAML " +
                    "counterpart, so the published token surface describes only part of the motion system",
                    highConfidence: false);
                continue;
            }

            report.Check(
                prefix,
                Math.Abs(dampingValue - spring.DampingRatio) < 1e-9 &&
                Math.Abs(stiffnessValue - spring.Stiffness) < 1e-9,
                $"XAML declares ({dampingValue:0.##}, {stiffnessValue:0}) but MdMotionTokens.{tokenName} is " +
                $"({spring.DampingRatio:0.##}, {spring.Stiffness:0})");
        }

        // Stated once, as a finding rather than a comment, because the gap between "there is a
        // token for it" and "changing the token changes the behaviour" is exactly the kind of
        // thing a token catalogue is read as promising.
        report.Note(
            "Md.Sys.Motion.*",
            "These keys are declarative. MdMotion resolves only Md.Sys.Motion.Scheme from resources " +
            "and takes every spring from the compiled MdMotionTokens constants, so overriding a " +
            "spring key in a consumer theme does not currently retarget any animation.");
    }

    /// <summary>
    /// The runner integrates the spring numerically at 120 Hz sub-steps while <c>MdSpring</c>
    /// solves it in closed form. Advancing with an explicit clock makes the comparison exact and
    /// removes every source of flake: no timers, no wall-clock, no frame scheduling.
    /// </summary>
    private static void VerifySpringRunnerTrajectory(MdSpecReport report)
    {
        var owner = new Border();
        MdMotion.SetScheme(owner, MdMotionScheme.Standard);
        var spec = MdMotion.Resolve(owner, MdMotionKind.Spatial, MdMotionSpeed.Default);

        var fine = TraceRunner(owner, spec, TimeSpan.FromMilliseconds(1));
        var coarse = TraceRunner(owner, spec, TimeSpan.FromMilliseconds(2));
        var realistic = TraceRunner(owner, spec, TimeSpan.FromMilliseconds(16));

        report.Check(
            "runner/trajectory",
            fine.Finite && fine.WorstDelta <= 0.02,
            $"MdSpatialSpringRunner diverges from the closed-form spring by {fine.WorstDelta:0.#####} at " +
            $"t={fine.WorstAt:0.###}s when stepped at 1ms, which is far more than discretisation error can " +
            "explain; the runtime integrator and MdSpring are not solving the same equation");

        // The runner uses semi-implicit Euler, which is first-order accurate: halving the step must
        // halve the error. Asserting the convergence ORDER rather than a hand-picked tolerance is
        // what actually proves the integrator is consistent with the closed form - a scheme that
        // integrated the wrong damping term would still be "close" at a fixed step size, but it
        // would not converge at rate one.
        var ratio = fine.WorstDelta > 1e-9 ? coarse.WorstDelta / fine.WorstDelta : double.NaN;
        report.Check(
            "runner/first-order-convergence",
            ratio is > 1.5 and < 2.5,
            $"halving the integration step changed the error by a factor of {ratio:0.###} instead of ~2 " +
            $"(1ms: {fine.WorstDelta:0.#####}, 2ms: {coarse.WorstDelta:0.#####}); the runtime integrator is " +
            "not a consistent first-order discretisation of the spring it claims to implement");

        report.Check(
            "runner/settles",
            Math.Abs(fine.Final - 1.0) <= 0.02,
            $"after the published {spec.Duration.TotalMilliseconds:0}ms the runner is at {fine.Final:0.####}, " +
            "not at its target");

        report.Check(
            "runner/applies-every-frame",
            fine.Applied > 0,
            "the runner never invoked its apply callback, so nothing would animate");

        report.Check(
            "runner/stops",
            !fine.StillRunning,
            "the runner is still scheduling frames after reaching its target, which keeps the render " +
            "loop awake and drains battery on mobile",
            highConfidence: false);

        // Recorded, not gated: at a real 60Hz frame budget the first-order integrator trails the
        // analytic curve by this much mid-flight. It is a property of the chosen scheme, not a
        // defect, but it should be a visible number rather than folk knowledge.
        report.Note(
            "runner/60hz-deviation",
            $"stepped at a realistic 16ms frame the runner's peak deviation from the closed-form spring is " +
            $"{realistic.WorstDelta:0.####} (at t={realistic.WorstAt:0.###}s). Semi-implicit Euler is " +
            "first-order, so this scales linearly with frame time.");
    }

    private readonly record struct MdRunnerTrace(
        double WorstDelta, double WorstAt, double Final, int Applied, bool Finite, bool StillRunning);

    private static MdRunnerTrace TraceRunner(AvaloniaObject owner, MdMotionSpec spec, TimeSpan frame)
    {
        var applied = 0;
        using var runner = new MdSpatialSpringRunner(owner, _ => applied++);
        runner.Retarget(1.0);

        var elapsed = 0d;
        var worst = 0d;
        var worstAt = 0d;
        var finite = true;

        while (elapsed < spec.Duration.TotalSeconds)
        {
            runner.Advance(frame);
            elapsed += frame.TotalSeconds;

            if (double.IsNaN(runner.Position) || double.IsInfinity(runner.Position))
            {
                finite = false;
                break;
            }

            var delta = Math.Abs(spec.Spring.Sample(elapsed) - runner.Position);
            if (delta > worst)
            {
                worst = delta;
                worstAt = elapsed;
            }
        }

        return new MdRunnerTrace(worst, worstAt, runner.Position, applied, finite, runner.IsRunning);
    }

    /// <summary>
    /// Retargeting mid-flight must keep momentum. A duration-based transition restarts from zero
    /// velocity and visibly stutters; this is the property that distinguishes the two.
    /// </summary>
    private static void VerifySpringRunnerInterruption(MdSpecReport report)
    {
        var owner = new Border();
        MdMotion.SetScheme(owner, MdMotionScheme.Expressive);

        using var runner = new MdSpatialSpringRunner(owner, _ => { });
        runner.Retarget(1.0);
        for (var i = 0; i < 6; i++)
        {
            runner.Advance(TimeSpan.FromMilliseconds(8));
        }

        var velocityBefore = runner.Velocity;
        report.Check(
            "runner/in-flight-velocity",
            Math.Abs(velocityBefore) > 0.01,
            $"the runner should carry velocity 48ms into a spring, got {velocityBefore:0.####}");

        runner.Retarget(2.0);
        report.Check(
            "runner/retarget-preserves-momentum",
            Math.Abs(runner.Velocity - velocityBefore) < 1e-9,
            $"retargeting reset velocity from {velocityBefore:0.####} to {runner.Velocity:0.####} instead of " +
            "carrying it into the new spring");

        report.Check(
            "runner/retarget-updates-target",
            Math.Abs(runner.Target - 2.0) < 1e-9,
            $"target is {runner.Target:0.####} after retargeting to 2.0");

        // Retargeting to where the spring already rests must degenerate to a snap rather than
        // scheduling frames that would never produce a visible change.
        runner.SnapTo(3.0);
        runner.Retarget(3.0);
        report.Check(
            "runner/no-op-retarget-does-not-schedule",
            !runner.IsRunning,
            "retargeting to the current resting value started a frame loop that can never move");
    }

    private static (double X, double V) RungeKutta(double x, double v, double stiffness, double damping, double h)
    {
        double AccelerationAt(double position, double velocity) =>
            stiffness * (1 - position) - damping * velocity;

        var k1X = v;
        var k1V = AccelerationAt(x, v);
        var k2X = v + 0.5 * h * k1V;
        var k2V = AccelerationAt(x + 0.5 * h * k1X, v + 0.5 * h * k1V);
        var k3X = v + 0.5 * h * k2V;
        var k3V = AccelerationAt(x + 0.5 * h * k2X, v + 0.5 * h * k2V);
        var k4X = v + h * k3V;
        var k4V = AccelerationAt(x + h * k3X, v + h * k3V);

        return (
            x + h / 6 * (k1X + 2 * k2X + 2 * k3X + k4X),
            v + h / 6 * (k1V + 2 * k2V + 2 * k3V + k4V));
    }
}
