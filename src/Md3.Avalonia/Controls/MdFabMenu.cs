using System.Threading;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>An M3 FAB menu for two to six related actions with reversible expand/collapse motion.</summary>
[PseudoClasses(":opening", ":open", ":closing", ":closed", ":primary", ":secondary", ":tertiary", ":left", ":right", ":expand-up", ":expand-down", ":invalid-item-count", ":reduced-motion", ":no-motion")]
public class MdFabMenu : ItemsControl
{
    private ItemsPresenter? _menuItems;
    private Control? _trigger;
    private Control? _focusBeforeOpen;

    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<MdFabMenu, bool>(
        nameof(IsOpen), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<bool> AreItemsVisibleProperty =
        AvaloniaProperty.Register<MdFabMenu, bool>(nameof(AreItemsVisible));
    public static readonly StyledProperty<MdFabColor> ColorStyleProperty =
        AvaloniaProperty.Register<MdFabMenu, MdFabColor>(nameof(ColorStyle), MdFabColor.PrimaryContainer);
    public static readonly StyledProperty<MdFabAlignment> AlignmentProperty =
        AvaloniaProperty.Register<MdFabMenu, MdFabAlignment>(nameof(Alignment), MdFabAlignment.End);
    public static readonly StyledProperty<MdFabMenuExpansionDirection> ExpansionDirectionProperty =
        AvaloniaProperty.Register<MdFabMenu, MdFabMenuExpansionDirection>(
            nameof(ExpansionDirection), MdFabMenuExpansionDirection.Up);
    public static readonly StyledProperty<object?> OpenIconProperty =
        AvaloniaProperty.Register<MdFabMenu, object?>(nameof(OpenIcon));
    public static readonly StyledProperty<object?> CloseIconProperty =
        AvaloniaProperty.Register<MdFabMenu, object?>(nameof(CloseIcon));

    private readonly DispatcherTimer _closeTimer;
    private CancellationTokenSource? _openFrameCancellation;
    private bool _isAttached;

    static MdFabMenu()
    {
        IsOpenProperty.Changed.AddClassHandler<MdFabMenu>((menu, _) => menu.UpdateOpenState());
        ColorStyleProperty.Changed.AddClassHandler<MdFabMenu>((menu, _) => menu.UpdateColorPseudoClasses());
        AlignmentProperty.Changed.AddClassHandler<MdFabMenu>((menu, _) => menu.UpdateAlignment());
        FlowDirectionProperty.Changed.AddClassHandler<MdFabMenu>((menu, _) => menu.UpdateAlignment());
        ExpansionDirectionProperty.Changed.AddClassHandler<MdFabMenu>((menu, _) => menu.UpdateExpansionDirection());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdFabMenu>((menu, _) => menu.UpdateMotion());
    }

    public MdFabMenu()
    {
        _closeTimer = new DispatcherTimer();
        _closeTimer.Tick += (_, _) => FinishClosing();
        UpdateColorPseudoClasses();
        UpdateAlignment();
        UpdateExpansionDirection();
        SetMotionPseudoClass(closed: true);
        AddHandler(Button.ClickEvent, OnDescendantButtonClick, RoutingStrategies.Bubble, handledEventsToo: true);
        LayoutUpdated += (_, _) =>
        {
            UpdateMenuItemGeometry();
            UpdateItemCountDiagnostic();
        };
    }

    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public bool AreItemsVisible { get => GetValue(AreItemsVisibleProperty); private set => SetCurrentValue(AreItemsVisibleProperty, value); }
    public MdFabColor ColorStyle { get => GetValue(ColorStyleProperty); set => SetValue(ColorStyleProperty, value); }
    public MdFabAlignment Alignment { get => GetValue(AlignmentProperty); set => SetValue(AlignmentProperty, value); }
    public MdFabMenuExpansionDirection ExpansionDirection { get => GetValue(ExpansionDirectionProperty); set => SetValue(ExpansionDirectionProperty, value); }
    public object? OpenIcon { get => GetValue(OpenIconProperty); set => SetValue(OpenIconProperty, value); }
    public object? CloseIcon { get => GetValue(CloseIconProperty); set => SetValue(CloseIconProperty, value); }

    /// <summary>Whether the menu contains the Material-recommended two through six actions.</summary>
    public bool HasRecommendedItemCount => Items.Count is >= 2 and <= 6;

    /// <summary>A non-throwing diagnostic that allows authoring tools to flag unsupported action counts.</summary>
    public string? ItemCountDiagnostic => HasRecommendedItemCount
        ? null
        : $"Material FAB menus should contain 2–6 actions; this menu contains {Items.Count}.";

    public void Show() => SetCurrentValue(IsOpenProperty, true);
    public void Dismiss() => SetCurrentValue(IsOpenProperty, false);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _menuItems = e.NameScope.Find<ItemsPresenter>("PART_MenuItems");
        _trigger = e.NameScope.Find<Control>("PART_Trigger");
        UpdateAlignment();
        UpdateExpansionDirection();
        UpdateMotion();
        UpdateItemCountDiagnostic();
        UpdateTriggerAutomation();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        UpdateItemCountDiagnostic();
        UpdateOpenState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        _openFrameCancellation?.Cancel();
        _openFrameCancellation?.Dispose();
        _openFrameCancellation = null;
        _closeTimer.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsOpen)
        {
            Dismiss();
            e.Handled = true;
            return;
        }

