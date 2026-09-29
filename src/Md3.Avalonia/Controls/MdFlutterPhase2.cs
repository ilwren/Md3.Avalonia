using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A Material card-backed, host-sortable paginated data table.</summary>
[PseudoClasses(":first-page", ":last-page", ":empty")]
public sealed class MdPaginatedDataTable : TemplatedControl
{
    public static readonly StyledProperty<object?> HeaderProperty = AvaloniaProperty.Register<MdPaginatedDataTable, object?>(nameof(Header));
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = AvaloniaProperty.Register<MdPaginatedDataTable, IEnumerable?>(nameof(ItemsSource));
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty = AvaloniaProperty.Register<MdPaginatedDataTable, IDataTemplate?>(nameof(ItemTemplate));
    public static readonly StyledProperty<int> PageIndexProperty = AvaloniaProperty.Register<MdPaginatedDataTable, int>(nameof(PageIndex), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<int> RowsPerPageProperty = AvaloniaProperty.Register<MdPaginatedDataTable, int>(nameof(RowsPerPage), 10, defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<IReadOnlyList<int>> AvailableRowsPerPageProperty = AvaloniaProperty.Register<MdPaginatedDataTable, IReadOnlyList<int>>(nameof(AvailableRowsPerPage), new[] { 5, 10, 20, 50 });
    public static readonly StyledProperty<int?> TotalItemCountProperty = AvaloniaProperty.Register<MdPaginatedDataTable, int?>(nameof(TotalItemCount));
    public static readonly StyledProperty<bool> IsLoadingProperty = AvaloniaProperty.Register<MdPaginatedDataTable, bool>(nameof(IsLoading));
    public static readonly StyledProperty<ICommand?> PageChangedCommandProperty = AvaloniaProperty.Register<MdPaginatedDataTable, ICommand?>(nameof(PageChangedCommand));
    public static readonly DirectProperty<MdPaginatedDataTable, IReadOnlyList<object?>> PageItemsProperty = AvaloniaProperty.RegisterDirect<MdPaginatedDataTable, IReadOnlyList<object?>>(nameof(PageItems), control => control.PageItems);
    public static readonly DirectProperty<MdPaginatedDataTable, string> PageLabelProperty = AvaloniaProperty.RegisterDirect<MdPaginatedDataTable, string>(nameof(PageLabel), control => control.PageLabel);

    private IReadOnlyList<object?> _pageItems = Array.Empty<object?>();
    private string _pageLabel = "0 of 0";
    private Button? _previousButton;
    private Button? _nextButton;

    static MdPaginatedDataTable()
    {
        ItemsSourceProperty.Changed.AddClassHandler<MdPaginatedDataTable>((control, _) => control.RebuildPage());
        PageIndexProperty.Changed.AddClassHandler<MdPaginatedDataTable>((control, _) => control.RebuildPage());
        RowsPerPageProperty.Changed.AddClassHandler<MdPaginatedDataTable>((control, _) => control.OnRowsPerPageChanged());
        TotalItemCountProperty.Changed.AddClassHandler<MdPaginatedDataTable>((control, _) => control.RebuildPage());
        IsLoadingProperty.Changed.AddClassHandler<MdPaginatedDataTable>((control, _) => control.UpdatePseudoClasses());
    }

    public MdPaginatedDataTable() => RebuildPage();

    public object? Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
    public IEnumerable? ItemsSource { get => GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public IDataTemplate? ItemTemplate { get => GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }
    public int PageIndex { get => GetValue(PageIndexProperty); set => SetValue(PageIndexProperty, value); }
    public int RowsPerPage { get => GetValue(RowsPerPageProperty); set => SetValue(RowsPerPageProperty, value); }
    public IReadOnlyList<int> AvailableRowsPerPage { get => GetValue(AvailableRowsPerPageProperty); set => SetValue(AvailableRowsPerPageProperty, value); }
    public int? TotalItemCount { get => GetValue(TotalItemCountProperty); set => SetValue(TotalItemCountProperty, value); }
    public bool IsLoading { get => GetValue(IsLoadingProperty); set => SetValue(IsLoadingProperty, value); }
    public ICommand? PageChangedCommand { get => GetValue(PageChangedCommandProperty); set => SetValue(PageChangedCommandProperty, value); }
    public IReadOnlyList<object?> PageItems => _pageItems;
    public string PageLabel => _pageLabel;
    public int FirstRowIndex => PageIndex * Math.Max(1, RowsPerPage);

    public event EventHandler<MdPageChangedEventArgs>? PageChanged;

    public void NextPage() => SetPage(PageIndex + 1);
    public void PreviousPage() => SetPage(PageIndex - 1);
    public void FirstPage() => SetPage(0);

    public void SetPage(int pageIndex)
    {
        var pageCount = GetPageCount();
        var target = Math.Clamp(pageIndex, 0, Math.Max(0, pageCount - 1));
        if (target == PageIndex)
        {
            RebuildPage();
            return;
        }
        SetCurrentValue(PageIndexProperty, target);
        var args = new MdPageChangedEventArgs(target, FirstRowIndex, Math.Max(1, RowsPerPage));
        if (PageChangedCommand?.CanExecute(args) == true) PageChangedCommand.Execute(args);
        PageChanged?.Invoke(this, args);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_previousButton is not null) _previousButton.Click -= OnPrevious;
        if (_nextButton is not null) _nextButton.Click -= OnNext;
        base.OnApplyTemplate(e);
        _previousButton = e.NameScope.Find<Button>("PART_PreviousPageButton");
        _nextButton = e.NameScope.Find<Button>("PART_NextPageButton");
        if (_previousButton is not null) _previousButton.Click += OnPrevious;
        if (_nextButton is not null) _nextButton.Click += OnNext;
    }

    private void OnPrevious(object? sender, RoutedEventArgs e) => PreviousPage();
    private void OnNext(object? sender, RoutedEventArgs e) => NextPage();

    private void OnRowsPerPageChanged()
    {
        if (RowsPerPage < 1) SetCurrentValue(RowsPerPageProperty, 1);
        if (PageIndex != 0) SetCurrentValue(PageIndexProperty, 0);
        RebuildPage();
    }

    private int GetItemCount()
    {
        if (TotalItemCount is { } total) return Math.Max(0, total);
        return ItemsSource switch
        {
            ICollection collection => collection.Count,
            null => 0,
            _ => ItemsSource.Cast<object?>().Count()
        };
    }

    private int GetPageCount()
    {
        var count = GetItemCount();
        return count == 0 ? 1 : (int)Math.Ceiling(count / (double)Math.Max(1, RowsPerPage));
    }

    private void RebuildPage()
    {
        var count = GetItemCount();
        var pageCount = GetPageCount();
        var safePage = Math.Clamp(PageIndex, 0, Math.Max(0, pageCount - 1));
        if (safePage != PageIndex)
        {
            SetCurrentValue(PageIndexProperty, safePage);
            return;
        }
        var first = safePage * Math.Max(1, RowsPerPage);
        var items = ItemsSource?.Cast<object?>().Skip(first).Take(Math.Max(1, RowsPerPage)).ToArray() ?? Array.Empty<object?>();
        SetAndRaise(PageItemsProperty, ref _pageItems, items);
        var label = count == 0 ? "0 of 0" : $"{first + 1}–{Math.Min(count, first + items.Length)} of {count}";
        SetAndRaise(PageLabelProperty, ref _pageLabel, label);
        UpdatePseudoClasses();
    }

    private void UpdatePseudoClasses()
    {
        var count = GetItemCount();
        PseudoClasses.Set(":first-page", PageIndex <= 0);
        PseudoClasses.Set(":last-page", PageIndex >= GetPageCount() - 1);
        PseudoClasses.Set(":empty", count == 0);
        PseudoClasses.Set(":loading", IsLoading);
    }
}

public sealed class MdPageChangedEventArgs(int pageIndex, int firstRowIndex, int rowsPerPage) : EventArgs
{
    public int PageIndex { get; } = pageIndex;
    public int FirstRowIndex { get; } = firstRowIndex;
    public int RowsPerPage { get; } = rowsPerPage;
}

/// <summary>A list whose item order can be changed by pointer drag, keyboard alternatives, commands, or direct calls.</summary>
[PseudoClasses(":reordering", ":reduced-motion", ":no-motion")]
public sealed class MdReorderableList : ListBox
{
    public static readonly StyledProperty<ICommand?> ReorderCommandProperty = AvaloniaProperty.Register<MdReorderableList, ICommand?>(nameof(ReorderCommand));
    public static readonly StyledProperty<bool> IsReorderEnabledProperty = AvaloniaProperty.Register<MdReorderableList, bool>(nameof(IsReorderEnabled), true);
    public static readonly StyledProperty<bool> BuildDefaultDragHandlesProperty = AvaloniaProperty.Register<MdReorderableList, bool>(nameof(BuildDefaultDragHandles), true);

    private int _dragIndex = -1;
    private int _dropIndex = -1;
    private Point _dragStart;
    private bool _dragStarted;
    private ListBoxItem? _dragContainer;
    private ListBoxItem? _dropContainer;
    private TranslateTransform? _dragTransform;
    private double _autoScrollCompensation;
    private readonly Dictionary<ListBoxItem, TranslateTransform> _reflowTransforms = [];

    static MdReorderableList() =>
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdReorderableList>((list, _) => list.UpdateMotion());

    public MdReorderableList()
    {
        UpdateMotion();
        // ListBoxItem consumes primary presses. Observe them on the tunnel route so every
        // part of the row—not just an optional drag handle—can begin a real drag.
        AddHandler(PointerPressedEvent, OnPreviewPointerPressed, RoutingStrategies.Tunnel, true);
    }

    public ICommand? ReorderCommand { get => GetValue(ReorderCommandProperty); set => SetValue(ReorderCommandProperty, value); }
    public bool IsReorderEnabled { get => GetValue(IsReorderEnabledProperty); set => SetValue(IsReorderEnabledProperty, value); }
    public bool BuildDefaultDragHandles { get => GetValue(BuildDefaultDragHandlesProperty); set => SetValue(BuildDefaultDragHandlesProperty, value); }

    public event EventHandler<MdReorderEventArgs>? ReorderStarted;
    public event EventHandler<MdReorderEventArgs>? ReorderRequested;
    public event EventHandler<MdReorderEventArgs>? ReorderCompleted;

    public bool MoveItem(int oldIndex, int newIndex)
    {
        var count = ItemCount;
        if (!IsReorderEnabled || oldIndex < 0 || oldIndex >= count || newIndex < 0 || newIndex >= count || oldIndex == newIndex) return false;
        var args = new MdReorderEventArgs(oldIndex, newIndex);
        if (ReorderCommand?.CanExecute(args) == true) ReorderCommand.Execute(args);
        ReorderRequested?.Invoke(this, args);
        if (!args.Handled)
        {
            if (ItemsSource is IList source && !source.IsReadOnly && !source.IsFixedSize)
            {
                var item = source[oldIndex];
                source.RemoveAt(oldIndex);
                source.Insert(newIndex, item);
            }
            else if (ItemsSource is null)
            {
                var item = Items[oldIndex];
                Items.RemoveAt(oldIndex);
                Items.Insert(newIndex, item);
            }
        }
        ReorderCompleted?.Invoke(this, args);
        SelectedIndex = newIndex;
        return true;
    }

    public bool MoveSelectedUp() => SelectedIndex > 0 && MoveItem(SelectedIndex, SelectedIndex - 1);
    public bool MoveSelectedDown() => SelectedIndex >= 0 && SelectedIndex < ItemCount - 1 && MoveItem(SelectedIndex, SelectedIndex + 1);

    private void OnPreviewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!BeginPointerDrag(e)) return;
        // Prevent ListBoxItem from replacing our capture; retain ordinary row selection.
        SelectedIndex = _dragIndex;
        e.Handled = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.Handled) BeginPointerDrag(e);
    }

    private bool BeginPointerDrag(PointerPressedEventArgs e)
    {
        if (!IsReorderEnabled || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return false;
        var source = e.Source as Visual;
        var row = source as ListBoxItem ?? source?.FindAncestorOfType<ListBoxItem>();
        if (row is null) return false;
        if (BuildDefaultDragHandles &&
            !source!.GetVisualAncestors().Prepend(source).OfType<Control>()
                .Any(control => control.Name == "PART_DragHandle"))
            return false;
        var index = IndexFromContainer(row);
        if (index < 0) return false;
        _dragIndex = index;
        _dropIndex = _dragIndex;
        _dragStart = e.GetPosition(this);
        _autoScrollCompensation = 0;
        _dragStarted = false;
        _dragContainer = row;
        PrepareReflowTransforms();
        _dragTransform = GetReflowTransform(row);
        _dragTransform.Transitions = null;
        e.Pointer.Capture(this);
        return true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_dragIndex < 0 || !ReferenceEquals(e.Pointer.Captured, this) || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        var current = e.GetPosition(this);
        var delta = current.Y - _dragStart.Y;
        if (!_dragStarted && Math.Abs(delta) < 4) return;
        if (!_dragStarted)
        {
            _dragStarted = true;
            _dragContainer?.Classes.Set("dragging", true);
            PseudoClasses.Set(":reordering", true);
            ReorderStarted?.Invoke(this, new MdReorderEventArgs(_dragIndex, _dragIndex));
        }
        if (_dragContainer is not null && _dragTransform is not null)
        {
            _dragTransform.Y = delta + _autoScrollCompensation;
            _dragContainer.ZIndex = 2;
        }
        AutoScroll(e);
        var target = GetRealizedContainers().OfType<ListBoxItem>()
            .Select(item => (Item: item, Index: IndexFromContainer(item)))
            // The dragged row follows the pointer, so including it here would make it the
            // nearest row forever and prevent the drop index from ever changing.
            .Where(pair => pair.Index >= 0 && !ReferenceEquals(pair.Item, _dragContainer))
            .OrderBy(pair => Math.Abs((pair.Item.TranslatePoint(new Point(0, pair.Item.Bounds.Height / 2), this)?.Y ?? 0) - current.Y))
            .FirstOrDefault();
        if (target.Item is not null && target.Index != _dropIndex)
        {
            _dropContainer?.Classes.Set("drop-target", false);
            _dropContainer = target.Item;
            _dropContainer.Classes.Set("drop-target", target.Index != _dragIndex);
            _dropIndex = target.Index;
            ApplyReflowGap();
        }
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        var oldIndex = _dragIndex;
        var newIndex = _dropIndex;
        var wasDragging = _dragStarted;
        if (wasDragging && oldIndex >= 0 && newIndex >= 0 && oldIndex != newIndex) MoveItem(oldIndex, newIndex);
        ResetDragVisuals();
        if (ReferenceEquals(e.Pointer.Captured, this)) e.Pointer.Capture(null);
        if (wasDragging) e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        ResetDragVisuals();
        base.OnPointerCaptureLost(e);
    }

    private void PrepareReflowTransforms()
    {
        var realized = GetRealizedContainers().OfType<ListBoxItem>().ToArray();
        foreach (var stale in _reflowTransforms.Keys.Except(realized).ToArray())
            _reflowTransforms.Remove(stale);
        foreach (var row in realized)
            GetReflowTransform(row);
    }

    private TranslateTransform GetReflowTransform(ListBoxItem row)
    {
        if (_reflowTransforms.TryGetValue(row, out var transform)) return transform;
        transform = row.RenderTransform as TranslateTransform ?? new TranslateTransform();
        transform.Transitions = MdMotionTransitions.Collect(MdMotionTransitions.CreateDouble(
            this, TranslateTransform.YProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast));
        row.RenderTransform = transform;
        row.RenderTransformOrigin = RelativePoint.TopLeft;
        _reflowTransforms[row] = transform;
        return transform;
    }

    private void ApplyReflowGap()
    {
        if (_dragIndex < 0) return;
        var useSpatialFeedback = MdMotion.Resolve(this, MdMotionKind.Spatial, MdMotionSpeed.Fast).IsEnabled;
        var draggedHeight = _dragContainer is null
            ? 0
            : _dragContainer.Bounds.Height + _dragContainer.Margin.Top + _dragContainer.Margin.Bottom;
        foreach (var row in GetRealizedContainers().OfType<ListBoxItem>())
        {
            if (ReferenceEquals(row, _dragContainer)) continue;
            var index = IndexFromContainer(row);
            var targetY = useSpatialFeedback && _dropIndex > _dragIndex && index > _dragIndex && index <= _dropIndex
                ? -draggedHeight
                : useSpatialFeedback && _dropIndex < _dragIndex && index >= _dropIndex && index < _dragIndex
                    ? draggedHeight
                    : 0;
            GetReflowTransform(row).Y = targetY;
        }
    }

    private void AutoScroll(PointerEventArgs e)
    {
        var scroll = this.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (scroll is null || scroll.Viewport.Height <= 0 || scroll.Extent.Height <= scroll.Viewport.Height) return;
        var point = e.GetPosition(scroll);
        const double edge = 40;
        var velocity = point.Y < edge
            ? -Math.Clamp((edge - point.Y) / edge * 18, 4, 18)
            : point.Y > scroll.Viewport.Height - edge
                ? Math.Clamp((point.Y - (scroll.Viewport.Height - edge)) / edge * 18, 4, 18)
                : 0;
        if (velocity == 0) return;
        var previous = scroll.Offset.Y;
        var next = Math.Clamp(previous + velocity, 0, Math.Max(0, scroll.Extent.Height - scroll.Viewport.Height));
        if (Math.Abs(next - previous) < 0.01) return;
        scroll.Offset = new Vector(scroll.Offset.X, next);
        _autoScrollCompensation += next - previous;
        if (_dragTransform is not null)
            _dragTransform.Y += next - previous;
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        foreach (var pair in _reflowTransforms)
        {
            pair.Value.Transitions = ReferenceEquals(pair.Key, _dragContainer)
                ? null
                : MdMotionTransitions.Collect(MdMotionTransitions.CreateDouble(
                    this, TranslateTransform.YProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast));
        }
    }

    private void ResetDragVisuals()
    {
        if (_dragContainer is not null)
        {
            _dragContainer.Classes.Set("dragging", false);
            if (_dragTransform is not null)
            {
                _dragTransform.Transitions = MdMotionTransitions.Collect(MdMotionTransitions.CreateDouble(
                    this, TranslateTransform.YProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast));
                _dragTransform.Y = 0;
            }
            _dragContainer.ZIndex = 0;
        }
        foreach (var pair in _reflowTransforms)
        {
            pair.Value.Transitions ??= MdMotionTransitions.Collect(MdMotionTransitions.CreateDouble(
                this, TranslateTransform.YProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast));
            pair.Value.Y = 0;
        }
        _dropContainer?.Classes.Set("drop-target", false);
        _dragContainer = null;
        _dropContainer = null;
        _dragTransform = null;
        _dragIndex = -1;
        _dropIndex = -1;
        _autoScrollCompensation = 0;
        _dragStarted = false;
        PseudoClasses.Set(":reordering", false);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.Up && MoveSelectedUp()) e.Handled = true;
        else if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.Down && MoveSelectedDown()) e.Handled = true;
        else base.OnKeyDown(e);
    }
}

