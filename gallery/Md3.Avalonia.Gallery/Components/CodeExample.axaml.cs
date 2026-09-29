using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Styling;
using System.IO;
using System.Xml;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Components;

/// <summary>A selectable AvaloniaEdit AXAML/C# preview with tabs and one-click copy.</summary>
public partial class CodeExample : UserControl
{
    public static readonly StyledProperty<string> XamlCodeProperty =
        AvaloniaProperty.Register<CodeExample, string>(nameof(XamlCode), string.Empty);

    public static readonly StyledProperty<string> CSharpCodeProperty =
        AvaloniaProperty.Register<CodeExample, string>(nameof(CSharpCode), string.Empty);

    // Backward-compatible aliases used by older Gallery samples.
    public static readonly StyledProperty<string> LanguageProperty =
        AvaloniaProperty.Register<CodeExample, string>(nameof(Language), "AXAML");

    public static readonly StyledProperty<string> CodeProperty =
        AvaloniaProperty.Register<CodeExample, string>(nameof(Code), string.Empty);

    private bool _showingCSharp;
    private readonly IHighlightingDefinition? _xamlHighlighting;
    private readonly IHighlightingDefinition? _csharpHighlighting;
    private readonly IHighlightingDefinition _darkXamlHighlighting;
    private readonly IHighlightingDefinition _darkCsharpHighlighting;

    static CodeExample()
    {
        XamlCodeProperty.Changed.AddClassHandler<CodeExample>((example, _) => example.UpdateEditors());
        CSharpCodeProperty.Changed.AddClassHandler<CodeExample>((example, _) => example.UpdateEditors());
        CodeProperty.Changed.AddClassHandler<CodeExample>((example, _) => example.UpdateEditors());
        LanguageProperty.Changed.AddClassHandler<CodeExample>((example, _) => example.UpdateEditors());
    }

    public CodeExample()
    {
        InitializeComponent();
        _xamlHighlighting = HighlightingManager.Instance.GetDefinition("XML");
        _csharpHighlighting = HighlightingManager.Instance.GetDefinition("C#");
        _darkXamlHighlighting = LoadDarkDefinition(DarkXamlDefinition);
        _darkCsharpHighlighting = LoadDarkDefinition(DarkCsharpDefinition);
        ActualThemeVariantChanged += (_, _) => UpdateEditorTheme();
        UpdateEditorTheme();
        UpdateEditors();
    }

    public string XamlCode
    {
        get => GetValue(XamlCodeProperty);
        set => SetValue(XamlCodeProperty, value);
    }

    public string CSharpCode
    {
        get => GetValue(CSharpCodeProperty);
        set => SetValue(CSharpCodeProperty, value);
    }

    public string Language
    {
        get => GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value);
    }

    public string Code
    {
        get => GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    private string EffectiveXaml => !string.IsNullOrWhiteSpace(XamlCode)
        ? XamlCode
        : Language.Contains("C#", StringComparison.OrdinalIgnoreCase) ? string.Empty : Code;

    private string EffectiveCSharp => !string.IsNullOrWhiteSpace(CSharpCode)
        ? CSharpCode
        : Language.Contains("C#", StringComparison.OrdinalIgnoreCase) ? Code : "// Add the equivalent direct-control C# here.";

    private void UpdateEditors()
    {
        if (XamlEditor is null || CSharpEditor is null)
        {
            return;
        }

        XamlEditor.Text = EffectiveXaml;
        CSharpEditor.Text = EffectiveCSharp;
    }

    private void UpdateEditorTheme()
    {
        if (XamlEditor is null || CSharpEditor is null) return;
        var dark = ActualThemeVariant == ThemeVariant.Dark ||
                   Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

        // Built-in definitions target light editors. Dark mode uses equivalent syntax groups with
        // contrast-checked Material-friendly colors rather than disabling highlighting.
        XamlEditor.SyntaxHighlighting = dark ? _darkXamlHighlighting : _xamlHighlighting;
        CSharpEditor.SyntaxHighlighting = dark ? _darkCsharpHighlighting : _csharpHighlighting;
    }

    private static IHighlightingDefinition LoadDarkDefinition(string xshd)
    {
        using var reader = XmlReader.Create(new StringReader(xshd));
        return HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }

    private const string DarkXamlDefinition = """
        <SyntaxDefinition name="Material dark AXAML" extensions=".axaml" xmlns="http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008">
          <Color name="Comment" foreground="#FF9AA0A6" fontStyle="italic" />
          <Color name="Tag" foreground="#FF8AB4F8" />
          <Color name="Attribute" foreground="#FFF28B82" />
          <Color name="Value" foreground="#FFA8DAB5" />
          <RuleSet>
          <Rule color="Tag">(?&lt;=&lt;/?)[A-Za-z_][A-Za-z0-9_.:-]*</Rule>
          <Rule color="Attribute">[A-Za-z_][A-Za-z0-9_.:-]*(?=\s*=)</Rule>
          <Span color="Value" begin="&quot;" end="&quot;" />
          <Span color="Comment" begin="&lt;!--" end="--&gt;" />
          </RuleSet>
        </SyntaxDefinition>
        """;

    private const string DarkCsharpDefinition = """
        <SyntaxDefinition name="Material dark C#" extensions=".cs" xmlns="http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008">
          <Color name="Comment" foreground="#FF9AA0A6" fontStyle="italic" />
          <Color name="Keyword" foreground="#FFC58AF9" fontWeight="bold" />
          <Color name="String" foreground="#FFA8DAB5" />
          <Color name="Number" foreground="#FFFDD663" />
          <RuleSet>
          <Span color="Comment" begin="//" end="\n" />
          <Span color="Comment" multiline="true" begin="/\*" end="\*/" />
          <Span color="String" begin="&quot;" end="&quot;" escapeCharacter="\" />
          <Rule color="Keyword">\b(abstract|as|async|await|base|bool|break|case|catch|class|const|continue|default|delegate|do|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|record|ref|required|return|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|var|virtual|void|volatile|while|with|yield)\b</Rule>
          <Rule color="Number">\b(0[xX][0-9a-fA-F]+|[0-9]+(\.[0-9]+)?)\b</Rule>
          </RuleSet>
        </SyntaxDefinition>
        """;

    private void LanguageSelectionChanged(object? sender, SelectionChangedEventArgs e) =>
        SetLanguage((sender as MdSegmentedButtonGroup)?.SelectedIndex == 1);

    private void SetLanguage(bool showCSharp)
    {
        _showingCSharp = showCSharp;
        if (XamlEditor is null || CSharpEditor is null || LanguageTabs is null) return;
        XamlEditor.IsVisible = !showCSharp;
        CSharpEditor.IsVisible = showCSharp;
        var index = showCSharp ? 1 : 0;
        if (LanguageTabs.SelectedIndex != index) LanguageTabs.SelectedIndex = index;
    }

    private async void CopyCode(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(_showingCSharp ? EffectiveCSharp : EffectiveXaml);
        }
    }
}