        if (e.Key is Key.Down or Key.Up or Key.Home or Key.End)
        {
            if (!IsOpen) Show();
            FocusAction(e.Key);
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void UpdateOpenState()
    {
        _closeTimer.Stop();
        _openFrameCancellation?.Cancel();
        _openFrameCancellation?.Dispose();
        _openFrameCancellation = null;
        UpdateItemCountDiagnostic();
        UpdateTriggerAutomation();
        if (IsOpen)
        {
            _focusBeforeOpen ??= TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Control;
            AreItemsVisible = true;
            SetMotionPseudoClass(opening: true);
            if (!_isAttached) return;

            var cancellation = _openFrameCancellation = new CancellationTokenSource();
            Dispatcher.UIThread.Post(() =>
            {
                if (!cancellation.IsCancellationRequested && _isAttached && IsOpen)
                {
                    SetMotionPseudoClass(open: true);
                    FocusFirstAction();
                }
                if (ReferenceEquals(_openFrameCancellation, cancellation))
                {
                    _openFrameCancellation.Dispose();
                    _openFrameCancellation = null;
                }
            }, DispatcherPriority.Loaded);
        }
        else if (AreItemsVisible)
        {
            RestoreFocusAfterClose();
            SetMotionPseudoClass(closing: true);
            var exitDuration = MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast);
            if (exitDuration <= TimeSpan.Zero)
            {
                FinishClosing();
            }
            else
            {
                _closeTimer.Interval = exitDuration;
                _closeTimer.Start();
            }
        }
        else
        {
            SetMotionPseudoClass(closed: true);
        }
    }

    private void FinishClosing()
    {
        _closeTimer.Stop();
        AreItemsVisible = false;
        SetMotionPseudoClass(closed: true);
    }

