using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class FormValidationGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public FormValidationGalleryPage()
    {
        InitializeComponent();
        RoleField.ItemsSource = new[] { L("Designer", "设计师"), L("Developer", "开发者"), L("Tester", "测试人员") };
    }

    private void FormNameChanged(object? sender, TextChangedEventArgs e)
    {
        NameField.MarkTouched();
        NameField.Value = (sender as TextBox)?.Text;
    }

    private void ValidateForm(object? sender, RoutedEventArgs e) => AccountForm.Submit();

    private void FormSubmitted(object? sender, MdFormSubmittedEventArgs e) =>
        FormStatus.Text = e.IsValid ? L("Form is valid and ready to submit.", "表单有效，可以提交。") : L("Correct the fields marked in error.", "请修正标记为错误的字段。");
}
