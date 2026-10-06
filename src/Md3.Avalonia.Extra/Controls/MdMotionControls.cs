using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Extra.Controls;

public enum MdSkeletonShape { Rectangle, RoundedRectangle, Circle, Text }

/// <summary>Coordinates loading and motion state for a set of skeleton placeholders.</summary>
public sealed class MdSkeletonGroup : StackPanel
{
    public static readonly StyledProperty<bool> IsLoadingProperty = AvaloniaProperty.Register<MdSkeletonGroup, bool>(nameof(IsLoading), true);
    public static readonly StyledProperty<bool> IsAnimationEnabledProperty = AvaloniaProperty.Register<MdSkeletonGroup, bool>(nameof(IsAnimationEnabled), true);
    static MdSkeletonGroup()
    {
        IsLoadingProperty.Changed.AddClassHandler<MdSkeletonGroup>((group, _) => group.ApplyState());
        IsAnimationEnabledProperty.Changed.AddClassHandler<MdSkeletonGroup>((group, _) => group.ApplyState());
    }
    public MdSkeletonGroup() => Children.CollectionChanged += (_, _) => ApplyState();
    public bool IsLoading { get => GetValue(IsLoadingProperty); set => SetValue(IsLoadingProperty, value); }
    public bool IsAnimationEnabled { get => GetValue(IsAnimationEnabledProperty); set => SetValue(IsAnimationEnabledProperty, value); }
    public void ApplyState()
    {
        foreach (var skeleton in Children.OfType<MdSkeleton>()) { skeleton.IsLoading = IsLoading; skeleton.IsAnimationEnabled = IsAnimationEnabled; }
    }
}

/// <summary>Material skeleton placeholder that preserves content size and respects reduced-motion through IsAnimationEnabled.</summary>
[PseudoClasses(":loading", ":loaded", ":circle", ":text")]
public sealed class MdSkeleton : ContentControl
{
    public static readonly StyledProperty<bool> IsLoadingProperty = AvaloniaProperty.Register<MdSkeleton, bool>(nameof(IsLoading), true);
    public static readonly StyledProperty<MdSkeletonShape> ShapeProperty = AvaloniaProperty.Register<MdSkeleton, MdSkeletonShape>(nameof(Shape), MdSkeletonShape.RoundedRectangle);
    public static readonly StyledProperty<bool> IsAnimationEnabledProperty = AvaloniaProperty.Register<MdSkeleton, bool>(nameof(IsAnimationEnabled), true);
    public static readonly StyledProperty<IBrush?> BaseBrushProperty = AvaloniaProperty.Register<MdSkeleton, IBrush?>(nameof(BaseBrush));
    public static readonly StyledProperty<IBrush?> HighlightBrushProperty = AvaloniaProperty.Register<MdSkeleton, IBrush?>(nameof(HighlightBrush));
    public static readonly DirectProperty<MdSkeleton, double> PulseOpacityProperty = AvaloniaProperty.RegisterDirect<MdSkeleton, double>(nameof(PulseOpacity), control => control.PulseOpacity);
    private readonly DispatcherTimer _pulseTimer = new() { Interval = TimeSpan.FromMilliseconds(33) };
    private readonly System.Diagnostics.Stopwatch _pulseClock = new();
    private double _pulseOpacity = .72;
    static MdSkeleton()
    {
        IsLoadingProperty.Changed.AddClassHandler<MdSkeleton>((control, _) => control.UpdateState());
        IsAnimationEnabledProperty.Changed.AddClassHandler<MdSkeleton>((control, _) => control.UpdateAnimation());
        ShapeProperty.Changed.AddClassHandler<MdSkeleton>((control, _) => control.UpdateState());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdSkeleton>((control, _) => control.UpdateAnimation());
    }
    public MdSkeleton()
    {
        _pulseTimer.Tick += (_, _) =>
        {
            var phase = (_pulseClock.Elapsed.TotalMilliseconds % 1400) / 1400 * Math.PI * 2;
            SetAndRaise(PulseOpacityProperty, ref _pulseOpacity, .72 + Math.Sin(phase) * .18);
        };
        UpdateState();
    }
    public bool IsLoading { get => GetValue(IsLoadingProperty); set => SetValue(IsLoadingProperty, value); }
    public MdSkeletonShape Shape { get => GetValue(ShapeProperty); set => SetValue(ShapeProperty, value); }
    public bool IsAnimationEnabled { get => GetValue(IsAnimationEnabledProperty); set => SetValue(IsAnimationEnabledProperty, value); }
    public IBrush? BaseBrush { get => GetValue(BaseBrushProperty); set => SetValue(BaseBrushProperty, value); }
    public IBrush? HighlightBrush { get => GetValue(HighlightBrushProperty); set => SetValue(HighlightBrushProperty, value); }
    public double PulseOpacity => _pulseOpacity;
    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e) { base.OnAttachedToVisualTree(e); UpdateAnimation(); }
    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e) { _pulseTimer.Stop(); _pulseClock.Stop(); base.OnDetachedFromVisualTree(e); }
    private void UpdateState()
    {
        PseudoClasses.Set(":loading", IsLoading); PseudoClasses.Set(":loaded", !IsLoading);
        PseudoClasses.Set(":circle", Shape == MdSkeletonShape.Circle); PseudoClasses.Set(":text", Shape == MdSkeletonShape.Text);
        UpdateAnimation();
    }
    private void UpdateAnimation()
    {
        _pulseTimer.Stop(); _pulseClock.Stop();
        var allowsAmbientMotion = MdMotion.GetScheme(this) is MdMotionScheme.Expressive or MdMotionScheme.Standard;
        if (IsLoading && IsAnimationEnabled && allowsAmbientMotion && this.IsAttachedToVisualTree()) { _pulseClock.Restart(); _pulseTimer.Start(); }
        else SetAndRaise(PulseOpacityProperty, ref _pulseOpacity, 1);
    }
}

