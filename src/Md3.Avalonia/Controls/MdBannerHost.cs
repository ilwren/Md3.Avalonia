using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Hosts one persistent Material banner above its main content. A banner can be supplied as an
/// <see cref="MdBanner"/> or as a view model resolved by <see cref="BannerTemplate"/> or a matching
/// inherited <c>DataTemplate</c> whose root is an <see cref="MdBanner"/>.
/// </summary>
/// <remarks>
/// View code can use <see cref="ShowAsync(object, CancellationToken)"/> directly. View models can
/// receive only <see cref="IMdBannerService"/> while the view assigns the shared concrete service
/// to <see cref="Service"/>. Two-way <see cref="IsOpen"/> and <see cref="Banner"/> remain available
/// for binding-driven surfaces.
/// </remarks>
[TemplatePart("PART_MainContent", typeof(ContentPresenter))]
[TemplatePart("PART_BannerPresenter", typeof(ContentPresenter))]
[PseudoClasses(":open", ":closed", ":present")]
public sealed class MdBannerHost : ContentControl, IMdBannerService
{
    public static readonly StyledProperty<object?> BannerProperty =
        AvaloniaProperty.Register<MdBannerHost, object?>(nameof(Banner));
    public static readonly StyledProperty<IDataTemplate?> BannerTemplateProperty =
        AvaloniaProperty.Register<MdBannerHost, IDataTemplate?>(nameof(BannerTemplate));
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MdBannerHost, bool>(nameof(IsOpen),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<MdBannerService?> ServiceProperty =
        AvaloniaProperty.Register<MdBannerHost, MdBannerService?>(nameof(Service));

    private readonly MdPresenceController _presence;
    private readonly MdBannerService _localService = new();
    private MdBannerService? _connectedService;
    private MdBannerService.Request? _activeRequest;
    private ContentPresenter? _bannerPresenter;
    private MdBanner? _displayedBanner;
    private bool _isAttached;
    private bool _synchronizingBanner;
    private object? _closeResult;

    static MdBannerHost()
    {
        IsOpenProperty.Changed.AddClassHandler<MdBannerHost>((host, _) => host.UpdateOpenState());
        BannerProperty.Changed.AddClassHandler<MdBannerHost>((host, _) => host.ScheduleBannerStateUpdate());
        BannerTemplateProperty.Changed.AddClassHandler<MdBannerHost>((host, _) => host.ScheduleBannerStateUpdate());
        ServiceProperty.Changed.AddClassHandler<MdBannerHost>((host, _) => host.ConnectService());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdBannerHost>((host, _) => host.UpdateMotion());
    }

    public MdBannerHost()
    {
        _presence = new MdPresenceController(SetPresence);
        _presence.Initialize(IsOpen);
        UpdateOpenState();
    }

    /// <summary>The active banner control or view model.</summary>
    public object? Banner
    {
        get => GetValue(BannerProperty);
        set => SetValue(BannerProperty, value);
    }

    /// <summary>
    /// An optional template for <see cref="Banner"/>. Leave null to select an inherited
    /// <c>DataTemplate</c> by model type. The template should create an <see cref="MdBanner"/>.
    /// </summary>
    public IDataTemplate? BannerTemplate
    {
        get => GetValue(BannerTemplateProperty);
        set => SetValue(BannerTemplateProperty, value);
    }

    /// <summary>Whether the banner surface is open. The default binding mode is two-way.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// The shared service injected into view models. Leave null to use a private service for the
    /// host's direct API.
    /// </summary>
    public MdBannerService? Service
    {
        get => GetValue(ServiceProperty);
        set => SetValue(ServiceProperty, value);
    }

    /// <inheritdoc />
    public Task<object?> ShowAsync(object banner, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(banner);
        return (Service ?? _localService).ShowAsync(banner, cancellationToken);
    }

    /// <inheritdoc />
    public void Show(object banner) => (Service ?? _localService).Show(banner);

    /// <inheritdoc />
    public Task<object?> ReplaceAsync(object banner, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(banner);
        return (Service ?? _localService).ReplaceAsync(banner, cancellationToken);
    }

    /// <inheritdoc />
    public void Replace(object banner) => (Service ?? _localService).Replace(banner);

    /// <inheritdoc />
    public void Close(object? result = null)
    {
        _closeResult = result;
        SetCurrentValue(IsOpenProperty, false);
        _closeResult = null;
    }

    /// <summary>Displays the next queued request. Called by <see cref="MdBannerService"/>.</summary>
    internal void TryShowNext(MdBannerService service)
    {
        if (!_isAttached || !ReferenceEquals(_connectedService, service) ||
            _activeRequest is not null || IsOpen || _presence.IsPresent || _bannerPresenter is null)
            return;

        if (service.AcquireCurrent(this) is not { } request) return;
        _activeRequest = request;
        SetCurrentValue(BannerProperty, request.Banner);
        SetCurrentValue(IsOpenProperty, true);
    }

    internal void ReleaseService(MdBannerService service)
    {
        if (!ReferenceEquals(_connectedService, service)) return;
        CompleteActiveRequest(null);
        service.Detach(this);
        _connectedService = null;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _bannerPresenter = e.NameScope.Find<ContentPresenter>("PART_BannerPresenter");
        if (_bannerPresenter is not null) _bannerPresenter.IsVisible = _presence.IsPresent;
        ScheduleBannerStateUpdate();
        _connectedService?.Pump();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        ConnectService();
        UpdateOpenState();
        ScheduleBannerStateUpdate();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        _presence.Stop();
        UnsubscribeDisplayedBanner();
        if (_connectedService is { } service)
        {
            CompleteActiveRequest(null);
            service.Detach(this);
        }
        base.OnDetachedFromVisualTree(e);
    }

    private void ConnectService()
    {
        var service = Service ?? _localService;
        if (ReferenceEquals(_connectedService, service))
        {
            if (_isAttached) service.Attach(this);
            return;
        }

        DisconnectService();
        _connectedService = service;
        if (_isAttached) service.Attach(this);
    }

    private void DisconnectService()
    {
        if (_connectedService is null) return;
        CompleteActiveRequest(null);
        _connectedService.Detach(this);
        _connectedService = null;
    }

    private void CompleteActiveRequest(object? result)
    {
        if (_activeRequest is not { } request) return;
        _activeRequest = null;
        _connectedService?.Complete(this, request, result);
    }

    private void ScheduleBannerStateUpdate()
    {
        UpdateBannerState();
        if (_isAttached)
            Dispatcher.UIThread.Post(UpdateBannerState, DispatcherPriority.Render);
    }

    private void UpdateBannerState()
    {
        var displayedBanner = Banner as MdBanner ??
            _bannerPresenter?.GetVisualDescendants().OfType<MdBanner>().FirstOrDefault();
        if (!ReferenceEquals(_displayedBanner, displayedBanner))
        {
            UnsubscribeDisplayedBanner();
            _displayedBanner = displayedBanner;
            if (_displayedBanner is not null)
            {
                _displayedBanner.Dismissed += OnDisplayedBannerDismissed;
                _displayedBanner.PropertyChanged += OnDisplayedBannerPropertyChanged;
            }
        }

        if (_displayedBanner is null || _displayedBanner.IsOpen == IsOpen) return;
        _synchronizingBanner = true;
        try
        {
            _displayedBanner.SetCurrentValue(MdBanner.IsOpenProperty, IsOpen);
        }
        finally
        {
            _synchronizingBanner = false;
        }
    }

    private void UnsubscribeDisplayedBanner()
    {
        if (_displayedBanner is null) return;
        _displayedBanner.Dismissed -= OnDisplayedBannerDismissed;
        _displayedBanner.PropertyChanged -= OnDisplayedBannerPropertyChanged;
        _displayedBanner = null;
    }

    private void OnDisplayedBannerDismissed(object? sender, EventArgs e)
    {
        if (IsOpen) Close();
    }

    private void OnDisplayedBannerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != MdBanner.IsOpenProperty || _synchronizingBanner || _displayedBanner is null)
            return;

        if (!_displayedBanner.IsOpen && IsOpen)
            Close();
        else if (_displayedBanner.IsOpen != IsOpen)
            ScheduleBannerStateUpdate();
    }

    private void UpdateOpenState()
    {
        if (IsOpen)
        {
            PseudoClasses.Set(":closed", false);
            PseudoClasses.Set(":open", true);
            _presence.Update(true, TimeSpan.Zero);
        }
        else
        {
            PseudoClasses.Set(":open", false);
            PseudoClasses.Set(":closed", true);
            _presence.Update(false,
                MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
        }

        ScheduleBannerStateUpdate();
        if (!IsOpen) CompleteActiveRequest(_closeResult);
    }

    private void UpdateMotion()
    {
        if (!IsOpen)
            _presence.Update(false,
                MdMotion.GetExitDuration(this, MdMotionSpeed.Fast, MdMotionSpeed.Fast));
    }

    private void SetPresence(bool present)
    {
        PseudoClasses.Set(":present", present);
        if (_bannerPresenter is not null) _bannerPresenter.IsVisible = present;
        if (!present) _connectedService?.Pump();
    }
}
