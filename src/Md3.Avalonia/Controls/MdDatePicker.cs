using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Localization;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A self-contained Material 3 date picker with docked and modal presentations. It does not rely
/// on Avalonia's global DatePicker or Calendar themes and supports two-way SelectedDate binding.
/// </summary>
[PseudoClasses(":docked", ":modal", ":has-value", ":popup-present", ":reduced-motion", ":no-motion")]
public class MdDatePicker : TemplatedControl, IMdPopupOwner, IMdPopupPresenceOwner
{
    public static readonly StyledProperty<DateTimeOffset?> SelectedDateProperty =
        AvaloniaProperty.Register<MdDatePicker, DateTimeOffset?>(nameof(SelectedDate),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<DateTimeOffset> DisplayDateProperty =
        AvaloniaProperty.Register<MdDatePicker, DateTimeOffset>(nameof(DisplayDate), DateTimeOffset.Now);

    public static readonly StyledProperty<DateTimeOffset?> MinimumDateProperty =
        AvaloniaProperty.Register<MdDatePicker, DateTimeOffset?>(nameof(MinimumDate));

    public static readonly StyledProperty<DateTimeOffset?> MaximumDateProperty =
        AvaloniaProperty.Register<MdDatePicker, DateTimeOffset?>(nameof(MaximumDate));

    public static readonly StyledProperty<MdDatePickerMode> ModeProperty =
        AvaloniaProperty.Register<MdDatePicker, MdDatePickerMode>(nameof(Mode));

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MdDatePicker, bool>(nameof(IsOpen),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<object?> LabelProperty =
        AvaloniaProperty.Register<MdDatePicker, object?>(nameof(Label), "Date");

    public static readonly StyledProperty<string> DateFormatProperty =
        AvaloniaProperty.Register<MdDatePicker, string>(nameof(DateFormat), "d");

    public static readonly DirectProperty<MdDatePicker, string> DisplayTextProperty =
        AvaloniaProperty.RegisterDirect<MdDatePicker, string>(nameof(DisplayText), picker => picker.DisplayText);

    public static readonly DirectProperty<MdDatePicker, string> MonthTextProperty =
        AvaloniaProperty.RegisterDirect<MdDatePicker, string>(nameof(MonthText), picker => picker.MonthText);

    public static readonly DirectProperty<MdDatePicker, IReadOnlyList<MdCalendarDay>> CalendarDaysProperty =
        AvaloniaProperty.RegisterDirect<MdDatePicker, IReadOnlyList<MdCalendarDay>>(
            nameof(CalendarDays), picker => picker.CalendarDays);
    public static readonly DirectProperty<MdDatePicker, IReadOnlyList<MdCalendarDay>> OutgoingCalendarDaysProperty =
        AvaloniaProperty.RegisterDirect<MdDatePicker, IReadOnlyList<MdCalendarDay>>(
            nameof(OutgoingCalendarDays), picker => picker.OutgoingCalendarDays);
    public static readonly DirectProperty<MdDatePicker, bool> IsMonthTransitionActiveProperty =
        AvaloniaProperty.RegisterDirect<MdDatePicker, bool>(
            nameof(IsMonthTransitionActive), picker => picker.IsMonthTransitionActive);

    public static readonly DirectProperty<MdDatePicker, IReadOnlyList<string>> WeekdayLabelsProperty =
        AvaloniaProperty.RegisterDirect<MdDatePicker, IReadOnlyList<string>>(
            nameof(WeekdayLabels), picker => picker.WeekdayLabels);
    public static readonly DirectProperty<MdDatePicker, string> TodayTextProperty =
        AvaloniaProperty.RegisterDirect<MdDatePicker, string>(nameof(TodayText), picker => picker.TodayText);
    public static readonly DirectProperty<MdDatePicker, string> CancelTextProperty =
        AvaloniaProperty.RegisterDirect<MdDatePicker, string>(nameof(CancelText), picker => picker.CancelText);
    public static readonly DirectProperty<MdDatePicker, string> ConfirmTextProperty =
        AvaloniaProperty.RegisterDirect<MdDatePicker, string>(nameof(ConfirmText), picker => picker.ConfirmText);
    public static readonly DirectProperty<MdDatePicker, string> HeaderTextProperty =
        AvaloniaProperty.RegisterDirect<MdDatePicker, string>(nameof(HeaderText), picker => picker.HeaderText);
    public static readonly DirectProperty<MdDatePicker, bool> IsPopupOpenProperty =
        AvaloniaProperty.RegisterDirect<MdDatePicker, bool>(nameof(IsPopupOpen), picker => picker.IsPopupOpen);

    private readonly ObservableCollection<MdCalendarDay> _calendarDays = [];
    private readonly ObservableCollection<MdCalendarDay> _outgoingCalendarDays = [];
    private readonly ObservableCollection<string> _weekdayLabels = [];
    private string _displayText = "Choose date";
    private string _monthText = string.Empty;
    private string _todayText = "Today";
    private string _cancelText = "Cancel";
    private string _confirmText = "OK";
    private string _headerText = "Choose date";
    private string _defaultLabelText = "Date";
    private Button? _anchorButton;
    private Button? _previousButton;
    private Button? _nextButton;
    private Button? _todayButton;
    private Button? _cancelButton;
    private Button? _confirmButton;
    private ItemsControl? _daysHost;
    private ItemsControl? _outgoingDaysHost;
    private TextBlock? _monthHeader;
    private Popup? _popup;
    private Border? _surface;
    private readonly TranslateTransform _monthTransform = new();
    private readonly TranslateTransform _outgoingMonthTransform = new();
    private readonly DispatcherTimer _monthCompletion = new();
    private readonly MdPresenceController _popupPresence;
    private DateTimeOffset? _valueAtOpen;
    private bool _commitModalSelection;
    private bool _isPopupOpen;
    private bool _isMonthTransitionActive;
    private int _monthAnimationVersion;
    private int _monthDirection = 1;
    private DateTimeOffset _activeDate = DateTimeOffset.Now;

    static MdDatePicker()
    {
        SelectedDateProperty.Changed.AddClassHandler<MdDatePicker>((picker, _) => picker.OnSelectedDateChanged());
        DisplayDateProperty.Changed.AddClassHandler<MdDatePicker>((picker, change) => picker.OnDisplayDateChanged(change));
        MinimumDateProperty.Changed.AddClassHandler<MdDatePicker>((picker, _) => picker.RebuildCalendar());
        MaximumDateProperty.Changed.AddClassHandler<MdDatePicker>((picker, _) => picker.RebuildCalendar());
        ModeProperty.Changed.AddClassHandler<MdDatePicker>((picker, _) => picker.UpdatePseudoClasses());
        IsOpenProperty.Changed.AddClassHandler<MdDatePicker>((picker, _) => picker.OnOpenStateChanged());
        DateFormatProperty.Changed.AddClassHandler<MdDatePicker>((picker, _) => picker.UpdateDisplayText());
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdDatePicker>((picker, _) => picker.OnCultureChanged());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdDatePicker>((picker, _) => picker.UpdateMotion());
    }

    public MdDatePicker()
    {
        _popupPresence = new MdPresenceController(SetPopupPresence);
        _monthCompletion.Tick += (_, _) => CompleteMonthTransition();
        _popupPresence.Initialize(IsOpen);
        UpdatePseudoClasses();
        UpdateLocalizedText();
        UpdateDisplayText();
        RebuildCalendar();
    }

    public DateTimeOffset? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public DateTimeOffset DisplayDate
    {
        get => GetValue(DisplayDateProperty);
        set => SetValue(DisplayDateProperty, value);
    }

    public DateTimeOffset? MinimumDate
    {
        get => GetValue(MinimumDateProperty);
        set => SetValue(MinimumDateProperty, value);
    }

    public DateTimeOffset? MaximumDate
    {
        get => GetValue(MaximumDateProperty);
        set => SetValue(MaximumDateProperty, value);
    }

    public MdDatePickerMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    bool IMdPopupOwner.IsMaterialPopupOpen
    {
        get => IsOpen;
        set => SetCurrentValue(IsOpenProperty, value);
    }

    void IMdPopupPresenceOwner.ClosePopupImmediately() => _popupPresence.Initialize(false);

    public object? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string DateFormat
    {
        get => GetValue(DateFormatProperty);
        set => SetValue(DateFormatProperty, value);
    }

    public string DisplayText
    {
        get => _displayText;
        private set => SetAndRaise(DisplayTextProperty, ref _displayText, value);
    }

    public string MonthText
    {
        get => _monthText;
        private set => SetAndRaise(MonthTextProperty, ref _monthText, value);
    }

    public IReadOnlyList<MdCalendarDay> CalendarDays => _calendarDays;
    public IReadOnlyList<MdCalendarDay> OutgoingCalendarDays => _outgoingCalendarDays;
    public bool IsMonthTransitionActive
    {
        get => _isMonthTransitionActive;
        private set => SetAndRaise(IsMonthTransitionActiveProperty, ref _isMonthTransitionActive, value);
    }
    public IReadOnlyList<string> WeekdayLabels => _weekdayLabels;
    public string TodayText => _todayText;
    public string CancelText => _cancelText;
    public string ConfirmText => _confirmText;
    public string HeaderText => _headerText;
    public bool IsPopupOpen
    {
        get => _isPopupOpen;
        private set => SetAndRaise(IsPopupOpenProperty, ref _isPopupOpen, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateHandlers();
        if (_popup is not null) _popup.Closed -= OnPopupClosed;
        base.OnApplyTemplate(e);

        _anchorButton = e.NameScope.Find<Button>("PART_AnchorButton");
        _previousButton = e.NameScope.Find<Button>("PART_PreviousMonthButton");
        _nextButton = e.NameScope.Find<Button>("PART_NextMonthButton");
        _todayButton = e.NameScope.Find<Button>("PART_TodayButton");
        _cancelButton = e.NameScope.Find<Button>("PART_CancelButton");
        _confirmButton = e.NameScope.Find<Button>("PART_ConfirmButton");
        _daysHost = e.NameScope.Find<ItemsControl>("PART_DaysHost");
        _outgoingDaysHost = e.NameScope.Find<ItemsControl>("PART_OutgoingDaysHost");
        _monthHeader = e.NameScope.Find<TextBlock>("PART_MonthHeader");
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _surface = e.NameScope.Find<Border>("PART_Surface");
        if (_daysHost is not null) _daysHost.RenderTransform = _monthTransform;
        if (_outgoingDaysHost is not null) _outgoingDaysHost.RenderTransform = _outgoingMonthTransform;
        // Native PopupRoot breaks inherited DataContext on desktop. Set the picker owner explicitly
        // before the popup is opened so month, weekday, day and action bindings cannot render blank.
        if (_surface is not null) _surface.DataContext = this;
        if (_popup is not null) _popup.Closed += OnPopupClosed;

        if (_anchorButton is not null) _anchorButton.Click += OnAnchorClick;
        if (_previousButton is not null) _previousButton.Click += OnPreviousMonth;
        if (_nextButton is not null) _nextButton.Click += OnNextMonth;
        if (_todayButton is not null) _todayButton.Click += OnToday;
        if (_cancelButton is not null) _cancelButton.Click += OnCancel;
        if (_confirmButton is not null) _confirmButton.Click += OnConfirm;
        _daysHost?.AddHandler(Button.ClickEvent, OnDayClick);
        _daysHost?.AddHandler(InputElement.KeyDownEvent, OnCalendarKeyDown,
            RoutingStrategies.Tunnel, handledEventsToo: true);
        _daysHost?.AddHandler(InputElement.GotFocusEvent, OnCalendarDayGotFocus,
            RoutingStrategies.Bubble, handledEventsToo: true);
        _popupPresence.Initialize(IsOpen);
        UpdateMotion();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _popupPresence.Stop();
        _monthCompletion.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsOpen)
        {
            SetCurrentValue(IsOpenProperty, false);
            _anchorButton?.Focus();
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void DetachTemplateHandlers()
    {
        if (_anchorButton is not null) _anchorButton.Click -= OnAnchorClick;
        if (_previousButton is not null) _previousButton.Click -= OnPreviousMonth;
        if (_nextButton is not null) _nextButton.Click -= OnNextMonth;
        if (_todayButton is not null) _todayButton.Click -= OnToday;
        if (_cancelButton is not null) _cancelButton.Click -= OnCancel;
        if (_confirmButton is not null) _confirmButton.Click -= OnConfirm;
        _daysHost?.RemoveHandler(Button.ClickEvent, OnDayClick);
        _daysHost?.RemoveHandler(InputElement.KeyDownEvent, OnCalendarKeyDown);
        _daysHost?.RemoveHandler(InputElement.GotFocusEvent, OnCalendarDayGotFocus);
    }

    private void OnAnchorClick(object? sender, RoutedEventArgs e) =>
        SetCurrentValue(IsOpenProperty, !IsOpen);

    private void OnPreviousMonth(object? sender, RoutedEventArgs e) =>
        SetCurrentValue(DisplayDateProperty, DisplayDate.AddMonths(-1));

    private void OnNextMonth(object? sender, RoutedEventArgs e) =>
        SetCurrentValue(DisplayDateProperty, DisplayDate.AddMonths(1));

    private void OnToday(object? sender, RoutedEventArgs e)
    {
        var today = DateTimeOffset.Now;
        SetCurrentValue(DisplayDateProperty, today);
        SelectDate(today);
    }

    private void OnCancel(object? sender, RoutedEventArgs e) =>
        SetCurrentValue(IsOpenProperty, false);

    private void OnConfirm(object? sender, RoutedEventArgs e)
    {
        _commitModalSelection = true;
        SetCurrentValue(IsOpenProperty, false);
    }

    private void OnDayClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is Button { DataContext: MdCalendarDay day } && day.IsEnabled)
        {
            SelectDate(day.Date);
        }
    }

    private void OnCalendarDayGotFocus(object? sender, FocusChangedEventArgs e)
    {
        if (e.Source is Button { DataContext: MdCalendarDay day } button && day.IsEnabled)
        {
            _activeDate = day.Date;
            SetActiveTabStop(button);
        }
    }

    private void OnCalendarKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsOpen || e.Source is not Visual source || _daysHost is null ||
            (!ReferenceEquals(source, _daysHost) && !source.GetVisualAncestors().Contains(_daysHost)))
            return;

        var culture = MdLocalization.ResolveCulture(this);
        var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
        var dayOffset = (7 + (int)_activeDate.DayOfWeek - (int)firstDayOfWeek) % 7;
        var rtl = FlowDirection == FlowDirection.RightToLeft;
        DateTimeOffset? target = e.Key switch
        {
            Key.Left => _activeDate.AddDays(rtl ? 1 : -1),
            Key.Right => _activeDate.AddDays(rtl ? -1 : 1),
            Key.Up => _activeDate.AddDays(-7),
            Key.Down => _activeDate.AddDays(7),
            Key.Home => _activeDate.AddDays(-dayOffset),
            Key.End => _activeDate.AddDays(6 - dayOffset),
            Key.PageUp when e.KeyModifiers.HasFlag(KeyModifiers.Shift) => _activeDate.AddYears(-1),
            Key.PageDown when e.KeyModifiers.HasFlag(KeyModifiers.Shift) => _activeDate.AddYears(1),
            Key.PageUp => _activeDate.AddMonths(-1),
            Key.PageDown => _activeDate.AddMonths(1),
            _ => null
        };
        if (target is null) return;
        MoveActiveDate(target.Value);
        e.Handled = true;
    }

