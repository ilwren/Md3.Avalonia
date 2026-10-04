using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class FlutterParityGalleryPage : UserControl
{
    private readonly MdPickerRestorationStore _restorationStore = new();
    private readonly MdLicenseEntry[] _licenses;
    private MdSimpleDialog? _simpleDialog;
    private bool _heroExpanded;
    private bool _heroTransitionRunning;
    private bool _adaptiveSyncRunning;
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);
    private MdDialogHost ActiveDialogHost => TopLevel.GetTopLevel(this) is MainWindow main ? main.DialogHost : ParityDialogHost;

    public FlutterParityGalleryPage()
    {
        InitializeComponent();
        RoleField.ItemsSource = new[] { L("Designer", "设计师"), L("Developer", "开发者"), L("Tester", "测试人员") };
        _licenses =
        [
            new MdLicenseEntry("Md3.Avalonia", "Apache-2.0", L("Copyright contributors. Licensed under Apache License 2.0.", "版权所有者为各贡献者；根据 Apache License 2.0 授权。")),
            new MdLicenseEntry("Avalonia", "MIT", L("Avalonia is used through its public package APIs.", "Avalonia 通过其公开包 API 使用。")),
            new MdLicenseEntry("Material Symbols", "Apache-2.0", L("The optional icon package contains catalog metadata and the upstream license; the preview does not bundle a TTF.", "可选图标包包含目录元数据与上游许可证；预览版不捆绑 TTF。"))
        ];
        HeroSource.TransitionRequested += (_, args) => AdvancedStatus.Text = L($"Avatar moved: {args.SourceBounds} → {args.DestinationBounds}.", $"头像已移动：{args.SourceBounds} → {args.DestinationBounds}。");
        HeroDestination.TransitionRequested += (_, args) => AdvancedStatus.Text = L($"Avatar returned: {args.SourceBounds} → {args.DestinationBounds}.", $"头像已返回：{args.SourceBounds} → {args.DestinationBounds}。");
        ShortcutScope.Register(new KeyGesture(Key.S, KeyModifiers.Control | KeyModifiers.Shift),
            new RelayCommand(ExecuteShortcutAction));
    }

    private void FormNameChanged(object? sender, TextChangedEventArgs e)
    {
        NameField.MarkTouched();
        NameField.Value = (sender as TextBox)?.Text;
    }

    private void ValidateForm(object? sender, RoutedEventArgs e) => AccountForm.Submit();
    private void FormSubmitted(object? sender, MdFormSubmittedEventArgs e) =>
        FormStatus.Text = e.IsValid ? L("Form is valid and ready to submit.", "表单有效，可以提交。") : L("Correct the fields marked in error.", "请修正标记为错误的字段。");

    private void OpenSimpleDialog(object? sender, RoutedEventArgs e)
    {
        var dialog = new MdSimpleDialog
        {
            Title = L("Set backup account", "设置备份账户"),
            ItemsSource = new[] { "material@example.com", "avalonia@example.com", "android@example.com" },
            IsOpen = true
        };
        dialog.ItemSelected += SimpleDialogSelected;
        dialog.Dismissed += SimpleDialogDismissed;
        _simpleDialog = dialog;
        _ = ActiveDialogHost.ShowAsync(dialog);
        DialogStatus.Text = L("Choose an account in the modal dialog.", "请在模态对话框中选择账户。");
    }

    private void SimpleDialogSelected(object? sender, object? item)
    {
        DialogStatus.Text = L($"Backup account set to {item}.", $"备份账户已设置为 {item}。");
        ActiveDialogHost.Close(item);
        _simpleDialog = null;
    }

    private void SimpleDialogDismissed(object? sender, EventArgs e)
    {
        // Dismissed now only fires when the dialog closed without a choice, so this no longer
        // has to infer the outcome from a field the selection handler has already cleared.
        if (ActiveDialogHost.IsOpen) ActiveDialogHost.Close();
        DialogStatus.Text = L("Account selection cancelled.", "已取消账户选择。");
        _simpleDialog = null;
    }

    private void OpenAboutDialog(object? sender, RoutedEventArgs e)
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
        _ = ActiveDialogHost.ShowAsync(about);
        LicenseStatus.Text = L("About dialog opened; choose View licenses or Close.", "“关于”对话框已打开；请选择“查看许可证”或“关闭”。");
    }

    private void ShowLicenseList(object? sender, RoutedEventArgs e)
    {
        var page = new MdLicensePage { Licenses = _licenses, FilterLabel = L("Filter packages", "筛选包"), Width = 760, Height = 440 };
        var back = new MdButton { Content = L("Back to about", "返回关于"), Variant = MdButtonVariant.Text, Size = MdButtonSize.ExtraSmall };
        back.Click += OpenAboutDialog;
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
            MinWidth = 820,
            MaxWidth = 900
        };
        _ = ActiveDialogHost.ShowAsync(dialog);
        LicenseStatus.Text = L("License dialog opened; select a package or return to About.", "许可证对话框已打开；请选择包或返回“关于”。");
    }

    private void SavePickerState(object? sender, RoutedEventArgs e)
    {
        _restorationStore.SaveDate("parity.date", RestorableDate.SelectedDate);
        _restorationStore.SaveTime("parity.time", RestorableTime.SelectedTime);
        RestorationStatus.Text = L("Picker state saved.", "选择器状态已保存。");
    }

    private void ClearPickerValues(object? sender, RoutedEventArgs e)
    {
        RestorableDate.SelectedDate = null;
        RestorableTime.SelectedTime = null;
        RestorationStatus.Text = L("Values cleared; saved state retained.", "值已清除；已保存状态仍保留。");
    }

    private void RestorePickerState(object? sender, RoutedEventArgs e)
    {
        RestorableDate.SelectedDate = _restorationStore.RestoreDate("parity.date");
        RestorableTime.SelectedTime = _restorationStore.RestoreTime("parity.time");
        RestorationStatus.Text = L("Picker state restored.", "选择器状态已恢复。");
    }

    private void SheetExtentChanged(object? sender, double extent) => SheetStatus.Text = L($"Extent {extent:P0}", $"展开比例 {extent:P0}");

    private void ToggleVirtualKeyboard(object? sender, RoutedEventArgs e)
    {
        if (DemoKeyboardHost.KeyboardHeight > 0)
        {
            DemoKeyboardHost.KeyboardHeight = 0;
            KeyboardStatus.Text = L("Virtual keyboard inactive (height: 0dp).", "虚拟键盘已关闭 (高度: 0dp)。");
        }
        else
        {
            DemoKeyboardHost.KeyboardHeight = 150;
            KeyboardStatus.Text = L("Virtual keyboard active (height: 150dp). Viewport adjusted.", "虚拟键盘已激活 (高度: 150dp)。视口已自动调整。");
        }
    }

    private void FocusBottomInput(object? sender, RoutedEventArgs e)
    {
        BottomInput.Focus();
        KeyboardStatus.Text = L("Focused bottom input. Scrolled into view.", "已聚焦底部输入框并滚动至可视区域。");
    }

    private void AdaptiveControlChanged(object? sender, RoutedEventArgs e) =>
        AdaptiveStatus.Text = L($"Platform policy: {(AutomaticAdaptiveSwitch.IsChecked == true ? "automatic" : "manual")}; iOS preview {(CupertinoAdaptiveSwitch.IsChecked == true ? "enabled" : "disabled")}.", $"平台策略：{(AutomaticAdaptiveSwitch.IsChecked == true ? "自动" : "手动")}；iOS 预览已{(CupertinoAdaptiveSwitch.IsChecked == true ? "启用" : "禁用")}。");

    private async void RunAdaptiveSync(object? sender, RoutedEventArgs e)
    {
        if (_adaptiveSyncRunning) return;
        _adaptiveSyncRunning = true;
        MaterialAdaptiveProgress.Value = 0;
        CupertinoAdaptiveProgress.Value = 0;
        for (var value = 0; value <= 100; value += 5)
        {
            MaterialAdaptiveProgress.Value = value;
            CupertinoAdaptiveProgress.Value = value;
            AdaptiveStatus.Text = L($"Uploading design tokens · {value}%", $"正在上传设计令牌 · {value}%");
            await Task.Delay(35);
        }
        AdaptiveStatus.Text = L("Upload complete in both platform presentations.", "两种平台呈现均已完成上传。");
        _adaptiveSyncRunning = false;
    }

    private void DemoControlFocused(object? sender, RoutedEventArgs e)
    {
        var label = (sender as ContentControl)?.Content?.ToString() ?? L("control", "控件");
        AdvancedStatus.Text = L($"Keyboard focus moved to {label}.", $"键盘焦点已移至 {label}。");
    }

    private void FocusFirstControl(object? sender, RoutedEventArgs e)
    {
        FocusAlpha.Focus();
        AdvancedStatus.Text = L("Focus traversal reset to Alpha; Tab now advances through Beta and Gamma.", "焦点遍历已重置到 Alpha；按 Tab 会依次进入 Beta 和 Gamma。");
    }

    private void RunShortcutAction(object? sender, RoutedEventArgs e) => ExecuteShortcutAction();

    private void ExecuteShortcutAction() =>
        AdvancedStatus.Text = L($"Shortcut action ran at {DateTime.Now:HH:mm:ss}.", $"快捷键操作已于 {DateTime.Now:HH:mm:ss} 执行。");

    private async void RequestHero(object? sender, RoutedEventArgs e)
    {
        // The flight is awaited now, so a second click mid-air would interleave the two toggles.
        if (_heroTransitionRunning) return;
        _heroTransitionRunning = true;
        var source = _heroExpanded ? HeroDestination : HeroSource;
        var destination = _heroExpanded ? HeroSource : HeroDestination;

        // The destination is hidden until this click, so it has no bounds yet. Starting the
        // flight in the same pass made MdHero measure and photograph a zero-sized rectangle and
        // give up, and hiding the source immediately afterwards reflowed the row under a
        // transition that had already recorded where things were -- hence the fragment of content
        // visible for the first frames. Reveal, let one layout pass run, fly, and only then hide
        // the source, which stays in the layout at zero opacity while the flight is in the air.
        destination.IsVisible = true;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Loaded);
        await source.TransitionToAsync(destination);
        source.IsVisible = false;
        _heroExpanded = !_heroExpanded;
        _heroTransitionRunning = false;
    }
}
