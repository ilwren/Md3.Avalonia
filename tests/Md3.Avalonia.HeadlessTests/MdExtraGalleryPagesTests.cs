using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Gallery;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class MdExtraGalleryPagesTests
{
    private static readonly (string Title, string ClassName, string Chinese, Type Page, Type Control, string ButtonName)[] Demos =
    [
        ("Auto-size text", "MdAutoSizeText", "自动缩放文本", typeof(Gallery.Pages.AutoSizeTextDemoPage), typeof(MdAutoSizeText), "AutoSizeTextNav"),
        ("Confetti", "MdConfetti", "庆祝粒子", typeof(Gallery.Pages.ConfettiDemoPage), typeof(MdConfetti), "ConfettiNav"),
        ("Emoji picker", "MdEmojiPicker", "表情选择器", typeof(Gallery.Pages.EmojiPickerDemoPage), typeof(MdEmojiPicker), "EmojiPickerNav"),
        ("Expandable text", "MdExpandableText", "可展开文本", typeof(Gallery.Pages.ExpandableTextDemoPage), typeof(MdExpandableText), "ExpandableTextNav"),
        ("Marquee", "MdMarquee", "滚动文本", typeof(Gallery.Pages.MarqueeDemoPage), typeof(MdMarquee), "MarqueeNav"),
        ("Media progress bar", "MdMediaProgressBar", "媒体进度条", typeof(Gallery.Pages.MediaProgressDemoPage), typeof(MdMediaProgressBar), "MediaProgressNav"),
        ("Page indicator", "MdPageIndicator", "页面指示器", typeof(Gallery.Pages.PageIndicatorDemoPage), typeof(MdPageIndicator), "PageIndicatorNav"),
        ("Slide to confirm", "MdSlideToConfirm", "滑动确认", typeof(Gallery.Pages.SlideToConfirmDemoPage), typeof(MdSlideToConfirm), "SlideToConfirmNav"),
        ("Wheel picker", "MdWheelPicker", "数字滚轮", typeof(Gallery.Pages.WheelPickerDemoPage), typeof(MdWheelPicker), "WheelPickerNav")
    ];

    [AvaloniaFact]
    public void Desktop_Search_And_Direct_Navigation_Find_Real_Controls()
    {
        var window = new MainWindow { Width = 1280, Height = 860 };
        window.Show();
        try
        {
            var search = window.FindControl<MdSearchBar>("GallerySearch")!;
            foreach (var demo in Demos)
            {
                Assert.Contains(demo.Title, window.IndexedPageTitles);
                foreach (var query in new[] { demo.ClassName, demo.Chinese })
                {
                    search.Text = query;
                    Dispatcher.UIThread.RunJobs();
                    Assert.True(window.FindControl<MdButton>(demo.ButtonName)!.IsVisible, query);
                }
                search.Text = "";
                Assert.True(window.NavigateToIndexedPage(demo.Title));
                Assert.IsType(demo.Page, window.CurrentPage);
                Assert.Contains(window.CurrentPage!.GetLogicalDescendants(), child => demo.Control.IsInstanceOfType(child));
            }
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Android_Drawer_Entries_Open_The_Same_Dedicated_Pages()
    {
        var shell = new AndroidGalleryView();
        var window = new Window { Width = 390, Height = 780, Content = shell };
        window.Show();
        try
        {
            foreach (var demo in Demos)
            {
                Assert.True(shell.NavigateToPage(demo.Title), demo.Title);
                Assert.Equal(demo.Title, shell.CurrentPageTitle);
                var viewport = Assert.IsAssignableFrom<Control>(shell.FindControl<ContentControl>("MobilePageHost")!.Content);
                var page = Assert.Single(viewport.GetLogicalDescendants().OfType<Control>().Prepend(viewport), child => child.GetType() == demo.Page);
                Assert.Contains(page.GetLogicalDescendants(), child => demo.Control.IsInstanceOfType(child));
            }
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Automation_Interactions_Update_The_Demo_Status_And_Reset()
    {
        var window = new MainWindow { Width = 1280, Height = 860 };
        window.Show();
        try
        {
            window.NavigateToIndexedPage("Wheel picker");
            var wheel = window.CurrentPage!.GetLogicalDescendants().OfType<MdWheelPicker>().Single();
            Range(wheel).SetValue(15);
            Assert.Contains(window.CurrentPage.GetLogicalDescendants().OfType<TextBlock>(), text => text.Text == "Selected: 15");

            window.NavigateToIndexedPage("Page indicator");
            var dots = window.CurrentPage!.GetLogicalDescendants().OfType<MdPageIndicator>().Single();
            Range(dots).SetValue(2);
            Assert.Contains(window.CurrentPage.GetLogicalDescendants().OfType<TextBlock>(), text => text.Text == "Page 3 of 4");

            window.NavigateToIndexedPage("Media progress bar");
            var media = window.CurrentPage!.GetLogicalDescendants().OfType<MdMediaProgressBar>().Single();
            Range(media).SetValue(60);
            Assert.Contains(window.CurrentPage.GetLogicalDescendants().OfType<TextBlock>(), text => text.Text == "Seek position: 60 seconds");

            window.NavigateToIndexedPage("Slide to confirm");
            var slide = window.CurrentPage!.GetLogicalDescendants().OfType<MdSlideToConfirm>().Single();
            var peer = ControlAutomationPeer.CreatePeerForElement(slide);
            Assert.IsAssignableFrom<IInvokeProvider>(peer.GetProvider<IInvokeProvider>()).Invoke();
            Assert.Contains(window.CurrentPage.GetLogicalDescendants().OfType<TextBlock>(), text => text.Text == "Confirmation received.");
            var reset = window.CurrentPage.GetLogicalDescendants().OfType<MdButton>().Single(button => Equals(button.Content, "Reset"));
            reset.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.False(slide.IsConfirmed);
            Assert.Contains(window.CurrentPage.GetLogicalDescendants().OfType<TextBlock>(), text => text.Text == "Waiting for confirmation.");
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Language_Switch_Recreates_The_Same_Dedicated_Page()
    {
        var window = new MainWindow { Width = 1280, Height = 860 };
        window.Show();
        try
        {
            var languages = window.FindControl<ComboBox>("LanguageSelector")!;
            foreach (var demo in Demos)
            {
                languages.SelectedIndex = 0;
                Assert.True(window.NavigateToIndexedPage(demo.Title));
                languages.SelectedIndex = 1;
                Assert.IsType(demo.Page, window.CurrentPage);
                Assert.Contains(window.CurrentPage!.GetLogicalDescendants(), child => demo.Control.IsInstanceOfType(child));
                Assert.Contains(window.CurrentPage.GetLogicalDescendants().OfType<TextBlock>(), block => !string.IsNullOrEmpty(block.Text) && block.Text.Any(c => c >= '\u4e00' && c <= '\u9fff'));
            }
            languages.SelectedIndex = 0;
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Related_Controls_Are_Visible_And_Their_Examples_Respond()
    {
        var window = new MainWindow { Width = 1280, Height = 860 };
        window.Show();
        try
        {
            window.NavigateToIndexedPage("Avatar");
            Assert.Single(window.CurrentPage!.GetLogicalDescendants().OfType<MdAvatarGroup>());

            window.NavigateToIndexedPage("Skeleton");
            var group = window.CurrentPage!.GetLogicalDescendants().OfType<MdSkeletonGroup>().Single();
            var toggle = window.CurrentPage.GetLogicalDescendants().OfType<MdButton>().Single(b => Equals(b.Content, "Toggle loading"));
            toggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.False(group.IsLoading);
            Assert.All(group.Children.OfType<MdSkeleton>(), skeleton => Assert.False(skeleton.IsLoading));

            window.NavigateToIndexedPage("Motion");
            var fade = window.CurrentPage!.GetLogicalDescendants().OfType<MdFadeThrough>().Single();
            var axis = window.CurrentPage.GetLogicalDescendants().OfType<MdSharedAxis>().Single();
            var swap = window.CurrentPage.GetLogicalDescendants().OfType<MdButton>().Single(b => Equals(b.Content, "Switch content"));
            swap.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal("Second view", fade.Content);
            Assert.Equal("Second view", axis.Content);

            window.NavigateToIndexedPage("Adaptive layout");
            Assert.Single(window.CurrentPage!.GetLogicalDescendants().OfType<MdSafeArea>());
            window.NavigateToIndexedPage("Date and time pickers");
            Assert.Single(window.CurrentPage!.GetLogicalDescendants().OfType<MdDatePickerDialog>());
            Assert.Single(window.CurrentPage.GetLogicalDescendants().OfType<MdTimePickerDialog>());
            window.NavigateToIndexedPage("Rich editor");
            Assert.Single(window.CurrentPage!.GetLogicalDescendants().OfType<MdRichTextEditor>());
        }
        finally { window.Close(); }
    }

    private static IRangeValueProvider Range(Control control)
    {
        var peer = ControlAutomationPeer.CreatePeerForElement(control);
        return Assert.IsAssignableFrom<IRangeValueProvider>(peer.GetProvider<IRangeValueProvider>());
    }
}
