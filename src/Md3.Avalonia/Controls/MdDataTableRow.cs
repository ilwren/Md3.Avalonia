using Avalonia.Automation.Peers;
using Avalonia.Controls;

namespace Md3.Avalonia.Controls;

/// <summary>A selectable row for <see cref="MdDataTable"/> with arbitrary column content.</summary>
public sealed class MdDataTableRow : ListBoxItem
{
    protected override AutomationPeer OnCreateAutomationPeer() => new MdDataTableRowAutomationPeer(this);
}

internal sealed class MdDataTableRowAutomationPeer(MdDataTableRow owner) : ListItemAutomationPeer(owner)
{
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.DataItem;
}
