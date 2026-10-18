using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Themes;

namespace Md3.Avalonia.Gallery.Pages;

public partial class SignaturePadGalleryPage : UserControl
{
    public SignaturePadGalleryPage()
    {
        InitializeComponent();

        // The bare canvas draws itself and registers no styles, so the theme reaches it the way it
        // reaches a map or a plot: an entry point that writes the settings it draws with. This is
        // the same ink the signature pad gets from the ControlTheme.
        BareCanvas.Loaded += (_, _) => MaterialInkCanvasTheme.Follow(BareCanvas);

        Pad.StrokeCompleted += OnSignatureChanged;
        Pad.Cleared += OnSignatureChanged;
        EraseToggle.Content = "Erase";
        UpdateReadout();
    }

    private void ToggleErase(object? sender, RoutedEventArgs e)
    {
        // Only safe between strokes - upstream refuses a mode switch mid-stroke. A button click
        // cannot land mid-stroke, which is why the affordance is a button and not a hold.
        Pad.IsErasing = !Pad.IsErasing;
        EraseToggle.Content = Pad.IsErasing ? "Pen" : "Erase";
    }

    private void ClearThePad(object? sender, RoutedEventArgs e) => Pad.Clear();

    private void OnSignatureChanged(object? sender, RoutedEventArgs e) => UpdateReadout();

    private void UpdateReadout() =>
        StrokeReadout.Text = Pad.StrokeCount switch
        {
            0 => "No strokes yet.",
            1 => "One stroke.",
            var count => $"{count} strokes.",
        };
}
