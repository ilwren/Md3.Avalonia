using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Gallery.Pages;

public partial class DraggableTabsGalleryPage : UserControl
{
    // TabsControl mutates ItemsSource directly (reorder, close, tear out, re-attach), so the
    // collection has to implement IList - ObservableCollection does.
    private readonly ObservableCollection<DemoTab> _tabs = [.. DemoTab.Create("Overview", "Tokens", "Motion")];
    private readonly ObservableCollection<DemoTab> _pinnedTabs = [.. DemoTab.Create("Pinned", "Notes")];

    public DraggableTabsGalleryPage()
    {
        InitializeComponent();

        MainTabs.ItemsSource = _tabs;
        MainTabs.NewItemFactory = () => new DemoTab($"Tab {_tabs.Count + 1}", "Added from the strip's own add button.");
        PinnedTabs.ItemsSource = _pinnedTabs;

        // TabsControl does not select anything by itself, and a strip with nothing selected shows an
        // empty content host.
        MainTabs.SelectedIndex = 0;
        PinnedTabs.SelectedIndex = 0;
    }

    // The same command the strip's own add button invokes, so the demo button and the built-in one
    // cannot drift apart.
    private void AddTab(object? sender, RoutedEventArgs e) => MainTabs.AddItemCommand.Execute(null);

    // Closing removes the item from the collection the strip is bound to; that is all TabsControl's
    // own close button does, minus the container bookkeeping.
    private void CloseSelectedTab(object? sender, RoutedEventArgs e)
    {
        if (MainTabs.SelectedItem is DemoTab tab && _tabs.Count > 1)
        {
            _tabs.Remove(tab);
        }
    }
}
