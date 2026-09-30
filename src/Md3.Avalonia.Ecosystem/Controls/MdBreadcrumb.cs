using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
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

namespace Md3.Avalonia.Ecosystem.Controls;

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

    static MdBreadcrumb()
    {
        SeparatorProperty.Changed.AddClassHandler<MdBreadcrumb>((breadcrumb, _) => breadcrumb.RefreshContainers());
        MaxDisplayedItemsProperty.Changed.AddClassHandler<MdBreadcrumb>((breadcrumb, _) => breadcrumb.RefreshContainers());
    }

    public MdBreadcrumb()
    {
        SelectionChanged += OnSelectionChanged;
    }

    public object? Separator { get => GetValue(SeparatorProperty); set => SetValue(SeparatorProperty, value); }
    public ICommand? ItemInvokedCommand { get => GetValue(ItemInvokedCommandProperty); set => SetValue(ItemInvokedCommandProperty, value); }
    public int MaxDisplayedItems { get => GetValue(MaxDisplayedItemsProperty); set => SetValue(MaxDisplayedItemsProperty, value); }
    public int ItemsBeforeCollapse { get => GetValue(ItemsBeforeCollapseProperty); set => SetValue(ItemsBeforeCollapseProperty, value); }
    public int ItemsAfterCollapse { get => GetValue(ItemsAfterCollapseProperty); set => SetValue(ItemsAfterCollapseProperty, value); }

    public event EventHandler<object?>? ItemInvoked;

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
        if (container is ListBoxItem listItem)
        {
            var isLast = index == ItemCount - 1;
            var isFirst = index == 0;
            listItem.Classes.Set(":last", isLast);
            listItem.Classes.Set(":first", isFirst);

            if (item is MdBreadcrumbItem model)
            {
                listItem.Classes.Set(":has-icon", model.Icon is not null);
                listItem.Classes.Set(":current", model.IsCurrent);
                listItem.IsEnabled = model.IsEnabled;
            }
            else
            {
                listItem.Classes.Set(":has-icon", false);
                listItem.Classes.Set(":current", isLast);
            }

            if (listItem.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault(t => t.Name == "PART_Separator") is { } separatorText)
            {
                separatorText.Text = Separator?.ToString() ?? "›";
                separatorText.IsVisible = !isLast;
            }

            if (item is MdBreadcrumbItem { Icon: { } icon } &&
                listItem.GetVisualDescendants().OfType<MdSymbolPresenter>().FirstOrDefault(p => p.Name == "PART_IconPresenter") is { } iconPresenter)
            {
                iconPresenter.Content = icon;
            }
        }
    }

    private void RefreshContainers()
    {
        var containers = GetRealizedContainers().ToArray();
        for (var i = 0; i < containers.Length; i++)
        {
            var container = containers[i];
            var index = IndexFromContainer(container);
            if (index >= 0)
            {
                var item = ItemFromContainer(container);
                PrepareContainerForItemOverride(container, item, index);
            }
        }
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_invoking && SelectedItem is { } item) RaiseItemInvoked(item);
    }

    private void RaiseItemInvoked(object item)
    {
        if (item is MdBreadcrumbItem model)
        {
            if (model.Command?.CanExecute(model.CommandParameter) == true)
                model.Command.Execute(model.CommandParameter);
        }

        if (ItemInvokedCommand?.CanExecute(item) == true)
            ItemInvokedCommand.Execute(item);

        ItemInvoked?.Invoke(this, item);
    }
}
