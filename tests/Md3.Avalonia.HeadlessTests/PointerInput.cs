using System;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Drives a real pointer at a control instead of raising its click event.
/// </summary>
/// <remarks>
/// <para>
/// Raising <c>Button.ClickEvent</c> skips hit testing, so it proves a handler is wired and
/// nothing else. It cannot see an occluding layer, a zero-opacity ancestor, a control pushed
/// outside its clip, or a render transform that moves the painted pixels away from the
/// coordinates the pointer is given. Every defect of the form "the user says it does not
/// respond to clicks" lives in exactly that blind spot.
/// </para>
/// <para>
/// Settling first is not optional. Avalonia's animation clock runs on wall time, so forcing
/// render-timer ticks does not fast-forward it: a popup surface measured straight after opening
/// sat at Opacity 0.101, and was still at 0.364 after 120 forced ticks. Clicking during that
/// window misses the target and lands on whatever is underneath, which produces a failure that
/// looks exactly like a product defect and is not one. <see cref="Settle"/> waits for the
/// target's transform and effective opacity to stop changing before anything is clicked.
/// </para>
/// </remarks>
internal static class PointerInput
{
    private static readonly TimeSpan SettleTimeout = TimeSpan.FromSeconds(5);

    /// <summary>Clicks <paramref name="target"/> with the real pointer, asserting it is reachable.</summary>
    internal static void Click(Control target)
    {
        var root = RootOf(target);
        var centre = Settle(target, root);

        var underCursor = root.GetVisualsAt(centre).ToList();
        Assert.True(
            underCursor.Any(visual => ReferenceEquals(visual, target)
                                      || target.GetSelfAndVisualDescendants().Contains(visual)),
            $"a real pointer at {centre} cannot reach {Describe(target)}: " +
            $"bounds {target.Bounds}, effective opacity {EffectiveOpacity(target):0.###}, " +
            $"topmost under the cursor is " +
            (underCursor.Count == 0
                ? "nothing at all"
                : string.Join(" / ", underCursor.Take(4).Select(Describe))));

        root.MouseDown(centre, MouseButton.Left, RawInputModifiers.None);
        root.MouseUp(centre, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary>
    /// Asserts a real pointer can reach <paramref name="target"/> without clicking it.
    /// </summary>
    internal static void AssertReachable(Control target)
    {
        var root = RootOf(target);
        var centre = Settle(target, root);
        var underCursor = root.GetVisualsAt(centre).ToList();
        Assert.True(
            underCursor.Any(visual => ReferenceEquals(visual, target)
                                      || target.GetSelfAndVisualDescendants().Contains(visual)),
            $"a real pointer at {centre} cannot reach {Describe(target)}; topmost under the " +
            $"cursor is {string.Join(" / ", underCursor.Take(4).Select(Describe))}");
    }

    /// <summary>
    /// Pumps until the target stops moving and stops fading, then returns its centre in
    /// <paramref name="root"/> coordinates.
    /// </summary>
    private static Point Settle(Control target, TopLevel root)
    {
        Dispatcher.UIThread.RunJobs();

        var deadline = DateTime.UtcNow + SettleTimeout;
        (Matrix Transform, double Opacity, Rect Bounds) previous = default;
        var stableSamples = 0;

        while (DateTime.UtcNow < deadline)
        {
            var sample = (target.TransformToVisual(root) ?? Matrix.Identity,
                          EffectiveOpacity(target),
                          target.Bounds);

            stableSamples = sample.Equals(previous) ? stableSamples + 1 : 0;
            previous = sample;

            // Two identical samples in a row: nothing is animating any more.
            if (stableSamples >= 2 && sample.Item2 > 0 && sample.Item3.Width > 0)
            {
                break;
            }

            Thread.Sleep(20);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
        }

        Assert.True(target.Bounds is { Width: > 0, Height: > 0 },
            $"{Describe(target)} never got a layout box, so there is nothing to click");
        Assert.True(EffectiveOpacity(target) > 0,
            $"{Describe(target)} settled fully transparent, so a pointer cannot reach it");

        var centre = target.TranslatePoint(
            new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), root);
        Assert.True(centre.HasValue,
            $"{Describe(target)} is not connected to {root.GetType().Name}, so its position " +
            "cannot be resolved");
        return centre!.Value;
    }

    private static TopLevel RootOf(Control target)
    {
        var root = target.GetSelfAndVisualAncestors().OfType<TopLevel>().FirstOrDefault();
        Assert.True(root is not null,
            $"{Describe(target)} is not attached to a top level, so no pointer can be sent to it");
        return root!;
    }

    private static double EffectiveOpacity(Visual visual) =>
        visual.GetSelfAndVisualAncestors().OfType<Visual>().Aggregate(1d, (acc, v) => acc * v.Opacity);

    private static string Describe(Visual visual) =>
        visual is StyledElement { Name: { Length: > 0 } name }
            ? $"{visual.GetType().Name}#{name}"
            : visual.GetType().Name;
}
