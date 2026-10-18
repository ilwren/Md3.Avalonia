using Avalonia.Controls;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;

namespace Md3.Avalonia.Gallery.Pages;

public partial class LiveChartsGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public LiveChartsGalleryPage()
    {
        InitializeComponent();

        // The charts themselves are configured the way an application would: paints, sizes and
        // radii are left to the theme that App.axaml.cs installed, so what these views show is what
        // Md3.Avalonia.LiveCharts2 produces rather than what this page typed in.
        Cartesian.Series =
        [
            new ColumnSeries<double>
            {
                Name = L("Installs", "安装量"),
                Values = [128, 164, 149, 203, 246, 288],
            },
            new LineSeries<double>
            {
                Name = L("Crash-free sessions", "无崩溃会话"),
                Values = [92, 93, 91, 95, 96, 97],
            },
        ];
        Cartesian.XAxes =
        [
            new Axis { Labels = [L("Jan", "1月"), L("Feb", "2月"), L("Mar", "3月"), L("Apr", "4月"), L("May", "5月"), L("Jun", "6月")] },
        ];
        Cartesian.YAxes = [new Axis { Name = L("Per month", "每月") }];
        Cartesian.LegendPosition = LegendPosition.Bottom;

        Pie.Series =
        [
            new PieSeries<double> { Name = L("Buttons", "按钮"), Values = [42], InnerRadius = 60 },
            new PieSeries<double> { Name = L("Text fields", "文本字段"), Values = [28], InnerRadius = 60 },
            new PieSeries<double> { Name = L("Lists", "列表"), Values = [19], InnerRadius = 60 },
        ];
        Pie.LegendPosition = LegendPosition.Right;
    }
}
