using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A pull-to-refresh host inspired by Flutter's RefreshIndicator.</summary>
[PseudoClasses(":idle", ":pulling", ":armed", ":refreshing", ":reduced-motion", ":no-motion")]
public sealed class MdRefreshIndicator : ContentControl
{
    public static readonly StyledProperty<bool> IsRefreshingProperty =
        AvaloniaProperty.Register<MdRefreshIndicator, bool>(nameof(IsRefreshing),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<double> TriggerDistanceProperty =
        AvaloniaProperty.Register<MdRefreshIndicator, double>(nameof(TriggerDistance), 72);
    public static readonly StyledProperty<double> DisplacementProperty =
        AvaloniaProperty.Register<MdRefreshIndicator, double>(nameof(Displacement), 48);
    public static readonly StyledProperty<ICommand?> RefreshCommandProperty =
        AvaloniaProperty.Register<MdRefreshIndicator, ICommand?>(nameof(RefreshCommand));
    public static readonly StyledProperty<object?> RefreshCommandParameterProperty =
        AvaloniaProperty.Register<MdRefreshIndicator, object?>(nameof(RefreshCommandParameter));
    public static readonly DirectProperty<MdRefreshIndicator, double> PullOffsetProperty =
        AvaloniaProperty.RegisterDirect<MdRefreshIndicator, double>(nameof(PullOffset), host => host.PullOffset);
    public static readonly StyledProperty<string> PullInstructionProperty =
        AvaloniaProperty.Register<MdRefreshIndicator, string>(nameof(PullInstruction), "Pull to refresh");
    public static readonly StyledProperty<string> ReleaseInstructionProperty =
        AvaloniaProperty.Register<MdRefreshIndicator, string>(nameof(ReleaseInstruction), "Release to refresh");
    public static readonly StyledProperty<string> RefreshingTextProperty =
        AvaloniaProperty.Register<MdRefreshIndicator, string>(nameof(RefreshingText), "Refreshing…");
    public static readonly DirectProperty<MdRefreshIndicator, double> PullProgressProperty =
        AvaloniaProperty.RegisterDirect<MdRefreshIndicator, double>(nameof(PullProgress), host => host.PullProgress);
    public static readonly DirectProperty<MdRefreshIndicator, string> StateTextProperty =
        AvaloniaProperty.RegisterDirect<MdRefreshIndicator, string>(nameof(StateText), host => host.StateText);

    private double _pullOffset;
    private double _pullProgress;
    private string _stateText = "Pull to refresh";
    private Point _start;
    private bool _tracking;
    private bool _pulling;
    private CancellationTokenSource? _refreshCancellation;
    private Border? _indicatorHost;
    private Border? _indicatorSurface;
    private Control? _pullProgressPresenter;
    private Control? _refreshingPresenter;
    private readonly DispatcherTimer _settleCleanupTimer = new();

    static MdRefreshIndicator()
    {
        IsRefreshingProperty.Changed.AddClassHandler<MdRefreshIndicator>((indicator, _) => indicator.OnRefreshingChanged());
        PullInstructionProperty.Changed.AddClassHandler<MdRefreshIndicator>((indicator, _) => indicator.UpdatePseudoClasses());
        ReleaseInstructionProperty.Changed.AddClassHandler<MdRefreshIndicator>((indicator, _) => indicator.UpdatePseudoClasses());
        RefreshingTextProperty.Changed.AddClassHandler<MdRefreshIndicator>((indicator, _) => indicator.UpdatePseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdRefreshIndicator>((indicator, _) => indicator.UpdateMotion());
    }

    public MdRefreshIndicator()
    {
        _settleCleanupTimer.Tick += (_, _) =>
        {
            _settleCleanupTimer.Stop();
            if (_indicatorHost is not null) _indicatorHost.Transitions = null;
        };
        UpdatePseudoClasses();
    }

    public bool IsRefreshing { get => GetValue(IsRefreshingProperty); set => SetValue(IsRefreshingProperty, value); }
    public double TriggerDistance { get => GetValue(TriggerDistanceProperty); set => SetValue(TriggerDistanceProperty, value); }
    public double Displacement { get => GetValue(DisplacementProperty); set => SetValue(DisplacementProperty, value); }
    public ICommand? RefreshCommand { get => GetValue(RefreshCommandProperty); set => SetValue(RefreshCommandProperty, value); }
    public object? RefreshCommandParameter { get => GetValue(RefreshCommandParameterProperty); set => SetValue(RefreshCommandParameterProperty, value); }
    public string PullInstruction { get => GetValue(PullInstructionProperty); set => SetValue(PullInstructionProperty, value); }
    public string ReleaseInstruction { get => GetValue(ReleaseInstructionProperty); set => SetValue(ReleaseInstructionProperty, value); }
    public string RefreshingText { get => GetValue(RefreshingTextProperty); set => SetValue(RefreshingTextProperty, value); }
    public double PullOffset => _pullOffset;
    public double PullProgress => _pullProgress;
    public string StateText => _stateText;

    public event EventHandler? RefreshRequested;
    /// <summary>Optional async handler. It is canceled when the control detaches and completes the indicator automatically.</summary>
    public Func<CancellationToken, ValueTask>? RefreshHandler { get; set; }

    public void BeginRefresh() => _ = RequestRefreshAsync();

    public async ValueTask RequestRefreshAsync(CancellationToken cancellationToken = default)
    {
        if (IsRefreshing) return;
        _refreshCancellation?.Cancel(); _refreshCancellation?.Dispose();
        _refreshCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        SetCurrentValue(IsRefreshingProperty, true);
        var parameter = RefreshCommandParameter;
        if (RefreshCommand?.CanExecute(parameter) == true) RefreshCommand.Execute(parameter);
        RefreshRequested?.Invoke(this, EventArgs.Empty);
        if (RefreshHandler is null) return;
        try { await RefreshHandler(_refreshCancellation.Token); }
        catch (OperationCanceledException) when (_refreshCancellation.IsCancellationRequested) { }
        finally { if (!_refreshCancellation.IsCancellationRequested) CompleteRefresh(); }
    }

    public void CompleteRefresh() => SetCurrentValue(IsRefreshingProperty, false);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _indicatorHost = e.NameScope.Find<Border>("PART_IndicatorHost");
        _indicatorSurface = e.NameScope.Find<Border>("PART_IndicatorSurface");
        _pullProgressPresenter = e.NameScope.Find<Control>("PART_PullProgress");
        _refreshingPresenter = e.NameScope.Find<Control>("PART_Refreshing");
        UpdateMotion();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (IsRefreshing || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || !IsAtTop()) return;
        CancelSettleForDirectManipulation();
        _start = e.GetPosition(this);
        _tracking = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_tracking || IsRefreshing) return;
        var delta = e.GetPosition(this).Y - _start.Y;
        if (delta <= 0)
        {
            if (_pulling) SetPullOffset(0);
            return;
        }
        if (!IsAtTop()) return;
        _pulling = true;
        if (e.Pointer.Captured is null) e.Pointer.Capture(this);
        // Flutter applies increasing drag resistance as the indicator approaches its extent.
        var resisted = Math.Min(TriggerDistance * 1.45, Math.Sqrt(delta) * 9);
        SetPullOffset(resisted);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_tracking) return;
        _tracking = false;
        var armed = PullProgress >= 1;
        _pulling = false;
        EnableSettleMotion();
        if (armed) BeginRefresh();
        else SetPullOffset(0);
        e.Pointer.Capture(null);
        e.Handled = armed;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (!_tracking || IsRefreshing) return;
        _tracking = false;
        _pulling = false;
        EnableSettleMotion();
        SetPullOffset(0);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _refreshCancellation?.Cancel();
        _settleCleanupTimer.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private bool IsAtTop()
    {
        var scroller = this.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        return scroller is null || scroller.Offset.Y <= 0.5;
    }

