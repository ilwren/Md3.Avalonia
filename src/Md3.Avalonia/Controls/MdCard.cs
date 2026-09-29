using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A Material 3 card. It derives from Button so interactive cards preserve commands,
/// keyboard activation, focus and automation without imposing a global Button style.
/// </summary>
[PseudoClasses(":elevated", ":filled", ":outlined", ":interactive", ":reduced-motion", ":no-motion")]
public sealed class MdCard : Button
{
    private Border? _stateLayer;
    public static readonly StyledProperty<MdCardVariant> VariantProperty =
        AvaloniaProperty.Register<MdCard, MdCardVariant>(nameof(Variant), MdCardVariant.Elevated);

    public static readonly StyledProperty<bool> IsInteractiveProperty =
        AvaloniaProperty.Register<MdCard, bool>(nameof(IsInteractive), true);

    public static readonly StyledProperty<BoxShadows> ElevationProperty =
        AvaloniaProperty.Register<MdCard, BoxShadows>(nameof(Elevation));

    static MdCard()
    {
        VariantProperty.Changed.AddClassHandler<MdCard>((card, _) => card.UpdatePseudoClasses());
        IsInteractiveProperty.Changed.AddClassHandler<MdCard>((card, _) => card.UpdateInteractiveState());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdCard>((card, _) => card.UpdateMotion());
    }

    public MdCard()
    {
        UpdatePseudoClasses();
        UpdateInteractiveState();
    }

    public MdCardVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public BoxShadows Elevation
    {
        get => GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    /// <summary>Set false for a purely presentational card that is skipped by keyboard focus.</summary>
    public bool IsInteractive
    {
        get => GetValue(IsInteractiveProperty);
        set => SetValue(IsInteractiveProperty, value);
    }

    protected override void OnClick()
    {
        if (IsInteractive)
        {
            base.OnClick();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _stateLayer = e.NameScope.Find<Border>("PART_StateLayer");
        UpdateMotion();
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_stateLayer is not null)
            _stateLayer.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":elevated", Variant == MdCardVariant.Elevated);
        PseudoClasses.Set(":filled", Variant == MdCardVariant.Filled);
        PseudoClasses.Set(":outlined", Variant == MdCardVariant.Outlined);
    }

    private void UpdateInteractiveState()
    {
        PseudoClasses.Set(":interactive", IsInteractive);
        SetCurrentValue(FocusableProperty, IsInteractive);
        SetCurrentValue(IsTabStopProperty, IsInteractive);
    }
}
