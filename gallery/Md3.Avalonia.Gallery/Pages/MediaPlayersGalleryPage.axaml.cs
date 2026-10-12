using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Gallery.Media;

namespace Md3.Avalonia.Gallery.Pages;

/// <summary>
/// Gallery host for real local media. The LibVLCSharp session lives in the Gallery assembly;
/// the public Md3.Avalonia controls remain backend-neutral request/state controls.
/// </summary>
public partial class MediaPlayersGalleryPage : UserControl
{
    private sealed record PickedMediaFile(string Title, string FilePath, string? TemporaryFilePath);

    private static readonly FilePickerFileType AudioFileType = new("Audio media")
    {
        Patterns = ["*.mp3", "*.wav", "*.m4a", "*.aac", "*.flac", "*.ogg", "*.opus", "*.wma"],
        MimeTypes = ["audio/*"],
        AppleUniformTypeIdentifiers = ["public.audio"]
    };

    private static readonly FilePickerFileType VideoFileType = new("Video media")
    {
        Patterns = ["*.mp4", "*.m4v", "*.mov", "*.mkv", "*.avi", "*.webm", "*.mpeg", "*.mpg"],
        MimeTypes = ["video/*"],
        AppleUniformTypeIdentifiers = ["public.movie"]
    };

    private readonly DispatcherTimer _playbackTimer;
    private readonly List<PickedMediaFile> _audioQueue = [];
    private GalleryMediaPlaybackSession? _session;
    private PickedMediaFile? _videoFile;
    private int _audioIndex = -1;
    private bool _audioLoaded;
    private bool _videoLoaded;
    private bool _isShuffleEnabled;
    private bool _isVideoFullScreen;
    private bool _queueWasVisibleBeforeFullScreen;
    private Window? _fullScreenWindow;
    private WindowState? _windowStateBeforeFullScreen;
    private MdPlaybackRepeatMode _repeatMode = MdPlaybackRepeatMode.None;

