using Avalonia.Controls;
using Avalonia.Threading;

namespace Md3.Avalonia.Gallery.Pages;

public partial class ClockSamplePage : UserControl
{
    private readonly DispatcherTimer _clockTimer;

    public ClockSamplePage()
    {
        InitializeComponent();
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += OnClockTick;
        _clockTimer.Start();
        UpdateClock();
    }

    private void OnClockTick(object? sender, EventArgs e)
    {
        UpdateClock();
    }

    private void UpdateClock()
    {
        if (ClockTimeText is not null)
        {
            ClockTimeText.Text = DateTime.Now.ToString("HH:mm");
        }
    }
}
