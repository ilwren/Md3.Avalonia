using System.Diagnostics;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Material 3 Expressive loading indicator. The active mark morphs through the official seven-shape
/// sequence while a constant and a spring-driven rotation are composed around it.
/// </summary>
[PseudoClasses(":contained", ":uncontained")]
public sealed class MdLoadingIndicator : ProgressBar
{
    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<MdLoadingIndicator, double>(nameof(Size), 48);
    public static readonly StyledProperty<IBrush?> IndicatorBrushProperty =
        AvaloniaProperty.Register<MdLoadingIndicator, IBrush?>(nameof(IndicatorBrush));
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<MdLoadingIndicator, bool>(nameof(IsActive), true);
    public static readonly StyledProperty<bool> IsContainedProperty =
        AvaloniaProperty.Register<MdLoadingIndicator, bool>(nameof(IsContained));
    public static readonly StyledProperty<IBrush?> ContainerBrushProperty =
        AvaloniaProperty.Register<MdLoadingIndicator, IBrush?>(nameof(ContainerBrush));

    private const double SecondsPerShape = 0.650;
    private readonly DispatcherTimer _timer;
    private readonly long _createdAt = Stopwatch.GetTimestamp();

    static MdLoadingIndicator()
    {
        SizeProperty.Changed.AddClassHandler<MdLoadingIndicator>((indicator, _) => indicator.ApplySize());
        IsActiveProperty.Changed.AddClassHandler<MdLoadingIndicator>((indicator, _) => indicator.UpdateTimer());
        IsContainedProperty.Changed.AddClassHandler<MdLoadingIndicator>((indicator, _) => indicator.UpdateContainedState());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdLoadingIndicator>((indicator, _) => indicator.UpdateTimer());
        AffectsRender<MdLoadingIndicator>(IndicatorBrushProperty, ContainerBrushProperty, IsContainedProperty);
    }

    public MdLoadingIndicator()
    {
        IsIndeterminate = true;
        IsHitTestVisible = false;
        AutomationProperties.SetName(this, "Loading");
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += (_, _) => InvalidateVisual();
        ApplySize();
        UpdateContainedState();
    }

    public double Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public IBrush? IndicatorBrush { get => GetValue(IndicatorBrushProperty); set => SetValue(IndicatorBrushProperty, value); }
    public bool IsActive { get => GetValue(IsActiveProperty); set => SetValue(IsActiveProperty, value); }
    public bool IsContained { get => GetValue(IsContainedProperty); set => SetValue(IsContainedProperty, value); }
    public IBrush? ContainerBrush { get => GetValue(ContainerBrushProperty); set => SetValue(ContainerBrushProperty, value); }

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
        if (IndicatorBrush is null || Bounds.Width <= 0 || Bounds.Height <= 0) return;

        var shortest = Math.Min(Bounds.Width, Bounds.Height);
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        if (IsContained && ContainerBrush is not null)
        {
            context.DrawRectangle(ContainerBrush, null,
                new RoundedRect(new Rect(Bounds.Size), shortest / 2));
        }

        var scheme = MdMotion.GetScheme(this);
        var allowsAmbientMotion = scheme is MdMotionScheme.Expressive or MdMotionScheme.Standard;
        var seconds = IsActive && allowsAmbientMotion
            ? Stopwatch.GetElapsedTime(_createdAt).TotalSeconds
            : 0;

        var timeline = seconds / SecondsPerShape;
        var shapeIndex = (int)Math.Floor(timeline);
        var linearFraction = timeline - Math.Floor(timeline);
        var morphFraction = SpringFraction(linearFraction);
        var from = PositiveModulo(shapeIndex, MdMaterialLoadingShapes.All.Length);
        var to = (from + 1) % MdMaterialLoadingShapes.All.Length;

        // LoadingIndicatorAnimatorDelegate combines 50° constant and 90° morph rotation per cycle.
        var rotationDegrees = shapeIndex * 140 + linearFraction * 50 + morphFraction * 90;
        var rotation = rotationDegrees * Math.PI / 180;
        var pulse = scheme is MdMotionScheme.None or MdMotionScheme.Reduced
            ? 1
            : 1 + Math.Sin(timeline * Math.PI * 2) * 0.025;
        var radius = shortest * 0.395 * pulse; // 38dp active geometry in the 48dp container.

        var source = MdMaterialLoadingShapes.All[from];
        var destination = MdMaterialLoadingShapes.All[to];
        var geometry = new StreamGeometry();
        using (var stream = geometry.Open())
        {
            for (var index = 0; index < source.Length; index++)
            {
                var x = source[index].X + (destination[index].X - source[index].X) * morphFraction;
                var y = source[index].Y + (destination[index].Y - source[index].Y) * morphFraction;
                var rotatedX = x * Math.Cos(rotation) - y * Math.Sin(rotation);
                var rotatedY = x * Math.Sin(rotation) + y * Math.Cos(rotation);
                var point = new Point(center.X + rotatedX * radius, center.Y + rotatedY * radius);
                if (index == 0) stream.BeginFigure(point, true);
                else stream.LineTo(point);
            }
            stream.EndFigure(true);
        }
        context.DrawGeometry(IndicatorBrush, null, geometry);
    }

    private static double SpringFraction(double fraction)
    {
        // Underdamped k=200, ζ≈0.6 response, clamped for stable polygon correspondence.
        var value = 1 - Math.Exp(-6.2 * fraction) *
            (Math.Cos(9.8 * fraction) + 0.63 * Math.Sin(9.8 * fraction));
        return Math.Clamp(value, 0, 1);
    }

    private static int PositiveModulo(int value, int divisor) => (value % divisor + divisor) % divisor;

    private void ApplySize()
    {
        SetCurrentValue(WidthProperty, Size);
        SetCurrentValue(HeightProperty, Size);
        InvalidateMeasure();
    }

    private void UpdateContainedState()
    {
        PseudoClasses.Set(":contained", IsContained);
        PseudoClasses.Set(":uncontained", !IsContained);
        InvalidateVisual();
    }

    private void UpdateTimer()
    {
        if (IsActive && this.IsAttachedToVisualTree() &&
            MdMotion.GetScheme(this) is MdMotionScheme.Expressive or MdMotionScheme.Standard)
            _timer.Start();
        else
        {
            _timer.Stop();
            InvalidateVisual();
        }
    }
}