public sealed class MdReorderEventArgs(int oldIndex, int newIndex) : EventArgs
{
    public int OldIndex { get; } = oldIndex;
    public int NewIndex { get; } = newIndex;
    public bool Handled { get; set; }
}

/// <summary>A Material image/content tile with optional overlay header and footer bars.</summary>
[PseudoClasses(":has-header", ":has-footer", ":activated", ":favorite")]
public sealed class MdGridTile : Button
{
    public static readonly StyledProperty<object?> HeaderProperty = AvaloniaProperty.Register<MdGridTile, object?>(nameof(Header));
    public static readonly StyledProperty<object?> FooterProperty = AvaloniaProperty.Register<MdGridTile, object?>(nameof(Footer));
    public static readonly StyledProperty<bool> IsActivatedProperty = AvaloniaProperty.Register<MdGridTile, bool>(nameof(IsActivated), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> IsFavoriteProperty = AvaloniaProperty.Register<MdGridTile, bool>(nameof(IsFavorite), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    static MdGridTile()
    {
        HeaderProperty.Changed.AddClassHandler<MdGridTile>((tile, _) => tile.UpdatePseudoClasses());
        FooterProperty.Changed.AddClassHandler<MdGridTile>((tile, _) => tile.UpdatePseudoClasses());
        IsActivatedProperty.Changed.AddClassHandler<MdGridTile>((tile, _) => tile.UpdatePseudoClasses());
        IsFavoriteProperty.Changed.AddClassHandler<MdGridTile>((tile, _) => tile.UpdatePseudoClasses());
    }

    public MdGridTile() => UpdatePseudoClasses();
    public object? Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
    public object? Footer { get => GetValue(FooterProperty); set => SetValue(FooterProperty, value); }
    public bool IsActivated { get => GetValue(IsActivatedProperty); set => SetValue(IsActivatedProperty, value); }
    public bool IsFavorite { get => GetValue(IsFavoriteProperty); set => SetValue(IsFavoriteProperty, value); }
    public event EventHandler? Activated;
    public void Activate() { SetCurrentValue(IsActivatedProperty, true); Activated?.Invoke(this, EventArgs.Empty); }
    public void ToggleFavorite() => SetCurrentValue(IsFavoriteProperty, !IsFavorite);
    protected override void OnClick() { base.OnClick(); Activate(); }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":has-header", Header is not null);
        PseudoClasses.Set(":has-footer", Footer is not null);
        PseudoClasses.Set(":activated", IsActivated);
        PseudoClasses.Set(":favorite", IsFavorite);
    }
}