/// <summary>Staggers child entrance/exit with cancelable awaitable sequencing and real opacity transitions.</summary>
[PseudoClasses(":playing")]
public sealed class MdAnimationSequence : StackPanel
{
    public static readonly StyledProperty<TimeSpan> ItemDelayProperty = AvaloniaProperty.Register<MdAnimationSequence, TimeSpan>(nameof(ItemDelay), TimeSpan.FromMilliseconds(60));
    public static readonly StyledProperty<TimeSpan> ItemDurationProperty = AvaloniaProperty.Register<MdAnimationSequence, TimeSpan>(nameof(ItemDuration), TimeSpan.FromMilliseconds(220));
    public static readonly StyledProperty<bool> AutoPlayProperty = AvaloniaProperty.Register<MdAnimationSequence, bool>(nameof(AutoPlay), true);
    public static readonly StyledProperty<bool> ReverseProperty = AvaloniaProperty.Register<MdAnimationSequence, bool>(nameof(Reverse));
    public static readonly StyledProperty<bool> IsAnimationEnabledProperty = AvaloniaProperty.Register<MdAnimationSequence, bool>(nameof(IsAnimationEnabled), true);
    public static readonly DirectProperty<MdAnimationSequence, bool> IsPlayingProperty = AvaloniaProperty.RegisterDirect<MdAnimationSequence, bool>(nameof(IsPlaying), control => control.IsPlaying);
    private bool _isPlaying;
    private CancellationTokenSource? _cancellation;
    static MdAnimationSequence() =>
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdAnimationSequence>((sequence, _) => sequence.OnMotionSchemeChanged());
    public TimeSpan ItemDelay { get => GetValue(ItemDelayProperty); set => SetValue(ItemDelayProperty, value); }
    public TimeSpan ItemDuration { get => GetValue(ItemDurationProperty); set => SetValue(ItemDurationProperty, value); }
    public bool AutoPlay { get => GetValue(AutoPlayProperty); set => SetValue(AutoPlayProperty, value); }
    public bool Reverse { get => GetValue(ReverseProperty); set => SetValue(ReverseProperty, value); }
    public bool IsAnimationEnabled { get => GetValue(IsAnimationEnabledProperty); set => SetValue(IsAnimationEnabledProperty, value); }
    public bool IsPlaying { get => _isPlaying; private set { SetAndRaise(IsPlayingProperty, ref _isPlaying, value); PseudoClasses.Set(":playing", value); } }
    public event EventHandler? Completed;

