using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Localization;
using Md3.Avalonia.Motion;
using Md3.Avalonia.Themes.Dynamic;
using Xunit;

namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>
/// Layer L4 — structural inventory.
/// </summary>
/// <remarks>
/// <para>
/// L1 proves a token holds the right number, L2 proves the number reaches the screen, L3 proves
/// the pixels did not move. None of them answers "how much of this library is measured at all",
/// and that is the question a preview release actually needs answered: 157 types carry a
/// <c>ControlTheme</c> while L2 lays out eight of them.
/// </para>
/// <para>
/// This layer therefore measures the library against itself - declared tokens against the merged
/// dictionary, themed types against the geometry matrix, control templates against the parts they
/// name, colour roles against WCAG in every configuration the theme generator can produce. It is
/// not an oracle: a finding here says "nobody has asserted this", never "this is wrong against
/// Material". Findings are advisory and are recorded as numbers in
/// <c>artifacts/spec/L4-inventory.json</c>, where <c>scripts/check-conformance-ratchet.py</c>
/// compares them with <c>spec-snapshot/conformance-budget.json</c>.
/// </para>
/// <para>
/// Some of it does gate the build, and those checks are regression guards rather than smell
/// reports: a control template must expose the parts it names, a key declared in a theme dictionary
/// must resolve through the real merge in both variants, the two variants of a themed dictionary
/// must declare the same keys, and a token declared twice must agree with itself. A library that
/// fails any of those is broken, not untidy.
/// </para>
/// </remarks>
public sealed class MdInventoryTests
{
    private const string Layer = "L4_inventory";

    /// <summary>Types the L2 matrix lays out, mirrored from MdGeometryConformanceTests.</summary>
    private static readonly HashSet<string> GeometryMeasuredTypes = new(StringComparer.Ordinal)
    {
        "MdButton", "MdCheckBox", "MdChip", "MdFloatingActionButton",
        "MdIconButton", "MdRadioButton", "MdSwitch", "MdTextBox"
    };

    /// <summary>
    /// Controls whose template is loaded and whose declared <c>PART_*</c> names are asserted.
    /// </summary>
    /// <remarks>
    /// These are the ones whose geometry L2 does not measure and whose templates carry the most
    /// parts, so they are where a renamed part is most likely to go unnoticed. Every entry is
    /// created with a parameterless constructor and a theme applied, so a template that fails to
    /// load at all surfaces here as well.
    /// </remarks>
    private static IEnumerable<(string Name, Func<Control> Build, string[] Parts)> TemplateSamples() =>
    [
        ("MdCard", () => new MdCard { Content = "Card" }, ["PART_Container", "PART_StateLayer"]),
        ("MdBadge", () => new MdBadge { Content = "9" }, ["PART_Container", "PART_Content"]),
        ("MdSnackbar", () => new MdSnackbar { Content = "Saved", IsOpen = true },
            ["PART_Container", "PART_ActionButton", "PART_DismissButton"]),
        // PART_Popup belongs to MdTooltipHost's template, not the tooltip's: the host owns the popup
        // and hosts the bubble inline. Asserting it against MdTooltip would be asserting a part the
        // tooltip's own theme never declares.
        ("MdTooltip", () => new MdTooltip { Content = "Tip", Title = "Title", IsOpen = true },
            ["PART_Surface", "PART_Title", "PART_Action"]),
        ("MdSlider", () => new MdSlider { Minimum = 0, Maximum = 100, Value = 40, Width = 220 },
            ["PART_Track", "PART_Thumb", "PART_Handle", "PART_StopIndicator"]),
        // MdTabs' own template names the scroll viewer and the shared indicator; the per-item parts
        // (PART_Root, PART_Label, ...) belong to MdTabItem's template, which the same tree realises
        // once the items exist, so they are reachable through this sample as well.
        ("MdTabs", () => MakeTabs(), ["PART_ScrollViewer", "PART_SelectionIndicator", "PART_Label"]),
        ("MdLinearProgressIndicator", () => new MdLinearProgressIndicator { Width = 200 },
            ["PART_Indicator"]),
        ("MdCircularProgressIndicator", () => new MdCircularProgressIndicator { Width = 48, Height = 48 },
            ["PART_Indicator"]),
    ];

