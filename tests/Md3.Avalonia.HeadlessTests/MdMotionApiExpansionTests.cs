using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Covers the API and motion expansion surface: easing tokens, the new Dismissible threshold,
/// resize and cross-axis knobs, the Hero shuttle/placeholder/gesture protocol, Stepper builders,
/// the sheet controller and min-extent contract, and the container transform's elevation and
/// interruptible redirect.
/// </summary>
public sealed class MdMotionApiExpansionTests
{
    [Fact]
    public void Easing_Tokens_Solve_The_Material_Curves_At_Their_Endpoints()
    {
        var curves = new[]
        {
            MdEasing.Standard,
            MdEasing.StandardAccelerate,
            MdEasing.StandardDecelerate,
            MdEasing.Emphasized,
            MdEasing.EmphasizedAccelerate,
            MdEasing.Legacy
        };
        foreach (var easing in curves)
        {
            Assert.Equal(0d, easing.Ease(0), 6);
            Assert.Equal(1d, easing.Ease(1), 6);
            var previous = 0d;
            for (var step = 1; step <= 20; step++)
            {
                var value = easing.Ease(step / 20d);
                Assert.InRange(value, previous - 1e-9, 1d);
                previous = value;
            }
        }

        // The expressive era renamed the emphasized curves without changing them.
        Assert.Same(MdEasing.Emphasized, MdEasing.Expressive);
        Assert.Same(MdEasing.EmphasizedAccelerate, MdEasing.ExpressiveAccelerate);
        Assert.Same(MdEasing.EmphasizedDecelerate, MdEasing.ExpressiveDecelerate);

        var custom = new MdCubicBezierEasing(0.05, 0.7, 0.1, 1.0);
        Assert.Equal(0.05, custom.X1);
        Assert.Equal(0.7, custom.Y1);
        Assert.Equal(0.1, custom.X2);
        Assert.Equal(1.0, custom.Y2);
        Assert.Equal(0d, custom.Ease(0), 6);
        Assert.Equal(1d, custom.Ease(1), 6);
    }

    [Fact]
    public void Motion_Duration_Tiers_Follow_The_Token_Scale()
    {
        var tiers = new[]
        {
            MdMotionDurations.Short1, MdMotionDurations.Short2, MdMotionDurations.Short3, MdMotionDurations.Short4,
            MdMotionDurations.Medium1, MdMotionDurations.Medium2, MdMotionDurations.Medium3, MdMotionDurations.Medium4,
            MdMotionDurations.Long1, MdMotionDurations.Long2, MdMotionDurations.Long3, MdMotionDurations.Long4
        };
        for (var index = 1; index < tiers.Length; index++)
            Assert.True(tiers[index] > tiers[index - 1]);
        Assert.Equal(TimeSpan.FromMilliseconds(50), MdMotionDurations.Short1);
        Assert.Equal(TimeSpan.FromMilliseconds(600), MdMotionDurations.Long4);
    }

    [Fact]
    public void Rect_Tweens_Interpolate_The_Flight_Bounds()
    {
        var from = new Rect(0, 0, 100, 50);
        var to = new Rect(100, 100, 100, 50);

        var linearMid = MdRectTweens.Linear(from, to, 0.5);
        Assert.Equal(50, linearMid.X, 6);
        Assert.Equal(50, linearMid.Y, 6);
        Assert.Equal(from, MdRectTweens.Linear(from, to, 0));
        Assert.Equal(to, MdRectTweens.Linear(from, to, 1));

        Assert.Equal(from, MdRectTweens.Arc(from, to, 0));
        Assert.Equal(to, MdRectTweens.Arc(from, to, 1));
        Assert.NotEqual(MdRectTweens.Linear(from, to, 0.5), MdRectTweens.Arc(from, to, 0.5));
    }

