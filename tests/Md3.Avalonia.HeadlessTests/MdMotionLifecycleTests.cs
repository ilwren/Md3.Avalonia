using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Extra.Infrastructure;
using Md3.Avalonia.Motion;
using Md3.Avalonia.Themes.Dynamic;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdMotionLifecycleTests
{
    [Fact]
    public void Motion_Resolver_Distinguishes_Physics_Reduced_And_None()
    {
        var expressive = MdMotion.Resolve(MdMotionScheme.Expressive, MdMotionKind.Spatial, MdMotionSpeed.Default);
        var standard = MdMotion.Resolve(MdMotionScheme.Standard, MdMotionKind.Spatial, MdMotionSpeed.Default);
        var reducedSpatial = MdMotion.Resolve(MdMotionScheme.Reduced, MdMotionKind.Spatial, MdMotionSpeed.Default);
        var reducedEffects = MdMotion.Resolve(MdMotionScheme.Reduced, MdMotionKind.Effects, MdMotionSpeed.Default);
        var none = MdMotion.Resolve(MdMotionScheme.None, MdMotionKind.Effects, MdMotionSpeed.Default);

        Assert.True(expressive.IsEnabled);
        Assert.True(standard.IsEnabled);
        Assert.NotEqual(expressive.Spring, standard.Spring);
        Assert.NotEqual(expressive.Duration, standard.Duration);
        Assert.False(reducedSpatial.IsEnabled);
        Assert.True(reducedEffects.IsEnabled);
        Assert.Equal(TimeSpan.FromMilliseconds(100), reducedEffects.Duration);
        Assert.False(none.IsEnabled);
        Assert.Equal(TimeSpan.Zero, none.Duration);

        var reducedEasing = reducedEffects.CreateEasing();
        Assert.InRange(reducedEasing.Ease(0.5), 0, 1);
        Assert.Equal(1, reducedEasing.Ease(1));
    }

    [AvaloniaFact]
    public void Theme_Manager_Propagates_Motion_Scheme_From_Each_TopLevel()
    {
        var application = Assert.IsAssignableFrom<Application>(Application.Current);
        var window = new Window { Content = new MdButton { Content = "Motion" } };
        try
        {
            MdThemeManager.Apply(application, new MdThemeOptions { MotionScheme = MdMotionScheme.Reduced }, dark: false);
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var button = Assert.IsType<MdButton>(window.Content);
            Assert.Equal(MdMotionScheme.Reduced, MdMotion.GetScheme(window));
            Assert.Equal(MdMotionScheme.Reduced, MdMotion.GetScheme(button));
        }
        finally
        {
            window.Close();
            MdThemeManager.Apply(application, new MdThemeOptions(), dark: false);
        }
    }

    [AvaloniaFact]
    public void Dialog_Keeps_Overlay_Present_Until_Exit_Completes()
    {
        var host = new MdDialogHost { Dialog = new MdDialog { Headline = "Confirm", Content = "Proceed?" } };
        using var scope = Show(host);
        host.IsOpen = true;
        Dispatcher.UIThread.RunJobs();
        var overlay = host.GetVisualDescendants().OfType<Grid>().Single(control => control.Name == "PART_Overlay");
        var presenter = host.GetVisualDescendants().OfType<ContentPresenter>().Single(control => control.Name == "PART_DialogPresenter");
        Assert.True(overlay.IsVisible);

        host.Close();
        Dispatcher.UIThread.RunJobs();
        Assert.True(overlay.IsVisible);
        var overlayFade = Assert.IsType<DoubleTransition>(Assert.Single(overlay.Transitions!));
        Assert.IsType<MdSpringEasing>(overlayFade.Easing);

        MdMotion.SetScheme(host, MdMotionScheme.Reduced);
        Assert.NotNull(presenter.Transitions);
        Assert.Single(presenter.Transitions!);
        Assert.IsType<DoubleTransition>(presenter.Transitions![0]);

        MdMotion.SetScheme(host, MdMotionScheme.None);
        Dispatcher.UIThread.RunJobs();
        Assert.False(overlay.IsVisible);
        Assert.Null(presenter.Transitions);
    }

    [AvaloniaFact]
    public void Sheet_Uses_Full_Extent_And_Reduced_Motion_Removes_Spatial_Travel()
    {
        var sheet = new MdSheetHost
        {
            Height = 500,
            SheetExtent = 260,
            SheetContent = new TextBlock { Text = "Sheet" },
            Content = new Border()
        };
        using var scope = Show(sheet, 700, 520);
        sheet.Show();
        Dispatcher.UIThread.RunJobs();
        var layer = sheet.GetVisualDescendants().OfType<Grid>().Single(control => control.Name == "PART_Layer");
        var surface = sheet.GetVisualDescendants().OfType<Border>().Single(control => control.Name == "PART_Surface");
        var transform = Assert.IsType<TranslateTransform>(surface.RenderTransform);

        sheet.Dismiss();
        Dispatcher.UIThread.RunJobs();
        Assert.True(layer.IsVisible);
        // Headless may not advance the first transition frame deterministically; verify the
        // configured spatial transition rather than asserting one particular sampled value.
        Assert.NotEmpty(transform.Transitions!);
        Assert.All(transform.Transitions!, transition => Assert.IsType<DoubleTransition>(transition));

        sheet.Show();
        MdMotion.SetScheme(sheet, MdMotionScheme.Reduced);
        sheet.Dismiss();
        Dispatcher.UIThread.RunJobs();
        Assert.True(layer.IsVisible);
        Assert.Equal(0, transform.Y);
        Assert.Null(transform.Transitions);

        MdMotion.SetScheme(sheet, MdMotionScheme.None);
        Dispatcher.UIThread.RunJobs();
        Assert.False(layer.IsVisible);
    }

    [AvaloniaFact]
    public void Drawer_Snackbar_Banner_And_Search_Retain_Closing_Content()
    {
        var drawer = new MdNavigationDrawer { IsOpen = true, DrawerContent = "Navigation", Content = new Border() };
        var snackbar = new MdSnackbar { IsOpen = true, Duration = TimeSpan.Zero, Content = "Saved" };
        var banner = new MdBanner { IsOpen = true, Content = "Offline" };
        var search = new MdSearchView { IsOpen = true, Header = new MdSearchBar(), Content = new TextBlock { Text = "Results" } };
        var panel = new StackPanel { Children = { drawer, snackbar, banner, search } };
        using var scope = Show(panel, 900, 900);
        Dispatcher.UIThread.RunJobs();

        var drawerSurface = drawer.GetVisualDescendants().OfType<Border>().Single(control => control.Name == "PART_Drawer");
        var results = search.GetVisualDescendants().OfType<ContentPresenter>().Single(control => control.Name == "PART_Results");
        drawer.Dismiss();
        snackbar.Dismiss();
        banner.Dismiss();
        search.Dismiss();
        Dispatcher.UIThread.RunJobs();

        Assert.True(drawerSurface.IsVisible);
        Assert.True(snackbar.IsVisible);
        Assert.True(banner.IsVisible);
        Assert.True(results.IsVisible);

        MdMotion.SetScheme(drawer, MdMotionScheme.None);
        MdMotion.SetScheme(snackbar, MdMotionScheme.None);
        MdMotion.SetScheme(banner, MdMotionScheme.None);
        MdMotion.SetScheme(search, MdMotionScheme.None);
        Dispatcher.UIThread.RunJobs();

        Assert.False(drawerSurface.IsVisible);
        Assert.False(snackbar.IsVisible);
        Assert.False(banner.IsVisible);
        Assert.False(results.IsVisible);
    }

    [Fact]
    public void Popup_Owners_Separate_Desired_Open_State_From_Visual_Presence()
    {
        var menu = new MdMenuAnchor();
        menu.Show();
        Assert.True(menu.IsOpen);
        Assert.True(menu.IsPopupOpen);
        menu.Dismiss();
        Assert.False(menu.IsOpen);
        Assert.True(menu.IsPopupOpen);
        MdMotion.SetScheme(menu, MdMotionScheme.None);
        Assert.False(menu.IsPopupOpen);

        var tooltip = new MdTooltipHost { Tooltip = new MdTooltip { Content = "Help" } };
        tooltip.Show();
        Assert.True(tooltip.IsOpen);
        Assert.True(tooltip.IsPopupOpen);
        tooltip.Dismiss();
        Assert.False(tooltip.IsOpen);
        Assert.True(tooltip.IsPopupOpen);
        MdMotion.SetScheme(tooltip, MdMotionScheme.None);
        Assert.False(tooltip.IsPopupOpen);
    }

    [Fact]
    public void Combo_And_AutoComplete_Presence_Reverses_And_Replacement_Unmounts_Old_Host()
    {
        var combo = new MdComboBox { ItemsSource = new[] { "One", "Two" } };
        combo.IsDropDownOpen = true;
        Assert.True(combo.IsPopupOpen);
        combo.IsDropDownOpen = false;
        Assert.False(combo.IsDropDownOpen);
        Assert.True(combo.IsPopupOpen);

        // A quick reopen cancels the pending exit instead of flickering the host away.
        combo.IsDropDownOpen = true;
        Assert.True(combo.IsPopupOpen);

        var autocomplete = new MdAutoCompleteBox
        {
            ItemsSource = new[] { "One", "Two" },
            MinimumPrefixLength = 0,
            Text = "O"
        };
        autocomplete.PopulateComplete();
        autocomplete.IsDropDownOpen = true;
        Assert.False(combo.IsDropDownOpen);
        Assert.False(combo.IsPopupOpen);
        Assert.True(autocomplete.IsDropDownOpen);
        Assert.True(autocomplete.IsPopupOpen);

        autocomplete.IsDropDownOpen = false;
        Assert.True(autocomplete.IsPopupOpen);
        MdMotion.SetScheme(autocomplete, MdMotionScheme.None);
        Assert.False(autocomplete.IsPopupOpen);
    }

    [Fact]
    public void Dropdown_Menu_Closes_Native_Host_In_The_Same_Turn()
    {
        var menu = new MdDropdownMenu { Content = new MdMenu() };
        menu.IsOpen = true;
        Assert.True(menu.IsPopupOpen);
        menu.IsOpen = false;
        Assert.False(menu.IsPopupOpen);
    }

    [AvaloniaFact]
    public void Navigation_And_Tabs_Use_Scheme_Aware_Indicator_Motion()
    {
        var first = new MdNavigationBarItem { Label = "Home", Icon = "home", SelectedIcon = "home-filled" };
        var second = new MdNavigationBarItem { Label = "Search", Icon = "search", SelectedIcon = "search-filled" };
        var navigation = new MdNavigationBar { Items = { first, second }, SelectedIndex = 0 };
        var tab1 = new MdTabItem { Content = "Overview" };
        var tab2 = new MdTabItem { Content = "Details" };
        var tabs = new MdTabs { Width = 500, Items = { tab1, tab2 }, SelectedIndex = 0 };
        using var scope = Show(new StackPanel { Children = { navigation, tabs } });
        Dispatcher.UIThread.RunJobs();

        var navIndicator = first.GetVisualDescendants().OfType<Border>()
            .Single(control => control.Name == "PART_Indicator");
        Assert.Equal(3, navIndicator.Transitions!.Count);
        Assert.True(tab1.UseSharedIndicator);
        var sharedIndicator = tabs.GetVisualDescendants().OfType<Border>()
            .Single(control => control.Name == "PART_SelectionIndicator");
        Assert.True(sharedIndicator.IsVisible);
        Assert.Equal(2, sharedIndicator.Transitions!.Count);
        Assert.Single(Assert.IsType<TranslateTransform>(sharedIndicator.RenderTransform).Transitions!);

        tabs.SelectedIndex = 1;
        navigation.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();
        Assert.True(sharedIndicator.IsVisible);

        MdMotion.SetScheme(first, MdMotionScheme.Reduced);
        MdMotion.SetScheme(tabs, MdMotionScheme.Reduced);
        Assert.Equal(2, navIndicator.Transitions!.Count);
        Assert.Null(Assert.IsType<TranslateTransform>(sharedIndicator.RenderTransform).Transitions);
        Assert.Single(sharedIndicator.Transitions!);

        MdMotion.SetScheme(first, MdMotionScheme.None);
        MdMotion.SetScheme(tabs, MdMotionScheme.None);
        Assert.Null(navIndicator.Transitions);
        Assert.Null(sharedIndicator.Transitions);
    }

    [AvaloniaFact]
    public void Expansion_And_Step_Content_Stay_Mounted_During_Exit()
    {
        var panel = new MdExpansionPanel { Header = "Panel", Content = "Body", IsExpanded = true };
        var firstStep = new MdStep { Header = "First", Content = "First body" };
        var secondStep = new MdStep { Header = "Second", Content = "Second body" };
        var stepper = new MdStepper { Items = { firstStep, secondStep }, ActiveStep = 0 };
        using var scope = Show(new StackPanel { Children = { panel, stepper } }, 760, 700);
        Dispatcher.UIThread.RunJobs();

        var panelContent = panel.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(control => control.Name == "PART_ExpandedContent");
        var stepContent = firstStep.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(control => control.Name == "PART_StepContent");
        panel.Collapse();
        stepper.Next();
        Dispatcher.UIThread.RunJobs();
        Assert.True(panelContent.IsVisible);
        Assert.True(stepContent.IsVisible);
        Assert.False(panelContent.IsHitTestVisible);
        Assert.False(stepContent.IsHitTestVisible);

        MdMotion.SetScheme(panel, MdMotionScheme.None);
        MdMotion.SetScheme(firstStep, MdMotionScheme.None);
        Dispatcher.UIThread.RunJobs();
        Assert.False(panelContent.IsVisible);
        Assert.False(stepContent.IsVisible);
    }

    [Fact]
    public void Ecosystem_Popover_Retains_Exit_And_Immediately_Replaces_Old_Host()
    {
        var first = new MdPopover { Anchor = "First", PopoverContent = "One" };
        var second = new MdPopover { Anchor = "Second", PopoverContent = "Two" };
        first.Show();
        Assert.True(first.IsPopupOpen);
        first.Dismiss();
        Assert.True(first.IsPopupOpen);
        first.Show();

        second.Show();
        Assert.False(first.IsOpen);
        Assert.False(first.IsPopupOpen);
        Assert.True(second.IsOpen);
        Assert.True(second.IsPopupOpen);

        second.Dismiss();
        MdMotion.SetScheme(second, MdMotionScheme.None);
        Assert.False(second.IsPopupOpen);
    }

    [AvaloniaFact]
    public void Simple_Dialog_Keeps_Surface_Present_Through_Exit()
    {
        var dialog = new MdSimpleDialog { Title = "Choose", ItemsSource = new[] { "One", "Two" }, IsOpen = true };
        using var scope = Show(dialog);
        var surface = dialog.GetVisualDescendants().OfType<Border>()
            .Single(control => control.Name == "PART_Surface");
        dialog.Dismiss();
        Dispatcher.UIThread.RunJobs();
        Assert.True(surface.IsVisible);
        Assert.False(surface.IsHitTestVisible,
            "The exiting dialog surface must remain visible for motion but stop intercepting input immediately.");
        Assert.Equal(2, surface.Transitions!.Count);

        MdMotion.SetScheme(dialog, MdMotionScheme.Reduced);
        Assert.Single(surface.Transitions!);
        MdMotion.SetScheme(dialog, MdMotionScheme.None);
        Dispatcher.UIThread.RunJobs();
        Assert.False(surface.IsVisible,
            "No-motion must remove the retained dialog surface without waiting for an exit timer.");
        Assert.Null(surface.Transitions);
    }

    [AvaloniaFact]
    public void Command_Palette_Backdrop_And_Surface_Have_Exit_Presence()
    {
        var palette = new MdCommandPalette();
        using var scope = Show(palette);
        var backdrop = palette.GetVisualDescendants().OfType<Grid>()
            .Single(control => control.Name == "PART_Backdrop");
        var surface = palette.GetVisualDescendants().OfType<Border>()
            .Single(control => control.Name == "PART_Surface");

        palette.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.True(backdrop.IsVisible);
        Assert.Equal(2, surface.Transitions!.Count);
        palette.Dismiss();
        Dispatcher.UIThread.RunJobs();
        Assert.True(backdrop.IsVisible);
        Assert.False(backdrop.IsHitTestVisible);

        MdMotion.SetScheme(palette, MdMotionScheme.Reduced);
        Assert.Single(surface.Transitions!);
        MdMotion.SetScheme(palette, MdMotionScheme.None);
        Dispatcher.UIThread.RunJobs();
        Assert.False(backdrop.IsVisible);
        Assert.Null(surface.Transitions);
    }

    [AvaloniaFact]
    public void Carousel_Uses_Large_Medium_Small_Morph_And_Disables_Spatial_Motion_When_Reduced()
    {
        var carousel = new MdCarousel
        {
            Width = 720,
            Height = 220,
            ItemWidth = 240,
            ItemHeight = 200,
            Variant = MdCarouselVariant.MultiBrowse,
            ItemsSource = new[] { "One", "Two", "Three", "Four", "Five" },
            SelectedIndex = 2
        };
        MdMotion.SetScheme(carousel, MdMotionScheme.None);
        using var scope = Show(carousel, 760, 280);
        Dispatcher.UIThread.RunJobs();

        var items = carousel.GetVisualDescendants().OfType<ListBoxItem>().ToArray();
        Assert.True(items.Length >= 3);
        Assert.Contains(items, item => Math.Abs(item.Width - 240) < 0.01);
        Assert.Contains(items, item => Math.Abs(item.Width - 172.8) < 0.01);
        Assert.Contains(items, item => Math.Abs(item.Width - 115.2) < 0.01);
        Assert.All(items, item => Assert.Null(item.Transitions));

        MdMotion.SetScheme(carousel, MdMotionScheme.Standard);
        carousel.SelectedIndex = 3;
        Dispatcher.UIThread.RunJobs();
        Assert.All(items, item => Assert.Single(item.Transitions!));
        var scroll = carousel.GetVisualDescendants().OfType<ScrollViewer>().First();
        Assert.Single(scroll.Transitions!);

        MdMotion.SetScheme(carousel, MdMotionScheme.Reduced);
        Assert.All(items, item => Assert.Null(item.Transitions));
        Assert.Null(scroll.Transitions);
    }

    [AvaloniaFact]
    public void Refresh_Slider_And_RangeSlider_Keep_Direct_Manipulation_One_To_One()
    {
        var refresh = new MdRefreshIndicator
        {
            Width = 420,
            Height = 220,
            TriggerDistance = 200,
            Content = new Border { Background = Brushes.Transparent }
        };
        var slider = new MdSlider { Width = 420, Value = 25, ShowValueIndicator = true };
        var range = new MdRangeSlider { Width = 420, LowerValue = 25, UpperValue = 75, ShowValueIndicators = true };
        var stack = new StackPanel { Children = { refresh, slider, range } };
        using var scope = Show(stack, 520, 520);
        Dispatcher.UIThread.RunJobs();

        var refreshHost = refresh.GetVisualDescendants().OfType<Border>()
            .Single(control => control.Name == "PART_IndicatorHost");
        var refreshSurface = refresh.GetVisualDescendants().OfType<Border>()
            .Single(control => control.Name == "PART_IndicatorSurface");
        var refreshPoint = refresh.TranslatePoint(new Point(210, 80), scope.Window)!.Value;
        scope.Window.MouseMove(refreshPoint, RawInputModifiers.None);
        scope.Window.MouseDown(refreshPoint, MouseButton.Left, RawInputModifiers.None);
        scope.Window.MouseMove(refreshPoint.WithY(refreshPoint.Y + 64), RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();
        Assert.True(refresh.PullOffset > 0);
        Assert.Null(refreshHost.Transitions);
        scope.Window.MouseUp(refreshPoint.WithY(refreshPoint.Y + 64), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.NotNull(refreshHost.Transitions);

        var thumb = slider.GetVisualDescendants().OfType<MdSliderThumb>().Single();
        var thumbPoint = thumb.TranslatePoint(new Point(2, 22), scope.Window)!.Value;
        scope.Window.MouseMove(thumbPoint, RawInputModifiers.None);
        scope.Window.MouseDown(thumbPoint, MouseButton.Left, RawInputModifiers.None);
        scope.Window.MouseMove(thumbPoint.WithX(thumbPoint.X + 80), RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();
        Assert.Null(slider.Transitions);
        Assert.Equal(slider.Value, slider.VisualValue, 6);
        scope.Window.MouseUp(thumbPoint.WithX(thumbPoint.X + 80), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.Single(slider.Transitions!);

        var lowerPoint = range.TranslatePoint(new Point(24 + (range.Bounds.Width - 48) * .25, range.Bounds.Height - 24), scope.Window)!.Value;
        scope.Window.MouseMove(lowerPoint, RawInputModifiers.None);
        scope.Window.MouseDown(lowerPoint, MouseButton.Left, RawInputModifiers.None);
        scope.Window.MouseMove(lowerPoint.WithX(lowerPoint.X + 40), RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();
        Assert.Null(range.Transitions);
        Assert.Equal(range.LowerValue, range.VisualLowerValue, 6);
        scope.Window.MouseUp(lowerPoint.WithX(lowerPoint.X + 40), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(4, range.Transitions!.Count);

        MdMotion.SetScheme(refresh, MdMotionScheme.Reduced);
        MdMotion.SetScheme(slider, MdMotionScheme.Reduced);
        MdMotion.SetScheme(range, MdMotionScheme.Reduced);
        Assert.Null(refreshHost.Transitions);
        Assert.Single(refreshSurface.Transitions!);
        Assert.Null(slider.Transitions);
        Assert.Equal(2, range.Transitions!.Count);

        MdMotion.SetScheme(refresh, MdMotionScheme.None);
        MdMotion.SetScheme(range, MdMotionScheme.None);
        Assert.Null(refreshSurface.Transitions);
        Assert.Null(range.Transitions);
    }

    [AvaloniaFact]
    public void Dismissible_And_Slidable_Use_Release_Settle_And_Reduced_Semantics()
    {
        var dismissible = new MdDismissible { Width = 360, Height = 72, Content = "Dismiss" };
        var slidable = new MdSlidableItem
        {
            Width = 360,
            Height = 72,
            Content = "Slide",
            StartActions = "Archive",
            EndActions = "Delete"
        };
        using var scope = Show(new StackPanel { Children = { dismissible, slidable } });
        Dispatcher.UIThread.RunJobs();

        var dismissRoot = dismissible.GetVisualDescendants().OfType<Border>()
            .Single(control => control.Name == "PART_Root");
        var dismissForeground = dismissible.GetVisualDescendants().OfType<Border>()
            .Single(control => control.Name == "PART_Foreground");
        var dismissTransform = Assert.IsType<TranslateTransform>(dismissForeground.RenderTransform);
        Assert.True(dismissible.Dismiss());
        Assert.True(dismissRoot.IsVisible);
        Assert.Single(dismissTransform.Transitions!);
        Assert.Single(dismissRoot.Transitions!);
        MdMotion.SetScheme(dismissible, MdMotionScheme.None);
        Dispatcher.UIThread.RunJobs();
        Assert.False(dismissRoot.IsVisible);
        Assert.Equal(0, dismissible.VisualHeight);

        var slideForeground = slidable.GetVisualDescendants().OfType<Border>()
            .Single(control => control.Name == "PART_Foreground");
        var slideTransform = Assert.IsType<TranslateTransform>(slideForeground.RenderTransform);
        var startAction = slidable.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(control => control.Name == "PART_StartActions");
        slidable.OpenStart();
        Assert.Single(slideTransform.Transitions!);
        Assert.Equal(2, startAction.Transitions!.Count);
        MdMotion.SetScheme(slidable, MdMotionScheme.Reduced);
        Assert.Null(slideTransform.Transitions);
        Assert.Single(startAction.Transitions!);
        MdMotion.SetScheme(slidable, MdMotionScheme.None);
        Assert.Null(startAction.Transitions);
    }

    [AvaloniaFact]
    public async Task Dismissible_Awaits_Async_Confirmation_And_Settles_When_Rejected()
    {
        var confirmation = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var dismissible = new MdDismissible
        {
            Width = 320,
            Height = 64,
            Content = "Archive",
            ConfirmDismissAsync = (_, _) => confirmation.Task
        };
        using var scope = Show(dismissible, 400, 160);

        var rejected = dismissible.DismissAsync();
        Assert.True(dismissible.IsConfirmationPending);
        Assert.Contains(":confirming", dismissible.Classes);
        Assert.False(dismissible.IsDismissed);
        confirmation.SetResult(false);
        Assert.False(await rejected);
        Assert.False(dismissible.IsConfirmationPending);
        Assert.False(dismissible.IsDismissed);
        Assert.Equal(0, dismissible.DragOffset);

        dismissible.ConfirmDismissAsync = (_, _) => Task.FromResult(true);
        Assert.True(await dismissible.DismissAsync(MdDismissDirection.EndToStart));
        Assert.True(dismissible.IsDismissed);
        Assert.True(dismissible.DragOffset < 0);
    }

    [Fact]
    public void Date_And_Time_Picker_Popups_Retain_Exit_And_Replace_Each_Other()
    {
        var date = new MdDatePicker();
        date.IsOpen = true;
        Assert.True(date.IsPopupOpen);
        date.IsOpen = false;
        Assert.True(date.IsPopupOpen);
        date.IsOpen = true;

        var time = new MdTimePicker();
        time.IsOpen = true;
        Assert.False(date.IsOpen);
        Assert.False(date.IsPopupOpen);
        Assert.True(time.IsPopupOpen);
        time.IsOpen = false;
        Assert.True(time.IsPopupOpen);
        MdMotion.SetScheme(time, MdMotionScheme.None);
        Assert.False(time.IsPopupOpen);
    }

    [AvaloniaFact]
    public void Picker_Month_Mode_And_Surface_Transitions_Follow_Motion_Scheme()
    {
        var date = new DatePickerMotionProbe();
        var time = new TimePickerMotionProbe();
        using var scope = Show(new StackPanel { Children = { date, time } });
        Dispatcher.UIThread.RunJobs();

        Assert.NotNull(date.Surface);
        Assert.Equal(2, date.Surface!.Transitions!.Count);
        Assert.Single(date.DaysHost!.Transitions!);
        Assert.Single(Assert.IsType<TranslateTransform>(date.DaysHost.RenderTransform).Transitions!);
        date.DisplayDate = date.DisplayDate.AddMonths(1);
        Dispatcher.UIThread.RunJobs();
        Assert.True(date.IsMonthTransitionActive);
        Assert.Equal(42, date.OutgoingCalendarDays.Count);
        Assert.True(date.OutgoingDaysHost!.IsVisible);
        Assert.Single(date.OutgoingDaysHost.Transitions!);
        Assert.Single(Assert.IsType<TranslateTransform>(date.OutgoingDaysHost.RenderTransform).Transitions!);

        Assert.NotNull(time.Surface);
        Assert.Equal(2, time.Surface!.Transitions!.Count);
        Assert.Single(time.DialPanel!.Transitions!);
        time.Mode = MdTimePickerMode.Input;
        Dispatcher.UIThread.RunJobs();

        MdMotion.SetScheme(date, MdMotionScheme.Reduced);
        MdMotion.SetScheme(time, MdMotionScheme.Reduced);
        Assert.Single(date.Surface.Transitions!);
        Assert.Null(Assert.IsType<TranslateTransform>(date.DaysHost.RenderTransform).Transitions);
        Assert.Single(time.Surface.Transitions!);
        MdMotion.SetScheme(date, MdMotionScheme.None);
        MdMotion.SetScheme(time, MdMotionScheme.None);
        Assert.Null(date.Surface.Transitions);
        Assert.Null(time.Surface.Transitions);
    }

    [AvaloniaFact]
    public void Chip_Segmented_And_TimeDial_State_Changes_Are_Scheme_Aware()
    {
        var chip = new MdChip
        {
            Variant = MdChipVariant.Filter,
            Content = "Filter",
            LeadingIcon = "filter"
        };
        var segment = new MdSegmentedButton { Content = "Day", Icon = "day", SelectedIcon = "done" };
        var group = new MdSegmentedButtonGroup { Items = { segment }, SelectedIndex = 0 };
        var dial = new MdTimeDial { Width = 256, Height = 256, Hour = 3 };
        using var scope = Show(new StackPanel { Children = { chip, group, dial } }, 520, 460);
        Dispatcher.UIThread.RunJobs();

        var chipSelectedIcon = chip.GetVisualDescendants().OfType<MdIcon>()
            .Single(control => control.Name == "PART_SelectedIcon");
        var chipLeadingIcon = chip.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(control => control.Name == "PART_LeadingIcon");
        chip.IsChecked = true;
        Assert.Single(chipSelectedIcon.Transitions!);
        Assert.Single(chipLeadingIcon.Transitions!);

        var segmentSelectedIcon = segment.GetVisualDescendants().OfType<MdSymbolPresenter>()
            .Single(control => control.Name == "PART_SelectedIcon");
        var segmentIcon = segment.GetVisualDescendants().OfType<MdSymbolPresenter>()
            .Single(control => control.Name == "PART_Icon");
        Assert.Single(segmentSelectedIcon.Transitions!);
        Assert.Single(segmentIcon.Transitions!);
        Assert.Equal(2, dial.Transitions!.Count);

        var center = dial.TranslatePoint(new Point(dial.Bounds.Width / 2, dial.Bounds.Height / 2), scope.Window)!.Value;
        var handPoint = center.WithY(center.Y - 90);
        scope.Window.MouseMove(handPoint, RawInputModifiers.None);
        scope.Window.MouseDown(handPoint, MouseButton.Left, RawInputModifiers.None);
        scope.Window.MouseMove(handPoint.WithX(handPoint.X + 60), RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();
        Assert.Null(dial.Transitions);
        scope.Window.MouseUp(handPoint.WithX(handPoint.X + 60), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(2, dial.Transitions!.Count);

        MdMotion.SetScheme(chip, MdMotionScheme.None);
        MdMotion.SetScheme(segment, MdMotionScheme.None);
        MdMotion.SetScheme(dial, MdMotionScheme.Reduced);
        Assert.Null(chipSelectedIcon.Transitions);
        Assert.Null(segmentIcon.Transitions);
        Assert.Single(dial.Transitions!);
        MdMotion.SetScheme(dial, MdMotionScheme.None);
        Assert.Null(dial.Transitions);
    }

    [AvaloniaFact]
    public void Reorderable_List_Drag_Is_Direct_Reflows_Adjacent_Rows_And_AutoScrolls()
    {
        var source = new ObservableCollection<string>(Enumerable.Range(0, 16).Select(index => $"Row {index}"));
        var list = new MdReorderableList { Width = 360, Height = 190, ItemsSource = source };
        using var scope = Show(list, 440, 260);
        var rows = list.GetVisualDescendants().OfType<ListBoxItem>().ToArray();
        var handle = rows[0].GetVisualDescendants().OfType<Border>().Single(border => border.Name == "PART_DragHandle");
        var from = handle.TranslatePoint(new Point(handle.Bounds.Width / 2, handle.Bounds.Height / 2), scope.Window)!.Value;
        var target = rows[Math.Min(2, rows.Length - 1)];
        var to = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), scope.Window)!.Value;

        scope.Window.MouseMove(from, RawInputModifiers.None);
        scope.Window.MouseDown(from, MouseButton.Left, RawInputModifiers.None);
        scope.Window.MouseMove(to, RawInputModifiers.LeftMouseButton);

        Assert.Contains(":reordering", list.Classes);
        var draggedTransform = Assert.IsType<TranslateTransform>(rows[0].RenderTransform);
        Assert.True(Math.Abs(draggedTransform.Y) > 1);
        var reflow = rows.Skip(1).Select(row => row.RenderTransform).OfType<TranslateTransform>().First(transform => Math.Abs(transform.Y) > 1);
        Assert.NotNull(reflow.Transitions);
        Assert.Contains(reflow.Transitions!, transition => transition is DoubleTransition { Property: var property } && property == TranslateTransform.YProperty);

        var scroll = list.GetVisualDescendants().OfType<ScrollViewer>().Single();
        var edge = list.TranslatePoint(new Point(list.Bounds.Width / 2, list.Bounds.Height - 3), scope.Window)!.Value;
        scope.Window.MouseMove(edge, RawInputModifiers.LeftMouseButton);
        Assert.True(scroll.Offset.Y > 0);

        scope.Window.MouseUp(edge, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.DoesNotContain(":reordering", list.Classes);
    }

    [AvaloniaFact]
    public void Tree_Expansion_Reveals_Children_Reflows_Rows_And_Reduces_To_Effects()
    {
        var child = new MdTreeNode("child", "Child");
        var branch = new MdTreeNode("branch", "Branch", [child]);
        var sibling = new MdTreeNode("sibling", "Sibling");
        var tree = new MdTreeView { Width = 420, Height = 260, Roots = [branch, sibling] };
        using var scope = Show(tree, 500, 340);

        Assert.True(tree.Expand(branch));
        Dispatcher.UIThread.RunJobs();
        var rows = tree.GetVisualDescendants().OfType<ListBoxItem>().ToArray();
        var childRow = rows.Single(row => row.Content is MdTreeRow { Node: var node } && ReferenceEquals(node, child));
        var siblingRow = rows.Single(row => row.Content is MdTreeRow { Node: var node } && ReferenceEquals(node, sibling));
        Assert.Contains(childRow.Transitions!, transition => transition is DoubleTransition { Property: var property } && property == Visual.OpacityProperty);
        var siblingTransform = Assert.IsType<TranslateTransform>(siblingRow.RenderTransform);
        Assert.Contains(siblingTransform.Transitions!, transition => transition is DoubleTransition { Property: var property } && property == TranslateTransform.YProperty);

        tree.Collapse(branch);
        Dispatcher.UIThread.RunJobs();
        MdMotion.SetScheme(tree, MdMotionScheme.Reduced);
        tree.Expand(branch);
        Dispatcher.UIThread.RunJobs();
        childRow = tree.GetVisualDescendants().OfType<ListBoxItem>()
            .Single(row => row.Content is MdTreeRow { Node: var node } && ReferenceEquals(node, child));
        var effect = Assert.IsType<DoubleTransition>(Assert.Single(childRow.Transitions!));
        Assert.Equal(TimeSpan.FromMilliseconds(100), effect.Duration);
        Assert.Null(Assert.IsType<TranslateTransform>(childRow.RenderTransform).Transitions);
    }

    [AvaloniaFact]
    public void Transfer_Animates_Cross_Column_FLIP_And_Uses_Fade_For_Reduced_Motion()
    {
        var transfer = new MdTransfer
        {
            Width = 720,
            ItemsSource = new[] { "A", "B", "C", "D" },
            SelectedItems = new[] { "A" }
        };
        using var scope = Show(transfer, 800, 420);

        transfer.MoveToTarget(["B"]);
        Dispatcher.UIThread.RunJobs();
        var target = transfer.GetVisualDescendants().OfType<ListBox>().Single(list => list.Name == "PART_Target");
        var movedRow = target.GetVisualDescendants().OfType<ListBoxItem>().Single(row => Equals(row.Content, "B"));
        var movedTransform = Assert.IsType<TranslateTransform>(movedRow.RenderTransform);
        Assert.NotNull(movedTransform.Transitions);
        Assert.Contains(movedTransform.Transitions!, transition => transition is DoubleTransition { Property: var property } && property == TranslateTransform.XProperty);
        Assert.Contains(movedTransform.Transitions!, transition => transition is DoubleTransition { Property: var property } && property == TranslateTransform.YProperty);

        MdMotion.SetScheme(transfer, MdMotionScheme.Reduced);
        transfer.MoveToTarget(["C"]);
        Dispatcher.UIThread.RunJobs();
        movedRow = target.GetVisualDescendants().OfType<ListBoxItem>().Single(row => Equals(row.Content, "C"));
        var effect = Assert.IsType<DoubleTransition>(Assert.Single(movedRow.Transitions!));
        Assert.Equal(TimeSpan.FromMilliseconds(100), effect.Duration);
        Assert.Null(Assert.IsType<TranslateTransform>(movedRow.RenderTransform).Transitions);
    }

    [AvaloniaFact]
    public async Task Ambient_And_Sequenced_Ecosystem_Motion_Respect_Reduced_And_None()
    {
        var skeleton = new MdSkeleton { Width = 200, Height = 32, IsLoading = true };
        using var scope = Show(skeleton);
        MdMotion.SetScheme(skeleton, MdMotionScheme.Reduced);
        Assert.Equal(1, skeleton.PulseOpacity);

        var sequence = new MdAnimationSequence { AutoPlay = false };
        sequence.Children.Add(new Border());
        sequence.Children.Add(new Border());
        MdMotion.SetScheme(sequence, MdMotionScheme.Reduced);
        await sequence.PlayAsync();
        Assert.All(sequence.Children, child =>
        {
            var transition = Assert.IsType<DoubleTransition>(Assert.Single(child.Transitions!));
            Assert.Equal(TimeSpan.FromMilliseconds(100), transition.Duration);
            Assert.Equal(1, child.Opacity);
        });

        MdMotion.SetScheme(sequence, MdMotionScheme.None);
        await sequence.PlayAsync();
        Assert.All(sequence.Children, child => Assert.Null(child.Transitions));
        Assert.False(sequence.IsPlaying);
    }

    [AvaloniaFact]
    public void Formerly_Fixed_Template_Transitions_Follow_Runtime_Motion_Scheme()
    {
        var button = new MdButton { Content = "Button" };
        var checkBox = new MdCheckBox { Content = "Check" };
        var radio = new MdRadioButton { Content = "Radio" };
        var motionSwitch = new MdSwitch();
        var textBox = new MdTextBox { Label = "Name", Text = "Value" };
        var pin = new MdPinCellPresenter { Cell = new MdPinCell(0, "5", true, true, false, 48, default) };
        var content = new StackPanel { Children = { button, checkBox, radio, motionSwitch, textBox, pin } };
        using var scope = Show(content, 520, 520);

        var buttonContainer = Part<Border>(button, "PART_Container");
        var checkGlyph = Part<global::Avalonia.Controls.Shapes.Path>(checkBox, "PART_CheckGlyph");
        var radioDot = Part<Border>(radio, "PART_Dot");
        var switchTrack = Part<Border>(motionSwitch, "PART_Track");
        var switchThumbState = Part<Border>(motionSwitch, "PART_ThumbState");
        var switchThumb = Part<Border>(motionSwitch, "PART_Thumb");
        var textLabel = Part<ContentPresenter>(textBox, "PART_Label");
        var pinCell = Part<Border>(pin, "PART_Cell");

        MdMotion.SetScheme(content, MdMotionScheme.Standard);
        Assert.NotNull(buttonContainer.Transitions);
        Assert.NotNull(checkGlyph.Transitions);
        Assert.NotNull(radioDot.Transitions);
        Assert.NotNull(switchThumb.Transitions);
        Assert.NotNull(textLabel.Transitions);
        Assert.NotNull(pinCell.Transitions);

        MdMotion.SetScheme(content, MdMotionScheme.Reduced);
        Assert.Null(buttonContainer.Transitions);
        Assert.Single(checkGlyph.Transitions!);
        Assert.Single(radioDot.Transitions!);
        Assert.Equal(2, switchTrack.Transitions!.Count);
        Assert.Single(switchThumbState.Transitions!);
        Assert.Single(switchThumb.Transitions!);
        Assert.Single(textLabel.Transitions!);
        Assert.Equal(2, pinCell.Transitions!.Count);
        Assert.All(pinCell.Transitions!, transition =>
            Assert.Equal(TimeSpan.FromMilliseconds(100), Assert.IsType<BrushTransition>(transition).Duration));

        MdMotion.SetScheme(content, MdMotionScheme.None);
        foreach (var control in new Control[]
                 {
                     buttonContainer, checkGlyph, radioDot, switchTrack, switchThumbState,
                     switchThumb, textLabel, pinCell
                 })
            Assert.Null(control.Transitions);
    }

    [AvaloniaFact]
    public void Indeterminate_Loading_And_Ripple_Timers_React_To_Runtime_Scheme_And_Detach()
    {
        var circular = new MdCircularProgressIndicator { IsIndeterminate = true };
        var linear = new MdLinearProgressIndicator { IsIndeterminate = true };
        var loading = new MdLoadingIndicator { IsActive = true };
        var button = new MdButton { Width = 180, Height = 56, Content = "Ripple" };
        var root = new StackPanel { Children = { circular, linear, loading, button } };
        var scope = Show(root, 420, 360);
        var circularTimer = PrivateTimer(circular);
        var linearTimer = PrivateTimer(linear);
        var loadingTimer = PrivateTimer(loading);
        var ripple = button.GetVisualDescendants().OfType<MdRipplePresenter>().Single();
        var rippleTimer = PrivateTimer(ripple);

        Assert.True(circularTimer.IsEnabled);
        Assert.True(linearTimer.IsEnabled);
        Assert.True(loadingTimer.IsEnabled);

        var point = button.TranslatePoint(new Point(40, 28), scope.Window)!.Value;
        scope.Window.MouseMove(point, RawInputModifiers.None);
        scope.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        Assert.True(rippleTimer.IsEnabled);

        MdMotion.SetScheme(root, MdMotionScheme.Reduced);
        Assert.False(circularTimer.IsEnabled);
        Assert.False(linearTimer.IsEnabled);
        Assert.False(loadingTimer.IsEnabled);
        Assert.True(rippleTimer.IsEnabled); // Reduced keeps only its short full-size fade.

        MdMotion.SetScheme(root, MdMotionScheme.None);
        Assert.False(rippleTimer.IsEnabled);
        MdMotion.SetScheme(root, MdMotionScheme.Expressive);
        Assert.True(circularTimer.IsEnabled);
        Assert.True(linearTimer.IsEnabled);
        Assert.True(loadingTimer.IsEnabled);

        scope.Dispose();
        Assert.False(circularTimer.IsEnabled);
        Assert.False(linearTimer.IsEnabled);
        Assert.False(loadingTimer.IsEnabled);
        Assert.False(rippleTimer.IsEnabled);
    }

    [AvaloniaFact]
    public async Task Hero_Uses_Overlay_Flight_And_Cleans_Up_On_Scheme_Change_And_Detach()
    {
        var source = new MdHero { Tag = "avatar", Width = 72, Height = 72, Content = new Border { Background = Brushes.Red } };
        var destination = new MdHero
        {
            Tag = "avatar", Width = 128, Height = 128,
            Content = new Border { Background = Brushes.Blue }
        };
        Canvas.SetLeft(destination, 260);
        Canvas.SetTop(destination, 120);
        var canvas = new Canvas { Children = { source, destination } };
        var layers = new VisualLayerManager { EnableOverlayLayer = true, Child = canvas };
        var scope = Show(layers, 520, 360);
        var overlay = Assert.IsType<OverlayLayer>(OverlayLayer.GetOverlayLayer(source));
        var baselineChildren = overlay.Children.Count;
        var completed = 0;
        source.TransitionCompleted += (_, _) => completed++;

        var canceledFlight = source.TransitionToAsync(destination).AsTask();
        Dispatcher.UIThread.RunJobs();
        Assert.True(source.IsTransitioning);
        Assert.True(destination.IsTransitioning);
        Assert.Equal(baselineChildren + 1, overlay.Children.Count);
        Assert.Equal(0, source.Opacity);
        Assert.Equal(0, destination.Opacity);

        MdMotion.SetScheme(source, MdMotionScheme.None);
        Assert.False(await canceledFlight);
        Dispatcher.UIThread.RunJobs();
        Assert.False(source.IsTransitioning);
        Assert.False(destination.IsTransitioning);
        Assert.Equal(baselineChildren, overlay.Children.Count);
        Assert.Equal(1, source.Opacity);
        Assert.Equal(1, destination.Opacity);
        Assert.Equal(0, completed);

        MdMotion.SetScheme(source, MdMotionScheme.Reduced);
        Assert.True(await source.TransitionToAsync(destination));
        Assert.Equal(1, completed);
        Assert.Equal(baselineChildren, overlay.Children.Count);

        MdMotion.SetScheme(source, MdMotionScheme.Standard);
        var detachedFlight = source.TransitionToAsync(destination).AsTask();
        Dispatcher.UIThread.RunJobs();
        scope.Dispose();
        Assert.False(await detachedFlight);
        Assert.False(source.IsTransitioning);
        Assert.False(destination.IsTransitioning);
    }

    [AvaloniaFact]
    public async Task Draggable_Sheet_Separates_Animate_Jump_Drag_And_Scheme_Semantics()
    {
        var sheet = new MdDraggableScrollableSheet
        {
            Width = 480,
            Height = 600,
            MinimumExtent = .2,
            InitialExtent = .4,
            MaximumExtent = 1,
            SnapSizes = [.2, .5, 1],
            Header = "Drag",
            Content = new Border()
        };
        using var scope = Show(sheet, 520, 640);
        var surface = Part<Border>(sheet, "PART_Surface");

        MdMotion.SetScheme(sheet, MdMotionScheme.Standard);
        var animation = sheet.AnimateToAsync(.8).AsTask();
        Assert.True(sheet.IsAnimating);
        Assert.NotNull(surface.Transitions);
        sheet.JumpTo(.3);
        Assert.False(sheet.IsAnimating);
        Assert.Equal(.3, sheet.Extent, 6);
        Assert.Null(surface.Transitions);
        await animation;
        Dispatcher.UIThread.RunJobs();
        Assert.NotNull(surface.Transitions);

        MdMotion.SetScheme(sheet, MdMotionScheme.Reduced);
        var completions = 0;
        sheet.AnimationCompleted += (_, _) => completions++;
        await sheet.AnimateToAsync(.6);
        Assert.False(sheet.IsAnimating);
        Assert.Null(surface.Transitions);
        Assert.Equal(.6, sheet.Extent, 6);
        Assert.Equal(1, completions);

        MdMotion.SetScheme(sheet, MdMotionScheme.None);
        await sheet.AnimateToAsync(.9);
        Assert.False(sheet.IsAnimating);
        Assert.Null(surface.Transitions);
        Assert.Equal(.9, sheet.Extent, 6);
        Assert.Equal(2, completions);
    }

    [AvaloniaFact]
    public void Snackbar_Dialog_And_Ecosystem_Popups_Have_Staged_Reversible_Presence()
    {
        var snackbar = new MdSnackbar { Content = "Saved", Duration = TimeSpan.Zero };
        var dialog = new MdDialogHost { Dialog = new MdDialog { Headline = "Confirm", Content = "Body" } };
        var cascader = new MdCascader
        {
            ItemsSource = [new MdCascaderItem("docs", "Docs", [new MdCascaderItem("api", "API")])]
        };
        var asyncSelect = new MdAsyncSelect { ItemsSource = new[] { "One", "Two" } };
        var root = new StackPanel { Children = { snackbar, dialog, cascader, asyncSelect } };
        using var scope = Show(root, 720, 720);

        snackbar.IsOpen = true;
        dialog.IsOpen = true;
        cascader.IsDropDownOpen = true;
        Assert.Contains(":closed", snackbar.Classes);
        Assert.Contains(":closed", dialog.Classes);
        Assert.Contains(":closed", cascader.Classes);
        Assert.True(snackbar.IsVisible);
        Assert.True(Part<Grid>(dialog, "PART_Overlay").IsVisible);

        Dispatcher.UIThread.RunJobs();
        Assert.Contains(":open", snackbar.Classes);
        Assert.Contains(":open", dialog.Classes);
        Assert.Contains(":open", cascader.Classes);
        Assert.Equal(2, Part<Border>(cascader, "PART_Surface").Transitions!.Count);
        var asyncSurface = PrivateField<Border>(asyncSelect, "_surface");
        Assert.Equal(2, asyncSurface.Transitions!.Count);

        snackbar.IsOpen = false;
        dialog.IsOpen = false;
        cascader.IsDropDownOpen = false;
        Assert.True(snackbar.IsVisible);
        Assert.True(Part<Grid>(dialog, "PART_Overlay").IsVisible);

        snackbar.IsOpen = true;
        dialog.IsOpen = true;
        cascader.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();
        Assert.Contains(":open", snackbar.Classes);
        Assert.Contains(":open", dialog.Classes);
        Assert.Contains(":open", cascader.Classes);

        MdMotion.SetScheme(root, MdMotionScheme.None);
        snackbar.IsOpen = false;
        dialog.IsOpen = false;
        cascader.IsDropDownOpen = false;
        Assert.False(snackbar.IsVisible);
        Assert.False(Part<Grid>(dialog, "PART_Overlay").IsVisible);
        Assert.Null(Part<Border>(cascader, "PART_Surface").Transitions);
        Assert.Null(asyncSurface.Transitions);

        snackbar.IsOpen = true;
        dialog.IsOpen = true;
        cascader.IsDropDownOpen = true;
        Assert.Contains(":open", snackbar.Classes);
        Assert.Contains(":open", dialog.Classes);
        Assert.Contains(":open", cascader.Classes);

        // Native Popup rendering is not available in this headless host; detached owners still
        // prove coordination, delayed visual presence, reversal, and None's immediate unmount.
        var detachedCascader = new MdCascader();
        detachedCascader.IsDropDownOpen = true;
        var detachedAsync = new MdAsyncSelect();
        detachedAsync.IsDropDownOpen = true;
        Assert.False(detachedCascader.IsDropDownOpen);
        Assert.True(detachedAsync.IsPopupOpen);
        detachedAsync.IsDropDownOpen = false;
        Assert.True(detachedAsync.IsPopupOpen);
        detachedAsync.IsDropDownOpen = true;
        Assert.True(detachedAsync.IsPopupOpen);
        MdMotion.SetScheme(detachedAsync, MdMotionScheme.None);
        detachedAsync.IsDropDownOpen = false;
        Assert.False(detachedAsync.IsPopupOpen);
    }

    [AvaloniaFact]
    public async Task Async_Select_Detach_Cancels_Search_And_Prevents_Late_Visual_State_Writes()
    {
        var completion = new TaskCompletionSource<IReadOnlyList<object?>>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var select = new MdAsyncSelect
        {
            SearchProvider = (_, _) => new ValueTask<IReadOnlyList<object?>>(completion.Task)
        };
        var scope = Show(select);
        var search = select.SearchAsync().AsTask();
        Assert.Equal(MdAsyncRequestState.Loading, select.State);

        scope.Dispose();
        completion.SetResult(["Late result"]);
        await search;

        Assert.Empty(select.Results);
        Assert.False(select.IsDropDownOpen);
        Assert.False(select.IsPopupOpen);
    }

    [AvaloniaFact]
    public void Pointer_Capture_Loss_Restores_RangeSlider_And_Slidable_Settle_State()
    {
        var range = new MdRangeSlider { Width = 400, Height = 56, LowerValue = 25, UpperValue = 75 };
        var slidable = new MdSlidableItem
        {
            Width = 400, Height = 72, Content = "Message", StartActions = "Archive", EndActions = "Delete"
        };
        var root = new StackPanel { Children = { range, slidable } };
        using var scope = Show(root, 500, 260);
        var foreground = Part<Border>(slidable, "PART_Foreground");
        var foregroundTransform = Assert.IsType<TranslateTransform>(foreground.RenderTransform);

        var slideStart = slidable.TranslatePoint(new Point(180, 36), scope.Window)!.Value;
        scope.Window.MouseMove(slideStart, RawInputModifiers.None);
        scope.Window.MouseDown(slideStart, MouseButton.Left, RawInputModifiers.None);
        scope.Window.MouseMove(slideStart.WithX(slideStart.X + 48), RawInputModifiers.LeftMouseButton);
        Assert.Contains(":dragging", slidable.Classes);
        Assert.Null(foregroundTransform.Transitions);

        void StealFromSlidable(object? _, PointerEventArgs e) => e.Pointer.Capture(range);
        slidable.AddHandler(InputElement.PointerMovedEvent, StealFromSlidable,
            global::Avalonia.Interactivity.RoutingStrategies.Bubble, true);
        scope.Window.MouseMove(slideStart.WithX(slideStart.X + 64), RawInputModifiers.LeftMouseButton);
        slidable.RemoveHandler(InputElement.PointerMovedEvent, StealFromSlidable);
        Dispatcher.UIThread.RunJobs();
        Assert.DoesNotContain(":dragging", slidable.Classes);
        Assert.NotNull(foregroundTransform.Transitions);
        scope.Window.MouseUp(slideStart.WithX(slideStart.X + 64), MouseButton.Left, RawInputModifiers.None);

        var rangePoint = range.TranslatePoint(new Point(110, 28), scope.Window)!.Value;
        scope.Window.MouseMove(rangePoint, RawInputModifiers.None);
        scope.Window.MouseDown(rangePoint, MouseButton.Left, RawInputModifiers.None);
        scope.Window.MouseMove(rangePoint.WithX(rangePoint.X + 24), RawInputModifiers.LeftMouseButton);
        Assert.Null(range.Transitions);

        void StealFromRange(object? _, PointerEventArgs e) => e.Pointer.Capture(slidable);
        range.AddHandler(InputElement.PointerMovedEvent, StealFromRange,
            global::Avalonia.Interactivity.RoutingStrategies.Bubble, true);
        scope.Window.MouseMove(rangePoint.WithX(rangePoint.X + 40), RawInputModifiers.LeftMouseButton);
        range.RemoveHandler(InputElement.PointerMovedEvent, StealFromRange);
        Dispatcher.UIThread.RunJobs();
        Assert.NotNull(range.Transitions);
        scope.Window.MouseUp(rangePoint.WithX(rangePoint.X + 40), MouseButton.Left, RawInputModifiers.None);
    }

    [AvaloniaFact]
    public async Task Fab_Tree_Transfer_And_Sequence_Cancel_Delayed_Or_MidFlight_Work()
    {
        var fab = new MdFabMenu { ItemsSource = new[] { "One", "Two" }, IsOpen = true };
        var branch = new MdTreeNode("branch", "Branch", [new MdTreeNode("child", "Child")]);
        var tree = new MdTreeView { Width = 360, Height = 220, Roots = [branch] };
        var transfer = new MdTransfer
        {
            Width = 640, Height = 260, ItemsSource = new[] { "A", "B", "C" }, SelectedItems = new[] { "A" }
        };
        var sequence = new MdAnimationSequence
        {
            AutoPlay = false,
            ItemDelay = TimeSpan.FromSeconds(1),
            ItemDuration = TimeSpan.FromSeconds(1),
            Children = { new Border(), new Border() }
        };
        var root = new StackPanel { Children = { fab, tree, transfer, sequence } };
        var scope = Show(root, 760, 820);
        var fabTimer = PrivateTimer(fab, "_closeTimer");
        MdMotion.SetScheme(tree, MdMotionScheme.Standard);
        MdMotion.SetScheme(transfer, MdMotionScheme.Standard);
        MdMotion.SetScheme(sequence, MdMotionScheme.Standard);

        fab.IsOpen = false;
        Assert.True(fabTimer.IsEnabled);
        Assert.True(fab.AreItemsVisible);

        tree.Expand(branch);
        transfer.MoveToTarget(["B"]);
        var sequenceTask = sequence.PlayAsync().AsTask();
        Assert.True(sequence.IsPlaying);
        Assert.All(sequence.Children, child => Assert.NotNull(child.Transitions));

        MdMotion.SetScheme(tree, MdMotionScheme.None);
        MdMotion.SetScheme(transfer, MdMotionScheme.None);
        MdMotion.SetScheme(sequence, MdMotionScheme.None);
        Dispatcher.UIThread.RunJobs();
        await sequenceTask;
        Assert.False(sequence.IsPlaying);
        Assert.All(sequence.Children, child =>
        {
            Assert.Equal(1, child.Opacity);
            Assert.Null(child.Transitions);
        });
        Assert.All(tree.GetVisualDescendants().OfType<ListBoxItem>(), row =>
        {
            Assert.Equal(1, row.Opacity);
            Assert.Null(row.Transitions);
            if (row.RenderTransform is TranslateTransform transform) Assert.Null(transform.Transitions);
        });
        Assert.All(transfer.GetVisualDescendants().OfType<ListBoxItem>(), row =>
        {
            Assert.Equal(1, row.Opacity);
            Assert.Null(row.Transitions);
            if (row.RenderTransform is TranslateTransform transform) Assert.Null(transform.Transitions);
        });

        scope.Dispose();
        Assert.False(fabTimer.IsEnabled);
        var visibleAfterDetach = fab.AreItemsVisible;
        await Task.Delay(MdMotion.Resolve(MdMotionScheme.Standard, MdMotionKind.Effects, MdMotionSpeed.Fast).Duration + TimeSpan.FromMilliseconds(40));
        Assert.Equal(visibleAfterDetach, fab.AreItemsVisible);
    }

    private static T Part<T>(Control owner, string name) where T : Control =>
        owner.GetVisualDescendants().OfType<T>().Single(control => control.Name == name);

    private static DispatcherTimer PrivateTimer(object owner, string fieldName = "_timer") =>
        PrivateField<DispatcherTimer>(owner, fieldName);

    private static T PrivateField<T>(object owner, string fieldName) where T : class =>
        Assert.IsType<T>(owner.GetType()
            .GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(owner));

    private static Scope Show(Control content, double width = 760, double height = 560)
    {
        var window = new Window { Width = width, Height = height, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new Scope(window);
    }

    private sealed class DatePickerMotionProbe : MdDatePicker
    {
        protected override Type StyleKeyOverride => typeof(MdDatePicker);
        public Border? Surface { get; private set; }
        public ItemsControl? DaysHost { get; private set; }
        public ItemsControl? OutgoingDaysHost { get; private set; }
        protected override void OnApplyTemplate(global::Avalonia.Controls.Primitives.TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            Surface = e.NameScope.Find<Border>("PART_Surface");
            DaysHost = e.NameScope.Find<ItemsControl>("PART_DaysHost");
            OutgoingDaysHost = e.NameScope.Find<ItemsControl>("PART_OutgoingDaysHost");
        }
    }

    private sealed class TimePickerMotionProbe : MdTimePicker
    {
        protected override Type StyleKeyOverride => typeof(MdTimePicker);
        public Border? Surface { get; private set; }
        public Control? DialPanel { get; private set; }
        protected override void OnApplyTemplate(global::Avalonia.Controls.Primitives.TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            Surface = e.NameScope.Find<Border>("PART_Surface");
            DialPanel = e.NameScope.Find<Control>("PART_DialPanel");
        }
    }

    private sealed class Scope(Window window) : IDisposable
    {
        public Window Window { get; } = window;
        public void Dispose() => Window.Close();
    }
}
