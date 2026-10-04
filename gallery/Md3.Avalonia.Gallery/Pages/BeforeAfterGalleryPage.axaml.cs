using Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class BeforeAfterGalleryPage : UserControl
{
    public BeforeAfterGalleryPage()
    {
        InitializeComponent();

        // The page used to wipe between the words "BEFORE" and "AFTER", which never read as an
        // image comparison at all. Two grades of one photograph make the divider legible, and the
        // readout shows Position is a real two-way value and not just a drag gesture.
        Update();
        PhotoCompare.PositionChanged += (_, _) => Update();
    }

    private void Update() => PositionValue.Text = $"Position = {PhotoCompare.Position:0.00}";
}
