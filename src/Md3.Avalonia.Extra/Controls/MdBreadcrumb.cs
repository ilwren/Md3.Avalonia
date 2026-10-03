using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>An individual breadcrumb item model with support for icons, commands, links, and selection state.</summary>
public class MdBreadcrumbItem : AvaloniaObject
{
    public static readonly StyledProperty<object?> LabelProperty =
        AvaloniaProperty.Register<MdBreadcrumbItem, object?>(nameof(Label));
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<MdBreadcrumbItem, object?>(nameof(Icon));
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<MdBreadcrumbItem, ICommand?>(nameof(Command));
    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<MdBreadcrumbItem, object?>(nameof(CommandParameter));
    public static readonly StyledProperty<string?> HrefProperty =
        AvaloniaProperty.Register<MdBreadcrumbItem, string?>(nameof(Href));
    public static readonly StyledProperty<bool> IsCurrentProperty =
        AvaloniaProperty.Register<MdBreadcrumbItem, bool>(nameof(IsCurrent));
    public static readonly StyledProperty<bool> IsEnabledProperty =
        AvaloniaProperty.Register<MdBreadcrumbItem, bool>(nameof(IsEnabled), true);

    public object? Label { get => GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public object? Icon { get => GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public ICommand? Command { get => GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
    public object? CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }
    public string? Href { get => GetValue(HrefProperty); set => SetValue(HrefProperty, value); }
    public bool IsCurrent { get => GetValue(IsCurrentProperty); set => SetValue(IsCurrentProperty, value); }
    public bool IsEnabled { get => GetValue(IsEnabledProperty); set => SetValue(IsEnabledProperty, value); }

    public override string ToString() => Label?.ToString() ?? string.Empty;
}

/// <summary>
/// A desktop and touch breadcrumb path supporting rich item models, icons, custom separators,
/// overflow collapsing, MVVM commands, and direct selection APIs aligned with Flutter ecosystem patterns.
/// </summary>
public sealed class MdBreadcrumb : ListBox
{
    private bool _invoking;
    private bool _updatingOverflow;
    private bool _isOverflowExpanded;

    public static readonly StyledProperty<object?> SeparatorProperty =
        AvaloniaProperty.Register<MdBreadcrumb, object?>(nameof(Separator), "›");
    public static readonly StyledProperty<ICommand?> ItemInvokedCommandProperty =
        AvaloniaProperty.Register<MdBreadcrumb, ICommand?>(nameof(ItemInvokedCommand));
    public static readonly StyledProperty<int> MaxDisplayedItemsProperty =
        AvaloniaProperty.Register<MdBreadcrumb, int>(nameof(MaxDisplayedItems), 0);
    public static readonly StyledProperty<int> ItemsBeforeCollapseProperty =
        AvaloniaProperty.Register<MdBreadcrumb, int>(nameof(ItemsBeforeCollapse), 1);
    public static readonly StyledProperty<int> ItemsAfterCollapseProperty =
        AvaloniaProperty.Register<MdBreadcrumb, int>(nameof(ItemsAfterCollapse), 1);
    public static readonly StyledProperty<object?> OverflowContentProperty =
        AvaloniaProperty.Register<MdBreadcrumb, object?>(nameof(OverflowContent), "…");
    public static readonly DirectProperty<MdBreadcrumb, bool> IsOverflowExpandedProperty =
        AvaloniaProperty.RegisterDirect<MdBreadcrumb, bool>(nameof(IsOverflowExpanded), control => control.IsOverflowExpanded);

    static MdBreadcrumb()
    {
        SeparatorProperty.Changed.AddClassHandler<MdBreadcrumb>((breadcrumb, _) => breadcrumb.UpdateItemContainers());
        MaxDisplayedItemsProperty.Changed.AddClassHandler<MdBreadcrumb>((breadcrumb, _) => breadcrumb.ResetOverflow());
        ItemsBeforeCollapseProperty.Changed.AddClassHandler<MdBreadcrumb>((breadcrumb, _) => breadcrumb.ResetOverflow());
        ItemsAfterCollapseProperty.Changed.AddClassHandler<MdBreadcrumb>((breadcrumb, _) => breadcrumb.ResetOverflow());
        OverflowContentProperty.Changed.AddClassHandler<MdBreadcrumb>((breadcrumb, _) => breadcrumb.UpdateItemContainers());
    }

    public MdBreadcrumb()
    {
        AutomationProperties.SetName(this, "Breadcrumb");
        SelectionChanged += OnSelectionChanged;
        LayoutUpdated += (_, _) => UpdateOverflowOnly();
    }

    public object? Separator { get => GetValue(SeparatorProperty); set => SetValue(SeparatorProperty, value); }
    public ICommand? ItemInvokedCommand { get => GetValue(ItemInvokedCommandProperty); set => SetValue(ItemInvokedCommandProperty, value); }
    public int MaxDisplayedItems { get => GetValue(MaxDisplayedItemsProperty); set => SetValue(MaxDisplayedItemsProperty, value); }
    public int ItemsBeforeCollapse { get => GetValue(ItemsBeforeCollapseProperty); set => SetValue(ItemsBeforeCollapseProperty, value); }
    public int ItemsAfterCollapse { get => GetValue(ItemsAfterCollapseProperty); set => SetValue(ItemsAfterCollapseProperty, value); }
    public object? OverflowContent { get => GetValue(OverflowContentProperty); set => SetValue(OverflowContentProperty, value); }
    public bool IsOverflowExpanded
    {
        get => _isOverflowExpanded;
        private set => SetAndRaise(IsOverflowExpandedProperty, ref _isOverflowExpanded, value);
    }

