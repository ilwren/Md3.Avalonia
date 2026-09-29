using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A Material slider retaining Avalonia Slider value, keyboard, pointer and range behavior.</summary>
[PseudoClasses(":value-indicator", ":stop-indicator", ":dragging", ":reduced-motion", ":no-motion")]
public sealed class MdSlider : Slider
{
    public static readonly StyledProperty<bool> ShowValueIndicatorProperty =
        AvaloniaProperty.Register<MdSlider, bool>(nameof(ShowValueIndicator));
    public static readonly StyledProperty<bool> ShowStopIndicatorProperty =
        AvaloniaProperty.Register<MdSlider, bool>(nameof(ShowStopIndicator), true);
    public static readonly StyledProperty<string> ValueFormatProperty =
        AvaloniaProperty.Register<MdSlider, string>(nameof(ValueFormat), "0");
    public static readonly DirectProperty<MdSlider, string> DisplayValueProperty =
        AvaloniaProperty.RegisterDirect<MdSlider, string>(nameof(DisplayValue), control => control.DisplayValue);
    public static readonly StyledProperty<double> VisualValueProperty =
        AvaloniaProperty.Register<MdSlider, double>(nameof(VisualValue));

    private string _displayValue = string.Empty;
    private Thumb? _thumb;
    private Control? _valueIndicator;
    private Control? _stateLayer;
    private bool _isDragging;
    private bool _isPointerInteracting;

    static MdSlider()
    {
        ShowValueIndicatorProperty.Changed.AddClassHandler<MdSlider>((slider, _) => slider.UpdateVisualState());
        ShowStopIndicatorProperty.Changed.AddClassHandler<MdSlider>((slider, _) => slider.UpdateVisualState());
        ValueProperty.Changed.AddClassHandler<MdSlider>((slider, _) => slider.OnValueChanged());
        VisualValueProperty.Changed.AddClassHandler<MdSlider>((slider, _) => slider.SyncValueFromTrack());
        ValueFormatProperty.Changed.AddClassHandler<MdSlider>((slider, _) => slider.UpdateVisualState());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdSlider>((slider, _) => slider.UpdateMotion());
    }

    public MdSlider()
    {
        AddHandler(PointerPressedEvent, OnPreviewPointerPressed, RoutingStrategies.Tunnel, true);
        AddHandler(PointerReleasedEvent, OnPreviewPointerReleased, RoutingStrategies.Tunnel, true);
        SetCurrentValue(VisualValueProperty, Value);
        UpdateVisualState();
    }

    public bool ShowValueIndicator { get => GetValue(ShowValueIndicatorProperty); set => SetValue(ShowValueIndicatorProperty, value); }
    public bool ShowStopIndicator { get => GetValue(ShowStopIndicatorProperty); set => SetValue(ShowStopIndicatorProperty, value); }
    public string ValueFormat { get => GetValue(ValueFormatProperty); set => SetValue(ValueFormatProperty, value); }
    public string DisplayValue => _displayValue;
    /// <summary>The rendered value. It follows pointer drag directly and settles keyboard/programmatic changes.</summary>
    public double VisualValue => GetValue(VisualValueProperty);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_thumb is not null)
        {
            _thumb.DragStarted -= OnThumbDragStarted;
            _thumb.DragCompleted -= OnThumbDragCompleted;
        }
        base.OnApplyTemplate(e);
        _thumb = e.NameScope.Find<Thumb>("PART_Thumb");
        _valueIndicator = e.NameScope.Find<Control>("PART_ThumbValue");
        _stateLayer = e.NameScope.Find<Control>("PART_ThumbStateLayer");
        if (_thumb is not null)
        {
            _thumb.DragStarted += OnThumbDragStarted;
            _thumb.DragCompleted += OnThumbDragCompleted;
        }
        SetCurrentValue(VisualValueProperty, Value);
        UpdateMotion();
    }

    private void OnPreviewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _isPointerInteracting = true;
        Transitions = null;
        SetCurrentValue(VisualValueProperty, Value);
    }

    private void OnPreviewPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isPointerInteracting) return;
        _isPointerInteracting = false;
        SetCurrentValue(VisualValueProperty, Value);
        UpdateMotion();
    }

    private void OnThumbDragStarted(object? sender, VectorEventArgs e)
    {
        _isDragging = true;
        PseudoClasses.Set(":dragging", true);
        Transitions = null;
        SetCurrentValue(VisualValueProperty, Value);
    }

    private void OnThumbDragCompleted(object? sender, VectorEventArgs e)
    {
        _isDragging = false;
        PseudoClasses.Set(":dragging", false);
        SetCurrentValue(VisualValueProperty, Value);
        UpdateMotion();
    }

    private void SyncValueFromTrack()
    {
        if (_isPointerInteracting && Math.Abs(Value - VisualValue) > 0.0000001)
            SetCurrentValue(ValueProperty, VisualValue);
    }

    private void OnValueChanged()
    {
        if (_isDragging)
        {
            Transitions = null;
            SetCurrentValue(VisualValueProperty, Value);
        }
        else
        {
            SetCurrentValue(VisualValueProperty, Value);
        }
        UpdateVisualState();
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        Transitions = _isDragging || _isPointerInteracting
            ? null
            : MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, VisualValueProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast));
        foreach (var control in new[] { _valueIndicator, _stateLayer })
        {
            if (control is not null)
                control.Transitions = MdMotionTransitions.Collect(
                    MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
                    MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
        }
        if (scheme is MdMotionScheme.Reduced or MdMotionScheme.None)
            SetCurrentValue(VisualValueProperty, Value);
    }

    private void UpdateVisualState()
    {
        var value = Value.ToString(string.IsNullOrWhiteSpace(ValueFormat) ? "0" : ValueFormat,
            System.Globalization.CultureInfo.CurrentCulture);
        SetAndRaise(DisplayValueProperty, ref _displayValue, value);
        PseudoClasses.Set(":value-indicator", ShowValueIndicator);
        PseudoClasses.Set(":stop-indicator", ShowStopIndicator);
    }
}
