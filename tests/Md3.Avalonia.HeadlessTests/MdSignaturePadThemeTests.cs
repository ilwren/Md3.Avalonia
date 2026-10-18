using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DotNetCampus.Inking;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Themes;
using SkiaSharp;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Pins what <see cref="MaterialInkCanvasTheme"/> does to a bare canvas and what
/// <see cref="MdSignaturePad"/> adds around it: the ink comes from the tokens instead of upstream's
/// red, the settings upstream owns are left alone, and the chrome - placeholder, clear, erasing -
/// follows the state it claims to follow.
/// </summary>
/// <remarks>
/// Strokes are produced by real pointer input rather than by constructing one. Upstream builds a
/// static stroke from an <c>InkId</c>, a <c>StylusPointListSpan</c> and an
/// <c>ISkiaInkStrokeRenderer</c>, none of which are a host's business; driving the canvas the way a
/// user does also proves the pad forwards the events the chrome depends on.
/// </remarks>
public sealed class MdSignaturePadThemeTests
{
    private const double PadWidth = 320;
    private const double PadHeight = 200;

    [AvaloniaFact]
    public void Apply_Gives_A_Bare_Canvas_On_Surface_Ink_Instead_Of_Upstream_Red()
    {
        var canvas = new InkCanvas();
        using var scope = Show(canvas, 360, 240);

        MaterialInkCanvasTheme.Apply(canvas, dark: false);

        // Upstream ships red at ten units thick; the theme exists so neither survives.
        Assert.Equal(ToSk(OnSurface(dark: false)), canvas.AvaloniaSkiaInkCanvas.Settings.InkColor);
        Assert.Equal((float)MaterialInkCanvasTheme.DefaultInkThickness, canvas.AvaloniaSkiaInkCanvas.Settings.InkThickness);
    }

    [AvaloniaFact]
    public void Apply_For_The_Dark_Variant_Takes_The_Dark_Token()
    {
        var canvas = new InkCanvas();
        using var scope = Show(canvas, 360, 240);

        MaterialInkCanvasTheme.Apply(canvas, dark: true);

        Assert.Equal(ToSk(OnSurface(dark: true)), canvas.AvaloniaSkiaInkCanvas.Settings.InkColor);
        Assert.NotEqual(OnSurface(dark: false), OnSurface(dark: true));
    }

    [AvaloniaFact]
    public void Follow_Reapplies_When_The_Theme_Variant_Changes()
    {
        var canvas = new InkCanvas();
        using var scope = Show(canvas, 360, 240);
        MaterialInkCanvasTheme.Follow(canvas);
        Assert.Equal(ToSk(OnSurface(dark: false)), canvas.AvaloniaSkiaInkCanvas.Settings.InkColor);

        scope.Window.RequestedThemeVariant = ThemeVariant.Dark;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(ToSk(OnSurface(dark: true)), canvas.AvaloniaSkiaInkCanvas.Settings.InkColor);
    }

    [AvaloniaFact]
    public void The_Theme_Leaves_The_Settings_Upstream_Owns_Alone()
    {
        var canvas = new InkCanvas();
        using var scope = Show(canvas, 360, 240);
        var before = canvas.AvaloniaSkiaInkCanvas.Settings;
        var eraserBefore = before.EraserSize;

        MaterialInkCanvasTheme.Apply(canvas, dark: false);

        // The theme writes the two values that decide what ink looks like and nothing else, so a
        // host that tuned erasing or bitmap caching keeps its tuning.
        Assert.Equal(eraserBefore, canvas.AvaloniaSkiaInkCanvas.Settings.EraserSize);
    }

    [AvaloniaFact]
    public void The_Signature_Pad_Takes_Its_Ink_From_The_On_Surface_Token()
    {
        var pad = new MdSignaturePad { Width = PadWidth, Height = PadHeight };
        using var scope = Show(pad, 360, 260);

        var brush = Assert.IsAssignableFrom<ISolidColorBrush>(pad.InkBrush);
        Assert.Equal(OnSurface(dark: false), brush.Color);
        Assert.Equal(3, pad.InkThickness);
    }

    [AvaloniaFact]
    public void A_Non_Solid_Ink_Brush_Falls_Back_To_Black_Rather_Than_Throwing()
    {
        var pad = new MdSignaturePad { Width = PadWidth, Height = PadHeight };
        using var scope = Show(pad, 360, 260);
        var canvas = Part<InkCanvas>(pad, "PART_Canvas");

        pad.InkBrush = new LinearGradientBrush
        {
            GradientStops = { new GradientStop(Colors.Red, 0), new GradientStop(Colors.Blue, 1) },
        };

        // A gradient has no Skia colour, and a signature pad that throws on one is worse than one
        // that draws black.
        Assert.Equal(SKColors.Black, canvas.AvaloniaSkiaInkCanvas.Settings.InkColor);
    }

