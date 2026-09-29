using System.Collections;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Ecosystem.Controls;

/// <summary>A desktop/touch breadcrumb path with binding, command, event, and direct selection APIs.</summary>
public sealed class MdBreadcrumb : ListBox
{
    private bool _invoking;
    public MdBreadcrumb() => SelectionChanged += OnSelectionChanged;
    public static readonly StyledProperty<object?> SeparatorProperty = AvaloniaProperty.Register<MdBreadcrumb, object?>(nameof(Separator), "›");
    public static readonly StyledProperty<ICommand?> ItemInvokedCommandProperty = AvaloniaProperty.Register<MdBreadcrumb, ICommand?>(nameof(ItemInvokedCommand));
    public object? Separator { get => GetValue(SeparatorProperty); set => SetValue(SeparatorProperty, value); }
    public ICommand? ItemInvokedCommand { get => GetValue(ItemInvokedCommandProperty); set => SetValue(ItemInvokedCommandProperty, value); }
    public event EventHandler<object?>? ItemInvoked;

    public void Invoke(object? item)
    {
        if (item is null) return;
        _invoking = true;
        try { SelectedItem = item; }
        finally { _invoking = false; }
        RaiseItemInvoked(item);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_invoking && SelectedItem is { } item) RaiseItemInvoked(item);
    }

    private void RaiseItemInvoked(object item)
    {
        if (ItemInvokedCommand?.CanExecute(item) == true) ItemInvokedCommand.Execute(item);
        ItemInvoked?.Invoke(this, item);
    }
}
