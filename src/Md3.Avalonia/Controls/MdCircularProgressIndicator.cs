using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A determinate or indeterminate Material circular progress indicator.</summary>
public sealed class MdCircularProgressIndicator : ProgressBar
{
    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<MdCircularProgressIndicator, double>(nameof(Size), 48);
    public static readonly StyledProperty<IBrush?> IndicatorBrushProperty =
        AvaloniaProperty.Register<MdCircularProgressIndicator, IBrush?>(nameof(IndicatorBrush));
    public static readonly StyledProperty<IBrush?> TrackBrushProperty =
        AvaloniaProperty.Register<MdCircularProgressIndicator, IBrush?>(nameof(TrackBrush));
    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<MdCircularProgressIndicator, double>(nameof(StrokeThickness), 4);

    private readonly DispatcherTimer _timer;
    private readonly long _createdAt = Stopwatch.GetTimestamp();

    static MdCircularProgressIndicator()
    {
        AffectsRender<MdCircularProgressIndicator>(ValueProperty, MinimumProperty, MaximumProperty,
            IsIndeterminateProperty, SizeProperty, IndicatorBrushProperty, TrackBrushProperty,
            StrokeThicknessProperty);
        SizeProperty.Changed.AddClassHandler<MdCircularProgressIndicator>((indicator, _) => indicator.ApplySize());
        IsIndeterminateProperty.Changed.AddClassHandler<MdCircularProgressIndicator>((indicator, _) => indicator.UpdateTimer());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdCircularProgressIndicator>((indicator, _) => indicator.UpdateTimer());
    }

    public MdCircularProgressIndicator()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += (_, _) => InvalidateVisual();
        ApplySize();
    }

    public double Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public IBrush? IndicatorBrush { get => GetValue(IndicatorBrushProperty); set => SetValue(IndicatorBrushProperty, value); }
    public IBrush? TrackBrush { get => GetValue(TrackBrushProperty); set => SetValue(TrackBrushProperty, value); }
    public double StrokeThickness { get => GetValue(StrokeThicknessProperty); set => SetValue(StrokeThicknessProperty, value); }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateTimer();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _timer.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (Bounds.Width <= 0 || Bounds.Height <= 0 || IndicatorBrush is null || TrackBrush is null)
        {
            return;
        }

        var thickness = Math.Max(1, StrokeThickness);
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var radius = Math.Max(0, Math.Min(Bounds.Width, Bounds.Height) / 2 - thickness / 2);
        context.DrawEllipse(null, new Pen(TrackBrush, thickness), center, radius, radius);

        var range = Math.Max(double.Epsilon, Maximum - Minimum);
        var fraction = Math.Clamp((Value - Minimum) / range, 0, 1);
        var startAngle = -Math.PI / 2;
        if (IsIndeterminate)
        {
            if (MdMotion.GetScheme(this) is MdMotionScheme.Expressive or MdMotionScheme.Standard)
                startAngle += Stopwatch.GetElapsedTime(_createdAt).TotalSeconds * Math.PI * 1.6;
            fraction = 0.72;
        }
        DrawArc(context, center, radius, startAngle, fraction * Math.PI * 2, thickness);
    }

    private void DrawArc(DrawingContext context, Point center, double radius, double start, double sweep, double thickness)
    {
        if (sweep <= 0)
        {
            return;
        }
        var segments = Math.Max(2, (int)Math.Ceiling(72 * sweep / (Math.PI * 2)));
        var pen = new Pen(IndicatorBrush, thickness);
        var previous = PointOnCircle(center, radius, start);
        for (var index = 1; index <= segments; index++)
        {
            var point = PointOnCircle(center, radius, start + sweep * index / segments);
            context.DrawLine(pen, previous, point);
            previous = point;
        }
        var capRadius = thickness / 2;
        context.DrawEllipse(IndicatorBrush, null, PointOnCircle(center, radius, start), capRadius, capRadius);
        context.DrawEllipse(IndicatorBrush, null, PointOnCircle(center, radius, start + sweep), capRadius, capRadius);
    }

    private static Point PointOnCircle(Point center, double radius, double angle) =>
        new(center.X + Math.Cos(angle) * radius, center.Y + Math.Sin(angle) * radius);

    private void ApplySize()
    {
        SetCurrentValue(WidthProperty, Size);
        SetCurrentValue(HeightProperty, Size);
        InvalidateMeasure();
    }

    private void UpdateTimer()
    {
        if (IsIndeterminate && this.IsAttachedToVisualTree() &&
            MdMotion.GetScheme(this) is MdMotionScheme.Expressive or MdMotionScheme.Standard)
        {
            _timer.Start();
        }
        else
        {
            _timer.Stop();
            InvalidateVisual();
        }
    }
}
