using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>
/// Converts a slash-separated route path into breadcrumb items. Each item's
/// <see cref="MdBreadcrumbItem.CommandParameter"/> is the path prefix it represents.
/// 将斜杠分隔的路由路径转换为面包屑项，并将对应路径前缀作为每项的命令参数。
/// </summary>
/// <remarks>
/// This converter is intended for application routes and logical paths. It normalizes
/// backslashes to slashes, but does not canonicalize operating-system paths such as UNC paths.
/// </remarks>
public sealed class MdBreadcrumbPathConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrWhiteSpace(path))
            return Array.Empty<MdBreadcrumbItem>();

        var normalizedPath = path.Trim().Replace('\\', '/');
        var isRooted = normalizedPath.StartsWith("/", StringComparison.Ordinal);
        var segments = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var items = new List<MdBreadcrumbItem>(segments.Length + (isRooted ? 1 : 0));
        var prefix = isRooted ? "/" : string.Empty;

        if (isRooted)
        {
            items.Add(new MdBreadcrumbItem
            {
                Label = "/",
                CommandParameter = "/",
                IsCurrent = segments.Length == 0,
                UseAccentColor = segments.Length > 0
            });
        }

        for (var index = 0; index < segments.Length; index++)
        {
            var segment = segments[index];
            prefix = prefix == "/"
                ? "/" + segment
                : string.IsNullOrEmpty(prefix) ? segment : prefix + "/" + segment;

            var isCurrent = index == segments.Length - 1;
            items.Add(new MdBreadcrumbItem
            {
                Label = segment,
                CommandParameter = prefix,
                IsCurrent = isCurrent,
                UseAccentColor = !isCurrent
            });
        }

        return items;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("MdBreadcrumbPathConverter supports one-way conversion only.");
}
