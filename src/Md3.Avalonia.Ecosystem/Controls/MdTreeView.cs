using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Ecosystem.Controls;

/// <summary>A mutable, binding-friendly node used by <see cref="MdTreeView"/>.</summary>
public sealed class MdTreeNode : INotifyPropertyChanged
{
    private string _label;
    private bool _isExpanded;
    private object? _data;

    public MdTreeNode(string id, string label, IEnumerable<MdTreeNode>? children = null, object? data = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
        _label = label;
        _data = data;
        Children = children is null ? [] : new ObservableCollection<MdTreeNode>(children);
    }

    public string Id { get; }
    public string Label { get => _label; set => Set(ref _label, value); }
    public object? Data { get => _data; set => Set(ref _data, value); }
    public bool IsExpanded { get => _isExpanded; set => Set(ref _isExpanded, value); }
    public ObservableCollection<MdTreeNode> Children { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>Flattened row state projected by <see cref="MdTreeView"/>.</summary>
public sealed record MdTreeRow(
    MdTreeNode Node,
    int Depth,
    MdTreeNode? Parent,
    Thickness Indentation,
    bool HasChildren,
    bool IsExpanded,
    double ExpansionAngle,
    bool ShowGuide);

/// <summary>
/// A Material hierarchical list with arbitrary depth, selection, keyboard traversal, RTL-aware
/// indentation and direct expand/collapse APIs. It derives from ListBox to retain native selection
/// and automation semantics without globally styling Avalonia's TreeView.
/// </summary>
public sealed class MdTreeView : ListBox
{
    public static readonly StyledProperty<IEnumerable<MdTreeNode>?> RootsProperty =
        AvaloniaProperty.Register<MdTreeView, IEnumerable<MdTreeNode>?>(nameof(Roots));

    public static readonly StyledProperty<MdTreeNode?> SelectedNodeProperty =
        AvaloniaProperty.Register<MdTreeView, MdTreeNode?>(nameof(SelectedNode), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> IndentationProperty =
        AvaloniaProperty.Register<MdTreeView, double>(nameof(Indentation), 24, validate: value => value >= 12);

    public static readonly StyledProperty<bool> ShowGuidesProperty =
        AvaloniaProperty.Register<MdTreeView, bool>(nameof(ShowGuides), false);

    public static readonly StyledProperty<ICommand?> NodeInvokedCommandProperty =
        AvaloniaProperty.Register<MdTreeView, ICommand?>(nameof(NodeInvokedCommand));

    public static readonly StyledProperty<ICommand?> ExpansionChangedCommandProperty =
        AvaloniaProperty.Register<MdTreeView, ICommand?>(nameof(ExpansionChangedCommand));

    public static readonly DirectProperty<MdTreeView, IReadOnlyList<MdTreeRow>> VisibleRowsProperty =
        AvaloniaProperty.RegisterDirect<MdTreeView, IReadOnlyList<MdTreeRow>>(nameof(VisibleRows), control => control.VisibleRows);

    private IReadOnlyList<MdTreeRow> _visibleRows = [];
    private INotifyCollectionChanged? _rootCollection;
    private readonly HashSet<MdTreeNode> _observedNodes = [];
    private bool _syncingSelection;
    private bool _suppressNodeRebuild;
    private int _animationVersion;

    static MdTreeView()
    {
        RootsProperty.Changed.AddClassHandler<MdTreeView>((control, _) => control.OnRootsChanged());
        SelectedNodeProperty.Changed.AddClassHandler<MdTreeView>((control, _) => control.SyncSelectedRow());
        IndentationProperty.Changed.AddClassHandler<MdTreeView>((control, _) => control.Rebuild());
        ShowGuidesProperty.Changed.AddClassHandler<MdTreeView>((control, _) => control.Rebuild());
        FlowDirectionProperty.Changed.AddClassHandler<MdTreeView>((control, _) => control.Rebuild());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdTreeView>((control, _) => control.CancelMotion());
    }

    public MdTreeView()
    {
        AutomationProperties.SetName(this, "Hierarchy");
        SelectionChanged += OnSelectionChanged;
        AddHandler(Button.ClickEvent, OnButtonClick, RoutingStrategies.Bubble, true);
        DoubleTapped += (_, _) =>
        {
            if (SelectedNode is { } node) Invoke(node);
        };
    }

    public IEnumerable<MdTreeNode>? Roots { get => GetValue(RootsProperty); set => SetValue(RootsProperty, value); }
    public MdTreeNode? SelectedNode { get => GetValue(SelectedNodeProperty); set => SetValue(SelectedNodeProperty, value); }
    public double Indentation { get => GetValue(IndentationProperty); set => SetValue(IndentationProperty, value); }
    public bool ShowGuides { get => GetValue(ShowGuidesProperty); set => SetValue(ShowGuidesProperty, value); }
    public ICommand? NodeInvokedCommand { get => GetValue(NodeInvokedCommandProperty); set => SetValue(NodeInvokedCommandProperty, value); }
    public ICommand? ExpansionChangedCommand { get => GetValue(ExpansionChangedCommandProperty); set => SetValue(ExpansionChangedCommandProperty, value); }
    public IReadOnlyList<MdTreeRow> VisibleRows { get => _visibleRows; private set => SetAndRaise(VisibleRowsProperty, ref _visibleRows, value); }

    public event EventHandler<MdTreeNode>? NodeInvoked;
    public event EventHandler<MdTreeNode>? ExpansionChanged;

    /// <summary>Expands a node and refreshes the visible flattened rows.</summary>
    public bool Expand(MdTreeNode node) => SetExpanded(node, true);

    /// <summary>Collapses a node and its visible descendants.</summary>
    public bool Collapse(MdTreeNode node) => SetExpanded(node, false);

    public bool Toggle(MdTreeNode node) => SetExpanded(node, !node.IsExpanded);

    public void ExpandAll()
    {
        var previousNodes = VisibleRows.Select(row => row.Node).ToHashSet();
        var previousLayout = CaptureRowLayout();
        _suppressNodeRebuild = true;
        try { foreach (var node in Traverse()) node.IsExpanded = node.Children.Count > 0; }
        finally { _suppressNodeRebuild = false; }
        Rebuild(previousNodes, previousLayout);
    }

    public void CollapseAll()
    {
        var previousNodes = VisibleRows.Select(row => row.Node).ToHashSet();
        var previousLayout = CaptureRowLayout();
        _suppressNodeRebuild = true;
        try { foreach (var node in Traverse()) node.IsExpanded = false; }
        finally { _suppressNodeRebuild = false; }
        Rebuild(previousNodes, previousLayout);
    }

    public bool SelectById(string id)
    {
        var node = Traverse().FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.Ordinal));
        if (node is null) return false;
        Reveal(node);
        SetCurrentValue(SelectedNodeProperty, node);
        return true;
    }

    public void Invoke(MdTreeNode node)
    {
        SetCurrentValue(SelectedNodeProperty, node);
        if (NodeInvokedCommand?.CanExecute(node) == true) NodeInvokedCommand.Execute(node);
        NodeInvoked?.Invoke(this, node);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelMotion();
        base.OnDetachedFromVisualTree(e);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_syncingSelection) return;
        var node = (SelectedItem as MdTreeRow)?.Node;
        if (!ReferenceEquals(node, SelectedNode)) SetCurrentValue(SelectedNodeProperty, node);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var row = SelectedItem as MdTreeRow;
        if (row is not null)
        {
            var expandKey = FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft ? Key.Left : Key.Right;
            var collapseKey = expandKey == Key.Right ? Key.Left : Key.Right;
            if (e.Key == expandKey)
            {
                if (row.HasChildren && !row.IsExpanded) Expand(row.Node);
                else if (row.Node.Children.Count > 0) SetCurrentValue(SelectedNodeProperty, row.Node.Children[0]);
                e.Handled = true;
            }
            else if (e.Key == collapseKey)
            {
                if (row.IsExpanded) Collapse(row.Node);
                else if (row.Parent is not null) SetCurrentValue(SelectedNodeProperty, row.Parent);
                e.Handled = true;
            }
            else if (e.Key is Key.Enter or Key.Space)
            {
                Invoke(row.Node);
                e.Handled = true;
            }
        }
        if (!e.Handled) base.OnKeyDown(e);
    }

