namespace Md3.Avalonia.Controls;

public sealed class MdPlaybackRequestEventArgs(MdPlaybackRequest request) : EventArgs
{
    public MdPlaybackRequest Request { get; } = request;
}
