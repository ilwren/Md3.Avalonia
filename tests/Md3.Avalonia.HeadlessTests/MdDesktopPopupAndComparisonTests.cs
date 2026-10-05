using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Regressions from the thirteenth review pass: a dialog flow that queued instead of advancing,
/// popup surfaces whose shadow and entrance offset fell outside the popup window, a floating
/// label laid out above its own control, and a comparison slider whose two layers drifted apart.
/// </summary>
public sealed class MdDesktopPopupAndComparisonTests
{
    private static T Descendant<T>(Visual root, string name) where T : Visual =>
        root.GetVisualDescendants().OfType<T>().Single(v => (v as StyledElement)?.Name == name);

    // ---------------------------------------------------------------- dialog replacement

    private sealed record StepOne;
    private sealed record StepTwo;

    [AvaloniaFact]
    public async Task Replacing_A_Dialog_Swaps_It_Instead_Of_Queueing_Behind_It()
    {
        // The about dialog's "View licenses" called ShowAsync, which is a queue: the license
        // dialog only appeared once the user pressed Close, so Close looked like it opened a
        // dialog instead of dismissing one.
        var service = new MdDialogService();
        var host = new MdDialogHost { Content = new TextBlock { Text = "Page" }, Service = service };
        host.DataTemplates.Add(new FuncDataTemplate<StepOne>((_, _) => new MdDialog { Headline = "One" }));
        host.DataTemplates.Add(new FuncDataTemplate<StepTwo>((_, _) => new MdDialog { Headline = "Two" }));
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            var first = ((IMdDialogService)service).ShowAsync(new StepOne());
            Dispatcher.UIThread.RunJobs();
            Assert.IsType<StepOne>(host.Dialog);

            var second = ((IMdDialogService)service).ReplaceAsync(new StepTwo());
            Dispatcher.UIThread.RunJobs();

            // The one it displaced completes immediately, and the replacement is on screen now -
            // not after another close.
            Assert.Null(await first);
            Assert.IsType<StepTwo>(host.Dialog);
            Assert.True(host.IsOpen);

            host.Close("done");
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("done", await second);

            // And nothing is left queued to ambush the next close.
            Assert.False(host.IsOpen);
            Assert.False(service.IsOpen);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void A_Replacement_Jumps_Ahead_Of_Whatever_Was_Already_Queued()
    {
        var service = new MdDialogService();
        var host = new MdDialogHost { Content = new TextBlock(), Service = service };
        host.DataTemplates.Add(new FuncDataTemplate<StepOne>((_, _) => new MdDialog { Headline = "One" }));
        host.DataTemplates.Add(new FuncDataTemplate<StepTwo>((_, _) => new MdDialog { Headline = "Two" }));
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            _ = ((IMdDialogService)service).ShowAsync(new StepOne());
            Dispatcher.UIThread.RunJobs();
            var queued = ((IMdDialogService)service).ShowAsync(new StepOne());
            Dispatcher.UIThread.RunJobs();

            _ = ((IMdDialogService)service).ReplaceAsync(new StepTwo());
            Dispatcher.UIThread.RunJobs();
            Assert.IsType<StepTwo>(host.Dialog);

            // The queued request is still waiting its turn rather than being dropped.
            Assert.False(queued.IsCompleted);
            host.Close();
            Dispatcher.UIThread.RunJobs();
            Assert.IsType<StepOne>(host.Dialog);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public async Task Replacing_With_Nothing_Displayed_Behaves_Like_Showing()
    {
        var service = new MdDialogService();
        var host = new MdDialogHost { Content = new TextBlock(), Service = service };
        host.DataTemplates.Add(new FuncDataTemplate<StepOne>((_, _) => new MdDialog { Headline = "One" }));
        var window = new Window { Width = 640, Height = 480, Content = host };
        window.Show();
        try
        {
            var pending = ((IMdDialogService)service).ReplaceAsync(new StepOne());
            Dispatcher.UIThread.RunJobs();
            Assert.True(host.IsOpen);
            Assert.IsType<StepOne>(host.Dialog);
            host.Close(7);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(7, await pending);
        }
        finally { window.Close(); }
    }

    // ---------------------------------------------------------------- popup window bleed

    [AvaloniaFact]
    public void Dropdown_Surfaces_Reserve_Room_For_Their_Shadow_And_Entrance_Offset()
    {
        // A popup is a real window on desktop and is sized to its child's layout box. The
        // elevation shadow and the entrance translate both draw outside that box, so on Windows
        // they were clipped by the window edge - the top of the menu came up cut off. Android
        // renders popups into the TopLevel overlay, which is why it looked fine there.
        var cascader = new MdCascader { Label = "Region" };
        var combo = new MdComboBox();
        var date = new MdDatePicker();
        var time = new MdTimePicker();
        var panel = new StackPanel { Children = { cascader, combo, date, time } };
        var window = new Window { Width = 640, Height = 640, Content = panel };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            foreach (var (control, part) in new (Control, string)[]
            {
                (cascader, "PART_Surface"), (combo, "PART_MenuSurface"),
                (date, "PART_Surface"), (time, "PART_Surface"),
            })
            {
                var surface = PopupChildren(control).FirstOrDefault(b => b.Name == part);
                Assert.True(surface is not null, $"{control.GetType().Name} has no {part}");
                Assert.True(surface!.Margin.Top >= 8,
                    $"{control.GetType().Name}.{part} reserves only {surface.Margin.Top} above itself; " +
                    "the 8px entrance offset and the top of the shadow need room inside the popup window.");
                Assert.True(surface.Margin.Bottom >= 8,
                    $"{control.GetType().Name}.{part} reserves only {surface.Margin.Bottom} below itself.");
            }
        }
        finally { window.Close(); }
    }

    private static IEnumerable<Border> PopupChildren(Control control) =>
        control.GetVisualDescendants().OfType<Popup>()
            .Select(p => p.Child)
            .OfType<Border>();

    // ---------------------------------------------------------------- cascader label

    [AvaloniaFact]
    public void Cascader_Keeps_Its_Floating_Label_Inside_Its_Own_Bounds()
    {
        // The label hung 7px above the anchor with nothing reserving that room, so the top of
        // the glyphs was outside the control's box and got clipped - it read as half a label.
        var cascader = new MdCascader { Label = "Region", Width = 320 };
        var window = new Window { Width = 480, Height = 320, Content = new StackPanel { Children = { cascader } } };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var label = Descendant<Border>(cascader, "PART_Label");
            Assert.True(label.IsVisible);

            var topLeft = label.TranslatePoint(new Point(0, 0), cascader);
            Assert.NotNull(topLeft);
            Assert.True(topLeft!.Value.Y >= 0,
                $"the label starts at y={topLeft.Value.Y} inside the cascader, so its top is outside the control.");
            Assert.True(topLeft.Value.Y + label.Bounds.Height <= cascader.Bounds.Height + .5);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Cascader_Drops_The_Reserved_Room_When_There_Is_No_Label()
    {
        var labelled = new MdCascader { Label = "Region", Width = 320 };
        var bare = new MdCascader { Label = null, Width = 320 };
        var window = new Window { Width = 480, Height = 400, Content = new StackPanel { Children = { labelled, bare } } };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.False(Descendant<Border>(bare, "PART_Label").IsVisible);
            Assert.Equal(0, Descendant<Grid>(bare, "PART_Fields").Margin.Top);
            Assert.True(Descendant<Grid>(labelled, "PART_Fields").Margin.Top > 0);
        }
        finally { window.Close(); }
    }

    // ---------------------------------------------------------------- comparison slider

    private static MdBeforeAfter CreateComparison() => new()
    {
        Width = 400,
        Height = 200,
        Before = new Border { Background = Brushes.Black },
        After = new Border { Background = Brushes.White },
    };

    [AvaloniaFact]
    public void Comparison_Keeps_Both_Layers_Registered_As_The_Divider_Moves()
    {
        // The revealed layer was sized to the whole control but left to centre itself inside the
        // narrow clip, so it slid sideways as the divider moved and the two images no longer
        // lined up - the comparison showed the wrong pixels against each other.
        var control = CreateComparison();
        var window = new Window { Width = 480, Height = 300, Content = control };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var clip = Descendant<Border>(control, "PART_AfterClip");
            var after = Assert.IsAssignableFrom<Control>(clip.Child);

            foreach (var position in new[] { .1, .5, .9 })
            {
                control.Position = position;
                Dispatcher.UIThread.RunJobs();
                var origin = after.TranslatePoint(new Point(0, 0), control);
                Assert.NotNull(origin);
                Assert.True(Math.Abs(origin!.Value.X) < .5,
                    $"at position {position} the revealed layer starts at x={origin.Value.X}; " +
                    "it has to stay pinned to the control's own origin to compare the same pixels.");
                Assert.True(Math.Abs(origin.Value.Y) < .5);
            }
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Comparison_Moves_By_Dragging_The_Handle_Not_By_Clicking_The_Image()
    {
        var control = CreateComparison();
        var window = new Window { Width = 480, Height = 300, Content = control };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var thumb = Descendant<Border>(control, "PART_Thumb");
            Assert.True(thumb.IsVisible);

            // The handle sits on the divider.
            var centre = thumb.TranslatePoint(new Point(thumb.Bounds.Width / 2, thumb.Bounds.Height / 2), control);
            Assert.NotNull(centre);
            Assert.True(Math.Abs(centre!.Value.X - control.Bounds.Width * .5) < 1.5,
                $"the handle is at x={centre.Value.X} but the divider is at {control.Bounds.Width * .5}.");

            // Grabbing it off-centre must not snap the divider under the pointer.
            var grab = thumb.TranslatePoint(new Point(4, thumb.Bounds.Height / 2), window);
            Assert.NotNull(grab);
            window.MouseMove(grab!.Value, RawInputModifiers.None);
            window.MouseDown(grab.Value, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(.5, control.Position, 3);
            Assert.Contains(":dragging", PseudoClassNames(control));

            window.MouseMove(grab.Value.WithX(grab.Value.X + 40), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(.5 + 40d / control.Bounds.Width, control.Position, 3);

            window.MouseUp(grab.Value.WithX(grab.Value.X + 40), MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.DoesNotContain(":dragging", PseudoClassNames(control));
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Comparison_Steps_Toward_A_Click_On_The_Image_Instead_Of_Teleporting()
    {
        // Clicking anywhere used to drop the divider on the pointer, so the image could not be
        // clicked without destroying the comparison the user had set up.
        var control = CreateComparison();
        var window = new Window { Width = 480, Height = 300, Content = control };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var far = control.TranslatePoint(new Point(control.Bounds.Width - 4, control.Bounds.Height / 2), window);
            Assert.NotNull(far);
            window.MouseMove(far!.Value, RawInputModifiers.None);
            window.MouseDown(far.Value, MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(far.Value, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.True(control.Position > .5, "a click toward the right edge should move the divider that way");
            Assert.True(control.Position < .75, $"but one step, not a jump to the pointer - got {control.Position}");
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Comparison_Handle_Is_Hidden_When_The_Control_Is_Display_Only()
    {
        var control = CreateComparison();
        control.IsInteractive = false;
        var window = new Window { Width = 480, Height = 300, Content = control };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.False(Descendant<Border>(control, "PART_Thumb").IsVisible);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Comparison_Grabs_The_Divider_When_It_Sits_At_An_Edge()
    {
        // The handle straddles the divider, so at position 0 (and 1) half of it falls outside the
        // control and is clipped away. A press aimed at it missed the visual and fell through to
        // the track-step branch, which shoved the divider away from the cursor instead of
        // grabbing it - the "pointer drifts far from the handle" report for top/left slider.
        foreach (var orientation in new[] { MdComparisonOrientation.Horizontal, MdComparisonOrientation.Vertical })
        {
            var control = CreateComparison();
            control.Orientation = orientation;
            control.Position = 0;
            var window = new Window { Width = 480, Height = 300, Content = control };
            window.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();
                var origin = control.TranslatePoint(new Point(0, 0), window);
                Assert.NotNull(origin);
                var start = origin!.Value + new Vector(4, 4);

                window.MouseDown(start, MouseButton.Left, RawInputModifiers.None);
                Dispatcher.UIThread.RunJobs();
                Assert.Contains(":dragging", PseudoClassNames(control));
                Assert.Equal(0, control.Position, 3);

                var travel = orientation == MdComparisonOrientation.Horizontal
                    ? new Vector(80, 0)
                    : new Vector(0, 40);
                window.MouseMove(start + travel, RawInputModifiers.None);
                Dispatcher.UIThread.RunJobs();
                var expected = orientation == MdComparisonOrientation.Horizontal
                    ? 80d / control.Bounds.Width
                    : 40d / control.Bounds.Height;
                Assert.Equal(expected, control.Position, 3);

                window.MouseUp(start + travel, MouseButton.Left, RawInputModifiers.None);
                Dispatcher.UIThread.RunJobs();
                Assert.DoesNotContain(":dragging", PseudoClassNames(control));
            }
            finally { window.Close(); }
        }
    }

    [AvaloniaFact]
    public void Time_Picker_Popup_Surface_Forwards_Wheel_And_Keys_To_The_Control()
    {
        // On desktop the popup lives in its own PopupRoot window, so an event raised inside it
        // never reaches the picker: the route ends at the popup root. The control-level
        // OnPointerWheelChanged override was therefore dead on Windows while it kept working on
        // Android, where popups render into the TopLevel overlay. The surface has to forward.
        var picker = new MdTimePicker { Hour = 10, Minute = 30, MinuteStep = 5 };
        var window = new Window { Width = 480, Height = 640, Content = picker };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            picker.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var popup = Descendant<Popup>(picker, "PART_Popup");
            var surface = Assert.IsAssignableFrom<InputElement>(popup.Child);

            surface.RaiseEvent(new PointerWheelEventArgs(
                surface, new Pointer(0, PointerType.Mouse, true), surface, default,
                0, new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other),
                KeyModifiers.None, new Vector(0, 1))
            { RoutedEvent = InputElement.PointerWheelChangedEvent });
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(35, picker.Minute);

            surface.RaiseEvent(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Key = Key.Escape,
                Source = surface,
            });
            Dispatcher.UIThread.RunJobs();
            Assert.False(picker.IsOpen, "Escape inside the popup has to reach the picker and close it.");
        }
        finally { window.Close(); }
    }

    private static IEnumerable<string> PseudoClassNames(StyledElement element) =>
        ((IEnumerable<string>)element.Classes).Where(c => c.StartsWith(':'));
}