    [AvaloniaFact]
    public void Dismissible_Vertical_Axis_Directions_Travel_The_Cross_Axis()
    {
        var dismissible = new MdDismissible
        {
            Width = 360,
            Height = 72,
            Content = "Item",
            Direction = MdDismissDirection.Vertical
        };
        using var scope = Show(dismissible, 420, 180);
        var runner = PrivateField<MdSpatialSpringRunner>(dismissible, "_offsetRunner");

        Assert.True(dismissible.Dismiss(MdDismissDirection.Up));
        runner.Advance(TimeSpan.FromMilliseconds(16));
        Assert.Equal(-72, runner.Target, 6);
        Assert.Equal(-72, dismissible.DragOffset, 6);
        Assert.Contains(":up", dismissible.Classes);

        dismissible.Restore();
        Assert.True(dismissible.Dismiss(MdDismissDirection.Down));
        runner.Advance(TimeSpan.FromMilliseconds(16));
        Assert.Equal(72, runner.Target, 6);
        Assert.Contains(":down", dismissible.Classes);

        // A horizontal surface refuses vertical directions.
        dismissible.Restore();
        dismissible.Direction = MdDismissDirection.Horizontal;
        Assert.False(dismissible.Dismiss(MdDismissDirection.Up));
        Assert.False(dismissible.IsDismissed);
    }

    [AvaloniaFact]
    public void Dismissible_UpdateOffset_Reports_Live_Direction_Offset_And_Progress()
    {
        var dismissible = new MdDismissible
        {
            Width = 360,
            Height = 72,
            Content = "Item",
            DismissThresholds = new Dictionary<MdDismissDirection, double>
            {
                [MdDismissDirection.StartToEnd] = 0.3
            },
            ResizeDuration = TimeSpan.FromMilliseconds(200),
            CrossAxisEndOffset = 0.25
        };
        using var scope = Show(dismissible, 420, 180);
        var runner = PrivateField<MdSpatialSpringRunner>(dismissible, "_offsetRunner");
        var updates = new List<MdDismissOffsetEventArgs>();
        dismissible.UpdateOffset += (_, args) => updates.Add(args);

        Assert.True(dismissible.Dismiss(MdDismissDirection.StartToEnd));
        for (var frame = 0; frame < 200 && runner.IsRunning; frame++)
            runner.Advance(TimeSpan.FromMilliseconds(16));

        Assert.NotEmpty(updates);
        Assert.All(updates, args =>
        {
            Assert.Equal(MdDismissDirection.StartToEnd, args.Direction);
            Assert.InRange(args.Progress, 0d, 1d);
            Assert.True(args.Offset >= 0);
        });
        Assert.Equal(360, dismissible.DragOffset, 6);
        Assert.Equal(1d, updates[^1].Progress, 6);

        // Cross-axis end offset shifts the root perpendicular to the dismissal axis and
        // Restore puts it back.
        var root = Part<Border>(dismissible, "PART_Root");
        var cross = Assert.IsType<TranslateTransform>(root.RenderTransform);
        Assert.Equal(0.25 * 72, cross.Y, 6);
        Assert.Equal(0, cross.X, 6);
        dismissible.Restore();
        Assert.Equal(0, cross.Y, 6);
    }

