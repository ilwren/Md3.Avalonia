using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Md3.Avalonia.Localization;

namespace Md3.Avalonia.Controls;

/// <summary>Creates a localized Material editing menu without replacing Avalonia TextBox globally.</summary>
internal static class MdTextEditingContextMenu
{
    public static void Attach(TextBox textBox)
    {
        if (textBox.ContextMenu is not null) return;

        var undo = CreateItem(() => textBox.Undo(), new KeyGesture(Key.Z, KeyModifiers.Control));
        var redo = CreateItem(() => textBox.Redo(), new KeyGesture(Key.Y, KeyModifiers.Control));
        var cut = CreateItem(() => textBox.Cut(), new KeyGesture(Key.X, KeyModifiers.Control));
        var copy = CreateItem(() => textBox.Copy(), new KeyGesture(Key.C, KeyModifiers.Control));
        var paste = CreateItem(() => textBox.Paste(), new KeyGesture(Key.V, KeyModifiers.Control));
        var selectAll = CreateItem(textBox.SelectAll, new KeyGesture(Key.A, KeyModifiers.Control));
        var menu = new ContextMenu
        {
            Items = { undo, redo, new Separator(), cut, copy, paste, new Separator(), selectAll }
        };

        TopLevel? dismissRoot = null;
        void DismissOnOutsidePress(object? sender, PointerPressedEventArgs args)
        {
            // Popup events are routed to their owning TopLevel. IsPointerOver stays true for
            // presses within the menu's PopupRoot, so only an actual outside press dismisses it.
            if (menu.IsOpen && !menu.IsPointerOver)
            {
                menu.Close();
            }
        }

        menu.Opening += (_, _) =>
        {
            undo.Header = MdLocalization.GetString("Undo", textBox);
            redo.Header = MdLocalization.GetString("Redo", textBox);
            cut.Header = MdLocalization.GetString("Cut", textBox);
            copy.Header = MdLocalization.GetString("Copy", textBox);
            paste.Header = MdLocalization.GetString("Paste", textBox);
            selectAll.Header = MdLocalization.GetString("SelectAll", textBox);

            undo.IsEnabled = !textBox.IsReadOnly && textBox.CanUndo;
            redo.IsEnabled = !textBox.IsReadOnly && textBox.CanRedo;
            var hasSelection = textBox.SelectionStart != textBox.SelectionEnd;
            cut.IsEnabled = !textBox.IsReadOnly && hasSelection;
            copy.IsEnabled = hasSelection;
            paste.IsEnabled = !textBox.IsReadOnly;
            selectAll.IsEnabled = !string.IsNullOrEmpty(textBox.Text);

            if (ResourceNodeExtensions.FindResource(textBox, "MdTextContextMenuTheme") is ControlTheme menuTheme)
                menu.Theme = menuTheme;
            if (ResourceNodeExtensions.FindResource(textBox, "MdTextContextMenuItemTheme") is ControlTheme itemTheme)
            {
                foreach (var item in menu.Items.OfType<MenuItem>()) item.Theme = itemTheme;
            }
            if (ResourceNodeExtensions.FindResource(textBox, "MdTextContextMenuSeparatorTheme") is ControlTheme separatorTheme)
            {
                foreach (var separator in menu.Items.OfType<Separator>()) separator.Theme = separatorTheme;
            }
        };
        menu.Opened += (_, _) =>
        {
            if (TopLevel.GetTopLevel(menu) is PopupRoot popupRoot)
            {
                popupRoot.Background = Brushes.Transparent;
                popupRoot.TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
                popupRoot.TransparencyBackgroundFallback = menu.Background ?? Brushes.Transparent;
            }
            dismissRoot = TopLevel.GetTopLevel(textBox);
            dismissRoot?.AddHandler(InputElement.PointerPressedEvent, DismissOnOutsidePress,
                RoutingStrategies.Tunnel, handledEventsToo: true);
        };
        menu.Closed += (_, _) =>
        {
            dismissRoot?.RemoveHandler(InputElement.PointerPressedEvent, DismissOnOutsidePress);
            dismissRoot = null;
        };

        textBox.ContextMenu = menu;
    }

    private static MenuItem CreateItem(Action action, KeyGesture gesture)
    {
        var item = new MenuItem { InputGesture = gesture };
        item.Click += (_, _) => action();
        return item;
    }
}
