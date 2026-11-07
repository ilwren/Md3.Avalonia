using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>
/// Layer L4 — control template agreement.
/// </summary>
/// <remarks>
/// <para>
/// A <c>ControlTemplate</c> names its parts with <c>x:Name="PART_*"</c> and the control's own source
/// reaches them through <c>OnApplyTemplate</c>. Avalonia's contract for that pair is
/// <c>[TemplatePart]</c>: the attribute is what a consumer reads, what the documentation generation
/// lists, and what a rename has to keep in step.
/// </para>
/// <para>
/// Two measurements come out of that contract, and they are opposite in kind:
/// </para>
/// <list type="bullet">
///   <item>
///     <b>Gated:</b> a part the source declares with <c>[TemplatePart]</c> must exist in the
///     matching <c>ControlTheme</c>. An attribute that names a part no template provides is a
///     broken promise and fails the build.
///   </item>
///   <item>
///     <b>Reported:</b> a part a template names without a declaration. 470 of the 500 part names in
///     this repository are in that state, which is the actual size of the gap: the templates work,
///     but a rename on either side is caught by nothing.
///   </item>
/// </list>
/// <para>
/// The check parses the committed AXAML rather than loading every template in a headless window.
/// An open popup or a selected item is not reachable from a bare control, so a visual-tree version
/// of this test would depend on which sample value it picked; parsing makes it total.
/// </para>
/// </remarks>
public sealed class MdTemplateSourceTests
{
    private const string Layer = "L4_inventory";

    private static readonly Regex PartName = new(
        "x:Name=\"(?<name>PART_[A-Za-z0-9_]+)\"",
        RegexOptions.Compiled);

    private static readonly Regex AttributeName = new(
        "\\[TemplatePart\\(\"(?<name>PART_[A-Za-z0-9_]+)\"",
        RegexOptions.Compiled);

    /// <summary>
    /// Every part declared through <c>[TemplatePart]</c> has to be named by the template of the
    /// type that declares it. Zero today; a rename on either side fails here.
    /// </summary>
    [Fact]
    public void Declared_Template_Parts_Exist_In_Their_Template()
    {
        var declared = DeclaredParts();
        var themed = ThemedParts();
        var failures = new List<string>();

        foreach (var (type, parts) in declared.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (!themed.TryGetValue(type, out var actual))
            {
                failures.Add($"{type}: declares [TemplatePart] but no ControlTheme targets that type");
                continue;
            }

            foreach (var part in parts.Where(part => !actual.Contains(part)).OrderBy(name => name, StringComparer.Ordinal))
            {
                failures.Add($"{type}: [TemplatePart(\"{part}\")] but no template names that part");
            }
        }

        Assert.True(declared.Count > 0, "no [TemplatePart] attribute was found; the source scan is not working");
        Assert.True(failures.Count == 0,
            $"{failures.Count} declared template part(s) are missing from their template:" +
            $"{Environment.NewLine}  " + string.Join(Environment.NewLine + "  ", failures));

        var report = new MdSpecReport("L4-template-parts", Layer, "L4 — declared template parts", MdSpecPolicy.Load());
        foreach (var (type, parts) in declared.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            report.Pass($"{type} ({parts.Count} part(s))");
        }

        report.Metric("declared_template_parts", declared.Sum(pair => pair.Value.Count));
        report.Metric("types_with_template_part_attribute", declared.Count);
        report.Complete();
    }

    /// <summary>
    /// The inventory half: how much of each template is actually a contract, and how much is a name
    /// that only the AXAML knows about.
    /// </summary>
    [Fact]
    public void The_Layer_Measures_How_Much_Of_Each_Template_Is_A_Contract()
    {
        var declared = DeclaredParts();
        var themed = ThemedParts();
        var policy = MdSpecPolicy.Load();
        var report = new MdSpecReport("L4-template-coverage", Layer, "L4 — template coverage", policy);

        Assert.True(policy.IsLayerEnabled(Layer), "L4_inventory is disabled in spec-snapshot/conformance-policy.json.");

        var everyPart = themed.Values.SelectMany(parts => parts).ToList();
        var undeclared = themed
            .SelectMany(pair => pair.Value
                .Where(part => !declared.TryGetValue(pair.Key, out var list) || !list.Contains(part))
                .Select(part => $"{pair.Key}.{part}"))
            .OrderBy(entry => entry, StringComparer.Ordinal)
            .ToList();

        var typesWithoutAttribute = themed
            .Where(pair => pair.Value.Count > 0 && !declared.ContainsKey(pair.Key))
            .Select(pair => pair.Key)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        report.Metric("control_themes", themed.Count);
        report.Metric("template_part_names", everyPart.Count);
        report.Metric("declared_template_parts", everyPart.Count - undeclared.Count);

        report.Inventory("undeclared_template_parts", undeclared,
            "A PART_* name that exists only inside a ControlTheme. The template works, but nothing " +
            "outside the AXAML knows the name: a rename is caught by no test, no analyzer and no " +
            "documentation. Adding [TemplatePart] to the type is the fix, and it is mechanical.");

        report.Inventory("templates_without_part_contract", typesWithoutAttribute,
            "A themed control whose template names at least one part but whose type declares no " +
            "[TemplatePart] attribute at all, so every name in that template is unowned.");

        report.Metric("gated_template_parts", declared.Sum(pair => pair.Value.Count));
        report.Metric(
            "templates_with_a_part_contract",
            themed.Count(pair => declared.ContainsKey(pair.Key)));
        report.Complete();
    }

