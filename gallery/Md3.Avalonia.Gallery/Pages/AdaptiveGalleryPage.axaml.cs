using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class AdaptiveGalleryPage : UserControl
{
    private static readonly (string Title, string Description)[] Destinations =
    [
        ("Home", "Home summary and recent activity are active."),
        ("Search", "Search tools and saved queries are active."),
        ("Settings", "Application and account settings are active.")
    ];

    private int _suiteDestination;

    public AdaptiveGalleryPage() => InitializeComponent();

    private void RailSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not MdNavigationRail rail || RailDestinationTitle is null || RailDestinationDescription is null) return;
        var index = Math.Clamp(rail.SelectedIndex, 0, Destinations.Length - 1);
        RailDestinationTitle.Text = Destinations[index].Title;
        RailDestinationDescription.Text = Destinations[index].Description;
    }

    private void SetSuiteWidth(object? sender, RoutedEventArgs e)
    {
        if ((sender as Control)?.Tag?.ToString() is not { } text || !double.TryParse(text, out var width)) return;
        // The page scrolls vertically only: a medium/expanded width beyond the viewport used to
        // clip on phones. Clamp to what is actually visible, and lift the XAML MaxWidth so the
        // expanded preset can really reach 1040 on wide screens.
        var available = Bounds.Width > 0 ? Bounds.Width - 8 : width;
        DemoSuite.MaxWidth = double.PositiveInfinity;
        DemoSuite.Width = Math.Max(320, Math.Min(width, available));
        Dispatcher.UIThread.Post(UpdateSuiteStatus, DispatcherPriority.Loaded);
    }

    private void SuiteDestinationChanged(object? sender, SelectionChangedEventArgs e)
    {
        _suiteDestination = sender switch
        {
            MdNavigationBar bar => Math.Max(0, bar.SelectedIndex),
            MdNavigationRail rail => Math.Max(0, rail.SelectedIndex),
            _ => _suiteDestination
        };
        UpdateSuiteContent();
    }

    private void SuiteDrawerDestinationChanged(object? sender, RoutedEventArgs e)
    {
        if ((sender as Control)?.Tag?.ToString() is { } text && int.TryParse(text, out var index))
        {
            _suiteDestination = Math.Clamp(index, 0, Destinations.Length - 1);
            UpdateSuiteContent();
        }
    }

    private void UpdateSuiteContent()
    {
        if (SuiteDestinationTitle is null || SuiteDestinationDescription is null || DemoSuite is null || SuiteStatus is null) return;
        var destination = Destinations[Math.Clamp(_suiteDestination, 0, Destinations.Length - 1)];
        SuiteDestinationTitle.Text = destination.Title;
        SuiteDestinationDescription.Text = $"{destination.Description} Simulated viewport: {DemoSuite.Width:0} dp.";
        UpdateSuiteStatus();
    }

    private void UpdateSuiteStatus()
    {
        if (DemoSuite is null || SuiteStatus is null) return;
        var label = DemoSuite.Width >= DemoSuite.DrawerBreakpoint ? "expanded · navigation drawer"
            : DemoSuite.Width >= DemoSuite.RailBreakpoint ? "medium · navigation rail"
            : "compact · navigation bar";
        SuiteStatus.Text = $"{label} · {Destinations[Math.Clamp(_suiteDestination, 0, Destinations.Length - 1)].Title} destination.";
    }
}
