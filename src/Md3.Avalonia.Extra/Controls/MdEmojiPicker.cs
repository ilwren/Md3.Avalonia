using System.Collections.Specialized;
using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Localization;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>One host-supplied emoji sequence and its searchable, human-readable metadata.</summary>
/// <param name="Emoji">Text inserted when selected; may contain multiple Unicode scalars.</param>
/// <param name="Name">Accessible name and search text, normally in English.</param>
/// <param name="Category">Host-defined category identifier.</param>
/// <param name="Keywords">Optional additional search text.</param>
/// <param name="ChineseName">Optional Chinese accessible name and search text.</param>
public sealed record MdEmojiItem(string Emoji, string Name, string Category, string? Keywords = null, string? ChineseName = null);

/// <summary>A small, replaceable emoji picker with search, categories and in-memory recents.</summary>
/// <remarks>Glyph rendering depends on platform fonts. Selection emits text; it does not insert,
/// copy, persist, validate Unicode, or guarantee that the platform can display every sequence.</remarks>
[TemplatePart("PART_Search", typeof(TextBox))]
[TemplatePart("PART_Categories", typeof(WrapPanel))]
[TemplatePart("PART_Items", typeof(WrapPanel))]
[TemplatePart("PART_Status", typeof(TextBlock))]
public sealed class MdEmojiPicker : TemplatedControl
{
    // These few manually curated examples are not a Unicode/CLDR dataset. Hosts may replace them.
    private static readonly object AllCategory = new();
    private static readonly object RecentCategory = new();
    private static readonly IReadOnlyList<MdEmojiItem> Defaults = Array.AsReadOnly(new MdEmojiItem[]
    {
        new("😀", "Grinning face", "Faces", "happy smile", "笑脸"),
        new("😊", "Smiling face", "Faces", "happy smile", "微笑"),
        new("😂", "Face with tears of joy", "Faces", "laugh funny", "笑哭"),
        new("🥰", "Smiling face with hearts", "Faces", "love happy", "爱心笑脸"),
        new("😢", "Crying face", "Faces", "sad tears", "哭脸"),
        new("🤔", "Thinking face", "Faces", "think question", "思考"),
        new("👍", "Thumbs up", "Hands", "yes like", "赞"),
        new("👋", "Waving hand", "Hands", "hello goodbye", "挥手"),
        new("👏", "Clapping hands", "Hands", "applause", "鼓掌"),
        new("🙏", "Folded hands", "Hands", "please thanks", "合十"),
        new("❤️", "Red heart", "Symbols", "love", "红心"),
        new("⭐", "Star", "Symbols", "favorite", "星星"),
        new("🎉", "Party popper", "Celebration", "party confetti", "庆祝"),
        new("🎂", "Birthday cake", "Celebration", "cake", "生日蛋糕"),
        new("🌻", "Sunflower", "Nature", "flower", "向日葵"),
        new("🐱", "Cat face", "Nature", "pet animal", "猫脸")
    });

