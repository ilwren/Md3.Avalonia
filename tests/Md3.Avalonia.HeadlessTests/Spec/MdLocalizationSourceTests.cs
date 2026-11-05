using System.Text.RegularExpressions;
using Md3.Avalonia.Localization;
using Xunit;

namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>
/// Layer L4 — localization tables, read from source.
/// </summary>
/// <remarks>
/// <para>
/// <c>MdLocalization.Strings</c> is a static dictionary literal. A key written twice inside one
/// initialiser does not throw and cannot be seen through the dictionary afterwards: the later entry
/// simply wins. The only place that defect exists is the source text, so this test parses the
/// initialiser rather than reading the runtime table that the scorecard already covers.
/// </para>
/// <para>
/// The table half is a real invariant rather than a smell: every culture must be able to render
/// every key the English table defines, and the <c>{0}</c> placeholders must agree. A missing key
/// falls back to English silently; a missing placeholder throws <see cref="FormatException"/> at
/// the moment a control tries to speak.
/// </para>
/// </remarks>
public sealed class MdLocalizationSourceTests
{
    private const string Layer = "L4_inventory";

    private static readonly Regex Table = new(
        "\\[\"(?<culture>[A-Za-z-]+)\"\\]\\s*=\\s*new Dictionary<string, string>",
        RegexOptions.Compiled);

    private static readonly Regex Entry = new(
        "\\[\"(?<key>[A-Za-z0-9_.]+)\"\\]\\s*=\\s*\"(?<value>(?:[^\"\\\\]|\\\\.)*)\"",
        RegexOptions.Compiled);

    private static readonly Regex Placeholder = new("\\{(?<index>\\d+)\\}", RegexOptions.Compiled);

    [Fact]
    public void No_Culture_Table_Defines_A_Key_Twice()
    {
        var duplicates = new List<string>();
        var files = 0;
        var entries = 0;

        foreach (var file in SourceFiles())
        {
            var text = File.ReadAllText(file);
            if (!text.Contains("MdLocalization", StringComparison.Ordinal))
            {
                continue;
            }

            files++;
            foreach (var (culture, table) in Tables(text))
            {
                entries += table.Count;
                duplicates.AddRange(table
                    .GroupBy(pair => pair.Key, StringComparer.Ordinal)
                    .Where(group => group.Count() > 1)
                    .Select(group => $"{Path.GetFileName(file)}: '{culture}/{group.Key}' is defined {group.Count()} times"));
            }
        }

        Assert.True(files > 0, "no localization source file was found");
        Assert.True(duplicates.Count == 0,
            $"{duplicates.Count} duplicate localization key(s):{Environment.NewLine}  " +
            string.Join(Environment.NewLine + "  ", duplicates));

        var report = new MdSpecReport("L4-localization-source", Layer, "L4 — localization source",
            MdSpecPolicy.Load());
        report.Metric("localization_duplicate_keys", duplicates.Count);
        report.Metric("localization_entries", entries);
        report.Pass($"localization source ({files} file(s), {entries} entries, no duplicate key)");
        report.Complete();
    }

    [Fact]
    public void Every_Culture_Can_Render_Every_English_Key_With_The_Same_Placeholders()
    {
        // Grouped rather than keyed: a source file may open a second table for the same culture
        // (no shipped file does, but the parser is not the place to assume it), and collapsing
        // those here is what keeps the comparison a comparison of cultures.
        var tables = Tables(File.ReadAllText(LocalizationFile()))
            .GroupBy(pair => pair.Culture, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.SelectMany(pair => pair.Table)
                    .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal),
                StringComparer.Ordinal);

        Assert.Contains("en", tables.Keys);

        var english = tables["en"];
        var mismatches = new List<string>();

        foreach (var (culture, table) in tables.Where(pair => pair.Key != "en"))
        {
            mismatches.AddRange(english.Keys
                .Where(key => !table.ContainsKey(key))
                .Select(key => $"{culture} is missing '{key}'"));

            mismatches.AddRange(table.Keys
                .Where(key => !english.ContainsKey(key))
                .Select(key => $"{culture} defines '{key}', which has no English default"));

            foreach (var (key, value) in english)
            {
                if (!table.TryGetValue(key, out var translated))
                {
                    continue;
                }

                var expected = Indices(value);
                var actual = Indices(translated);
                if (!expected.SequenceEqual(actual))
                {
                    mismatches.Add(
                        $"{culture} '{key}' uses {{{string.Join(",", actual)}}} where English uses {{{string.Join(",", expected)}}}");
                }
            }
        }

