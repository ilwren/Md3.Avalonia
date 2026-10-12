using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>Identifies the kind of an entry returned by a file-system provider.</summary>
public enum MdFileEntryKind
{
    File,
    Directory
}

/// <summary>Display mode for <see cref="MdFileItemsView"/>.</summary>
public enum MdFileViewMode
{
    List,
    Grid,
    Details
}

/// <summary>Sort keys understood by the stock file-explorer surface.</summary>
public enum MdFileSortKey
{
    Name,
    Kind,
    Size,
    Modified
}

/// <summary>File operation requested by the file-explorer controls.</summary>
public enum MdFileOperationKind
{
    CreateDirectory,
    Rename,
    Delete,
    Copy,
    Move
}

/// <summary>How a provider should handle a destination that already exists.</summary>
public enum MdFileConflictPolicy
{
    Fail,
    Skip,
    Overwrite,
    KeepBoth
}

/// <summary>An addressable storage root. <see cref="RootPath"/> is provider-specific and may be a URI; <see cref="Icon"/> may be an <see cref="IImage"/> or icon-font glyph string.</summary>
public sealed record MdFileLocation(string Id, string Name, string RootPath, object? Icon = null)
{
    public override string ToString() => Name;
}

/// <summary>Provider-neutral metadata for one file or directory. RelativePath is always relative to its location.</summary>
public sealed record MdFileEntry(
    string Id,
    string Name,
    string RelativePath,
    MdFileEntryKind Kind,
    long? SizeBytes = null,
    DateTimeOffset? LastModified = null,
    string? ContentType = null,
    bool IsHidden = false,
    bool IsReadOnly = false,
    bool IsSymbolicLink = false,
    string? Glyph = null)
{
    /// <summary>An optional provider-supplied image thumbnail. Consumers own and should dispose returned disposable images.</summary>
    public IImage? Thumbnail { get; init; }
    public bool IsDirectory => Kind == MdFileEntryKind.Directory;
    public bool CanNavigate => IsDirectory && !IsSymbolicLink;
    public string Extension => Path.GetExtension(Name);
    public string DisplaySize => IsDirectory ? string.Empty : FormatSize(SizeBytes);

    public static string FormatSize(long? bytes)
    {
        if (bytes is null) return string.Empty;
        var value = Math.Max(0, bytes.Value);
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var unit = 0;
        double size = value;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }
        return unit == 0 ? $"{value} B" : $"{size:0.#} {units[unit]}";
    }
}

/// <summary>Content supplied to a preview pane. The consumer owns and should dispose any returned disposable image.</summary>
public sealed record MdFilePreview(
    string Title,
    IImage? Image = null,
    string? Text = null,
    string? Description = null,
    string? Error = null);

/// <summary>Progress reported by a provider while copying or moving entries.</summary>
public sealed record MdFileOperationProgress(string? CurrentItem, long CompletedItems, long? TotalItems)
{
    public double? Fraction => TotalItems is > 0 ? Math.Clamp(CompletedItems / (double)TotalItems.Value, 0, 1) : null;
}

/// <summary>A provider-neutral operation request. Paths are relative to their respective locations.</summary>
public sealed record MdFileOperationRequest(
    MdFileOperationKind Kind,
    string SourceLocationId,
    string SourcePath,
    string? DestinationLocationId = null,
    string? DestinationPath = null,
    string? NewName = null,
    MdFileConflictPolicy ConflictPolicy = MdFileConflictPolicy.Fail);

/// <summary>Result of a provider operation. Conflicts are returned as data so the UI can ask the user; partial changes flag failed operations that still changed filesystem state.</summary>
public sealed record MdFileOperationResult(
    bool Succeeded,
    bool WasSkipped = false,
    bool HasConflict = false,
    string? ResultPath = null,
    string? ErrorMessage = null,
    bool HasPartialChanges = false)
{
    public static MdFileOperationResult Success(string? resultPath = null) => new(true, ResultPath: resultPath);
    public static MdFileOperationResult Skipped(string? resultPath = null) => new(true, WasSkipped: true, ResultPath: resultPath);
    public static MdFileOperationResult Conflict(string? path = null) => new(false, HasConflict: true, ResultPath: path);
    public static MdFileOperationResult Failure(string message, bool hasPartialChanges = false) =>
        new(false, ErrorMessage: message, HasPartialChanges: hasPartialChanges);
}