/// <summary>A one- or two-line overlay used in an <see cref="MdGridTile"/>.</summary>
[PseudoClasses(":has-subtitle", ":has-leading", ":has-trailing", ":interactive", ":pressed")]
public sealed class MdGridTileBar : ContentControl
{
    public static readonly StyledProperty<object?> LeadingProperty = AvaloniaProperty.Register<MdGridTileBar, object?>(nameof(Leading));
    public static readonly StyledProperty<object?> TitleProperty = AvaloniaProperty.Register<MdGridTileBar, object?>(nameof(Title));
    public static readonly StyledProperty<object?> SubtitleProperty = AvaloniaProperty.Register<MdGridTileBar, object?>(nameof(Subtitle));
    public static readonly StyledProperty<object?> TrailingProperty = AvaloniaProperty.Register<MdGridTileBar, object?>(nameof(Trailing));
    public static readonly StyledProperty<bool> IsInteractiveProperty = AvaloniaProperty.Register<MdGridTileBar, bool>(nameof(IsInteractive));
    public static readonly StyledProperty<ICommand?> CommandProperty = AvaloniaProperty.Register<MdGridTileBar, ICommand?>(nameof(Command));
    public static readonly StyledProperty<object?> CommandParameterProperty = AvaloniaProperty.Register<MdGridTileBar, object?>(nameof(CommandParameter));

