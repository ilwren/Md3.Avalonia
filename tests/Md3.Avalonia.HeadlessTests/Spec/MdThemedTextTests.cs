using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Localization;
using Xunit;

namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>
/// Layer L4 — the chrome a shipped theme draws must be able to follow the inherited culture.
/// </summary>
/// <remarks>
/// <para>
/// A sentence authored inside a <c>ControlTemplate</c> renders in that language for every consumer:
/// no application can retarget it, no translation can reach it, and <c>MdLocalization</c> never sees
/// it. The lint catches the mechanical case (<c>attrs="English words"</c> in a theme file, now zero),
/// but a string that was moved into C# and never wired to a <c>TemplateBinding</c> would pass the
/// lint and still be frozen — so this class checks the other direction: the localized text a control
/// exposes must actually change when the inherited culture does.
/// </para>
/// <para>
/// The controls are exercised directly rather than through a window, and each assignment goes
/// through the attached <see cref="MdLocalization.CultureProperty"/> because that is the mechanism a
/// consumer uses; setting <c>CultureInfo.CurrentCulture</c> would prove something else entirely.
/// </para>
/// </remarks>
public sealed class MdThemedTextTests
{
    private const string Layer = "L4_inventory";

    [Fact]
    public void Localized_Chrome_Follows_The_Inherited_Culture()
    {
        var english = new System.Globalization.CultureInfo("en");
        var chinese = new System.Globalization.CultureInfo("zh-Hans");

        // --- MdTimePicker: the dial hint used to be a literal inside MdTimePicker.axaml. ---------
        var picker = new MdTimePicker();
        MdLocalization.SetCulture(picker, english);
        var englishHint = picker.DialHintText;
        MdLocalization.SetCulture(picker, chinese);
        var chineseHint = picker.DialHintText;

        // --- MdPaginatedDataTable: "Rows per page" used to be a literal. -----------------------
        var table = new MdPaginatedDataTable();
        MdLocalization.SetCulture(table, english);
        var englishRows = table.RowsPerPageLabel;
        MdLocalization.SetCulture(table, chinese);
        var chineseRows = table.RowsPerPageLabel;

        // --- MdColorPicker: twelve section headings and channel labels used to be literals. ----
        var colorPicker = new MdColorPicker();
        MdLocalization.SetCulture(colorPicker, english);
        var englishChrome = ColorPickerChrome(colorPicker);
        MdLocalization.SetCulture(colorPicker, chinese);
        var chineseChrome = ColorPickerChrome(colorPicker);

        var failures = new List<string>();
        Check(failures, "MdTimePicker.DialHintText", englishHint, chineseHint);
        Check(failures, "MdPaginatedDataTable.RowsPerPageLabel", englishRows, chineseRows);
        foreach (var key in englishChrome.Keys)
        {
            Check(failures, "MdColorPicker." + key, englishChrome[key], chineseChrome[key]);
        }

        Assert.True(failures.Count == 0,
            $"{failures.Count} themed label(s) do not follow the inherited culture:" +
            $"{Environment.NewLine}  " + string.Join(Environment.NewLine + "  ", failures));

        var report = new MdSpecReport("L4-themed-text", Layer, "L4 — themed text follows the culture",
            MdSpecPolicy.Load());
        report.Metric("localized_chrome_labels", 2 + englishChrome.Count);
        report.Pass($"{2 + englishChrome.Count} chrome label(s) switch with the inherited culture");
        report.Complete();
    }

    /// <summary>Every localized label MdColorPicker presents in its own template.</summary>
    private static Dictionary<string, string> ColorPickerChrome(MdColorPicker picker) => new(StringComparer.Ordinal)
    {
        [nameof(MdColorPicker.SourceColorsText)] = picker.SourceColorsText,
        [nameof(MdColorPicker.SourceColorsHint)] = picker.SourceColorsHint,
        [nameof(MdColorPicker.TonalPaletteText)] = picker.TonalPaletteText,
        [nameof(MdColorPicker.TonalPaletteHint)] = picker.TonalPaletteHint,
        [nameof(MdColorPicker.AdjustColorText)] = picker.AdjustColorText,
        [nameof(MdColorPicker.AdjustColorHint)] = picker.AdjustColorHint,
        [nameof(MdColorPicker.HueText)] = picker.HueText,
        [nameof(MdColorPicker.SaturationText)] = picker.SaturationText,
        [nameof(MdColorPicker.ValueText)] = picker.ValueText,
        [nameof(MdColorPicker.OpacityText)] = picker.OpacityText,
        [nameof(MdColorPicker.RecentColorsText)] = picker.RecentColorsText,
        [nameof(MdColorPicker.RecentColorsHint)] = picker.RecentColorsHint
    };

    /// <summary>
    /// A label passes when it is non-empty and differs between the two cultures.
    /// </summary>
    /// <remarks>
    /// Emptiness is checked as well as inequality because the failure mode of an unwired property is
    /// usually a blank string, which is "different from both" and would otherwise pass.
    /// </remarks>
    private static void Check(List<string> failures, string subject, string english, string chinese)
    {
        if (string.IsNullOrWhiteSpace(english))
        {
            failures.Add($"{subject} is empty under en");
        }
        else if (string.IsNullOrWhiteSpace(chinese))
        {
            failures.Add($"{subject} is empty under zh-Hans");
        }
        else if (string.Equals(english, chinese, StringComparison.Ordinal))
        {
            failures.Add($"{subject} is '{english}' in both cultures; it is not localized");
        }
    }
}
