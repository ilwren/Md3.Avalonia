namespace Md3.Avalonia.Controls;

/// <summary>Describes a user-requested seek position. The player remains responsible for seeking.</summary>
public sealed class MdSeekRequestedEventArgs(TimeSpan position) : EventArgs
{
    /// <summary>The target position requested by the user.</summary>
    public TimeSpan Position { get; } = position;
}
