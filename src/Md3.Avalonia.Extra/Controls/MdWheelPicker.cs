using System.Globalization;
using System.Diagnostics;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Threading;
using Md3.Avalonia.Localization;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>A bounded integer wheel with a centered selection and direct manipulation.</summary>
/// <remarks>Values are discrete steps anchored at Minimum. A Maximum that is not on the step grid
/// is a bound, not an extra value. A dragged wheel carries release velocity into a bounded
/// deceleration and snaps to a step; Reduced and None motion snap immediately.</remarks>
public sealed class MdWheelPicker : TemplatedControl
{
    public static readonly StyledProperty<int> MinimumProperty = AvaloniaProperty.Register<MdWheelPicker, int>(
        nameof(Minimum), coerce: static (o, v) => Math.Min(v, ((MdWheelPicker)o).Maximum));
    public static readonly StyledProperty<int> MaximumProperty = AvaloniaProperty.Register<MdWheelPicker, int>(
        nameof(Maximum), 100, coerce: static (o, v) => Math.Max(v, ((MdWheelPicker)o).Minimum));
    public static readonly StyledProperty<int> StepProperty = AvaloniaProperty.Register<MdWheelPicker, int>(
        nameof(Step), 1, coerce: static (_, v) => Math.Max(1, v));
    public static readonly StyledProperty<int> ValueProperty = AvaloniaProperty.Register<MdWheelPicker, int>(
        nameof(Value), defaultBindingMode: BindingMode.TwoWay, coerce: static (o, v) => ((MdWheelPicker)o).Snap(v));
    public static readonly StyledProperty<double> ItemHeightProperty = AvaloniaProperty.Register<MdWheelPicker, double>(
        nameof(ItemHeight), 40, coerce: static (_, v) => double.IsFinite(v) ? Math.Clamp(v, 32, 100) : 40);
    public static readonly StyledProperty<bool> IsReadOnlyProperty = AvaloniaProperty.Register<MdWheelPicker, bool>(nameof(IsReadOnly));
    public static readonly StyledProperty<IBrush?> SelectionBrushProperty = AvaloniaProperty.Register<MdWheelPicker, IBrush?>(nameof(SelectionBrush));
    public static readonly StyledProperty<IBrush?> SelectionForegroundProperty = AvaloniaProperty.Register<MdWheelPicker, IBrush?>(nameof(SelectionForeground));

    private IPointer? _pointer;
    private double _startY;
    private double _startPosition;
    private double _position;
    private double _velocity;
    private double _snapTarget;
    private bool _moved;
    private bool _settingUserValue;
    private WheelMotionPhase _motion;
    private readonly WheelReleaseVelocity _releaseVelocity = new();
    private readonly DispatcherTimer _motionTimer;
    private readonly Stopwatch _motionClock = new();

    private enum WheelMotionPhase { None, Inertia, Snap }

    static MdWheelPicker()
    {
        AffectsRender<MdWheelPicker>(ValueProperty, MinimumProperty, MaximumProperty, StepProperty, ItemHeightProperty,
            ForegroundProperty, FontFamilyProperty, FontSizeProperty, FontWeightProperty, FontStyleProperty,
            SelectionBrushProperty, SelectionForegroundProperty, FlowDirectionProperty);
        AffectsMeasure<MdWheelPicker>(ItemHeightProperty);
        MinimumProperty.Changed.AddClassHandler<MdWheelPicker>((c, _) =>
        {
            c.CancelInteraction(); c.CoerceValue(MaximumProperty); c.CoerceValue(ValueProperty); c.SyncPosition();
        });
        MaximumProperty.Changed.AddClassHandler<MdWheelPicker>((c, _) =>
        {
            c.CancelInteraction(); c.CoerceValue(MinimumProperty); c.CoerceValue(ValueProperty); c.SyncPosition();
        });
        StepProperty.Changed.AddClassHandler<MdWheelPicker>((c, _) =>
        {
            c.CancelInteraction(); c.CoerceValue(ValueProperty); c.SyncPosition();
        });
        ItemHeightProperty.Changed.AddClassHandler<MdWheelPicker>((c, _) => c.CancelInteraction());
        ValueProperty.Changed.AddClassHandler<MdWheelPicker>((c, _) =>
        {
            if (!c._settingUserValue) c.CancelInteraction();
        });
        IsReadOnlyProperty.Changed.AddClassHandler<MdWheelPicker>((c, _) => { if (c.IsReadOnly) c.CancelInteraction(); });
        IsEffectivelyEnabledProperty.Changed.AddClassHandler<MdWheelPicker>((c, _) => { if (!c.IsEffectivelyEnabled) c.CancelInteraction(); });
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdWheelPicker>((c, _) =>
        {
            if (c._motion != WheelMotionPhase.None && !c.MotionEnabled()) c.StopMotionAndSnap();
        });
    }

