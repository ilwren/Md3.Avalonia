using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Md3.Avalonia.Extra.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// MdWaveform draws itself, so the theme, the peak reduction and the scrub are all worth pinning:
/// none of them can be seen from a property dump, and a waveform that quietly stopped splitting
/// played from unplayed would still report a healthy property set.
/// </summary>
public sealed class MdWaveformTests
{
    [AvaloniaFact]
    public void Theme_Fills_The_Brushes_From_Material_Roles()
    {
        var waveform = new MdWaveform { Width = 400, Height = 96 };
        var window = new Window { Width = 480, Height = 400, Content = waveform };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            // Primary over SurfaceContainerHighest is the split a Material 3 slider already makes
            // between its active and inactive track, so the waveform borrows it rather than
            // inventing a pair that a custom scheme would not recolour.
            Assert.Equal(TokenColor(waveform, "Md.Sys.Color.Primary.Brush"), BrushColor(waveform.PlayedBrush));
            Assert.Equal(TokenColor(waveform, "Md.Sys.Color.SurfaceContainerHighest.Brush"), BrushColor(waveform.UnplayedBrush));
            Assert.Equal(TokenColor(waveform, "Md.Sys.Color.Primary.Brush"), BrushColor(waveform.PlayheadBrush));
            Assert.Equal(TokenColor(waveform, "Md.Sys.Color.OutlineVariant.Brush"), BrushColor(waveform.BaselineBrush));

            // The radius comes from the shape scale, which is why the property is a CornerRadius
            // and not a double: a double could not take the token.
            Assert.True(waveform.TryFindResource("Md.Sys.Shape.Corner.ExtraSmall", waveform.ActualThemeVariant, out var token),
                "the shape token the theme binds BarCornerRadius to is missing");
            Assert.Equal(Assert.IsType<CornerRadius>(token), waveform.BarCornerRadius);
            Assert.True(waveform.Focusable, "a scrubbable waveform has to be reachable by keyboard");
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Peaks_From_Samples_Takes_The_Loudest_Sample_In_Each_Bar()
    {
        // Four bars over eight samples: each bar is two samples and keeps the larger magnitude.
        float[] samples = { .1f, .5f, -.9f, .2f, .3f, .3f, .05f, .05f };
        var peaks = MdWaveform.PeaksFromSamples(samples, 4);
        float[] expected = { .5f, .9f, .3f, .05f };

        Assert.Equal(expected.Length, peaks.Length);
        for (var i = 0; i < expected.Length; i++)
            Assert.True(Math.Abs(expected[i] - peaks[i]) < 1e-6, $"bar {i}: expected {expected[i]}, got {peaks[i]}");
    }

    [AvaloniaFact]
    public void Peaks_From_Samples_Clamps_Hot_Input_And_Survives_A_Short_Buffer()
    {
        // A recording above full scale must not draw outside the control.
        float[] hot = { 2f, -3f };
        var clamped = MdWaveform.PeaksFromSamples(hot, 2);
        Assert.True(clamped[0] == 1f && clamped[1] == 1f, $"hot input was not clamped: {clamped[0]}, {clamped[1]}");

        // More bars than samples: every bar still takes a sample rather than drawing nothing.
        float[] two = { .4f, .8f };
        var sparse = MdWaveform.PeaksFromSamples(two, 5);
        Assert.Equal(5, sparse.Length);
        var drewTheLouderSample = false;
        foreach (var value in sparse) drewTheLouderSample |= Math.Abs(value - .8f) < 1e-6;
        Assert.True(drewTheLouderSample, "a short buffer drew nothing recognisable");

        var empty = MdWaveform.PeaksFromSamples(ReadOnlySpan<float>.Empty, 3);
        Assert.Equal(3, empty.Length);
        Assert.True(empty[0] == 0f && empty[1] == 0f && empty[2] == 0f, "an empty buffer should be silent");

        float[] one = { .5f };
        Assert.Throws<ArgumentOutOfRangeException>(() => MdWaveform.PeaksFromSamples(one, 0));
    }

    [AvaloniaFact]
    public void Dragging_Seeks_And_The_Position_Comes_Back()
    {
        var waveform = new MdWaveform { Width = 400, Height = 96, Peaks = new float[] { 1, 1, 1, 1 } };
        var window = new Window { Width = 400, Height = 200, Content = waveform };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.True(waveform.Bounds.Width > 100, "the waveform never got a size");

            var width = waveform.Bounds.Width;
            Point At(double fraction) => waveform.TranslatePoint(
                new Point(width * fraction, waveform.Bounds.Height / 2), window)!.Value;

            window.MouseDown(At(.25), MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(.25, waveform.PlayheadPosition, 3);

            window.MouseMove(At(.7), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(.7, waveform.PlayheadPosition, 3);

            // Past either edge the playhead stops at the end instead of running away.
            window.MouseMove(At(1.4), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, waveform.PlayheadPosition, 3);

            window.MouseUp(At(1.4), MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Keyboard_Moves_The_Playhead_But_A_Non_Interactive_Waveform_Ignores_It()
    {
        var waveform = new MdWaveform { Width = 400, Height = 96, KeyboardStep = .1 };
        var window = new Window { Width = 400, Height = 200, Content = waveform };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            Key(waveform, Key.Right);
            Key(waveform, Key.Right);
            Assert.Equal(.2, waveform.PlayheadPosition, 3);

            Key(waveform, Key.End);
            Assert.Equal(1, waveform.PlayheadPosition, 3);
            Key(waveform, Key.Right);
            Assert.Equal(1, waveform.PlayheadPosition, 3);

            Key(waveform, Key.Home);
            Assert.Equal(0, waveform.PlayheadPosition, 3);

            waveform.IsInteractive = false;
            Key(waveform, Key.Right);
            Assert.Equal(0, waveform.PlayheadPosition, 3);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Rendered_Frame_Splits_Played_From_Unplayed_And_Leaves_Silence_Empty()
    {
        // Half-height bars, no gaps, no rounding, no playhead: every column is solid, the bottom
        // half is bar and the top half is not, and the only thing that can differ left from right
        // is the played/unplayed split.
        var peaks = new float[40];
        Array.Fill(peaks, .5f);

        var waveform = new MdWaveform
        {
            Width = 400,
            Height = 200,
            Peaks = peaks,
            PlayheadPosition = .5,
            ShowPlayhead = false,
            IsMirrored = false,
            BarGap = 0,
            BarCornerRadius = new CornerRadius(0),
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top,
        };
        var window = new Window { Width = 400, Height = 200, Content = waveform };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var pixels = ReadPixels(window, out var size);

            uint At(double xFraction, double yFraction) =>
                pixels[(int)(size.Height * yFraction) * size.Width + (int)(size.Width * xFraction)];

            var played = At(.25, .85);
            var unplayed = At(.75, .85);
            var silence = At(.25, .1);

            Assert.NotEqual(played, unplayed);
            Assert.NotEqual(played, silence);
            Assert.NotEqual(unplayed, silence);
            Assert.Equal(unplayed, At(.9, .85));
            Assert.Equal(played, At(.1, .85));
        }
        finally { window.Close(); }
    }

    private static void Key(Control target, Key key)
    {
        target.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = key,
            Source = target,
        });
        Dispatcher.UIThread.RunJobs();
    }

    private static Color TokenColor(Control scope, string key)
    {
        Assert.True(scope.TryFindResource(key, scope.ActualThemeVariant, out var value), key);
        return BrushColor(value);
    }

    private static Color BrushColor(object? value) => Assert.IsType<SolidColorBrush>(value).Color;

    private static uint[] ReadPixels(Window window, out PixelSize size)
    {
        var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        size = frame!.PixelSize;
        var pixels = new uint[size.Width * size.Height];
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            frame.CopyPixels(new PixelRect(0, 0, size.Width, size.Height),
                handle.AddrOfPinnedObject(), pixels.Length * 4, size.Width * 4);
        }
        finally { handle.Free(); }
        return pixels;
    }
}
