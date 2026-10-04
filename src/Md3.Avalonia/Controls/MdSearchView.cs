using System.Reflection;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Motion;

namespace Md3.Avalonia.Controls;

/// <summary>An expandable Material search results surface paired with an arbitrary search-bar header.</summary>
[TemplatePart("PART_Results", typeof(ContentPresenter))]
[PseudoClasses(":open", ":closed", ":present", ":reduced-motion", ":no-motion")]
public sealed class MdSearchView : ContentControl
{
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<MdSearchView, object?>(nameof(Header));
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MdSearchView, bool>(nameof(IsOpen),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<KeyGesture?> OpenGestureProperty =
        AvaloniaProperty.Register<MdSearchView, KeyGesture?>(nameof(OpenGesture), new KeyGesture(Key.K, KeyModifiers.Control));
    public static readonly StyledProperty<bool> IsShortcutEnabledProperty =
        AvaloniaProperty.Register<MdSearchView, bool>(nameof(IsShortcutEnabled), true);
    public static readonly StyledProperty<object?> SelectedResultProperty =
        AvaloniaProperty.Register<MdSearchView, object?>(nameof(SelectedResult),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);
    public static readonly StyledProperty<string> ResultDisplayMemberPathProperty =
        AvaloniaProperty.Register<MdSearchView, string>(nameof(ResultDisplayMemberPath), string.Empty);
    public static readonly StyledProperty<bool> CloseOnResultCommitProperty =
        AvaloniaProperty.Register<MdSearchView, bool>(nameof(CloseOnResultCommit), true);
    public static readonly StyledProperty<ICommand?> ResultCommittedCommandProperty =
        AvaloniaProperty.Register<MdSearchView, ICommand?>(nameof(ResultCommittedCommand));

    private readonly MdPresenceController _presence;
    private InputElement? _headerInput;
    private TextBox? _headerTextBox;
    private bool _suppressAutoOpen;
    private string? _committedHeaderText;
    private TopLevel? _topLevel;
    private ContentPresenter? _results;
    private Control? _focusBeforeOpen;
    private bool _wasOpen;

    static MdSearchView()
    {
        IsOpenProperty.Changed.AddClassHandler<MdSearchView>((view, _) => view.UpdateVisualState());
        MdMotion.SchemeProperty.Changed.AddClassHandler<MdSearchView>((view, _) => view.UpdateMotion());
    }

    // Android has no Escape key. The system back gesture arrives as TopLevel.BackRequested and
    // has to dismiss this surface, or it is unreachable by the one gesture phone users rely on.
    private readonly MdBackScope _backScope;

    public MdSearchView()
    {
        _backScope = new MdBackScope(this, OnBackRequested);
        _presence = new MdPresenceController(value => PseudoClasses.Set(":present", value));
        _presence.Initialize(IsOpen);
        UpdateVisualState();
    }

    public object? Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public KeyGesture? OpenGesture { get => GetValue(OpenGestureProperty); set => SetValue(OpenGestureProperty, value); }
    public bool IsShortcutEnabled { get => GetValue(IsShortcutEnabledProperty); set => SetValue(IsShortcutEnabledProperty, value); }
    public object? SelectedResult { get => GetValue(SelectedResultProperty); set => SetValue(SelectedResultProperty, value); }
    public string ResultDisplayMemberPath { get => GetValue(ResultDisplayMemberPathProperty); set => SetValue(ResultDisplayMemberPathProperty, value); }
    public bool CloseOnResultCommit { get => GetValue(CloseOnResultCommitProperty); set => SetValue(CloseOnResultCommitProperty, value); }
    public ICommand? ResultCommittedCommand { get => GetValue(ResultCommittedCommandProperty); set => SetValue(ResultCommittedCommandProperty, value); }

    public event EventHandler<MdSearchResultCommittedEventArgs>? ResultCommitted;

    public void Show()
    {
        CaptureFocusBeforeOpen();
        SetCurrentValue(IsOpenProperty, true);
        FocusHeader();
    }

    public void Dismiss() => SetCurrentValue(IsOpenProperty, false);

    private void OnHeaderTextChanged(object? sender, TextChangedEventArgs e)
    {
        // Typing in the header has to bring the view back. Committing a result writes the chosen
        // text into the search bar and then closes the view, and nothing reopened it afterwards:
        // clearing the field and typing again filtered a result list nobody could see, so the
        // suggestions appeared to be gone for good. Only the user's own typing counts, hence the
        // focus check, and an empty field stays closed so clearing alone does not expand it.
        if (_suppressAutoOpen || IsOpen) return;
        if (sender is not TextBox { Text.Length: > 0 } header || !header.IsKeyboardFocusWithin) return;

        // The text a commit just wrote is not the user typing. A flag held only for the duration
        // of CommitResult is not enough: the header's TextChanged can arrive on a later dispatcher
        // pass, once the flag is already cleared, which reopened the view the commit had closed.
        // Comparing against the committed text is state, not timing, so it holds whenever the
        // event lands and stops applying the moment the user actually edits the field.
        if (string.Equals(header.Text, _committedHeaderText, StringComparison.Ordinal)) return;
        Show();
    }

    /// <summary>
    /// Commits a result through one binding/command/event path, updates a search-bar header,
    /// and optionally closes the expanded view.
    /// </summary>
    public void CommitResult(object? result, string? displayText = null)
    {
        _suppressAutoOpen = true;
        try
        {
            CommitResultCore(result, displayText);
        }
        finally
        {
            _suppressAutoOpen = false;
        }
    }

    private void CommitResultCore(object? result, string? displayText)
    {
        SetCurrentValue(SelectedResultProperty, result);
        displayText ??= ResolveDisplayText(result);
        if (Header is MdSearchBar searchBar && displayText is not null)
        {
            _committedHeaderText = displayText;
            searchBar.SetCurrentValue(TextBox.TextProperty, displayText);
            searchBar.CaretIndex = displayText.Length;
            searchBar.SelectionStart = displayText.Length;
            searchBar.SelectionEnd = displayText.Length;
        }

        var args = new MdSearchResultCommittedEventArgs(result, displayText);
        if (ResultCommittedCommand?.CanExecute(args) == true) ResultCommittedCommand.Execute(args);
        ResultCommitted?.Invoke(this, args);
        if (CloseOnResultCommit) Dismiss();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _topLevel = TopLevel.GetTopLevel(this);
        _topLevel?.AddHandler(InputElement.KeyDownEvent, OnTopLevelPreviewKeyDown,
            RoutingStrategies.Tunnel, handledEventsToo: true);
        UpdateVisualState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _topLevel?.RemoveHandler(InputElement.KeyDownEvent, OnTopLevelPreviewKeyDown);
        _topLevel = null;
        _presence.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_headerInput is not null)
            _headerInput.RemoveHandler(InputElement.KeyDownEvent, OnHeaderPreviewKeyDown);
        if (_headerTextBox is not null)
            _headerTextBox.TextChanged -= OnHeaderTextChanged;
        base.OnApplyTemplate(e);
        _headerInput = Header as InputElement;
        _headerInput?.AddHandler(InputElement.KeyDownEvent, OnHeaderPreviewKeyDown,
            RoutingStrategies.Tunnel, handledEventsToo: true);
        _headerTextBox = Header as TextBox;
        if (_headerTextBox is not null)
            _headerTextBox.TextChanged += OnHeaderTextChanged;
        _results = e.NameScope.Find<ContentPresenter>("PART_Results");
        UpdateMotion();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsOpen)
        {
            Dismiss();
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void OnTopLevelPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (IsOpen && e.Key == Key.Escape && e.Source is Visual source && IsWithinSearchView(source))
        {
            // Escape closes the expanded surface without clearing the preserved query first.
            Dismiss();
            e.Handled = true;
            return;
        }
        if (!IsShortcutEnabled || OpenGesture is null || !OpenGesture.Matches(e)) return;
        Show();
        e.Handled = true;
    }