    [AvaloniaFact]
    public void The_Pad_Is_Empty_And_Shows_Its_Placeholder_Before_The_First_Stroke()
    {
        var pad = new MdSignaturePad { Width = PadWidth, Height = PadHeight, Placeholder = "Sign above the line" };
        using var scope = Show(pad, 360, 260);

        Assert.True(pad.IsEmpty);
        Assert.Equal(0, pad.StrokeCount);
        Assert.Contains(":empty", pad.Classes);
        Assert.True(Part<TextBlock>(pad, "PART_Placeholder").IsVisible);
    }

    [AvaloniaFact]
    public void Drawing_Commits_A_Stroke_And_Hides_The_Placeholder()
    {
        var pad = new MdSignaturePad { Width = PadWidth, Height = PadHeight, Placeholder = "Sign above the line" };
        using var scope = Show(pad, 360, 260);
        var completed = 0;
        pad.StrokeCompleted += (_, _) => completed++;

        Draw(scope.Window, pad);

        Assert.Equal(1, pad.StrokeCount);
        Assert.False(pad.IsEmpty);
        Assert.Equal(1, completed);
        Assert.DoesNotContain(":empty", pad.Classes);
        Assert.False(Part<TextBlock>(pad, "PART_Placeholder").IsVisible);
    }

    [AvaloniaFact]
    public void Clear_Removes_Every_Stroke_And_Raises_Cleared()
    {
        var pad = new MdSignaturePad { Width = PadWidth, Height = PadHeight, Placeholder = "Sign above the line" };
        using var scope = Show(pad, 360, 260);
        Draw(scope.Window, pad);
        Draw(scope.Window, pad, from: new Point(30, 130), to: new Point(280, 60));
        Assert.Equal(2, pad.StrokeCount);
        var cleared = 0;
        pad.Cleared += (_, _) => cleared++;

        pad.Clear();

        Assert.Equal(0, pad.StrokeCount);
        Assert.Equal(1, cleared);
        Assert.True(pad.IsEmpty);
        Assert.Contains(":empty", pad.Classes);
    }

    [AvaloniaFact]
    public void Erasing_Switches_The_Canvas_To_Point_Erasing()
    {
        var pad = new MdSignaturePad { Width = PadWidth, Height = PadHeight };
        using var scope = Show(pad, 360, 260);
        var canvas = Part<InkCanvas>(pad, "PART_Canvas");
        Assert.Equal(InkCanvasEditingMode.Ink, canvas.EditingMode);

        pad.IsErasing = true;

        Assert.Equal(InkCanvasEditingMode.EraseByPoint, canvas.EditingMode);
        Assert.Contains(":erasing", pad.Classes);
    }

    [AvaloniaFact]
    public void The_Baseline_And_Clear_Button_Follow_Their_Switches()
    {
        var pad = new MdSignaturePad { Width = PadWidth, Height = PadHeight, ShowBaseline = false, ShowClearButton = false };
        using var scope = Show(pad, 360, 260);

        Assert.False(Part<Border>(pad, "PART_Baseline").IsVisible);
        Assert.False(Part<Button>(pad, "PART_ClearButton").IsVisible);
    }

    private static void Draw(Window window, Control target, Point? from = null, Point? to = null)
    {
        var start = target.TranslatePoint(from ?? new Point(40, 60), window)!.Value;
        var end = target.TranslatePoint(to ?? new Point(260, 140), window)!.Value;
        window.MouseMove(start, RawInputModifiers.None);
        window.MouseDown(start, MouseButton.Left, RawInputModifiers.None);
        window.MouseMove(new Point((start.X + end.X) / 2, (start.Y + end.Y) / 2), RawInputModifiers.None);
        window.MouseMove(end, RawInputModifiers.None);
        window.MouseUp(end, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
    }

    private static T Part<T>(Control owner, string name) where T : class
        => owner.GetVisualDescendants().OfType<T>().Single(control => ((Control)(object)control).Name == name);

    private static SKColor ToSk(Color color) => new(color.R, color.G, color.B, color.A);

    private static Color OnSurface(bool dark)
    {
        Assert.True(
            Application.Current!.TryGetResource("Md.Sys.Color.OnSurface", dark ? ThemeVariant.Dark : ThemeVariant.Light, out var value),
            $"Md.Sys.Color.OnSurface is not registered for the {(dark ? "dark" : "light")} variant");
        return Assert.IsType<Color>(value);
    }

    private static WindowScope Show(Control content, double width, double height)
    {
        MdMotion.SetScheme(content, MdMotionScheme.None);
        var window = new Window { Width = width, Height = height, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowScope(window);
    }

    private sealed class WindowScope(Window window) : IDisposable
    {
        public Window Window => window;
        public void Dispose() => window.Close();
    }
}
