using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A Material tab strip and selected-content host with native <see cref="TabControl"/> semantics.
/// Content is presented only by the required PART_SelectedContentHost; this avoids re-parenting the
/// selected visual through a second transition host when tabs are changed rapidly.
/// </summary>
[PseudoClasses(":scrollable", ":fixed", ":reduced-motion", ":no-motion")]
public sealed class MdTabView : TabControl
{
    public static readonly StyledProperty<bool> IsScrollableProperty =
        AvaloniaProperty.Register<MdTabView, bool>(nameof(IsScrollable));

    public static readonly StyledProperty<MdTabVariant> VariantProperty =
        AvaloniaProperty.Register<MdTabView, MdTabVariant>(nameof(Variant), MdTabVariant.Secondary);

    // Retained as a read-only compatibility property. The template intentionally uses
    // PART_SelectedContentHost directly rather than presenting this object a second time.
    public static readonly DirectProperty<MdTabView, object?> AnimatedSelectedContentProperty =
        AvaloniaProperty.RegisterDirect<MdTabView, object?>(nameof(AnimatedSelectedContent), view => view.AnimatedSelectedContent);

    private readonly TranslateTransform _contentTransform = new();
    private object? _animatedSelectedContent;
    private ContentPresenter? _selectedContentHost;
    private int _contentAnimationVersion;
    private int _lastSelectedIndex = -1;

    static MdTabView()
    {
        IsScrollableProperty.Changed.AddClassHandler<MdTabView>((view, _) => view.UpdatePseudoClasses());
        VariantProperty.Changed.AddClassHandler<MdTabView>((view, _) => view.PushVariant());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdTabView>((view, _) => view.UpdateMotion());
    }

    public MdTabView()
    {
        UpdatePseudoClasses();
        SelectionChanged += OnSelectionChanged;
        LayoutUpdated += OnFirstLayoutUpdated;
    }

    public bool IsScrollable
    {
        get => GetValue(IsScrollableProperty);
        set => SetValue(IsScrollableProperty, value);
    }

    public object? AnimatedSelectedContent => _animatedSelectedContent;

    /// <summary>
    /// Material tab style applied to every item: <see cref="MdTabVariant.Primary"/> renders the
    /// text-first look with a full-width active indicator; <see cref="MdTabVariant.Secondary"/>
    /// (default) renders icon+label with the short centered indicator. This unifies the style
    /// dimension previously spread across the separate MdTabs/MdTabsView controls.
    /// </summary>
    public MdTabVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is MdTabViewItem tabItem) tabItem.SetCurrentValue(MdTabViewItem.VariantProperty, Variant);
    }

    private void PushVariant()
    {
        foreach (var tabItem in this.GetVisualDescendants().OfType<MdTabViewItem>())
        {
            tabItem.SetCurrentValue(MdTabViewItem.VariantProperty, Variant);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _selectedContentHost = e.NameScope.Find<ContentPresenter>("PART_SelectedContentHost");
        if (_selectedContentHost is not null) _selectedContentHost.RenderTransform = _contentTransform;
        _tabStripBorder = e.NameScope.Find<Border>("PART_TabStripBorder");
        _lastSelectedIndex = SelectedIndex;
        UpdateMotion();
        SetAndRaise(AnimatedSelectedContentProperty, ref _animatedSelectedContent, SelectedContent);
    }

    private Border? _tabStripBorder;
    private int _placementNudgesRemaining = 2;

    private void OnFirstLayoutUpdated(object? sender, EventArgs e)
    {
        LayoutUpdated -= OnFirstLayoutUpdated;
        Dispatcher.UIThread.Post(VerifyStripPlacement, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Upstream Avalonia issue 22094 can leave a Bottom-docked tab strip invisible on the first
    /// frame at any non-100% Windows display scaling (125/150/200%) or with window size
    /// constraints: the fill sibling is arranged over the whole client area and paints above the
    /// strip, while the strip's own bounds can look perfectly correct - only a manual resize
    /// re-runs the layout that repairs it. After the first layout the strip rect and the
    /// strip/host overlap are verified, and when either is wrong an imperceptible one-time,
    /// one-device-pixel width nudge forces the same full relayout a resize would.
    /// </summary>
    private void VerifyStripPlacement()
    {
        if (_tabStripBorder is not { } strip || _selectedContentHost is not { } host || Bounds.Height <= 0)
        {
            return;
        }

        var stripBroken = strip.Bounds.Height < 1 ||
                          strip.Bounds.Y < -0.5 ||
                          strip.Bounds.Y + strip.Bounds.Height > Bounds.Height + 0.5;
        var overlap = host.Bounds.Intersect(strip.Bounds);
        var paintedOver = overlap.Width > 1 && overlap.Height > 1;
        if (!stripBroken && !paintedOver) return;

        if (TopLevel.GetTopLevel(this) is { } topLevel && _placementNudgesRemaining > 0)
        {
            _placementNudgesRemaining--;

            // A whole device pixel guarantees a real platform size change (sub-pixel dips can
            // round to the same physical size and skip the WM_SIZE-driven relayout).
            var scaling = Math.Max(0.25, topLevel.RenderScaling);
            topLevel.Width = topLevel.Bounds.Width + 1.0 / scaling;
            Dispatcher.UIThread.Post(VerifyStripPlacement, DispatcherPriority.Loaded);
        }
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SetAndRaise(AnimatedSelectedContentProperty, ref _animatedSelectedContent, SelectedContent);
        if (_selectedContentHost is not { } host)
            return;

        // A single presenter receives a fade-through plus a short directional shared-axis move.
        // Reduced keeps only the fade; None commits synchronously. Versioning coalesces bursts.
        var direction = SelectedIndex >= _lastSelectedIndex ? 1 : -1;
        _lastSelectedIndex = SelectedIndex;
        var scheme = MdMotion.GetScheme(this);
        if (scheme == MdMotionScheme.None)
        {
            _contentTransform.X = 0;
            host.Opacity = 1;
            return;
        }

        var version = ++_contentAnimationVersion;
        host.Opacity = 0;
        _contentTransform.X = scheme == MdMotionScheme.Reduced ? 0 : direction * 16;
        Dispatcher.UIThread.Post(() =>
        {
            if (version == _contentAnimationVersion && ReferenceEquals(host, _selectedContentHost))
            {
                _contentTransform.X = 0;
                host.Opacity = 1;
            }
        }, DispatcherPriority.Render);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":scrollable", IsScrollable);
        PseudoClasses.Set(":fixed", !IsScrollable);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_selectedContentHost is not null)
        {
            _selectedContentHost.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects));
        }
        _contentTransform.Transitions = MdMotionTransitions.Collect(
            MdMotionTransitions.CreateDouble(this, TranslateTransform.XProperty, MdMotionKind.Spatial));
        if (scheme is MdMotionScheme.Reduced or MdMotionScheme.None) _contentTransform.X = 0;
    }
}