    // ---------------------------------------------------------------------------------------

    private static Dictionary<string, HashSet<string>> DeclaredParts()
    {
        var declared = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        var root = MdSpecPaths.RequireRoot();

        foreach (var file in SourceFiles(root, "*.cs"))
        {
            var text = File.ReadAllText(file);
            var matches = AttributeName.Matches(text);
            if (matches.Count == 0)
            {
                continue;
            }

            // The attribute sits directly above its class declaration, so the nearest class name
            // after each attribute belongs to it.
            foreach (Match match in matches)
            {
                var rest = text[match.Index..];
                var classMatch = Regex.Match(rest, "class\\s+(?<type>[A-Za-z0-9_]+)");
                if (!classMatch.Success)
                {
                    continue;
                }

                var type = classMatch.Groups["type"].Value;
                if (!declared.TryGetValue(type, out var parts))
                {
                    parts = new HashSet<string>(StringComparer.Ordinal);
                    declared[type] = parts;
                }

                parts.Add(match.Groups["name"].Value);
            }
        }

        return declared;
    }

    /// <summary>
    /// Every <c>PART_*</c> name each <c>ControlTheme</c> actually contains, keyed by target type.
    /// </summary>
    /// <remarks>
    /// This walks the parsed document, not the text between two open tags. A regex window that runs
    /// from one <c>&lt;ControlTheme&gt;</c> to the next swallows everything after a self-closing
    /// theme - nested grids, sibling templates and the generated item themes in between - and
    /// attributes the part names it finds there to the wrong control. The window was worth 39 false
    /// entries: <c>MdDataTable</c> was credited with <c>PART_ContinueButton</c>, which belongs to
    /// <c>MdStepper</c> twenty lines further down the same file. A control that declares a part on
    /// the strength of that number would promise something its own template never delivers.
    /// </remarks>
    private static Dictionary<string, HashSet<string>> ThemedParts()
    {
        var themed = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        var root = MdSpecPaths.RequireRoot();

        foreach (var file in SourceFiles(root, "*.axaml"))
        {
            XDocument document;
            try
            {
                document = XDocument.Load(file);
            }
            catch (System.Xml.XmlException exception)
            {
                throw new InvalidOperationException($"{file} is not well-formed XML", exception);
            }

            foreach (var theme in document.Descendants()
                         .Where(element => element.Name.LocalName == "ControlTheme"))
            {
                var type = theme.Attribute("TargetType")?.Value;
                if (type is null)
                {
                    continue;
                }

                type = type[(type.LastIndexOf(':') + 1)..];
                if (!themed.TryGetValue(type, out var parts))
                {
                    parts = new HashSet<string>(StringComparer.Ordinal);
                    themed[type] = parts;
                }

                foreach (var element in theme.Descendants())
                {
                    var name = element.Attributes()
                        .FirstOrDefault(attribute => attribute.Name.LocalName == "Name"
                                                     && attribute.Name.NamespaceName.Contains("xaml"))
                        ?.Value;
                    if (name is not null && name.StartsWith("PART_", StringComparison.Ordinal))
                    {
                        parts.Add(name);
                    }
                }
            }
        }

        return themed;
    }

    private static IEnumerable<string> SourceFiles(string root, string pattern) =>
        Directory.EnumerateFiles(Path.Combine(root, "src"), pattern, SearchOption.AllDirectories)
            .Where(path => !path.Contains("/obj/", StringComparison.Ordinal)
                           && !path.Contains("/bin/", StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.Ordinal);
}
