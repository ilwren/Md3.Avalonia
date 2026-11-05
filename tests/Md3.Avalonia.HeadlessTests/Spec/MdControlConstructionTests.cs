using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
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
/// This layer asks the question that has to be answered first, for every themed control at once:
/// create it through its public parameterless constructor, put it in a window, let the theme apply,
/// lay it out, and check that something was actually realised. A control whose template throws, or
/// whose <c>OnApplyTemplate</c> dereferences a part it did not get, or that binds a
/// <c>DynamicResource</c> that does not exist - all of those fail here with the control's name
/// attached, instead of surfacing as an unrelated crash in a gallery page months later.
/// </para>
/// <para>
/// It does not assert geometry and does not compare anything to the oracle: "lays out at the right
/// height" is L2's question and "lays out at all" is this one. What it produces is the coverage
/// number the budget tracks (<c>constructible_themed_controls</c>), plus a named inventory entry
/// for anything it cannot reach - usually a constructor that needs arguments, which is a design
/// fact rather than a defect, but which does put that control outside every number here.
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
            .ToList();

        Assert.True(candidates.Count > 100,
            $"only {candidates.Count} themed controls were found; the surface scan is not working");

        var constructed = 0;
        var unreachable = new List<string>();
        var stuck = new List<string>();

        foreach (var type in candidates)
        {
            if (!MdSpecSurface.HasParameterlessConstructor(type))
            {
                unreachable.Add(
                    $"{type.Name}: {string.Join(", ", type.GetConstructors().Select(ctor => ctor.GetParameters().Length + " arg(s)"))}");
                continue;
            }

            // One window per control, not one window for all of them: a single bad control in a
            // shared tree throws during the layout pass of its neighbours and takes the batch with
            // it, and the resulting message names the wrong control.
            try
            {
                var control = (Control)Activator.CreateInstance(type)!;
                using (MdVisualTreeQuery.Show(control, 360, 260))
                {
                    // A TemplatedControl with a null Template after being attached and laid out
                    // never had its ControlTheme applied, which is a shipped control that renders
                    // as nothing rather than a subtle geometry error.
                    if (control is TemplatedControl { Template: null })
                    {
                        stuck.Add($"{type.Name}: attached but no ControlTemplate was applied");
                        continue;
                    }

                    if (control.GetVisualChildren().FirstOrDefault() is null)
                    {
                        stuck.Add($"{type.Name}: applied a template but realised no visual children");
                        continue;
                    }

                    constructed++;
                    report.Pass(type.Name);
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

        report.Metric("constructed_themed_controls", constructed);
        report.Metric("constructible_themed_controls", candidates.Count);

        report.Inventory(
            "themed_controls_needing_arguments",
            unreachable,
            "A themed control with no public parameterless constructor, so no generic matrix can lay "
            + "it out. Usually a dialog or an overlay that needs its owner - a design fact, not a "
            + "defect - but it does put that control outside every coverage number this harness "
            + "reports, which is worth knowing before reading those numbers as complete.");

        report.Note("construction",
            $"{constructed} of {candidates.Count} themed controls were constructed, themed and laid "
            + "out with a realised visual tree.");

        report.Complete();
    }
}
