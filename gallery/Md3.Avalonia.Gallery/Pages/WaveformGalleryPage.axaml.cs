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
    private DemoTrack.Track _track;

    public WaveformGalleryPage()
    {
        InitializeComponent();
        Playback.Content = L("Play demo track", "播放示例音频");

        try
        {
            _track = DemoTrack.Load();
            var peaks = MdWaveform.PeaksFromSamples(_track.Samples, 320);
            Waveform.Peaks = peaks;
            Meter.Peaks = MdWaveform.PeaksFromSamples(_track.Samples, 96);
            Static.Peaks = peaks;
        }
        catch (Exception exception)
        {
            // A waveform with no peaks draws its baseline, which is the control's designed empty
            // state, so a missing or unreadable asset leaves a sensible page instead of an
            // exception. The readout carries the reason so it cannot be mistaken for silence.
            PositionReadout.Text = L($"Audio unavailable: {exception.Message}", $"音频不可用：{exception.Message}");
            Playback.IsEnabled = false;
            return;
        }

        UpdateReadout();
        _playback.Tick += (_, _) => Advance();
    }

    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    private void TogglePlayback(object? sender, RoutedEventArgs e)
    {
        if (_playback.IsEnabled) _playback.Stop(); else _playback.Start();
        Playback.Content = _playback.IsEnabled
            ? L("Pause", "暂停")
            : L("Play demo track", "播放示例音频");
    }

    private void Advance()
    {
        var duration = _track.Duration;
        if (duration <= TimeSpan.Zero)
        {
            _playback.Stop();
            return;
        }

        // The playhead advances by wall-clock time, not by a fixed fraction of the track, so the
        // sweep takes as long as the recording does.
        var next = Waveform.PlayheadPosition + _playback.Interval / duration;
        Waveform.PlayheadPosition = next >= 1 ? 0 : next;
        UpdateReadout();
    }

    private void UpdateReadout()
    {
        var total = _track.Duration;
        var at = TimeSpan.FromSeconds(total.TotalSeconds * Waveform.PlayheadPosition);
        PositionReadout.Text = L(
            $"Okay (excerpt) · {at:mm\\:ss} / {total:mm\\:ss}",
            $"《Okay》选段 · {at:mm\\:ss} / {total:mm\\:ss}");
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _playback.Stop();
        base.OnDetachedFromVisualTree(e);
    }
}
