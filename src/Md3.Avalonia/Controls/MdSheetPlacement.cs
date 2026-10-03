using Avalonia.Media;

namespace Md3.Avalonia.Controls;

public enum MdSheetPlacement
{
    Bottom,
    /// <summary>Logical leading side; left in LTR and right in RTL.</summary>
    Start,
    /// <summary>Logical trailing side; right in LTR and left in RTL.</summary>
    End,
    /// <summary>Physical left side retained for compatibility.</summary>
    Left,
    /// <summary>Physical right side retained for compatibility.</summary>
    Right
}

internal static class MdSheetPlacementExtensions
{
    public static MdSheetPlacement Resolve(this MdSheetPlacement placement, FlowDirection direction) => placement switch
    {
        MdSheetPlacement.Start => direction == FlowDirection.LeftToRight ? MdSheetPlacement.Left : MdSheetPlacement.Right,
        MdSheetPlacement.End => direction == FlowDirection.LeftToRight ? MdSheetPlacement.Right : MdSheetPlacement.Left,
        _ => placement
    };
}
