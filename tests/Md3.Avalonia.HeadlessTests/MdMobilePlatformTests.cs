using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
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

    // Each modal surface gets content in the slot the focus trap actually scopes to: Content is
    // the page behind the scrim, not the modal. An empty scope leaves the trap with nothing to
    // focus and it keeps re-posting a redirect.
    private static MdDialogHost NewDialog(bool isOpen) => new()
    {
        Content = new Button { Content = "Page" },
        Dialog = new Button { Content = "OK" },
        IsOpen = isOpen
    };

    private static MdSheetHost NewSheet(bool isModal, bool isOpen) => new()
    {
        Content = new Button { Content = "Page" },
        SheetContent = new Button { Content = "Share" },
        IsModal = isModal,
        IsOpen = isOpen
    };

    private static MdNavigationDrawer NewDrawer(bool isOpen) => new()
    {
        Content = new Button { Content = "Page" },
        DrawerContent = new Button { Content = "Inbox" },
        IsModal = true,
        IsOpen = isOpen
    };

    // ---- back navigation -------------------------------------------------

    [AvaloniaFact]
    public void Back_Request_Closes_An_Open_Dialog_Host()
    {
        var host = NewDialog(isOpen: true);
        var window = ShowWindow(host);
        try
        {
            Assert.True(MdBackNavigation.RequestBack(host));
            Dispatcher.UIThread.RunJobs();
            Assert.False(host.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Back_Request_Is_Not_Consumed_When_Nothing_Is_Open()
    {
        var host = NewDialog(isOpen: false);
        var window = ShowWindow(host);
        try
        {
            // The activity must still be allowed to finish, so an unhandled request stays unhandled.
            Assert.False(MdBackNavigation.RequestBack(host));
            Assert.Equal(0, MdBackNavigation.GetHandlerCount(window));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Back_Request_Unwinds_Nested_Surfaces_Newest_First()
    {
        var drawer = NewDrawer(isOpen: false);
        var dialog = NewDialog(isOpen: false);
        var window = ShowWindow(new Panel { Children = { drawer, dialog } });
        try
        {
            drawer.IsOpen = true;
            dialog.IsOpen = true;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(2, MdBackNavigation.GetHandlerCount(window));

            // The dialog opened last, so it goes first and the drawer underneath is untouched.
            Assert.True(MdBackNavigation.RequestBack(window));
            Dispatcher.UIThread.RunJobs();
            Assert.False(dialog.IsOpen);
            Assert.True(drawer.IsOpen);

            Assert.True(MdBackNavigation.RequestBack(window));
            Dispatcher.UIThread.RunJobs();
            Assert.False(drawer.IsOpen);

            Assert.False(MdBackNavigation.RequestBack(window));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Platform_Back_Requested_Event_Is_Marked_Handled_Once_A_Surface_Consumes_It()
    {
        var sheet = NewSheet(isModal: true, isOpen: true);
        var window = ShowWindow(sheet);
        try
        {
            Dispatcher.UIThread.RunJobs();
            var args = new RoutedEventArgs(TopLevel.BackRequestedEvent);
            window.RaiseEvent(args);
            Dispatcher.UIThread.RunJobs();

            Assert.True(args.Handled);
            Assert.False(sheet.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Closing_A_Surface_Unregisters_Its_Back_Handler()
    {
        var view = new MdSearchView { Content = new Button { Content = "Alpha" }, IsOpen = true };
        var window = ShowWindow(view);
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(window));

            view.IsOpen = false;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(0, MdBackNavigation.GetHandlerCount(window));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void A_Non_Modal_Sheet_Ignores_Back_So_The_Page_Behind_It_Can_Navigate()
    {
        var sheet = NewSheet(isModal: false, isOpen: true);
        var window = ShowWindow(sheet);
        try
        {
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
    public void Detaching_A_Surface_Releases_Its_Registration()
    {
        var panel = new Panel();
        var dialog = NewDialog(isOpen: true);
        panel.Children.Add(dialog);
        var window = ShowWindow(panel);
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(window));

            panel.Children.Remove(dialog);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(0, MdBackNavigation.GetHandlerCount(window));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void A_Scope_Registers_Only_Once_It_Has_A_Window_To_Register_Against()
    {
        // A control is routinely opened before it is attached; the registration must still land.
        var detached = NewDialog(isOpen: true);
        Assert.False(MdBackNavigation.RequestBack(detached));

        var window = ShowWindow(detached);
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(window));
            Assert.True(MdBackNavigation.RequestBack(detached));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Registrations_Are_Independent_Per_Window()
    {
        var first = NewDialog(isOpen: true);
        var second = NewDialog(isOpen: true);
        var firstWindow = ShowWindow(first);
        var secondWindow = ShowWindow(second);
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(firstWindow));
            Assert.Equal(1, MdBackNavigation.GetHandlerCount(secondWindow));

            MdBackNavigation.RequestBack(firstWindow);
            Dispatcher.UIThread.RunJobs();
            Assert.False(first.IsOpen);
            Assert.True(second.IsOpen);
        }
        finally
        {
            firstWindow.Close();
            secondWindow.Close();
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
            Dispatcher.UIThread.RunJobs();
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
            Dispatcher.UIThread.RunJobs();
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
            Dispatcher.UIThread.RunJobs();
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
            Dispatcher.UIThread.RunJobs();
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
            Dispatcher.UIThread.RunJobs();
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
            Dispatcher.UIThread.RunJobs();
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
