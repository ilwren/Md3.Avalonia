using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Gallery;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>The comparison divider has to stay under the pointer that is dragging it.</summary>
public sealed class MdComparisonDragTests
{
    [AvaloniaFact]
    public void Divider_Tracks_The_Pointer_When_Dragged_From_The_Start_Edge()
    {
        // Reported as a large gap between cursor and handle when the divider sits at the top or
        // the left. At those positions half the handle is clipped outside the control, so a
        // press lands on the track beside it rather than on the handle, and the grab used to
        // keep that offset for the rest of the drag.
        foreach (var orientation in new[]
                 {
                     MdComparisonOrientation.Horizontal,
                     MdComparisonOrientation.Vertical,
                 })
        {
            var comparison = new MdBeforeAfter
            {
                Orientation = orientation,
                Position = 0,
                Width = 400,
                Height = 300,
                Before = new Border(),
                After = new Border(),
            };
            var window = new Window { Width = 600, Height = 500, Content = comparison };
            window.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();
                var extent = orientation == MdComparisonOrientation.Horizontal
                    ? comparison.Bounds.Width
                    : comparison.Bounds.Height;
                Assert.True(extent > 100, $"{orientation}: control never got a size");

                Point At(double along) => comparison.TranslatePoint(
                    orientation == MdComparisonOrientation.Horizontal
                        ? new Point(along, comparison.Bounds.Height / 2)
                        : new Point(comparison.Bounds.Width / 2, along),
                    window)!.Value;

                // Press beside the clipped handle, inside the grab radius.
                window.MouseDown(At(18), MouseButton.Left, RawInputModifiers.None);
                Dispatcher.UIThread.RunJobs();

                var worstGap = 0d;
                var worstAt = 0d;
                foreach (var along in new[] { 40d, 90d, 150d, 220d })
                {
                    window.MouseMove(At(along), RawInputModifiers.None);
                    Dispatcher.UIThread.RunJobs();

                    var dividerAt = comparison.Position * extent;
                    var gap = Math.Abs(dividerAt - along);
                    if (gap > worstGap) { worstGap = gap; worstAt = along; }
                }

                window.MouseUp(At(220), MouseButton.Left, RawInputModifiers.None);
                Dispatcher.UIThread.RunJobs();

                Assert.True(worstGap <= 1,
                    $"{orientation}: the divider trailed the pointer by {worstGap:0.##} dip at " +
                    $"{worstAt:0} (position {comparison.Position:0.###} of {extent:0})");
            }
            finally { window.Close(); }
        }
    }

    [AvaloniaFact]
    public void First_Gallery_Drag_From_Left_And_Top_Edges_Does_Not_Focus_Or_Scroll_During_Drag()
    {
        foreach (var orientation in new[]
                 {
                     MdComparisonOrientation.Horizontal,
                     MdComparisonOrientation.Vertical,
                 })
        {
            var window = new MainWindow { Width = 1400, Height = 900 };
            window.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();
                window.GetVisualDescendants().OfType<MdButton>()
                    .Single(button => button.Name == "BeforeAfterNav")
                    .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Dispatcher.UIThread.RunJobs();

                var comparison = window.GetVisualDescendants().OfType<MdBeforeAfter>()
                    .Single(control => control.IsInteractive && control.Orientation == orientation);
                var pageScroll = comparison.GetVisualAncestors().OfType<MdScrollViewer>().First();
                var extent = orientation == MdComparisonOrientation.Horizontal
                    ? comparison.Bounds.Width
                    : comparison.Bounds.Height;
                Assert.True(extent > 100, $"{orientation}: the comparison never got a desktop-sized layout.");
                Assert.False(comparison.IsKeyboardFocusWithin,
                    $"{orientation}: the first pointer interaction must start unfocused.");

                // Put the left/top handle just inside the page viewport while the control itself
                // is partly clipped. Focusing it here would make the scroll host move the control
                // underneath the stationary mouse, which is the one-time first-drag failure.
                var viewportTop = pageScroll.TranslatePoint(default, window)!.Value.Y;
                var viewportBottom = pageScroll.TranslatePoint(
                    new Point(0, pageScroll.Bounds.Height), window)!.Value.Y;
                var controlTop = comparison.TranslatePoint(default, window)!.Value.Y;
                var desiredTop = orientation == MdComparisonOrientation.Horizontal
                    ? viewportTop + 20 - comparison.Bounds.Height / 2
                    : viewportBottom - 20;
                var maxOffset = Math.Max(0, pageScroll.Extent.Height - pageScroll.Viewport.Height);
                var offsetY = Math.Clamp(pageScroll.Offset.Y + controlTop - desiredTop, 0, maxOffset);
                pageScroll.Offset = new Vector(pageScroll.Offset.X, offsetY);
                comparison.Position = 0;
                Dispatcher.UIThread.RunJobs();

                controlTop = comparison.TranslatePoint(default, window)!.Value.Y;
                var startLocal = orientation == MdComparisonOrientation.Horizontal
                    ? new Point(18, comparison.Bounds.Height / 2)
                    : new Point(comparison.Bounds.Width / 2, 18);
                var startWindow = comparison.TranslatePoint(startLocal, window)!.Value;
                if (orientation == MdComparisonOrientation.Horizontal)
                    Assert.True(controlTop < viewportTop,
                        "the horizontal comparison must be partly clipped at the top of the viewport");
                else
                    Assert.True(controlTop + comparison.Bounds.Height > viewportBottom,
                        "the vertical comparison must be partly clipped at the bottom of the viewport");
                Assert.InRange(startWindow.Y, viewportTop, viewportBottom - 1);

                var scrollBeforePress = pageScroll.Offset;
                window.MouseDown(startWindow, MouseButton.Left, RawInputModifiers.None);
                Dispatcher.UIThread.RunJobs();

                Assert.False(comparison.IsKeyboardFocusWithin,
                    $"{orientation}: pointer-down must not trigger first-focus scrolling during the drag.");
                Assert.Equal(scrollBeforePress, pageScroll.Offset);
                Assert.InRange(comparison.Position, 0, .1);

                var axis = orientation == MdComparisonOrientation.Horizontal
                    ? new Vector(1, 0)
                    : new Vector(0, 1);
                var lastWindowPoint = startWindow;
                var worstGap = 0d;
                foreach (var delta in new[] { -1d, -4d, -10d, -18d })
                {
                    lastWindowPoint = startWindow + axis * delta;
                    window.MouseMove(lastWindowPoint, RawInputModifiers.None);
                    Dispatcher.UIThread.RunJobs();

                    var currentOrigin = comparison.TranslatePoint(default, window)!.Value;
                    var pointerLocal = lastWindowPoint - currentOrigin;
                    var pointerAlong = orientation == MdComparisonOrientation.Horizontal
                        ? pointerLocal.X
                        : pointerLocal.Y;
                    var gap = Math.Abs(comparison.Position * extent - pointerAlong);
                    worstGap = Math.Max(worstGap, gap);
                    Assert.Equal(scrollBeforePress, pageScroll.Offset);
                }

                window.MouseUp(lastWindowPoint, MouseButton.Left, RawInputModifiers.None);
                Dispatcher.UIThread.RunJobs();

                Assert.True(comparison.IsKeyboardFocusWithin,
                    $"{orientation}: keyboard focus should be assigned after pointer tracking ends.");
                Assert.Equal(scrollBeforePress, pageScroll.Offset);
                Assert.True(worstGap <= 1,
                    $"{orientation}: the first edge drag left the divider {worstGap:0.##} dip from the pointer.");
                Assert.Equal(0, comparison.Position, 3);
            }
            finally { window.Close(); }
        }
    }
}