/// <summary>
/// Provider contract consumed by the Material file-explorer controls. A location and its relative
/// paths are opaque to the UI, which allows Android SAF/document-tree and other URI providers to
/// avoid pretending that every resource is a desktop file path.
/// </summary>
public interface IMdFileSystemProvider
{
    ValueTask<IReadOnlyList<MdFileLocation>> GetLocationsAsync(CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<MdFileEntry>> GetEntriesAsync(
        string locationId,
        string relativePath,
        bool includeHidden = false,
        CancellationToken cancellationToken = default);

    ValueTask<MdFileOperationResult> ExecuteAsync(
        MdFileOperationRequest request,
        IProgress<MdFileOperationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    ValueTask<MdFilePreview?> GetPreviewAsync(
        string locationId,
        MdFileEntry entry,
        CancellationToken cancellationToken = default);
}

/// <summary>Optional provider extension for thumbnails separately from full-size previews. Returned disposable images transfer ownership to the caller.</summary>
public interface IMdFileThumbnailProvider
{
    ValueTask<IImage?> GetThumbnailAsync(
        string locationId,
        MdFileEntry entry,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Path-based local provider for app-owned directories and desktop roots explicitly supplied by the
/// host. It does not request broad mobile-storage access or discover every mounted volume; use a
/// platform-specific <see cref="IMdFileSystemProvider"/> for Android SAF or other URI-based storage.
/// </summary>
public sealed class MdLocalFileSystemProvider : IMdFileSystemProvider, IMdFileThumbnailProvider
{
    private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".md", ".json", ".xml", ".csv", ".log", ".cs", ".axaml", ".yaml", ".yml", ".ini", ".config"
    };

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp"
    };

    private readonly IReadOnlyDictionary<string, MdFileLocation> _locations;

    public MdLocalFileSystemProvider(IEnumerable<MdFileLocation> locations)
    {
        ArgumentNullException.ThrowIfNull(locations);
        var map = new Dictionary<string, MdFileLocation>(StringComparer.Ordinal);
        foreach (var location in locations)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(location.Id);
            ArgumentException.ThrowIfNullOrWhiteSpace(location.Name);
            ArgumentException.ThrowIfNullOrWhiteSpace(location.RootPath);
            var fullRoot = Path.GetFullPath(location.RootPath);
            if (!Directory.Exists(fullRoot))
                throw new DirectoryNotFoundException($"The location root '{fullRoot}' does not exist.");
            if (!map.TryAdd(location.Id, location with { RootPath = fullRoot }))
                throw new ArgumentException($"Duplicate location id '{location.Id}'.", nameof(locations));
        }
        _locations = new ReadOnlyDictionary<string, MdFileLocation>(map);
    }

    public ValueTask<IReadOnlyList<MdFileLocation>> GetLocationsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<IReadOnlyList<MdFileLocation>>(_locations.Values.ToArray());
    }

