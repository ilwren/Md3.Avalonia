using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class GridTileGalleryPage : UserControl
{
    private bool _gridTileFavorite;
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public GridTileGalleryPage() => InitializeComponent();

    private void OpenGridTile(object? sender, RoutedEventArgs e)
    {
        AuroraTile.Activate();
        GridTileStatus.Text = L("Opened Aurora dashboard project · the primary outline marks the active tile.", "已打开 Aurora 仪表板项目 · 主色描边表示当前活动磁贴。");
    }

    private void GridTileBarInvoked(object? sender, EventArgs e)
    {
        AuroraTile.IsActivated = true;
        GridTileStatus.Text = L("Opened Aurora project details from GridTileBar.", "已从 GridTileBar 打开 Aurora 项目详情。");
    }

    private void ToggleGridTileFavorite(object? sender, RoutedEventArgs e)
    {
        _gridTileFavorite = !_gridTileFavorite;
        AuroraTile.IsFavorite = _gridTileFavorite;
        GridTileStatus.Text = _gridTileFavorite
            ? L("Aurora dashboard added to favorites · the tertiary badge is visible.", "已收藏 Aurora 仪表板 · 三级色徽标已显示。")
            : L("Aurora dashboard removed from favorites.", "已取消收藏 Aurora 仪表板。");
        e.Handled = true;
    }
}
