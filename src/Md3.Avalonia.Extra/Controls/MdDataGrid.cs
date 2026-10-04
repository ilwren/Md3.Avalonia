using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Extra.Controls;

public sealed class MdDataGridColumn : AvaloniaObject
{
    public static readonly StyledProperty<object?> HeaderProperty = AvaloniaProperty.Register<MdDataGridColumn, object?>(nameof(Header));
    public static readonly StyledProperty<string> PropertyNameProperty = AvaloniaProperty.Register<MdDataGridColumn, string>(nameof(PropertyName), string.Empty);
    public static readonly StyledProperty<GridLength> WidthProperty = AvaloniaProperty.Register<MdDataGridColumn, GridLength>(nameof(Width), new GridLength(1, GridUnitType.Star));
    public static readonly StyledProperty<double> MinimumWidthProperty = AvaloniaProperty.Register<MdDataGridColumn, double>(nameof(MinimumWidth), 72);
    public static readonly StyledProperty<bool> IsEditableProperty = AvaloniaProperty.Register<MdDataGridColumn, bool>(nameof(IsEditable));
    public object? Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
    public string PropertyName { get => GetValue(PropertyNameProperty); set => SetValue(PropertyNameProperty, value); }
    public GridLength Width { get => GetValue(WidthProperty); set => SetValue(WidthProperty, value); }
    public double MinimumWidth { get => GetValue(MinimumWidthProperty); set => SetValue(MinimumWidthProperty, value); }
    public bool IsEditable { get => GetValue(IsEditableProperty); set => SetValue(IsEditableProperty, value); }

    /// <summary>
    /// Reads the cell value without reflection. When set, <see cref="PropertyName"/> is used only
    /// as the column key for sorting, and the model's properties no longer have to survive
    /// trimming. Prefer this in a trimmed or size-sensitive application.
    /// </summary>
    public Func<object?, object?>? ValueSelector { get; set; }

    /// <summary>
    /// Writes an edited value back without reflection. Pair it with <see cref="ValueParser"/>
    /// when the column is not a string column.
    /// </summary>
    public Action<object, object?>? ValueSetter { get; set; }

    /// <summary>
    /// Converts edited text to the model's value. When null the value is converted with the
    /// target type's <see cref="System.ComponentModel.TypeConverter"/>, which needs reflection.
    /// Return null to reject the edit.
    /// </summary>
    public Func<string?, object?>? ValueParser { get; set; }

    internal object? Read(object? item) =>
        ValueSelector is { } selector ? selector(item) : MdMemberAccess.GetValue(item, PropertyName);
}

public sealed class MdDataGridCellEditEventArgs(object item, MdDataGridColumn column, object? oldValue, object? newValue) : EventArgs
{
    public object Item { get; } = item;
    public MdDataGridColumn Column { get; } = column;
    public object? OldValue { get; } = oldValue;
    public object? NewValue { get; } = newValue;
    public bool Cancel { get; set; }
}

/// <summary>A virtualized Material enterprise grid with sticky header, sorting/filtering, column resize/reorder, editing, and clipboard text APIs.</summary>
[PseudoClasses(":editing", ":filtered")]
public sealed class MdDataGrid : ListBox
{
    public static readonly StyledProperty<IEnumerable?> DataSourceProperty = AvaloniaProperty.Register<MdDataGrid, IEnumerable?>(nameof(DataSource));
    public static readonly StyledProperty<bool> IsHeaderStickyProperty = AvaloniaProperty.Register<MdDataGrid, bool>(nameof(IsHeaderSticky), true);
    public static readonly StyledProperty<string?> SortColumnProperty = AvaloniaProperty.Register<MdDataGrid, string?>(nameof(SortColumn));
    public static readonly StyledProperty<ListSortDirection?> SortDirectionProperty = AvaloniaProperty.Register<MdDataGrid, ListSortDirection?>(nameof(SortDirection));
    public static readonly StyledProperty<string?> FilterTextProperty = AvaloniaProperty.Register<MdDataGrid, string?>(nameof(FilterText));
    public ObservableCollection<MdDataGridColumn> Columns { get; } = [];
    private Func<object?, bool>? _filter;

    static MdDataGrid()
    {
        DataSourceProperty.Changed.AddClassHandler<MdDataGrid>((grid, _) => grid.RefreshView());
        FilterTextProperty.Changed.AddClassHandler<MdDataGrid>((grid, _) => grid.RefreshView());
    }
    public MdDataGrid()
    {
        SelectionMode = SelectionMode.Multiple;
        Columns.CollectionChanged += (_, _) => { RefreshView(); InvalidateMeasure(); };
        AddHandler(Button.ClickEvent, OnHeaderClick, RoutingStrategies.Bubble, true);
    }

