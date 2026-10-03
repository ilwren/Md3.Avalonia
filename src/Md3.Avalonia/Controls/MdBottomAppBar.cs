using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>A baseline Material 3 bottom app bar retained for compatibility; prefer MdToolbar for new designs.</summary>
[PseudoClasses(":fab-left", ":fab-right")]
public sealed class MdBottomAppBar : ContentControl
{
    public static readonly StyledProperty<object?> ActionsProperty =
        AvaloniaProperty.Register<MdBottomAppBar, object?>(nameof(Actions));
    public static readonly StyledProperty<object?> FloatingActionProperty =
        AvaloniaProperty.Register<MdBottomAppBar, object?>(nameof(FloatingAction));
    public static readonly StyledProperty<MdFabAlignment> FloatingActionAlignmentProperty =
        AvaloniaProperty.Register<MdBottomAppBar, MdFabAlignment>(nameof(FloatingActionAlignment), MdFabAlignment.End);

    static MdBottomAppBar()
    {
        FloatingActionAlignmentProperty.Changed.AddClassHandler<MdBottomAppBar>((bar, _) => bar.UpdateAlignment());
        FlowDirectionProperty.Changed.AddClassHandler<MdBottomAppBar>((bar, _) => bar.UpdateAlignment());
    }

    public MdBottomAppBar() => UpdateAlignment();

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

    /// <summary>Logical edge for the floating action. <see cref="MdFabAlignment.End"/> mirrors in RTL.</summary>
    public MdFabAlignment FloatingActionAlignment
    {
        get => GetValue(FloatingActionAlignmentProperty);
        set => SetValue(FloatingActionAlignmentProperty, value);
    }

    private void UpdateAlignment()
    {
        var left = FloatingActionAlignment.ResolvesLeft(FlowDirection);
        PseudoClasses.Set(":fab-left", left);
        PseudoClasses.Set(":fab-right", !left);
    }
}
