using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Md3.Avalonia.Localization;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>A one-shot confirmation surface with a draggable thumb and accessible button alternative.</summary>
/// <remarks>Drag from logical start toward logical end and release beyond the threshold, or use
/// Enter, Space or the automation invoke action. This is not a security or authorization boundary.</remarks>
public sealed class MdSlideToConfirm : TemplatedControl
{
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<MdSlideToConfirm, string?>(nameof(Text));
    public static readonly StyledProperty<string?> ConfirmedTextProperty = AvaloniaProperty.Register<MdSlideToConfirm, string?>(nameof(ConfirmedText));
    public static readonly StyledProperty<bool> IsConfirmedProperty = AvaloniaProperty.Register<MdSlideToConfirm, bool>(
        nameof(IsConfirmed), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<double> ThresholdProperty = AvaloniaProperty.Register<MdSlideToConfirm, double>(
        nameof(Threshold), 0.8, coerce: static (_, value) => double.IsFinite(value) ? Math.Clamp(value, 0.1, 1) : 0.8);
    public static readonly StyledProperty<IBrush?> TrackBrushProperty = AvaloniaProperty.Register<MdSlideToConfirm, IBrush?>(nameof(TrackBrush));
    public static readonly StyledProperty<IBrush?> ProgressBrushProperty = AvaloniaProperty.Register<MdSlideToConfirm, IBrush?>(nameof(ProgressBrush));
    public static readonly StyledProperty<IBrush?> ThumbBrushProperty = AvaloniaProperty.Register<MdSlideToConfirm, IBrush?>(nameof(ThumbBrush));
    public static readonly StyledProperty<IBrush?> ThumbForegroundProperty = AvaloniaProperty.Register<MdSlideToConfirm, IBrush?>(nameof(ThumbForeground));
    public static readonly DirectProperty<MdSlideToConfirm, double> ProgressProperty =
        AvaloniaProperty.RegisterDirect<MdSlideToConfirm, double>(nameof(Progress), control => control.Progress);
    public static readonly DirectProperty<MdSlideToConfirm, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<MdSlideToConfirm, bool>(nameof(IsDragging), control => control.IsDragging);

    private readonly MdSpatialSpringRunner _spring;
    private IPointer? _pointer;
    private double _startX;
    private double _startProgress;
    private double _progress;
    private bool _dragging;

    static MdSlideToConfirm()
    {
        AffectsRender<MdSlideToConfirm>(TextProperty, ConfirmedTextProperty, IsConfirmedProperty, FlowDirectionProperty,
            TrackBrushProperty, ProgressBrushProperty, ThumbBrushProperty, ThumbForegroundProperty, ForegroundProperty,
            FontFamilyProperty, FontSizeProperty, FontStyleProperty, FontWeightProperty);
        IsConfirmedProperty.Changed.AddClassHandler<MdSlideToConfirm>((c, _) => c.SyncConfirmed());
        IsEffectivelyEnabledProperty.Changed.AddClassHandler<MdSlideToConfirm>((c, _) =>
        {
            if (!c.IsEffectivelyEnabled) { c.EndPointer(); c._spring.SnapTo(c.IsConfirmed ? 1 : 0); }
        });
        FlowDirectionProperty.Changed.AddClassHandler<MdSlideToConfirm>((c, _) =>
        {
            if (c.IsDragging) { c.EndPointer(); c._spring.SnapTo(c.IsConfirmed ? 1 : 0); }
        });
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdSlideToConfirm>((c, _) =>
        {
            if (c._spring.IsRunning && !MdMotion.Resolve(c, MdMotionKind.Spatial, MdMotionSpeed.Fast).IsEnabled)
                c._spring.SnapTo(c.IsConfirmed ? 1 : 0);
        });
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdSlideToConfirm>((c, _) => c.InvalidateVisual());
    }

    /// <summary>Creates a confirmation control ready for pointer, keyboard and automation input.</summary>
    public MdSlideToConfirm()
    {
        Focusable = true;
        ClipToBounds = true;
        _spring = new MdSpatialSpringRunner(this, SetProgress);
        GotFocus += (_, _) => InvalidateVisual();
        LostFocus += (_, _) => InvalidateVisual();
    }

    /// <summary>Unconfirmed label; null uses the localized default.</summary>
    public string? Text { get => GetValue(TextProperty); set => SetValue(TextProperty, value); }
    /// <summary>Confirmed label; null uses the localized default.</summary>
    public string? ConfirmedText { get => GetValue(ConfirmedTextProperty); set => SetValue(ConfirmedTextProperty, value); }
    /// <summary>Completed state. Programmatic changes synchronize the visual but do not raise Confirmed.</summary>
    public bool IsConfirmed { get => GetValue(IsConfirmedProperty); set => SetValue(IsConfirmedProperty, value); }
    /// <summary>Minimum fraction of thumb travel required on release, coerced to 0.1–1.</summary>
    public double Threshold { get => GetValue(ThresholdProperty); set => SetValue(ThresholdProperty, value); }
    /// <summary>Background track brush.</summary>
    public IBrush? TrackBrush { get => GetValue(TrackBrushProperty); set => SetValue(TrackBrushProperty, value); }
    /// <summary>Completed portion of the track.</summary>
    public IBrush? ProgressBrush { get => GetValue(ProgressBrushProperty); set => SetValue(ProgressBrushProperty, value); }
    /// <summary>Thumb fill brush.</summary>
    public IBrush? ThumbBrush { get => GetValue(ThumbBrushProperty); set => SetValue(ThumbBrushProperty, value); }
    /// <summary>Thumb arrow brush.</summary>
    public IBrush? ThumbForeground { get => GetValue(ThumbForegroundProperty); set => SetValue(ThumbForegroundProperty, value); }
    /// <summary>Current visual progress in the interval 0–1.</summary>
    public double Progress => _progress;
    /// <summary>Whether a pointer owns the active gesture.</summary>
    public bool IsDragging => _dragging;
    /// <summary>Raised once per confirmation, whether from pointer, keyboard, automation or Confirm().</summary>
    public event EventHandler? Confirmed;

    internal string DisplayLabel => IsConfirmed
        ? ConfirmedText ?? MdLocalization.GetString("SlideConfirmed", this)
        : Text ?? MdLocalization.GetString("SlideToConfirm", this);

    /// <summary>Confirms immediately if enabled and not already confirmed; returns whether it fired.</summary>
    public bool Confirm()
    {
        if (!IsEffectivelyEnabled || IsConfirmed) return false;
        EndPointer();
        SetCurrentValue(IsConfirmedProperty, true);
        if (!IsConfirmed) return false;
        Confirmed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>Resets the one-shot state without raising Confirmed.</summary>
    public void Reset() => SetCurrentValue(IsConfirmedProperty, false);

    private void SyncConfirmed()
    {
        EndPointer();
        _spring.SnapTo(IsConfirmed ? 1 : 0);
        InvalidateVisual();
    }

    private void SetProgress(double value)
    {
        if (SetAndRaise(ProgressProperty, ref _progress, Math.Clamp(value, 0, 1))) InvalidateVisual();
    }

    protected override Size MeasureOverride(Size availableSize) => new(280, 64);

    private (double Size, double Start, double Travel) Geometry()
    {
        var size = Math.Max(0, Math.Min(48, Math.Min(Bounds.Width - 16, Bounds.Height - 16)));
        var start = Math.Min(8 + size / 2, Bounds.Width / 2);
        return (size, start, Math.Max(0, Bounds.Width - 2 * start));
    }

    private double ThumbX()
    {
        var (_, start, travel) = Geometry();
        var x = start + travel * Progress;
        return FlowDirection == FlowDirection.RightToLeft ? Bounds.Width - x : x;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width <= 0 || height <= 0) return;
        context.DrawRectangle(TrackBrush ?? Brushes.LightGray, null, new RoundedRect(new Rect(Bounds.Size), height / 2));
        var x = ThumbX();
        var rtl = FlowDirection == FlowDirection.RightToLeft;
        if (Progress > 0)
        {
            var fill = rtl ? new Rect(x, 0, width - x, height) : new Rect(0, 0, x, height);
            using (context.PushClip(new RoundedRect(new Rect(Bounds.Size), height / 2)))
                context.DrawRectangle(ProgressBrush ?? Brushes.Gray, null, fill);
        }
        var label = new FormattedText(DisplayLabel, CultureInfo.CurrentCulture, FlowDirection,
            new Typeface(FontFamily, FontStyle, FontWeight), Math.Max(1, FontSize), Foreground ?? Brushes.Black);
        using (context.PushClip(new Rect(Bounds.Size)))
            context.DrawText(label, new Point((width - label.Width) / 2, (height - label.Height) / 2));

        var (thumbSize, _, _) = Geometry();
        if (thumbSize > 0)
        {
            var center = new Point(x, height / 2);
            context.DrawEllipse(ThumbBrush ?? Brushes.White, null, center, thumbSize / 2, thumbSize / 2);
            var direction = rtl ? -1d : 1d;
            var pen = new Pen(ThumbForeground ?? Brushes.Black, 2);
            context.DrawLine(pen, new Point(x - 7 * direction, center.Y), new Point(x + 7 * direction, center.Y));
            context.DrawLine(pen, new Point(x + 2 * direction, center.Y - 5), new Point(x + 7 * direction, center.Y));
            context.DrawLine(pen, new Point(x + 2 * direction, center.Y + 5), new Point(x + 7 * direction, center.Y));
        }
        if (IsKeyboardFocusWithin)
            context.DrawRectangle(null, new Pen(Foreground ?? Brushes.Black, 2),
                new Rect(1, 1, Math.Max(0, width - 2), Math.Max(0, height - 2)), height / 2);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var (size, _, travel) = Geometry();
        var point = e.GetPosition(this);
        if (!IsEffectivelyEnabled || IsConfirmed || _pointer is not null || size <= 0 || travel <= 0 ||
            Math.Abs(point.X - ThumbX()) > size / 2 || Math.Abs(point.Y - Bounds.Height / 2) > size / 2 ||
            (e.Pointer.Type == PointerType.Mouse && !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)) return;
        _spring.Stop();
        _pointer = e.Pointer;
        _startX = point.X;
        _startProgress = Progress;
        SetAndRaise(IsDraggingProperty, ref _dragging, true);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    private void UpdateDrag(double x)
    {
        var (_, _, travel) = Geometry();
        if (travel <= 0) return;
        var direction = FlowDirection == FlowDirection.RightToLeft ? -1 : 1;
        _spring.SnapTo(Math.Clamp(_startProgress + direction * (x - _startX) / travel, 0, 1));
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!ReferenceEquals(_pointer, e.Pointer) || !ReferenceEquals(e.Pointer.Captured, this)) return;
        UpdateDrag(e.GetPosition(this).X);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!ReferenceEquals(_pointer, e.Pointer)) return;
        UpdateDrag(e.GetPosition(this).X);
        var commit = Progress >= Threshold;
        EndPointer();
        if (commit) Confirm();
        else _spring.Retarget(0, MdMotionSpeed.Fast);
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (ReferenceEquals(_pointer, e.Pointer))
        {
            _pointer = null;
            SetAndRaise(IsDraggingProperty, ref _dragging, false);
            _spring.Retarget(IsConfirmed ? 1 : 0, MdMotionSpeed.Fast);
        }
        base.OnPointerCaptureLost(e);
    }

