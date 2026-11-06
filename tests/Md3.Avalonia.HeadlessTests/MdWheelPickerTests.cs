using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Localization;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdWheelPickerTests
{
    [AvaloniaFact]
    public void Values_Snap_To_The_Grid_And_Recoerce_When_Bounds_Change()
    {
        var wheel = new MdWheelPicker { Minimum = -10, Maximum = 11, Step = 4, Value = -8 };
        Assert.Equal(-6, wheel.Value); // midpoint ties go up
        wheel.Value = 11;
        Assert.Equal(10, wheel.Value); // 11 is a bound, not a grid value
        wheel.Step = 3;
        Assert.Equal(11, wheel.Value); // underlying requested value is re-coerced
        wheel.Minimum = -1;
        Assert.Equal(11, wheel.Value);
        wheel.Maximum = 8;
        Assert.Equal(8, wheel.Value);
        wheel.Step = 0;
        Assert.Equal(1, wheel.Step);
        wheel.ItemHeight = double.NaN;
        Assert.Equal(40, wheel.ItemHeight);
    }

    [AvaloniaFact]
    public void Extreme_Integer_Ranges_Do_Not_Overflow()
    {
        var wheel = new MdWheelPicker { Minimum = int.MinValue, Maximum = int.MaxValue, Step = int.MaxValue };
        wheel.Value = int.MaxValue;
        Assert.Equal(int.MaxValue - 1, wheel.Value);
        wheel.Value = int.MinValue;
        Assert.Equal(int.MinValue, wheel.Value);
    }

    [AvaloniaFact]
    public void Keyboard_Wheel_And_Automation_Respect_ReadOnly_And_Focus()
    {
        var wheel = Create();
        var other = new Button { Content = "Other" };
        var window = Show(new StackPanel { Children = { wheel, other } });
        try
        {
            Assert.True(other.Focus());
            var changes = 0;
            wheel.ValueChanged += (_, _) => changes++;
            var point = wheel.TranslatePoint(new Point(60, 100), window)!.Value;
            window.MouseWheel(point, new Vector(0, -1), RawInputModifiers.None);
            Assert.Equal(10, wheel.Value); // unfocused wheel must not steal page scroll
            Assert.True(wheel.Focus());
            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            Assert.Equal(15, wheel.Value);
            window.MouseWheel(point, new Vector(0, 1), RawInputModifiers.None);
            Assert.Equal(10, wheel.Value);
            var peer = ControlAutomationPeer.CreatePeerForElement(wheel);
            MdLocalization.SetCulture(wheel, CultureInfo.GetCultureInfo("zh-CN"));
            Assert.Equal("数字选择器", peer.GetName());
            Assert.Equal(AutomationControlType.Spinner, peer.GetAutomationControlType());
            var range = Assert.IsAssignableFrom<IRangeValueProvider>(peer.GetProvider<IRangeValueProvider>());
            Assert.Equal(30, range.Maximum);
            range.SetValue(22);
            Assert.Equal(20, wheel.Value);
            Assert.Equal(3, changes);
            wheel.IsReadOnly = true;
            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            window.MouseWheel(point, new Vector(0, -1), RawInputModifiers.None);
            Assert.True(range.IsReadOnly);
            Assert.Throws<InvalidOperationException>(() => range.SetValue(5));
            Assert.Equal(20, wheel.Value);
            Assert.Equal(3, changes);
            AutomationProperties.SetName(wheel, "Quantity");
            Assert.Equal("Quantity", peer.GetName());
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Pointer_Taps_And_Drags_Use_The_Same_Visible_Rows()
    {
        var wheel = Create();
        var window = Show(wheel);
        try
        {
            var changes = 0;
            wheel.ValueChanged += (_, _) => changes++;
            Point At(double y) => wheel.TranslatePoint(new Point(60, y), window)!.Value;
            window.MouseMove(At(140), RawInputModifiers.None);
            window.MouseDown(At(140), MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(At(140), MouseButton.Left, RawInputModifiers.None);
            Assert.Equal(15, wheel.Value);
            window.MouseMove(At(100), RawInputModifiers.None);
            window.MouseDown(At(100), MouseButton.Left, RawInputModifiers.None);
            window.MouseMove(At(20), RawInputModifiers.LeftMouseButton);
            window.MouseUp(At(20), MouseButton.Left, RawInputModifiers.None);
            Assert.Equal(25, wheel.Value);
            Assert.Equal(2, changes);
            wheel.IsReadOnly = true;
            window.MouseDown(At(140), MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(At(140), MouseButton.Left, RawInputModifiers.None);
            Assert.Equal(25, wheel.Value);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Automation_Reports_The_Last_Reachable_Value_And_Rejects_Invalid_Requests()
    {
        var wheel = Create();
        wheel.Maximum = 32;
        var peer = ControlAutomationPeer.CreatePeerForElement(wheel);
        var range = Assert.IsAssignableFrom<IRangeValueProvider>(peer.GetProvider<IRangeValueProvider>());
        Assert.Equal(30, range.Maximum);
        Assert.Equal(5, range.SmallChange);
        Assert.Throws<ArgumentOutOfRangeException>(() => range.SetValue(32));
        Assert.Throws<ArgumentOutOfRangeException>(() => range.SetValue(double.NaN));
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
}
