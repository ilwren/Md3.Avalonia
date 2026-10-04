using System.Collections;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Localization;
using Md3.Avalonia.Motion;
using Path = Avalonia.Controls.Shapes.Path;

namespace Md3.Avalonia.Extra.Controls;

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
    public static readonly DirectProperty<MdCascader, bool> IsPopupOpenProperty = AvaloniaProperty.RegisterDirect<MdCascader, bool>(nameof(IsPopupOpen), control => control.IsPopupOpen);
    private IReadOnlyList<MdCascaderItem> _selectedPath = Array.Empty<MdCascaderItem>();
    private string _displayText = string.Empty;
    private IReadOnlyList<IReadOnlyList<MdCascaderItem>> _columns = Array.Empty<IReadOnlyList<MdCascaderItem>>();
    private bool _isPopupOpen;
    private readonly MdPresenceController _presence;
    private ItemsControl? _columnsHost;
    private Button? _anchorButton;
    private Border? _surface;
    private Popup? _popup;
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
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdCascader>((control, _) => control.UpdateDisplayText());
    }
    public MdCascader()
    {
        _presence = new MdPresenceController(SetPopupPresence);
        _presence.Initialize(IsDropDownOpen);
        AutomationProperties.SetName(this, Label ?? MdLocalization.GetString("HierarchySelector", this));
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
    /// <summary>The actual popup-host lifetime, including the exit-motion interval.</summary>
    public bool IsPopupOpen
    {
        get => _isPopupOpen;
        private set => SetAndRaise(IsPopupOpenProperty, ref _isPopupOpen, value);
    }
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
        if (_columnsHost is not null)
        {
            _columnsHost.RemoveHandler(ListBox.SelectionChangedEvent, OnColumnSelectionChanged);
            _columnsHost.RemoveHandler(InputElement.KeyDownEvent, OnPopupKeyDown);
        }
        if (_anchorButton is not null) _anchorButton.Click -= OnAnchorClick;
        if (_popup is not null) _popup.Closed -= OnPopupClosed;
        base.OnApplyTemplate(e);
        _columnsHost = e.NameScope.Find<ItemsControl>("PART_Columns");
        _anchorButton = e.NameScope.Find<Button>("PART_Anchor");
        _surface = e.NameScope.Find<Border>("PART_Surface");
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _arrow = e.NameScope.Find<Path>("PART_Arrow");
        _columnsHost?.AddHandler(ListBox.SelectionChangedEvent, OnColumnSelectionChanged);
        _columnsHost?.AddHandler(InputElement.KeyDownEvent, OnPopupKeyDown, RoutingStrategies.Tunnel, true);
        if (_anchorButton is not null) _anchorButton.Click += OnAnchorClick;
        if (_popup is not null) _popup.Closed += OnPopupClosed;
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
        if (!HandlePopupKey(e)) base.OnKeyDown(e);
    }

    private void OnPopupKeyDown(object? sender, KeyEventArgs e) => HandlePopupKey(e);

    private bool HandlePopupKey(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsDropDownOpen)
        {
            IsDropDownOpen = false;
            _anchorButton?.Focus();
            e.Handled = true;
            return true;
        }

        if (!IsDropDownOpen && e.Key is Key.Enter or Key.Space or Key.Down)
        {
            IsDropDownOpen = true;
            FocusColumn(Math.Max(0, SelectedPath.Count - 1));
            e.Handled = true;
            return true;
        }

        if (IsDropDownOpen && e.Source is Visual source)
        {
            var lists = GetColumnLists();
            var current = source as ListBox ?? source.FindAncestorOfType<ListBox>();
            var level = current is null ? -1 : Array.IndexOf(lists, current);
            var expandKey = FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft ? Key.Left : Key.Right;
            var collapseKey = expandKey == Key.Right ? Key.Left : Key.Right;
            if (level >= 0 && e.Key == expandKey && level + 1 < lists.Length)
            {
                FocusColumn(level + 1);
                e.Handled = true;
                return true;
            }
            if (level > 0 && e.Key == collapseKey)
            {
                TrimPath(level);
                FocusColumn(level - 1);
                e.Handled = true;
                return true;
            }
        }
        return false;
    }
    private void OnColumnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.Source is not ListBox list || list.SelectedItem is not MdCascaderItem item) return;
        var level = Enumerable.Range(0, Columns.Count).FirstOrDefault(index => ReferenceEquals(Columns[index], list.ItemsSource));
        Select(level, item);
        list.SelectedItem = null;
        if (item.HasChildren) Dispatcher.UIThread.Post(() => FocusColumn(level + 1), DispatcherPriority.Input);
        e.Handled = true;
    }

    private ListBox[] GetColumnLists() => _columnsHost?.GetVisualDescendants().OfType<ListBox>().ToArray() ?? [];

    private void FocusColumn(int level)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var lists = GetColumnLists();
            if (lists.Length == 0) return;
            level = Math.Clamp(level, 0, lists.Length - 1);
            var list = lists[level];
            // Moving keyboard focus must not commit a hierarchy choice. Selecting index zero here
            // raised SelectionChanged merely by opening the popup and could expand a branch before
            // the user invoked it.
            var focusIndex = list.SelectedIndex >= 0 ? list.SelectedIndex : 0;
            list.Focus(NavigationMethod.Directional);
            if (list.ItemCount > 0)
                (list.ContainerFromIndex(focusIndex) as Control)?.Focus(NavigationMethod.Directional);
        }, DispatcherPriority.Input);
    }

    private void TrimPath(int level)
    {
        if (level <= 0 || SelectedPath.Count < level) return;
        var path = SelectedPath.Take(level).ToArray();
        SetAndRaise(SelectedPathProperty, ref _selectedPath, path);
        var columns = new List<IReadOnlyList<MdCascaderItem>> { (ItemsSource ?? Array.Empty<MdCascaderItem>()).ToArray() };
        foreach (var selected in path)
            if (selected.Children is { Count: > 0 }) columns.Add(selected.Children);
        SetAndRaise(ColumnsProperty, ref _columns, columns);
        UpdateDisplayText();
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
        AutomationProperties.SetName(this, Label ?? MdLocalization.GetString("HierarchySelector", this));
        AutomationProperties.SetHelpText(this, value);
        PseudoClasses.Set(":has-value", SelectedPath.Count > 0);
    }

    void IMdPopupPresenceOwner.ClosePopupImmediately() => _presence.Initialize(false);

    private void SetPopupPresence(bool value)
    {
        IsPopupOpen = value;
        PseudoClasses.Set(":present", value);
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        if (IsDropDownOpen) SetCurrentValue(IsDropDownOpenProperty, false);
        _presence.Initialize(false);
    }

    private void UpdateOpenState()
    {
        MdPopupCoordinator.NotifyStateChanged(this);
        var version = ++_openStateVersion;
        if (IsDropDownOpen)
        {
            _presence.Update(true, TimeSpan.Zero);
            if (_isAttached) FocusColumn(Math.Max(0, SelectedPath.Count - 1));
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
                    }, DispatcherPriority.Loaded);
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

