using Avalonia.Controls;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class TasksSamplePage : UserControl
{
    public TasksSamplePage()
    {
        InitializeComponent();
    }

    private void OnAddTaskClick(object? sender, RoutedEventArgs e)
    {
        if (NewTaskInput is not null && !string.IsNullOrWhiteSpace(NewTaskInput.Text))
        {
            if (TaskStatus is not null)
                TaskStatus.Text = $"Task added: \"{NewTaskInput.Text}\"";
            NewTaskInput.Text = string.Empty;
        }
    }
}
