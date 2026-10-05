using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System.Globalization;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Extra.Infrastructure;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Pages;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>Regression coverage for the 23-item dark-theme acceptance report.</summary>
public sealed class MdReportedIssuesTests
{
    [AvaloniaFact]
    public void Gallery_Navigation_Has_Exactly_One_Active_Destination()
    {
        var window = new MainWindow();
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            // Synthetic on purpose: the assertion below is about which destination ends up
            // active, not about whether the rail button is reachable.
            var carouselDestination = window.GetVisualDescendants().OfType<MdButton>()
                .Single(button => button.Name == "CarouselNav");
            carouselDestination.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            var navigationButtons = window.GetVisualDescendants().OfType<MdButton>()
                .Where(button => button.Name?.EndsWith("Nav", StringComparison.Ordinal) == true)
                .ToArray();
            Assert.Equal("CarouselNav", Assert.Single(navigationButtons,
                button => button.Variant == MdButtonVariant.Tonal).Name);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Android_Gallery_Constrains_Wide_Page_Controls_At_360_And_412_Dip()
    {
        foreach (var width in new[] { 360d, 412d })
        {
            var view = new AndroidGalleryView();
            using var host = Show(view, width, 760);
            // The destinations live in the navigation drawer, which starts closed and fully
            // transparent. The synthetic click this replaced skipped straight past that, so it
            // was navigating in a way no user can. Open the drawer the way a user would first.
            var drawer = view.GetVisualDescendants().OfType<MdNavigationDrawer>().Single();
            drawer.Show();
            Dispatcher.UIThread.RunJobs();

            var destination = view.GetVisualDescendants().OfType<MdButton>()
                .Single(button => Equals(button.Content, "Segmented buttons"));
            PointerInput.Click(destination);
            Dispatcher.UIThread.RunJobs();

            var pageHost = view.GetVisualDescendants().OfType<ContentControl>()
                .Single(control => control.Name == "MobilePageHost");
            var page = view.GetVisualDescendants().OfType<SegmentedButtonGalleryPage>().Single();
            var groups = page.GetVisualDescendants().OfType<MdSegmentedButtonGroup>().ToArray();
            Assert.NotEmpty(groups);
            Assert.All(groups, group =>
            {
                Assert.True(double.IsNaN(group.Width));
                Assert.InRange(group.Bounds.Width, 1, Math.Max(1, pageHost.Bounds.Width - 32));
            });
            Assert.Contains(page.GetVisualDescendants().OfType<ScrollViewer>(),
                viewer => viewer.VerticalScrollBarVisibility == ScrollBarVisibility.Auto);

            host.Window.Width = 900;
            Dispatcher.UIThread.RunJobs();
            // Two authored groups plus the one inside the page's CodeExample tab strip.
            Assert.Equal(new[] { 480d, 560d, double.NaN }, groups.Select(group => group.Width).ToArray());
            Assert.All(groups.Take(2), group => Assert.Equal(HorizontalAlignment.Left, group.HorizontalAlignment));
        }
    }

    [AvaloniaFact]
    public void Carousel_Tap_Selects_Content_Without_Changing_Keyline_Item_Sizes()
    {
        var carousel = new MdCarousel
        {
            Width = 620,
            Height = 220,
            ItemWidth = 240,
            ItemHeight = 200,
            Variant = MdCarouselVariant.MultiBrowse,
            SelectedIndex = 0,
            ItemsSource = new[] { "One", "Two", "Three", "Four" }
        };
        using var host = Show(carousel, 680, 280);
        Dispatcher.UIThread.RunJobs();
        var containers = carousel.GetVisualDescendants().OfType<ListBoxItem>().ToArray();
        var widthsBeforeTap = containers.Select(item => item.Bounds.Width).ToArray();
        var second = containers[1];
        var point = second.TranslatePoint(new Point(second.Bounds.Width / 2, second.Bounds.Height / 2), host.Window)!.Value;

        host.Window.MouseMove(point, RawInputModifiers.None);
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        host.Window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(1, carousel.SelectedIndex);
        Assert.Equal(widthsBeforeTap, containers.Select(item => item.Bounds.Width).ToArray());
        Assert.Contains(containers, item => Math.Abs(item.Bounds.Width - 56) < 0.01);
    }

    [AvaloniaFact]
    public void Carousel_MultiBrowse_Keylines_Fit_Compact_Viewport_And_Use_Material_Midpoint()
    {
        var carousel = new MdCarousel
        {
            Width = 220,
            Height = 180,
            ItemWidth = 240,
            ItemHeight = 160,
            ItemSpacing = 8,
            SmallItemWidth = 56,
            Variant = MdCarouselVariant.MultiBrowse,
            SelectedIndex = 0,
            ItemsSource = new[] { "One", "Two", "Three", "Four" }
        };
        using var host = Show(carousel, 260, 220);
        Dispatcher.UIThread.RunJobs();

        var widths = carousel.GetVisualDescendants().OfType<ListBoxItem>()
            .Select(item => item.Bounds.Width)
            .ToArray();
        // The virtualizing panel may initially realize only the focal container. Its compact
        // width still proves that the conceptual large+medium+56+spacing arrangement was solved
        // against the 220-DIP viewport instead of retaining the authored 240-DIP width.
        Assert.NotEmpty(widths);
        Assert.InRange(widths[0], 79.9, 80.1);
    }

    [AvaloniaFact]
    public void MaterialTheme_Native_ItemsControl_Renders_ItemsSource_And_DataTemplate()
    {
        var items = new ItemsControl
        {
            Width = 320,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma" },
            ItemTemplate = new FuncDataTemplate<string>((value, _) =>
                new TextBlock { Text = $"Item: {value}" })
        };

        using var host = Show(items, 360, 220);
        Dispatcher.UIThread.RunJobs();

        Assert.NotNull(items.Template);
        Assert.Single(items.GetVisualDescendants().OfType<ItemsPresenter>());
        var labels = items.GetVisualDescendants().OfType<TextBlock>()
            .Select(text => text.Text)
            .ToArray();
        Assert.Equal(new[] { "Item: Alpha", "Item: Beta", "Item: Gamma" }, labels);
        Assert.True(items.Bounds.Height > 0);
    }

    [AvaloniaFact]
    public void Gallery_Search_CtrlK_And_Query_Filter_Work_In_The_Navigation_Page_Lifecycle()
    {
        var page = new SearchGalleryPage();
        using var host = Show(page, 900, 700);
        host.Window.KeyPress(Key.K, RawInputModifiers.Control, PhysicalKey.K, "k");
        host.Window.KeyRelease(Key.K, RawInputModifiers.Control, PhysicalKey.K, "k");
        Dispatcher.UIThread.RunJobs();

        var view = page.GetVisualDescendants().OfType<MdSearchView>().Single();
        var query = page.GetVisualDescendants().OfType<MdSearchBar>().Single(control => control.Name == "ExpandedSearchBar");
        var results = page.GetVisualDescendants().OfType<MdList>().Single();
        Assert.True(view.IsOpen);

        query.Text = "tokens";
        Dispatcher.UIThread.RunJobs();
        Assert.Single(results.Items.Cast<object>());
    }

    [AvaloniaFact]
    public void Reorderable_List_Uses_Real_Pointer_Drag_And_Drop_Targets()
    {
        var source = new ObservableCollection<string> { "Design", "Core", "Gallery", "Android" };
        var list = new MdReorderableList { Width = 360, Height = 260, ItemsSource = source };
        using var host = Show(list, 440, 320);
        var rows = list.GetVisualDescendants().OfType<ListBoxItem>().ToArray();
        Assert.Equal(4, rows.Length);
        var handle = rows[0].GetVisualDescendants().OfType<Border>().Single(border => border.Name == "PART_DragHandle");
        var from = handle.TranslatePoint(new Point(handle.Bounds.Width / 2, handle.Bounds.Height / 2), host.Window)!.Value;
        var to = rows[2].TranslatePoint(new Point(rows[2].Bounds.Width - 24, rows[2].Bounds.Height / 2), host.Window)!.Value;

        host.Window.MouseMove(from, RawInputModifiers.None);
        host.Window.MouseDown(from, MouseButton.Left, RawInputModifiers.None);
        host.Window.MouseMove(to, RawInputModifiers.LeftMouseButton);
        host.Window.MouseUp(to, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(new[] { "Core", "Gallery", "Design", "Android" }, source);
        Assert.DoesNotContain(":reordering", list.Classes);
    }

    [AvaloniaFact]
    public void GridTile_And_Bar_Expose_Observable_Activation_And_Favorite_State()
    {
        var bar = new MdGridTileBar { IsInteractive = true, Title = "Aurora" };
        var tile = new MdGridTile { Footer = bar };
        var barInvoked = 0;
        bar.Invoked += (_, _) => barInvoked++;
        Assert.True(bar.Invoke());
        tile.Activate();
        tile.ToggleFavorite();

        Assert.Equal(1, barInvoked);
        Assert.True(tile.IsActivated);
        Assert.True(tile.IsFavorite);
        Assert.Contains(":activated", tile.Classes);
        Assert.Contains(":favorite", tile.Classes);
    }

    [AvaloniaFact]
    public void AsyncSelect_Commit_Fills_The_Field_Closes_And_Can_Reopen()
    {
        // Keep this state-machine test detached because the headless platform intentionally
        // has neither a native popup implementation nor an overlay host.
        var select = new MdAsyncSelect { ItemsSource = new[] { "Alabama", "Alaska", "Arizona" }, Debounce = TimeSpan.Zero };
        select.IsDropDownOpen = true;
        select.Commit("Alaska");
        Assert.Equal("Alaska", select.SelectedItem);
        Assert.Equal("Alaska", select.Query);
        Assert.False(select.IsDropDownOpen);

        select.IsDropDownOpen = true;
        Assert.True(select.IsDropDownOpen);
        Assert.Contains("Alaska", select.Results);
    }

    [AvaloniaFact]
    public void Calendar_Range_Gesture_Normalizes_A_Continuous_Drag_In_Either_Direction()
    {
        var calendar = new MdCalendar { SelectionMode = MdCalendarSelectionMode.Range };
        var start = new DateTimeOffset(2026, 9, 18, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 9, 12, 0, 0, 0, TimeSpan.Zero);
        Assert.True(calendar.BeginRangeSelection(start));
        Assert.True(calendar.UpdateRangeSelection(end));
        Assert.True(calendar.CompleteRangeSelection());
        Assert.Equal(end, calendar.SelectedDate);
        Assert.Equal(start, calendar.RangeEnd);
        Assert.Equal(7, calendar.VisibleDays.Count(day => day.IsSelected));
    }

    [AvaloniaFact]
    public void Cascader_Field_Uses_A_Single_Centered_Path_And_Commits_Only_At_A_Leaf()
    {
        var leaf = new MdCascaderItem("desktop", "Desktop");
        var root = new MdCascaderItem("engineering", "Engineering", [leaf]);
        var cascader = new MdCascader { ItemsSource = new[] { root } };
        var commits = 0;
        cascader.SelectionChanged += (_, _) => commits++;
        cascader.Select(0, root);
        Assert.Equal("Engineering", cascader.DisplayText);
        Assert.True(cascader.IsDropDownOpen || commits == 0);
        cascader.Select(1, leaf);
        Assert.Equal("Engineering / Desktop", cascader.DisplayText);
        Assert.Equal(1, commits);
        Assert.False(cascader.IsDropDownOpen);
    }

    [AvaloniaFact]
    public void Transfer_Moves_Actual_Items_When_Dropped_On_The_Other_List()
    {
        var transfer = new MdTransfer
        {
            Width = 760,
            Height = 320,
            ItemsSource = new[] { "Accessibility", "Android", "Desktop" },
            SelectedItems = new[] { "Desktop" }
        };
        using var host = Show(transfer, 840, 400);
        var lists = transfer.GetVisualDescendants().OfType<ListBox>().ToArray();
        Assert.Equal(2, lists.Length);
        var sourceRow = lists[0].GetVisualDescendants().OfType<ListBoxItem>().First();
        var from = sourceRow.TranslatePoint(new Point(24, sourceRow.Bounds.Height / 2), host.Window)!.Value;
        var to = lists[1].TranslatePoint(new Point(40, Math.Max(70, lists[1].Bounds.Height / 2)), host.Window)!.Value;

        host.Window.MouseMove(from, RawInputModifiers.None);
        host.Window.MouseDown(from, MouseButton.Left, RawInputModifiers.None);
        host.Window.MouseMove(to, RawInputModifiers.LeftMouseButton);
        host.Window.MouseUp(to, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.Contains("Accessibility", transfer.TargetItems);
        Assert.DoesNotContain("Accessibility", transfer.AvailableItems);
    }

    [AvaloniaFact]
    public void BuiltIn_Rich_Editor_Adapter_Changes_Source_And_Rendered_Preview()
    {
        var editor = new TextBox { Text = "Material editor" };
        var preview = new StackPanel();
        var adapter = new MdTextBoxRichEditorAdapter(editor, preview);
        editor.SelectionStart = 0;
        editor.SelectionEnd = 8;
        adapter.Execute(MdRichEditorCommand.Bold);

        Assert.Equal("**Material** editor", editor.Text);
        Assert.NotEmpty(preview.Children);
        Assert.Equal(MdRichEditorCommand.Bold, adapter.LastCommand);

        editor.SelectionStart = 0;
        editor.SelectionEnd = editor.Text.Length;
        adapter.Execute(MdRichEditorCommand.ClearFormatting);
        Assert.Equal("Material editor", editor.Text);
    }

    [AvaloniaFact]
    public void Chat_Supports_MultiSelect_Cancel_Delete_Quote_And_Failed_Retry()
    {
        var first = new MdChatMessage("1", MdChatMessageRole.Assistant, "First", DateTimeOffset.Now, "Assistant");
        var second = new MdChatMessage("2", MdChatMessageRole.User, "Second", DateTimeOffset.Now, "You");
        var failed = new MdChatMessage("3", MdChatMessageRole.User, "Failed", DateTimeOffset.Now, "You", MdAsyncRequestState.Error);
        var chat = new MdChatView { MessagesSource = new[] { first, second, failed } };
        chat.ToggleMessageSelection(first);
        chat.ToggleMessageSelection(second);
        Assert.Equal(2, chat.SelectionCount);
        Assert.True(chat.QuoteSelected());
        Assert.Equal(second, chat.QuotedMessage);
        Assert.Equal(0, chat.SelectionCount);

        var retried = false;
        chat.RetryRequested += (_, message) => retried = ReferenceEquals(message, failed);
        Assert.True(chat.Retry(failed));
        Assert.True(retried);

        IReadOnlyList<MdChatMessage>? deleted = null;
        chat.DeleteRequested += (_, messages) => deleted = messages;
        chat.ToggleMessageSelection(first);
        Assert.True(chat.DeleteSelected());
        Assert.Single(deleted!);
        chat.CancelQuote();
        Assert.Null(chat.QuotedMessage);
    }

    [AvaloniaFact]
    public void Pin_Input_Accepts_Rapid_Continuous_Digits_And_Shows_One_Active_Caret_Cell()
    {
        var pin = new MdPinInput { Length = 6 };
        using var host = Show(pin, 430, 140);
        var input = pin.GetVisualDescendants().OfType<TextBox>().Single(control => control.Name == "PART_Input");
        input.Focus();
        foreach (var text in new[] { "1", "12", "123", "1234", "12345", "123456" }) input.Text = text;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("123456", pin.Code);
        Assert.Equal(6, pin.Cells.Count(cell => cell.IsFilled));
        Assert.Single(pin.Cells, cell => cell.IsActive);
    }

    [AvaloniaFact]
    public void Tree_Toggle_Button_Expands_And_Collapses_Without_A_Guide_Through_The_Arrow()
    {
        var root = new MdTreeNode("root", "Root", [new MdTreeNode("child", "Child")]);
        var tree = new MdTreeView { Roots = new[] { root }, Width = 420, Height = 220 };
        using var host = Show(tree, 480, 280);
        var toggle = tree.GetVisualDescendants().OfType<ToggleButton>().Single();
        PointerInput.Click(toggle);
        Dispatcher.UIThread.RunJobs();
        Assert.True(root.IsExpanded);
        Assert.Equal(2, tree.VisibleRows.Count);
        Assert.False(tree.ShowGuides);

        toggle = tree.GetVisualDescendants().OfType<ToggleButton>().First();
        PointerInput.Click(toggle);
        Assert.False(root.IsExpanded);
    }

    [AvaloniaFact]
    public void Rating_Can_Increase_And_Decrease_To_The_Exact_Half_Star()
    {
        var rating = new MdRating { ItemSize = 32, Spacing = 4, Precision = .5, Value = 1 };
        const double targetPitch = 48 + 4;
        rating.SetValueFromPosition(3 * targetPitch + 48);
        Assert.Equal(4, rating.Value);
        rating.SetValueFromPosition(targetPitch + 24);
        Assert.Equal(1.5, rating.Value);
    }

    [AvaloniaFact]
    public void Language_Switch_Recreates_The_Active_Page_For_Closed_Popups_And_Virtualized_Data()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;
        var window = new MainWindow();
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var selector = window.GetVisualDescendants().OfType<MdComboBox>().Single(control => control.Name == "LanguageSelector");
            var host = window.GetVisualDescendants().OfType<ContentControl>().Single(control => control.Name == "PageHost");
            var englishPage = host.Content;
            selector.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("zh", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
            Assert.NotSame(englishPage, host.Content);
            Assert.Contains("按钮", window.GetVisualDescendants().OfType<TextBlock>().Select(text => text.Text));
        }
        finally
        {
            window.Close();
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    [AvaloniaFact]
    public void Breadcrumb_Supports_Flutter_Style_Scroll_Overflow_Without_Persistent_Double_Selection()
    {
        var breadcrumb = new MdBreadcrumb
        {
            Width = 240,
            OverflowBehavior = MdBreadcrumbOverflowBehavior.Scroll,
            ItemsSource = new[] { "Home", "Library", "Components", "Carousel" }
        };
        object? invoked = null;
        breadcrumb.ItemInvoked += (_, item) => invoked = item;

        using var host = Show(breadcrumb, 280, 100);
        Dispatcher.UIThread.RunJobs();
        var scroll = breadcrumb.GetVisualDescendants().OfType<ScrollViewer>()
            .Single(viewer => viewer.Name == "PART_BreadcrumbScrollViewer");
        Assert.Equal(ScrollBarVisibility.Auto, scroll.HorizontalScrollBarVisibility);
        Assert.Equal(ScrollBarVisibility.Disabled, scroll.VerticalScrollBarVisibility);

        breadcrumb.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal("Library", invoked);
        Assert.Equal(-1, breadcrumb.SelectedIndex);
        Assert.DoesNotContain(breadcrumb.GetVisualDescendants().OfType<ListBoxItem>(), item => item.IsSelected);
    }

    [AvaloniaFact]
    public void Breadcrumb_Renders_Rich_Labels_And_Separators_On_The_First_Layout()
    {
        var breadcrumb = new MdBreadcrumb
        {
            Separator = "/",
            ItemsSource = new[]
            {
                new MdBreadcrumbItem { Label = "Home" },
                new MdBreadcrumbItem { Label = "Components" },
                new MdBreadcrumbItem { Label = "Color picker", IsCurrent = true }
            }
        };

        using var host = Show(breadcrumb, 520, 100);
        Dispatcher.UIThread.RunJobs();

        var textBlocks = breadcrumb.GetVisualDescendants().OfType<TextBlock>().ToArray();
        var labels = textBlocks.Select(text => text.Text).Where(text => text is not null).ToArray();
        Assert.Contains("Home", labels);
        Assert.Contains("Components", labels);
        Assert.Contains("Color picker", labels);
        var separators = breadcrumb.GetVisualDescendants().OfType<ContentPresenter>()
            .Where(presenter => presenter.Name == "PART_Separator")
            .ToArray();
        Assert.Equal(3, separators.Length);
        Assert.Equal(2, separators.Count(presenter => presenter.IsVisible && Equals(presenter.Content, "/")));

        var containers = breadcrumb.GetVisualDescendants().OfType<ListBoxItem>().ToArray();
        Assert.Equal(3, containers.Length);
        Assert.Contains("current", containers[^1].Classes);
    }

    [AvaloniaFact]
    public void Breadcrumb_SeparatorTemplate_And_Trailing_Separator_Are_Explicit()
    {
        var breadcrumb = new MdBreadcrumb
        {
            Separator = "/",
            ShowTrailingSeparator = true,
            SeparatorTemplate = new FuncDataTemplate<string>((value, _) =>
                new TextBlock { Text = $"separator:{value}" }),
            ItemsSource = new[] { "Home", "Current" }
        };

        using var host = Show(breadcrumb, 320, 100);
        Dispatcher.UIThread.RunJobs();

        var separators = breadcrumb.GetVisualDescendants().OfType<ContentPresenter>()
            .Where(presenter => presenter.Name == "PART_Separator")
            .ToArray();
        Assert.Equal(2, separators.Length);
        Assert.All(separators, presenter => Assert.True(presenter.IsVisible));
        Assert.Equal(2, breadcrumb.GetVisualDescendants().OfType<TextBlock>()
            .Count(text => text.Text == "separator:/"));
    }

    [AvaloniaFact]
    public void ColorPicker_Swatch_Button_Uses_A_Command_And_Shows_Selection()
    {
        var picker = new MdColorPicker { Width = 400 };
        using var host = Show(picker, 460, 620);
        Dispatcher.UIThread.RunJobs();

        var red = Color.Parse("#F44336");
        var swatch = picker.GetVisualDescendants().OfType<Button>()
            .First(button => button.CommandParameter is Color color && color == red);
        Assert.Same(picker.SelectColorCommand, swatch.Command);

        swatch.Command!.Execute(swatch.CommandParameter);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(red, picker.SelectedColor);
        Assert.Equal("#F44336", picker.SelectedHex);
        var selectionRing = swatch.GetVisualDescendants().OfType<Border>()
            .Single(border => border.Name == "PART_SelectionRing");
        Assert.True(selectionRing.IsVisible);
    }

    [AvaloniaFact]
    public void ColorPicker_Uses_Material_Surface_And_Connected_Mode_Controls()
    {
        var picker = new MdColorPicker { Width = 400 };
        using var host = Show(picker, 460, 700);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(new CornerRadius(28), picker.CornerRadius);
        Assert.Equal(new Thickness(0), picker.BorderThickness);

        var group = picker.GetVisualDescendants().OfType<MdConnectedButtonGroup>().Single();
        var modeButtons = group.GetVisualDescendants().OfType<MdButton>().ToArray();
        Assert.Equal(3, modeButtons.Length);
        Assert.Equal(MdButtonVariant.Tonal, modeButtons[0].Variant);

        picker.PickerMode = MdColorPickerMode.SpectrumSliders;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(MdButtonVariant.Text, modeButtons[0].Variant);
        Assert.Equal(MdButtonVariant.Tonal, modeButtons[1].Variant);
    }

    [AvaloniaFact]
    public void ColorPicker_Uses_Internal_Viewport_When_Dialog_Sized_Content_Is_Constrained()
    {
        var picker = new MdColorPicker { Width = 400, Height = 320 };
        using var host = Show(picker, 460, 360);
        Dispatcher.UIThread.RunJobs();

        var scrollViewer = picker.GetVisualDescendants().OfType<ScrollViewer>()
            .Single(viewer => viewer.Name == "PART_ContentScrollViewer");
        Assert.Equal(ScrollBarVisibility.Auto, scrollViewer.VerticalScrollBarVisibility);
        Assert.Equal(ScrollBarVisibility.Disabled, scrollViewer.HorizontalScrollBarVisibility);
        Assert.NotNull(scrollViewer.Content);
        Assert.Equal(640, picker.MaxHeight);
    }

    [AvaloniaFact]
    public void ColorPicker_Panel_Visibility_Properties_Select_An_Available_Mode()
    {
        var picker = new MdColorPicker
        {
            Width = 400,
            IsPreviewPanelVisible = false,
            IsModeSelectorVisible = false,
            IsMaterialPalettePanelVisible = false,
            IsSpectrumPanelVisible = true,
            IsRecentColorsPanelVisible = true
        };
        using var host = Show(picker, 460, 500);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(MdColorPickerMode.SpectrumSliders, picker.PickerMode);
        Assert.False(picker.GetVisualDescendants().OfType<Border>()
            .Single(control => control.Name == "PART_PreviewPanel").IsVisible);
        Assert.False(picker.GetVisualDescendants().OfType<MdConnectedButtonGroup>()
            .Single(control => control.Name == "PART_ModeSelector").IsVisible);
        Assert.False(picker.GetVisualDescendants().OfType<StackPanel>()
            .Single(control => control.Name == "PART_PaletteSection").IsVisible);
        Assert.True(picker.GetVisualDescendants().OfType<StackPanel>()
            .Single(control => control.Name == "PART_SpectrumSection").IsVisible);
        Assert.False(picker.GetVisualDescendants().OfType<StackPanel>()
            .Single(control => control.Name == "PART_PresetsSection").IsVisible);

        picker.IsModeSelectorVisible = true;
        Dispatcher.UIThread.RunJobs();
        Assert.True(picker.GetVisualDescendants().OfType<MdConnectedButtonGroup>()
            .Single(control => control.Name == "PART_ModeSelector").IsVisible);

        picker.IsRecentColorsPanelVisible = false;
        Dispatcher.UIThread.RunJobs();
        Assert.False(picker.GetVisualDescendants().OfType<MdConnectedButtonGroup>()
            .Single(control => control.Name == "PART_ModeSelector").IsVisible);
    }

    [AvaloniaFact]
    public void ColorPickerButton_Tracks_Popup_And_Hex_State()
    {
        // Keep popup state testing detached because the headless platform intentionally
        // has neither a native popup implementation nor an overlay host.
        var button = new MdColorPickerButton { SelectedColor = Color.Parse("#006A6A") };

        Assert.Equal("#006A6A", button.SelectedHex);
        button.IsDropDownOpen = true;
        Assert.True(button.IsDropDownOpen);
        button.IsDropDownOpen = false;
        Assert.False(button.IsDropDownOpen);
    }

    [AvaloniaFact]
    public void Borderless_Window_Computes_FAStyle_Template_Settings_And_Preserves_Native_Frame()
    {
        var adapter = new CapturingAdapter();
        var window = new MdBorderlessWindow { PlatformAdapter = adapter, TitleBarHeight = 44, PreserveNativeBorder = true };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(adapter.Options);
            Assert.True(adapter.Options!.PreserveNativeBorder);
            Assert.Equal(44, window.TemplateSettings.TitleBarHeight);
            Assert.Equal(44, window.TemplateSettings.ContentMargin.Top);
            Assert.True(window.TemplateSettings.IsClientAreaExtended);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Simple_Dialog_Selecting_An_Item_Does_Not_Also_Report_A_Dismissal()
    {
        // Reported from the gallery: clicking an account answered "selection cancelled". Choosing
        // an item closed the dialog through Dismiss(), so ItemSelected and Dismissed both fired
        // for one click and the cancel listener, running second, overwrote the selection.
        var dialog = new MdSimpleDialog
        {
            ItemsSource = new[] { "material@example.com", "avalonia@example.com" },
            IsOpen = true
        };

        object? selected = null;
        var dismissals = 0;
        dialog.ItemSelected += (_, item) => selected = item;
        dialog.Dismissed += (_, _) => dismissals++;

        using var host = Show(dialog, 600, 400);
        Dispatcher.UIThread.RunJobs();

        var items = dialog.GetVisualDescendants().OfType<SelectingItemsControl>().First(c => c.Name == "PART_ItemsHost");
        items.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("avalonia@example.com", selected);
        Assert.Equal(0, dismissals);
        Assert.False(dialog.IsOpen);

        // Cancelling still reports exactly one dismissal.
        dialog.Show();
        dialog.Dismiss();
        Assert.Equal(1, dismissals);
    }

    [AvaloniaFact]
    public void Simple_Dialog_Options_Run_Edge_To_Edge_With_M3_Spacing()
    {
        // The other half of the same report: the options were inset by the surface's own 24 of
        // padding, so a row's hover and selection layers stopped short of the dialog edge and the
        // title/option rhythm was a flat 12 everywhere. M3 composes it the way Flutter's
        // SimpleDialog does - title padded 24/24/24/0, content 0/12/0/16, option 24/8 - which puts
        // 20 between the title and the first option and 24 under the last, with the option's own
        // padding supplying the side inset so its state layer spans the full width.
        var dialog = new MdSimpleDialog
        {
            Title = "Set backup account",
            ItemsSource = new[] { "material@example.com", "avalonia@example.com" },
            IsOpen = true
        };
        using var host = Show(dialog, 700, 520);
        Dispatcher.UIThread.RunJobs();

        var surface = dialog.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "PART_Surface");
        var items = dialog.GetVisualDescendants().OfType<SelectingItemsControl>().Single(c => c.Name == "PART_ItemsHost");
        var option = items.GetVisualDescendants().OfType<ListBoxItem>().First();

        Assert.Equal(new Thickness(24, 8), option.Padding);
        Assert.Equal(new CornerRadius(0), option.CornerRadius);

        // Full bleed: the row starts at the surface's edge and is as wide as it is.
        var offset = option.TranslatePoint(default, surface);
        Assert.NotNull(offset);
        Assert.Equal(0d, offset!.Value.X, 1);
        Assert.Equal(surface.Bounds.Width, option.Bounds.Width, 1);

        // And the title collapses when there is none instead of leaving its 24/24 behind.
        var untitled = new MdSimpleDialog { ItemsSource = new[] { "One" }, IsOpen = true };
        using var untitledHost = Show(untitled, 700, 520);
        Dispatcher.UIThread.RunJobs();
        var title = untitled.GetVisualDescendants().OfType<ContentPresenter>().Single(p => p.Name == "PART_Title");
        Assert.False(title.IsVisible);
    }

    [AvaloniaFact]
    public void Expanded_Search_Reopens_When_The_Header_Is_Typed_In_Again()
    {
        // Reported from the gallery: choose a result, clear the field, type again, and the
        // suggestions never came back. Committing a result writes the chosen text into the header
        // and closes the view, and nothing reopened it, so the filter ran against a hidden list.
        // The header presenter stays visible while the view is closed, so typing in it must bring
        // the results back.
        var header = new MdSearchBar();
        var view = new MdSearchView
        {
            Header = header,
            Content = new TextBlock { Text = "results" },
            IsOpen = true
        };

        using var host = Show(view, 800, 600);
        Dispatcher.UIThread.RunJobs();

        view.CommitResult("MdSearchView.cs", "MdSearchView.cs");
        Dispatcher.UIThread.RunJobs();
        Assert.False(view.IsOpen, "committing a result closes the view and must not reopen it");

        header.Focus();
        Dispatcher.UIThread.RunJobs();
        Assert.True(header.IsKeyboardFocusWithin, "the header stays visible while the view is closed, so it must be focusable");

        // Clearing the field on its own leaves the view closed.
        header.Text = string.Empty;
        Dispatcher.UIThread.RunJobs();
        Assert.False(view.IsOpen, "clearing the field alone must not expand the view");

        // Typing brings the result list back.
        header.Text = "search";
        Dispatcher.UIThread.RunJobs();
        Assert.True(view.IsOpen);
    }

    private static Scope Show(Control content, double width, double height)
    {
        var window = new Window { Width = width, Height = height, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new Scope(window);
    }

    private sealed class Scope(Window window) : IDisposable
    {
        public Window Window { get; } = window;
        public void Dispose() => Window.Close();
    }

    private sealed class CapturingAdapter : IMdWindowPlatformAdapter
    {
        public MdWindowPlatform Platform => MdWindowPlatform.Windows;
        public MdWindowCapabilities Capabilities => MdWindowCapabilities.All;
        public MdBorderlessWindowOptions? Options { get; private set; }
        public void Apply(MdBorderlessWindow window, MdBorderlessWindowOptions options) => Options = options;
        public bool TryBeginMove(MdBorderlessWindow window, PointerPressedEventArgs args) => true;
        public bool TryBeginResize(MdBorderlessWindow window, WindowEdge edge, PointerPressedEventArgs args) => true;
        public bool TryShowSystemMenu(MdBorderlessWindow window, Point clientPoint) => false;
    }
}
