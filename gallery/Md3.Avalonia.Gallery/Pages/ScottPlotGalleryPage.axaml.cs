using Avalonia.Controls;
using Md3.Avalonia.Themes;
using ScottPlot;

namespace Md3.Avalonia.Gallery.Pages;

public partial class ScottPlotGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public ScottPlotGalleryPage()
    {
        InitializeComponent();

        // The plots are themed the way an application would theme them: the theme applies the
        // Material rules for the variant each control actually has and re-applies them when the
        // application switches, because ScottPlot bakes colours into a plot when they are set.
        MaterialScottPlotTheme.Follow(Signals);
        MaterialScottPlotTheme.Follow(Bars);

        var sine = Signals.Plot.Add.Signal(Generate.Sin(51));
        sine.LegendText = L("Signal", "信号");
        var cosine = Signals.Plot.Add.Signal(Generate.Cos(51));
        cosine.LegendText = L("Cosine", "余弦");
        Signals.Plot.Axes.Bottom.Label.Text = L("Samples", "采样点");
        Signals.Plot.Axes.Title.Label.Text = L("Signal plot", "信号图");

        var installs = Bars.Plot.Add.Bars([128, 164, 149, 203, 246, 288]);
        installs.LegendText = L("Installs", "安装量");
        Bars.Plot.Axes.Bottom.Label.Text = L("Month", "月份");
        Bars.Plot.Axes.Title.Label.Text = L("Bars", "柱状图");
    }
}
