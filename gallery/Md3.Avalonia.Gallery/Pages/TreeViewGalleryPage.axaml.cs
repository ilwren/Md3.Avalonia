using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class TreeViewGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public TreeViewGalleryPage()
    {
        InitializeComponent();
        var controls = new MdTreeNode("controls", L("Controls", "控件"),
        [
            new MdTreeNode("input", L("Input", "输入"), [new MdTreeNode("pin", L("PIN input", "PIN 输入")), new MdTreeNode("tags", L("Tag input", "标签输入"))]),
            new MdTreeNode("navigation", L("Navigation", "导航"), [new MdTreeNode("tree", L("Tree view", "树形视图"))])
        ], L("Material components", "Material 组件")) { IsExpanded = true };
        controls.Children[0].IsExpanded = true;
        var platforms = new MdTreeNode("platforms", L("Platforms", "平台"),
        [
            new MdTreeNode("desktop", L("Desktop", "桌面"), data: L("Windows, macOS and Linux", "Windows、macOS 与 Linux")),
            new MdTreeNode("android", "Android", data: L("Touch and soft-keyboard host", "触摸与软键盘宿主"))
        ], L("Target hosts", "目标宿主")) { IsExpanded = true };
        Tree.Roots = [controls, platforms];
        Tree.SelectedNode = controls.Children[0].Children[0];
    }

    private void TreeNodeInvoked(object? sender, MdTreeNode node) =>
        TreeStatus.Text = L($"Invoked {node.Label} ({node.Id}).", $"已调用 {node.Label}（{node.Id}）。");

    private void TreeExpansionChanged(object? sender, MdTreeNode node) =>
        TreeStatus.Text = L($"{node.Label} is {(node.IsExpanded ? "expanded" : "collapsed")}.", $"{node.Label} 已{(node.IsExpanded ? "展开" : "折叠")}。");

    private void ExpandTree(object? sender, RoutedEventArgs e)
    {
        Tree.ExpandAll();
        TreeStatus.Text = L($"Expanded {Tree.VisibleRows.Count} visible nodes.", $"已展开 {Tree.VisibleRows.Count} 个可见节点。");
    }

    private void CollapseTree(object? sender, RoutedEventArgs e)
    {
        Tree.CollapseAll();
        TreeStatus.Text = L("All hierarchy branches collapsed.", "已折叠全部层级分支。");
    }

    private void SelectAndroidNode(object? sender, RoutedEventArgs e) =>
        TreeStatus.Text = Tree.SelectById("android")
            ? L("Android revealed and selected through the direct API.", "已通过直接 API 显示并选择 Android。")
            : L("Android node was not found.", "未找到 Android 节点。");

    private void ToggleTreeRtl(object? sender, RoutedEventArgs e)
    {
        Tree.FlowDirection = Tree.FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft
            ? global::Avalonia.Media.FlowDirection.LeftToRight
            : global::Avalonia.Media.FlowDirection.RightToLeft;
        TreeStatus.Text = L($"Tree flow direction: {Tree.FlowDirection}.", $"树形视图流向：{Tree.FlowDirection}。");
    }
}
