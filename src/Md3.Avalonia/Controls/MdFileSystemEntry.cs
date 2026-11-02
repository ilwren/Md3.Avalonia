using System.Globalization;
using Md3.Avalonia.Localization;

namespace Md3.Avalonia.Controls;

/// <summary>A real file-system entry displayed by <see cref="MdFileSystemPicker"/>.</summary>
public sealed class MdFileSystemEntry
{
    internal MdFileSystemEntry(
        string name,
        string fullPath,
        bool isDirectory,
        DateTimeOffset modified,
        long? size,
        CultureInfo culture)
    {
        Name = name;
        FullPath = fullPath;
        IsDirectory = isDirectory;
        Modified = modified;
        Size = size;
        Details = BuildDetails(isDirectory, modified, size, culture);
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

    private static string BuildDetails(
        bool isDirectory,
        DateTimeOffset modified,
        long? size,
        CultureInfo culture)
    {
        var kind = isDirectory
            ? MdLocalization.GetString("FileSystemFolder", culture)
            : FormatSize(size.GetValueOrDefault(), culture);
        return $"{kind} · {modified.ToString("g", culture)}";
    }

    private static string FormatSize(long value, CultureInfo culture)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var size = Math.Max(0, (double)value);
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        var number = size.ToString(unit == 0 ? "0" : "0.#", culture);
        return $"{number} {units[unit]}";
    }
}
