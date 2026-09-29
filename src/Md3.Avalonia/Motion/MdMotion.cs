using Avalonia;

namespace Md3.Avalonia.Motion;

/// <summary>Describes whether a motion changes geometry or only a visual effect.</summary>
public enum MdMotionKind
{
    /// <summary>Position, size, rotation and corner-shape motion.</summary>
    Spatial,

    /// <summary>Opacity, color, elevation and other non-geometric effects.</summary>
    Effects
}

/// <summary>Material motion speed tier.</summary>
public enum MdMotionSpeed
{
    Fast,
    Default,
    Slow
}

/// <summary>A resolved motion token for a particular element and scheme.</summary>
public readonly record struct MdMotionSpec(
    MdMotionScheme Scheme,
    MdMotionKind Kind,
    MdMotionSpeed Speed,
    MdSpring Spring,
    TimeSpan Duration,
    bool IsEnabled)
{
    /// <summary>Creates an easing that samples this token's spring over its settling duration.</summary>
    public MdSpringEasing CreateEasing() => new(Spring, Duration, Scheme == MdMotionScheme.Reduced);
}

/// <summary>Inherited motion configuration and token resolver for a window or subtree.</summary>
public sealed class MdMotion : AvaloniaObject
{
    /// <summary>Application resource read by <c>MaterialTheme</c> at every top-level root.</summary>
    public const string SchemeResourceKey = "Md.Sys.Motion.Scheme";

    public static readonly AttachedProperty<MdMotionScheme> SchemeProperty =
        AvaloniaProperty.RegisterAttached<MdMotion, StyledElement, MdMotionScheme>(
            "Scheme", MdMotionScheme.Expressive, inherits: true);

    private MdMotion()
    {
    }

    public static MdMotionScheme GetScheme(AvaloniaObject element)
    {
        // Respect an explicit value anywhere in the inheritance chain first. When no subtree
        // override exists, use the application resource written by MdThemeManager so top-levels
        // created after a theme update still start with the selected scheme.
        for (AvaloniaObject? current = element; current is not null;
             current = (current as StyledElement)?.Parent)
        {
            if (current.IsSet(SchemeProperty)) return element.GetValue(SchemeProperty);
        }

        if (Application.Current?.Resources.TryGetValue(SchemeResourceKey, out var resource) == true &&
            resource is MdMotionScheme applicationScheme)
        {
            return applicationScheme;
        }

        return element.GetValue(SchemeProperty);
    }

    public static void SetScheme(AvaloniaObject element, MdMotionScheme value) => element.SetValue(SchemeProperty, value);

    /// <summary>Resolves the inherited scheme into one Material motion token.</summary>
    public static MdMotionSpec Resolve(AvaloniaObject element, MdMotionKind kind, MdMotionSpeed speed) =>
        Resolve(GetScheme(element), kind, speed);

    /// <summary>Resolves an explicit scheme into one Material motion token.</summary>
    public static MdMotionSpec Resolve(MdMotionScheme scheme, MdMotionKind kind, MdMotionSpeed speed)
    {
        if (scheme == MdMotionScheme.None)
        {
            return new MdMotionSpec(scheme, kind, speed, default, TimeSpan.Zero, false);
        }

        // Reduced motion keeps short effect fades but removes spatial travel, scale and morphing.
        if (scheme == MdMotionScheme.Reduced)
        {
            return kind == MdMotionKind.Spatial
                ? new MdMotionSpec(scheme, kind, speed, default, TimeSpan.Zero, false)
                : new MdMotionSpec(scheme, kind, speed, MdMotionTokens.FastEffects,
                    TimeSpan.FromMilliseconds(100), true);
        }

        var spring = kind == MdMotionKind.Effects
            ? speed switch
            {
                MdMotionSpeed.Fast => MdMotionTokens.FastEffects,
                MdMotionSpeed.Slow => MdMotionTokens.SlowEffects,
                _ => MdMotionTokens.DefaultEffects
            }
            : (scheme, speed) switch
            {
                (MdMotionScheme.Standard, MdMotionSpeed.Fast) => MdMotionTokens.StandardFastSpatial,
                (MdMotionScheme.Standard, MdMotionSpeed.Slow) => MdMotionTokens.StandardSlowSpatial,
                (MdMotionScheme.Standard, _) => MdMotionTokens.StandardDefaultSpatial,
                (_, MdMotionSpeed.Fast) => MdMotionTokens.ExpressiveFastSpatial,
                (_, MdMotionSpeed.Slow) => MdMotionTokens.ExpressiveSlowSpatial,
                _ => MdMotionTokens.ExpressiveDefaultSpatial
            };

        // Durations are the approximate 0.1% settling points of the frozen spring tokens. The
        // easing snaps exactly to one at the endpoint so a Transition never leaves a residual.
        var duration = kind == MdMotionKind.Effects
            ? speed switch
            {
                MdMotionSpeed.Fast => TimeSpan.FromMilliseconds(150),
                MdMotionSpeed.Slow => TimeSpan.FromMilliseconds(325),
                _ => TimeSpan.FromMilliseconds(230)
            }
            : (scheme, speed) switch
            {
                (MdMotionScheme.Standard, MdMotionSpeed.Fast) => TimeSpan.FromMilliseconds(225),
                (MdMotionScheme.Standard, MdMotionSpeed.Slow) => TimeSpan.FromMilliseconds(485),
                (MdMotionScheme.Standard, _) => TimeSpan.FromMilliseconds(320),
                (_, MdMotionSpeed.Fast) => TimeSpan.FromMilliseconds(360),
                (_, MdMotionSpeed.Slow) => TimeSpan.FromMilliseconds(600),
                _ => TimeSpan.FromMilliseconds(435)
            };

        return new MdMotionSpec(scheme, kind, speed, spring, duration, true);
    }

    /// <summary>
    /// Returns the time for which an element must remain present while its exit motion runs.
    /// Reduced motion keeps only the short effect fade; None removes the element immediately.
    /// </summary>
    public static TimeSpan GetExitDuration(
        AvaloniaObject element,
        MdMotionSpeed spatialSpeed = MdMotionSpeed.Default,
        MdMotionSpeed effectsSpeed = MdMotionSpeed.Default)
    {
        var spatial = Resolve(element, MdMotionKind.Spatial, spatialSpeed);
        var effects = Resolve(element, MdMotionKind.Effects, effectsSpeed);
        return spatial.Duration >= effects.Duration ? spatial.Duration : effects.Duration;
    }
}