    public MdWheelPicker()
    {
        Focusable = true;
        ClipToBounds = true;
        _motionTimer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(16) };
        _motionTimer.Tick += OnMotionTick;
        SyncPosition();
        GotFocus += (_, _) => InvalidateVisual();
        LostFocus += (_, _) => InvalidateVisual();
    }

    /// <summary>Inclusive lower bound; coerced to no greater than Maximum.</summary>
    public int Minimum { get => GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    /// <summary>Inclusive upper bound; coerced to no less than Minimum.</summary>
    public int Maximum { get => GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    /// <summary>Positive integer spacing between selectable values.</summary>
    public int Step { get => GetValue(StepProperty); set => SetValue(StepProperty, value); }
    /// <summary>Selected grid value; clamped and snapped, with midpoint ties toward the larger value.</summary>
    public int Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    /// <summary>Height in DIP of each visible row, coerced to 32–100.</summary>
    public double ItemHeight { get => GetValue(ItemHeightProperty); set => SetValue(ItemHeightProperty, value); }
    /// <summary>Prevents pointer, keyboard and automation changes, without disabling inspection.</summary>
    public bool IsReadOnly { get => GetValue(IsReadOnlyProperty); set => SetValue(IsReadOnlyProperty, value); }
    /// <summary>Brush behind the selected row, supplied by the Extra theme.</summary>
    public IBrush? SelectionBrush { get => GetValue(SelectionBrushProperty); set => SetValue(SelectionBrushProperty, value); }
    /// <summary>Brush for selected text, supplied by the Extra theme.</summary>
    public IBrush? SelectionForeground { get => GetValue(SelectionForegroundProperty); set => SetValue(SelectionForegroundProperty, value); }
    /// <summary>Raised only when user input or automation changes the selected value.</summary>
    public event EventHandler<int>? ValueChanged;

    private long LastIndex => ((long)Maximum - Minimum) / Step;
    internal int LastValue => (int)((long)Minimum + LastIndex * Step);
    private long CurrentIndex => ((long)Value - Minimum) / Step;
    private int Snap(int value)
    {
        var offset = (long)Math.Clamp(value, Minimum, Maximum) - Minimum;
        var index = Math.Min((offset + Step / 2L) / Step, LastIndex);
        return (int)((long)Minimum + index * Step);
    }

    protected override Size MeasureOverride(Size availableSize) => new(120, ItemHeight * 5);

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.DrawRectangle(Brushes.Transparent, null, new Rect(Bounds.Size));
        var rowHeight = Math.Min(ItemHeight, Bounds.Height / 5);
        if (rowHeight <= 0 || Bounds.Width <= 0) return;
        var center = Bounds.Height / 2;
        context.DrawRectangle(SelectionBrush ?? Brushes.LightGray, null,
            new RoundedRect(new Rect(0, center - rowHeight / 2, Bounds.Width, rowHeight), 8));
        using (context.PushClip(new Rect(Bounds.Size)))
        {
            // Include an extra row on each side so fractional positions do not leave gaps.
            var centerIndex = (long)Math.Floor(_position);
            for (var offset = -3; offset <= 3; offset++)
            {
                var index = centerIndex + offset;
                if (index < 0 || index > LastIndex) continue;
                var value = (long)Minimum + index * Step;
                var text = new FormattedText(value.ToString(CultureInfo.CurrentCulture), CultureInfo.CurrentCulture,
                    FlowDirection, new Typeface(FontFamily, FontStyle, FontWeight), Math.Max(1, FontSize),
                    index == CurrentIndex ? SelectionForeground ?? Brushes.Black : Foreground ?? Brushes.Gray);
                context.DrawText(text, new Point((Bounds.Width - text.Width) / 2, center + (index - _position) * rowHeight - text.Height / 2));
            }
        }
        if (IsKeyboardFocusWithin)
            context.DrawRectangle(null, new Pen(SelectionForeground ?? Brushes.Black, 2),
                new Rect(1, 1, Math.Max(0, Bounds.Width - 2), Math.Max(0, Bounds.Height - 2)), 8);
    }

    private bool RequestIndex(long index)
    {
        if (IsReadOnly || !IsEffectivelyEnabled) return false;
        var target = (int)((long)Minimum + Math.Clamp(index, 0, LastIndex) * Step);
        if (target == Value) return false;
        var previous = Value;
        _settingUserValue = true;
        try { SetCurrentValue(ValueProperty, target); }
        finally { _settingUserValue = false; }
        // A binding may have synchronously replaced/coerced the requested value.
        if (Value != target) CancelInteraction();
        if (Value == previous) return false;
        ValueChanged?.Invoke(this, Value);
        return true;
    }

    private static long NearestIndex(double position) => (long)Math.Floor(position + 0.5);

    private void SyncPosition()
    {
        _position = CurrentIndex;
        InvalidateVisual();
    }

    private void SetPosition(double position)
    {
        _position = Math.Clamp(position, 0, (double)LastIndex);
        InvalidateVisual();
        RequestIndex(NearestIndex(_position));
    }

    private bool MotionEnabled() => MdMotion.Resolve(this, MdMotionKind.Spatial, MdMotionSpeed.Default).IsEnabled;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (IsReadOnly || !IsEffectivelyEnabled || _pointer is not null ||
            (e.Pointer.Type == PointerType.Mouse && !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)) return;
        StopMotion(); // re-grab the currently displayed position, without a jump
        Focus();
        _pointer = e.Pointer;
        _startY = e.GetPosition(this).Y;
        _startPosition = _position;
        _releaseVelocity.Reset(e.Timestamp, _position);
        _moved = false;
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    private void UpdateDrag(double y, ulong timestamp)
    {
        var delta = _startY - y;
        if (Math.Abs(delta) >= 8) _moved = true;
        var rowHeight = Math.Min(ItemHeight, Bounds.Height / 5);
        if (_moved && rowHeight > 0)
        {
            SetPosition(_startPosition + delta / rowHeight);
            if (_pointer is not null) _releaseVelocity.Add(timestamp, _position);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!ReferenceEquals(_pointer, e.Pointer) || !ReferenceEquals(e.Pointer.Captured, this)) return;
        UpdateDrag(e.GetPosition(this).Y, e.Timestamp);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!ReferenceEquals(_pointer, e.Pointer)) return;
        UpdateDrag(e.GetPosition(this).Y, e.Timestamp);
        if (!ReferenceEquals(_pointer, e.Pointer)) return; // host changed state during the callback
        var moved = _moved;
        var velocity = _releaseVelocity.Velocity;
        CancelPointer();
        if (!moved)
        {
            var rowHeight = Math.Min(ItemHeight, Bounds.Height / 5);
            if (rowHeight > 0)
                RequestIndex(CurrentIndex + (long)Math.Clamp(Math.Round((e.GetPosition(this).Y - Bounds.Height / 2) / rowHeight, MidpointRounding.AwayFromZero), -2, 2));
            SyncPosition();
        }
        else StartMotion(velocity);
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (ReferenceEquals(_pointer, e.Pointer))
        {
            _pointer = null;
            StopMotionAndSnap(); // cancellation never flings
        }
        base.OnPointerCaptureLost(e);
    }

    private void CancelPointer()
    {
        var pointer = _pointer;
        _pointer = null;
        if (pointer is not null && ReferenceEquals(pointer.Captured, this)) pointer.Capture(null);
    }

    private void CancelInteraction()
    {
        CancelPointer();
        StopMotionAndSnap();
    }

    private void StopMotion()
    {
        _motion = WheelMotionPhase.None;
        _velocity = 0;
        _motionTimer?.Stop();
        _motionClock.Reset();
    }

    private void StopMotionAndSnap()
    {
        StopMotion();
        SyncPosition();
    }

    private void StartMotion(double velocity)
    {
        if (!MotionEnabled() || IsReadOnly || !IsEffectivelyEnabled)
        {
            StopMotionAndSnap();
            return;
        }
        _velocity = velocity;
        _motion = Math.Abs(velocity) >= 0.75 ? WheelMotionPhase.Inertia : WheelMotionPhase.Snap;
        _snapTarget = NearestIndex(_position);
        _motionClock.Restart();
        _motionTimer.Start();
    }

    private void OnMotionTick(object? sender, EventArgs e)
    {
        var elapsed = _motionClock.Elapsed;
        _motionClock.Restart();
        AdvanceMotion(elapsed);
    }

    // Advancing by bounded substeps keeps the snap stable under occasional delayed frames.
    internal void AdvanceMotion(TimeSpan elapsed)
    {
        if (_motion == WheelMotionPhase.None || elapsed <= TimeSpan.Zero) return;
        if (!MotionEnabled()) { StopMotionAndSnap(); return; }
        var remaining = Math.Min(elapsed.TotalSeconds, 0.05);
        while (remaining > 0 && _motion != WheelMotionPhase.None)
        {
            var dt = Math.Min(1d / 120d, remaining);
            remaining -= dt;
            var phase = _motion;
            if (phase == WheelMotionPhase.Inertia)
            {
                const double friction = 7;
                var decay = Math.Exp(-friction * dt);
                var next = _position + _velocity * (1 - decay) / friction;
                _velocity *= decay;
                var boundary = next <= 0 || next >= LastIndex;
                SetPosition(next);
                if (_motion != phase) return; // a host callback can cancel or replace the motion
                if (boundary || Math.Abs(_velocity) < 0.75)
                {
                    if (boundary) _velocity = 0;
                    _snapTarget = NearestIndex(_position);
                    _motion = WheelMotionPhase.Snap;
                }
            }
            else
            {
                var spring = MdMotion.Resolve(this, MdMotionKind.Spatial, MdMotionSpeed.Default).Spring;
                var damping = 2 * spring.DampingRatio * Math.Sqrt(spring.Stiffness);
                _velocity += (spring.Stiffness * (_snapTarget - _position) - damping * _velocity) * dt;
                SetPosition(_position + _velocity * dt);
                if (_motion != phase) return;
                if (Math.Abs(_snapTarget - _position) < 0.002 && Math.Abs(_velocity) < 0.025)
                {
                    SetPosition(_snapTarget);
                    if (_motion == phase) StopMotionAndSnap();
                }
            }
        }
    }

    internal bool IsSettling => _motion != WheelMotionPhase.None;
    internal double VisualPosition => _position;

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelInteraction();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (IsKeyboardFocusWithin && e.Delta.Y != 0)
        {
            CancelInteraction();
            if (RequestIndex(CurrentIndex + (e.Delta.Y > 0 ? -1 : 1))) e.Handled = true;
            SyncPosition();
        }
        if (!e.Handled) base.OnPointerWheelChanged(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (IsReadOnly || !IsEffectivelyEnabled) { base.OnKeyDown(e); return; }
        long target;
        if (e.Key == Key.Up) target = CurrentIndex - 1;
        else if (e.Key == Key.Down) target = CurrentIndex + 1;
        else if (e.Key == Key.PageUp) target = CurrentIndex - 5;
        else if (e.Key == Key.PageDown) target = CurrentIndex + 5;
        else if (e.Key == Key.Home) target = 0;
        else if (e.Key == Key.End) target = LastIndex;
        else { base.OnKeyDown(e); return; }
        CancelInteraction();
        RequestIndex(target);
        SyncPosition();
        e.Handled = true;
    }

    internal void RequestAutomationValue(double value)
    {
        if (!double.IsFinite(value) || value < Minimum || value > LastValue) throw new ArgumentOutOfRangeException(nameof(value));
        if (IsReadOnly) throw new InvalidOperationException("The wheel picker is read-only.");
        var index = (long)Math.Round((value - Minimum) / Step, MidpointRounding.AwayFromZero);
        CancelInteraction();
        RequestIndex(index);
        SyncPosition();
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new MdWheelPickerAutomationPeer(this);
}