    public async ValueTask PlayAsync(CancellationToken cancellationToken = default)
    {
        var previous = _cancellation;
        _cancellation = null;
        previous?.Cancel();
        var operation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _cancellation = operation;
        var token = operation.Token;
        IsPlaying = true;
        try
        {
            var children = Reverse ? Children.Reverse().ToArray() : Children.ToArray();
            var scheme = MdMotion.GetScheme(this);
            var spec = MdMotion.Resolve(this, MdMotionKind.Effects, MdMotionSpeed.Default);
            if (!IsAnimationEnabled || !spec.IsEnabled)
            {
                foreach (var child in children) { child.Transitions = null; child.Opacity = 1; }
                Completed?.Invoke(this, EventArgs.Empty);
                return;
            }

            var duration = scheme == MdMotionScheme.Reduced
                ? spec.Duration
                : ItemDuration <= TimeSpan.Zero ? spec.Duration : ItemDuration;
            var delay = scheme == MdMotionScheme.Reduced ? TimeSpan.Zero : ItemDelay;
            foreach (var child in children)
            {
                var transition = MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects);
                if (transition is not null) transition.Duration = duration;
                child.Transitions = MdMotionTransitions.Collect(transition);
                child.Opacity = 0;
            }
            await Task.Yield();
            foreach (var child in children)
            {
                token.ThrowIfCancellationRequested();
                child.Opacity = 1;
                if (delay > TimeSpan.Zero) await Task.Delay(delay, token);
            }
            if (duration > TimeSpan.Zero) await Task.Delay(duration, token);
            Completed?.Invoke(this, EventArgs.Empty);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        finally
        {
            if (ReferenceEquals(_cancellation, operation))
            {
                _cancellation = null;
                IsPlaying = false;
            }
            operation.Dispose();
        }
    }
    public void Stop(bool showAll = true)
    {
        _cancellation?.Cancel(); IsPlaying = false; if (showAll) foreach (var child in Children) child.Opacity = 1;
    }
    private void OnMotionSchemeChanged()
    {
        Stop();
        foreach (var child in Children)
        {
            child.Transitions = null;
            child.Opacity = 1;
        }
    }
    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e) { base.OnAttachedToVisualTree(e); if (AutoPlay) _ = PlayAsync(); }
    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e) { Stop(); base.OnDetachedFromVisualTree(e); }
}

public enum MdSharedAxisKind { X, Y, Z }

/// <summary>Axis an <see cref="MdRevealHost"/> grows and shrinks along.</summary>
public enum MdRevealAxis { Vertical, Horizontal, Both }

/// <summary>
/// Reveals its child by progressively granting it layout space along one axis while the child
/// keeps its full measured extent. Material expands and collapses a surface by clipping it, not
/// by squashing it, so text inside never reflows mid-transition. Animate <see cref="Fraction"/>.
/// </summary>
public sealed class MdRevealHost : Decorator
{
    public static readonly StyledProperty<double> FractionProperty =
        AvaloniaProperty.Register<MdRevealHost, double>(nameof(Fraction), 1d);

    public static readonly StyledProperty<MdRevealAxis> AxisProperty =
        AvaloniaProperty.Register<MdRevealHost, MdRevealAxis>(nameof(Axis), MdRevealAxis.Vertical);

    static MdRevealHost() => AffectsMeasure<MdRevealHost>(FractionProperty, AxisProperty);

    public MdRevealHost() => ClipToBounds = true;

    /// <summary>Revealed portion of the child's measured extent, from 0 to 1.</summary>
    public double Fraction { get => GetValue(FractionProperty); set => SetValue(FractionProperty, value); }

    /// <summary>Axis the reveal runs along.</summary>
    public MdRevealAxis Axis { get => GetValue(AxisProperty); set => SetValue(AxisProperty, value); }

    private bool RevealsWidth => Axis is MdRevealAxis.Horizontal or MdRevealAxis.Both;
    private bool RevealsHeight => Axis is MdRevealAxis.Vertical or MdRevealAxis.Both;

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Child is not { } child) return default;
        child.Measure(new Size(
            RevealsWidth ? double.PositiveInfinity : availableSize.Width,
            RevealsHeight ? double.PositiveInfinity : availableSize.Height));
        var desired = child.DesiredSize;
        var fraction = Math.Clamp(Fraction, 0d, 1d);
        return new Size(
            RevealsWidth ? desired.Width * fraction : desired.Width,
            RevealsHeight ? desired.Height * fraction : desired.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Child is not { } child) return finalSize;
        var desired = child.DesiredSize;
        child.Arrange(new Rect(0, 0,
            RevealsWidth ? Math.Max(finalSize.Width, desired.Width) : finalSize.Width,
            RevealsHeight ? Math.Max(finalSize.Height, desired.Height) : finalSize.Height));
        return finalSize;
    }
}

