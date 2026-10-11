using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Md3.Avalonia.Localization;

namespace Md3.Avalonia.Chat;

/// <summary>Load state of an <see cref="MdAttachmentImage"/>.</summary>
public enum MdImageLoadState
{
    /// <summary>No attachment is set.</summary>
    Empty,
    /// <summary>The picture is being loaded through <see cref="IMdImageProvider"/>.</summary>
    Loading,
    /// <summary>The picture is shown.</summary>
    Loaded,
    /// <summary>The picture could not be loaded; the control offers a retry.</summary>
    Failed,
}

/// <summary>
/// Shows the picture of an image attachment. A picture that is already decoded is shown directly; a
/// <see cref="MdChatAttachment.Uri"/> is loaded through <see cref="Provider"/> with a loading state and
/// a retry button when loading fails.
/// </summary>
[PseudoClasses(":loading", ":loaded", ":failed")]
public sealed class MdAttachmentImage : TemplatedControl
{
    public static readonly StyledProperty<MdChatAttachment?> AttachmentProperty =
        AvaloniaProperty.Register<MdAttachmentImage, MdChatAttachment?>(nameof(Attachment));
    public static readonly StyledProperty<IMdImageProvider?> ProviderProperty =
        AvaloniaProperty.Register<MdAttachmentImage, IMdImageProvider?>(nameof(Provider));
    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<MdAttachmentImage, IImage?>(nameof(Source));
    public static readonly DirectProperty<MdAttachmentImage, MdImageLoadState> LoadStateProperty =
        AvaloniaProperty.RegisterDirect<MdAttachmentImage, MdImageLoadState>(nameof(LoadState), control => control.LoadState);
    public static readonly DirectProperty<MdAttachmentImage, string> StatusTextProperty =
        AvaloniaProperty.RegisterDirect<MdAttachmentImage, string>(nameof(StatusText), control => control.StatusText);

    private MdImageLoadState _loadState = MdImageLoadState.Empty;
    private string _statusText = string.Empty;
    private CancellationTokenSource? _cancellation;
    private Button? _retryButton;

    static MdAttachmentImage()
    {
        AttachmentProperty.Changed.AddClassHandler<MdAttachmentImage>((control, _) => control.Reload());
        ProviderProperty.Changed.AddClassHandler<MdAttachmentImage>((control, _) => control.Reload());
    }

    public MdAttachmentImage() => UpdateState(MdImageLoadState.Empty);

    /// <summary>The attachment to show.</summary>
    public MdChatAttachment? Attachment { get => GetValue(AttachmentProperty); set => SetValue(AttachmentProperty, value); }

    /// <summary>Loads <see cref="MdChatAttachment.Uri"/> when the attachment has no decoded picture.</summary>
    public IMdImageProvider? Provider { get => GetValue(ProviderProperty); set => SetValue(ProviderProperty, value); }

    /// <summary>The picture currently shown.</summary>
    public IImage? Source { get => GetValue(SourceProperty); private set => SetValue(SourceProperty, value); }

    /// <summary>The current load state.</summary>
    public MdImageLoadState LoadState { get => _loadState; private set => SetAndRaise(LoadStateProperty, ref _loadState, value); }

    /// <summary>Localized text for the loading or failed state; empty otherwise.</summary>
    public string StatusText { get => _statusText; private set => SetAndRaise(StatusTextProperty, ref _statusText, value); }

    /// <summary>Loads the picture again, for example after a failure.</summary>
    public void Retry() => Reload();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_retryButton is not null) _retryButton.Click -= OnRetryClick;
        _retryButton = e.NameScope.Find<Button>("PART_Retry");
        if (_retryButton is null) return;
        _retryButton.Click += OnRetryClick;
        _retryButton.Content = MdLocalization.GetString("Retry", this);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // A load cancelled by detaching (for example when the list recycles the row) is restarted.
        if (LoadState == MdImageLoadState.Loading && _cancellation is null) Reload();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _cancellation?.Cancel();
        _cancellation = null;
    }

    private void OnRetryClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        Retry();
    }

    private void Reload()
    {
        _cancellation?.Cancel();
        _cancellation = null;
        var attachment = Attachment;
        if (attachment is null)
        {
            Source = null;
            UpdateState(MdImageLoadState.Empty);
            return;
        }
        if (attachment.Image is { } decoded)
        {
            Source = decoded;
            UpdateState(MdImageLoadState.Loaded);
            return;
        }
        // The thumbnail, when there is one, stands in until the full picture arrives.
        Source = attachment.Thumbnail;
        var provider = Provider;
        var uri = attachment.Uri;
        if (provider is null || string.IsNullOrWhiteSpace(uri))
        {
            UpdateState(MdImageLoadState.Failed);
            return;
        }
        var cancellation = new CancellationTokenSource();
        _cancellation = cancellation;
        UpdateState(MdImageLoadState.Loading);
        _ = LoadAsync(provider, uri, cancellation);
    }

    private async Task LoadAsync(IMdImageProvider provider, string uri, CancellationTokenSource cancellation)
    {
        try
        {
            var image = await provider.LoadImageAsync(uri, cancellation.Token);
            // A superseded or cancelled load must not overwrite the current state.
            if (!ReferenceEquals(_cancellation, cancellation) || cancellation.IsCancellationRequested) return;
            Source = image;
            UpdateState(image is null ? MdImageLoadState.Failed : MdImageLoadState.Loaded);
        }
        catch (OperationCanceledException)
        {
            // Superseded, reloaded or detached: nothing to show.
        }
        catch (Exception)
        {
            if (ReferenceEquals(_cancellation, cancellation) && !cancellation.IsCancellationRequested)
                UpdateState(MdImageLoadState.Failed);
        }
    }

    private void UpdateState(MdImageLoadState state)
    {
        LoadState = state;
        PseudoClasses.Set(":loading", state == MdImageLoadState.Loading);
        PseudoClasses.Set(":loaded", state == MdImageLoadState.Loaded);
        PseudoClasses.Set(":failed", state == MdImageLoadState.Failed);
        StatusText = state switch
        {
            MdImageLoadState.Loading => MdLocalization.GetString("ChatImageLoading", this),
            MdImageLoadState.Failed => MdLocalization.GetString("ChatImageFailed", this),
            _ => string.Empty,
        };
    }
}
