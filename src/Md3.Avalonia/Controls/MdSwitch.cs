using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A Material switch with native ToggleButton command, keyboard and automation semantics.</summary>
[PseudoClasses(":show-icons", ":reduced-motion", ":no-motion")]
public class MdSwitch : ToggleButton
{
    private Border? _track;
    private Border? _thumbState;
    private Border? _thumb;
    public static readonly StyledProperty<bool> ShowIconsProperty =
        AvaloniaProperty.Register<MdSwitch, bool>(nameof(ShowIcons));

    static MdSwitch()
    {
        ShowIconsProperty.Changed.AddClassHandler<MdSwitch>((control, _) =>
            control.PseudoClasses.Set(":show-icons", control.ShowIcons));
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdSwitch>((control, _) => control.UpdateMotion());
    }

    public MdSwitch() => PseudoClasses.Set(":show-icons", ShowIcons);

    public bool ShowIcons { get => GetValue(ShowIconsProperty); set => SetValue(ShowIconsProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _track = e.NameScope.Find<Border>("PART_Track");
        _thumbState = e.NameScope.Find<Border>("PART_ThumbState");
        _thumb = e.NameScope.Find<Border>("PART_Thumb");
        UpdateMotion();
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_track is not null)
            _track.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateBrush(this, Border.BackgroundProperty, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateBrush(this, Border.BorderBrushProperty, MdMotionSpeed.Fast));
        if (_thumbState is not null)
            _thumbState.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
        if (_thumb is not null)
            _thumb.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateBrush(this, Border.BackgroundProperty, MdMotionSpeed.Fast));
    }
}
