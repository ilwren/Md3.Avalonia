using Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class TimelineGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public TimelineGalleryPage()
    {
        InitializeComponent();
        TimelineDescription.Text = L(
            "An ordered sequence of states with continuous vertical or horizontal connectors. Active work uses an accessible progress ring that becomes a static state cue when motion is reduced.",
            "使用连续垂直或水平连接线表达有序状态。进行中的任务带有可识别的进度环；减少动效时仍保留静态状态提示。");
        VerticalTitle.Text = L("Vertical · running state", "垂直布局 · 运行中状态");
        HorizontalDescription.Text = L(
            "The same item model can follow a horizontal axis. Scroll to inspect every stage on compact windows.",
            "同一组数据也可沿水平轴展示；在较窄窗口中可滚动查看全部阶段。");
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
