using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A selectable destination in <see cref="MdNavigationBar"/>.</summary>
[PseudoClasses(":has-selected-icon", ":has-badge", ":stacked", ":horizontal", ":reduced-motion", ":no-motion")]
public sealed class MdNavigationBarItem : ListBoxItem
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<MdNavigationBarItem, object?>(nameof(Icon));
    public static readonly StyledProperty<object?> SelectedIconProperty =
        AvaloniaProperty.Register<MdNavigationBarItem, object?>(nameof(SelectedIcon));
    public static readonly StyledProperty<object?> LabelProperty =
        AvaloniaProperty.Register<MdNavigationBarItem, object?>(nameof(Label));
    public static readonly StyledProperty<object?> BadgeProperty =
        AvaloniaProperty.Register<MdNavigationBarItem, object?>(nameof(Badge));
    public static readonly StyledProperty<MdNavigationBarLayout> LayoutProperty =
        AvaloniaProperty.Register<MdNavigationBarItem, MdNavigationBarLayout>(nameof(Layout));

    private Border? _indicator;
    private Border? _stateLayer;
    private ContentPresenter? _icon;
    private ContentPresenter? _selectedIcon;

    static MdNavigationBarItem()
    {
        SelectedIconProperty.Changed.AddClassHandler<MdNavigationBarItem>((item, _) => item.UpdatePseudoClasses());
        BadgeProperty.Changed.AddClassHandler<MdNavigationBarItem>((item, _) => item.UpdatePseudoClasses());
        LayoutProperty.Changed.AddClassHandler<MdNavigationBarItem>((item, _) => item.UpdatePseudoClasses());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdNavigationBarItem>((item, _) => item.UpdateMotion());
    }

    public MdNavigationBarItem() => UpdatePseudoClasses();

    public object? Icon { get => GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public object? SelectedIcon { get => GetValue(SelectedIconProperty); set => SetValue(SelectedIconProperty, value); }
    public object? Label { get => GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public object? Badge { get => GetValue(BadgeProperty); set => SetValue(BadgeProperty, value); }
    public MdNavigationBarLayout Layout { get => GetValue(LayoutProperty); set => SetValue(LayoutProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _indicator = e.NameScope.Find<Border>("PART_Indicator");
        _stateLayer = e.NameScope.Find<Border>("PART_StateLayer");
        _icon = e.NameScope.Find<ContentPresenter>("PART_Icon");
        _selectedIcon = e.NameScope.Find<ContentPresenter>("PART_SelectedIcon");
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
        var iconTransitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        if (_icon is not null) _icon.Transitions = iconTransitions;
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
        PseudoClasses.Set(":stacked", Layout == MdNavigationBarLayout.Stacked);
        PseudoClasses.Set(":horizontal", Layout == MdNavigationBarLayout.Horizontal);
    }
}