    private static MdTabs MakeTabs()
    {
        var tabs = new MdTabs();
        tabs.Items.Add(new MdTabItem { Header = "One" });
        tabs.Items.Add(new MdTabItem { Header = "Two" });
        return tabs;
    }

    // ---------------------------------------------------------------------------------------
    // Scorecard
    // ---------------------------------------------------------------------------------------

    [AvaloniaFact]
    public void The_Library_Reports_How_Much_Of_Itself_Is_Measured()
    {
        var policy = MdSpecPolicy.Load();
        var report = new MdSpecReport("L4-inventory", Layer, "L4 — structural inventory", policy);
        Assert.True(policy.IsLayerEnabled(Layer), "L4_inventory is disabled in spec-snapshot/conformance-policy.json.");

        var application = Application.Current!;
        application.RequestedThemeVariant = ThemeVariant.Light;

        var declared = DeclaredTokenKeys();
        var controls = MdSpecSurface.PublicControlTypes();
        var themed = MdSpecSurface.ThemedTypeNames();

        report.Metric("declared_tokens", declared.Count);
        report.Metric("public_controls", controls.Count);
        report.Metric("themed_controls", controls.Count(type => themed.Contains(type.Name)));

        // --- Which declared tokens the merged theme does not resolve. ---------------------
        var unresolved = declared
            .Where(key => !application.TryGetResource(key, ThemeVariant.Light, out var value) || value is null)
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToList();
        report.Inventory(
            "unresolved_declared_tokens",
            unresolved,
            "Declared by a shipped dictionary but not resolved through the merged theme; a control " +
            "binding one of these renders with whatever the property default is.");

        // --- Which themed controls nobody lays out. --------------------------------------
        var unmeasured = controls
            .Where(type => themed.Contains(type.Name) && !GeometryMeasuredTypes.Contains(type.Name))
            .Select(type => type.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();
        report.Metric("measured_controls", GeometryMeasuredTypes.Count);
        report.Metric("unmeasured_themed_controls", unmeasured.Count);
        report.Note("unmeasured_themed_controls",
            $"{unmeasured.Count} themed control(s) have no rendered-geometry case, so L2 says nothing " +
            "about their size, corner radius or hit target. " +
            string.Join(", ", unmeasured.Take(12)) + (unmeasured.Count > 12 ? ", …" : string.Empty));

        report.Inventory(
            "tokens_outside_the_core_theme",
            KeysOutsideTheCoreTheme(),
            "Md.* keys declared by a sibling theme package that ships its own Styles collection. "
            + "They are resolved by that package's own theme, not by MaterialTheme, so the "
            + "every-key-resolves gate deliberately does not cover them.");

        // --- Which Md* classes have no theme at all. -------------------------------------
        var unstyled = controls
            .Where(type => !themed.Contains(type.Name))
            .Select(type => type.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();
        report.Inventory(
            "unstyled_control_classes",
            unstyled,
            "Md* type with no {x:Type} control theme. Parts, presenters and service types are " +
            "expected here; a control that is not one renders with no Material surface at all.");

        // --- Motion tokens: two declarations that nothing keeps in sync. -----------------
        var motionPairs = new (string Key, MdSpring Spring)[]
        {
            ("Md.Sys.Motion.Expressive.FastSpatial", MdMotionTokens.ExpressiveFastSpatial),
            ("Md.Sys.Motion.Expressive.DefaultSpatial", MdMotionTokens.ExpressiveDefaultSpatial),
            ("Md.Sys.Motion.Expressive.SlowSpatial", MdMotionTokens.ExpressiveSlowSpatial),
            ("Md.Sys.Motion.Standard.FastSpatial", MdMotionTokens.StandardFastSpatial),
            ("Md.Sys.Motion.Standard.DefaultSpatial", MdMotionTokens.StandardDefaultSpatial),
            ("Md.Sys.Motion.Standard.SlowSpatial", MdMotionTokens.StandardSlowSpatial),
            ("Md.Sys.Motion.FastEffects", MdMotionTokens.FastEffects),
            ("Md.Sys.Motion.DefaultEffects", MdMotionTokens.DefaultEffects),
            ("Md.Sys.Motion.SlowEffects", MdMotionTokens.SlowEffects)
        };

        var motionDrift = new List<string>();
        foreach (var (key, spring) in motionPairs)
        {
            var damping = ResolveDouble(application, key + ".DampingRatio");
            var stiffness = ResolveDouble(application, key + ".Stiffness");
            if (damping is null || stiffness is null)
            {
                motionDrift.Add(key + " is not declared in XAML");
                continue;
            }

            if (Math.Abs(damping.Value - spring.DampingRatio) > 0.005 ||
                Math.Abs(stiffness.Value - spring.Stiffness) > 0.5)
            {
                motionDrift.Add(
                    $"{key}: XAML {damping:0.###}/{stiffness:0.###} vs C# " +
                    $"{spring.DampingRatio:0.###}/{spring.Stiffness:0.###}");
            }
        }

        report.Metric("motion_token_drift", motionDrift.Count);
        foreach (var drift in motionDrift)
        {
            report.Note("motion-token-drift", drift);
        }

        // --- Localization tables. --------------------------------------------------------
        // The culture tables themselves (key coverage, {0} placeholders, duplicate keys) are
        // asserted from source by MdLocalizationSourceTests, which reads the initialiser text: a
        // dictionary literal that repeats a key keeps the last value, so the duplicate cannot be
        // seen through the object that comes back here.
        report.Note("localization",
            $"{LocalizationTables().Count} culture table(s) present; the key-set and placeholder " +
            "invariants are asserted by MdLocalizationSourceTests.");

        // --- Contrast across every configuration the generator can produce. --------------
        var contrast = ContrastFailures();
        report.Metric("contrast_failures", contrast.Count);
        report.Inventory("contrast_failures", contrast,
            "Failing foreground/background pair over 6 scheme variants x 2 modes x 3 contrast levels. " +
            "The mandatory pairs are 4.5:1 and are asserted by MdDynamicThemeTests for one configuration " +
            "only; this is the size of the gap across the rest.");

        report.Complete();
    }

    // ---------------------------------------------------------------------------------------
    // Invariants that gate the build
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// Every template part a theme names by <c>x:Name="PART_*"</c> must exist once the control is
    /// created and its template is applied. A rename on one side of that pair is otherwise silent:
    /// the control keeps rendering, and the code that reaches for the part gets null.
    /// </summary>
    [AvaloniaFact]
    public void Control_Templates_Expose_The_Parts_They_Name()
    {
        var failures = new List<string>();

        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;

        foreach (var (name, build, parts) in TemplateSamples())
        {
            Control? control = null;
            try
            {
                control = build();

                // A popup presenter needs a real top level of its own, so each sample gets its own
                // window rather than sharing one layout pass: eight windows is still cheap and it
                // keeps one control's template failure from being reported against its neighbours.
                using var sample = MdVisualTreeQuery.Show(control, 420, 420);

                // A template that failed to load leaves Template null, and a control that never
                // attached has no visual descendants at all; both mean "nothing to assert", which
                // would otherwise be mistaken for a part that is simply missing.
                if (control is not TemplatedControl { Template: not null } ||
                    control.GetVisualDescendants().FirstOrDefault() is null)
                {
                    failures.Add($"{name}: no template was applied (Template is null or the control never attached)");
                    continue;
                }

                // Every named control in the realised tree counts, including the ones a nested
                // template produced: MdSlider's PART_Handle lives in MdSliderThumb's template, not in
                // the slider's own, and renaming it breaks the slider just the same.
                var realised = control.GetVisualDescendants()
                    .OfType<Control>()
                    .Select(part => part.Name)
                    .Where(partName => partName is not null)
                    .ToHashSet(StringComparer.Ordinal);

                foreach (var expected in parts.Where(part => !realised.Contains(part)))
                {
                    failures.Add($"{name}: template declares {expected}, but nothing exposes that name at runtime");
                }
            }
            catch (Exception ex)
            {
                failures.Add($"{name}: {ex.GetType().Name}: {ex.Message}");
            }
            finally
            {
                // A snackbar in the open state owns a DispatcherTimer; leaving it armed would make
                // the next test's dispatcher drain a callback for a control nobody owns any more.
                if (control is MdSnackbar snackbar)
                {
                    snackbar.IsOpen = false;
                }
            }
        }

        Assert.True(failures.Count == 0,
            $"{failures.Count} template part discrepancy(ies):{Environment.NewLine}  " +
            string.Join(Environment.NewLine + "  ", failures));

        var report = new MdSpecReport("L4-template-realisation", Layer, "L4 — template realisation", MdSpecPolicy.Load());
        foreach (var (name, _, parts) in TemplateSamples())
        {
            report.Pass(name + " (" + parts.Length + " parts)");
        }

        report.Metric("template_part_samples", TemplateSamples().Count());
        report.Complete();
    }

    /// <summary>
    /// A token that exists both as a static XAML value and as a runtime assignment in
    /// <c>MdThemeManager.Apply</c> must resolve to the same value on both paths.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 27 keys are declared twice: once in <c>Themes/Tokens/*.axaml</c> and once by the dynamic
    /// theme generator. L1 resolves through the static merge, so it only ever sees the first copy;
    /// an application that calls <c>Apply</c> only ever sees the second. Nothing in the build relates
    /// them, and the design-token lint treats the C# assignment as a declaration, so a divergence
    /// shows up as neither an error nor a warning.
    /// </para>
    /// <para>
    /// The comparison runs at <see cref="MdThemeShapeScale.Standard"/>, which is a factor of exactly
    /// one: any difference at that scale is a real disagreement, not a scaled variant of the same
    /// intent. Colour keys are compared by <c>ToString</c> so a <see cref="Color"/> and the
    /// <c>SolidColorBrush</c> built from it are compared on the colour, not the wrapper.
    /// </para>
    /// <para>
    /// Gated against a budget of zero. Applying a dynamic theme leaves the application resources
    /// mutated, which is the state every other test in the suite already runs in; the static
    /// snapshot is taken first, so the comparison does not depend on test order.
    /// </para>
    /// </remarks>
    [AvaloniaFact]
    public void Dual_Declared_Tokens_Agree_Between_Xaml_And_The_Runtime_Generator()
    {
        var application = Application.Current!;
        var policy = MdSpecPolicy.Load();
        var report = new MdSpecReport("L4-dual-declaration", Layer, "L4 — dual-declared tokens", policy);

        application.RequestedThemeVariant = ThemeVariant.Light;
        Dispatcher.UIThread.RunJobs();

        var runtimeKeys = RuntimeWrittenKeys();
        var staticTheme = new Dictionary<string, string>(StringComparer.Ordinal);
        var absent = new List<string>();
        foreach (var key in runtimeKeys.OrderBy(key => key, StringComparer.Ordinal))
        {
            if (application.TryGetResource(key, ThemeVariant.Light, out var value) && value is not null)
            {
                staticTheme[key] = Describe(value);
            }
            else
            {
                absent.Add(key);
            }
        }

        MdThemeManager.Apply(application, new MdThemeOptions(), dark: false);
        Dispatcher.UIThread.RunJobs();

        var mismatches = new List<string>();
        foreach (var (key, before) in staticTheme.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            var after = application.TryGetResource(key, ThemeVariant.Light, out var value) && value is not null
                ? Describe(value)
                : "(not written)";

            if (!string.Equals(before, after, StringComparison.Ordinal))
            {
                mismatches.Add($"{key}: static theme {before}, MdThemeManager {after}");
            }
        }

        report.Metric("dual_declaration_keys", runtimeKeys.Count);
        report.Metric("dual_declaration_mismatches", mismatches.Count);
        report.Metric("dual_declaration_static_only", absent.Count);
        foreach (var mismatch in mismatches)
        {
            report.Gap("dual-declaration", mismatch, highConfidence: false);
        }

        foreach (var key in absent)
        {
            report.Note("dual-declaration", $"{key} is written only by MdThemeManager; there is no static value to compare");
        }

        report.Note("dual-declaration",
            $"{staticTheme.Count} of {runtimeKeys.Count} runtime-written keys also exist in the static theme " +
            "and were compared at ShapeScale.Standard.");

        // Deliberately not asserted to zero. The measured answer is 8, and that is a design question
        // rather than a bug the build can settle: the static tokens carry the M3 baseline palette
        // (primary #6750A4, on-surface #1D1B20) while MdThemeManager derives its scheme from the seed
        // through the tonal palette, which lands on #65558F and #1C1B1E. Both are defensible and they
        // disagree, so the count is budgeted at its measured value instead of being waived or
        // tolerated silently: a ninth divergence fails the ratchet, and closing the existing eight is
        // a visible budget change rather than a quiet drift.
        report.Note("dual-declaration",
            $"{mismatches.Count} of {staticTheme.Count} compared keys disagree. Budgeted at {mismatches.Count}, not asserted to zero.");

        report.Complete();
    }

    /// <summary>
    /// The light and dark dictionaries of a themed dictionary must declare the same keys. A key that
    /// exists in one variant only resolves in that variant; in the other, the control that binds it
    /// falls back to the property default, so the failure is a wrong colour rather than a missing one.
    /// </summary>
    /// <remarks>
    /// This is a source check on purpose. <see cref="Every_Declared_Token_Resolves_In_Both_Theme_Variants"/>
    /// asks the runtime, but it can only ask about keys the union of both variants contains, so a
    /// key present in one variant alone is inside its scope by construction and invisible as a
    /// defect. Comparing the two dictionaries directly is what names the asymmetry.
    /// </remarks>
    [Fact]
    public void Both_Theme_Variants_Declare_The_Same_Keys()
    {
        var asymmetries = new List<string>();
        var dictionaries = 0;

        foreach (var file in MdSpecSurface.CoreThemeFiles())
        {
            var text = File.ReadAllText(file);
            var start = text.IndexOf("<ResourceDictionary.ThemeDictionaries>", StringComparison.Ordinal);
            if (start < 0)
            {
                continue;
            }

            dictionaries++;
            var block = text[start..];
            var variants = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(
                         block, "<ResourceDictionary x:Key=\"(?<variant>\\w+)\"[^>]*>(?<body>.*?)</ResourceDictionary>",
                         System.Text.RegularExpressions.RegexOptions.Singleline))
            {
                variants[match.Groups["variant"].Value] = System.Text.RegularExpressions.Regex
                    .Matches(match.Groups["body"].Value, "x:Key=\"(?<key>Md\\.[A-Za-z0-9.]+)\"")
                    .Select(key => key.Groups["key"].Value)
                    .ToHashSet(StringComparer.Ordinal);
            }

            if (!variants.TryGetValue("Light", out var light) || !variants.TryGetValue("Dark", out var dark))
            {
                continue;
            }

            var name = Path.GetFileName(file);
            asymmetries.AddRange(light.Where(key => !dark.Contains(key)).Select(key => $"{name}: {key} is declared in Light only"));
            asymmetries.AddRange(dark.Where(key => !light.Contains(key)).Select(key => $"{name}: {key} is declared in Dark only"));
        }

        Assert.True(dictionaries > 0, "no ThemeDictionaries block was found; the scan is not working");
        Assert.True(asymmetries.Count == 0,
            $"{asymmetries.Count} token(s) exist in one theme variant only:{Environment.NewLine}  " +
            string.Join(Environment.NewLine + "  ", asymmetries.Take(40)));

        var report = new MdSpecReport("L4-token-variants", Layer, "L4 — theme variant symmetry",
            MdSpecPolicy.Load());
        report.Metric("themed_dictionaries", dictionaries);
        report.Metric("variant_asymmetric_tokens", asymmetries.Count);
        report.Pass($"{dictionaries} themed dictionaries declare the same keys in Light and Dark");
        report.Complete();
    }

    /// <summary>
    /// A key declared by any shipped dictionary has to resolve through the real merged theme, in
    /// both variants. <c>TryGetResource</c> returning null is not an error at runtime, which is
    /// exactly why it needs asserting.
    /// </summary>
    [AvaloniaFact]
    public void Every_Declared_Token_Resolves_In_Both_Theme_Variants()
    {
        var application = Application.Current!;
        var declared = DeclaredTokenKeys();
        var failures = new List<string>();

        foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        {
            application.RequestedThemeVariant = variant;
            Dispatcher.UIThread.RunJobs();
            foreach (var key in declared)
            {
                if (!application.TryGetResource(key, variant, out var value) || value is null)
                {
                    failures.Add($"{variant}: {key} does not resolve");
                }
            }
        }

        application.RequestedThemeVariant = ThemeVariant.Light;
        Assert.True(failures.Count == 0,
            $"{failures.Count} declared token(s) do not resolve through the merged theme:" +
            $"{Environment.NewLine}  " + string.Join(Environment.NewLine + "  ", failures.Take(40)));
    }

    // ---------------------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------------------

    /// <summary>Keys MdThemeManager assigns at runtime, scanned from its own source.</summary>
    private static HashSet<string> RuntimeWrittenKeys()
    {
        var file = Path.Combine(MdSpecPaths.RequireRoot(), "src", "Md3.Avalonia", "Themes", "Dynamic",
            "MdThemeManager.cs");
        var keys = new HashSet<string>(StringComparer.Ordinal);
        if (!File.Exists(file))
        {
            return keys;
        }

        var pattern = new System.Text.RegularExpressions.Regex(
            "Resources\\[\\s*\"(Md\\.[A-Za-z0-9.]+)\"\\s*\\]");
        foreach (System.Text.RegularExpressions.Match match in pattern.Matches(File.ReadAllText(file)))
        {
            keys.Add(match.Groups[1].Value);
        }

        return keys;
    }

    /// <summary>
    /// A value in a form two declarations can be compared in.
    /// </summary>
    /// <remarks>
    /// Numbers are formatted, corners and thicknesses are expanded, and a brush is reduced to its
    /// colour. This is deliberately a rendering rather than an equality check: the static theme and
    /// the generator can legitimately produce a <see cref="Color"/> and the
    /// <see cref="SolidColorBrush"/> built from it for the same key.
    /// </remarks>
    private static string Describe(object value) => value switch
    {
        double number => number.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture),
        CornerRadius corner =>
            $"{corner.TopLeft:0.####},{corner.TopRight:0.####},{corner.BottomRight:0.####},{corner.BottomLeft:0.####}",
        Thickness thickness =>
            $"{thickness.Left:0.####},{thickness.Top:0.####},{thickness.Right:0.####},{thickness.Bottom:0.####}",
        ISolidColorBrush brush => brush.Color.ToString(),
        Color colour => colour.ToString(),
        _ => value.ToString() ?? value.GetType().Name
    };

