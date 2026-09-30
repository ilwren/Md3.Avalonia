using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class ClockSamplePage : UserControl
{
    private readonly DispatcherTimer _clockTimer;
    private int _timerSeconds = 300;
    private bool _timerRunning;
    private int _stopwatchHundredths = 0;
    private bool _stopwatchRunning;

    public ClockSamplePage()
    {
        InitializeComponent();
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _clockTimer.Tick += OnClockTick;
        _clockTimer.Start();
    }

    private void OnClockTick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        if (CurrentTimeText is not null)
            CurrentTimeText.Text = now.ToString("HH:mm:ss");
        if (CurrentDateText is not null)
            CurrentDateText.Text = now.ToString("dddd, MMMM dd · Local time");

        if (_stopwatchRunning)
        {
            _stopwatchHundredths += 5;
            var totalSeconds = _stopwatchHundredths / 100;
            var minutes = totalSeconds / 60;
            var seconds = totalSeconds % 60;
            var hundredths = _stopwatchHundredths % 100;
            if (StopwatchDisplay is not null)
                StopwatchDisplay.Text = $"{minutes:D2}:{seconds:D2}.{hundredths:D2}";
        }
    }

    private void OnTabChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (AlarmSection is null || ClockSection is null || TimerSection is null || StopwatchSection is null) return;
        var index = ClockTabs?.SelectedIndex ?? 0;
        AlarmSection.IsVisible = index == 0;
        ClockSection.IsVisible = index == 1;
        TimerSection.IsVisible = index == 2;
        StopwatchSection.IsVisible = index == 3;
    }

    private void OnAddAlarmClick(object? sender, RoutedEventArgs e)
    {
        if (ClockStatus is not null)
            ClockStatus.Text = "Opening time picker to set new alarm...";
    }

    private void OnAddTimerMinute(object? sender, RoutedEventArgs e)
    {
        if (sender is MdChip chip)
        {
            if (chip.Content?.ToString()?.Contains("1 min") == true) _timerSeconds += 60;
            else if (chip.Content?.ToString()?.Contains("5 min") == true) _timerSeconds += 300;
            else if (chip.Content?.ToString()?.Contains("10 min") == true) _timerSeconds += 600;

            var min = _timerSeconds / 60;
            var sec = _timerSeconds % 60;
            if (TimerDisplay is not null) TimerDisplay.Text = $"{min:D2}:{sec:D2}";
        }
    }

    private void OnTimerToggle(object? sender, RoutedEventArgs e)
    {
        _timerRunning = !_timerRunning;
        if (TimerStartBtn is not null)
            TimerStartBtn.Content = _timerRunning ? "Pause" : "Start";
    }

    private void OnTimerReset(object? sender, RoutedEventArgs e)
    {
        _timerRunning = false;
        _timerSeconds = 300;
        if (TimerDisplay is not null) TimerDisplay.Text = "05:00";
        if (TimerStartBtn is not null) TimerStartBtn.Content = "Start";
    }

    private void OnStopwatchToggle(object? sender, RoutedEventArgs e)
    {
        _stopwatchRunning = !_stopwatchRunning;
        if (StopwatchToggleBtn is not null)
            StopwatchToggleBtn.Content = _stopwatchRunning ? "Stop" : "Start";
    }

    private void OnStopwatchLap(object? sender, RoutedEventArgs e)
    {
        if (ClockStatus is not null)
            ClockStatus.Text = $"Recorded lap: {StopwatchDisplay?.Text}";
    }
}