/// <summary>
/// Stacks two children and reports a size interpolated between their measured extents. This is
/// the bounds half of a Material container transform: the surface grows from the source's size
/// to the destination's size while the two are cross-faded. Animate <see cref="Progress"/>.
/// </summary>
public sealed class MdMorphPanel : Panel
{
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<MdMorphPanel, double>(nameof(Progress));

    static MdMorphPanel() => AffectsMeasure<MdMorphPanel>(ProgressProperty);

    public MdMorphPanel() => ClipToBounds = true;

    /// <summary>Position between the first child's extent (0) and the second child's extent (1).</summary>
    public double Progress { get => GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var child in Children) child.Measure(availableSize);
        if (Children.Count == 0) return default;
        if (Children.Count == 1) return Children[0].DesiredSize;

        var from = Children[0].DesiredSize;
        var to = Children[1].DesiredSize;
        var progress = Math.Clamp(Progress, 0d, 1d);
        return new Size(
            from.Width + (to.Width - from.Width) * progress,
            from.Height + (to.Height - from.Height) * progress);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var child in Children)
        {
            var desired = child.DesiredSize;
            child.Arrange(new Rect(0, 0,
                Math.Max(finalSize.Width, desired.Width),
                Math.Max(finalSize.Height, desired.Height)));
        }

        return finalSize;
    }
}

/// <summary>
/// Shared-axis content transition. Replacing <see cref="ContentControl.Content"/> slides and
/// fades the outgoing content out along <see cref="Axis"/> while the incoming content arrives
/// from the opposite side, which is the Material shared-axis choreography.
/// </summary>
[PseudoClasses(":x-axis", ":y-axis", ":z-axis", ":forward", ":backward")]
public class MdSharedAxis : ContentControl
{
    public static readonly StyledProperty<MdSharedAxisKind> AxisProperty =
        AvaloniaProperty.Register<MdSharedAxis, MdSharedAxisKind>(nameof(Axis), MdSharedAxisKind.X);

    public static readonly StyledProperty<bool> ForwardProperty =
        AvaloniaProperty.Register<MdSharedAxis, bool>(nameof(Forward), true);

    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<MdSharedAxis, TimeSpan>(nameof(Duration), TimeSpan.FromMilliseconds(300));

    /// <summary>Travel distance of the slide, in DIP.</summary>
    public const double SlideDistance = 30d;

    private readonly MdContentPhaser _phaser;

    static MdSharedAxis()
    {
        AxisProperty.Changed.AddClassHandler<MdSharedAxis>((control, _) => control.UpdatePseudoClasses());
        ForwardProperty.Changed.AddClassHandler<MdSharedAxis>((control, _) => control.UpdatePseudoClasses());
    }

    public MdSharedAxis()
    {
        _phaser = new MdContentPhaser(this, OutgoingTransform, IncomingTransform);
        UpdatePseudoClasses();
    }

    public MdSharedAxisKind Axis { get => GetValue(AxisProperty); set => SetValue(AxisProperty, value); }
    public bool Forward { get => GetValue(ForwardProperty); set => SetValue(ForwardProperty, value); }
    public TimeSpan Duration { get => GetValue(DurationProperty); set => SetValue(DurationProperty, value); }

    /// <summary>True while the outgoing content is still mounted for its exit phase.</summary>
    public bool IsTransitioning => _phaser.IsTransitioning;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _phaser.Attach(e.NameScope);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty) _phaser.Swap(change.OldValue, change.NewValue, Duration);
        else if (change.Property == ContentTemplateProperty) _phaser.SetTemplate(ContentTemplate);
    }

    private ITransform OutgoingTransform() => Shift(Forward ? -SlideDistance : SlideDistance, outgoing: true);
    private ITransform IncomingTransform() => Shift(Forward ? SlideDistance : -SlideDistance, outgoing: false);

    private ITransform Shift(double offset, bool outgoing) => Axis switch
    {
        MdSharedAxisKind.X => MdContentPhaser.Translate(offset, 0),
        MdSharedAxisKind.Y => MdContentPhaser.Translate(0, offset),
        // The Z axis scales through the surface instead of sliding across it.
        _ => MdContentPhaser.Scale(outgoing ? 1.1 : 0.8)
    };

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":x-axis", Axis == MdSharedAxisKind.X);
        PseudoClasses.Set(":y-axis", Axis == MdSharedAxisKind.Y);
        PseudoClasses.Set(":z-axis", Axis == MdSharedAxisKind.Z);
        PseudoClasses.Set(":forward", Forward);
        PseudoClasses.Set(":backward", !Forward);
    }
}

