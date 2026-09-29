namespace Md3.Avalonia.Controls;

/// <summary>Immutable day data exposed by <see cref="MdDatePicker"/> for template customization.</summary>
public sealed record MdCalendarDay(
    DateTimeOffset Date,
    string DayText,
    string AccessibleText,
    bool IsCurrentMonth,
    bool IsSelected,
    bool IsToday,
    bool IsEnabled);
