using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class FloatingWindowGalleryPage : UserControl
{
    // Where the window starts, and where "Reset position" puts it back.
    private const double HomeX = 140;
    private const double HomeY = 28;

    // Fast enough to look like playback, slow enough that the playhead is visibly a step at a time.
    private readonly DispatcherTimer _playback = new() { Interval = TimeSpan.FromMilliseconds(50) };
    private DemoTrack.Track _track;
    private TimeSpan _playbackPosition;

    public FloatingWindowGalleryPage()
    {
        InitializeComponent();
        Player.Title = L("Now playing", "正在播放");
        PlayerToggle.Content = L("Play demo track", "播放示例音频");
        Reopen.Content = L("Bring it back", "重新显示");
        ResetPosition.Content = L("Reset position", "重置位置");

        try
        {
            _track = DemoTrack.Load();
            PlayerWave.Peaks = MdWaveform.PeaksFromSamples(_track.Samples, 220);
        }
        catch (Exception exception)
        {
            // A waveform with no peaks draws its baseline, which is the control's designed empty
            // state, so a missing asset leaves a window you can still drag and minimize.
            PlayerReadout.Text = L($"Audio unavailable: {exception.Message}", $"音频不可用：{exception.Message}");
            PlayerToggle.IsEnabled = false;
            return;
        }

        UpdateReadout();
        _playback.Tick += (_, _) => Advance();
    }

    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    private void TogglePlayer(object? sender, RoutedEventArgs e)
    {
        if (_playback.IsEnabled) _playback.Stop(); else _playback.Start();
        // The bars pulse only while something is actually playing.
        PlayerWave.IsPulsing = _playback.IsEnabled;
        PlayerToggle.Content = _playback.IsEnabled
            ? L("Pause", "暂停")
            : L("Play demo track", "播放示例音频");
    }

    private void Advance()
    {
        var duration = _track.Duration;
        if (duration <= TimeSpan.Zero) return;
        var next = _playbackPosition + _playback.Interval;
        _playbackPosition = next >= duration ? TimeSpan.Zero : next;
        UpdateReadout();
    }

    private void UpdateReadout()
    {
        if (_track.Duration <= TimeSpan.Zero) return;
        PlayerReadout.Text = $"{_playbackPosition:mm\\:ss} / {_track.Duration:mm\\:ss}";
    }

    private void ReopenPlayer(object? sender, RoutedEventArgs e)
    {
        Player.IsOpen = true;
        Player.IsMinimized = false;
    }

    private void ResetPlayerPosition(object? sender, RoutedEventArgs e)
    {
        Player.X = HomeX;
        Player.Y = HomeY;
    }

    private void OnPlayerClosed(object? sender, RoutedEventArgs e)
    {
        // Playback is the window's reason for existing, so closing it stops the track rather than
        // leaving a timer running behind an invisible surface.
        _playback.Stop();
        PlayerWave.IsPulsing = false;
        PlayerToggle.Content = L("Play demo track", "播放示例音频");
    }
}
