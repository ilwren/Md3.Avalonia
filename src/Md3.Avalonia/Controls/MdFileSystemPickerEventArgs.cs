namespace Md3.Avalonia.Controls;

/// <summary>Provides the normalized absolute paths confirmed in an <see cref="MdFileSystemPicker"/>.</summary>
public sealed class MdFileSystemSelectionEventArgs(IReadOnlyList<string> paths) : EventArgs
{
    /// <summary>The selected real file-system paths.</summary>
    public IReadOnlyList<string> Paths { get; } = paths;
}

/// <summary>Describes a file-system location that could not be opened.</summary>
public sealed class MdFileSystemNavigationFailedEventArgs(string path, Exception exception) : EventArgs
{
    /// <summary>The requested location.</summary>
    public string Path { get; } = path;

    /// <summary>The underlying file-system failure.</summary>
    public Exception Exception { get; } = exception;
}
