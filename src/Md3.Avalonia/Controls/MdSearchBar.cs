using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Controls;

/// <summary>A floating Material search field that preserves native TextBox editing, IME and selection.</summary>
[TemplatePart("PART_ClearButton", typeof(Button))]
[PseudoClasses(":empty", ":has-text", ":has-leading", ":has-trailing", ":clear-visible")]
public sealed class MdSearchBar : TextBox
{
    public static readonly StyledProperty<object?> LeadingContentProperty =
        AvaloniaProperty.Register<MdSearchBar, object?>(nameof(LeadingContent));
    public static readonly StyledProperty<object?> TrailingContentProperty =
        AvaloniaProperty.Register<MdSearchBar, object?>(nameof(TrailingContent));
    public static readonly StyledProperty<bool> ShowClearButtonProperty =
        AvaloniaProperty.Register<MdSearchBar, bool>(nameof(ShowClearButton), true);
    public static readonly StyledProperty<ICommand?> SearchCommandProperty =
        AvaloniaProperty.Register<MdSearchBar, ICommand?>(nameof(SearchCommand));
    public static readonly StyledProperty<object?> SearchCommandParameterProperty =
        AvaloniaProperty.Register<MdSearchBar, object?>(nameof(SearchCommandParameter));

    private Button? _clearButton;

    static MdSearchBar()
    {
        TextProperty.Changed.AddClassHandler<MdSearchBar>((bar, _) => bar.UpdatePseudoClasses());
        LeadingContentProperty.Changed.AddClassHandler<MdSearchBar>((bar, _) => bar.UpdatePseudoClasses());
        TrailingContentProperty.Changed.AddClassHandler<MdSearchBar>((bar, _) => bar.UpdatePseudoClasses());
        ShowClearButtonProperty.Changed.AddClassHandler<MdSearchBar>((bar, _) => bar.UpdatePseudoClasses());
    }

    public MdSearchBar()
    {
        UpdatePseudoClasses();
        MdTextEditingContextMenu.Attach(this);
    }

    public object? LeadingContent { get => GetValue(LeadingContentProperty); set => SetValue(LeadingContentProperty, value); }
    public object? TrailingContent { get => GetValue(TrailingContentProperty); set => SetValue(TrailingContentProperty, value); }
    public bool ShowClearButton { get => GetValue(ShowClearButtonProperty); set => SetValue(ShowClearButtonProperty, value); }
    public ICommand? SearchCommand { get => GetValue(SearchCommandProperty); set => SetValue(SearchCommandProperty, value); }
    public object? SearchCommandParameter { get => GetValue(SearchCommandParameterProperty); set => SetValue(SearchCommandParameterProperty, value); }

    public event EventHandler<string>? SearchSubmitted;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_clearButton is not null) _clearButton.Click -= OnClearClicked;
        base.OnApplyTemplate(e);
        _clearButton = e.NameScope.Find<Button>("PART_ClearButton");
        if (_clearButton is not null) _clearButton.Click += OnClearClicked;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var parameter = SearchCommandParameter ?? Text;
            if (SearchCommand?.CanExecute(parameter) == true) SearchCommand.Execute(parameter);
            SearchSubmitted?.Invoke(this, Text ?? string.Empty);
            e.Handled = true;
            return;
        }
        if (e.Key == Key.Escape && !string.IsNullOrEmpty(Text))
        {
            Clear();
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void OnClearClicked(object? sender, RoutedEventArgs e)
    {
        Clear();
        Focus();
        e.Handled = true;
    }

    private void UpdatePseudoClasses()
    {
        var hasText = !string.IsNullOrEmpty(Text);
        PseudoClasses.Set(":empty", !hasText);
        PseudoClasses.Set(":has-text", hasText);
        PseudoClasses.Set(":has-leading", LeadingContent is not null);
        PseudoClasses.Set(":has-trailing", TrailingContent is not null);
        PseudoClasses.Set(":clear-visible", ShowClearButton && hasText);
    }
}
