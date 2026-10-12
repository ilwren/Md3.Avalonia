using System.ComponentModel;

namespace Md3.Avalonia.Chat;

/// <summary>
/// A stable list row for one message. The chat list binds to rows rather than to the messages themselves, so a
/// streamed replacement only updates <see cref="Message"/> and the bubble inside the row is not rebuilt.
/// </summary>
internal sealed class MdChatRow : INotifyPropertyChanged
{
    private MdChatMessage _message;
    private MdChatMessageLayout? _layout;

    public MdChatRow(MdChatMessage message) => _message = message;

    public MdChatMessage Message
    {
        get => _message;
        set
        {
            if (ReferenceEquals(_message, value)) return;
            _message = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
        }
    }

    /// <summary>How the message is grouped and separated from the one before it; set by the chat view.</summary>
    public MdChatMessageLayout? Layout
    {
        get => _layout;
        set
        {
            if (_layout == value) return;
            _layout = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Layout)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
