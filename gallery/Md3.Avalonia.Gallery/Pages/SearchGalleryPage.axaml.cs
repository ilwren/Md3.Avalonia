using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class SearchGalleryPage : UserControl
{
    private readonly SearchResult[] _allResults;
    private TopLevel? _topLevel;

    public SearchGalleryPage()
    {
        InitializeComponent();
        _allResults =
        [
            new("MaterialTheme.axaml", GalleryLocalization.Choose("Themes · AXAML", "主题 · AXAML")),
            new("MdSearchBar.cs", GalleryLocalization.Choose("Controls · C#", "控件 · C#")),
            new("MdSearchView.cs", GalleryLocalization.Choose("Controls · C#", "控件 · C#")),
            new("SearchGalleryPage.axaml", GalleryLocalization.Choose("Gallery · AXAML", "图库 · AXAML")),
            new("SearchGalleryPage.axaml.cs", GalleryLocalization.Choose("Gallery · C#", "图库 · C#")),
            new("SearchTokens.axaml", GalleryLocalization.Choose("Theme tokens · AXAML", "主题令牌 · AXAML")),
            new("README.md", GalleryLocalization.Choose("Documentation · Markdown", "文档 · Markdown"))
        ];
        ApplyFilter(string.Empty);
    }

    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _topLevel = TopLevel.GetTopLevel(this);
        _topLevel?.AddHandler(InputElement.KeyDownEvent, OnGalleryShortcut, RoutingStrategies.Tunnel, true);
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _topLevel?.RemoveHandler(InputElement.KeyDownEvent, OnGalleryShortcut);
        _topLevel = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnGalleryShortcut(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.K || !e.KeyModifiers.HasFlag(KeyModifiers.Control)) return;
        OpenSearch(GalleryLocalization.Choose("Ctrl+K opened expanded search; results filter as you type.", "Ctrl+K 已打开展开式搜索；输入时会实时筛选结果。"));
        e.Handled = true;
    }

    private void OpenExpandedSearch(object? sender, RoutedEventArgs e) =>
        OpenSearch(GalleryLocalization.Choose("Expanded search opened; type to filter, choose a result, or press Escape to close.", "展开式搜索已打开；输入以筛选、选择结果，或按 Escape 关闭。"));

    private void OpenSearch(string status)
    {
        ExpandedSearchView.Show();
        ExpandedSearchBar.Focus();
        ExpandedSearchBar.SelectAll();
        SearchStatus.Text = status;
    }

    private void OnSearchSubmitted(object? sender, string query) =>
        SearchStatus.Text = GalleryLocalization.Choose($"Submitted: {query}", $"已提交：{query}");

    private void OnExpandedQueryChanged(object? sender, TextChangedEventArgs e) =>
        ApplyFilter((sender as TextBox)?.Text ?? string.Empty);

    private void ApplyFilter(string query)
    {
        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var filtered = _allResults.Where(result => terms.All(term =>
            result.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            result.Category.Contains(term, StringComparison.OrdinalIgnoreCase))).ToArray();
        ExpandedSearchResults.ItemsSource = filtered;
        NoSearchResults.IsVisible = filtered.Length == 0;
        SearchStatus.Text = string.IsNullOrWhiteSpace(query)
            ? GalleryLocalization.Choose($"Showing all {filtered.Length} files.", $"正在显示全部 {filtered.Length} 个文件。")
            : GalleryLocalization.Choose($"{filtered.Length} result{(filtered.Length == 1 ? string.Empty : "s")} for “{query}”.", $"“{query}”有 {filtered.Length} 个结果。");
    }

    private void OnExpandedResultSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (ExpandedSearchResults.SelectedItem is not SearchResult result) return;
        ExpandedSearchResults.SelectedItem = null;
        ExpandedSearchView.CommitResult(result, result.Name);
        SearchStatus.Text = GalleryLocalization.Choose($"Selected {result.Name} · {result.Category}.", $"已选择 {result.Name} · {result.Category}。");
    }

    private void OnExpandedSearchSubmitted(object? sender, string query)
    {
        if (ExpandedSearchResults.ItemsSource?.Cast<SearchResult>().FirstOrDefault() is { } first)
        {
            ExpandedSearchView.CommitResult(first, first.Name);
            SearchStatus.Text = GalleryLocalization.Choose($"Opened first result: {first.Name}.", $"已打开首个结果：{first.Name}。");
        }
        else
        {
            SearchStatus.Text = GalleryLocalization.Choose($"No result for “{query}”.", $"没有“{query}”的结果。");
        }
    }

}

/// <summary>Row model for the SearchGalleryPage.axaml sample; public so the template can compile its bindings.</summary>
public sealed record SearchResult(string Name, string Category);
