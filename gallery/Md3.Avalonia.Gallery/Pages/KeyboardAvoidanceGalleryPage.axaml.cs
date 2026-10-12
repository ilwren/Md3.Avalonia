using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class KeyboardAvoidanceGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public KeyboardAvoidanceGalleryPage()
    {
        InitializeComponent();
        if (!OperatingSystem.IsAndroid())
        {
            DesktopNote.IsVisible = true;
            ((TextBlock)DesktopNote.Child).Text = L(
                "On desktop this page is a simulation: soft-keyboard avoidance exists for the Android IME, while desktop input methods never cover the window. The strip below stands in for a keyboard inset so MdKeyboardAvoidingHost can be exercised on any platform.",
                "桌面端本页为模拟演示：软键盘避让是为 Android 输入法设计的，桌面输入法不会遮挡窗口。下方色条用于模拟键盘高度，以便在任意平台上验证 MdKeyboardAvoidingHost 的行为。");
        }
    }

    private void ToggleVirtualKeyboard(object? sender, RoutedEventArgs e)
    {
        if (DemoKeyboardHost.KeyboardHeight > 0)
        {
            DemoKeyboardHost.KeyboardHeight = 0;
            if (FakeKeyboard is not null) FakeKeyboard.Height = 0;
            KeyboardStatus.Text = L("Virtual keyboard inactive (height: 0dp).", "虚拟键盘已关闭 (高度: 0dp)。");
        }
        else
        {
            DemoKeyboardHost.KeyboardHeight = 150;
            if (FakeKeyboard is not null) FakeKeyboard.Height = 150;
            KeyboardStatus.Text = L("Virtual keyboard active (height: 150dp). Viewport adjusted.", "虚拟键盘已激活 (高度: 150dp)。视口已自动调整。");
        }
    }

    private void FocusBottomInput(object? sender, RoutedEventArgs e)
    {
        BottomInput.Focus();
        KeyboardStatus.Text = L("Focused bottom input. Scrolled into view.", "已聚焦底部输入框并滚动至可视区域。");
    }
}
