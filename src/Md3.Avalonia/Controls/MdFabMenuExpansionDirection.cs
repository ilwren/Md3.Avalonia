namespace Md3.Avalonia.Controls;

/// <summary>
/// Specifies the direction in which a <see cref="MdFabMenu"/> places its actions relative to the
/// anchored trigger. This is independent from the menu control's alignment inside its parent.
/// </summary>
public enum MdFabMenuExpansionDirection
{
    /// <summary>Place actions above the trigger, matching the Material FAB menu pattern.</summary>
    Up,

    /// <summary>Place actions below the trigger for layouts whose anchor is near the top edge.</summary>
    Down
}
