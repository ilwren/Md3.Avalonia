using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A segment in a single- or multi-select Material segmented button group.</summary>
[PseudoClasses(":has-icon", ":has-selected-icon", ":reduced-motion", ":no-motion")]
public sealed class MdSegmentedButton : ListBoxItem
{
    public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<MdSegmentedButton, object?>(nameof(Icon));
    public static readonly StyledProperty<object?> SelectedIconProperty = AvaloniaProperty.Register<MdSegmentedButton, object?>(nameof(SelectedIcon));
    public static readonly StyledProperty<object?> ValueProperty = AvaloniaProperty.Register<MdSegmentedButton, object?>(nameof(Value));
    public static readonly StyledProperty<CornerRadius> ContainerCornerRadiusProperty = AvaloniaProperty.Register<MdSegmentedButton, CornerRadius>(nameof(ContainerCornerRadius), new CornerRadius(0));

    private Border? _root;
    private Border? _state;
    private Control? _icon;
    private Control? _selectedIcon;

    static MdSegmentedButton()
    {
        IconProperty.Changed.AddClassHandler<MdSegmentedButton>((item, _) => item.UpdatePseudoClasses());
        SelectedIconProperty.Changed.AddClassHandler<MdSegmentedButton>((item, _) => item.UpdatePseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdSegmentedButton>((item, _) => item.UpdateMotion());
    }

    public MdSegmentedButton() => UpdatePseudoClasses();
    public object? Icon { get => GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public object? SelectedIcon { get => GetValue(SelectedIconProperty); set => SetValue(SelectedIconProperty, value); }
    public object? Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public CornerRadius ContainerCornerRadius { get => GetValue(ContainerCornerRadiusProperty); set => SetValue(ContainerCornerRadiusProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _root = e.NameScope.Find<Border>("PART_Root");
        _state = e.NameScope.Find<Border>("PART_State");
        _icon = e.NameScope.Find<Control>("PART_Icon");
        _selectedIcon = e.NameScope.Find<Control>("PART_SelectedIcon");
        UpdateMotion();
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateBrush(this, ForegroundProperty, MdMotionSpeed.Fast),
            MdMotionTransitions.CreateBrush(this, BackgroundProperty, MdMotionSpeed.Fast));
        if (_root is not null)
        {
            _root.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateBrush(this, BackgroundProperty, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateBrush(this, BorderBrushProperty, MdMotionSpeed.Fast));
        }
        foreach (var control in new[] { _state, _icon, _selectedIcon })
        {
            if (control is not null)
                control.Transitions = MdMotionTransitions.Collect(
                    MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":has-icon", Icon is not null);
        PseudoClasses.Set(":has-selected-icon", SelectedIcon is not null);
    }
}
