using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;

namespace Md3.Avalonia.Controls;

/// <summary>Details handed to <see cref="MdStepper.ControlsBuilder"/> for the active step.</summary>
public sealed class MdStepControlsContext(MdStepper stepper, int stepIndex, int stepCount)
{
    public MdStepper Stepper { get; } = stepper;
    public int StepIndex { get; } = stepIndex;
    public int StepCount { get; } = stepCount;
    public bool CanContinue => StepIndex < StepCount - 1;
    public bool CanCancel => StepIndex > 0;

    /// <summary>Advances like the default continue button would.</summary>
    public void Continue() => Stepper.Next();

    /// <summary>Goes back like the default cancel button would.</summary>
    public void Cancel() => Stepper.Previous();
}

/// <summary>Details handed to <see cref="MdStepper.StepIconBuilder"/> for one step.</summary>
public sealed class MdStepIconContext(MdStep step, int stepIndex)
{
    public MdStep Step { get; } = step;
    public int StepIndex { get; } = stepIndex;
    public MdStepState State => Step.State;
    public bool IsActive => Step.IsActive;
}

/// <summary>A Flutter-style Material stepper with linear/non-linear navigation and command hooks.</summary>
/// <remarks>
/// <para>
/// The default <see cref="Orientation"/> is vertical: every step renders its header and, when active,
/// its body below the header. With <see cref="Orientation"/> set to <c>Orientation.Horizontal</c>
/// the steps lay out as a header row (indicator above label, horizontal connectors between steps) and
/// the active step's content moves into a shared body below the row, mirroring Flutter's
/// <c>StepperType.horizontal</c>.
/// </para>
/// </remarks>
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
    public static readonly StyledProperty<IBrush?> ConnectorBrushProperty =
        AvaloniaProperty.Register<MdStepper, IBrush?>(nameof(ConnectorBrush));
    public static readonly StyledProperty<double> ConnectorThicknessProperty =
        AvaloniaProperty.Register<MdStepper, double>(nameof(ConnectorThickness), 2d);
    public static readonly StyledProperty<Func<MdStepControlsContext, Control?>?> ControlsBuilderProperty =
        AvaloniaProperty.Register<MdStepper, Func<MdStepControlsContext, Control?>?>(nameof(ControlsBuilder));
    public static readonly StyledProperty<Func<MdStepIconContext, Control?>?> StepIconBuilderProperty =
        AvaloniaProperty.Register<MdStepper, Func<MdStepIconContext, Control?>?>(nameof(StepIconBuilder));
    public static readonly DirectProperty<MdStepper, object?> ActiveStepContentProperty =
        AvaloniaProperty.RegisterDirect<MdStepper, object?>(nameof(ActiveStepContent), stepper => stepper.ActiveStepContent);
    public static readonly DirectProperty<MdStepper, IDataTemplate?> ActiveStepContentTemplateProperty =
        AvaloniaProperty.RegisterDirect<MdStepper, IDataTemplate?>(
            nameof(ActiveStepContentTemplate), stepper => stepper.ActiveStepContentTemplate);

    private Button? _continueButton;
    private Button? _cancelButton;
    private object? _activeStepContent;
    private IDataTemplate? _activeStepContentTemplate;
    private Control? _customControls;
    private object? _buildersSignature;

    static MdStepper()
    {
        ActiveStepProperty.Changed.AddClassHandler<MdStepper>((stepper, change) =>
        {
            stepper.CoerceActiveStep();
            stepper.InvalidateBuilders();
            stepper.UpdateSteps();
            stepper.ActiveStepChanged?.Invoke(stepper, stepper.ActiveStep);
        });
        // A direction change re-styles every step (the horizontal header template is swapped in
        // through each step's own pseudo class), so the steps must be refreshed as well.
        OrientationProperty.Changed.AddClassHandler<MdStepper>((stepper, _) =>
        {
            stepper.UpdatePseudoClasses();
            stepper.UpdateSteps();
        });
        ConnectorBrushProperty.Changed.AddClassHandler<MdStepper>((stepper, _) => stepper.UpdateSteps());
        ConnectorThicknessProperty.Changed.AddClassHandler<MdStepper>((stepper, _) => stepper.UpdateSteps());
        ControlsBuilderProperty.Changed.AddClassHandler<MdStepper>((stepper, _) => stepper.InvalidateBuilders());
        StepIconBuilderProperty.Changed.AddClassHandler<MdStepper>((stepper, _) => stepper.InvalidateBuilders());
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

    /// <summary>
    /// Brush for the step connectors; <c>null</c> falls back to the theme's outline-variant
    /// resource. While set it is a local value and therefore overrides the theme's active-step
    /// highlight as well.
    /// </summary>
    public IBrush? ConnectorBrush { get => GetValue(ConnectorBrushProperty); set => SetValue(ConnectorBrushProperty, value); }

    /// <summary>Connector thickness in DIP; the theme default is 2.</summary>
    public double ConnectorThickness { get => GetValue(ConnectorThicknessProperty); set => SetValue(ConnectorThicknessProperty, value); }

    /// <summary>
    /// Builds the active step's control row instead of the default continue/cancel buttons. The
    /// returned control is re-hosted whenever the active step changes; return <c>null</c> to fall
    /// back to the default buttons. Mirrors Flutter's <c>Stepper.controlsBuilder</c>.
    /// </summary>
    public Func<MdStepControlsContext, Control?>? ControlsBuilder
    {
        get => GetValue(ControlsBuilderProperty);
        set => SetValue(ControlsBuilderProperty, value);
    }

    /// <summary>
    /// Builds a replacement for a step's indicator content (number, check, error mark). Rebuilt
    /// when the active step or the builder changes; the context carries the step state at build
    /// time. Mirrors Flutter's <c>Stepper.stepIconBuilder</c>.
    /// </summary>
    public Func<MdStepIconContext, Control?>? StepIconBuilder
    {
        get => GetValue(StepIconBuilderProperty);
        set => SetValue(StepIconBuilderProperty, value);
    }

    /// <summary>Gets the content of the active step, used by the horizontal layout's shared body.</summary>
    // The getter must read the field, not GetValue: a getter-only DirectProperty whose CLR
    // getter calls GetValue recurses through the property registry until the stack overflows.
    public object? ActiveStepContent => _activeStepContent;

    /// <summary>Gets the content template of the active step, used by the horizontal layout's shared body.</summary>
    public IDataTemplate? ActiveStepContentTemplate => _activeStepContentTemplate;

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
        if (_customControls?.Parent is Panel oldHost) oldHost.Children.Remove(_customControls);
        _customControls = null;
        // Builders can have run before a template host existed. Rebuild for the new host even
        // when all of their logical inputs are unchanged.
        _buildersSignature = null;
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

    private MdStep[] GatherSteps() => Items.OfType<MdStep>()
        .Concat(this.GetLogicalDescendants().OfType<MdStep>())
        .Distinct()
        .ToArray();

    private void UpdateSteps()
    {
        var steps = GatherSteps();
        for (var index = 0; index < steps.Length; index++)
        {
            // Per-step position drives the connector visibility (the last step never draws a
            // trailing connector) and, together with the direction, the header template.
            steps[index].SetStepState(
                index + 1,
                index == ActiveStep,
                index == 0,
                index == steps.Length - 1,
                Orientation == Orientation.Horizontal);
            steps[index].SetConnector(ConnectorBrush, ConnectorThickness);
        }

        // The horizontal layout renders the active step's content in a shared body below the
        // header row, so the template needs the active item's content as a property.
        var active = steps.Length > 0 ? steps[Math.Clamp(ActiveStep, 0, steps.Length - 1)] : null;
        SetAndRaise(ActiveStepContentProperty, ref _activeStepContent, active?.Content);
        SetAndRaise(ActiveStepContentTemplateProperty, ref _activeStepContentTemplate, active?.ContentTemplate);

        UpdateBuilders(steps);
        UpdatePseudoClasses();
    }

    /// <summary>
    /// Marks the builder outputs stale so the next <see cref="UpdateSteps"/> rebuilds them. The
    /// builders create live controls, so they run only when their inputs change - navigation or
    /// an explicit invalidation - and not on every layout pass.
    /// </summary>
    private void InvalidateBuilders()
    {
        _buildersSignature = null;
        UpdateSteps();
    }

    private void UpdateBuilders(MdStep[] steps)
    {
        var signature = (ControlsBuilder, StepIconBuilder, ActiveStep, Orientation, steps.Length);
        if (Equals(signature, _buildersSignature)) return;
        _buildersSignature = signature;

        UpdateCustomControls();
        for (var index = 0; index < steps.Length; index++)
        {
            steps[index].SetCustomIcon(StepIconBuilder is null
                ? null
                : StepIconBuilder(new MdStepIconContext(steps[index], index)));
        }
    }

    private void UpdateCustomControls()
    {
        var host = (_continueButton?.Parent ?? _cancelButton?.Parent) as Panel;
        if (_customControls is not null && host is not null) host.Children.Remove(_customControls);
        _customControls = ControlsBuilder?.Invoke(new MdStepControlsContext(this, ActiveStep, StepCount));
        if (host is not null && _customControls is not null) host.Children.Add(_customControls);
        var useCustom = _customControls is not null;
        if (_continueButton is not null) _continueButton.IsVisible = !useCustom;
        if (_cancelButton is not null) _cancelButton.IsVisible = !useCustom;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":vertical", Orientation == Orientation.Vertical);
        PseudoClasses.Set(":horizontal", Orientation == Orientation.Horizontal);
        PseudoClasses.Set(":first-step", ActiveStep <= 0);
        PseudoClasses.Set(":last-step", StepCount > 0 && ActiveStep >= StepCount - 1);
    }
}
