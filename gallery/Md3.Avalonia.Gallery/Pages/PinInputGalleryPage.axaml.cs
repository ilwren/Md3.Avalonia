using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class PinInputGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public PinInputGalleryPage() => InitializeComponent();

    private void PinCompleted(object? sender, string code) =>
        PinStatus.Text = L($"Completed: {code}. VerifyCommand/event paths are now eligible.", $"已完成：{code}。现在可执行验证命令/事件路径。");

    private void SetPinDemo(object? sender, RoutedEventArgs e) => PinInput.SetCode("273841");

    private void ClearPin(object? sender, RoutedEventArgs e)
    {
        PinInput.Clear();
        PinStatus.Text = L("Cleared; focus returned to the editor.", "已清除；焦点已返回编辑器。");
    }

    private void TogglePinMask(object? sender, RoutedEventArgs e)
    {
        PinInput.IsObscured = !PinInput.IsObscured;
        PinStatus.Text = PinInput.IsObscured ? L("Digits are masked.", "数字已遮罩。") : L("Digits are visible.", "数字可见。");
    }

    private void TogglePinError(object? sender, RoutedEventArgs e)
    {
        PinInput.IsError = !PinInput.IsError;
        PinStatus.Text = PinInput.IsError ? L("Error state enabled.", "已启用错误状态。") : L("Error state cleared.", "已清除错误状态。");
    }
}
