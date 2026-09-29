using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A Material 3 icon toggle button with separate selected and unselected icon slots.</summary>
[PseudoClasses(":standard", ":filled", ":tonal", ":outlined", ":xsmall", ":small", ":medium", ":large", ":xlarge", ":narrow", ":default-width", ":wide", ":round", ":square", ":has-selected-icon", ":selected-shape-morph", ":reduced-motion", ":no-motion")]
public class MdToggleIconButton : ToggleButton
{
    private Border? _container;
    private Border? _stateLayer;
    public static readonly StyledProperty<MdIconButtonVariant> VariantProperty = AvaloniaProperty.Register<MdToggleIconButton, MdIconButtonVariant>(nameof(Variant));
    public static readonly StyledProperty<MdButtonSize> SizeProperty = AvaloniaProperty.Register<MdToggleIconButton, MdButtonSize>(nameof(Size), MdButtonSize.Small);
    public static readonly StyledProperty<MdIconButtonWidth> WidthModeProperty = AvaloniaProperty.Register<MdToggleIconButton, MdIconButtonWidth>(nameof(WidthMode), MdIconButtonWidth.Default);
    public static readonly StyledProperty<MdButtonShape> ShapeProperty = AvaloniaProperty.Register<MdToggleIconButton, MdButtonShape>(nameof(Shape), MdButtonShape.Round);
    public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<MdToggleIconButton, object?>(nameof(Icon));
    public static readonly StyledProperty<object?> SelectedIconProperty = AvaloniaProperty.Register<MdToggleIconButton, object?>(nameof(SelectedIcon));
    public static readonly StyledProperty<double> ContainerHeightProperty = AvaloniaProperty.Register<MdToggleIconButton, double>(nameof(ContainerHeight), 40);
    public static readonly StyledProperty<double> ContainerWidthProperty = AvaloniaProperty.Register<MdToggleIconButton, double>(nameof(ContainerWidth), 40);
    public static readonly StyledProperty<double> IconSizeProperty = AvaloniaProperty.Register<MdToggleIconButton, double>(nameof(IconSize), 24);
    public static readonly StyledProperty<CornerRadius> ContainerCornerRadiusProperty = AvaloniaProperty.Register<MdToggleIconButton, CornerRadius>(nameof(ContainerCornerRadius), new CornerRadius(20));
    public static readonly StyledProperty<IBrush?> StateLayerBrushProperty = AvaloniaProperty.Register<MdToggleIconButton, IBrush?>(nameof(StateLayerBrush));
    public static readonly StyledProperty<bool> EnableSelectedShapeMorphProperty = AvaloniaProperty.Register<MdToggleIconButton, bool>(nameof(EnableSelectedShapeMorph), true);

    static MdToggleIconButton()
    {
        VariantProperty.Changed.AddClassHandler<MdToggleIconButton>((x, _) => x.UpdatePseudoClasses());
        SizeProperty.Changed.AddClassHandler<MdToggleIconButton>((x, _) => x.UpdatePseudoClasses());
        WidthModeProperty.Changed.AddClassHandler<MdToggleIconButton>((x, _) => x.UpdatePseudoClasses());
        ShapeProperty.Changed.AddClassHandler<MdToggleIconButton>((x, _) => x.UpdatePseudoClasses());
        SelectedIconProperty.Changed.AddClassHandler<MdToggleIconButton>((x, _) => x.UpdatePseudoClasses());
        EnableSelectedShapeMorphProperty.Changed.AddClassHandler<MdToggleIconButton>((x, _) => x.UpdatePseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdToggleIconButton>((x, _) => x.UpdateMotion());
    }
    public MdToggleIconButton() => UpdatePseudoClasses();

    public MdIconButtonVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public MdButtonSize Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public MdIconButtonWidth WidthMode { get => GetValue(WidthModeProperty); set => SetValue(WidthModeProperty, value); }
    public MdButtonShape Shape { get => GetValue(ShapeProperty); set => SetValue(ShapeProperty, value); }
    public object? Icon { get => GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public object? SelectedIcon { get => GetValue(SelectedIconProperty); set => SetValue(SelectedIconProperty, value); }
    public double ContainerHeight { get => GetValue(ContainerHeightProperty); set => SetValue(ContainerHeightProperty, value); }
    public double ContainerWidth { get => GetValue(ContainerWidthProperty); set => SetValue(ContainerWidthProperty, value); }
    public double IconSize { get => GetValue(IconSizeProperty); set => SetValue(IconSizeProperty, value); }
    public CornerRadius ContainerCornerRadius { get => GetValue(ContainerCornerRadiusProperty); set => SetValue(ContainerCornerRadiusProperty, value); }
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
        PseudoClasses.Set(":standard", Variant == MdIconButtonVariant.Standard);
        PseudoClasses.Set(":filled", Variant == MdIconButtonVariant.Filled);
        PseudoClasses.Set(":tonal", Variant == MdIconButtonVariant.Tonal);
        PseudoClasses.Set(":outlined", Variant == MdIconButtonVariant.Outlined);
        PseudoClasses.Set(":xsmall", Size == MdButtonSize.ExtraSmall);
        PseudoClasses.Set(":small", Size == MdButtonSize.Small);
        PseudoClasses.Set(":medium", Size == MdButtonSize.Medium);
        PseudoClasses.Set(":large", Size == MdButtonSize.Large);
        PseudoClasses.Set(":xlarge", Size == MdButtonSize.ExtraLarge);
        PseudoClasses.Set(":narrow", WidthMode == MdIconButtonWidth.Narrow);
        PseudoClasses.Set(":default-width", WidthMode == MdIconButtonWidth.Default);
        PseudoClasses.Set(":wide", WidthMode == MdIconButtonWidth.Wide);
        PseudoClasses.Set(":round", Shape == MdButtonShape.Round);
        PseudoClasses.Set(":square", Shape == MdButtonShape.Square);
        PseudoClasses.Set(":has-selected-icon", SelectedIcon is not null);
        PseudoClasses.Set(":selected-shape-morph", EnableSelectedShapeMorph);
    }
}
