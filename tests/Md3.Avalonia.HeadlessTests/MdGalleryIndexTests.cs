using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
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
    public void Gallery_Navigation_And_Overview_Are_Alphabetical_And_Numeric_Is_A_Control()
    {
        var window = new MainWindow { Width = 1280, Height = 860 };
        window.Show();
        try
        {
            var indexedTitles = window.IndexedPageTitles.ToArray();
            Assert.Equal(indexedTitles.OrderBy(title => title, StringComparer.OrdinalIgnoreCase), indexedTitles);
            Assert.Contains("Numeric input", indexedTitles);

            var allComponentsHeader = window.GetVisualDescendants().OfType<TextBlock>()
                .Single(text => text.Text == "ALL COMPONENTS");
            var sidebar = allComponentsHeader.GetVisualAncestors().OfType<StackPanel>()
                .First(stack => stack.Children.Contains(allComponentsHeader));
            var allComponentsStart = sidebar.Children.IndexOf(allComponentsHeader);
            var navigationTitles = sidebar.Children.Skip(allComponentsStart + 1)
                .OfType<MdButton>()
                .Select(button => button.Content?.ToString() ?? string.Empty)
                .ToArray();
            Assert.Equal(navigationTitles.OrderBy(title => title, StringComparer.OrdinalIgnoreCase), navigationTitles);
            Assert.Contains("Numeric input", navigationTitles);

            var communicationHeader = Assert.Single(sidebar.Children.OfType<TextBlock>(),
                text => text.Text == "COMMUNICATION");
            var communicationStart = sidebar.Children.IndexOf(communicationHeader);
            var libraryHeader = Assert.Single(sidebar.Children.OfType<TextBlock>(),
                text => text.Text == "LIBRARY EXTENSIONS");
            var libraryStart = sidebar.Children.IndexOf(libraryHeader);
            var communicationButtons = sidebar.Children.Skip(communicationStart + 1)
                .Take(libraryStart - communicationStart - 1).OfType<MdButton>().ToArray();
            Assert.Contains(communicationButtons, button => button.Content?.ToString() == "Notifications");
            var libraryButtons = sidebar.Children.Skip(libraryStart + 1)
                .Take(allComponentsStart - libraryStart - 1).OfType<MdButton>().ToArray();
            var libraryTitles = libraryButtons.Select(button => button.Content?.ToString() ?? string.Empty).ToArray();
            Assert.Equal(libraryTitles.OrderBy(title => title, StringComparer.OrdinalIgnoreCase), libraryTitles);
            Assert.DoesNotContain(libraryButtons, button => button.Name == "NumericNav");
            Assert.DoesNotContain(libraryButtons, button => button.Content?.ToString() == "Notifications");
        }
        finally { window.Close(); }

        var overview = new ComponentsOverviewGalleryPage();
        var host = new Window { Width = 1280, Height = 900, Content = overview };
        host.Show();
        try
        {
            var cards = overview.GetLogicalDescendants().OfType<Border>()
                .Where(border => border.Classes.Contains("gallery-section"))
                .ToArray();
            var categoryTitles = cards.Select(card => card.GetLogicalDescendants().OfType<TextBlock>()
                    .Single(text => text.Classes.Contains("h2")).Text ?? string.Empty)
                .ToArray();
            Assert.Equal(categoryTitles.OrderBy(title => title, StringComparer.OrdinalIgnoreCase), categoryTitles);

            foreach (var card in cards)
            {
                var links = card.GetLogicalDescendants().OfType<MdButton>()
                    .Select(button => button.Content?.ToString() ?? string.Empty).ToArray();
                Assert.Equal(links.OrderBy(title => title, StringComparer.OrdinalIgnoreCase), links);
            }

            var notificationsButton = overview.GetLogicalDescendants().OfType<MdButton>()
                .Single(button => button.Content?.ToString() == "Notifications");
            var notificationsCard = notificationsButton.GetLogicalAncestors().OfType<Border>()
                .Single(border => border.Classes.Contains("gallery-section"));
            Assert.Equal("Communication", notificationsCard.GetLogicalDescendants().OfType<TextBlock>()
                .Single(text => text.Classes.Contains("h2")).Text);

            var numericButton = overview.GetLogicalDescendants().OfType<MdButton>()
                .Single(button => button.Content?.ToString() == "Numeric input");
            var numericCard = numericButton.GetLogicalAncestors().OfType<Border>()
                .Single(border => border.Classes.Contains("gallery-section"));
            Assert.Equal("Text input", numericCard.GetLogicalDescendants().OfType<TextBlock>()
                .Single(text => text.Classes.Contains("h2")).Text);
        }
        finally { host.Close(); }
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
