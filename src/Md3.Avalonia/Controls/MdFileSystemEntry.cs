namespace Md3.Avalonia.Controls;

/// <summary>A real file-system entry displayed by <see cref="MdFileSystemPicker"/>.</summary>
public sealed class MdFileSystemEntry
{
    internal MdFileSystemEntry(
        string name,
        string fullPath,
        bool isDirectory,
        DateTimeOffset modified,
        long? size)
    {
        Name = name;
        FullPath = fullPath;
        IsDirectory = isDirectory;
        Modified = modified;
        Size = size;
        Details = BuildDetails(isDirectory, modified, size);
    }

    /// <summary>The final file or directory name.</summary>
    public string Name { get; }

    /// <summary>The normalized absolute path.</summary>
    public string FullPath { get; }

    /// <summary>Whether this entry is a directory.</summary>
    public bool IsDirectory { get; }

    /// <summary>The last-write timestamp reported by the file system.</summary>
    public DateTimeOffset Modified { get; }

    /// <summary>The file size in bytes, or <see langword="null"/> for a directory.</summary>
    public long? Size { get; }

    /// <summary>A compact, display-ready type, size, and modified-time description.</summary>
    public string Details { get; }

    private static string BuildDetails(bool isDirectory, DateTimeOffset modified, long? size)
    {
        var kind = isDirectory ? "Folder" : FormatSize(size.GetValueOrDefault());
        return $"{kind} · {modified:g}";
    }

    private static string FormatSize(long value)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var size = Math.Max(0, (double)value);
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return unit == 0 ? $"{size:0} {units[unit]}" : $"{size:0.#} {units[unit]}";
    }
}
