using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Md3.Avalonia.Extra.Infrastructure;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>A start/end swipe-action container with RTL-aware thresholds and keyboard alternatives.</summary>
[PseudoClasses(":dragging", ":start-open", ":end-open", ":reduced-motion", ":no-motion")]
public sealed class MdSlidableItem : ContentControl
{
    public static readonly StyledProperty<object?> StartActionsProperty = AvaloniaProperty.Register<MdSlidableItem, object?>(nameof(StartActions));
    public static readonly StyledProperty<object?> EndActionsProperty = AvaloniaProperty.Register<MdSlidableItem, object?>(nameof(EndActions));
    public static readonly StyledProperty<double> OpenThresholdProperty = AvaloniaProperty.Register<MdSlidableItem, double>(nameof(OpenThreshold), 0.28);
    public static readonly StyledProperty<double> ActionExtentProperty = AvaloniaProperty.Register<MdSlidableItem, double>(nameof(ActionExtent), 144);
    public static readonly StyledProperty<double> DismissThresholdProperty = AvaloniaProperty.Register<MdSlidableItem, double>(nameof(DismissThreshold), 1.25);
    public static readonly StyledProperty<bool> CloseOnActionProperty = AvaloniaProperty.Register<MdSlidableItem, bool>(nameof(CloseOnAction), true);
    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<MdSlidableItem, bool>(nameof(IsOpen), false, defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<ICommand?> StartActionCommandProperty = AvaloniaProperty.Register<MdSlidableItem, ICommand?>(nameof(StartActionCommand));
    public static readonly StyledProperty<ICommand?> EndActionCommandProperty = AvaloniaProperty.Register<MdSlidableItem, ICommand?>(nameof(EndActionCommand));
    public static readonly DirectProperty<MdSlidableItem, double> OffsetProperty = AvaloniaProperty.RegisterDirect<MdSlidableItem, double>(nameof(Offset), item => item.Offset);
    public static readonly DirectProperty<MdSlidableItem, double> RevealProgressProperty = AvaloniaProperty.RegisterDirect<MdSlidableItem, double>(nameof(RevealProgress), item => item.RevealProgress);

    private Point _start;
    private double _startOffset;
    private double _offset;
    private double _revealProgress;
    private bool _dragging;
    private Border? _foreground;
    private TranslateTransform? _foregroundTransform;
    private Control? _startActionPresenter;
    private Control? _endActionPresenter;
    private double _lastPointerX;
    private ulong _lastPointerTimestamp;
    private double _horizontalVelocity;
    private bool _updatingOpenState;

    static MdSlidableItem()
    {
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdSlidableItem>((item, _) => item.UpdateMotion());
        IsOpenProperty.Changed.AddClassHandler<MdSlidableItem>((item, change) =>
        {
            if (item._updatingOpenState) return;
            if (change.NewValue is true && item.Offset == 0) item.OpenStart();
            else if (change.NewValue is false && item.Offset != 0) item.Close();
        });
    }

    public object? StartActions { get => GetValue(StartActionsProperty); set => SetValue(StartActionsProperty, value); }
    public object? EndActions { get => GetValue(EndActionsProperty); set => SetValue(EndActionsProperty, value); }
    public double OpenThreshold { get => GetValue(OpenThresholdProperty); set => SetValue(OpenThresholdProperty, value); }
    public double ActionExtent { get => GetValue(ActionExtentProperty); set => SetValue(ActionExtentProperty, value); }
    public double DismissThreshold { get => GetValue(DismissThresholdProperty); set => SetValue(DismissThresholdProperty, value); }
    public bool CloseOnAction { get => GetValue(CloseOnActionProperty); set => SetValue(CloseOnActionProperty, value); }
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public ICommand? StartActionCommand { get => GetValue(StartActionCommandProperty); set => SetValue(StartActionCommandProperty, value); }
    public ICommand? EndActionCommand { get => GetValue(EndActionCommandProperty); set => SetValue(EndActionCommandProperty, value); }
    public double Offset => _offset;
    public double RevealProgress => _revealProgress;
    public bool IsStartOpen => Offset > 0;
    public bool IsEndOpen => Offset < 0;

    public event EventHandler? Opened;
    public event EventHandler? Closed;
    public void OpenStart() { UpdateMotion(); SetOffset(Math.Max(0, ActionExtent)); SetOpenState(true); }
    public void OpenEnd() { UpdateMotion(); SetOffset(-Math.Max(0, ActionExtent)); SetOpenState(true); }
    public void Close() { UpdateMotion(); SetOffset(0); SetOpenState(false); Closed?.Invoke(this, EventArgs.Empty); }
    private void SetOpenState(bool value)
    {
        _updatingOpenState = true;
        try { SetCurrentValue(IsOpenProperty, value); }
        finally { _updatingOpenState = false; }
    }
    public void InvokeStart() { if (StartActionCommand?.CanExecute(DataContext) == true) StartActionCommand.Execute(DataContext); if (CloseOnAction) Close(); }
    public void InvokeEnd() { if (EndActionCommand?.CanExecute(DataContext) == true) EndActionCommand.Execute(DataContext); if (CloseOnAction) Close(); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _foreground = e.NameScope.Find<Border>("PART_Foreground");
        _foregroundTransform = _foreground?.RenderTransform as TranslateTransform;
        _startActionPresenter = e.NameScope.Find<Control>("PART_StartActions");
        _endActionPresenter = e.NameScope.Find<Control>("PART_EndActions");
        UpdateMotion();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _start = e.GetPosition(this); _startOffset = Offset; _dragging = true;
        _lastPointerX = _start.X; _lastPointerTimestamp = e.Timestamp; _horizontalVelocity = 0;
        if (_foregroundTransform is not null) _foregroundTransform.Transitions = null;
        if (_startActionPresenter is not null) _startActionPresenter.Transitions = null;
        if (_endActionPresenter is not null) _endActionPresenter.Transitions = null;
        PseudoClasses.Set(":dragging", true);
    }
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_dragging) return;
        var position = e.GetPosition(this);
        var delta = position.X - _start.X;
        var movement = position.X - _lastPointerX;
        if (FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft) { delta = -delta; movement = -movement; }
        if (Math.Abs(delta) < 4) return;
        var elapsed = e.Timestamp > _lastPointerTimestamp ? e.Timestamp - _lastPointerTimestamp : 1;
        _horizontalVelocity = movement / (double)elapsed;
        _lastPointerX = position.X; _lastPointerTimestamp = e.Timestamp;
        e.Pointer.Capture(this);
        var maxExtent = Math.Max(0, ActionExtent);
        var targetOffset = _startOffset + delta;
        if (targetOffset > maxExtent)
            targetOffset = maxExtent + (targetOffset - maxExtent) * 0.35;
        else if (targetOffset < -maxExtent)
            targetOffset = -maxExtent + (targetOffset + maxExtent) * 0.35;
        SetOffset(targetOffset);
        e.Handled = true;
    }
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_dragging) return;
        FinishDrag();
        e.Pointer.Capture(null); e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (_dragging) FinishDrag();
    }
    private void FinishDrag()
    {
        _dragging = false;
        PseudoClasses.Set(":dragging", false);
        UpdateMotion();
        var maxExtent = Math.Max(0, ActionExtent);
        var threshold = Math.Max(1, Bounds.Width) * Math.Clamp(OpenThreshold, 0.05, 0.9);
        var fling = Math.Abs(_horizontalVelocity) >= 0.5 && Math.Sign(_horizontalVelocity) == Math.Sign(Offset);

        if (Offset >= maxExtent * Math.Max(1, DismissThreshold) && StartActionCommand is not null)
        {
            InvokeStart();
            return;
        }
        if (Offset <= -maxExtent * Math.Max(1, DismissThreshold) && EndActionCommand is not null)
        {
            InvokeEnd();
            return;
        }

        if (Offset >= threshold || (fling && Offset > 0)) OpenStart();
        else if (Offset <= -threshold || (fling && Offset < 0)) OpenEnd();
        else Close();
        if (Offset != 0) Opened?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.Left) { OpenStart(); e.Handled = true; }
        else if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.Right) { OpenEnd(); e.Handled = true; }
        else if (e.Key == Key.Escape && Offset != 0) { Close(); e.Handled = true; }
        else base.OnKeyDown(e);
    }
    private void SetOffset(double value)
    {
        SetAndRaise(OffsetProperty, ref _offset, value);
        var progress = Math.Clamp(Math.Abs(value) / Math.Max(1, ActionExtent), 0, 1);
        SetAndRaise(RevealProgressProperty, ref _revealProgress, progress);
        PseudoClasses.Set(":start-open", value > 0.5);
        PseudoClasses.Set(":end-open", value < -0.5);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_foregroundTransform is not null)
        {
            _foregroundTransform.Transitions = _dragging
                ? null
                : MdMotionTransitions.Collect(
                    MdMotionTransitions.CreateDouble(this, TranslateTransform.XProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast));
        }
        foreach (var presenter in new[] { _startActionPresenter, _endActionPresenter })
        {
            if (presenter is not null)
            {
                presenter.Transitions = _dragging
                    ? null
                    : MdMotionTransitions.Collect(
                        MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
                        MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
            }
        }
    }
}

