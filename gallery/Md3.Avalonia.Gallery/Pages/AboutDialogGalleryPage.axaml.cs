using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class AboutDialogGalleryPage : UserControl
{
    private readonly MdLicenseEntry[] _licenses;
    private int _dialogStatusVersion;
    private MdDialogHost? _observedHost;
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    private MdDialogHost ActiveDialogHost => TopLevel.GetTopLevel(this) is MainWindow main ? main.DialogHost : LocalDialogHost;

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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_observedHost is not null) _observedHost.PropertyChanged -= OnDialogHostPropertyChanged;
        _observedHost = null;
        ++_dialogStatusVersion;
        base.OnDetachedFromVisualTree(e);
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
        ShowObservedDialog(
            ActiveDialogHost,
            about,
            replace,
            L("About dialog opened; choose View licenses or Close.", "“关于”对话框已打开；请选择“查看许可证”或“关闭”。"));
    }

    private void ShowLicenseList(object? sender, RoutedEventArgs e)
    {
        // Maxima, not fixed dimensions: a dialog is 760x440 on a roomy desktop but must be free
        // to become the compact license layout inside a phone's 24dp overlay margins.
        var page = new MdLicensePage
        {
            Licenses = _licenses,
            FilterLabel = L("Filter packages", "筛选包"),
            MaxWidth = 760,
            MaxHeight = 440,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        var back = new MdButton { Content = L("Back to about", "返回关于"), Variant = MdButtonVariant.Text, Size = MdButtonSize.ExtraSmall };
        back.Click += BackToAboutDialog;
        var close = new MdButton { Content = L("Close", "关闭"), Variant = MdButtonVariant.Text, Size = MdButtonSize.ExtraSmall };
        close.Click += (_, _) => ActiveDialogHost.Close();
        var actions = new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal, Spacing = 8 };
        actions.Children.Add(back);
        actions.Children.Add(close);
        var dialog = new MdDialog
        {
            Headline = L("Open source licenses", "开源许可证"),
            Content = page,
            Actions = actions,
            MinWidth = 0,
            MaxWidth = 900,
            MaxHeight = 640
        };
        ShowObservedDialog(
            ActiveDialogHost,
            dialog,
            replace: true,
            L("License dialog opened; select a package or return to About.", "许可证对话框已打开；请选择包或返回“关于”。"));
    }

    private void ShowObservedDialog(MdDialogHost host, object surface, bool replace, string openedStatus)
    {
        var version = ++_dialogStatusVersion;
        if (!ReferenceEquals(_observedHost, host))
        {
            if (_observedHost is not null) _observedHost.PropertyChanged -= OnDialogHostPropertyChanged;
            _observedHost = host;
            host.PropertyChanged += OnDialogHostPropertyChanged;
        }

        // Increment the version before replacing: Replace briefly closes the old surface while it
        // swaps requests, and its close notification must not overwrite the new surface's status.
        if (replace) host.Replace(surface);
        else host.Show(surface);

        // The service accepts a request synchronously, but the presenter does not become visible
        // until its render turn. Announce what is actually on screen, not what was merely queued.
        Dispatcher.UIThread.Post(() =>
        {
            if (version == _dialogStatusVersion && host.IsOpen && ReferenceEquals(host.Dialog, surface))
                LicenseStatus.Text = openedStatus;
        }, DispatcherPriority.Render);
    }

    private void OnDialogHostPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != MdDialogHost.IsOpenProperty || e.NewValue is not false || sender is not MdDialogHost host)
            return;

        var version = _dialogStatusVersion;
        Dispatcher.UIThread.Post(() =>
        {
            if (version == _dialogStatusVersion && !host.IsOpen)
                LicenseStatus.Text = L("About dialog is closed.", "“关于”对话框已关闭。");
        }, DispatcherPriority.Render);
    }
}
