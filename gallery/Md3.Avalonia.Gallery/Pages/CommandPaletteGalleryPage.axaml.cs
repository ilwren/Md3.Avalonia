using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class CommandPaletteGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public CommandPaletteGalleryPage()
    {
        InitializeComponent();
        // On the combined page these commands reached into three other sections. A command
        // should act on what its own page owns.
        CommandPalette.ItemsSource = new[]
        {
            new MdCommandItem(L("Toggle loading placeholder", "切换加载占位"), new RelayCommand(() => DemoSkeleton.IsLoading = !DemoSkeleton.IsLoading), Description: L("Switch the skeleton between loading and loaded", "在加载与已加载之间切换骨架屏"), Keywords: "loading shimmer skeleton"),
            new MdCommandItem(L("Show loaded content", "显示已加载内容"), new RelayCommand(() => DemoSkeleton.IsLoading = false), Description: L("Reveal the content behind the placeholder", "显示占位之后的内容"), Keywords: "content ready"),
            new MdCommandItem(L("Reset status line", "重置状态行"), new RelayCommand(() => PaletteStatus.Text = L("Open the palette and run a command, or filter by keyword.", "打开命令面板并执行命令，或按关键词筛选。")), Description: L("Clear the last reported command", "清除上一次报告的命令"), Keywords: "reset clear")
        };
    }

    private void OpenCommandPalette(object? sender, RoutedEventArgs e) => CommandPalette.Show();

    private void CommandPaletteInvoked(object? sender, MdCommandItem item) =>
        PaletteStatus.Text = L($"Command executed: {item.Title}.", $"已执行命令：{item.Title}。");
}
