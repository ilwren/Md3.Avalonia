using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A determinate or indeterminate Material linear progress indicator with flat or wavy shape.
/// The indeterminate state runs the M3 disjoint two-segment sweep.</summary>
public sealed class MdLinearProgressIndicator : ProgressBar
{
    public static readonly StyledProperty<IBrush?> IndicatorBrushProperty =
        AvaloniaProperty.Register<MdLinearProgressIndicator, IBrush?>(nameof(IndicatorBrush));
    public static readonly StyledProperty<IBrush?> TrackBrushProperty =
        AvaloniaProperty.Register<MdLinearProgressIndicator, IBrush?>(nameof(TrackBrush));
    public static readonly StyledProperty<double> TrackThicknessProperty =
        AvaloniaProperty.Register<MdLinearProgressIndicator, double>(nameof(TrackThickness), 4);
    public static readonly StyledProperty<MdProgressShape> ShapeProperty =
        AvaloniaProperty.Register<MdLinearProgressIndicator, MdProgressShape>(nameof(Shape));
    public static readonly StyledProperty<double> WaveAmplitudeProperty =
        AvaloniaProperty.Register<MdLinearProgressIndicator, double>(nameof(WaveAmplitude), 3);
    public static readonly StyledProperty<double> WaveLengthProperty =
        AvaloniaProperty.Register<MdLinearProgressIndicator, double>(nameof(WaveLength), 24);

    private static TimeSpan FrameInterval => OperatingSystem.IsAndroid()
        ? TimeSpan.FromMilliseconds(33)
        : TimeSpan.FromMilliseconds(16);

    private readonly DispatcherTimer _timer;
    private readonly long _createdAt = Stopwatch.GetTimestamp();

    static MdLinearProgressIndicator()
    {
        AffectsRender<MdLinearProgressIndicator>(ValueProperty, MinimumProperty, MaximumProperty,
            IsIndeterminateProperty, IndicatorBrushProperty, TrackBrushProperty, TrackThicknessProperty,
            ShapeProperty, WaveAmplitudeProperty, WaveLengthProperty);
        IsIndeterminateProperty.Changed.AddClassHandler<MdLinearProgressIndicator>((indicator, _) => indicator.UpdateTimer());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdLinearProgressIndicator>((indicator, _) => indicator.UpdateTimer());
    }

