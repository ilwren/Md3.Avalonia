using System.Globalization;
using Avalonia;

namespace Md3.Avalonia.Localization;

/// <summary>Inherited culture and built-in strings used by Material controls.</summary>
public sealed class MdLocalization : AvaloniaObject
{
    public static readonly AttachedProperty<CultureInfo?> CultureProperty =
        AvaloniaProperty.RegisterAttached<MdLocalization, StyledElement, CultureInfo?>(
            "Culture", defaultValue: null, inherits: true);

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Strings =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
        {
            ["en"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ChooseDate"] = "Choose date", ["ChooseTime"] = "Choose time",
                ["Date"] = "Date", ["Time"] = "Time", ["Today"] = "Today",
                ["Selected"] = "Selected", ["Unavailable"] = "Unavailable",
                ["Cancel"] = "Cancel", ["OK"] = "OK", ["Clear"] = "Clear",
                ["Undo"] = "Undo", ["Redo"] = "Redo", ["Cut"] = "Cut",
                ["Copy"] = "Copy", ["Paste"] = "Paste", ["SelectAll"] = "Select all"
            },
            ["zh"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ChooseDate"] = "选择日期", ["ChooseTime"] = "选择时间",
                ["Date"] = "日期", ["Time"] = "时间", ["Today"] = "今天",
                ["Selected"] = "已选择", ["Unavailable"] = "不可用",
                ["Cancel"] = "取消", ["OK"] = "确定", ["Clear"] = "清除",
                ["Undo"] = "撤销", ["Redo"] = "重做", ["Cut"] = "剪切",
                ["Copy"] = "复制", ["Paste"] = "粘贴", ["SelectAll"] = "全选"
            }
        };

    private MdLocalization()
    {
    }

    public static CultureInfo? GetCulture(StyledElement element) => element.GetValue(CultureProperty);
    public static void SetCulture(StyledElement element, CultureInfo? value) => element.SetValue(CultureProperty, value);

    public static CultureInfo ResolveCulture(StyledElement? element = null) =>
        element?.GetValue(CultureProperty) ?? CultureInfo.CurrentCulture;

    public static string GetString(string key, StyledElement? element = null) =>
        GetString(key, ResolveCulture(element));

    public static string GetString(string key, CultureInfo culture)
    {
        if (Strings.TryGetValue(culture.TwoLetterISOLanguageName, out var table) &&
            table.TryGetValue(key, out var value)) return value;
        return Strings["en"].TryGetValue(key, out value) ? value : key;
    }
}
