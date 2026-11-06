using System.Globalization;
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
using Md3.Avalonia.Localization;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>A bounded integer wheel with a centered selection and direct manipulation.</summary>
/// <remarks>Values are discrete steps anchored at Minimum. A Maximum that is not on the step grid
/// is a bound, not an extra value. The control snaps immediately; it does not simulate inertia.</remarks>
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
    private long _startIndex;
    private bool _moved;

    static MdWheelPicker()
    {
        AffectsRender<MdWheelPicker>(ValueProperty, MinimumProperty, MaximumProperty, StepProperty, ItemHeightProperty,
            ForegroundProperty, FontFamilyProperty, FontSizeProperty, FontWeightProperty, FontStyleProperty,
            SelectionBrushProperty, SelectionForegroundProperty, FlowDirectionProperty);
        AffectsMeasure<MdWheelPicker>(ItemHeightProperty);
        MinimumProperty.Changed.AddClassHandler<MdWheelPicker>((c, _) => { c.CoerceValue(MaximumProperty); c.CoerceValue(ValueProperty); });
        MaximumProperty.Changed.AddClassHandler<MdWheelPicker>((c, _) => { c.CoerceValue(MinimumProperty); c.CoerceValue(ValueProperty); });
        StepProperty.Changed.AddClassHandler<MdWheelPicker>((c, _) => c.CoerceValue(ValueProperty));
        IsReadOnlyProperty.Changed.AddClassHandler<MdWheelPicker>((c, _) => { if (c.IsReadOnly) c.CancelPointer(); });
    }

    public MdWheelPicker()
    {
        Focusable = true;
        ClipToBounds = true;
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
            for (var offset = -2; offset <= 2; offset++)
            {
                var index = CurrentIndex + offset;
                if (index < 0 || index > LastIndex) continue;
                var value = (long)Minimum + index * Step;
                var text = new FormattedText(value.ToString(CultureInfo.CurrentCulture), CultureInfo.CurrentCulture,
                    FlowDirection, new Typeface(FontFamily, FontStyle, FontWeight), Math.Max(1, FontSize),
                    offset == 0 ? SelectionForeground ?? Brushes.Black : Foreground ?? Brushes.Gray);
                context.DrawText(text, new Point((Bounds.Width - text.Width) / 2, center + offset * rowHeight - text.Height / 2));
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
        SetCurrentValue(ValueProperty, target);
        ValueChanged?.Invoke(this, Value);
        return true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (IsReadOnly || !IsEffectivelyEnabled || _pointer is not null ||
            (e.Pointer.Type == PointerType.Mouse && !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)) return;
        Focus();
        _pointer = e.Pointer;
        _startY = e.GetPosition(this).Y;
        _startIndex = CurrentIndex;
        _moved = false;
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!ReferenceEquals(_pointer, e.Pointer) || !ReferenceEquals(e.Pointer.Captured, this)) return;
        var delta = _startY - e.GetPosition(this).Y;
        if (Math.Abs(delta) >= 8) _moved = true;
        var rowHeight = Math.Min(ItemHeight, Bounds.Height / 5);
        if (_moved && rowHeight > 0)
            RequestIndex(_startIndex + (long)Math.Clamp(Math.Round(delta / rowHeight, MidpointRounding.AwayFromZero), -LastIndex, LastIndex));
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!ReferenceEquals(_pointer, e.Pointer)) return;
        if (!_moved)
        {
            var rowHeight = Math.Min(ItemHeight, Bounds.Height / 5);
            if (rowHeight > 0)
                RequestIndex(CurrentIndex + (long)Math.Clamp(Math.Round((e.GetPosition(this).Y - Bounds.Height / 2) / rowHeight, MidpointRounding.AwayFromZero), -2, 2));
        }
        CancelPointer();
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        _pointer = null;
        base.OnPointerCaptureLost(e);
    }

    private void CancelPointer()
    {
        var pointer = _pointer;
        _pointer = null;
        if (pointer is not null && ReferenceEquals(pointer.Captured, this)) pointer.Capture(null);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelPointer();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (IsKeyboardFocusWithin && e.Delta.Y != 0 && RequestIndex(CurrentIndex + (e.Delta.Y > 0 ? -1 : 1)))
            e.Handled = true;
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
        RequestIndex(target);
        e.Handled = true;
    }

    internal void RequestAutomationValue(double value)
    {
        if (!double.IsFinite(value) || value < Minimum || value > LastValue) throw new ArgumentOutOfRangeException(nameof(value));
        if (IsReadOnly) throw new InvalidOperationException("The wheel picker is read-only.");
        var index = (long)Math.Round((value - Minimum) / Step, MidpointRounding.AwayFromZero);
        RequestIndex(index);
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new MdWheelPickerAutomationPeer(this);
}

internal sealed class MdWheelPickerAutomationPeer : ControlAutomationPeer, IRangeValueProvider
{
    public MdWheelPickerAutomationPeer(MdWheelPicker owner) : base(owner)
    {
        owner.PropertyChanged += (_, e) =>
        {
            if (e.Property == MdWheelPicker.ValueProperty)
                RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, Convert.ToDouble(e.OldValue), Convert.ToDouble(e.NewValue));
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
