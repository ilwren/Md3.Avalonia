using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using Md3.Avalonia.Localization;

namespace Md3.Avalonia.Chat;

/// <summary>
/// A fenced code block with an optional language label, a horizontally scrolling body, a copy button and
/// optional syntax highlighting through <see cref="Highlighter"/>.
/// </summary>
public sealed class MdCodeBlock : TemplatedControl
{
    public static readonly StyledProperty<string?> LanguageProperty =
        AvaloniaProperty.Register<MdCodeBlock, string?>(nameof(Language));
    public static readonly StyledProperty<string?> CodeProperty =
        AvaloniaProperty.Register<MdCodeBlock, string?>(nameof(Code));
    public static readonly StyledProperty<IMdSyntaxHighlighter?> HighlighterProperty =
        AvaloniaProperty.Register<MdCodeBlock, IMdSyntaxHighlighter?>(nameof(Highlighter));

    private Button? _copyButton;
    private SelectableTextBlock? _body;

    static MdCodeBlock()
    {
        CodeProperty.Changed.AddClassHandler<MdCodeBlock>((block, _) => block.UpdateBody());
        LanguageProperty.Changed.AddClassHandler<MdCodeBlock>((block, _) => block.UpdateBody());
        HighlighterProperty.Changed.AddClassHandler<MdCodeBlock>((block, _) => block.UpdateBody());
    }

    /// <summary>Language label shown above the code, for example "csharp". May be empty.</summary>
    public string? Language { get => GetValue(LanguageProperty); set => SetValue(LanguageProperty, value); }

    /// <summary>The code text. Copied verbatim by the copy button.</summary>
    public string? Code { get => GetValue(CodeProperty); set => SetValue(CodeProperty, value); }

    /// <summary>Optional highlighter that colours the code. When null the code is shown as plain text.</summary>
    public IMdSyntaxHighlighter? Highlighter { get => GetValue(HighlighterProperty); set => SetValue(HighlighterProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_copyButton is not null) _copyButton.Click -= OnCopyClick;
        _copyButton = e.NameScope.Find<Button>("PART_Copy");
        if (_copyButton is not null)
        {
            _copyButton.Click += OnCopyClick;
            _copyButton.Content = MdLocalization.GetString("ChatCopyCode", this);
        }
        _body = e.NameScope.Find<SelectableTextBlock>("PART_Body");
        UpdateBody();
    }

    private void UpdateBody()
    {
        if (_body is null) return;
        var code = Code ?? string.Empty;
        _body.Inlines.Clear();
        if (Highlighter is null)
        {
            _body.Text = code;
            return;
        }
        _body.Text = null;
        foreach (var token in Highlighter.Highlight(code, Language))
            _body.Inlines.Add(new Run { Text = token.Text, Foreground = BrushFor(token.Kind) });
    }

    private IBrush? BrushFor(MdCodeTokenKind kind)
    {
        var key = kind switch
        {
            MdCodeTokenKind.Keyword => "Md.Sys.Color.Primary.Brush",
            MdCodeTokenKind.Type => "Md.Sys.Color.Secondary.Brush",
            MdCodeTokenKind.String => "Md.Sys.Color.Tertiary.Brush",
            MdCodeTokenKind.Number => "Md.Sys.Color.Tertiary.Brush",
            MdCodeTokenKind.Comment => "Md.Sys.Color.OnSurfaceVariant.Brush",
            _ => null,
        };
        if (key is null) return null;
        return this.TryGetResource(key, null, out var value) && value is IBrush brush ? brush : null;
    }

    private async void OnCopyClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        try
        {
            if (TopLevel.GetTopLevel(this)?.Clipboard is not { } clipboard)
                throw new InvalidOperationException("Clipboard unavailable.");
            await clipboard.SetTextAsync(Code ?? string.Empty);
            SetCopyStatus(MdLocalization.GetString("ChatCodeCopied", this));
        }
        catch (Exception)
        {
            SetCopyStatus(MdLocalization.GetString("ChatCopyFailed", this));
        }
    }

    private void SetCopyStatus(string text)
    {
        if (_copyButton is not null) _copyButton.Content = text;
    }
}
