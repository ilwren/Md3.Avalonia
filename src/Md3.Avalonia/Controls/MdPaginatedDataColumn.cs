using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Md3.Avalonia.Controls;

/// <summary>Describes a column header for <see cref="MdPaginatedDataTable"/>. Row cells remain supplied by the table's item template.</summary>
public sealed class MdPaginatedDataColumn : INotifyPropertyChanged
{
    private string? _label;
    private string? _tooltip;
    private bool _isNumeric;
    private bool _isSortable;

    /// <summary>The header text.</summary>
    public string? Label { get => _label; set => SetField(ref _label, value); }
    /// <summary>Optional help text displayed over the header.</summary>
    public string? Tooltip { get => _tooltip; set => SetField(ref _tooltip, value); }
    /// <summary>Whether the header is aligned to the trailing edge. The item template is responsible for aligning cells.</summary>
    public bool IsNumeric { get => _isNumeric; set => SetField(ref _isNumeric, value); }
    /// <summary>Whether the header can request a sort from the host.</summary>
    public bool IsSortable { get => _isSortable; set => SetField(ref _isSortable, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

/// <summary>A host-handled sorting request for a paginated table column.</summary>
public sealed class MdPaginatedDataTableSortEventArgs(int columnIndex, MdPaginatedDataColumn column, bool ascending) : EventArgs
{
    public int ColumnIndex { get; } = columnIndex;
    public MdPaginatedDataColumn Column { get; } = column;
    public bool Ascending { get; } = ascending;
}
