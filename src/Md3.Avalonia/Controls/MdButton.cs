using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A lookless Material Design 3 button. Its default theme is scoped to <see cref="MdButton"/>
/// and never replaces the theme of Avalonia's <see cref="Button"/>.
/// </summary>
[PseudoClasses(
    ":filled", ":tonal", ":outlined", ":text", ":elevated",
    ":xsmall", ":small", ":medium", ":large", ":xlarge",
    ":round", ":square", ":has-leading-icon", ":has-trailing-icon",
    ":reduced-motion", ":no-motion")]
public class MdButton : Button
{
    private Border? _container;
    private Border? _stateLayer;
    public static readonly StyledProperty<MdButtonVariant> VariantProperty =
        AvaloniaProperty.Register<MdButton, MdButtonVariant>(
            nameof(Variant),
            MdButtonVariant.Filled);

    public static readonly StyledProperty<MdButtonSize> SizeProperty =
        AvaloniaProperty.Register<MdButton, MdButtonSize>(
            nameof(Size),
            MdButtonSize.Small);

    public static readonly StyledProperty<MdButtonShape> ShapeProperty =
        AvaloniaProperty.Register<MdButton, MdButtonShape>(
            nameof(Shape),
            MdButtonShape.Round);

    public static readonly StyledProperty<object?> LeadingIconProperty =
        AvaloniaProperty.Register<MdButton, object?>(nameof(LeadingIcon));

    public static readonly StyledProperty<object?> TrailingIconProperty =
        AvaloniaProperty.Register<MdButton, object?>(nameof(TrailingIcon));

    public static readonly StyledProperty<double> ContainerHeightProperty =
        AvaloniaProperty.Register<MdButton, double>(nameof(ContainerHeight), 40d);

    public static readonly StyledProperty<CornerRadius> ContainerCornerRadiusProperty =
        AvaloniaProperty.Register<MdButton, CornerRadius>(
            nameof(ContainerCornerRadius),
            new CornerRadius(20));

    public static readonly StyledProperty<double> IconSizeProperty =
        AvaloniaProperty.Register<MdButton, double>(nameof(IconSize), 20d);

    public static readonly StyledProperty<double> IconSpacingProperty =
        AvaloniaProperty.Register<MdButton, double>(nameof(IconSpacing), 8d);

    public static readonly StyledProperty<IBrush?> StateLayerBrushProperty =
        AvaloniaProperty.Register<MdButton, IBrush?>(nameof(StateLayerBrush));

    public static readonly StyledProperty<IBrush?> FocusIndicatorBrushProperty =
        AvaloniaProperty.Register<MdButton, IBrush?>(nameof(FocusIndicatorBrush));

    static MdButton()
    {
        VariantProperty.Changed.AddClassHandler<MdButton>((button, _) => button.UpdateVariantPseudoClasses());
        SizeProperty.Changed.AddClassHandler<MdButton>((button, _) => button.UpdateSizePseudoClasses());
        ShapeProperty.Changed.AddClassHandler<MdButton>((button, _) => button.UpdateShapePseudoClasses());
        LeadingIconProperty.Changed.AddClassHandler<MdButton>((button, _) => button.UpdateIconPseudoClasses());
        TrailingIconProperty.Changed.AddClassHandler<MdButton>((button, _) => button.UpdateIconPseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdButton>((button, _) => button.UpdateMotion());
    }

    public MdButton()
    {
        UpdateVariantPseudoClasses();
        UpdateSizePseudoClasses();
        UpdateShapePseudoClasses();
        UpdateIconPseudoClasses();
    }

    /// <summary>Gets or sets the button color configuration.</summary>
    public MdButtonVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Gets or sets the Material 3 Expressive size.</summary>
    public MdButtonSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Gets or sets the resting container shape.</summary>
    public MdButtonShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    /// <summary>Gets or sets optional content displayed before the label.</summary>
    public object? LeadingIcon
    {
        get => GetValue(LeadingIconProperty);
        set => SetValue(LeadingIconProperty, value);
    }

    /// <summary>Gets or sets optional content displayed after the label.</summary>
    public object? TrailingIcon
    {
        get => GetValue(TrailingIconProperty);
        set => SetValue(TrailingIconProperty, value);
    }

    /// <summary>Gets or sets the visual container height in DIPs.</summary>
    public double ContainerHeight
    {
        get => GetValue(ContainerHeightProperty);
        set => SetValue(ContainerHeightProperty, value);
    }

    /// <summary>Gets or sets the current visual container corner radius.</summary>
    public CornerRadius ContainerCornerRadius
    {
        get => GetValue(ContainerCornerRadiusProperty);
        set => SetValue(ContainerCornerRadiusProperty, value);
    }

    /// <summary>Gets or sets the slot size for leading and trailing icons.</summary>
    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>Gets or sets spacing between icon slots and the label.</summary>
    public double IconSpacing
    {
        get => GetValue(IconSpacingProperty);
        set => SetValue(IconSpacingProperty, value);
    }

    /// <summary>Gets or sets the semantic state-layer brush.</summary>
    public IBrush? StateLayerBrush
    {
        get => GetValue(StateLayerBrushProperty);
        set => SetValue(StateLayerBrushProperty, value);
    }

    /// <summary>Gets or sets the keyboard focus indicator brush.</summary>
    public IBrush? FocusIndicatorBrush
    {
        get => GetValue(FocusIndicatorBrushProperty);
        set => SetValue(FocusIndicatorBrushProperty, value);
    }

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
                MdMotionTransitions.CreateCornerRadius(this, Border.CornerRadiusProperty));
        if (_stateLayer is not null)
            _stateLayer.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
    }

    private void UpdateVariantPseudoClasses()
    {
        var value = Variant;
        PseudoClasses.Set(":filled", value == MdButtonVariant.Filled);
        PseudoClasses.Set(":tonal", value == MdButtonVariant.Tonal);
        PseudoClasses.Set(":outlined", value == MdButtonVariant.Outlined);
        PseudoClasses.Set(":text", value == MdButtonVariant.Text);
        PseudoClasses.Set(":elevated", value == MdButtonVariant.Elevated);
    }

    private void UpdateSizePseudoClasses()
    {
        var value = Size;
        PseudoClasses.Set(":xsmall", value == MdButtonSize.ExtraSmall);
        PseudoClasses.Set(":small", value == MdButtonSize.Small);
        PseudoClasses.Set(":medium", value == MdButtonSize.Medium);
        PseudoClasses.Set(":large", value == MdButtonSize.Large);
        PseudoClasses.Set(":xlarge", value == MdButtonSize.ExtraLarge);
    }

    private void UpdateShapePseudoClasses()
    {
        var isSquare = Shape == MdButtonShape.Square;
        PseudoClasses.Set(":round", !isSquare);
        PseudoClasses.Set(":square", isSquare);
    }

    private void UpdateIconPseudoClasses()
    {
        PseudoClasses.Set(":has-leading-icon", LeadingIcon is not null);
        PseudoClasses.Set(":has-trailing-icon", TrailingIcon is not null);
    }

}
