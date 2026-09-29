using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A Material 3 radio button preserving Avalonia grouping, command and automation behavior.</summary>
[PseudoClasses(":md-error", ":reduced-motion", ":no-motion")]
public sealed class MdRadioButton : RadioButton
{
    private Border? _stateLayer;
    private Border? _outerCircle;
    private Border? _dot;
    public static readonly StyledProperty<bool> IsErrorProperty =
        AvaloniaProperty.Register<MdRadioButton, bool>(nameof(IsError));

    public static readonly StyledProperty<IBrush?> StateLayerBrushProperty =
        AvaloniaProperty.Register<MdRadioButton, IBrush?>(nameof(StateLayerBrush));

    static MdRadioButton()
    {
        IsErrorProperty.Changed.AddClassHandler<MdRadioButton>((radio, _) =>
            radio.PseudoClasses.Set(":md-error", radio.IsError));
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdRadioButton>((radio, _) => radio.UpdateMotion());
    }

    public bool IsError
    {
        get => GetValue(IsErrorProperty);
        set => SetValue(IsErrorProperty, value);
    }

    public IBrush? StateLayerBrush
    {
        get => GetValue(StateLayerBrushProperty);
        set => SetValue(StateLayerBrushProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _stateLayer = e.NameScope.Find<Border>("PART_StateLayer");
        _outerCircle = e.NameScope.Find<Border>("PART_OuterCircle");
        _dot = e.NameScope.Find<Border>("PART_Dot");
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
        if (_outerCircle is not null)
            _outerCircle.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateBrush(this, Border.BorderBrushProperty, MdMotionSpeed.Fast));
        if (_dot is not null)
            _dot.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
    }
}
