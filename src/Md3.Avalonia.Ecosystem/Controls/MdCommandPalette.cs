using System.Collections;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Ecosystem.Controls;

public sealed record MdCommandItem(string Title, ICommand Command, object? Parameter = null, string? Description = null, string? Keywords = null, KeyGesture? Gesture = null);

/// <summary>A keyboard-first searchable command surface with real shortcut routing.</summary>
[PseudoClasses(":open", ":present", ":empty", ":reduced-motion", ":no-motion")]
public sealed class MdCommandPalette : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = AvaloniaProperty.Register<MdCommandPalette, IEnumerable?>(nameof(ItemsSource));
    public static readonly StyledProperty<string?> QueryProperty = AvaloniaProperty.Register<MdCommandPalette, string?>(nameof(Query), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<MdCommandPalette, bool>(nameof(IsOpen), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<KeyGesture?> OpenGestureProperty = AvaloniaProperty.Register<MdCommandPalette, KeyGesture?>(nameof(OpenGesture), new KeyGesture(Key.P, KeyModifiers.Control | KeyModifiers.Shift));
    public static readonly StyledProperty<int> SelectedIndexProperty = AvaloniaProperty.Register<MdCommandPalette, int>(nameof(SelectedIndex));
    public static readonly DirectProperty<MdCommandPalette, IReadOnlyList<MdCommandItem>> FilteredItemsProperty = AvaloniaProperty.RegisterDirect<MdCommandPalette, IReadOnlyList<MdCommandItem>>(nameof(FilteredItems), control => control.FilteredItems);

    private IReadOnlyList<MdCommandItem> _filteredItems = Array.Empty<MdCommandItem>();
    private TextBox? _queryBox;
    private SelectingItemsControl? _itemsHost;
    private Grid? _backdrop;
    private Border? _surface;
    private TopLevel? _topLevel;
    private readonly MdPresenceController _presence;
    private int _resultsVersion;

    static MdCommandPalette()
    {
        ItemsSourceProperty.Changed.AddClassHandler<MdCommandPalette>((control, _) => control.Refresh());
        QueryProperty.Changed.AddClassHandler<MdCommandPalette>((control, _) => control.Refresh());
        IsOpenProperty.Changed.AddClassHandler<MdCommandPalette>((control, _) => control.OnOpenChanged());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdCommandPalette>((control, _) => control.UpdateMotion());
    }
    public MdCommandPalette()
    {
        _presence = new MdPresenceController(present => PseudoClasses.Set(":present", present));
        _presence.Initialize(IsOpen);
        Refresh();
    }

    public IEnumerable? ItemsSource { get => GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public string? Query { get => GetValue(QueryProperty); set => SetValue(QueryProperty, value); }
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public KeyGesture? OpenGesture { get => GetValue(OpenGestureProperty); set => SetValue(OpenGestureProperty, value); }
    public int SelectedIndex { get => GetValue(SelectedIndexProperty); set => SetValue(SelectedIndexProperty, value); }
    public IReadOnlyList<MdCommandItem> FilteredItems => _filteredItems;

    public event EventHandler<MdCommandItem>? CommandInvoked;
    public void Show() => SetCurrentValue(IsOpenProperty, true);
    public void Dismiss() => SetCurrentValue(IsOpenProperty, false);

    public bool ExecuteSelected()
    {
        if (SelectedIndex < 0 || SelectedIndex >= FilteredItems.Count) return false;
        return Execute(FilteredItems[SelectedIndex]);
    }

    public bool Execute(MdCommandItem item)
    {
        if (!item.Command.CanExecute(item.Parameter)) return false;
        item.Command.Execute(item.Parameter);
        CommandInvoked?.Invoke(this, item);
        Dismiss();
        return true;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_itemsHost is not null)
            _itemsHost.RemoveHandler(InputElement.PointerReleasedEvent, OnItemPointerReleased);
        if (_backdrop is not null)
            _backdrop.RemoveHandler(InputElement.PointerPressedEvent, OnBackdropPressed);
        base.OnApplyTemplate(e);
        _queryBox = e.NameScope.Find<TextBox>("PART_QueryBox");
        _itemsHost = e.NameScope.Find<SelectingItemsControl>("PART_ItemsHost");
        _backdrop = e.NameScope.Find<Grid>("PART_Backdrop");
        _surface = e.NameScope.Find<Border>("PART_Surface");
        _itemsHost?.AddHandler(InputElement.PointerReleasedEvent, OnItemPointerReleased, RoutingStrategies.Bubble, true);
        _backdrop?.AddHandler(InputElement.PointerPressedEvent, OnBackdropPressed, RoutingStrategies.Bubble, true);
        _presence.Initialize(IsOpen);
        UpdateMotion();
        UpdateHitTesting();
    }

    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _topLevel = TopLevel.GetTopLevel(this);
        _topLevel?.AddHandler(InputElement.KeyDownEvent, OnTopLevelKeyDown, RoutingStrategies.Tunnel, true);
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _topLevel?.RemoveHandler(InputElement.KeyDownEvent, OnTopLevelKeyDown);
        _topLevel = null;
        _presence.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!IsOpen) { base.OnKeyDown(e); return; }
        if (e.Key == Key.Escape) { Dismiss(); e.Handled = true; }
        else if (e.Key == Key.Down) { SelectedIndex = Math.Min(FilteredItems.Count - 1, SelectedIndex + 1); e.Handled = true; }
        else if (e.Key == Key.Up) { SelectedIndex = Math.Max(0, SelectedIndex - 1); e.Handled = true; }
        else if (e.Key == Key.Enter && ExecuteSelected()) e.Handled = true;
        else base.OnKeyDown(e);
    }

    private void OnTopLevelKeyDown(object? sender, KeyEventArgs e)
    {
        if (OpenGesture?.Matches(e) == true) { Show(); e.Handled = true; return; }
        if (!IsOpen)
        {
            var command = (ItemsSource ?? Array.Empty<object>()).OfType<MdCommandItem>().FirstOrDefault(item => item.Gesture?.Matches(e) == true);
            if (command is not null && Execute(command)) e.Handled = true;
        }
    }

    private void OnBackdropPressed(object? sender, PointerPressedEventArgs e)
    {
        if (IsOpen && ReferenceEquals(e.Source, _backdrop))
        {
            Dismiss();
            e.Handled = true;
        }
    }

    private void OnItemPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Left) return;
        var source = e.Source as Visual;
        var row = source as ListBoxItem ?? source?.FindAncestorOfType<ListBoxItem>();
        if (row?.DataContext is not MdCommandItem item) return;
        SelectedIndex = _filteredItems.ToList().IndexOf(item);
        if (Execute(item)) e.Handled = true;
    }

    private void OnOpenChanged()
    {
        if (IsOpen)
        {
            _presence.Update(true, TimeSpan.Zero);
            PseudoClasses.Set(":open", true);
            Dispatcher.UIThread.Post(() => { if (IsOpen) _queryBox?.Focus(); }, DispatcherPriority.Loaded);
        }
        else
        {
            PseudoClasses.Set(":open", false);
            _presence.Update(false, MdMotion.GetExitDuration(this));
        }
        UpdateHitTesting();
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_backdrop is not null)
        {
            _backdrop.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects));
        }
        if (_surface is not null)
        {
            _surface.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty));
        }
        if (_itemsHost is not null)
        {
            _itemsHost.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        }
        if (!IsOpen) _presence.Update(false, MdMotion.GetExitDuration(this));
    }

    private void UpdateHitTesting()
    {
        if (_backdrop is not null) _backdrop.IsHitTestVisible = IsOpen;
    }

    private void Refresh()
    {
        var animateResults = _itemsHost is not null && MdMotion.GetScheme(this) != MdMotionScheme.None;
        var version = ++_resultsVersion;
        if (animateResults) _itemsHost!.Opacity = 0;
        var terms = (Query ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var items = (ItemsSource ?? Array.Empty<object>()).OfType<MdCommandItem>()
            .Where(item => terms.All(term => item.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                            item.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) == true ||
                                            item.Keywords?.Contains(term, StringComparison.OrdinalIgnoreCase) == true))
            .ToArray();
        SetAndRaise(FilteredItemsProperty, ref _filteredItems, items);
        SelectedIndex = items.Length == 0 ? -1 : Math.Clamp(SelectedIndex, 0, items.Length - 1);
        PseudoClasses.Set(":empty", items.Length == 0);
        if (animateResults)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (version == _resultsVersion && _itemsHost is not null) _itemsHost.Opacity = 1;
            }, DispatcherPriority.Render);
        }
    }
}
