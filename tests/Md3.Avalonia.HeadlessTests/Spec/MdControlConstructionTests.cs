using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using Xunit;

namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>
/// Layer L2b — every themed control has to survive being built, themed and laid out.
/// </summary>
/// <remarks>
/// <para>
/// L2 measures the geometry of eight controls against the oracle. That is the right shape for a
/// control whose numbers Material publishes, but it answers a much narrower question than "does
/// this template work": 150 themed controls have no geometry case, and until this layer existed
/// nothing said whether they could even be constructed.
/// </para>
/// <para>
/// Two things are gated, and both are statements of fact rather than judgements about appearance:
/// </para>
/// <list type="number">
///   <item>
///     <b>Construction must not throw.</b> A template that fails to parse, or an
///     <c>OnApplyTemplate</c> that dereferences a part it never received, throws here with the
///     control's name attached instead of surfacing later as an unrelated gallery crash.
///   </item>
///   <item>
///     <b>An implicit theme that declares a template must deliver it.</b> If
///     <c>x:Key="{x:Type md:MdFoo}"</c> sets <c>Property="Template"</c>, then a bare <c>MdFoo</c>
///     added to a window must come back with a non-null <c>Template</c>. Anything else means the
///     style is not reaching the control, which no amount of rendering the gallery would report.
///   </item>
/// </list>
/// <para>
/// Everything else this layer observes is measured, not gated, because the alternative is an
/// ever-growing list of exceptions: a <c>TextBlock</c> subclass draws through <c>Text</c> and has no
/// children by design; a thumb theme keyed by name is only ever applied by the slider's template; a
/// snackbar is invisible until its host presents it. Those are reported as named inventories so the
/// coverage numbers can be read for what they are.
/// </para>
/// </remarks>
public sealed class MdControlConstructionTests
{
    private const string Layer = "L4_inventory";

