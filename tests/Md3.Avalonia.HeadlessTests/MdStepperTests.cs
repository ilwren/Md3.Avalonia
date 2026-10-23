using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Layout;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdStepperTests
{
    [AvaloniaFact]
    public void Defaults_Are_Vertical_With_First_Step_Active()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var stepper = MakeStepper(Orientation.Vertical, Step("One"), Step("Two"));
        using var host = Show(stepper);

        Assert.Equal(0, stepper.ActiveStep);
        Assert.Contains(":vertical", stepper.Classes);
        Assert.DoesNotContain(":horizontal", stepper.Classes);
        Assert.Contains(":first-step", stepper.Classes);
        Assert.DoesNotContain(":last-step", stepper.Classes);
        Assert.NotNull(stepper.Template);
    }

    [AvaloniaFact]
    public void Horizontal_Orientation_Switches_The_Layout_PseudoClass()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var stepper = MakeStepper(Orientation.Vertical, Step("One"), Step("Two"));
        using var host = Show(stepper);

        stepper.Orientation = Orientation.Horizontal;

        Assert.Contains(":horizontal", stepper.Classes);
        Assert.DoesNotContain(":vertical", stepper.Classes);
    }

    [AvaloniaFact]
    public void Next_Previous_And_GoTo_Move_The_Active_Step()
    {
        var changed = new List<int>();
        var stepper = MakeStepper(Orientation.Horizontal, Step("One"), Step("Two"), Step("Three"));
        stepper.ActiveStepChanged += (_, index) => changed.Add(index);
        using var host = Show(stepper);

        stepper.Next();
        Assert.Equal(1, stepper.ActiveStep);
        Assert.DoesNotContain(":first-step", stepper.Classes);

        stepper.Next();
        Assert.Equal(2, stepper.ActiveStep);
        Assert.Contains(":last-step", stepper.Classes);

        stepper.Previous();
        Assert.Equal(1, stepper.ActiveStep);

        stepper.GoTo(0);
        Assert.Equal(0, stepper.ActiveStep);

        Assert.Equal(new[] { 1, 2, 1, 0 }, changed);
    }

    [AvaloniaFact]
    public void Linear_Stepper_Rejects_Forward_Jumps_Beyond_The_Next_Step()
    {
        var stepper = MakeStepper(Orientation.Horizontal, Step("One"), Step("Two"), Step("Three"));
        using var host = Show(stepper);

        Assert.True(stepper.IsLinear);
        stepper.GoTo(2);
        Assert.Equal(0, stepper.ActiveStep);

        stepper.GoTo(1);
        Assert.Equal(1, stepper.ActiveStep);
    }

    [AvaloniaFact]
    public void NonLinear_Stepper_Allows_Jumping_To_Any_Step()
    {
        var stepper = MakeStepper(Orientation.Horizontal, Step("One"), Step("Two"), Step("Three"));
        stepper.IsLinear = false;
        using var host = Show(stepper);

        stepper.GoTo(2);
        Assert.Equal(2, stepper.ActiveStep);
    }

    [AvaloniaFact]
    public void Next_Fires_Continue_Requested_And_Executes_The_Command()
    {
        var requested = new List<int>();
        var command = new TestCommand();
        var stepper = MakeStepper(Orientation.Horizontal, Step("One"), Step("Two"));
        stepper.ContinueRequested += (_, index) => requested.Add(index);
        stepper.ContinueCommand = command;
        using var host = Show(stepper);

        stepper.Next();

        Assert.Equal(new[] { 0 }, requested);
        Assert.True(command.Executed);
    }

    [AvaloniaFact]
    public void Horizontal_Shared_Body_Tracks_The_Active_Step()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var body = new TextBlock { Text = "body two" };
        var stepper = MakeStepper(Orientation.Horizontal, Step("One", "first"), Step("Two", body), Step("Three", "third"));
        using var host = Show(stepper);

        Assert.Equal("first", stepper.ActiveStepContent);

        stepper.GoTo(1);
        Assert.Same(body, stepper.ActiveStepContent);

        stepper.GoTo(2);
        Assert.Equal("third", stepper.ActiveStepContent);
    }

    [AvaloniaFact]
    public void Steps_Carry_Position_Number_And_Active_PseudoClasses()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var one = Step("One");
        var two = Step("Two");
        var three = Step("Three");
        var stepper = MakeStepper(Orientation.Horizontal, one, two, three);
        using var host = Show(stepper);

        stepper.GoTo(1);

        Assert.Contains(":step-first", one.Classes);
        Assert.Contains(":step-last", three.Classes);
        Assert.Contains(":active", two.Classes);
        Assert.DoesNotContain(":active", one.Classes);
        Assert.DoesNotContain(":active", three.Classes);
        Assert.Equal(2, two.StepNumber);
        Assert.True(two.IsActive);
    }

    private static MdStepper MakeStepper(Orientation orientation, params MdStep[] steps)
    {
        var stepper = new MdStepper { Orientation = orientation };
        foreach (var step in steps)
            stepper.Items.Add(step);
        return stepper;
    }

    private static MdStep Step(string header, object? content = null) => new() { Header = header, Content = content };

    private static IDisposable Show(Control content) => new WindowScope(ShowWindow(content));

    private static Window ShowWindow(Control content)
    {
        var window = new Window { Width = 720, Height = 480, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private sealed class WindowScope(Window window) : IDisposable
    {
        public void Dispose() => window.Close();
    }

    private sealed class TestCommand : System.Windows.Input.ICommand
    {
        public bool Executed { get; private set; }
        public event System.EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => Executed = true;
    }
}