    private void EndPointer()
    {
        var pointer = _pointer;
        _pointer = null;
        SetAndRaise(IsDraggingProperty, ref _dragging, false);
        if (pointer is not null && ReferenceEquals(pointer.Captured, this)) pointer.Capture(null);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Space)
        {
            Confirm();
            e.Handled = true;
        }
        else base.OnKeyDown(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        EndPointer();
        _spring.SnapTo(IsConfirmed ? 1 : 0);
        base.OnDetachedFromVisualTree(e);
    }

    internal bool IsSettling => _spring.IsRunning;
    internal void AdvanceSettle(TimeSpan elapsed) => _spring.Advance(elapsed);
    protected override AutomationPeer OnCreateAutomationPeer() => new MdSlideToConfirmAutomationPeer(this);
}

internal sealed class MdSlideToConfirmAutomationPeer : ControlAutomationPeer, IInvokeProvider
{
    private readonly MdSlideToConfirm _control;
    private string? _name;
    public MdSlideToConfirmAutomationPeer(MdSlideToConfirm owner) : base(owner)
    {
        _control = owner;
        _name = GetNameCore();
        owner.PropertyChanged += (_, e) =>
        {
            if (e.Property == MdSlideToConfirm.TextProperty || e.Property == MdSlideToConfirm.ConfirmedTextProperty ||
                e.Property == MdSlideToConfirm.IsConfirmedProperty || e.Property == MdLocalization.CultureProperty)
            {
                var name = GetNameCore();
                if (name != _name) RaisePropertyChangedEvent(AutomationElementIdentifiers.NameProperty, _name, name);
                _name = name;
            }
        };
    }

    public void Invoke()
    {
        EnsureEnabled();
        if (!_control.Confirm()) throw new InvalidOperationException("The action has already been confirmed.");
    }

    protected override string? GetNameCore()
    {
        var name = base.GetNameCore();
        return string.IsNullOrWhiteSpace(name) ? _control.DisplayLabel : name;
    }
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Button;
    protected override bool IsControlElementCore() => true;
    protected override bool IsContentElementCore() => true;
}
