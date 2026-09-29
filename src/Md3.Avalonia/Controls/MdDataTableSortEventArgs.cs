namespace Md3.Avalonia.Controls;

public sealed class MdDataTableSortEventArgs(string column, MdDataTableSortDirection direction) : EventArgs
{
    public string Column { get; } = column;
    public MdDataTableSortDirection Direction { get; } = direction;
}
