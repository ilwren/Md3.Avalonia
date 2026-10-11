using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Md3.Avalonia.Controls;

internal static class MdMediaControlHelpers
{
    private const string EmptyGlyph = "\u200B";

    public static void SetIcon(StyledElement owner, MdIconButton? button, string resourceKey, string name)
    {
        if (button is null) return;
        var glyph = Application.Current?.TryGetResource(resourceKey, owner.ActualThemeVariant, out var value) == true
            ? value
            : EmptyGlyph;
        button.SetCurrentValue(MdIconButton.IconProperty, glyph);
        AutomationProperties.SetName(button, name);
        ToolTip.SetTip(button, name);
    }

    public static void RequestPlayback(
        StyledElement owner,
        ICommand? command,
        EventHandler<MdPlaybackRequestEventArgs>? handler,
        MdPlaybackRequest request)
    {
        if (command?.CanExecute(request) == true)
            command.Execute(request);
        handler?.Invoke(owner, new MdPlaybackRequestEventArgs(request));
    }

    public static void RequestSeek(
        StyledElement owner,
        ICommand? command,
        EventHandler<MdSeekRequestedEventArgs>? handler,
        TimeSpan position)
    {
        if (command?.CanExecute(position) == true)
            command.Execute(position);
        handler?.Invoke(owner, new MdSeekRequestedEventArgs(position));
    }

    public static void RequestOpen(
        StyledElement owner,
        ICommand? command,
        EventHandler? handler)
    {
        if (command?.CanExecute(null) == true)
            command.Execute(null);
        handler?.Invoke(owner, EventArgs.Empty);
    }

    public static bool IsInsideInteractiveControl(object? source, Control owner)
    {
        if (source is not Visual visual) return false;
        for (Visual? current = visual; current is not null && !ReferenceEquals(current, owner); current = current.GetVisualParent())
            if (current is Button or Slider or MdSeekBar)
                return true;
        return false;
    }
}
