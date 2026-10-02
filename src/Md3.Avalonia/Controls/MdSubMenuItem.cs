using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

/// <summary>A command-capable Material menu item that opens a keyboard-accessible cascading submenu.</summary>
[PseudoClasses(":submenu-open", ":rtl")]
public sealed class MdSubMenuItem : MdMenuItem
{
    public static readonly StyledProperty<object?> SubmenuProperty =
        AvaloniaProperty.Register<MdSubMenuItem, object?>(nameof(Submenu));
    public static readonly StyledProperty<bool> IsSubmenuOpenProperty =
        AvaloniaProperty.Register<MdSubMenuItem, bool>(nameof(IsSubmenuOpen),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    private Popup? _submenuPopup;
    private MdMenu? _subMenu;

    static MdSubMenuItem()
    {
        IsSubmenuOpenProperty.Changed.AddClassHandler<MdSubMenuItem>((item, _) => item.UpdateSubmenuState());
        SubmenuProperty.Changed.AddClassHandler<MdSubMenuItem>((item, _) => item.SubscribeToSubmenu());
        FlowDirectionProperty.Changed.AddClassHandler<MdSubMenuItem>((item, _) => item.UpdateDirection());
    }

    public MdSubMenuItem()
    {
        Click += (_, _) => SetCurrentValue(IsSubmenuOpenProperty, !IsSubmenuOpen);
        PointerEntered += (_, _) =>
        {
            if (IsEffectivelyEnabled) SetCurrentValue(IsSubmenuOpenProperty, true);
        };
        UpdateDirection();
    }

    public object? Submenu
    {
        get => GetValue(SubmenuProperty);
        set => SetValue(SubmenuProperty, value);
    }

    public bool IsSubmenuOpen
    {
        get => GetValue(IsSubmenuOpenProperty);
        set => SetValue(IsSubmenuOpenProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachSubmenu();
        base.OnApplyTemplate(e);
        _submenuPopup = e.NameScope.Find<Popup>("PART_SubmenuPopup");
        SubscribeToSubmenu();
        UpdateDirection();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeToSubmenu();
        UpdateDirection();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeFromSubmenu();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var openKey = FlowDirection == FlowDirection.RightToLeft ? Key.Left : Key.Right;
        var closeKey = FlowDirection == FlowDirection.RightToLeft ? Key.Right : Key.Left;
        if (e.Key == openKey && Submenu is not null)
        {
            SetCurrentValue(IsSubmenuOpenProperty, true);
            FocusSubmenu();
            e.Handled = true;
            return;
        }
        if ((e.Key == closeKey || e.Key == Key.Escape) && IsSubmenuOpen)
        {
            SetCurrentValue(IsSubmenuOpenProperty, false);
            Focus();
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new MdSubMenuItemAutomationPeer(this);

    private void UpdateSubmenuState()
    {
        PseudoClasses.Set(":submenu-open", IsSubmenuOpen);
        if (IsSubmenuOpen) FocusSubmenu();
    }

    private void FocusSubmenu() => Dispatcher.UIThread.Post(() =>
    {
        if (IsSubmenuOpen) (Submenu as MdMenu ?? _subMenu)?.FocusFirstItem();
    }, DispatcherPriority.Loaded);

    private void UpdateDirection()
    {
        var rtl = FlowDirection == FlowDirection.RightToLeft;
        PseudoClasses.Set(":rtl", rtl);
        if (_submenuPopup is not null)
        {
            _submenuPopup.Placement = rtl
                ? PlacementMode.LeftEdgeAlignedTop
                : PlacementMode.RightEdgeAlignedTop;
            _submenuPopup.HorizontalOffset = rtl ? 4 : -4;
        }
    }

    private void OnSubmenuDismissRequested(object? sender, EventArgs e)
    {
        SetCurrentValue(IsSubmenuOpenProperty, false);
        if (sender is MdMenu { IsHierarchyDismissal: true })
        {
            this.FindAncestorOfType<MdMenu>()?.RequestHierarchyDismissal();
        }
        else
        {
            Focus();
        }
    }

    private void SubscribeToSubmenu()
    {
        var menu = Submenu as MdMenu;
        if (ReferenceEquals(menu, _subMenu)) return;
        if (_subMenu is not null) _subMenu.DismissRequested -= OnSubmenuDismissRequested;
        _subMenu = menu;
        if (_subMenu is not null) _subMenu.DismissRequested += OnSubmenuDismissRequested;
    }

    private void UnsubscribeFromSubmenu()
    {
        if (_subMenu is not null) _subMenu.DismissRequested -= OnSubmenuDismissRequested;
        _subMenu = null;
    }

    private void DetachSubmenu()
    {
        UnsubscribeFromSubmenu();
        _submenuPopup = null;
    }
}

internal sealed class MdSubMenuItemAutomationPeer(MdSubMenuItem owner)
    : MdMenuItemAutomationPeer(owner), IExpandCollapseProvider
{
    private MdSubMenuItem SubmenuOwner => (MdSubMenuItem)Owner;

    public ExpandCollapseState ExpandCollapseState => SubmenuOwner.IsSubmenuOpen
        ? ExpandCollapseState.Expanded
        : ExpandCollapseState.Collapsed;

    public bool ShowsMenu => true;

    public void Expand()
    {
        EnsureEnabled();
        SubmenuOwner.SetCurrentValue(MdSubMenuItem.IsSubmenuOpenProperty, true);
    }

    public void Collapse()
    {
        EnsureEnabled();
        SubmenuOwner.SetCurrentValue(MdSubMenuItem.IsSubmenuOpenProperty, false);
    }
}
