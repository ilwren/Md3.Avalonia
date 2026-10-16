using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Pages;
using Md3.Avalonia.Motion;
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
            var labelText = Descendant<TextBlock>(cascader, "PART_LabelText");
            Assert.True(label.IsVisible);
            Assert.True(labelText.IsVisible);

            var topLeft = label.TranslatePoint(new Point(0, 0), cascader);
            var textTopLeft = labelText.TranslatePoint(new Point(0, 0), cascader);
            Assert.NotNull(topLeft);
            Assert.NotNull(textTopLeft);
            Assert.True(topLeft!.Value.Y >= 8,
                $"the label starts at y={topLeft.Value.Y} inside the cascader, without enough room above it.");
            Assert.True(topLeft.Value.Y + label.Bounds.Height <= cascader.Bounds.Height + .5);
            Assert.True(textTopLeft!.Value.Y >= 0 && textTopLeft.Value.Y + labelText.Bounds.Height <= cascader.Bounds.Height + .5,
                $"the label text bounds {textTopLeft.Value.Y:0.##}..{textTopLeft.Value.Y + labelText.Bounds.Height:0.##} " +
                $"fall outside the cascader height {cascader.Bounds.Height:0.##}.");
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
            Assert.Equal(16, Descendant<Grid>(labelled, "PART_Fields").Margin.Top);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Cascader_Floating_Label_Is_Transparent_Over_A_Non_Surface_Background()
    {
        var accent = Color.FromRgb(54, 126, 154);
        var backgroundBrush = new SolidColorBrush(accent);
        var cascader = new MdCascader { Label = "Destination", Width = 320 };
        var backdrop = new Border
        {
            Background = backgroundBrush,
            Padding = new Thickness(24),
            Child = cascader,
        };
        var window = new Window { Width = 480, Height = 240, Background = backgroundBrush, Content = backdrop };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var label = Descendant<Border>(cascader, "PART_Label");
            var outline = Descendant<MdOutlinedFieldBorder>(cascader, "PART_AnchorOutline");
            var labelBrush = Assert.IsAssignableFrom<ISolidColorBrush>(label.Background);

            Assert.Equal((byte)0, labelBrush.Color.A);
            Assert.True(outline.IsNotched,
                "the outline should draw a real gap beneath the label instead of a fixed-colour patch");
            Assert.Same(label, outline.NotchTarget);
            Assert.Same(backgroundBrush, backdrop.Background);

            // Compare actual rendered pixels: the clear part of the notch must show the colored
            // host surface, not a baked-in light/dark Surface patch.
            using var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            var labelOrigin = label.TranslatePoint(default, window);
            var outlineOrigin = outline.TranslatePoint(default, window);
            var backdropOrigin = backdrop.TranslatePoint(default, window);
            Assert.NotNull(labelOrigin);
            Assert.NotNull(outlineOrigin);
            Assert.NotNull(backdropOrigin);
            var notchPixel = ReadPixel(frame!,
                new Point(labelOrigin!.Value.X + 1, outlineOrigin!.Value.Y + .5), window.RenderScaling);
            var hostPixel = ReadPixel(frame!,
                new Point(backdropOrigin!.Value.X + 6, backdropOrigin.Value.Y + 6), window.RenderScaling);
            Assert.Equal(hostPixel, notchPixel);
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
    public void Comparison_First_Arrange_Uses_Final_Size_For_Initial_Position()
    {
        // ArrangeOverride runs before Layoutable updates Bounds. The initial Position must be
        // reflected in the very first arranged handle, without waiting for a later value change.
        foreach (var orientation in new[]
                 {
                     MdComparisonOrientation.Horizontal,
                     MdComparisonOrientation.Vertical,
                 })
        {
            var control = new MdBeforeAfter
            {
                Orientation = orientation,
                Before = new Border { Width = 400, Height = 200, Background = Brushes.Black },
                After = new Border { Width = 400, Height = 200, Background = Brushes.White },
            };
            var window = new Window { Width = 480, Height = 300, Content = control };
            window.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(.5, control.Position);

                // Force a known old size, then arrange from zero to a different final size. This
                // makes the test independent of the window's initial layout pass while preserving
                // the same stale-Bounds ordering as the first template arrange.
                control.Measure(new Size(600, 300));
                control.Arrange(new Rect(0, 0, 600, 300));
                Assert.Equal(new Size(600, 300), control.Bounds.Size);
                control.Arrange(new Rect(0, 0, 0, 0));
                Assert.Equal(new Size(0, 0), control.Bounds.Size);
                control.Arrange(new Rect(0, 0, 400, 200));
                Assert.Equal(new Size(400, 200), control.Bounds.Size);

                var thumb = Descendant<Border>(control, "PART_Thumb");
                var centre = thumb.TranslatePoint(
                    new Point(thumb.Bounds.Width / 2, thumb.Bounds.Height / 2), control);
                Assert.NotNull(centre);
                var actual = orientation == MdComparisonOrientation.Horizontal
                    ? centre!.Value.X
                    : centre!.Value.Y;
                var extent = orientation == MdComparisonOrientation.Horizontal ? 400 : 200;
                var expected = extent * control.Position;
                Assert.True(Math.Abs(actual - expected) < 1.5,
                    $"{orientation}: the initial handle is at {actual} instead of Position .5 " +
                    $"in a {extent}-dip arrange.");
            }
            finally { window.Close(); }
        }
    }

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

                // The press snaps the divider under the pointer. Making the press a grab was
                // the first half of this fix; keeping the press offset for the rest of the drag
                // was the other half of the same report, because at an edge the only reachable
                // press is beside the clipped handle.
                var pressOffset = orientation == MdComparisonOrientation.Horizontal
                    ? 4d / control.Bounds.Width
                    : 4d / control.Bounds.Height;
                Assert.Equal(pressOffset, control.Position, 3);

                var travel = orientation == MdComparisonOrientation.Horizontal
                    ? new Vector(80, 0)
                    : new Vector(0, 40);
                window.MouseMove(start + travel, RawInputModifiers.None);
                Dispatcher.UIThread.RunJobs();
                var expected = orientation == MdComparisonOrientation.Horizontal
                    ? 84d / control.Bounds.Width
                    : 44d / control.Bounds.Height;
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

    [AvaloniaFact]
    public void Time_Picker_Field_Adjusts_On_The_Wheel_Once_Focused_And_Lets_The_Page_Scroll_Otherwise()
    {
        // The scroll-to-adjust gesture on the closed field was lost when the wheel handler was
        // gated on IsOpen. Nothing caught it because the only wheel coverage opened the popup
        // first, so it asserted the one state where the gesture still worked.
        var picker = new MdTimePicker { Hour = 10, Minute = 30, MinuteStep = 5 };
        var window = new Window { Width = 480, Height = 640, Content = picker };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.False(picker.IsOpen);

            var text = Descendant<TextBlock>(picker, "PART_DisplayText");
            Point At(double fraction) => text
                .TranslatePoint(new Point(text.Bounds.Width * fraction, text.Bounds.Height / 2), window)!
                .Value;

            // Unfocused the wheel has to pass straight through, or scrolling any page with a
            // picker on it would silently rewrite the time.
            window.MouseWheel(At(0.75), new Vector(0, 1), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(10, picker.Hour);
            Assert.Equal(30, picker.Minute);

            Descendant<Button>(picker, "PART_AnchorButton").Focus();
            Dispatcher.UIThread.RunJobs();
            Assert.True(picker.IsKeyboardFocusWithin);

            // Right of centre is the minute segment, and it steps by MinuteStep.
            window.MouseWheel(At(0.75), new Vector(0, 1), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(35, picker.Minute);
            Assert.Equal(10, picker.Hour);

            // Left of centre is the hour segment.
            window.MouseWheel(At(0.1), new Vector(0, 1), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(11, picker.Hour);
            Assert.Equal(35, picker.Minute);

            // Minutes carry into the hour rather than wrapping on their own.
            picker.Minute = 55;
            Dispatcher.UIThread.RunJobs();
            window.MouseWheel(At(0.75), new Vector(0, 1), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(0, picker.Minute);
            Assert.Equal(12, picker.Hour);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Picker_Popup_Surfaces_Line_Up_With_The_Bottom_Left_Of_Their_Anchor()
    {
        // Both picker popups pad their surface to leave room for the shadow and the entrance
        // translate, because a popup window is sized to its child's layout box and anything
        // painted outside it is clipped. That padding has to be cancelled by an equal negative
        // offset on the Popup, or the whole surface sits low and to the right of its anchor.
        // Measuring the surface against the anchor is what pins the two numbers together.
        foreach (var (name, picker, open) in new (string, Control, Action<Control>)[]
                 {
                     ("MdTimePicker", new MdTimePicker { Hour = 10, Minute = 30 },
                         c => ((MdTimePicker)c).IsOpen = true),
                     ("MdDatePicker", new MdDatePicker(),
                         c => ((MdDatePicker)c).IsOpen = true),
                 })
        {
            // Give the picker its natural size near the top left, the way a page lays one out.
            // Making it the window's only content stretches the anchor to the full 900x760, which
            // leaves no room below it and forces the popup into a corner - an artefact of the
            // harness that looks exactly like a placement bug.
            var window = new Window
            {
                Width = 900,
                Height = 760,
                Content = new StackPanel
                {
                    Margin = new Thickness(24),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Children = { picker },
                },
            };
            window.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();
                open(picker);
                Dispatcher.UIThread.RunJobs();

                // The surface is the popup's child, not a visual descendant of the picker.
                var popup = Descendant<Popup>(picker, "PART_Popup");
                var surface = Assert.IsAssignableFrom<Border>(popup.Child);
                Assert.Equal("PART_Surface", surface.Name);
                PointerInput.AssertReachable(surface);

                var anchor = Descendant<Button>(picker, "PART_AnchorButton");
                var anchorBottomLeft = anchor
                    .TranslatePoint(new Point(0, anchor.Bounds.Height), window)!.Value;
                var surfaceTopLeft = surface.TranslatePoint(default, window)!.Value;

                var chain = string.Join(" < ", surface.GetSelfAndVisualAncestors()
                    .OfType<Visual>()
                    .Select(v => $"{v.GetType().Name}[{v.Bounds.X:0.#},{v.Bounds.Y:0.#} " +
                                 $"{v.Bounds.Width:0.#}x{v.Bounds.Height:0.#}]"));

                Assert.True(Math.Abs(surfaceTopLeft.X - anchorBottomLeft.X) <= 1,
                    $"{name} popup is offset horizontally by " +
                    $"{surfaceTopLeft.X - anchorBottomLeft.X:0.##} dip: surface starts at " +
                    $"{surfaceTopLeft.X:0.##}, anchor at {anchorBottomLeft.X:0.##}, " +
                    $"anchor box {anchor.Bounds}; chain: {chain}");
                Assert.True(Math.Abs(surfaceTopLeft.Y - anchorBottomLeft.Y) <= 1,
                    $"{name} popup is offset vertically by " +
                    $"{surfaceTopLeft.Y - anchorBottomLeft.Y:0.##} dip: surface starts at " +
                    $"{surfaceTopLeft.Y:0.##}, anchor bottom at {anchorBottomLeft.Y:0.##}");
            }
            finally { window.Close(); }
        }
    }

    [AvaloniaFact]
    public void Cascader_Dropdown_Lands_Below_Its_Anchor_And_Fits_On_The_Real_Page()
    {
        // Measured on the real page, because the reported truncation is about where the dropdown
        // lands in a scrolling page rather than about the control in isolation.
        // Hosted in MainWindow rather than shown bare so the test includes the desktop shell,
        // scrolling page, and the actual Cascader gallery data. The popup is forced into the
        // owner window's overlay layer because native desktop popup windows clipped this surface.
        var window = new MainWindow { Width = 1400, Height = 900 };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.GetVisualDescendants().OfType<MdButton>()
                .Single(button => button.Name == "CascaderNav")
                .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            var cascader = window.GetVisualDescendants().OfType<MdCascader>().First();
            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var popup = Descendant<Popup>(cascader, "PART_Popup");
            Assert.True(popup.ShouldUseOverlayLayer,
                "The Cascader dropdown must use the owner window's overlay layer rather than a clipped desktop popup window.");
            var surface = Assert.IsAssignableFrom<Border>(popup.Child);
            Assert.True(surface.IsVisible);
            PointerInput.AssertReachable(surface);

            var anchor = Descendant<Button>(cascader, "PART_Anchor");
            var anchorBottomLeft = anchor
                .TranslatePoint(new Point(0, anchor.Bounds.Height), window)!.Value;
            var topLeft = surface.TranslatePoint(default, window)!.Value;
            var chain = string.Join(" < ", surface.GetSelfAndVisualAncestors().OfType<Visual>()
                .Select(v => $"{v.GetType().Name}[{v.Bounds.X:0.#},{v.Bounds.Y:0.#} " +
                             $"{v.Bounds.Width:0.#}x{v.Bounds.Height:0.#}]"));

            Assert.True(Math.Abs(topLeft.X - anchorBottomLeft.X) <= 1,
                $"dropdown is offset horizontally by {topLeft.X - anchorBottomLeft.X:0.##} dip " +
                $"(surface {topLeft.X:0.##}, anchor {anchorBottomLeft.X:0.##}); chain: {chain}");

            Assert.True(topLeft.Y >= -0.5,
                $"dropdown top is cut off above the window edge at y={topLeft.Y:0.##}; chain: {chain}");

            Assert.True(Math.Abs(topLeft.Y - anchorBottomLeft.Y) <= 1,
                $"dropdown is offset vertically by {topLeft.Y - anchorBottomLeft.Y:0.##} dip " +
                $"(surface {topLeft.Y:0.##}, anchor bottom {anchorBottomLeft.Y:0.##}); chain: {chain}");

            Assert.True(topLeft.Y + surface.Bounds.Height <= window.Height + 0.5,
                $"dropdown runs {topLeft.Y + surface.Bounds.Height - window.Height:0.##} dip past " +
                $"the bottom of the window; chain: {chain}");
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Cascader_First_Open_Is_Aligned_And_Clicking_A_Path_Updates_The_Visible_Field()
    {
        // Exercise the actual Gallery tree and pointer selection, not just MdCascader.Select:
        // the user-visible defect was that the status line updated while the anchor stayed blank.
        var window = new MainWindow { Width = 1400, Height = 900 };
        MdMotion.SetScheme(window, MdMotionScheme.None);
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.GetVisualDescendants().OfType<MdButton>()
                .Single(button => button.Name == "CascaderNav")
                .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            var cascader = window.GetVisualDescendants().OfType<MdCascader>().First();
            var rootItems = Assert.IsAssignableFrom<IEnumerable<MdCascaderItem>>(cascader.ItemsSource).ToArray();
            var design = rootItems.Single(item => Equals(item.Value, "design"));
            var material = design.Children!.Single(item => Equals(item.Value, "material"));
            var controls = material.Children!.Single(item => Equals(item.Value, "controls"));
            MdMotion.SetScheme(cascader, MdMotionScheme.None);

            var displayText = Descendant<TextBlock>(cascader, "PART_DisplayText");
            Assert.Equal(cascader.PlaceholderText, displayText.Text);
            var displayOrigin = displayText.TranslatePoint(default, window);
            Assert.NotNull(displayOrigin);
            var displayBounds = new Rect(displayOrigin!.Value, displayText.Bounds.Size);
            using var initialFieldFrame = window.CaptureRenderedFrame();
            Assert.NotNull(initialFieldFrame);

            // Check the very first open frame before interacting with the popup. The surface
            // must be visible, hittable, below the field, and inside the real Gallery window.
            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();
            var popup = Descendant<Popup>(cascader, "PART_Popup");
            var surface = Assert.IsAssignableFrom<Border>(popup.Child);
            Assert.True(popup.IsUsingOverlayLayer);
            Assert.True(surface.IsVisible);
            PointerInput.AssertReachable(surface);

            var anchor = Descendant<Button>(cascader, "PART_Anchor");
            var anchorBottomLeft = anchor.TranslatePoint(new Point(0, anchor.Bounds.Height), window);
            var surfaceTopLeft = surface.TranslatePoint(default, window);
            Assert.NotNull(anchorBottomLeft);
            Assert.NotNull(surfaceTopLeft);
            Assert.InRange(Math.Abs(surfaceTopLeft!.Value.X - anchorBottomLeft!.Value.X), 0, 1);
            Assert.InRange(Math.Abs(surfaceTopLeft.Value.Y - anchorBottomLeft.Value.Y), 0, 1);
            Assert.True(surfaceTopLeft.Value.X >= 0 &&
                        surfaceTopLeft.Value.X + surface.Bounds.Width <= window.ClientSize.Width + .5,
                "the newly opened Cascader surface should not be clipped by either horizontal window edge");
            Assert.True(surfaceTopLeft.Value.Y + surface.Bounds.Height <= window.ClientSize.Height + .5,
                "the newly opened Cascader surface should fit in the owner window on its first frame");
            Assert.Single(surface.GetVisualDescendants().OfType<ListBox>());

            void ClickItem(int column, MdCascaderItem item)
            {
                Dispatcher.UIThread.RunJobs();
                var lists = surface.GetVisualDescendants().OfType<ListBox>()
                    .OrderBy(list => list.TranslatePoint(default, window)!.Value.X)
                    .ToArray();
                Assert.True(column < lists.Length, $"expected Cascader column {column}, got {lists.Length}");
                var index = cascader.Columns[column].ToList().FindIndex(candidate => ReferenceEquals(candidate, item));
                Assert.True(index >= 0, $"'{item.Label}' is not in Cascader column {column}");
                var row = Assert.IsAssignableFrom<Control>(lists[column].ContainerFromIndex(index));
                PointerInput.AssertReachable(row);
                var point = row.TranslatePoint(new Point(row.Bounds.Width / 2, row.Bounds.Height / 2), window);
                Assert.NotNull(point);
                window.MouseMove(point!.Value, RawInputModifiers.None);
                window.MouseDown(point.Value, MouseButton.Left, RawInputModifiers.None);
                window.MouseUp(point.Value, MouseButton.Left, RawInputModifiers.None);
                Dispatcher.UIThread.RunJobs();
            }

            ClickItem(0, design);
            Assert.Equal(2, cascader.Columns.Count);
            Assert.Equal(design.Label, displayText.Text);
            Assert.Equal(design.Label, cascader.DisplayText);

            ClickItem(1, material);
            Assert.Equal(3, cascader.Columns.Count);
            Assert.Equal(string.Join(cascader.PathSeparator, design.Label, material.Label), displayText.Text);
            Assert.True(surface.Bounds.Width >= 3 * 160,
                "the expanded path should expose all three columns instead of remaining under the anchor");

            ClickItem(2, controls);
            var expectedPath = string.Join(cascader.PathSeparator, design.Label, material.Label, controls.Label);
            Assert.False(cascader.IsDropDownOpen);
            Assert.Equal(expectedPath, cascader.DisplayText);
            Assert.Equal(expectedPath, displayText.Text);
            Assert.Equal(new[] { design, material, controls }, cascader.SelectedPath);
            var status = window.GetVisualDescendants().OfType<TextBlock>()
                .Single(text => text.Name == "CascaderStatus");
            Assert.Contains(controls.Label, status.Text);

            using var selectedFieldFrame = window.CaptureRenderedFrame();
            Assert.NotNull(selectedFieldFrame);
            var changedFieldPixels = CountChangedPixels(
                initialFieldFrame!, selectedFieldFrame!, displayBounds, window.RenderScaling);
            Assert.True(changedFieldPixels > 12,
                $"only {changedFieldPixels} pixels changed inside the Cascader field text bounds; " +
                "the selected path may not have been painted in the control itself.");
        }
        finally { window.Close(); }
    }

    private static uint ReadPixel(Bitmap frame, Point logicalPoint, double scaling)
    {
        var pixels = ReadPixels(frame, out var size);
        var x = Math.Clamp((int)Math.Floor(logicalPoint.X * scaling), 0, size.Width - 1);
        var y = Math.Clamp((int)Math.Floor(logicalPoint.Y * scaling), 0, size.Height - 1);
        return pixels[y * size.Width + x];
    }

    private static int CountChangedPixels(Bitmap before, Bitmap after, Rect logicalBounds, double scaling)
    {
        var beforePixels = ReadPixels(before, out var beforeSize);
        var afterPixels = ReadPixels(after, out var afterSize);
        if (beforeSize.Width != afterSize.Width || beforeSize.Height != afterSize.Height)
            throw new InvalidOperationException("Rendered frames have different pixel sizes.");

        var left = Math.Clamp((int)Math.Floor(logicalBounds.Left * scaling), 0, beforeSize.Width);
        var top = Math.Clamp((int)Math.Floor(logicalBounds.Top * scaling), 0, beforeSize.Height);
        var right = Math.Clamp((int)Math.Ceiling(logicalBounds.Right * scaling), left, beforeSize.Width);
        var bottom = Math.Clamp((int)Math.Ceiling(logicalBounds.Bottom * scaling), top, beforeSize.Height);
        var changed = 0;
        for (var y = top; y < bottom; y++)
        for (var x = left; x < right; x++)
        {
            var index = y * beforeSize.Width + x;
            if (beforePixels[index] != afterPixels[index]) changed++;
        }

        return changed;
    }

    private static uint[] ReadPixels(Bitmap frame, out PixelSize size)
    {
        size = frame.PixelSize;
        var pixels = new uint[size.Width * size.Height];
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            frame.CopyPixels(
                new PixelRect(0, 0, size.Width, size.Height),
                handle.AddrOfPinnedObject(),
                pixels.Length * sizeof(uint),
                size.Width * sizeof(uint));
        }
        finally
        {
            handle.Free();
        }

        return pixels;
    }

    private static IEnumerable<string> PseudoClassNames(StyledElement element) =>
        ((IEnumerable<string>)element.Classes).Where(c => c.StartsWith(':'));
}
