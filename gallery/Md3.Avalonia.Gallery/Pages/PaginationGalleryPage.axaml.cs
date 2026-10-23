using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class PaginationGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);
    private bool _syncing;

    public PaginationGalleryPage()
    {
        InitializeComponent();
        SlideOneText.Text = L("Slide 1", "第 1 张");
        SlideTwoText.Text = L("Slide 2", "第 2 张");
        SlideThreeText.Text = L("Slide 3", "第 3 张");
        PageStatus.Text = L("Page 1 of 12", "第 1 页，共 12 页");
        DotsStatus.Text = L("Slide 1 of 3", "第 1 张，共 3 张");
        VerticalStatus.Text = L("Page 2 of 6", "第 2 页，共 6 页");
    }

    private void NumberedPagerChanged(object? sender, PipsPagerSelectedIndexChangedEventArgs e)
    {
        PageProgress.Value = NumberedPager.SelectedPageIndex;
        PageStatus.Text = L($"Page {NumberedPager.SelectedPageIndex + 1} of 12", $"第 {NumberedPager.SelectedPageIndex + 1} 页，共 12 页");
    }

    private void DotsPagerChanged(object? sender, PipsPagerSelectedIndexChangedEventArgs e)
    {
        if (_syncing) return;
        _syncing = true;
        DotsCarousel.SelectedIndex = DotsPager.SelectedPageIndex;
        _syncing = false;
        DotsStatus.Text = L($"Slide {DotsPager.SelectedPageIndex + 1} of 3", $"第 {DotsPager.SelectedPageIndex + 1} 张，共 3 张");
    }

    private void DotsCarouselChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_syncing || DotsCarousel.SelectedIndex < 0) return;
        _syncing = true;
        DotsPager.SelectedPageIndex = DotsCarousel.SelectedIndex;
        _syncing = false;
        DotsStatus.Text = L($"Slide {DotsCarousel.SelectedIndex + 1} of 3", $"第 {DotsCarousel.SelectedIndex + 1} 张，共 3 张");
    }

    private void VerticalPagerChanged(object? sender, PipsPagerSelectedIndexChangedEventArgs e)
    {
        VerticalStatus.Text = L($"Page {VerticalPager.SelectedPageIndex + 1} of 6", $"第 {VerticalPager.SelectedPageIndex + 1} 页，共 6 页");
    }
}
