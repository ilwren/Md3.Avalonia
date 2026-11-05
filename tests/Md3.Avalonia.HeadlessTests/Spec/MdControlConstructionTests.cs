using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using Xunit;

namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>
/// Layer L2b — can every themed control be built, themed and laid out at all?
/// </summary>
/// <remarks>
/// <para>
/// L2 measures the geometry of eight controls against the oracle. That is the right shape for a
/// control whose numbers Material publishes, but it answers a much narrower question than "does
/// this template work": 150 themed controls have no geometry case, and until this layer existed
/// nothing in the suite said whether they could even be constructed.
/// </para>
/// <para>
/// This layer asks the question that has to be answered first: create the control through a public
/// constructor, put it in a window, let its theme apply, lay it out, and check that it is actually
/// in a state where it could render. Three outcomes are possible, and they are not the same thing:
/// </para>
/// <list type="bullet">
///   <item><b>rendered</b> — a template was applied and it realised visual children (or, for a
///     control that draws itself, it derives from <see cref="Control"/> and overrides
///     <c>Render</c>). This is the healthy case.</item>
///   <item><b>hidden by design</b> — the control is present and themed but <c>IsVisible</c> is
///     false, because its theme defers presentation to a host: a snackbar is invisible until the
///     host marks it present, a resize grip until the window reports custom chrome. Nothing was
///     realised because nothing was meant to be.</item>
///   <item><b>stuck</b> — a template was applied and the control is visible, yet it produced no
///     visual children and draws nothing of its own. That is a control that renders as an empty
///     box, and it fails the build.</item>
/// </list>
/// <para>
/// It does not assert geometry and does not compare anything to the oracle: "lays out at the right
/// height" is L2's question, "lays out at all" is this one. What it produces is the number the
/// budget tracks (<c>constructed_themed_controls</c>) plus named inventories for the two other
/// outcomes, because a control that is legitimately hidden and a control that is silently empty look
/// identical in a property dump.
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

        var themed = MdSpecSurface.ThemedTypeNames();
        var candidates = MdSpecSurface.PublicControlTypes()
            .Where(type => themed.Contains(type.Name))
            // A Window cannot be a child of another window: the headless platform assigns it a
            // TopLevelHost of its own, and adding it to a ContentPresenter throws. Windows are
            // covered by the desktop adapter tests, so they are reported rather than laid out here.
            .Where(type => !typeof(TopLevel).IsAssignableFrom(type))
            .ToList();

        var topLevels = MdSpecSurface.PublicControlTypes()
            .Where(type => themed.Contains(type.Name) && typeof(TopLevel).IsAssignableFrom(type))
            .Select(type => type.Name)
            .ToList();

        Assert.True(candidates.Count > 100,
            $"only {candidates.Count} themed controls were found; the surface scan is not working");

        var rendered = 0;
        var hidden = new List<string>();
        var unreachable = new List<string>();
        var stuck = new List<string>();

        foreach (var type in candidates)
        {
            if (!MdSpecSurface.HasParameterlessConstructor(type))
            {
                unreachable.Add($"{type.Name}: {DescribeConstructors(type)}");
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
                    var hasChildren = control.GetVisualChildren().FirstOrDefault() is not null;
                    var drawsItself = OverridesRender(type);

                    if (hasChildren || drawsItself)
                    {
                        rendered++;
                        report.Pass(type.Name);
                        continue;
                    }

                    // Nothing was realised. Whether that is a defect depends entirely on whether the
                    // control was supposed to be on screen at all.
                    if (!control.IsEffectivelyVisible)
                    {
                        hidden.Add($"{type.Name}: IsVisible is false in its theme until a host presents it");
                        continue;
                    }

                    if (control is TemplatedControl { Template: null })
                    {
                        stuck.Add($"{type.Name}: attached, visible, but no ControlTemplate was applied");
                        continue;
                    }

                    stuck.Add($"{type.Name}: visible, themed, but realises no children and does not draw itself");
                }
            }
            catch (Exception ex)
            {
                stuck.Add($"{type.Name}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        Assert.True(stuck.Count == 0,
            $"{stuck.Count} themed control(s) could not be built, themed and laid out:" +
            $"{Environment.NewLine}  " + string.Join(Environment.NewLine + "  ", stuck));

        report.Metric("constructed_themed_controls", rendered);
        report.Metric("constructible_themed_controls", candidates.Count);

        report.Inventory(
            "themed_controls_needing_arguments",
            unreachable,
            "A themed control with no public parameterless constructor, so no generic matrix can lay "
            + "it out. Usually a dialog or an overlay that needs its owner - a design fact, not a "
            + "defect - but it does put that control outside every coverage number this harness "
            + "reports, which matters before reading those numbers as complete.");

        report.Inventory(
            "themed_controls_hidden_by_design",
            hidden,
            "A themed control that is present and laid out but invisible until its host presents it. "
            + "The template is not missing and nothing is broken; the control is simply not on screen "
            + "in a bare window, so no generic matrix can measure it either.");

        report.Inventory(
            "top_level_themed_controls",
            topLevels,
            "A Window or other TopLevel with a control theme. It cannot be a child of another window, "
            + "so this layer reports it instead of laying it out; the desktop adapter tests cover the "
            + "chrome these themes drive.");

        report.Note("construction",
            $"{rendered} of {candidates.Count} themed controls rendered under a bare window; "
            + $"{hidden.Count} are invisible by design and {unreachable.Count} need constructor "
            + $"arguments. {topLevels.Count} TopLevel theme(s) are out of scope for a generic matrix.");

        report.Complete();
    }

    /// <summary>How a control with no parameterless constructor can be reached, in one line.</summary>
    private static string DescribeConstructors(Type type) =>
        string.Join(
            " or ",
            type.GetConstructors()
                .OrderBy(ctor => ctor.GetParameters().Length)
                .Select(ctor => $"({string.Join(", ", ctor.GetParameters().Select(p => p.ParameterType.Name))})"));

    /// <summary>
    /// True when the type draws itself in <c>Render</c> instead of through a template.
    /// </summary>
    /// <remarks>
    /// A self-drawing control legitimately has no visual children, so counting "no children" as a
    /// failure would flag every one of them. Only an override declared on the type itself counts: a
    /// control that merely inherits its parent's drawing code is covered by that parent's own case,
    /// and treating it as self-drawing here would hide a genuinely empty template.
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
