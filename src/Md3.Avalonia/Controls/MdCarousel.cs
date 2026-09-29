using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A horizontally scrolling Material 3 carousel. Direct manipulation remains 1:1; selection,
/// keyboard and autoplay changes use a token-driven snap settle and resize the realized large,
/// medium and small items without replacing their content.
/// </summary>
[PseudoClasses(":multi-browse", ":hero", ":center-aligned", ":uncontained", ":reduced-motion", ":no-motion")]
public sealed class MdCarousel : ListBox
{
    public static readonly StyledProperty<MdCarouselVariant> VariantProperty =
        AvaloniaProperty.Register<MdCarousel, MdCarouselVariant>(nameof(Variant));
    public static readonly StyledProperty<double> ItemWidthProperty =
        AvaloniaProperty.Register<MdCarousel, double>(nameof(ItemWidth), 240);
    public static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<MdCarousel, double>(nameof(ItemHeight), 220);
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<MdCarousel, double>(nameof(ItemSpacing), 8);
    public static readonly StyledProperty<bool> IsAutoPlayProperty =
        AvaloniaProperty.Register<MdCarousel, bool>(nameof(IsAutoPlay));
    public static readonly StyledProperty<TimeSpan> AutoPlayIntervalProperty =
        AvaloniaProperty.Register<MdCarousel, TimeSpan>(nameof(AutoPlayInterval), TimeSpan.FromSeconds(4));
    public static readonly StyledProperty<bool> IsInfiniteLoopProperty =
        AvaloniaProperty.Register<MdCarousel, bool>(nameof(IsInfiniteLoop));
    public static readonly StyledProperty<bool> PauseOnPointerOverProperty =
        AvaloniaProperty.Register<MdCarousel, bool>(nameof(PauseOnPointerOver), true);
    public static readonly StyledProperty<MdCarouselController?> ControllerProperty =
        AvaloniaProperty.Register<MdCarousel, MdCarouselController?>(nameof(Controller));

    private readonly DispatcherTimer _autoPlayTimer = new();
    private readonly DispatcherTimer _wheelSnapTimer = new();
    private readonly DispatcherTimer _settleCleanupTimer = new();
    private bool _pointerOver;
    private MdCarouselController? _attachedController;
    private ScrollViewer? _scrollViewer;
    private int _settleVersion;

    static MdCarousel()
    {
        VariantProperty.Changed.AddClassHandler<MdCarousel>((carousel, _) =>
        {
            carousel.UpdatePseudoClasses();
            carousel.UpdateRealizedContainers();
            carousel.ScheduleSelectedSettle();
        });
        ItemWidthProperty.Changed.AddClassHandler<MdCarousel>((carousel, _) =>
        {
            carousel.UpdateRealizedContainers();
            carousel.ScheduleSelectedSettle();
        });
        ItemHeightProperty.Changed.AddClassHandler<MdCarousel>((carousel, _) => carousel.UpdateRealizedContainers());
        ItemSpacingProperty.Changed.AddClassHandler<MdCarousel>((carousel, _) => carousel.UpdateRealizedContainers());
        IsAutoPlayProperty.Changed.AddClassHandler<MdCarousel>((carousel, _) => carousel.UpdateAutoPlay());
        AutoPlayIntervalProperty.Changed.AddClassHandler<MdCarousel>((carousel, _) => carousel.UpdateAutoPlay());
        ControllerProperty.Changed.AddClassHandler<MdCarousel>((carousel, _) => carousel.UpdateController());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdCarousel>((carousel, _) => carousel.UpdateMotion());
    }

    public MdCarousel()
    {
        _autoPlayTimer.Tick += (_, _) => MoveNext();
        _wheelSnapTimer.Tick += (_, _) =>
        {
            _wheelSnapTimer.Stop();
            SnapToNearestRealizedItem();
        };
        _settleCleanupTimer.Tick += (_, _) =>
        {
            _settleCleanupTimer.Stop();
            if (_scrollViewer is not null) _scrollViewer.Transitions = null;
        };
        SelectionChanged += (_, _) =>
        {
            UpdateRealizedContainers();
            ScheduleSelectedSettle();
        };
        ScrollGesture += (_, _) => CancelSettleForDirectManipulation();
        ScrollGestureEnded += (_, _) => SnapToNearestRealizedItem();
        PointerWheelChanged += (_, _) =>
        {
            CancelSettleForDirectManipulation();
            _wheelSnapTimer.Stop();
            _wheelSnapTimer.Interval = TimeSpan.FromMilliseconds(120);
            _wheelSnapTimer.Start();
        };
        UpdatePseudoClasses();
    }

