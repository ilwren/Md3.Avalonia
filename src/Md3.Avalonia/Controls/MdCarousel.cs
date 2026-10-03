using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A horizontally scrolling Material 3 carousel. Direct manipulation remains 1:1; keyline
/// navigation, keyboard and autoplay changes use a token-driven snap settle and resize the
/// realized large, medium and small items without replacing their content. Tap selection does not
/// move the keyline or resize cards.
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
    public static readonly StyledProperty<double> SmallItemWidthProperty =
        AvaloniaProperty.Register<MdCarousel, double>(nameof(SmallItemWidth), 56);
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
    private int _layoutAnchorIndex;
    private bool _isPointerSelection;
    private bool _updatingSelection;

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
        ItemSpacingProperty.Changed.AddClassHandler<MdCarousel>((carousel, _) =>
        {
            carousel.UpdateRealizedContainers();
            carousel.ScheduleSelectedSettle();
        });
        SmallItemWidthProperty.Changed.AddClassHandler<MdCarousel>((carousel, _) =>
        {
            carousel.UpdateRealizedContainers();
            carousel.ScheduleSelectedSettle();
        });
        IsAutoPlayProperty.Changed.AddClassHandler<MdCarousel>((carousel, _) => carousel.UpdateAutoPlay());
        AutoPlayIntervalProperty.Changed.AddClassHandler<MdCarousel>((carousel, _) => carousel.UpdateAutoPlay());
        ControllerProperty.Changed.AddClassHandler<MdCarousel>((carousel, _) => carousel.UpdateController());
        FlowDirectionProperty.Changed.AddClassHandler<MdCarousel>((carousel, _) =>
        {
            carousel.UpdateRealizedContainers();
            carousel.ScheduleSelectedSettle();
        });
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
        AddHandler(PointerPressedEvent, PreviewCarouselPointerPressed, RoutingStrategies.Tunnel);
        SelectionChanged += (_, _) =>
        {
            // Material carousel taps invoke/select content but do not resize it. Geometry follows
            // the keyline/scroll anchor, matching Flutter CarouselView.onTap semantics.
            if (!_isPointerSelection && !_updatingSelection && SelectedIndex >= 0)
                SetLayoutAnchor(SelectedIndex, settle: true);
            UpdateSelectionAnnouncement();
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
        AutomationProperties.SetName(this, "Carousel");
        AutomationProperties.SetLiveSetting(this, AutomationLiveSetting.Polite);
        UpdatePseudoClasses();
    }

    public MdCarouselVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public double ItemWidth { get => GetValue(ItemWidthProperty); set => SetValue(ItemWidthProperty, value); }
    public double ItemHeight { get => GetValue(ItemHeightProperty); set => SetValue(ItemHeightProperty, value); }
    public double ItemSpacing { get => GetValue(ItemSpacingProperty); set => SetValue(ItemSpacingProperty, value); }
    /// <summary>Preferred small keyline width, constrained to Material's 40–56 DIP range.</summary>
    public double SmallItemWidth { get => GetValue(SmallItemWidthProperty); set => SetValue(SmallItemWidthProperty, value); }
    public bool IsAutoPlay { get => GetValue(IsAutoPlayProperty); set => SetValue(IsAutoPlayProperty, value); }
    public TimeSpan AutoPlayInterval { get => GetValue(AutoPlayIntervalProperty); set => SetValue(AutoPlayIntervalProperty, value); }
    public bool IsInfiniteLoop { get => GetValue(IsInfiniteLoopProperty); set => SetValue(IsInfiniteLoopProperty, value); }
    public bool PauseOnPointerOver { get => GetValue(PauseOnPointerOverProperty); set => SetValue(PauseOnPointerOverProperty, value); }
    public MdCarouselController? Controller { get => GetValue(ControllerProperty); set => SetValue(ControllerProperty, value); }

    public bool MoveNext() => SelectRelative(1);
    public bool MovePrevious() => SelectRelative(-1);
    /// <summary>Moves the Material keyline layout to an item and settles it through the native scroller.</summary>
    public bool ScrollTo(int index)
    {
        if (index < 0 || index >= ItemCount) return false;
        SelectAndSettle(index);
        return true;
    }
    public void StartAutoPlay() => SetCurrentValue(IsAutoPlayProperty, true);
    public void StopAutoPlay() => SetCurrentValue(IsAutoPlayProperty, false);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
        _layoutAnchorIndex = Math.Max(0, SelectedIndex);
        UpdateMotion();
        UpdateRealizedContainers();
        ScheduleSelectedSettle();
    }

    private void PreviewCarouselPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isPointerSelection = e.Source is Visual source && source.GetVisualAncestors()
            .Prepend(source)
            .OfType<ListBoxItem>()
            .Any();
        Dispatcher.UIThread.Post(() => _isPointerSelection = false, DispatcherPriority.Background);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        CancelSettleForDirectManipulation();
        base.OnPointerPressed(e);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (Math.Abs(e.NewSize.Width - e.PreviousSize.Width) < 0.01) return;
        UpdateRealizedContainers();
        ScheduleSelectedSettle();
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
        var rtl = FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft;
        if (e.Key == Key.Right && (rtl ? MovePrevious() : MoveNext())) e.Handled = true;
        else if (e.Key == Key.Left && (rtl ? MoveNext() : MovePrevious())) e.Handled = true;
        else if (e.Key == Key.Home && ScrollTo(0)) e.Handled = true;
        else if (e.Key == Key.End && ScrollTo(ItemCount - 1)) e.Handled = true;
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
        container.SetCurrentValue(MarginProperty, FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft
            ? new Thickness(ItemSpacing, 0, 0, 0)
            : new Thickness(0, 0, ItemSpacing, 0));
        if (container is ListBoxItem item)
        {
            if (ResourceNodeExtensions.FindResource(this, "MdCarouselItemTheme") is ControlTheme theme)
                item.SetCurrentValue(ThemeProperty, theme);
            item.SetCurrentValue(ListBoxItem.CornerRadiusProperty,
                Variant == MdCarouselVariant.Uncontained ? new CornerRadius(12) : new CornerRadius(28));
            AutomationProperties.SetHelpText(item, $"Item {index + 1} of {ItemCount}");
        }
    }

    private double GetTargetWidth(int index)
    {
        var preferredLarge = Math.Max(0, ItemWidth);
        if (index < 0 || Variant == MdCarouselVariant.Uncontained) return preferredLarge;

        var preferredSmall = Math.Min(preferredLarge, Math.Clamp(SmallItemWidth, 40, 56));
        var viewport = GetLayoutViewportWidth();
        var large = preferredLarge;
        var small = preferredSmall;

        if (viewport > 0 && ItemCount > 1)
        {
            // The Material arrangements fit their visible keylines into the viewport instead of
            // allowing the authored large extent to push the preview keyline out of sight.
            switch (Variant)
            {
                case MdCarouselVariant.MultiBrowse when ItemCount >= 3:
                {
                    // Android's reference strategy targets medium=(large+small)/2. Solving
                    // large+medium+small+2*spacing <= viewport gives this maximum large extent.
                    var availableItemsWidth = Math.Max(0, viewport - 2 * Math.Max(0, ItemSpacing));
                    var maximumLarge = 2 * availableItemsWidth / 3 - small;
                    large = Math.Min(preferredLarge, Math.Max(small, maximumLarge));
                    break;
                }
                case MdCarouselVariant.CenterAligned when ItemCount >= 3:
                    large = Math.Min(preferredLarge,
                        Math.Max(small, viewport - 2 * small - 2 * Math.Max(0, ItemSpacing)));
                    break;
                case MdCarouselVariant.MultiBrowse:
                case MdCarouselVariant.Hero:
                case MdCarouselVariant.CenterAligned:
                    large = Math.Min(preferredLarge,
                        Math.Max(small, viewport - small - Math.Max(0, ItemSpacing)));
                    break;
            }
            small = Math.Min(small, large);
        }

        var distance = Math.Abs(index - _layoutAnchorIndex);
        if (distance == 0) return large;
        return Variant switch
        {
            // Material Components Android derives the medium keyline from the midpoint between
            // the current small and large arrangement sizes.
            MdCarouselVariant.MultiBrowse when distance == 1 => (large + small) / 2,
            MdCarouselVariant.MultiBrowse => small,
            MdCarouselVariant.Hero => small,
            MdCarouselVariant.CenterAligned => small,
            _ => large
        };
    }

    private double GetLayoutViewportWidth()
    {
        var width = _scrollViewer?.Viewport.Width ?? 0;
        if (width <= 0 || double.IsNaN(width) || double.IsInfinity(width)) width = Bounds.Width;
        if (width <= 0 || double.IsNaN(width) || double.IsInfinity(width)) return 0;
        return Math.Max(0, width - Padding.Left - Padding.Right);
    }

    private bool SelectRelative(int delta)
    {
        var count = ItemCount;
        if (count <= 0) return false;
        var next = _layoutAnchorIndex + delta;
        if (next < 0 || next >= count)
        {
            if (!IsInfiniteLoop) return false;
            next = (next % count + count) % count;
        }
        SelectAndSettle(next);
        return true;
    }

    private void SelectAndSettle(int index)
    {
        _updatingSelection = true;
        try { SetCurrentValue(SelectedIndexProperty, index); }
        finally { _updatingSelection = false; }
        SetLayoutAnchor(index, settle: true);
    }

    private void SetLayoutAnchor(int index, bool settle)
    {
        if (index < 0 || index >= ItemCount) return;
        _layoutAnchorIndex = index;
        UpdateRealizedContainers();
        if (settle) ScheduleSelectedSettle();
    }

    private void ScheduleSelectedSettle()
    {
        var version = ++_settleVersion;
        Dispatcher.UIThread.Post(() =>
        {
            if (version != _settleVersion) return;

            // ItemCount and the virtualizing viewport can settle after the first container is
            // prepared. Recompute the realized keylines now, then let one layout pass commit their
            // Bounds before calculating the native scroll offset.
            UpdateRealizedContainers();
            Dispatcher.UIThread.Post(() =>
            {
                if (version == _settleVersion) SettleSelectedItem();
            }, DispatcherPriority.Render);
        }, DispatcherPriority.Render);
    }

    private void SettleSelectedItem()
    {
        if (_scrollViewer is null || _layoutAnchorIndex < 0 || _layoutAnchorIndex >= ItemCount) return;
        var selected = ContainerFromIndex(_layoutAnchorIndex) as Control;
        var origin = selected?.TranslatePoint(default, this);
        var selectedWidth = selected?.Bounds.Width ?? GetTargetWidth(_layoutAnchorIndex);
        var rtl = FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft;
        var maxOffset = Math.Max(0, _scrollViewer.Extent.Width - _scrollViewer.Viewport.Width);
        double target;
        if (origin is not null)
        {
            var desiredX = Variant == MdCarouselVariant.CenterAligned
                ? Math.Max(0, (Bounds.Width - selectedWidth) / 2)
                : rtl ? Math.Max(0, Bounds.Width - Padding.Right - selectedWidth) : Padding.Left;
            target = _scrollViewer.Offset.X + origin.Value.X - desiredX;
        }
        else
        {
            target = Padding.Left;
            for (var index = 0; index < _layoutAnchorIndex; index++)
                target += GetTargetWidth(index) + ItemSpacing;
            if (Variant == MdCarouselVariant.CenterAligned)
                target -= Math.Max(0, (Bounds.Width - selectedWidth) / 2);
            if (rtl) target = maxOffset - target;
        }

        target = Math.Clamp(target, 0, maxOffset);
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
        var rtl = FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft;
        var targetX = Variant == MdCarouselVariant.CenterAligned
            ? Bounds.Width / 2
            : rtl ? Bounds.Width - Padding.Right : Padding.Left;
        var nearest = containers
            .Select(control => new
            {
                Control = control,
                Point = control.TranslatePoint(new Point(
                    Variant == MdCarouselVariant.CenterAligned ? control.Bounds.Width / 2 : rtl ? control.Bounds.Width : 0, 0), this)
            })
            .Where(value => value.Point is not null)
            .OrderBy(value => Math.Abs(value.Point!.Value.X - targetX))
            .FirstOrDefault();
        if (nearest is null) return;
        var index = IndexFromContainer(nearest.Control);
        if (index >= 0 && index != _layoutAnchorIndex) SelectAndSettle(index);
        else ScheduleSelectedSettle();
    }

    private void UpdateSelectionAnnouncement()
    {
        AutomationProperties.SetHelpText(this, SelectedIndex >= 0
            ? $"Selected item {SelectedIndex + 1} of {ItemCount}"
            : $"{ItemCount} items");
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
