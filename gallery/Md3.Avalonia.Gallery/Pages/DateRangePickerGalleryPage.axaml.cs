using Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class DateRangePickerGalleryPage : UserControl
{
    public DateRangePickerGalleryPage() => InitializeComponent();

    // Both ends are bindable; the control validates start <= end itself.
}
