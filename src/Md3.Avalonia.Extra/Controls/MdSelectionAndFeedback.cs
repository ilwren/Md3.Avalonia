using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Infrastructure;
using Md3.Avalonia.Localization;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>Searchable async single-selection control with debounce, cancellation, keyboard navigation, empty, and error states.</summary>
[PseudoClasses(":open", ":closed", ":popup-present", ":loading", ":empty", ":error", ":reduced-motion", ":no-motion")]
public sealed class MdAsyncSelect : TemplatedControl, IMdPopupOwner, IMdPopupPresenceOwner
{
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = AvaloniaProperty.Register<MdAsyncSelect, IEnumerable?>(nameof(ItemsSource));
    public static readonly StyledProperty<string?> QueryProperty = AvaloniaProperty.Register<MdAsyncSelect, string?>(nameof(Query), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<object?> SelectedItemProperty = AvaloniaProperty.Register<MdAsyncSelect, object?>(nameof(SelectedItem), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> IsDropDownOpenProperty = AvaloniaProperty.Register<MdAsyncSelect, bool>(nameof(IsDropDownOpen), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<TimeSpan> DebounceProperty = AvaloniaProperty.Register<MdAsyncSelect, TimeSpan>(nameof(Debounce), TimeSpan.FromMilliseconds(250));
    public static readonly StyledProperty<string> DisplayMemberPathProperty = AvaloniaProperty.Register<MdAsyncSelect, string>(nameof(DisplayMemberPath), string.Empty);
    public static readonly StyledProperty<bool> IsMultiSelectProperty = AvaloniaProperty.Register<MdAsyncSelect, bool>(nameof(IsMultiSelect));
    public static readonly DirectProperty<MdAsyncSelect, IReadOnlyList<object?>> ResultsProperty = AvaloniaProperty.RegisterDirect<MdAsyncSelect, IReadOnlyList<object?>>(nameof(Results), control => control.Results);
    public static readonly DirectProperty<MdAsyncSelect, MdAsyncRequestState> StateProperty = AvaloniaProperty.RegisterDirect<MdAsyncSelect, MdAsyncRequestState>(nameof(State), control => control.State);
    public static readonly DirectProperty<MdAsyncSelect, Exception?> ErrorProperty = AvaloniaProperty.RegisterDirect<MdAsyncSelect, Exception?>(nameof(Error), control => control.Error);
    public static readonly DirectProperty<MdAsyncSelect, bool> IsPopupOpenProperty = AvaloniaProperty.RegisterDirect<MdAsyncSelect, bool>(nameof(IsPopupOpen), control => control.IsPopupOpen);
    private readonly DispatcherTimer _timer = new();
    private readonly MdPresenceController _popupPresence;
    private IReadOnlyList<object?> _results = Array.Empty<object?>();
    private MdAsyncRequestState _state;
    private Exception? _error;
    private CancellationTokenSource? _cancellation;
    private ListBox? _list;
    private TextBox? _queryBox;
    private Popup? _popup;
    private Border? _surface;
    private bool _isPopupOpen;
    private int _openStateVersion;
    private int _searchVersion;
    private bool _isAttached;
    private bool _synchronizingDisplayText;
    private bool _synchronizingSelection;

    static MdAsyncSelect()
    {
        ItemsSourceProperty.Changed.AddClassHandler<MdAsyncSelect>((control, _) => control.FilterLocal());
        QueryProperty.Changed.AddClassHandler<MdAsyncSelect>((control, _) => { if (!control._synchronizingDisplayText) control.ScheduleSearch(); });
        SelectedItemProperty.Changed.AddClassHandler<MdAsyncSelect>((control, _) => control.SyncSelectedDisplayText());
        IsDropDownOpenProperty.Changed.AddClassHandler<MdAsyncSelect>((control, _) => control.UpdateOpenState());
        IsMultiSelectProperty.Changed.AddClassHandler<MdAsyncSelect>((control, _) => control.UpdateSelectionMode());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdAsyncSelect>((control, _) => control.UpdateMotion());
    }
    public MdAsyncSelect()
    {
        _popupPresence = new MdPresenceController(control => SetPopupPresence(control));
        _popupPresence.Initialize(IsDropDownOpen);
        _timer.Tick += async (_, _) => { _timer.Stop(); await SearchAsync(); };
        UpdateRequestState();
        UpdateOpenState();
    }
    public IEnumerable? ItemsSource { get => GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public string? Query { get => GetValue(QueryProperty); set => SetValue(QueryProperty, value); }
    public object? SelectedItem { get => GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }
    public bool IsDropDownOpen { get => GetValue(IsDropDownOpenProperty); set => SetValue(IsDropDownOpenProperty, value); }
    public bool IsMaterialPopupOpen
    {
        get => IsDropDownOpen;
        set => SetCurrentValue(IsDropDownOpenProperty, value);
    }
    public TimeSpan Debounce { get => GetValue(DebounceProperty); set => SetValue(DebounceProperty, value); }
    public string DisplayMemberPath { get => GetValue(DisplayMemberPathProperty); set => SetValue(DisplayMemberPathProperty, value); }
    public bool IsMultiSelect { get => GetValue(IsMultiSelectProperty); set => SetValue(IsMultiSelectProperty, value); }
    public ObservableCollection<object?> SelectedItems { get; } = [];
    public IReadOnlyList<object?> Results => _results;
    public bool IsPopupOpen
    {
        get => _isPopupOpen;
        private set => SetAndRaise(IsPopupOpenProperty, ref _isPopupOpen, value);
    }
    public MdAsyncRequestState State { get => _state; private set { SetAndRaise(StateProperty, ref _state, value); UpdateRequestState(); } }
    public Exception? Error { get => _error; private set => SetAndRaise(ErrorProperty, ref _error, value); }
    public Func<string, CancellationToken, ValueTask<IReadOnlyList<object?>>>? SearchProvider { get; set; }
    public event EventHandler<object?>? SelectionCommitted;

    public async ValueTask SearchAsync()
    {
        var previous = _cancellation;
        _cancellation = null;
        previous?.Cancel();
        var version = ++_searchVersion;
        if (SearchProvider is null) { FilterLocal(); return; }

        var operation = new CancellationTokenSource();
        _cancellation = operation;
        State = MdAsyncRequestState.Loading;
        Error = null;
        try
        {
            var result = await SearchProvider(Query ?? string.Empty, operation.Token);
            if (operation.IsCancellationRequested || version != _searchVersion) return;
            SetAndRaise(ResultsProperty, ref _results, result);
            State = result.Count == 0 ? MdAsyncRequestState.Empty : MdAsyncRequestState.Data;
            SetCurrentValue(IsDropDownOpenProperty, true);
        }
        catch (OperationCanceledException) when (operation.IsCancellationRequested) { }
        catch (Exception exception)
        {
            if (version != _searchVersion) return;
            Error = exception;
            State = MdAsyncRequestState.Error;
        }
        finally
        {
            if (ReferenceEquals(_cancellation, operation)) _cancellation = null;
            operation.Dispose();
        }
    }
    public void Commit(object? item)
    {
        if (IsMultiSelect)
        {
            if (SelectedItems.Contains(item)) SelectedItems.Remove(item); else SelectedItems.Add(item);
        }
        else
        {
            SelectedItem = item;
            SyncSelectedDisplayText();
        }
        SelectionCommitted?.Invoke(this, item);
        if (!IsMultiSelect) IsDropDownOpen = false;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_list is not null) _list.SelectionChanged -= OnSelectionChanged;
        if (_popup is not null) _popup.Closed -= OnPopupClosed;
        if (_queryBox is not null)
        {
            _queryBox.GotFocus -= OnQueryGotFocus;
            _queryBox.PointerPressed -= OnQueryPointerPressed;
        }
        base.OnApplyTemplate(e);
        _list = e.NameScope.Find<ListBox>("PART_Results");
        _queryBox = e.NameScope.Find<TextBox>("PART_Query");
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _surface = e.NameScope.Find<Border>("PART_Surface");
        if (_list is not null) _list.SelectionChanged += OnSelectionChanged;
        if (_popup is not null) _popup.Closed += OnPopupClosed;
        if (_queryBox is not null)
        {
            _queryBox.GotFocus += OnQueryGotFocus;
            _queryBox.PointerPressed += OnQueryPointerPressed;
        }
        SyncSelectedDisplayText();
        UpdateSelectionMode();
        UpdateMotion();
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsDropDownOpen) { IsDropDownOpen = false; e.Handled = true; }
        else if (e.Key == Key.Down) { IsDropDownOpen = true; if (_list is not null) _list.SelectedIndex = Math.Min(Results.Count - 1, _list.SelectedIndex + 1); e.Handled = true; }
        else if (e.Key == Key.Up && _list is not null) { _list.SelectedIndex = Math.Max(0, _list.SelectedIndex - 1); e.Handled = true; }
        else if (e.Key == Key.Enter && _list?.SelectedItem is { } item) { Commit(item); e.Handled = true; }
        else base.OnKeyDown(e);
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
        ++_searchVersion;
        _timer.Stop();
        var operation = _cancellation;
        _cancellation = null;
        operation?.Cancel();
        _popupPresence.Stop();
        base.OnDetachedFromVisualTree(e);
    }
    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_synchronizingSelection) return;
        if (!IsMultiSelect)
        {
            if (_list?.SelectedItem is not { } item) return;
            Commit(item);
            _synchronizingSelection = true;
            _list.SelectedItem = null;
            _synchronizingSelection = false;
            return;
        }
        foreach (var item in e.AddedItems.Cast<object?>()) if (!SelectedItems.Contains(item)) { SelectedItems.Add(item); SelectionCommitted?.Invoke(this, item); }
        foreach (var item in e.RemovedItems.Cast<object?>()) SelectedItems.Remove(item);
    }
    private void UpdateSelectionMode()
    {
        if (_list is not null) _list.SelectionMode = IsMultiSelect ? SelectionMode.Multiple : SelectionMode.Single;
    }
    private void OnQueryGotFocus(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => OpenDropDownFromEditor();
    private void OnQueryPointerPressed(object? sender, PointerPressedEventArgs e) => OpenDropDownFromEditor();
    private void OpenDropDownFromEditor()
    {
        SetCurrentValue(IsDropDownOpenProperty, true);
        if (Results.Count == 0) _ = SearchAsync();
    }
    private void SyncSelectedDisplayText()
    {
        if (IsMultiSelect) return;
        var text = SelectedItem is null ? string.Empty : GetDisplay(SelectedItem);
        if (string.Equals(Query, text, StringComparison.Ordinal)) return;
        _synchronizingDisplayText = true;
        try { SetCurrentValue(QueryProperty, text); }
        finally { _synchronizingDisplayText = false; }
        if (_queryBox is not null)
        {
            _queryBox.CaretIndex = text.Length;
            _queryBox.SelectionStart = 0;
            _queryBox.SelectionEnd = text.Length;
        }
    }
    private void ScheduleSearch() { _timer.Stop(); _timer.Interval = Debounce; if (Debounce <= TimeSpan.Zero) _ = SearchAsync(); else _timer.Start(); }
    private void FilterLocal()
    {
        var query = Query?.Trim();
        var values = (ItemsSource ?? Array.Empty<object>()).Cast<object?>().Where(item => string.IsNullOrEmpty(query) || GetDisplay(item).Contains(query, StringComparison.OrdinalIgnoreCase)).ToArray();
        SetAndRaise(ResultsProperty, ref _results, values); State = values.Length == 0 ? MdAsyncRequestState.Empty : MdAsyncRequestState.Data;
    }
    private string GetDisplay(object? item) => string.IsNullOrWhiteSpace(DisplayMemberPath) ? item?.ToString() ?? string.Empty : item?.GetType().GetProperty(DisplayMemberPath)?.GetValue(item)?.ToString() ?? string.Empty;

    void IMdPopupPresenceOwner.ClosePopupImmediately() => _popupPresence.Initialize(false);

    private void UpdateRequestState()
    {
        PseudoClasses.Set(":loading", State == MdAsyncRequestState.Loading);
        PseudoClasses.Set(":empty", State == MdAsyncRequestState.Empty);
        PseudoClasses.Set(":error", State == MdAsyncRequestState.Error);
    }

    private void UpdateOpenState()
    {
        MdPopupCoordinator.NotifyStateChanged(this);
        var version = ++_openStateVersion;
        if (IsDropDownOpen)
        {
            _popupPresence.Update(true, TimeSpan.Zero);
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
            _popupPresence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
        }
    }

    private void SetPopupPresence(bool value)
    {
        IsPopupOpen = value;
        PseudoClasses.Set(":popup-present", value);
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        if (IsDropDownOpen) SetCurrentValue(IsDropDownOpenProperty, false);
        _popupPresence.Initialize(false);
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
        if (IsDropDownOpen && scheme == MdMotionScheme.None)
        {
            ++_openStateVersion;
            PseudoClasses.Set(":closed", false);
            PseudoClasses.Set(":open", true);
        }
        else if (!IsDropDownOpen)
        {
            _popupPresence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
        }
    }
}

public enum MdCalendarSelectionMode { Single, Range, Multiple }
public sealed record MdCalendarDay(DateTimeOffset Date, bool IsCurrentMonth, bool IsToday, bool IsSelected, bool IsRangeStart, bool IsRangeEnd, object? Badge = null, bool IsEnabled = true);

[PseudoClasses(":outside", ":today", ":selected", ":range-start", ":range-end")]
public sealed class MdCalendarDayPresenter : ContentControl
{
    public static readonly StyledProperty<MdCalendarDay?> DayProperty = AvaloniaProperty.Register<MdCalendarDayPresenter, MdCalendarDay?>(nameof(Day));
    static MdCalendarDayPresenter() => DayProperty.Changed.AddClassHandler<MdCalendarDayPresenter>((control, _) => control.UpdateState());
    public MdCalendarDayPresenter() => UpdateState();
    public MdCalendarDay? Day { get => GetValue(DayProperty); set => SetValue(DayProperty, value); }
    private void UpdateState()
    {
        var day = Day;
        IsEnabled = day?.IsEnabled != false;
        PseudoClasses.Set(":outside", day?.IsCurrentMonth == false); PseudoClasses.Set(":today", day?.IsToday == true);
        PseudoClasses.Set(":selected", day?.IsSelected == true); PseudoClasses.Set(":range-start", day?.IsRangeStart == true); PseudoClasses.Set(":range-end", day?.IsRangeEnd == true);
    }
}

/// <summary>Keyboard-navigable month calendar model supporting single, range, multiple, disabled dates, and badges.</summary>
public sealed class MdCalendar : TemplatedControl
{
    public static readonly StyledProperty<DateTimeOffset> DisplayMonthProperty = AvaloniaProperty.Register<MdCalendar, DateTimeOffset>(nameof(DisplayMonth), new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, 1, 0, 0, 0, DateTimeOffset.Now.Offset));
    public static readonly StyledProperty<DateTimeOffset?> SelectedDateProperty = AvaloniaProperty.Register<MdCalendar, DateTimeOffset?>(nameof(SelectedDate), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<DateTimeOffset?> RangeEndProperty = AvaloniaProperty.Register<MdCalendar, DateTimeOffset?>(nameof(RangeEnd), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<MdCalendarSelectionMode> SelectionModeProperty = AvaloniaProperty.Register<MdCalendar, MdCalendarSelectionMode>(nameof(SelectionMode));
    public static readonly DirectProperty<MdCalendar, IReadOnlyList<MdCalendarDay>> VisibleDaysProperty = AvaloniaProperty.RegisterDirect<MdCalendar, IReadOnlyList<MdCalendarDay>>(nameof(VisibleDays), control => control.VisibleDays);
    public static readonly DirectProperty<MdCalendar, IReadOnlyList<string>> WeekdayLabelsProperty = AvaloniaProperty.RegisterDirect<MdCalendar, IReadOnlyList<string>>(nameof(WeekdayLabels), control => control.WeekdayLabels);
    public static readonly DirectProperty<MdCalendar, string> DisplayMonthTextProperty = AvaloniaProperty.RegisterDirect<MdCalendar, string>(nameof(DisplayMonthText), control => control.DisplayMonthText);
    private IReadOnlyList<MdCalendarDay> _visibleDays = Array.Empty<MdCalendarDay>();
    private IReadOnlyList<string> _weekdayLabels = Array.Empty<string>();
    private string _displayMonthText = string.Empty;
    private readonly HashSet<DateTime> _multipleDates = [];
    private ListBox? _daysList;
    private Button? _previousButton;
    private Button? _nextButton;
    private DateTimeOffset? _dragAnchor;
    private bool _dragRangeActive;
    private bool _dragRangeMoved;
    private bool _suppressDaySelection;
    static MdCalendar()
    {
        DisplayMonthProperty.Changed.AddClassHandler<MdCalendar>((control, _) => control.Rebuild());
        SelectedDateProperty.Changed.AddClassHandler<MdCalendar>((control, change) =>
        {
            if (change.NewValue is DateTimeOffset date && (control.DisplayMonth.Year != date.Year || control.DisplayMonth.Month != date.Month))
            {
                control.DisplayMonth = new DateTimeOffset(date.Year, date.Month, 1, 0, 0, 0, date.Offset);
            }
            control.Rebuild();
        });
        RangeEndProperty.Changed.AddClassHandler<MdCalendar>((control, _) => control.Rebuild());
        SelectionModeProperty.Changed.AddClassHandler<MdCalendar>((control, _) => control.Rebuild());
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdCalendar>((control, _) => control.Rebuild());
    }
    public MdCalendar() => Rebuild();
    public DateTimeOffset DisplayMonth { get => GetValue(DisplayMonthProperty); set => SetValue(DisplayMonthProperty, value); }
    public DateTimeOffset? SelectedDate { get => GetValue(SelectedDateProperty); set => SetValue(SelectedDateProperty, value); }
    public DateTimeOffset? RangeEnd { get => GetValue(RangeEndProperty); set => SetValue(RangeEndProperty, value); }
    public MdCalendarSelectionMode SelectionMode { get => GetValue(SelectionModeProperty); set => SetValue(SelectionModeProperty, value); }
    public IReadOnlyList<MdCalendarDay> VisibleDays => _visibleDays;
    public IReadOnlyList<string> WeekdayLabels => _weekdayLabels;
    public string DisplayMonthText => _displayMonthText;
    public Func<DateTimeOffset, bool>? IsDateEnabled { get; set; }
    public Func<DateTimeOffset, object?>? BadgeProvider { get; set; }
    public event EventHandler<DateTimeOffset>? DateInvoked;
    public void NextMonth() => DisplayMonth = DisplayMonth.AddMonths(1);
    public void PreviousMonth() => DisplayMonth = DisplayMonth.AddMonths(-1);

    /// <summary>Begins a continuous range gesture. Pointer hosts and direct-API consumers share this path.</summary>
    public bool BeginRangeSelection(DateTimeOffset date)
    {
        if (SelectionMode != MdCalendarSelectionMode.Range || IsDateEnabled?.Invoke(date) == false) return false;
        DisplayMonth = new DateTimeOffset(date.Year, date.Month, 1, 0, 0, 0, date.Offset);
        _dragAnchor = date.Date;
        _dragRangeActive = true;
        _dragRangeMoved = false;
        SetCurrentValue(SelectedDateProperty, date.Date);
        SetCurrentValue(RangeEndProperty, date.Date);
        Rebuild();
        return true;
    }

    /// <summary>Updates the live range while a pointer is captured.</summary>
    public bool UpdateRangeSelection(DateTimeOffset date)
    {
        if (!_dragRangeActive || _dragAnchor is null || IsDateEnabled?.Invoke(date) == false) return false;
        date = date.Date;
        var start = date < _dragAnchor.Value ? date : _dragAnchor.Value;
        var end = date < _dragAnchor.Value ? _dragAnchor.Value : date;
        _dragRangeMoved |= date != _dragAnchor.Value;
        SetCurrentValue(SelectedDateProperty, start);
        SetCurrentValue(RangeEndProperty, end);
        Rebuild();
        return true;
    }

    /// <summary>Completes the active range gesture and emits the same invocation event as click/keyboard selection.</summary>
    public bool CompleteRangeSelection()
    {
        if (!_dragRangeActive || _dragAnchor is null) return false;
        var invoked = RangeEnd ?? SelectedDate ?? _dragAnchor.Value;
        if (!_dragRangeMoved) SetCurrentValue(RangeEndProperty, null);
        _dragRangeActive = false;
        _dragRangeMoved = false;
        _dragAnchor = null;
        DateInvoked?.Invoke(this, invoked);
        Rebuild();
        return true;
    }

    public void SelectDate(DateTimeOffset date)
    {
        if (IsDateEnabled?.Invoke(date) == false) return;
        date = date.Date;
        DisplayMonth = new DateTimeOffset(date.Year, date.Month, 1, 0, 0, 0, date.Offset);
        if (SelectionMode == MdCalendarSelectionMode.Multiple)
        {
            if (!_multipleDates.Add(date.Date)) _multipleDates.Remove(date.Date);
        }
        else if (SelectionMode == MdCalendarSelectionMode.Range)
        {
            if (SelectedDate is null || RangeEnd is not null) { SelectedDate = date; RangeEnd = null; }
            else if (date < SelectedDate.Value) { RangeEnd = SelectedDate; SelectedDate = date; }
            else RangeEnd = date;
        }
        else SelectedDate = date;
        DateInvoked?.Invoke(this, date); Rebuild();
    }
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_daysList is not null)
        {
            _daysList.SelectionChanged -= OnDaySelectionChanged;
            _daysList.RemoveHandler(InputElement.PointerPressedEvent, OnDayPointerPressed);
            _daysList.RemoveHandler(InputElement.PointerMovedEvent, OnDayPointerMoved);
            _daysList.RemoveHandler(InputElement.PointerReleasedEvent, OnDayPointerReleased);
        }
        if (_previousButton is not null) _previousButton.Click -= OnPreviousMonth;
        if (_nextButton is not null) _nextButton.Click -= OnNextMonth;
        base.OnApplyTemplate(e);
        _daysList = e.NameScope.Find<ListBox>("PART_Days");
        _previousButton = e.NameScope.Find<Button>("PART_PreviousMonth");
        _nextButton = e.NameScope.Find<Button>("PART_NextMonth");
        if (_daysList is not null)
        {
            _daysList.SelectionChanged += OnDaySelectionChanged;
            _daysList.AddHandler(InputElement.PointerPressedEvent, OnDayPointerPressed, global::Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
            _daysList.AddHandler(InputElement.PointerMovedEvent, OnDayPointerMoved, global::Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
            _daysList.AddHandler(InputElement.PointerReleasedEvent, OnDayPointerReleased, global::Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        }
        if (_previousButton is not null) _previousButton.Click += OnPreviousMonth;
        if (_nextButton is not null) _nextButton.Click += OnNextMonth;
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        var current = SelectedDate ?? DisplayMonth;
        var next = e.Key switch { Key.Left => current.AddDays(-1), Key.Right => current.AddDays(1), Key.Up => current.AddDays(-7), Key.Down => current.AddDays(7), Key.PageUp => current.AddMonths(-1), Key.PageDown => current.AddMonths(1), Key.Home => current.AddDays(-(int)current.DayOfWeek), Key.End => current.AddDays(6 - (int)current.DayOfWeek), _ => current };
        if (next != current) { SelectDate(next); DisplayMonth = new DateTimeOffset(next.Year, next.Month, 1, 0, 0, 0, next.Offset); e.Handled = true; }
        else base.OnKeyDown(e);
    }
    private void OnDayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (SelectionMode != MdCalendarSelectionMode.Range || _daysList is null || !e.GetCurrentPoint(_daysList).Properties.IsLeftButtonPressed) return;
        if (FindDayAtSource(e.Source) is not { IsEnabled: true } day || !BeginRangeSelection(day.Date)) return;
        _suppressDaySelection = true;
        e.Pointer.Capture(_daysList);
        e.Handled = true;
    }

    private void OnDayPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_dragRangeActive || _daysList is null || !ReferenceEquals(e.Pointer.Captured, _daysList)) return;
        if (FindDayAtPosition(e.GetPosition(_daysList)) is { IsEnabled: true } day) UpdateRangeSelection(day.Date);
        e.Handled = true;
    }

