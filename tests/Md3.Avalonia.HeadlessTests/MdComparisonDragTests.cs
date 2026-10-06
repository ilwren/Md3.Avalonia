using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Extra.Controls;
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
}
