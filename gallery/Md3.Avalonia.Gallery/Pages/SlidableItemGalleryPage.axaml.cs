using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;

namespace Md3.Avalonia.Gallery.Pages;

public partial class SlidableItemGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public SlidableItemGalleryPage()
    {
        InitializeComponent();
        SlidableDescription.Text = L(
            "Drag horizontally to reveal semantic start or end actions. Distance and velocity settle the pane; tap an exposed action to run it, or tap the row to close. Ctrl+Left, Ctrl+Right and Escape provide keyboard access.",
            "水平拖动可显示语义化的起始或结束操作。组件会根据距离和速度自动吸附；点击已显示的操作即可执行，点击内容行则关闭菜单。也可使用 Ctrl+Left、Ctrl+Right 和 Escape 键操作。");
        BehindTitle.Text = L("Behind motion", "Behind 动效");
        BehindDescription.Text = L(
            "The actions remain behind the row while the foreground tracks the pointer. Opening another item in the same group closes the first.",
            "操作区固定在内容行后方，前景会跟随指针移动。打开同组中的另一项时，前一项会自动关闭。");
        ScrollTitle.Text = L("Scroll motion and RTL", "Scroll 动效与 RTL");
        ScrollDescription.Text = L(
            "Scroll motion lets the action travel in from the outer edge. In right-to-left flow, start/end panes and physical translation mirror automatically.",
            "Scroll 动效会让操作从外侧边缘移入。在从右到左的布局中，起始/结束操作区与实际位移会自动镜像。");
        ActionStatus.Text = L("No action selected yet.", "尚未选择操作。");
        BehindSlidable.StartActionCommand = new RelayCommand(() => ReportAction(L("Design review notes archived.", "设计评审记录已归档。")));
        BehindSlidable.EndActionCommand = new RelayCommand(() => ReportAction(L("Design review notes deleted.", "设计评审记录已删除。")));
        ScrollSlidable.StartActionCommand = new RelayCommand(() => ReportAction(L("RTL row archived from its start side.", "RTL 行已从起始侧归档。")));
        ScrollSlidable.EndActionCommand = new RelayCommand(() => ReportAction(L("RTL row deleted from its end side.", "RTL 行已从结束侧删除。")));

        if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
        {
            if (KeyboardHint is not null) KeyboardHint.IsVisible = false;
            if (SlidableDescription is not null)
            {
                SlidableDescription.Text = L(
                    "Drag horizontally to reveal semantic start or end actions. Distance and velocity settle the pane; tap an exposed action to run it, or tap the row to close.",
                    "水平拖动可显示语义化的起始或结束操作。组件会根据距离和速度自动吸附；点击已显示的操作即可执行，点击内容行则关闭菜单。");
            }
        }
    }

    private void ReportAction(string message) => ActionStatus.Text = message;

    private void CloseSlidable(object? sender, RoutedEventArgs e)
    {
        BehindSlidable.Close();
        ScrollSlidable.Close();
    }
}
