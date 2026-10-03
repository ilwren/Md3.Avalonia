using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class BorderlessWindowGalleryPage : UserControl
{
    public BorderlessWindowGalleryPage()
    {
        InitializeComponent();
        var adapter = MdWindowPlatformAdapterResolver.Resolve();
        AdapterStatus.Text = GalleryLocalization.Choose(
            $"Resolved {adapter.Platform}; capabilities: {adapter.Capabilities}.",
            $"已解析平台 {adapter.Platform}；能力：{adapter.Capabilities}。");
    }

    private void OpenDemoWindow(object? sender, RoutedEventArgs e)
    {
        var window = new MdBorderlessWindow
        {
            Title = GalleryLocalization.Choose("Material borderless window", "Material 无边框窗口"),
            Width = 760,
            Height = 480,
            MinWidth = 420,
            MinHeight = 300,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            PreserveNativeBorder = true,
            ExtendIntoTitleBar = true,
            TitleBarHeight = 40
        };
        var body = new StackPanel
        {
            Margin = new Thickness(32),
            Spacing = 16,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        body.Children.Add(new TextBlock
        {
            Text = GalleryLocalization.Choose("Drag the title bar or double-click it.", "拖动标题栏或双击标题栏。"),
            FontSize = 24,
            TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
            TextAlignment = global::Avalonia.Media.TextAlignment.Center
        });
        body.Children.Add(new TextBlock
        {
            Text = GalleryLocalization.Choose(
                "The MdBorderlessWindow template now owns its title bar, all four clipped corners, caption buttons and eight resize roles.",
                "MdBorderlessWindow 模板现在自行提供标题栏、四角裁剪、标题按钮与八方向缩放区域。"),
            TextWrapping = global::Avalonia.Media.TextWrapping.Wrap,
            MaxWidth = 520,
            TextAlignment = global::Avalonia.Media.TextAlignment.Center
        });
        window.Content = body;
        if (TopLevel.GetTopLevel(this) is Window owner) window.Show(owner); else window.Show();
    }
}
