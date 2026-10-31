using Avalonia.Threading;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Holds banner requests for one attached <see cref="MdBannerHost"/>. Register the same instance
/// as <see cref="IMdBannerService"/> for view models and assign it to the host in the application
/// shell, mirroring <see cref="MdSnackbarService"/>.
/// </summary>
/// <remarks>
/// A request made before the host exists is held until one attaches, so a view model may show a
/// banner from its constructor or a startup command. Material allows one banner at a time, so a
/// request made while another is displayed waits rather than replacing it.
/// </remarks>
public sealed class MdBannerService : IMdBannerService
{
    private readonly object _gate = new();
    private readonly List<Request> _pending = [];
    private WeakReference<MdBannerHost>? _host;
    private Request? _current;

    /// <inheritdoc />
    public bool IsOpen
    {
        get { lock (_gate) return _current is not null; }
    }

    /// <inheritdoc />
    public Task<object?> ShowAsync(object banner, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(banner);
        var request = new Request(banner);
        lock (_gate) _pending.Add(request);

        if (cancellationToken.CanBeCanceled)
            request.Cancellation = cancellationToken.Register(() => Cancel(request));

        Pump();
        return request.Completion.Task;
    }

    /// <inheritdoc />
    public void Show(object banner) => _ = ShowAsync(banner);

    /// <inheritdoc />
    public Task<object?> ReplaceAsync(object banner, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(banner);
        var request = new Request(banner);
        bool displacing;
        lock (_gate)
        {
            // Head of the queue, so the close below hands the host this request and not whatever
            // was already waiting.
            _pending.Insert(0, request);
            displacing = _current is not null;
        }

        if (cancellationToken.CanBeCanceled)
            request.Cancellation = cancellationToken.Register(() => Cancel(request));

        // Closing completes the displayed request, which pumps the queue and picks this one up.
        if (displacing) Close();
        Pump();
        return request.Completion.Task;
    }

    /// <inheritdoc />
    public void Replace(object banner) => _ = ReplaceAsync(banner);

    /// <inheritdoc />
    public void Close(object? result = null) => OnUiThread(host => host.Close(result));

    internal void Attach(MdBannerHost host)
    {
        MdBannerHost? previous = null;
        lock (_gate)
        {
            if (_host?.TryGetTarget(out previous) == true && ReferenceEquals(previous, host)) return;
            _host = new WeakReference<MdBannerHost>(host);
        }

        previous?.ReleaseService(this);
        host.TryShowNext(this);
    }

    internal void Detach(MdBannerHost host)
    {
        lock (_gate)
        {
            if (_host?.TryGetTarget(out var attached) == true && ReferenceEquals(attached, host))
                _host = null;
        }
    }

    /// <summary>Hands the host the next request to display, if this service still owns that host.</summary>
    internal Request? AcquireCurrent(MdBannerHost host)
    {
        lock (_gate)
        {
            if (_host?.TryGetTarget(out var attached) != true || !ReferenceEquals(attached, host))
                return null;
            if (_current is null && _pending.Count > 0)
            {
                _current = _pending[0];
                _pending.RemoveAt(0);
            }
            return _current;
        }
    }

    internal void Complete(MdBannerHost host, Request request, object? result)
    {
        var completed = false;
        lock (_gate)
        {
            if (_host?.TryGetTarget(out var attached) == true && ReferenceEquals(attached, host) &&
                ReferenceEquals(_current, request))
            {
                _current = null;
                completed = true;
            }
        }

        if (!completed) return;
        request.Cancellation.Dispose();
        request.Completion.TrySetResult(result);
        Pump();
    }

    private void Cancel(Request request)
    {
        bool isCurrent;
        lock (_gate)
        {
            isCurrent = ReferenceEquals(_current, request);
            if (!isCurrent) _pending.Remove(request);
        }

        // The displayed one has to close through the host so the surface and focus unwind;
        // a queued one never reached the host and completes here.
        if (isCurrent) Close();
        else
        {
            request.Cancellation.Dispose();
            request.Completion.TrySetResult(null);
        }
    }

    internal void Pump() => OnUiThread(host => host.TryShowNext(this));

    private void OnUiThread(Action<MdBannerHost> action)
    {
        // Staying synchronous on the UI thread keeps `host.ShowAsync(x)` followed by a read of
        // IsOpen/Banner behaving the way direct code-behind callers already expect.
        if (Dispatcher.UIThread.CheckAccess())
        {
            if (GetAttachedHost() is { } host) action(host);
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (GetAttachedHost() is { } host) action(host);
        });
    }

    private MdBannerHost? GetAttachedHost()
    {
        lock (_gate)
            return _host?.TryGetTarget(out var host) == true ? host : null;
    }

    internal sealed class Request(object banner)
    {
        public object Banner { get; } = banner;
        public TaskCompletionSource<object?> Completion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public CancellationTokenRegistration Cancellation { get; set; }
    }
}
