using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Md3.Avalonia.Gallery.Pages;

public partial class GridSplitterGalleryPage : UserControl
{
    private bool _statusUpdatePending;

    public GridSplitterGalleryPage()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) => Dispatcher.UIThread.Post(UpdateStatus, DispatcherPriority.Loaded);
    }

    private void OnSplitterDragDelta(object? sender, VectorEventArgs e) => ScheduleStatusUpdate();

    private void OnSplitterDragCompleted(object? sender, VectorEventArgs e) => ScheduleStatusUpdate();

    private void OnSplitterReset(object? sender, EventArgs e) => ScheduleStatusUpdate();

    private void ScheduleStatusUpdate()
    {
        if (_statusUpdatePending) return;
        _statusUpdatePending = true;
        Dispatcher.UIThread.Post(() =>
        {
            _statusUpdatePending = false;
            UpdateStatus();
        }, DispatcherPriority.Render);
    }

    private void EqualizeColumns(object? sender, RoutedEventArgs e)
    {
        ColumnSplitter.ResetToEqual();
        UpdateStatus();
    }

    private void EqualizeRows(object? sender, RoutedEventArgs e)
    {
        RowSplitter.ResetToEqual();
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (ColumnDemo.ColumnDefinitions.Count >= 3)
        {
            var first = ColumnDemo.ColumnDefinitions[0].ActualWidth;
            var second = ColumnDemo.ColumnDefinitions[2].ActualWidth;
            ColumnStatus.Text = string.Format(
                GalleryLocalization.Choose(
                    "Left {0:0} dp · Right {1:0} dp",
                    "左侧 {0:0} dp · 右侧 {1:0} dp"),
                first,
                second);
        }

        if (RowDemo.RowDefinitions.Count >= 3)
        {
            var first = RowDemo.RowDefinitions[0].ActualHeight;
            var second = RowDemo.RowDefinitions[2].ActualHeight;
            RowStatus.Text = string.Format(
                GalleryLocalization.Choose(
                    "Top {0:0} dp · Bottom {1:0} dp",
                    "上方 {0:0} dp · 下方 {1:0} dp"),
                first,
                second);
        }
    }
}