/// <summary>Provider-agnostic incremental paging view with loading, empty, error, retry, refresh, and completion states.</summary>
[PseudoClasses(":idle", ":loading", ":data", ":empty", ":error", ":completed")]
public sealed class MdPagedItemsView : TemplatedControl
{
    public static readonly StyledProperty<int> PageSizeProperty = AvaloniaProperty.Register<MdPagedItemsView, int>(nameof(PageSize), 20);
    public static readonly StyledProperty<bool> AutoLoadProperty = AvaloniaProperty.Register<MdPagedItemsView, bool>(nameof(AutoLoad), true);
    public static readonly StyledProperty<object?> EmptyContentProperty = AvaloniaProperty.Register<MdPagedItemsView, object?>(nameof(EmptyContent));
    /// <summary>Template for each loaded record. Without it every page renders as bare ToString text.</summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty = AvaloniaProperty.Register<MdPagedItemsView, IDataTemplate?>(nameof(ItemTemplate));
    public static readonly StyledProperty<object?> ErrorContentProperty = AvaloniaProperty.Register<MdPagedItemsView, object?>(nameof(ErrorContent));
    public static readonly StyledProperty<ICommand?> PageRequestCommandProperty = AvaloniaProperty.Register<MdPagedItemsView, ICommand?>(nameof(PageRequestCommand));
    public static readonly DirectProperty<MdPagedItemsView, IReadOnlyList<object?>> ItemsProperty = AvaloniaProperty.RegisterDirect<MdPagedItemsView, IReadOnlyList<object?>>(nameof(Items), view => view.Items);
    public static readonly DirectProperty<MdPagedItemsView, MdAsyncRequestState> StateProperty = AvaloniaProperty.RegisterDirect<MdPagedItemsView, MdAsyncRequestState>(nameof(State), view => view.State);
    public static readonly DirectProperty<MdPagedItemsView, Exception?> ErrorProperty = AvaloniaProperty.RegisterDirect<MdPagedItemsView, Exception?>(nameof(Error), view => view.Error);

