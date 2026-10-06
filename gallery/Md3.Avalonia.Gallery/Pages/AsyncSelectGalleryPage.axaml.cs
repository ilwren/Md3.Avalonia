using Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class AsyncSelectGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public AsyncSelectGalleryPage()
    {
        InitializeComponent();
        AsyncSelect.ItemsSource = new[]
        {
            L("Alabama", "阿拉巴马州"), L("Alaska", "阿拉斯加州"), L("Arizona", "亚利桑那州"), L("California", "加利福尼亚州"),
            L("Colorado", "科罗拉多州"), L("New Jersey", "新泽西州"), L("New York", "纽约州"), L("Washington", "华盛顿州")
        };
    }

    private void AsyncSelectionCommitted(object? sender, object? item) =>
        AsyncSelectStatus.Text = L($"Selected state: {item}.", $"已选择州：{item}。");
}
