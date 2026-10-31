using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
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
        // Animate normalized progress instead of a fixed 260-DIP offset. Each track converts that
        // progress to its own measured travel, which keeps the circles inside narrow Android views.
        _from = _atEnd ? 1 : 0;
        _to = _atEnd ? 0 : 1;
        _atEnd = !_atEnd;
        _startedAt = Stopwatch.GetTimestamp();
        _timer.Start();
    }

    private void OnMorphClicked(object? sender, RoutedEventArgs e)
    {
        if (MorphDemo is not null)
            MorphDemo.Toggle();
    }

    private void OnMorphClicked(object? sender, PointerPressedEventArgs e)
    {
        if (MorphDemo is not null)
            MorphDemo.Toggle();
    }

    private void OnTogglePanelClicked(object? sender, RoutedEventArgs e)
    {
        if (AnimatedPanel is not null)
            AnimatedPanel.IsContentVisible = !AnimatedPanel.IsContentVisible;
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

    private void MotionTracksSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        // Rotation and split-screen resizing can shrink a track after an animation completed.
        // Recompute the endpoint from the new arranged width instead of retaining a stale offset.
        if (_timer.IsEnabled) return;
        var progress = _atEnd ? 1d : 0d;
        SetPosition(ExpressiveDot, progress);
        SetPosition(StandardDot, progress);
        SetPosition(ReducedDot, progress);
    }

    private double Interpolate(double progress) => _from + (_to - _from) * progress;

    private static void SetPosition(Control control, double progress)
    {
        var trackWidth = (control.Parent as Control)?.Bounds.Width ?? 0;
        control.RenderTransform = new TranslateTransform(
            CalculateTrackTranslation(progress, trackWidth, control.Bounds.Width), 0);
    }

    /// <summary>Maps spring progress to a translation that cannot leave the measured track.</summary>
    internal static double CalculateTrackTranslation(double progress, double trackWidth, double dotWidth) =>
        Math.Clamp(progress, 0, 1) * Math.Max(0, trackWidth - dotWidth);
}