    public MediaPlayersGalleryPage()
    {
        InitializeComponent();
        AutomationProperties.SetLiveSetting(StatusLabel, AutomationLiveSetting.Polite);
        _playbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _playbackTimer.Tick += OnPlaybackTimerTick;
        RefreshAudioQueue();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _playbackTimer.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _playbackTimer.Stop();
        DisposePlaybackSession();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && _isVideoFullScreen)
        {
            SetVideoFullScreen(false);
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private async void OpenAudioFiles(object? sender, RoutedEventArgs e)
    {
        var pickedFiles = await PickMediaFilesAsync(AudioFileType, "Open audio files", allowMultiple: true);
        if (pickedFiles.Count == 0)
            return;
        if (!this.IsAttachedToVisualTree())
        {
            DeleteTemporaryFiles(pickedFiles);
            return;
        }

        var session = EnsurePlaybackSession();
        if (session is null)
        {
            DeleteTemporaryFiles(pickedFiles);
            return;
        }

        var previousQueue = _audioQueue.ToArray();
        var previousIndex = _audioIndex;
        _audioQueue.Clear();
        _audioQueue.AddRange(pickedFiles);
        _audioIndex = -1;

        if (!LoadAudioTrack(0))
        {
            DeleteTemporaryFiles(_audioQueue);
            _audioQueue.Clear();
            _audioQueue.AddRange(previousQueue);
            if (previousIndex >= 0 && previousIndex < _audioQueue.Count && LoadAudioTrack(previousIndex))
            {
                SetStatus($"The new audio selection could not be played. Continuing with: {_audioQueue[previousIndex].Title}");
                RefreshAudioQueue();
                return;
            }

            DeleteTemporaryFiles(previousQueue);
            _audioQueue.Clear();
            ResetAudioPlaybackState();
            RefreshAudioQueue();
            return;
        }

        DeleteTemporaryFiles(previousQueue);
        RefreshAudioQueue();
    }

    private async void OpenVideoFile(object? sender, RoutedEventArgs e)
    {
        var pickedFiles = await PickMediaFilesAsync(VideoFileType, "Open a video file", allowMultiple: false);
        if (pickedFiles.Count == 0)
            return;
        if (!this.IsAttachedToVisualTree())
        {
            DeleteTemporaryFiles(pickedFiles);
            return;
        }

        var session = EnsurePlaybackSession();
        if (session is null)
        {
            DeleteTemporaryFiles(pickedFiles);
            return;
        }

        var previousFile = _videoFile;
        var selectedFile = pickedFiles[0];
        _videoFile = selectedFile;
        try
        {
            if (!session.OpenVideo(selectedFile.FilePath))
                throw new InvalidOperationException("LibVLC could not start the selected video.");

            _videoLoaded = true;
            VideoSurfacePlaceholder.IsVisible = false;
            VideoControls.Title = selectedFile.Title;
            VideoControls.IsEnabled = true;
            VideoControls.IsVisible = true;
            SyncVideoState();
            SetStatus($"Playing video: {selectedFile.Title}");
            DeleteTemporaryFile(previousFile);
        }
        catch (Exception exception)
        {
            var failedFile = _videoFile;
            _videoFile = previousFile;
            DeleteTemporaryFile(failedFile);
            _videoLoaded = false;

            if (previousFile is not null && _session is not null)
            {
                try
                {
                    _videoLoaded = _session.OpenVideo(previousFile.FilePath);
                }
                catch
                {
                    _videoLoaded = false;
                }
            }

            VideoSurfacePlaceholder.IsVisible = !_videoLoaded;
            VideoControls.IsEnabled = _videoLoaded;
            VideoControls.IsVisible = _videoLoaded;
            VideoControls.Title = _videoLoaded ? _videoFile?.Title : null;
            if (_videoLoaded)
                SyncVideoState();
            SetStatus(_videoLoaded
                ? $"Could not open the new video. Continuing with: {previousFile!.Title}. {exception.Message}"
                : $"Could not open the selected video: {exception.Message}");
        }
    }

    private async Task<IReadOnlyList<PickedMediaFile>> PickMediaFilesAsync(
        FilePickerFileType fileType,
        string title,
        bool allowMultiple)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null)
        {
            SetStatus("This Gallery host does not provide a local file picker.");
            return [];
        }

