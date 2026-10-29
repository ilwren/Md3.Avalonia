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