    public IEnumerable? DataSource { get => GetValue(DataSourceProperty); set => SetValue(DataSourceProperty, value); }
    public bool IsHeaderSticky { get => GetValue(IsHeaderStickyProperty); set => SetValue(IsHeaderStickyProperty, value); }
    public string? SortColumn { get => GetValue(SortColumnProperty); set => SetValue(SortColumnProperty, value); }
    public ListSortDirection? SortDirection { get => GetValue(SortDirectionProperty); set => SetValue(SortDirectionProperty, value); }
    public string? FilterText { get => GetValue(FilterTextProperty); set => SetValue(FilterTextProperty, value); }

    public event EventHandler? ViewChanged;
    public event EventHandler<MdDataGridCellEditEventArgs>? CellEditCommitting;

    public void SetFilter(Func<object?, bool>? filter) { _filter = filter; RefreshView(); }
    public void ClearFilter() { _filter = null; FilterText = null; RefreshView(); }
    public void SortBy(string propertyName, ListSortDirection? direction = null)
    {
        var sameColumn = string.Equals(SortColumn, propertyName, StringComparison.Ordinal);
        var nextDirection = direction ?? (sameColumn && SortDirection == ListSortDirection.Ascending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending);
        SortColumn = propertyName;
        SortDirection = nextDirection;
        RefreshView();
    }
    public bool ResizeColumn(int index, GridLength width)
    {
        if (index < 0 || index >= Columns.Count) return false;
        Columns[index].Width = width; InvalidateMeasure(); return true;
    }
    public bool MoveColumn(int oldIndex, int newIndex)
    {
        if (oldIndex < 0 || oldIndex >= Columns.Count || newIndex < 0 || newIndex >= Columns.Count || oldIndex == newIndex) return false;
        Columns.Move(oldIndex, newIndex); return true;
    }
    public string BuildClipboardText(bool selectedOnly = true)
    {
        var selection = SelectedItems;
        var rows = selectedOnly && selection is { Count: > 0 } ? selection.Cast<object?>() : (ItemsSource ?? Array.Empty<object>()).Cast<object?>();
        var header = string.Join('\t', Columns.Select(column => column.Header?.ToString() ?? column.PropertyName));
        var values = rows.Select(item => string.Join('\t', Columns.Select(column => column.Read(item)?.ToString() ?? string.Empty)));
        return string.Join(Environment.NewLine, new[] { header }.Concat(values));
    }
    internal bool CommitEdit(object item, MdDataGridColumn column, string? text)
    {
        var oldValue = column.Read(item);
        object? value;

        if (column.ValueSetter is { } setter)
        {
            // Fully reflection-free: the column knows how to parse and how to store.
            if (column.ValueParser is { } parser)
            {
                value = parser(text);
                if (value is null && !string.IsNullOrEmpty(text)) return false;
            }
            else
            {
                value = text;
            }

            var directArgs = new MdDataGridCellEditEventArgs(item, column, oldValue, value);
            CellEditCommitting?.Invoke(this, directArgs);
            if (directArgs.Cancel) return false;
            setter(item, value);
            return true;
        }

        var accessor = MdMemberAccess.For(item.GetType(), column.PropertyName);
        if (accessor?.CanWrite != true) return false;
        if (column.ValueParser is { } reflectionParser)
        {
            value = reflectionParser(text);
            if (value is null && !string.IsNullOrEmpty(text)) return false;
        }
        else if (!MdMemberAccess.TryConvert(text, accessor.MemberType, out value))
        {
            return false;
        }

        var args = new MdDataGridCellEditEventArgs(item, column, oldValue, value);
        CellEditCommitting?.Invoke(this, args);
        if (args.Cancel) return false;
        accessor.Set(item, value);
        return true;
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is ListBoxItem row && item is not null)
            row.Content = new MdDataGridRowPresenter { Item = item, Owner = this, HorizontalAlignment = HorizontalAlignment.Stretch };
    }

    private void OnHeaderClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is Button { Classes: var classes, DataContext: MdDataGridColumn column } &&
            classes.Contains("sort-header") && !string.IsNullOrWhiteSpace(column.PropertyName))
        {
            SortBy(column.PropertyName);
            e.Handled = true;
        }
    }

    private void RefreshView()
    {
        IEnumerable<object?> query = (DataSource ?? Array.Empty<object>()).Cast<object?>();
        if (_filter is not null) query = query.Where(_filter);
        if (!string.IsNullOrWhiteSpace(FilterText))
        {
            var text = FilterText.Trim();
            query = query.Where(item => Columns.Any(column => column.Read(item)?.ToString()?.Contains(text, StringComparison.OrdinalIgnoreCase) == true));
        }
        if (!string.IsNullOrWhiteSpace(SortColumn) && SortDirection is { } direction)
        {
            query = direction == ListSortDirection.Ascending
                ? query.OrderBy(item => ReadSortKey(item), ObjectValueComparer.Instance)
                : query.OrderByDescending(item => ReadSortKey(item), ObjectValueComparer.Instance);
        }
        var selection = (SelectedItems ?? Array.Empty<object>()).Cast<object?>().ToHashSet();
        var view = query.ToArray();
        ItemsSource = view;
        if (SelectedItems is { } selectedItems)
            foreach (var item in view.Where(selection.Contains)) if (!selectedItems.Contains(item)) selectedItems.Add(item);
        PseudoClasses.Set(":filtered", _filter is not null || !string.IsNullOrWhiteSpace(FilterText));
        ViewChanged?.Invoke(this, EventArgs.Empty);
    }

    // Sorting is keyed by column name, so honour that column's selector when it has one rather
    // than falling back to reflection for the one read that happens most often.
    private object? ReadSortKey(object? item)
    {
        var column = Columns.FirstOrDefault(candidate => candidate.PropertyName == SortColumn);
        return column is not null ? column.Read(item) : MdMemberAccess.GetValue(item, SortColumn);
    }

    internal static object? GetValue(object? item, string propertyName) => MdMemberAccess.GetValue(item, propertyName);
    private sealed class ObjectValueComparer : IComparer<object?>
    {
        public static ObjectValueComparer Instance { get; } = new();
        public int Compare(object? x, object? y)
        {
            if (ReferenceEquals(x, y)) return 0; if (x is null) return -1; if (y is null) return 1;
            return x is IComparable comparable ? comparable.CompareTo(y) : string.Compare(x.ToString(), y.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}

public sealed class MdDataGridHeaderPresenter : Grid
{
    public static readonly StyledProperty<MdDataGrid?> OwnerProperty =
        AvaloniaProperty.Register<MdDataGridHeaderPresenter, MdDataGrid?>(nameof(Owner));
    private MdDataGrid? _subscribedOwner;

    static MdDataGridHeaderPresenter() =>
        OwnerProperty.Changed.AddClassHandler<MdDataGridHeaderPresenter>((presenter, _) => presenter.OnOwnerChanged());

    public MdDataGrid? Owner { get => GetValue(OwnerProperty); set => SetValue(OwnerProperty, value); }

    private void OnOwnerChanged()
    {
        if (_subscribedOwner is not null) _subscribedOwner.Columns.CollectionChanged -= OnColumnsChanged;
        _subscribedOwner = Owner;
        if (_subscribedOwner is not null) _subscribedOwner.Columns.CollectionChanged += OnColumnsChanged;
        Rebuild();
    }

    private void OnColumnsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        Children.Clear();
        ColumnDefinitions.Clear();
        if (Owner is null) return;
        for (var index = 0; index < Owner.Columns.Count; index++)
        {
            var column = Owner.Columns[index];
            ColumnDefinitions.Add(new ColumnDefinition(column.Width.Value, column.Width.GridUnitType) { MinWidth = column.MinimumWidth });
            var button = new MdButton
            {
                Content = column.Header,
                DataContext = column,
                Variant = MdButtonVariant.Text,
                Size = MdButtonSize.ExtraSmall,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                MinHeight = 48,
                CornerRadius = new CornerRadius(0)
            };
            button.Classes.Add("sort-header");
            SetColumn(button, index);
            Children.Add(button);
        }
    }
}

internal sealed class MdDataGridRowPresenter : Grid
{
    private object? _item;
    private MdDataGrid? _owner;
    public object? Item { get => _item; set { _item = value; Rebuild(); } }
    public MdDataGrid? Owner { get => _owner; set { _owner = value; Rebuild(); } }
    private void Rebuild()
    {
        Children.Clear(); ColumnDefinitions.Clear();
        if (Item is null || Owner is null) return;
        for (var index = 0; index < Owner.Columns.Count; index++)
        {
            var column = Owner.Columns[index];
            ColumnDefinitions.Add(new ColumnDefinition(column.Width.Value, column.Width.GridUnitType) { MinWidth = column.MinimumWidth });
            Control cell;
            var value = column.Read(Item);
            if (column.IsEditable)
            {
                var editor = new TextBox { Text = value?.ToString(), Margin = new Thickness(8, 4), Background = null, BorderThickness = new Thickness(0) };
                editor.LostFocus += (_, _) => Owner.CommitEdit(Item, column, editor.Text);
                cell = editor;
            }
            else cell = new TextBlock { Text = value?.ToString(), Margin = new Thickness(12, 0), VerticalAlignment = VerticalAlignment.Center };
            SetColumn(cell, index); Children.Add(cell);
        }
    }
}
