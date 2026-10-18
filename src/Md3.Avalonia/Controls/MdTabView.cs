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

    private void OnFirstLayoutUpdated(object? sender, EventArgs e)
    {
        LayoutUpdated -= OnFirstLayoutUpdated;
        Dispatcher.UIThread.Post(RepairFirstFrame, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// First-frame cure for upstream Avalonia issue 22094: at non-100% Windows display scaling
    /// the bottom tab strip can be missing from the first frame (either never painted, or
    /// arranged against a stale pre-constraint size), and field reports show that ANY real
    /// repaint or resize heals it. Remote diagnosis is not always possible, so the repair is
    /// unconditional and layered: force a repaint of the strip/host/top level (plus a delayed
    /// second repaint), then perform a small alternating one-device-pixel width dance that
    /// provokes the same real WM_SIZE-driven full relayouts a manual resize would, restoring the
    /// original window size at the end. In healthy environments this is at most a sub-pixel-wide,
    /// sub-second startup jitter.
    /// </summary>
    private void RepairFirstFrame()
    {
        ShowDiagnostics();
        if (TopLevel.GetTopLevel(this) is { } topLevel)
        {
            InvalidateRepairTargets(topLevel);

            if (!_firstFrameRepairArmed)
            {
                _firstFrameRepairArmed = true;

                // A second, delayed repaint in case the first one races the initial composition.
                var repaintTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
                repaintTimer.Tick += (_, _) =>
                {
                    repaintTimer.Stop();
                    InvalidateRepairTargets(topLevel);
                };
                repaintTimer.Start();

                StartResizeNudges();
            }
        }
    }

    private bool _firstFrameRepairArmed;
    private int _resizeNudgesRemaining = 6;
    private bool _nudgePhase;

    private void InvalidateRepairTargets(TopLevel topLevel)
    {
        topLevel.InvalidateVisual();
        _tabStripBorder?.InvalidateVisual();
        (_tabStripBorder?.Parent as Visual)?.InvalidateVisual();
        _selectedContentHost?.InvalidateVisual();
    }

    /// <summary>
    /// Alternating one-device-pixel width nudges: four right after the first frame plus two on a
    /// slower timer, because the bad arrange can also settle late. Alternation guarantees at
    /// least one direction is not clamped by the window's Min/Max constraints, and an even count
    /// leaves the window exactly where it started.
    /// </summary>
    private void StartResizeNudges()
    {
        // Timer-paced on purpose: nudges fired back-to-back within one frame would be coalesced
        // (net zero change, no platform resize at all). Each tick is a separate, materialized
        // window size change, so every tick provokes a genuine WM_SIZE-driven relayout.
        NudgeWindowSize();
        var nudgeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        nudgeTimer.Tick += (_, _) =>
        {
            if (_resizeNudgesRemaining <= 0)
            {
                nudgeTimer.Stop();
                return;
            }

            NudgeWindowSize();
        };
        nudgeTimer.Start();
    }

    private void NudgeWindowSize()
    {
        if (_resizeNudgesRemaining <= 0 || TopLevel.GetTopLevel(this) is not { } topLevel)
        {
            return;
        }

        _resizeNudgesRemaining--;

        // A whole device pixel guarantees a real platform size change (sub-pixel dips can round
        // to the same physical size and skip the WM_SIZE-driven relayout entirely).
        var scaling = Math.Max(0.25, topLevel.RenderScaling);
        var pixel = 1.0 / scaling;
        topLevel.Width = topLevel.Bounds.Width + (_nudgePhase ? pixel : -pixel);
        _nudgePhase = !_nudgePhase;
    }

    // Offline field diagnostics: set MD3_DIAG=1 in the environment before launch and a small
    // always-on-top window reports the scaling, the relevant bounds and the repair activity in
    // place - no network, DevTools or account needed, a plain screenshot is enough.
    private static readonly bool DiagEnabled = Environment.GetEnvironmentVariable("MD3_DIAG") is "1" or "true";
    private Window? _diagWindow;
    private TextBlock? _diagText;

    private void ShowDiagnostics()
    {
        if (!DiagEnabled || _diagWindow is not null) return;

        _diagText = new TextBlock { FontSize = 12, FontFamily = new global::Avalonia.Media.FontFamily("Consolas,Menlo,monospace") };
        _diagWindow = new Window
        {
            Title = "Md3 diagnostics",
            Width = 460,
            Height = 230,
            Topmost = true,
            ShowInTaskbar = false,
            CanResize = false,
            Content = new Border
            {
                Background = global::Avalonia.Media.Brushes.Black,
                Padding = new Thickness(10),
                Child = _diagText,
            },
        };
        _diagText.Foreground = global::Avalonia.Media.Brushes.Lime;
        var diagTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        diagTimer.Tick += (_, _) => UpdateDiagnostics();
        diagTimer.Start();
        _diagWindow.Closed += (_, _) => diagTimer.Stop();
        _diagWindow.Show();
        UpdateDiagnostics();
    }

    private void UpdateDiagnostics()
    {
        if (_diagText is null) return;
        var tl = TopLevel.GetTopLevel(this);
        static string B(Visual? v) => v is null ? "null" : $"{v.Bounds.X:0.#},{v.Bounds.Y:0.#} {v.Bounds.Width:0.#}x{v.Bounds.Height:0.#}";
        _diagText.Text =
            $"scaling   : {tl?.RenderScaling.ToString("0.##") ?? "?"}\n" +
            $"window    : {B(tl)} (W={tl?.Width.ToString("0.##") ?? "?"})\n" +
            $"tabView   : {B(this)}\n" +
            $"strip     : {B(_tabStripBorder)}\n" +
            $"host      : {B(_selectedContentHost)}\n" +
            $"nudges    : fired {6 - _resizeNudgesRemaining}/6  repaintArmed={_firstFrameRepairArmed}\n" +
            $"placement : {TabStripPlacement}";
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