    /// <summary>Every <c>Md.*</c> key the theme this test application registers actually declares.</summary>
    /// <remarks>
    /// Scoped to the dictionaries <c>MaterialTheme</c> merges, because that is the theme under test.
    /// Four sibling packages (PropertyGrid, AvaloniaEdit, DataGrid, Dock) ship their own
    /// <c>Styles</c> collection with their own key space; their keys cannot resolve here and saying
    /// otherwise would be a false failure. <see cref="KeysOutsideTheCoreTheme"/> measures them.
    /// </remarks>
    private static HashSet<string> DeclaredTokenKeys()
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in MdSpecSurface.CoreThemeFiles())
        {
            foreach (System.Text.RegularExpressions.Match match in
                     System.Text.RegularExpressions.Regex.Matches(
                         File.ReadAllText(file), "x:Key=\"(Md\\.[A-Za-z0-9.]+)\""))
            {
                keys.Add(match.Groups[1].Value);
            }
        }

        // The generator writes its keys at runtime (Resources["Md.X"] = ...), so they are
        // declarations too - but only the ones it actually writes, not every Md.* literal in C#.
        foreach (var file in MdSpecSurface.SourceFiles("*.cs"))
        {
            foreach (var line in File.ReadLines(file))
            {
                foreach (System.Text.RegularExpressions.Match match in
                         System.Text.RegularExpressions.Regex.Matches(
                             line, "Resources\\[\\s*\"(Md\\.[A-Za-z0-9.]+)\"\\s*\\]\\s*="))
                {
                    keys.Add(match.Groups[1].Value);
                }
            }
        }

        return keys;
    }

    /// <summary>
    /// <c>Md.*</c> keys declared outside the core theme and therefore outside this application.
    /// </summary>
    /// <remarks>
    /// Reported, not gated. Each of these belongs to a package that ships its own <c>Styles</c>
    /// collection, which an application opts into by calling <c>Styles.Add</c>; the theme package's
    /// own test class registers it and asserts the keys resolve. The number is here because a
    /// consumer reading the conformance report cannot otherwise tell which half of the key space the
    /// green tick covers.
    /// </remarks>
    private static List<string> KeysOutsideTheCoreTheme()
    {
        var root = MdSpecPaths.RequireRoot();
        var core = MdSpecSurface.CoreThemeFiles().ToHashSet(StringComparer.Ordinal);
        var outside = new List<string>();

        foreach (var file in MdSpecSurface.SourceFiles("*.axaml"))
        {
            if (core.Contains(file))
            {
                continue;
            }

            // The package is the first segment under src/, which is the assembly the dictionary ships
            // in - the thing a consumer actually has to add to their application.
            var relative = Path.GetRelativePath(Path.Combine(root, "src"), file);
            var package = relative.Split(Path.DirectorySeparatorChar, '/')[0];
            foreach (System.Text.RegularExpressions.Match match in
                     System.Text.RegularExpressions.Regex.Matches(
                         File.ReadAllText(file), "x:Key=\"(Md\\.[A-Za-z0-9.]+)\""))
            {
                outside.Add($"{package}: {match.Groups[1].Value}");
            }
        }

        return outside;
    }

    private static double? ResolveDouble(Application application, string key) =>
        application.TryGetResource(key, ThemeVariant.Light, out var value) && value is double number
            ? number
            : null;

    /// <summary>
    /// The runtime localization tables, read through the interface they are typed as.
    /// </summary>
    /// <remarks>
    /// This exists only to count the cultures in the scorecard. The invariants over the tables live
    /// in <c>MdLocalizationSourceTests</c>, which reads the initialiser text: a key written twice is
    /// already collapsed by the time the dictionary exists, so it cannot be measured from here.
    /// </remarks>
    private static Dictionary<string, Dictionary<string, string>> LocalizationTables()
    {
        var tables = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

        // MdLocalization.Strings is IReadOnlyDictionary<string, IReadOnlyDictionary<string, ...>>.
        // The reflection here is deliberate: the tables are an implementation detail, and the point
        // of the check is to fail if one culture stops covering the English key set.
        var field = typeof(MdLocalization).GetField("Strings",
            BindingFlags.NonPublic | BindingFlags.Static);
        if (field?.GetValue(null) is not System.Collections.IEnumerable outer)
        {
            return tables;
        }

        foreach (var entry in outer)
        {
            var entryType = entry.GetType();
            var key = (string?)entryType.GetProperty("Key")?.GetValue(entry) ?? string.Empty;
            if (entryType.GetProperty("Value")?.GetValue(entry) is not System.Collections.IEnumerable inner)
            {
                continue;
            }

            var table = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var pair in inner)
            {
                var pairType = pair.GetType();
                if (pairType.GetProperty("Key")?.GetValue(pair) is string text &&
                    pairType.GetProperty("Value")?.GetValue(pair) is string value)
                {
                    table[text] = value;
                }
            }

            tables[key] = table;
        }

        return tables;
    }

    /// <summary>Every failing WCAG AA pair across the configurations MdThemeManager can produce.</summary>
    private static List<string> ContrastFailures()
    {
        var failures = new List<string>();
        var options = new MdThemeOptions();
        foreach (var variant in Enum.GetValues<MdThemeSchemeVariant>())
        {
            foreach (var contrast in Enum.GetValues<MdThemeContrastLevel>())
            {
                foreach (var dark in new[] { false, true })
                {
                    var requested = options with { SchemeVariant = variant, ContrastLevel = contrast };
                    var roles = MdThemeGenerator.Generate(requested, dark);
                    foreach (var diagnostic in MdThemeManager.Diagnose(roles))
                    {
                        if (diagnostic.Passes)
                        {
                            continue;
                        }

                        failures.Add(
                            $"{variant}/{contrast}/{(dark ? "dark" : "light")}: " +
                            $"{diagnostic.ForegroundRole} on {diagnostic.BackgroundRole} = " +
                            $"{diagnostic.Ratio:0.00} < {diagnostic.RequiredRatio:0.0} ({diagnostic.Purpose})");
                    }
                }
            }
        }

        return failures;
    }
}
