using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class MaskedTextBoxGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public MaskedTextBoxGalleryPage()
    {
        InitializeComponent();
        PinStatus.Text = L("Waiting for input.", "等待输入。");
    }

    private void PinTextChanged(object? sender, TextChangedEventArgs e)
    {
        // MaskCompleted is a live read of the mask provider; report it whenever the text changes.
        PinStatus.Text = PinBox.MaskCompleted == true
            ? L("PIN complete.", "PIN 已输入完整。")
            : L("Waiting for input.", "等待输入。");
    }
}
