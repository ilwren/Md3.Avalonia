using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class ResultViewGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public ResultViewGalleryPage()
    {
        InitializeComponent();
        ResultView.ActionCommand = new RelayCommand(() =>
        {
            ResultView.Kind = MdResultKind.Success;
            ResultView.Title = L("Record created", "记录已创建");
            ResultView.Content = L("The primary result action executed successfully.", "主要结果操作已成功执行。");
        });
    }

    // The whole demo is the action command wired in the constructor.
}