    [AvaloniaFact]
    public void Dismissible_Resize_Duration_Sizes_The_Height_Collapse()
    {
        var dismissible = new MdDismissible
        {
            Width = 360,
            Height = 72,
            Content = "Item",
            ResizeDuration = TimeSpan.FromMilliseconds(200)
        };
        using var scope = Show(dismissible, 420, 180);

        // With motion off the collapse is immediate and the resize duration is bypassed,
        // matching the reduced-motion contract.
        MdMotion.SetScheme(dismissible, MdMotionScheme.None);
        Assert.True(dismissible.Dismiss());
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(0d, dismissible.VisualHeight);
        Assert.DoesNotContain(":present", dismissible.Classes);

        dismissible.Restore();
        Dispatcher.UIThread.RunJobs();
        MdMotion.SetScheme(dismissible, MdMotionScheme.Standard);
        Assert.True(dismissible.Dismiss());

        // The height capture waits for the collapse to start; once it does, the explicit
        // ResizeDuration - not the motion duration - sizes the completion timer.
        var collapseDelay = PrivateField<DispatcherTimer>(dismissible, "_collapseDelay");
        Assert.True(collapseDelay.IsEnabled);
        var beginCollapse = typeof(MdDismissible).GetMethod(
            "BeginHeightCollapse",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        beginCollapse.Invoke(dismissible, null);
        Assert.Equal(72d, dismissible.VisualHeight);
        var completion = PrivateField<DispatcherTimer>(dismissible, "_collapseCompletion");
        Assert.True(completion.IsEnabled);
        Assert.Equal(TimeSpan.FromMilliseconds(200), completion.Interval);

        dismissible.Restore();
    }

    [AvaloniaFact]
    public void Dismissible_Thresholds_Per_Direction_Gate_The_Release()
    {
        var dismissible = new MdDismissible
        {
            Width = 360,
            Height = 72,
            Content = "Item"
        };
        using var scope = Show(dismissible, 420, 180);
        var start = dismissible.TranslatePoint(new Point(180, 36), scope.Window)!.Value;

        // 200 DIP over 360 clears the default 0.4 threshold. Repeated stationary moves bleed
        // the synthesized velocity away first, so the release is judged by the threshold alone
        // instead of sneaking through the fling fast path.
        var end = start + new Vector(200, 0);
        scope.Window.MouseMove(start, RawInputModifiers.None);
        scope.Window.MouseDown(start, MouseButton.Left, RawInputModifiers.None);
        scope.Window.MouseMove(end, RawInputModifiers.LeftMouseButton);
        for (var frame = 0; frame < 40; frame++)
            scope.Window.MouseMove(end, RawInputModifiers.LeftMouseButton);
        scope.Window.MouseUp(end, MouseButton.Left, RawInputModifiers.None);
        Assert.True(dismissible.IsDismissed);

        // The per-direction override raises the bar past the same drag.
        dismissible.Restore();
        dismissible.DismissThresholds = new Dictionary<MdDismissDirection, double>
        {
            [MdDismissDirection.StartToEnd] = 0.95
        };
        Dispatcher.UIThread.RunJobs();
        scope.Window.MouseMove(start, RawInputModifiers.None);
        scope.Window.MouseDown(start, MouseButton.Left, RawInputModifiers.None);
        scope.Window.MouseMove(end, RawInputModifiers.LeftMouseButton);
        for (var frame = 0; frame < 40; frame++)
            scope.Window.MouseMove(end, RawInputModifiers.LeftMouseButton);
        scope.Window.MouseUp(end, MouseButton.Left, RawInputModifiers.None);
        Assert.False(dismissible.IsDismissed);
    }

    [AvaloniaFact]
    public async Task Hero_Gesture_Gating_Shuttle_And_Placeholder_Follow_The_Hero_Protocol()
    {
        var source = new MdHero { Tag = "avatar", Width = 72, Height = 72, Content = new Border() };
        var destination = new MdHero { Tag = "avatar", Width = 128, Height = 128, Content = new Border() };
        Canvas.SetLeft(destination, 260);
        Canvas.SetTop(destination, 120);
        var canvas = new Canvas { Children = { source, destination } };
        var layers = new VisualLayerManager { EnableOverlayLayer = true, Child = canvas };
        using var scope = Show(layers, 520, 360);
        var overlay = Assert.IsType<OverlayLayer>(OverlayLayer.GetOverlayLayer(source));
        var baseline = overlay.Children.Count;

        // Gesture-driven flights need both heroes to opt in.
        source.TransitionOnUserGestures = true;
        Assert.False(await source.TransitionToAsync(destination, isUserGesture: true));
        destination.TransitionOnUserGestures = true;

        var gestureRequested = new List<bool>();
        source.TransitionRequested += (_, args) => gestureRequested.Add(args.IsUserGesture);
        var gestureFlight = source.TransitionToAsync(destination, isUserGesture: true).AsTask();
        Dispatcher.UIThread.RunJobs();
        Assert.True(source.IsTransitioning);
        Assert.True(gestureRequested[^1]);
        MdMotion.SetScheme(source, MdMotionScheme.None);
        Assert.False(await gestureFlight);
        MdMotion.SetScheme(source, MdMotionScheme.Reduced);

        // A shuttle builder replaces the bitmap cross-fade; the placeholder covers the source
        // slot for the length of the flight.
        MdHeroFlightShuttleContext? shuttleContext = null;
        var shuttle = new Border();
        MdHero? placeholderHero = null;
        source.FlightShuttleBuilder = context =>
        {
            shuttleContext = context;
            return shuttle;
        };
        source.PlaceholderBuilder = hero =>
        {
            placeholderHero = hero;
            return new Border();
        };
        var requested = new List<bool>();
        source.TransitionRequested += (_, args) => requested.Add(args.IsUserGesture);

        var flight = source.TransitionToAsync(destination).AsTask();
        Dispatcher.UIThread.RunJobs();
        Assert.NotNull(shuttleContext);
        Assert.Same(source, placeholderHero);
        Assert.True(shuttle.IsAttachedToVisualTree());
        Assert.True(overlay.Children.Count >= baseline + 2); // shuttle + placeholder
        Assert.False(requested[^1]);

        // The context drives the shuttle's bounds through the rect tween.
        Assert.Equal(shuttleContext!.SourceBounds, shuttleContext.CurrentBounds);
        shuttleContext.Progress = 1;
        var arrival = shuttleContext.CurrentBounds;
        Assert.Equal(shuttleContext.DestinationBounds.X, arrival.X, 6);
        Assert.Equal(shuttleContext.DestinationBounds.Y, arrival.Y, 6);
        Assert.Equal(shuttleContext.DestinationBounds.Width, arrival.Width, 6);
        Assert.Equal(shuttleContext.DestinationBounds.Height, arrival.Height, 6);

        Assert.True(await flight);
        Assert.Equal(baseline, overlay.Children.Count);
    }

    [AvaloniaFact]
    public void Stepper_Builders_And_Connector_Replace_The_Stock_Visuals()
    {
        var contexts = new List<MdStepControlsContext>();
        var iconCalls = new List<(MdStep Step, int Index)>();
        var customControls = new List<Control>();
        var customIcons = new List<Control>();
        var steps = new[] { Step("One"), Step("Two"), Step("Three") };
        var stepper = MakeStepper(Orientation.Vertical, steps);
        stepper.ConnectorBrush = Brushes.Red;
        stepper.ConnectorThickness = 4;
        stepper.ControlsBuilder = context =>
        {
            contexts.Add(context);
            var control = new Button { Content = "Custom" };
            customControls.Add(control);
            return control;
        };
        stepper.StepIconBuilder = context =>
        {
            iconCalls.Add((context.Step, context.StepIndex));
            var icon = new Border { Width = 18, Height = 18 };
            customIcons.Add(icon);
            return icon;
        };
        using var host = Show(stepper);

        // Connector brush and thickness land on the step's connector part.
        var connector = steps[0].GetVisualDescendants().OfType<Border>()
            .Single(control => control.Name == "PART_Connector");
        Assert.Same(Brushes.Red, connector.Background);
        Assert.Equal(4, connector.Width);

        // The controls builder replaces the default continue/cancel row for the active step.
        var context = contexts.Last();
        Assert.Equal(0, context.StepIndex);
        Assert.Equal(3, context.StepCount);
        Assert.True(context.CanContinue);
        Assert.False(context.CanCancel);
        Assert.False(Part<Button>(stepper, "PART_ContinueButton").IsVisible);
        Assert.NotNull(customControls.Last().Parent);
        Assert.Same(customControls.Last().Parent, Part<Button>(stepper, "PART_ContinueButton").Parent);

        // The icon builder swaps the indicator contents and hides the stock visuals.
        for (var index = 0; index < steps.Length; index++)
        {
            var lastCall = iconCalls.FindLastIndex(call => call.Index == index);
            Assert.True(lastCall >= 0);
            Assert.Same(steps[index], iconCalls[lastCall].Step);
            var indicator = steps[index].GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_Indicator");
            Assert.Contains(customIcons[lastCall], indicator.GetVisualDescendants());
            var number = steps[index].GetVisualDescendants().OfType<Control>()
                .Single(control => control.Name == "PART_StepNumber");
            Assert.False(number.IsVisible);
        }

        // Navigation re-runs both builders for the new active step.
        contexts.Clear();
        iconCalls.Clear();
        stepper.Next();
        Dispatcher.UIThread.RunJobs();
        var next = contexts.Last();
        Assert.Equal(1, next.StepIndex);
        Assert.True(next.CanCancel);
        Assert.All(Enumerable.Range(0, steps.Length), index =>
            Assert.Contains(iconCalls, call => call.Index == index && ReferenceEquals(call.Step, steps[index])));

        // The context methods drive the stepper like the stock buttons do.
        next.Continue();
        Assert.Equal(2, stepper.ActiveStep);
    }

    [AvaloniaFact]
    public void Sheet_Controller_MinExtent_Contract_And_Snap_Duration_Extend_The_Surface()
    {
        var sheet = new MdDraggableScrollableSheet
        {
            Width = 480,
            Height = 600,
            MinimumExtent = 0.2,
            InitialExtent = 0.4,
            MaximumExtent = 1,
            SnapSizes = [0.2, 0.5, 1],
            Header = "Drag",
            Content = new Border()
        };
        using var scope = Show(sheet, 520, 640);
        var surface = Part<Border>(sheet, "PART_Surface");

        // The controller mirrors Flutter's DraggableScrollableController.
        var controller = new MdDraggableScrollableController();
        Assert.False(controller.IsAttached);
        sheet.Controller = controller;
        Assert.True(controller.IsAttached);
        Assert.Equal(0.5, controller.CurrentExtent, 6);
        controller.JumpTo(0.3);
        Assert.Equal(0.3, controller.CurrentExtent, 6);
        controller.AnimateTo(0.75);
        Assert.Equal(0.75, controller.CurrentExtent, 6);
        controller.Reset();
        Assert.Equal(0.4, controller.CurrentExtent, 6);
        sheet.Controller = null;
        Assert.False(controller.IsAttached);

        // An explicit snap duration replaces the motion duration on the surface transition.
        MdMotion.SetScheme(sheet, MdMotionScheme.Standard);
        sheet.SnapAnimationDuration = TimeSpan.FromMilliseconds(120);
        Dispatcher.UIThread.RunJobs();
        var transition = Assert.IsType<DoubleTransition>(Assert.Single(surface.Transitions!));
        Assert.Equal(TimeSpan.FromMilliseconds(120), transition.Duration);

        // A drag settling at the minimum extent reports MinExtentReached while opted in.
        var reached = 0;
        sheet.MinExtentReached += (_, _) => reached++;
        DragHandleToMinimum(sheet, scope.Window);
        Assert.Equal(0.2, sheet.Extent, 3);
        Assert.Equal(1, reached);

        // The opt-out keeps the same settle silent.
        sheet.ShouldCloseOnMinExtent = false;
        sheet.JumpTo(0.5);
        DragHandleToMinimum(sheet, scope.Window);
        Assert.Equal(1, reached);
    }

    [AvaloniaFact]
    public void Container_Transform_Redirects_MidFlight_And_Interpolates_Elevation()
    {
        var transform = new MdContainerTransform
        {
            ClosedContent = new Border { Width = 120, Height = 60 },
            OpenContent = new Border { Width = 320, Height = 200 },
            ClosedElevation = 2,
            OpenElevation = 8,
            Duration = TimeSpan.FromMilliseconds(300),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        MdMotion.SetScheme(transform, MdMotionScheme.None);
        using var scope = Show(transform);
        var morph = Part<MdMorphPanel>(transform, "PART_Morph");
        var container = Part<Border>(transform, "PART_Container");

        // The shadow derives from the interpolated elevation.
        Assert.Equal(1, container.BoxShadow.Count);
        var closedShadow = container.BoxShadow[0];
        Assert.Equal(2 * 0.5, closedShadow.OffsetY, 6);
        Assert.Equal(2 * 2 + 2, closedShadow.Blur, 6);
        transform.Toggle();
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, container.BoxShadow.Count);
        var openShadow = container.BoxShadow[0];
        Assert.Equal(8 * 0.5, openShadow.OffsetY, 6);
        Assert.Equal(8 * 2 + 2, openShadow.Blur, 6);

        transform.OpenElevation = 0;
        Assert.Equal(0, container.BoxShadow.Count);

        // A mid-flight redirect keeps the current progress and scales the travel by what is
        // left instead of restarting a full-length animation. The progress is parked while the
        // motion scheme is off so no transition is running underneath it.
        transform.IsExpanded = false;
        Dispatcher.UIThread.RunJobs();
        morph.SetValue(MdMorphPanel.ProgressProperty, 0.4);
        Assert.Equal(0.4, morph.Progress, 6);
        MdMotion.SetScheme(transform, MdMotionScheme.Standard);
        transform.OpenElevation = 8;
        transform.IsExpanded = true;
        var travel = TimeSpan.FromMilliseconds(300 * 0.6);
        var morphTransition = Assert.IsType<DoubleTransition>(Assert.Single(morph.Transitions!));
        Assert.Equal(travel, morphTransition.Duration);

        // The content cross-fade splits the redirected travel 30/70.
        var closed = Part<ContentPresenter>(transform, "PART_ClosedContent");
        var open = Part<ContentPresenter>(transform, "PART_OpenContent");
        var exit = Assert.IsType<DoubleTransition>(Assert.Single(closed.Transitions!));
        var enter = Assert.IsType<DoubleTransition>(Assert.Single(open.Transitions!));
        Assert.Equal(TimeSpan.FromMilliseconds(300 * 0.6 * 0.3), exit.Duration);
        Assert.Equal(TimeSpan.FromMilliseconds(300 * 0.6 * 0.3), enter.Delay);
        Assert.Equal(TimeSpan.FromMilliseconds(300 * 0.6 * 0.7), enter.Duration);
    }

    [AvaloniaFact]
    public void ContentPhaser_Snapshots_The_Outgoing_Control_Without_Reparenting_It()
    {
        var original = new Border { Width = 100, Height = 50, Background = Brushes.Green };
        var fade = new MdFadeThrough { Content = original, Duration = TimeSpan.FromMilliseconds(200) };
        MdMotion.SetScheme(fade, MdMotionScheme.Standard);
        using var scope = Show(fade, 300, 200);

        var replacement = new Border { Width = 120, Height = 60, Background = Brushes.Blue };
        fade.Content = replacement;
        Dispatcher.UIThread.RunJobs();

        var outgoing = Part<ContentPresenter>(fade, "PART_OutgoingContent");
        var image = Assert.IsType<Image>(outgoing.Content);
        Assert.IsType<RenderTargetBitmap>(image.Source);
        Assert.Equal(100, image.Width);
        Assert.Equal(50, image.Height);

        // The live control is never moved into the retained presenter; only its snapshot is.
        Assert.DoesNotContain(original, outgoing.GetVisualDescendants());
        var primary = Part<ContentPresenter>(fade, "PART_Content");
        Assert.Contains(replacement, primary.GetVisualDescendants());
    }

    [AvaloniaFact]
    public void Visual_Snapshot_Captures_Rooted_Controls_Only()
    {
        Assert.Null(MdVisualSnapshot.TryCapture(new Border { Width = 10, Height = 10 }));

        var control = new Border { Width = 40, Height = 20, Background = Brushes.Red };
        using var scope = Show(control, 120, 80);
        var snapshot = MdVisualSnapshot.TryCapture(control);
        Assert.NotNull(snapshot);
        Assert.Equal(40, snapshot!.PixelSize.Width);
        Assert.Equal(20, snapshot.PixelSize.Height);
        snapshot.Dispose();
    }

    private static void DragHandleToMinimum(MdDraggableScrollableSheet sheet, Window window)
    {
        var handle = sheet.GetVisualDescendants().OfType<Control>()
            .Single(control => control.Name == "PART_DragHandle");
        var start = handle.TranslatePoint(
            new Point(handle.Bounds.Width / 2, handle.Bounds.Height / 2), window)!.Value;
        var end = start + new Vector(0, 400);
        window.MouseMove(start, RawInputModifiers.None);
        window.MouseDown(start, MouseButton.Left, RawInputModifiers.None);
        window.MouseMove(end, RawInputModifiers.LeftMouseButton);
        window.MouseUp(end, MouseButton.Left, RawInputModifiers.None);
    }

    private static MdStepper MakeStepper(Orientation orientation, params MdStep[] steps)
    {
        var stepper = new MdStepper { Orientation = orientation };
        foreach (var step in steps)
            stepper.Items.Add(step);
        return stepper;
    }

    private static MdStep Step(string header, object? content = null) => new() { Header = header, Content = content };

    private static T Part<T>(Control owner, string name) where T : Control =>
        owner.GetVisualDescendants().OfType<T>().Single(control => control.Name == name);

    private static T PrivateField<T>(object owner, string fieldName) where T : class =>
        Assert.IsType<T>(owner.GetType()
            .GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(owner));

    private static Scope Show(Control content, double width = 760, double height = 560)
    {
        var window = new Window { Width = width, Height = height, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new Scope(window);
    }

    private sealed class Scope(Window window) : IDisposable
    {
        public Window Window { get; } = window;
        public void Dispose() => Window.Close();
    }
}
