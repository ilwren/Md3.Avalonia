namespace Md3.Avalonia.Controls;

/// <summary>
/// UI-independent entry point used by view models to enqueue messages in an attached
/// <see cref="MdSnackbarHost"/>.
/// </summary>
public interface IMdSnackbarService
{
    /// <summary>Queues a message and completes when its action is invoked or it is dismissed.</summary>
    Task<MdSnackbarResult> ShowAsync(MdSnackbarMessage message);

    /// <summary>Queues a message without waiting for its result.</summary>
    void Show(MdSnackbarMessage message);

    /// <summary>Dismisses the currently displayed message, if any.</summary>
    void Dismiss();
}
