using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A header, optional subtitle and body participating in an <see cref="MdStepper"/>.</summary>
[PseudoClasses(":active", ":inactive", ":content-present", ":complete", ":editing", ":error", ":disabled",
    ":reduced-motion", ":no-motion", ":step-first", ":step-last", ":horizontal")]
public sealed class MdStep : HeaderedContentControl
{
    public static readonly StyledProperty<object?> SubtitleProperty =
        AvaloniaProperty.Register<MdStep, object?>(nameof(Subtitle));
    public static readonly StyledProperty<MdStepState> StateProperty =
        AvaloniaProperty.Register<MdStep, MdStepState>(nameof(State));
    public static readonly DirectProperty<MdStep, bool> IsActiveProperty =
        AvaloniaProperty.RegisterDirect<MdStep, bool>(nameof(IsActive), step => step.IsActive);
    public static readonly DirectProperty<MdStep, int> StepNumberProperty =
        AvaloniaProperty.RegisterDirect<MdStep, int>(nameof(StepNumber), step => step.StepNumber);

    private Button? _headerButton;
    private ContentPresenter? _contentPresenter;
    private Border? _indicator;
    private Control? _stepNumberPresenter;
    private Control? _completeIcon;
    private Control? _errorIcon;
    private Border? _connector;
    private Border? _hConnector;
    private Control? _customIcon;
    private IBrush? _connectorBrush;
    private double _connectorThickness = 2d;
    private readonly MdPresenceController _presence;
    private bool _isActive;
    private int _stepNumber = 1;

    static MdStep()
    {
        StateProperty.Changed.AddClassHandler<MdStep>((step, _) => step.UpdatePseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdStep>((step, _) => step.UpdateMotion());
    }

    public MdStep()
    {
        _presence = new MdPresenceController(present => PseudoClasses.Set(":content-present", present));
        _presence.Initialize(IsActive);
        UpdatePseudoClasses();
    }

    public object? Subtitle { get => GetValue(SubtitleProperty); set => SetValue(SubtitleProperty, value); }
    public MdStepState State { get => GetValue(StateProperty); set => SetValue(StateProperty, value); }
    public bool IsActive => _isActive;
    public int StepNumber => _stepNumber;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_headerButton is not null) _headerButton.Click -= OnHeaderClick;
        base.OnApplyTemplate(e);
        _headerButton = e.NameScope.Find<Button>("PART_HeaderButton");
        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_StepContent");
        _indicator = e.NameScope.Find<Border>("PART_Indicator");
        _stepNumberPresenter = e.NameScope.Find<Control>("PART_StepNumber");
        _completeIcon = e.NameScope.Find<Control>("PART_CompleteIcon");
        _errorIcon = e.NameScope.Find<Control>("PART_ErrorIcon");
        _connector = e.NameScope.Find<Border>("PART_Connector");
        _hConnector = e.NameScope.Find<Border>("PART_HConnector");
        if (_headerButton is not null) _headerButton.Click += OnHeaderClick;
        _presence.Initialize(IsActive);
        ApplyConnector();
        ApplyCustomIcon();
        UpdateMotion();
    }

    // Avalonia 12 only lets a control set the pseudo classes it declares itself, so the
    // stepper funnels the position flags through here instead of touching the step's classes.
    internal void SetStepState(int number, bool active, bool first, bool last, bool horizontal)
    {
        SetAndRaise(StepNumberProperty, ref _stepNumber, number);
        SetAndRaise(IsActiveProperty, ref _isActive, active);
        PseudoClasses.Set(":step-first", first);
        PseudoClasses.Set(":step-last", last);
        PseudoClasses.Set(":horizontal", horizontal);
        UpdatePseudoClasses();
        _presence.Update(active, MdMotion.GetExitDuration(this));
        if (_contentPresenter is not null) _contentPresenter.IsHitTestVisible = active;
    }

    /// <summary>
    /// Applies the stepper-level connector brush and thickness to whichever connector template is
    /// active; a <c>null</c> brush hands the value back to the theme.
    /// </summary>
    internal void SetConnector(IBrush? brush, double thickness)
    {
        if (Equals(_connectorBrush, brush) && _connectorThickness.Equals(thickness)) return;
        _connectorBrush = brush;
        _connectorThickness = thickness;
        ApplyConnector();
    }

    private void ApplyConnector()
    {
        foreach (var connector in new[] { _connector, _hConnector })
        {
            if (connector is null) continue;
            if (_connectorBrush is not null) connector.Background = _connectorBrush;
            else connector.ClearValue(Visual.BackgroundProperty);
        }
        if (_connector is not null) _connector.Width = _connectorThickness;
        if (_hConnector is not null) _hConnector.Height = _connectorThickness;
    }

    /// <summary>
    /// Installs a custom indicator built by <see cref="MdStepper.StepIconBuilder"/>. While one is
    /// present the stock number/check/error visuals are hidden but not removed.
    /// </summary>
    internal void SetCustomIcon(Control? icon)
    {
        if (ReferenceEquals(_customIcon, icon)) return;
        if (_customIcon is not null && _indicator?.Child is Panel oldHost) oldHost.Children.Remove(_customIcon);
        _customIcon = icon;
        ApplyCustomIcon();
    }

    private void ApplyCustomIcon()
    {
        var useCustom = _customIcon is not null;
        if (_stepNumberPresenter is not null) _stepNumberPresenter.IsVisible = !useCustom;
        if (_completeIcon is not null) _completeIcon.IsVisible = !useCustom;
        if (_errorIcon is not null) _errorIcon.IsVisible = !useCustom;
        if (useCustom && _customIcon is not null && _indicator?.Child is Panel host && !host.Children.Contains(_customIcon))
            host.Children.Add(_customIcon);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _presence.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private void OnHeaderClick(object? sender, RoutedEventArgs e)
    {
        if (State == MdStepState.Disabled) return;
        this.GetLogicalAncestors().OfType<MdStepper>().FirstOrDefault()?.GoTo(StepNumber - 1);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_contentPresenter is not null)
        {
            _contentPresenter.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty));
        }
        if (_indicator is not null)
        {
            _indicator.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateBrush(this, BackgroundProperty, MdMotionSpeed.Fast));
        }
        foreach (var presenter in new[] { _stepNumberPresenter, _completeIcon, _errorIcon })
        {
            if (presenter is not null)
                presenter.Transitions = MdMotionTransitions.Collect(
                    MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        }
        if (!IsActive) _presence.Update(false, MdMotion.GetExitDuration(this));
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":active", IsActive);
        PseudoClasses.Set(":inactive", !IsActive);
        PseudoClasses.Set(":complete", State == MdStepState.Complete);
        PseudoClasses.Set(":editing", State == MdStepState.Editing);
        PseudoClasses.Set(":error", State == MdStepState.Error);
        PseudoClasses.Set(":disabled", State == MdStepState.Disabled);
    }
}
