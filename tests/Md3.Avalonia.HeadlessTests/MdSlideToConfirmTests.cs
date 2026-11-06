using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Gallery.Pages;
using Md3.Avalonia.Localization;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdSlideToConfirmTests
{
    [AvaloniaFact]
    public void Only_Thumb_Drag_Past_Threshold_On_Release_Confirms_Once()
    {
        var slide = Create();
        MdMotion.SetScheme(slide, MdMotionScheme.None);
        var window = Show(slide);
        try
        {
            var count = 0;
            slide.Confirmed += (_, _) => count++;
            Drag(window, slide, 140, 270); // the track is not the thumb
            Assert.False(slide.IsConfirmed);
            Drag(window, slide, 32, 140); // below threshold
            Assert.Equal(0, slide.Progress);
            Assert.Equal(0, count);
            Down(window, slide, 32);
            Move(window, slide, 225);
            Assert.True(slide.IsDragging);
            Assert.False(slide.IsConfirmed); // crossing the threshold is not enough until release
            Up(window, slide, 225);
            Assert.True(slide.IsConfirmed);
            Assert.False(slide.IsDragging);
            Assert.Equal(1, slide.Progress);
            Assert.Equal(1, count);
            Drag(window, slide, 248, 32);
            Assert.Equal(1, count);
            slide.Reset();
            Drag(window, slide, 32, 248);
            Assert.Equal(2, count);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Rtl_Drag_Uses_Logical_Start_To_End()
    {
        var slide = Create();
        slide.FlowDirection = FlowDirection.RightToLeft;
        MdMotion.SetScheme(slide, MdMotionScheme.None);
        var window = Show(slide);
        try
        {
            Drag(window, slide, 32, 248);
            Assert.False(slide.IsConfirmed);
            Drag(window, slide, 248, 32);
            Assert.True(slide.IsConfirmed);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Keyboard_And_TwoWay_Binding_Keep_OneShot_And_Reset_Contracts()
    {
        var model = new Model();
        var slide = Create();
        slide.Bind(MdSlideToConfirm.IsConfirmedProperty, new Binding(nameof(Model.Value)) { Source = model, Mode = BindingMode.TwoWay });
        var window = Show(slide);
        try
        {
            var count = 0;
            slide.Confirmed += (_, _) => count++;
            Assert.True(slide.Focus());
            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Dispatcher.UIThread.RunJobs();
            Assert.True(model.Value);
            Assert.Equal(1, count);
            window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, null);
            Assert.Equal(1, count);
            slide.Reset();
            Dispatcher.UIThread.RunJobs();
            Assert.False(model.Value);
            model.Value = true; // host state updates are not new actions
            Dispatcher.UIThread.RunJobs();
            Assert.True(slide.IsConfirmed);
            Assert.Equal(1, count);
            model.Value = false;
            Dispatcher.UIThread.RunJobs();
            window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, null);
            Dispatcher.UIThread.RunJobs();
            Assert.True(model.Value);
            Assert.Equal(2, count);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Automation_Is_A_Localized_Button_And_Respects_Disabled_And_Completion()
    {
        var slide = Create();
        MdLocalization.SetCulture(slide, CultureInfo.GetCultureInfo("zh-CN"));
        var window = Show(slide);
        try
        {
            var count = 0;
            slide.Confirmed += (_, _) => count++;
            var peer = ControlAutomationPeer.CreatePeerForElement(slide);
            Assert.Equal(AutomationControlType.Button, peer.GetAutomationControlType());
            Assert.Equal("滑动以确认", peer.GetName());
            var invoke = Assert.IsAssignableFrom<IInvokeProvider>(peer.GetProvider<IInvokeProvider>());
            invoke.Invoke();
            Assert.Equal("滑动以确认", peer.GetName());
            Assert.Equal("已确认", peer.GetItemStatus());
            Assert.Equal(1, count);
            Assert.Throws<InvalidOperationException>(() => invoke.Invoke());
            slide.Reset();
            slide.IsEnabled = false;
            Assert.Throws<ElementNotEnabledException>(() => invoke.Invoke());
            Assert.Equal(1, count);
            slide.IsEnabled = true;
            AutomationProperties.SetName(slide, "Transfer funds");
            Assert.Equal("Transfer funds", peer.GetName());
            invoke.Invoke();
            Assert.Equal("Transfer funds", peer.GetName());
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Pointer_Is_Canceled_On_Disable_Direction_Change_And_Detach()
    {
        var slide = Create();
        var window = Show(slide);
        try
        {
            Down(window, slide, 32);
            Move(window, slide, 180);
            Assert.True(slide.IsDragging);
            slide.IsEnabled = false;
            Assert.False(slide.IsDragging);
            Assert.Equal(0, slide.Progress);
            Up(window, slide, 248);
            slide.IsEnabled = true;
            Assert.False(slide.IsConfirmed);
            Down(window, slide, 32);
            Move(window, slide, 180);
            slide.FlowDirection = FlowDirection.RightToLeft;
            Assert.False(slide.IsDragging);
            Assert.Equal(0, slide.Progress);
            Up(window, slide, 248);
            slide.FlowDirection = FlowDirection.LeftToRight;
            Down(window, slide, 32);
            Move(window, slide, 180);
            window.Content = null;
            Assert.False(slide.IsDragging);
            Assert.Equal(0, slide.Progress);
            Assert.False(slide.IsSettling);
            Assert.False(slide.IsConfirmed);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Capture_Loss_And_Reset_Cancel_Only_The_Active_Gesture()
    {
        var slide = Create();
        MdMotion.SetScheme(slide, MdMotionScheme.None);
        IPointer? active = null;
        slide.AddHandler(InputElement.PointerPressedEvent, (_, e) => active = e.Pointer, RoutingStrategies.Bubble, true);
        var window = Show(slide);
        try
        {
            Down(window, slide, 32);
            Move(window, slide, 180);
            Assert.True(slide.IsDragging);
            using var other = new Pointer(Pointer.GetNextFreeId(), PointerType.Touch, false);
            slide.RaiseEvent(new PointerCaptureLostEventArgs(slide, other));
            Assert.True(slide.IsDragging); // an unrelated finger cannot cancel the owner
            Assert.NotNull(active);
            active!.Capture(null);
            Assert.False(slide.IsDragging);
            Assert.Equal(0, slide.Progress);
            Up(window, slide, 248);
            Assert.False(slide.IsConfirmed);
            Down(window, slide, 32);
            Move(window, slide, 180);
            slide.Reset(); // even when already unconfirmed, abort a pending drag
            Assert.False(slide.IsDragging);
            Assert.Equal(0, slide.Progress);
            Up(window, slide, 248);
            Assert.False(slide.IsConfirmed);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Spring_Settles_And_Reduced_Motion_Snaps_Even_MidFlight()
    {
        var slide = Create();
        MdMotion.SetScheme(slide, MdMotionScheme.Standard);
        var window = Show(slide);
        try
        {
            Drag(window, slide, 32, 140);
            Assert.True(slide.IsSettling);
            Assert.InRange(slide.Progress, 0.49, 0.51);
            for (var i = 0; i < 10; i++) slide.AdvanceSettle(TimeSpan.FromMilliseconds(16));
            Assert.True(slide.Progress < 0.49);
            MdMotion.SetScheme(slide, MdMotionScheme.Reduced);
            Assert.False(slide.IsSettling);
            Assert.Equal(0, slide.Progress);
            Drag(window, slide, 32, 140);
            Assert.False(slide.IsSettling);
            Assert.Equal(0, slide.Progress);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Threshold_Is_Coerced_And_Host_State_Does_Not_Raise_The_Action()
    {
        var slide = Create();
        var count = 0;
        slide.Confirmed += (_, _) => count++;
        slide.Threshold = double.NaN;
        Assert.Equal(.8, slide.Threshold);
        slide.Threshold = -1;
        Assert.Equal(.1, slide.Threshold);
        slide.Threshold = 2;
        Assert.Equal(1, slide.Threshold);
        slide.IsConfirmed = true;
        Assert.Equal(1, slide.Progress);
        Assert.Equal(0, count);
        Assert.False(slide.Confirm());
        slide.Reset();
        Assert.True(slide.Confirm());
        Assert.Equal(1, count);
    }

    [AvaloniaFact]
    public void Rendered_Thumb_Moves_From_Start_To_End_When_Confirmed()
    {
        var slide = Create();
        slide.TrackBrush = Brushes.Blue;
        slide.ProgressBrush = Brushes.Blue;
        slide.ThumbBrush = Brushes.Red;
        var window = Show(slide);
        try
        {
            uint Pixel(double x, double y)
            {
                Dispatcher.UIThread.RunJobs();
                var frame = window.CaptureRenderedFrame();
                Assert.NotNull(frame);
                var size = frame!.PixelSize;
                var pixels = new uint[size.Width * size.Height];
                var pinned = GCHandle.Alloc(pixels, GCHandleType.Pinned);
                try { frame.CopyPixels(new PixelRect(0, 0, size.Width, size.Height), pinned.AddrOfPinnedObject(), pixels.Length * 4, size.Width * 4); }
                finally { pinned.Free(); }
                var point = slide.TranslatePoint(new Point(x, y), window)!.Value;
                return pixels[(int)point.Y * size.Width + (int)point.X];
            }
            var start = Pixel(32, 23); // away from the arrow
            var end = Pixel(248, 23);
            Assert.NotEqual(start, end);
            Assert.True(slide.Confirm());
            Assert.Equal(start, Pixel(248, 23));
            Assert.Equal(end, Pixel(32, 23));
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Gallery_Native_Smoke_Exercises_Callback_And_Reset()
    {
        var page = new DismissibleGalleryPage();
        var window = Show(page);
        try
        {
            page.RunSlideNativeAotSmoke();
            var slide = page.GetVisualDescendants().OfType<MdSlideToConfirm>().Single();
            Assert.False(slide.IsConfirmed);
            Assert.Equal(0, slide.Progress);
        }
        finally { window.Close(); }
    }

    private static MdSlideToConfirm Create() => new()
    {
        Width = 280, Height = 64, HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top
    };
    private static Window Show(Control control)
    {
        var window = new Window { Width = 600, Height = 500, Content = control };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
    private static Point At(Window w, MdSlideToConfirm c, double x) => c.TranslatePoint(new Point(x, 32), w)!.Value;
    private static void Down(Window w, MdSlideToConfirm c, double x)
    {
        var p = At(w, c, x);
        w.MouseMove(p, RawInputModifiers.None);
        w.MouseDown(p, MouseButton.Left, RawInputModifiers.None);
    }
    private static void Move(Window w, MdSlideToConfirm c, double x) => w.MouseMove(At(w, c, x), RawInputModifiers.LeftMouseButton);
    private static void Up(Window w, MdSlideToConfirm c, double x) => w.MouseUp(At(w, c, x), MouseButton.Left, RawInputModifiers.None);
    private static void Drag(Window w, MdSlideToConfirm c, double from, double to) { Down(w, c, from); Move(w, c, to); Up(w, c, to); }

    private sealed class Model : INotifyPropertyChanged
    {
        private bool _value;
        public bool Value { get => _value; set { _value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))); } }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
