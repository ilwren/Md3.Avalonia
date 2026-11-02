using Avalonia.Controls;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery;

internal static class GalleryDialogHosts
{
    internal static MdDialogHost? Find(Control control)
    {
        if (TopLevel.GetTopLevel(control) is MainWindow window)
            return window.DialogHost;

        return control.GetVisualAncestors()
            .OfType<AndroidGalleryView>()
            .FirstOrDefault()
            ?.DialogHost;
    }
}
