using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdWheelPickerInertiaTests
{
    [Fact]
    public void Velocity_Uses_The_Latest_Direction_And_Discards_A_Pause_Or_Clock_Reset()
    {
        var samples = new WheelReleaseVelocity();
        samples.Reset(1000, 0);
        samples.Add(1050, 2);
        Assert.Equal(24, samples.Velocity);
        samples.Add(1075, 1.5);
        samples.Add(1100, 1);
        Assert.Equal(-20, samples.Velocity);
        samples.Add(1250, 1);
        Assert.Equal(0, samples.Velocity);
        samples.Add(10, 2);
        samples.Add(10, 3);
        Assert.Equal(0, samples.Velocity);
    }

    [AvaloniaFact]
    public void Flick_Continues_Smoothly_Then_Settles_On_A_Valid_Selection()
    {
        var wheel = Create();
        var window = Show(wheel);
        try
        {
            var changes = new List<int>();
            wheel.ValueChanged += (_, value) => changes.Add(value);
            using var pointer = Touch();
            Press(wheel, window, pointer, 100, 1000);
            Move(wheel, window, pointer, 46, 1040); // 1.35 rows; selection is already discrete
            Assert.InRange(wheel.VisualPosition, 51.34, 51.36);
            Assert.Equal(51, wheel.Value);
            Release(wheel, window, pointer, 46, 1050);
            Assert.True(wheel.IsSettling);
            var released = wheel.VisualPosition;
            wheel.AdvanceMotion(TimeSpan.FromMilliseconds(16));
            Assert.True(wheel.VisualPosition > released);
            Finish(wheel);
            Assert.True(wheel.Value > 51);
            Assert.Equal((double)wheel.Value, wheel.VisualPosition);
            Assert.Equal(wheel.Value, changes[^1]);
            Assert.All(changes, value => Assert.InRange(value, 0, 100));
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Paused_And_Reversed_Gestures_Use_Their_Own_Release_Velocity()
    {
        var wheel = Create();
        var window = Show(wheel);
        try
        {
            using var pointer = Touch();
            Press(wheel, window, pointer, 100, 1000);
            Move(wheel, window, pointer, 46, 1040);
            Release(wheel, window, pointer, 46, 1200);
            Finish(wheel);
            Assert.Equal(51, wheel.Value);
            Assert.Equal(51, wheel.VisualPosition);

            wheel.Value = 50;
            Press(wheel, window, pointer, 100, 2000);
            Move(wheel, window, pointer, 60, 2040);
            Move(wheel, window, pointer, 84, 2080);
            Release(wheel, window, pointer, 84, 2090);
            var before = wheel.VisualPosition;
            wheel.AdvanceMotion(TimeSpan.FromMilliseconds(16));
            Assert.True(wheel.VisualPosition < before);
            Finish(wheel);
            Assert.True(wheel.Value < 50);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Fling_Stops_At_Last_Reachable_Bound_And_Cannot_Overflow()
    {
        var wheel = Create();
        wheel.Minimum = -10;
        wheel.Maximum = 11;
        wheel.Step = 4;
        wheel.Value = 6;
        var window = Show(wheel);
        try
        {
            using var pointer = Touch();
            Press(wheel, window, pointer, 100, 1000);
            Move(wheel, window, pointer, 60, 1040);
            Release(wheel, window, pointer, 60, 1050);
            for (var i = 0; i < 180 && wheel.IsSettling; i++)
            {
                wheel.AdvanceMotion(TimeSpan.FromMilliseconds(16));
                Assert.InRange(wheel.VisualPosition, 0, 5);
            }
            Assert.False(wheel.IsSettling);
            Assert.Equal(10, wheel.Value); // 11 is off-grid
            wheel.Minimum = int.MinValue;
            wheel.Maximum = int.MaxValue;
            wheel.Step = 1;
            wheel.Value = int.MaxValue - 1;
            Press(wheel, window, pointer, 100, 2000);
            Move(wheel, window, pointer, 20, 2040);
            Release(wheel, window, pointer, 20, 2050);
            Finish(wheel);
            Assert.Equal(int.MaxValue, wheel.Value);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Regrab_And_Host_Value_Interrupt_Momentum_Without_A_Jump()
    {
        var wheel = Create();
        var window = Show(wheel);
        try
        {
            using var pointer = Touch();
            Flick(wheel, window, pointer);
            wheel.AdvanceMotion(TimeSpan.FromMilliseconds(16));
            var position = wheel.VisualPosition;
            Press(wheel, window, pointer, 100, 1100);
            Assert.False(wheel.IsSettling);
            Assert.Equal(position, wheel.VisualPosition);
            Move(wheel, window, pointer, 80, 1120);
            Assert.InRange(wheel.VisualPosition, position + .49, position + .51);
            wheel.Value = 20;
            Assert.Equal(20, wheel.VisualPosition);
            Release(wheel, window, pointer, 20, 1130);
            Assert.False(wheel.IsSettling);
            Assert.Equal(20, wheel.Value);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Interruption_By_Callback_Grid_Changes_Disable_Detach_And_Reduced_Motion_Is_Safe()
    {
        var wheel = Create();
        var window = Show(wheel);
        try
        {
            using var pointer = Touch();
            var interrupted = false;
            wheel.ValueChanged += (_, value) =>
            {
                if (!interrupted && value >= 52) { interrupted = true; wheel.Value = 10; }
            };
            Flick(wheel, window, pointer);
            for (var i = 0; i < 100 && wheel.IsSettling; i++) wheel.AdvanceMotion(TimeSpan.FromMilliseconds(16));
            Assert.True(interrupted);
            Assert.False(wheel.IsSettling);
            Assert.Equal(10, wheel.Value);
            Assert.Equal(10, wheel.VisualPosition);

            wheel.Value = 50;
            Flick(wheel, window, pointer);
            wheel.Step = 2;
            Assert.False(wheel.IsSettling);
            Assert.Equal((wheel.Value - wheel.Minimum) / 2d, wheel.VisualPosition);
            Flick(wheel, window, pointer);
            wheel.IsReadOnly = true;
            Assert.False(wheel.IsSettling);
            wheel.IsReadOnly = false;
            Flick(wheel, window, pointer);
            wheel.IsEnabled = false;
            Assert.False(wheel.IsSettling);
            wheel.IsEnabled = true;
            Flick(wheel, window, pointer);
            MdMotion.SetScheme(wheel, MdMotionScheme.Reduced);
            Assert.False(wheel.IsSettling);
            Assert.Equal((wheel.Value - wheel.Minimum) / 2d, wheel.VisualPosition);
            Flick(wheel, window, pointer);
            Assert.False(wheel.IsSettling);
            MdMotion.SetScheme(wheel, MdMotionScheme.Expressive);
            Flick(wheel, window, pointer);
            window.Content = null;
            Assert.False(wheel.IsSettling);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Keyboard_And_Automation_Cancel_Momentum_And_Retain_One_Step_Semantics()
    {
        var wheel = Create();
        var window = Show(wheel);
        try
        {
            using var pointer = Touch();
            Flick(wheel, window, pointer);
            var before = wheel.Value;
            Assert.True(wheel.Focus());
            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            Assert.False(wheel.IsSettling);
            Assert.Equal(before + 1, wheel.Value);
            Flick(wheel, window, pointer);
            var peer = ControlAutomationPeer.CreatePeerForElement(wheel);
            var range = Assert.IsAssignableFrom<IRangeValueProvider>(peer.GetProvider<IRangeValueProvider>());
            range.SetValue(33);
            Assert.Equal(33, wheel.Value);
            Assert.Equal(33, wheel.VisualPosition);
            Assert.False(wheel.IsSettling);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public async Task Real_Timer_Advances_And_Detachment_Stops_It()
    {
        var wheel = Create();
        var window = Show(wheel);
        try
        {
            using var pointer = Touch();
            Flick(wheel, window, pointer);
            var initial = wheel.VisualPosition;
            await Task.Delay(180);
            Assert.True(wheel.VisualPosition > initial);
            window.Content = null;
            var detached = wheel.VisualPosition;
            Assert.False(wheel.IsSettling);
            await Task.Delay(80);
            Assert.Equal(detached, wheel.VisualPosition);
        }
        finally { window.Close(); }
    }

    private static MdWheelPicker Create()
    {
        var wheel = new MdWheelPicker { Minimum = 0, Maximum = 100, Step = 1, Value = 50, Width = 120, Height = 200,
            HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        MdMotion.SetScheme(wheel, MdMotionScheme.Expressive);
        return wheel;
    }
    private static Window Show(Control content)
    {
        var window = new Window { Width = 300, Height = 260, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
    private static Pointer Touch() => new(Pointer.GetNextFreeId(), PointerType.Touch, true);
    private static Point At(MdWheelPicker wheel, Window window, double y) => wheel.TranslatePoint(new Point(60, y), window)!.Value;
    private static void Press(MdWheelPicker wheel, Window window, IPointer pointer, double y, ulong time) =>
        wheel.RaiseEvent(new PointerPressedEventArgs(wheel, pointer, window, At(wheel, window, y), time, new PointerPointProperties(), KeyModifiers.None));
    private static void Move(MdWheelPicker wheel, Window window, IPointer pointer, double y, ulong time) =>
        wheel.RaiseEvent(new PointerEventArgs(InputElement.PointerMovedEvent, wheel, pointer, window, At(wheel, window, y), time, new PointerPointProperties(), KeyModifiers.None));
    private static void Release(MdWheelPicker wheel, Window window, IPointer pointer, double y, ulong time) =>
        wheel.RaiseEvent(new PointerReleasedEventArgs(wheel, pointer, window, At(wheel, window, y), time, new PointerPointProperties(), KeyModifiers.None, MouseButton.Left));
    private static void Flick(MdWheelPicker wheel, Window window, IPointer pointer)
    {
        Press(wheel, window, pointer, 100, 1000);
        Move(wheel, window, pointer, 60, 1040);
        Release(wheel, window, pointer, 60, 1050);
    }
    private static void Finish(MdWheelPicker wheel)
    {
        for (var i = 0; i < 240 && wheel.IsSettling; i++) wheel.AdvanceMotion(TimeSpan.FromMilliseconds(16));
        Assert.False(wheel.IsSettling);
    }
}
