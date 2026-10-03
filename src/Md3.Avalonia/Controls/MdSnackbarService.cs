using Avalonia.Threading;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Queues snackbar descriptions for one attached <see cref="MdSnackbarHost"/>. Register the same
/// instance as <see cref="IMdSnackbarService"/> for view models and assign it to the host in the
/// application shell.
/// </summary>
public sealed class MdSnackbarService : IMdSnackbarService
{
    private readonly object _gate = new();
    private readonly Queue<Request> _pending = new();
    private WeakReference<MdSnackbarHost>? _host;
    private Request? _current;

    public Task<MdSnackbarResult> ShowAsync(MdSnackbarMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var request = new Request(message);
        lock (_gate) _pending.Enqueue(request);
        NotifyHost();
        return request.Completion.Task;
    }

    public void Show(MdSnackbarMessage message) => _ = ShowAsync(message);

    public void Dismiss()
    {
        Dispatcher.UIThread.Post(() => GetAttachedHost()?.DismissFromService(this));
    }

    internal void Attach(MdSnackbarHost host)
    {
        MdSnackbarHost? previous = null;
        lock (_gate)
        {
            if (_host?.TryGetTarget(out previous) == true && ReferenceEquals(previous, host)) return;
            _host = new WeakReference<MdSnackbarHost>(host);
        }

        previous?.ReleaseService(this);
        host.TryShowNext(this);
    }

    internal void Detach(MdSnackbarHost host)
    {
        lock (_gate)
        {
            if (_host?.TryGetTarget(out var attached) == true && ReferenceEquals(attached, host))
                _host = null;
        }
    }

    internal Request? AcquireCurrent(MdSnackbarHost host)
    {
        lock (_gate)
        {
            if (_host?.TryGetTarget(out var attached) != true || !ReferenceEquals(attached, host))
                return null;
            if (_current is null && _pending.Count > 0) _current = _pending.Dequeue();
            return _current;
        }
    }

    internal void Complete(MdSnackbarHost host, Request request, MdSnackbarResult result)
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

        if (completed) request.Completion.TrySetResult(result);
    }

    private MdSnackbarHost? GetAttachedHost()
    {
        lock (_gate)
            return _host?.TryGetTarget(out var host) == true ? host : null;
    }

    private void NotifyHost()
    {
        Dispatcher.UIThread.Post(() => GetAttachedHost()?.TryShowNext(this));
    }

    internal sealed class Request
    {
        public Request(MdSnackbarMessage message)
        {
            Message = message;
            Completion = new TaskCompletionSource<MdSnackbarResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public MdSnackbarMessage Message { get; }
        public TaskCompletionSource<MdSnackbarResult> Completion { get; }
    }
}
