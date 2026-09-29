using System.Collections;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Motion;
using Path = Avalonia.Controls.Shapes.Path;

namespace Md3.Avalonia.Ecosystem.Controls;

public sealed record MdCascaderItem(object? Value, string Label, IReadOnlyList<MdCascaderItem>? Children = null, bool IsEnabled = true)
{
    public bool HasChildren => Children is { Count: > 0 };
}

/// <summary>Hierarchical single-path selection with keyboard traversal and MVVM-friendly selected path.</summary>
[PseudoClasses(":open", ":closed", ":present", ":has-value", ":reduced-motion", ":no-motion")]
public sealed class MdCascader : TemplatedControl, IMdPopupOwner, IMdPopupPresenceOwner
{
    public static readonly StyledProperty<IEnumerable<MdCascaderItem>?> ItemsSourceProperty = AvaloniaProperty.Register<MdCascader, IEnumerable<MdCascaderItem>?>(nameof(ItemsSource));
    public static readonly StyledProperty<bool> IsDropDownOpenProperty = AvaloniaProperty.Register<MdCascader, bool>(nameof(IsDropDownOpen), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<string?> LabelProperty = AvaloniaProperty.Register<MdCascader, string?>(nameof(Label), "Selected hierarchy");
    public static readonly StyledProperty<string?> PlaceholderTextProperty = AvaloniaProperty.Register<MdCascader, string?>(nameof(PlaceholderText), "Choose a destination");
    public static readonly StyledProperty<string> PathSeparatorProperty = AvaloniaProperty.Register<MdCascader, string>(nameof(PathSeparator), " / ");
    public static readonly DirectProperty<MdCascader, string> DisplayTextProperty = AvaloniaProperty.RegisterDirect<MdCascader, string>(nameof(DisplayText), control => control.DisplayText);
    public static readonly DirectProperty<MdCascader, IReadOnlyList<MdCascaderItem>> SelectedPathProperty = AvaloniaProperty.RegisterDirect<MdCascader, IReadOnlyList<MdCascaderItem>>(nameof(SelectedPath), control => control.SelectedPath);
    public static readonly DirectProperty<MdCascader, IReadOnlyList<IReadOnlyList<MdCascaderItem>>> ColumnsProperty = AvaloniaProperty.RegisterDirect<MdCascader, IReadOnlyList<IReadOnlyList<MdCascaderItem>>>(nameof(Columns), control => control.Columns);
    private IReadOnlyList<MdCascaderItem> _selectedPath = Array.Empty<MdCascaderItem>();
    private string _displayText = string.Empty;
    private IReadOnlyList<IReadOnlyList<MdCascaderItem>> _columns = Array.Empty<IReadOnlyList<MdCascaderItem>>();
    private readonly MdPresenceController _presence;
    private ItemsControl? _columnsHost;
    private Button? _anchorButton;
    private Border? _surface;
    private Path? _arrow;
    private int _openStateVersion;
    private bool _isAttached;
    static MdCascader()
    {
        ItemsSourceProperty.Changed.AddClassHandler<MdCascader>((control, _) => control.Reset());
        IsDropDownOpenProperty.Changed.AddClassHandler<MdCascader>((control, _) => control.UpdateOpenState());
        PlaceholderTextProperty.Changed.AddClassHandler<MdCascader>((control, _) => control.UpdateDisplayText());
        PathSeparatorProperty.Changed.AddClassHandler<MdCascader>((control, _) => control.UpdateDisplayText());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdCascader>((control, _) => control.UpdateMotion());
    }
    public MdCascader()
    {
        _presence = new MdPresenceController(value => PseudoClasses.Set(":present", value));
        _presence.Initialize(IsDropDownOpen);
        Reset();
        UpdateOpenState();
    }
    public IEnumerable<MdCascaderItem>? ItemsSource { get => GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public bool IsDropDownOpen { get => GetValue(IsDropDownOpenProperty); set => SetValue(IsDropDownOpenProperty, value); }
    public bool IsMaterialPopupOpen
    {
        get => IsDropDownOpen;
        set => SetCurrentValue(IsDropDownOpenProperty, value);
    }
    public string? Label { get => GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public string? PlaceholderText { get => GetValue(PlaceholderTextProperty); set => SetValue(PlaceholderTextProperty, value); }
    public string PathSeparator { get => GetValue(PathSeparatorProperty); set => SetValue(PathSeparatorProperty, value); }
    public string DisplayText => _displayText;
    public IReadOnlyList<MdCascaderItem> SelectedPath => _selectedPath;
    public IReadOnlyList<IReadOnlyList<MdCascaderItem>> Columns => _columns;
    public event EventHandler<IReadOnlyList<MdCascaderItem>>? SelectionChanged;
    public void Select(int level, MdCascaderItem item)
    {
        if (!item.IsEnabled || level < 0 || level >= Columns.Count) return;
        var path = SelectedPath.Take(level).Append(item).ToArray();
        SetAndRaise(SelectedPathProperty, ref _selectedPath, path);
        UpdateDisplayText();
        var columns = new List<IReadOnlyList<MdCascaderItem>> { (ItemsSource ?? Array.Empty<MdCascaderItem>()).ToArray() };
        foreach (var selected in path) if (selected.Children is { Count: > 0 }) columns.Add(selected.Children);
        SetAndRaise(ColumnsProperty, ref _columns, columns);
        if (item.Children is not { Count: > 0 }) { IsDropDownOpen = false; SelectionChanged?.Invoke(this, path); }
    }
    public void Clear() { SetAndRaise(SelectedPathProperty, ref _selectedPath, Array.Empty<MdCascaderItem>()); UpdateDisplayText(); Reset(); SelectionChanged?.Invoke(this, SelectedPath); }
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_columnsHost is not null) _columnsHost.RemoveHandler(ListBox.SelectionChangedEvent, OnColumnSelectionChanged);
        if (_anchorButton is not null) _anchorButton.Click -= OnAnchorClick;
        base.OnApplyTemplate(e);
        _columnsHost = e.NameScope.Find<ItemsControl>("PART_Columns");
        _anchorButton = e.NameScope.Find<Button>("PART_Anchor");
        _surface = e.NameScope.Find<Border>("PART_Surface");
        _arrow = e.NameScope.Find<Path>("PART_Arrow");
        _columnsHost?.AddHandler(ListBox.SelectionChangedEvent, OnColumnSelectionChanged);
        if (_anchorButton is not null) _anchorButton.Click += OnAnchorClick;
        UpdateMotion();
    }

    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        UpdateOpenState();
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        ++_openStateVersion;
        _presence.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private void OnAnchorClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) =>
        SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { IsDropDownOpen = false; e.Handled = true; }
        else if (e.Key is Key.Enter or Key.Space) { IsDropDownOpen = !IsDropDownOpen; e.Handled = true; }
        else base.OnKeyDown(e);
    }
    private void OnColumnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.Source is not ListBox list || list.SelectedItem is not MdCascaderItem item) return;
        var level = Enumerable.Range(0, Columns.Count).FirstOrDefault(index => ReferenceEquals(Columns[index], list.ItemsSource));
        Select(level, item);
        list.SelectedItem = null;
        e.Handled = true;
    }
    private void Reset()
    {
        IReadOnlyList<IReadOnlyList<MdCascaderItem>> value = [(ItemsSource ?? Array.Empty<MdCascaderItem>()).ToArray()];
        SetAndRaise(ColumnsProperty, ref _columns, value);
        UpdateDisplayText();
    }
    private void UpdateDisplayText()
    {
        var value = SelectedPath.Count == 0 ? PlaceholderText ?? string.Empty : string.Join(PathSeparator, SelectedPath.Select(item => item.Label));
        SetAndRaise(DisplayTextProperty, ref _displayText, value);
        PseudoClasses.Set(":has-value", SelectedPath.Count > 0);
    }

    void IMdPopupPresenceOwner.ClosePopupImmediately() => _presence.Initialize(false);

    private void UpdateOpenState()
    {
        MdPopupCoordinator.NotifyStateChanged(this);
        var version = ++_openStateVersion;
        if (IsDropDownOpen)
        {
            _presence.Update(true, TimeSpan.Zero);
            if (MdMotion.GetScheme(this) == MdMotionScheme.None)
            {
                PseudoClasses.Set(":closed", false);
                PseudoClasses.Set(":open", true);
            }
            else
            {
                PseudoClasses.Set(":open", false);
                PseudoClasses.Set(":closed", true);
                if (_isAttached)
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (version != _openStateVersion || !_isAttached || !IsDropDownOpen) return;
                        PseudoClasses.Set(":closed", false);
                        PseudoClasses.Set(":open", true);
                    }, DispatcherPriority.Render);
            }
        }
        else
        {
            PseudoClasses.Set(":open", false);
            PseudoClasses.Set(":closed", true);
            _presence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
        }
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_surface is not null)
            _surface.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
        if (_arrow is not null)
            _arrow.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
        if (IsDropDownOpen && scheme == MdMotionScheme.None)
        {
            ++_openStateVersion;
            PseudoClasses.Set(":closed", false);
            PseudoClasses.Set(":open", true);
        }
        else if (!IsDropDownOpen)
        {
            _presence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
        }
    }
}

