using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>A fixed or horizontally scrollable Material tab row retaining native ListBox selection.</summary>
[PseudoClasses(":primary", ":secondary", ":fixed", ":scrollable", ":reduced-motion", ":no-motion")]
public sealed class MdTabs : ListBox
{
    public static readonly AttachedProperty<MdTabVariant> VariantProperty =
        AvaloniaProperty.RegisterAttached<MdTabs, StyledElement, MdTabVariant>(
            nameof(Variant), MdTabVariant.Primary, inherits: true);

    public static readonly StyledProperty<bool> IsScrollableProperty =
        AvaloniaProperty.Register<MdTabs, bool>(nameof(IsScrollable));

    private readonly TranslateTransform _indicatorTransform = new();
    private readonly MdSpatialSpringRunner _indicatorXRunner;
    private readonly MdSpatialSpringRunner _indicatorWidthRunner;
    private Border? _selectionIndicator;
    private double _lastIndicatorX = double.NaN;
    private double _lastIndicatorWidth = double.NaN;
    private int _indicatorVersion;

    static MdTabs()
    {
        VariantProperty.Changed.AddClassHandler<MdTabs>((tabs, _) =>
        {
            tabs.UpdatePseudoClasses();
            tabs.ScheduleIndicatorUpdate();
        });
        IsScrollableProperty.Changed.AddClassHandler<MdTabs>((tabs, _) =>
        {
            tabs.UpdatePseudoClasses();
            tabs.ScheduleIndicatorUpdate();
        });
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdTabs>((tabs, _) => tabs.UpdateMotion());
    }

    public MdTabs()
    {
        _indicatorXRunner = new MdSpatialSpringRunner(this, value => _indicatorTransform.X = value);
        _indicatorWidthRunner = new MdSpatialSpringRunner(this, value =>
        {
            if (_selectionIndicator is not null) _selectionIndicator.Width = value;
        });
        UpdatePseudoClasses();
        SelectionChanged += (_, _) => ScheduleIndicatorUpdate();
        LayoutUpdated += (_, _) => UpdateIndicatorGeometry();
    }

    public MdTabVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public bool IsScrollable
    {
        get => GetValue(IsScrollableProperty);
        set => SetValue(IsScrollableProperty, value);
    }

    public static MdTabVariant GetVariant(StyledElement element) => element.GetValue(VariantProperty);
    public static void SetVariant(StyledElement element, MdTabVariant value) => element.SetValue(VariantProperty, value);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _selectionIndicator = e.NameScope.Find<Border>("PART_SelectionIndicator");
        if (_selectionIndicator is not null)
            _selectionIndicator.RenderTransform = _indicatorTransform;
        UpdateMotion();
        ScheduleIndicatorUpdate();
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _indicatorXRunner.Stop();
        _indicatorWidthRunner.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is MdTabItem materialItem)
        {
            materialItem.SetCurrentValue(MdTabItem.UseSharedIndicatorProperty, true);
        }
        else if (container is ListBoxItem listItem &&
                 ResourceNodeExtensions.FindResource(this, "MdTabsGeneratedItemTheme") is ControlTheme theme)
        {
            listItem.SetCurrentValue(ThemeProperty, theme);
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":primary", Variant == MdTabVariant.Primary);
        PseudoClasses.Set(":secondary", Variant == MdTabVariant.Secondary);
        PseudoClasses.Set(":fixed", !IsScrollable);
        PseudoClasses.Set(":scrollable", IsScrollable);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_selectionIndicator is not null)
        {
            // Effects remain duration-based; critical indicator geometry uses the velocity-preserving runners.
            _selectionIndicator.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast));
        }
        _indicatorTransform.Transitions = null;
        ScheduleIndicatorUpdate();
    }

    private void ScheduleIndicatorUpdate()
    {
        var version = ++_indicatorVersion;
        Dispatcher.UIThread.Post(() =>
        {
            if (version == _indicatorVersion) UpdateIndicatorGeometry();
        }, DispatcherPriority.Render);
    }

    private void UpdateIndicatorGeometry()
    {
        if (_selectionIndicator is null) return;
        var selected = GetRealizedContainers().OfType<MdTabItem>().FirstOrDefault(item => item.IsSelected);
        var origin = selected?.TranslatePoint(default, this);
        if (selected is null || origin is null || selected.Bounds.Width <= 0)
        {
            _selectionIndicator.IsVisible = false;
            return;
        }

        var width = Variant == MdTabVariant.Secondary
            ? Math.Max(0, selected.Bounds.Width - 32)
            : 24;
        var x = origin.Value.X + (selected.Bounds.Width - width) / 2;
        if (Math.Abs(x - _lastIndicatorX) < 0.01 && Math.Abs(width - _lastIndicatorWidth) < 0.01)
            return;

        var firstGeometry = double.IsNaN(_lastIndicatorX) || double.IsNaN(_lastIndicatorWidth);
        _lastIndicatorX = x;
        _lastIndicatorWidth = width;
        _selectionIndicator.IsVisible = true;
        _selectionIndicator.Height = Variant == MdTabVariant.Secondary ? 2 : 3;

        var scheme = MdMotion.GetScheme(this);
        if (firstGeometry || scheme is MdMotionScheme.Reduced or MdMotionScheme.None)
        {
            if (scheme == MdMotionScheme.Reduced) _selectionIndicator.Opacity = 0;
            _indicatorXRunner.SnapTo(x);
            _indicatorWidthRunner.SnapTo(width);
            if (scheme == MdMotionScheme.Reduced)
            {
                var version = ++_indicatorVersion;
                Dispatcher.UIThread.Post(() =>
                {
                    if (version == _indicatorVersion && _selectionIndicator is not null)
                        _selectionIndicator.Opacity = 1;
                }, DispatcherPriority.Render);
            }
            else _selectionIndicator.Opacity = 1;
        }
        else
        {
            _indicatorXRunner.Retarget(x);
            _indicatorWidthRunner.Retarget(width);
            _selectionIndicator.Opacity = 1;
        }
    }
}
