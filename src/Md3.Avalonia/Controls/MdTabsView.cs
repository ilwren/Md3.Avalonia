using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

/// <summary>
/// The content area for an <see cref="MdTabs"/> bar: its children are the pages, matched to the
/// bar's tabs by position, and only the selected one is shown.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MdTabs"/> is a bar and nothing else, because Material places it in app bars and
/// headers where the pages cannot live alongside it. This is the other half of that pair, in the
/// same shape as Flutter's <c>TabBar</c> and <c>TabBarView</c>:
/// </para>
/// <code><![CDATA[
/// <md:MdTabs x:Name="Sections">
///   <md:MdTabItem Content="Flights" />
///   <md:MdTabItem Content="Trips" />
/// </md:MdTabs>
/// <md:MdTabsView Tabs="{Binding #Sections}">
///   <Border>Flights page</Border>
///   <Border>Trips page</Border>
/// </md:MdTabsView>
/// ]]></code>
/// <para>
/// Selection is kept in step in both directions, so paging from code moves the bar too. Use
/// <see cref="MdTabView"/> instead when the tabs and their content sit together.
/// </para>
/// </remarks>
public sealed class MdTabsView : TabControl
{
    /// <summary>The tab bar driving this content area.</summary>
    public static readonly StyledProperty<MdTabs?> TabsProperty =
        AvaloniaProperty.Register<MdTabsView, MdTabs?>(nameof(Tabs));

    private MdTabs? _subscribed;

    static MdTabsView()
    {
        TabsProperty.Changed.AddClassHandler<MdTabsView>((view, e) =>
            view.Rebind(e.OldValue as MdTabs, e.NewValue as MdTabs));
        SelectedIndexProperty.Changed.AddClassHandler<MdTabsView>((view, _) => view.PushToBar());
    }

    public MdTabs? Tabs { get => GetValue(TabsProperty); set => SetValue(TabsProperty, value); }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // A {Binding #Name} target can resolve before this view is in the tree, so take the
        // bar's current tab on attach rather than waiting for the next selection change.
        Rebind(null, Tabs);
        PullFromBar();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Rebind(_subscribed, null);
        base.OnDetachedFromVisualTree(e);
    }

    private void Rebind(MdTabs? oldTabs, MdTabs? newTabs)
    {
        if (ReferenceEquals(_subscribed, newTabs)) return;
        if (_subscribed is not null) _subscribed.SelectionChanged -= OnBarSelectionChanged;
        if (oldTabs is not null && !ReferenceEquals(oldTabs, _subscribed)) oldTabs.SelectionChanged -= OnBarSelectionChanged;
        _subscribed = newTabs;
        if (_subscribed is not null) _subscribed.SelectionChanged += OnBarSelectionChanged;
        PullFromBar();
    }

    private void OnBarSelectionChanged(object? sender, SelectionChangedEventArgs e) => PullFromBar();

    private void PullFromBar()
    {
        if (_subscribed is null) return;
        var index = _subscribed.SelectedIndex;
        // A bar with more tabs than this view has pages must not blank the page that is showing.
        if (index < 0 || index >= ItemCount || index == SelectedIndex) return;
        SetCurrentValue(SelectedIndexProperty, index);
    }

    private void PushToBar()
    {
        // Comparing before assigning is what stops the two controls echoing each other; a
        // suppression flag would also have to survive re-entrancy from the bar's own animation.
        if (_subscribed is null || SelectedIndex < 0 || SelectedIndex >= _subscribed.ItemCount) return;
        if (_subscribed.SelectedIndex == SelectedIndex) return;
        _subscribed.SetCurrentValue(SelectingItemsControl.SelectedIndexProperty, SelectedIndex);
    }
}
