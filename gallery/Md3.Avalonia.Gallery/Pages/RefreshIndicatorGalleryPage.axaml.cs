using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Md3.Avalonia.Gallery.Pages;

public partial class RefreshIndicatorGalleryPage : UserControl
{
    private readonly DispatcherTimer _refreshTimer;
    private int _refreshCount;
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public RefreshIndicatorGalleryPage()
    {
        InitializeComponent();
        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(650) };
        _refreshTimer.Tick += (_, _) =>
        {
            _refreshTimer.Stop();
            _refreshCount++;
            RefreshStatus.Text = L($"Updated just now · refresh {_refreshCount}", $"刚刚更新 · 第 {_refreshCount} 次刷新");
            DemoRefresh.CompleteRefresh();
        };
    }

    private void StartRefresh(object? sender, RoutedEventArgs e) => DemoRefresh.BeginRefresh();

    private void RefreshRequested(object? sender, EventArgs e)
    {
        RefreshStatus.Text = L("Refreshing…", "正在刷新…");
        _refreshTimer.Stop();
        _refreshTimer.Start();
    }
}