    private void MoveActiveDate(DateTimeOffset target)
    {
        _activeDate = ClampToEnabledDate(target);
        if (_activeDate.Year != DisplayDate.Year || _activeDate.Month != DisplayDate.Month)
            SetCurrentValue(DisplayDateProperty, _activeDate);
        FocusActiveDate();
    }

    private DateTimeOffset ClampToEnabledDate(DateTimeOffset value)
    {
        if (MinimumDate is { } minimum && value.Date < minimum.Date) value = minimum;
        if (MaximumDate is { } maximum && value.Date > maximum.Date) value = maximum;
        return value;
    }

    private void FocusActiveDate()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!IsOpen || _daysHost is null) return;
            var target = _daysHost.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(button => button.DataContext is MdCalendarDay day &&
                    day.IsEnabled && day.Date.Date == _activeDate.Date);
            SetActiveTabStop(target);
            target?.Focus(NavigationMethod.Directional);
        }, DispatcherPriority.Loaded);
    }

    private void SetActiveTabStop(Button? active)
    {
        if (_daysHost is null) return;
        foreach (var button in _daysHost.GetVisualDescendants().OfType<Button>())
            KeyboardNavigation.SetIsTabStop(button, ReferenceEquals(button, active));
    }

    private void SelectDate(DateTimeOffset value)
    {
        _activeDate = value;
        SetCurrentValue(SelectedDateProperty, value);
        SetCurrentValue(DisplayDateProperty, value);
        if (Mode == MdDatePickerMode.Docked)
        {
            SetCurrentValue(IsOpenProperty, false);
        }
    }

    private void OnOpenStateChanged()
    {
        if (IsOpen)
        {
            _popupPresence.Update(true, TimeSpan.Zero);
            _valueAtOpen = SelectedDate;
            _commitModalSelection = false;
            _activeDate = ClampToEnabledDate(SelectedDate ?? DateTimeOffset.Now);
            if (_activeDate.Year != DisplayDate.Year || _activeDate.Month != DisplayDate.Month)
                SetCurrentValue(DisplayDateProperty, _activeDate);
            MdPopupCoordinator.NotifyStateChanged(this);
            FocusActiveDate();
            return;
        }

        if (Mode == MdDatePickerMode.Modal && !_commitModalSelection && SelectedDate != _valueAtOpen)
            SetCurrentValue(SelectedDateProperty, _valueAtOpen);
        _commitModalSelection = false;
        _popupPresence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
        MdPopupCoordinator.NotifyStateChanged(this);
        Dispatcher.UIThread.Post(() => _anchorButton?.Focus(), DispatcherPriority.Input);
    }

    private void OnDisplayDateChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var monthChanged = false;
        if (change.OldValue is DateTimeOffset oldDate && change.NewValue is DateTimeOffset newDate)
        {
            monthChanged = oldDate.Year != newDate.Year || oldDate.Month != newDate.Month;
            if (monthChanged)
            {
                _monthDirection = newDate >= oldDate ? 1 : -1;
                _outgoingCalendarDays.Clear();
                foreach (var day in _calendarDays) _outgoingCalendarDays.Add(day);
            }
        }
        RebuildCalendar();
        if (monthChanged) AnimateMonthChange();
    }

    private void AnimateMonthChange()
    {
        _monthCompletion.Stop();
        var version = ++_monthAnimationVersion;
        var spatial = MdMotion.Resolve(this, MdMotionKind.Spatial, MdMotionSpeed.Fast);
        var effects = MdMotion.Resolve(this, MdMotionKind.Effects, MdMotionSpeed.Fast);
        if (_daysHost is null || _outgoingDaysHost is null || (!spatial.IsEnabled && !effects.IsEnabled))
        {
            CompleteMonthTransition();
            return;
        }

        IsMonthTransitionActive = true;
        _daysHost.Opacity = effects.IsEnabled ? 0 : 1;
        _outgoingDaysHost.Opacity = 1;
        _monthHeader?.SetCurrentValue(OpacityProperty, effects.IsEnabled ? 0 : 1);
        _monthTransform.X = spatial.IsEnabled ? _monthDirection * 48 : 0;
        _outgoingMonthTransform.X = 0;
        Dispatcher.UIThread.Post(() =>
        {
            if (version != _monthAnimationVersion || _daysHost is null || _outgoingDaysHost is null) return;
            _monthTransform.X = 0;
            _outgoingMonthTransform.X = spatial.IsEnabled ? -_monthDirection * 48 : 0;
            _daysHost.Opacity = 1;
            _outgoingDaysHost.Opacity = effects.IsEnabled ? 0 : 1;
            _monthHeader?.SetCurrentValue(OpacityProperty, 1);
        }, DispatcherPriority.Render);

        var duration = spatial.Duration > effects.Duration ? spatial.Duration : effects.Duration;
        if (duration <= TimeSpan.Zero)
        {
            CompleteMonthTransition();
            return;
        }
        _monthCompletion.Interval = duration;
        _monthCompletion.Start();
    }

    private void CompleteMonthTransition()
    {
        _monthCompletion.Stop();
        _monthAnimationVersion++;
        IsMonthTransitionActive = false;
        _outgoingCalendarDays.Clear();
        _monthTransform.X = 0;
        _outgoingMonthTransform.X = 0;
        if (_daysHost is not null) _daysHost.Opacity = 1;
        if (_outgoingDaysHost is not null) _outgoingDaysHost.Opacity = 0;
        _monthHeader?.SetCurrentValue(OpacityProperty, 1);
    }

    private void SetPopupPresence(bool value)
    {
        IsPopupOpen = value;
        PseudoClasses.Set(":popup-present", value);
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        if (IsOpen) SetCurrentValue(IsOpenProperty, false);
        _popupPresence.Initialize(false);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_surface is not null)
        {
            _surface.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
        }
        if (_daysHost is not null)
        {
            _daysHost.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        }
        if (_outgoingDaysHost is not null)
        {
            _outgoingDaysHost.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        }
        _monthTransform.Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, TranslateTransform.XProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast));
        _outgoingMonthTransform.Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, TranslateTransform.XProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast));
        if (_monthHeader is not null)
        {
            _monthHeader.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        }
        if (scheme == MdMotionScheme.None && IsMonthTransitionActive) CompleteMonthTransition();
        if (!IsOpen)
            _popupPresence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
    }

    private void OnSelectedDateChanged()
    {
        if (SelectedDate is { } selected)
        {
            SetCurrentValue(DisplayDateProperty, selected);
        }
        UpdateDisplayText();
        UpdatePseudoClasses();
        RebuildCalendar();
    }

    private void OnCultureChanged()
    {
        UpdateLocalizedText();
        UpdateDisplayText();
        RebuildCalendar();
    }

    private void UpdateLocalizedText()
    {
        var culture = MdLocalization.ResolveCulture(this);
        if (Equals(Label, _defaultLabelText))
        {
            _defaultLabelText = MdLocalization.GetString("Date", culture);
            SetCurrentValue(LabelProperty, _defaultLabelText);
        }
        SetAndRaise(TodayTextProperty, ref _todayText, MdLocalization.GetString("Today", culture));
        SetAndRaise(CancelTextProperty, ref _cancelText, MdLocalization.GetString("Cancel", culture));
        SetAndRaise(ConfirmTextProperty, ref _confirmText, MdLocalization.GetString("OK", culture));
        SetAndRaise(HeaderTextProperty, ref _headerText, MdLocalization.GetString("ChooseDate", culture));
    }

    private void UpdateDisplayText()
    {
        var culture = MdLocalization.ResolveCulture(this);
        DisplayText = SelectedDate?.ToString(DateFormat, culture) ?? MdLocalization.GetString("ChooseDate", culture);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":docked", Mode == MdDatePickerMode.Docked);
        PseudoClasses.Set(":modal", Mode == MdDatePickerMode.Modal);
        PseudoClasses.Set(":has-value", SelectedDate is not null);
    }

    private void RebuildCalendar()
    {
        var culture = MdLocalization.ResolveCulture(this);
        var monthStart = new DateTimeOffset(DisplayDate.Year, DisplayDate.Month, 1, 0, 0, 0, DisplayDate.Offset);
        MonthText = monthStart.ToString("Y", culture);
        var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
        var leadingDays = (7 + (int)monthStart.DayOfWeek - (int)firstDayOfWeek) % 7;
        var firstVisible = monthStart.AddDays(-leadingDays);
        var today = DateTimeOffset.Now.Date;

        _weekdayLabels.Clear();
        for (var index = 0; index < 7; index++)
        {
            var day = (DayOfWeek)(((int)firstDayOfWeek + index) % 7);
            var label = culture.TwoLetterISOLanguageName == "zh"
                ? day switch
                {
                    DayOfWeek.Sunday => "日",
                    DayOfWeek.Monday => "一",
                    DayOfWeek.Tuesday => "二",
                    DayOfWeek.Wednesday => "三",
                    DayOfWeek.Thursday => "四",
                    DayOfWeek.Friday => "五",
                    _ => "六"
                }
                : culture.DateTimeFormat.AbbreviatedDayNames[(int)day][..1];
            _weekdayLabels.Add(label);
        }
        var selectedDate = SelectedDate?.Date;
        var minimum = MinimumDate?.Date;
        var maximum = MaximumDate?.Date;

        _calendarDays.Clear();
        for (var index = 0; index < 42; index++)
        {
            var date = firstVisible.AddDays(index);
            var value = date.Date;
            var enabled = (!minimum.HasValue || value >= minimum.Value) &&
                          (!maximum.HasValue || value <= maximum.Value);
            var isSelected = selectedDate == value;
            var isToday = today == value;
            var accessibleText = date.ToString("D", culture);
            if (isSelected) accessibleText += $", {MdLocalization.GetString("Selected", culture)}";
            if (isToday) accessibleText += $", {MdLocalization.GetString("Today", culture)}";
            if (!enabled) accessibleText += $", {MdLocalization.GetString("Unavailable", culture)}";
            _calendarDays.Add(new MdCalendarDay(
                date,
                date.Day.ToString(culture),
                accessibleText,
                date.Month == monthStart.Month,
                isSelected,
                isToday,
                enabled));
        }
    }
}
