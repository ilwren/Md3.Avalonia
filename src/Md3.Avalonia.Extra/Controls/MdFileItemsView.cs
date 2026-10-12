using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>
/// Material file collection supporting compact list, icon grid and metadata-rich details modes.
/// It is a ListBox, so standard Avalonia selection, virtualization, keyboard focus and binding APIs
/// remain available. Set <see cref="GlyphSelector"/> to use symbols from an optional icon package.
/// </summary>
[PseudoClasses(":list", ":grid", ":details")]
public sealed class MdFileItemsView : ListBox
{
    public static readonly StyledProperty<MdFileViewMode> ViewModeProperty =
        AvaloniaProperty.Register<MdFileItemsView, MdFileViewMode>(nameof(ViewMode), MdFileViewMode.List);
    public static readonly StyledProperty<Func<MdFileEntry, string?>?> GlyphSelectorProperty =
        AvaloniaProperty.Register<MdFileItemsView, Func<MdFileEntry, string?>?>(nameof(GlyphSelector));
    public static readonly StyledProperty<bool> IsCompactProperty =
        AvaloniaProperty.Register<MdFileItemsView, bool>(nameof(IsCompact));
    public static readonly DirectProperty<MdFileItemsView, bool> IsSelectionModeProperty =
        AvaloniaProperty.RegisterDirect<MdFileItemsView, bool>(nameof(IsSelectionMode), view => view.IsSelectionMode);

    private MdFileEntry[] _selectionBeforePointerPress = Array.Empty<MdFileEntry>();
    private bool _hasPointerSelectionSnapshot;
    private bool _suppressNextTap;
    private bool _isSelectionMode;

    static MdFileItemsView()
    {
        ViewModeProperty.Changed.AddClassHandler<MdFileItemsView>((view, _) => view.UpdateViewMode());
        GlyphSelectorProperty.Changed.AddClassHandler<MdFileItemsView>((view, _) => view.UpdateItemTemplate());
        IsCompactProperty.Changed.AddClassHandler<MdFileItemsView>((view, _) => view.UpdateItemTemplate());
    }

    public MdFileItemsView()
    {
        SelectionMode = global::Avalonia.Controls.SelectionMode.Multiple;
        ItemTemplate = CreateItemTemplate();
        UpdateViewMode();
        Tapped += OnTapped;
        DoubleTapped += OnDoubleTapped;
        ContextRequested += OnContextRequested;
    }

    /// <summary>Gets or sets the current list, grid or details presentation.</summary>
    public MdFileViewMode ViewMode { get => GetValue(ViewModeProperty); set => SetValue(ViewModeProperty, value); }

    /// <summary>Optionally maps a file entry to a glyph codepoint supported by the host's icon font.</summary>
    public Func<MdFileEntry, string?>? GlyphSelector { get => GetValue(GlyphSelectorProperty); set => SetValue(GlyphSelectorProperty, value); }

    /// <summary>Uses a compact row layout when the available width is narrow.</summary>
    public bool IsCompact { get => GetValue(IsCompactProperty); set => SetValue(IsCompactProperty, value); }

    /// <summary>Gets whether touch selection mode is active after a long press.</summary>
    public bool IsSelectionMode { get => _isSelectionMode; private set => SetAndRaise(IsSelectionModeProperty, ref _isSelectionMode, value); }

    public event EventHandler<MdFileEntry>? ItemActivated;
    public event EventHandler<MdFileEntry>? ItemContextRequested;
    public event EventHandler? RenameRequested;
    public event EventHandler? DeleteRequested;

    public IReadOnlyList<MdFileEntry> SelectedEntries =>
        SelectedItems?.OfType<MdFileEntry>().ToArray() ?? Array.Empty<MdFileEntry>();

    public void ClearSelection()
    {
        IsSelectionMode = false;
        if (SelectedItems is { } selection) selection.Clear();
        else SelectedItem = null;
    }

    /// <summary>Enters touch-style selection mode and optionally includes an initial entry.</summary>
    public void BeginSelectionMode(MdFileEntry? initialEntry = null)
    {
        IsSelectionMode = true;
        if (initialEntry is null || SelectedItems is not { } selection) return;
        var currentEntry = Items.OfType<MdFileEntry>().FirstOrDefault(entry => entry.Id == initialEntry.Id);
        if (currentEntry is not null && !selection.OfType<MdFileEntry>().Any(entry => entry.Id == currentEntry.Id))
            selection.Add(currentEntry);
    }

    /// <summary>Leaves touch-style selection mode and clears the current selection.</summary>
    public void EndSelectionMode() => ClearSelection();

