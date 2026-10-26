using Avalonia.Platform;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery;

/// <summary>
/// Optional desktop seam used by the shared Gallery without placing LibVLC in the Android or
/// NativeAOT dependency graph.
/// </summary>
public static class GalleryVideoBackend
{
    private static readonly object ExtractionLock = new();
    private const string AssetUri =
        "avares://Md3.Avalonia.Gallery/Assets/Media/material-motion.mp4";

    /// <summary>
    /// Set by a non-AOT desktop host to create its real player. Android, headless tests, and the
    /// NativeAOT Gallery intentionally leave this unset and use the deterministic visual fallback.
    /// </summary>
    public static Func<Uri, MdVideoPlayer>? Factory { get; set; }

    /// <summary>Extracts the embedded MP4 to a stable local path that native players can open.</summary>
    public static Uri ExtractBundledSample()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Md3.Avalonia.Gallery", "video");
        var path = Path.Combine(directory, "material-motion-v1.mp4");
        lock (ExtractionLock)
        {
            Directory.CreateDirectory(directory);
            using var source = AssetLoader.Open(new Uri(AssetUri));
            if (!File.Exists(path) || new FileInfo(path).Length != source.Length)
            {
                using var destination = File.Create(path);
                source.CopyTo(destination);
            }
        }
        return new Uri(Path.GetFullPath(path));
    }
}