    private void OnHeaderPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        // A SearchBar normally consumes Escape to clear its text. In an expanded SearchView,
        // Escape is the modal dismissal gesture and therefore wins in the tunneling phase.
        if (e.Key == Key.Escape && IsOpen)
        {
            Dismiss();
            e.Handled = true;
        }
        else if (e.Key == Key.Down && IsOpen && FocusFirstResult())
        {
            // Down moves from the query field into the first keyboard-focusable result.
            e.Handled = true;
        }
    }

    private void CaptureFocusBeforeOpen()
    {
        if (_wasOpen || _focusBeforeOpen is not null) return;
        _focusBeforeOpen = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Control;
    }

    private void FocusHeader()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (IsOpen && _headerInput is { IsEffectivelyEnabled: true, IsVisible: true })
                _headerInput.Focus(NavigationMethod.Unspecified);
        }, DispatcherPriority.Input);
    }

    private bool FocusFirstResult()
    {
        if (_results is null) return false;
        var result = _results.GetVisualDescendants().OfType<Control>()
            .Where(control => control.Focusable && control.IsVisible && control.IsEffectivelyEnabled &&
                              KeyboardNavigation.GetIsTabStop(control))
            .OrderBy(KeyboardNavigation.GetTabIndex)
            .FirstOrDefault();
        return result?.Focus(NavigationMethod.Directional) == true;
    }

    private void RestoreFocusAfterClose()
    {
        var target = _focusBeforeOpen;
        _focusBeforeOpen = null;
        if (target is null || !target.IsAttachedToVisualTree() || !target.IsEffectivelyEnabled) return;
        Dispatcher.UIThread.Post(() => target.Focus(NavigationMethod.Unspecified), DispatcherPriority.Input);
    }

    private bool IsWithinSearchView(Visual source) =>
        ReferenceEquals(source, this) || source.GetVisualAncestors().Contains(this);

    private string? ResolveDisplayText(object? result)
    {
        if (result is null) return null;
        if (string.IsNullOrWhiteSpace(ResultDisplayMemberPath)) return result.ToString();
        return result.GetType().GetProperty(ResultDisplayMemberPath,
            BindingFlags.Instance | BindingFlags.Public)?.GetValue(result)?.ToString();
    }

    private void UpdateVisualState()
    {
        _backScope.Update(IsOpen);
        if (IsOpen)
        {
            if (!_wasOpen) CaptureFocusBeforeOpen();
            _presence.Update(true, TimeSpan.Zero);
            PseudoClasses.Set(":open", true);
            PseudoClasses.Set(":closed", false);
            if (this.IsAttachedToVisualTree()) FocusHeader();
        }
        else
        {
            PseudoClasses.Set(":open", false);
            PseudoClasses.Set(":closed", true);
            _presence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Default, MdMotionSpeed.Slow));
            if (_wasOpen) RestoreFocusAfterClose();
        }
        _wasOpen = IsOpen;
    }

    private void UpdateMotion()
    {
        var scheme = MdMotion.GetScheme(this);
        PseudoClasses.Set(":reduced-motion", scheme == MdMotionScheme.Reduced);
        PseudoClasses.Set(":no-motion", scheme == MdMotionScheme.None);
        if (_results is not null)
        {
            _results.Transitions = MdMotionTransitions.Collect(
                MdMotionTransitions.CreateDouble(this, OpacityProperty, MdMotionKind.Effects, MdMotionSpeed.Slow),
                MdMotionTransitions.CreateTransform(this, RenderTransformProperty));
        }
        if (!IsOpen)
            _presence.Update(false, MdMotion.GetExitDuration(this, MdMotionSpeed.Default, MdMotionSpeed.Slow));
    }

    private bool OnBackRequested()
    {
        if (!IsOpen) return false;
        Dismiss();
        return true;
    }

}

public sealed class MdSearchResultCommittedEventArgs(object? result, string? displayText) : EventArgs
{
    public object? Result { get; } = result;
    public string? DisplayText { get; } = displayText;}