    static MdGridTileBar()
    {
        LeadingProperty.Changed.AddClassHandler<MdGridTileBar>((bar, _) => bar.UpdatePseudoClasses());
        SubtitleProperty.Changed.AddClassHandler<MdGridTileBar>((bar, _) => bar.UpdatePseudoClasses());
        TrailingProperty.Changed.AddClassHandler<MdGridTileBar>((bar, _) => bar.UpdatePseudoClasses());
        IsInteractiveProperty.Changed.AddClassHandler<MdGridTileBar>((bar, _) => bar.UpdatePseudoClasses());
    }

    public MdGridTileBar() => UpdatePseudoClasses();
    public object? Leading { get => GetValue(LeadingProperty); set => SetValue(LeadingProperty, value); }
    public object? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public object? Subtitle { get => GetValue(SubtitleProperty); set => SetValue(SubtitleProperty, value); }
    public object? Trailing { get => GetValue(TrailingProperty); set => SetValue(TrailingProperty, value); }
    public bool IsInteractive { get => GetValue(IsInteractiveProperty); set => SetValue(IsInteractiveProperty, value); }
    public ICommand? Command { get => GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
    public object? CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }
    public event EventHandler? Invoked;

    public bool Invoke()
    {
        if (!IsInteractive) return false;
        if (Command?.CanExecute(CommandParameter) == true) Command.Execute(CommandParameter);
        Invoked?.Invoke(this, EventArgs.Empty);
        return true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!IsInteractive || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || IsInteractiveDescendant(e.Source as Visual)) return;
        PseudoClasses.Set(":pressed", true);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!ReferenceEquals(e.Pointer.Captured, this)) return;
        var invoke = new Rect(Bounds.Size).Contains(e.GetPosition(this));
        PseudoClasses.Set(":pressed", false);
        e.Pointer.Capture(null);
        if (invoke) Invoke();
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        PseudoClasses.Set(":pressed", false);
        base.OnPointerCaptureLost(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (IsInteractive && e.Key is Key.Enter or Key.Space && Invoke()) e.Handled = true;
        else base.OnKeyDown(e);
    }

    private bool IsInteractiveDescendant(Visual? source)
    {
        if (source is null) return false;
        foreach (var visual in source.GetVisualAncestors().Prepend(source))
        {
            if (ReferenceEquals(visual, this)) break;
            if (visual is Button) return true;
        }
        return false;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":has-leading", Leading is not null);
        PseudoClasses.Set(":has-subtitle", Subtitle is not null);
        PseudoClasses.Set(":has-trailing", Trailing is not null);
        PseudoClasses.Set(":interactive", IsInteractive);
        SetCurrentValue(FocusableProperty, IsInteractive);
        SetCurrentValue(IsTabStopProperty, IsInteractive);
    }
}