    public event EventHandler<object?>? ItemInvoked;

    public void ExpandOverflow()
    {
        if (IsOverflowExpanded) return;
        IsOverflowExpanded = true;
        UpdateItemContainers();
    }

    public void CollapseOverflow()
    {
        if (!IsOverflowExpanded) return;
        IsOverflowExpanded = false;
        UpdateItemContainers();
    }

    public void Invoke(object? item)
    {
        if (item is null) return;
        _invoking = true;
        try { SelectedItem = item; }
        finally { _invoking = false; }
        RaiseItemInvoked(item);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is not ListBoxItem listItem) return;

        var model = item as MdBreadcrumbItem;
        var isLast = index == ItemCount - 1;
        var isCurrent = model?.IsCurrent ?? isLast;
        GetOverflowState(index, out var isVisible, out var isOverflow);

        // Classes collection entries never include the ':' selector prefix. Using pseudo-class
        // syntax here meant none of the breadcrumb item styles could match at runtime.
        listItem.Content = isOverflow ? OverflowContent : model?.Label ?? item;
        listItem.Classes.Set("first", index == 0);
        listItem.Classes.Set("last", isLast);
        listItem.Classes.Set("has-icon", !isOverflow && model?.Icon is not null);
        listItem.Classes.Set("current", !isOverflow && isCurrent);
        listItem.Classes.Set("overflow", isOverflow);
        listItem.IsVisible = isVisible;
        listItem.IsEnabled = isOverflow || model?.IsEnabled != false;
        KeyboardNavigation.SetIsTabStop(listItem, isOverflow || !isCurrent);
        AutomationProperties.SetName(listItem, isOverflow ? "Show full breadcrumb path" : (model?.Label ?? item)?.ToString());
        AutomationProperties.SetHelpText(listItem, isCurrent ? "Current page" : $"Item {index + 1} of {ItemCount}");

        // The package supplies the complete ListBoxItem theme, so instantiate it before resolving
        // named parts. This also makes rich breadcrumb content visible on the first layout pass.
        listItem.ApplyTemplate();

        if (listItem.GetVisualDescendants().OfType<TextBlock>()
            .FirstOrDefault(text => text.Name == "PART_Separator") is { } separatorText)
        {
            separatorText.Text = Separator?.ToString() ?? "›";
            separatorText.IsVisible = !isLast;
        }

        if (listItem.GetVisualDescendants().OfType<MdSymbolPresenter>()
            .FirstOrDefault(presenter => presenter.Name == "PART_IconPresenter") is { } iconPresenter)
        {
            iconPresenter.Content = isOverflow ? null : model?.Icon;
        }
    }

    private void ResetOverflow()
    {
        IsOverflowExpanded = false;
        UpdateItemContainers();
    }

    private void UpdateOverflowOnly()
    {
        if (_updatingOverflow) return;
        _updatingOverflow = true;
        try
        {
            foreach (var container in GetRealizedContainers().OfType<ListBoxItem>())
            {
                var index = IndexFromContainer(container);
                if (index < 0) continue;
                GetOverflowState(index, out var isVisible, out var isOverflow);
                if (container.IsVisible != isVisible || container.Classes.Contains("overflow") != isOverflow)
                    PrepareContainerForItemOverride(container, ItemFromContainer(container), index);
            }
        }
        finally { _updatingOverflow = false; }
    }

    private void UpdateItemContainers()
    {
        if (_updatingOverflow) return;
        _updatingOverflow = true;
        try
        {
            foreach (var container in GetRealizedContainers().ToArray())
            {
                var index = IndexFromContainer(container);
                if (index >= 0) PrepareContainerForItemOverride(container, ItemFromContainer(container), index);
            }
        }
        finally { _updatingOverflow = false; }
    }

    private void GetOverflowState(int index, out bool visible, out bool overflow)
    {
        visible = true;
        overflow = false;
        if (IsOverflowExpanded || MaxDisplayedItems <= 0 || ItemCount <= MaxDisplayedItems) return;

        var maximum = Math.Max(2, MaxDisplayedItems);
        var before = Math.Clamp(ItemsBeforeCollapse, 0, Math.Max(0, maximum - 2));
        var after = Math.Clamp(ItemsAfterCollapse, 1, Math.Max(1, maximum - before - 1));
        if (before + after + 1 > maximum) before = Math.Max(0, maximum - after - 1);
        var overflowIndex = before;
        overflow = index == overflowIndex;
        visible = index < before || overflow || index >= ItemCount - after;
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_invoking || SelectedIndex < 0 || SelectedItem is not { } item) return;
        if (ContainerFromIndex(SelectedIndex) is ListBoxItem container && container.Classes.Contains("overflow"))
        {
            _invoking = true;
            try { SelectedIndex = -1; }
            finally { _invoking = false; }
            ExpandOverflow();
            return;
        }
        if (item is MdBreadcrumbItem { IsCurrent: true } || SelectedIndex == ItemCount - 1) return;
        RaiseItemInvoked(item);
    }

    private void RaiseItemInvoked(object item)
    {
        if (item is MdBreadcrumbItem model)
        {
            if (model.Command?.CanExecute(model.CommandParameter) == true)
                model.Command.Execute(model.CommandParameter);
            else if (Uri.TryCreate(model.Href, UriKind.Absolute, out var uri) && TopLevel.GetTopLevel(this)?.Launcher is { } launcher)
                _ = launcher.LaunchUriAsync(uri);
        }

        if (ItemInvokedCommand?.CanExecute(item) == true)
            ItemInvokedCommand.Execute(item);

        ItemInvoked?.Invoke(this, item);
    }
}
