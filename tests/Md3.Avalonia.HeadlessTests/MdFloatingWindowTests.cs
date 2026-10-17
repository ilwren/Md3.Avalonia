using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Motion;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// A floating window's defining behaviours are all things a property dump cannot show: whether a
/// drag actually moves it, whether it stops at the edge of its host instead of vanishing off it,
/// and whether minimizing leaves the hosted content mounted. Each of those would look identical in
/// a control that never wired its template parts up at all.
/// </summary>
public sealed class MdFloatingWindowTests
{
    [AvaloniaFact]
    public void Window_Moves_By_How_Far_The_Title_Bar_Was_Dragged()
    {
        var window = new MdFloatingWindow { Width = 280, Height = 180, X = 16, Y = 16, Title = "Now playing" };
        var host = new Grid { Width = 600, Height = 400, Children = { window } };
        using var scope = Show(host, 600, 400);

        var start = CenterOf(Part<Control>(window, "PART_TitleBar"), scope.Window);
        scope.Window.MouseMove(start, RawInputModifiers.None);
        scope.Window.MouseDown(start, MouseButton.Left, RawInputModifiers.None);
        scope.Window.MouseMove(start.WithX(start.X + 40).WithY(start.Y + 24), RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(56, window.X, 3);
        Assert.Equal(40, window.Y, 3);
        // The margin is how the position is realised, so it has to agree with X and Y.
        Assert.Equal(56, window.Margin.Left, 3);
        Assert.Equal(40, window.Margin.Top, 3);
        Assert.Contains(":dragging", window.Classes);

        scope.Window.MouseUp(start.WithX(start.X + 40).WithY(start.Y + 24), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.DoesNotContain(":dragging", window.Classes);
    }

    [AvaloniaFact]
    public void Window_Stays_Inside_Its_Host_When_Dragged_Past_Either_Edge()
    {
        var window = new MdFloatingWindow { Width = 280, Height = 180, X = 16, Y = 16 };
        var host = new Grid { Width = 420, Height = 320, Children = { window } };
        using var scope = Show(host, 420, 320);
        var titleBar = Part<Control>(window, "PART_TitleBar");

        // Deltas are derived from the measured geometry so the drag always overshoots the clamp
        // while the pointer itself stays over the window: input from outside a headless window is
        // not something this test should be betting on.
        DragTo(titleBar, scope.Window, host.Bounds.Width - 8, host.Bounds.Height - 8);
        Dispatcher.UIThread.RunJobs();

        // Measured against the host rather than a literal, so the assertion still holds if the
        // headless client area is not exactly the requested size.
        Assert.Equal(host.Bounds.Width - window.Bounds.Width, window.X, 3);
        Assert.Equal(host.Bounds.Height - window.Bounds.Height, window.Y, 3);

        DragTo(titleBar, scope.Window, 8, 8);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(0, window.X, 3);
        Assert.Equal(0, window.Y, 3);
    }

    [AvaloniaFact]
    public void Window_Does_Not_Move_When_Dragging_Is_Switched_Off()
    {
        var window = new MdFloatingWindow { Width = 280, Height = 180, X = 16, Y = 16, CanDrag = false };
        var host = new Grid { Width = 600, Height = 400, Children = { window } };
        using var scope = Show(host, 600, 400);

        Drag(Part<Control>(window, "PART_TitleBar"), scope.Window, 80, 60);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(16, window.X, 3);
        Assert.Equal(16, window.Y, 3);
        Assert.DoesNotContain(":dragging", window.Classes);
    }

    [AvaloniaFact]
    public void Minimizing_Hides_The_Content_But_Leaves_It_Mounted()
    {
        var player = new TextBlock { Text = "player" };
        var window = new MdFloatingWindow { Width = 280, Height = 200, Content = player };
        var host = new Grid { Width = 600, Height = 400, Children = { window } };
        using var scope = Show(host, 600, 400);
        var contentHost = Part<Control>(window, "PART_ContentHost");
        Assert.True(contentHost.IsVisible);

        window.IsMinimized = true;
        Dispatcher.UIThread.RunJobs();

        Assert.False(contentHost.IsVisible);
        Assert.Contains(":minimized", window.Classes);
        // The point of the collapse: nothing was torn down, so a hosted player keeps its state.
        Assert.Same(player, window.GetVisualDescendants().OfType<TextBlock>().Single());
        // An explicit height would leave a tall empty box, so the title bar takes over sizing.
        Assert.True(double.IsNaN(window.Height));
    }

    [AvaloniaFact]
    public void Restoring_Returns_The_Height_The_Window_Had_Before_It_Collapsed()
    {
        var window = new MdFloatingWindow { Width = 280, Height = 200, Content = new TextBlock { Text = "player" } };
        var host = new Grid { Width = 600, Height = 400, Children = { window } };
        using var scope = Show(host, 600, 400);

        window.IsMinimized = true;
        Dispatcher.UIThread.RunJobs();
        window.IsMinimized = false;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(200, window.Height, 3);
        Assert.True(Part<Control>(window, "PART_ContentHost").IsVisible);
        Assert.DoesNotContain(":minimized", window.Classes);
    }

    [AvaloniaFact]
    public void The_Grip_Resizes_The_Window_And_Stops_At_Its_Minimum()
    {
        var window = new MdFloatingWindow
        {
            Width = 280, Height = 180, MinWidth = 200, MinHeight = 120
        };
        var host = new Grid { Width = 600, Height = 400, Children = { window } };
        using var scope = Show(host, 600, 400);
        var grip = Part<Control>(window, "PART_ResizeGrip");

        Drag(grip, scope.Window, 60, 40);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(340, window.Width, 3);
        Assert.Equal(220, window.Height, 3);

        // Shrinking drags towards the top-left corner, which is as far as the pointer can go and
        // still be over the window - far enough that only the minimums stop it.
        DragTo(grip, scope.Window, 8, 8);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(200, window.Width, 3);
        Assert.Equal(120, window.Height, 3);
    }

    [AvaloniaFact]
    public void Closing_Hides_The_Window_And_Raises_Closed()
    {
        var window = new MdFloatingWindow { Width = 280, Height = 180, Title = "Inspector" };
        // None removes the exit hold, so presence drops on the click rather than after a timer.
        MdMotion.SetScheme(window, MdMotionScheme.None);
        var host = new Grid { Width = 600, Height = 400, Children = { window } };
        using var scope = Show(host, 600, 400);

        var closed = 0;
        window.Closed += (_, _) => closed++;
        Assert.True(window.IsVisible);

        var close = Part<Button>(window, "PART_CloseButton");
        close.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, close));
        Dispatcher.UIThread.RunJobs();

        Assert.False(window.IsOpen);
        Assert.False(window.IsHitTestVisible);
        Assert.False(window.IsVisible);
        Assert.Equal(1, closed);
    }