public enum MdDismissDirection { Horizontal, StartToEnd, EndToStart }

/// <summary>A swipe-dismiss surface with command, event, confirmation, and direct-operation paths.</summary>
[PseudoClasses(":dragging", ":confirming", ":dismissed", ":collapsing", ":present", ":start", ":end", ":reduced-motion", ":no-motion")]
public sealed class MdDismissible : ContentControl
{
    public static readonly StyledProperty<object?> BackgroundContentProperty = AvaloniaProperty.Register<MdDismissible, object?>(nameof(BackgroundContent));
    public static readonly StyledProperty<object?> SecondaryBackgroundContentProperty = AvaloniaProperty.Register<MdDismissible, object?>(nameof(SecondaryBackgroundContent));
    public static readonly StyledProperty<MdDismissDirection> DirectionProperty = AvaloniaProperty.Register<MdDismissible, MdDismissDirection>(nameof(Direction));
    public static readonly StyledProperty<double> DismissThresholdProperty = AvaloniaProperty.Register<MdDismissible, double>(nameof(DismissThreshold), 0.4);
    public static readonly StyledProperty<bool> IsDismissedProperty = AvaloniaProperty.Register<MdDismissible, bool>(nameof(IsDismissed), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<ICommand?> DismissCommandProperty = AvaloniaProperty.Register<MdDismissible, ICommand?>(nameof(DismissCommand));
    public static readonly StyledProperty<Func<MdDismissDirection, CancellationToken, Task<bool>>?> ConfirmDismissAsyncProperty =
        AvaloniaProperty.Register<MdDismissible, Func<MdDismissDirection, CancellationToken, Task<bool>>?>(nameof(ConfirmDismissAsync));
    public static readonly DirectProperty<MdDismissible, double> DragOffsetProperty = AvaloniaProperty.RegisterDirect<MdDismissible, double>(nameof(DragOffset), control => control.DragOffset);
    public static readonly DirectProperty<MdDismissible, bool> IsConfirmationPendingProperty =
        AvaloniaProperty.RegisterDirect<MdDismissible, bool>(nameof(IsConfirmationPending), control => control.IsConfirmationPending);
    public static readonly StyledProperty<double> VisualHeightProperty = AvaloniaProperty.Register<MdDismissible, double>(nameof(VisualHeight), double.NaN);

    private Point _start;
    private bool _dragging;
    private double _dragOffset;
    private Border? _root;
    private Border? _foreground;
    private TranslateTransform? _foregroundTransform;
    private readonly DispatcherTimer _collapseDelay = new();
    private readonly DispatcherTimer _collapseCompletion = new();
    private double _lastPointerX;
    private ulong _lastPointerTimestamp;
    private double _horizontalVelocity;
    private bool _isConfirmationPending;
    private CancellationTokenSource? _confirmationCancellation;
    private int _confirmationVersion;

    static MdDismissible()
    {
        IsDismissedProperty.Changed.AddClassHandler<MdDismissible>((control, _) => control.OnDismissedChanged());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdDismissible>((control, _) => control.UpdateMotion());
    }

    public MdDismissible()
    {
        _collapseDelay.Tick += (_, _) => BeginHeightCollapse();
        _collapseCompletion.Tick += (_, _) =>
        {
            _collapseCompletion.Stop();
            SetPresence(false);
        };
        SetPresence(true);
        UpdatePseudoClasses();
    }

    public object? BackgroundContent { get => GetValue(BackgroundContentProperty); set => SetValue(BackgroundContentProperty, value); }
    public object? SecondaryBackgroundContent { get => GetValue(SecondaryBackgroundContentProperty); set => SetValue(SecondaryBackgroundContentProperty, value); }
    public MdDismissDirection Direction { get => GetValue(DirectionProperty); set => SetValue(DirectionProperty, value); }
    public double DismissThreshold { get => GetValue(DismissThresholdProperty); set => SetValue(DismissThresholdProperty, value); }
    public bool IsDismissed { get => GetValue(IsDismissedProperty); set => SetValue(IsDismissedProperty, value); }
    public ICommand? DismissCommand { get => GetValue(DismissCommandProperty); set => SetValue(DismissCommandProperty, value); }
    public Func<MdDismissDirection, CancellationToken, Task<bool>>? ConfirmDismissAsync { get => GetValue(ConfirmDismissAsyncProperty); set => SetValue(ConfirmDismissAsyncProperty, value); }
    public double DragOffset => _dragOffset;
    public bool IsConfirmationPending { get => _isConfirmationPending; private set => SetAndRaise(IsConfirmationPendingProperty, ref _isConfirmationPending, value); }
    public double VisualHeight => GetValue(VisualHeightProperty);

    public event EventHandler<MdDismissRequestedEventArgs>? DismissRequested;
    public event EventHandler<Exception>? DismissConfirmationFailed;
    public event EventHandler? Dismissed;

    public bool Dismiss(MdDismissDirection direction = MdDismissDirection.StartToEnd)
    {
        if (!IsDirectionAllowed(direction) || IsDismissed || IsConfirmationPending) return false;
        if (ConfirmDismissAsync is not null)
        {
            var operation = DismissAsync(direction);
            return !operation.IsCompleted || operation.GetAwaiter().GetResult();
        }
        return RequestDismiss(direction) && CompleteDismiss(direction);
    }

    public async Task<bool> DismissAsync(
        MdDismissDirection direction = MdDismissDirection.StartToEnd,
        CancellationToken cancellationToken = default)
    {
        if (!IsDirectionAllowed(direction) || IsDismissed || IsConfirmationPending || !RequestDismiss(direction)) return false;
        var confirmation = ConfirmDismissAsync;
        if (confirmation is null) return CompleteDismiss(direction);

        var version = ++_confirmationVersion;
        _confirmationCancellation?.Cancel();
        _confirmationCancellation?.Dispose();
        var confirmationCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _confirmationCancellation = confirmationCancellation;
        IsConfirmationPending = true;
        PseudoClasses.Set(":confirming", true);
        try
        {
            if (!await confirmation(direction, confirmationCancellation.Token) ||
                version != _confirmationVersion || confirmationCancellation.IsCancellationRequested)
            {
                SetDragOffset(0);
                return false;
            }
            return CompleteDismiss(direction);
        }
        catch (OperationCanceledException) when (confirmationCancellation.IsCancellationRequested)
        {
            SetDragOffset(0);
            return false;
        }
        catch (Exception error)
        {
            SetDragOffset(0);
            DismissConfirmationFailed?.Invoke(this, error);
            return false;
        }
        finally
        {
            confirmationCancellation.Dispose();
            if (version == _confirmationVersion)
            {
                IsConfirmationPending = false;
                PseudoClasses.Set(":confirming", false);
                if (ReferenceEquals(_confirmationCancellation, confirmationCancellation))
                    _confirmationCancellation = null;
            }
        }
    }

    private bool RequestDismiss(MdDismissDirection direction)
    {
        var args = new MdDismissRequestedEventArgs(direction);
        DismissRequested?.Invoke(this, args);
        return !args.Cancel;
    }

    private bool CompleteDismiss(MdDismissDirection direction)
    {
        if (DismissCommand?.CanExecute(direction) == true) DismissCommand.Execute(direction);
        SetCurrentValue(IsDismissedProperty, true);
        SetDragOffset(direction == MdDismissDirection.EndToStart ? -Math.Max(1, Bounds.Width) : Math.Max(1, Bounds.Width));
        Dismissed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public void Restore()
    {
        CancelConfirmation();
        _collapseDelay.Stop();
        _collapseCompletion.Stop();
        SetPresence(true);
        SetCurrentValue(VisualHeightProperty, double.NaN);
        SetCurrentValue(IsDismissedProperty, false);
        SetDragOffset(0);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _root = e.NameScope.Find<Border>("PART_Root");
        _foreground = e.NameScope.Find<Border>("PART_Foreground");
        _foregroundTransform = _foreground?.RenderTransform as TranslateTransform;
        UpdateMotion();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (IsDismissed || IsConfirmationPending || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _start = e.GetPosition(this);
        _lastPointerX = _start.X;
        _lastPointerTimestamp = e.Timestamp;
        _horizontalVelocity = 0;
        _dragging = true;
        if (_foregroundTransform is not null) _foregroundTransform.Transitions = null;
        PseudoClasses.Set(":dragging", true);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_dragging) return;
        var position = e.GetPosition(this);
        var delta = position.X - _start.X;
        if (delta == 0 || !IsOffsetAllowed(delta)) return;
        var elapsed = e.Timestamp > _lastPointerTimestamp ? e.Timestamp - _lastPointerTimestamp : 1;
        _horizontalVelocity = (position.X - _lastPointerX) / elapsed;
        _lastPointerX = position.X;
        _lastPointerTimestamp = e.Timestamp;
        e.Pointer.Capture(this);
        SetDragOffset(delta);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_dragging) return;
        _dragging = false;
        PseudoClasses.Set(":dragging", false);
        UpdateMotion();
        var threshold = Math.Clamp(DismissThreshold, 0.05, 1) * Math.Max(1, Bounds.Width);
        var fling = Math.Abs(_horizontalVelocity) >= 0.6 && Math.Sign(_horizontalVelocity) == Math.Sign(DragOffset);
        if (Math.Abs(DragOffset) >= threshold || fling)
            Dismiss(DragOffset < 0 ? MdDismissDirection.EndToStart : MdDismissDirection.StartToEnd);
        else SetDragOffset(0);
        e.Pointer.Capture(null);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelConfirmation();
        _collapseDelay.Stop();
        _collapseCompletion.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private void OnDismissedChanged()
    {
        UpdatePseudoClasses();
        if (!IsDismissed)
        {
            _collapseDelay.Stop();
            _collapseCompletion.Stop();
            SetPresence(true);
            SetCurrentValue(VisualHeightProperty, double.NaN);
            return;
        }

        SetPresence(true);
        var exit = MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast);
        if (exit <= TimeSpan.Zero)
        {
            BeginHeightCollapse();
            return;
        }
        _collapseDelay.Stop();
        _collapseDelay.Interval = exit;
        _collapseDelay.Start();
    }

    private void BeginHeightCollapse()
    {
        _collapseDelay.Stop();
        PseudoClasses.Set(":collapsing", true);
        var height = Math.Max(0, Bounds.Height);
        SetCurrentValue(VisualHeightProperty, height);
        var spec = MdMotion.Resolve(this, MdMotionKind.Spatial, MdMotionSpeed.Default);
        if (!spec.IsEnabled)
        {
            SetCurrentValue(VisualHeightProperty, 0);
            SetPresence(false);
            return;
        }
        Dispatcher.UIThread.Post(() => SetCurrentValue(VisualHeightProperty, 0), DispatcherPriority.Render);
        _collapseCompletion.Stop();
        _collapseCompletion.Interval = spec.Duration;
        _collapseCompletion.Start();
    }

    private void SetPresence(bool value)
    {
        PseudoClasses.Set(":present", value);
        if (!value) PseudoClasses.Set(":collapsing", false);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_foreground is not null)
        {
            _foreground.Transitions = _dragging
                ? null
                : MdMotionTransitions.Collect(
                    MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        }
        if (_foregroundTransform is not null)
        {
            _foregroundTransform.Transitions = _dragging
                ? null
                : MdMotionTransitions.Collect(
                    MdMotionTransitions.CreateDouble(this, TranslateTransform.XProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast));
        }
        if (_root is not null)
        {
            _root.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, HeightProperty, MdMotionKind.Spatial));
        }
        if (IsDismissed) OnDismissedChanged();
    }

    private bool IsDirectionAllowed(MdDismissDirection direction) => Direction == MdDismissDirection.Horizontal || Direction == direction;
    private bool IsOffsetAllowed(double value) => Direction == MdDismissDirection.Horizontal || Direction == MdDismissDirection.StartToEnd && value > 0 || Direction == MdDismissDirection.EndToStart && value < 0;

    private void SetDragOffset(double value)
    {
        SetAndRaise(DragOffsetProperty, ref _dragOffset, value);
        PseudoClasses.Set(":start", value > 0);
        PseudoClasses.Set(":end", value < 0);
    }

    private void CancelConfirmation()
    {
        _confirmationVersion++;
        _confirmationCancellation?.Cancel();
        _confirmationCancellation?.Dispose();
        _confirmationCancellation = null;
        IsConfirmationPending = false;
        PseudoClasses.Set(":confirming", false);
    }

    private void UpdatePseudoClasses() => PseudoClasses.Set(":dismissed", IsDismissed);
}

public sealed class MdDismissRequestedEventArgs(MdDismissDirection direction) : EventArgs
{
    public MdDismissDirection Direction { get; } = direction;
    public bool Cancel { get; set; }
}
