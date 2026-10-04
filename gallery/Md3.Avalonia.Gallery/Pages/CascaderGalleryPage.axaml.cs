using Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class CascaderGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public CascaderGalleryPage()
    {
        InitializeComponent();
        Cascader.ItemsSource = new[]
        {
            new MdCascaderItem("design", L("Design", "设计"), [new MdCascaderItem("material", "Material", [new MdCascaderItem("controls", L("Controls", "控件")), new MdCascaderItem("motion", L("Motion", "动效"))])]),
            new MdCascaderItem("engineering", L("Engineering", "工程"), [new MdCascaderItem("desktop", L("Desktop", "桌面")), new MdCascaderItem("android", "Android")])
        };
    }

    private void CascaderSelectionChanged(object? sender, IReadOnlyList<MdCascaderItem> path) =>
        CascaderStatus.Text = path.Count == 0
            ? L("Hierarchy cleared.", "层级选择已清除。")
            : L($"Destination: {string.Join(" / ", path.Select(item => item.Label))}.", $"目标：{string.Join(" / ", path.Select(item => item.Label))}。");
}