    private void SetMotionPseudoClass(bool opening = false, bool open = false, bool closing = false, bool closed = false)
    {
        PseudoClasses.Set(":opening", opening);
        PseudoClasses.Set(":open", open);
        PseudoClasses.Set(":closing", closing);
        PseudoClasses.Set(":closed", closed);
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_menuItems is not null)
            _menuItems.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, MaxHeightProperty, MdMotionKind.Spatial, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Fast),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty, MdMotionSpeed.Fast));
        if (!IsOpen && AreItemsVisible)
        {
            _closeTimer.Stop();
            var exitDuration = MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast);
            if (exitDuration <= TimeSpan.Zero) FinishClosing();
            else { _closeTimer.Interval = exitDuration; _closeTimer.Start(); }
        }
    }

    private void UpdateMenuItemGeometry()
    {
        var hAlign = Alignment.ResolvesLeft(FlowDirection) ? global::Avalonia.Layout.HorizontalAlignment.Left : global::Avalonia.Layout.HorizontalAlignment.Right;
        foreach (var item in this.GetVisualDescendants().OfType<MdExtendedFloatingActionButton>())
        {
            item.SetCurrentValue(MdExtendedFloatingActionButton.ContainerHeightProperty, 56);
            item.SetCurrentValue(MdExtendedFloatingActionButton.ContainerCornerRadiusProperty, new CornerRadius(28));
            item.SetCurrentValue(MdExtendedFloatingActionButton.IconSizeProperty, 24);
            item.SetCurrentValue(MdExtendedFloatingActionButton.IconSpacingProperty, 8);
            item.SetCurrentValue(HorizontalAlignmentProperty, hAlign);
        }
        if (Items is { } items)
        {
            foreach (var item in items.OfType<Control>())
            {
                item.SetCurrentValue(HorizontalAlignmentProperty, hAlign);
            }
        }
    }

    private void UpdateItemCountDiagnostic()
    {
        PseudoClasses.Set(":invalid-item-count", !HasRecommendedItemCount);
        if (!HasRecommendedItemCount && string.IsNullOrWhiteSpace(AutomationProperties.GetHelpText(this)))
            AutomationProperties.SetHelpText(this, ItemCountDiagnostic);
        else if (HasRecommendedItemCount && AutomationProperties.GetHelpText(this)?.StartsWith("Material FAB menus", StringComparison.Ordinal) == true)
            AutomationProperties.SetHelpText(this, null);
    }

    private void UpdateTriggerAutomation()
    {
        if (_trigger is null) return;
        AutomationProperties.SetName(_trigger, IsOpen ? "Close action menu" : "Open action menu");
        AutomationProperties.SetHelpText(_trigger, ItemCountDiagnostic);
    }

    private IReadOnlyList<Control> GetActionControls() => _menuItems is null
        ? []
        : _menuItems.GetVisualDescendants().OfType<Control>()
            .Where(control => control is Button && control.IsVisible && control.IsEffectivelyEnabled && control.Focusable)
            .ToArray();

    private void FocusFirstAction()
    {
        var actions = GetActionControls();
        if (actions.Count > 0) actions[0].Focus(NavigationMethod.Tab);
    }

    private void FocusAction(Key key)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!IsOpen) return;
            var actions = GetActionControls();
            if (actions.Count == 0) return;
            var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
            var current = -1;
            for (var index = 0; index < actions.Count; index++)
            {
                if (ReferenceEquals(actions[index], focused) ||
                    focused is Visual visual && visual.GetVisualAncestors().Contains(actions[index]))
                {
                    current = index;
                    break;
                }
            }
            var target = key switch
            {
                Key.Home => 0,
                Key.End => actions.Count - 1,
                Key.Up when current < 0 => actions.Count - 1,
                Key.Up => (current - 1 + actions.Count) % actions.Count,
                _ => (current + 1) % actions.Count
            };
            actions[target].Focus(NavigationMethod.Directional);
        }, DispatcherPriority.Input);
    }

    private void RestoreFocusAfterClose()
    {
        var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Visual;
        var focusWasInActions = focused is not null && _menuItems is not null &&
            (ReferenceEquals(focused, _menuItems) || focused.GetVisualAncestors().Contains(_menuItems));
        var target = focusWasInActions ? _trigger : _focusBeforeOpen;
        _focusBeforeOpen = null;
        if (target is { IsEffectivelyEnabled: true, IsVisible: true })
            Dispatcher.UIThread.Post(() => target.Focus(NavigationMethod.Unspecified), DispatcherPriority.Input);
    }

    private void OnDescendantButtonClick(object? sender, RoutedEventArgs e)
    {
        if (!IsOpen || e.Source is not Visual source || _trigger is null) return;
        if (ReferenceEquals(source, _trigger) || source.GetVisualAncestors().Contains(_trigger)) return;
        if (_menuItems is not null && (ReferenceEquals(source, _menuItems) || source.GetVisualAncestors().Contains(_menuItems)))
            Dismiss();
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new MdFabMenuAutomationPeer(this);

    private void UpdateColorPseudoClasses()
    {
        PseudoClasses.Set(":primary", ColorStyle is MdFabColor.PrimaryContainer or MdFabColor.Primary);
        PseudoClasses.Set(":secondary", ColorStyle is MdFabColor.SecondaryContainer or MdFabColor.Secondary);
        PseudoClasses.Set(":tertiary", ColorStyle is MdFabColor.TertiaryContainer or MdFabColor.Tertiary);
    }

    private void UpdateAlignment()
    {
        var isLeft = Alignment.ResolvesLeft(FlowDirection);
        PseudoClasses.Set(":left", isLeft);
        PseudoClasses.Set(":right", !isLeft);
        var hAlign = isLeft ? global::Avalonia.Layout.HorizontalAlignment.Left : global::Avalonia.Layout.HorizontalAlignment.Right;
        if (_menuItems is not null)
        {
            _menuItems.HorizontalAlignment = hAlign;
            if (_menuItems.Panel is Control panel)
            {
                panel.HorizontalAlignment = hAlign;
            }
        }
        if (_trigger is not null)
        {
            _trigger.HorizontalAlignment = hAlign;
        }
        UpdateTransformOrigin();
        UpdateMenuItemGeometry();
    }

    private void UpdateExpansionDirection()
    {
        var expandsDown = ExpansionDirection == MdFabMenuExpansionDirection.Down;
        PseudoClasses.Set(":expand-up", !expandsDown);
        PseudoClasses.Set(":expand-down", expandsDown);
        UpdateTransformOrigin();
        InvalidateArrange();
    }

    private void UpdateTransformOrigin()
    {
        if (_menuItems is null) return;
        var x = Alignment.ResolvesLeft(FlowDirection) ? 0d : 1d;
        var y = ExpansionDirection == MdFabMenuExpansionDirection.Down ? 0d : 1d;
        _menuItems.RenderTransformOrigin = new RelativePoint(x, y, RelativeUnit.Relative);
    }
}

internal sealed class MdFabMenuAutomationPeer(MdFabMenu owner)
    : ItemsControlAutomationPeer(owner), IExpandCollapseProvider
{
    private MdFabMenu MenuOwner => (MdFabMenu)Owner;

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Menu;
    protected override bool IsControlElementCore() => true;
    protected override bool IsContentElementCore() => true;

    public ExpandCollapseState ExpandCollapseState => MenuOwner.IsOpen
        ? ExpandCollapseState.Expanded
        : ExpandCollapseState.Collapsed;

    public bool ShowsMenu => true;

    public void Expand()
    {
        EnsureEnabled();
        MenuOwner.Show();
    }

    public void Collapse()
    {
        EnsureEnabled();
        MenuOwner.Dismiss();
    }
}
