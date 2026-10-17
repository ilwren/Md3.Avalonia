using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class WaterfallGalleryPage : UserControl
{
    public WaterfallGalleryPage()
    {
        InitializeComponent();
        PopulateFeed();
    }

    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    private void PopulateFeed()
    {
        // Built as controls rather than bound through an ItemTemplate: the gallery is published
        // natively by the AOT probe, and a template binding against a view model's properties is
        // reflection the trimmer is under no obligation to keep. A layout demo does not need it.
        double[] heights = { 150, 210, 120, 180, 240, 160, 130, 200, 110 };
        MdCardVariant[] variants = { MdCardVariant.Elevated, MdCardVariant.Filled, MdCardVariant.Outlined };

        for (var index = 0; index < heights.Length; index++)
        {
            Feed.Items.Add(new MdCard
            {
                Variant = variants[index % variants.Length],
                Height = heights[index],
                Padding = new Thickness(14),
                Content = new TextBlock
                {
                    Text = $"{L("Item", "条目")} {index + 1}",
                    TextWrapping = TextWrapping.Wrap,
                    Classes = { "body" }
                }
            });
        }
    }
}
