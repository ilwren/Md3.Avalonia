using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class SlidableItemGalleryPage : UserControl
{
    public SlidableItemGalleryPage()
    {
        InitializeComponent();
        if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
        {
            // Touch devices have no modifier keys; pointing at Ctrl shortcuts there is noise.
            if (KeyboardHint is not null) KeyboardHint.IsVisible = false;
            if (SlidableDescription is not null)
            {
                SlidableDescription.Text = GalleryLocalization.Choose(
                    "A list row that reveals actions when swiped either way.",
                    "向任意方向水平滑动即可展开操作的列表行。");
            }
        }
    }

    private void CloseSlidable(object? sender, RoutedEventArgs e) => Slidable.Close();
}
