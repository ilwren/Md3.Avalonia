using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Md3.Avalonia.Extra.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdAutoSizeTextTests
{
    [AvaloniaFact]
    public void Resizes_In_Both_Directions_Without_Overwriting_FontSize_Binding()
    {
        var model = new SizeModel { Size = 32 };
        var text = Create("The quick brown fox jumps", 120, 55);
        text.Bind(TemplatedControl.FontSizeProperty, new Binding(nameof(SizeModel.Size)) { Source = model });
        var window = Show(text);
        try
        {
            Assert.InRange(text.EffectiveFontSize, 8, 31);
            Assert.Equal(32, text.FontSize);
            text.Width = 600;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(32, text.EffectiveFontSize);
            model.Size = 28;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(28, text.FontSize);
            Assert.Equal(28, text.EffectiveFontSize);
            text.Text = "A substantially longer example that should shrink even at this width when the height is small";
            text.Width = 110;
            Dispatcher.UIThread.RunJobs();
            Assert.InRange(text.EffectiveFontSize, 8, 27);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Wrapped_Text_Respects_MaxLines_And_Padding()
    {
        var text = Create("one two three four five six seven eight", 180, 150);
        text.TextWrapping = TextWrapping.Wrap;
        text.MaxLines = 1;
        text.Padding = new Thickness(10);
        var window = Show(text);
        try
        {
            var oneLine = text.EffectiveFontSize;
            text.MaxLines = 3;
            Dispatcher.UIThread.RunJobs();
            Assert.True(text.EffectiveFontSize > oneLine);
            var padded = text.EffectiveFontSize;
            text.Padding = new Thickness(0);
            Dispatcher.UIThread.RunJobs();
            Assert.True(text.EffectiveFontSize >= padded);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Minimum_Step_Height_And_Unbounded_Measure_Are_Handled()
    {
        var text = Create("A very long text that cannot possibly fit", 20, 10);
        text.MinFontSize = 14;
        text.StepGranularity = 3;
        text.TextTrimming = TextTrimming.CharacterEllipsis;
        var window = Show(text);
        try
        {
            Assert.Equal(14, text.EffectiveFontSize);
            Assert.NotNull(window.CaptureRenderedFrame());
            text.Width = double.NaN;
            text.Height = double.NaN;
            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Assert.Equal(32, text.EffectiveFontSize);
            text.Measure(new Size(20, double.PositiveInfinity));
            Assert.Equal(14, text.EffectiveFontSize);
            text.MinFontSize = double.NaN;
            text.StepGranularity = 0;
            Assert.Equal(12, text.MinFontSize);
            Assert.Equal(0.1, text.StepGranularity);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Automation_Exposes_Text_And_Respects_Explicit_Name()
    {
        var text = Create("Accessible text", 160, 55);
        var window = Show(text);
        try
        {
            var peer = ControlAutomationPeer.CreatePeerForElement(text);
            Assert.Equal(AutomationControlType.Text, peer.GetAutomationControlType());
            Assert.Equal("Accessible text", peer.GetName());
            var changes = 0;
            peer.PropertyChanged += (_, e) => { if (e.Property == AutomationElementIdentifiers.NameProperty) changes++; };
            text.Text = "Changed";
            Assert.Equal("Changed", peer.GetName());
            Assert.Equal(1, changes);
            AutomationProperties.SetName(text, "Custom name");
            Assert.Equal("Custom name", peer.GetName());
            var before = changes;
            text.Text = "Another change";
            Assert.Equal(before, changes);
            Assert.Equal("Custom name", peer.GetName());
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Rtl_Text_Is_Painted_And_Fits_Bounded_Width()
    {
        var text = Create("مرحبا بالعالم مرحبا", 120, 50);
        text.FlowDirection = FlowDirection.RightToLeft;
        text.Foreground = Brushes.Red;
        var window = Show(text);
        try
        {
            Assert.InRange(text.EffectiveFontSize, 8, 31);
            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            var pixels = new uint[frame!.PixelSize.Width * frame.PixelSize.Height];
            var pinned = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try { frame.CopyPixels(new PixelRect(0, 0, frame.PixelSize.Width, frame.PixelSize.Height), pinned.AddrOfPinnedObject(), pixels.Length * 4, frame.PixelSize.Width * 4); }
            finally { pinned.Free(); }
            // Only inspect the control's own rectangle, not window chrome. Red glyphs on white
            // have little green regardless of whether the frame is BGRA or RGBA.
            var origin = text.TranslatePoint(new Point(0, 0), window)!.Value;
            var left = Math.Max(0, (int)origin.X);
            var top = Math.Max(0, (int)origin.Y);
            var right = Math.Min(frame.PixelSize.Width, (int)Math.Ceiling(origin.X + text.Bounds.Width));
            var bottom = Math.Min(frame.PixelSize.Height, (int)Math.Ceiling(origin.Y + text.Bounds.Height));
            var found = false;
            for (var y = top; y < bottom && !found; y++)
                for (var x = left; x < right && !found; x++)
                {
                    var p = pixels[y * frame.PixelSize.Width + x];
                    found = (p & 0x00ff00u) < 0x004000u && ((p & 0xffu) > 200 || ((p >> 16) & 0xffu) > 200);
                }
            Assert.True(found, "Expected red text pixels inside the control bounds.");
        }
        finally { window.Close(); }
    }

    private static MdAutoSizeText Create(string value, double width, double height) => new()
    {
        Text = value, Width = width, Height = height, FontSize = 32, MinFontSize = 8,
        HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top
    };

    private static Window Show(Control content)
    {
        var window = new Window { Width = 700, Height = 200, Content = content, Background = Brushes.White };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private sealed class SizeModel : INotifyPropertyChanged
    {
        private double _size;
        public double Size { get => _size; set { _size = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Size))); } }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
