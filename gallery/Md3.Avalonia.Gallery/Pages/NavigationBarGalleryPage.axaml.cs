using Avalonia.Controls;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class NavigationBarGalleryPage : UserControl
{
    private static readonly (string Title, string Description, string? Glyph)[] Destinations =
    [
        ("Home", "Home destination content is active.", MdSymbols.Home),
        ("Search", "Search destination content is active.", MdSymbols.Search),
        ("Favorites", "Favorites destination content is active.", MdSymbols.Favorite),
        ("Profile", "Profile destination content is active.", MdSymbols.Person)
    ];

    public NavigationBarGalleryPage() => InitializeComponent();

    private void DestinationChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not MdNavigationBar navigation ||
            DestinationTitle is null || DestinationDescription is null || DestinationIcon is null) return;
        var index = Math.Clamp(navigation.SelectedIndex, 0, Destinations.Length - 1);
        var destination = Destinations[index];
        DestinationTitle.Text = destination.Title;
        DestinationDescription.Text = destination.Description;
        DestinationIcon.Glyph = destination.Glyph;
    }
}