    [AvaloniaFact]
    public void The_Minimize_Button_Collapses_And_Expands_The_Window()
    {
        var window = new MdFloatingWindow { Width = 280, Height = 200, Content = new TextBlock { Text = "player" } };
        var host = new Grid { Width = 600, Height = 400, Children = { window } };
        using var scope = Show(host, 600, 400);
        var button = Part<Button>(window, "PART_MinimizeButton");

        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, button));
        Dispatcher.UIThread.RunJobs();
        Assert.True(window.IsMinimized);

        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, button));
        Dispatcher.UIThread.RunJobs();
        Assert.False(window.IsMinimized);
        Assert.Equal(200, window.Height, 3);
    }

    [AvaloniaFact]
    public void The_Surface_Takes_Its_Elevation_And_Corner_From_Material_Tokens()
    {
        var window = new MdFloatingWindow { Width = 280, Height = 180 };
        var host = new Grid { Width = 600, Height = 400, Children = { window } };
        using var scope = Show(host, 600, 400);
        var root = Part<Border>(window, "PART_Root");

        Assert.Equal(TokenBoxShadow(window, "Md.Sys.Elevation.Level3"), root.BoxShadow);
        Assert.Equal(TokenCornerRadius(window, "Md.Sys.Shape.Corner.ExtraLarge"), root.CornerRadius);
    }

    [AvaloniaFact]
    public void Dragging_Raises_The_Surface_To_A_Higher_Elevation()
    {
        var window = new MdFloatingWindow { Width = 280, Height = 180 };
        var host = new Grid { Width = 600, Height = 400, Children = { window } };
        using var scope = Show(host, 600, 400);
        var root = Part<Border>(window, "PART_Root");
        var titleBar = Part<Control>(window, "PART_TitleBar");

        var start = CenterOf(titleBar, scope.Window);
        scope.Window.MouseMove(start, RawInputModifiers.None);
        scope.Window.MouseDown(start, MouseButton.Left, RawInputModifiers.None);
        scope.Window.MouseMove(start.WithX(start.X + 24), RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();

        // A surface that moves is a surface the pointer is holding, and Material raises it while
        // it is held - the same step up a dragged card gets.
        Assert.Equal(TokenBoxShadow(window, "Md.Sys.Elevation.Level5"), root.BoxShadow);

        scope.Window.MouseUp(start.WithX(start.X + 24), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(TokenBoxShadow(window, "Md.Sys.Elevation.Level3"), root.BoxShadow);
    }

    [AvaloniaFact]
    public void The_Header_Buttons_Follow_The_Capabilities_The_Host_Asked_For()
    {
        var bare = new MdFloatingWindow { Width = 280, Height = 180, CanMinimize = false, CanClose = false, CanResize = false };
        var host = new Grid { Width = 600, Height = 400, Children = { bare } };
        using var scope = Show(host, 600, 400);

        Assert.False(Part<Button>(bare, "PART_MinimizeButton").IsVisible);
        Assert.False(Part<Button>(bare, "PART_CloseButton").IsVisible);
        Assert.False(Part<Control>(bare, "PART_ResizeGrip").IsVisible);
    }

    /// <summary>Drags a template part so the pointer ends at a given point in window coordinates.</summary>
    private static void DragTo(Control part, Window window, double toX, double toY)
    {
        var start = CenterOf(part, window);
        Drag(part, window, toX - start.X, toY - start.Y);
    }

    /// <summary>Drags a template part by a delta, pressing and releasing as a pointer would.</summary>
    private static void Drag(Control part, Window window, double deltaX, double deltaY)
    {
        var start = CenterOf(part, window);
        window.MouseMove(start, RawInputModifiers.None);
        window.MouseDown(start, MouseButton.Left, RawInputModifiers.None);
        window.MouseMove(start.WithX(start.X + deltaX).WithY(start.Y + deltaY), RawInputModifiers.LeftMouseButton);
        window.MouseUp(start.WithX(start.X + deltaX).WithY(start.Y + deltaY), MouseButton.Left, RawInputModifiers.None);
    }

    private static Point CenterOf(Control part, Window window) =>
        part.TranslatePoint(new Point(part.Bounds.Width / 2, part.Bounds.Height / 2), window)!.Value;

    private static T Part<T>(Control owner, string name) where T : Control =>
        owner.GetVisualDescendants().OfType<T>().Single(control => control.Name == name);

    private static BoxShadows TokenBoxShadow(Control scope, string key)
    {
        Assert.True(scope.TryFindResource(key, scope.ActualThemeVariant, out var value), key);
        return Assert.IsType<BoxShadows>(value);
    }

    private static CornerRadius TokenCornerRadius(Control scope, string key)
    {
        Assert.True(scope.TryFindResource(key, scope.ActualThemeVariant, out var value), key);
        return Assert.IsType<CornerRadius>(value);
    }

    private static WindowScope Show(Control content, double width, double height)
    {
        MdMotion.SetScheme(content, MdMotionScheme.None);
        var window = new Window { Width = width, Height = height, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowScope(window);
    }

    private sealed class WindowScope(Window window) : IDisposable
    {
        public Window Window => window;
        public void Dispose() => window.Close();
    }
}
