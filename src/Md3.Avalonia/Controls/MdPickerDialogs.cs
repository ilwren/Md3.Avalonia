namespace Md3.Avalonia.Controls;

/// <summary>A modal-configured Material date picker with the full MdDatePicker API.</summary>
public sealed class MdDatePickerDialog : MdDatePicker
{
    public MdDatePickerDialog() => Mode = MdDatePickerMode.Modal;
    protected override Type StyleKeyOverride => typeof(MdDatePicker);
}

/// <summary>A modal-configured Material time picker with the full MdTimePicker API.</summary>
public sealed class MdTimePickerDialog : MdTimePicker
{
    public MdTimePickerDialog() => Mode = MdTimePickerMode.Dial;
    protected override Type StyleKeyOverride => typeof(MdTimePicker);
}