    private readonly ObservableCollection<object?> _items = [];
    private MdAsyncRequestState _state;
    private Exception? _error;
    private CancellationTokenSource? _requestCancellation;
    private int _nextPageKey;
    private bool _lastPage;
    private Button? _retryButton;
    private Button? _loadMoreButton;

    public int PageSize { get => GetValue(PageSizeProperty); set => SetValue(PageSizeProperty, value); }
    public bool AutoLoad { get => GetValue(AutoLoadProperty); set => SetValue(AutoLoadProperty, value); }
    public object? EmptyContent { get => GetValue(EmptyContentProperty); set => SetValue(EmptyContentProperty, value); }
    public object? ErrorContent { get => GetValue(ErrorContentProperty); set => SetValue(ErrorContentProperty, value); }
    public ICommand? PageRequestCommand { get => GetValue(PageRequestCommandProperty); set => SetValue(PageRequestCommandProperty, value); }
    public IDataTemplate? ItemTemplate { get => GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }
    public IReadOnlyList<object?> Items => _items;
    public MdAsyncRequestState State { get => _state; private set { SetAndRaise(StateProperty, ref _state, value); UpdateState(); } }
    public Exception? Error { get => _error; private set => SetAndRaise(ErrorProperty, ref _error, value); }
    public Func<MdPageRequest, ValueTask<MdPageResult<object?>>>? PageProvider { get; set; }

