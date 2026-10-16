using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class WaveformGalleryPage : UserControl
{
    // Fast enough to look like playback, slow enough that the playhead is visibly a step at a time
    // rather than a slide, which is what makes the two-way binding legible.
    private readonly DispatcherTimer _playback = new() { Interval = TimeSpan.FromMilliseconds(50) };

    public WaveformGalleryPage()
    {
        InitializeComponent();
        var peaks = SyntheticPeaks(240);
        Waveform.Peaks = peaks;
        Meter.Peaks = SyntheticPeaks(96);
        Static.Peaks = peaks;
        UpdateReadout();
        _playback.Tick += (_, _) => Advance();
    }

    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    /// <summary>
    /// A damped sweep. The gallery has no audio to decode and does not want a dependency in order to
    /// draw a waveform, so the samples are synthetic; reducing them goes through the same
    /// <see cref="MdWaveform.PeaksFromSamples"/> a real host would call.
    /// </summary>
    private static float[] SyntheticPeaks(int bars)
    {
        var samples = new float[bars * 64];
        for (var i = 0; i < samples.Length; i++)
        {
            var t = (double)i / samples.Length;
            var envelope = Math.Exp(-2.4 * t) * (0.55 + 0.45 * Math.Sin(t * Math.PI * 3));
            samples[i] = (float)(envelope * Math.Sin(t * Math.PI * 2 * 90));
        }
        return MdWaveform.PeaksFromSamples(samples, bars);
    }

    private void TogglePlayback(object? sender, RoutedEventArgs e)
    {
        if (_playback.IsEnabled) _playback.Stop(); else _playback.Start();
        Playback.Content = _playback.IsEnabled
            ? L("Pause playback", "暂停播放")
            : L("Simulate playback", "模拟播放");
    }

    private void Advance()
    {
        var next = Waveform.PlayheadPosition + .004;
        Waveform.PlayheadPosition = next > 1 ? 0 : next;
        UpdateReadout();
    }

    private void UpdateReadout()
    {
        var percent = Waveform.PlayheadPosition * 100;
        PositionReadout.Text = L($"Playhead at {percent:0}%", $"播放位置 {percent:0}%");
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _playback.Stop();
        base.OnDetachedFromVisualTree(e);
    }
}
