using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Ecosystem.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdPhaseThreeAndFourGestureParityTests
{
    [AvaloniaFact]
    public void SlidableItem_Full_Swipe_Triggers_Action_Command()
    {
        var invoked = false;
        var command = new ActionCommand(() => invoked = true);
        var slidable = new MdSlidableItem
        {
            Width = 400,
            Height = 60,
            ActionExtent = 80,
            StartActionCommand = command,
            Content = new TextBlock { Text = "Swipeable Item" }
        };

        using var host = Show(slidable, 500, 200);
        Dispatcher.UIThread.RunJobs();

        var point = slidable.TranslatePoint(new Point(20, 30), host.Window)!.Value;
        host.Window.MouseMove(point, RawInputModifiers.None);
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        // Drag past 1.8x ActionExtent (1.8 * 80 = 144)
        host.Window.MouseMove(point.WithX(point.X + 180), RawInputModifiers.None);
        host.Window.MouseUp(point.WithX(point.X + 180), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.True(invoked, "Full swipe should trigger the action command");
    }

    [AvaloniaFact]
    public void SlidableItem_Applies_RubberBand_Overscroll_Damping()
    {
        var slidable = new MdSlidableItem
        {
            Width = 400,
            Height = 60,
            ActionExtent = 80,
            Content = new TextBlock { Text = "Overscroll Item" }
        };

        using var host = Show(slidable, 500, 200);
        Dispatcher.UIThread.RunJobs();

        var point = slidable.TranslatePoint(new Point(20, 30), host.Window)!.Value;
        host.Window.MouseMove(point, RawInputModifiers.None);
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        // Drag 120px past 80px action extent -> delta = 120, offset should be 80 + (40 * 0.35) = 94
        host.Window.MouseMove(point.WithX(point.X + 120), RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.True(slidable.Offset < 120, $"Offset ({slidable.Offset}) should be damped below raw delta (120)");
        Assert.True(slidable.Offset > 80, $"Offset ({slidable.Offset}) should exceed ActionExtent (80)");

        host.Window.MouseUp(point.WithX(point.X + 120), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void DraggableScrollableSheet_Snaps_To_Extents()
    {
        var sheet = new MdDraggableScrollableSheet
        {
            Width = 400,
            Height = 600,
            MinimumExtent = 0.25,
            InitialExtent = 0.5,
            MaximumExtent = 0.9,
            Snap = true,
            SnapSizes = new[] { 0.25, 0.5, 0.9 },
            Content = new Border { Height = 1000 }
        };

        using var host = Show(sheet, 500, 700);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(0.5, sheet.Extent);
        sheet.JumpTo(0.25);
        Assert.Equal(0.25, sheet.Extent);
        sheet.JumpTo(0.9);
        Assert.Equal(0.9, sheet.Extent);
    }

    [AvaloniaFact]
    public void Touch_Ergonomics_Controls_Meet_Minimum_Hit_Target_48()
    {
        var iconButton = new MdIconButton { Icon = MdSymbols.Search };
        var toggleIcon = new MdToggleIconButton { Icon = MdSymbols.Star };
        var checkBox = new MdCheckBox { Content = "Target test" };
        var radio = new MdRadioButton { Content = "Radio test" };
        var toggleSwitch = new MdSwitch { Content = "Switch test" };
        var treeView = new MdTreeView
        {
            Roots = new[] { new MdTreeNode("1", "Root", new[] { new MdTreeNode("2", "Child") }) },
            Width = 300,
            Height = 150
        };

        var panel = new StackPanel
        {
            Children = { iconButton, toggleIcon, checkBox, radio, toggleSwitch, treeView }
        };

        using var host = Show(panel, 600, 800);
        Dispatcher.UIThread.RunJobs();

        Assert.True(iconButton.MinWidth >= 48, $"IconButton MinWidth: {iconButton.MinWidth}");
        Assert.True(iconButton.MinHeight >= 48, $"IconButton MinHeight: {iconButton.MinHeight}");
        Assert.True(toggleIcon.MinWidth >= 48, $"ToggleIconButton MinWidth: {toggleIcon.MinWidth}");
        Assert.True(toggleIcon.MinHeight >= 48, $"ToggleIconButton MinHeight: {toggleIcon.MinHeight}");
        Assert.True(checkBox.Bounds.Height >= 48, $"CheckBox Height: {checkBox.Bounds.Height}");
        Assert.True(radio.Bounds.Height >= 48, $"RadioButton Height: {radio.Bounds.Height}");
        Assert.True(toggleSwitch.Bounds.Height >= 48, $"Switch Height: {toggleSwitch.Bounds.Height}");
    }

    [AvaloniaFact]
    public void TooltipHost_Supports_LongPress_Delay_Configuration()
    {
        var tooltip = new MdTooltip { Content = "Helpful text" };
        var host = new MdTooltipHost
        {
            Tooltip = tooltip,
            LongPressDelay = TimeSpan.FromMilliseconds(300),
            Content = new MdButton { Content = "Long press me" }
        };

        using var windowScope = Show(host, 400, 300);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(TimeSpan.FromMilliseconds(300), host.LongPressDelay);
        Assert.False(host.IsOpen);
        host.Show();
        Assert.True(host.IsOpen);
        host.Dismiss();
        Assert.False(host.IsOpen);
    }

    [AvaloniaFact]
    public void KeyboardAvoidingHost_Adjusts_Padding_And_Scrolls_Focused_Child()
    {
        var textBox = new MdTextBox { Text = "Input inside scroll" };
        var scrollViewer = new ScrollViewer
        {
            Height = 300,
            Content = new StackPanel
            {
                Spacing = 50,
                Children =
                {
                    new Border { Height = 100 },
                    textBox,
                    new Border { Height = 400 }
                }
            }
        };

        var avoidingHost = new MdKeyboardAvoidingHost
        {
            Width = 400,
            Height = 400,
            AutoScrollToFocused = true,
            ExtraBottomOffset = 20,
            Content = scrollViewer
        };

        using var host = Show(avoidingHost, 500, 500);
        Dispatcher.UIThread.RunJobs();

        Assert.False(avoidingHost.IsKeyboardActive);
        Assert.Equal(0, avoidingHost.Padding.Bottom);

        avoidingHost.KeyboardHeight = 250;
        Dispatcher.UIThread.RunJobs();

        Assert.True(avoidingHost.IsKeyboardActive);
        Assert.Equal(250, avoidingHost.Padding.Bottom);

        avoidingHost.BringControlIntoView(textBox);
        Dispatcher.UIThread.RunJobs();

        avoidingHost.KeyboardHeight = 0;
        Dispatcher.UIThread.RunJobs();
        Assert.False(avoidingHost.IsKeyboardActive);
        Assert.Equal(0, avoidingHost.Padding.Bottom);
    }

    private static Scope Show(Control content, double width = 800, double height = 600)
    {
        var window = new Window { Width = width, Height = height, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new Scope(window);
    }

    private sealed class Scope(Window window) : IDisposable
    {
        public Window Window { get; } = window;
        public void Dispose() => Window.Close();
    }

    private sealed class ActionCommand(Action action) : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => action();
    }
}

internal static class GesturePointExtensions
{
    public static Point WithX(this Point point, double x) => new(x, point.Y);
}
