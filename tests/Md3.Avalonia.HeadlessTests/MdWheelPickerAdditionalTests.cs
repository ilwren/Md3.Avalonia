using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Extra.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdWheelPickerAdditionalTests
{
    [AvaloniaFact]
    public void TwoWay_Binding_Survives_User_Changes_And_Host_Updates()
    {
        var model = new Model { Quantity = 10 };
        var wheel = Create();
        wheel.Bind(MdWheelPicker.ValueProperty, new Binding(nameof(Model.Quantity)) { Source = model, Mode = BindingMode.TwoWay });
        var window = Show(wheel);
        try
        {
            Assert.Equal(10, wheel.Value);
            Assert.True(wheel.Focus());
            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(15, model.Quantity);
            model.Quantity = 25;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(25, wheel.Value);
            window.KeyPress(Key.Up, RawInputModifiers.None, PhysicalKey.ArrowUp, null);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(20, model.Quantity);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Selected_Band_Is_Rendered_And_Pointer_Capture_Is_Canceled_When_ReadOnly()
    {
        var wheel = Create();
        wheel.SelectionBrush = Brushes.Red;
        var window = Show(wheel);
        try
        {
            Dispatcher.UIThread.RunJobs();
            var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            var size = frame!.PixelSize;
            var pixels = new uint[size.Width * size.Height];
            var pinned = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try { frame.CopyPixels(new PixelRect(0, 0, size.Width, size.Height), pinned.AddrOfPinnedObject(), pixels.Length * 4, size.Width * 4); }
            finally { pinned.Free(); }
            var origin = wheel.TranslatePoint(new Point(), window)!.Value;
            uint Pixel(int x, int y) => pixels[((int)origin.Y + y) * size.Width + (int)origin.X + x];
            Assert.NotEqual(Pixel(10, 100), Pixel(10, 20));

            Point At(double y) => wheel.TranslatePoint(new Point(60, y), window)!.Value;
            window.MouseMove(At(100), RawInputModifiers.None);
            window.MouseDown(At(100), MouseButton.Left, RawInputModifiers.None);
            wheel.IsReadOnly = true;
            window.MouseMove(At(20), RawInputModifiers.LeftMouseButton);
            window.MouseUp(At(20), MouseButton.Left, RawInputModifiers.None);
            Assert.Equal(10, wheel.Value);
            wheel.IsReadOnly = false;
            window.MouseMove(At(100), RawInputModifiers.None);
            window.MouseDown(At(100), MouseButton.Left, RawInputModifiers.None);
            wheel.IsEnabled = false;
            window.MouseMove(At(20), RawInputModifiers.LeftMouseButton);
            window.MouseUp(At(20), MouseButton.Left, RawInputModifiers.None);
            wheel.IsEnabled = true;
            Assert.Equal(10, wheel.Value);
        }
        finally { window.Close(); }
    }

    private static MdWheelPicker Create() => new()
    {
        Minimum = 0, Maximum = 30, Step = 5, Value = 10, Width = 120, Height = 200,
        HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top
    };

    private static Window Show(Control control)
    {
        var window = new Window { Width = 300, Height = 260, Content = control };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private sealed class Model : INotifyPropertyChanged
    {
        private int _quantity;
        public int Quantity { get => _quantity; set { _quantity = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantity))); } }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