    private void OnRefreshingChanged()
    {
        _tracking = false;
        _pulling = false;
        EnableSettleMotion();
        SetPullOffset(IsRefreshing ? Math.Max(0, Displacement) : 0);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_indicatorSurface is not null)
        {
            _indicatorSurface.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
        }
        foreach (var presenter in new[] { _pullProgressPresenter, _refreshingPresenter })
        {
            if (presenter is not null)
                presenter.Transitions = MdMotionTransitions.Collect(
                    MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        }
        if (scheme is MdMotionScheme.Reduced or MdMotionScheme.None && _indicatorHost is not null)
            _indicatorHost.Transitions = null;
    }

    private void EnableSettleMotion()
    {
        if (_indicatorHost is null) return;
        var spec = MdMotion.Resolve(this, MdMotionKind.Spatial, MdMotionSpeed.Fast);
        _indicatorHost.Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, HeightProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast));
        _settleCleanupTimer.Stop();
        if (spec.IsEnabled)
        {
            _settleCleanupTimer.Interval = spec.Duration + TimeSpan.FromMilliseconds(20);
            _settleCleanupTimer.Start();
        }
    }

    private void CancelSettleForDirectManipulation()
    {
        _settleCleanupTimer.Stop();
        if (_indicatorHost is not null) _indicatorHost.Transitions = null;
    }

    private void SetPullOffset(double value)
    {
        value = Math.Max(0, value);
        SetAndRaise(PullOffsetProperty, ref _pullOffset, value);
        var progress = Math.Clamp(value / Math.Max(1, TriggerDistance), 0, 1);
        SetAndRaise(PullProgressProperty, ref _pullProgress, progress);
        UpdatePseudoClasses();
    }

    private void UpdatePseudoClasses()
    {
        var armed = _pulling && PullProgress >= 1;
        PseudoClasses.Set(":refreshing", IsRefreshing);
        PseudoClasses.Set(":pulling", _pulling && !armed);
        PseudoClasses.Set(":armed", armed);
        PseudoClasses.Set(":idle", !IsRefreshing && !_pulling);
        var text = IsRefreshing ? RefreshingText : armed ? ReleaseInstruction : PullInstruction;
        SetAndRaise(StateTextProperty, ref _stateText, text);
    }
}
