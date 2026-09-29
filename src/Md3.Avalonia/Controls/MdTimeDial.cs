using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>Identifies which value an <see cref="MdTimeDial"/> edits.</summary>
public enum MdTimeDialPart
{
    Hour,
    Minute
}

/// <summary>
/// Interactive Material clock face used by <see cref="MdTimePicker"/>. The minute hand has
/// one-minute precision even though the face labels every five minutes. In 24-hour mode the hour
/// face uses outer 1–12 and inner 13–00 rings.
/// </summary>
[PseudoClasses(":dragging", ":reduced-motion", ":no-motion")]
public sealed class MdTimeDial : Control
{
    public static readonly StyledProperty<int> HourProperty =
        AvaloniaProperty.Register<MdTimeDial, int>(nameof(Hour), 12,
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<int> MinuteProperty =
        AvaloniaProperty.Register<MdTimeDial, int>(nameof(Minute), 0,
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<MdTimeDialPart> ActivePartProperty =
        AvaloniaProperty.Register<MdTimeDial, MdTimeDialPart>(nameof(ActivePart));
    public static readonly StyledProperty<bool> Is24HourProperty =
        AvaloniaProperty.Register<MdTimeDial, bool>(nameof(Is24Hour));
    public static readonly StyledProperty<IBrush?> DialBackgroundProperty =
        AvaloniaProperty.Register<MdTimeDial, IBrush?>(nameof(DialBackground));
    public static readonly StyledProperty<IBrush?> HandBrushProperty =
        AvaloniaProperty.Register<MdTimeDial, IBrush?>(nameof(HandBrush));
    public static readonly StyledProperty<IBrush?> LabelBrushProperty =
        AvaloniaProperty.Register<MdTimeDial, IBrush?>(nameof(LabelBrush));
    public static readonly StyledProperty<IBrush?> SelectedLabelBrushProperty =
        AvaloniaProperty.Register<MdTimeDial, IBrush?>(nameof(SelectedLabelBrush));
    public static readonly StyledProperty<double> VisualHandPositionProperty =
        AvaloniaProperty.Register<MdTimeDial, double>(nameof(VisualHandPosition));
    public static readonly StyledProperty<double> ContentOpacityProperty =
        AvaloniaProperty.Register<MdTimeDial, double>(nameof(ContentOpacity), 1);

    private bool _dragging;
    private int _partChangeVersion;

    static MdTimeDial()
    {
        AffectsRender<MdTimeDial>(HourProperty, MinuteProperty, ActivePartProperty, Is24HourProperty,
            DialBackgroundProperty, HandBrushProperty, LabelBrushProperty, SelectedLabelBrushProperty,
            VisualHandPositionProperty, ContentOpacityProperty);
        HourProperty.Changed.AddClassHandler<MdTimeDial>((dial, _) => dial.UpdateVisualHand());
        MinuteProperty.Changed.AddClassHandler<MdTimeDial>((dial, _) => dial.UpdateVisualHand());
        ActivePartProperty.Changed.AddClassHandler<MdTimeDial>((dial, _) => dial.OnActivePartChanged());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdTimeDial>((dial, _) => dial.UpdateMotion());
    }

    public MdTimeDial()
    {
        Focusable = true;
        Cursor = new Cursor(StandardCursorType.Hand);
        SetCurrentValue(VisualHandPositionProperty, GetLogicalHandPosition());
        UpdateMotion();
    }

    public int Hour { get => GetValue(HourProperty); set => SetValue(HourProperty, value); }
    public int Minute { get => GetValue(MinuteProperty); set => SetValue(MinuteProperty, value); }
    public MdTimeDialPart ActivePart { get => GetValue(ActivePartProperty); set => SetValue(ActivePartProperty, value); }
    public bool Is24Hour { get => GetValue(Is24HourProperty); set => SetValue(Is24HourProperty, value); }
    public IBrush? DialBackground { get => GetValue(DialBackgroundProperty); set => SetValue(DialBackgroundProperty, value); }
    public IBrush? HandBrush { get => GetValue(HandBrushProperty); set => SetValue(HandBrushProperty, value); }
    public IBrush? LabelBrush { get => GetValue(LabelBrushProperty); set => SetValue(LabelBrushProperty, value); }
    public IBrush? SelectedLabelBrush { get => GetValue(SelectedLabelBrushProperty); set => SetValue(SelectedLabelBrushProperty, value); }
    public double VisualHandPosition => GetValue(VisualHandPositionProperty);
    public double ContentOpacity => GetValue(ContentOpacityProperty);

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var size = Math.Min(Bounds.Width, Bounds.Height);
        if (size <= 0) return;

        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var radius = Math.Max(0, size / 2 - 2);
        var outerLabelRadius = Math.Max(18, radius - 27);
        var innerLabelRadius = Math.Max(12, radius - 64);
        var handBrush = HandBrush ?? Brushes.Blue;
        var labelBrush = LabelBrush ?? Brushes.Black;
        var selectedLabelBrush = SelectedLabelBrush ?? Brushes.White;

        context.DrawEllipse(DialBackground ?? Brushes.LightGray, null, center, radius, radius);
        using var contentOpacity = context.PushOpacity(ContentOpacity);

        var period = ActivePart == MdTimeDialPart.Minute ? 60d : 12d;
        var visualPosition = ((VisualHandPosition % period) + period) % period;
        var slot = ActivePart == MdTimeDialPart.Minute
            ? (int)Math.Round(visualPosition / 5) % 12
            : (int)Math.Round(visualPosition) % 12;
        var handAngle = visualPosition / period * Math.Tau;
        var useInnerRing = ActivePart == MdTimeDialPart.Hour && Is24Hour && NormalizeHour(Hour) is 0 or >= 13;
        var handRadius = useInnerRing ? innerLabelRadius : outerLabelRadius;
        var selectedCenter = PointOnCircle(center, handRadius, handAngle);

        context.DrawLine(new Pen(handBrush, 2), center, selectedCenter);
        context.DrawEllipse(handBrush, null, center, 4, 4);
        context.DrawEllipse(handBrush, null, selectedCenter, 21, 21);

        for (var index = 0; index < 12; index++)
        {
            var angle = index / 12d * Math.Tau;
            var outerText = ActivePart == MdTimeDialPart.Minute
                ? (index * 5).ToString("00", CultureInfo.CurrentCulture)
                : (index == 0 ? 12 : index).ToString(CultureInfo.CurrentCulture);
            DrawLabel(context, outerText, PointOnCircle(center, outerLabelRadius, angle),
                !useInnerRing && index == slot ? selectedLabelBrush : labelBrush,
                14);

            if (ActivePart == MdTimeDialPart.Hour && Is24Hour)
            {
                var innerHour = index == 0 ? 0 : index + 12;
                DrawLabel(context, innerHour.ToString("00", CultureInfo.CurrentCulture),
                    PointOnCircle(center, innerLabelRadius, angle),
                    useInnerRing && index == slot ? selectedLabelBrush : labelBrush,
                    Math.Max(10, (14) - 2));
            }
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
        _dragging = true;
        PseudoClasses.Set(":dragging", true);
        Transitions = null;
        UpdateFromPoint(e.GetPosition(this));
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (ReferenceEquals(e.Pointer.Captured, this) && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            UpdateFromPoint(e.GetPosition(this));
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (ReferenceEquals(e.Pointer.Captured, this))
        {
            UpdateFromPoint(e.GetPosition(this));
            e.Pointer.Capture(null);
            _dragging = false;
            PseudoClasses.Set(":dragging", false);
            UpdateMotion();
            e.Handled = true;
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (!_dragging) return;
        _dragging = false;
        PseudoClasses.Set(":dragging", false);
        UpdateMotion();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var delta = e.Key switch { Key.Right or Key.Up => 1, Key.Left or Key.Down => -1, _ => 0 };
        if (delta != 0)
        {
            if (ActivePart == MdTimeDialPart.Hour)
                SetCurrentValue(HourProperty, (NormalizeHour(Hour) + delta + 24) % 24);
            else
                SetCurrentValue(MinuteProperty, ((Minute + delta) % 60 + 60) % 60);
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void OnActivePartChanged()
    {
        var scheme = MdMotion.GetScheme(this);
        if (scheme != MdMotionScheme.None)
        {
            SetCurrentValue(ContentOpacityProperty, 0);
            var version = ++_partChangeVersion;
            Dispatcher.UIThread.Post(() =>
            {
                if (version == _partChangeVersion) SetCurrentValue(ContentOpacityProperty, 1);
            }, DispatcherPriority.Render);
        }
        UpdateVisualHand();
    }

    private void UpdateVisualHand()
    {
        var period = ActivePart == MdTimeDialPart.Minute ? 60d : 12d;
        var desired = GetLogicalHandPosition();
        if (_dragging || MdMotion.GetScheme(this) is MdMotionScheme.Reduced or MdMotionScheme.None)
        {
            SetCurrentValue(VisualHandPositionProperty, desired);
            return;
        }
        var current = VisualHandPosition;
        var delta = ((desired - current + period / 2) % period + period) % period - period / 2;
        SetCurrentValue(VisualHandPositionProperty, current + delta);
    }

    private double GetLogicalHandPosition() => ActivePart == MdTimeDialPart.Minute
        ? ((Minute % 60) + 60) % 60
        : ((Hour % 12) + 12) % 12;

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        Transitions = _dragging
            ? null
            : MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, VisualHandPositionProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateDouble(this, ContentOpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        if (scheme is MdMotionScheme.Reduced or MdMotionScheme.None)
            SetCurrentValue(VisualHandPositionProperty, GetLogicalHandPosition());
    }

    private void UpdateFromPoint(Point point)
    {
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var dx = point.X - center.X;
        var dy = point.Y - center.Y;
        var angle = Math.Atan2(dx, -dy);
        if (angle < 0) angle += Math.Tau;

        if (ActivePart == MdTimeDialPart.Minute)
        {
            SetCurrentValue(MinuteProperty, (int)Math.Round(angle / Math.Tau * 60) % 60);
            return;
        }

        var slot = (int)Math.Round(angle / Math.Tau * 12) % 12;
        if (Is24Hour)
        {
            var radius = Math.Sqrt(dx * dx + dy * dy);
            var threshold = Math.Min(Bounds.Width, Bounds.Height) * 0.34;
            var hour = radius < threshold ? (slot == 0 ? 0 : slot + 12) : (slot == 0 ? 12 : slot);
            SetCurrentValue(HourProperty, hour);
        }
        else
        {
            var periodOffset = NormalizeHour(Hour) >= 12 ? 12 : 0;
            SetCurrentValue(HourProperty, (slot % 12) + periodOffset);
        }
    }

    private void DrawLabel(DrawingContext context, string text, Point center, IBrush brush, double size)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Medium),
            size,
            brush);
        context.DrawText(formatted, new Point(center.X - formatted.Width / 2, center.Y - formatted.Height / 2));
    }

    private static Point PointOnCircle(Point center, double radius, double angle) =>
        new(center.X + Math.Sin(angle) * radius, center.Y - Math.Cos(angle) * radius);

    private static int NormalizeHour(int hour) => ((hour % 24) + 24) % 24;
}