        Assert.True(mismatches.Count == 0,
            $"{mismatches.Count} localization table mismatch(es):{Environment.NewLine}  " +
            string.Join(Environment.NewLine + "  ", mismatches.Take(40)));

        var report = new MdSpecReport("L4-localization-tables", Layer, "L4 — localization tables",
            MdSpecPolicy.Load());
        report.Metric("localization_table_mismatches", mismatches.Count);
        report.Metric("localization_cultures", tables.Count);
        report.Metric("localization_keys", english.Count);
        report.Pass($"{tables.Count} culture(s), {english.Count} key(s), placeholders agree");
        report.Complete();
    }

    /// <summary>
    /// The same invariant against the table the runtime actually holds. The source scan above proves
    /// the file is well formed; this proves the initialiser that ran produced the same thing.
    /// </summary>
    [Fact]
    public void The_Runtime_Table_Matches_The_Source_Table()
    {
        var field = typeof(MdLocalization).GetField("Strings",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(field);

        // Strings is IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>: the nested
        // values are read-only, so the comparison goes through the generic interface rather than
        // through the non-generic IDictionary a mutable table would expose.
        var runtime = field!.GetValue(null);
        var outer = Assert.IsAssignableFrom<System.Collections.IEnumerable>(runtime);
        var actual = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        foreach (var pair in outer)
        {
            var pairType = pair.GetType();
            var culture = (string)pairType.GetProperty("Key")!.GetValue(pair)!;
            var table = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var entry in (System.Collections.IEnumerable)pairType.GetProperty("Value")!.GetValue(pair)!)
            {
                var entryType = entry.GetType();
                table[(string)entryType.GetProperty("Key")!.GetValue(entry)!] =
                    (string)entryType.GetProperty("Value")!.GetValue(entry)!;
            }

            actual[culture] = table;
        }

        var source = Tables(File.ReadAllText(LocalizationFile()))
            .ToDictionary(
                group => group.Key,
                group => group.SelectMany(pair => pair.Table)
                    .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal),
                StringComparer.Ordinal);

        Assert.Equal(source.Count, actual.Count);
        foreach (var (culture, table) in source)
        {
            Assert.True(actual.ContainsKey(culture), $"the runtime table has no '{culture}' entry");
            Assert.Equal(table.Count, actual[culture].Count);
            foreach (var (key, value) in table)
            {
                Assert.Equal(value, actual[culture][key]);
            }
        }
    }

    // ---------------------------------------------------------------------------------------

    private static string LocalizationFile() =>
        Path.Combine(MdSpecPaths.RequireRoot(), "src", "Md3.Avalonia", "Localization", "MdLocalization.cs");

    private static IEnumerable<string> SourceFiles() =>
        Directory
            .EnumerateFiles(Path.Combine(MdSpecPaths.RequireRoot(), "src"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains("/obj/", StringComparison.Ordinal)
                           && !path.Contains("/bin/", StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.Ordinal);

    private static IEnumerable<(string Culture, List<KeyValuePair<string, string>> Table)> Tables(string text)
    {
        var tables = Table.Matches(text);
        for (var index = 0; index < tables.Count; index++)
        {
            var culture = tables[index].Groups["culture"].Value;
            var end = index + 1 < tables.Count ? tables[index + 1].Index : text.Length;
            var block = text[tables[index].Index..end];
            var entries = Entry.Matches(block)
                .Select(match => new KeyValuePair<string, string>(
                    match.Groups["key"].Value,
                    Regex.Unescape(match.Groups["value"].Value)))
                .ToList();

            yield return (culture, entries);
        }
    }

    private static int[] Indices(string value) =>
        Placeholder.Matches(value)
            .Select(match => int.Parse(match.Groups["index"].Value, System.Globalization.CultureInfo.InvariantCulture))
            .Distinct()
            .Order()
            .ToArray();
}