    public async ValueTask<IReadOnlyList<MdFileEntry>> GetEntriesAsync(
        string locationId,
        string relativePath,
        bool includeHidden = false,
        CancellationToken cancellationToken = default)
    {
        var directory = Resolve(locationId, relativePath);
        return await Task.Run<IReadOnlyList<MdFileEntry>>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Directory.Exists(directory)) throw new DirectoryNotFoundException(directory);
            var root = GetLocation(locationId).RootPath;
            var entries = new List<MdFileEntry>();
            foreach (var path in Directory.EnumerateFileSystemEntries(directory))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var name = Path.GetFileName(path);
                var attributes = File.GetAttributes(path);
                var hidden = name.StartsWith(".", StringComparison.Ordinal) || attributes.HasFlag(FileAttributes.Hidden);
                if (hidden && !includeHidden) continue;
                var isDirectory = attributes.HasFlag(FileAttributes.Directory);
                var isLink = attributes.HasFlag(FileAttributes.ReparsePoint);
                var relative = ToPortableRelative(Path.GetRelativePath(root, path));
                DateTimeOffset? modified = null;
                if (!isLink)
                {
                    var lastWriteTime = isDirectory
                        ? new DirectoryInfo(path).LastWriteTimeUtc
                        : new FileInfo(path).LastWriteTimeUtc;
                    modified = new DateTimeOffset(lastWriteTime, TimeSpan.Zero);
                }
                long? size = null;
                if (!isDirectory && !isLink)
                {
                    try { size = new FileInfo(path).Length; }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
                var readOnly = attributes.HasFlag(FileAttributes.ReadOnly);
                entries.Add(new MdFileEntry(
                    Id: relative,
                    Name: name,
                    RelativePath: relative,
                    Kind: isDirectory ? MdFileEntryKind.Directory : MdFileEntryKind.File,
                    SizeBytes: size,
                    LastModified: modified,
                    ContentType: isDirectory ? "inode/directory" : GuessContentType(Path.GetExtension(name)),
                    IsHidden: hidden,
                    IsReadOnly: readOnly,
                    IsSymbolicLink: isLink));
            }
            return entries
                .OrderByDescending(entry => entry.IsDirectory)
                .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<MdFileOperationResult> ExecuteAsync(
        MdFileOperationRequest request,
        IProgress<MdFileOperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            return await Task.Run(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var sourceRelativePath = MdFileLocationBar.NormalizeRelativePath(request.SourcePath);
                if (sourceRelativePath is null)
                    return MdFileOperationResult.Failure("The source path must stay inside the configured location.");
                if ((request.Kind is MdFileOperationKind.Rename or MdFileOperationKind.Delete or MdFileOperationKind.Copy or MdFileOperationKind.Move) &&
                    string.IsNullOrEmpty(sourceRelativePath))
                    return MdFileOperationResult.Failure("The location root cannot be renamed, deleted, copied or moved.");
                var source = Resolve(request.SourceLocationId, sourceRelativePath,
                    allowFinalReparsePoint: request.Kind is MdFileOperationKind.Delete or MdFileOperationKind.Rename or MdFileOperationKind.Move);
                switch (request.Kind)
                {
                    case MdFileOperationKind.CreateDirectory:
                    {
                        var name = ValidateName(request.NewName);
                        var target = Resolve(request.SourceLocationId, CombineRelative(request.SourcePath, name));
                        if (ResolveConflict(ref target, request.SourceLocationId, request.ConflictPolicy, isDirectory: true, out var backup) is { } conflict)
                            return conflict;
                        try
                        {
                            Directory.CreateDirectory(target);
                            ReportProgress(progress, new MdFileOperationProgress(name, 1, 1));
                            DeleteBackup(backup);
                            return MdFileOperationResult.Success(ToPortableRelative(Path.GetRelativePath(GetLocation(request.SourceLocationId).RootPath, target)));
                        }
                        catch
                        {
                            RestoreBackup(backup, target);
                            throw;
                        }
                    }
                    case MdFileOperationKind.Rename:
                    {
                        var name = ValidateName(request.NewName);
                        var target = Resolve(request.SourceLocationId, CombineRelative(ParentRelative(sourceRelativePath), name));
                        if (PathEquals(source, target)) return MdFileOperationResult.Success(sourceRelativePath);
                        if (ResolveConflict(ref target, request.SourceLocationId, request.ConflictPolicy,
                                File.GetAttributes(source).HasFlag(FileAttributes.Directory), out var backup) is { } conflict)
                            return conflict;
                        try
                        {
                            await MoveEntityAsync(source, target, progress, cancellationToken).ConfigureAwait(false);
                            DeleteBackup(backup);
                            return MdFileOperationResult.Success(ToPortableRelative(Path.GetRelativePath(GetLocation(request.SourceLocationId).RootPath, target)));
                        }
                        catch (SourceCleanupException exception)
                        {
                            return PreserveFailedMove(exception, target, backup);
                        }
                        catch
                        {
                            RestoreBackup(backup, target);
                            throw;
                        }
                    }
                    case MdFileOperationKind.Delete:
                    {
                        await DeleteEntityAsync(source, progress, cancellationToken).ConfigureAwait(false);
                        return MdFileOperationResult.Success();
                    }
                    case MdFileOperationKind.Copy:
                    case MdFileOperationKind.Move:
                    {
                        if (string.IsNullOrWhiteSpace(request.DestinationLocationId) || request.DestinationPath is null)
                            return MdFileOperationResult.Failure("A destination location and directory are required.");
                        var destinationDirectory = Resolve(request.DestinationLocationId, request.DestinationPath);
                        if (!Directory.Exists(destinationDirectory))
                            return MdFileOperationResult.Failure("The destination directory does not exist.");
                        var destinationName = request.NewName is null ? Path.GetFileName(source) : ValidateName(request.NewName);
                        var destination = Resolve(request.DestinationLocationId,
                            CombineRelative(request.DestinationPath, destinationName));
                        if (PathEquals(source, destination))
                        {
                            if (request.Kind == MdFileOperationKind.Move || request.ConflictPolicy == MdFileConflictPolicy.Overwrite)
                                return MdFileOperationResult.Success(sourceRelativePath);
                            if (request.ConflictPolicy == MdFileConflictPolicy.Skip)
                                return MdFileOperationResult.Skipped(sourceRelativePath);
                            if (request.ConflictPolicy != MdFileConflictPolicy.KeepBoth)
                                return MdFileOperationResult.Conflict(sourceRelativePath);
                        }
                        if (Directory.Exists(source) && IsPathWithin(source, destination))
                            return MdFileOperationResult.Failure("A folder cannot be copied or moved into itself or one of its descendants.");
                        if (ResolveConflict(ref destination, request.DestinationLocationId, request.ConflictPolicy,
                                File.GetAttributes(source).HasFlag(FileAttributes.Directory), out var backup) is { } conflict)
                            return conflict;
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            if (request.Kind == MdFileOperationKind.Move)
                                await MoveEntityAsync(source, destination, progress, cancellationToken).ConfigureAwait(false);
                            else
                                await CopyEntityAsync(source, destination, progress, cancellationToken).ConfigureAwait(false);
                            DeleteBackup(backup);
                            return MdFileOperationResult.Success(ToPortableRelative(Path.GetRelativePath(
                                GetLocation(request.DestinationLocationId).RootPath, destination)));
                        }
                        catch (SourceCleanupException exception)
                        {
                            return PreserveFailedMove(exception, destination, backup);
                        }
                        catch
                        {
                            RestoreBackup(backup, destination);
                            throw;
                        }
                    }
                    default:
                        return MdFileOperationResult.Failure($"Unsupported operation: {request.Kind}.");
                }
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or KeyNotFoundException or NotSupportedException)
        {
            return MdFileOperationResult.Failure(exception.Message);
        }
    }

    public async ValueTask<MdFilePreview?> GetPreviewAsync(
        string locationId,
        MdFileEntry entry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        cancellationToken.ThrowIfCancellationRequested();
        if (entry.IsSymbolicLink)
            return new MdFilePreview(entry.Name, Error: "Symbolic links are not followed by this local provider.");
        if (entry.IsDirectory)
            return new MdFilePreview(entry.Name, Description: "Folder");
        var path = Resolve(locationId, entry.RelativePath);
        if (!File.Exists(path)) return new MdFilePreview(entry.Name, Error: "This item is no longer available.");
        var extension = Path.GetExtension(entry.Name);
        if (TextExtensions.Contains(extension))
        {
            return await Task.Run(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8192, useAsync: true);
                using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
                const int previewLimit = 16_384;
                var buffer = new char[previewLimit + 1];
                var totalRead = 0;
                while (totalRead < buffer.Length)
                {
                    var read = await reader.ReadAsync(buffer.AsMemory(totalRead), cancellationToken).ConfigureAwait(false);
                    if (read == 0) break;
                    totalRead += read;
                }
                var text = new string(buffer, 0, Math.Min(totalRead, previewLimit));
                if (totalRead > previewLimit) text += Environment.NewLine + "…";
                return new MdFilePreview(entry.Name, Text: text, Description: entry.DisplaySize);
            }, cancellationToken).ConfigureAwait(false);
        }
        if (ImageExtensions.Contains(extension))
        {
            return await Task.Run<MdFilePreview?>(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var stream = File.OpenRead(path);
                var bitmap = new Bitmap(stream);
                return new MdFilePreview(entry.Name, Image: bitmap, Description: entry.DisplaySize);
            }, cancellationToken).ConfigureAwait(false);
        }
        return new MdFilePreview(entry.Name, Description: entry.ContentType ?? entry.Extension, Error: "No inline preview is available for this file type.");
    }

    public async ValueTask<IImage?> GetThumbnailAsync(
        string locationId,
        MdFileEntry entry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        cancellationToken.ThrowIfCancellationRequested();
        if (entry.IsDirectory || entry.IsSymbolicLink || !ImageExtensions.Contains(Path.GetExtension(entry.Name))) return null;
        var path = Resolve(locationId, entry.RelativePath);
        if (!File.Exists(path)) return null;
        return await Task.Run<IImage?>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = File.OpenRead(path);
            return Bitmap.DecodeToWidth(stream, 256);
        }, cancellationToken).ConfigureAwait(false);
    }

    private MdFileLocation GetLocation(string locationId) =>
        _locations.TryGetValue(locationId, out var location)
            ? location
            : throw new KeyNotFoundException($"Unknown file location '{locationId}'.");

    private static bool IsReparsePoint(string path)
    {
        try { return File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint); }
        catch (FileNotFoundException) { return false; }
        catch (DirectoryNotFoundException) { return false; }
    }

    private string Resolve(string locationId, string? relativePath, bool allowFinalReparsePoint = false)
    {
        var root = GetLocation(locationId).RootPath;
        var portable = (relativePath ?? string.Empty).Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        var full = Path.GetFullPath(Path.Combine(root, portable));
        var relative = Path.GetRelativePath(root, full);
        if (Path.IsPathRooted(relative) || relative == ".." || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("The requested path is outside the configured location.");

        // A lexical root check alone can be escaped through a symbolic link inside the root. Refuse
        // to traverse reparse points in parent segments. Delete, rename and same-volume move may
        // operate on a final link entry itself; those paths inspect its attributes and never follow it.
        var cursor = root;
        if (relative != ".")
        {
            var segments = relative.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            for (var index = 0; index < segments.Length; index++)
            {
                cursor = Path.Combine(cursor, segments[index]);
                var isAllowedFinalLink = allowFinalReparsePoint && index == segments.Length - 1;
                if (!isAllowedFinalLink && IsReparsePoint(cursor))
                    throw new UnauthorizedAccessException("Symbolic links are not followed by this local provider.");
            }
        }
        return full;
    }

    private static string ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name is "." or ".." || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.Contains('/') || name.Contains('\\'))
            throw new ArgumentException("Enter a valid file or folder name.", nameof(name));
        return name.Trim();
    }

    private static string CombineRelative(string parent, string name) =>
        string.IsNullOrWhiteSpace(parent) ? name : $"{parent.TrimEnd('/', '\\')}/{name}";

    private static string ParentRelative(string relative)
    {
        var normalized = ToPortableRelative(relative).TrimEnd('/');
        var separator = normalized.LastIndexOf('/');
        return separator < 0 ? string.Empty : normalized[..separator];
    }

    private static string ToPortableRelative(string path) => path.Replace('\\', '/');

    private static bool PathEquals(string left, string right) =>
        string.Equals(Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar),
            Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    private static bool IsPathWithin(string directory, string candidate)
    {
        var fullDirectory = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var fullCandidate = Path.GetFullPath(candidate);
        return fullCandidate.StartsWith(fullDirectory,
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    private MdFileOperationResult? ResolveConflict(
        ref string target,
        string locationId,
        MdFileConflictPolicy policy,
        bool isDirectory,
        out string? backup)
    {
        backup = null;
        if (!File.Exists(target) && !Directory.Exists(target)) return null;
        var root = GetLocation(locationId).RootPath;
        var relativeTarget = ToPortableRelative(Path.GetRelativePath(root, target));
        switch (policy)
        {
            case MdFileConflictPolicy.Skip:
                return MdFileOperationResult.Skipped(relativeTarget);
            case MdFileConflictPolicy.Overwrite:
                backup = MoveToUniqueBackup(target);
                return null;
            case MdFileConflictPolicy.KeepBoth:
                target = FindKeepBothPath(target, isDirectory);
                return null;
            default:
                return MdFileOperationResult.Conflict(relativeTarget);
        }
    }

    private static string MoveToUniqueBackup(string target)
    {
        var parent = Path.GetDirectoryName(target) ?? throw new IOException("The conflicting item has no parent directory.");
        var backup = Path.Combine(parent, $".{Path.GetFileName(target)}.{Guid.NewGuid():N}.md3backup");
        if (Directory.Exists(target)) Directory.Move(target, backup);
        else File.Move(target, backup);
        return backup;
    }

    private static void DeleteBackup(string? backup)
    {
        if (backup is null) return;
        try { DeleteEntity(backup); }
        catch (Exception) { /* Keep the recovery copy; cleanup must not roll back a committed operation. */ }
    }

    private static void RestoreBackup(string? backup, string target)
    {
        if (backup is null || (!File.Exists(backup) && !Directory.Exists(backup))) return;
        if (File.Exists(target) || Directory.Exists(target)) DeleteEntity(target);
        if (Directory.Exists(backup)) Directory.Move(backup, target);
        else File.Move(backup, target);
    }

    private static string FindKeepBothPath(string path, bool isDirectory)
    {
        var directory = Path.GetDirectoryName(path) ?? string.Empty;
        var name = isDirectory ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path);
        var extension = isDirectory ? string.Empty : Path.GetExtension(path);
        for (var suffix = 2; suffix < 10_000; suffix++)
        {
            var candidate = Path.Combine(directory, $"{name} ({suffix}){extension}");
            if (!File.Exists(candidate) && !Directory.Exists(candidate)) return candidate;
        }
        throw new IOException("Could not find an unused name for the copied item.");
    }

    private static void ReportProgress(
        IProgress<MdFileOperationProgress>? progress,
        MdFileOperationProgress update)
    {
        if (progress is null) return;
        try { progress.Report(update); }
        catch (Exception) { /* A progress observer must not roll back a completed filesystem mutation. */ }
    }

    private static MdFileOperationResult PreserveFailedMove(
        SourceCleanupException exception,
        string destination,
        string? backup)
    {
        if (backup is null || (!File.Exists(backup) && !Directory.Exists(backup)))
            return MdFileOperationResult.Failure(exception.Message, hasPartialChanges: true);
        try
        {
            var backupIsDirectory = Directory.Exists(backup);
            var recoveryPath = FindKeepBothPath(destination, backupIsDirectory);
            if (backupIsDirectory) Directory.Move(backup, recoveryPath);
            else File.Move(backup, recoveryPath);
            return MdFileOperationResult.Failure(
                $"{exception.Message} The previous destination was preserved as '{Path.GetFileName(recoveryPath)}'.",
                hasPartialChanges: true);
        }
        catch (Exception recoveryException)
        {
            return MdFileOperationResult.Failure(
                $"{exception.Message} The previous destination remains in recovery backup '{backup}': {recoveryException.Message}",
                hasPartialChanges: true);
        }
    }

    private static async Task MoveEntityAsync(
        string source,
        string destination,
        IProgress<MdFileOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        try
        {
            var sourceAttributes = File.GetAttributes(source);
            if (sourceAttributes.HasFlag(FileAttributes.Directory)) Directory.Move(source, destination);
            else File.Move(source, destination);
        }
        catch (IOException exception) when (!File.Exists(destination) && !Directory.Exists(destination) &&
                   (!string.Equals(Path.GetPathRoot(source), Path.GetPathRoot(destination),
                       OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) ||
                    (exception.HResult & 0xFFFF) is 17 or 18))
        {
            // A cross-volume rename is not atomic. Copy first, then remove the source only after
            // the full destination tree has been written successfully.
            await CopyEntityAsync(source, destination, progress, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                DeleteEntity(source);
            }
            catch (Exception cleanupException)
            {
                throw new SourceCleanupException(
                    $"The destination copy at '{destination}' is complete, but the source could not be fully removed: {cleanupException.Message}",
                    cleanupException);
            }
        }
        ReportProgress(progress, new MdFileOperationProgress(Path.GetFileName(source), 1, 1));
    }

    private static void DeleteEntity(string path)
    {
        var attributes = File.GetAttributes(path);
        if (attributes.HasFlag(FileAttributes.Directory) && !attributes.HasFlag(FileAttributes.ReparsePoint))
        {
            foreach (var child in Directory.EnumerateFileSystemEntries(path)) DeleteEntity(child);
            Directory.Delete(path, recursive: false);
        }
        else if (attributes.HasFlag(FileAttributes.Directory))
        {
            Directory.Delete(path);
        }
        else
        {
            File.Delete(path);
        }
    }

    private static async Task DeleteEntityAsync(
        string path,
        IProgress<MdFileOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        var total = CountEntities(path, cancellationToken);
        var completed = 0L;
        await DeleteRecursiveAsync(path, progress, cancellationToken, total, () => ++completed).ConfigureAwait(false);
    }

    private static async Task DeleteRecursiveAsync(
        string path,
        IProgress<MdFileOperationProgress>? progress,
        CancellationToken cancellationToken,
        long total,
        Func<long> completeOne)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var attributes = File.GetAttributes(path);
        if (attributes.HasFlag(FileAttributes.Directory) && !attributes.HasFlag(FileAttributes.ReparsePoint))
        {
            foreach (var child in Directory.EnumerateFileSystemEntries(path))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await DeleteRecursiveAsync(child, progress, cancellationToken, total, completeOne).ConfigureAwait(false);
            }
            cancellationToken.ThrowIfCancellationRequested();
            Directory.Delete(path, recursive: false);
        }
        else if (attributes.HasFlag(FileAttributes.Directory))
        {
            Directory.Delete(path);
        }
        else
        {
            File.Delete(path);
        }
        ReportProgress(progress, new MdFileOperationProgress(Path.GetFileName(path), completeOne(), total));
    }

    private static async Task CopyEntityAsync(
        string source,
        string destination,
        IProgress<MdFileOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        var total = CountEntities(source, cancellationToken);
        var completed = 0L;
        var parent = Path.GetDirectoryName(destination) ?? throw new IOException("The destination has no parent directory.");
        var temporary = Path.Combine(parent, $".{Path.GetFileName(destination)}.{Guid.NewGuid():N}.md3copy");
        try
        {
            await CopyRecursiveAsync(source, temporary, progress, cancellationToken, total, () => ++completed).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (Directory.Exists(temporary)) Directory.Move(temporary, destination);
            else File.Move(temporary, destination);
        }
        catch
        {
            try
            {
                if (File.Exists(temporary) || Directory.Exists(temporary)) DeleteEntity(temporary);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            throw;
        }
    }

    private static long CountEntities(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var attributes = File.GetAttributes(path);
        if (!attributes.HasFlag(FileAttributes.Directory) || attributes.HasFlag(FileAttributes.ReparsePoint)) return 1;
        var count = 1L;
        foreach (var child in Directory.EnumerateFileSystemEntries(path))
        {
            cancellationToken.ThrowIfCancellationRequested();
            count += CountEntities(child, cancellationToken);
        }
        return count;
    }

    private static async Task CopyRecursiveAsync(
        string source,
        string destination,
        IProgress<MdFileOperationProgress>? progress,
        CancellationToken cancellationToken,
        long total,
        Func<long> completeOne)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var attributes = File.GetAttributes(source);
        if (attributes.HasFlag(FileAttributes.ReparsePoint))
            throw new UnauthorizedAccessException("Symbolic links are not copied by this local provider.");
        if (attributes.HasFlag(FileAttributes.Directory))
        {
            Directory.CreateDirectory(destination);
            ReportProgress(progress, new MdFileOperationProgress(Path.GetFileName(source), completeOne(), total));
            foreach (var child in Directory.EnumerateFileSystemEntries(source))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await CopyRecursiveAsync(child, Path.Combine(destination, Path.GetFileName(child)), progress, cancellationToken, total, completeOne).ConfigureAwait(false);
            }
            return;
        }

        await using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, useAsync: true);
        await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true);
        await input.CopyToAsync(output, 64 * 1024, cancellationToken).ConfigureAwait(false);
        ReportProgress(progress, new MdFileOperationProgress(Path.GetFileName(source), completeOne(), total));
    }

    private sealed class SourceCleanupException(string message, Exception innerException)
        : IOException(message, innerException)
    {
    }

    private static string? GuessContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".txt" or ".log" or ".md" => "text/plain",
        ".json" => "application/json",
        ".pdf" => "application/pdf",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".mp3" or ".wav" => "audio",
        ".mp4" or ".mov" => "video",
        ".zip" => "application/zip",
        _ => null
    };
}
