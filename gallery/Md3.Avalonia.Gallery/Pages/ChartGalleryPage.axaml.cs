using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class ChartGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public ChartGalleryPage()
    {
        InitializeComponent();
        Chart.Series =
        [
            new MdChartSeries(L("Adoption", "采用率"), [new(0, 18, L("Jan", "1月")), new(1, 31, L("Feb", "2月")), new(2, 28, L("Mar", "3月")), new(3, 54, L("Apr", "4月")), new(4, 68, L("May", "5月"))]),
            new MdChartSeries(L("Quality", "质量"), [new(0, 42), new(1, 45), new(2, 58), new(3, 72), new(4, 88)])
        ];
    }

    private void ChangeChartKind(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string tag } && Enum.TryParse<MdChartKind>(tag, out var kind)) Chart.Kind = kind;
    }

    private void ChartPointChanged(object? sender, MdChartPoint? point) => ChartStatus.Text = point is null
        ? L("Move the pointer across the chart.", "在图表上移动指针。")
        : $"{point.Label ?? $"X {point.X:0.#}"}: {point.Y:0.#}%.";
}