    private bool SetExpanded(MdTreeNode node, bool expanded)
    {
        if (node.Children.Count == 0 || node.IsExpanded == expanded) return false;
        var previousNodes = VisibleRows.Select(row => row.Node).ToHashSet();
        var previousLayout = CaptureRowLayout();
        _suppressNodeRebuild = true;
        try { node.IsExpanded = expanded; }
        finally { _suppressNodeRebuild = false; }
        Rebuild(previousNodes, previousLayout);
        if (ExpansionChangedCommand?.CanExecute(node) == true) ExpansionChangedCommand.Execute(node);
        ExpansionChanged?.Invoke(this, node);
        return true;
    }

    private void Reveal(MdTreeNode target)
    {
        var path = new List<MdTreeNode>();
        if (!FindPath(Roots ?? [], target, path, [])) return;
        var previousNodes = VisibleRows.Select(row => row.Node).ToHashSet();
        var previousLayout = CaptureRowLayout();
        _suppressNodeRebuild = true;
        try { foreach (var ancestor in path.Take(path.Count - 1)) ancestor.IsExpanded = true; }
        finally { _suppressNodeRebuild = false; }
        Rebuild(previousNodes, previousLayout);
    }

    private static bool FindPath(IEnumerable<MdTreeNode> nodes, MdTreeNode target, List<MdTreeNode> path, HashSet<MdTreeNode> visited)
    {
        foreach (var node in nodes)
        {
            if (!visited.Add(node)) continue;
            path.Add(node);
            if (ReferenceEquals(node, target) || FindPath(node.Children, target, path, visited)) return true;
            path.RemoveAt(path.Count - 1);
        }
        return false;
    }

