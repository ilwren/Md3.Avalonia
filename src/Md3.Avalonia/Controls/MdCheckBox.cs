using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Path = Avalonia.Controls.Shapes.Path;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A lookless Material Design 3 checkbox with selected, unselected, indeterminate and error states.
/// The theme is scoped to <see cref="MdCheckBox"/> and does not replace Avalonia's native
/// <see cref="CheckBox"/> theme.
/// </summary>
[PseudoClasses(":md-error", ":reduced-motion", ":no-motion")]
public class MdCheckBox : CheckBox
{
    private Border? _stateLayer;
    private Border? _container;
    private Path? _checkGlyph;
    private Border? _indeterminateGlyph;
    public static readonly StyledProperty<bool> IsErrorProperty =
        AvaloniaProperty.Register<MdCheckBox, bool>(nameof(IsError));

    public static readonly StyledProperty<IBrush?> StateLayerBrushProperty =
        AvaloniaProperty.Register<MdCheckBox, IBrush?>(nameof(StateLayerBrush));

    public static readonly StyledProperty<IBrush?> FocusIndicatorBrushProperty =
        AvaloniaProperty.Register<MdCheckBox, IBrush?>(nameof(FocusIndicatorBrush));

    static MdCheckBox()
    {
        IsErrorProperty.Changed.AddClassHandler<MdCheckBox>((checkBox, _) =>
            checkBox.UpdateErrorPseudoClass());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdCheckBox>((checkBox, _) => checkBox.UpdateMotion());
    }

    public MdCheckBox()
    {
        UpdateErrorPseudoClass();
    }

    /// <summary>Gets or sets whether the checkbox uses Material's error state colors.</summary>
    public bool IsError
    {
        get => GetValue(IsErrorProperty);
        set => SetValue(IsErrorProperty, value);
    }

    /// <summary>Gets or sets the semantic brush used by the circular interaction state layer.</summary>
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
        _stateLayer = e.NameScope.Find<Border>("PART_StateLayer");
        _container = e.NameScope.Find<Border>("PART_Container");
        _checkGlyph = e.NameScope.Find<Path>("PART_CheckGlyph");
        _indeterminateGlyph = e.NameScope.Find<Border>("PART_IndeterminateGlyph");
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
        if (_container is not null)
            _container.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateBrush(this, Border.BackgroundProperty, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateBrush(this, Border.BorderBrushProperty, MdMotionSpeed.Fast));
        if (_checkGlyph is not null)
            _checkGlyph.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateDouble(this, Path.StrokeDashOffsetProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
        if (_indeterminateGlyph is not null)
            _indeterminateGlyph.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
    }

    private void UpdateErrorPseudoClass() => PseudoClasses.Set(":md-error", IsError);
}
