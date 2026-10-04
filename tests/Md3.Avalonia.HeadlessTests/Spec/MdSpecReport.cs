using System.Text;
using Xunit;

namespace Md3.Avalonia.HeadlessTests.Spec;

internal enum MdSpecOutcome
{
    Pass,
    Gap,
    Advisory,
    Waived
}

internal sealed record MdSpecFinding(MdSpecOutcome Outcome, string Subject, string Message, string? Waiver);

/// <summary>
/// Accumulates the findings of one verification layer, writes a markdown report to
/// <c>artifacts/spec/</c>, and decides whether the run fails.
/// </summary>
/// <remarks>
/// Every layer reports an aggregate rather than asserting case by case. A matrix of several hundred
/// cases that stops at the first failure tells you almost nothing; one that lists all divergences at
/// once is directly actionable, and it keeps the whole matrix inside a single Avalonia headless
/// session instead of paying window setup per case.
/// </remarks>
internal sealed class MdSpecReport
{
    private readonly List<MdSpecFinding> _findings = new();
    private readonly string _reportName;
    private readonly string _policyLayer;
    private readonly string _title;
    private readonly MdSpecPolicy _policy;

    /// <param name="reportName">File stem under <c>artifacts/spec/</c>.</param>
    /// <param name="policyLayer">
    /// Layer id as spelled in <c>conformance-policy.json</c>. Kept separate from
    /// <paramref name="reportName"/> because one policy layer can emit several reports (L5 splits
    /// its UI-free physics from its runtime checks) and because a silent mismatch between the two
    /// would make every waiver in that layer stop matching without anything failing.
    /// </param>
    public MdSpecReport(string reportName, string policyLayer, string title, MdSpecPolicy policy)
    {
        _reportName = reportName;
        _policyLayer = policyLayer;
        _title = title;
        _policy = policy;
    }

    public int Checks { get; private set; }

    public void Pass(string subject)
    {
        Checks++;
        _findings.Add(new MdSpecFinding(MdSpecOutcome.Pass, subject, string.Empty, null));
    }

    /// <summary>Records a divergence from the oracle.</summary>
    /// <param name="subject">Stable identifier: a resource key, a matrix case name, or a scene name.</param>
    /// <param name="message">What was expected and what was observed.</param>
    /// <param name="highConfidence">
    /// Only high-confidence findings can fail the build, and only in <c>enforce</c> mode.
    /// </param>
    /// <param name="waiverKey">Key to match against the policy's waiver list; defaults to <paramref name="subject"/>.</param>
    public void Gap(string subject, string message, bool highConfidence = true, string? waiverKey = null)
    {
        Checks++;
        var waiver = _policy.FindWaiver(_policyLayer, waiverKey ?? subject);
        var outcome = waiver is not null
            ? MdSpecOutcome.Waived
            : highConfidence ? MdSpecOutcome.Gap : MdSpecOutcome.Advisory;
        _findings.Add(new MdSpecFinding(outcome, subject, message, waiver));
    }

    /// <summary>Records an observation that is informational by construction and can never fail.</summary>
    public void Note(string subject, string message)
    {
        Checks++;
        _findings.Add(new MdSpecFinding(MdSpecOutcome.Advisory, subject, message, null));
    }

    public void Check(string subject, bool condition, string failureMessage, bool highConfidence = true)
    {
        if (condition)
        {
            Pass(subject);
        }
        else
        {
            Gap(subject, failureMessage, highConfidence);
        }
    }

    /// <summary>Compares a scalar against the oracle, reporting instead of throwing.</summary>
    public void CheckScalar(string subject, double expected, double actual, double tolerance, bool highConfidence = true)
    {
        var delta = Math.Abs(expected - actual);
        Check(
            subject,
            delta <= tolerance,
            $"expected {expected:0.####}, observed {actual:0.####} (delta {delta:0.####} > tolerance {tolerance:0.####})",
            highConfidence);
    }

