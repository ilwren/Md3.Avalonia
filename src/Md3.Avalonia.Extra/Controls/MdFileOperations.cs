using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>Current lifecycle state of a provider operation shown in the operation center.</summary>
public enum MdFileOperationState
{
    Queued,
    Running,
    Completed,
    Skipped,
    Conflict,
    Failed,
    Cancelled
}

/// <summary>Observable record for one running or completed file operation.</summary>
public sealed class MdFileOperationTask : INotifyPropertyChanged, IDisposable
{
    private readonly CancellationTokenSource _cancellation;
    private MdFileOperationState _state = MdFileOperationState.Queued;
    private MdFileOperationProgress? _progress;
    private MdFileOperationResult? _result;

    internal MdFileOperationTask(MdFileOperationRequest request, CancellationToken cancellationToken)
    {
        Request = request;
        _cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Title = request.Kind switch
        {
            MdFileOperationKind.CreateDirectory => $"Creating {request.NewName ?? "folder"}",
            MdFileOperationKind.Rename => $"Renaming {request.SourcePath}",
            MdFileOperationKind.Delete => $"Deleting {request.SourcePath}",
            MdFileOperationKind.Copy => $"Copying {request.SourcePath}",
            MdFileOperationKind.Move => $"Moving {request.SourcePath}",
            _ => "File operation"
        };
    }

    public MdFileOperationRequest Request { get; }
    public string Title { get; }
    public MdFileOperationState State { get => _state; private set => SetField(ref _state, value); }
    public MdFileOperationProgress? Progress { get => _progress; private set { if (SetField(ref _progress, value)) OnPropertyChanged(nameof(Fraction)); } }
    public MdFileOperationResult? Result { get => _result; private set => SetField(ref _result, value); }
    public string? ErrorMessage => Result?.ErrorMessage;
    public double? Fraction => Progress?.Fraction;
    public string StatusText => State switch
    {
        MdFileOperationState.Queued => "Waiting",
        MdFileOperationState.Running => Progress?.CurrentItem is { Length: > 0 } current ? $"Working on {current}" : "In progress",
        MdFileOperationState.Completed => "Completed",
        MdFileOperationState.Skipped => "Skipped",
        MdFileOperationState.Conflict => "Needs attention",
        MdFileOperationState.Failed => ErrorMessage ?? "Failed",
        MdFileOperationState.Cancelled => "Cancelled",
        _ => string.Empty
    };
    public bool CanCancel => State is MdFileOperationState.Queued or MdFileOperationState.Running;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void Cancel()
    {
        if (CanCancel && !_cancellation.IsCancellationRequested) _cancellation.Cancel();
    }

    internal CancellationToken Token => _cancellation.Token;
    internal void SetState(MdFileOperationState state) => State = state;
    internal void SetProgress(MdFileOperationProgress progress) => Progress = progress;

    internal void Complete(MdFileOperationResult result)
    {
        Result = result;
        State = result.HasConflict ? MdFileOperationState.Conflict
            : result.WasSkipped ? MdFileOperationState.Skipped
            : result.Succeeded ? MdFileOperationState.Completed
            : MdFileOperationState.Failed;
        OnPropertyChanged(nameof(ErrorMessage));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(CanCancel));
    }

    internal void Cancelled()
    {
        State = MdFileOperationState.Cancelled;
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(CanCancel));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        if (propertyName is nameof(State) or nameof(Result) or nameof(Progress))
        {
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(CanCancel));
        }
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public void Dispose() => _cancellation.Dispose();
}

/// <summary>Serializes operations through one provider and keeps observable progress and results.</summary>
public sealed class MdFileOperationQueue : ObservableCollection<MdFileOperationTask>
{
    private readonly SemaphoreSlim _serial = new(1, 1);

