using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>Arguments for tag collection changes.</summary>
public sealed class MdTagChangedEventArgs(string tag, int index) : EventArgs
{
    public string Tag { get; } = tag;
    public int Index { get; } = index;
}

/// <summary>Theme-facing entry that connects one Material input chip to the parent remove API.</summary>
public sealed record MdTagEntry(string Value, ICommand RemoveCommand);

/// <summary>
/// A Material tag editor composed from removable input chips and a native text field. It supports
/// mutable MVVM lists, suggestions, validation, keyboard creation/removal, commands, events and
/// direct collection APIs.
/// </summary>
[PseudoClasses(":empty", ":has-suggestions", ":md-error", ":limit-reached")]
public sealed class MdTagInput : TemplatedControl
{
    public static readonly StyledProperty<IList<string>?> TagsSourceProperty =
        AvaloniaProperty.Register<MdTagInput, IList<string>?>(nameof(TagsSource), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<IEnumerable<string>?> SuggestionsSourceProperty =
        AvaloniaProperty.Register<MdTagInput, IEnumerable<string>?>(nameof(SuggestionsSource));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<MdTagInput, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string> PlaceholderTextProperty =
        AvaloniaProperty.Register<MdTagInput, string>(nameof(PlaceholderText), "Add tag");

    public static readonly StyledProperty<string> SeparatorsProperty =
        AvaloniaProperty.Register<MdTagInput, string>(nameof(Separators), ",;");

    public static readonly StyledProperty<int> MaximumTagsProperty =
        AvaloniaProperty.Register<MdTagInput, int>(nameof(MaximumTags), 0, validate: value => value >= 0);

    public static readonly StyledProperty<int> MaximumSuggestionsProperty =
        AvaloniaProperty.Register<MdTagInput, int>(nameof(MaximumSuggestions), 6, validate: value => value >= 1);

    public static readonly StyledProperty<bool> AllowDuplicatesProperty =
        AvaloniaProperty.Register<MdTagInput, bool>(nameof(AllowDuplicates));

    public static readonly StyledProperty<bool> CommitOnLostFocusProperty =
        AvaloniaProperty.Register<MdTagInput, bool>(nameof(CommitOnLostFocus), true);

    public static readonly StyledProperty<StringComparison> ComparisonProperty =
        AvaloniaProperty.Register<MdTagInput, StringComparison>(nameof(Comparison), StringComparison.OrdinalIgnoreCase);

    public static readonly StyledProperty<Func<string, string?>?> TagValidatorProperty =
        AvaloniaProperty.Register<MdTagInput, Func<string, string?>?>(nameof(TagValidator));

    public static readonly StyledProperty<ICommand?> TagAddedCommandProperty =
        AvaloniaProperty.Register<MdTagInput, ICommand?>(nameof(TagAddedCommand));

    public static readonly StyledProperty<ICommand?> TagRemovedCommandProperty =
        AvaloniaProperty.Register<MdTagInput, ICommand?>(nameof(TagRemovedCommand));

    public static readonly DirectProperty<MdTagInput, IReadOnlyList<string>> TagsProperty =
        AvaloniaProperty.RegisterDirect<MdTagInput, IReadOnlyList<string>>(nameof(Tags), control => control.Tags);

    public static readonly DirectProperty<MdTagInput, IReadOnlyList<MdTagEntry>> EntriesProperty =
        AvaloniaProperty.RegisterDirect<MdTagInput, IReadOnlyList<MdTagEntry>>(nameof(Entries), control => control.Entries);

    public static readonly DirectProperty<MdTagInput, IReadOnlyList<string>> FilteredSuggestionsProperty =
        AvaloniaProperty.RegisterDirect<MdTagInput, IReadOnlyList<string>>(nameof(FilteredSuggestions), control => control.FilteredSuggestions);

    public static readonly DirectProperty<MdTagInput, string?> ValidationMessageProperty =
        AvaloniaProperty.RegisterDirect<MdTagInput, string?>(nameof(ValidationMessage), control => control.ValidationMessage);

    private IReadOnlyList<string> _tags = [];
    private IReadOnlyList<MdTagEntry> _entries = [];
    private IReadOnlyList<string> _filteredSuggestions = [];
    private string? _validationMessage;
    private INotifyCollectionChanged? _observableSource;
    private MdTextBox? _input;
    private bool _processingText;

    static MdTagInput()
    {
        TagsSourceProperty.Changed.AddClassHandler<MdTagInput>((control, _) => control.OnTagsSourceChanged());
        SuggestionsSourceProperty.Changed.AddClassHandler<MdTagInput>((control, _) => control.RefreshSuggestions());
        TextProperty.Changed.AddClassHandler<MdTagInput>((control, _) => control.OnTextChanged());
        MaximumTagsProperty.Changed.AddClassHandler<MdTagInput>((control, _) => control.UpdatePseudoClasses());
        MaximumSuggestionsProperty.Changed.AddClassHandler<MdTagInput>((control, _) => control.RefreshSuggestions());
        ComparisonProperty.Changed.AddClassHandler<MdTagInput>((control, _) => control.RefreshSuggestions());
    }

    public MdTagInput()
    {
        AutomationProperties.SetName(this, "Tags");
        RemoveTagCommand = new RelayCommand(parameter =>
        {
            if (parameter is string tag) RemoveTag(tag);
        });
        AddHandler(Button.ClickEvent, OnButtonClick);
        Refresh();
    }

    public IList<string>? TagsSource { get => GetValue(TagsSourceProperty); set => SetValue(TagsSourceProperty, value); }
    public IEnumerable<string>? SuggestionsSource { get => GetValue(SuggestionsSourceProperty); set => SetValue(SuggestionsSourceProperty, value); }
    public string? Text { get => GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string PlaceholderText { get => GetValue(PlaceholderTextProperty); set => SetValue(PlaceholderTextProperty, value); }
    public string Separators { get => GetValue(SeparatorsProperty); set => SetValue(SeparatorsProperty, value); }
    public int MaximumTags { get => GetValue(MaximumTagsProperty); set => SetValue(MaximumTagsProperty, value); }
    public int MaximumSuggestions { get => GetValue(MaximumSuggestionsProperty); set => SetValue(MaximumSuggestionsProperty, value); }
    public bool AllowDuplicates { get => GetValue(AllowDuplicatesProperty); set => SetValue(AllowDuplicatesProperty, value); }
    public bool CommitOnLostFocus { get => GetValue(CommitOnLostFocusProperty); set => SetValue(CommitOnLostFocusProperty, value); }
    public StringComparison Comparison { get => GetValue(ComparisonProperty); set => SetValue(ComparisonProperty, value); }
    public Func<string, string?>? TagValidator { get => GetValue(TagValidatorProperty); set => SetValue(TagValidatorProperty, value); }
    public ICommand? TagAddedCommand { get => GetValue(TagAddedCommandProperty); set => SetValue(TagAddedCommandProperty, value); }
    public ICommand? TagRemovedCommand { get => GetValue(TagRemovedCommandProperty); set => SetValue(TagRemovedCommandProperty, value); }
    public IReadOnlyList<string> Tags { get => _tags; private set => SetAndRaise(TagsProperty, ref _tags, value); }
    public IReadOnlyList<MdTagEntry> Entries { get => _entries; private set => SetAndRaise(EntriesProperty, ref _entries, value); }
    public IReadOnlyList<string> FilteredSuggestions { get => _filteredSuggestions; private set => SetAndRaise(FilteredSuggestionsProperty, ref _filteredSuggestions, value); }
    public string? ValidationMessage { get => _validationMessage; private set => SetAndRaise(ValidationMessageProperty, ref _validationMessage, value); }

    /// <summary>Command used by the default input-chip template; direct callers should use <see cref="RemoveTag"/>.</summary>
    public ICommand RemoveTagCommand { get; }

    public event EventHandler<MdTagChangedEventArgs>? TagAdded;
    public event EventHandler<MdTagChangedEventArgs>? TagRemoved;

    /// <summary>Validates and adds a trimmed tag. Returns false and sets ValidationMessage on rejection.</summary>
    public bool AddTag(string? value)
    {
        var tag = value?.Trim() ?? string.Empty;
        var error = Validate(tag);
        if (error is not null)
        {
            ValidationMessage = error;
            UpdatePseudoClasses();
            return false;
        }

        var list = EnsureWritableSource();
        list.Add(tag);
        Refresh();
        var index = Tags.Count - 1;
        if (TagAddedCommand?.CanExecute(tag) == true) TagAddedCommand.Execute(tag);
        TagAdded?.Invoke(this, new MdTagChangedEventArgs(tag, index));
        ValidationMessage = null;
        SetCurrentValue(TextProperty, string.Empty);
        return true;
    }

    public bool RemoveTag(string? value)
    {
        if (value is null) return false;
        var index = FindIndex(value);
        if (index < 0) return false;
        var removed = Tags[index];
        var list = EnsureWritableSource();
        if (index >= list.Count) return false;
        list.RemoveAt(index);
        Refresh();
        if (TagRemovedCommand?.CanExecute(removed) == true) TagRemovedCommand.Execute(removed);
        TagRemoved?.Invoke(this, new MdTagChangedEventArgs(removed, index));
        ValidationMessage = null;
        return true;
    }

    public void ClearTags()
    {
        var list = EnsureWritableSource();
        var removed = list.ToArray();
        list.Clear();
        Refresh();
        for (var index = 0; index < removed.Length; index++)
        {
            if (TagRemovedCommand?.CanExecute(removed[index]) == true) TagRemovedCommand.Execute(removed[index]);
            TagRemoved?.Invoke(this, new MdTagChangedEventArgs(removed[index], index));
        }
    }

    public bool CommitText() => AddTag(Text);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_input is not null)
        {
            _input.KeyDown -= OnInputKeyDown;
            _input.LostFocus -= OnInputLostFocus;
        }
        base.OnApplyTemplate(e);
        _input = e.NameScope.Find<MdTextBox>("PART_Input");
        if (_input is not null)
        {
            _input.KeyDown += OnInputKeyDown;
            _input.LostFocus += OnInputLostFocus;
            AutomationProperties.SetName(_input, PlaceholderText);
        }
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter)
        {
            CommitText();
            e.Handled = true;
        }
        else if (e.Key == Key.Back && string.IsNullOrEmpty(Text) && Tags.Count > 0)
        {
            RemoveTag(Tags[^1]);
            e.Handled = true;
        }
        else if (e.Key == Key.Down && FilteredSuggestions.Count > 0)
        {
            AddTag(FilteredSuggestions[0]);
            e.Handled = true;
        }
    }

    private void OnInputLostFocus(object? sender, RoutedEventArgs e)
    {
        if (CommitOnLostFocus && !string.IsNullOrWhiteSpace(Text)) CommitText();
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is MdSuggestionChip { Name: "PART_Suggestion", DataContext: string suggestion })
        {
            AddTag(suggestion);
            _input?.Focus();
            e.Handled = true;
        }
    }

    private string? Validate(string tag)
    {
        if (tag.Length == 0) return "Enter a tag";
        if (MaximumTags > 0 && Tags.Count >= MaximumTags) return $"Maximum {MaximumTags} tags";
        if (!AllowDuplicates && Tags.Any(existing => string.Equals(existing, tag, Comparison))) return "Tag already added";
        return TagValidator?.Invoke(tag);
    }

    private int FindIndex(string tag)
    {
        for (var index = 0; index < Tags.Count; index++)
            if (string.Equals(Tags[index], tag, Comparison)) return index;
        return -1;
    }

    private IList<string> EnsureWritableSource()
    {
        if (TagsSource is { IsReadOnly: false } mutable) return mutable;
        var replacement = new ObservableCollection<string>(Tags);
        SetCurrentValue(TagsSourceProperty, replacement);
        return replacement;
    }

    private void OnTagsSourceChanged()
    {
        if (_observableSource is not null) _observableSource.CollectionChanged -= OnCollectionChanged;
        _observableSource = TagsSource as INotifyCollectionChanged;
        if (_observableSource is not null) _observableSource.CollectionChanged += OnCollectionChanged;
        Refresh();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

    private void OnTextChanged()
    {
        if (_processingText) return;
        var text = Text ?? string.Empty;
        if (!string.IsNullOrEmpty(Separators) && text.Any(Separators.Contains))
        {
            _processingText = true;
            var fragments = text.Split(Separators.ToCharArray(), StringSplitOptions.None);
            for (var index = 0; index < fragments.Length - 1; index++) AddTag(fragments[index]);
            SetCurrentValue(TextProperty, fragments[^1]);
            _processingText = false;
        }
        RefreshSuggestions();
    }

    private void Refresh()
    {
        Tags = TagsSource?.ToArray() ?? [];
        Entries = Tags.Select(tag => new MdTagEntry(tag, RemoveTagCommand)).ToArray();
        RefreshSuggestions();
        UpdatePseudoClasses();
    }

    private void RefreshSuggestions()
    {
        var query = Text?.Trim() ?? string.Empty;
        FilteredSuggestions = query.Length == 0
            ? []
            : (SuggestionsSource ?? [])
                .Where(suggestion => suggestion.Contains(query, Comparison))
                .Where(suggestion => AllowDuplicates || !Tags.Any(tag => string.Equals(tag, suggestion, Comparison)))
                .Take(MaximumSuggestions)
                .ToArray();
        UpdatePseudoClasses();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":empty", Tags.Count == 0);
        PseudoClasses.Set(":has-suggestions", FilteredSuggestions.Count > 0);
        PseudoClasses.Set(":md-error", ValidationMessage is not null);
        PseudoClasses.Set(":limit-reached", MaximumTags > 0 && Tags.Count >= MaximumTags);
    }

    private sealed class RelayCommand(Action<object?> execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute(parameter);
    }
}
