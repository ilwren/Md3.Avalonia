using Avalonia.Controls;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class PickerGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public PickerGalleryPage()
    {
        InitializeComponent();

        // Defect #9 was reported as "the picker doesn't apply the selection". The page showed no
        // bound value anywhere, so neither outcome was observable: a committed selection and a
        // rolled-back one looked identical. Each picker now prints what it is actually bound to.
        Track(DockedDate, DockedDateValue, L("docked · commits as you pick", "停靠 · 选择即提交"));
        Track(ModalDate, ModalDateValue, L("modal · commits on OK", "模态 · 按 OK 提交"));
        Track(DialTime, DialTimeValue, L("dial · commits on OK", "表盘 · 按 OK 提交"));
        Track(InputTime, InputTimeValue, L("input · commits on OK or Enter", "输入 · 按 OK 或回车提交"));
        Track(TwentyFourHourTime, TwentyFourHourTimeValue, L("input · 24-hour · commits on OK", "输入 · 24 小时制 · 按 OK 提交"));
    }

    private static void Track(MdDatePicker picker, TextBlock readout, string mode)
    {
        // Two values, not one. If the field above keeps showing its seeded date while these move,
        // the selection committed and the template is not refreshing; if these do not move either,
        // the click never reached the picker. One screenshot tells the two apart - which matters
        // because the headless suite always takes the overlay popup path and can see neither.
        void Update() => readout.Text = picker.SelectedDate is { } value
            ? L($"SelectedDate = {value:yyyy-MM-dd} · field shows \"{picker.DisplayText}\" ({mode})",
                $"SelectedDate = {value:yyyy-MM-dd} · 字段显示 \"{picker.DisplayText}\"（{mode}）")
            : L($"SelectedDate = null · field shows \"{picker.DisplayText}\" ({mode})",
                $"SelectedDate = null · 字段显示 \"{picker.DisplayText}\"（{mode}）");

        Update();
        picker.PropertyChanged += (_, e) =>
        {
            if (e.Property == MdDatePicker.SelectedDateProperty ||
                e.Property == MdDatePicker.DisplayTextProperty ||
                e.Property == MdDatePicker.IsOpenProperty) Update();
        };
    }

    private static void Track(MdTimePicker picker, TextBlock readout, string mode)
    {
        void Update() => readout.Text = picker.SelectedTime is { } value
            ? L($"SelectedTime = {value:hh\\:mm} ({mode})", $"SelectedTime = {value:hh\\:mm}（{mode}）")
            : L($"SelectedTime = null ({mode})", $"SelectedTime = null（{mode}）");

        Update();
        picker.PropertyChanged += (_, e) =>
        {
            if (e.Property == MdTimePicker.SelectedTimeProperty) Update();
        };
    }
}
