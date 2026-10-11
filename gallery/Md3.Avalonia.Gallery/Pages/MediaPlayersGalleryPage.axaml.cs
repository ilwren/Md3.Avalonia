using Avalonia.Controls;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class MediaPlayersGalleryPage : UserControl
{
    public MediaPlayersGalleryPage() => InitializeComponent();

    private void PlaybackRequested(object? sender, MdPlaybackRequestEventArgs e)
    {
        StatusLabel.Text = $"{e.Request} requested. The gallery demonstrates UI state only; connect this event to your playback service.";
    }

    private void SeekRequested(object? sender, MdSeekRequestedEventArgs e)
    {
        switch (sender)
        {
            case MdSeekBar seekBar:
                seekBar.Position = e.Position;
                break;
            case MdVideoPlayerControls videoControls:
                videoControls.Position = e.Position;
                break;
            case MdNowPlayingPanel nowPlaying:
                nowPlaying.Position = e.Position;
                break;
        }
        StatusLabel.Text = $"Seek requested at {e.Position:hh\\:mm\\:ss}. The host application decides how to service it.";
    }

    private void OpenRequested(object? sender, EventArgs e)
    {
        StatusLabel.Text = "Open requested. Navigate to a full-screen or now-playing page in your application.";
    }
}