// A recent, direction-aware sample window in row indices per second. A pause, out-of-order
// timestamps and a reversed gesture cannot accidentally reuse older momentum.
internal sealed class WheelReleaseVelocity
{
    private readonly List<(ulong Time, double Position)> _samples = [];
    internal void Reset(ulong timestamp, double position) { _samples.Clear(); Add(timestamp, position); }
    internal void Add(ulong timestamp, double position)
    {
        if (_samples.Count > 0 && timestamp < _samples[^1].Time) _samples.Clear();
        while (_samples.Count > 0 && timestamp - _samples[0].Time > 120) _samples.RemoveAt(0);
        if (_samples.Count > 0 && timestamp == _samples[^1].Time) _samples[^1] = (timestamp, position);
        else _samples.Add((timestamp, position));
        if (_samples.Count > 16) _samples.RemoveAt(0);
    }
    internal double Velocity
    {
        get
        {
            if (_samples.Count < 2) return 0;
            var start = 0;
            var direction = 0;
            for (var i = _samples.Count - 2; i >= 0; i--)
            {
                var delta = _samples[i + 1].Position - _samples[i].Position;
                var sign = Math.Abs(delta) < 0.0001 ? 0 : Math.Sign(delta);
                if (sign != 0 && direction != 0 && sign != direction) { start = i + 1; break; }
                if (sign != 0) direction = sign;
            }
            var first = _samples[start];
            var last = _samples[^1];
            var elapsed = last.Time - first.Time;
            if (elapsed == 0) return 0;
            return Math.Clamp((last.Position - first.Position) * 1000 / Math.Max(8d, elapsed), -24, 24);
        }
    }
}

