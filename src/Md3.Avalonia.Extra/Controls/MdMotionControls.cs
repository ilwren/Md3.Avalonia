using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Media;
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
