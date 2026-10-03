using System.Windows.Input;

namespace Md3.Avalonia.Controls;

/// <summary>Describes one transient message presented by <see cref="MdSnackbarHost"/>.</summary>
public sealed class MdSnackbarMessage
{
    public MdSnackbarMessage(object content)
    {
        ArgumentNullException.ThrowIfNull(content);
        Content = content;
    }

    /// <summary>The message body, normally a short string.</summary>
    public object Content { get; }

    /// <summary>Optional label for the single Material snackbar action.</summary>
    public object? ActionContent { get; init; }

    /// <summary>Optional command invoked by the action button.</summary>
    public ICommand? ActionCommand { get; init; }

    /// <summary>Parameter supplied to <see cref="ActionCommand"/>.</summary>
    public object? ActionCommandParameter { get; init; }

    /// <summary>Whether to show a separate dismiss affordance.</summary>
    public bool IsDismissible { get; init; }

    /// <summary>Visible duration. Set to <see cref="TimeSpan.Zero"/> to keep the message open. Messages with an action remain open regardless of this value.</summary>
    public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(4);
}

/// <summary>Identifies how a message presented by <see cref="MdSnackbarHost"/> was closed.</summary>
public enum MdSnackbarResult
{
    Dismissed,
    ActionInvoked
}