/// <summary>
/// Fade-through content transition. The outgoing content fades out over the first 30% of
/// <see cref="Duration"/>; the incoming content then fades and scales in over the remaining 70%,
/// which is Material's fade-through timing for unrelated content.
/// </summary>
public class MdFadeThrough : ContentControl
{
    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<MdFadeThrough, TimeSpan>(nameof(Duration), TimeSpan.FromMilliseconds(240));

    private readonly MdContentPhaser _phaser;

    public MdFadeThrough() =>
        _phaser = new MdContentPhaser(this, () => MdContentPhaser.Scale(1d), () => MdContentPhaser.Scale(0.92));

    public TimeSpan Duration { get => GetValue(DurationProperty); set => SetValue(DurationProperty, value); }

    /// <summary>True while the outgoing content is still mounted for its exit phase.</summary>
    public bool IsTransitioning => _phaser.IsTransitioning;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _phaser.Attach(e.NameScope);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty) _phaser.Swap(change.OldValue, change.NewValue, Duration);
        else if (change.Property == ContentTemplateProperty) _phaser.SetTemplate(ContentTemplate);
    }
}

/// <summary>
/// An open-container transition surface. Expanding morphs the container's bounds and corner
/// radius from the closed content's geometry to the open content's, cross-fading the two on
/// Material's 30/70 split. The outgoing content stays mounted until its fade completes.
/// </summary>
[PseudoClasses(":expanded", ":collapsed")]
public class MdContainerTransform : TemplatedControl
{
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<MdContainerTransform, bool>(nameof(IsExpanded), false, defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<object?> ClosedContentProperty =
        AvaloniaProperty.Register<MdContainerTransform, object?>(nameof(ClosedContent));

    public static readonly StyledProperty<object?> OpenContentProperty =
        AvaloniaProperty.Register<MdContainerTransform, object?>(nameof(OpenContent));

    public static readonly StyledProperty<CornerRadius> ClosedCornerRadiusProperty =
        AvaloniaProperty.Register<MdContainerTransform, CornerRadius>(nameof(ClosedCornerRadius), new CornerRadius(16));

    public static readonly StyledProperty<CornerRadius> OpenCornerRadiusProperty =
        AvaloniaProperty.Register<MdContainerTransform, CornerRadius>(nameof(OpenCornerRadius), new CornerRadius(28));

    public static readonly StyledProperty<IBrush?> ContainerBackgroundProperty =
        AvaloniaProperty.Register<MdContainerTransform, IBrush?>(nameof(ContainerBackground));

    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<MdContainerTransform, TimeSpan>(nameof(Duration), TimeSpan.FromMilliseconds(350));

    private Border? _container;
    private MdMorphPanel? _morph;
    private ContentPresenter? _closed;
    private ContentPresenter? _open;

    static MdContainerTransform()
    {
        IsExpandedProperty.Changed.AddClassHandler<MdContainerTransform>((ct, _) => ct.UpdateState(animate: true));
        DurationProperty.Changed.AddClassHandler<MdContainerTransform>((ct, _) => ct.UpdateState(animate: false));
    }

    public MdContainerTransform() => UpdateState(animate: false);

    public bool IsExpanded { get => GetValue(IsExpandedProperty); set => SetValue(IsExpandedProperty, value); }
    public object? ClosedContent { get => GetValue(ClosedContentProperty); set => SetValue(ClosedContentProperty, value); }
    public object? OpenContent { get => GetValue(OpenContentProperty); set => SetValue(OpenContentProperty, value); }
    public CornerRadius ClosedCornerRadius { get => GetValue(ClosedCornerRadiusProperty); set => SetValue(ClosedCornerRadiusProperty, value); }
    public CornerRadius OpenCornerRadius { get => GetValue(OpenCornerRadiusProperty); set => SetValue(OpenCornerRadiusProperty, value); }
    public IBrush? ContainerBackground { get => GetValue(ContainerBackgroundProperty); set => SetValue(ContainerBackgroundProperty, value); }
    public TimeSpan Duration { get => GetValue(DurationProperty); set => SetValue(DurationProperty, value); }

    public void Toggle() => IsExpanded = !IsExpanded;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _container = e.NameScope.Find<Border>("PART_Container");
        _morph = e.NameScope.Find<MdMorphPanel>("PART_Morph");
        _closed = e.NameScope.Find<ContentPresenter>("PART_ClosedContent");
        _open = e.NameScope.Find<ContentPresenter>("PART_OpenContent");
        UpdateState(animate: false);
    }

