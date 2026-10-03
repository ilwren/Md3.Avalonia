using Avalonia.Media;

namespace Md3.Avalonia.Controls;

/// <summary>Specifies a logical or physical horizontal alignment for floating actions.</summary>
public enum MdFabAlignment
{
    /// <summary>Align to the logical trailing edge. This is the Material default and mirrors in RTL.</summary>
    End,

    /// <summary>Align to the logical leading edge. This mirrors in RTL.</summary>
    Start,

    /// <summary>Align to the physical right edge. Retained for source compatibility.</summary>
    Right,

    /// <summary>Align to the physical left edge. Retained for source compatibility.</summary>
    Left
}

internal static class MdFabAlignmentExtensions
{
    public static bool ResolvesLeft(this MdFabAlignment alignment, FlowDirection direction) => alignment switch
    {
        MdFabAlignment.Left => true,
        MdFabAlignment.Right => false,
        MdFabAlignment.Start => direction == FlowDirection.LeftToRight,
        _ => direction == FlowDirection.RightToLeft
    };
}
