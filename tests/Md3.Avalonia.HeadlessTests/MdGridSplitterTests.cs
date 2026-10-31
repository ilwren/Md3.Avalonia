using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Pages;
using Md3.Avalonia.Localization;
using Md3.Avalonia.Motion;
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

            Assert.Equal(16, splitter.Bounds.Width, 1);
            Assert.Equal("调整列宽", AutomationProperties.GetName(splitter));
            var columnVisual = splitter.GetVisualDescendants().OfType<Grid>()
                .Single(control => control.Name == "PART_ColumnVisual");
            var rowVisual = splitter.GetVisualDescendants().OfType<Grid>()
                .Single(control => control.Name == "PART_RowVisual");
            Assert.True(columnVisual.IsVisible);
            Assert.False(rowVisual.IsVisible);
            var handle = splitter.GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_ColumnHandle");
            Assert.True(handle.IsVisible);
            Assert.Equal(6, handle.Width);
            Assert.Equal(56, handle.Height);
            Assert.Equal(3, handle.GetVisualDescendants().OfType<Ellipse>().Count());
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
    public void Home_Key_Requests_The_Hosts_Default_Pane_Sizes()
    {
        var splitter = new MdGridSplitter
        {
            [Grid.ColumnProperty] = 1,
            ResizeDirection = GridResizeDirection.Columns
        };
        var requested = 0;
        splitter.ResetRequested += (_, _) => requested++;
        var window = new Window { Width = 612, Height = 240, Content = ColumnGrid(splitter, "2*,Auto,*") };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.True(splitter.Focus());
            window.KeyPress(Key.Home, RawInputModifiers.None, PhysicalKey.Home, null);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, requested);
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
            Dispatcher.UIThread.RunJobs();

            // Live mode must expose the resized layout before pointer release.
            Assert.True(grid.ColumnDefinitions[0].ActualWidth > before);

            splitter.RaiseEvent(new VectorEventArgs { RoutedEvent = Thumb.DragCompletedEvent });
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(16, splitter.Bounds.Width, 1);
            Assert.False(splitter.GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_ColumnHandle").IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Material_State_Motion_Uses_Theme_Scheme_And_Can_Be_Disabled()
    {
        var splitter = new MdGridSplitter
        {
            [Grid.ColumnProperty] = 1,
            ResizeDirection = GridResizeDirection.Columns
        };
        MdMotion.SetScheme(splitter, MdMotionScheme.Expressive);
        var window = new Window { Width = 612, Height = 240, Content = ColumnGrid(splitter, "*,Auto,*") };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var stateLayer = splitter.GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_ColumnStateLayer");
            var handle = splitter.GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_ColumnHandle");
            Assert.NotNull(stateLayer.Transitions);
            Assert.NotNull(handle.Transitions);

            MdMotion.SetScheme(splitter, MdMotionScheme.None);

            Assert.Null(stateLayer.Transitions);
            Assert.Null(handle.Transitions);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Gallery_Demos_Start_With_Useful_Travel_And_Live_Row_Updates()
    {
        var page = new GridSplitterGalleryPage();
        var columnDemo = page.FindControl<Grid>("ColumnDemo")!;
        var rowDemo = page.FindControl<Grid>("RowDemo")!;
        var rowSplitter = page.FindControl<MdGridSplitter>("RowSplitter")!;

        Assert.NotEqual(columnDemo.ColumnDefinitions[0].Width, columnDemo.ColumnDefinitions[2].Width);
        Assert.Equal(120, columnDemo.ColumnDefinitions[0].MinWidth);
        Assert.Equal(16, columnDemo.ColumnDefinitions[1].Width.Value);
        Assert.Equal(GridUnitType.Pixel, columnDemo.ColumnDefinitions[1].Width.GridUnitType);
        Assert.Equal(120, columnDemo.ColumnDefinitions[2].MinWidth);
        Assert.Equal(rowDemo.RowDefinitions[0].Height, rowDemo.RowDefinitions[2].Height);
        Assert.Equal(80, rowDemo.RowDefinitions[0].MinHeight);
        Assert.Equal(80, rowDemo.RowDefinitions[2].MinHeight);
        Assert.False(rowSplitter.ShowsPreview);
    }

    [AvaloniaFact]
    public void Gallery_Column_Demo_Resizes_From_A_Real_Pointer_Drag()
    {
        var gallery = new MainWindow { Width = 1180, Height = 820 };
        gallery.Show();
        try
        {
            Assert.True(gallery.NavigateToIndexedPage("Grid splitter"));
            Dispatcher.UIThread.RunJobs();

            var page = Assert.IsType<GridSplitterGalleryPage>(gallery.CurrentPage);
            var grid = page.FindControl<Grid>("ColumnDemo")!;
            var splitter = page.FindControl<MdGridSplitter>("ColumnSplitter")!;
            var before = grid.ColumnDefinitions[0].ActualWidth;
            var start = splitter.TranslatePoint(
                new Point(splitter.Bounds.Width / 2, splitter.Bounds.Height / 2), gallery)!.Value;
            var end = start.WithX(start.X + 72);

            gallery.MouseMove(start, RawInputModifiers.None);
            gallery.MouseDown(start, MouseButton.Left, RawInputModifiers.None);
            gallery.MouseMove(end, RawInputModifiers.LeftMouseButton);
            Dispatcher.UIThread.RunJobs();

            Assert.True(grid.ColumnDefinitions[0].ActualWidth > before + 60,
                $"Expected a live horizontal resize; before={before}, after={grid.ColumnDefinitions[0].ActualWidth}.");

            gallery.MouseUp(end, MouseButton.Left, RawInputModifiers.None);
        }
        finally
        {
            gallery.Close();
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