    private void UpdateState(bool animate)
    {
        PseudoClasses.Set(":expanded", IsExpanded);
        PseudoClasses.Set(":collapsed", !IsExpanded);
        if (_container is null || _morph is null || _closed is null || _open is null) return;

        var spec = MdMotion.Resolve(this, MdMotionKind.Spatial, MdMotionSpeed.Default);
        var total = animate && spec.IsEnabled && Duration > TimeSpan.Zero ? Duration : TimeSpan.Zero;
        var (exit, enter) = MdContentPhaser.SplitPhases(total);

        _container.Transitions = total > TimeSpan.Zero
            ? MdMotionTransitions.Collect(new CornerRadiusTransition
            {
                Property = Border.CornerRadiusProperty,
                Duration = total,
                Easing = spec.CreateEasing()
            })
            : null;
        _morph.Transitions = total > TimeSpan.Zero
            ? MdMotionTransitions.Collect(new DoubleTransition
            {
                Property = MdMorphPanel.ProgressProperty,
                Duration = total,
                Easing = spec.CreateEasing()
            })
            : null;

        var incoming = IsExpanded ? _open : _closed;
        var outgoing = IsExpanded ? _closed : _open;
        MdContentPhaser.ApplyCrossFade(incoming, outgoing, exit, enter, spec);

        _container.CornerRadius = IsExpanded ? OpenCornerRadius : ClosedCornerRadius;
        _morph.Progress = IsExpanded ? 1d : 0d;
    }
}

public enum MdVisibilityTransition
{
    Fade,
    ExpandVertical,
    ExpandHorizontal,
    Scale,
    SlideAndFade
}

/// <summary>
/// Animates its content in and out. Expanding transitions clip the content as the host grows, so
/// nothing reflows; the others fade, scale or slide. The content stays mounted for the whole exit
/// phase, so hiding never snaps.
/// </summary>
[PseudoClasses(":visible", ":hidden")]
public class MdAnimatedVisibility : ContentControl
{
    public static readonly StyledProperty<bool> IsContentVisibleProperty =
        AvaloniaProperty.Register<MdAnimatedVisibility, bool>(nameof(IsContentVisible), true, defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<MdVisibilityTransition> TransitionProperty =
        AvaloniaProperty.Register<MdAnimatedVisibility, MdVisibilityTransition>(nameof(Transition), MdVisibilityTransition.ExpandVertical);

    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<MdAnimatedVisibility, TimeSpan>(nameof(Duration), TimeSpan.FromMilliseconds(250));

    /// <summary>Travel distance of the slide-and-fade transition, in DIP.</summary>
    public const double SlideDistance = 16d;

    private readonly MdPresenceController _presence;
    private MdRevealHost? _reveal;
    private ContentPresenter? _content;

    static MdAnimatedVisibility()
    {
        IsContentVisibleProperty.Changed.AddClassHandler<MdAnimatedVisibility>((av, _) => av.UpdateState(animate: true));
        TransitionProperty.Changed.AddClassHandler<MdAnimatedVisibility>((av, _) => av.UpdateState(animate: false));
    }

    public MdAnimatedVisibility()
    {
        _presence = new MdPresenceController(present =>
        {
            if (_reveal is not null) _reveal.IsVisible = present;
        });
        _presence.Initialize(IsContentVisible);
        UpdateState(animate: false);
    }

    public bool IsContentVisible { get => GetValue(IsContentVisibleProperty); set => SetValue(IsContentVisibleProperty, value); }
    public MdVisibilityTransition Transition { get => GetValue(TransitionProperty); set => SetValue(TransitionProperty, value); }
    public TimeSpan Duration { get => GetValue(DurationProperty); set => SetValue(DurationProperty, value); }

    /// <summary>True while hidden content is still mounted for its exit transition.</summary>
    public bool IsContentPresent => _presence.IsPresent;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _reveal = e.NameScope.Find<MdRevealHost>("PART_Reveal");
        _content = e.NameScope.Find<ContentPresenter>("PART_Content");
        _presence.Initialize(IsContentVisible);
        UpdateState(animate: false);
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _presence.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private bool UsesReveal => Transition is MdVisibilityTransition.ExpandVertical or MdVisibilityTransition.ExpandHorizontal;

    private void UpdateState(bool animate)
    {
        PseudoClasses.Set(":visible", IsContentVisible);
        PseudoClasses.Set(":hidden", !IsContentVisible);
        if (_reveal is null || _content is null) return;

        var spec = MdMotion.Resolve(this, MdMotionKind.Spatial, MdMotionSpeed.Default);
        var duration = animate && spec.IsEnabled && Duration > TimeSpan.Zero ? Duration : TimeSpan.Zero;

        _reveal.Axis = Transition == MdVisibilityTransition.ExpandHorizontal
            ? MdRevealAxis.Horizontal
            : MdRevealAxis.Vertical;
        _reveal.Transitions = duration > TimeSpan.Zero && UsesReveal
            ? MdMotionTransitions.Collect(new DoubleTransition
            {
                Property = MdRevealHost.FractionProperty,
                Duration = duration,
                Easing = spec.CreateEasing()
            })
            : null;
        _content.Transitions = duration > TimeSpan.Zero
            ? MdMotionTransitions.Collect(
                new DoubleTransition { Property = OpacityProperty, Duration = duration, Easing = spec.CreateEasing() },
                new TransformOperationsTransition { Property = RenderTransformProperty, Duration = duration, Easing = spec.CreateEasing() })
            : null;

        // Presence is kept for the whole exit so the content is still mounted while it animates
        // away. Without it the host would collapse on the first frame and nothing would be seen.
        _presence.Update(IsContentVisible, duration);
        if (IsContentVisible) _reveal.IsVisible = true;

        _reveal.Fraction = !UsesReveal || IsContentVisible ? 1d : 0d;
        _content.Opacity = IsContentVisible ? 1d : 0d;
        _content.IsHitTestVisible = IsContentVisible;
        _content.RenderTransform = HiddenOffset(IsContentVisible);
    }

    private ITransform HiddenOffset(bool visible) => Transition switch
    {
        MdVisibilityTransition.Scale => MdContentPhaser.Scale(visible ? 1d : 0.8d),
        MdVisibilityTransition.SlideAndFade => MdContentPhaser.Translate(0, visible ? 0d : SlideDistance),
        _ => MdContentPhaser.Scale(1d)
    };
}

/// <summary>
/// Drives a two-presenter content swap: the outgoing value is moved into a retained presenter and
/// phased out while the incoming value phases in. Material splits the two phases 30/70 so the
/// surface is never showing two things at full strength at once.
/// </summary>
internal sealed class MdContentPhaser
{
    private readonly ContentControl _owner;
    private readonly Func<ITransform> _outgoingTransform;
    private readonly Func<ITransform> _incomingTransform;
    private readonly MdPresenceController _presence;
    private ContentPresenter? _primary;
    private ContentPresenter? _outgoing;

