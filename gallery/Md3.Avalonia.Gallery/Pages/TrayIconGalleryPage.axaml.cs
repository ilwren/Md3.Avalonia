using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

/// <summary>
/// Desktop-only destination. Android and the browser resolve the no-op tray adapter, so this page
/// stays out of the single-view navigation for the same reason the borderless-window sample does.
/// </summary>
public partial class TrayIconGalleryPage : UserControl
{
    private MdTrayIcon? _trayIcon;
    private RenderTargetBitmap? _iconBitmap;
    private int _activations;

    public TrayIconGalleryPage()
    {
        InitializeComponent();
        var platform = MdTrayIconPlatformAdapterResolver.CurrentPlatform;
        var support = MdTrayIconPlatformAdapterResolver.IsSupported
            ? GalleryLocalization.Choose(
                "This platform family owns a notification area, so an icon can appear. The adapter still reports unsupported if no windowing platform supplies one (headless hosts, or the X11 XEmbed fallback).",
                "该平台提供通知区域，图标可以出现。若窗口平台未提供实现（无头宿主或 X11 XEmbed 回退），适配器仍会报告为不支持。")
            : GalleryLocalization.Choose(
                "This platform has no notification area, so the resolved adapter is a no-op and every call is safe.",
                "该平台没有通知区域，因此解析出的适配器为空实现，所有调用都是安全的。");
        PlatformStatus.Text = GalleryLocalization.Choose(
            $"Resolved platform: {platform}. {support}",
            $"已解析平台：{platform}。{support}");
        DemoStatus.Text = GalleryLocalization.Choose(
            "The menu below is built from MdTrayMenuItem entries and exported as a native menu.",
            "下面的菜单由 MdTrayMenuItem 条目构建，并导出为原生菜单。");
        ActivationLog.Text = GalleryLocalization.Choose(
            "Entries are wired to commands, so the view model receives the same notification whether the platform reports a click or a menu entry.",
            "条目由命令驱动，因此无论平台上报的是点击还是菜单项，视图模型收到的通知都一致。");
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ReleaseTrayIcon(report: false);
    }

    private void TrayDemoToggled(object? sender, RoutedEventArgs e)
    {
        if (DemoSwitch.IsChecked == true) ShowTrayIcon();
        else ReleaseTrayIcon(report: true);
    }

    private void RaiseDemoEntry(object? sender, RoutedEventArgs e) => _trayIcon?.Activate();

    private void ShowTrayIcon()
    {
        if (_trayIcon is not null) return;

        var tray = new MdTrayIcon
        {
            ToolTipText = "Material Gallery",
            Icon = CreateIcon(),
            Command = new GalleryCommand(_ => Report("Icon activated"))
        };

        tray.Items.Add(new MdTrayMenuItem("Open gallery window")
        {
            Command = new GalleryCommand(_ => Report("Open gallery window")),
            Gesture = new KeyGesture(Key.O, KeyModifiers.Control)
        });
        tray.Items.Add(new MdTrayMenuItem("Start minimized")
        {
            ToggleType = MenuItemToggleType.CheckBox,
            Command = new GalleryCommand(_ => Report("Toggled start minimized"))
        });

        var theme = new MdTrayMenuItem("Theme");
        theme.Items.Add(new MdTrayMenuItem("Light")
        {
            ToggleType = MenuItemToggleType.Radio,
            IsChecked = true,
            Command = new GalleryCommand(_ => Report("Theme: Light"))
        });
        theme.Items.Add(new MdTrayMenuItem("Dark")
        {
            ToggleType = MenuItemToggleType.Radio,
            Command = new GalleryCommand(_ => Report("Theme: Dark"))
        });
        tray.Items.Add(theme);
        tray.Items.Add(new MdTrayMenuItem("Export (unavailable)") { IsEnabled = false });
        tray.Items.Add(new MdTrayMenuItemSeparator());
        tray.Items.Add(new MdTrayMenuItem("Quit") { Command = new GalleryCommand(_ => Report("Quit")) });

        _trayIcon = tray;
        Report(GalleryLocalization.Choose(
            $"Tray icon shown with {tray.Items.Count} entries.",
            $"托盘图标已显示，共 {tray.Items.Count} 个条目。"));
    }

    private void ReleaseTrayIcon(bool report)
    {
        if (_trayIcon is null) return;
        _trayIcon.Dispose();
        _trayIcon = null;
        _iconBitmap?.Dispose();
        _iconBitmap = null;
        if (report) Report(GalleryLocalization.Choose("Tray icon removed.", "托盘图标已移除。"));
    }

    /// <summary>
    /// The platform takes a bitmap, so the Material glyph is rasterised. The same helper the
    /// desktop shell uses for its own tray icon builds it, so the sample cannot drift away from
    /// what the application actually ships.
    /// </summary>
    private WindowIcon? CreateIcon()
    {
        _iconBitmap?.Dispose();
        _iconBitmap = TrayIconArt.Create(Application.Current!, MdSymbols.Notifications);
        return _iconBitmap is null ? null : new WindowIcon(_iconBitmap);
    }

    private void Report(string message)
    {
        _activations++;
        ActivationLog.Text = GalleryLocalization.Choose(
            $"{message} · {_activations} activation(s) this session.",
            $"{message} · 本次会话 {_activations} 次激活。");
    }
}
