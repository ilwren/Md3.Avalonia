using Avalonia;
using Avalonia.Controls.Primitives;

namespace Md3.Avalonia.Controls;

/// <summary>A Material date-range picker composed from two localized Material calendar pickers.</summary>
public sealed class MdDateRangePicker : TemplatedControl
{
    public static readonly StyledProperty<DateTimeOffset?> StartDateProperty = AvaloniaProperty.Register<MdDateRangePicker, DateTimeOffset?>(nameof(StartDate), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<DateTimeOffset?> EndDateProperty = AvaloniaProperty.Register<MdDateRangePicker, DateTimeOffset?>(nameof(EndDate), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<DateTimeOffset?> MinimumDateProperty = AvaloniaProperty.Register<MdDateRangePicker, DateTimeOffset?>(nameof(MinimumDate));
    public static readonly StyledProperty<DateTimeOffset?> MaximumDateProperty = AvaloniaProperty.Register<MdDateRangePicker, DateTimeOffset?>(nameof(MaximumDate));
    public static readonly StyledProperty<object?> StartLabelProperty = AvaloniaProperty.Register<MdDateRangePicker, object?>(nameof(StartLabel), "Start date");
    public static readonly StyledProperty<object?> EndLabelProperty = AvaloniaProperty.Register<MdDateRangePicker, object?>(nameof(EndLabel), "End date");
    public static readonly StyledProperty<MdDatePickerMode> ModeProperty = AvaloniaProperty.Register<MdDateRangePicker, MdDatePickerMode>(nameof(Mode));

    private bool _normalizing;

    static MdDateRangePicker()
    {
        StartDateProperty.Changed.AddClassHandler<MdDateRangePicker>((picker, _) => picker.NormalizeRange(true));
        EndDateProperty.Changed.AddClassHandler<MdDateRangePicker>((picker, _) => picker.NormalizeRange(false));
    }

    public DateTimeOffset? StartDate { get => GetValue(StartDateProperty); set => SetValue(StartDateProperty, value); }
    public DateTimeOffset? EndDate { get => GetValue(EndDateProperty); set => SetValue(EndDateProperty, value); }
    public DateTimeOffset? MinimumDate { get => GetValue(MinimumDateProperty); set => SetValue(MinimumDateProperty, value); }
    public DateTimeOffset? MaximumDate { get => GetValue(MaximumDateProperty); set => SetValue(MaximumDateProperty, value); }
    public object? StartLabel { get => GetValue(StartLabelProperty); set => SetValue(StartLabelProperty, value); }
    public object? EndLabel { get => GetValue(EndLabelProperty); set => SetValue(EndLabelProperty, value); }
    public MdDatePickerMode Mode { get => GetValue(ModeProperty); set => SetValue(ModeProperty, value); }

    public void Clear()
    {
        SetCurrentValue(StartDateProperty, null);
        SetCurrentValue(EndDateProperty, null);
    }

    private void NormalizeRange(bool startChanged)
    {
        if (_normalizing || StartDate is null || EndDate is null || StartDate <= EndDate) return;
        _normalizing = true;
        if (startChanged) SetCurrentValue(EndDateProperty, StartDate);
        else SetCurrentValue(StartDateProperty, EndDate);
        _normalizing = false;
    }
}
