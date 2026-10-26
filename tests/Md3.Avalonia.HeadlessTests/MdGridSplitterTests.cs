using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Localization;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdGridSplitterTests
{
    [AvaloniaFact]
    public void Column_Splitter_Uses_Material_Track_Grip_And_Localized_Automation()
    {
        var splitter = new MdGridSplitter
        {
            [Grid.ColumnProperty] = 1,
            ResizeDirection = GridResizeDirection.Columns
        };
        MdLocalization.SetCulture(splitter, CultureInfo.GetCultureInfo("zh-CN"));
        var grid = ColumnGrid(splitter, "*,Auto,*");
        var window = new Window { Width = 612, Height = 300, Content = grid };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(12, splitter.Bounds.Width, 1);
            Assert.Equal("调整列宽", AutomationProperties.GetName(splitter));
            var columnVisual = splitter.GetVisualDescendants().OfType<Grid>()
                .Single(control => control.Name == "PART_ColumnVisual");
            var rowVisual = splitter.GetVisualDescendants().OfType<Grid>()
                .Single(control => control.Name == "PART_RowVisual");
            Assert.True(columnVisual.IsVisible);
            Assert.False(rowVisual.IsVisible);
            Assert.True(splitter.GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_ColumnHandle").IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ResetToEqual_Equalizes_Star_Columns_And_Raises_One_Event()
    {
        var splitter = new MdGridSplitter
        {
            [Grid.ColumnProperty] = 1,
            ResizeDirection = GridResizeDirection.Columns
        };
        var grid = ColumnGrid(splitter, "2*,Auto,*");
        var resetCount = 0;
        splitter.SplitReset += (_, _) => resetCount++;
        var window = new Window { Width = 612, Height = 240, Content = grid };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.True(grid.ColumnDefinitions[0].ActualWidth > grid.ColumnDefinitions[2].ActualWidth);

            Assert.True(splitter.ResetToEqual());
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, resetCount);
            Assert.Equal(grid.ColumnDefinitions[0].ActualWidth, grid.ColumnDefinitions[2].ActualWidth, 1);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ResetToEqual_Respects_Minimums_For_Rows()
    {
        var splitter = new MdGridSplitter
        {
            [Grid.RowProperty] = 1,
            ResizeDirection = GridResizeDirection.Rows
        };
        var rows = new RowDefinitions
        {
            new RowDefinition(1, GridUnitType.Star) { MinHeight = 240 },
            new RowDefinition(GridLength.Auto),
            new RowDefinition(2, GridUnitType.Star) { MinHeight = 80 }
        };
        var grid = new Grid
        {
            RowDefinitions = rows,
            Children =
            {
                new Border { [Grid.RowProperty] = 0 },
                splitter,
                new Border { [Grid.RowProperty] = 2 }
            }
        };
        var window = new Window { Width = 500, Height = 412, Content = grid };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.True(splitter.ResetToEqual());
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(240, rows[0].ActualHeight, 1);
            Assert.Equal(160, rows[2].ActualHeight, 1);
            Assert.Equal("Resize rows", AutomationProperties.GetName(splitter));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Inherited_Drag_Engine_Still_Resizes_And_Hidden_Grip_Keeps_The_Track()
    {
        var splitter = new MdGridSplitter
        {
            [Grid.ColumnProperty] = 1,
            ResizeDirection = GridResizeDirection.Columns,
            ShowHandle = false
        };
        var grid = ColumnGrid(splitter, "*,Auto,*");
        var window = new Window { Width = 612, Height = 240, Content = grid };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var before = grid.ColumnDefinitions[0].ActualWidth;

            splitter.RaiseEvent(new VectorEventArgs { RoutedEvent = Thumb.DragStartedEvent });
            splitter.RaiseEvent(new VectorEventArgs
            {
                RoutedEvent = Thumb.DragDeltaEvent,
                Vector = new Vector(40, 0)
            });
            splitter.RaiseEvent(new VectorEventArgs { RoutedEvent = Thumb.DragCompletedEvent });
            Dispatcher.UIThread.RunJobs();

            Assert.True(grid.ColumnDefinitions[0].ActualWidth > before);
            Assert.Equal(12, splitter.Bounds.Width, 1);
            Assert.False(splitter.GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_ColumnHandle").IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    private static Grid ColumnGrid(MdGridSplitter splitter, string definitions) => new()
    {
        ColumnDefinitions = new ColumnDefinitions(definitions),
        Children =
        {
            new Border { [Grid.ColumnProperty] = 0 },
            splitter,
            new Border { [Grid.ColumnProperty] = 2 }
        }
    };
}
