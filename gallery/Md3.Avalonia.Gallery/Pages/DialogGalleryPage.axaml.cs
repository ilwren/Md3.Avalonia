using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public sealed record DiscardDraftDialogModel(string Headline, string Message);

public sealed record ReviewChangesDialogModel(string Headline, string Message);

/// <summary>The dialog a view model asks for; its buttons answer through the same service.</summary>
public sealed class ConfirmDeleteDialogModel
{
    private readonly IMdDialogService _dialogs;

    public ConfirmDeleteDialogModel(IMdDialogService dialogs, string item)
    {
        _dialogs = dialogs;
        Headline = $"Delete {item}?";
        ConfirmCommand = new RelayCommand(() => _dialogs.Close(true));
        CancelCommand = new RelayCommand(() => _dialogs.Close(false));
    }

    public string Headline { get; }
    public string Message => "This cannot be undone. The view model decides what happens next.";
    public RelayCommand ConfirmCommand { get; }
    public RelayCommand CancelCommand { get; }
}

/// <summary>
/// Holds no control reference at all — only <see cref="IMdDialogService"/>. The same surface is
/// driven from code-behind in the panel above, which is the point of the service seam.
/// </summary>
public sealed class DialogServiceDemoViewModel : INotifyPropertyChanged
{
    private readonly IMdDialogService _dialogs;
    private string _status = "No item deleted yet.";

    public DialogServiceDemoViewModel(IMdDialogService dialogs)
    {
        _dialogs = dialogs;
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public AsyncRelayCommand DeleteCommand { get; }

    public string Status
    {
        get => _status;
        private set
        {
            if (_status == value) return;
            _status = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        }
    }

    private async Task DeleteAsync()
    {
        var confirmed = await _dialogs.ShowAsync<bool>(new ConfirmDeleteDialogModel(_dialogs, "Quarterly report"));
        Status = confirmed ? "Deleted Quarterly report." : "Kept Quarterly report.";
    }
}

public partial class DialogGalleryPage : UserControl
{
    public DialogGalleryPage()
    {
        InitializeComponent();

        // What an application does once at composition: one service, assigned to the host and
        // injected into the view model. Nothing hands the view model a control.
        var dialogs = new MdDialogService();
        ServiceDialogHost.Service = dialogs;
        ServiceDemoRoot.DataContext = new DialogServiceDemoViewModel(dialogs);
    }

    private void OpenBasicDialog(object? sender, RoutedEventArgs e)
    {
        _ = BasicDialogHost.ShowAsync(new DiscardDraftDialogModel(
            "Discard draft?",
            "This draft has unsaved changes. Discarding it cannot be undone."));
    }

    private void OpenFullScreenDialog(object? sender, RoutedEventArgs e)
    {
        _ = BasicDialogHost.ShowAsync(new ReviewChangesDialogModel(
            "Review changes",
            "Review the pending changes before returning to the editor."));
    }

    private void CloseDialog(object? sender, RoutedEventArgs e) => BasicDialogHost.Close();
}
