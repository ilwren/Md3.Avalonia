using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace Md3.Avalonia.Controls;

/// <summary>Groups expansion panels and optionally enforces radio-style single expansion.</summary>
public sealed class MdExpansionPanelList : ItemsControl
{
    public static readonly StyledProperty<bool> AllowMultipleProperty =
        AvaloniaProperty.Register<MdExpansionPanelList, bool>(nameof(AllowMultiple), true);

    private bool _coercing;

    public bool AllowMultiple { get => GetValue(AllowMultipleProperty); set => SetValue(AllowMultipleProperty, value); }

    internal void NotifyExpanded(MdExpansionPanel expanded)
    {
        if (AllowMultiple || _coercing) return;
        _coercing = true;
        try
        {
            foreach (var panel in Items.OfType<MdExpansionPanel>()
                         .Concat(this.GetLogicalDescendants().OfType<MdExpansionPanel>())
                         .Distinct()
                         .Where(panel => !ReferenceEquals(panel, expanded) && panel.IsExpanded))
            {
                panel.SetCurrentValue(MdExpansionPanel.IsExpandedProperty, false);
            }
        }
        finally
        {
            _coercing = false;
        }
    }
}