    public static readonly StyledProperty<IEnumerable<MdEmojiItem>?> ItemsSourceProperty =
        AvaloniaProperty.Register<MdEmojiPicker, IEnumerable<MdEmojiItem>?>(nameof(ItemsSource));
    public static readonly StyledProperty<string?> SelectedEmojiProperty =
        AvaloniaProperty.Register<MdEmojiPicker, string?>(nameof(SelectedEmoji), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<string?> SearchTextProperty =
        AvaloniaProperty.Register<MdEmojiPicker, string?>(nameof(SearchText), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<string?> SelectedCategoryProperty =
        AvaloniaProperty.Register<MdEmojiPicker, string?>(nameof(SelectedCategory), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<bool> IsRecentViewProperty =
        AvaloniaProperty.Register<MdEmojiPicker, bool>(nameof(IsRecentView), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<int> RecentLimitProperty =
        AvaloniaProperty.Register<MdEmojiPicker, int>(nameof(RecentLimit), 24, coerce: static (_, v) => Math.Clamp(v, 0, 100));
    public static readonly StyledProperty<int> MaxVisibleItemsProperty =
        AvaloniaProperty.Register<MdEmojiPicker, int>(nameof(MaxVisibleItems), 120, coerce: static (_, v) => Math.Clamp(v, 1, 500));
    public static readonly StyledProperty<FontFamily?> EmojiFontFamilyProperty =
        AvaloniaProperty.Register<MdEmojiPicker, FontFamily?>(nameof(EmojiFontFamily));
    public static readonly DirectProperty<MdEmojiPicker, IReadOnlyList<MdEmojiItem>> VisibleItemsProperty =
        AvaloniaProperty.RegisterDirect<MdEmojiPicker, IReadOnlyList<MdEmojiItem>>(nameof(VisibleItems), c => c.VisibleItems);
    public static readonly DirectProperty<MdEmojiPicker, IReadOnlyList<string>> RecentEmojiProperty =
        AvaloniaProperty.RegisterDirect<MdEmojiPicker, IReadOnlyList<string>>(nameof(RecentEmoji), c => c.RecentEmoji);
    public static readonly DirectProperty<MdEmojiPicker, int> ResultCountProperty =
        AvaloniaProperty.RegisterDirect<MdEmojiPicker, int>(nameof(ResultCount), c => c.ResultCount);

    private IReadOnlyList<MdEmojiItem> _visibleItems = Array.Empty<MdEmojiItem>();
    private IReadOnlyList<string> _recentEmoji = Array.Empty<string>();
    private int _resultCount;
    private readonly List<string> _recents = [];
    private INotifyCollectionChanged? _observed;
    private TextBox? _search;
    private WrapPanel? _categories;
    private WrapPanel? _items;
    private TextBlock? _status;
    private bool _attached;
    private bool _syncingSearch;

    static MdEmojiPicker()
    {
        ItemsSourceProperty.Changed.AddClassHandler<MdEmojiPicker>((c, _) => { c.ObserveSource(); c.Rebuild(); });
        SearchTextProperty.Changed.AddClassHandler<MdEmojiPicker>((c, _) => { c.SyncSearch(); c.Rebuild(); });
        SelectedCategoryProperty.Changed.AddClassHandler<MdEmojiPicker>((c, _) => c.Rebuild());
        IsRecentViewProperty.Changed.AddClassHandler<MdEmojiPicker>((c, _) => c.Rebuild());
        RecentLimitProperty.Changed.AddClassHandler<MdEmojiPicker>((c, _) => { c.TrimRecents(); c.Rebuild(); });
        MaxVisibleItemsProperty.Changed.AddClassHandler<MdEmojiPicker>((c, _) => c.Rebuild());
        EmojiFontFamilyProperty.Changed.AddClassHandler<MdEmojiPicker>((c, _) => c.Rebuild());
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdEmojiPicker>((c, _) => c.Rebuild());
    }

    /// <summary>Creates a picker with a small, replaceable example catalog.</summary>
    public MdEmojiPicker() => Rebuild();

    /// <summary>Optional replacement catalog. Null uses 16 curated examples; an empty sequence displays no items.</summary>
    public IEnumerable<MdEmojiItem>? ItemsSource { get => GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    /// <summary>Last selected text. Programmatic assignment does not add recents or emit selection.</summary>
    public string? SelectedEmoji { get => GetValue(SelectedEmojiProperty); set => SetValue(SelectedEmojiProperty, value); }
    /// <summary>Case-insensitive substring filter over names, keywords, category and emoji text.</summary>
    public string? SearchText { get => GetValue(SearchTextProperty); set => SetValue(SearchTextProperty, value); }
    /// <summary>Category identifier; null or empty means all. Ignored while IsRecentView is true.</summary>
    public string? SelectedCategory { get => GetValue(SelectedCategoryProperty); set => SetValue(SelectedCategoryProperty, value); }
    /// <summary>Shows recently chosen entries instead of the selected category.</summary>
    public bool IsRecentView { get => GetValue(IsRecentViewProperty); set => SetValue(IsRecentViewProperty, value); }
    /// <summary>Maximum number of distinct in-memory recent sequences, coerced to 0–100.</summary>
    public int RecentLimit { get => GetValue(RecentLimitProperty); set => SetValue(RecentLimitProperty, value); }
    /// <summary>Maximum realized results, coerced to 1–500. ResultCount reports the full filtered count.</summary>
    public int MaxVisibleItems { get => GetValue(MaxVisibleItemsProperty); set => SetValue(MaxVisibleItemsProperty, value); }
    /// <summary>Optional font for emoji glyphs; null uses the inherited platform font and fallback.</summary>
    public FontFamily? EmojiFontFamily { get => GetValue(EmojiFontFamilyProperty); set => SetValue(EmojiFontFamilyProperty, value); }
    /// <summary>Currently displayed entries, in display order.</summary>
    public IReadOnlyList<MdEmojiItem> VisibleItems => _visibleItems;
    /// <summary>Distinct most-recent-first sequences, kept only for this picker instance.</summary>
    public IReadOnlyList<string> RecentEmoji => _recentEmoji;
    /// <summary>Number of matching entries before the visible-result limit.</summary>
    public int ResultCount => _resultCount;
    /// <summary>Raised for each user selection, including a repeated selection of the same emoji.</summary>
    public event EventHandler<MdEmojiItem>? EmojiSelected;

    /// <summary>Clears the instance's recent history.</summary>
    public void ClearRecents()
    {
        _recents.Clear();
        PublishRecents();
        Rebuild();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_search is not null) _search.TextChanged -= OnSearchChanged;
        base.OnApplyTemplate(e);
        _search = e.NameScope.Find<TextBox>("PART_Search");
        _categories = e.NameScope.Find<WrapPanel>("PART_Categories");
        _items = e.NameScope.Find<WrapPanel>("PART_Items");
        _status = e.NameScope.Find<TextBlock>("PART_Status");
        if (_search is not null) _search.TextChanged += OnSearchChanged;
        SyncSearch();
        Rebuild();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attached = true;
        ObserveSource();
        Rebuild();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _attached = false;
        ObserveSource();
        base.OnDetachedFromVisualTree(e);
    }

    private void ObserveSource()
    {
        if (_observed is not null) _observed.CollectionChanged -= OnSourceChanged;
        _observed = _attached ? ItemsSource as INotifyCollectionChanged : null;
        if (_observed is not null) _observed.CollectionChanged += OnSourceChanged;
    }

    private void OnSourceChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();
    private void OnSearchChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_syncingSearch && _search is not null) SetCurrentValue(SearchTextProperty, _search.Text);
    }

    private void SyncSearch()
    {
        if (_search is null || _search.Text == (SearchText ?? string.Empty)) return;
        _syncingSearch = true;
        _search.Text = SearchText ?? string.Empty;
        _syncingSearch = false;
    }

    private void TrimRecents()
    {
        if (_recents.Count > RecentLimit) _recents.RemoveRange(RecentLimit, _recents.Count - RecentLimit);
        PublishRecents();
    }

    private void PublishRecents() => SetAndRaise(RecentEmojiProperty, ref _recentEmoji, Array.AsReadOnly(_recents.ToArray()));

    private void Pick(MdEmojiItem item)
    {
        SetCurrentValue(SelectedEmojiProperty, item.Emoji);
        _recents.Remove(item.Emoji);
        if (RecentLimit > 0) _recents.Insert(0, item.Emoji);
        TrimRecents();
        if (IsRecentView) Rebuild();
        EmojiSelected?.Invoke(this, item);
    }

    private static bool Contains(string? value, string query) => value?.Contains(query, StringComparison.OrdinalIgnoreCase) == true;
    private bool Matches(MdEmojiItem item, string query) => query.Length == 0 || Contains(item.Emoji, query) ||
        Contains(item.Name, query) || Contains(item.ChineseName, query) || Contains(item.Keywords, query) || Contains(item.Category, query);

    private string Label(MdEmojiItem item) => MdLocalization.ResolveCulture(this).TwoLetterISOLanguageName == "zh" &&
        !string.IsNullOrWhiteSpace(item.ChineseName) ? item.ChineseName! :
        string.IsNullOrWhiteSpace(item.Name) ? item.Emoji : item.Name;

    private string CategoryLabel(string category) => category switch
    {
        "Faces" => MdLocalization.GetString("EmojiFaces", this),
        "Hands" => MdLocalization.GetString("EmojiHands", this),
        "Symbols" => MdLocalization.GetString("EmojiSymbols", this),
        "Celebration" => MdLocalization.GetString("EmojiCelebration", this),
        "Nature" => MdLocalization.GetString("EmojiNature", this),
        _ => category
    };

    private void Rebuild()
    {
        // A source with mutable entries must notify collection changes (or be reassigned).
        var all = (ItemsSource ?? Defaults).Where(item => item is not null && !string.IsNullOrWhiteSpace(item.Emoji)).ToArray();
        var query = (SearchText ?? string.Empty).Trim();
        IEnumerable<MdEmojiItem> candidates;
        if (IsRecentView)
        {
            var byEmoji = all.GroupBy(item => item.Emoji, StringComparer.Ordinal).ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            candidates = _recents.Where(byEmoji.ContainsKey).Select(key => byEmoji[key]);
        }
        else candidates = string.IsNullOrWhiteSpace(SelectedCategory) ? all : all.Where(item => string.Equals(item.Category, SelectedCategory, StringComparison.Ordinal));
        var matching = candidates.Where(item => Matches(item, query)).ToArray();
        SetAndRaise(ResultCountProperty, ref _resultCount, matching.Length);
        SetAndRaise(VisibleItemsProperty, ref _visibleItems, Array.AsReadOnly(matching.Take(MaxVisibleItems).ToArray()));

        if (_search is not null)
        {
            _search.PlaceholderText = MdLocalization.GetString("EmojiSearch", this);
            AutomationProperties.SetName(_search, MdLocalization.GetString("EmojiSearch", this));
        }
        if (_categories is not null)
        {
            var focused = _categories.Children.OfType<MdButton>().FirstOrDefault(button => button.IsKeyboardFocusWithin)?.Tag;
            _categories.Children.Clear();
            AddCategory(AllCategory, MdLocalization.GetString("All", this), !IsRecentView && string.IsNullOrWhiteSpace(SelectedCategory), () =>
            { SetCurrentValue(IsRecentViewProperty, false); SetCurrentValue(SelectedCategoryProperty, null); });
            AddCategory(RecentCategory, MdLocalization.GetString("EmojiRecent", this), IsRecentView, () => SetCurrentValue(IsRecentViewProperty, true));
            foreach (var category in all.Select(item => item.Category).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct(StringComparer.Ordinal))
            {
                var captured = category;
                AddCategory(captured, CategoryLabel(captured), !IsRecentView && string.Equals(SelectedCategory, captured, StringComparison.Ordinal), () =>
                { SetCurrentValue(SelectedCategoryProperty, captured); SetCurrentValue(IsRecentViewProperty, false); });
            }
            if (focused is not null)
                _categories.Children.OfType<MdButton>().FirstOrDefault(button => Equals(button.Tag, focused))?.Focus();
        }
        if (_items is not null)
        {
            var focused = _items.Children.OfType<MdButton>().FirstOrDefault(button => button.IsKeyboardFocusWithin)?.Tag as MdEmojiItem;
            _items.Children.Clear();
            foreach (var item in VisibleItems)
            {
                var label = Label(item);
                var text = new TextBlock { Text = item.Emoji, FontSize = 24, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                if (EmojiFontFamily is not null) text.FontFamily = EmojiFontFamily;
                var button = new MdButton { Content = text, Tag = item, Width = 48, Height = 48, ContainerHeight = 48, Padding = new Thickness(0), Variant = MdButtonVariant.Text };
                AutomationProperties.SetName(button, label);
                ToolTip.SetTip(button, label);
                button.Click += (_, _) => Pick(item);
                _items.Children.Add(button);
            }
            if (focused is not null)
            {
                var replacement = _items.Children.OfType<MdButton>().FirstOrDefault(button => button.Tag is MdEmojiItem item && item.Emoji == focused.Emoji);
                if (replacement is not null) replacement.Focus();
                else _search?.Focus();
            }
        }
        if (_status is not null)
        {
            _status.Text = ResultCount == 0 ? MdLocalization.GetString(IsRecentView && query.Length == 0 ? "EmojiNoRecent" : "NoResults", this) :
                ResultCount > VisibleItems.Count ? MdLocalization.Format("EmojiShowing", this, VisibleItems.Count, ResultCount) : string.Empty;
            _status.IsVisible = _status.Text.Length > 0;
        }
    }

    private void AddCategory(object identity, string label, bool selected, Action action)
    {
        if (_categories is null) return;
        var button = new MdButton { Content = label, Tag = identity, Variant = selected ? MdButtonVariant.Tonal : MdButtonVariant.Text, Margin = new Thickness(0, 0, 4, 4) };
        AutomationProperties.SetName(button, selected ? MdLocalization.Format("EmojiSelectedCategory", this, label) : label);
        button.Click += (_, _) => action();
        _categories.Children.Add(button);
    }
}