    private void OnDayPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_dragRangeActive || _daysList is null) return;
        if (FindDayAtPosition(e.GetPosition(_daysList)) is { IsEnabled: true } day) UpdateRangeSelection(day.Date);
        CompleteRangeSelection();
        _suppressDaySelection = false;
        _daysList.SelectedItem = null;
        if (ReferenceEquals(e.Pointer.Captured, _daysList)) e.Pointer.Capture(null);
        e.Handled = true;
    }

    private MdCalendarDay? FindDayAtSource(object? source)
    {
        var visual = source as Visual;
        var row = visual as ListBoxItem ?? visual?.FindAncestorOfType<ListBoxItem>();
        return row?.DataContext as MdCalendarDay ?? row?.Content as MdCalendarDay;
    }

    private MdCalendarDay? FindDayAtPosition(Point position)
    {
        if (_daysList is null) return null;
        foreach (var row in _daysList.GetRealizedContainers().OfType<ListBoxItem>())
        {
            if (row.TranslatePoint(default, _daysList) is not { } origin) continue;
            if (new Rect(origin, row.Bounds.Size).Contains(position))
                return row.DataContext as MdCalendarDay ?? row.Content as MdCalendarDay;
        }
        return null;
    }

    private void OnDaySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_suppressDaySelection || _daysList?.SelectedItem is not MdCalendarDay day) return;
        SelectDate(day.Date);
        _daysList.SelectedItem = null;
    }
    private void OnPreviousMonth(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => PreviousMonth();
    private void OnNextMonth(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => NextMonth();
    private void Rebuild()
    {
        var culture = MdLocalization.ResolveCulture(this);
        var month = new DateTimeOffset(DisplayMonth.Year, DisplayMonth.Month, 1, 0, 0, 0, DisplayMonth.Offset);
        var firstDay = culture.DateTimeFormat.FirstDayOfWeek;
        var leadingDays = (7 + (int)month.DayOfWeek - (int)firstDay) % 7;
        var start = month.AddDays(-leadingDays);
        var labels = Enumerable.Range(0, 7)
            .Select(index => culture.DateTimeFormat.GetShortestDayName((DayOfWeek)(((int)firstDay + index) % 7)))
            .ToArray();
        SetAndRaise(WeekdayLabelsProperty, ref _weekdayLabels, labels);
        SetAndRaise(DisplayMonthTextProperty, ref _displayMonthText, month.ToString("Y", culture));
        var rangeStart = SelectedDate?.Date; var rangeEnd = RangeEnd?.Date;
        var days = Enumerable.Range(0, 42).Select(offset =>
        {
            var date = start.AddDays(offset); var selected = SelectionMode == MdCalendarSelectionMode.Multiple ? _multipleDates.Contains(date.Date) : rangeStart == date.Date || rangeEnd == date.Date || (rangeStart <= date.Date && rangeEnd >= date.Date);
            return new MdCalendarDay(date, date.Month == month.Month, date.Date == DateTimeOffset.Now.Date, selected, rangeStart == date.Date, rangeEnd == date.Date, BadgeProvider?.Invoke(date), IsDateEnabled?.Invoke(date) != false);
        }).ToArray();
        SetAndRaise(VisibleDaysProperty, ref _visibleDays, days);
    }
}

public enum MdResultKind { Information, Success, Warning, Error, Empty }
/// <summary>Standardized result/empty/error surface with a real primary action command.</summary>
[PseudoClasses(":information", ":success", ":warning", ":error", ":empty")]
public sealed class MdResultView : ContentControl
{
    public static readonly StyledProperty<MdResultKind> KindProperty = AvaloniaProperty.Register<MdResultView, MdResultKind>(nameof(Kind));
    public static readonly StyledProperty<object?> TitleProperty = AvaloniaProperty.Register<MdResultView, object?>(nameof(Title));
    public static readonly StyledProperty<object?> IllustrationProperty = AvaloniaProperty.Register<MdResultView, object?>(nameof(Illustration));
    public static readonly StyledProperty<object?> ActionContentProperty = AvaloniaProperty.Register<MdResultView, object?>(nameof(ActionContent));
    public static readonly StyledProperty<ICommand?> ActionCommandProperty = AvaloniaProperty.Register<MdResultView, ICommand?>(nameof(ActionCommand));
    public static readonly StyledProperty<object?> ActionCommandParameterProperty = AvaloniaProperty.Register<MdResultView, object?>(nameof(ActionCommandParameter));
    static MdResultView() => KindProperty.Changed.AddClassHandler<MdResultView>((view, _) => view.UpdateState());
    public MdResultView() => UpdateState();
    public MdResultKind Kind { get => GetValue(KindProperty); set => SetValue(KindProperty, value); }
    public object? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public object? Illustration { get => GetValue(IllustrationProperty); set => SetValue(IllustrationProperty, value); }
    public object? ActionContent { get => GetValue(ActionContentProperty); set => SetValue(ActionContentProperty, value); }
    public ICommand? ActionCommand { get => GetValue(ActionCommandProperty); set => SetValue(ActionCommandProperty, value); }
    public object? ActionCommandParameter { get => GetValue(ActionCommandParameterProperty); set => SetValue(ActionCommandParameterProperty, value); }
    private void UpdateState() { foreach (var kind in Enum.GetValues<MdResultKind>()) PseudoClasses.Set($":{kind.ToString().ToLowerInvariant()}", Kind == kind); }
}