    public async Task<MdFileOperationResult> ExecuteAsync(
        IMdFileSystemProvider provider,
        MdFileOperationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(request);
        var task = new MdFileOperationTask(request, cancellationToken);
        Add(task);
        var acquired = false;
        try
        {
            await _serial.WaitAsync(task.Token);
            acquired = true;
            task.SetState(MdFileOperationState.Running);
            var progress = new Progress<MdFileOperationProgress>(task.SetProgress);
            var result = await provider.ExecuteAsync(request, progress, task.Token);
            task.Complete(result);
            return result;
        }
        catch (OperationCanceledException)
        {
            task.Cancelled();
            var result = MdFileOperationResult.Failure("Operation cancelled.");
            return result;
        }
        catch (Exception exception)
        {
            var result = MdFileOperationResult.Failure(exception.Message);
            task.Complete(result);
            return result;
        }
        finally
        {
            if (acquired) _serial.Release();
            if (task.State is MdFileOperationState.Completed or MdFileOperationState.Skipped or MdFileOperationState.Conflict or MdFileOperationState.Failed or MdFileOperationState.Cancelled)
                task.Dispose();
        }
    }

    public void RemoveFinished()
    {
        foreach (var task in this.Where(task => !task.CanCancel).ToArray())
        {
            Remove(task);
            task.Dispose();
        }
    }
}

/// <summary>Live Material progress list for queued and completed file operations.</summary>
public sealed class MdFileOperationCenter : UserControl
{
    public static readonly StyledProperty<MdFileOperationQueue?> QueueProperty =
        AvaloniaProperty.Register<MdFileOperationCenter, MdFileOperationQueue?>(nameof(Queue));

    private readonly ItemsControl _items;
    private readonly MdText _emptyText;
    private MdFileOperationQueue? _observedQueue;

    static MdFileOperationCenter()
    {
        QueueProperty.Changed.AddClassHandler<MdFileOperationCenter>((center, _) => center.UpdateQueue());
    }

    public MdFileOperationCenter()
    {
        _emptyText = new MdText { Text = "No recent file activity", StyleRole = MdTextStyle.BodySmall, Opacity = 0.72 };
        _items = new ItemsControl
        {
            ItemTemplate = new FuncDataTemplate<MdFileOperationTask>((task, _) => task is null ? null : CreateTaskView(task))
        };
        Content = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                _emptyText,
                new MdScrollViewer
                {
                    MaxHeight = 240,
                    VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                    Content = _items
                }
            }
        };
        UpdateQueue();
    }

    public MdFileOperationQueue? Queue { get => GetValue(QueueProperty); set => SetValue(QueueProperty, value); }

    private void UpdateQueue()
    {
        if (_observedQueue is not null) _observedQueue.CollectionChanged -= OnQueueCollectionChanged;
        _observedQueue = Queue;
        if (_observedQueue is not null) _observedQueue.CollectionChanged += OnQueueCollectionChanged;
        if (_items is null) return;
        _items.ItemsSource = Queue;
        _emptyText.IsVisible = Queue is null || Queue.Count == 0;
    }

    private void OnQueueCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
        _emptyText.IsVisible = Queue is null || Queue.Count == 0;

    private static Control CreateTaskView(MdFileOperationTask task)
    {
        var title = new MdText { Text = task.Title, StyleRole = MdTextStyle.TitleSmall, TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis };
        var status = new MdText { Text = task.StatusText, StyleRole = MdTextStyle.BodySmall, TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis };
        var progress = new MdLinearProgressIndicator
        {
            Minimum = 0,
            Maximum = 1,
            Height = 12,
            IsIndeterminate = task.Fraction is null,
            Value = task.Fraction ?? 0,
            IsVisible = task.State is MdFileOperationState.Queued or MdFileOperationState.Running
        };
        var cancel = new MdButton { Content = "Cancel", Variant = MdButtonVariant.Text, IsVisible = task.CanCancel };
        cancel.Click += (_, _) => task.Cancel();
        task.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(task.State) or nameof(task.Progress) or nameof(task.StatusText) or nameof(task.Fraction))
            {
                status.Text = task.StatusText;
                progress.IsIndeterminate = task.Fraction is null;
                progress.Value = task.Fraction ?? 0;
                progress.IsVisible = task.State is MdFileOperationState.Queued or MdFileOperationState.Running;
                cancel.IsVisible = task.CanCancel;
            }
        };
        var layout = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto"),
            RowSpacing = 2,
            Children = { title, status, progress, cancel }
        };
        Grid.SetRow(status, 1);
        Grid.SetRow(progress, 2);
        Grid.SetRow(cancel, 0);
        Grid.SetColumn(cancel, 1);
        return new MdSurface
        {
            Variant = MdSurfaceVariant.Container,
            Padding = new Thickness(12),
            Content = layout
        };
    }
}
