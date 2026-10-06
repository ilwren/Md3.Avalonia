namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>
/// Resolves the repository locations the conformance harness reads from and writes to.
/// Tests execute out of <c>bin/&lt;config&gt;/net10.0</c>, so the root is discovered by walking up
/// to the solution file rather than being hard-coded.
/// </summary>
internal static class MdSpecPaths
{
    private static readonly Lazy<string?> RootLazy = new(FindRepositoryRoot);

    /// <summary>Repository root, or <see langword="null"/> when the tests run outside a checkout.</summary>
    public static string? RepositoryRoot => RootLazy.Value;

    public static string SpecSnapshotDirectory => Path.Combine(RequireRoot(), "spec-snapshot");

    /// <summary>Generated reports. Git-ignored; published as a CI artifact.</summary>
    public static string ArtifactDirectory => Path.Combine(RequireRoot(), "artifacts", "spec");

    /// <summary>Committed golden images for layer L3.</summary>
    public static string BaselineDirectory =>
        Path.Combine(RequireRoot(), "tests", "Md3.Avalonia.HeadlessTests", "Spec", "baselines");

    public static string RequireRoot() =>
        RepositoryRoot ?? throw new InvalidOperationException(
            "Could not locate Md3.Avalonia.sln above " + AppContext.BaseDirectory +
            ". The specification conformance tests must run from inside a repository checkout.");

    public static string EnsureArtifactDirectory()
    {
        var directory = ArtifactDirectory;
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string? FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        for (var depth = 0; depth < 12 && directory is not null; depth++)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Md3.Avalonia.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
