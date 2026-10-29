using Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class TimelineGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public TimelineGalleryPage()
    {
        InitializeComponent();
        var now = DateTimeOffset.Now;
        var items = new[]
        {
            new MdTimelineItem(L("Created", "已创建"), L("Draft and assets prepared", "草稿和资源已准备"), now.AddHours(-4), MdTimelineItemState.Completed),
            new MdTimelineItem(L("Reviewing", "正在评审"), L("Accessibility and package checks are running", "正在进行无障碍和包检查"), now.AddMinutes(-18), MdTimelineItemState.Active),
            new MdTimelineItem(L("Publish", "发布"), L("Waiting for review", "等待评审完成"), null, MdTimelineItemState.Neutral)
        };
        VerticalTimeline.ItemsSource = items;
        HorizontalTimeline.ItemsSource = items;
    }
}
