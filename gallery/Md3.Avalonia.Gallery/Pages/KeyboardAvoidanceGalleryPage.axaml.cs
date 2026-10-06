using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class KeyboardAvoidanceGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public KeyboardAvoidanceGalleryPage() => InitializeComponent();

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
}
