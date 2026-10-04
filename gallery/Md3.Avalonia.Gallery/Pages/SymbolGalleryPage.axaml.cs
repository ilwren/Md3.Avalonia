using System.Globalization;
using Avalonia.Controls;
using Avalonia.Platform;
using Md3.Avalonia.Gallery.Components;
using Md3.Avalonia.Icons;

namespace Md3.Avalonia.Gallery.Pages;

public partial class SymbolGalleryPage : UserControl
{
    private const double SymbolTileWidth = 158;
    private const double SymbolRowHeight = 122;
    private readonly IReadOnlyList<SymbolEntry> _symbols;
    private IReadOnlyList<SymbolEntry> _visibleSymbols = [];
    private int _columnCount = 6;

    public SymbolGalleryPage()
    {
        InitializeComponent();
        if (!MdExternalMaterialSymbols.IsAvailable)
        {
            _symbols = [];
            UnavailablePanel.IsVisible = true;
            CatalogPanel.IsVisible = false;
            return;
        }

        _symbols = LoadCatalog();
        UnavailablePanel.IsVisible = false;
        CatalogPanel.IsVisible = true;
        FontStatus.Text = $"Loaded {MdExternalMaterialSymbols.LoadedFamilyName} · {_symbols.Count:N0} supported glyphs · {MdExternalMaterialSymbols.VerifiedControlGlyphCount} control tokens verified · {MdExternalMaterialSymbols.LoadedFontPath}";
        Show(_symbols);
    }

    private void FilterSymbols(object? sender, TextChangedEventArgs e)
    {
        var query = SymbolSearch.Text?.Trim();
        Show(string.IsNullOrEmpty(query)
            ? _symbols
            : _symbols.Where(symbol => symbol.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToArray());
    }

    private void Show(IEnumerable<SymbolEntry> symbols)
    {
        _visibleSymbols = symbols as IReadOnlyList<SymbolEntry> ?? symbols.ToArray();
        CatalogScroller.Offset = default;
        ResultCount.Text = $"{_visibleSymbols.Count:N0} of {_symbols.Count:N0} official symbols";
        RefreshCatalogLayout();
    }

    private void CatalogScrolled(object? sender, ScrollChangedEventArgs e) => RenderVisibleRows();

    private void CatalogSizeChanged(object? sender, SizeChangedEventArgs e) => RefreshCatalogLayout();

    private void RefreshCatalogLayout()
    {
        var width = CatalogScroller.Viewport.Width > 0
            ? CatalogScroller.Viewport.Width
            : CatalogScroller.Bounds.Width;
        var columns = width > 0 ? Math.Max(1, (int)(width / SymbolTileWidth)) : 6;
        _columnCount = columns;
        var rowCount = (int)Math.Ceiling(_visibleSymbols.Count / (double)_columnCount);
        SymbolCanvas.Height = rowCount * SymbolRowHeight;
        RenderVisibleRows();
    }

    private void RenderVisibleRows()
    {
        SymbolCanvas.Children.Clear();
        if (_visibleSymbols.Count == 0) return;

        var firstRow = Math.Max(0, (int)(CatalogScroller.Offset.Y / SymbolRowHeight) - 1);
        var viewportHeight = CatalogScroller.Viewport.Height > 0
            ? CatalogScroller.Viewport.Height
            : CatalogScroller.Bounds.Height;
        var visibleRowCount = Math.Max(1, (int)Math.Ceiling(viewportHeight / SymbolRowHeight) + 2);
        var totalRows = (int)Math.Ceiling(_visibleSymbols.Count / (double)_columnCount);
        var lastRowExclusive = Math.Min(totalRows, firstRow + visibleRowCount);

        for (var rowIndex = firstRow; rowIndex < lastRowExclusive; rowIndex++)
        {
            var row = new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal };
            var firstItem = rowIndex * _columnCount;
            var lastItem = Math.Min(_visibleSymbols.Count, firstItem + _columnCount);
            for (var index = firstItem; index < lastItem; index++)
            {
                var symbol = _visibleSymbols[index];
                row.Children.Add(new SymbolTile
                {
                    SymbolName = symbol.Name,
                    Glyph = symbol.Glyph,
                    CopyText = $"{{x:Static md:MdSymbols.{ToPropertyName(symbol.Name)}}}"
                });
            }

            Canvas.SetTop(row, rowIndex * SymbolRowHeight);
            SymbolCanvas.Children.Add(row);
        }
    }

    private static string ToPropertyName(string symbolName)
    {
        var builder = new System.Text.StringBuilder(symbolName.Length);
        var capitalize = true;
        foreach (var character in symbolName)
        {
            if (character is '_' or '-' or ' ')
            {
                capitalize = true;
                continue;
            }
            builder.Append(capitalize ? char.ToUpperInvariant(character) : character);
            capitalize = false;
        }
        return builder.ToString();
    }

    private static IReadOnlyList<SymbolEntry> LoadCatalog()
    {
        var uri = new Uri("avares://Md3.Avalonia.Icons/Assets/Fonts/MaterialSymbolsRounded.codepoints");
        using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);
        var symbols = new List<SymbolEntry>(4300);
        while (reader.ReadLine() is { } line)
        {
            var columns = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (columns.Length != 2 ||
                !int.TryParse(columns[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var codepoint)) continue;
            var glyph = char.ConvertFromUtf32(codepoint);
            // A retained older official TTF can legitimately predate newer catalog additions.
            // Never render a missing-glyph box: expose the complete catalog actually supported by
            // the configured font and let compatibility aliases handle changed common codepoints.
            if (MdExternalMaterialSymbols.HasGlyph(glyph)) symbols.Add(new SymbolEntry(columns[0], glyph));
        }
        return symbols;
    }
}

public sealed record SymbolEntry(string Name, string Glyph);