    internal MdContentPhaser(ContentControl owner, Func<ITransform> outgoingTransform, Func<ITransform> incomingTransform)
    {
        _owner = owner;
        _outgoingTransform = outgoingTransform;
        _incomingTransform = incomingTransform;
        _presence = new MdPresenceController(present =>
        {
            if (_outgoing is null) return;
            _outgoing.IsVisible = present;
            if (!present)
            {
                _outgoing.Content = null;
                _outgoing.UpdateChild();
            }
        });
    }

    internal bool IsTransitioning => _presence.IsPresent;

    /// <summary>Material's fade-through split: 30% for the exit, 70% for the entrance.</summary>
    internal static (TimeSpan Exit, TimeSpan Enter) SplitPhases(TimeSpan total) => total <= TimeSpan.Zero
        ? (TimeSpan.Zero, TimeSpan.Zero)
        : (TimeSpan.FromMilliseconds(total.TotalMilliseconds * 0.3),
           TimeSpan.FromMilliseconds(total.TotalMilliseconds * 0.7));

    internal static TransformOperations Translate(double x, double y)
    {
        var builder = new TransformOperations.Builder(1);
        builder.AppendTranslate(x, y);
        return builder.Build();
    }

    internal static TransformOperations Scale(double scale)
    {
        var builder = new TransformOperations.Builder(1);
        builder.AppendScale(scale, scale);
        return builder.Build();
    }

