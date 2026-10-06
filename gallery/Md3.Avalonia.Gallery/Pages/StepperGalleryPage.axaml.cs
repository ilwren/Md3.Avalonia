using Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class StepperGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public StepperGalleryPage()
    {
        InitializeComponent();
        PlanBox.ItemsSource = new[] { L("Starter", "入门版"), L("Team", "团队版"), L("Enterprise", "企业版") };
    }

    private void StepperChanged(object? sender, int step) =>
        StepperStatus.Text = L($"Step {step + 1} is active.", $"当前为第 {step + 1} 步。");

    private void StepperContinue(object? sender, int step)
    {
        if (step >= 2) StepperStatus.Text = L("Flow completed.", "流程已完成。");
    }
}
