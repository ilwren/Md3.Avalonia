using Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class TimelineGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public TimelineGalleryPage()
    {
        InitializeComponent();
        Timeline.ItemsSource = new[]
        {
            new MdTimelineItem(L("Created", "已创建"), L("Draft created", "草稿已创建"), DateTimeOffset.Now.AddHours(-4), MdTimelineItemState.Completed),
            new MdTimelineItem(L("Review", "评审"), L("Accessibility and visuals", "无障碍与视觉"), DateTimeOffset.Now.AddHours(-1), MdTimelineItemState.Active),
            new MdTimelineItem(L("Publish", "发布"), L("Package validation", "包验证"), null, MdTimelineItemState.Neutral)
        };
    }

    // The timeline is data only; there is nothing to handle.
}
