using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class PickerRestorationGalleryPage : UserControl
{
    private readonly MdPickerRestorationStore _restorationStore = new();
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public PickerRestorationGalleryPage() => InitializeComponent();

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
}