    public MdCarouselVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public double ItemWidth { get => GetValue(ItemWidthProperty); set => SetValue(ItemWidthProperty, value); }
    public double ItemHeight { get => GetValue(ItemHeightProperty); set => SetValue(ItemHeightProperty, value); }
    public double ItemSpacing { get => GetValue(ItemSpacingProperty); set => SetValue(ItemSpacingProperty, value); }
    public bool IsAutoPlay { get => GetValue(IsAutoPlayProperty); set => SetValue(IsAutoPlayProperty, value); }
    public TimeSpan AutoPlayInterval { get => GetValue(AutoPlayIntervalProperty); set => SetValue(AutoPlayIntervalProperty, value); }
    public bool IsInfiniteLoop { get => GetValue(IsInfiniteLoopProperty); set => SetValue(IsInfiniteLoopProperty, value); }
    public bool PauseOnPointerOver { get => GetValue(PauseOnPointerOverProperty); set => SetValue(PauseOnPointerOverProperty, value); }
    public MdCarouselController? Controller { get => GetValue(ControllerProperty); set => SetValue(ControllerProperty, value); }

    public bool MoveNext() => SelectRelative(1);
    public bool MovePrevious() => SelectRelative(-1);
    public void StartAutoPlay() => SetCurrentValue(IsAutoPlayProperty, true);
    public void StopAutoPlay() => SetCurrentValue(IsAutoPlayProperty, false);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
        UpdateMotion();
        ScheduleSelectedSettle();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        CancelSettleForDirectManipulation();
        base.OnPointerPressed(e);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _pointerOver = true;
        UpdateAutoPlay();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _pointerOver = false;
        UpdateAutoPlay();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateAutoPlay();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _autoPlayTimer.Stop();
        _wheelSnapTimer.Stop();
        _settleCleanupTimer.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Right && MoveNext()) e.Handled = true;
        else if (e.Key == Key.Left && MovePrevious()) e.Handled = true;
        else base.OnKeyDown(e);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        PrepareContainer(container, index);
    }

    private void UpdateRealizedContainers()
    {
        foreach (var container in GetRealizedContainers())
            PrepareContainer(container, IndexFromContainer(container));
    }

    private void PrepareContainer(Control container, int index)
    {
        container.Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, WidthProperty, MdMotionKind.Spatial));
        container.SetCurrentValue(WidthProperty, GetTargetWidth(index));
        container.SetCurrentValue(HeightProperty, ItemHeight);
        container.SetCurrentValue(MarginProperty, new Thickness(0, 0, ItemSpacing, 0));
        if (container is ListBoxItem item && ResourceNodeExtensions.FindResource(this, "MdCarouselItemTheme") is ControlTheme theme)
        {
            item.SetCurrentValue(ThemeProperty, theme);
            item.SetCurrentValue(ListBoxItem.CornerRadiusProperty,
                Variant == MdCarouselVariant.Uncontained ? new CornerRadius(12) : new CornerRadius(28));
        }
    }

    private double GetTargetWidth(int index)
    {
        if (index < 0 || SelectedIndex < 0 || Variant == MdCarouselVariant.Uncontained) return ItemWidth;
        var distance = Math.Abs(index - SelectedIndex);
        if (distance == 0) return ItemWidth;
        return Variant switch
        {
            MdCarouselVariant.MultiBrowse when distance == 1 => Math.Max(96, ItemWidth * 0.72),
            MdCarouselVariant.MultiBrowse => Math.Max(56, ItemWidth * 0.48),
            MdCarouselVariant.Hero => Math.Max(56, ItemWidth * 0.30),
            MdCarouselVariant.CenterAligned => Math.Max(96, ItemWidth * 0.72),
            _ => ItemWidth
        };
    }

    private bool SelectRelative(int delta)
    {
        var count = ItemCount;
        if (count <= 0) return false;
        var next = SelectedIndex < 0 ? 0 : SelectedIndex + delta;
        if (next < 0 || next >= count)
        {
            if (!IsInfiniteLoop) return false;
            next = (next % count + count) % count;
        }
        SelectedIndex = next;
        return true;
    }

    private void ScheduleSelectedSettle()
    {
        var version = ++_settleVersion;
        Dispatcher.UIThread.Post(() =>
        {
            if (version == _settleVersion) SettleSelectedItem();
        }, DispatcherPriority.Render);
    }

    private void SettleSelectedItem()
    {
        if (_scrollViewer is null || SelectedIndex < 0) return;
        var selected = ContainerFromIndex(SelectedIndex) as Control;
        var origin = selected?.TranslatePoint(default, this);
        var selectedWidth = selected?.Bounds.Width ?? GetTargetWidth(SelectedIndex);
        double target;
        if (origin is not null)
        {
            var desiredX = Variant == MdCarouselVariant.CenterAligned
                ? Math.Max(0, (Bounds.Width - selectedWidth) / 2)
                : Padding.Left;
            target = _scrollViewer.Offset.X + origin.Value.X - desiredX;
        }
        else
        {
            target = Padding.Left;
            for (var index = 0; index < SelectedIndex; index++)
                target += GetTargetWidth(index) + ItemSpacing;
            if (Variant == MdCarouselVariant.CenterAligned)
                target -= Math.Max(0, (Bounds.Width - selectedWidth) / 2);
        }

        target = Math.Clamp(target, 0, Math.Max(0, _scrollViewer.Extent.Width - _scrollViewer.Viewport.Width));
        var spec = MdMotion.Resolve(this, MdMotionKind.Spatial, MdMotionSpeed.Default);
        _scrollViewer.Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateVector(this, ScrollViewer.OffsetProperty));
        _scrollViewer.Offset = new Vector(target, _scrollViewer.Offset.Y);
        if (spec.IsEnabled)
        {
            _settleCleanupTimer.Stop();
            _settleCleanupTimer.Interval = spec.Duration + TimeSpan.FromMilliseconds(20);
            _settleCleanupTimer.Start();
        }
        else
        {
            _scrollViewer.Transitions = null;
        }
    }

    private void CancelSettleForDirectManipulation()
    {
        ++_settleVersion;
        _settleCleanupTimer.Stop();
        if (_scrollViewer is not null) _scrollViewer.Transitions = null;
    }

    private void SnapToNearestRealizedItem()
    {
        var containers = GetRealizedContainers().Where(control => control.Bounds.Width > 0).ToArray();
        if (containers.Length == 0) return;
        var targetX = Variant == MdCarouselVariant.CenterAligned ? Bounds.Width / 2 : Padding.Left;
        var nearest = containers
            .Select(control => new
            {
                Control = control,
                Point = control.TranslatePoint(new Point(
                    Variant == MdCarouselVariant.CenterAligned ? control.Bounds.Width / 2 : 0, 0), this)
            })
            .Where(value => value.Point is not null)
            .OrderBy(value => Math.Abs(value.Point!.Value.X - targetX))
            .FirstOrDefault();
        if (nearest is null) return;
        var index = IndexFromContainer(nearest.Control);
        if (index >= 0 && index != SelectedIndex) SelectedIndex = index;
        else ScheduleSelectedSettle();
    }

    private void UpdateController()
    {
        _attachedController?.Detach(this);
        _attachedController = Controller;
        _attachedController?.Attach(this);
    }

    private void UpdateAutoPlay()
    {
        _autoPlayTimer.Stop();
        _autoPlayTimer.Interval = AutoPlayInterval <= TimeSpan.Zero ? TimeSpan.FromSeconds(4) : AutoPlayInterval;
        var allowsAmbientMotion = MdMotion.GetScheme(this) is MdMotionScheme.Expressive or MdMotionScheme.Standard;
        if (allowsAmbientMotion && IsAutoPlay && ItemCount > 1 &&
            (!PauseOnPointerOver || !_pointerOver) && this.IsAttachedToVisualTree())
        {
            _autoPlayTimer.Start();
        }
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        UpdateRealizedContainers();
        UpdateAutoPlay();
        if (scheme is MdMotionScheme.Reduced or MdMotionScheme.None)
            CancelSettleForDirectManipulation();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":multi-browse", Variant == MdCarouselVariant.MultiBrowse);
        PseudoClasses.Set(":hero", Variant == MdCarouselVariant.Hero);
        PseudoClasses.Set(":center-aligned", Variant == MdCarouselVariant.CenterAligned);
        PseudoClasses.Set(":uncontained", Variant == MdCarouselVariant.Uncontained);
    }
}
