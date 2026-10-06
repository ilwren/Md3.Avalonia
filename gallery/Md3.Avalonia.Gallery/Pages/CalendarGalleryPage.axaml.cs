using Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class CalendarGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public CalendarGalleryPage()
    {
        InitializeComponent();
        Calendar.SelectionMode = MdCalendarSelectionMode.Range;
        Calendar.BadgeProvider = date => date.Day is 5 or 14 or 24 ? "•" : null;
    }

    private void CalendarDateInvoked(object? sender, DateTimeOffset date) =>
        CalendarStatus.Text = Calendar.RangeEnd is { } end
            ? L($"Range: {Calendar.SelectedDate:d} – {end:d}", $"范围：{Calendar.SelectedDate:d} – {end:d}")
            : L($"Range starts {date:d}. Drag to an end date.", $"范围起点为 {date:d}。请拖动到结束日期。");
}
