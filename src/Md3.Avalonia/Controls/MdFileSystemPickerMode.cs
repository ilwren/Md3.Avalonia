namespace Md3.Avalonia.Controls;

/// <summary>Determines whether <see cref="MdFileSystemPicker"/> returns files or directories.</summary>
public enum MdFileSystemPickerMode
{
    /// <summary>Directories remain navigable and files can be selected.</summary>
    File,

    /// <summary>Directories can be selected or opened; files are omitted.</summary>
    Folder
}
