using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Extra.Infrastructure;
using Md3.Avalonia.Gallery.Pages;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdEcosystemWaveAndWindowTests
{
    [AvaloniaFact]
    public void WaveA_CommandPalette_Filters_Executes_And_Closes()
    {
        var executions = 0;
        var palette = new MdCommandPalette
        {
            ItemsSource = new[]
            {
                new MdCommandItem("Open settings", new TestCommand(_ => executions++), Keywords: "preferences"),
                new MdCommandItem("Create document", new TestCommand(_ => executions++), Keywords: "new file")
            },
            Query = "pref"
        };
        Assert.Single(palette.FilteredItems);
        palette.Show();
        Assert.True(palette.IsOpen);
        Assert.True(palette.ExecuteSelected());
        Assert.Equal(1, executions);
        Assert.False(palette.IsOpen);
    }

    [AvaloniaFact]
    public async Task WaveB_Paging_Sliding_Masonry_And_DataGrid_APIs_Work()
    {
        var pages = new MdPagedItemsView { PageSize = 2 };
        pages.PageProvider = request => ValueTask.FromResult(new MdPageResult<object?>([$"P{request.PageKey}-1", $"P{request.PageKey}-2"], request.PageKey == 0 ? 1 : null, request.PageKey > 0));
        await pages.LoadNextPageAsync();
        Assert.Equal(MdAsyncRequestState.Data, pages.State);
        await pages.LoadNextPageAsync();
        Assert.Equal(4, pages.Items.Count);
        Assert.Equal(MdAsyncRequestState.Completed, pages.State);

        var slidable = new MdSlidableItem { ActionExtent = 120 };
        slidable.OpenEnd(); Assert.Equal(-120, slidable.Offset); slidable.Close(); Assert.Equal(0, slidable.Offset);

        var grid = new MdDataGrid { DataSource = new[] { new Row("Zed", 1), new Row("Ada", 3) } };
        grid.Columns.Add(new MdDataGridColumn { Header = "Name", PropertyName = nameof(Row.Name) });
        grid.Columns.Add(new MdDataGridColumn { Header = "Score", PropertyName = nameof(Row.Score), IsEditable = true });
        grid.SortBy(nameof(Row.Name), ListSortDirection.Ascending);
        Assert.StartsWith("Name\tScore", grid.BuildClipboardText(false));
        Assert.Equal("Ada", ((Row)grid.ItemsSource!.Cast<object>().First()).Name);

        var masonry = new MdMasonryPanel { Width = 400, ColumnCount = 2 };
        masonry.Children.Add(new Border { Height = 80 }); masonry.Children.Add(new Border { Height = 120 });
        using var host = Show(masonry, 420, 300);
        Dispatcher.UIThread.RunJobs();
        Assert.True(masonry.Bounds.Height > 0);
    }

    [AvaloniaFact]
    public void WaveC_Calendar_Cascader_Transfer_And_Result_APIs_Work()
    {
        var calendar = new MdCalendar { SelectionMode = MdCalendarSelectionMode.Range };
        var first = new DateTimeOffset(2026, 9, 4, 0, 0, 0, TimeSpan.Zero);
        var second = first.AddDays(5);
        calendar.SelectDate(first); calendar.SelectDate(second);
        Assert.Equal(first, calendar.SelectedDate); Assert.Equal(second, calendar.RangeEnd); Assert.Equal(42, calendar.VisibleDays.Count);

        var leaf = new MdCascaderItem("leaf", "Leaf");
        var parent = new MdCascaderItem("parent", "Parent", [leaf]);
        var cascader = new MdCascader { ItemsSource = [parent], IsDropDownOpen = true };
        cascader.Select(0, parent); cascader.Select(1, leaf);
        Assert.Equal([parent, leaf], cascader.SelectedPath);
        Assert.False(cascader.IsDropDownOpen);

        var transfer = new MdTransfer { ItemsSource = new[] { "A", "B", "C" }, SelectedItems = new[] { "A" } };
        transfer.MoveToTarget(["B"]); Assert.Equal(2, transfer.TargetItems.Count);
        transfer.MoveToSource(["A"]); Assert.Equal(["B"], transfer.TargetItems);
        var multi = new MdAsyncSelect { ItemsSource = new[] { "A", "B" }, IsMultiSelect = true };
        multi.Commit("A"); multi.Commit("B"); Assert.Equal(["A", "B"], multi.SelectedItems);
        Assert.Equal(MdResultKind.Empty, new MdResultView { Kind = MdResultKind.Empty }.Kind);
    }

    [AvaloniaFact]
    public void WaveD_Chart_Editor_And_Chat_Contracts_Are_Provider_Neutral()
    {
        var chart = new MdChart { Series = [new MdChartSeries("Series", [new(0, 1), new(1, 3)])], Width = 400, Height = 240 };
        Assert.Contains("Series\t0\t1", chart.BuildAccessibleTable());
        var adapter = new EditorAdapter();
        var editor = new MdRichEditor { Adapter = adapter };
        Assert.True(editor.Execute(MdRichEditorCommand.Bold));
        Assert.Equal(MdRichEditorCommand.Bold, adapter.LastCommand);
        Assert.True(editor.Execute(MdRichEditorCommand.HorizontalRule));
        Assert.Equal(MdRichEditorCommand.HorizontalRule, adapter.LastCommand);
        Assert.NotEmpty(MdRichEditorCommandConverters.GetGlyph(MdRichEditorCommand.Bold));
        Assert.NotEmpty(MdRichEditorCommandConverters.GetTooltip(MdRichEditorCommand.Italic));

        var submitted = string.Empty;
        var attachmentInvoked = false;
        var chat = new MdChatView { ComposerText = "Hello" };
        chat.MessageSubmitted += (_, text) => submitted = text;
        chat.AttachmentRequested += (_, _) => attachmentInvoked = true;
        Assert.True(chat.Submit()); Assert.Equal("Hello", submitted); Assert.Empty(chat.ComposerText!);
        Assert.False(attachmentInvoked);

        var msg = new MdChatMessage("101", MdChatMessageRole.Assistant, "Hi", DateTimeOffset.Now, "Material Bot");
        var presenter = new MdChatMessagePresenter { Message = msg };
        Assert.NotEmpty(presenter.FormattedTime);
        Assert.Equal("MB", presenter.SenderInitials);
        Assert.NotEmpty(presenter.AvatarGlyph);

        using var host = Show(chart, 440, 280);
        Dispatcher.UIThread.RunJobs();
        Assert.NotNull(((Scope)host).Window.CaptureRenderedFrame());
    }

    [AvaloniaFact]
    public async Task WaveE_Sequence_Refresh_And_Carousel_APIs_Work()
    {
        var sequence = new MdAnimationSequence { AutoPlay = false, ItemDelay = TimeSpan.Zero, ItemDuration = TimeSpan.Zero };
        sequence.Children.Add(new Border()); sequence.Children.Add(new Border());
        await sequence.PlayAsync();
        Assert.All(sequence.Children, child => Assert.Equal(1, child.Opacity));
        var skeleton = new MdSkeleton();
        var skeletons = new MdSkeletonGroup { IsLoading = false, Children = { skeleton } };
        skeletons.ApplyState(); Assert.False(skeleton.IsLoading);

        var refreshed = false;
        var refresh = new MdRefreshIndicator { RefreshHandler = _ => { refreshed = true; return ValueTask.CompletedTask; } };
        await refresh.RequestRefreshAsync();
        Assert.True(refreshed); Assert.False(refresh.IsRefreshing);

        var controller = new MdCarouselController();
        var carousel = new MdCarousel { ItemsSource = new[] { "A", "B", "C" }, SelectedIndex = 2, IsInfiniteLoop = true, Controller = controller };
        Assert.True(controller.Next()); Assert.Equal(0, carousel.SelectedIndex);
    }

    [AvaloniaFact]
    public void AdaptiveLayout_Exposes_Five_Breakpoints_And_Input_Mode()
    {
        var adaptive = new MdAdaptiveLayout { CompactContent = "C", MediumContent = "M", ExpandedContent = "E", LargeContent = "L", ExtraLargeContent = "XL", InputMode = MdAdaptiveInputMode.Touch };
        using var host = Show(adaptive, 1300, 700);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(MdAdaptiveBreakpoint.Large, adaptive.Breakpoint);
        Assert.Equal("Large", adaptive.BreakpointName);
        Assert.Equal(MdAdaptiveInputMode.Touch, adaptive.EffectiveInputMode);
    }

    [AvaloniaFact]
    public void Borderless_Window_Uses_Injectable_Adapter_And_Caption_APIs()
    {
        var adapter = new TestWindowAdapter();
        var window = new MdBorderlessWindow { PlatformAdapter = adapter, Width = 500, Height = 360 };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.True(adapter.Applied);
            Assert.Equal(MdWindowCapabilities.All, window.Capabilities);
            var original = window.WindowState;
            var toggled = false;
            window.MaximizeRestoreRequested += (_, _) => toggled = true;
            window.ToggleMaximizeCommand.Execute(null);
            Assert.True(toggled);
            Assert.NotEqual(original, window.WindowState);
            window.ToggleMaximizeRestore();
            Assert.Equal(WindowState.Normal, window.WindowState);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Advanced_Ecosystem_And_Borderless_Gallery_Pages_Render()
    {
        using var ecosystem = Show(new EcosystemGalleryPage(), 1100, 800);
        Dispatcher.UIThread.RunJobs();
        Assert.NotNull(((Scope)ecosystem).Window.CaptureRenderedFrame());
        using var borderless = Show(new BorderlessWindowGalleryPage(), 1100, 800);
        Dispatcher.UIThread.RunJobs();
        Assert.NotNull(((Scope)borderless).Window.CaptureRenderedFrame());
    }

    private static IDisposable Show(Control control, double width = 800, double height = 600)
    {
        var window = new Window { Width = width, Height = height, Content = control };
        window.Show();
        return new Scope(window);
    }
    private sealed class Scope(Window window) : IDisposable { public Window Window { get; } = window; public void Dispose() => Window.Close(); }
    private sealed class Row(string name, int score) { public string Name { get; } = name; public int Score { get; set; } = score; }
    private sealed class TestCommand(Action<object?> execute) : ICommand { public event EventHandler? CanExecuteChanged { add { } remove { } } public bool CanExecute(object? parameter) => true; public void Execute(object? parameter) => execute(parameter); }
    private sealed class EditorAdapter : IMdRichEditorAdapter
    {
        public object? Document { get; set; }
        public MdRichEditorCommand? LastCommand { get; private set; }
        public event EventHandler? StateChanged;
        public bool CanExecute(MdRichEditorCommand command, object? parameter = null) => true;
        public void Execute(MdRichEditorCommand command, object? parameter = null) { LastCommand = command; StateChanged?.Invoke(this, EventArgs.Empty); }
    }
    private sealed class TestWindowAdapter : IMdWindowPlatformAdapter
    {
        public MdWindowPlatform Platform => MdWindowPlatform.Unknown;
        public MdWindowCapabilities Capabilities => MdWindowCapabilities.All;
        public bool Applied { get; private set; }
        public void Apply(MdBorderlessWindow window, MdBorderlessWindowOptions options) => Applied = true;
        public bool TryBeginMove(MdBorderlessWindow window, PointerPressedEventArgs args) => true;
        public bool TryBeginResize(MdBorderlessWindow window, WindowEdge edge, PointerPressedEventArgs args) => true;
        public bool TryShowSystemMenu(MdBorderlessWindow window, Point clientPoint) => true;
    }
}