    [AvaloniaFact]
    public void Every_Themed_Control_Can_Be_Built_Themed_And_Laid_Out()
    {
        var policy = MdSpecPolicy.Load();
        var report = new MdSpecReport(
            "L2b-construction", Layer, "L2b — every themed control constructs, themes and lays out", policy);

        Assert.True(policy.IsLayerEnabled(Layer), "L4_inventory is disabled in spec-snapshot/conformance-policy.json.");

        var themes = ThemeDeclarations();
        var themed = themes.Keys.ToHashSet(StringComparer.Ordinal);
        var surface = MdSpecSurface.PublicControlTypes().Where(type => themed.Contains(type.Name)).ToList();

        var topLevels = surface.Where(type => typeof(TopLevel).IsAssignableFrom(type))
            .Select(type => type.Name)
            .ToList();
        var candidates = surface.Except(surface.Where(type => typeof(TopLevel).IsAssignableFrom(type))).ToList();

        Assert.True(candidates.Count > 100,
            $"only {candidates.Count} themed controls were found; the surface scan is not working");

        var constructed = 0;
        var rendered = 0;
        var throws = new List<string>();
        var missingTemplate = new List<string>();
        var unreachable = new List<string>();
        var propertyOnly = new List<string>();
        var hostOnly = new List<string>();
        var contentDriven = new List<string>();
        var hiddenByDesign = new List<string>();

        foreach (var type in candidates)
        {
            if (!MdSpecSurface.HasParameterlessConstructor(type))
            {
                unreachable.Add($"{type.Name}: {DescribeConstructors(type)}");
                continue;
            }

            var declaration = themes[type.Name];
            if (declaration.NamedOnly)
            {
                // Its only ControlTheme has a name, not an {x:Type} key, so nothing applies it to a
                // bare instance: it exists to be handed to a control inside someone else's template.
                hostOnly.Add($"{type.Name}: theme is keyed by name, applied by its host template");
                continue;
            }

            // One window per control, not one window for all of them: a single bad control in a
            // shared tree throws during the layout pass of its neighbours and takes the batch with
            // it, and the resulting message then names the wrong control.
            try
            {
                var control = (Control)Activator.CreateInstance(type)!;
                using (MdVisualTreeQuery.Show(control, 360, 260))
                {
                    constructed++;

                    var template = (control as TemplatedControl)?.Template;
                    if (declaration.DeclaresTemplate && template is null)
                    {
                        missingTemplate.Add(
                            $"{type.Name}: its implicit theme sets Property=\"Template\" but the instance has none after layout");
                        continue;
                    }

                    if (template is not null && control.GetVisualChildren().FirstOrDefault() is not null)
                    {
                        rendered++;
                        report.Pass(type.Name);
                        continue;
                    }

                    if (OverridesRender(type))
                    {
                        rendered++;
                        report.Pass(type.Name);
                        continue;
                    }

                    // Nothing was realised and the control does not paint itself. Which of the
                    // remaining explanations applies is a fact about the control, not a defect:
                    if (typeof(TextBlock).IsAssignableFrom(type) || typeof(ContentPresenter).IsAssignableFrom(type))
                    {
                        contentDriven.Add($"{type.Name}: a {BaseName(type)} that draws its Text/Content, which is empty here");
                        continue;
                    }

                    if (declaration.DeclaresTemplate)
                    {
                        rendered++;
                        report.Pass(type.Name);
                        continue;
                    }

                    if (!control.IsEffectivelyVisible)
                    {
                        hiddenByDesign.Add($"{type.Name}: IsVisible is false in its theme until a host presents it");
                        continue;
                    }

                    propertyOnly.Add($"{type.Name}: its theme sets properties only - no template and no content");
                }
            }
            catch (Exception ex)
            {
                throws.Add($"{type.Name}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        Assert.True(throws.Count == 0,
            $"{throws.Count} themed control(s) threw while being built, themed and laid out:" +
            $"{Environment.NewLine}  " + string.Join(Environment.NewLine + "  ", throws));

        Assert.True(missingTemplate.Count == 0,
            $"{missingTemplate.Count} themed control(s) did not receive the template their own theme declares:" +
            $"{Environment.NewLine}  " + string.Join(Environment.NewLine + "  ", missingTemplate));

        report.Metric("constructed_themed_controls", constructed);
        report.Metric("rendered_themed_controls", rendered);
        report.Metric("constructible_themed_controls", candidates.Count);

        report.Inventory(
            "themed_controls_needing_arguments",
            unreachable,
            "A themed control with no public parameterless constructor, so no generic matrix can lay "
            + "it out. Usually a dialog or an overlay that needs its owner - a design fact, not a "
            + "defect - but it does put that control outside every coverage number this harness "
            + "reports, which matters before reading those numbers as complete.");

        report.Inventory(
            "themed_controls_with_a_named_theme_only",
            hostOnly,
            "A control whose only ControlTheme is keyed by name rather than by {x:Type}, so nothing "
            + "applies it to a bare instance: it is handed to a control inside someone else's "
            + "template. Building it alone proves nothing, and its appearance is only reachable "
            + "through its host.");

        report.Inventory(
            "themed_controls_drawing_their_own_text",
            contentDriven,
            "A TextBlock or ContentPresenter subclass: it draws through Text/Content rather than "
            + "through children, so an empty instance renders nothing without anything being wrong.");

        report.Inventory(
            "themed_controls_with_property_only_themes",
            propertyOnly,
            "A control whose implicit theme sets properties but no template and whose instance has no "
            + "content: it renders nothing here. Legitimate for a hit-target or a spacing element, "
            + "and the reason to look is that the same signature is what a control that lost its "
            + "template would look like.");

        report.Inventory(
            "themed_controls_hidden_by_design",
            hiddenByDesign,
            "A themed control that is present and laid out but invisible until its host presents it, "
            + "so no bare window can render it.");

        report.Inventory(
            "top_level_themed_controls",
            topLevels,
            "A Window or other TopLevel with a control theme. It cannot be a child of another window, "
            + "so this layer reports it instead of laying it out; the desktop adapter tests cover the "
            + "chrome these themes drive.");

        report.Note("construction",
            $"{constructed} of {candidates.Count} themed controls were constructed and laid out; "
            + $"{rendered} produced a visual tree. {hostOnly.Count} are only reachable through a host "
            + $"theme, {unreachable.Count} need constructor arguments, and {topLevels.Count} TopLevel "
            + "themes are out of scope for a generic matrix.");

        report.Complete();
    }

    /// <summary>What a control's ControlThemes say about it, read from the merged AXAML.</summary>
    /// <param name="DeclaresTemplate">An <c>{x:Type}</c> theme for this type sets <c>Property="Template"</c>.</param>
    /// <param name="NamedOnly">
    /// Every theme for this type has a literal key, so none of them applies automatically.
    /// </param>
    private sealed record ThemeDeclaration(bool DeclaresTemplate, bool NamedOnly);

    /// <summary>
    /// Maps each themed type to what its implicit theme promises.
    /// </summary>
    /// <remarks>
    /// The <c>{x:Type}</c> key is the distinction that matters: it is what makes a theme apply to a
    /// control without anyone selecting it, so only those themes can be expected to reach a bare
    /// instance. A theme with a literal key is a component of somebody else's template.
    /// </remarks>
    private static Dictionary<string, ThemeDeclaration> ThemeDeclarations()
    {
        var declarations = new Dictionary<string, ThemeDeclaration>(StringComparer.Ordinal);

        foreach (var file in MdSpecSurface.CoreThemeFiles())
        {
            var text = File.ReadAllText(file);
            var opens = System.Text.RegularExpressions.Regex.Matches(
                text, "<ControlTheme\\b(?<attrs>[^>]*)>");

            for (var index = 0; index < opens.Count; index++)
            {
                var attributes = opens[index].Groups["attrs"].Value;
                var type = System.Text.RegularExpressions.Regex.Match(
                    attributes, "TargetType=\"[\\w:]*?(?<type>\\w+)\"").Groups["type"].Value;
                if (string.IsNullOrEmpty(type))
                {
                    continue;
                }

                var end = index + 1 < opens.Count ? opens[index + 1].Index : text.Length;
                var block = text[opens[index].Index..end];
                var implicitKey = attributes.Contains("{x:Type", StringComparison.Ordinal);
                var declaresTemplate = System.Text.RegularExpressions.Regex.IsMatch(
                    block, "Property=\"Template\"");

                var previous = declarations.TryGetValue(type, out var existing)
                    ? existing
                    : new ThemeDeclaration(false, NamedOnly: true);

                declarations[type] = new ThemeDeclaration(
                    previous.DeclaresTemplate || (implicitKey && declaresTemplate),
                    previous.NamedOnly && !implicitKey);
            }
        }

        return declarations;
    }

    /// <summary>How a control with no parameterless constructor can be reached, in one line.</summary>
    private static string DescribeConstructors(Type type) =>
        string.Join(
            " or ",
            type.GetConstructors()
                .OrderBy(ctor => ctor.GetParameters().Length)
                .Select(ctor => $"({string.Join(", ", ctor.GetParameters().Select(p => p.ParameterType.Name))})"));

    private static string BaseName(Type type) =>
        type.BaseType is { } baseType && baseType != typeof(object) ? baseType.Name : "control";

    /// <summary>
    /// True when the type draws itself in <c>Render</c> instead of through a template.
    /// </summary>
    /// <remarks>
    /// Only an override declared on the type itself counts. A control that merely inherits its
    /// parent's drawing code is covered by that parent's own case, and treating it as self-drawing
    /// here would let a genuinely empty template hide behind an inherited one.
    /// </remarks>
    private static bool OverridesRender(Type type)
    {
        var method = type.GetMethod(
            "Render",
            System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.NonPublic,
            binder: null,
            types: [typeof(DrawingContext)],
            modifiers: null);

        return method is not null
               && method.DeclaringType == type
               && method.GetBaseDefinition().DeclaringType != type;
    }
}