    private IEnumerable<MdTreeNode> Traverse()
    {
        var stack = new Stack<MdTreeNode>((Roots ?? []).Reverse());
        var visited = new HashSet<MdTreeNode>();
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (!visited.Add(node)) continue;
            yield return node;
            for (var i = node.Children.Count - 1; i >= 0; i--) stack.Push(node.Children[i]);
        }
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is Button { Name: "PART_Toggle", DataContext: MdTreeRow row })
        {
            Toggle(row.Node);
            e.Handled = true;
        }
    }

    private void OnRootsChanged()
    {
        if (_rootCollection is not null) _rootCollection.CollectionChanged -= OnRootCollectionChanged;
        _rootCollection = Roots as INotifyCollectionChanged;
        if (_rootCollection is not null) _rootCollection.CollectionChanged += OnRootCollectionChanged;
        Rebuild();
    }

    private void OnRootCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();
    private void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();
    private void OnNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_suppressNodeRebuild) return;
        if (e.PropertyName == nameof(MdTreeNode.IsExpanded))
            Rebuild(VisibleRows.Select(row => row.Node).ToHashSet(), CaptureRowLayout());
        else
            Rebuild();
    }

    private void Rebuild(HashSet<MdTreeNode>? previousNodes = null, Dictionary<MdTreeNode, double>? previousLayout = null)
    {
        foreach (var node in _observedNodes)
        {
            node.PropertyChanged -= OnNodePropertyChanged;
            node.Children.CollectionChanged -= OnChildrenChanged;
        }
        _observedNodes.Clear();

        var rows = new List<MdTreeRow>();
        var visited = new HashSet<MdTreeNode>();
        foreach (var root in Roots ?? []) AddVisible(root, null, 0, rows, visited);
        VisibleRows = rows;
        SetCurrentValue(ItemsSourceProperty, rows);
        SyncSelectedRow();
        var version = ++_animationVersion;
        if (previousNodes is not null && previousLayout is not null)
            Dispatcher.UIThread.Post(() => AnimateRebuild(previousNodes, previousLayout, version), DispatcherPriority.Loaded);
    }

    private Dictionary<MdTreeNode, double> CaptureRowLayout()
    {
        var result = new Dictionary<MdTreeNode, double>();
        foreach (var container in GetRealizedContainers().OfType<ListBoxItem>())
        {
            if (container.Content is MdTreeRow row && container.TranslatePoint(default, this) is { } point)
                result[row.Node] = point.Y;
        }
        return result;
    }

    private void AnimateRebuild(HashSet<MdTreeNode> previousNodes, Dictionary<MdTreeNode, double> previousLayout, int version)
    {
        if (version != _animationVersion) return;
        UpdateLayout();
        var spatial = MdMotion.Resolve(this, MdMotionKind.Spatial, MdMotionSpeed.Fast);
        var effects = MdMotion.Resolve(this, MdMotionKind.Effects, MdMotionSpeed.Fast);
        if (!spatial.IsEnabled && !effects.IsEnabled) return;

        var pending = new List<(ListBoxItem Container, TranslateTransform Transform, bool Reveal)>();
        foreach (var container in GetRealizedContainers().OfType<ListBoxItem>())
        {
            if (container.Content is not MdTreeRow row) continue;
            var transform = container.RenderTransform as TranslateTransform ?? new TranslateTransform();
            container.RenderTransform = transform;
            container.RenderTransformOrigin = RelativePoint.TopLeft;
            var isReveal = !previousNodes.Contains(row.Node);
            var currentY = container.TranslatePoint(default, this)?.Y ?? 0;
            var deltaY = previousLayout.TryGetValue(row.Node, out var oldY) ? oldY - currentY : -12;
            container.Transitions = null;
            transform.Transitions = null;
            container.Opacity = isReveal && effects.IsEnabled ? 0 : 1;
            transform.Y = spatial.IsEnabled ? deltaY : 0;
            if (Math.Abs(transform.Y) > 0.01 || container.Opacity < 1)
                pending.Add((container, transform, isReveal));
        }

        if (pending.Count == 0) return;
        Dispatcher.UIThread.Post(() =>
        {
            if (version != _animationVersion) return;
            foreach (var entry in pending)
            {
                entry.Container.Transitions = MdMotionTransitions.Collect(
                    MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
                entry.Transform.Transitions = MdMotionTransitions.Collect(
                    MdMotionTransitions.CreateDouble(this, TranslateTransform.YProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast));
                entry.Container.Opacity = 1;
                entry.Transform.Y = 0;
            }
        }, DispatcherPriority.Render);
    }

    private void CancelMotion()
    {
        ++_animationVersion;
        foreach (var container in GetRealizedContainers().OfType<ListBoxItem>())
        {
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

    private void AddVisible(MdTreeNode node, MdTreeNode? parent, int depth, List<MdTreeRow> rows, HashSet<MdTreeNode> visited)
    {
        if (!visited.Add(node)) return;
        if (_observedNodes.Add(node))
        {
            node.PropertyChanged += OnNodePropertyChanged;
            node.Children.CollectionChanged += OnChildrenChanged;
        }
        var amount = depth * Indentation;
        var margin = FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft
            ? new Thickness(0, 0, amount, 0)
            : new Thickness(amount, 0, 0, 0);
        var collapsedAngle = FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft ? 180 : 0;
        rows.Add(new MdTreeRow(node, depth, parent, margin, node.Children.Count > 0, node.IsExpanded,
            node.IsExpanded ? 90 : collapsedAngle, ShowGuides && depth > 0));
        if (!node.IsExpanded) return;
        foreach (var child in node.Children) AddVisible(child, node, depth + 1, rows, visited);
    }

    private void SyncSelectedRow()
    {
        if (_syncingSelection) return;
        _syncingSelection = true;
        try
        {
            SelectedItem = VisibleRows.FirstOrDefault(row => ReferenceEquals(row.Node, SelectedNode));
            if (SelectedItem is not null) ScrollIntoView(SelectedItem);
        }
        finally { _syncingSelection = false; }
    }
}