    internal static void ApplyCrossFade(
        ContentPresenter incoming,
        ContentPresenter outgoing,
        TimeSpan exit,
        TimeSpan enter,
        MdMotionSpec spec)
    {
        incoming.Transitions = enter > TimeSpan.Zero
            ? MdMotionTransitions.Collect(new DoubleTransition
            {
                Property = Visual.OpacityProperty,
                Duration = enter,
                Delay = exit,
                Easing = spec.CreateEasing()
            })
            : null;
        outgoing.Transitions = exit > TimeSpan.Zero
            ? MdMotionTransitions.Collect(new DoubleTransition
            {
                Property = Visual.OpacityProperty,
                Duration = exit,
                Easing = spec.CreateEasing()
            })
            : null;

        incoming.IsVisible = true;
        outgoing.IsVisible = true;
        incoming.IsHitTestVisible = true;
        outgoing.IsHitTestVisible = false;
        incoming.Opacity = 1d;
        outgoing.Opacity = 0d;
    }

    internal void Attach(INameScope nameScope)
    {
        _primary = nameScope.Find<ContentPresenter>("PART_Content");
        _outgoing = nameScope.Find<ContentPresenter>("PART_OutgoingContent");
        if (_primary is null) return;

        _primary.Opacity = 1d;
        _primary.RenderTransform = MdContentPhaser.Scale(1d);
        if (_outgoing is not null)
        {
            _outgoing.ContentTemplate = _owner.ContentTemplate;
            _outgoing.IsVisible = false;
            _outgoing.Opacity = 0d;
        }

        _presence.Initialize(false);
    }

    internal void SetTemplate(IDataTemplate? template)
    {
        if (_outgoing is not null) _outgoing.ContentTemplate = template;
    }

    internal void Swap(object? oldContent, object? newContent, TimeSpan duration)
    {
        _ = newContent;
        if (_primary is null) return;

        var spec = MdMotion.Resolve(_owner, MdMotionKind.Effects, MdMotionSpeed.Default);
        var total = spec.IsEnabled && duration > TimeSpan.Zero ? duration : TimeSpan.Zero;
        var (exit, enter) = SplitPhases(total);

        if (total <= TimeSpan.Zero)
        {
            _primary.Transitions = null;
            _primary.Opacity = 1d;
            _primary.RenderTransform = Scale(1d);
            _presence.Update(false, TimeSpan.Zero);
            return;
        }

        PhaseOutgoing(oldContent, exit, spec);

        // Jump the incoming content to its entry pose with no transition attached, then install
        // the delayed transition and set the resting pose. Assigning both in one pass would make
        // the presenter animate *from* wherever it happened to be.
        _primary.Transitions = null;
        _primary.Opacity = 0d;
        _primary.RenderTransform = _incomingTransform();
        _primary.Transitions = MdMotionTransitions.Collect(
            new DoubleTransition { Property = Visual.OpacityProperty, Duration = enter, Delay = exit, Easing = spec.CreateEasing() },
            new TransformOperationsTransition { Property = Visual.RenderTransformProperty, Duration = enter, Delay = exit, Easing = spec.CreateEasing() });
        _primary.Opacity = 1d;
        _primary.RenderTransform = Scale(1d);
    }

    /// <summary>
    /// Shows the previous value in the retained presenter for its exit phase.
    /// </summary>
    /// <remarks>
    /// Only values the outgoing presenter can realise independently are retained. A Control is
    /// already mounted in the primary presenter and a visual may have exactly one parent, so
    /// moving it would mean reparenting a live element mid-transition. The incoming phase still
    /// runs for Control content; what is skipped is the overlap, not the animation.
    /// </remarks>
    private void PhaseOutgoing(object? oldContent, TimeSpan exit, MdMotionSpec spec)
    {
        if (_outgoing is null || oldContent is null || oldContent is Control || exit <= TimeSpan.Zero)
        {
            _presence.Update(false, TimeSpan.Zero);
            return;
        }

        _outgoing.ContentTemplate = _owner.ContentTemplate;
        _outgoing.Content = oldContent;
        _outgoing.UpdateChild();
        _outgoing.IsVisible = true;
        _outgoing.IsHitTestVisible = false;
        _outgoing.Transitions = null;
        _outgoing.Opacity = 1d;
        _outgoing.RenderTransform = Scale(1d);
        _outgoing.Transitions = MdMotionTransitions.Collect(
            new DoubleTransition { Property = Visual.OpacityProperty, Duration = exit, Easing = spec.CreateEasing() },
            new TransformOperationsTransition { Property = Visual.RenderTransformProperty, Duration = exit, Easing = spec.CreateEasing() });
        _outgoing.Opacity = 0d;
        _outgoing.RenderTransform = _outgoingTransform();
        _presence.Update(true, exit);
        _presence.Update(false, exit);
    }

}