internal sealed class MdWheelPickerAutomationPeer : ControlAutomationPeer, IRangeValueProvider
{
    private int _minimum;
    private int _maximum;
    private bool _readOnly;

    public MdWheelPickerAutomationPeer(MdWheelPicker owner) : base(owner)
    {
        _minimum = owner.Minimum;
        _maximum = owner.LastValue;
        _readOnly = owner.IsReadOnly;
        owner.PropertyChanged += (_, e) =>
        {
            if (e.Property == MdWheelPicker.ValueProperty)
                RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, Convert.ToDouble(e.OldValue), Convert.ToDouble(e.NewValue));
            if (e.Property == MdWheelPicker.MinimumProperty || e.Property == MdWheelPicker.MaximumProperty || e.Property == MdWheelPicker.StepProperty)
            {
                var minimum = owner.Minimum;
                var maximum = owner.LastValue;
                if (_minimum != minimum) RaisePropertyChangedEvent(RangeValuePatternIdentifiers.MinimumProperty, (double)_minimum, (double)minimum);
                if (_maximum != maximum) RaisePropertyChangedEvent(RangeValuePatternIdentifiers.MaximumProperty, (double)_maximum, (double)maximum);
                _minimum = minimum;
                _maximum = maximum;
            }
            if (e.Property == MdWheelPicker.IsReadOnlyProperty && _readOnly != owner.IsReadOnly)
            {
                RaisePropertyChangedEvent(RangeValuePatternIdentifiers.IsReadOnlyProperty, _readOnly, owner.IsReadOnly);
                _readOnly = owner.IsReadOnly;
            }
        };
    }

    private MdWheelPicker Picker => (MdWheelPicker)Owner;
    public bool IsReadOnly => Picker.IsReadOnly;
    public double Minimum => Picker.Minimum;
    public double Maximum => Picker.LastValue;
    public double Value => Picker.Value;
    public double SmallChange => Picker.Step;
    public double LargeChange => (double)Picker.Step * 5;
    public void SetValue(double value) { EnsureEnabled(); Picker.RequestAutomationValue(value); }
    protected override string? GetNameCore()
    {
        var name = base.GetNameCore();
        return string.IsNullOrWhiteSpace(name) ? MdLocalization.GetString("WheelPicker", Picker) : name;
    }
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Spinner;
    protected override bool IsControlElementCore() => true;
    protected override bool IsContentElementCore() => true;
}
