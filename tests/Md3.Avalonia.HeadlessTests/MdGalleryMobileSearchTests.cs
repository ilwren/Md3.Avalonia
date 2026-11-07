using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdGalleryMobileSearchTests
{
    [AvaloniaFact]
    public void Home_Search_Shows_And_Opens_Results_By_Class_And_Chinese_Name()
    {
        var shell = new AndroidGalleryView();
        var host = new Window { Width = 390, Height = 780, Content = shell };
        host.Show();
        try
        {
            var search = shell.FindControl<MdSearchBar>("HomeSearch")!;
            var results = shell.FindControl<StackPanel>("HomeSearchResults")!;
            var status = shell.FindControl<TextBlock>("HomeSearchStatus")!;
            search.Text = "MdEmojiPicker";
            Dispatcher.UIThread.RunJobs();
            var result = Assert.Single(results.Children.OfType<MdButton>());
            Assert.Equal("Emoji picker", result.Content);
            result.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal("Emoji picker", shell.CurrentPageTitle);

            Assert.True(shell.NavigateToPage("Overview"));
            search.Text = "页面指示器";
            Dispatcher.UIThread.RunJobs();
            result = Assert.Single(results.Children.OfType<MdButton>());
            Assert.Equal("Page indicator", result.Content);

            search.Text = "no such component 493837";
            Dispatcher.UIThread.RunJobs();
            Assert.Empty(results.Children);
            Assert.True(status.IsVisible);
            Assert.Equal("No matching components.", status.Text);
            search.Text = "";
            Dispatcher.UIThread.RunJobs();
            Assert.Empty(results.Children);
            Assert.False(status.IsVisible);
        }
        finally { host.Close(); }
    }

    [AvaloniaFact]
    public void Home_And_Drawer_Share_The_Query_When_Results_Exceed_The_Home_Limit()
    {
        var shell = new AndroidGalleryView();
        var host = new Window { Width = 390, Height = 780, Content = shell };
        host.Show();
        try
        {
            var home = shell.FindControl<MdSearchBar>("HomeSearch")!;
            var drawerSearch = shell.FindControl<MdSearchBar>("DrawerSearch")!;
            var items = shell.FindControl<StackPanel>("MobileNavigationItems")!;
            var results = shell.FindControl<StackPanel>("HomeSearchResults")!;
            var status = shell.FindControl<TextBlock>("HomeSearchStatus")!;
            home.Text = "a";
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("a", drawerSearch.Text);
            Assert.Equal(8, results.Children.OfType<MdButton>().Count());
            Assert.Contains("Open the menu", status.Text ?? string.Empty);
            Assert.True(items.Children.OfType<MdButton>().Count(button => button.IsVisible) > 8);

            drawerSearch.Text = "mdrevealhost";
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("mdrevealhost", home.Text);
            Assert.Equal("Motion", Assert.Single(results.Children.OfType<MdButton>()).Content);
            Assert.Equal("Motion", Assert.Single(items.Children.OfType<MdButton>(), button => button.IsVisible).Content);

            drawerSearch.Text = "";
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("", home.Text);
            Assert.Empty(results.Children);
            Assert.All(items.Children.OfType<MdButton>(), button => Assert.True(button.IsVisible));
        }
        finally { host.Close(); }
    }

    [AvaloniaFact]
    public void Drawer_Filters_Sections_And_Enter_Navigates_Then_Clear_Restores_Them()
    {
        var shell = new AndroidGalleryView();
        var host = new Window { Width = 390, Height = 780, Content = shell };
        host.Show();
        try
        {
            var drawer = shell.FindControl<MdNavigationDrawer>("MobileDrawer")!;
            var search = shell.FindControl<MdSearchBar>("DrawerSearch")!;
            var items = shell.FindControl<StackPanel>("MobileNavigationItems")!;
            var status = shell.FindControl<TextBlock>("DrawerSearchStatus")!;
            drawer.IsOpen = true;
            search.Text = "mdwheelpicker";
            Dispatcher.UIThread.RunJobs();
            var visible = items.Children.OfType<MdButton>().Where(button => button.IsVisible).ToArray();
            Assert.Equal("Wheel picker", Assert.Single(visible).Content);
            Assert.Single(items.Children.OfType<TextBlock>(), header => header.IsVisible);
            Assert.True(status.IsVisible);
            Assert.True(search.Focus());
            host.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            host.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Wheel picker", shell.CurrentPageTitle);
            Assert.False(drawer.IsOpen);

            drawer.IsOpen = true;
            search.Text = "never matched 12345";
            Dispatcher.UIThread.RunJobs();
            Assert.DoesNotContain(items.Children.OfType<MdButton>(), button => button.IsVisible);
            Assert.DoesNotContain(items.Children.OfType<TextBlock>(), header => header.IsVisible);
            Assert.Equal("No matching components.", status.Text);
            search.Text = "";
            Dispatcher.UIThread.RunJobs();
            Assert.All(items.Children.OfType<MdButton>(), button => Assert.True(button.IsVisible));
            Assert.All(items.Children.OfType<TextBlock>(), header => Assert.True(header.IsVisible));
            Assert.False(status.IsVisible);
        }
        finally { host.Close(); }
    }

    [AvaloniaFact]
    public void Desktop_Keywords_Are_Shared_And_Mobile_Index_Covers_Supported_Pages()
    {
        var desktop = new MainWindow();
        var mobile = new AndroidGalleryView();
        var host = new Window { Width = 390, Height = 780, Content = mobile };
        desktop.Show();
        host.Show();
        try
        {
            var items = mobile.FindControl<StackPanel>("MobileNavigationItems")!;
            var mobileTitles = items.Children.OfType<MdButton>().Select(button => button.Content?.ToString()).ToHashSet();
            foreach (var title in desktop.IndexedPageTitles)
            {
                Assert.False(string.IsNullOrEmpty(GallerySearchTerms.For(title)), title);
                // Native chrome requires a desktop lifetime. The shared Gallery currently targets
                // net10.0, so it does not load the Notifications Android-specific backend.
                if (title is "Borderless windows" or "Notifications") continue;
                Assert.Contains(title, mobileTitles);
            }
            Assert.True(GallerySearchTerms.Matches("Motion", "MdRevealHost"));
            Assert.True(GallerySearchTerms.Matches("Borderless windows", "MdWindowResizeGrip"));
            Assert.True(GallerySearchTerms.Matches("Emoji picker", "表情选择器"));
        }
        finally { host.Close(); desktop.Close(); }
    }
}
