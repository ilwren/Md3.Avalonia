using System.Reflection;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>
/// The library's public surface, read from the source tree and from the assemblies it ships.
/// </summary>
/// <remarks>
/// <para>
/// Two L4 checks need the same three answers - which types exist, which of them carry a control
/// theme, and which <c>Md.*</c> keys the theme an application actually registers declares - so they
/// live here rather than in whichever test happened to need them first. Duplicating a scan is how
/// two reports start disagreeing about the size of the same gap.
/// </para>
/// <para>
/// Everything here reads files rather than a built assembly, because the questions are about what
/// the repository declares, not about what one compile emitted.
/// </para>
/// </remarks>
internal static class MdSpecSurface
{
    /// <summary>
    /// Public, non-abstract <c>Md*</c> classes from the assemblies that ship controls.
    /// </summary>
    /// <remarks>
    /// The test project references every theme package, so an assembly-wide scan would also count a
    /// third-party type that happens to start with <c>Md</c>. The set is pinned to the three
    /// assemblies that actually ship controls.
    /// </remarks>
    public static IReadOnlyList<Type> PublicControlTypes()
    {
        var assemblies = new[]
        {
            typeof(MdButton).Assembly,          // Md3.Avalonia
            typeof(MdRating).Assembly,          // Md3.Avalonia.Extra
            typeof(MdSignaturePad).Assembly     // Md3.Avalonia.InkCanvas (namespace Md3.Avalonia.Controls)
        };

        return assemblies
            .Distinct()
            .SelectMany(assembly => assembly.GetExportedTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false }
                           && type.Name.StartsWith("Md", StringComparison.Ordinal))
            .Distinct()
            .OrderBy(type => type.Name, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>Type names that carry a <c>{x:Type}</c> control theme (or a keyed one) in any theme file.</summary>
    public static HashSet<string> ThemedTypeNames()
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in SourceFiles("*.axaml"))
        {
            var text = File.ReadAllText(file);
            foreach (Match match in Regex.Matches(text, "x:Key=\"\\{x:Type [\\w:]*?(\\w+)\\}\""))
            {
                names.Add(match.Groups[1].Value);
            }

            foreach (Match match in Regex.Matches(text, "ControlTheme[^>]*TargetType=\"[\\w:]*?(\\w+)\""))
            {
                names.Add(match.Groups[1].Value);
            }
        }

        return names;
    }

    /// <summary>
    /// The AXAML files <c>MaterialTheme</c> merges, resolved from its own include list.
    /// </summary>
    /// <remarks>
    /// Read from the include list rather than by globbing the directory, so a file that exists but
    /// nobody merged - the exact shape of "declared and never loaded" - cannot pass a check by being
    /// counted.
    /// </remarks>
    public static IReadOnlyList<string> CoreThemeFiles()
    {
        var root = MdSpecPaths.RequireRoot();
        var theme = Path.Combine(root, "src", "Md3.Avalonia", "Themes", "MaterialTheme.axaml");
        var files = new List<string>();
        foreach (Match match in Regex.Matches(
                     File.ReadAllText(theme), "avares://Md3\\.Avalonia/(?<path>Themes/[^\"]+\\.axaml)"))
        {
            var path = Path.Combine(root, "src", "Md3.Avalonia", match.Groups["path"].Value);
            if (File.Exists(path))
            {
                files.Add(path);
            }
        }

        if (files.Count <= 20)
        {
            throw new InvalidOperationException(
                $"only {files.Count} dictionaries were resolved from MaterialTheme.axaml; the include scan is not working");
        }

        return files;
    }

    /// <summary>Every <c>Md.*</c> key a shipped AXAML dictionary declares, anywhere in <c>src</c>.</summary>
    public static HashSet<string> AllDeclaredKeys()
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in SourceFiles("*.axaml"))
        {
            foreach (Match match in Regex.Matches(File.ReadAllText(file), "x:Key=\"(Md\\.[A-Za-z0-9.]+)\""))
            {
                keys.Add(match.Groups[1].Value);
            }
        }

        return keys;
    }

    public static bool IsBuildOutput(string path) =>
        path.Contains("/obj/", StringComparison.Ordinal) || path.Contains("/bin/", StringComparison.Ordinal);

    /// <summary>First-party files under <c>src</c>, build output excluded, in a stable order.</summary>
    public static IEnumerable<string> SourceFiles(string pattern) =>
        Directory
            .EnumerateFiles(Path.Combine(MdSpecPaths.RequireRoot(), "src"), pattern, SearchOption.AllDirectories)
            .Where(path => !IsBuildOutput(path))
            .OrderBy(path => path, StringComparer.Ordinal);

    /// <summary>
    /// True when the type can be created without arguments, which is the precondition for laying it
    /// out in a generic matrix.
    /// </summary>
    public static bool HasParameterlessConstructor(Type type) =>
        type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes) is not null;
}
