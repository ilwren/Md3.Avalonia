namespace Md3.Avalonia.Controls;

/// <summary>
/// UI-independent entry point used by view models to show a banner in an attached
/// <see cref="MdBannerHost"/>. Inject this instead of handing a view model a reference to the
/// control: the view model names the banner by model object, the host resolves the view from its
/// <c>DataTemplates</c>, and the same surface keeps working from view code-behind.
/// </summary>
public interface IMdBannerService
{
    /// <summary>True while a banner requested through this service is displayed.</summary>
    bool IsOpen { get; }

    /// <summary>
    /// Shows <paramref name="banner"/> and completes with the value passed to <see cref="Close"/>.
    /// Completes with null when the banner's dismiss action, a two-way binding, or
    /// <paramref name="cancellationToken"/> closes it. A request made while another banner is open
    /// — or before a host is attached — waits its turn rather than replacing it.
    /// </summary>
    Task<object?> ShowAsync(object banner, CancellationToken cancellationToken = default);

    /// <summary>Shows a banner without waiting for its result.</summary>
    void Show(object banner);

    /// <summary>
    /// Swaps the displayed banner for <paramref name="banner"/>, completing the one it replaces
    /// with null. Use this when a persistent status changes and the new state should be shown next,
    /// rather than waiting behind the previous banner as a normal <see cref="ShowAsync"/> request.
    /// </summary>
    /// <remarks>
    /// The replacement jumps ahead of anything already queued, so it is displayed next even when
    /// other requests are waiting. With nothing displayed this behaves like <see cref="ShowAsync"/>.
    /// </remarks>
    Task<object?> ReplaceAsync(object banner, CancellationToken cancellationToken = default);

    /// <summary>Replaces the displayed banner without waiting for the new one's result.</summary>
    void Replace(object banner);

    /// <summary>Closes the displayed banner, completing its pending <see cref="ShowAsync"/> with <paramref name="result"/>.</summary>
    void Close(object? result = null);
}

/// <summary>Typed conveniences over <see cref="IMdBannerService"/>.</summary>
public static class MdBannerServiceExtensions
{
    /// <summary>
    /// Shows a banner and returns its result as <typeparamref name="TResult"/>, or
    /// <c>default</c> when the banner was dismissed without one.
    /// </summary>
    public static async Task<TResult?> ShowAsync<TResult>(
        this IMdBannerService service, object banner, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        return await service.ShowAsync(banner, cancellationToken).ConfigureAwait(true) is TResult result
            ? result
            : default;
    }
}
