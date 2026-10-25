using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A Material 3 menu surface with roving focus, disabled-item skipping, Home/End, type-ahead,
/// activation and Escape dismissal behavior.
/// </summary>
public sealed class MdMenu : ItemsControl
{
    private string _typeAhead = string.Empty;
    private DateTimeOffset _lastTypeAhead;
    private bool _isHierarchyDismissal;

    public MdMenu()
    {
        AddHandler(Button.ClickEvent, OnItemClick, RoutingStrategies.Bubble);
        AddHandler(InputElement.GotFocusEvent, OnItemGotFocus, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    /// <summary>Raised when Escape/Tab is pressed or a command item is invoked.</summary>
    public event EventHandler? DismissRequested;

    internal bool IsHierarchyDismissal => _isHierarchyDismissal;

    /// <summary>Moves keyboard focus to the first enabled, visible menu item.</summary>
    public bool FocusFirstItem() => FocusBoundary(first: true);

    /// <summary>Moves keyboard focus to the last enabled, visible menu item.</summary>
    public bool FocusLastItem() => FocusBoundary(first: false);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down:
                e.Handled = MoveFocus(1);
                break;
            case Key.Up:
                e.Handled = MoveFocus(-1);
                break;
            case Key.Home:
                e.Handled = FocusFirstItem();
                break;
            case Key.End:
                e.Handled = FocusLastItem();
                break;
            case Key.Escape:
                DismissRequested?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                break;
            case Key.Tab:
                RequestHierarchyDismissal();
                e.Handled = true;
                break;
            default:
                e.Handled = TryTypeAhead(e.Key);
                break;
        }

        if (!e.Handled) base.OnKeyDown(e);
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new MdMenuAutomationPeer(this);

    private List<MdMenuItem> GetFocusableItems()
    {
        var result = this.GetVisualDescendants().OfType<MdMenuItem>()
            .Where(item => item.IsVisible && item.IsEffectivelyEnabled && item.Focusable)
            .Distinct()
            .ToList();
        if (result.Count > 0) return result;
        return Items.OfType<MdMenuItem>()
            .Where(item => item.IsVisible && item.IsEffectivelyEnabled && item.Focusable)
            .ToList();
    }

    private bool FocusBoundary(bool first)
    {
        var items = GetFocusableItems();
        if (items.Count == 0) return false;
        var target = first ? items[0] : items[^1];
        SetRovingItem(items, target);
        return target.Focus(NavigationMethod.Directional);
    }

    private bool MoveFocus(int direction)
    {
        var items = GetFocusableItems();
        if (items.Count == 0) return false;
        var focusedIndex = items.FindIndex(item => item.IsKeyboardFocusWithin);
        var targetIndex = focusedIndex < 0
            ? (direction > 0 ? 0 : items.Count - 1)
            : (focusedIndex + direction + items.Count) % items.Count;
        var target = items[targetIndex];
        SetRovingItem(items, target);
        return target.Focus(NavigationMethod.Directional);
    }

    private bool TryTypeAhead(Key key)
    {
        var symbol = key.ToString();
        if (symbol.Length != 1 || !char.IsLetterOrDigit(symbol[0])) return false;

        var now = DateTimeOffset.UtcNow;
        if (now - _lastTypeAhead > TimeSpan.FromMilliseconds(700)) _typeAhead = string.Empty;
        _lastTypeAhead = now;
        _typeAhead += symbol;

        var items = GetFocusableItems();
        var match = items.FirstOrDefault(item => GetItemText(item)
            .StartsWith(_typeAhead, StringComparison.CurrentCultureIgnoreCase));
        if (match is null && _typeAhead.Length > 1)
        {
            _typeAhead = symbol;
            match = items.FirstOrDefault(item => GetItemText(item)
                .StartsWith(_typeAhead, StringComparison.CurrentCultureIgnoreCase));
        }
        if (match is null) return false;
        SetRovingItem(items, match);
        return match.Focus(NavigationMethod.Directional);
    }

    private void OnItemGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (e.Source is MdMenuItem item)
            SetRovingItem(GetFocusableItems(), item);
    }

    private static void SetRovingItem(IEnumerable<MdMenuItem> items, MdMenuItem active)
    {
        foreach (var item in items)
            KeyboardNavigation.SetIsTabStop(item, ReferenceEquals(item, active));
    }

    private static string GetItemText(MdMenuItem item) => item.Content switch
    {
        string text => text,
        TextBlock textBlock => textBlock.Text ?? string.Empty,
        _ => item.Content?.ToString() ?? string.Empty
    };

    internal void RequestHierarchyDismissal()
    {
        _isHierarchyDismissal = true;
        try
        {
            DismissRequested?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _isHierarchyDismissal = false;
        }
    }

    private void OnItemClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is MdMenuItem and not MdSubMenuItem)
            RequestHierarchyDismissal();
    }
}

internal sealed class MdMenuAutomationPeer(MdMenu owner) : ItemsControlAutomationPeer(owner)
{
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Menu;
}