    public void SelectAllEntries()
    {
        if (SelectedItems is not { } selection) return;
        selection.Clear();
        var entries = Items.OfType<MdFileEntry>().ToArray();
        foreach (var entry in entries) selection.Add(entry);
        IsSelectionMode = entries.Length > 0;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter when SelectedItem is MdFileEntry entry:
                ItemActivated?.Invoke(this, entry);
                e.Handled = true;
                return;
            case Key.F2 when SelectedEntries.Count == 1:
                RenameRequested?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                return;
            case Key.Delete when SelectedEntries.Count > 0:
                DeleteRequested?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                return;
            case Key.Escape when IsSelectionMode || SelectedEntries.Count > 0:
                EndSelectionMode();
                e.Handled = true;
                return;
            case Key.A when (e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Meta)) != 0:
                SelectAllEntries();
                e.Handled = true;
                return;
        }
        base.OnKeyDown(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Touch) _suppressNextTap = false;
        _selectionBeforePointerPress = SelectedEntries.ToArray();
        _hasPointerSelectionSnapshot = true;
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed && e.Source is Visual visual)
            RaiseContextRequest(visual, e);
        base.OnPointerPressed(e);
    }

    protected override void OnHolding(HoldingRoutedEventArgs e)
    {
        if (IsCompact && e.PointerType == PointerType.Touch && e.HoldingState == HoldingState.Started &&
            e.Source is Visual visual && FindEntry(visual) is { } entry)
        {
            if (_hasPointerSelectionSnapshot) RestoreSelection(_selectionBeforePointerPress);
            BeginSelectionMode(entry);
            _suppressNextTap = true;
            e.Handled = true;
        }
        base.OnHolding(e);
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (e.Pointer.Type != PointerType.Touch || e.Source is not Visual visual || FindEntry(visual) is not { } entry)
            return;

        if (_suppressNextTap)
        {
            _suppressNextTap = false;
            _hasPointerSelectionSnapshot = false;
            e.Handled = true;
            return;
        }

        if (IsSelectionMode)
        {
            if (_hasPointerSelectionSnapshot) RestoreSelection(_selectionBeforePointerPress);
            ToggleSelection(entry);
            _hasPointerSelectionSnapshot = false;
            e.Handled = true;
        }
        else if (IsCompact && entry.CanNavigate && !entry.IsSymbolicLink)
        {
            ItemActivated?.Invoke(this, entry);
            _hasPointerSelectionSnapshot = false;
            e.Handled = true;
        }
        else
        {
            _hasPointerSelectionSnapshot = false;
        }
    }

    private void OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (e.Source is Visual visual) RaiseContextRequest(visual, e);
    }

    private void RaiseContextRequest(Visual source, RoutedEventArgs e)
    {
        var entry = FindEntry(source);
        if (entry is null) return;
        if (SelectedItems?.OfType<MdFileEntry>().Any(selected => selected.Id == entry.Id) != true)
        {
            if (SelectedItems is { } selection) selection.Clear();
            SelectedItem = entry;
        }
        ItemContextRequested?.Invoke(this, entry);
        e.Handled = true;
    }

    private MdFileEntry? FindEntry(Visual source)
    {
        var container = source as ListBoxItem ?? source.GetVisualAncestors().OfType<ListBoxItem>().FirstOrDefault();
        return container is null ? null : ItemFromContainer(container) as MdFileEntry;
    }

    private void RestoreSelection(IEnumerable<MdFileEntry> entries)
    {
        if (SelectedItems is not { } selection) return;
        selection.Clear();
        var visible = Items.OfType<MdFileEntry>().ToDictionary(entry => entry.Id, StringComparer.Ordinal);
        foreach (var entry in entries)
            if (visible.TryGetValue(entry.Id, out var current)) selection.Add(current);
    }

    private void ToggleSelection(MdFileEntry entry)
    {
        if (SelectedItems is not { } selection) return;
        var currentEntry = Items.OfType<MdFileEntry>().FirstOrDefault(candidate => candidate.Id == entry.Id);
        if (currentEntry is null) return;
        var selectedEntry = selection.OfType<MdFileEntry>().FirstOrDefault(candidate => candidate.Id == currentEntry.Id);
        if (selectedEntry is not null)
        {
            selection.Remove(selectedEntry);
            if (selection.Count == 0) IsSelectionMode = false;
        }
        else
        {
            IsSelectionMode = true;
            selection.Add(currentEntry);
        }
    }

    private void OnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (SelectedItem is MdFileEntry entry) ItemActivated?.Invoke(this, entry);
    }

    private void UpdateViewMode()
    {
        PseudoClasses.Set(":list", ViewMode == MdFileViewMode.List);
        PseudoClasses.Set(":grid", ViewMode == MdFileViewMode.Grid);
        PseudoClasses.Set(":details", ViewMode == MdFileViewMode.Details);
        UpdateItemTemplate();
    }

    private void UpdateItemTemplate() => ItemTemplate = CreateItemTemplate();

    private IDataTemplate CreateItemTemplate() => new FuncDataTemplate<MdFileEntry>(
        (entry, _) => entry is null ? null : CreateItem(entry, ViewMode));

    private Control CreateItem(MdFileEntry entry, MdFileViewMode mode)
    {
        return mode == MdFileViewMode.Grid ? CreateGridTile(entry) : CreateRow(entry, mode);
    }

    private Control CreateGridTile(MdFileEntry entry)
    {
        var tile = new Border
        {
            MinWidth = 132,
            Width = 148,
            MinHeight = 148,
            Padding = new Thickness(12),
            Margin = new Thickness(4),
            CornerRadius = new CornerRadius(16),
            Child = new StackPanel
            {
                Spacing = 8,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Children =
                {
                    CreateFileIcon(entry, 88),
                    new MdText
                    {
                        Text = entry.Name,
                        StyleRole = MdTextStyle.TitleSmall,
                        TextWrapping = TextWrapping.Wrap,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        MaxLines = 2,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    },
                    new MdText
                    {
                        Text = entry.IsDirectory ? "Folder" : entry.DisplaySize,
                        StyleRole = MdTextStyle.BodySmall,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    }
                }
            }
        };
        AutomationProperties.SetName(tile, entry.Name);
        return tile;
    }

    private Control CreateRow(MdFileEntry entry, MdFileViewMode mode)
    {
        var details = mode == MdFileViewMode.Details;
        var narrow = IsCompact;
        var row = new Grid
        {
            ColumnDefinitions = details
                ? narrow ? new ColumnDefinitions("40,*,88") : new ColumnDefinitions("40,*,120,84,136")
                : new ColumnDefinitions("48,*"),
            ColumnSpacing = details ? 12 : 8,
            MinHeight = details ? 48 : 64,
            Margin = new Thickness(8, details ? 2 : 4),
            VerticalAlignment = VerticalAlignment.Center
        };
        var nameStack = new StackPanel
        {
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new MdText
                {
                    Text = entry.Name,
                    StyleRole = details ? MdTextStyle.BodyLarge : MdTextStyle.TitleSmall,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    VerticalAlignment = VerticalAlignment.Center
                },
                new MdText
                {
                    Text = details ? null : entry.IsDirectory ? "Folder" : entry.ContentType ?? entry.Extension,
                    StyleRole = MdTextStyle.BodySmall,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    IsVisible = !details && !string.IsNullOrEmpty(entry.IsDirectory ? "Folder" : entry.ContentType ?? entry.Extension)
                }
            }
        };

        var icon = CreateFileIcon(entry, details ? 32 : 40);
        Grid.SetColumn(icon, 0);
        Grid.SetColumn(nameStack, 1);
        row.Children.Add(icon);
        row.Children.Add(nameStack);

        if (details && narrow)
        {
            AddDetailColumn(row, entry.DisplaySize, 2, MdTextStyle.BodyMedium);
        }
        else if (details)
        {
            AddDetailColumn(row, entry.IsDirectory ? "Folder" : entry.ContentType ?? entry.Extension, 2, MdTextStyle.BodyMedium);
            AddDetailColumn(row, entry.DisplaySize, 3, MdTextStyle.BodyMedium);
            AddDetailColumn(row, entry.LastModified?.ToLocalTime().ToString("g") ?? string.Empty, 4, MdTextStyle.BodySmall);
        }

        AutomationProperties.SetName(row, entry.Name);
        return row;
    }

    private Control CreateFileIcon(MdFileEntry entry, double size)
    {
        var glyph = GlyphSelector?.Invoke(entry) ?? entry.Glyph;
        Control content = entry.Thumbnail is { } thumbnail
            ? new Border
            {
                Width = size,
                Height = size,
                CornerRadius = new CornerRadius(12),
                ClipToBounds = true,
                Child = new Image { Source = thumbnail, Stretch = Stretch.UniformToFill, Width = size, Height = size }
            }
            : glyph is not null
                ? new MdIcon { Glyph = glyph, Width = size * 0.54, Height = size * 0.54, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }
                : new MdText
                {
                    Text = entry.IsDirectory ? "DIR" : string.IsNullOrEmpty(entry.Extension) ? "FILE" : entry.Extension.TrimStart('.').ToUpperInvariant(),
                    StyleRole = MdTextStyle.LabelSmall,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                };
        return new MdSurface
        {
            Variant = MdSurfaceVariant.ContainerHigh,
            Width = size,
            Height = size,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Content = content
        };
    }

    private static void AddDetailColumn(Grid grid, string text, int column, MdTextStyle style)
    {
        var label = new MdText
        {
            Text = text,
            StyleRole = style,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetColumn(label, column);
        grid.Children.Add(label);
    }
}
