using System.ComponentModel;

namespace Md3.Avalonia.Chat;

/// <summary>
/// A stable list row for one message. The chat list binds to rows rather than to the messages themselves, so a
/// streamed replacement only updates <see cref="Message"/> and the bubble inside the row is not rebuilt.
/// </summary>
internal sealed class MdChatRow : INotifyPropertyChanged
{
    private MdChatMessage _message;

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

    public event PropertyChangedEventHandler? PropertyChanged;
}
