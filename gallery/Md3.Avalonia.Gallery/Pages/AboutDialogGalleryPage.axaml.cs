using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class AboutDialogGalleryPage : UserControl
{
    private readonly MdLicenseEntry[] _licenses;
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    private MdDialogHost ActiveDialogHost => GalleryDialogHosts.Find(this) ?? LocalDialogHost;

    public AboutDialogGalleryPage()
    {
        InitializeComponent();
        _licenses =
        [
            new MdLicenseEntry("Md3.Avalonia", "Apache-2.0", L("Copyright contributors. Licensed under Apache License 2.0.", "版权所有者为各贡献者；根据 Apache License 2.0 授权。")),
            new MdLicenseEntry("Avalonia", "MIT", L("Avalonia is used through its public package APIs.", "Avalonia 通过其公开包 API 使用。")),
            new MdLicenseEntry("Material Symbols", "Apache-2.0", L("The optional icon package contains catalog metadata and the upstream license; the preview does not bundle a TTF.", "可选图标包包含目录元数据与上游许可证；预览版不捆绑 TTF。"))
        ];
    }

    private void OpenAboutDialog(object? sender, RoutedEventArgs e) => ShowAboutDialog(replace: false);

    // "Back to about" is pressed from inside the license dialog, so it has to swap the displayed
    // surface. Showing would queue the about box behind the license list and reveal it only once
    // the user closed that - which reads as the Close button opening a dialog.
    private void BackToAboutDialog(object? sender, RoutedEventArgs e) => ShowAboutDialog(replace: true);

    private void ShowAboutDialog(bool replace)
    {
        var showLicenses = new MdButton { Content = L("View licenses", "查看许可证"), Variant = MdButtonVariant.Text, Size = MdButtonSize.ExtraSmall };
        showLicenses.Click += ShowLicenseList;
        var close = new MdButton { Content = L("Close", "关闭"), Variant = MdButtonVariant.Text, Size = MdButtonSize.ExtraSmall };
        close.Click += (_, _) => ActiveDialogHost.Close();
        var actions = new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal, Spacing = 8 };
        actions.Children.Add(showLicenses);
        actions.Children.Add(close);
        var about = new MdAboutDialog
        {
            ApplicationName = "Md3.Avalonia",
            ApplicationVersion = L("Preview", "预览版"),
            Legalese = L("Apache-2.0 licensed Material control library.", "采用 Apache-2.0 许可证的 Material 控件库。"),
            Content = actions
        };
        var host = ActiveDialogHost;
        _ = replace ? host.ReplaceAsync(about) : host.ShowAsync(about);
        LicenseStatus.Text = L("About dialog opened; choose View licenses or Close.", "“关于”对话框已打开；请选择“查看许可证”或“关闭”。");
    }

    private void ShowLicenseList(object? sender, RoutedEventArgs e)
    {
        var host = ActiveDialogHost;
        var viewportWidth = host.Bounds.Width > 0
            ? host.Bounds.Width
            : TopLevel.GetTopLevel(this)?.ClientSize.Width ?? Bounds.Width;
        // The two-pane desktop surface needs 820 DIP plus the host's outer margin. Narrow
        // viewports use an actual full-screen dialog instead of retaining that desktop minimum.
        var useFullScreen = viewportWidth <= 0 || viewportWidth < 900;
        var page = new MdLicensePage
        {
            Licenses = _licenses,
            FilterLabel = L("Filter packages", "筛选包")
        };
        if (!useFullScreen)
        {
            page.Width = 760;
            page.Height = 440;
        }

        var back = new MdButton { Content = L("Back to about", "返回关于"), Variant = MdButtonVariant.Text, Size = MdButtonSize.ExtraSmall };
        back.Click += BackToAboutDialog;
        var close = new MdButton { Content = L("Close", "关闭"), Variant = MdButtonVariant.Text, Size = MdButtonSize.ExtraSmall };
        close.Click += (_, _) => host.Close();
        var actions = new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal, Spacing = 8 };
        actions.Children.Add(back);
        actions.Children.Add(close);
        var dialog = new MdDialog
        {
            Headline = L("Open source licenses", "开源许可证"),
            Content = page,
            Actions = actions,
            Variant = useFullScreen ? MdDialogVariant.FullScreen : MdDialogVariant.Basic
        };
        if (!useFullScreen)
        {
            dialog.MinWidth = 820;
            dialog.MaxWidth = 900;
        }

        _ = host.ReplaceAsync(dialog);
        LicenseStatus.Text = L("License dialog opened; select a package or return to About.", "许可证对话框已打开；请选择包或返回“关于”。");
    }
}
