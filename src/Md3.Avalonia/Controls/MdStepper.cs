using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Threading;

namespace Md3.Avalonia.Controls;

/// <summary>A Flutter-style Material stepper with linear/non-linear navigation and command hooks.</summary>
[PseudoClasses(":vertical", ":horizontal", ":first-step", ":last-step")]
public sealed class MdStepper : ItemsControl
{
    public static readonly StyledProperty<int> ActiveStepProperty =
        AvaloniaProperty.Register<MdStepper, int>(nameof(ActiveStep),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<MdStepper, Orientation>(nameof(Orientation), Orientation.Vertical);
    public static readonly StyledProperty<bool> IsLinearProperty =
        AvaloniaProperty.Register<MdStepper, bool>(nameof(IsLinear), true);
    public static readonly StyledProperty<object?> ContinueContentProperty =
        AvaloniaProperty.Register<MdStepper, object?>(nameof(ContinueContent), "Continue");
    public static readonly StyledProperty<object?> CancelContentProperty =
        AvaloniaProperty.Register<MdStepper, object?>(nameof(CancelContent), "Back");
    public static readonly StyledProperty<ICommand?> ContinueCommandProperty =
        AvaloniaProperty.Register<MdStepper, ICommand?>(nameof(ContinueCommand));
    public static readonly StyledProperty<ICommand?> CancelCommandProperty =
        AvaloniaProperty.Register<MdStepper, ICommand?>(nameof(CancelCommand));

    private Button? _continueButton;
    private Button? _cancelButton;

    static MdStepper()
    {
        ActiveStepProperty.Changed.AddClassHandler<MdStepper>((stepper, change) =>
        {
            stepper.CoerceActiveStep();
            stepper.UpdateSteps();
            stepper.ActiveStepChanged?.Invoke(stepper, stepper.ActiveStep);
        });
        OrientationProperty.Changed.AddClassHandler<MdStepper>((stepper, _) => stepper.UpdatePseudoClasses());
    }

    public MdStepper()
    {
        UpdatePseudoClasses();
        LayoutUpdated += (_, _) => UpdateSteps();
    }

    public int ActiveStep { get => GetValue(ActiveStepProperty); set => SetValue(ActiveStepProperty, value); }
    public Orientation Orientation { get => GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }
    public bool IsLinear { get => GetValue(IsLinearProperty); set => SetValue(IsLinearProperty, value); }
    public object? ContinueContent { get => GetValue(ContinueContentProperty); set => SetValue(ContinueContentProperty, value); }
    public object? CancelContent { get => GetValue(CancelContentProperty); set => SetValue(CancelContentProperty, value); }
    public ICommand? ContinueCommand { get => GetValue(ContinueCommandProperty); set => SetValue(ContinueCommandProperty, value); }
    public ICommand? CancelCommand { get => GetValue(CancelCommandProperty); set => SetValue(CancelCommandProperty, value); }

    public event EventHandler<int>? ContinueRequested;
    public event EventHandler<int>? CancelRequested;
    public event EventHandler<int>? ActiveStepChanged;

    public void Next()
    {
        ContinueRequested?.Invoke(this, ActiveStep);
        if (ContinueCommand?.CanExecute(ActiveStep) == true) ContinueCommand.Execute(ActiveStep);
        if (ActiveStep < Math.Max(0, StepCount - 1)) SetCurrentValue(ActiveStepProperty, ActiveStep + 1);
    }

    public void Previous()
    {
        CancelRequested?.Invoke(this, ActiveStep);
        if (CancelCommand?.CanExecute(ActiveStep) == true) CancelCommand.Execute(ActiveStep);
        if (ActiveStep > 0) SetCurrentValue(ActiveStepProperty, ActiveStep - 1);
    }

    public void GoTo(int index)
    {
        var target = Math.Clamp(index, 0, Math.Max(0, StepCount - 1));
        if (IsLinear && target > ActiveStep + 1) return;
        SetCurrentValue(ActiveStepProperty, target);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_continueButton is not null) _continueButton.Click -= OnContinue;
        if (_cancelButton is not null) _cancelButton.Click -= OnCancel;
        base.OnApplyTemplate(e);
        _continueButton = e.NameScope.Find<Button>("PART_ContinueButton");
        _cancelButton = e.NameScope.Find<Button>("PART_CancelButton");
        if (_continueButton is not null) _continueButton.Click += OnContinue;
        if (_cancelButton is not null) _cancelButton.Click += OnCancel;
        Dispatcher.UIThread.Post(UpdateSteps, DispatcherPriority.Loaded);
    }

    private int StepCount => Items.Count;
    private void OnContinue(object? sender, RoutedEventArgs e) => Next();
    private void OnCancel(object? sender, RoutedEventArgs e) => Previous();

    private void CoerceActiveStep()
    {
        var coerced = Math.Clamp(ActiveStep, 0, Math.Max(0, StepCount - 1));
        if (coerced != ActiveStep) SetCurrentValue(ActiveStepProperty, coerced);
    }

    private void UpdateSteps()
    {
        var steps = Items.OfType<MdStep>()
            .Concat(this.GetLogicalDescendants().OfType<MdStep>())
            .Distinct()
            .ToArray();
        for (var index = 0; index < steps.Length; index++)
            steps[index].SetStepState(index + 1, index == ActiveStep);
        UpdatePseudoClasses();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":vertical", Orientation == Orientation.Vertical);
        PseudoClasses.Set(":horizontal", Orientation == Orientation.Horizontal);
        PseudoClasses.Set(":first-step", ActiveStep <= 0);
        PseudoClasses.Set(":last-step", StepCount > 0 && ActiveStep >= StepCount - 1);
    }
}
