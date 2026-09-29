using Avalonia;
using Avalonia.Controls;

namespace Md3.Avalonia.Controls;

/// <summary>A baseline Material 3 bottom app bar retained for compatibility; prefer MdToolbar for new designs.</summary>
public sealed class MdBottomAppBar : ContentControl
{
    public static readonly StyledProperty<object?> ActionsProperty =
        AvaloniaProperty.Register<MdBottomAppBar, object?>(nameof(Actions));
    public static readonly StyledProperty<object?> FloatingActionProperty =
        AvaloniaProperty.Register<MdBottomAppBar, object?>(nameof(FloatingAction));

    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    public object? FloatingAction
    {
        get => GetValue(FloatingActionProperty);
        set => SetValue(FloatingActionProperty, value);
    }
}
