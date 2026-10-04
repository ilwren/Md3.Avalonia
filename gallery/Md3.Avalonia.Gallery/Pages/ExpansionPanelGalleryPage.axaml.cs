using Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

// The panels are self-contained: MdExpansionPanelList owns the single/multi expansion policy,
// so this page has no handlers of its own.
public partial class ExpansionPanelGalleryPage : UserControl
{
    public ExpansionPanelGalleryPage() => InitializeComponent();
}
