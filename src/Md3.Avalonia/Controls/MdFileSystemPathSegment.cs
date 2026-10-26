namespace Md3.Avalonia.Controls;

/// <summary>A navigable segment of the directory currently shown by <see cref="MdFileSystemPicker"/>.</summary>
public sealed class MdFileSystemPathSegment
{
    internal MdFileSystemPathSegment(string label, string fullPath, bool isCurrent)
    {
        Label = label;
        FullPath = fullPath;
        IsCurrent = isCurrent;
    }

    /// <summary>The compact file-system name displayed in the breadcrumb trail.</summary>
    public string Label { get; }

    /// <summary>The normalized absolute directory represented by this segment.</summary>
    public string FullPath { get; }

    /// <summary>Whether this segment represents the directory currently being browsed.</summary>
    public bool IsCurrent { get; }
}