/// <summary>Responsive three-region layout used by <see cref="MdTransfer"/>.</summary>
public sealed class MdTransferLayoutPanel : Panel
{
    public static readonly StyledProperty<bool> IsCompactProperty = AvaloniaProperty.Register<MdTransferLayoutPanel, bool>(nameof(IsCompact));
    public static readonly StyledProperty<double> SpacingProperty = AvaloniaProperty.Register<MdTransferLayoutPanel, double>(nameof(Spacing), 12);

    static MdTransferLayoutPanel() => AffectsMeasure<MdTransferLayoutPanel>(IsCompactProperty, SpacingProperty);

    public bool IsCompact { get => GetValue(IsCompactProperty); set => SetValue(IsCompactProperty, value); }
    public double Spacing { get => GetValue(SpacingProperty); set => SetValue(SpacingProperty, value); }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count == 0) return default;
        if (IsCompact)
        {
            var compactHeight = 0d;
            var compactWidth = 0d;
            foreach (var child in Children)
            {
                child.Measure(new Size(availableSize.Width, double.PositiveInfinity));
                compactWidth = Math.Max(compactWidth, child.DesiredSize.Width);
                compactHeight += child.DesiredSize.Height;
            }
            compactHeight += Math.Max(0, Children.Count - 1) * Spacing;
            return new Size(compactWidth, compactHeight);
        }

        var actions = Children.Count > 1 ? Children[1] : null;
        actions?.Measure(new Size(double.PositiveInfinity, availableSize.Height));
        var actionWidth = actions?.DesiredSize.Width ?? 0;
        var availableColumnWidth = double.IsInfinity(availableSize.Width)
            ? double.PositiveInfinity
            : Math.Max(0, (availableSize.Width - actionWidth - Math.Max(0, Children.Count - 1) * Spacing) / Math.Max(1, Children.Count - 1));
        var width = actionWidth;
        var height = actions?.DesiredSize.Height ?? 0;
        for (var index = 0; index < Children.Count; index++)
        {
            if (index == 1) continue;
            Children[index].Measure(new Size(availableColumnWidth, availableSize.Height));
            width += Children[index].DesiredSize.Width;
            height = Math.Max(height, Children[index].DesiredSize.Height);
        }
        width += Math.Max(0, Children.Count - 1) * Spacing;
        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (IsCompact)
        {
            var y = 0d;
            foreach (var child in Children)
            {
                child.Arrange(new Rect(0, y, finalSize.Width, child.DesiredSize.Height));
                y += child.DesiredSize.Height + Spacing;
            }
            return finalSize;
        }

        var actionWidth = Children.Count > 1 ? Children[1].DesiredSize.Width : 0;
        var columnWidth = Math.Max(0, (finalSize.Width - actionWidth - Math.Max(0, Children.Count - 1) * Spacing) / Math.Max(1, Children.Count - 1));
        var rtl = FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft;
        var x = rtl ? finalSize.Width : 0d;
        for (var index = 0; index < Children.Count; index++)
        {
            var width = index == 1 ? actionWidth : columnWidth;
            if (rtl) x -= width;
            Children[index].Arrange(new Rect(x, 0, width, finalSize.Height));
            x += rtl ? -Spacing : width + Spacing;
        }
        return finalSize;
    }
}