    public MdLinearProgressIndicator()
    {
        // Render-priority ticks land with the frame; software-rasterized backends (Android
        // emulators) get a 30 fps cap - motion stays time-based, so the wave remains smooth.
        _timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = FrameInterval };
        _timer.Tick += (_, _) => InvalidateVisual();
        SetCurrentValue(MinHeightProperty, 12d);
    }

    public IBrush? IndicatorBrush { get => GetValue(IndicatorBrushProperty); set => SetValue(IndicatorBrushProperty, value); }
    public IBrush? TrackBrush { get => GetValue(TrackBrushProperty); set => SetValue(TrackBrushProperty, value); }
    public double TrackThickness { get => GetValue(TrackThicknessProperty); set => SetValue(TrackThicknessProperty, value); }
    public MdProgressShape Shape { get => GetValue(ShapeProperty); set => SetValue(ShapeProperty, value); }
    public double WaveAmplitude { get => GetValue(WaveAmplitudeProperty); set => SetValue(WaveAmplitudeProperty, value); }
    public double WaveLength { get => GetValue(WaveLengthProperty); set => SetValue(WaveLengthProperty, value); }

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
        if (Bounds.Width <= 0 || Bounds.Height <= 0 || IndicatorBrush is null || TrackBrush is null) return;

        var thickness = Math.Max(1, TrackThickness);
        var centerY = Bounds.Height / 2;
        if (IsIndeterminate)
        {
            DrawIndeterminateSegments(context, thickness, centerY);
            return;
        }

        var range = Math.Max(double.Epsilon, Maximum - Minimum);
        var fraction = Math.Clamp((Value - Minimum) / range, 0, 1);
        var end = Math.Clamp(Bounds.Width * fraction, 0, Bounds.Width);
        var gap = Math.Max(4, thickness * 1.5);
        var trackPen = new Pen(TrackBrush, thickness, lineCap: PenLineCap.Round);

        // Active and inactive geometries are mutually exclusive. This prevents the straight track
        // from showing through the troughs of a wavy active segment.
        DrawTrackSegment(context, trackPen, Math.Min(Bounds.Width, end + gap), Bounds.Width, centerY);

        if (end <= 0) return;
        if (Shape == MdProgressShape.Wavy) DrawWave(context, 0, end, centerY, thickness);
        else
        {
            var activePen = new Pen(IndicatorBrush, thickness, lineCap: PenLineCap.Round);
            context.DrawLine(activePen, new Point(0, centerY), new Point(end, centerY));
        }
    }

    // M3 disjoint indeterminate sweep (material-components-android LinearIndeterminateDisjointAnimatorDelegate):
    // a 1800 ms loop where two segments grow from their head, cross the track, and are overtaken
    // by their tail. Segment i's start edge uses entry 2i, its end edge entry 2i + 1.
    private const double IndeterminateLoopMs = 1800;
    private static readonly double[] IndeterminateDelaysMs = { 1267, 1000, 333, 0 };
    private static readonly double[] IndeterminateDurationsMs = { 533, 567, 850, 750 };
    // R.anim.linear_indeterminate_line{1,2}_{head,tail}_interpolator control points.
    private static readonly (double X1, double Y1, double X2, double Y2)[] IndeterminateEasings =
    {
        (0.2, 0.0, 0.8, 1.0),
        (0.4, 0.0, 1.0, 1.0),
        (0.0, 0.0, 0.65, 1.0),
        (0.1, 0.0, 0.45, 1.0),
    };

    private void DrawIndeterminateSegments(DrawingContext context, double thickness, double centerY)
    {
        var width = Bounds.Width;
        var playtime = Stopwatch.GetElapsedTime(_createdAt).TotalMilliseconds % IndeterminateLoopMs;
        var trackPen = new Pen(TrackBrush, thickness, lineCap: PenLineCap.Round);
        var activePen = new Pen(IndicatorBrush, thickness, lineCap: PenLineCap.Round);
        var gap = Math.Max(4, thickness * 1.5);

        var spans = new List<(double Start, double End)>(2);
        for (var i = 0; i < 2; i++)
        {
            var start = Math.Clamp(SampleIndeterminateEdge(playtime, 2 * i), 0, 1) * width;
            var end = Math.Clamp(SampleIndeterminateEdge(playtime, 2 * i + 1), 0, 1) * width;
            if (end - start > thickness) spans.Add((start, end));
        }

        spans.Sort(static (a, b) => a.Start.CompareTo(b.Start));
        var cursor = 0d;
        foreach (var span in spans)
        {
            DrawTrackSegment(context, trackPen, Math.Min(width, cursor + gap), Math.Max(0, span.Start - gap), centerY);
            cursor = span.End;
        }

        DrawTrackSegment(context, trackPen, Math.Min(width, cursor + gap), width, centerY);

        foreach (var span in spans)
        {
            if (Shape == MdProgressShape.Wavy) DrawWave(context, span.Start, span.End, centerY, thickness);
            else context.DrawLine(activePen, new Point(span.Start, centerY), new Point(span.End, centerY));
        }
    }

    internal static double SampleIndeterminateEdge(double playtime, int index)
    {
        var raw = (playtime - IndeterminateDelaysMs[index]) / IndeterminateDurationsMs[index];
        var (x1, y1, x2, y2) = IndeterminateEasings[index];
        return CubicBezier(x1, y1, x2, y2, Math.Clamp(raw, 0, 1));
    }

    /// <summary>Solves the cubic bezier easing curve Android's path interpolators describe.</summary>
    internal static double CubicBezier(double x1, double y1, double x2, double y2, double t)
    {
        if (t <= 0) return 0;
        if (t >= 1) return 1;
        var u = t;
        for (var i = 0; i < 8; i++)
        {
            var inv = 1 - u;
            var x = 3 * inv * inv * u * x1 + 3 * inv * u * u * x2 + u * u * u - t;
            if (Math.Abs(x) < 1e-6) break;
            var dx = 3 * inv * inv * x1 + 6 * inv * u * (x2 - x1) + 3 * u * u * (1 - x2);
            if (Math.Abs(dx) < 1e-9) break;
            u = Math.Clamp(u - x / dx, 0, 1);
        }

        var invU = 1 - u;
        return 3 * invU * invU * u * y1 + 3 * invU * u * u * y2 + u * u * u;
    }

    private static void DrawTrackSegment(DrawingContext context, Pen pen, double start, double end, double y)
    {
        if (end - start > pen.Thickness / 2)
            context.DrawLine(pen, new Point(start, y), new Point(end, y));
    }

    private void DrawWave(DrawingContext context, double start, double end, double centerY, double thickness)
    {
        var wavelength = Math.Max(8, WaveLength);
        var amplitude = Math.Max(0, WaveAmplitude);
        var waveNumber = Math.PI * 2 / wavelength;
        var geometry = new StreamGeometry();
        using (var stream = geometry.Open())
        {
            stream.BeginFigure(new Point(start, centerY), false);
            var x0 = start;
            while (x0 < end)
            {
                var x1 = Math.Min(end, x0 + wavelength / 4);
                var local0 = x0 - start;
                var local1 = x1 - start;
                var y0 = centerY + Math.Sin(local0 * waveNumber) * amplitude;
                var y1 = centerY + Math.Sin(local1 * waveNumber) * amplitude;
                var slope0 = Math.Cos(local0 * waveNumber) * amplitude * waveNumber;
                var slope1 = Math.Cos(local1 * waveNumber) * amplitude * waveNumber;
                var dx = x1 - x0;
                stream.CubicBezierTo(
                    new Point(x0 + dx / 3, y0 + slope0 * dx / 3),
                    new Point(x1 - dx / 3, y1 - slope1 * dx / 3),
                    new Point(x1, y1));
                x0 = x1;
            }
            stream.EndFigure(false);
        }
        var pen = new Pen(IndicatorBrush, thickness, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        context.DrawGeometry(null, pen, geometry);
    }

    private void UpdateTimer()
    {
        if (IsIndeterminate && this.IsAttachedToVisualTree() &&
            MdMotion.GetScheme(this) is MdMotionScheme.Expressive or MdMotionScheme.Standard)
            _timer.Start();
        else
        {
            _timer.Stop();
            InvalidateVisual();
        }
    }
}
