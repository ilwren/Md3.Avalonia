using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A Material 3 Expressive selection button backed by Avalonia ToggleButton.</summary>
[PseudoClasses(":elevated", ":filled", ":tonal", ":outlined", ":xsmall", ":small", ":medium", ":large", ":xlarge", ":round", ":square", ":has-leading-icon", ":has-trailing-icon", ":selected-shape-morph", ":reduced-motion", ":no-motion")]
public class MdToggleButton : ToggleButton
{
    private Border? _container;
    private Border? _stateLayer;
    public static readonly StyledProperty<MdToggleButtonVariant> VariantProperty =
        AvaloniaProperty.Register<MdToggleButton, MdToggleButtonVariant>(nameof(Variant), MdToggleButtonVariant.Filled);
    public static readonly StyledProperty<MdButtonSize> SizeProperty =
        AvaloniaProperty.Register<MdToggleButton, MdButtonSize>(nameof(Size), MdButtonSize.Small);
    public static readonly StyledProperty<MdButtonShape> ShapeProperty =
        AvaloniaProperty.Register<MdToggleButton, MdButtonShape>(nameof(Shape), MdButtonShape.Round);
    public static readonly StyledProperty<object?> LeadingIconProperty =
        AvaloniaProperty.Register<MdToggleButton, object?>(nameof(LeadingIcon));
    public static readonly StyledProperty<object?> TrailingIconProperty =
        AvaloniaProperty.Register<MdToggleButton, object?>(nameof(TrailingIcon));
    public static readonly StyledProperty<double> ContainerHeightProperty =
        AvaloniaProperty.Register<MdToggleButton, double>(nameof(ContainerHeight), 40);
    public static readonly StyledProperty<CornerRadius> ContainerCornerRadiusProperty =
        AvaloniaProperty.Register<MdToggleButton, CornerRadius>(nameof(ContainerCornerRadius), new CornerRadius(20));
    public static readonly StyledProperty<double> IconSizeProperty =
        AvaloniaProperty.Register<MdToggleButton, double>(nameof(IconSize), 20);
    public static readonly StyledProperty<double> IconSpacingProperty =
        AvaloniaProperty.Register<MdToggleButton, double>(nameof(IconSpacing), 8);
    public static readonly StyledProperty<IBrush?> StateLayerBrushProperty =
        AvaloniaProperty.Register<MdToggleButton, IBrush?>(nameof(StateLayerBrush));
    public static readonly StyledProperty<bool> EnableSelectedShapeMorphProperty =
        AvaloniaProperty.Register<MdToggleButton, bool>(nameof(EnableSelectedShapeMorph), true);

    static MdToggleButton()
    {
        VariantProperty.Changed.AddClassHandler<MdToggleButton>((x, _) => x.UpdatePseudoClasses());
        SizeProperty.Changed.AddClassHandler<MdToggleButton>((x, _) => x.UpdatePseudoClasses());
        ShapeProperty.Changed.AddClassHandler<MdToggleButton>((x, _) => x.UpdatePseudoClasses());
        LeadingIconProperty.Changed.AddClassHandler<MdToggleButton>((x, _) => x.UpdatePseudoClasses());
        TrailingIconProperty.Changed.AddClassHandler<MdToggleButton>((x, _) => x.UpdatePseudoClasses());
        EnableSelectedShapeMorphProperty.Changed.AddClassHandler<MdToggleButton>((x, _) => x.UpdatePseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdToggleButton>((x, _) => x.UpdateMotion());
    }

    public MdToggleButton() => UpdatePseudoClasses();

    public MdToggleButtonVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public MdButtonSize Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public MdButtonShape Shape { get => GetValue(ShapeProperty); set => SetValue(ShapeProperty, value); }
    public object? LeadingIcon { get => GetValue(LeadingIconProperty); set => SetValue(LeadingIconProperty, value); }
    public object? TrailingIcon { get => GetValue(TrailingIconProperty); set => SetValue(TrailingIconProperty, value); }
    public double ContainerHeight { get => GetValue(ContainerHeightProperty); set => SetValue(ContainerHeightProperty, value); }
    public CornerRadius ContainerCornerRadius { get => GetValue(ContainerCornerRadiusProperty); set => SetValue(ContainerCornerRadiusProperty, value); }
    public double IconSize { get => GetValue(IconSizeProperty); set => SetValue(IconSizeProperty, value); }
    public double IconSpacing { get => GetValue(IconSpacingProperty); set => SetValue(IconSpacingProperty, value); }
    public IBrush? StateLayerBrush { get => GetValue(StateLayerBrushProperty); set => SetValue(StateLayerBrushProperty, value); }
    public bool EnableSelectedShapeMorph { get => GetValue(EnableSelectedShapeMorphProperty); set => SetValue(EnableSelectedShapeMorphProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _container = e.NameScope.Find<Border>("PART_Container");
        _stateLayer = e.NameScope.Find<Border>("PART_StateLayer");
        UpdateMotion();
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_container is not null)
            _container.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateCornerRadius(this, Border.CornerRadiusProperty),
                MdMotionTransitions.CreateBrush(this, Border.BackgroundProperty, MdMotionSpeed.Fast));
        if (_stateLayer is not null)
            _stateLayer.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":elevated", Variant == MdToggleButtonVariant.Elevated);
        PseudoClasses.Set(":filled", Variant == MdToggleButtonVariant.Filled);
        PseudoClasses.Set(":tonal", Variant == MdToggleButtonVariant.Tonal);
        PseudoClasses.Set(":outlined", Variant == MdToggleButtonVariant.Outlined);
        PseudoClasses.Set(":xsmall", Size == MdButtonSize.ExtraSmall);
        PseudoClasses.Set(":small", Size == MdButtonSize.Small);
        PseudoClasses.Set(":medium", Size == MdButtonSize.Medium);
        PseudoClasses.Set(":large", Size == MdButtonSize.Large);
        PseudoClasses.Set(":xlarge", Size == MdButtonSize.ExtraLarge);
        PseudoClasses.Set(":round", Shape == MdButtonShape.Round);
        PseudoClasses.Set(":square", Shape == MdButtonShape.Square);
        PseudoClasses.Set(":has-leading-icon", LeadingIcon is not null);
        PseudoClasses.Set(":has-trailing-icon", TrailingIcon is not null);
        PseudoClasses.Set(":selected-shape-morph", EnableSelectedShapeMorph);
    }
}
