using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Md3.Avalonia.Localization;

namespace Md3.Avalonia.Chat;

/// <summary>
/// A fenced code block with an optional language label, a horizontally scrolling body and a copy
/// button. Syntax highlighting is not included; a provider may supply a highlighted control instead.
/// </summary>
public sealed class MdCodeBlock : TemplatedControl
{
    public static readonly StyledProperty<string?> LanguageProperty =
        AvaloniaProperty.Register<MdCodeBlock, string?>(nameof(Language));
    public static readonly StyledProperty<string?> CodeProperty =
        AvaloniaProperty.Register<MdCodeBlock, string?>(nameof(Code));

    private Button? _copyButton;

    /// <summary>Language label shown above the code, for example "csharp". May be empty.</summary>
    public string? Language { get => GetValue(LanguageProperty); set => SetValue(LanguageProperty, value); }

    /// <summary>The code text. Copied verbatim by the copy button.</summary>
    public string? Code { get => GetValue(CodeProperty); set => SetValue(CodeProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_copyButton is not null) _copyButton.Click -= OnCopyClick;
        _copyButton = e.NameScope.Find<Button>("PART_Copy");
        if (_copyButton is null) return;
        _copyButton.Click += OnCopyClick;
        _copyButton.Content = MdLocalization.GetString("ChatCopyCode", this);
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