    /// <summary>Writes the report and fails the test when the policy says the findings are blocking.</summary>
    public void Complete()
    {
        var blocking = _policy.IsEnforcing
            ? _findings.Where(f => f.Outcome == MdSpecOutcome.Gap).ToList()
            : new List<MdSpecFinding>();

        var path = Path.Combine(MdSpecPaths.EnsureArtifactDirectory(), _reportName + ".md");
        File.WriteAllText(path, Render(), Encoding.UTF8);

        // A conformance layer that checked nothing would otherwise be an unconditional pass, which
        // spec-snapshot/manifest.json explicitly forbids as evidence.
        Assert.True(
            Checks > 0,
            $"{_reportName} produced zero checks. An empty conformance layer is an unconditional pass and is not valid evidence.");

        if (blocking.Count > 0)
        {
            var detail = string.Join(
                Environment.NewLine,
                blocking.Take(40).Select(f => $"  - {f.Subject}: {f.Message}"));
            Assert.Fail(
                $"{_title}: {blocking.Count} high-confidence divergence(s) from the frozen specification oracle " +
                $"while the policy is in 'enforce' mode.{Environment.NewLine}{detail}{Environment.NewLine}" +
                $"Full report: artifacts/spec/{_reportName}.md");
        }
    }

    private string Render()
    {
        var passes = _findings.Count(f => f.Outcome == MdSpecOutcome.Pass);
        var gaps = _findings.Where(f => f.Outcome == MdSpecOutcome.Gap).ToList();
        var advisories = _findings.Where(f => f.Outcome == MdSpecOutcome.Advisory).ToList();
        var waived = _findings.Where(f => f.Outcome == MdSpecOutcome.Waived).ToList();

        var builder = new StringBuilder();
        builder.AppendLine($"# {_title}");
        builder.AppendLine();
        builder.AppendLine($"- policy mode: `{_policy.Mode}`" +
                           (_policy.IsEnforcing ? " (high-confidence gaps fail the build)" : " (findings are reported, nothing fails)"));
        builder.AppendLine($"- checks: **{Checks}**");
        builder.AppendLine($"- conforming: **{passes}**");
        builder.AppendLine($"- high-confidence gaps: **{gaps.Count}**");
        builder.AppendLine($"- advisory: **{advisories.Count}**");
        builder.AppendLine($"- waived: **{waived.Count}**");
        builder.AppendLine();

        AppendSection(builder, "High-confidence gaps", gaps,
            "These block `enforce` mode. Fix them, or add a waiver with a reason to spec-snapshot/conformance-policy.json.");
        AppendSection(builder, "Advisory findings", advisories,
            "Medium-confidence oracle entries and observations. These never fail a build; re-verify the source before promoting one to `high`.");
        AppendSection(builder, "Waived", waived, "Deliberate divergences recorded in the policy file.");

        if (gaps.Count == 0 && advisories.Count == 0 && waived.Count == 0)
        {
            builder.AppendLine("All checks conform to the frozen specification oracle.");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static void AppendSection(StringBuilder builder, string heading, List<MdSpecFinding> items, string blurb)
    {
        if (items.Count == 0)
        {
            return;
        }

        builder.AppendLine($"## {heading} ({items.Count})");
        builder.AppendLine();
        builder.AppendLine(blurb);
        builder.AppendLine();
        builder.AppendLine("| subject | detail |");
        builder.AppendLine("| --- | --- |");
        foreach (var item in items.Take(300))
        {
            var detail = item.Message.Replace("|", "\\|", StringComparison.Ordinal);
            if (item.Waiver is not null)
            {
                detail += " _(waived: " + item.Waiver.Replace("|", "\\|", StringComparison.Ordinal) + ")_";
            }

            builder.AppendLine($"| `{item.Subject}` | {detail} |");
        }

        if (items.Count > 300)
        {
            builder.AppendLine($"| … | {items.Count - 300} more suppressed |");
        }

        builder.AppendLine();
    }
}
