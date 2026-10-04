using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Md3.Avalonia.Gallery.Pages;

public partial class RatingGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public RatingGalleryPage() => InitializeComponent();

    private void RatingChanged(object? sender, RangeBaseValueChangedEventArgs e) =>
        RatingStatus.Text = L($"Rating {e.NewValue:0.#} of 5", $"评分 {e.NewValue:0.#} / 5");
}
