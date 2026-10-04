using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Motion;
using CoreCalendarDay = Md3.Avalonia.Controls.MdCalendarDay;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdP0AccessibilityAndKeyboardTests
{
    [AvaloniaFact]
    public void Menu_Uses_Roving_Focus_Wraps_And_Skips_Disabled_Items()
    {
        var first = new MdMenuItem { Content = "Alpha" };
        var disabled = new MdMenuItem { Content = "Blocked", IsEnabled = false };
        var last = new MdMenuItem { Content = "Charlie" };
        var menu = new MdMenu { Items = { first, disabled, last } };
        using var scope = Show(menu);

        Assert.True(menu.FocusFirstItem());
        Assert.True(first.IsFocused);
        Assert.True(KeyboardNavigation.GetIsTabStop(first));
        Assert.False(KeyboardNavigation.GetIsTabStop(last));

        Press(scope.Window, Key.Down, PhysicalKey.ArrowDown);
        Assert.True(last.IsFocused);
        Assert.False(KeyboardNavigation.GetIsTabStop(first));
        Assert.True(KeyboardNavigation.GetIsTabStop(last));

        Press(scope.Window, Key.Down, PhysicalKey.ArrowDown);
        Assert.True(first.IsFocused);
        Press(scope.Window, Key.End, PhysicalKey.End);
        Assert.True(last.IsFocused);
        Press(scope.Window, Key.Home, PhysicalKey.Home);
        Assert.True(first.IsFocused);
        Press(scope.Window, Key.C, PhysicalKey.C, "c");
        Assert.True(last.IsFocused);
    }

    [AvaloniaFact]
    public void Menu_Anchor_Opens_From_Arrow_And_Restores_Previous_Focus_On_Escape()
    {
        var opener = new Button { Content = "Before" };
        var first = new MdMenuItem { Content = "First" };
        var menu = new MdMenu { Items = { first, new MdMenuItem { Content = "Second" } } };
        var anchor = new MdMenuAnchor { Content = "Actions", Menu = menu };
        var root = new StackPanel { Children = { opener, anchor } };
        using var scope = Show(root);

        Assert.True(opener.Focus());
        anchor.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.True(first.IsFocused);

        Press(scope.Window, Key.Escape, PhysicalKey.Escape);
        Dispatcher.UIThread.RunJobs();
        Assert.False(anchor.IsOpen);
        Assert.True(opener.IsFocused);

        Assert.True(anchor.Focus());
        Press(scope.Window, Key.Up, PhysicalKey.ArrowUp);
        Dispatcher.UIThread.RunJobs();
        Assert.True(anchor.IsOpen);
        Assert.True(((MdMenuItem)menu.Items[1]!).IsFocused);
        Press(scope.Window, Key.Tab, PhysicalKey.Tab, "\t");
        Assert.False(anchor.IsOpen);
        Assert.True(anchor.IsFocused);
    }

    [AvaloniaFact]
    public void Cascading_Menu_Escape_Closes_One_Level_And_Leaf_Activation_Closes_Hierarchy()
    {
        var leaf = new MdMenuItem { Content = "Nested action" };
        var childMenu = new MdMenu { Items = { leaf } };
        var submenuItem = new MdSubMenuItem { Content = "More", Submenu = childMenu };
        var rootMenu = new MdMenu { Items = { submenuItem } };
        var anchor = new MdMenuAnchor { Content = "Actions", Menu = rootMenu };
        using var scope = Show(anchor);

        anchor.Show();
        Dispatcher.UIThread.RunJobs();
        var expandCollapse = Assert.IsAssignableFrom<IExpandCollapseProvider>(
            ControlAutomationPeer.CreatePeerForElement(submenuItem).GetProvider<IExpandCollapseProvider>());
        Assert.Equal(ExpandCollapseState.Collapsed, expandCollapse.ExpandCollapseState);
        expandCollapse.Expand();
        Assert.Equal(ExpandCollapseState.Expanded, expandCollapse.ExpandCollapseState);
        Dispatcher.UIThread.RunJobs();
        Assert.True(leaf.IsFocused);

        Press(scope.Window, Key.Escape, PhysicalKey.Escape);
        Assert.True(anchor.IsOpen);
        Assert.False(submenuItem.IsSubmenuOpen);
        Assert.True(submenuItem.IsFocused);

        submenuItem.IsSubmenuOpen = true;
        Dispatcher.UIThread.RunJobs();
        Assert.IsAssignableFrom<IInvokeProvider>(
            ControlAutomationPeer.CreatePeerForElement(leaf).GetProvider<IInvokeProvider>()).Invoke();
        Dispatcher.UIThread.RunJobs();
        Assert.False(anchor.IsOpen);
    }

    [AvaloniaFact]
    public void Dialog_Host_Constrains_Focus_Isolates_Background_And_Restores_Opener()
    {
        var opener = new Button { Content = "Open" };
        var dialogAction = new Button { Content = "Confirm" };
        var dialogCancel = new Button { Content = "Cancel" };
        var dialog = new MdDialog
        {
            Headline = "Confirmation",
            Content = new StackPanel { Children = { dialogAction, dialogCancel } }
        };
        var host = new MdDialogHost { Content = opener, Dialog = dialog };
        using var scope = Show(host);

        var mainContent = Part<ContentPresenter>(host, "PART_MainContent");
        Assert.True(opener.Focus());
        host.IsOpen = true;
        Dispatcher.UIThread.RunJobs();

        Assert.False(mainContent.IsHitTestVisible);
        Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(mainContent));
        Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(opener));
        Assert.True(dialogAction.IsFocused);
        var dialogPeer = ControlAutomationPeer.CreatePeerForElement(dialog);
        Assert.Equal(AutomationControlType.Window, dialogPeer.GetAutomationControlType());
        Assert.Equal("Confirmation", dialogPeer.GetName());

        Press(scope.Window, Key.Tab, PhysicalKey.Tab, "\t", RawInputModifiers.Shift);
        Assert.True(dialogCancel.IsFocused);

        opener.Focus();
        Dispatcher.UIThread.RunJobs();
        Assert.True(dialogAction.IsFocused);

        host.IsOpen = false;
        Dispatcher.UIThread.RunJobs();
        Assert.True(mainContent.IsHitTestVisible);
        Assert.Equal(AccessibilityView.Default, AutomationProperties.GetAccessibilityView(opener));
        Assert.True(opener.IsFocused);
    }

    [AvaloniaFact]
    public void Standalone_Simple_Dialog_Makes_External_Siblings_Inert()
    {
        var background = new Button { Content = "Background" };
        var dialog = new MdSimpleDialog
        {
            Title = "Choose",
            ItemsSource = new[] { "One", "Two" }
        };
        var root = new Grid { Children = { background, dialog } };
        using var scope = Show(root);

        Assert.True(background.Focus());
        dialog.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.False(background.IsHitTestVisible);
        Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(background));

        dialog.Dismiss();
        Dispatcher.UIThread.RunJobs();
        Assert.True(background.IsHitTestVisible);
        Assert.True(background.IsFocused);
    }

    [AvaloniaFact]
    public void Modal_Drawer_And_Sheet_Isolate_Their_Main_Content()
    {
        var drawerMain = new Button { Content = "Drawer background" };
        var drawer = new MdNavigationDrawer
        {
            IsModal = true,
            IsOpen = false,
            Content = drawerMain,
            DrawerContent = new Button { Content = "Destination" }
        };
        var sheetMain = new Button { Content = "Sheet background" };
        var sheet = new MdSheetHost
        {
            IsModal = true,
            Content = sheetMain,
            SheetContent = new Button { Content = "Sheet action" }
        };
        var root = new Grid { Children = { drawer, sheet } };
        using var scope = Show(root);

        var drawerBackground = Part<ContentPresenter>(drawer, "PART_MainContent");
        drawer.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.False(drawerBackground.IsHitTestVisible);
        drawer.Dismiss();
        Dispatcher.UIThread.RunJobs();
        Assert.True(drawerBackground.IsHitTestVisible);

        var sheetBackground = Part<ContentPresenter>(sheet, "PART_MainContent");
        sheet.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.False(sheetBackground.IsHitTestVisible);
        var handle = Part<Button>(sheet, "PART_DragHandle");
        Assert.True(handle.Bounds.Width >= 48);
        Assert.True(handle.Bounds.Height >= 48);
        var invoke = Assert.IsAssignableFrom<IInvokeProvider>(
            ControlAutomationPeer.CreatePeerForElement(handle).GetProvider<IInvokeProvider>());
        invoke.Invoke();
        Assert.False(sheet.IsOpen);
        Assert.True(sheetBackground.IsHitTestVisible);

        sheet.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.True(handle.Focus());
        Press(scope.Window, Key.Enter, PhysicalKey.Enter, "\r");
        Assert.False(sheet.IsOpen);
    }

    [AvaloniaFact]
    public void Date_Picker_Uses_One_Tab_Stop_And_Arrow_Page_Keyboard_Navigation()
    {
        var selected = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var picker = new MdDatePicker { SelectedDate = selected, DisplayDate = selected };
        MdMotion.SetScheme(picker, MdMotionScheme.None);
        using var scope = Show(picker, 520, 720);

        picker.IsOpen = true;
        Dispatcher.UIThread.RunJobs();
        var focused = Assert.IsType<Button>(scope.Window.FocusManager?.GetFocusedElement());
        var day = Assert.IsType<CoreCalendarDay>(focused.DataContext);
        Assert.Equal(selected.Date, day.Date.Date);
        Assert.Contains(Md3.Avalonia.Localization.MdLocalization.GetString("Selected", picker), day.AccessibleText);
        Assert.Single(scope.Window.GetVisualDescendants().OfType<Button>(),
            button => button.DataContext is CoreCalendarDay && KeyboardNavigation.GetIsTabStop(button));

        Press(scope.Window, Key.Right, PhysicalKey.ArrowRight);
        focused = Assert.IsType<Button>(scope.Window.FocusManager?.GetFocusedElement());
        day = Assert.IsType<CoreCalendarDay>(focused.DataContext);
        Assert.Equal(selected.AddDays(1).Date, day.Date.Date);

        Press(scope.Window, Key.Down, PhysicalKey.ArrowDown);
        focused = Assert.IsType<Button>(scope.Window.FocusManager?.GetFocusedElement());
        day = Assert.IsType<CoreCalendarDay>(focused.DataContext);
        Assert.Equal(selected.AddDays(8).Date, day.Date.Date);

        Press(scope.Window, Key.PageDown, PhysicalKey.PageDown);
        Dispatcher.UIThread.RunJobs();
        focused = Assert.IsType<Button>(scope.Window.FocusManager?.GetFocusedElement());
        day = Assert.IsType<CoreCalendarDay>(focused.DataContext);
        Assert.Equal(selected.AddDays(8).AddMonths(1).Date, day.Date.Date);
    }

    [AvaloniaFact]
    public void Time_Dial_Exposes_Range_And_Invokable_Virtual_Values()
    {
        var dial = new MdTimeDial
        {
            Width = 256,
            Height = 256,
            ActivePart = MdTimeDialPart.Hour,
            Is24Hour = false,
            Hour = 15
        };
        using var scope = Show(dial);
        var peer = ControlAutomationPeer.CreatePeerForElement(dial);
        var range = Assert.IsAssignableFrom<IRangeValueProvider>(peer.GetProvider<IRangeValueProvider>());

        Assert.Equal(1, range.Minimum);
        Assert.Equal(12, range.Maximum);
        Assert.Equal(3, range.Value);
        range.SetValue(7);
        Assert.Equal(19, dial.Hour);

        var values = peer.GetChildren();
        Assert.Equal(12, values.Count);
        var twelve = Assert.Single(values, child => child.GetAutomationId() == "Hour_12");
        Assert.Contains("12 hour", twelve.GetName());
        Assert.Equal(ToggleState.Off,
            Assert.IsAssignableFrom<IToggleProvider>(twelve.GetProvider<IToggleProvider>()).ToggleState);
        Assert.IsAssignableFrom<IInvokeProvider>(twelve.GetProvider<IInvokeProvider>()).Invoke();
        Assert.Equal(12, dial.Hour);
        Assert.Equal(ToggleState.On,
            Assert.IsAssignableFrom<IToggleProvider>(twelve.GetProvider<IToggleProvider>()).ToggleState);

        dial.Is24Hour = true;
        Assert.Equal(24, peer.GetChildren().Count);
        Assert.Equal(0, range.Minimum);
        Assert.Equal(23, range.Maximum);
    }

    [AvaloniaFact]
    public void Range_Slider_Exposes_Two_Independent_Range_Thumbs()
    {
        var slider = new MdRangeSlider
        {
            Width = 400,
            Height = 80,
            Minimum = 0,
            Maximum = 100,
            LowerValue = 20,
            UpperValue = 80,
            Step = 5,
            LowerThumbName = "Minimum price",
            UpperThumbName = "Maximum price"
        };
        using var scope = Show(slider);
        var peer = ControlAutomationPeer.CreatePeerForElement(slider);
        var thumbs = peer.GetChildren();

        Assert.Equal(2, thumbs.Count);
        var lower = Assert.IsAssignableFrom<IRangeValueProvider>(thumbs[0].GetProvider<IRangeValueProvider>());
        var upper = Assert.IsAssignableFrom<IRangeValueProvider>(thumbs[1].GetProvider<IRangeValueProvider>());
        Assert.Equal("Minimum price", thumbs[0].GetName());
        Assert.Equal("Maximum price", thumbs[1].GetName());
        Assert.Equal(0, lower.Minimum);
        Assert.Equal(80, lower.Maximum);
        Assert.Equal(20, lower.Value);
        Assert.Equal(20, upper.Minimum);
        Assert.Equal(100, upper.Maximum);
        Assert.Equal(80, upper.Value);
        var lowerBounds = thumbs[0].GetBoundingRectangle();
        var upperBounds = thumbs[1].GetBoundingRectangle();
        Assert.True(lowerBounds.Width >= 47.9, $"Lower thumb bounds were {lowerBounds}.");
        Assert.True(upperBounds.Width >= 47.9, $"Upper thumb bounds were {upperBounds}.");

        lower.SetValue(35);
        upper.SetValue(65);
        Assert.Equal(35, slider.LowerValue);
        Assert.Equal(65, slider.UpperValue);
    }

    [AvaloniaFact]
    public void Range_Slider_Keyboard_Switches_Thumbs_And_Honors_Rtl()
    {
        var slider = new MdRangeSlider
        {
            Width = 400,
            Height = 80,
            LowerValue = 20,
            UpperValue = 80,
            Step = 5,
            FlowDirection = FlowDirection.RightToLeft
        };
        using var scope = Show(slider);
        Assert.True(slider.Focus());

        Press(scope.Window, Key.Left, PhysicalKey.ArrowLeft);
        Assert.Equal(25, slider.LowerValue);
        Press(scope.Window, Key.Tab, PhysicalKey.Tab, "\t");
        Press(scope.Window, Key.Right, PhysicalKey.ArrowRight);
        Assert.Equal(75, slider.UpperValue);
        Press(scope.Window, Key.Home, PhysicalKey.Home);
        Assert.Equal(slider.LowerValue, slider.UpperValue);
    }

    [AvaloniaFact]
    public void Focus_Traversal_Group_Applies_TabIndex_Policy()
    {
        var firstVisual = new Button { Content = "First visual" };
        var firstByIndex = new Button { Content = "First by index" };
        var secondByIndex = new Button { Content = "Second by index" };
        KeyboardNavigation.SetTabIndex(firstVisual, 20);
        KeyboardNavigation.SetTabIndex(firstByIndex, 0);
        KeyboardNavigation.SetTabIndex(secondByIndex, 10);
        var group = new MdFocusTraversalGroup
        {
            Cycle = true,
            Policy = MdFocusTraversalPolicy.TabIndex,
            Content = new StackPanel { Children = { firstVisual, firstByIndex, secondByIndex } }
        };
        using var scope = Show(group);

        Assert.True(firstByIndex.Focus());
        Press(scope.Window, Key.Tab, PhysicalKey.Tab, "\t");
        Assert.True(secondByIndex.IsFocused);
        Press(scope.Window, Key.Tab, PhysicalKey.Tab, "\t");
        Assert.True(firstVisual.IsFocused);
        Press(scope.Window, Key.Tab, PhysicalKey.Tab, "\t");
        Assert.True(firstByIndex.IsFocused);
    }

    [AvaloniaFact]
    public void Keyboard_Avoiding_Host_Preserves_Base_Padding()
    {
        var host = new MdKeyboardAvoidingHost { Padding = new Thickness(1, 2, 3, 4) };
        host.KeyboardHeight = 100;
        Assert.True(host.IsKeyboardActive);
        Assert.Equal(new Thickness(1, 2, 3, 104), host.Padding);

        host.Padding = new Thickness(10);
        Assert.Equal(new Thickness(10, 10, 10, 110), host.Padding);
        host.KeyboardHeight = 0;
        Assert.False(host.IsKeyboardActive);
        Assert.Equal(new Thickness(10), host.Padding);
    }

    [Fact]
    public void Completed_Motion_APIs_No_Longer_Claim_To_Be_Experimental()
    {
        // These four shipped as state shells: pseudo-classes plus an IsVisible switch, no
        // choreography. They now run real Material transitions with a retained outgoing phase,
        // so the warning attribute has to come off with them. The behaviour that replaced the
        // shells is pinned in MdMotionLifecycleTests; this only guards the labelling.
        var types = new[]
        {
            typeof(MdSharedAxis),
            typeof(MdFadeThrough),
            typeof(MdContainerTransform),
            typeof(MdAnimatedVisibility)
        };
        Assert.All(types, type => Assert.Empty(type.GetCustomAttributes(typeof(MdExperimentalAttribute), false)));
        Assert.All(types, type => Assert.NotNull(type.GetProperty("Duration")));
    }

    private static void Press(Window window, Key key, PhysicalKey physicalKey, string? text = null,
        RawInputModifiers modifiers = RawInputModifiers.None)
    {
        window.KeyPress(key, modifiers, physicalKey, text);
        window.KeyRelease(key, modifiers, physicalKey, text);
        Dispatcher.UIThread.RunJobs();
    }

    private static T Part<T>(Control owner, string name) where T : Control =>
        owner.GetVisualDescendants().OfType<T>().Single(control => control.Name == name);

    private static WindowScope Show(Control content, double width = 700, double height = 700)
    {
        var window = new Window { Width = width, Height = height, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowScope(window);
    }

    private sealed class WindowScope(Window window) : IDisposable
    {
        public Window Window { get; } = window;
        public void Dispose() => Window.Close();
    }
}
