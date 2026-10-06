using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Covers the two Android platform seams: the system back gesture reaching modal surfaces, and
/// content staying clear of the status bar, cutout and gesture handle.
/// </summary>
public sealed class MdMobilePlatformTests
{
    private static Window ShowWindow(object content)
    {
        var window = new Window { Width = 400, Height = 800, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    /// <summary>
    /// A stand-in for a dismissible surface. The registration mechanism is tested against this
    /// rather than against a real dialog so a failure points at the mechanism.
    /// </summary>
    private sealed class TestSurface : ContentControl
    {
        private readonly MdBackScope _backScope;
        private bool _isOpen;

        public TestSurface(bool isOpen = false)
        {
            _backScope = new MdBackScope(this, OnBackRequested);
            IsOpen = isOpen;
        }

        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                _isOpen = value;
                _backScope.Update(value);
            }
        }

        public bool ConsumesBack { get; set; } = true;

        public int BackCount { get; private set; }

        private bool OnBackRequested()
        {
            BackCount++;
            if (!IsOpen || !ConsumesBack) return false;
            IsOpen = false;
            return true;
        }
    }

    // ---- back navigation -------------------------------------------------

    [AvaloniaFact]
    public void Back_Request_Closes_The_Open_Surface()
    {
        var surface = new TestSurface(isOpen: true);
        var window = ShowWindow(surface);
        try
        {
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(window));
            Assert.True(MdBackNavigation.RequestBack(surface));
            Assert.False(surface.IsOpen);
            Assert.Equal(0, MdBackNavigation.GetHandlerCount(window));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Back_Request_Is_Not_Consumed_When_Nothing_Is_Open()
    {
        var surface = new TestSurface(isOpen: false);
        var window = ShowWindow(surface);
        try
        {
            // The activity must still be allowed to finish, so an unhandled request stays unhandled.
            Assert.Equal(0, MdBackNavigation.GetHandlerCount(window));
            Assert.False(MdBackNavigation.RequestBack(surface));
            Assert.Equal(0, surface.BackCount);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Back_Request_Unwinds_Nested_Surfaces_Newest_First()
    {
        var outer = new TestSurface();
        var inner = new TestSurface();
        var window = ShowWindow(new Panel { Children = { outer, inner } });
        try
        {
            outer.IsOpen = true;
            inner.IsOpen = true;
            Assert.Equal(2, MdBackNavigation.GetHandlerCount(window));

            // The inner surface opened last, so it goes first and the one under it is untouched.
            Assert.True(MdBackNavigation.RequestBack(window));
            Assert.False(inner.IsOpen);
            Assert.True(outer.IsOpen);

            Assert.True(MdBackNavigation.RequestBack(window));
            Assert.False(outer.IsOpen);
            Assert.False(MdBackNavigation.RequestBack(window));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void A_Surface_That_Declines_Passes_The_Request_Down_The_Stack()
    {
        var consuming = new TestSurface(isOpen: true);
        var declining = new TestSurface(isOpen: true) { ConsumesBack = false };
        var window = ShowWindow(new Panel { Children = { consuming, declining } });
        try
        {
            Assert.True(MdBackNavigation.RequestBack(window));
            Assert.Equal(1, declining.BackCount);
            Assert.True(declining.IsOpen);
            Assert.False(consuming.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Closing_A_Surface_Unregisters_Its_Back_Handler()
    {
        var surface = new TestSurface(isOpen: true);
        var window = ShowWindow(surface);
        try
        {
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(window));
            surface.IsOpen = false;
            Assert.Equal(0, MdBackNavigation.GetHandlerCount(window));

            surface.IsOpen = true;
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(window));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Detaching_A_Surface_Releases_Its_Registration()
    {
        var panel = new Panel();
        var surface = new TestSurface(isOpen: true);
        panel.Children.Add(surface);
        var window = ShowWindow(panel);
        try
        {
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(window));
            panel.Children.Remove(surface);
            Assert.Equal(0, MdBackNavigation.GetHandlerCount(window));

            panel.Children.Add(surface);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(window));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void A_Scope_Registers_Only_Once_It_Has_A_Window_To_Register_Against()
    {
        // A surface is routinely opened before it is attached; the registration must still land.
        var surface = new TestSurface(isOpen: true);
        Assert.False(MdBackNavigation.RequestBack(surface));

        var window = ShowWindow(surface);
        try
        {
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(window));
            Assert.True(MdBackNavigation.RequestBack(surface));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void A_Manual_Registration_Can_Be_Disposed_Twice_Safely()
    {
        var anchor = new Border();
        var window = ShowWindow(anchor);
        try
        {
            var calls = 0;
            var registration = MdBackNavigation.Register(anchor, () =>
            {
                calls++;
                return true;
            });

            Assert.True(MdBackNavigation.RequestBack(anchor));
            Assert.Equal(1, calls);

            registration.Dispose();
            registration.Dispose();
            Assert.False(MdBackNavigation.RequestBack(anchor));
            Assert.Equal(1, calls);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Registering_Without_A_Window_Is_Inert_Rather_Than_Fatal()
    {
        var anchor = new Border();
        var registration = MdBackNavigation.Register(anchor, () => true);
        registration.Dispose();
        Assert.False(MdBackNavigation.RequestBack(anchor));
    }

    [AvaloniaFact]
    public void Platform_Back_Requested_Event_Is_Marked_Handled_Once_A_Surface_Consumes_It()
    {
        var surface = new TestSurface(isOpen: true);
        var window = ShowWindow(surface);
        try
        {
            var args = new RoutedEventArgs(TopLevel.BackRequestedEvent);
            window.RaiseEvent(args);

            // Handled is what stops Android popping the activity behind the surface.
            Assert.True(args.Handled);
            Assert.False(surface.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Platform_Back_Request_Stays_Unhandled_When_No_Surface_Wants_It()
    {
        var surface = new TestSurface(isOpen: false);
        var window = ShowWindow(surface);
        try
        {
            var args = new RoutedEventArgs(TopLevel.BackRequestedEvent);
            window.RaiseEvent(args);
            Assert.False(args.Handled);
        }
        finally
        {
            window.Close();
        }
    }

    // ---- the real surfaces ----------------------------------------------
    // Each is opened after the window is shown, which is both how they are used and the only
    // order the modal focus trap settles in.

    [AvaloniaFact]
    public void Back_Request_Closes_A_Modal_Navigation_Drawer()
    {
        var drawer = new MdNavigationDrawer
        {
            IsModal = true,
            IsOpen = false,
            Content = new Button { Content = "Page" },
            DrawerContent = new Button { Content = "Destination" }
        };
        var window = ShowWindow(drawer);
        try
        {
            drawer.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(window));

            Assert.True(MdBackNavigation.RequestBack(drawer));
            Dispatcher.UIThread.RunJobs();
            Assert.False(drawer.IsOpen);
            Assert.Equal(0, MdBackNavigation.GetHandlerCount(window));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Back_Request_Closes_A_Modal_Sheet()
    {
        var sheet = new MdSheetHost
        {
            IsModal = true,
            IsOpen = false,
            Content = new Button { Content = "Page" },
            SheetContent = new Button { Content = "Share" }
        };
        var window = ShowWindow(sheet);
        try
        {
            sheet.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.True(MdBackNavigation.RequestBack(sheet));
            Dispatcher.UIThread.RunJobs();
            Assert.False(sheet.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void A_Non_Modal_Sheet_Ignores_Back_So_The_Page_Behind_It_Can_Navigate()
    {
        var sheet = new MdSheetHost
        {
            IsModal = false,
            IsOpen = false,
            Content = new Button { Content = "Page" },
            SheetContent = new Button { Content = "Share" }
        };
        var window = ShowWindow(sheet);
        try
        {
            sheet.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.False(MdBackNavigation.RequestBack(sheet));
            Assert.True(sheet.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Back_Request_Closes_An_Open_Dialog_Host()
    {
        var host = new MdDialogHost
        {
            Content = new Button { Content = "Page" },
            Dialog = new MdDialog { Headline = "Delete draft?", Content = "This cannot be undone." }
        };
        var window = ShowWindow(host);
        try
        {
            host.SetCurrentValue(MdDialogHost.IsOpenProperty, true);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(window));

            Assert.True(MdBackNavigation.RequestBack(host));
            Dispatcher.UIThread.RunJobs();
            Assert.False(host.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    // ---- safe area -------------------------------------------------------

    [AvaloniaFact]
    public void Safe_Area_Insets_Its_Child_On_Every_Edge()
    {
        var child = new Border();
        var area = new MdSafeArea
        {
            Child = child,
            SafeAreaPadding = new Thickness(0, 48, 0, 24)
        };
        var window = ShowWindow(area);
        try
        {
            Assert.Equal(new Thickness(0, 48, 0, 24), area.EffectivePadding);
            Assert.Equal(48, child.Bounds.Top, 1);
            Assert.Equal(area.Bounds.Height - 24, child.Bounds.Bottom, 1);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Safe_Area_Honours_The_Selected_Edges_Only()
    {
        var child = new Border();
        var area = new MdSafeArea
        {
            Child = child,
            Edges = MdSafeAreaEdges.Bottom,
            SafeAreaPadding = new Thickness(12, 48, 12, 24)
        };
        var window = ShowWindow(area);
        try
        {
            // A top app bar wants its surface behind the status bar, so Top is deliberately dropped.
            Assert.Equal(new Thickness(0, 0, 0, 24), area.EffectivePadding);
            Assert.Equal(0, child.Bounds.Top, 1);
            Assert.Equal(0, child.Bounds.Left, 1);
            Assert.Equal(area.Bounds.Height - 24, child.Bounds.Bottom, 1);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Safe_Area_Falls_Back_To_The_Minimum_Padding_Where_The_Inset_Is_Smaller()
    {
        var area = new MdSafeArea
        {
            Child = new Border(),
            MinimumPadding = new Thickness(16),
            SafeAreaPadding = new Thickness(0, 48, 0, 8)
        };
        var window = ShowWindow(area);
        try
        {
            Assert.Equal(new Thickness(16, 48, 16, 16), area.EffectivePadding);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Safe_Area_Reports_Whether_The_Platform_Inset_Is_Doing_Anything()
    {
        var area = new MdSafeArea { Child = new Border(), MinimumPadding = new Thickness(8) };
        var window = ShowWindow(area);
        try
        {
            Assert.False(area.IsSafeAreaActive);
            area.SafeAreaPadding = new Thickness(0, 48, 0, 0);
            Assert.True(area.IsSafeAreaActive);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Safe_Area_Adds_The_Inset_To_Its_Own_Desired_Size()
    {
        var area = new MdSafeArea
        {
            Child = new Border { Width = 100, Height = 40 },
            SafeAreaPadding = new Thickness(0, 48, 0, 24),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        var window = ShowWindow(area);
        try
        {
            Assert.Equal(100, area.DesiredSize.Width, 1);
            Assert.Equal(40 + 48 + 24, area.DesiredSize.Height, 1);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Safe_Area_Re_Lays_Out_When_The_Inset_Changes()
    {
        var child = new Border();
        var area = new MdSafeArea { Child = child };
        var window = ShowWindow(area);
        try
        {
            Assert.Equal(0, child.Bounds.Top, 1);

            // Rotating the device, or the keyboard appearing, moves the inset at runtime.
            area.SafeAreaPadding = new Thickness(0, 64, 0, 0);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(64, child.Bounds.Top, 1);
        }
        finally
        {
            window.Close();
        }
    }

    // The five surfaces that handled Escape but never registered for back. A dismissible surface
    // the gesture cannot reach is worse than no surface: back pops the activity out from under it.

    [AvaloniaFact]
    public void Back_Request_Closes_A_Simple_Dialog_And_Reports_One_Dismissal()
    {
        var dialog = new MdSimpleDialog { ItemsSource = new[] { "One", "Two" } };
        var window = ShowWindow(dialog);
        try
        {
            var dismissals = 0;
            dialog.Dismissed += (_, _) => dismissals++;

            dialog.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(window));

            Assert.True(MdBackNavigation.RequestBack(dialog));
            Dispatcher.UIThread.RunJobs();
            Assert.False(dialog.IsOpen);
            // Back is a cancellation, so it reports exactly once - the trap defect 14 fell into.
            Assert.Equal(1, dismissals);
            Assert.Equal(0, MdBackNavigation.GetHandlerCount(window));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Back_Request_Closes_A_Command_Palette()
    {
        var palette = new MdCommandPalette();
        var window = ShowWindow(palette);
        try
        {
            palette.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(window));

            Assert.True(MdBackNavigation.RequestBack(palette));
            Dispatcher.UIThread.RunJobs();
            Assert.False(palette.IsOpen);
            Assert.Equal(0, MdBackNavigation.GetHandlerCount(window));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Back_Request_Closes_A_Popover()
    {
        var popover = new MdPopover { PopoverContent = new Button { Content = "Action" } };
        var window = ShowWindow(popover);
        try
        {
            popover.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(window));

            Assert.True(MdBackNavigation.RequestBack(popover));
            Dispatcher.UIThread.RunJobs();
            Assert.False(popover.IsOpen);
            Assert.Equal(0, MdBackNavigation.GetHandlerCount(window));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Back_Request_Closes_The_Open_Drop_Downs()
    {
        var cascader = new MdCascader();
        var select = new MdAsyncSelect();
        var window = ShowWindow(new StackPanel { Children = { cascader, select } });
        try
        {
            // One at a time. MdPopupCoordinator allows a single Material popup per UI thread, so
            // opening the second drop-down closes the first and the registrations never stack -
            // which is also why back never has to unwind two of these.
            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(window));

            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();
            Assert.False(cascader.IsDropDownOpen);
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(window));

            Assert.True(MdBackNavigation.RequestBack(window));
            Dispatcher.UIThread.RunJobs();
            Assert.False(select.IsDropDownOpen);
            Assert.Equal(0, MdBackNavigation.GetHandlerCount(window));

            // Nothing left to close, so the platform gets the request back.
            Assert.False(MdBackNavigation.RequestBack(window));

            // And the cascader on its own still answers.
            cascader.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(MdBackNavigation.RequestBack(window));
            Dispatcher.UIThread.RunJobs();
            Assert.False(cascader.IsDropDownOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Platform_Insets_Are_Zero_Where_There_Is_No_Insets_Manager()
    {
        var border = new Border();
        var window = ShowWindow(border);
        try
        {
            // Desktop and headless have no insets manager; the helper must not throw.
            Assert.Equal(default, MdSafeArea.GetPlatformInsets(border));
        }
        finally
        {
            window.Close();
        }
    }
}
