namespace Md3.Avalonia.Controls;

/// <summary>
/// UI-independent entry point used by view models to show a dialog in an attached
/// <see cref="MdDialogHost"/>. Inject this instead of handing a view model a reference to the
/// control: the view model names the dialog by model object, the host resolves the view from its
/// <c>DataTemplates</c>, and the same surface keeps working from view code-behind.
/// </summary>
public interface IMdDialogService
{
    /// <summary>True while a dialog requested through this service is displayed.</summary>
    bool IsOpen { get; }

    /// <summary>
    /// Shows <paramref name="dialog"/> and completes with the value passed to <see cref="Close"/>.
    /// Completes with null when the dialog is dismissed by the scrim, Escape, the Android back
    /// gesture, or <paramref name="cancellationToken"/>. A request made while another dialog is
    /// open — or before a host is attached — waits its turn rather than replacing it.
    /// </summary>
    Task<object?> ShowAsync(object dialog, CancellationToken cancellationToken = default);

    /// <summary>Shows a dialog without waiting for its result.</summary>
    void Show(object dialog);

    /// <summary>
    /// Swaps the displayed dialog for <paramref name="dialog"/>, completing the one it replaces
    /// with null. Use this for a step inside one flow — an about box opening its license list, a
    /// wizard advancing a page — where <see cref="ShowAsync"/> would queue the new dialog behind
    /// the old one and only reveal it once the user closed what they were looking at.
    /// </summary>
    /// <remarks>
    /// The replacement jumps ahead of anything already queued, so it is displayed next even when
    /// other requests are waiting. With nothing displayed this behaves like <see cref="ShowAsync"/>.
    /// </remarks>
    Task<object?> ReplaceAsync(object dialog, CancellationToken cancellationToken = default);

    /// <summary>Replaces the displayed dialog without waiting for the new one's result.</summary>
    void Replace(object dialog);

    /// <summary>Closes the displayed dialog, completing its pending <see cref="ShowAsync"/> with <paramref name="result"/>.</summary>
    void Close(object? result = null);
}

/// <summary>Typed conveniences over <see cref="IMdDialogService"/>.</summary>
public static class MdDialogServiceExtensions
{
    /// <summary>
    /// Shows a dialog and returns its result as <typeparamref name="TResult"/>, or
    /// <c>default</c> when the dialog was dismissed without one.
    /// </summary>
    public static async Task<TResult?> ShowAsync<TResult>(
        this IMdDialogService service, object dialog, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        return await service.ShowAsync(dialog, cancellationToken).ConfigureAwait(true) is TResult result
            ? result
            : default;
    }
}
