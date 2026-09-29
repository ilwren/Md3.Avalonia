using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>Template part used by <see cref="MdSlider"/> to carry Material value-indicator state.</summary>
[PseudoClasses(":show-value", ":reduced-motion", ":no-motion")]
public sealed class MdSliderThumb : Thumb
{
    public static readonly StyledProperty<bool> ShowValueIndicatorProperty =
        AvaloniaProperty.Register<MdSliderThumb, bool>(nameof(ShowValueIndicator));
    public static readonly StyledProperty<string> DisplayValueProperty =
        AvaloniaProperty.Register<MdSliderThumb, string>(nameof(DisplayValue), string.Empty);

    private Control? _valueIndicator;
    private Control? _stateLayer;

    static MdSliderThumb()
    {
        ShowValueIndicatorProperty.Changed.AddClassHandler<MdSliderThumb>((thumb, _) => thumb.UpdatePseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdSliderThumb>((thumb, _) => thumb.UpdateMotion());
    }

    public MdSliderThumb() => UpdatePseudoClasses();

    public bool ShowValueIndicator
    {
        get => GetValue(ShowValueIndicatorProperty);
        set => SetValue(ShowValueIndicatorProperty, value);
    }

    public string DisplayValue
    {
        get => GetValue(DisplayValueProperty);
        set => SetValue(DisplayValueProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _valueIndicator = e.NameScope.Find<Control>("PART_ThumbValue");
        _stateLayer = e.NameScope.Find<Control>("PART_ThumbStateLayer");
        UpdateMotion();
    }

    private void UpdatePseudoClasses() => PseudoClasses.Set(":show-value", ShowValueIndicator);

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        foreach (var control in new[] { _valueIndicator, _stateLayer })
        {
            if (control is not null)
            {
                control.Transitions = MdMotionTransitions.Collect(
                    MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
                    MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
            }
        }
    }
}
