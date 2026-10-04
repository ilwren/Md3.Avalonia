using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.Gallery.Pages;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// The components overview is a hand-written index that links to pages by title, so it silently
/// rots whenever a page is renamed, split, or added. Defect #12 split five crowded pages into
/// forty-five, and two of the overview's links were already pointing at pages that no longer
/// existed. These tests make that a build failure instead of a dead button.
/// </summary>
public sealed class MdGalleryIndexTests
{
    [AvaloniaFact]
    public void Components_Overview_Links_Resolve_And_Cover_Every_Indexed_Page()
    {
        var window = new MainWindow();
        window.Show();
        string[] indexed;
        try
        {
            indexed = [.. window.IndexedPageTitles];
        }
        finally { window.Close(); }

        Assert.NotEmpty(indexed);
        Assert.Equal(indexed.Length, indexed.Distinct(StringComparer.Ordinal).Count());

        var overview = new ComponentsOverviewGalleryPage();
        var host = new Window { Width = 1280, Height = 900, Content = overview };
        host.Show();
        string[] links;
        try
        {
            links = [.. overview.GetLogicalDescendants().OfType<MdButton>()
                .Select(button => button.Tag as string)
                .Where(tag => !string.IsNullOrEmpty(tag))
                .Select(tag => tag!)];
        }
        finally { host.Close(); }

        Assert.Equal(links.Length, links.Distinct(StringComparer.Ordinal).Count());
        Assert.Empty(links.Except(indexed, StringComparer.Ordinal));
        Assert.Empty(indexed.Except(links, StringComparer.Ordinal));
    }

    [AvaloniaFact]
    public void Every_Overview_Link_Navigates_To_Its_Page()
    {
        var window = new MainWindow { Width = 1280, Height = 860 };
        window.Show();
        try
        {
            var overview = new ComponentsOverviewGalleryPage();
            var probe = new Window { Width = 1280, Height = 900, Content = overview };
            probe.Show();
            var titles = overview.GetLogicalDescendants().OfType<MdButton>()
                .Select(button => button.Tag as string)
                .Where(tag => !string.IsNullOrEmpty(tag))
                .Select(tag => tag!)
                .ToArray();
            probe.Close();

            // NavigateToIndexedPage matches on the title with ordinal equality, so a title that
            // resolves here is exactly a link that works in the running gallery.
            Assert.All(titles, title => Assert.True(window.NavigateToIndexedPage(title),
                $"The components overview links to \"{title}\", which is not in the gallery index."));
        }
        finally { window.Close(); }
    }
}