    public event EventHandler<MdPageRequest>? PageRequested;
    public event EventHandler? Refreshed;

    public async ValueTask LoadNextPageAsync()
    {
        if (State == MdAsyncRequestState.Loading || _lastPage) return;
        _requestCancellation?.Cancel();
        _requestCancellation?.Dispose();
        _requestCancellation = new CancellationTokenSource();
        var request = new MdPageRequest(_nextPageKey, Math.Max(1, PageSize), CancellationToken: _requestCancellation.Token);
        PageRequested?.Invoke(this, request);
        if (PageRequestCommand?.CanExecute(request) == true) PageRequestCommand.Execute(request);
        if (PageProvider is null) { State = _items.Count == 0 ? MdAsyncRequestState.Empty : MdAsyncRequestState.Data; return; }
        State = MdAsyncRequestState.Loading;
        Error = null;
        try
        {
            var result = await PageProvider(request);
            foreach (var item in result.Items) _items.Add(item);
            _nextPageKey = result.NextPageKey ?? _nextPageKey + 1;
            _lastPage = result.IsLastPage || result.NextPageKey is null;
            State = _items.Count == 0 ? MdAsyncRequestState.Empty : _lastPage ? MdAsyncRequestState.Completed : MdAsyncRequestState.Data;
        }
        catch (OperationCanceledException) when (_requestCancellation.IsCancellationRequested) { }
        catch (Exception exception) { Error = exception; State = MdAsyncRequestState.Error; }
    }

