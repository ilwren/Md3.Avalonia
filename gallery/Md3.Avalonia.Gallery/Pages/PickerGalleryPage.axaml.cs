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
        void Update() => readout.Text = picker.SelectedDate is { } value
            ? L($"SelectedDate = {value:yyyy-MM-dd} ({mode})", $"SelectedDate = {value:yyyy-MM-dd}（{mode}）")
            : L($"SelectedDate = null ({mode})", $"SelectedDate = null（{mode}）");

        Update();
        picker.PropertyChanged += (_, e) =>
        {
            if (e.Property == MdDatePicker.SelectedDateProperty) Update();
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