public enum MdTransferLayoutMode
{
    Auto,
    Standard,
    Compact
}

/// <summary>Dual-list transfer control with checked moves, select-all, pointer drag/drop, keyboard APIs, and stable source order.</summary>
[PseudoClasses(":dragging", ":drag-to-source", ":drag-to-target", ":compact", ":rtl")]
public sealed class MdTransfer : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = AvaloniaProperty.Register<MdTransfer, IEnumerable?>(nameof(ItemsSource));
    public static readonly StyledProperty<IEnumerable?> SelectedItemsProperty = AvaloniaProperty.Register<MdTransfer, IEnumerable?>(nameof(SelectedItems), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<string?> SourceFilterProperty = AvaloniaProperty.Register<MdTransfer, string?>(nameof(SourceFilter));
    public static readonly StyledProperty<string?> TargetFilterProperty = AvaloniaProperty.Register<MdTransfer, string?>(nameof(TargetFilter));
    public static readonly StyledProperty<bool> IsDragEnabledProperty = AvaloniaProperty.Register<MdTransfer, bool>(nameof(IsDragEnabled), true);
    public static readonly StyledProperty<MdTransferLayoutMode> LayoutModeProperty = AvaloniaProperty.Register<MdTransfer, MdTransferLayoutMode>(nameof(LayoutMode));
    public static readonly DirectProperty<MdTransfer, IReadOnlyList<object?>> AvailableItemsProperty = AvaloniaProperty.RegisterDirect<MdTransfer, IReadOnlyList<object?>>(nameof(AvailableItems), control => control.AvailableItems);
    public static readonly DirectProperty<MdTransfer, IReadOnlyList<object?>> TargetItemsProperty = AvaloniaProperty.RegisterDirect<MdTransfer, IReadOnlyList<object?>>(nameof(TargetItems), control => control.TargetItems);
    public static readonly DirectProperty<MdTransfer, string> StatusTextProperty = AvaloniaProperty.RegisterDirect<MdTransfer, string>(nameof(StatusText), control => control.StatusText);
    public static readonly DirectProperty<MdTransfer, string> AllTextProperty = AvaloniaProperty.RegisterDirect<MdTransfer, string>(nameof(AllText), control => control.AllText);
    public static readonly DirectProperty<MdTransfer, string> SourceFilterLabelProperty = AvaloniaProperty.RegisterDirect<MdTransfer, string>(nameof(SourceFilterLabel), control => control.SourceFilterLabel);
    public static readonly DirectProperty<MdTransfer, string> TargetFilterLabelProperty = AvaloniaProperty.RegisterDirect<MdTransfer, string>(nameof(TargetFilterLabel), control => control.TargetFilterLabel);
    public static readonly DirectProperty<MdTransfer, bool> IsCompactProperty = AvaloniaProperty.RegisterDirect<MdTransfer, bool>(nameof(IsCompact), control => control.IsCompact);
    private IReadOnlyList<object?> _available = Array.Empty<object?>();
    private IReadOnlyList<object?> _target = Array.Empty<object?>();
    private string _statusText = string.Empty;
    private string _allText = string.Empty;
    private string _sourceFilterLabel = string.Empty;
    private string _targetFilterLabel = string.Empty;
    private bool _isCompact;
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
    private Image? _dragGhost;
    private OverlayLayer? _dragGhostLayer;
    private RenderTargetBitmap? _dragGhostBitmap;
    private Vector _dragGhostOffset;
    private int _transferAnimationVersion;
    static MdTransfer()
    {
        ItemsSourceProperty.Changed.AddClassHandler<MdTransfer>((control, _) => control.Refresh());
        SelectedItemsProperty.Changed.AddClassHandler<MdTransfer>((control, _) => control.Refresh());
        SourceFilterProperty.Changed.AddClassHandler<MdTransfer>((control, _) => control.Refresh());
        TargetFilterProperty.Changed.AddClassHandler<MdTransfer>((control, _) => control.Refresh());
        LayoutModeProperty.Changed.AddClassHandler<MdTransfer>((control, _) => control.UpdateLayoutState());
        BoundsProperty.Changed.AddClassHandler<MdTransfer>((control, _) => control.UpdateLayoutState());
        FlowDirectionProperty.Changed.AddClassHandler<MdTransfer>((control, _) => control.UpdateLayoutState());
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdTransfer>((control, _) => control.UpdateLocalizedAutomation());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdTransfer>((control, _) => control.CancelTransferMotion());
    }
    public MdTransfer()
    {
        AutomationProperties.SetName(this, MdLocalization.GetString("TransferList", this));
        AutomationProperties.SetLiveSetting(this, AutomationLiveSetting.Polite);
        Refresh();
        UpdateLocalizedAutomation();
        UpdateLayoutState();
    }
    public IEnumerable? ItemsSource { get => GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public IEnumerable? SelectedItems { get => GetValue(SelectedItemsProperty); set => SetValue(SelectedItemsProperty, value); }
    public string? SourceFilter { get => GetValue(SourceFilterProperty); set => SetValue(SourceFilterProperty, value); }
    public string? TargetFilter { get => GetValue(TargetFilterProperty); set => SetValue(TargetFilterProperty, value); }
    public bool IsDragEnabled { get => GetValue(IsDragEnabledProperty); set => SetValue(IsDragEnabledProperty, value); }
    public MdTransferLayoutMode LayoutMode { get => GetValue(LayoutModeProperty); set => SetValue(LayoutModeProperty, value); }
    public IReadOnlyList<object?> AvailableItems => _available;
    public IReadOnlyList<object?> TargetItems => _target;
    public string StatusText { get => _statusText; private set => SetAndRaise(StatusTextProperty, ref _statusText, value); }
    public string AllText => _allText;
    public string SourceFilterLabel => _sourceFilterLabel;
    public string TargetFilterLabel => _targetFilterLabel;
    public bool IsCompact => _isCompact;
    public event EventHandler? SelectionChanged;
    public void MoveToTarget(IEnumerable<object?> items)
    {
        var current = (SelectedItems ?? Array.Empty<object>()).Cast<object?>().ToList();
        var moved = items.Where(item => !current.Contains(item)).Distinct().ToArray();
        if (moved.Length == 0) return;
        var previousLayout = CaptureItemLayout();
        foreach (var item in moved) current.Add(item);
        SelectedItems = current;
        StatusText = MdLocalization.Format("MovedItemsToSelected", this, moved.Length);
        AutomationProperties.SetHelpText(this, StatusText);
        ScheduleTransferMotion(moved, previousLayout);
        FocusTransferredItem(moved[0], _targetList);
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
        StatusText = MdLocalization.Format("MovedItemsToAvailable", this, moved.Length);
        AutomationProperties.SetHelpText(this, StatusText);
        ScheduleTransferMotion(moved, previousLayout);
        FocusTransferredItem(moved[0], _availableList);
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
        if (_availableList is not null)
        {
            _availableList.KeyDown += OnListKeyDown;
            _availableList.AddHandler(InputElement.PointerPressedEvent, OnListPointerPressed, RoutingStrategies.Tunnel, true);
        }
        if (_targetList is not null)
        {
            _targetList.KeyDown += OnListKeyDown;
            _targetList.AddHandler(InputElement.PointerPressedEvent, OnListPointerPressed, RoutingStrategies.Tunnel, true);
        }
        UpdateLocalizedAutomation();
        UpdateLayoutState();
    }

    private void UpdateLocalizedAutomation()
    {
        SetAndRaise(AllTextProperty, ref _allText, MdLocalization.GetString("All", this));
        SetAndRaise(SourceFilterLabelProperty, ref _sourceFilterLabel, MdLocalization.GetString("FilterAvailable", this));
        SetAndRaise(TargetFilterLabelProperty, ref _targetFilterLabel, MdLocalization.GetString("FilterSelected", this));
        AutomationProperties.SetName(this, MdLocalization.GetString("TransferList", this));
        if (_availableList is not null) AutomationProperties.SetName(_availableList, MdLocalization.GetString("AvailableItems", this));
        if (_targetList is not null) AutomationProperties.SetName(_targetList, MdLocalization.GetString("SelectedItems", this));
        if (_moveTarget is not null) AutomationProperties.SetName(_moveTarget, MdLocalization.GetString("MoveSelectedToTarget", this));
        if (_moveSource is not null) AutomationProperties.SetName(_moveSource, MdLocalization.GetString("MoveSelectedToSource", this));
        if (_moveAllTarget is not null) AutomationProperties.SetName(_moveAllTarget, MdLocalization.GetString("MoveAllToTarget", this));
        if (_moveAllSource is not null) AutomationProperties.SetName(_moveAllSource, MdLocalization.GetString("MoveAllToSource", this));
    }

    private void UpdateLayoutState()
    {
        var compact = LayoutMode == MdTransferLayoutMode.Compact ||
                      (LayoutMode == MdTransferLayoutMode.Auto && Bounds.Width > 0 && Bounds.Width < 600);
        SetAndRaise(IsCompactProperty, ref _isCompact, compact);
        PseudoClasses.Set(":compact", compact);
        PseudoClasses.Set(":rtl", FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft);
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
        if (_availableList is not null)
        {
            _availableList.RemoveHandler(InputElement.PointerPressedEvent, OnListPointerPressed);
            _availableList.KeyDown -= OnListKeyDown;
        }
        if (_targetList is not null)
        {
            _targetList.RemoveHandler(InputElement.PointerPressedEvent, OnListPointerPressed);
            _targetList.KeyDown -= OnListKeyDown;
        }
        if (_moveTarget is not null) _moveTarget.Click -= OnMoveTarget;
        if (_moveSource is not null) _moveSource.Click -= OnMoveSource;
        if (_moveAllTarget is not null) _moveAllTarget.Click -= OnMoveAllTarget;
        if (_moveAllSource is not null) _moveAllSource.Click -= OnMoveAllSource;
    }
    private void OnListKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not ListBox list || !e.KeyModifiers.HasFlag(KeyModifiers.Control)) return;
        var moveToTargetKey = FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft ? Key.Left : Key.Right;
        var moveToSourceKey = moveToTargetKey == Key.Right ? Key.Left : Key.Right;
        if (ReferenceEquals(list, _availableList) && (e.Key == moveToTargetKey || e.Key == Key.Enter))
        {
            MoveToTarget((list.SelectedItems ?? Array.Empty<object>()).Cast<object?>());
            e.Handled = true;
        }
        else if (ReferenceEquals(list, _targetList) && (e.Key == moveToSourceKey || e.Key == Key.Back))
        {
            MoveToSource((list.SelectedItems ?? Array.Empty<object>()).Cast<object?>());
            e.Handled = true;
        }
    }

    private void FocusTransferredItem(object? item, ListBox? destination)
    {
        if (destination is null || item is null) return;
        Dispatcher.UIThread.Post(() =>
        {
            destination.SelectedItem = item;
            destination.ScrollIntoView(item);
            (destination.ContainerFromItem(item) as Control)?.Focus(NavigationMethod.Directional);
        }, DispatcherPriority.Input);
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
        EnsureDragGhost();
        MoveDragGhost(position);
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

    /// <summary>
    /// Raises a picture of the dragged row into the overlay layer.
    /// </summary>
    /// <remarks>
    /// Translating the row itself kept it inside its own ListBox, so the scroll viewport clipped
    /// it the moment it moved towards the other list: the row people were dragging slid under its
    /// siblings and out of sight exactly when they needed to aim it. The overlay layer sits above
    /// both lists and nothing clips it, so the ghost stays under the pointer the whole way across.
    /// </remarks>
    private void EnsureDragGhost()
    {
        if (_dragGhost is not null || _dragContainer is null) return;
        var layer = OverlayLayer.GetOverlayLayer(this);
        if (layer is null) return;
        var bounds = _dragContainer.Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0) return;
        if (_dragContainer.TranslatePoint(default, this) is not { } origin) return;

        // Where inside the row the pointer went down, so the ghost keeps the same grip.
        _dragGhostOffset = _dragStart - origin;

        var scale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        var pixelSize = new PixelSize(
            Math.Max(1, (int)Math.Ceiling(bounds.Width * scale)),
            Math.Max(1, (int)Math.Ceiling(bounds.Height * scale)));
        var bitmap = new RenderTargetBitmap(pixelSize, new Vector(96 * scale, 96 * scale));
        bitmap.Render(_dragContainer);

        _dragGhostBitmap = bitmap;
        _dragGhost = new Image
        {
            Source = bitmap,
            Width = bounds.Width,
            Height = bounds.Height,
            Opacity = 0.85,
            IsHitTestVisible = false
        };
        _dragGhostLayer = layer;
        layer.Children.Add(_dragGhost);

        // CancelTransferMotion deliberately skips the dragged container, so dimming it here is
        // not fought by the transfer animation.
        _dragContainer.Opacity = 0.4;
    }

    private void MoveDragGhost(Point position)
    {
        if (_dragGhost is null || _dragGhostLayer is null) return;
        if (this.TranslatePoint(position - _dragGhostOffset, _dragGhostLayer) is not { } target) return;
        Canvas.SetLeft(_dragGhost, target.X);
        Canvas.SetTop(_dragGhost, target.Y);
    }

    private void FinishDragVisual()
    {
        if (_dragGhost is not null) _dragGhostLayer?.Children.Remove(_dragGhost);
        _dragGhost = null;
        _dragGhostLayer = null;
        _dragGhostBitmap?.Dispose();
        _dragGhostBitmap = null;
        if (_dragContainer is not null)
        {
            _dragContainer.Opacity = 1;
            _dragContainer.ZIndex = 0;
        }
        _dragContainer = null;
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
        if (string.IsNullOrWhiteSpace(StatusText))
            StatusText = $"{available.Length} available, {target.Length} selected";
        AutomationProperties.SetHelpText(this, StatusText);
    }
    private static bool Matches(object? item, string? filter) => string.IsNullOrWhiteSpace(filter) || item?.ToString()?.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase) == true;
}
