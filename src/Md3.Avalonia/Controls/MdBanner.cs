using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A persistent Material banner with leading, content and action slots.</summary>
[PseudoClasses(":open", ":closed", ":present", ":actions-below", ":inline-actions", ":reduced-motion", ":no-motion")]
public sealed class MdBanner : ContentControl
{
    public static readonly StyledProperty<object?> LeadingContentProperty =
        AvaloniaProperty.Register<MdBanner, object?>(nameof(LeadingContent));
    public static readonly StyledProperty<object?> ActionsProperty =
        AvaloniaProperty.Register<MdBanner, object?>(nameof(Actions));
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MdBanner, bool>(nameof(IsOpen), true,
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> ForceActionsBelowProperty =
        AvaloniaProperty.Register<MdBanner, bool>(nameof(ForceActionsBelow));
    public static readonly StyledProperty<bool> IsDismissibleProperty =
        AvaloniaProperty.Register<MdBanner, bool>(nameof(IsDismissible));
    public static readonly StyledProperty<ICommand?> DismissCommandProperty =
        AvaloniaProperty.Register<MdBanner, ICommand?>(nameof(DismissCommand));
    public static readonly StyledProperty<object?> DismissCommandParameterProperty =
        AvaloniaProperty.Register<MdBanner, object?>(nameof(DismissCommandParameter));

    private readonly MdPresenceController _presence;
    private Button? _dismissButton;

    static MdBanner()
    {
        IsOpenProperty.Changed.AddClassHandler<MdBanner>((banner, _) => banner.UpdateVisualState());
        ForceActionsBelowProperty.Changed.AddClassHandler<MdBanner>((banner, _) => banner.UpdateVisualState());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdBanner>((banner, _) => banner.UpdateMotion());
    }

    public MdBanner()
    {
        _presence = new MdPresenceController(value => PseudoClasses.Set(":present", value));
        _presence.Initialize(IsOpen);
        UpdateVisualState();
    }

    public object? LeadingContent { get => GetValue(LeadingContentProperty); set => SetValue(LeadingContentProperty, value); }
    public object? Actions { get => GetValue(ActionsProperty); set => SetValue(ActionsProperty, value); }
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public bool ForceActionsBelow { get => GetValue(ForceActionsBelowProperty); set => SetValue(ForceActionsBelowProperty, value); }
    public bool IsDismissible { get => GetValue(IsDismissibleProperty); set => SetValue(IsDismissibleProperty, value); }
    public ICommand? DismissCommand { get => GetValue(DismissCommandProperty); set => SetValue(DismissCommandProperty, value); }
    public object? DismissCommandParameter { get => GetValue(DismissCommandParameterProperty); set => SetValue(DismissCommandParameterProperty, value); }

    public event EventHandler? Dismissed;

    public void Show() => SetCurrentValue(IsOpenProperty, true);

    public void Dismiss()
    {
        var parameter = DismissCommandParameter;
        if (DismissCommand?.CanExecute(parameter) == true) DismissCommand.Execute(parameter);
        SetCurrentValue(IsOpenProperty, false);
        Dismissed?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_dismissButton is not null) _dismissButton.Click -= OnDismissClick;
        base.OnApplyTemplate(e);
        _dismissButton = e.NameScope.Find<Button>("PART_DismissButton");
        if (_dismissButton is not null) _dismissButton.Click += OnDismissClick;
        UpdateMotion();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateVisualState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _presence.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private void OnDismissClick(object? sender, RoutedEventArgs e) => Dismiss();

    private void UpdateVisualState()
    {
        if (IsOpen)
        {
            _presence.Update(true, TimeSpan.Zero);
            PseudoClasses.Set(":open", true);
            PseudoClasses.Set(":closed", false);
        }
        else
        {
            PseudoClasses.Set(":open", false);
            PseudoClasses.Set(":closed", true);
            _presence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
        }
        PseudoClasses.Set(":actions-below", ForceActionsBelow);
        PseudoClasses.Set(":inline-actions", !ForceActionsBelow);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
            MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
        if (!IsOpen)
            _presence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
    }
}
