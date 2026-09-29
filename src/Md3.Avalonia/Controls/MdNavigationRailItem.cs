using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A selectable destination within <see cref="MdNavigationRail"/>.</summary>
[PseudoClasses(":has-selected-icon", ":has-badge", ":expanded", ":collapsed", ":reduced-motion", ":no-motion")]
public sealed class MdNavigationRailItem : ListBoxItem
{
    public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<MdNavigationRailItem, object?>(nameof(Icon));
    public static readonly StyledProperty<object?> SelectedIconProperty = AvaloniaProperty.Register<MdNavigationRailItem, object?>(nameof(SelectedIcon));
    public static readonly StyledProperty<object?> LabelProperty = AvaloniaProperty.Register<MdNavigationRailItem, object?>(nameof(Label));
    public static readonly StyledProperty<object?> BadgeProperty = AvaloniaProperty.Register<MdNavigationRailItem, object?>(nameof(Badge));
    public static readonly StyledProperty<bool> IsExpandedProperty = AvaloniaProperty.Register<MdNavigationRailItem, bool>(nameof(IsExpanded));

    private Border? _indicator;
    private Border? _stateLayer;
    private Control? _icon;
    private Control? _selectedIcon;

    static MdNavigationRailItem()
    {
        SelectedIconProperty.Changed.AddClassHandler<MdNavigationRailItem>((item, _) => item.UpdatePseudoClasses());
        BadgeProperty.Changed.AddClassHandler<MdNavigationRailItem>((item, _) => item.UpdatePseudoClasses());
        IsExpandedProperty.Changed.AddClassHandler<MdNavigationRailItem>((item, _) => item.UpdatePseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdNavigationRailItem>((item, _) => item.UpdateMotion());
    }

    public MdNavigationRailItem() => UpdatePseudoClasses();
    public object? Icon { get => GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public object? SelectedIcon { get => GetValue(SelectedIconProperty); set => SetValue(SelectedIconProperty, value); }
    public object? Label { get => GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public object? Badge { get => GetValue(BadgeProperty); set => SetValue(BadgeProperty, value); }
    public bool IsExpanded { get => GetValue(IsExpandedProperty); set => SetValue(IsExpandedProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _indicator = e.NameScope.Find<Border>("PART_Indicator");
        _stateLayer = e.NameScope.Find<Border>("PART_State");
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
            MdMotionTransitions.CreateBrush(this, ForegroundProperty, MdMotionSpeed.Fast));
        if (_indicator is not null)
        {
            _indicator.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateBrush(this, BackgroundProperty, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
        }
        if (_stateLayer is not null)
        {
            _stateLayer.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        }
        if (_icon is not null)
        {
            _icon.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        }
        if (_selectedIcon is not null)
        {
            _selectedIcon.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":has-selected-icon", SelectedIcon is not null);
        PseudoClasses.Set(":has-badge", Badge is not null);
        PseudoClasses.Set(":expanded", IsExpanded);
        PseudoClasses.Set(":collapsed", !IsExpanded);
    }
}