        try
        {
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = allowMultiple,
                FileTypeFilter = [fileType]
            });

            var result = new List<PickedMediaFile>(files.Count);
            var consumedFiles = new HashSet<IStorageFile>();
            try
            {
                foreach (var file in files)
                {
                    consumedFiles.Add(file);
                    result.Add(await MakeReadableFileAsync(file));
                }
                return result;
            }
            catch
            {
                foreach (var file in files)
                {
                    if (!consumedFiles.Contains(file))
                        file.Dispose();
                }
                DeleteTemporaryFiles(result);
                throw;
            }
        }
        catch (Exception exception)
        {
            SetStatus($"Could not open the file picker: {exception.Message}");
            return [];
        }
    }

    private static async Task<PickedMediaFile> MakeReadableFileAsync(IStorageFile file)
    {
        try
        {
            if (file.Path.IsFile && File.Exists(file.Path.LocalPath))
                return new PickedMediaFile(Path.GetFileNameWithoutExtension(file.Name), file.Path.LocalPath, null);

            // Android content:// selections and other provider-backed files need a filesystem path
            // for LibVLC. Copy only those; regular desktop files play directly from their original path.
            var extension = Path.GetExtension(file.Name);
            var temporaryPath = Path.Combine(Path.GetTempPath(), $"md3-gallery-{Guid.NewGuid():N}{extension}");
            try
            {
                await using var source = await file.OpenReadAsync();
                await using var destination = new FileStream(
                    temporaryPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true);
                await source.CopyToAsync(destination);
                return new PickedMediaFile(Path.GetFileNameWithoutExtension(file.Name), temporaryPath, temporaryPath);
            }
            catch
            {
                DeleteTemporaryFile(temporaryPath);
                throw;
            }
        }
        finally
        {
            file.Dispose();
        }
    }

    private GalleryMediaPlaybackSession? EnsurePlaybackSession()
    {
        if (_session is not null)
            return _session;

        GalleryMediaPlaybackSession? session = null;
        try
        {
            session = new GalleryMediaPlaybackSession(VideoSurface);
            session.AudioPlayer.EndReached += OnAudioEndReached;
            session.AudioPlayer.EncounteredError += OnAudioPlaybackError;
            session.VideoPlayer.EncounteredError += OnVideoPlaybackError;
            _session = session;
            return session;
        }
        catch (Exception exception)
        {
            try
            {
                session?.Dispose();
            }
            catch
            {
                // Preserve the original initialization failure for the Gallery status message.
            }
            _session = null;
            VideoSurface.Reset();
            SetStatus($"The Gallery playback backend could not start: {exception.Message} On Linux, install VLC/libvlc and its plugins.");
            return null;
        }
    }

    private bool LoadAudioTrack(int index)
    {
        if (_session is null || index < 0 || index >= _audioQueue.Count)
            return false;

        var previousIndex = _audioIndex;
        var wasPreviouslyLoaded = _audioLoaded;
        var track = _audioQueue[index];
        try
        {
            if (!_session.OpenAudio(track.FilePath))
                throw new InvalidOperationException("LibVLC could not start the selected audio file.");

            _audioIndex = index;
            _audioLoaded = true;
            SetAudioControlsEnabled(true);
            MiniPlayer.Title = track.Title;
            MiniPlayer.Artist = "Local audio file";
            NowPlaying.Title = track.Title;
            NowPlaying.Artist = "Local audio file";
            NowPlaying.Album = "Gallery playback queue";
            SyncAudioState();
            RefreshAudioQueue();
            SetStatus($"Playing audio: {track.Title}");
            return true;
        }
        catch (Exception exception)
        {
            SetStatus($"Could not open audio file '{track.Title}': {exception.Message}");
            if (wasPreviouslyLoaded && previousIndex >= 0 && previousIndex < _audioQueue.Count && _session is not null)
            {
                try
                {
                    if (_session.OpenAudio(_audioQueue[previousIndex].FilePath))
                    {
                        _audioIndex = previousIndex;
                        _audioLoaded = true;
                        SetAudioControlsEnabled(true);
                        SyncAudioState();
                        RefreshAudioQueue();
                        return false;
                    }
                }
                catch
                {
                    // Leave the controls cleared if the previous track cannot be resumed either.
                }
            }

            ResetAudioPlaybackState();
            return false;
        }
    }

    private void PlaybackRequested(object? sender, MdPlaybackRequestEventArgs e)
    {
        if (ReferenceEquals(sender, VideoControls))
        {
            HandleVideoRequest(e.Request);
            return;
        }

        HandleAudioRequest(e.Request);
    }

    private void HandleAudioRequest(MdPlaybackRequest request)
    {
        var player = _session?.AudioPlayer;
        if (!_audioLoaded || player is null)
            return;

        try
        {
            switch (request)
            {
                case MdPlaybackRequest.Play:
                    player.Play();
                    break;
                case MdPlaybackRequest.Pause:
                    player.SetPause(true);
                    break;
                case MdPlaybackRequest.Previous:
                    PreviousAudioTrack();
                    break;
                case MdPlaybackRequest.Next:
                    NextAudioTrack();
                    break;
                case MdPlaybackRequest.ToggleShuffle:
                    _isShuffleEnabled = NowPlaying.IsShuffleEnabled;
                    RefreshAudioQueue();
                    break;
                case MdPlaybackRequest.CycleRepeat:
                    _repeatMode = NowPlaying.RepeatMode;
                    break;
                case MdPlaybackRequest.OpenQueue:
                    QueuePanel.IsVisible = true;
                    QueuePanel.BringIntoView();
                    break;
            }
            SyncAudioState();
        }
        catch (Exception exception)
        {
            SetStatus($"Audio playback request failed: {exception.Message}");
        }
    }

    private void HandleVideoRequest(MdPlaybackRequest request)
    {
        var player = _session?.VideoPlayer;
        if (!_videoLoaded || player is null)
            return;

        try
        {
            switch (request)
            {
                case MdPlaybackRequest.Play:
                    player.Play();
                    break;
                case MdPlaybackRequest.Pause:
                    player.SetPause(true);
                    break;
                case MdPlaybackRequest.Rewind:
                    SeekPlayer(player, TimeSpan.FromMilliseconds(Math.Max(0, player.Time) - 10_000));
                    break;
                case MdPlaybackRequest.Forward:
                    SeekPlayer(player, TimeSpan.FromMilliseconds(Math.Max(0, player.Time) + 10_000));
                    break;
                case MdPlaybackRequest.Mute:
                    player.Mute = true;
                    break;
                case MdPlaybackRequest.Unmute:
                    player.Mute = false;
                    break;
                case MdPlaybackRequest.EnterFullScreen:
                    SetVideoFullScreen(true);
                    break;
                case MdPlaybackRequest.ExitFullScreen:
                    SetVideoFullScreen(false);
                    break;
            }
            SyncVideoState();
        }
        catch (Exception exception)
        {
            SetStatus($"Video playback request failed: {exception.Message}");
        }
    }

    private void SeekRequested(object? sender, MdSeekRequestedEventArgs e)
    {
        try
        {
            switch (sender)
            {
                case MdSeekBar when _audioLoaded && _session is not null:
                case MdNowPlayingPanel when _audioLoaded && _session is not null:
                    SeekPlayer(_session.AudioPlayer, e.Position);
                    SyncAudioState();
                    break;
                case MdVideoPlayerControls when _videoLoaded && _session is not null:
                    SeekPlayer(_session.VideoPlayer, e.Position);
                    SyncVideoState();
                    break;
            }
        }
        catch (Exception exception)
        {
            SetStatus($"Seeking failed: {exception.Message}");
        }
    }

    private void OpenRequested(object? sender, EventArgs e) => NowPlaying.BringIntoView();

    private void CloseQueue(object? sender, RoutedEventArgs e) => QueuePanel.IsVisible = false;

    private void OnPlaybackTimerTick(object? sender, EventArgs e)
    {
        if (_audioLoaded)
            SyncAudioState();
        if (_videoLoaded)
            SyncVideoState();
        if (_isVideoFullScreen)
            UpdateVideoFullScreenBounds();
    }

    private void OnAudioEndReached(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_audioQueue.Count == 0 || _audioIndex < 0)
                return;

            if (_repeatMode == MdPlaybackRepeatMode.One)
            {
                LoadAudioTrack(_audioIndex);
                return;
            }

            var next = GetNextAudioIndex();
            if (next >= 0)
                LoadAudioTrack(next);
            else
            {
                SyncAudioState();
                SetStatus($"Finished audio: {_audioQueue[_audioIndex].Title}");
            }
        });
    }

    private void OnAudioPlaybackError(object? sender, EventArgs e) =>
        Dispatcher.UIThread.Post(() => SetStatus("LibVLC reported an audio playback error. Check that the file uses a supported codec."));

    private void OnVideoPlaybackError(object? sender, EventArgs e) =>
        Dispatcher.UIThread.Post(() => SetStatus("LibVLC reported a video playback error. Check that the file uses a supported codec."));

    private void PreviousAudioTrack()
    {
        var player = _session!.AudioPlayer;
        if (player.Time > 3_000)
        {
            SeekPlayer(player, TimeSpan.Zero);
            return;
        }

        if (_audioIndex > 0)
            LoadAudioTrack(_audioIndex - 1);
        else if (_repeatMode == MdPlaybackRepeatMode.All && _audioQueue.Count > 1)
            LoadAudioTrack(_audioQueue.Count - 1);
        else
            SeekPlayer(player, TimeSpan.Zero);
    }

    private void NextAudioTrack()
    {
        var next = GetNextAudioIndex();
        if (next >= 0)
            LoadAudioTrack(next);
        else
            SetStatus("This is the last track in the selected queue. Choose Repeat All or add more audio files to continue.");
    }

    private int GetNextAudioIndex()
    {
        if (_audioQueue.Count <= 1)
            return _repeatMode == MdPlaybackRepeatMode.All && _audioQueue.Count == 1 ? _audioIndex : -1;

        if (_isShuffleEnabled)
        {
            var choices = Enumerable.Range(0, _audioQueue.Count).Where(index => index != _audioIndex).ToArray();
            return choices[Random.Shared.Next(choices.Length)];
        }

        if (_audioIndex + 1 < _audioQueue.Count)
            return _audioIndex + 1;
        return _repeatMode == MdPlaybackRepeatMode.All ? 0 : -1;
    }

    private void SyncAudioState()
    {
        if (!_audioLoaded || _session is null || _audioIndex < 0 || _audioIndex >= _audioQueue.Count)
            return;

        var player = _session.AudioPlayer;
        var duration = FromLibVlcMilliseconds(player.Length);
        var position = Clamp(FromLibVlcMilliseconds(player.Time), duration);
        var track = _audioQueue[_audioIndex];

        MiniPlayer.Title = track.Title;
        MiniPlayer.Artist = "Local audio file";
        MiniPlayer.Duration = duration;
        MiniPlayer.Position = position;
        MiniPlayer.BufferedPosition = duration;
        MiniPlayer.IsPlaying = player.IsPlaying;
        MiniPlayer.CanPrevious = _audioIndex > 0 || _repeatMode == MdPlaybackRepeatMode.All;
        MiniPlayer.CanNext = _audioIndex + 1 < _audioQueue.Count || _repeatMode == MdPlaybackRepeatMode.All || (_isShuffleEnabled && _audioQueue.Count > 1);

        NowPlaying.Title = track.Title;
        NowPlaying.Artist = "Local audio file";
        NowPlaying.Album = "Gallery playback queue";
        NowPlaying.Duration = duration;
        NowPlaying.Position = position;
        NowPlaying.BufferedPosition = duration;
        NowPlaying.IsPlaying = player.IsPlaying;
        NowPlaying.IsBusy = false;
        NowPlaying.IsShuffleEnabled = _isShuffleEnabled;
        NowPlaying.RepeatMode = _repeatMode;
        NowPlaying.CanPrevious = MiniPlayer.CanPrevious;
        NowPlaying.CanNext = MiniPlayer.CanNext;
        NowPlaying.CanOpenQueue = _audioQueue.Count > 0;

        DemoSeekBar.Duration = duration;
        DemoSeekBar.Position = position;
        DemoSeekBar.BufferedPosition = duration;
    }

    private void SyncVideoState()
    {
        if (!_videoLoaded || _session is null)
            return;

        var player = _session.VideoPlayer;
        var duration = FromLibVlcMilliseconds(player.Length);
        VideoControls.Title = _videoFile?.Title;
        VideoControls.Duration = duration;
        VideoControls.Position = Clamp(FromLibVlcMilliseconds(player.Time), duration);
        VideoControls.BufferedPosition = duration;
        VideoControls.IsPlaying = player.IsPlaying;
        VideoControls.IsBuffering = false;
        VideoControls.IsMuted = player.Mute;
        VideoControls.IsFullScreen = _isVideoFullScreen;
        VideoControls.CanPrevious = false;
        VideoControls.CanNext = false;
    }

    private void RefreshAudioQueue()
    {
        QueueItems.Children.Clear();
        for (var i = 0; i < _audioQueue.Count; i++)
        {
            var index = i;
            var track = _audioQueue[i];
            var button = new MdButton
            {
                Content = $"{i + 1:00}  ·  {track.Title}",
                Variant = i == _audioIndex ? MdButtonVariant.Tonal : MdButtonVariant.Text,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            button.Click += (_, _) =>
            {
                if (LoadAudioTrack(index))
                    QueuePanel.IsVisible = false;
            };
            QueueItems.Children.Add(button);
        }
    }

    private void SetAudioControlsEnabled(bool enabled)
    {
        DemoSeekBar.IsEnabled = enabled;
        MiniPlayer.IsEnabled = enabled;
        NowPlaying.IsEnabled = enabled;
    }

    private void ResetAudioPlaybackState()
    {
        _audioLoaded = false;
        _audioIndex = -1;
        SetAudioControlsEnabled(false);

        MiniPlayer.Title = "No audio selected";
        MiniPlayer.Artist = "Open a local audio file";
        MiniPlayer.Duration = TimeSpan.Zero;
        MiniPlayer.Position = TimeSpan.Zero;
        MiniPlayer.BufferedPosition = TimeSpan.Zero;
        MiniPlayer.IsPlaying = false;
        MiniPlayer.CanPrevious = false;
        MiniPlayer.CanNext = false;

        NowPlaying.Title = "No audio selected";
        NowPlaying.Artist = "Open a local audio file";
        NowPlaying.Album = "Gallery playback";
        NowPlaying.Duration = TimeSpan.Zero;
        NowPlaying.Position = TimeSpan.Zero;
        NowPlaying.BufferedPosition = TimeSpan.Zero;
        NowPlaying.IsPlaying = false;
        NowPlaying.IsBusy = false;
        NowPlaying.IsShuffleEnabled = _isShuffleEnabled;
        NowPlaying.RepeatMode = _repeatMode;
        NowPlaying.CanPrevious = false;
        NowPlaying.CanNext = false;
        NowPlaying.CanOpenQueue = false;

        DemoSeekBar.Duration = TimeSpan.Zero;
        DemoSeekBar.Position = TimeSpan.Zero;
        DemoSeekBar.BufferedPosition = TimeSpan.Zero;
    }

    private void SetVideoFullScreen(bool isFullScreen)
    {
        if (_isVideoFullScreen == isFullScreen)
            return;

        var window = TopLevel.GetTopLevel(this) as Window;
        if (isFullScreen)
        {
            _fullScreenWindow = window;
            _windowStateBeforeFullScreen = window?.WindowState;
            _queueWasVisibleBeforeFullScreen = QueuePanel.IsVisible;
            if (window is not null)
                window.WindowState = WindowState.FullScreen;
        }
        else
        {
            var fullScreenWindow = _fullScreenWindow ?? window;
            if (fullScreenWindow is not null && _windowStateBeforeFullScreen is WindowState previousState)
                fullScreenWindow.WindowState = previousState;
            _windowStateBeforeFullScreen = null;
            _fullScreenWindow = null;
        }

        _isVideoFullScreen = isFullScreen;
        IntroSection.IsVisible = !isFullScreen;
        SeekSection.IsVisible = !isFullScreen;
        VideoSectionTitle.IsVisible = !isFullScreen;
        VideoSectionDescription.IsVisible = !isFullScreen;
        OpenVideoButton.IsVisible = !isFullScreen;
        AudioMiniSection.IsVisible = !isFullScreen;
        NowPlayingSection.IsVisible = !isFullScreen;
        StatusPanel.IsVisible = !isFullScreen;
        UsageSection.IsVisible = !isFullScreen;
        QueuePanel.IsVisible = isFullScreen ? false : _queueWasVisibleBeforeFullScreen;
        ContentStack.Spacing = isFullScreen ? 0 : 28;
        ContentStack.Margin = isFullScreen ? new Thickness(0) : new Thickness(0, 0, 12, 32);
        VideoSection.Spacing = isFullScreen ? 0 : 14;
        VideoFrame.CornerRadius = new CornerRadius(isFullScreen ? 0 : 24);
        VideoFrame.Margin = new Thickness(0);
        VideoFrame.MaxWidth = isFullScreen ? double.PositiveInfinity : 900;
        VideoFrame.Height = isFullScreen ? PageScrollViewer.Bounds.Height : 330;
        VideoFrame.MinHeight = isFullScreen ? VideoFrame.Height : 330;
        PageScrollViewer.VerticalScrollBarVisibility = isFullScreen
            ? Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
            : Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;

        if (isFullScreen)
            UpdateVideoFullScreenBounds();

        SyncVideoState();
    }

    private void UpdateVideoFullScreenBounds()
    {
        var height = PageScrollViewer.Bounds.Height;
        if (height <= 0)
            height = (TopLevel.GetTopLevel(this)?.ClientSize.Height ?? 0) - 1;
        if (height > 0)
        {
            VideoFrame.Height = height;
            VideoFrame.MinHeight = height;
        }
    }

    private void DisposePlaybackSession()
    {
        if (_isVideoFullScreen)
            SetVideoFullScreen(false);

        var session = _session;
        _session = null;
        if (session is not null)
        {
            try
            {
                session.AudioPlayer.EndReached -= OnAudioEndReached;
                session.AudioPlayer.EncounteredError -= OnAudioPlaybackError;
                session.VideoPlayer.EncounteredError -= OnVideoPlaybackError;
            }
            catch
            {
                // Native event detachment is best-effort; still dispose all player resources.
            }

            try
            {
                session.Dispose();
            }
            catch (Exception exception)
            {
                SetStatus($"The Gallery playback backend did not shut down cleanly: {exception.Message}");
            }
        }

        VideoSurface.Reset();
        DeleteTemporaryFiles(_audioQueue);
        _audioQueue.Clear();
        DeleteTemporaryFile(_videoFile);
        _videoFile = null;
        _videoLoaded = false;
        QueuePanel.IsVisible = false;
        ResetAudioPlaybackState();
        VideoControls.IsEnabled = false;
        VideoControls.IsVisible = false;
        VideoControls.Title = null;
        VideoSurfacePlaceholder.IsVisible = true;
        VideoFileName.Text = string.Empty;
    }

    private static void SeekPlayer(LibVLCSharp.Shared.MediaPlayer player, TimeSpan position)
    {
        var length = player.Length;
        var targetMilliseconds = Math.Max(0, (long)position.TotalMilliseconds);
        if (length >= 0)
            targetMilliseconds = Math.Min(targetMilliseconds, length);
        player.Time = targetMilliseconds;
    }

    private static TimeSpan FromLibVlcMilliseconds(long milliseconds) =>
        TimeSpan.FromMilliseconds(Math.Max(0, milliseconds));

    private static TimeSpan Clamp(TimeSpan value, TimeSpan maximum) =>
        value < TimeSpan.Zero ? TimeSpan.Zero : value > maximum ? maximum : value;

    private void SetStatus(string message) => StatusLabel.Text = message;

    private static void DeleteTemporaryFiles(IEnumerable<PickedMediaFile> files)
    {
        foreach (var file in files)
            DeleteTemporaryFile(file);
    }

    private static void DeleteTemporaryFile(PickedMediaFile? file)
    {
        if (file?.TemporaryFilePath is not null)
            DeleteTemporaryFile(file.TemporaryFilePath);
    }

    private static void DeleteTemporaryFile(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
            // Best-effort cleanup; the operating system will reclaim the Gallery temp file later.
        }
        catch (UnauthorizedAccessException)
        {
            // Best-effort cleanup for provider-backed temporary media.
        }
    }
}
