using Avalonia.Controls;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class DateRangePickerGalleryPage : UserControl
{
    public DateRangePickerGalleryPage()
    {
        InitializeComponent();

        // Print the bound values: #9 was reported because a picker page that shows no value
        // cannot tell a committed selection apart from one that rolled back.
        Update();
        RangePicker.PropertyChanged += (_, e) =>
        {
            if (e.Property == MdDateRangePicker.StartDateProperty ||
                e.Property == MdDateRangePicker.EndDateProperty)
                Update();
        };
    }

    // Both ends are bindable; the control validates start <= end itself.
    private void Update()
    {
        var start = RangePicker.StartDate is { } startDate ? $"{startDate:yyyy-MM-dd}" : "null";
        var end = RangePicker.EndDate is { } endDate ? $"{endDate:yyyy-MM-dd}" : "null";
        RangeValue.Text = $"StartDate = {start} \u00b7 EndDate = {end}";
    }
}