    public async ValueTask RefreshAsync()
    {
        _requestCancellation?.Cancel();
        _items.Clear(); _nextPageKey = 0; _lastPage = false; Error = null; State = MdAsyncRequestState.Idle;
        Refreshed?.Invoke(this, EventArgs.Empty);
        await LoadNextPageAsync();
    }
    public ValueTask RetryAsync() => LoadNextPageAsync();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_retryButton is not null) _retryButton.Click -= OnRetry;
        if (_loadMoreButton is not null) _loadMoreButton.Click -= OnLoadMore;
        base.OnApplyTemplate(e);
        _retryButton = e.NameScope.Find<Button>("PART_RetryButton");
        _loadMoreButton = e.NameScope.Find<Button>("PART_LoadMoreButton");
        if (_retryButton is not null) _retryButton.Click += OnRetry;
        if (_loadMoreButton is not null) _loadMoreButton.Click += OnLoadMore;
    }
    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (AutoLoad && _items.Count == 0 && State == MdAsyncRequestState.Idle) _ = LoadNextPageAsync();
    }
    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _requestCancellation?.Cancel();
        base.OnDetachedFromVisualTree(e);
    }
    private async void OnRetry(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => await RetryAsync();
    private async void OnLoadMore(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => await LoadNextPageAsync();
    private void UpdateState()
    {
        foreach (var value in Enum.GetValues<MdAsyncRequestState>()) PseudoClasses.Set($":{value.ToString().ToLowerInvariant()}", State == value);
    }
}

public enum MdMasonryLayoutStrategy { Masonry, Quilted, Woven }

/// <summary>A finite-viewport masonry/quilted/woven panel. Pair with an ItemsRepeater/ListBox for item realization.</summary>
public sealed class MdMasonryPanel : Panel
{
    public static readonly StyledProperty<int> ColumnCountProperty = AvaloniaProperty.Register<MdMasonryPanel, int>(nameof(ColumnCount), 2);
    public static readonly StyledProperty<double> ColumnSpacingProperty = AvaloniaProperty.Register<MdMasonryPanel, double>(nameof(ColumnSpacing), 12);
    public static readonly StyledProperty<double> RowSpacingProperty = AvaloniaProperty.Register<MdMasonryPanel, double>(nameof(RowSpacing), 12);
    public static readonly StyledProperty<MdMasonryLayoutStrategy> StrategyProperty = AvaloniaProperty.Register<MdMasonryPanel, MdMasonryLayoutStrategy>(nameof(Strategy));
    private Rect[] _layout = Array.Empty<Rect>();
    static MdMasonryPanel() => AffectsMeasure<MdMasonryPanel>(ColumnCountProperty, ColumnSpacingProperty, RowSpacingProperty, StrategyProperty);
    public int ColumnCount { get => GetValue(ColumnCountProperty); set => SetValue(ColumnCountProperty, value); }
    public double ColumnSpacing { get => GetValue(ColumnSpacingProperty); set => SetValue(ColumnSpacingProperty, value); }
    public double RowSpacing { get => GetValue(RowSpacingProperty); set => SetValue(RowSpacingProperty, value); }
    public MdMasonryLayoutStrategy Strategy { get => GetValue(StrategyProperty); set => SetValue(StrategyProperty, value); }

    protected override Size MeasureOverride(Size availableSize)
    {
        var columns = Math.Max(1, ColumnCount);
        var width = double.IsFinite(availableSize.Width) ? availableSize.Width : 800;
        var cellWidth = Math.Max(0, (width - Math.Max(0, ColumnSpacing) * (columns - 1)) / columns);
        var heights = new double[columns];
        _layout = new Rect[Children.Count];
        for (var index = 0; index < Children.Count; index++)
        {
            var child = Children[index];
            child.Measure(new Size(cellWidth, double.PositiveInfinity));
            var column = Strategy == MdMasonryLayoutStrategy.Quilted ? index % columns : Array.IndexOf(heights, heights.Min());
            var span = Strategy == MdMasonryLayoutStrategy.Quilted && index % 5 == 0 && columns > 1 ? 2 : 1;
            span = Math.Min(span, columns - column);
            var itemWidth = cellWidth * span + Math.Max(0, ColumnSpacing) * (span - 1);
            if (span > 1) child.Measure(new Size(itemWidth, double.PositiveInfinity));
            var itemHeight = child.DesiredSize.Height;
            if (Strategy == MdMasonryLayoutStrategy.Woven) itemHeight = Math.Max(itemHeight, cellWidth * (index % 2 == 0 ? 1.15 : 0.85));
            var y = Enumerable.Range(column, span).Select(c => heights[c]).Max();
            var x = column * (cellWidth + Math.Max(0, ColumnSpacing));
            _layout[index] = new Rect(x, y, itemWidth, itemHeight);
            for (var c = column; c < column + span; c++) heights[c] = y + itemHeight + Math.Max(0, RowSpacing);
        }
        return new Size(width, Math.Max(0, heights.DefaultIfEmpty().Max() - Math.Max(0, RowSpacing)));
    }
    protected override Size ArrangeOverride(Size finalSize)
    {
        for (var index = 0; index < Math.Min(Children.Count, _layout.Length); index++) Children[index].Arrange(_layout[index]);
        return finalSize;
    }
}
