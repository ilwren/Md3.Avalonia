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
/// A standalone Material 3 time picker with an interactive clock face and keyboard input mode.
/// SelectedTime, IsOpen, Hour and Minute all support two-way binding.
/// </summary>
[PseudoClasses(":dial", ":input", ":twenty-four-hour", ":has-value", ":hour-dial", ":minute-dial", ":popup-present", ":reduced-motion", ":no-motion")]
public class MdTimePicker : TemplatedControl, IMdPopupOwner, IMdPopupPresenceOwner
{
    public static readonly StyledProperty<TimeSpan?> SelectedTimeProperty =
        AvaloniaProperty.Register<MdTimePicker, TimeSpan?>(nameof(SelectedTime),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<int> HourProperty =
        AvaloniaProperty.Register<MdTimePicker, int>(nameof(Hour), 12,
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<int> MinuteProperty =
        AvaloniaProperty.Register<MdTimePicker, int>(nameof(Minute), 0,
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<int> MinuteStepProperty =
        AvaloniaProperty.Register<MdTimePicker, int>(nameof(MinuteStep), 1);

    public static readonly StyledProperty<bool> Is24HourProperty =
        AvaloniaProperty.Register<MdTimePicker, bool>(nameof(Is24Hour));

    public static readonly StyledProperty<MdTimePickerMode> ModeProperty =
        AvaloniaProperty.Register<MdTimePicker, MdTimePickerMode>(nameof(Mode));

    public static readonly StyledProperty<MdTimeDialPart> ActiveDialPartProperty =
        AvaloniaProperty.Register<MdTimePicker, MdTimeDialPart>(nameof(ActiveDialPart), MdTimeDialPart.Hour);

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MdTimePicker, bool>(nameof(IsOpen),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<object?> LabelProperty =
        AvaloniaProperty.Register<MdTimePicker, object?>(nameof(Label), "Time");

    public static readonly DirectProperty<MdTimePicker, string> DisplayTextProperty =
        AvaloniaProperty.RegisterDirect<MdTimePicker, string>(nameof(DisplayText), picker => picker.DisplayText);

    public static readonly DirectProperty<MdTimePicker, string> HourTextProperty =
        AvaloniaProperty.RegisterDirect<MdTimePicker, string>(nameof(HourText), picker => picker.HourText);

    public static readonly DirectProperty<MdTimePicker, string> MinuteTextProperty =
        AvaloniaProperty.RegisterDirect<MdTimePicker, string>(nameof(MinuteText), picker => picker.MinuteText);

    public static readonly DirectProperty<MdTimePicker, string> PeriodTextProperty =
        AvaloniaProperty.RegisterDirect<MdTimePicker, string>(nameof(PeriodText), picker => picker.PeriodText);
    public static readonly DirectProperty<MdTimePicker, string> CancelTextProperty =
        AvaloniaProperty.RegisterDirect<MdTimePicker, string>(nameof(CancelText), picker => picker.CancelText);
    public static readonly DirectProperty<MdTimePicker, string> ConfirmTextProperty =
        AvaloniaProperty.RegisterDirect<MdTimePicker, string>(nameof(ConfirmText), picker => picker.ConfirmText);
    public static readonly DirectProperty<MdTimePicker, string> HeaderTextProperty =
        AvaloniaProperty.RegisterDirect<MdTimePicker, string>(nameof(HeaderText), picker => picker.HeaderText);
    public static readonly DirectProperty<MdTimePicker, bool> IsPopupOpenProperty =
        AvaloniaProperty.RegisterDirect<MdTimePicker, bool>(nameof(IsPopupOpen), picker => picker.IsPopupOpen);

    private bool _syncing;
    private string _displayText = "Choose time";
    private string _hourText = "12";
    private string _minuteText = "00";
    private string _periodText = "PM";
    private string _cancelText = "Cancel";
    private string _confirmText = "OK";
    private string _headerText = "Choose time";
    private string _defaultLabelText = "Time";
    private TimeSpan? _valueAtOpen;
    private bool _commitOnClose;
    private Button? _anchorButton;
    private Button? _hourUpButton;
    private Button? _hourDownButton;
    private Button? _minuteUpButton;
    private Button? _minuteDownButton;
    private Button? _periodButton;
    private Button? _inputPeriodButton;
    private Control? _hourDialHost;
    private Control? _minuteDialHost;
    private Button? _cancelButton;
    private Button? _confirmButton;
    private TextBlock? _displayTextBlock;
    private MdTextBox? _hourInput;
    private MdTextBox? _minuteInput;
    private MdTimeDial? _clockFace;
    private Popup? _popup;
    private InputElement? _popupSurface;
    private Border? _surface;
    private Control? _dialPanel;
    private Control? _inputPanel;
    private readonly MdPresenceController _popupPresence;
    private bool _isPopupOpen;
    private int _modeAnimationVersion;

    static MdTimePicker()
    {
        SelectedTimeProperty.Changed.AddClassHandler<MdTimePicker>((picker, _) => picker.SyncFromSelectedTime());
        HourProperty.Changed.AddClassHandler<MdTimePicker>((picker, _) => picker.SyncFromParts());
        MinuteProperty.Changed.AddClassHandler<MdTimePicker>((picker, _) => picker.SyncFromParts());
        Is24HourProperty.Changed.AddClassHandler<MdTimePicker>((picker, _) => picker.RefreshTextAndState());
        ModeProperty.Changed.AddClassHandler<MdTimePicker>((picker, _) => picker.OnModeChanged());
        ActiveDialPartProperty.Changed.AddClassHandler<MdTimePicker>((picker, _) => picker.UpdatePseudoClasses());
        IsOpenProperty.Changed.AddClassHandler<MdTimePicker>((picker, _) => picker.OnOpenStateChanged());
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdTimePicker>((picker, _) => picker.OnCultureChanged());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdTimePicker>((picker, _) => picker.UpdateMotion());
    }

    // Android has no Escape key. The system back gesture arrives as TopLevel.BackRequested and
    // has to dismiss this surface, or it is unreachable by the one gesture phone users rely on.
    private readonly MdBackScope _backScope;

    public MdTimePicker()
    {
        _backScope = new MdBackScope(this, OnBackRequested);
        _popupPresence = new MdPresenceController(SetPopupPresence);
        _popupPresence.Initialize(IsOpen);
        UpdateLocalizedText();
        RefreshTextAndState();
    }

    public TimeSpan? SelectedTime
    {
        get => GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    public int Hour
    {
        get => GetValue(HourProperty);
        set => SetValue(HourProperty, value);
    }

    public int Minute
    {
        get => GetValue(MinuteProperty);
        set => SetValue(MinuteProperty, value);
    }

    public int MinuteStep
    {
        get => GetValue(MinuteStepProperty);
        set => SetValue(MinuteStepProperty, value);
    }

    public bool Is24Hour
    {
        get => GetValue(Is24HourProperty);
        set => SetValue(Is24HourProperty, value);
    }

    public MdTimePickerMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public MdTimeDialPart ActiveDialPart
    {
        get => GetValue(ActiveDialPartProperty);
        set => SetValue(ActiveDialPartProperty, value);
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

    public string DisplayText
    {
        get => _displayText;
        private set => SetAndRaise(DisplayTextProperty, ref _displayText, value);
    }

    public string HourText
    {
        get => _hourText;
        private set => SetAndRaise(HourTextProperty, ref _hourText, value);
    }

    public string MinuteText
    {
        get => _minuteText;
        private set => SetAndRaise(MinuteTextProperty, ref _minuteText, value);
    }

    public string PeriodText
    {
        get => _periodText;
        private set => SetAndRaise(PeriodTextProperty, ref _periodText, value);
    }

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
        DetachHandlers();
        if (_popup is not null) _popup.Closed -= OnPopupClosed;
        if (_popupSurface is not null)
        {
            _popupSurface.RemoveHandler(PointerWheelChangedEvent, OnPopupSurfaceWheel);
            _popupSurface.RemoveHandler(KeyDownEvent, OnPopupSurfaceKeyDown);
            _popupSurface = null;
        }
        base.OnApplyTemplate(e);
        _anchorButton = e.NameScope.Find<Button>("PART_AnchorButton");
        _hourUpButton = e.NameScope.Find<Button>("PART_HourUpButton");
        _hourDownButton = e.NameScope.Find<Button>("PART_HourDownButton");
        _minuteUpButton = e.NameScope.Find<Button>("PART_MinuteUpButton");
        _minuteDownButton = e.NameScope.Find<Button>("PART_MinuteDownButton");
        _periodButton = e.NameScope.Find<Button>("PART_PeriodButton");
        _inputPeriodButton = e.NameScope.Find<Button>("PART_InputPeriodButton");
        _hourDialHost = e.NameScope.Find<Control>("PART_HourDialHost");
        _minuteDialHost = e.NameScope.Find<Control>("PART_MinuteDialHost");
        _cancelButton = e.NameScope.Find<Button>("PART_CancelButton");
        _confirmButton = e.NameScope.Find<Button>("PART_ConfirmButton");
        _displayTextBlock = e.NameScope.Find<TextBlock>("PART_DisplayText");
        _hourInput = e.NameScope.Find<MdTextBox>("PART_HourInput");
        _minuteInput = e.NameScope.Find<MdTextBox>("PART_MinuteInput");
        _clockFace = e.NameScope.Find<MdTimeDial>("PART_ClockFace");
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _surface = e.NameScope.Find<Border>("PART_Surface");
        _dialPanel = e.NameScope.Find<Control>("PART_DialPanel");
        _inputPanel = e.NameScope.Find<Control>("PART_InputPanel");
        if (_popup is not null) _popup.Closed += OnPopupClosed;
        _popupSurface = _popup?.Child as InputElement;
        if (_popupSurface is not null)
        {
            // handledEventsToo: the dial and the spin buttons mark their own input handled.
            _popupSurface.AddHandler(PointerWheelChangedEvent, OnPopupSurfaceWheel, handledEventsToo: false);
            _popupSurface.AddHandler(KeyDownEvent, OnPopupSurfaceKeyDown, handledEventsToo: false);
        }

        if (_anchorButton is not null) _anchorButton.Click += OnAnchorClick;
        if (_hourUpButton is not null) _hourUpButton.Click += OnHourUp;
        if (_hourDownButton is not null) _hourDownButton.Click += OnHourDown;
        if (_minuteUpButton is not null) _minuteUpButton.Click += OnMinuteUp;
        if (_minuteDownButton is not null) _minuteDownButton.Click += OnMinuteDown;
        if (_periodButton is not null) _periodButton.Click += OnPeriodClick;
        if (_inputPeriodButton is not null) _inputPeriodButton.Click += OnPeriodClick;
        _hourDialHost?.AddHandler(Button.ClickEvent, OnHourDialSelected);
        _minuteDialHost?.AddHandler(Button.ClickEvent, OnMinuteDialSelected);
        if (_cancelButton is not null) _cancelButton.Click += OnCancel;
        if (_confirmButton is not null) _confirmButton.Click += OnConfirm;
        if (_hourInput is not null) _hourInput.LostFocus += OnInputLostFocus;
        if (_minuteInput is not null) _minuteInput.LostFocus += OnInputLostFocus;
        UpdateInputText();
        _popupPresence.Initialize(IsOpen);
        UpdateMotion();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _popupPresence.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        HandleWheel(e);
        if (!e.Handled) base.OnPointerWheelChanged(e);
    }

    // On desktop the popup is a separate PopupRoot window, so a wheel or key event raised inside
    // it never reaches this control: the route ends at the popup root. Android renders popups
    // into the TopLevel overlay, where the same event bubbles straight here - which is why the
    // scroll-to-adjust gesture worked there and was dead on Windows. The popup surface forwards
    // into the same handlers.
    private void HandleWheel(PointerWheelEventArgs e)
    {
        if (Math.Abs(e.Delta.Y) <= 0) return;
        var delta = e.Delta.Y > 0 ? 1 : -1;

        if (IsOpen)
        {
            var source = e.Source as Visual;
            var overHour = IsWithin(source, _hourDialHost) || _hourInput?.IsKeyboardFocusWithin == true;
            SetPart(Hour + (overHour ? delta : 0),
                Minute + (overHour ? 0 : delta * Math.Max(1, MinuteStep)));
            e.Handled = true;
            return;
        }

        // Closed, with the pointer over the anchor field. Gating this on IsOpen killed the
        // scroll-to-adjust gesture on the field itself, which is where people reach for it.
        //
        // Focus is what makes it safe to restore. A wheel handler that fires on hover alone
        // silently rewrites the time whenever someone scrolls a page that happens to have a
        // picker in it, and leaving the event unhandled is exactly what lets that scroll pass
        // through to the page.
        if (!IsKeyboardFocusWithin) return;

        // The anchor renders one run of text, so there is no hour element to hit: split it down
        // the middle instead, left for hours and right for minutes.
        var overHourSegment = _displayTextBlock is { Bounds.Width: > 0 } text &&
                              e.GetPosition(text).X < text.Bounds.Width / 2;
        SetPart(Hour + (overHourSegment ? delta : 0),
            Minute + (overHourSegment ? 0 : delta * Math.Max(1, MinuteStep)));
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        HandleKeyDown(e);
        if (!e.Handled) base.OnKeyDown(e);
    }

    private void HandleKeyDown(KeyEventArgs e)
    {
        if (IsOpen && (e.Key == Key.Up || e.Key == Key.Down))
        {
            var delta = e.Key == Key.Up ? 1 : -1;
            var minuteFocused = _minuteInput?.IsKeyboardFocusWithin == true ||
                                _minuteUpButton?.IsKeyboardFocusWithin == true ||
                                _minuteDownButton?.IsKeyboardFocusWithin == true;
            SetPart(Hour + (minuteFocused ? 0 : delta),
                Minute + (minuteFocused ? delta * Math.Max(1, MinuteStep) : 0));
            e.Handled = true;
            return;
        }
        if (e.Key == Key.Escape && IsOpen)
        {
            CancelSelection();
            e.Handled = true;
            return;
        }
        if (e.Key == Key.Enter && IsOpen && Mode == MdTimePickerMode.Input)
        {
            CommitInputText();
            _commitOnClose = true;
            SetCurrentValue(IsOpenProperty, false);
            e.Handled = true;
        }
    }

    private void OnPopupSurfaceWheel(object? sender, PointerWheelEventArgs e) => HandleWheel(e);

    private void OnPopupSurfaceKeyDown(object? sender, KeyEventArgs e) => HandleKeyDown(e);

    private void DetachHandlers()
    {
        if (_anchorButton is not null) _anchorButton.Click -= OnAnchorClick;
        if (_hourUpButton is not null) _hourUpButton.Click -= OnHourUp;
        if (_hourDownButton is not null) _hourDownButton.Click -= OnHourDown;
        if (_minuteUpButton is not null) _minuteUpButton.Click -= OnMinuteUp;
        if (_minuteDownButton is not null) _minuteDownButton.Click -= OnMinuteDown;
        if (_periodButton is not null) _periodButton.Click -= OnPeriodClick;
        if (_inputPeriodButton is not null) _inputPeriodButton.Click -= OnPeriodClick;
        _hourDialHost?.RemoveHandler(Button.ClickEvent, OnHourDialSelected);
        _minuteDialHost?.RemoveHandler(Button.ClickEvent, OnMinuteDialSelected);
        if (_cancelButton is not null) _cancelButton.Click -= OnCancel;
        if (_confirmButton is not null) _confirmButton.Click -= OnConfirm;
        if (_hourInput is not null) _hourInput.LostFocus -= OnInputLostFocus;
        if (_minuteInput is not null) _minuteInput.LostFocus -= OnInputLostFocus;
    }

    private void OnAnchorClick(object? sender, RoutedEventArgs e) =>
        SetCurrentValue(IsOpenProperty, !IsOpen);

    private void OnHourUp(object? sender, RoutedEventArgs e) => SetPart(Hour + 1, Minute);
    private void OnHourDown(object? sender, RoutedEventArgs e) => SetPart(Hour - 1, Minute);
    private void OnMinuteUp(object? sender, RoutedEventArgs e) => SetPart(Hour, Minute + Math.Max(1, MinuteStep));
    private void OnMinuteDown(object? sender, RoutedEventArgs e) => SetPart(Hour, Minute - Math.Max(1, MinuteStep));
    private void OnHourDialSelected(object? sender, RoutedEventArgs e) =>
        SetCurrentValue(ActiveDialPartProperty, MdTimeDialPart.Hour);
    private void OnMinuteDialSelected(object? sender, RoutedEventArgs e) =>
        SetCurrentValue(ActiveDialPartProperty, MdTimeDialPart.Minute);
    private void OnPeriodClick(object? sender, RoutedEventArgs e) => SetPart(Hour + 12, Minute);
    private void OnInputLostFocus(object? sender, RoutedEventArgs e) => CommitInputText();
    private void OnCancel(object? sender, RoutedEventArgs e) => CancelSelection();

    private void OnConfirm(object? sender, RoutedEventArgs e)
    {
        CommitInputText();
        _commitOnClose = true;
        SetCurrentValue(IsOpenProperty, false);
    }

    private void OnOpenStateChanged()
    {
        _backScope.Update(IsOpen);
        if (IsOpen)
        {
            _popupPresence.Update(true, TimeSpan.Zero);
            _valueAtOpen = SelectedTime;
            _commitOnClose = false;
            MdPopupCoordinator.NotifyStateChanged(this);
            Dispatcher.UIThread.Post(() =>
            {
                if (!IsOpen) return;
                if (Mode == MdTimePickerMode.Input && _hourInput is { } hourInput)
                {
                    // Opening an input picker means "edit this time", not merely "show a panel".
                    // Put the caret in the first field and select its existing value so the first
                    // digit replaces it instead of appending to a two-character field.
                    if (hourInput.Focus()) hourInput.SelectAll();
                }
                else
                {
                    _clockFace?.Focus(NavigationMethod.Directional);
                }
            }, DispatcherPriority.Loaded);
            return;
        }

        if (!_commitOnClose && SelectedTime != _valueAtOpen)
            SetCurrentValue(SelectedTimeProperty, _valueAtOpen);
        _commitOnClose = false;
        _popupPresence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
        MdPopupCoordinator.NotifyStateChanged(this);
        Dispatcher.UIThread.Post(() => _anchorButton?.Focus(), DispatcherPriority.Input);
    }

    private void OnModeChanged()
    {
        UpdatePseudoClasses();
        var target = Mode == MdTimePickerMode.Input ? _inputPanel : _dialPanel;
        if (target is null || MdMotion.GetScheme(this) == MdMotionScheme.None) return;
        target.Opacity = 0;
        var version = ++_modeAnimationVersion;
        Dispatcher.UIThread.Post(() =>
        {
            if (version == _modeAnimationVersion) target.Opacity = 1;
        }, DispatcherPriority.Render);
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
        foreach (var panel in new[] { _dialPanel, _inputPanel })
        {
            if (panel is not null)
                panel.Transitions = MdMotionTransitions.Collect(
                    MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        }
        if (!IsOpen)
            _popupPresence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
    }

    private void CancelSelection()
    {
        SetCurrentValue(SelectedTimeProperty, _valueAtOpen);
        SetCurrentValue(IsOpenProperty, false);
        _anchorButton?.Focus();
    }

    private void CommitInputText()
    {
        if (_hourInput is null || _minuteInput is null) return;
        if (!int.TryParse(_hourInput.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var hour) ||
            !int.TryParse(_minuteInput.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var minute))
        {
            UpdateInputText();
            return;
        }

        if (!Is24Hour)
        {
            hour = Math.Clamp(hour, 1, 12) % 12;
            if (PeriodText == "PM") hour += 12;
        }
        SetPart(hour, Math.Clamp(minute, 0, 59));
    }

    private static bool IsWithin(Visual? source, Visual? host) =>
        source is not null && host is not null &&
        (ReferenceEquals(source, host) || source.GetVisualAncestors().Contains(host));

    private void SetPart(int hour, int minute)
    {
        var totalMinutes = ((hour * 60 + minute) % 1440 + 1440) % 1440;
        _syncing = true;
        SetCurrentValue(HourProperty, totalMinutes / 60);
        SetCurrentValue(MinuteProperty, totalMinutes % 60);
        SetCurrentValue(SelectedTimeProperty, TimeSpan.FromMinutes(totalMinutes));
        _syncing = false;
        RefreshTextAndState();
    }

    private void SyncFromSelectedTime()
    {
        if (_syncing) return;
        _syncing = true;
        if (SelectedTime is { } time)
        {
            var normalized = ((int)time.TotalMinutes % 1440 + 1440) % 1440;
            SetCurrentValue(HourProperty, normalized / 60);
            SetCurrentValue(MinuteProperty, normalized % 60);
        }
        _syncing = false;
        RefreshTextAndState();
    }

    private void SyncFromParts()
    {
        if (_syncing) return;
        SetPart(Hour, Minute);
    }

    private void OnCultureChanged()
    {
        UpdateLocalizedText();
        RefreshTextAndState();
    }

    private void UpdateLocalizedText()
    {
        var culture = MdLocalization.ResolveCulture(this);
        if (Equals(Label, _defaultLabelText))
        {
            _defaultLabelText = MdLocalization.GetString("Time", culture);
            SetCurrentValue(LabelProperty, _defaultLabelText);
        }
        SetAndRaise(CancelTextProperty, ref _cancelText, MdLocalization.GetString("Cancel", culture));
        SetAndRaise(ConfirmTextProperty, ref _confirmText, MdLocalization.GetString("OK", culture));
        SetAndRaise(HeaderTextProperty, ref _headerText, MdLocalization.GetString("ChooseTime", culture));
    }

    private void RefreshTextAndState()
    {
        var culture = MdLocalization.ResolveCulture(this);
        var normalizedHour = ((Hour % 24) + 24) % 24;
        var displayHour = Is24Hour ? normalizedHour : (normalizedHour + 11) % 12 + 1;
        HourText = displayHour.ToString(Is24Hour ? "00" : "0", culture);
        MinuteText = Math.Clamp(Minute, 0, 59).ToString("00", culture);
        PeriodText = normalizedHour >= 12 ? "PM" : "AM";
        DisplayText = SelectedTime is null
            ? MdLocalization.GetString("ChooseTime", culture)
            : Is24Hour ? $"{normalizedHour:00}:{Minute:00}" : $"{displayHour}:{Minute:00} {PeriodText}";
        UpdatePseudoClasses();
        UpdateInputText();
    }

    private void UpdateInputText()
    {
        if (_hourInput is not null) _hourInput.SetCurrentValue(TextBox.TextProperty, HourText);
        if (_minuteInput is not null) _minuteInput.SetCurrentValue(TextBox.TextProperty, MinuteText);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":dial", Mode == MdTimePickerMode.Dial);
        PseudoClasses.Set(":input", Mode == MdTimePickerMode.Input);
        PseudoClasses.Set(":twenty-four-hour", Is24Hour);
        PseudoClasses.Set(":has-value", SelectedTime is not null);
        PseudoClasses.Set(":hour-dial", ActiveDialPart == MdTimeDialPart.Hour);
        PseudoClasses.Set(":minute-dial", ActiveDialPart == MdTimeDialPart.Minute);
    }

    private bool OnBackRequested()
    {
        if (!IsOpen) return false;
        CancelSelection();
        return true;
    }
}
