using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Md3.Avalonia.Extra.Infrastructure;
using Md3.Avalonia.Localization;

namespace Md3.Avalonia.Chat;

/// <summary>
/// Provider-neutral chat shell with composer, history paging, suggestions, bubble-only multi-selection,
/// quote/delete actions, and failed-message retry. Providers remain responsible for mutating their source.
/// </summary>
[PseudoClasses(":selection", ":quoted")]
public sealed class MdChatView : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> MessagesSourceProperty = AvaloniaProperty.Register<MdChatView, IEnumerable?>(nameof(MessagesSource));
    public static readonly StyledProperty<string?> ComposerTextProperty = AvaloniaProperty.Register<MdChatView, string?>(nameof(ComposerText), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<IEnumerable?> SuggestionsSourceProperty = AvaloniaProperty.Register<MdChatView, IEnumerable?>(nameof(SuggestionsSource));
    public static readonly StyledProperty<ICommand?> SendCommandProperty = AvaloniaProperty.Register<MdChatView, ICommand?>(nameof(SendCommand));
    public static readonly StyledProperty<ICommand?> AttachmentCommandProperty = AvaloniaProperty.Register<MdChatView, ICommand?>(nameof(AttachmentCommand));
    public static readonly StyledProperty<ICommand?> DeleteMessagesCommandProperty = AvaloniaProperty.Register<MdChatView, ICommand?>(nameof(DeleteMessagesCommand));
    public static readonly StyledProperty<ICommand?> QuoteMessageCommandProperty = AvaloniaProperty.Register<MdChatView, ICommand?>(nameof(QuoteMessageCommand));
    public static readonly StyledProperty<ICommand?> RetryMessageCommandProperty = AvaloniaProperty.Register<MdChatView, ICommand?>(nameof(RetryMessageCommand));
    public static readonly StyledProperty<IMdMarkdownProvider?> MarkdownProviderProperty = AvaloniaProperty.Register<MdChatView, IMdMarkdownProvider?>(nameof(MarkdownProvider));
    public static readonly StyledProperty<IMdImageProvider?> ImageProviderProperty = AvaloniaProperty.Register<MdChatView, IMdImageProvider?>(nameof(ImageProvider));
    public static readonly StyledProperty<bool> IsBusyProperty = AvaloniaProperty.Register<MdChatView, bool>(nameof(IsBusy));
    public static readonly StyledProperty<bool> AllowMultipleSelectionProperty = AvaloniaProperty.Register<MdChatView, bool>(nameof(AllowMultipleSelection), true);
    public static readonly DirectProperty<MdChatView, int> SelectionCountProperty = AvaloniaProperty.RegisterDirect<MdChatView, int>(nameof(SelectionCount), control => control.SelectionCount);
    public static readonly DirectProperty<MdChatView, MdChatMessage?> QuotedMessageProperty = AvaloniaProperty.RegisterDirect<MdChatView, MdChatMessage?>(nameof(QuotedMessage), control => control.QuotedMessage);
    public static readonly DirectProperty<MdChatView, string> StatusTextProperty = AvaloniaProperty.RegisterDirect<MdChatView, string>(nameof(StatusText), control => control.StatusText);
    public static readonly DirectProperty<MdChatView, string> CancelTextProperty = AvaloniaProperty.RegisterDirect<MdChatView, string>(nameof(CancelText), control => control.CancelText);
    public static readonly DirectProperty<MdChatView, string> RetryTextProperty = AvaloniaProperty.RegisterDirect<MdChatView, string>(nameof(RetryText), control => control.RetryText);
    public static readonly DirectProperty<MdChatView, string> AttachFileTextProperty = AvaloniaProperty.RegisterDirect<MdChatView, string>(nameof(AttachFileText), control => control.AttachFileText);
    public static readonly DirectProperty<MdChatView, string> SendMessageTextProperty = AvaloniaProperty.RegisterDirect<MdChatView, string>(nameof(SendMessageText), control => control.SendMessageText);
    public static readonly DirectProperty<MdChatView, string> MessagesTextProperty = AvaloniaProperty.RegisterDirect<MdChatView, string>(nameof(MessagesText), control => control.MessagesText);
    public static readonly DirectProperty<MdChatView, string> QuoteTextProperty = AvaloniaProperty.RegisterDirect<MdChatView, string>(nameof(QuoteText), control => control.QuoteText);
    public static readonly DirectProperty<MdChatView, string> DeleteTextProperty = AvaloniaProperty.RegisterDirect<MdChatView, string>(nameof(DeleteText), control => control.DeleteText);
    public static readonly DirectProperty<MdChatView, string> TypeMessageTextProperty = AvaloniaProperty.RegisterDirect<MdChatView, string>(nameof(TypeMessageText), control => control.TypeMessageText);
    public static readonly DirectProperty<MdChatView, string> SelectionCountTextProperty = AvaloniaProperty.RegisterDirect<MdChatView, string>(nameof(SelectionCountText), control => control.SelectionCountText);
    public static readonly DirectProperty<MdChatView, string> QuoteHeaderTextProperty = AvaloniaProperty.RegisterDirect<MdChatView, string>(nameof(QuoteHeaderText), control => control.QuoteHeaderText);

    private Button? _sendButton;
    private Button? _attachmentButton;
    private Button? _cancelSelectionButton;
    private Button? _deleteButton;
    private Button? _quoteButton;
    private Button? _cancelQuoteButton;
    private ItemsControl? _suggestionsHost;
    private ListBox? _messagesHost;
    private TextBox? _composer;
    private int _selectionCount;
    private MdChatMessage? _quotedMessage;
    private string _statusText = string.Empty;
    private string _cancelText = string.Empty;
    private string _retryText = string.Empty;
    private string _attachFileText = string.Empty;
    private string _sendMessageText = string.Empty;
    private string _messagesText = string.Empty;
    private string _quoteText = string.Empty;
    private string _deleteText = string.Empty;
    private string _typeMessageText = string.Empty;
    private string _selectionCountText = string.Empty;
    private string _quoteHeaderText = string.Empty;
    private bool _synchronizingSelection;
    private INotifyCollectionChanged? _observedMessages;

    static MdChatView()
    {
        MessagesSourceProperty.Changed.AddClassHandler<MdChatView>((control, _) => control.OnMessagesSourceChanged());
        IsBusyProperty.Changed.AddClassHandler<MdChatView>((control, _) => control.StatusText = MdLocalization.GetString(control.IsBusy ? "LoadingMessages" : "MessagesReady", control));
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdChatView>((control, _) => control.UpdateLocalizedText());
    }

    public MdChatView() => UpdateLocalizedText();

    public IEnumerable? MessagesSource { get => GetValue(MessagesSourceProperty); set => SetValue(MessagesSourceProperty, value); }
    public string? ComposerText { get => GetValue(ComposerTextProperty); set => SetValue(ComposerTextProperty, value); }
    public IEnumerable? SuggestionsSource { get => GetValue(SuggestionsSourceProperty); set => SetValue(SuggestionsSourceProperty, value); }
    public ICommand? SendCommand { get => GetValue(SendCommandProperty); set => SetValue(SendCommandProperty, value); }
    public ICommand? AttachmentCommand { get => GetValue(AttachmentCommandProperty); set => SetValue(AttachmentCommandProperty, value); }
    public ICommand? DeleteMessagesCommand { get => GetValue(DeleteMessagesCommandProperty); set => SetValue(DeleteMessagesCommandProperty, value); }
    public ICommand? QuoteMessageCommand { get => GetValue(QuoteMessageCommandProperty); set => SetValue(QuoteMessageCommandProperty, value); }
    public ICommand? RetryMessageCommand { get => GetValue(RetryMessageCommandProperty); set => SetValue(RetryMessageCommandProperty, value); }
    public bool IsBusy { get => GetValue(IsBusyProperty); set => SetValue(IsBusyProperty, value); }
    /// <summary>Renders text messages as Markdown or other rich content. Null keeps plain selectable text.</summary>
    public IMdMarkdownProvider? MarkdownProvider { get => GetValue(MarkdownProviderProperty); set => SetValue(MarkdownProviderProperty, value); }

    /// <summary>Loads picture attachments that carry a <see cref="MdChatAttachment.Uri"/>. Passed to every bubble.</summary>
    public IMdImageProvider? ImageProvider { get => GetValue(ImageProviderProperty); set => SetValue(ImageProviderProperty, value); }
    public bool AllowMultipleSelection { get => GetValue(AllowMultipleSelectionProperty); set => SetValue(AllowMultipleSelectionProperty, value); }
    public int SelectionCount => _selectionCount;
    public MdChatMessage? QuotedMessage => _quotedMessage;
    public string StatusText { get => _statusText; private set => SetAndRaise(StatusTextProperty, ref _statusText, value); }
    public string CancelText => _cancelText;
    public string RetryText => _retryText;
    public string AttachFileText => _attachFileText;
    public string SendMessageText => _sendMessageText;
    public string MessagesText => _messagesText;
    public string QuoteText => _quoteText;
    public string DeleteText => _deleteText;
    public string TypeMessageText => _typeMessageText;
    public string SelectionCountText => _selectionCountText;
    public string QuoteHeaderText => _quoteHeaderText;
    public ObservableCollection<MdChatMessage> SelectedMessages { get; } = [];
    public Func<CancellationToken, ValueTask<IReadOnlyList<MdChatMessage>>>? HistoryProvider { get; set; }

    public event EventHandler<string>? MessageSubmitted;
    public event EventHandler? AttachmentRequested;
    public event EventHandler<IReadOnlyList<MdChatMessage>>? HistoryLoaded;
    public event EventHandler<IReadOnlyList<MdChatMessage>>? MessageSelectionChanged;
    public event EventHandler<IReadOnlyList<MdChatMessage>>? DeleteRequested;
    public event EventHandler<MdChatMessage>? QuoteRequested;
    public event EventHandler<MdChatMessage>? RetryRequested;
    /// <summary>Raised when the user activates an attachment (opens a picture, file, or media item).</summary>
    public event EventHandler<MdChatAttachment>? AttachmentActivated;

    public bool Submit()
    {
        var text = ComposerText?.Trim();
        if (string.IsNullOrEmpty(text) || IsBusy) return false;
        if (SendCommand?.CanExecute(text) == true) SendCommand.Execute(text);
        MessageSubmitted?.Invoke(this, text);
        StatusText = MdLocalization.GetString("MessageSubmitted", this);
        ComposerText = string.Empty;
        CancelQuote();
        return true;
    }

    public bool ToggleMessageSelection(MdChatMessage message)
    {
        if (!AllowMultipleSelection && !SelectedMessages.Contains(message)) SelectedMessages.Clear();
        if (!SelectedMessages.Remove(message)) SelectedMessages.Add(message);
        SynchronizeHostSelection();
        UpdateSelectionState();
        return SelectedMessages.Contains(message);
    }

    public void ClearSelection()
    {
        SelectedMessages.Clear();
        SynchronizeHostSelection();
        UpdateSelectionState();
    }

    public bool DeleteSelected()
    {
        var selected = SelectedMessages.ToArray();
        if (selected.Length == 0) return false;
        if (DeleteMessagesCommand?.CanExecute(selected) == true) DeleteMessagesCommand.Execute(selected);
        DeleteRequested?.Invoke(this, selected);
        ClearSelection();
        StatusText = MdLocalization.Format("DeletedMessages", this, selected.Length);
        return true;
    }

    public bool QuoteSelected()
    {
        var message = SelectedMessages.LastOrDefault();
        if (message is null) return false;
        SetQuotedMessage(message);
        PseudoClasses.Set(":quoted", true);
        if (QuoteMessageCommand?.CanExecute(message) == true) QuoteMessageCommand.Execute(message);
        QuoteRequested?.Invoke(this, message);
        ClearSelection();
        _composer?.Focus();
        return true;
    }

    public void CancelQuote()
    {
        SetQuotedMessage(null);
        PseudoClasses.Set(":quoted", false);
    }

    public bool Retry(MdChatMessage message)
    {
        if (message.State != MdAsyncRequestState.Error) return false;
        if (RetryMessageCommand?.CanExecute(message) == true) RetryMessageCommand.Execute(message);
        RetryRequested?.Invoke(this, message);
        StatusText = MdLocalization.Format("RetryingMessageFrom", this, message.Sender ?? LocalizeRole(message.Role));
        return true;
    }

    public async ValueTask LoadHistoryAsync(CancellationToken cancellationToken = default)
    {
        if (HistoryProvider is null) return;
        IsBusy = true;
        try { HistoryLoaded?.Invoke(this, await HistoryProvider(cancellationToken)); }
        finally { IsBusy = false; }
    }

    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ObserveMessagesSource();
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (_observedMessages is not null) _observedMessages.CollectionChanged -= OnMessagesCollectionChanged;
        _observedMessages = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateHandlers();
        base.OnApplyTemplate(e);
        _sendButton = e.NameScope.Find<Button>("PART_SendButton");
        _attachmentButton = e.NameScope.Find<Button>("PART_AttachmentButton");
        _cancelSelectionButton = e.NameScope.Find<Button>("PART_CancelSelectionButton");
        _deleteButton = e.NameScope.Find<Button>("PART_DeleteButton");
        _quoteButton = e.NameScope.Find<Button>("PART_QuoteButton");
        _cancelQuoteButton = e.NameScope.Find<Button>("PART_CancelQuoteButton");
        _suggestionsHost = e.NameScope.Find<ItemsControl>("PART_SuggestionsHost");
        _messagesHost = e.NameScope.Find<ListBox>("PART_MessagesHost");
        if (_messagesHost is not null) _messagesHost.SelectionMode = SelectionMode.Multiple | SelectionMode.Toggle;
        _composer = e.NameScope.Find<TextBox>("PART_Composer");
        _composer?.AddHandler(InputElement.KeyDownEvent, OnComposerKeyDown, global::Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        AttachTemplateHandlers();
        SynchronizeHostSelection();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && SelectionCount > 0) { ClearSelection(); e.Handled = true; }
        else if (e.Key == Key.Escape && QuotedMessage is not null) { CancelQuote(); e.Handled = true; }
        else if (e.Key == Key.Delete && SelectionCount > 0 && DeleteSelected()) e.Handled = true;
        else if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift) && Submit()) e.Handled = true;
        else base.OnKeyDown(e);
    }

    // The composer is an AcceptsReturn TextBox, which consumes Enter before it bubbles back to this
    // control. Intercepting on the tunnel route lets plain Enter send while Shift+Enter still inserts
    // a line break. Enter is always swallowed so an empty or busy composer never gains a stray newline.
    private void OnComposerKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || e.KeyModifiers.HasFlag(KeyModifiers.Shift)) return;
        e.Handled = true;
        Submit();
    }

    private void AttachTemplateHandlers()
    {
        if (_sendButton is not null) _sendButton.Click += OnSend;
        if (_attachmentButton is not null) _attachmentButton.Click += OnAttachment;
        if (_cancelSelectionButton is not null) _cancelSelectionButton.Click += OnCancelSelection;
        if (_deleteButton is not null) _deleteButton.Click += OnDelete;
        if (_quoteButton is not null) _quoteButton.Click += OnQuote;
        if (_cancelQuoteButton is not null) _cancelQuoteButton.Click += OnCancelQuote;
        if (_messagesHost is not null)
        {
            _messagesHost.SelectionChanged += OnMessageSelectionChanged;
            _messagesHost.AddHandler(InputElement.PointerPressedEvent, OnMessagePointerPressed,
                global::Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
            _messagesHost.AddHandler(Button.ClickEvent, OnMessageActionClick);
        }
        _suggestionsHost?.AddHandler(Button.ClickEvent, OnSuggestionClick);
    }

    private void DetachTemplateHandlers()
    {
        _composer?.RemoveHandler(InputElement.KeyDownEvent, OnComposerKeyDown);
        if (_sendButton is not null) _sendButton.Click -= OnSend;
        if (_attachmentButton is not null) _attachmentButton.Click -= OnAttachment;
        if (_cancelSelectionButton is not null) _cancelSelectionButton.Click -= OnCancelSelection;
        if (_deleteButton is not null) _deleteButton.Click -= OnDelete;
        if (_quoteButton is not null) _quoteButton.Click -= OnQuote;
        if (_cancelQuoteButton is not null) _cancelQuoteButton.Click -= OnCancelQuote;
        if (_messagesHost is not null)
        {
            _messagesHost.SelectionChanged -= OnMessageSelectionChanged;
            _messagesHost.RemoveHandler(InputElement.PointerPressedEvent, OnMessagePointerPressed);
            _messagesHost.RemoveHandler(Button.ClickEvent, OnMessageActionClick);
        }
        _suggestionsHost?.RemoveHandler(Button.ClickEvent, OnSuggestionClick);
    }

    private void OnMessageSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_synchronizingSelection || _messagesHost is null) return;
        _synchronizingSelection = true;
        try
        {
            SelectedMessages.Clear();
            foreach (var message in (_messagesHost.SelectedItems ?? Array.Empty<object>()).OfType<MdChatMessage>())
            {
                if (!AllowMultipleSelection) SelectedMessages.Clear();
                SelectedMessages.Add(message);
            }
        }
        finally { _synchronizingSelection = false; }
        UpdateSelectionState();
    }

    private void SynchronizeHostSelection()
    {
        if (_messagesHost is null || _synchronizingSelection) return;
        _synchronizingSelection = true;
        try
        {
            var selectedItems = _messagesHost.SelectedItems;
            if (selectedItems is null) return;
            selectedItems.Clear();
            foreach (var message in SelectedMessages) selectedItems.Add(message);
        }
        finally { _synchronizingSelection = false; }
    }

    private void UpdateLocalizedText()
    {
        SetAndRaise(CancelTextProperty, ref _cancelText, MdLocalization.GetString("Cancel", this));
        SetAndRaise(RetryTextProperty, ref _retryText, MdLocalization.GetString("Retry", this));
        SetAndRaise(AttachFileTextProperty, ref _attachFileText, MdLocalization.GetString("AttachFile", this));
        SetAndRaise(SendMessageTextProperty, ref _sendMessageText, MdLocalization.GetString("SendMessage", this));
        SetAndRaise(MessagesTextProperty, ref _messagesText, MdLocalization.GetString("Message", this));
        SetAndRaise(QuoteTextProperty, ref _quoteText, MdLocalization.GetString("ChatQuote", this));
        SetAndRaise(DeleteTextProperty, ref _deleteText, MdLocalization.GetString("ChatDelete", this));
        SetAndRaise(TypeMessageTextProperty, ref _typeMessageText, MdLocalization.GetString("ChatTypeMessage", this));
        UpdateQuoteHeader();
        UpdateSelectionCountText();
    }

    private void UpdateQuoteHeader()
    {
        var text = _quotedMessage is null
            ? string.Empty
            : MdLocalization.Format("ChatReplyingTo", this, _quotedMessage.Sender ?? LocalizeRole(_quotedMessage.Role));
        SetAndRaise(QuoteHeaderTextProperty, ref _quoteHeaderText, text);
    }

    private void UpdateSelectionCountText() =>
        SetAndRaise(SelectionCountTextProperty, ref _selectionCountText,
            MdLocalization.Format("MessagesSelected", this, _selectionCount));

    private void SetQuotedMessage(MdChatMessage? message)
    {
        SetAndRaise(QuotedMessageProperty, ref _quotedMessage, message);
        UpdateQuoteHeader();
    }

    private string LocalizeRole(MdChatMessageRole role) => role switch
    {
        MdChatMessageRole.Assistant => MdLocalization.GetString("Assistant", this),
        MdChatMessageRole.System => MdLocalization.GetString("System", this),
        _ => MdLocalization.GetString("You", this)
    };

    private void OnMessagesSourceChanged()
    {
        ObserveMessagesSource();
        ReconcileSelection();
    }

    private void ObserveMessagesSource()
    {
        var next = this.IsAttachedToVisualTree() ? MessagesSource as INotifyCollectionChanged : null;
        if (ReferenceEquals(next, _observedMessages)) return;
        if (_observedMessages is not null) _observedMessages.CollectionChanged -= OnMessagesCollectionChanged;
        _observedMessages = next;
        if (_observedMessages is not null) _observedMessages.CollectionChanged += OnMessagesCollectionChanged;
    }

    private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ReconcileSelection();
        var message = e.NewItems?.OfType<MdChatMessage>().LastOrDefault();
        if (message is null) return;
        // A streamed update replaces the same message for every token. Announce a replacement only
        // when it introduces a failure, so assistive technology is not flooded.
        if (e.Action == NotifyCollectionChangedAction.Replace &&
            e.OldItems?.OfType<MdChatMessage>().FirstOrDefault() is { } previous &&
            previous.Id == message.Id && message.State != MdAsyncRequestState.Error) return;
        var senderName = message.Sender ?? LocalizeRole(message.Role);
        StatusText = message.State == MdAsyncRequestState.Error
            ? MdLocalization.Format("MessageFromFailed", this, senderName, message.ErrorText)
            : MdLocalization.Format("NewMessageFrom", this, senderName, message.Content);
    }

    private void ReconcileSelection()
    {
        var available = new Dictionary<string, MdChatMessage>(StringComparer.Ordinal);
        foreach (var message in (MessagesSource ?? Array.Empty<object>()).OfType<MdChatMessage>()) available[message.Id] = message;
        var changed = false;
        foreach (var selected in SelectedMessages.ToArray())
        {
            if (!available.TryGetValue(selected.Id, out var current))
            {
                SelectedMessages.Remove(selected);
                changed = true;
            }
            else if (!ReferenceEquals(current, selected))
            {
                // Streamed or edited messages arrive as new records. Keep the selection on the new instance.
                SelectedMessages[SelectedMessages.IndexOf(selected)] = current;
                changed = true;
            }
        }
        // Collection changes that do not touch the selection must not overwrite the status text.
        if (!changed) return;
        SynchronizeHostSelection();
        UpdateSelectionState();
    }

    private void UpdateSelectionState()
    {
        SetAndRaise(SelectionCountProperty, ref _selectionCount, SelectedMessages.Count);
        UpdateSelectionCountText();
        PseudoClasses.Set(":selection", SelectionCount > 0);
        StatusText = SelectionCount == 0
            ? MdLocalization.GetString("SelectionCleared", this)
            : MdLocalization.Format("MessagesSelected", this, SelectionCount);
        MessageSelectionChanged?.Invoke(this, SelectedMessages.ToArray());
    }

    private void OnSend(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => Submit();
    private void OnAttachment(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (AttachmentCommand?.CanExecute(null) == true) AttachmentCommand.Execute(null);
        AttachmentRequested?.Invoke(this, EventArgs.Empty);
    }
    private void OnCancelSelection(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => ClearSelection();
    private void OnDelete(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => DeleteSelected();
    private void OnQuote(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => QuoteSelected();
    private void OnCancelQuote(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) => CancelQuote();

    private void OnMessagePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var source = e.Source as Visual;
        if (source is null) return;
        var path = source.GetVisualAncestors().Prepend(source).ToArray();
        if (path.OfType<Button>().Any()) return;
        // Selection belongs to the visual bubble. Transparent space in a full-width virtualized
        // row must remain available for scrolling and must never toggle message selection.
        if (!path.OfType<Border>().Any(border => border.Name == "PART_Bubble"))
        {
            e.Handled = true;
            return;
        }

        // Select explicitly instead of depending on ListBoxItem's presenter surface. Rich and
        // selectable text inside the bubble may otherwise consume the press before ListBox sees it.
        var row = path.OfType<ListBoxItem>().FirstOrDefault();
        if (row?.DataContext is not MdChatMessage) return;
        var toggle = AllowMultipleSelection && e.KeyModifiers.HasFlag(KeyModifiers.Control);
        if (toggle)
        {
            row.IsSelected = !row.IsSelected;
        }
        else
        {
            _messagesHost?.UnselectAll();
            row.IsSelected = true;
        }
        e.Handled = true;
    }

    private void OnMessageActionClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Source is Button { Classes: var classes, DataContext: MdChatMessage message } && classes.Contains("retry"))
        {
            Retry(message);
            e.Handled = true;
        }
        else if (e.Source is Button { DataContext: MdChatAttachment attachment })
        {
            AttachmentActivated?.Invoke(this, attachment);
            e.Handled = true;
        }
    }

    private void OnSuggestionClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Source is Button { DataContext: { } suggestion })
        {
            SetCurrentValue(ComposerTextProperty, suggestion.ToString());
            _composer?.Focus();
            e.Handled = true;
        }
    }
}
