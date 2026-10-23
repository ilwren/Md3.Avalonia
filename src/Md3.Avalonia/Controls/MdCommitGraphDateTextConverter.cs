using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Formats a <see cref="DateTimeOffset"/> for <see cref="MdCommitGraph"/> rows: a short
/// date such as <c>Oct 22</c>, extended with the year when it is not the current year.
/// </summary>
[XamlCallable]
public class MdCommitGraphDateTextConverter : IValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTimeOffset date)
            return null;

        var hasYear = date.Year != DateTimeOffset.Now.Year;
        return culture is not null
            ? date.ToString(hasYear ? "MMM d, yyyy" : "MMM d", culture)
            : date.ToString(hasYear ? "MMM d, yyyy" : "MMM d");
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
