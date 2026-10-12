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
    public void First_Gallery_Drag_From_An_Edge_Tracks_The_Pointer_Before_Focus_Has_Been_Established()
    {
        // Exercise the real desktop gallery tree, not just an isolated control: its scroll host
        // and the first pointer-to-keyboard-focus transition are part of the first edge gesture.
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
                .Single(control => control.Orientation == MdComparisonOrientation.Vertical);
            var pageScroll = comparison.GetVisualAncestors().OfType<MdScrollViewer>().First();
            var extent = comparison.Bounds.Height;
            Assert.True(extent > 100, "The gallery comparison never got a desktop-sized layout.");
            Assert.False(comparison.IsKeyboardFocusWithin,
                "The regression must cover the first pointer press before the comparison has focus.");

            // Put the vertical comparison partly below the page viewport. On first focus the
            // ScrollViewer brings it into view, changing the pointer's local Y coordinate while
            // the mouse is down - exactly the one-time shift that a delta-only drag loses.
            var viewportBottom = pageScroll.TranslatePoint(
                new Point(0, pageScroll.Bounds.Height), window)!.Value.Y;
            var comparisonTop = comparison.TranslatePoint(default, window)!.Value.Y;
            var scrollDistance = comparisonTop - (viewportBottom - 20);
            if (scrollDistance > 0)
                pageScroll.Offset = new Vector(pageScroll.Offset.X, pageScroll.Offset.Y + scrollDistance);
            Dispatcher.UIThread.RunJobs();

            // Move to the top edge after the gallery has completed layout. This matches the
            // runtime geometry update before the first edge drag rather than initializing at 0.
            comparison.Position = 0;
            Dispatcher.UIThread.RunJobs();
            var startAlong = 18d;
            var startWindow = comparison.TranslatePoint(
                new Point(comparison.Bounds.Width / 2, startAlong), window)!.Value;
            var viewportTop = pageScroll.TranslatePoint(default, window)!.Value.Y;
            Assert.True(startWindow.Y >= viewportTop && startWindow.Y < viewportBottom,
                "The first edge press must land in the visible part of the scrolled comparison.");
            var topBeforeFocus = comparison.TranslatePoint(default, window)!.Value.Y;
            window.MouseDown(startWindow, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            var topAfterFocus = comparison.TranslatePoint(default, window)!.Value.Y;
            Assert.True(topAfterFocus < topBeforeFocus - 1,
                "Focusing the partly clipped comparison must scroll it into view so the first-drag regression is exercised.");

            var worstGap = 0d;
            var worstAt = 0d;
            foreach (var delta in new[] { -1d, -4d, -10d, -18d })
            {
                var pointerWindow = startWindow + new Vector(0, delta);
                window.MouseMove(pointerWindow, RawInputModifiers.None);
                Dispatcher.UIThread.RunJobs();

                var controlOrigin = comparison.TranslatePoint(default, window)!.Value;
                var pointerLocal = pointerWindow.Y - controlOrigin.Y;
                var gap = Math.Abs(comparison.Position * extent - pointerLocal);
                if (gap > worstGap) { worstGap = gap; worstAt = delta; }
            }

            var endWindow = startWindow + new Vector(0, -18);
            window.MouseUp(endWindow, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.True(comparison.IsKeyboardFocusWithin,
                "The pointer press should establish keyboard focus without shifting the first drag origin.");
            Assert.True(worstGap <= 1,
                $"the first gallery edge drag trailed the pointer by {worstGap:0.##} dip at " +
                $"{worstAt:0} (position {comparison.Position:0.###} of {extent:0}).");
        }
        finally { window.Close(); }
    }
}
