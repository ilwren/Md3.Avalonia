using Avalonia;
using Avalonia.Controls;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class RangeSliderGalleryPage : UserControl
{
    public RangeSliderGalleryPage() => InitializeComponent();

    private void RangePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is MdRangeSlider slider && RangeValueText is not null &&
            (e.Property == MdRangeSlider.LowerValueProperty || e.Property == MdRangeSlider.UpperValueProperty))
            RangeValueText.Text = $"{slider.LowerValue:0} — {slider.UpperValue:0}";
    }
}
