using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A compact Material 3 action button with an icon-only visual.</summary>
[PseudoClasses(":standard", ":filled", ":tonal", ":outlined", ":xsmall", ":small", ":medium", ":large", ":xlarge", ":narrow", ":default-width", ":wide", ":round", ":square", ":reduced-motion", ":no-motion")]
public class MdIconButton : Button
{
    private Border? _container;
    private Border? _stateLayer;
    public static readonly StyledProperty<MdIconButtonVariant> VariantProperty = AvaloniaProperty.Register<MdIconButton, MdIconButtonVariant>(nameof(Variant));
    public static readonly StyledProperty<MdButtonSize> SizeProperty = AvaloniaProperty.Register<MdIconButton, MdButtonSize>(nameof(Size), MdButtonSize.Small);
    public static readonly StyledProperty<MdIconButtonWidth> WidthModeProperty = AvaloniaProperty.Register<MdIconButton, MdIconButtonWidth>(nameof(WidthMode), MdIconButtonWidth.Default);
    public static readonly StyledProperty<MdButtonShape> ShapeProperty = AvaloniaProperty.Register<MdIconButton, MdButtonShape>(nameof(Shape), MdButtonShape.Round);
    public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<MdIconButton, object?>(nameof(Icon));
    public static readonly StyledProperty<double> ContainerHeightProperty = AvaloniaProperty.Register<MdIconButton, double>(nameof(ContainerHeight), 40);
    public static readonly StyledProperty<double> ContainerWidthProperty = AvaloniaProperty.Register<MdIconButton, double>(nameof(ContainerWidth), 40);
    public static readonly StyledProperty<double> IconSizeProperty = AvaloniaProperty.Register<MdIconButton, double>(nameof(IconSize), 24);
    public static readonly StyledProperty<CornerRadius> ContainerCornerRadiusProperty = AvaloniaProperty.Register<MdIconButton, CornerRadius>(nameof(ContainerCornerRadius), new CornerRadius(20));
    public static readonly StyledProperty<IBrush?> StateLayerBrushProperty = AvaloniaProperty.Register<MdIconButton, IBrush?>(nameof(StateLayerBrush));

    static MdIconButton()
    {
        VariantProperty.Changed.AddClassHandler<MdIconButton>((x, _) => x.UpdatePseudoClasses());
        SizeProperty.Changed.AddClassHandler<MdIconButton>((x, _) => x.UpdatePseudoClasses());
        WidthModeProperty.Changed.AddClassHandler<MdIconButton>((x, _) => x.UpdatePseudoClasses());
        ShapeProperty.Changed.AddClassHandler<MdIconButton>((x, _) => x.UpdatePseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdIconButton>((x, _) => x.UpdateMotion());
    }
    public MdIconButton() => UpdatePseudoClasses();

    public MdIconButtonVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public MdButtonSize Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public MdIconButtonWidth WidthMode { get => GetValue(WidthModeProperty); set => SetValue(WidthModeProperty, value); }
    public MdButtonShape Shape { get => GetValue(ShapeProperty); set => SetValue(ShapeProperty, value); }
    public object? Icon { get => GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public double ContainerHeight { get => GetValue(ContainerHeightProperty); set => SetValue(ContainerHeightProperty, value); }
    public double ContainerWidth { get => GetValue(ContainerWidthProperty); set => SetValue(ContainerWidthProperty, value); }
    public double IconSize { get => GetValue(IconSizeProperty); set => SetValue(IconSizeProperty, value); }
    public CornerRadius ContainerCornerRadius { get => GetValue(ContainerCornerRadiusProperty); set => SetValue(ContainerCornerRadiusProperty, value); }
    public IBrush? StateLayerBrush { get => GetValue(StateLayerBrushProperty); set => SetValue(StateLayerBrushProperty, value); }

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
    }
}
