using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A Material data-table surface. Native ListBox ItemsSource, ItemTemplate, multiple selection and
/// keyboard behavior are preserved; Header hosts the column labels and sorting actions.
/// </summary>
[PseudoClasses(":dividers", ":no-dividers", ":striped")]
public sealed class MdDataTable : ListBox
{
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<MdDataTable, object?>(nameof(Header));
    public static readonly StyledProperty<bool> ShowDividersProperty =
        AvaloniaProperty.Register<MdDataTable, bool>(nameof(ShowDividers), true);
    public static readonly StyledProperty<bool> IsStripedProperty =
        AvaloniaProperty.Register<MdDataTable, bool>(nameof(IsStriped));
    public static readonly StyledProperty<string?> SortColumnProperty =
        AvaloniaProperty.Register<MdDataTable, string?>(nameof(SortColumn));
    public static readonly StyledProperty<MdDataTableSortDirection> SortDirectionProperty =
        AvaloniaProperty.Register<MdDataTable, MdDataTableSortDirection>(nameof(SortDirection));

    static MdDataTable()
    {
        ShowDividersProperty.Changed.AddClassHandler<MdDataTable>((table, _) =>
        {
            table.UpdatePseudoClasses();
            table.UpdateRealizedRows();
        });
        IsStripedProperty.Changed.AddClassHandler<MdDataTable>((table, _) => table.UpdatePseudoClasses());
    }

    public MdDataTable()
    {
        SelectionMode = SelectionMode.Multiple;
        AutomationProperties.SetName(this, "Data table");
        AutomationProperties.SetLiveSetting(this, AutomationLiveSetting.Polite);
        UpdatePseudoClasses();
    }

    public object? Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
    public bool ShowDividers { get => GetValue(ShowDividersProperty); set => SetValue(ShowDividersProperty, value); }
    public bool IsStriped { get => GetValue(IsStripedProperty); set => SetValue(IsStripedProperty, value); }
    public string? SortColumn { get => GetValue(SortColumnProperty); set => SetValue(SortColumnProperty, value); }
    public MdDataTableSortDirection SortDirection { get => GetValue(SortDirectionProperty); set => SetValue(SortDirectionProperty, value); }

    public event EventHandler<MdDataTableSortEventArgs>? SortRequested;

    public void ToggleSort(string column)
    {
        var direction = string.Equals(SortColumn, column, StringComparison.Ordinal)
            ? SortDirection == MdDataTableSortDirection.Ascending
                ? MdDataTableSortDirection.Descending
                : MdDataTableSortDirection.Ascending
            : MdDataTableSortDirection.Ascending;
        SetCurrentValue(SortColumnProperty, column);
        SetCurrentValue(SortDirectionProperty, direction);
        AutomationProperties.SetHelpText(this, $"Sorted by {column}, {direction.ToString().ToLowerInvariant()}");
        SortRequested?.Invoke(this, new MdDataTableSortEventArgs(column, direction));
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is ListBoxItem row)
        {
            if (container is not MdDataTableRow &&
                ResourceNodeExtensions.FindResource(this, "MdDataTableGeneratedRowTheme") is global::Avalonia.Styling.ControlTheme theme)
                row.SetCurrentValue(ThemeProperty, theme);
            row.SetCurrentValue(ListBoxItem.BorderThicknessProperty,
                ShowDividers ? new Thickness(0, 0, 0, 1) : new Thickness(0));
            if (string.IsNullOrWhiteSpace(AutomationProperties.GetName(row)) && item is not null)
                AutomationProperties.SetName(row, item.ToString());
            AutomationProperties.SetHelpText(row, $"Row {index + 1} of {ItemCount}");
        }
        container.Classes.Set("odd", index % 2 != 0);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && ItemCount > 0 && e.Key is Key.Home or Key.End)
        {
            SelectedIndex = e.Key == Key.Home ? 0 : ItemCount - 1;
            ScrollIntoView(SelectedIndex);
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new MdDataTableAutomationPeer(this);

    private void UpdateRealizedRows()
    {
        foreach (var row in GetRealizedContainers().OfType<ListBoxItem>())
            row.SetCurrentValue(ListBoxItem.BorderThicknessProperty,
                ShowDividers ? new Thickness(0, 0, 0, 1) : new Thickness(0));
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":dividers", ShowDividers);
        PseudoClasses.Set(":no-dividers", !ShowDividers);
        PseudoClasses.Set(":striped", IsStriped);
    }
}

internal sealed class MdDataTableAutomationPeer(MdDataTable owner) : ListBoxAutomationPeer(owner)
{
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.DataGrid;
    protected override bool IsControlElementCore() => true;
    protected override bool IsContentElementCore() => true;
}
