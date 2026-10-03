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
                ["Copy"] = "Copy", ["Paste"] = "Paste", ["SelectAll"] = "Select all",
                ["InvalidValue"] = "Invalid value", ["RequiredField"] = "This field is required.",
                ["VerificationCode"] = "Verification code", ["InvalidVerificationCode"] = "Invalid verification code",
                ["Tags"] = "Tags", ["AddTag"] = "Add tag", ["EnterTag"] = "Enter a tag",
                ["MaximumTags"] = "Maximum {0} tags", ["TagAlreadyAdded"] = "Tag already added",
                ["Rating"] = "Rating", ["Carousel"] = "Carousel", ["Calendar"] = "Calendar", ["Loading"] = "Loading",
                ["ItemOf"] = "Item {0} of {1}", ["SelectedItemOf"] = "Selected item {0} of {1}",
                ["ItemsCount"] = "{0} items", ["CharactersEntered"] = "{0} of {1} characters entered", ["ReadOnly"] = "read only", ["ErrorPrefix"] = "Error: {0}",
                ["Search"] = "Search", ["NoResults"] = "No results", ["Dismiss"] = "Dismiss",
                ["InvalidHex"] = "Enter a valid HEX color", ["CopiedHex"] = "HEX value copied", ["CopyFailed"] = "Could not copy HEX value",
                ["PreviousMonth"] = "Previous month", ["NextMonth"] = "Next month",
                ["Expand"] = "Expand", ["Collapse"] = "Collapse", ["Remove"] = "Remove"
            },
            ["zh"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ChooseDate"] = "选择日期", ["ChooseTime"] = "选择时间",
                ["Date"] = "日期", ["Time"] = "时间", ["Today"] = "今天",
                ["Selected"] = "已选择", ["Unavailable"] = "不可用",
                ["Cancel"] = "取消", ["OK"] = "确定", ["Clear"] = "清除",
                ["Undo"] = "撤销", ["Redo"] = "重做", ["Cut"] = "剪切",
                ["Copy"] = "复制", ["Paste"] = "粘贴", ["SelectAll"] = "全选",
                ["InvalidValue"] = "值无效", ["RequiredField"] = "此字段为必填项。",
                ["VerificationCode"] = "验证码", ["InvalidVerificationCode"] = "验证码无效",
                ["Tags"] = "标签", ["AddTag"] = "添加标签", ["EnterTag"] = "请输入标签",
                ["MaximumTags"] = "最多 {0} 个标签", ["TagAlreadyAdded"] = "标签已添加",
                ["Rating"] = "评分", ["Carousel"] = "轮播", ["Calendar"] = "日历", ["Loading"] = "加载中",
                ["ItemOf"] = "第 {0} 项，共 {1} 项", ["SelectedItemOf"] = "已选择第 {0} 项，共 {1} 项",
                ["ItemsCount"] = "共 {0} 项", ["CharactersEntered"] = "已输入 {0}/{1} 个字符", ["ReadOnly"] = "只读", ["ErrorPrefix"] = "错误：{0}",
                ["Search"] = "搜索", ["NoResults"] = "无结果", ["Dismiss"] = "关闭",
                ["InvalidHex"] = "请输入有效的十六进制颜色", ["CopiedHex"] = "已复制十六进制颜色值", ["CopyFailed"] = "无法复制十六进制颜色值",
                ["PreviousMonth"] = "上个月", ["NextMonth"] = "下个月",
                ["Expand"] = "展开", ["Collapse"] = "收起", ["Remove"] = "移除"
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

    public static string Format(string key, StyledElement? element, params object?[] arguments) =>
        string.Format(ResolveCulture(element), GetString(key, element), arguments);
}
