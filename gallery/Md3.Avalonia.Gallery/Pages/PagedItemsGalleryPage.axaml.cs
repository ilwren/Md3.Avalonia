using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class PagedItemsGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public PagedItemsGalleryPage()
    {
        InitializeComponent();
        PagedItems.PageProvider = async request =>
        {
            // A provider that returns synchronously never lets the loading state be seen: the
            // view went Loading -> Data inside one dispatcher pass, so the progress indicator
            // was shown and hidden before it could ever be painted. Real paging is I/O.
            await Task.Delay(450, request.CancellationToken);
            var page = Enumerable.Range(request.PageKey * request.PageSize + 1, request.PageSize)
                .Select(number => (object?)new PagedRecord(
                    L($"Record {number}", $"记录 {number}"),
                    L($"Page {request.PageKey + 1} · loaded {DateTime.Now:HH:mm:ss}", $"第 {request.PageKey + 1} 页 · 加载于 {DateTime.Now:HH:mm:ss}")))
                .ToArray();
            return new MdPageResult<object?>(page, request.PageKey >= 2 ? null : request.PageKey + 1, request.PageKey >= 2);
        };
    }

    private async void LoadNextPage(object? sender, RoutedEventArgs e)
    {
        await PagedItems.LoadNextPageAsync();
        PagingStatus.Text = L($"{PagedItems.Items.Count} records loaded · state {PagedItems.State}.", $"已加载 {PagedItems.Items.Count} 条记录 · 状态 {PagedItems.State}。");
    }

    private async void RefreshPages(object? sender, RoutedEventArgs e)
    {
        await PagedItems.RefreshAsync();
        PagingStatus.Text = L($"Refreshed from page 1 · {PagedItems.Items.Count} records loaded.", $"已从第 1 页刷新 · 共加载 {PagedItems.Items.Count} 条记录。");
    }

    private void PagesRefreshed(object? sender, EventArgs e) =>
        PagingStatus.Text = L("Existing pages cleared; loading page 1…", "已清除现有页面；正在加载第 1 页…");

    private sealed record PagedRecord(string Title, string Detail);
}
