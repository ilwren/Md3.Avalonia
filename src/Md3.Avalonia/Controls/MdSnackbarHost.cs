using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Places snackbar messages over application content and connects an <see cref="MdSnackbarService"/>
/// to the visual tree. Messages are presented one at a time in queue order.
/// </summary>
[TemplatePart("PART_Snackbar", typeof(MdSnackbar))]
public sealed class MdSnackbarHost : ContentControl, IMdSnackbarService
{
    public static readonly StyledProperty<MdSnackbarService?> ServiceProperty =
        AvaloniaProperty.Register<MdSnackbarHost, MdSnackbarService?>(nameof(Service));

    private readonly MdSnackbarService _localService = new();
    private readonly DispatcherTimer _nextMessageTimer;
    private MdSnackbarService? _connectedService;
    private MdSnackbarService.Request? _activeRequest;
    private MdSnackbar? _snackbar;
    private bool _actionInvoked;
    private bool _isAttached;

    static MdSnackbarHost()
    {
        ServiceProperty.Changed.AddClassHandler<MdSnackbarHost>((host, _) => host.ConnectService());
    }

    public MdSnackbarHost()
    {
        _nextMessageTimer = new DispatcherTimer();
        _nextMessageTimer.Tick += (_, _) =>
        {
            _nextMessageTimer.Stop();
            if (_connectedService is not null) TryShowNext(_connectedService);
        };
    }

    /// <summary>
    /// Shared service used by view models. When null, the host's direct Show/ShowAsync methods use
    /// a private service.
    /// </summary>
    public MdSnackbarService? Service
    {
        get => GetValue(ServiceProperty);
        set => SetValue(ServiceProperty, value);
    }

    public Task<MdSnackbarResult> ShowAsync(MdSnackbarMessage message) =>
        (Service ?? _localService).ShowAsync(message);

    public void Show(MdSnackbarMessage message) => (Service ?? _localService).Show(message);

    public void Dismiss() => (Service ?? _localService).Dismiss();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_snackbar is not null)
        {
            _snackbar.ActionInvoked -= OnActionInvoked;
            _snackbar.Dismissed -= OnDismissed;
        }

        base.OnApplyTemplate(e);
        _snackbar = e.NameScope.Find<MdSnackbar>("PART_Snackbar");
        if (_snackbar is not null)
        {
            _snackbar.ActionInvoked += OnActionInvoked;
            _snackbar.Dismissed += OnDismissed;
            if (_activeRequest is not null) Present(_activeRequest.Message);
        }

        if (_connectedService is not null) TryShowNext(_connectedService);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        ConnectService();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        _nextMessageTimer.Stop();
        DisconnectService();
        base.OnDetachedFromVisualTree(e);
    }

    internal void TryShowNext(MdSnackbarService service)
    {
        if (!_isAttached || !ReferenceEquals(_connectedService, service) ||
            _activeRequest is not null || _snackbar is null)
            return;

        _activeRequest = service.AcquireCurrent(this);
        if (_activeRequest is not null) Present(_activeRequest.Message);
    }

    internal void DismissFromService(MdSnackbarService service)
    {
        if (ReferenceEquals(_connectedService, service) && _activeRequest is not null)
            _snackbar?.Dismiss();
    }

    internal void ReleaseService(MdSnackbarService service)
    {
        if (!ReferenceEquals(_connectedService, service)) return;
        _nextMessageTimer.Stop();
        if (_snackbar is not null) _snackbar.SetCurrentValue(MdSnackbar.IsOpenProperty, false);
        _activeRequest = null;
        _connectedService = null;
    }

    private void ConnectService()
    {
        if (!_isAttached) return;
        var service = Service ?? _localService;
        if (ReferenceEquals(_connectedService, service))
        {
            TryShowNext(service);
            return;
        }

        DisconnectService();
        _connectedService = service;
        service.Attach(this);
    }

    private void DisconnectService()
    {
        var service = _connectedService;
        if (service is null) return;
        _nextMessageTimer.Stop();
        if (_snackbar is not null) _snackbar.SetCurrentValue(MdSnackbar.IsOpenProperty, false);
        _activeRequest = null;
        _connectedService = null;
        service.Detach(this);
    }

    private void Present(MdSnackbarMessage message)
    {
        if (_snackbar is null) return;
        _actionInvoked = false;
        _snackbar.SetCurrentValue(ContentControl.ContentProperty, message.Content);
        _snackbar.SetCurrentValue(MdSnackbar.ActionContentProperty, message.ActionContent);
        _snackbar.SetCurrentValue(MdSnackbar.ActionCommandProperty, message.ActionCommand);
        _snackbar.SetCurrentValue(MdSnackbar.ActionCommandParameterProperty, message.ActionCommandParameter);
        _snackbar.SetCurrentValue(MdSnackbar.IsDismissibleProperty, message.IsDismissible);
        _snackbar.SetCurrentValue(MdSnackbar.DurationProperty, message.Duration);
        _snackbar.Show();
    }

    private void OnActionInvoked(object? sender, EventArgs e) => _actionInvoked = true;

    private void OnDismissed(object? sender, EventArgs e)
    {
        var service = _connectedService;
        var request = _activeRequest;
        if (service is null || request is null) return;

        _activeRequest = null;
        service.Complete(this, request,
            _actionInvoked ? MdSnackbarResult.ActionInvoked : MdSnackbarResult.Dismissed);
        _actionInvoked = false;

        var exitDuration = MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast);
        if (exitDuration <= TimeSpan.Zero)
        {
            TryShowNext(service);
            return;
        }

        _nextMessageTimer.Interval = exitDuration;
        _nextMessageTimer.Start();
    }
}
