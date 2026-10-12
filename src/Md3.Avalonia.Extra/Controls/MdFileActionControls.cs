using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>A reusable command description for file-selection and context menus.</summary>
public sealed record MdFileAction(
    string Id,
    string Label,
    string? IconGlyph = null,
    ICommand? Command = null,
    bool IsDestructive = false,
    bool IsEnabled = true);

/// <summary>Event raised when an action without an ICommand is activated.</summary>
public sealed class MdFileActionInvokedEventArgs(MdFileAction action, object? parameter) : EventArgs
{
    public MdFileAction Action { get; } = action;
    public object? Parameter { get; } = parameter;
}

/// <summary>Compact Material action row shown while one or more file entries are selected.</summary>
public sealed class MdFileSelectionToolbar : UserControl
{
    public static readonly StyledProperty<int> SelectedCountProperty =
        AvaloniaProperty.Register<MdFileSelectionToolbar, int>(nameof(SelectedCount));
    public static readonly StyledProperty<bool> IsBusyProperty =
        AvaloniaProperty.Register<MdFileSelectionToolbar, bool>(nameof(IsBusy));

    private readonly MdToolbar _toolbar;
    private readonly ObservableCollection<MdFileAction> _actions = [];

    static MdFileSelectionToolbar()
    {
        SelectedCountProperty.Changed.AddClassHandler<MdFileSelectionToolbar>((bar, _) => bar.Rebuild());
        IsBusyProperty.Changed.AddClassHandler<MdFileSelectionToolbar>((bar, _) => bar.Rebuild());
    }

    public MdFileSelectionToolbar()
    {
        _toolbar = new MdToolbar
        {
            Mode = MdToolbarMode.Docked,
            Density = MdToolbarDensity.Compact,
            Variant = MdToolbarVariant.Standard,
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };
        _actions.CollectionChanged += OnActionsChanged;
        Content = _toolbar;
        Rebuild();
    }

    public int SelectedCount { get => GetValue(SelectedCountProperty); set => SetValue(SelectedCountProperty, value); }
    public bool IsBusy { get => GetValue(IsBusyProperty); set => SetValue(IsBusyProperty, value); }
    public ObservableCollection<MdFileAction> Actions => _actions;

    public event EventHandler<MdFileActionInvokedEventArgs>? ActionInvoked;

    private void OnActionsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        if (_toolbar is null) return;
        _toolbar.Items.Clear();
        _toolbar.LeadingContent = new MdText
        {
            Text = SelectedCount == 1 ? "1 item selected" : $"{SelectedCount} items selected",
            StyleRole = MdTextStyle.TitleSmall,
            Margin = new Thickness(8, 0)
        };
        foreach (var action in _actions)
        {
            var button = new MdButton
            {
                Content = action.Label,
                Variant = action.IsDestructive ? MdButtonVariant.Text : MdButtonVariant.Tonal,
                IsEnabled = action.IsEnabled && !IsBusy,
                Command = action.Command,
                CommandParameter = SelectedCount
            };
            if (action.Command is null)
                button.Click += (_, _) => ActionInvoked?.Invoke(this, new MdFileActionInvokedEventArgs(action, SelectedCount));
            _toolbar.Items.Add(button);
        }
        IsVisible = SelectedCount > 0 && _actions.Count > 0;
    }
}

/// <summary>
/// Material menu for file actions. Use <see cref="ShowAt(Control)"/> to display it as a pointer
/// menu from mouse or touch context requests; the host continues to own provider operations.
/// </summary>
public sealed class MdFileContextMenu : UserControl
{
    private readonly MdMenu _menu;
    private readonly Popup _popup;
    private readonly ObservableCollection<MdFileAction> _actions = [];

    public MdFileContextMenu()
    {
        _menu = new MdMenu { MinWidth = 208 };
        _menu.DismissRequested += (_, _) => Dismiss();
        _popup = new Popup
        {
            Child = _menu,
            Placement = PlacementMode.Pointer,
            IsLightDismissEnabled = true
        };
        _actions.CollectionChanged += OnActionsChanged;
        Content = _popup;
    }

    public ObservableCollection<MdFileAction> Actions => _actions;
    public MdFileEntry? Entry { get; set; }
    public IReadOnlyList<MdFileEntry> Selection { get; set; } = Array.Empty<MdFileEntry>();
    public bool IsOpen => _popup.IsOpen;

    public event EventHandler<MdFileActionInvokedEventArgs>? ActionInvoked;

    public void ShowAt(Control target)
    {
        ArgumentNullException.ThrowIfNull(target);
        _popup.PlacementTarget = target;
        RebuildMenu();
        _popup.IsOpen = true;
        _menu.FocusFirstItem();
    }

    public void Dismiss() => _popup.IsOpen = false;

    private void OnActionsChanged(object? sender, NotifyCollectionChangedEventArgs e) => RebuildMenu();

    private void RebuildMenu()
    {
        _menu.Items.Clear();
        foreach (var action in _actions)
        {
            var item = new MdMenuItem
            {
                Content = action.Label,
                IsEnabled = action.IsEnabled,
                LeadingContent = action.IconGlyph is null ? null : new MdIcon { Glyph = action.IconGlyph },
                Command = action.Command,
                CommandParameter = Selection.Count == 0 ? Entry : Selection
            };
            if (action.Command is null)
                item.Click += (_, _) => ActionInvoked?.Invoke(this,
                    new MdFileActionInvokedEventArgs(action, Selection.Count == 0 ? Entry : Selection));
            _menu.Items.Add(item);
        }
    }

}
