using Avalonia;
using Avalonia.Animation;

namespace Md3.Avalonia.Motion;

/// <summary>Creates Avalonia transitions from the inherited Material motion scheme.</summary>
public static class MdMotionTransitions
{
    public static DoubleTransition? CreateDouble(
        AvaloniaObject owner,
        AvaloniaProperty property,
        MdMotionKind kind,
        MdMotionSpeed speed = MdMotionSpeed.Default)
    {
        var spec = MdMotion.Resolve(owner, kind, speed);
        return spec.IsEnabled
            ? new DoubleTransition
            {
                Property = property,
                Duration = spec.Duration,
                Easing = spec.CreateEasing()
            }
            : null;
    }

    public static VectorTransition? CreateVector(
        AvaloniaObject owner,
        AvaloniaProperty property,
        MdMotionKind kind = MdMotionKind.Spatial,
        MdMotionSpeed speed = MdMotionSpeed.Default)
    {
        var spec = MdMotion.Resolve(owner, kind, speed);
        return spec.IsEnabled
            ? new VectorTransition
            {
                Property = property,
                Duration = spec.Duration,
                Easing = spec.CreateEasing()
            }
            : null;
    }

    public static BrushTransition? CreateBrush(
        AvaloniaObject owner,
        AvaloniaProperty property,
        MdMotionSpeed speed = MdMotionSpeed.Default)
    {
        var spec = MdMotion.Resolve(owner, MdMotionKind.Effects, speed);
        return spec.IsEnabled
            ? new BrushTransition
            {
                Property = property,
                Duration = spec.Duration,
                Easing = spec.CreateEasing()
            }
            : null;
    }

    public static CornerRadiusTransition? CreateCornerRadius(
        AvaloniaObject owner,
        AvaloniaProperty property,
        MdMotionSpeed speed = MdMotionSpeed.Fast)
    {
        var spec = MdMotion.Resolve(owner, MdMotionKind.Spatial, speed);
        return spec.IsEnabled
            ? new CornerRadiusTransition
            {
                Property = property,
                Duration = spec.Duration,
                Easing = spec.CreateEasing()
            }
            : null;
    }

    public static TransformOperationsTransition? CreateTransform(
        AvaloniaObject owner,
        AvaloniaProperty property,
        MdMotionSpeed speed = MdMotionSpeed.Default)
    {
        var spec = MdMotion.Resolve(owner, MdMotionKind.Spatial, speed);
        return spec.IsEnabled
            ? new TransformOperationsTransition
            {
                Property = property,
                Duration = spec.Duration,
                Easing = spec.CreateEasing()
            }
            : null;
    }

    public static Transitions? Collect(params ITransition?[] candidates)
    {
        var result = new Transitions();
        foreach (var candidate in candidates)
        {
            if (candidate is not null) result.Add(candidate);
        }
        return result.Count == 0 ? null : result;
    }
}