/// <summary>Dual-list transfer control with checked moves, select-all, pointer drag/drop, keyboard APIs, and stable source order.</summary>
[PseudoClasses(":dragging", ":drag-to-source", ":drag-to-target")]
public sealed class MdTransfer : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = AvaloniaProperty.Register<MdTransfer, IEnumerable?>(nameof(ItemsSource));
    public static readonly StyledProperty<IEnumerable?> SelectedItemsProperty = AvaloniaProperty.Register<MdTransfer, IEnumerable?>(nameof(SelectedItems), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<string?> SourceFilterProperty = AvaloniaProperty.Register<MdTransfer, string?>(nameof(SourceFilter));
    public static readonly StyledProperty<string?> TargetFilterProperty = AvaloniaProperty.Register<MdTransfer, string?>(nameof(TargetFilter));
    public static readonly StyledProperty<bool> IsDragEnabledProperty = AvaloniaProperty.Register<MdTransfer, bool>(nameof(IsDragEnabled), true);
    public static readonly DirectProperty<MdTransfer, IReadOnlyList<object?>> AvailableItemsProperty = AvaloniaProperty.RegisterDirect<MdTransfer, IReadOnlyList<object?>>(nameof(AvailableItems), control => control.AvailableItems);
    public static readonly DirectProperty<MdTransfer, IReadOnlyList<object?>> TargetItemsProperty = AvaloniaProperty.RegisterDirect<MdTransfer, IReadOnlyList<object?>>(nameof(TargetItems), control => control.TargetItems);
    private IReadOnlyList<object?> _available = Array.Empty<object?>();
    private IReadOnlyList<object?> _target = Array.Empty<object?>();
    private ListBox? _availableList;
    private ListBox? _targetList;
    private Button? _moveTarget;
    private Button? _moveSource;
    private Button? _moveAllTarget;
    private Button? _moveAllSource;
    private IReadOnlyList<object?> _dragItems = Array.Empty<object?>();
    private bool _dragFromSource;
    private Point _dragStart;
    private bool _dragMoved;
    private ListBoxItem? _dragContainer;
    private TranslateTransform? _dragTransform;
    private int _transferAnimationVersion;
    static MdTransfer()
    {
        ItemsSourceProperty.Changed.AddClassHandler<MdTransfer>((control, _) => control.Refresh());
        SelectedItemsProperty.Changed.AddClassHandler<MdTransfer>((control, _) => control.Refresh());
        SourceFilterProperty.Changed.AddClassHandler<MdTransfer>((control, _) => control.Refresh());
        TargetFilterProperty.Changed.AddClassHandler<MdTransfer>((control, _) => control.Refresh());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdTransfer>((control, _) => control.CancelTransferMotion());
    }
    public MdTransfer() => Refresh();
    public IEnumerable? ItemsSource { get => GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public IEnumerable? SelectedItems { get => GetValue(SelectedItemsProperty); set => SetValue(SelectedItemsProperty, value); }
    public string? SourceFilter { get => GetValue(SourceFilterProperty); set => SetValue(SourceFilterProperty, value); }
    public string? TargetFilter { get => GetValue(TargetFilterProperty); set => SetValue(TargetFilterProperty, value); }
    public bool IsDragEnabled { get => GetValue(IsDragEnabledProperty); set => SetValue(IsDragEnabledProperty, value); }
    public IReadOnlyList<object?> AvailableItems => _available;
    public IReadOnlyList<object?> TargetItems => _target;
    public event EventHandler? SelectionChanged;
    public void MoveToTarget(IEnumerable<object?> items)
    {
        var current = (SelectedItems ?? Array.Empty<object>()).Cast<object?>().ToList();
        var moved = items.Where(item => !current.Contains(item)).Distinct().ToArray();
        if (moved.Length == 0) return;
        var previousLayout = CaptureItemLayout();
        foreach (var item in moved) current.Add(item);
        SelectedItems = current;
        ScheduleTransferMotion(moved, previousLayout);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
    public void MoveToSource(IEnumerable<object?> items)
    {
        var current = (SelectedItems ?? Array.Empty<object>()).Cast<object?>().ToArray();
        var requested = items.ToHashSet();
        var moved = current.Where(requested.Contains).Distinct().ToArray();
        if (moved.Length == 0) return;
        var previousLayout = CaptureItemLayout();
        SelectedItems = current.Where(item => !requested.Contains(item)).ToArray();
        ScheduleTransferMotion(moved, previousLayout);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
    public void MoveAllToTarget() => MoveToTarget(AvailableItems);
    public void MoveAllToSource() => MoveToSource(TargetItems);
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateHandlers();
        base.OnApplyTemplate(e);
        _availableList = e.NameScope.Find<ListBox>("PART_Available");
        _targetList = e.NameScope.Find<ListBox>("PART_Target");
        _moveTarget = e.NameScope.Find<Button>("PART_MoveTarget");
        _moveSource = e.NameScope.Find<Button>("PART_MoveSource");
        _moveAllTarget = e.NameScope.Find<Button>("PART_MoveAllTarget");
        _moveAllSource = e.NameScope.Find<Button>("PART_MoveAllSource");
        if (_moveTarget is not null) _moveTarget.Click += OnMoveTarget;
        if (_moveSource is not null) _moveSource.Click += OnMoveSource;
        if (_moveAllTarget is not null) _moveAllTarget.Click += OnMoveAllTarget;
        if (_moveAllSource is not null) _moveAllSource.Click += OnMoveAllSource;
        _availableList?.AddHandler(InputElement.PointerPressedEvent, OnListPointerPressed, RoutingStrategies.Tunnel, true);
        _targetList?.AddHandler(InputElement.PointerPressedEvent, OnListPointerPressed, RoutingStrategies.Tunnel, true);
    }
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        FinishDragVisual();
        _dragItems = Array.Empty<object?>();
        _dragMoved = false;
        PseudoClasses.Set(":dragging", false);
        PseudoClasses.Set(":drag-to-source", false);
        PseudoClasses.Set(":drag-to-target", false);
        CancelTransferMotion();
        base.OnDetachedFromVisualTree(e);
    }

    private void DetachTemplateHandlers()
    {
        _availableList?.RemoveHandler(InputElement.PointerPressedEvent, OnListPointerPressed);
        _targetList?.RemoveHandler(InputElement.PointerPressedEvent, OnListPointerPressed);
        if (_moveTarget is not null) _moveTarget.Click -= OnMoveTarget;
        if (_moveSource is not null) _moveSource.Click -= OnMoveSource;
        if (_moveAllTarget is not null) _moveAllTarget.Click -= OnMoveAllTarget;
        if (_moveAllSource is not null) _moveAllSource.Click -= OnMoveAllSource;
    }
    private void OnListPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsDragEnabled || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        var source = e.Source as Visual;
        var row = source as ListBoxItem ?? source?.FindAncestorOfType<ListBoxItem>();
        if (row is null || sender is not ListBox list) return;
        var item = row.DataContext ?? row.Content;
        if (item is null) return;
        _dragFromSource = ReferenceEquals(list, _availableList);
        if (list.SelectedItems?.Contains(item) != true) list.SelectedItem = item;
        _dragItems = (list.SelectedItems ?? Array.Empty<object>()).Cast<object?>().ToArray();
        if (_dragItems.Count == 0) _dragItems = [item];
        _dragStart = e.GetPosition(this);
        _dragMoved = false;
        _dragContainer = row;
        _dragTransform = row.RenderTransform as TranslateTransform ?? new TranslateTransform();
        _dragTransform.Transitions = null;
        row.RenderTransform = _dragTransform;
        row.RenderTransformOrigin = RelativePoint.TopLeft;
        row.ZIndex = 2;
        e.Pointer.Capture(this);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_dragItems.Count == 0 || !ReferenceEquals(e.Pointer.Captured, this)) return;
        var position = e.GetPosition(this);
        _dragMoved |= Math.Abs(position.X - _dragStart.X) > 6 || Math.Abs(position.Y - _dragStart.Y) > 6;
        if (!_dragMoved) return;
        var overSource = IsPointerWithin(_availableList, e);
        var overTarget = IsPointerWithin(_targetList, e);
        PseudoClasses.Set(":dragging", true);
        PseudoClasses.Set(":drag-to-source", !_dragFromSource && overSource);
        PseudoClasses.Set(":drag-to-target", _dragFromSource && overTarget);
        if (_dragTransform is not null)
        {
            _dragTransform.X = position.X - _dragStart.X;
            _dragTransform.Y = position.Y - _dragStart.Y;
        }
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_dragItems.Count == 0) return;
        var wasMoved = _dragMoved;
        if (wasMoved && _dragFromSource && IsPointerWithin(_targetList, e)) MoveToTarget(_dragItems);
        else if (wasMoved && !_dragFromSource && IsPointerWithin(_availableList, e)) MoveToSource(_dragItems);
        ResetDrag(e.Pointer);
        e.Handled = wasMoved;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        FinishDragVisual();
        _dragItems = Array.Empty<object?>();
        _dragMoved = false;
        PseudoClasses.Set(":dragging", false);
        PseudoClasses.Set(":drag-to-source", false);
        PseudoClasses.Set(":drag-to-target", false);
        base.OnPointerCaptureLost(e);
    }

    private static bool IsPointerWithin(Control? target, PointerEventArgs e) =>
        target is { IsVisible: true } && new Rect(target.Bounds.Size).Contains(e.GetPosition(target));

    private void ResetDrag(IPointer pointer)
    {
        FinishDragVisual();
        _dragItems = Array.Empty<object?>();
        _dragMoved = false;
        PseudoClasses.Set(":dragging", false);
        PseudoClasses.Set(":drag-to-source", false);
        PseudoClasses.Set(":drag-to-target", false);
        if (ReferenceEquals(pointer.Captured, this)) pointer.Capture(null);
    }

    private void FinishDragVisual()
    {
        if (_dragTransform is not null)
        {
            _dragTransform.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, TranslateTransform.XProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateDouble(this, TranslateTransform.YProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast));
            _dragTransform.X = 0;
            _dragTransform.Y = 0;
        }
        if (_dragContainer is not null) _dragContainer.ZIndex = 0;
        _dragContainer = null;
        _dragTransform = null;
    }

    private List<(object? Item, Point Position)> CaptureItemLayout()
    {
        var result = new List<(object?, Point)>();
        foreach (var list in new[] { _availableList, _targetList })
        {
            if (list is null) continue;
            foreach (var container in list.GetRealizedContainers().OfType<ListBoxItem>())
            {
                var item = container.DataContext ?? container.Content;
                if (container.TranslatePoint(default, this) is { } point)
                    result.Add((item, point));
            }
        }
        return result;
    }

    private void ScheduleTransferMotion(
        IReadOnlyList<object?> movedItems,
        List<(object? Item, Point Position)> previousLayout)
    {
        var version = ++_transferAnimationVersion;
        Dispatcher.UIThread.Post(
            () => AnimateTransfer(movedItems, previousLayout, version),
            DispatcherPriority.Loaded);
    }

    private void AnimateTransfer(
        IReadOnlyList<object?> movedItems,
        List<(object? Item, Point Position)> previousLayout,
        int version)
    {
        if (version != _transferAnimationVersion) return;
        UpdateLayout();
        var spatial = MdMotion.Resolve(this, MdMotionKind.Spatial, MdMotionSpeed.Default);
        var effects = MdMotion.Resolve(this, MdMotionKind.Effects, MdMotionSpeed.Fast);
        if (!spatial.IsEnabled && !effects.IsEnabled) return;

        var pending = new List<(ListBoxItem Container, TranslateTransform Transform)>();
        foreach (var list in new[] { _availableList, _targetList })
        {
            if (list is null) continue;
            foreach (var container in list.GetRealizedContainers().OfType<ListBoxItem>())
            {
                var item = container.DataContext ?? container.Content;
                var old = previousLayout.FirstOrDefault(entry => Equals(entry.Item, item));
                var hasOldPosition = previousLayout.Any(entry => Equals(entry.Item, item));
                var current = container.TranslatePoint(default, this) ?? default;
                var moved = movedItems.Any(candidate => Equals(candidate, item));
                var transform = container.RenderTransform as TranslateTransform ?? new TranslateTransform();
                container.RenderTransform = transform;
                container.RenderTransformOrigin = RelativePoint.TopLeft;
                container.Transitions = null;
                transform.Transitions = null;
                transform.X = spatial.IsEnabled && hasOldPosition ? old.Position.X - current.X : 0;
                transform.Y = spatial.IsEnabled && hasOldPosition ? old.Position.Y - current.Y : 0;
                container.Opacity = moved && !spatial.IsEnabled && effects.IsEnabled ? 0 : 1;
                if (Math.Abs(transform.X) > 0.01 || Math.Abs(transform.Y) > 0.01 || container.Opacity < 1)
                    pending.Add((container, transform));
            }
        }
        if (pending.Count == 0) return;

        Dispatcher.UIThread.Post(() =>
        {
            if (version != _transferAnimationVersion) return;
            foreach (var entry in pending)
            {
                entry.Container.Transitions = MdMotionTransitions.Collect(
                    MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
                entry.Transform.Transitions = MdMotionTransitions.Collect(
                    MdMotionTransitions.CreateDouble(this, TranslateTransform.XProperty, MdMotionKind.Spatial, MdMotionSpeed.Default),
                    MdMotionTransitions.CreateDouble(this, TranslateTransform.YProperty, MdMotionKind.Spatial, MdMotionSpeed.Default));
                entry.Container.Opacity = 1;
                entry.Transform.X = 0;
                entry.Transform.Y = 0;
            }
        }, DispatcherPriority.Render);
    }

    private void OnMoveTarget(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => MoveToTarget((_availableList?.SelectedItems ?? Array.Empty<object>()).Cast<object?>());
    private void OnMoveSource(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => MoveToSource((_targetList?.SelectedItems ?? Array.Empty<object>()).Cast<object?>());
    private void OnMoveAllTarget(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => MoveAllToTarget();
    private void OnMoveAllSource(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => MoveAllToSource();
    private void CancelTransferMotion()
    {
        ++_transferAnimationVersion;
        foreach (var list in new[] { _availableList, _targetList })
        {
            if (list is null) continue;
            foreach (var container in list.GetRealizedContainers().OfType<ListBoxItem>())
            {
                if (ReferenceEquals(container, _dragContainer)) continue;
                container.Transitions = null;
                container.Opacity = 1;
                if (container.RenderTransform is TranslateTransform transform)
                {
                    transform.Transitions = null;
                    transform.X = 0;
                    transform.Y = 0;
                }
            }
        }
    }

    private void Refresh()
    {
        _transferAnimationVersion++;
        var all = (ItemsSource ?? Array.Empty<object>()).Cast<object?>().ToArray();
        var selected = (SelectedItems ?? Array.Empty<object>()).Cast<object?>().ToHashSet();
        var available = all.Where(item => !selected.Contains(item) && Matches(item, SourceFilter)).ToArray();
        var target = all.Where(item => selected.Contains(item) && Matches(item, TargetFilter)).ToArray();
        SetAndRaise(AvailableItemsProperty, ref _available, available);
        SetAndRaise(TargetItemsProperty, ref _target, target);
    }
    private static bool Matches(object? item, string? filter) => string.IsNullOrWhiteSpace(filter) || item?.ToString()?.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase) == true;
}
