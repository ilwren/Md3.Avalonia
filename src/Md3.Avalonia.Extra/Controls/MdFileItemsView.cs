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
        DoubleTapped += OnDoubleTapped;
        ContextRequested += OnContextRequested;
    }

    /// <summary>Gets or sets the current list, grid or details presentation.</summary>
    public MdFileViewMode ViewMode { get => GetValue(ViewModeProperty); set => SetValue(ViewModeProperty, value); }

    /// <summary>Optionally maps a file entry to a glyph codepoint supported by the host's icon font.</summary>
    public Func<MdFileEntry, string?>? GlyphSelector { get => GetValue(GlyphSelectorProperty); set => SetValue(GlyphSelectorProperty, value); }

    /// <summary>Uses a compact row layout when the available width is narrow.</summary>
    public bool IsCompact { get => GetValue(IsCompactProperty); set => SetValue(IsCompactProperty, value); }

    public event EventHandler<MdFileEntry>? ItemActivated;
    public event EventHandler<MdFileEntry>? ItemContextRequested;
    public event EventHandler? RenameRequested;
    public event EventHandler? DeleteRequested;

    public IReadOnlyList<MdFileEntry> SelectedEntries =>
        SelectedItems?.OfType<MdFileEntry>().ToArray() ?? Array.Empty<MdFileEntry>();

    public void ClearSelection()
    {
        if (SelectedItems is { } selection) selection.Clear();
        else SelectedItem = null;
    }

    public void SelectAllEntries()
    {
        if (SelectedItems is not { } selection) return;
        selection.Clear();
        foreach (var entry in Items.OfType<MdFileEntry>()) selection.Add(entry);
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
            case Key.A when (e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Meta)) != 0:
                SelectAllEntries();
                e.Handled = true;
                return;
        }
        base.OnKeyDown(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed && e.Source is Visual visual)
            RaiseContextRequest(visual, e);
        base.OnPointerPressed(e);
    }

    private void OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (e.Source is Visual visual) RaiseContextRequest(visual, e);
    }

    private void RaiseContextRequest(Visual source, RoutedEventArgs e)
    {
        var container = source as ListBoxItem ?? source.GetVisualAncestors().OfType<ListBoxItem>().FirstOrDefault();
        if (container is null || ItemFromContainer(container) is not MdFileEntry entry) return;
        if (SelectedItems?.Contains(entry) != true) SelectedItem = entry;
        ItemContextRequested?.Invoke(this, entry);
        e.Handled = true;
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
