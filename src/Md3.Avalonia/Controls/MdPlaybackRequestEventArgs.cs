namespace Md3.Avalonia.Controls;

/// <summary>Describes a backend-neutral transport action requested by <see cref="MdVideoPlayer"/>.</summary>
public sealed class MdPlaybackRequestEventArgs : EventArgs
{
    public MdPlaybackRequestEventArgs(
        MdPlaybackRequest request,
        TimeSpan? position = null,
        double? value = null)
    {
        Request = request;
        Position = position;
        Value = value;
    }

    public MdPlaybackRequest Request { get; }

    /// <summary>The absolute target used by seek, rewind, and forward requests.</summary>
    public TimeSpan? Position { get; }

    /// <summary>The normalized volume or playback-rate value used by scalar requests.</summary>
    public double? Value { get; }
}
