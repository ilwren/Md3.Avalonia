using Avalonia.Automation.Peers;
using Avalonia.Controls;

namespace Md3.Avalonia.Controls;

/// <summary>Calendar collection host that exposes calendar semantics to platform automation.</summary>
public sealed class MdCalendarGrid : ItemsControl
{
    protected override AutomationPeer OnCreateAutomationPeer() => new MdCalendarGridAutomationPeer(this);
}

internal sealed class MdCalendarGridAutomationPeer(MdCalendarGrid owner) : ItemsControlAutomationPeer(owner)
{
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Calendar;
}
