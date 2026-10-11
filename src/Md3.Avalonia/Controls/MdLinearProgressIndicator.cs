using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A determinate or indeterminate Material linear progress indicator with flat or wavy shape.</summary>
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
        var range = Math.Max(double.Epsilon, Maximum - Minimum);
        var fraction = Math.Clamp((Value - Minimum) / range, 0, 1);
        var start = 0d;
        if (IsIndeterminate)
        {
            if (MdMotion.GetScheme(this) is MdMotionScheme.Expressive or MdMotionScheme.Standard)
            {
                var phase = Stopwatch.GetElapsedTime(_createdAt).TotalSeconds * 0.85 % 1;
                start = Math.Max(0, phase * 1.35 - 0.35) * Bounds.Width;
            }
            fraction = Math.Min(0.35, 1 - start / Bounds.Width);
        }

        var end = Math.Clamp(start + Bounds.Width * fraction, 0, Bounds.Width);
        var gap = Math.Max(4, thickness * 1.5);
        var trackPen = new Pen(TrackBrush, thickness, lineCap: PenLineCap.Round);

        // Active and inactive geometries are mutually exclusive. This prevents the straight track
        // from showing through the troughs of a wavy active segment.
        DrawTrackSegment(context, trackPen, 0, Math.Max(0, start - gap), centerY);
        DrawTrackSegment(context, trackPen, Math.Min(Bounds.Width, end + gap), Bounds.Width, centerY);

        if (end <= start) return;
        if (Shape == MdProgressShape.Wavy) DrawWave(context, start, end, centerY, thickness);
        else
        {
            var activePen = new Pen(IndicatorBrush, thickness, lineCap: PenLineCap.Round);
            context.DrawLine(activePen, new Point(start, centerY), new Point(end, centerY));
        }
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
