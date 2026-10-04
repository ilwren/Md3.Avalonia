using System.Collections;
using System.Diagnostics.CodeAnalysis;
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
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Infrastructure;
using Md3.Avalonia.Localization;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>One entry in a command palette.</summary>
/// <remarks>
/// The palette's item theme binds <see cref="Title"/> and <see cref="IsEnabled"/> by name from
/// XAML, which a trimmer cannot see. <see cref="MdCommandPalette"/> roots this type's public
/// properties so those two bindings keep working in a trimmed application.
/// </remarks>
public sealed record MdCommandItem(string Title, ICommand Command, object? Parameter = null, string? Description = null, string? Keywords = null, KeyGesture? Gesture = null)
{
    /// <summary>Whether <see cref="Command"/> can run with <see cref="Parameter"/> right now.</summary>
    public bool IsEnabled => Command.CanExecute(Parameter);
}

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
    public static readonly DirectProperty<MdCommandPalette, string> AccessibleNameProperty = AvaloniaProperty.RegisterDirect<MdCommandPalette, string>(nameof(AccessibleName), control => control.AccessibleName);
    public static readonly DirectProperty<MdCommandPalette, string> SearchPlaceholderProperty = AvaloniaProperty.RegisterDirect<MdCommandPalette, string>(nameof(SearchPlaceholder), control => control.SearchPlaceholder);
    public static readonly DirectProperty<MdCommandPalette, string> EmptyTextProperty = AvaloniaProperty.RegisterDirect<MdCommandPalette, string>(nameof(EmptyText), control => control.EmptyText);

    private IReadOnlyList<MdCommandItem> _filteredItems = Array.Empty<MdCommandItem>();
    private TextBox? _queryBox;
    private SelectingItemsControl? _itemsHost;
    private Grid? _backdrop;
    private Border? _surface;
    private TopLevel? _topLevel;
    private readonly MdPresenceController _presence;
    private readonly MdBackScope _backScope;
    private readonly MdFocusReturnScope _focusReturn = new();
    private readonly HashSet<ICommand> _observedCommands = [];
    private string _accessibleName = string.Empty;
    private string _searchPlaceholder = string.Empty;
    private string _emptyText = string.Empty;
    private int _resultsVersion;

    // EcoCommandPaletteItemTheme binds Title and IsEnabled by name, which only exists in XAML.
    // Rooting them here ties their survival to the control that needs them, so a trimmed app
    // that never uses the palette still drops both.
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(MdCommandItem))]
    static MdCommandPalette()
    {
        ItemsSourceProperty.Changed.AddClassHandler<MdCommandPalette>((control, _) => control.Refresh());
        QueryProperty.Changed.AddClassHandler<MdCommandPalette>((control, _) => control.Refresh());
        IsOpenProperty.Changed.AddClassHandler<MdCommandPalette>((control, _) => control.OnOpenChanged());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdCommandPalette>((control, _) => control.UpdateMotion());
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdCommandPalette>((control, _) => control.UpdateLocalizedText());
    }
    public MdCommandPalette()
    {
        _backScope = new MdBackScope(this, OnBackRequested);
        _presence = new MdPresenceController(present => PseudoClasses.Set(":present", present));
        _presence.Initialize(IsOpen);
        UpdateLocalizedText();
        Refresh();
    }

    public IEnumerable? ItemsSource { get => GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public string? Query { get => GetValue(QueryProperty); set => SetValue(QueryProperty, value); }
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public KeyGesture? OpenGesture { get => GetValue(OpenGestureProperty); set => SetValue(OpenGestureProperty, value); }
    public int SelectedIndex { get => GetValue(SelectedIndexProperty); set => SetValue(SelectedIndexProperty, value); }
    public IReadOnlyList<MdCommandItem> FilteredItems => _filteredItems;
    public string AccessibleName => _accessibleName;
    public string SearchPlaceholder => _searchPlaceholder;
    public string EmptyText => _emptyText;

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
        SynchronizeCommandHandlers();
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _topLevel?.RemoveHandler(InputElement.KeyDownEvent, OnTopLevelKeyDown);
        _topLevel = null;
        ClearCommandHandlers();
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
        if (IsOpen)
        {
            if (e.Key == Key.Escape)
            {
                Dismiss();
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Tab && TrapTabFocus(e)) return;
        }

        if (OpenGesture?.Matches(e) == true) { Show(); e.Handled = true; return; }
        if (!IsOpen)
        {
            var command = (ItemsSource ?? Array.Empty<object>()).OfType<MdCommandItem>().FirstOrDefault(item => item.Gesture?.Matches(e) == true);
            if (command is not null && Execute(command)) e.Handled = true;
        }
    }

    private bool TrapTabFocus(KeyEventArgs e)
    {
        if (_surface is null) return false;
        var focusable = _surface.GetVisualDescendants().OfType<Control>()
            .Where(control => control.Focusable && control.IsVisible && control.IsEnabled)
            .ToArray();
        if (focusable.Length == 0) return false;
        var focused = _topLevel?.FocusManager?.GetFocusedElement() as Visual;
        var inside = focused is not null &&
                     (ReferenceEquals(focused, _surface) || focused.GetVisualAncestors().Contains(_surface));
        var reverse = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var atBoundary = !inside || (reverse && ReferenceEquals(focused, focusable[0])) ||
                         (!reverse && ReferenceEquals(focused, focusable[^1]));
        if (!atBoundary) return false;
        focusable[reverse ? focusable.Length - 1 : 0].Focus(NavigationMethod.Tab);
        e.Handled = true;
        return true;
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
            _focusReturn.Capture(_topLevel?.FocusManager?.GetFocusedElement() as InputElement);
            _presence.Update(true, TimeSpan.Zero);
            PseudoClasses.Set(":open", true);
            Dispatcher.UIThread.Post(() => { if (IsOpen) _queryBox?.Focus(); }, DispatcherPriority.Loaded);
        }
        else
        {
            PseudoClasses.Set(":open", false);
            _presence.Update(false, MdMotion.GetExitDuration(this));
            _focusReturn.Restore();
        }
        UpdateHitTesting();
        _backScope.Update(IsOpen);
    }

    // The palette is a modal surface, so Android's back gesture has to close it the way Escape
    // does on desktop - otherwise back pops the whole activity out from under an open palette.
    private bool OnBackRequested()
    {
        if (!IsOpen) return false;
        Dismiss();
        return true;
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

    private void UpdateLocalizedText()
    {
        SetAndRaise(AccessibleNameProperty, ref _accessibleName, MdLocalization.GetString("CommandPalette", this));
        SetAndRaise(SearchPlaceholderProperty, ref _searchPlaceholder, MdLocalization.GetString("SearchCommands", this));
        SetAndRaise(EmptyTextProperty, ref _emptyText, MdLocalization.GetString("NoResults", this));
    }

    private void SynchronizeCommandHandlers()
    {
        var desired = _topLevel is null
            ? new HashSet<ICommand>()
            : (ItemsSource ?? Array.Empty<object>()).OfType<MdCommandItem>().Select(item => item.Command).ToHashSet();
        foreach (var command in _observedCommands.Where(command => !desired.Contains(command)).ToArray())
        {
            command.CanExecuteChanged -= OnCommandCanExecuteChanged;
            _observedCommands.Remove(command);
        }
        foreach (var command in desired.Where(command => !_observedCommands.Contains(command)))
        {
            command.CanExecuteChanged += OnCommandCanExecuteChanged;
            _observedCommands.Add(command);
        }
    }

    private void ClearCommandHandlers()
    {
        foreach (var command in _observedCommands) command.CanExecuteChanged -= OnCommandCanExecuteChanged;
        _observedCommands.Clear();
    }

    private void OnCommandCanExecuteChanged(object? sender, EventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess()) Refresh();
        else Dispatcher.UIThread.Post(Refresh);
    }

    private void Refresh()
    {
        SynchronizeCommandHandlers();
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
