using Avalonia.Automation.Peers;

namespace Md3.Avalonia.Controls;

internal class MdMenuItemAutomationPeer(MdMenuItem owner) : ButtonAutomationPeer(owner)
{
    protected MdMenuItem MenuOwner => (MdMenuItem)Owner;

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.MenuItem;

    protected override string? GetNameCore()
    {
        var name = base.GetNameCore();
        return string.IsNullOrWhiteSpace(name) && MenuOwner.Content is string text ? text : name;
    }
}
