using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Gallery.Pages;

public partial class MotionGalleryPage : UserControl
{
    private readonly DispatcherTimer _timer;
    private long _startedAt;
    private bool _atEnd;
    private double _from;
    private double _to;

    public MotionGalleryPage()
    {
        InitializeComponent();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += Animate;
    }

    private void PlayMotion(object? sender, RoutedEventArgs e)
    {
        _timer.Stop();
        _from = _atEnd ? 260 : 0;
        _to = _atEnd ? 0 : 260;
        _atEnd = !_atEnd;
        _startedAt = Stopwatch.GetTimestamp();
        _timer.Start();
    }

    private void Animate(object? sender, EventArgs e)
    {
        var elapsed = Stopwatch.GetElapsedTime(_startedAt).TotalSeconds * MotionSpeedSlider.Value;
        var expressive = Math.Clamp(MdMotionTokens.ExpressiveDefaultSpatial.Sample(elapsed), 0, 1.06);
        var standard = Math.Clamp(MdMotionTokens.StandardDefaultSpatial.Sample(elapsed), 0, 1.02);
        var reducedProgress = Math.Clamp(elapsed / 0.2, 0, 1);
        var reduced = 1 - Math.Pow(1 - reducedProgress, 3);
        SetPosition(ExpressiveDot, Interpolate(expressive));
        SetPosition(StandardDot, Interpolate(standard));
        SetPosition(ReducedDot, Interpolate(reduced));

        if (elapsed >= 0.9)
        {
            _timer.Stop();
            SetPosition(ExpressiveDot, _to);
            SetPosition(StandardDot, _to);
            SetPosition(ReducedDot, _to);
        }
    }

    private void MotionSpeedChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == RangeBase.ValueProperty && MotionSpeedText is not null && sender is MdSlider slider)
            MotionSpeedText.Text = $"{slider.Value:0.00}×";
    }

    private double Interpolate(double progress) => _from + (_to - _from) * progress;

    private static void SetPosition(Control control, double x) =>
        control.RenderTransform = new TranslateTransform(x, 0);
}
