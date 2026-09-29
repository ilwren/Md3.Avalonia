using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A Material plain or rich tooltip surface.</summary>
[TemplatePart("PART_Surface", typeof(Border))]
[PseudoClasses(":plain", ":rich", ":open", ":closed", ":has-title", ":has-action", ":reduced-motion", ":no-motion")]
public sealed class MdTooltip : ContentControl
{
    public static readonly StyledProperty<MdTooltipVariant> VariantProperty =
        AvaloniaProperty.Register<MdTooltip, MdTooltipVariant>(nameof(Variant));
    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<MdTooltip, object?>(nameof(Title));
    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<MdTooltip, object?>(nameof(ActionContent));
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MdTooltip, bool>(nameof(IsOpen),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    private Border? _surface;

    static MdTooltip()
    {
        VariantProperty.Changed.AddClassHandler<MdTooltip>((tooltip, _) => tooltip.UpdatePseudoClasses());
        TitleProperty.Changed.AddClassHandler<MdTooltip>((tooltip, _) => tooltip.UpdatePseudoClasses());
        ActionContentProperty.Changed.AddClassHandler<MdTooltip>((tooltip, _) => tooltip.UpdatePseudoClasses());
        IsOpenProperty.Changed.AddClassHandler<MdTooltip>((tooltip, _) => tooltip.UpdatePseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdTooltip>((tooltip, _) => tooltip.UpdateMotion());
    }

    public MdTooltip() => UpdatePseudoClasses();

    public MdTooltipVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public object? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public object? ActionContent { get => GetValue(ActionContentProperty); set => SetValue(ActionContentProperty, value); }
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }

    public void Show() => SetCurrentValue(IsOpenProperty, true);
    public void Dismiss() => SetCurrentValue(IsOpenProperty, false);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _surface = e.NameScope.Find<Border>("PART_Surface");
        UpdateMotion();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":plain", Variant == MdTooltipVariant.Plain);
        PseudoClasses.Set(":rich", Variant == MdTooltipVariant.Rich);
        PseudoClasses.Set(":open", IsOpen);
        PseudoClasses.Set(":closed", !IsOpen);
        PseudoClasses.Set(":has-title", Title is not null);
        PseudoClasses.Set(":has-action", ActionContent is not null);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_surface is null) return;
        _surface.Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
            MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
    }
}
